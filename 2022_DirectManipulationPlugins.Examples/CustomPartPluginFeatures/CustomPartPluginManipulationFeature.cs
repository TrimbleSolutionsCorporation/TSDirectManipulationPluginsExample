namespace Tekla.Structures.Plugins.DirectManipulation.Examples.Feature
{
    using System.Collections.Generic;

    using Core;
    using Core.Features;

    using CustomPartPlugin;

    using Model;

    /// <summary>
    /// An example class of a Direct Manipulation API manipulation feature for a custom part plugin.
    /// </summary>
    /// <seealso cref="CustomPartPluginManipulationFeatureBase" />
    public class CustomPartPluginManipulationFeature : CustomPartPluginManipulationFeatureBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPartPluginManipulationFeature"/> class.
        /// </summary>
        public CustomPartPluginManipulationFeature()
            : base(CustomPartPlugin.PluginName, useFeatureContextualToolBar: true)
        {
        }

        /// <inheritdoc />
        // Not used in this example. It shows some example controls
        protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        {
            if (toolbar == null)
            {
                return;
            }

            var label = toolbar.CreateLabel("Test label");
            label.Tooltip = "Wait, what?!";

            var button = toolbar.CreateButton("Hello World!");
            button.Tooltip = "More helpful tooltips for all! :D";

            var checkboxControl = toolbar.CreateCheckBox("Testing, testing!");
            checkboxControl.Tooltip = "Attention!!";

            object myObject = "Plaa";
            var dropDownControl = toolbar.CreateDropDown(new List<object> { "Hello", 2, 3.14159, myObject }, myObject);
            dropDownControl.Tooltip = "Objects!";

            var groupName = "teh group";
            var radioButton1 = toolbar.CreateRadioButton("1", group: groupName);
            radioButton1.Tooltip = "Radio Rock";

            var radioButton2 = toolbar.CreateRadioButton("2", group: groupName);
            radioButton2.Tooltip = "NRJ!";

            var radioButton3 = toolbar.CreateRadioButton("3", group: groupName);
            radioButton3.Tooltip = "Nova";

            var textControl = toolbar.CreateTextBox("Goodbye, World!");
            textControl.Tooltip = "Oh my!";

            var valueboxControl = toolbar.CreateValueTextBox(-3.5655);
            valueboxControl.Title = "ang =";
            valueboxControl.Tooltip = "Random";

            checkboxControl.StateChanged += (control, eventArgs) => { dropDownControl.Visible = !dropDownControl.Visible; };
        }

        /// <inheritdoc />
        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(CustomPart component)
        {
            yield return new CustomPartPluginManipulationContext(component, this);
        }
    }
}
