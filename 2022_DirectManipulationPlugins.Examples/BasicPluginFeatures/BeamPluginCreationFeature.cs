namespace BasicPluginFeatures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools.Picking;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Utilities;

    /// <summary>
    /// Direct Manipulation creation feature for the BeamPlugin class.
    /// </summary>
    public class BeamPluginCreationFeature : PluginCreationFeatureBase
    {
        /// <summary>
        /// The input range.
        /// </summary>
        private readonly InputRange inputRange;

        /// <summary>
        /// The current applied values.
        /// </summary>
        private Dictionary<string, object> currentAppliedValues;

        /// <summary>
        /// The picking tool.
        /// </summary>
        private PickingTool pickingTool;

        /// <summary>
        /// The picked points.
        /// </summary>
        private readonly List<Point> pickedPoints = new List<Point>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BeamPluginCreationFeature"/> class.
        /// </summary>
        public BeamPluginCreationFeature()
            : base(BasicPlugin.BeamPlugin.PluginName)
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
            this.pickingTool.PickSessionEnded += this.OnPickEnded;
            this.pickingTool.PickSessionInterrupted += this.OnSessionInterrupted;
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
            this.pickingTool.PickSessionEnded -= this.OnPickEnded;
            this.pickingTool.PickSessionInterrupted -= this.OnSessionInterrupted;
            this.pickingTool.PickUndone -= this.OnPickingUndone;
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PreviewRequested"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            this.currentAppliedValues = this.Component.GetAppliedAttributes();
            var profile = BasicPlugin.BeamPlugin.DefaultProfileName;
            var lengthFactor = 2.0;

            if (this.currentAppliedValues.TryGetValue("Profile", out var profileValue))
            {
                profile = profileValue.ToString();
            }

            if (this.currentAppliedValues.TryGetValue("LengthFactor", out var lengthFactorValue))
            {
                double.TryParse(lengthFactorValue.ToString(), out lengthFactor);
            }

            if (this.pickedPoints.Any())
            {
                var lengthVector = new Vector(eventArgs.HitPoint - this.pickedPoints.Last()) * lengthFactor;
                this.Graphics.DrawProfile(profile, new LineSegment(this.pickedPoints.Last(), lengthVector + this.pickedPoints.Last()), new Vector(0, 0, -150), rotationInDegrees: 90);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.ObjectPicked"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
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
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnInputValidationRequested(object sender, InputValidationEventArgs eventArgs)
        {
            // This is for simply illustrative purposes. The proper way
            // to get this same functionality is to set the input range to 
            // be exactly 2. The API takes care to keep the session going
            // until the minimun amount has been picked.

            // NOTE: When the session has been interrupted by the user, setting
            // the ContinueSession to true has no effect.
            if (this.pickedPoints.Count < Math.Max(this.inputRange.Minimum, 2))
            {
                eventArgs.ContinueSession = true;
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnPickEnded(object sender, EventArgs eventArgs)
        {
            var input = new ComponentInput();
            input.AddInputPolygon(new Polygon { Points = new ArrayList(this.pickedPoints) });
            this.CommitComponentInput(input);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (this.pickedPoints.Count > 0)
            {
                this.pickedPoints.RemoveAt(this.pickedPoints.Count - 1);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionInterrupted"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void OnSessionInterrupted(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("Picking session got interrtupted!");
        }
    }
}
