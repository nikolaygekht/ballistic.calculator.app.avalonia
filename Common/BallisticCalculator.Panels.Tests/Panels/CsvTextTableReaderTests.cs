using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// Import is all-or-nothing: only empty lines are skipped, an unparseable first line is an optional
/// header, and any other unparseable line rejects the whole file (see claude/07-26-drg-plan.md §0b).
/// </summary>
public class CsvTextTableReaderTests
{
    // Stands in for the typed parsers: both fields must be plain numbers, and it says which one is not.
    private static string? TwoNumbers(string a, string b)
    {
        if (!IsNumber(a)) return "the first value is not a number";
        if (!IsNumber(b)) return "the second value is not a number";
        return null;
    }

    private static bool IsNumber(string text) =>
        double.TryParse(text, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out _);

    #region Accepted

    [Fact]
    public void SemicolonWithHeader_ShouldReadRowsAndHeader()
    {
        var lines = new[] { "mach;bc", "1.5;0.462", "1.75;0.463" };

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Separator.Should().Be(';');
        table.HeaderFirst.Should().Be("mach");
        table.HeaderSecond.Should().Be("bc");
        table.Rows.Should().HaveCount(2);
        table.Rows[0].First.Should().Be("1.5");
        table.Rows[0].Second.Should().Be("0.462");
        table.Rows[0].LineNumber.Should().Be(2);
    }

    [Fact]
    public void NoHeader_ShouldReadFromFirstLine()
    {
        var lines = new[] { "1.5;0.462", "1.75;0.463" };

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.HeaderFirst.Should().BeNull();
        table.HeaderSecond.Should().BeNull();
        table.Rows.Should().HaveCount(2);
        table.Rows[0].LineNumber.Should().Be(1);
    }

    [Fact]
    public void SingleRow_ShouldBeAccepted()
    {
        // The BC editor legitimately takes one knot; a domain minimum is the parser's business.
        var ok = CsvTextTableReader.TryRead(new[] { "1.5;0.462" }, TwoNumbers, out var table, out _);

        ok.Should().BeTrue();
        table.Rows.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("1.5,0.462", ',')]
    [InlineData("1.5\t0.462", '\t')]
    [InlineData("1.5;0.462", ';')]
    public void Separators_ShouldBeDetected(string line, char expected)
    {
        var ok = CsvTextTableReader.TryRead(new[] { line }, TwoNumbers, out var table, out _);

        ok.Should().BeTrue();
        table.Separator.Should().Be(expected);
    }

    [Fact]
    public void SeparatorFallback_ShouldPickTheCandidateThatParsesEveryLine()
    {
        // Commas are decimal marks here, so ';' is the only separator under which every line parses.
        var lines = new[] { "1,5;0,462", "1,75;0,463" };

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Separator.Should().Be(';');
        table.Rows[0].First.Should().Be("1.5");    // normalized decimal mark
        table.Rows[0].Second.Should().Be("0.462");
    }

    [Fact]
    public void EmptyLines_ShouldBeSkippedAnywhere()
    {
        var lines = new[] { "", "mach;bc", "1.5;0.462", "   ", "1.75;0.463", "" };

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Rows.Should().HaveCount(2);
        table.Rows[1].LineNumber.Should().Be(5);   // line numbers count every physical line
    }

