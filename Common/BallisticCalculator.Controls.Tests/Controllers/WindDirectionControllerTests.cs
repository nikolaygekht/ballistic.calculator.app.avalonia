using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Controls.Models;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class WindDirectionControllerTests
{
    private const double Tolerance = 0.0001;

    #region CalculateArrow - Basic Geometry Tests

    [Fact]
    public void CalculateArrow_ShouldCalculateCenterAndRadius()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 0);

        // Assert
        arrow.CenterX.Should().Be(50);
        arrow.CenterY.Should().Be(50);
        arrow.Radius.Should().Be(50);
    }

    [Fact]
    public void CalculateArrow_WithRectangularControl_ShouldUseMinimumDimension()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(200, 100, 0);

        // Assert
        arrow.CenterX.Should().Be(100);
        arrow.CenterY.Should().Be(50);
        arrow.Radius.Should().Be(50); // Min of 100, 50
    }

    [Fact]
    public void CalculateArrow_EndPointShouldBeAtCenter()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 45);

        // Assert
        arrow.EndX.Should().Be(50);
        arrow.EndY.Should().Be(50);
    }

    #endregion

    #region CalculateArrow - Direction Tests

    [Fact]
    public void CalculateArrow_At0Degrees_ShouldStartFromBottom()
    {
        // Arrange: 0° = tailwind, wind from behind (bottom)
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 0);

        // Assert: Start should be at bottom center
        arrow.StartX.Should().BeApproximately(50, Tolerance);
        arrow.StartY.Should().BeApproximately(100, Tolerance); // Bottom
    }

    [Fact]
    public void CalculateArrow_At90Degrees_ShouldStartFromRight()
    {
        // Arrange: 90° = wind from right (crosswind blowing left)
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 90);

        // Assert: Start should be at right center
        arrow.StartX.Should().BeApproximately(100, Tolerance); // Right
        arrow.StartY.Should().BeApproximately(50, Tolerance);
    }

    [Fact]
    public void CalculateArrow_At180Degrees_ShouldStartFromTop()
    {
        // Arrange: 180° = headwind, wind from front (top)
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 180);

        // Assert: Start should be at top center
        arrow.StartX.Should().BeApproximately(50, Tolerance);
        arrow.StartY.Should().BeApproximately(0, Tolerance); // Top
    }

    [Fact]
    public void CalculateArrow_At270Degrees_ShouldStartFromLeft()
    {
        // Arrange: 270° = wind from left (crosswind blowing right)
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 270);

        // Assert: Start should be at left center
        arrow.StartX.Should().BeApproximately(0, Tolerance); // Left
        arrow.StartY.Should().BeApproximately(50, Tolerance);
    }

    [Fact]
    public void CalculateArrow_At45Degrees_ShouldStartFromBottomRight()
    {
        // Arrange: 45° = wind from bottom-right quadrant
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 45);

        // Assert: Start should be in bottom-right quadrant
        arrow.StartX.Should().BeGreaterThan(50); // Right of center
        arrow.StartY.Should().BeGreaterThan(50); // Below center
    }

    #endregion

    #region CalculateArrow - Arrowhead Tests

    [Fact]
    public void CalculateArrow_ArrowheadPointsShouldBeNearCenter()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 0);

        // Assert: Arrowhead points should be close to center (within 20% of radius)
        var maxDistance = arrow.Radius * 0.25;
        var head1Distance = Math.Sqrt(Math.Pow(arrow.Head1X - arrow.CenterX, 2) + Math.Pow(arrow.Head1Y - arrow.CenterY, 2));
        var head2Distance = Math.Sqrt(Math.Pow(arrow.Head2X - arrow.CenterX, 2) + Math.Pow(arrow.Head2Y - arrow.CenterY, 2));

        head1Distance.Should().BeLessThanOrEqualTo(maxDistance);
        head2Distance.Should().BeLessThanOrEqualTo(maxDistance);
    }

    [Fact]
    public void CalculateArrow_ArrowheadPointsShouldBeSymmetric()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var arrow = controller.CalculateArrow(100, 100, 0);

        // Assert: Head1 and Head2 should be symmetric about the arrow line
        // For 0° direction, they should have equal Y offset from center and opposite X offset
        var head1DistFromCenter = Math.Sqrt(Math.Pow(arrow.Head1X - arrow.CenterX, 2) + Math.Pow(arrow.Head1Y - arrow.CenterY, 2));
        var head2DistFromCenter = Math.Sqrt(Math.Pow(arrow.Head2X - arrow.CenterX, 2) + Math.Pow(arrow.Head2Y - arrow.CenterY, 2));

        head1DistFromCenter.Should().BeApproximately(head2DistFromCenter, Tolerance);
    }

    #endregion

    #region DirectionFromClick Tests

    [Fact]
    public void DirectionFromClick_AtBottom_ShouldReturn0()
    {
        // Arrange: Click at bottom center (wind from behind = tailwind)
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 50, 100);

        // Assert
        direction.Should().BeApproximately(0, Tolerance);
    }

    [Fact]
    public void DirectionFromClick_AtRight_ShouldReturn90()
    {
        // Arrange: Click at right center (wind from right)
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 100, 50);

        // Assert
        direction.Should().BeApproximately(90, Tolerance);
    }

    [Fact]
    public void DirectionFromClick_AtTop_ShouldReturn180()
    {
        // Arrange: Click at top center (wind from front = headwind)
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 50, 0);

        // Assert
        direction.Should().BeApproximately(180, Tolerance);
    }

    [Fact]
    public void DirectionFromClick_AtLeft_ShouldReturn270()
    {
        // Arrange: Click at left center (wind from left)
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 0, 50);

        // Assert
        direction.Should().BeApproximately(270, Tolerance);
    }

    [Fact]
    public void DirectionFromClick_AtBottomRight_ShouldReturn45()
    {
        // Arrange: Click at bottom-right corner
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 100, 100);

        // Assert
        direction.Should().BeApproximately(45, Tolerance);
    }

    [Fact]
    public void DirectionFromClick_AtTopLeft_ShouldReturn225()
    {
        // Arrange: Click at top-left corner
        var controller = new WindDirectionController();

        // Act
        var direction = controller.DirectionFromClick(100, 100, 0, 0);

        // Assert
        direction.Should().BeApproximately(225, Tolerance);
    }

    #endregion

    #region NormalizeAngle Tests

    [Fact]
    public void NormalizeAngle_PositiveWithinRange_ShouldReturnSame()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act & Assert
        controller.NormalizeAngle(45).Should().Be(45);
        controller.NormalizeAngle(180).Should().Be(180);
        controller.NormalizeAngle(359).Should().Be(359);
    }

    [Fact]
    public void NormalizeAngle_Zero_ShouldReturnZero()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var result = controller.NormalizeAngle(0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void NormalizeAngle_360_ShouldReturnZero()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act
        var result = controller.NormalizeAngle(360);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void NormalizeAngle_Negative_ShouldNormalize()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act & Assert
        controller.NormalizeAngle(-90).Should().Be(270);
        controller.NormalizeAngle(-180).Should().Be(180);
        controller.NormalizeAngle(-270).Should().Be(90);
    }

    [Fact]
    public void NormalizeAngle_GreaterThan360_ShouldNormalize()
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act & Assert
        controller.NormalizeAngle(450).Should().Be(90);
        controller.NormalizeAngle(720).Should().Be(0);
        controller.NormalizeAngle(800).Should().Be(80);
    }

    #endregion

    #region Round-Trip Tests

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(135)]
    [InlineData(180)]
    [InlineData(225)]
    [InlineData(270)]
    [InlineData(315)]
    public void RoundTrip_CalculateArrowThenDirectionFromClick_ShouldMatch(double direction)
    {
        // Arrange
        var controller = new WindDirectionController();

        // Act: Calculate arrow, then click at arrow start to get direction back
        var arrow = controller.CalculateArrow(100, 100, direction);
        var recoveredDirection = controller.DirectionFromClick(100, 100, arrow.StartX, arrow.StartY);

        // Assert: Handle 0°/360° equivalence
        var diff = Math.Abs(recoveredDirection - direction);
        if (diff > 180) diff = 360 - diff; // Handle wrap-around
        diff.Should().BeLessThanOrEqualTo(0.1);
    }

    #endregion
}
