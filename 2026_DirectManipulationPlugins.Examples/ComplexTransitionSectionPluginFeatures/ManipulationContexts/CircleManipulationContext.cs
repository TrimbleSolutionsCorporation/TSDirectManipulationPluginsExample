namespace ComplexTransitionSectionPluginFeatures.ManipulationContexts
{
    using ComplexTransitionSectionPlugin;
    using System;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;

    /// <summary>
    /// Represents a manipulation context to manipulate circles.
    /// </summary>
    public class CircleManipulationContext : ManipulationContext
    {
        /// <summary>
        /// The handle manager.
        /// </summary>
        private readonly IHandleManager handleManager;

        /// <summary>
        /// The radius manipulator at the top.
        /// </summary>
        private PointHandle radiusHandle;

        /// <summary>
        /// The manipulated circle.
        /// </summary>
        private Arc managedCircle;

        /// <summary>
        /// Creates an instance of the <see cref="CircleManipulationContext"/> class.
        /// </summary>
        /// <param name="component">The transition section component.</param>
        /// <param name="parentFeature">The parent feature.</param>
        public CircleManipulationContext(Component component, PluginManipulationFeatureBase parentFeature)
            : base(component, parentFeature)
        {
            this.handleManager = this.ParentFeature.HandleManager;
            this.managedCircle = this.GetCircle();
            this.CreateRadiusHandle();
        }

        /// <inheritdoc/>
        public override void UpdateContext()
        {
            base.UpdateContext();
            this.Component.Select();
            this.managedCircle = this.GetCircle();
            this.radiusHandle.Point = this.managedCircle.StartPoint;
            this.DrawRadialLine(this.managedCircle.Radius, this.managedCircle.StartPoint);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();
            this.radiusHandle.Dispose();
        }

        /// <summary>
        /// Creates the point handle to manipulate the radius of the circle.
        /// </summary>
        private void CreateRadiusHandle()
        {
            this.radiusHandle = this.handleManager.CreatePointHandle(this.managedCircle.StartPoint, HandleLocationType.InputPoint, HandleEffectType.Geometry);
            this.AttachHandlers();
        }

        /// <summary>
        /// Attaches the event handlers to the events of each point handle.
        /// </summary>
        private void AttachHandlers()
        {
            if (this.radiusHandle == null)
            {
                return;
            }

            this.radiusHandle.DragStarted += this.OnRadiusDragStarted;
            this.radiusHandle.DragOngoing += this.OnRadiusPointDragOngoing;
            this.radiusHandle.DragEnded += this.OnRadiusPointDragEnded;
        }

        /// <summary>
        /// Detaches the event handlers.
        /// </summary>
        private void DetachHandlers()
        {
            if (this.radiusHandle == null)
            {
                return;
            }

            this.radiusHandle.DragStarted -= this.OnRadiusDragStarted;
            this.radiusHandle.DragOngoing -= this.OnRadiusPointDragOngoing;
            this.radiusHandle.DragEnded -= this.OnRadiusPointDragEnded;
        }

        /// <summary>
        /// Called while the radius point dragging is started.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnRadiusDragStarted(object sender, DragEventArgs eventArgs)
        {
            this.Component.Select();
        }

        /// <summary>
        /// Called while the radius point is being dragged. It will reposition the point in the plane of the circle and
        /// round the radius.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">Event args.</param>
        private void OnRadiusPointDragOngoing(object sender, DragEventArgs eventArgs)
        {
            this.Graphics.Clear();
            if (!(sender is PointHandle radiusHandle))
            {
                return;
            }

            var pointOnPlane = Projection.PointToPlane(radiusHandle.Point, new GeometricPlane(this.managedCircle.CenterPoint, this.managedCircle.Normal));
            var radialVector = new Vector(pointOnPlane - this.managedCircle.CenterPoint);
            double newRadius = radialVector.GetLength();
            if (newRadius < GeometryConstants.DISTANCE_EPSILON)
            {
                radiusHandle.IsInvalid = true;
                return;
            }

            double roundedRadius = Math.Round(newRadius);
            var roundedRadialVector = roundedRadius * radialVector.GetNormal();

            var roundedPoint = this.managedCircle.CenterPoint + roundedRadialVector;
            radiusHandle.Point = roundedPoint;

            var previewArc = new Arc(this.managedCircle.CenterPoint, this.managedCircle.StartDirection, this.managedCircle.StartTangent, roundedRadius, 2 * Math.PI);

            this.Graphics.DrawArc(previewArc);
            this.DrawRadialLine(roundedRadius, roundedPoint);
        }

        /// <summary>
        /// Called while the radius point drag is ended. Will apply the changes to database.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">Event args.</param>
        private void OnRadiusPointDragEnded(object sender, DragEventArgs eventArgs)
        {
            if (!(sender is PointHandle radiusHandle))
            {
                return;
            }

            double newRadius = Distance.PointToPoint(radiusHandle.Point, this.managedCircle.CenterPoint);
            if (newRadius < GeometryConstants.DISTANCE_EPSILON)
            {
                return;
            }

            this.Component.SetAttribute(TransitionSectionPluginPropertyNames.CircleRadius, newRadius);
            this.Component.Modify();
            new Model().CommitChanges();
        }

        /// <summary>
        /// Draws a radial line.
        /// </summary>
        /// <param name="radius">Radius to write.</param>
        /// <param name="endPoint">End point of the line.</param>
        private void DrawRadialLine(double radius, Point endPoint)
        {
            this.Graphics.DrawLine(this.managedCircle.CenterPoint, endPoint);
            this.Graphics.DrawText($"r = {radius}", 0.5 * new Vector(endPoint + this.managedCircle.CenterPoint));
        }

        /// <summary>
        /// Gets the arc representing the circle at the top of the section.
        /// </summary>
        /// <returns>The arc.</returns>
        private Arc GetCircle()
        {
            var coordSys = DataFetcher.GetSectionAlignedCoordsys(this.Component);
            double sectionLength = DataFetcher.GetSectionLength(this.Component);

            var normal = coordSys.AxisX.Cross(coordSys.AxisY).GetNormal();
            var centerPoint = coordSys.Origin + sectionLength * normal;

            double radius = DataFetcher.GetRadius(this.Component);
            return new Arc(centerPoint, coordSys.AxisX, coordSys.AxisY, radius, 2 * Math.PI);
        }
    }
}
