namespace BasicPlugin
{
    using System;
    using Tekla.Structures.Dialog;

    /// <summary>
    /// This class uses the <seealso cref="PluginFormBase"/> class to define the user interface as a Windows Forms dialog.
    /// </summary>
    public partial class BeamPluginForm : PluginFormBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeamPluginForm"/> class.
        /// </summary>
        public BeamPluginForm()
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        protected override string LoadValuesPath(string fileName)
        {
            this.SetAttributeValue(this.textBoxLengthFactor, 2d);  // One line for each plugin attribute
            this.SetAttributeValue(this.textBoxProfile, BeamPlugin.DefaultProfileName);
            var result = base.LoadValuesPath(fileName);
            this.Apply();
            return result;
        }

        /// <summary>
        /// Handles the ApplyClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_ApplyClicked(object sender, EventArgs e)
        {
            this.Apply();
        }

        /// <summary>
        /// Handles the CancelClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the GetClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_GetClicked(object sender, EventArgs e)
        {
            this.Get();
        }

        /// <summary>
        /// Handles the ModifyClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_ModifyClicked(object sender, EventArgs e)
        {
            this.Modify();
        }

        /// <summary>
        /// Handles the OkClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_OkClicked(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        /// <summary>
        /// Handles the OnOffClicked event of the okApplyModifyGetOnOffCancel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OkApplyModifyGetOnOffCancel1_OnOffClicked(object sender, EventArgs e)
        {
            this.ToggleSelection();
        }
    }
}
