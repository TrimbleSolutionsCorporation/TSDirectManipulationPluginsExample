namespace BasicPluginFeatures
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;

    /// <summary>
    /// Manipulation context class for the <see cref="BeamPluginManipulationFeature"/> class.
    /// </summary>
    public sealed class BeamPluginManipulationContext : ManipulationContext
    {
        /// <summary>
        /// The handle manager.
        /// </summary>
        private IHandleManager handleManager;

        /// <summary>
        /// The point handles.
        /// </summary>
        private List<PointHandle> pointHandles;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeamPluginManipulationContext"/> class.
        /// </summary>
        /// <param name="component">The component to be manipulated.</param>
        /// <param name="feature">The parent feature.</param>
        public BeamPluginManipulationContext(Component component, PluginManipulationFeatureBase feature)
            : base(component, feature)
        {
            this.handleManager = feature.HandleManager;
            this.pointHandles = this.CreatePointHandles(component);
            this.AttachHandlers();
        }

        /// <inheritdoc />
        public override void UpdateContext()
        {
            this.UpdatePointHandles(this.Component, this.pointHandles);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();
            this.pointHandles.ForEach(handle => handle.Dispose());
        }

        /// <summary>
        /// Creates the point handles.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>A list of point handles based on the component input.</returns>
        private List<PointHandle> CreatePointHandles(Component component)
        {
            var handles = new List<PointHandle>();
            var inputArrayList = this.GetCurrentInput(component);

            foreach (Point point in inputArrayList)
            {
                var handle = this.handleManager.CreatePointHandle(point, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                handles.Add(handle);
            }

            return handles;
        }

        /// <summary>
        /// Updates the point handles.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="handles">The handles.</param>
        private void UpdatePointHandles(Component component, List<PointHandle> handles)
        {
            var inputArrayList = this.GetCurrentInput(component);
            var index = 0;

            foreach (Point input in inputArrayList)
            {
                handles[index].Point = input;
                index++;
            }
        }

        /// <summary>
        /// Gets the current input.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>An <see cref="ArrayList"/> containing the current component input.</returns>
        private ArrayList GetCurrentInput(Component component)
        {
            var inputArrayList = new ArrayList();
            var originalInput = component.GetComponentInput();

            if (originalInput == null)
            {
                return inputArrayList;
            }

            foreach (var inputItem in originalInput)
            {
                var item = inputItem as InputItem;
                if (item == null)
                {
                    continue;
                }

                switch (item.GetInputType())
                {
                    case InputItem.InputTypeEnum.INPUT_1_POINT:
                        inputArrayList.Add(item.GetData() as Point);
                        break;

                    case InputItem.InputTypeEnum.INPUT_2_POINTS:
                        inputArrayList.AddRange(item.GetData() as ArrayList ?? new ArrayList());
                        break;

                    case InputItem.InputTypeEnum.INPUT_POLYGON:
                        inputArrayList.AddRange(item.GetData() as ArrayList ?? new ArrayList());
                        break;

                    default:
                        break;
                }
            }

            return inputArrayList;
        }

        /// <summary>
        /// Modifies the input.
        /// </summary>
        /// <param name="points">The points.</param>
        private void ModifyInput(List<Point> points)
        {
            this.Graphics.Clear();
            var originalInput = this.Component.GetComponentInput();
            if (originalInput == null)
            {
                return;
            }

            var input = new ComponentInput();
            var index = 0;
            foreach (var inputItem in originalInput)
            {
                if (!(inputItem is InputItem item))
                {
                    continue;
                }

                switch (item.GetInputType())
                {
                    case InputItem.InputTypeEnum.INPUT_1_OBJECT:
                        input.AddInputObject(item.GetData() as ModelObject);
                        break;

                    case InputItem.InputTypeEnum.INPUT_N_OBJECTS:
                        input.AddInputObjects(item.GetData() as ArrayList);
                        break;

                    case InputItem.InputTypeEnum.INPUT_1_POINT:
                        input.AddOneInputPosition(points[index]);
                        index++;
                        break;

                    case InputItem.InputTypeEnum.INPUT_2_POINTS:
                        input.AddTwoInputPositions(points[index], points[index + 1]);
                        index += 2;
                        break;

                    case InputItem.InputTypeEnum.INPUT_POLYGON:
                        var polygon = new Polygon();
                        foreach (var point in points)
                        {
                            polygon.Points.Add(new Point(point));
                        }

                        input.AddInputPolygon(polygon);
                        break;

                    default:
                        break;
                }
            }

            this.ModifyComponentInput(input);
        }

        /// <summary>
        /// Event handler for the <see cref="PointHandle.DragOngoing"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnPointHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
        }

        /// <summary>
        /// Event handler for the <see cref="PointHandle.DragEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void OnPointHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            this.ModifyInput(this.pointHandles.Select(handle => handle.Point).ToList());
        }

        /// <summary>
        /// Attaches the event handlers to the events of each point handle.
        /// </summary>
        private void AttachHandlers()
        {
            this.pointHandles.ForEach(handle => 
            {
                handle.DragOngoing += this.OnPointHandleDragOngoing;
                handle.DragEnded += this.OnPointHandleDragEnded;
            });
        }

        /// <summary>
        /// Detaches the event handlers.
        /// </summary>
        private void DetachHandlers()
        {
            this.pointHandles.ForEach(handle =>
            {
                handle.DragOngoing -= this.OnPointHandleDragOngoing;
                handle.DragEnded -= this.OnPointHandleDragEnded;
            });
        }
    }
}