    [Fact]
    public void QuotedFields_ShouldBeUnquoted()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "\"1.5\";\"0.462\"" }, TwoNumbers, out var table, out _);

        ok.Should().BeTrue();
        table.Rows[0].First.Should().Be("1.5");
        table.Rows[0].Second.Should().Be("0.462");
    }

    [Fact]
    public void ExtraFields_ShouldUseTheFirstTwo()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "1.5;0.462;junk;more" }, TwoNumbers, out var table, out _);

        ok.Should().BeTrue();
        table.Rows[0].First.Should().Be("1.5");
        table.Rows[0].Second.Should().Be("0.462");
    }

    [Fact]
    public void TrailingSeparator_ShouldStillBeTwoFields()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "1.5,0.462,", "1.75,0.463," }, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Separator.Should().Be(',');
        table.Rows.Should().HaveCount(2);
        table.Rows[0].Second.Should().Be("0.462");
    }

    #endregion

    #region Rejected

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void OneBadLine_ShouldRejectTheWholeFile(int badLine)
    {
        var lines = new[] { "1.5;0.462", "1.75;0.463", "2;0.470", "2.25;0.480" };
        lines[badLine - 1] = "1400 d;bad";

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out var table, out var error);

        ok.Should().BeFalse();
        table.Should().BeNull();
        error.Should().Contain(badLine.ToString()).And.Contain("1400 d");
    }

    [Fact]
    public void CommentLine_ShouldRejectTheFile()
    {
        // '#' is not a comment marker under the all-or-nothing rule.
        var ok = CsvTextTableReader.TryRead(new[] { "1.5;0.462", "# note", "2;0.470" }, TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().Contain("2");
    }

    [Fact]
    public void OneColumnLine_ShouldRejectTheFile()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "1.5;0.462", "0.470" }, TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AmbiguousDecimalCommaUnderCommaSeparator_ShouldReject()
    {
        // "100,780,2" is either two values with a decimal comma or three columns. Reading it as
        // (100, 780) — which taking the first two fields would do — silently corrupts the curve.
        var ok = CsvTextTableReader.TryRead(new[] { "0,3078.8", "100,780,2" }, TwoNumbers, out var table, out var error);

        ok.Should().BeFalse();
        table.Should().BeNull();
        error.Should().Contain("2").And.Contain(";");   // points at the line and suggests ';'
    }

    [Fact]
    public void DecimalCommaUnderSemicolon_ShouldNotBeAmbiguous()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "0;3078,8", "100;780,2" }, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Rows[1].Second.Should().Be("780.2");
    }

    [Fact]
    public void HeaderOnly_ShouldReject()
    {
        var ok = CsvTextTableReader.TryRead(new[] { "mach;bc" }, TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EmptyInput_ShouldReject()
    {
        var ok = CsvTextTableReader.TryRead(Array.Empty<string>(), TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TooManyLines_ShouldReject()
    {
        var lines = Enumerable.Range(0, CsvTextTableReader.MaximumRows + 1)
                              .Select(i => $"{i};0.5").ToArray();

        var ok = CsvTextTableReader.TryRead(lines, TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().Contain(CsvTextTableReader.MaximumRows.ToString());
    }

    #endregion

    #region Files

    [Fact]
    public void ReadFile_CrLfAndMissingFinalEol_ShouldBeAccepted()
    {
        var path = Temp("0yd;3078.8\r\n100yd;3001.2\r\n200yd;2923.9");   // no trailing EOL
        File.WriteAllText(path, "0;3078.8\r\n100;3001.2\r\n200;2923.9");

        var ok = CsvTextTableReader.TryReadFile(path, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Rows.Should().HaveCount(3);
    }

    [Fact]
    public void ReadFile_Utf8Bom_ShouldNotBreakTheFirstRow()
    {
        var path = Temp("bom");
        File.WriteAllText(path, "1.5;0.462\n1.75;0.463", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var ok = CsvTextTableReader.TryReadFile(path, TwoNumbers, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Rows.Should().HaveCount(2);
        table.HeaderFirst.Should().BeNull();      // the BOM must not turn row 1 into a header
        table.Rows[0].First.Should().Be("1.5");
    }

    [Fact]
    public void ReadFile_Binary_ShouldReject()
    {
        var path = Temp("bin");
        File.WriteAllBytes(path, new byte[] { 0x1f, 0x8b, 0x00, 0x00, 0x42, 0x00, 0x13 });

        var ok = CsvTextTableReader.TryReadFile(path, TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReadFile_MissingPath_ShouldRejectWithoutThrowing()
    {
        var ok = CsvTextTableReader.TryReadFile(Path.Combine(Path.GetTempPath(), "no-such-file.csv"),
                                               TwoNumbers, out _, out var error);

        ok.Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    private static string Temp(string _) => Path.Combine(Path.GetTempPath(), $"csvreader-{Guid.NewGuid():N}.csv");

    #endregion
}
