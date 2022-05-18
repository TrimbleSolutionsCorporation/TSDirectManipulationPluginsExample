namespace Tekla.Structures.Plugins.DirectManipulation.Examples.Feature
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Core;
    using Core.Features;

    using CustomPartPlugin;

    using Geometry3d;

    using Model;

    using Services.Controls;

    using Services.Tools;
    using Services.Tools.Picking;
    using Services.Utilities;

    //using Properties = CustomPartPlugin.PluginPropertyNames;

    /// <summary>
    /// An example class of a Direct Manipulation API creation feature for a custom plugin.
    /// </summary>
    /// <seealso cref="CustomPartPluginCreationFeatureBase" />
    public class ExampleCustomPartCreationFeature : CustomPartPluginCreationFeatureBase
    {
        /// <summary>
        /// The plugin attributes.
        /// </summary>
        private Dictionary<string, object> pluginAttributes;

        /// <summary>
        /// The picking tool.
        /// </summary>
        private PickingTool pickingTool;

        /// <summary>
        /// The highlighter.
        /// </summary>
        private Highlighter highlighter;

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
            this.pickingTool?.Dispose();
            this.pickingTool = this.CreatePickingTool(InputRange.Exactly(2), InputTypes.Point);

            this.pickingTool.PickSessionEnded += this.OnPickSessionEnded;
            this.pickingTool.PreviewRequested += this.OnPreviewRequested;
            this.pickingTool.PickSessionInterrupted += this.OnSessionInterrupted;
            this.pickingTool.PickUndone += this.OnPickUndone;

            //this.highlighter = this.CreateHighlighter();

            this.pickingTool.StartPickingSession("Pick two points.");
        }

        /// <inheritdoc />
        protected override void Refresh()
        {
            this.pickedObjects.Clear();
            //this.highlighter.ClearHighlights();
        }

        /// <inheritdoc />
        //protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        //{
        //    this.FetchPluginAttributes();

        //    var button = toolbar.CreateButton("Start picking!");
        //    button.Tooltip = "Helpful tooltips for everyone!";
        //    button.Clicked += (sender, eventArgs) =>
        //    {
        //        this.FetchPluginAttributes();
        //        this.pickingTool?.StartPickingSession("Pick 2 points");
        //    };
        //}

        ///// <summary>
        ///// Extract line segments from points.
        ///// </summary>
        ///// <param name="points">The points.</param>
        ///// <returns>The <see cref="IEnumerable{LineSegment}"/> of line segments.</returns>
        //private static IEnumerable<LineSegment> PointsToSegments(IEnumerable<Point> points)
        //{
        //    return points.Zip(points.Skip(1), (p1, p2) => new LineSegment(p1, p2)).Where(s => !IsZeroLength(s));
        //}

        ///// <summary>
        ///// Check if the segment has a zero length relative to the epsilon.
        ///// </summary>
        ///// <param name="segment">The line segment</param>
        ///// <returns><see cref="bool"/> whether the length is zero.</returns>
        //private static bool IsZeroLength(LineSegment segment)
        //{
        //    return segment.Length() < GeometryConstants.DISTANCE_EPSILON;
        //}

        /// <summary>
        /// Fetch the plugin attributes.
        /// </summary>
        private void FetchPluginAttributes()
        {
            this.pluginAttributes = this.CustomPart.GetAppliedAttributes();
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PreviewRequested"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ToleratedObjectEventArgs"/> instance containing the event data.</param>
        private void OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            if (!this.pickedObjects.Any())
            {
                return;
            }

            var profile = CustomPartPlugin.DefaultProfileName;
            if (this.pluginAttributes.TryGetValue("Profile", out var value))
            {
                profile = value.ToString();
            }

            var points = this.pickedObjects.Select(o => o.HitPoint).Concat(new[] { eventArgs.HitPoint });

            var lengthVector = new Vector(eventArgs.HitPoint - points.Last());

            this.Graphics.DrawProfile(profile, new LineSegment(points.Last(), 
                lengthVector + points.Last()), new Vector(0, 0, -150), rotationInDegrees: 90);
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
            this.highlighter.ClearHighlights();
            this.pickedObjects.RemoveAt(this.pickedObjects.Count - 1);
        }
    }
}
