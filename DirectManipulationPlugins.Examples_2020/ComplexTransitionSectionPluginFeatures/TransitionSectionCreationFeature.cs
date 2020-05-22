namespace ComplexTransitionSectionPluginFeatures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ComplexTransitionSectionPlugin;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Controls;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools.Picking;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Utilities;

    /// <summary>
    /// Direct Manipulation feature for creating the transition section plugin.
    /// </summary>
    public sealed class TransitionSectionCreationFeature : PluginCreationFeatureBase
    {
        /// <summary>
        /// The input range.
        /// </summary>
        private readonly InputRange inputRange;

        /// <summary>
        /// Value box that contains the radius of the circular section.
        /// </summary>
        private ValueBoxControl radiusValueBox;

        /// <summary>
        /// Value box that contains the width of the rectangular section.
        /// </summary>
        private ValueBoxControl widthValueBox;

        /// <summary>
        /// Value box that contains the height of the rectangular section.
        /// </summary>
        private ValueBoxControl heightValueBox;

        /// <summary>
        /// The picking tool.
        /// </summary>
        private PickingTool pickingTool;

        /// <summary>
        /// The picked points.
        /// </summary>
        private readonly List<Point> pickedPoints = new List<Point>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionSectionCreationFeature"/> class.
        /// </summary>
        public TransitionSectionCreationFeature()
            : base(TransitionSectionPlugin.PluginName, useFeatureContextualToolBar: true)
        {
            this.inputRange = InputRange.AtMost(2);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            this.DetachHandlers();
            this.pickingTool?.Dispose();

            this.pickingTool = this.CreatePickingTool(this.inputRange, InputTypes.Point);
            this.AttachHandlers();

            this.pickingTool.StartPickingSession("Pick two points.");
        }

        /// <inheritdoc />
        protected override void Refresh()
        {
            this.pickedPoints.Clear();
        }

        /// <inheritdoc />
        protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        {
            this.radiusValueBox = toolbar.CreateValueTextBox(TransitionSectionPlugin.DefaultCircleRadius);
            this.radiusValueBox.Tooltip = "Top radius";
            this.radiusValueBox.Title = "r=";

            this.widthValueBox = toolbar.CreateValueTextBox(TransitionSectionPlugin.DefaultRectangleWidth);
            this.widthValueBox.Tooltip = "Bottom width";
            this.widthValueBox.Title = "w=";

            this.heightValueBox = toolbar.CreateValueTextBox(TransitionSectionPlugin.DefaultRectangleHeight);
            this.heightValueBox.Tooltip = "Bottom height";
            this.heightValueBox.Title = "h=";
        }

        /// <summary>
        /// Attaches handlers to the picking tool.
        /// </summary>
        private void AttachHandlers()
        {
            if (this.pickingTool == null)
            {
                return;
            }

            this.pickingTool.PreviewRequested += this.OnPreviewRequested;
            this.pickingTool.ObjectPicked += this.OnObjectPicked;
            this.pickingTool.InputValidationRequested += this.OnInputValidationRequested;
            this.pickingTool.PickSessionEnded += this.OnPickSessionEnded;
            this.pickingTool.PickUndone += this.OnPickingUndone;
        }

        /// <summary>
        /// Detaches handlers from the picking tool.
        /// </summary>
        private void DetachHandlers()
        {
            if (this.pickingTool == null)
            {
                return;
            }

            this.pickingTool.PreviewRequested -= this.OnPreviewRequested;
            this.pickingTool.ObjectPicked -= this.OnObjectPicked;
            this.pickingTool.InputValidationRequested -= this.OnInputValidationRequested;
            this.pickingTool.PickSessionEnded += this.OnPickSessionEnded;
            this.pickingTool.PickUndone -= this.OnPickingUndone;
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PreviewRequested"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ToleratedObjectEventArgs"/> instance containing the event data.</param>
        private void OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            if (this.pickedPoints.Count < 1)
            {
                return;
            }

            var centroid = this.pickedPoints.Last();
            var sectionLength = Distance.PointToPoint(eventArgs.HitPoint, centroid);

            var distances = new[] { this.heightValueBox.Value, this.widthValueBox.Value, this.radiusValueBox.Value, sectionLength };
            if (distances.Any(x => x < GeometryConstants.DISTANCE_EPSILON))
            {
                return;
            }

            var normal = new Vector(eventArgs.HitPoint - centroid);
            var transitionSection = new TransitionSectionGeometry(
                this.heightValueBox.Value,
                this.widthValueBox.Value,
                sectionLength,
                this.radiusValueBox.Value,
                centroid,
                normal);

            this.DrawTransitionSection(transitionSection);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.ObjectPicked"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ToleratedObjectEventArgs"/> instance containing the event data.</param>
        private void OnObjectPicked(object sender, ToleratedObjectEventArgs eventArgs)
        {
            if (!eventArgs.IsValid)
            {
                return;
            }

            this.pickedPoints.Add(eventArgs.HitPoint);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.InputValidationRequested"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="InputValidationEventArgs"/> instance containing the event data.</param>
        private void OnInputValidationRequested(object sender, InputValidationEventArgs eventArgs)
        {
            if (this.pickedPoints.Count < Math.Max(this.inputRange.Minimum, 2))
            {
                eventArgs.ContinueSession = true;
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnPickSessionEnded(object sender, EventArgs eventArgs)
        {
            var input = new ComponentInput();
            input.AddInputPolygon(new Polygon { Points = new ArrayList(this.pickedPoints) });

            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.RectangleWidth, this.widthValueBox.Value);
            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.RectangleHeight, this.heightValueBox.Value);
            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.CircleRadius, this.radiusValueBox.Value);

            this.CommitComponentInput(input);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (this.pickedPoints.Count > 0)
            {
                this.pickedPoints.RemoveAt(this.pickedPoints.Count - 1);
            }
        }

        /// <summary>
        /// Draws a preview of the transition section that will be inserted.
        /// </summary>
        /// <param name="geometry">Geometry of the transition section.</param>
        private void DrawTransitionSection(TransitionSectionGeometry geometry)
        {
            this.DrawPolycurve(geometry.RectangularSection);
            this.DrawPolycurve(geometry.CircularSection);

            var conicalSections = geometry.RectangularSection
                .OfType<Arc>()
                .Zip(geometry.CircularSection.Select(c => c as Arc), (b, t) => new { Bottom = b, Top = t });

            foreach (var cone in conicalSections)
            {
                this.Graphics.DrawLine(cone.Bottom.StartPoint, cone.Top.StartPoint);
                this.Graphics.DrawLine(cone.Bottom.EndPoint, cone.Top.EndPoint);
            }
        }

        /// <summary>
        /// Draws the preview of a polycurve.
        /// </summary>
        /// <param name="polycurve">Curve to draw.</param>
        private void DrawPolycurve(Polycurve polycurve)
        {
            foreach (var curve in polycurve)
            {
                if (curve is LineSegment segment)
                {
                    this.Graphics.DrawLine(segment.StartPoint, segment.EndPoint);
                }
                else if (curve is Arc arc)
                {
                    this.Graphics.DrawArc(arc);
                }
            }
        }
    }
}
