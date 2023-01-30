namespace ComplexTransitionSectionPluginFeatures
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    using ComplexTransitionSectionPlugin;

    /// <summary>
    /// Fetches data related to a transition section.
    /// </summary>
    public static class DataFetcher
    {
        /// <summary>
        /// Gets the top radius of the transition section.
        /// </summary>
        /// <param name="transitionSection">The section</param>
        /// <returns>The radius</returns>
        public static double GetRadius(Component transitionSection)
        {
            double radius = TransitionSectionPlugin.DefaultCircleRadius;
            transitionSection.GetAttribute(TransitionSectionPluginPropertyNames.CircleRadius, ref radius);

            return radius;
        }

        /// <summary>
        /// Gets the base width of the transition section.
        /// </summary>
        /// <param name="transitionSection">The section</param>
        /// <returns>The width</returns>
        public static double GetBaseWidth(Component transitionSection)
        {
            double width = TransitionSectionPlugin.DefaultRectangleWidth;
            transitionSection.GetAttribute(TransitionSectionPluginPropertyNames.RectangleWidth, ref width);

            return width;
        }

        /// <summary>
        /// Gets the base height of the transition section.
        /// </summary>
        /// <param name="transitionSection">The section</param>
        /// <returns>The height</returns>
        public static double GetBaseHeight(Component transitionSection)
        {
            double height = TransitionSectionPlugin.DefaultRectangleHeight;
            transitionSection.GetAttribute(TransitionSectionPluginPropertyNames.RectangleHeight, ref height);

            return height;
        }

        /// <summary>
        /// Gets the definition points of the transition section.
        /// </summary>
        /// <param name="transitionSection">The transition section.</param>
        /// <returns>The definition points of the transition section.</returns>
        private static List<Point> GetDefinitionPoints(Component transitionSection)
        {
            var componentInput = transitionSection.GetComponentInput();
            var pointList = new List<Point>();

            if (componentInput == null)
            {
                return pointList;
            }

            foreach (InputItem item in componentInput)
            {
                switch (item.GetInputType())
                {
                    case InputItem.InputTypeEnum.INPUT_1_POINT:
                        pointList.Add(item.GetData() as Point);
                        break;

                    case InputItem.InputTypeEnum.INPUT_2_POINTS:
                        pointList.AddRange((item.GetData() as ArrayList ?? new ArrayList()).Cast<Point>());
                        break;

                    case InputItem.InputTypeEnum.INPUT_POLYGON:
                        pointList.AddRange((item.GetData() as ArrayList ?? new ArrayList()).Cast<Point>());
                        break;
                }
            }

            return pointList;
        }

        /// <summary>
        /// Gets the base height of the transition section.
        /// </summary>
        /// <param name="transitionSection">The section</param>
        /// <returns>The length</returns>
        public static double GetSectionLength(Component transitionSection)
        {
            var pointList = GetDefinitionPoints(transitionSection);
            if (pointList.Count >= 2)
            {
                return Distance.PointToPoint(pointList[0], pointList[1]);
            }

            return TransitionSectionPlugin.DefaultSectionLength;
        }

        /// <summary>
        /// Gets a coordinate system aligned with the transition section.
        /// </summary>
        /// <param name="transitionSection">The section</param>
        /// <returns>The coordsys.</returns>
        public static CoordinateSystem GetSectionAlignedCoordsys(Component transitionSection)
        {
            var pointList = GetDefinitionPoints(transitionSection);
            if (pointList.Count >= 2)
            {
                return TransitionSectionGeometry.GetBaseCoordsys(pointList[0], new Vector(pointList[1] - pointList[0]));
            }

            return new CoordinateSystem();
        }
    }
}
