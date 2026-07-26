using System.Text;

namespace BallisticCalculator.Types;

/// <summary>One accepted data row: the two raw text fields and the physical line they came from.</summary>
public sealed record CsvTextRow(string First, string Second, int LineNumber);

/// <summary>
/// A fully accepted two-column table. <see cref="HeaderFirst"/>/<see cref="HeaderSecond"/> are null when
/// the file had no header; they are returned as text because mapping names to roles (and therefore the
/// column order) needs to know what the columns mean, which is the typed parsers' business.
/// </summary>
public sealed record CsvTextTable(IReadOnlyList<CsvTextRow> Rows, char Separator,
                                 string? HeaderFirst, string? HeaderSecond);

/// <summary>
/// Reads a two-column CSV of numbers-with-units into raw text fields, deliberately knowing nothing about
/// measurements or ballistic coefficients — the caller supplies a predicate that decides whether a row's
/// two fields parse.
/// <para>
/// Import is <b>all or nothing</b>: only empty lines are skipped, an unparseable first line is taken as an
/// optional header, and any other unparseable line rejects the whole file. A drag curve silently missing a
/// knot is worse than a refused file, because the user cannot see what is absent.
/// </para>
/// </summary>
public static class CsvTextTableReader
{
    /// <summary>Hard cap on data rows; a bigger file is refused rather than truncated.</summary>
    public const int MaximumRows = 50_000;

    /// <summary>Separator candidates, in the order they are tried.</summary>
    private static readonly char[] Separators = { ';', '\t', ',' };

    /// <summary>
    /// Reads lines into a table, or fails with a message naming the offending line.
    /// </summary>
    /// <param name="isUsableRow">
    /// Decides whether a split row's two fields both parse. Drives the header decision (an unusable first
    /// line is a header) and the separator choice (a candidate is accepted only if every data line is
    /// usable under it).
    /// </param>
    public static bool TryRead(IEnumerable<string> lines, Func<string, string, bool> isUsableRow,
                               out CsvTextTable table, out string error)
    {
        ArgumentNullException.ThrowIfNull(isUsableRow);

        table = null!;
        error = "";

        var all = lines as IList<string> ?? lines?.ToList() ?? (IList<string>)Array.Empty<string>();

        // Physical line numbers are kept so the error message points at what the user sees in an editor.
        var numbered = new List<(int Line, string Text)>();
        for (int i = 0; i < all.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(all[i]))
                numbered.Add((i + 1, all[i]));
        }

        if (numbered.Count == 0)
        {
            error = "The file contains no data.";
            return false;
        }

        if (numbered.Count > MaximumRows + 1)
        {
            error = $"The file has more than {MaximumRows} lines. Trim it and import again — " +
                    "importing part of a file would silently drop data.";
            return false;
        }

        // Try each separator and keep the failure from the candidate that got furthest, so the message
        // quotes real content instead of a line mis-split by the wrong separator.
        string bestError = "";
        int bestProgress = -1;

        foreach (var separator in Separators)
        {
            if (TryReadWith(numbered, separator, isUsableRow, out var candidate, out var candidateError, out var progress))
            {
                table = candidate;
                return true;
            }

            if (progress > bestProgress)
            {
                bestProgress = progress;
                bestError = candidateError;
            }
        }

