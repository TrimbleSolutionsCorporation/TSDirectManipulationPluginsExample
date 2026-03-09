namespace BasicPlugin
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using Tekla.Structures.Dialog.UIControls;

    /// <summary>
    /// A class that defines the structure of the user interface dialog as a Windows Forms dialog.
    /// </summary>
    partial class BeamPluginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private OkApplyModifyGetOnOffCancel okApplyModifyGetOnOffCancel1;
        private SaveLoad saveLoad1;
        private TextBox textBoxLengthFactor;
        private Label LengthFactor;
        private TextBox textBoxProfile;
        private Label label1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okApplyModifyGetOnOffCancel1 = new OkApplyModifyGetOnOffCancel();
            this.saveLoad1 = new SaveLoad();
            this.textBoxLengthFactor = new TextBox();
            this.LengthFactor = new Label();
            this.textBoxProfile = new TextBox();
            this.label1 = new Label();
            this.SuspendLayout();

            // Setting up okApplyModifyGetOnOffCancel1.
            this.structuresExtender.SetAttributeName(this.okApplyModifyGetOnOffCancel1, null);
            this.structuresExtender.SetAttributeTypeName(this.okApplyModifyGetOnOffCancel1, null);
            this.structuresExtender.SetBindPropertyName(this.okApplyModifyGetOnOffCancel1, null);
            this.okApplyModifyGetOnOffCancel1.Dock = DockStyle.Bottom;
            this.okApplyModifyGetOnOffCancel1.Location = new Point(0, 246);
            this.okApplyModifyGetOnOffCancel1.Name = "okApplyModifyGetOnOffCancel1";
            this.okApplyModifyGetOnOffCancel1.Size = new Size(519, 29);
            this.okApplyModifyGetOnOffCancel1.TabIndex = 0;
            this.okApplyModifyGetOnOffCancel1.OkClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_OkClicked);
            this.okApplyModifyGetOnOffCancel1.ApplyClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_ApplyClicked);
            this.okApplyModifyGetOnOffCancel1.ModifyClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_ModifyClicked);
            this.okApplyModifyGetOnOffCancel1.GetClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_GetClicked);
            this.okApplyModifyGetOnOffCancel1.OnOffClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_OnOffClicked);
            this.okApplyModifyGetOnOffCancel1.CancelClicked += new EventHandler(this.OkApplyModifyGetOnOffCancel1_CancelClicked);

            // Setting up saveLoad1.
            this.structuresExtender.SetAttributeName(this.saveLoad1, null);
            this.structuresExtender.SetAttributeTypeName(this.saveLoad1, null);
            this.saveLoad1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.structuresExtender.SetBindPropertyName(this.saveLoad1, null);
            this.saveLoad1.Dock = DockStyle.Top;
            this.saveLoad1.HelpFileType = SaveLoad.HelpFileTypeEnum.General;
            this.saveLoad1.HelpKeyword = "";
            this.saveLoad1.HelpUrl = "";
            this.saveLoad1.Location = new Point(0, 0);
            this.saveLoad1.Name = "saveLoad1";
            this.saveLoad1.SaveAsText = "";
            this.saveLoad1.Size = new Size(519, 43);
            this.saveLoad1.TabIndex = 1;
            this.saveLoad1.UserDefinedHelpFilePath = null;

            // Setting up textBoxLengthFactor.
            this.structuresExtender.SetAttributeName(this.textBoxLengthFactor, "LengthFactor");
            this.structuresExtender.SetAttributeTypeName(this.textBoxLengthFactor, "Double");
            this.structuresExtender.SetBindPropertyName(this.textBoxLengthFactor, null);
            this.textBoxLengthFactor.Location = new Point(303, 138);
            this.textBoxLengthFactor.Name = "textBoxLengthFactor";
            this.textBoxLengthFactor.Size = new Size(100, 20);
            this.textBoxLengthFactor.TabIndex = 2;

            // Setting up LengthFactor.
            this.structuresExtender.SetAttributeName(this.LengthFactor, null);
            this.structuresExtender.SetAttributeTypeName(this.LengthFactor, null);
            this.LengthFactor.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.LengthFactor, null);
            this.LengthFactor.Location = new Point(120, 141);
            this.LengthFactor.Name = "LengthFactor";
            this.LengthFactor.Size = new Size(70, 13);
            this.LengthFactor.TabIndex = 3;
            this.LengthFactor.Text = "Length factor";

            // Setting up textBoxProfile.
            this.structuresExtender.SetAttributeName(this.textBoxProfile, "Profile");
            this.structuresExtender.SetAttributeTypeName(this.textBoxProfile, "String");
            this.structuresExtender.SetBindPropertyName(this.textBoxProfile, null);
            this.textBoxProfile.Location = new Point(303, 90);
            this.textBoxProfile.Name = "textBoxProfile";
            this.textBoxProfile.Size = new Size(100, 20);
            this.textBoxProfile.TabIndex = 2;

            // Setting up label1.
            this.structuresExtender.SetAttributeName(this.label1, null);
            this.structuresExtender.SetAttributeTypeName(this.label1, null);
            this.label1.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.label1, null);
            this.label1.Location = new Point(120, 93);
            this.label1.Name = "label1";
            this.label1.Size = new Size(36, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Profile";

            // Setting up BeamPluginForm.
            this.structuresExtender.SetAttributeName(this, null);
            this.structuresExtender.SetAttributeTypeName(this, null);
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.structuresExtender.SetBindPropertyName(this, null);
            this.ClientSize = new Size(519, 275);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxProfile);
            this.Controls.Add(this.LengthFactor);
            this.Controls.Add(this.textBoxLengthFactor);
            this.Controls.Add(this.saveLoad1);
            this.Controls.Add(this.okApplyModifyGetOnOffCancel1);
            this.Name = "BeamPluginForm";
            this.Text = "BeamPluginForm";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}