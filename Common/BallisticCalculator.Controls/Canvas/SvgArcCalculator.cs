namespace BallisticCalculator.Controls.Canvas;

/// <summary>
/// Converts SVG arc parameters to the format needed by SkiaSharp.
/// Algorithm taken from Apache Batik project.
/// </summary>
internal class SvgArcCalculator
{
    public float CorrRx { get; }
    public float CorrRy { get; }
    public float CX { get; }
    public float CY { get; }
    public float AngleStart { get; }
    public float AngleExtent { get; }

    public SvgArcCalculator(float r, float x1, float y1, float x2, float y2, bool largeArc, bool clockwiseDirection)
    {
        // Convert parameters to doubles for calculation precision
        double rx = Math.Abs(r);
        double ry = Math.Abs(r);
        double angle = 0;

        double startX = x1;
        double startY = y1;
        double endX = x2;
        double endY = y2;

        // Compute the half distance between the current and the final point
        double dx2 = (startX - endX) / 2.0;
        double dy2 = (startY - endY) / 2.0;

        // Convert angle from degrees to radians
        double radAngle = angle * Math.PI / 180;
        double cosAngle = Math.Cos(radAngle);
        double sinAngle = Math.Sin(radAngle);

        // Step 1: Compute (x1, y1)
        double x1Calc = cosAngle * dx2 + sinAngle * dy2;
        double y1Calc = -sinAngle * dx2 + cosAngle * dy2;

        // Ensure radii are large enough
        double prx = rx * rx;
        double pry = ry * ry;
        double px1 = x1Calc * x1Calc;
        double py1 = y1Calc * y1Calc;

        // Check that radii are large enough
        double radiiCheck = px1 / prx + py1 / pry;
        if (radiiCheck > 1)
        {
            rx = Math.Sqrt(radiiCheck) * rx;
            ry = Math.Sqrt(radiiCheck) * ry;
            prx = rx * rx;
            pry = ry * ry;
        }

        // Step 2: Compute (cx1, cy1)
        double sign = largeArc == clockwiseDirection ? -1 : 1;
        double sq = ((prx * pry) - (prx * py1) - (pry * px1)) / ((prx * py1) + (pry * px1));
        sq = sq < 0 ? 0 : sq;
        double coef = sign * Math.Sqrt(sq);
        double cx1 = coef * ((rx * y1Calc) / ry);
        double cy1 = coef * -((ry * x1Calc) / rx);

        // Step 3: Compute (cx, cy) from (cx1, cy1)
        double sx2 = (startX + endX) / 2.0;
        double sy2 = (startY + endY) / 2.0;
        double cx = sx2 + (cosAngle * cx1 - sinAngle * cy1);
        double cy = sy2 + (sinAngle * cx1 + cosAngle * cy1);

        // Step 4: Compute the angleStart and the angleExtent
        double ux = x1Calc - cx1;
        double uy = y1Calc - cy1;
        double vx = -x1Calc - cx1;
        double vy = -y1Calc - cy1;

        // Compute the angle start
        double n = Math.Sqrt(ux * ux + uy * uy);
        double p = ux; // (1 * ux) + (0 * uy)
        sign = uy < 0 ? -1.0 : 1.0;
        double angleStart = sign * Math.Acos(p / n);
        angleStart = angleStart * 180 / Math.PI;

        // Compute the angle extent
        n = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
        p = ux * vx + uy * vy;
        sign = ux * vy - uy * vx < 0 ? -1.0 : 1.0;
        double angleExtent = sign * Math.Acos(p / n);
        angleExtent = angleExtent * 180 / Math.PI;

        if (!clockwiseDirection && angleExtent > 0)
        {
            angleExtent -= 360.0;
        }
        else if (clockwiseDirection && angleExtent < 0)
        {
            angleExtent += 360.0;
        }

        angleExtent %= 360.0;
        angleStart %= 360.0;

        CorrRx = (float)rx;
        CorrRy = (float)ry;
        CX = (float)cx;
        CY = (float)cy;
        AngleStart = (float)angleStart;
        AngleExtent = (float)angleExtent;
    }
}