        error = bestError;
        return false;
    }

    /// <summary>
    /// As <see cref="TryRead"/>, plus I/O, binary-content and encoding rejection. Handles a BOM, CRLF or LF
    /// endings, and a missing final end-of-line.
    /// </summary>
    public static bool TryReadFile(string? path, Func<string, string, bool> isUsableRow,
                                   out CsvTextTable table, out string error)
    {
        table = null!;
        error = "";

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "No file was selected.";
            return false;
        }

        string[] lines;
        try
        {
            var bytes = File.ReadAllBytes(path);

            // NUL bytes near the start mean this is not a text file (a .drg, .ammox or archive picked by
            // mistake); reading it as text would produce nonsense rows.
            var probe = Math.Min(bytes.Length, 4096);
            for (int i = 0; i < probe; i++)
            {
                if (bytes[i] == 0)
                {
                    error = $"{Path.GetFileName(path)} is not a text file.";
                    return false;
                }
            }

            var text = Decode(bytes);
            lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd('\r');
        }
        catch (Exception ex)
        {
            error = $"{Path.GetFileName(path)} could not be read: {ex.Message}";
            return false;
        }

        if (!TryRead(lines, isUsableRow, out table, out var readError))
        {
            error = $"{Path.GetFileName(path)}: {readError}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Decodes as UTF-8 (dropping a BOM, which would otherwise stay glued to the first field and break
    /// line 1's parse), falling back to the system default when the bytes are not valid UTF-8.
    /// </summary>
    private static string Decode(byte[] bytes)
    {
        try
        {
            var strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            var text = strict.GetString(bytes);
            return text.TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Default.GetString(bytes).TrimStart('﻿');
        }
    }

    /// <summary>
    /// Reads every line under one separator candidate. <paramref name="progress"/> is how many lines were
    /// consumed before failing, used to pick the most plausible candidate's error message.
    /// </summary>
    private static bool TryReadWith(List<(int Line, string Text)> numbered, char separator,
                                    Func<string, string, bool> isUsableRow,
                                    out CsvTextTable table, out string error, out int progress)
    {
        table = null!;
        error = "";
        progress = 0;

        var rows = new List<CsvTextRow>(numbered.Count);
        string? headerFirst = null;
        string? headerSecond = null;

        for (int i = 0; i < numbered.Count; i++)
        {
            var (line, text) = numbered[i];
            var parts = TrimTrailingEmpty(text.Split(separator));

            // Under a comma separator, extra fields cannot be told from decimal commas: "100,780,2" is
            // either two values with a decimal comma or three columns. Extra columns are tolerated for the
            // other separators (§0b), but here the file must be refused rather than read as (100, 780).
            if (separator == ',' && parts.Length > 2)
            {
                error = $"line {line} \"{text}\" — more than two comma-separated values, so a decimal " +
                        "comma cannot be told from an extra column. Use ';' as the separator. " +
                        "Nothing was imported.";
                return false;
            }

            string? first = null;
            string? second = null;
            if (parts.Length >= 2)
            {
                first = Normalize(parts[0], separator, out var firstAmbiguous);
                second = Normalize(parts[1], separator, out var secondAmbiguous);

                // Under a comma separator a field like "780,2" cannot be told from two fields; refuse
                // rather than silently reading it as 7802 (which is what the library's parser would do).
                if (firstAmbiguous || secondAmbiguous)
                {
                    error = $"line {line} \"{text}\" — the decimal separator is ambiguous under '{separator}'. " +
                            "Nothing was imported.";
                    return false;
                }
            }

            var usable = first != null && second != null && isUsableRow(first, second);

            if (!usable)
            {
                // An unusable first line is the optional header. Anywhere else it is a hard error.
                if (i == 0)
                {
                    headerFirst = parts.Length >= 1 ? Unquote(parts[0].Trim()) : null;
                    headerSecond = parts.Length >= 2 ? Unquote(parts[1].Trim()) : null;
                    progress = 1;
                    continue;
                }

                error = parts.Length < 2
                    ? $"line {line} \"{text}\" — expected two values separated by '{separator}'. Nothing was imported."
                    : $"line {line} \"{text}\" — could not be read as a value pair. Nothing was imported.";
                return false;
            }

            rows.Add(new CsvTextRow(first!, second!, line));
            progress = i + 1;
        }

        if (rows.Count == 0)
        {
            error = "The file contains a header but no data rows.";
            return false;
        }

        if (rows.Count > MaximumRows)
        {
            error = $"The file has more than {MaximumRows} data rows. Trim it and import again.";
            return false;
        }

        table = new CsvTextTable(rows, separator, headerFirst, headerSecond);
        return true;
    }

    /// <summary>
    /// Trims and unquotes a field, and rewrites a decimal comma to a dot. When the separator is itself a
    /// comma the field is flagged ambiguous instead, since "780,2" could equally be two fields.
    /// </summary>
    private static string Normalize(string field, char separator, out bool ambiguous)
    {
        ambiguous = false;
        var value = Unquote(field.Trim());

        if (!value.Contains(','))
            return value;

        if (separator == ',')
        {
            ambiguous = true;
            return value;
        }

        return value.Replace(',', '.');
    }

    /// <summary>
    /// Drops trailing empty fields so a line ending in a separator ("1.5,0.462,") is still two fields —
    /// common in exports, and not the same thing as a genuine extra column.
    /// </summary>
    private static string[] TrimTrailingEmpty(string[] parts)
    {
        int end = parts.Length;
        while (end > 0 && string.IsNullOrWhiteSpace(parts[end - 1]))
            end--;
        return end == parts.Length ? parts : parts[..end];
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1].Trim();
        return value;
    }
}
