namespace CustomPartPluginFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Utilities;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;

    /// <summary>
    /// The <see cref="CustomPartPluginManipulationContext"/> class represents an example manipulator for the <see cref="CustomPartPluginManipulationFeature"/>.
    /// </summary>
    /// <seealso cref="ManipulationContext" />
    public sealed class CustomPartPluginManipulationContext : ManipulationContext
    {
        /// <summary>
        /// The handle manager.
        /// </summary>
        private readonly IHandleManager handleManager;

        /// <summary>
        /// The point handles.
        /// </summary>
        private readonly List<PointHandle> pointHandles;

        /// <summary>
        /// The line handles.
        /// </summary>
        private readonly List<LineHandle> lineHandles;

        /// <summary>
        /// The manipulators.
        /// </summary>
        private readonly List<DistanceManipulator> manipulators;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPartPluginManipulationContext" /> class.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <param name="feature">The feature.</param>
        public CustomPartPluginManipulationContext(CustomPart component, CustomPartPluginManipulationFeatureBase feature)
            : base(component, feature)
        {
            this.handleManager = this.ParentFeature.HandleManager;
            this.pointHandles = this.CreatePointHandles(component);
            this.lineHandles = this.CreateLineHandles(component);
            this.pointHandles.ForEach(handle =>
            {
                handle.DragOngoing += this.OnPointHandleDragOngoing;
                handle.DragEnded += this.OnPointHandleDragEnded;
            });

            this.lineHandles.ForEach(handle =>
            {
                handle.DragOngoing += this.OnLineHandleDragOngoing;
                handle.DragEnded += this.OnLineHandleDragEnded;
            });

            this.manipulators = this.CreateManipulators(component, this.lineHandles);
            this.manipulators.ForEach(manipulator =>
            {
                manipulator.MeasureChanged += this.OnMeasureChanged;
                manipulator.MeasureChangeOngoing += this.OnMeasureChangeOngoing;
            });

            this.manipulators.ForEach(this.AddManipulator);
        }

        /// <inheritdoc />
        public override void UpdateContext()
        {
            this.UpdatePointHandles(this.CustomPart, this.pointHandles);
            this.UpdateLineHandles(this.CustomPart, this.lineHandles);
            this.UpdateDistanceManipulators();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }

            this.pointHandles.ForEach(handle =>
            {
                handle.DragOngoing -= this.OnPointHandleDragOngoing;
                handle.DragEnded -= this.OnPointHandleDragEnded;
            });

            this.lineHandles.ForEach(handle =>
            {
                handle.DragOngoing -= this.OnLineHandleDragOngoing;
                handle.DragEnded -= this.OnLineHandleDragEnded;
            });

            this.pointHandles.ForEach(handle => handle.Dispose());
            this.lineHandles.ForEach(handle => handle.Dispose());

            this.manipulators.ForEach(manipulator =>
            {
                manipulator.MeasureChanged -= this.OnMeasureChanged;
                manipulator.MeasureChangeOngoing -= this.OnMeasureChangeOngoing;
            });

            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }
        }

        /// <summary>
        /// Event handler for the <see cref="DraggableHandle.DragOngoing"/> event of <see cref="PointHandle"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnPointHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            this.DrawGraphics(this.pointHandles.Select(handle => handle.Point).ToList());
        }

        /// <summary>
        /// Event handler for the <see cref="DraggableHandle.DragEnded"/> event of <see cref="PointHandle"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnPointHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            this.ModifyInput(this.pointHandles.Select(handle => handle.Point).ToList());
        }

        /// <summary>
        /// Event handler for the <see cref="DraggableHandle.DragOngoing"/> event of <see cref="LineHandle"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnLineHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            this.DrawGraphics(this.GetLineHandlePoints(this.lineHandles, (LineHandle)sender));
        }

        /// <summary>
        /// Event handler for the <see cref="DraggableHandle.DragEnded"/> event of <see cref="LineHandle"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnLineHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            this.ModifyInput(this.GetLineHandlePoints(this.lineHandles, (LineHandle)sender));
        }

        /// <summary>
        /// Event handler for the <see cref="DistanceManipulator.MeasureChangeOngoing"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnMeasureChangeOngoing(object sender, EventArgs eventArgs)
        {
            this.DrawGraphics(this.pointHandles.Select(handle => handle.Point).ToList());
        }

        /// <summary>
        /// Event handler for the <see cref="DistanceManipulator.MeasureChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnMeasureChanged(object sender, EventArgs eventArgs)
        {
            var distanceManipulator = sender as DistanceManipulator;
            if (distanceManipulator == null)
            {
                return;
            }

            var currentManipulatorIndex = this.manipulators.IndexOf(distanceManipulator);
            var points = this.manipulators.Select(m => m.Segment.StartPoint).ToList();
            if (currentManipulatorIndex != this.manipulators.Count - 1)
            {
                points[currentManipulatorIndex + 1] = distanceManipulator.Segment.EndPoint;
            }

            points.Add(this.manipulators.Last().Segment.EndPoint);
            this.ModifyInput(points);
        }

        /// <summary>
        /// Draws the graphics.
        /// </summary>
        /// <param name="points">The points.</param>
        private void DrawGraphics(List<Point> points)
        {
            this.Graphics.Clear();
            var profile = "HEA200";

            string appliedProfile = string.Empty;
            this.CustomPart.GetAttribute("Profile", ref appliedProfile);

            if (!string.IsNullOrEmpty(appliedProfile))
                profile = appliedProfile;

            for (var i = 1; i < points.Count; i++)
            {
                var lineSegment = new LineSegment(points[i - 1], points[i]);
                this.Graphics.DrawProfile(profile, lineSegment, new Vector(0, 0, -100));
            }
        }

        /// <summary>
        /// Creates the manipulators.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <param name="handles">The handles.</param>
        /// <returns>
        /// A list of manipulators for this feature.
        /// </returns>
        private List<DistanceManipulator> CreateManipulators(
            CustomPart component,
            IEnumerable<LineHandle> handles)
        {
            var manipulatorList = new List<DistanceManipulator>();
            var distanceManipulators = handles.Select(
                    handle => new DistanceManipulator(component, this, handle.Line))
                .ToList();

            manipulatorList.AddRange(distanceManipulators);
            return manipulatorList;
        }

        /// <summary>
        /// Updates the instances of <see cref="DistanceManipulator"/>.
        /// </summary>
        private void UpdateDistanceManipulators()
        {
            for (var i = 0; i < this.lineHandles.Count; i++)
            {
                this.manipulators[i].Segment = this.lineHandles[i].Line;
            }
        }

        /// <summary>
        /// Creates the point handles.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <returns>A list of point handles based on the component input.</returns>
        private List<PointHandle> CreatePointHandles(CustomPart component)
        {
            var handles = new List<PointHandle>();
            var inputArrayList = component.GetInputPoints();

            foreach (Point point in inputArrayList)
            {
                var handle = this.handleManager.CreatePointHandle(point, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                this.SetHandleContextualToolbar(handle, t => this.DefinePointHandleContextualToolbar(handle, t));
                handles.Add(handle);
            }

            return handles;
        }

        /// <summary>
        /// Defines the point handle contextual toolbar.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="toolbar">The toolbar.</param>
        private void DefinePointHandleContextualToolbar(PointHandle handle, IToolbar toolbar)
        {
            var index = this.pointHandles.IndexOf(handle);
            toolbar.CreateButton("Success! " + index);
        }

        /// <summary>
        /// Updates the point handles for the custom part.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <param name="handles">The handles.</param>
        private void UpdatePointHandles(CustomPart component, IList<PointHandle> handles)
        {
            var inputArrayList = component.GetInputPoints();
            var index = 0;

            foreach (Point input in inputArrayList)
            {
                handles[index].Point = input;
                index++;
            }
        }

        /// <summary>
        /// Creates the line handles.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <returns>A list of line handles based on the componentContext input.</returns>
        private List<LineHandle> CreateLineHandles(CustomPart component)
        {
            var handles = new List<LineHandle>();
            var inputArrayList = component.GetInputPoints();

            for (var i = 1; i < inputArrayList.Count; i++)
            {
                var lineSegment = new LineSegment((Point)inputArrayList[i - 1], (Point)inputArrayList[i]);
                var handle = this.handleManager.CreateLineHandle(lineSegment, HandleLocationType.MidPoint, HandleEffectType.Geometry);
                handles.Add(handle);
            }

            return handles;
        }

        /// <summary>
        /// Updates the line handles.
        /// </summary>
        /// <param name="component">The custom part.</param>
        /// <param name="handles">The handles.</param>
        private void UpdateLineHandles(CustomPart component, IList<LineHandle> handles)
        {
            var inputArrayList = component.GetInputPoints();
            var index = 0;

            for (var i = 1; i < inputArrayList.Count; i++)
            {
                var lineSegment = new LineSegment((Point)inputArrayList[i - 1], (Point)inputArrayList[i]);
                handles[index].Line = lineSegment;
                index++;
            }
        }

        /// <summary>
        /// Gets the line handle points.
        /// </summary>
        /// <remarks>
        /// This method is used to define the proper placing of the line handle
        /// start and end points. The currently selected line handle is the one
        /// being moved around, and the rest of the points have to be retrieved
        /// based on that.
        /// </remarks>
        /// <param name="handles">The list of handles.</param>
        /// <param name="handle">The currently selected handle.</param>
        /// <returns>
        /// A list of the points of the line handles.
        /// </returns>
        private List<Point> GetLineHandlePoints(List<LineHandle> handles, LineHandle handle)
        {
            int currentHandleIndex = handles.IndexOf(handle);
            var points = this.manipulators.Select(m => m.Segment.StartPoint).ToList();
            if (currentHandleIndex != this.manipulators.Count - 1)
            {
                points[currentHandleIndex + 1] = handle.Line.EndPoint;
            }

            points.Add(handles.Last().Line.EndPoint);
            return points;
        }

        /// <summary>
        /// Modifies the input.
        /// </summary>
        /// <param name="points">The points.</param>
        private void ModifyInput(IList<Point> points)
        {
            this.Graphics.Clear();
            var originalInput = this.CustomPart.GetInputPoints();
            if (originalInput == null)
            {
                return;
            }

            var input = new Polygon();
            foreach (var point in points)
            {
                input.Points.Add(new Point(point));
            }

            this.ModifyCustomPartInput(input);
        }
    }
}
