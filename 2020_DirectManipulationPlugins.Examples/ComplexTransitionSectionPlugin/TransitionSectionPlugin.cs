namespace ComplexTransitionSectionPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;
    using Tekla.Structures.Plugins;

    using TSM = Tekla.Structures.Model;

    /// <summary>
    /// This example plugin creates a transition section from a rectangular steel pipe to a cylindrical pipe.
    /// The example leverages the interaction with the open DirectManipulation library, using the contextual toolbar
    /// and preview graphics.
    /// </summary>
    [Plugin(PluginName)]
    [PluginUserInterface("ComplexTransitionSectionPlugin.TransitionSectionForm")]
    public sealed class TransitionSectionPlugin : PluginBase
    {
        /// <summary>
        /// Gets the <see cref="TransitionSectionPluginData"/> data object of the plugin.
        /// </summary>
        private TransitionSectionPluginData Data { get; }

        /// <summary>
        /// Profile to use in the transition section example.
        /// </summary>
        private string sectionProfile;

        /// <summary>
        /// Material for the transition section.
        /// </summary>
        private string material;

        /// <summary>
        /// Finish for the transition section.
        /// </summary>
        private string finish;

        /// <summary>
        /// The rectangle width.
        /// </summary>
        private double rectangleWidth;

        /// <summary>
        /// The rectangle height.
        /// </summary>
        private double rectangleHeight;

        /// <summary>
        /// The circle radius
        /// </summary>
        private double circleRadius;

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public const string PluginName = "Transition section plugin example";

        /// <summary>
        /// The default rectangle width.
        /// </summary>
        public const double DefaultRectangleWidth = 3000.0;

        /// <summary>
        /// The default rectangle height.
        /// </summary>
        public const double DefaultRectangleHeight = 6000.0;

        /// <summary>
        /// The default circle radius.
        /// </summary>
        public const double DefaultCircleRadius = 1000.0;

        /// <summary>
        /// The default section length.
        /// </summary>
        public const double DefaultSectionLength = 1000.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionSectionPlugin"/> class.
        /// </summary>
        /// <param name="data">The <see cref="TransitionSectionPluginData"/> object for the plugin.</param>
        public TransitionSectionPlugin(TransitionSectionPluginData data)
        {
            this.Data = data;
        }

        /// <inheritdoc />
        public override bool Run(List<InputDefinition> input)
        {
            try
            {
                this.GetDataValues();
                var points = ((ArrayList)input[0].GetInput()).OfType<Point>().ToList();
                double transitionLength = Distance.PointToPoint(points[0], points[1]);

                var transitionSection = new TransitionSectionGeometry(this.rectangleHeight, this.rectangleWidth, transitionLength, this.circleRadius, points[0], new Vector(points[1] - points[0]));
                this.InsertTransitionSection(transitionSection);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Exception: " + ex);
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This function only gets called if there is no DM feature active.
        /// </remarks>
        public override List<InputDefinition> DefineInput()
        {
            var pointPicker = new Picker();

            var point1 = pointPicker.PickPoint();
            var point2 = pointPicker.PickPoint();

            var input = new InputDefinition(new ArrayList { point1, point2 });

            this.GetDataValues();

            return new List<InputDefinition> { input };
        }

        /// <summary>
        /// Inserts the full transition section in the model.
        /// </summary>
        /// <param name="geometry">Geometry descriptor of the transition section.</param>
        private void InsertTransitionSection(TransitionSectionGeometry geometry)
        {
            var rectangularBase = geometry.RectangularSection;
            var circle = geometry.CircularSection;

            var conicalSectionArcs = rectangularBase.OfType<Arc>()
                                                    .Zip(circle.Select(c => c as Arc), (b, t) => new { Bottom = b, Top = t })
                                                    .ToList();

            foreach (var arcs in conicalSectionArcs)
            {
                var loftedPlate = this.MakeConicalSection(arcs.Bottom, arcs.Top);
                loftedPlate.Insert();
            }

            for (var i = 0; i < conicalSectionArcs.Count; ++i)
            {
                var nextIdx = (i + 1) % conicalSectionArcs.Count;
                var plate = this.MakeTriangularSection(
                    conicalSectionArcs[i].Bottom, conicalSectionArcs[i].Top, conicalSectionArcs[nextIdx].Bottom);

                plate.Insert();
            }
        }

        /// <summary>
        /// Creates a conical section of the transition section.
        /// </summary>
        /// <param name="bottom">Bottom arc.</param>
        /// <param name="top">Top arc.</param>
        /// <returns>The conical section part.</returns>
        private TSM.LoftedPlate MakeConicalSection(Arc bottom, Arc top)
        {
            var loftedPlate = new TSM.LoftedPlate
            {
                BaseCurves = new List<ICurve> { bottom, top },
                FaceType = TSM.LoftedPlate.LoftedPlateFaceTypeEnum.Perpendicular,
                Finish = this.finish,
                Profile = { ProfileString = this.sectionProfile },
                Material = { MaterialString = this.material }
            };

            return loftedPlate;
        }

        /// <summary>
        /// Creates a triangular section for the transition section.
        /// </summary>
        /// <param name="bottom">Bottom arc of a conical section.</param>
        /// <param name="top">Top arc of a conical section.</param>
        /// <param name="nextBottom">Bottom arc of the next conical section.</param>
        /// <returns>The triangular section part.</returns>
        private TSM.ContourPlate MakeTriangularSection(Arc bottom, Arc top, Arc nextBottom)
        {
            var startPoint = new TSM.ContourPoint(bottom.EndPoint, new TSM.Chamfer());
            var bottomPoint = new TSM.ContourPoint(nextBottom.StartPoint, new TSM.Chamfer());
            var topPoint = new TSM.ContourPoint(top.EndPoint, new TSM.Chamfer());

            var triangularSection = new TSM.Contour()
            {
                ContourPoints = new ArrayList { startPoint, bottomPoint, topPoint }
            };

            var plate = new TSM.ContourPlate
            {
                Contour = triangularSection,
                Finish = this.finish,
                Profile = { ProfileString = this.sectionProfile },
                Material = { MaterialString = this.material }
            };

            return plate;
        }

        /// <summary>
        /// Gets the data values.
        /// </summary>
        private void GetDataValues()
        {
            this.sectionProfile = this.Data.SectionProfile;
            this.material = this.Data.Material;
            this.finish = this.Data.Finish;
            this.rectangleWidth = this.Data.RectangleWidth;
            this.rectangleHeight = this.Data.RectangleHeight;
            this.circleRadius = this.Data.CircleRadius;

            if (this.IsDefaultValue(this.sectionProfile))
                this.sectionProfile = "PL10";

            if (this.IsDefaultValue(this.material))
                this.material = "Steel_Undefined";

            if (this.IsDefaultValue(this.finish))
                this.finish = "PAINT";

            if (this.IsDefaultValue(this.rectangleWidth))
                this.rectangleWidth = DefaultRectangleWidth;

            if (this.IsDefaultValue(this.rectangleHeight))
                this.rectangleHeight = DefaultRectangleHeight;

            if (this.IsDefaultValue(this.circleRadius))
                this.circleRadius = DefaultCircleRadius;
        }
    }
}
