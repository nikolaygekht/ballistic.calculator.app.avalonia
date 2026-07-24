using Xunit;
using AwesomeAssertions;
using BallisticCalculator.Controls.Controllers;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class AzimuthDirectionControllerTests
{
    private readonly AzimuthDirectionController _controller = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(90, 90)]
    [InlineData(360, 0)]
    [InlineData(370, 10)]
    [InlineData(-10, 350)]
    public void NormalizeAngle_ShouldWrapTo0To360(double input, double expected)
    {
        _controller.NormalizeAngle(input).Should().BeApproximately(expected, 0.001);
    }

    // Click mapping: 0 = up (North), clockwise. Control is 100x100, center (50,50).
    [Theory]
    [InlineData(50, 10, 0)]    // straight up
    [InlineData(90, 50, 90)]   // right (East)
    [InlineData(50, 90, 180)]  // down (South)
    [InlineData(10, 50, 270)]  // left (West)
    public void DirectionFromClick_ShouldMapToCompassBearing(double clickX, double clickY, double expected)
    {
        _controller.DirectionFromClick(100, 100, clickX, clickY)
            .Should().BeApproximately(expected, 0.5);
    }

    [Fact]
    public void CalculateArrow_North_TipPointsUpFromCenter()
    {
        var arrow = _controller.CalculateArrow(100, 100, 0);

        arrow.CenterX.Should().BeApproximately(50, 0.001);
        arrow.CenterY.Should().BeApproximately(50, 0.001);
        // Shaft starts at center...
        arrow.StartX.Should().BeApproximately(50, 0.001);
        arrow.StartY.Should().BeApproximately(50, 0.001);
        // ...and the tip is at the top edge (shooter -> target, pointing North/up).
        arrow.EndX.Should().BeApproximately(50, 0.001);
        arrow.EndY.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void CalculateArrow_East_TipPointsRight()
    {
        var arrow = _controller.CalculateArrow(100, 100, 90);

        arrow.EndX.Should().BeApproximately(100, 0.001);
        arrow.EndY.Should().BeApproximately(50, 0.001);
    }

    [Fact]
    public void CalculateArrow_South_TipPointsDown()
    {
        var arrow = _controller.CalculateArrow(100, 100, 180);

        arrow.EndX.Should().BeApproximately(50, 0.001);
        arrow.EndY.Should().BeApproximately(100, 0.001);
    }
}
