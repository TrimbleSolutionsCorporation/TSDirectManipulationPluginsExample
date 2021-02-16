namespace ComplexTransitionSectionPluginFeatures
{
    using ComplexTransitionSectionPlugin;
    using ManipulationContexts;
    using System.Collections.Generic;
    using System.Linq;
    using Tekla.Structures;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Controls;

    /// <summary>
    /// Direct Manipulation feature for manipulating the transition section plugin.
    /// </summary>
    /// <seealso cref="PluginManipulationFeatureBase" />
    public sealed class TransitionSectionManipulationFeature : PluginManipulationFeatureBase
    {
        /// <summary>
        /// The rectangle height.
        /// </summary>
        private double rectangleHeight = TransitionSectionPlugin.DefaultRectangleHeight;

        /// <summary>
        /// The rectangle width.
        /// </summary>
        private double rectangleWidth = TransitionSectionPlugin.DefaultRectangleWidth;

        /// <summary>
        /// The circle radius.
        /// </summary>
        private double circleRadius = TransitionSectionPlugin.DefaultCircleRadius;

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
        /// Initializes a new instance of the <see cref="TransitionSectionManipulationFeature"/> class.
        /// </summary>
        public TransitionSectionManipulationFeature()
            : base(TransitionSectionPlugin.PluginName, useFeatureContextualToolBar: true)
        {
        }

        /// <inheritdoc />
        protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        {
            this.radiusValueBox = toolbar.CreateValueTextBox();
            this.radiusValueBox.Tooltip = "Top radius";
            this.radiusValueBox.Title = "r=";
            this.radiusValueBox.StateChanged += (control, eventArgs) =>
            {
                foreach (var component in this.Components)
                {
                    this.ModifyComponent(component, TransitionSectionPluginPropertyNames.CircleRadius, this.radiusValueBox?.Value ?? 0.0);
                }
            };

            this.widthValueBox = toolbar.CreateValueTextBox();
            this.widthValueBox.Tooltip = "Bottom width";
            this.widthValueBox.Title = "w=";
            this.widthValueBox.StateChanged += (control, eventArgs) =>
            {
                foreach (var component in this.Components)
                {
                    this.ModifyComponent(component, TransitionSectionPluginPropertyNames.RectangleWidth, this.widthValueBox?.Value ?? 0.0);
                }
            };

            this.heightValueBox = toolbar.CreateValueTextBox();
            this.heightValueBox.Tooltip = "Bottom height";
            this.heightValueBox.Title = "h=";
            this.heightValueBox.StateChanged += (control, eventArgs) =>
            {
                foreach (var component in this.Components)
                {
                    this.ModifyComponent(component, TransitionSectionPluginPropertyNames.RectangleHeight, this.heightValueBox?.Value ?? 0.0);
                }
            };
        }

        /// <inheritdoc />
        protected override void Refresh()
        {
            this.GetCurrentValues(this.Components.First());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(Component component)
        {
            yield return new RectangleManipulationContext(component, this);
            yield return new CircleManipulationContext(component, this);
        }

        /// <summary>
        /// Gets the current values of the plugin from the associated <see cref="TransitionSectionPluginData"/> data structure,
        /// and assigns the the values to the correct value text boxes.
        /// </summary>
        private void GetCurrentValues(Component component)
        {
            if (!(component.Select() && component.GetAttribute(TransitionSectionPluginPropertyNames.RectangleHeight, ref this.rectangleHeight)))
            {
                this.rectangleHeight = TransitionSectionPlugin.DefaultRectangleHeight;
            }

            if (!(component.Select() && component.GetAttribute(TransitionSectionPluginPropertyNames.RectangleWidth, ref this.rectangleWidth)))
            {
                this.rectangleWidth = TransitionSectionPlugin.DefaultRectangleWidth;
            }

            if (!(component.Select() && component.GetAttribute(TransitionSectionPluginPropertyNames.CircleRadius, ref this.circleRadius)))
            {
                this.circleRadius = TransitionSectionPlugin.DefaultCircleRadius;
            }

            this.radiusValueBox.Value = this.circleRadius;
            this.heightValueBox.Value = this.rectangleHeight;
            this.widthValueBox.Value = this.rectangleWidth;
        }

        /// <summary>
        /// Modifies the component.
        /// </summary>
        /// <param name="component">The component.</param>
        private void ModifyComponent(Component component, string key, double value)
        {
            if (component == null || component.Identifier.Equals(new Identifier()) || !component.Select())
            {
                return;
            }

            component.SetAttribute(key, value);
            component.Modify();
            new Model().CommitChanges();
        }
    }
}
