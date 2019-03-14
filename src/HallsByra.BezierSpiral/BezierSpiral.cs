using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HallsByra.BezierSpiral
{
    public struct Point
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point FromPolar(double angle, double radius) =>
            new Point(radius * Math.Cos(angle), radius * Math.Sin(angle));
    }

    public struct Bezier
    {
        public Point P0 { get; private set; }
        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public Point P3 { get; private set; }

        public Bezier(Point p0, Point p1, Point p2, Point p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
    }


    static class BezierSpiral
    {
        // Generate bezier curves that approximate an Archimedean spiral.
        public static Bezier[] Generate(Point center, double startAngle, double endAngle, double angleStep, double turnSeparation)
        {
            var fullAngleSegments = GenerateBezierSpiralSegments(startAngle, angleStep)
                .TakeWhile(c => c.P3.Angle < endAngle)
                .ToArray();
            var lastAngle = fullAngleSegments.Any()
                ? fullAngleSegments.Last().P3.Angle
                : startAngle;
            var restSegment = GenerateBezierSpiralSegments(lastAngle, endAngle - lastAngle)
                .Take(1);
            return fullAngleSegments
                .Concat(restSegment)
                .Select(b => ToBezier(b, center, turnSeparation))
                .ToArray();
        }

        // Spit out an endless sequence of bezier segments along a normalized spiral (origo-centered, radius = angle)
        static IEnumerable<BezierSpiralSegment> GenerateBezierSpiralSegments(double startAngle, double angleStep)
        {
            var startPoint = PointOnSpiral.At(startAngle);
            while (true)
            {
                var endPoint = PointOnSpiral.At(startPoint.Angle + angleStep);
                var bezier = BezierSpiralSegment.ForSpiralSegment(startPoint, endPoint);
                startPoint = endPoint;
                yield return bezier;
            }
        }

        // Transform a BezierSpiralSegment to a Bezier curve, e.g. strip it of algorithm-internal info
        // and transform it from a normalized spiral to the specified center/turnSeparation.
        private static Bezier ToBezier(BezierSpiralSegment src, Point center, double turnSeparation) =>
            new Bezier(
                p0: Transpose(src.P0.Point, center, turnSeparation),
                p1: Transpose(src.P1, center, turnSeparation),
                p2: Transpose(src.P2, center, turnSeparation),
                p3: Transpose(src.P3.Point, center, turnSeparation));

        private static Point Transpose(Point src, Point center, double turnSeparation) =>
            new Point(
                x: (src.X + center.X) * turnSeparation / (2 * Math.PI),
                y: (src.Y + center.Y) * turnSeparation / (2 * Math.PI));

        static Point TangentVectorAt(double theta) =>
            new Point(
                x: Math.Cos(theta) - theta * Math.Sin(theta),
                y: Math.Sin(theta) + theta * Math.Cos(theta));

        static double TangentAngleAt(double theta)
        {
            var vector = TangentVectorAt(theta);
            return Math.Atan2(vector.Y, vector.X);
        }

        // Information about a point on a normalized spiral.
        struct PointOnSpiral
        {
            public double Angle { get; private set; }
            public double TangentAngle { get; private set; }
            public Point Point { get; private set; }

            public static PointOnSpiral At(double angle)
            {
                return new PointOnSpiral
                {
                    Angle = angle,
                    TangentAngle = TangentAngleAt(angle),
                    Point = Point.FromPolar(angle, angle),
                };
            }
        }

        // Information about a bezier curve between two points on a normalized spiral.
        struct BezierSpiralSegment
        {
            public PointOnSpiral P0 { get; private set; }
            public Point P1 { get; private set; }
            public Point P2 { get; private set; }
            public PointOnSpiral P3 { get; private set; }

            // Creates a BezierSpiralSegment between two points on the spiral.
            // Placement of control points is based on https://stackoverflow.com/a/27863181/1345815
            public static BezierSpiralSegment ForSpiralSegment(PointOnSpiral p0, PointOnSpiral p3)
            {
                var cpOffset = 4 * Math.Tan((p3.Angle - p0.Angle) / 4) / 3;
                return new BezierSpiralSegment
                {
                    P0 = p0,
                    P1 = new Point(
                        x: Math.Cos(p0.TangentAngle) * cpOffset * p0.Angle + p0.Point.X,
                        y: Math.Sin(p0.TangentAngle) * cpOffset * p0.Angle + p0.Point.Y),
                    P2 = new Point(
                        x: Math.Cos(p3.TangentAngle - Math.PI) * cpOffset * p3.Angle + p3.Point.X,
                        y: Math.Sin(p3.TangentAngle - Math.PI) * cpOffset * p3.Angle + p3.Point.Y),
                    P3 = p3,
                };
            }
        }
    }

}
