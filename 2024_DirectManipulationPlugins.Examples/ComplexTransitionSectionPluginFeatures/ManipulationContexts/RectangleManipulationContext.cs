namespace ComplexTransitionSectionPluginFeatures.ManipulationContexts
{
    using System.Collections.Generic;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;

    using ComplexTransitionSectionPlugin;

    /// <summary>
    /// Manipulates the rectangular part of the transition section plugin.
    /// </summary>
    public class RectangleManipulationContext : ManipulationContext
    {
        /// <summary>
        /// The handle manager.
        /// </summary>
        private readonly IHandleManager handleManager;

        /// <summary>
        /// The handle to manipulate the top of the rectangle.
        /// </summary>
        private LineHandle topHeightHandle;

        /// <summary>
        /// The handle to manipulate the bottom of the rectangle.
        /// </summary>
        private LineHandle bottomHeightHandle;

        /// <summary>
        /// The handle to manipulate the left side of the rectangle.
        /// </summary>
        private LineHandle leftWidthHandle;

        /// <summary>
        /// The handle to manipulate the right side of the rectangle.
        /// </summary>
        private LineHandle rightWidthHandle;

        /// <summary>
        /// Creates an instance of the <see cref="RectangleManipulationContext"/>
        /// </summary>
        /// <param name="component">The transition section plugin</param>
        /// <param name="feature">The owning feature</param>
        public RectangleManipulationContext(Component component, PluginManipulationFeatureBase feature)
            : base(component, feature)
        {
            this.handleManager = this.ParentFeature.HandleManager;
            this.CreateHandles();
        }

        /// <inheritdoc/>
        public override void UpdateContext()
        {
            base.UpdateContext();
            this.Component.Select();

            var segments = this.GetRectangle();

            this.rightWidthHandle.Line = segments[0];
            this.topHeightHandle.Line = segments[1];
            this.leftWidthHandle.Line = segments[2];
            this.bottomHeightHandle.Line = segments[3];
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();
            this.rightWidthHandle.Dispose();
            this.topHeightHandle.Dispose();
            this.leftWidthHandle.Dispose();
            this.bottomHeightHandle.Dispose();
        }

        /// <summary>
        /// Creates the handles for the manipulation of the object.
        /// </summary>
        private void CreateHandles()
        {
            var segments = this.GetRectangle();
            this.rightWidthHandle = this.handleManager.CreateLineHandle(segments[0], HandleLocationType.Other, HandleEffectType.Geometry);
            this.topHeightHandle = this.handleManager.CreateLineHandle(segments[1], HandleLocationType.Other, HandleEffectType.Geometry);
            this.leftWidthHandle = this.handleManager.CreateLineHandle(segments[2], HandleLocationType.Other, HandleEffectType.Geometry);
            this.bottomHeightHandle = this.handleManager.CreateLineHandle(segments[3], HandleLocationType.Other, HandleEffectType.Geometry);

            this.AttachHandlers();
        }

        /// <summary>
        /// Attaches the event handlers to the events of each line handle.
        /// </summary>
        private void AttachHandlers()
        {
            if (this.rightWidthHandle == null)
            {
                return;
            }

            this.rightWidthHandle.DragStarted += this.OnDragStarted;
            this.rightWidthHandle.DragOngoing += this.OnWidthDragOngoing;
            this.rightWidthHandle.DragEnded += this.OnDragEnded;

            if (this.topHeightHandle == null)
            {
                return;
            }

            this.topHeightHandle.DragStarted += this.OnDragStarted;
            this.topHeightHandle.DragOngoing += this.OnHeightDragOngoing;
            this.topHeightHandle.DragEnded += this.OnDragEnded;

            if (this.leftWidthHandle == null)
            {
                return;
            }

            this.leftWidthHandle.DragStarted += this.OnDragStarted;
            this.leftWidthHandle.DragOngoing += this.OnWidthDragOngoing;
            this.leftWidthHandle.DragEnded += this.OnDragEnded;

            if (this.bottomHeightHandle == null)
            {
                return;
            }

            this.bottomHeightHandle.DragStarted += this.OnDragStarted;
            this.bottomHeightHandle.DragOngoing += this.OnHeightDragOngoing;
            this.bottomHeightHandle.DragEnded += this.OnDragEnded;
        }

        /// <summary>
        /// Detaches the event handlers.
        /// </summary>
        private void DetachHandlers()
        {
            if (this.rightWidthHandle == null)
            {
                return;
            }

            this.rightWidthHandle.DragStarted -= this.OnDragStarted;
            this.rightWidthHandle.DragOngoing -= this.OnWidthDragOngoing;
            this.rightWidthHandle.DragEnded -= this.OnDragEnded;

            if (this.topHeightHandle == null)
            {
                return;
            }

            this.topHeightHandle.DragStarted -= this.OnDragStarted;
            this.topHeightHandle.DragOngoing -= this.OnHeightDragOngoing;
            this.topHeightHandle.DragEnded -= this.OnDragEnded;

            if (this.leftWidthHandle == null)
            {
                return;
            }

            this.leftWidthHandle.DragStarted -= this.OnDragStarted;
            this.leftWidthHandle.DragOngoing -= this.OnWidthDragOngoing;
            this.leftWidthHandle.DragEnded -= this.OnDragEnded;

            if (this.bottomHeightHandle == null)
            {
                return;
            }

            this.bottomHeightHandle.DragStarted -= this.OnDragStarted;
            this.bottomHeightHandle.DragOngoing -= this.OnHeightDragOngoing;
            this.bottomHeightHandle.DragEnded -= this.OnDragEnded;
        }

        /// <summary>
        /// Called when any segment has started a drag.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnDragStarted(object sender, DragEventArgs eventArgs)
        {
            this.Component.Select();
        }

        /// <summary>
        /// Called when a width segment is being dragged.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnWidthDragOngoing(object sender, DragEventArgs eventArgs)
        {
            if (!(sender is LineHandle lineHandle))
            {
                return;
            }

            var segment = lineHandle.Line;
            var centroid = DataFetcher.GetSectionAlignedCoordsys(this.Component).Origin;

            var halfWidth = Distance.PointToLine(centroid, new Line(segment));

            this.rightWidthHandle.Line = PlaceAtDistanceFromPoint(this.rightWidthHandle.Line, centroid, halfWidth);
            this.leftWidthHandle.Line = PlaceAtDistanceFromPoint(this.leftWidthHandle.Line, centroid, halfWidth);

            this.Graphics.Clear();
            this.DrawRectangle(2.0 * halfWidth, DataFetcher.GetBaseHeight(this.Component));
        }

        /// <summary>
        /// Called when a height segment is being dragged.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnHeightDragOngoing(object sender, DragEventArgs eventArgs)
        {
            if (!(sender is LineHandle lineHandle))
            {
                return;
            }

            var segment = lineHandle.Line;
            var centroid = DataFetcher.GetSectionAlignedCoordsys(this.Component).Origin;

            var halfHeight = Distance.PointToLine(centroid, new Line(segment));

            this.topHeightHandle.Line = PlaceAtDistanceFromPoint(this.topHeightHandle.Line, centroid, halfHeight);
            this.bottomHeightHandle.Line = PlaceAtDistanceFromPoint(this.bottomHeightHandle.Line, centroid, halfHeight);

            this.Graphics.Clear();
            this.DrawRectangle(DataFetcher.GetBaseWidth(this.Component), 2.0 * halfHeight);
        }

        /// <summary>
        /// Called when any segment has ended a drag.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">Event args.</param>
        private void OnDragEnded(object sender, DragEventArgs eventArgs)
        {
            var centroid = DataFetcher.GetSectionAlignedCoordsys(this.Component).Origin;

            var height = 2.0 * Distance.PointToLine(centroid, new Line(this.topHeightHandle.Line));
            var width = 2.0 * Distance.PointToLine(centroid, new Line(this.rightWidthHandle.Line));

            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.RectangleWidth, width);
            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.RectangleHeight, height);

            this.Component.Modify();
            new Model().CommitChanges();
        }

        /// <summary>
        /// Draws the preview of the base of the transition section.
        /// </summary>
        /// <param name="width">Width of the base.</param>
        /// <param name="height">Height of the base.</param>
        private void DrawRectangle(double width, double height)
        {
            var coordSys = DataFetcher.GetSectionAlignedCoordsys(this.Component);

            var radius = DataFetcher.GetRadius(this.Component);
            var length = DataFetcher.GetSectionLength(this.Component);
            var geometry = new TransitionSectionGeometry(height, width, length, radius, coordSys.Origin, coordSys.AxisX.Cross(coordSys.AxisY));

            var rectangle = geometry.RectangularSection;
            foreach (var curve in rectangle)
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

        /// <summary>
        /// Places a line segment at a given distance from a point.
        /// </summary>
        /// <param name="segmentToPlace">Segment to place.</param>
        /// <param name="referencePoint">Reference point.</param>
        /// <param name="desiredDistance">Desired distance from the point.</param>
        /// <returns>A segment parallel to the given at a the given distance from the reference point.</returns>
        private static LineSegment PlaceAtDistanceFromPoint(LineSegment segmentToPlace, Point referencePoint, double desiredDistance)
        {
            var pointOnLine = Projection.PointToLine(referencePoint, new Line(segmentToPlace));

            var currentDistanceVector = new Vector(pointOnLine - referencePoint);
            var desiredDistanceVector = currentDistanceVector.GetNormal() * desiredDistance;

            var neededShift = desiredDistanceVector - currentDistanceVector;
            return new LineSegment(segmentToPlace.StartPoint + neededShift, segmentToPlace.EndPoint + neededShift);
        }

        /// <summary>
        /// Gets the rectangle, with the first segment being the right segment of the base, and going into counter
        /// clockwise direction.
        /// </summary>
        /// <returns>The list of segments.</returns>
        private List<LineSegment> GetRectangle()
        {
            var coordSys = DataFetcher.GetSectionAlignedCoordsys(this.Component);

            var height = DataFetcher.GetBaseHeight(this.Component);
            var width = DataFetcher.GetBaseWidth(this.Component);
            var radius = DataFetcher.GetRadius(this.Component);
            var length = DataFetcher.GetSectionLength(this.Component);
            var geometry = new TransitionSectionGeometry(height, width, length, radius, coordSys.Origin, coordSys.AxisX.Cross(coordSys.AxisY));

            return geometry.RectangularSection.OfType<LineSegment>().ToList();
        }
    }
}
