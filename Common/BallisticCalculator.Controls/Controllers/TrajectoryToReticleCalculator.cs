using BallisticCalculator;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using System;
using System.Collections.Generic;

namespace BallisticCalculator.Controls.Controllers;

public class TrajectoryToReticleCalculator
{
    public enum PointLocation
    {
        Near,
        Far,
    }

    public class Point
    {
        public required ReticleBulletDropCompensatorPoint BDCPoint { get; init; }
        public PointLocation Location { get; init; }
        public Measurement<DistanceUnit> Distance { get; init; }
    }

    private readonly TrajectoryPoint[] _trajectory;
    private readonly ReticleBulletDropCompensatorPointCollection _bulletDropCompensator;
    private readonly Measurement<DistanceUnit> _zeroDistance;

    public TrajectoryToReticleCalculator(TrajectoryPoint[] trajectory,
                                         ReticleBulletDropCompensatorPointCollection bulletDropCompensator,
                                         Measurement<DistanceUnit> zeroDistance)
    {
        _trajectory = trajectory ?? throw new ArgumentNullException(nameof(trajectory));
        _bulletDropCompensator = bulletDropCompensator ?? throw new ArgumentNullException(nameof(bulletDropCompensator));
        _zeroDistance = zeroDistance;
    }

    private readonly List<Point> _points = new();
    public IReadOnlyList<Point> Points => _points;

    /// <summary>
    /// Finds distances for BDC points by interpolating the trajectory.
    /// </summary>
    /// <param name="reticleScale">Always 1 for FFP scopes. MaxZoom/CurrentZoom for SFP scopes.</param>
    public void UpdatePoints(double reticleScale = 1.0)
    {
        _points.Clear();

        for (int i = 2; i < _trajectory.Length; i++)
        {
            var p1 = _trajectory[i - 1];
            var p2 = _trajectory[i];

            for (int j = 0; j < _bulletDropCompensator.Count; j++)
            {
                var bdc = _bulletDropCompensator[j];
                var drop = bdc.Position.Y * reticleScale;
                if ((drop >= p1.DropAdjustment && drop <= p2.DropAdjustment) ||
                    (drop <= p1.DropAdjustment && drop >= p2.DropAdjustment))
                {
                    var dropDelta = (drop - p2.DropAdjustment).Abs();
                    var dropRange = (p1.DropAdjustment - p2.DropAdjustment).Abs();
                    var distanceRange = p2.Distance - p1.Distance;
                    var distance = p2.Distance - distanceRange * (dropDelta / dropRange);
                    var location = distance >= _zeroDistance ? PointLocation.Far : PointLocation.Near;
                    _points.Add(new Point
                    {
                        BDCPoint = bdc,
                        Location = location,
                        Distance = distance,
                    });
                }
            }
        }
    }

    /// <summary>
    /// Binary search for the nearest trajectory point at the given distance.
    /// </summary>
    public TrajectoryPoint? FindDistance(Measurement<DistanceUnit> distance)
    {
        if (_trajectory == null || _trajectory.Length == 0)
            return null;

        if (_trajectory[0].Distance > distance)
            return null;
        if (_trajectory[^1].Distance < distance)
            return null;
        if (_trajectory[^2].Distance < distance)
            return _trajectory[^1];

        int lo = 0;
        int hi = _trajectory.Length - 2;

        while ((hi - lo) > 1)
        {
            int mid = (int)Math.Floor((hi + lo) / 2.0);
            if (_trajectory[mid].Distance < distance)
                lo = mid;
            else
                hi = mid;
        }

        if ((_trajectory[hi].Distance - distance) > (distance - _trajectory[lo].Distance))
            return _trajectory[lo];
        else
            return _trajectory[hi];
    }
}
