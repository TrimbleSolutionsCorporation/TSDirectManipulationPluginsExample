namespace ComplexTransitionSectionPlugin
{
    using System;
    using System.Linq;

    using Tekla.Structures.Geometry3d;

    /// <summary>
    /// This class defines the geometry of a transition section based on some configurable parameters
    /// </summary>
    public sealed class TransitionSectionGeometry
    {
        /// <summary>
        /// Defines the width of the rectangular section
        /// </summary>
        public double RectangularWidth { get; }

        /// <summary>
        /// Defines the height of the rectangular section
        /// </summary>
        public double RectangularHeight { get; }

        /// <summary>
        /// Defines the radius of the cylindrical section
        /// </summary>
        public double CircularRadius { get; }

        /// <summary>
        /// Defines the length of the transition section as measured in the normal direction
        /// </summary>
        public double TransitionLength { get; }

        /// <summary>
        /// Defines the centroid of the rectangular base section
        /// </summary>
        public Point BaseCentroid { get; }

        /// <summary>
        /// Defines the plane where the rectangular base is located
        /// </summary>
        public Vector Normal { get; }

        /// <summary>
        /// Gets the geometry of the rectangular section of the transition.The order of the
        /// polycurve is such that the arcs that define the corners of the section can be directly connected using
        /// a lofted plate with the arcs of the circular section in the order that they appear.
        /// </summary>
        public Polycurve RectangularSection { get; }

        /// <summary>
        /// Gets the four arcs that can be used to connect to the chamfered corners of the rectangular section for
        /// the transition section. The order of the arcs is the same as the order of the arcs in the rectangular
        /// section.
        /// </summary>
        public Polycurve CircularSection { get; }

        /// <summary>
        /// Constructs a transition section geometry
        /// </summary>
        /// <param name="rectangularHeight">Height of the rectangular base</param>
        /// <param name="rectangularWidth">Width of the rectangular base</param>
        /// <param name="transitionLength">Length of the transition section</param>
        /// <param name="circularSectionRadius">Radius of the cylindrical section</param>
        /// <param name="baseCentroid">Centroid of the rectangular base</param>
        /// <param name="normalDirection">Normal of the plane where the rectangular section is located</param>
        public TransitionSectionGeometry(double rectangularHeight, double rectangularWidth,
                                         double transitionLength, double circularSectionRadius,
                                         Point baseCentroid, Vector normalDirection)
        {
            this.RectangularHeight = rectangularHeight;
            this.RectangularWidth = rectangularWidth;
            this.TransitionLength = transitionLength;
            this.CircularRadius = circularSectionRadius;
            this.BaseCentroid = baseCentroid;
            this.Normal = normalDirection;

            this.RectangularSection = this.GetRectangularSection();
            this.CircularSection = this.GetCircularSection();
        }

        /// <summary>
        /// Gets the coordinate system of the base.
        /// </summary>
        /// <returns>The coordinate system.</returns>
        public static CoordinateSystem GetBaseCoordsys(Point baseCentroid, Vector normalDirection)
        {
            var globalUnitX = new Vector(1, 0, 0);
            var yAxis = normalDirection.Cross(globalUnitX).GetNormal();
            if (yAxis.GetLength() < GeometryConstants.DISTANCE_EPSILON)
            {
                yAxis = new Vector(0, 0, 1);
            }

            var xAxis = yAxis.Cross(normalDirection).GetNormal();

            return new CoordinateSystem(baseCentroid, xAxis, yAxis);
        }

        /// <summary>
        /// Calculates a polycurve that defines the rectangular base of the transition section.
        /// </summary>
        /// <returns>The geometry of the rectangular section</returns>
        private Polycurve GetRectangularSection()
        {
            // Note: The scale factor for the corner radius is chosen arbitrarily
            double cornerRadius = Math.Min(this.RectangularHeight, this.RectangularWidth) / 12.0;

            double halfWidth = this.RectangularWidth / 2.0;
            double halfHeight = this.RectangularHeight / 2.0;
            double XLength = this.RectangularWidth - 2.0 * cornerRadius;
            double YLength = this.RectangularHeight - 2.0 * cornerRadius;

            var positiveXSegment = new LineSegment(new Point(halfWidth, -YLength / 2.0, 0),
                                                   new Point(halfWidth, YLength / 2.0, 0));

            var polycurveInLocal = new PolycurveGeometryBuilder()
                .Append(positiveXSegment)
                .AppendTangentArc(new Point(halfWidth - cornerRadius, halfHeight, 0.0))
                .AppendTangentSegment(XLength)
                .AppendTangentArc(new Point(-halfWidth, halfHeight - cornerRadius, 0.0))
                .AppendTangentSegment(YLength)
                .AppendTangentArc(new Point(-(halfWidth - cornerRadius), -halfHeight, 0.0))
                .AppendTangentSegment(XLength)
                .AppendTangentArc(new Point(halfWidth, -(halfHeight - cornerRadius), 0.0))
                .GetPolycurve();

            return this.TransformToGlobal(polycurveInLocal);
        }

        /// <summary>
        /// Calculates four arcs that can be used to connect to the chamfered corners of the rectangular section for
        /// the transition section. The order of the arcs is the same as the order of the arcs in the rectangular
        /// section.
        /// </summary>
        /// <returns>Four arcs that form the circular section.</returns>
        private Polycurve GetCircularSection()
        {
            var centerPoint = new Point(0, 0, this.TransitionLength);
            var topRightStartPoint = new Point(this.CircularRadius, 0, this.TransitionLength);
            var normalInLocal = new Vector(0, 0, 1);

            var topRightArc = new Arc(centerPoint, topRightStartPoint, normalInLocal, Math.PI / 2.0);

            var polycurveInLocal = new PolycurveGeometryBuilder()
                .Append(topRightArc)
                .AppendTangentArc(new Point(-this.CircularRadius, 0, this.TransitionLength))
                .AppendTangentArc(new Point(0, -this.CircularRadius, this.TransitionLength))
                .AppendTangentArc(topRightStartPoint)
                .GetPolycurve();

            return this.TransformToGlobal(polycurveInLocal);
        }

        /// <summary>
        /// Transforms a geometry by a matrix.
        /// </summary>
        /// <param name="geometryInLocal">Geometry to transform.</param>
        /// <param name="matrixToGlobal">Transformation matrix.</param>
        /// <returns>The transformed geometry.</returns>
        private static ICurve TransformGeometryByMatrix(ICurve geometryInLocal, Matrix matrixToGlobal)
        {
            var startPoint = matrixToGlobal.Transform(geometryInLocal.StartPoint);
            var endPoint = matrixToGlobal.Transform(geometryInLocal.EndPoint);

            if (geometryInLocal is Arc arc)
            {
                var arcMidPoint = matrixToGlobal.Transform(arc.ArcMiddlePoint);
                return new Arc(startPoint, endPoint, arcMidPoint);
            }
            else if (geometryInLocal is LineSegment segment)
            {
                return new LineSegment(startPoint, endPoint);
            }

            throw new ArgumentException("Unknown geometry to transform");
        }

        /// <summary>
        /// Transforms a given polycurve in the local coordsys to global.
        /// </summary>
        /// <param name="curveInLocal">Curve in local coordinates.</param>
        /// <returns>The transformed polycurve in global coordinates.</returns>
        private Polycurve TransformToGlobal(Polycurve curveInLocal)
        {
            var coordSys = GetBaseCoordsys(this.BaseCentroid, this.Normal);
            var toGlobal = MatrixFactory.FromCoordinateSystem(coordSys);

            var transformedCurves = curveInLocal.Select(c => TransformGeometryByMatrix(c, toGlobal));
            var builder = new PolycurveGeometryBuilder();

            foreach (var curve in transformedCurves)
            {
                if (curve is Arc arc)
                {
                    builder.Append(arc);
                }
                else if (curve is LineSegment segment)
                {
                    builder.Append(segment);
                }
            }

            return builder.GetPolycurve();
        }
    }
}
