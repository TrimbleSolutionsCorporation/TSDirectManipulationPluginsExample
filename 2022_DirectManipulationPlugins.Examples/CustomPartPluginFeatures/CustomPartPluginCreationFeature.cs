namespace CustomPartPluginFeatures
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

    using CustomPartPlugin;

    /// <summary>
    /// An example class of a Direct Manipulation API creation feature for a custom plugin.
    /// </summary>
    /// <seealso cref="CustomPartPluginCreationFeatureBase" />
    public class ExampleCustomPartCreationFeature : CustomPartPluginCreationFeatureBase
    {
        /// <summary>
        /// The current applied values.
        /// </summary>
        private Dictionary<string, object> currentAppliedValues;

        /// <summary>
        /// The input range.
        /// </summary>
        private readonly InputRange inputRange;

        /// <summary>
        /// The plugin attributes.
        /// </summary>
        private Dictionary<string, object> pluginAttributes;

        /// <summary>
        /// The picking tool.
        /// </summary>
        private PickingTool pickingTool;

        /// <summary>
        /// The picked points.
        /// </summary>
        private readonly List<Point> pickedPoints = new List<Point>();

        /// <summary>
        /// The picked points.
        /// </summary>
        private readonly List<ToleratedObjectEventArgs> pickedObjects = new List<ToleratedObjectEventArgs>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleCustomPartCreationFeature"/> class.
        /// </summary>
        public ExampleCustomPartCreationFeature()
            : base(CustomPartPlugin.PluginName, false, 2)
        {
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
            this.pickedObjects.Clear();
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
            this.pickingTool.PickSessionInterrupted += this.OnSessionInterrupted;
            this.pickingTool.PickUndone += this.OnPickUndone;
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
            this.pickingTool.PickSessionEnded -= this.OnPickSessionEnded;
            this.pickingTool.PickSessionInterrupted -= this.OnSessionInterrupted;
            this.pickingTool.PickUndone -= this.OnPickUndone;
        }

        /// <summary>
        /// Fetch the plugin attributes.
        /// </summary>
        private void FetchPluginAttributes()
        {
            this.pluginAttributes = this.CustomPart.GetAppliedAttributes();
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
        /// Event handler for the <see cref="PickingTool.PreviewRequested"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ToleratedObjectEventArgs"/> instance containing the event data.</param>
        private void OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            this.currentAppliedValues = this.CustomPart.GetAppliedAttributes();
            var profile = CustomPartPlugin.DefaultProfileName;
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
        /// Event handler for the <see cref="PickingTool.PickSessionEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnPickSessionEnded(object sender, EventArgs eventArgs)
        {
            this.Graphics.Clear();
            if (this.pickedObjects.Count == 2)
            {
                var input = new Polygon() { Points = new ArrayList(this.pickedObjects.Select(o => o.HitPoint).ToList()) };
                this.CommitCustomPartInput(input);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionInterrupted"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnSessionInterrupted(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("Picking session got interrtupted!");
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnPickUndone(object sender, EventArgs eventArgs)
        {
            this.pickedObjects.RemoveAt(this.pickedObjects.Count - 1);
        }
    }
}
