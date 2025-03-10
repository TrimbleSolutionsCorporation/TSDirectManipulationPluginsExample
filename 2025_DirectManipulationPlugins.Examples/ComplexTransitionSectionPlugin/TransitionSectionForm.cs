namespace ComplexTransitionSectionPlugin
{
    using Tekla.Structures.Dialog;

    /// <summary>
    /// The <see cref="TransitionSectionForm"/> class represents the default user interface to the plugin.
    /// </summary>
    /// <remarks>
    /// This form is intentionally empty, as all interaction with the plugin is supposed to happen through the contextual toolbar.
    /// </remarks>
    /// <seealso cref="Tekla.Structures.Dialog.PluginFormBase" />
    public partial class TransitionSectionForm : PluginFormBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionSectionForm"/> class.
        /// </summary>
        public TransitionSectionForm()
        {
            this.InitializeComponent();
        }
    }
}
