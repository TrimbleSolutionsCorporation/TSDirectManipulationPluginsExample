namespace BasicPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;
    using Tekla.Structures.Plugins;

    using TSM = Tekla.Structures.Model;

    /// <summary>
    /// This class is used for storing data related to the current plugin.
    /// </summary>
    public class StructuresData
    {
        /// <summary>
        /// The length factor.
        /// </summary>
        [StructuresField("LengthFactor")]
        public double LengthFactor;

        /// <summary>
        /// The profile.
        /// </summary>
        [StructuresField("Profile")]
        public string Profile;
    }

    /// <summary>
    /// This plugin is similar to the example found in the Open API Reference PluginBase section.
    /// The plugin asks the user to pick two points. The plug-in then calculates new insertion points
    /// using a double parameter from the dialog and creates a beam.
    /// </summary>
    [Plugin(PluginName)]
    [PluginUserInterface("BasicPlugin.BeamPluginForm")]
    public class BeamPlugin : PluginBase
    {
        /// <summary>
        /// Gets the structures data.
        /// </summary>
        private StructuresData Data { get; }

        /// <summary>
        /// The current length factor.
        /// </summary>
        private double lengthFactor;

        /// <summary>
        /// The current profile.
        /// </summary>
        private string profile;

        /// <summary>
        /// The default profile name.
        /// </summary>
        public static readonly string DefaultProfileName = "HEA300";

        /// <summary>
        /// The name of the plugin. This value is used in the attribute above as well as with the Direct Manipulation creation and manipulation features.
        /// </summary>
        public const string PluginName = "Beam Plugin Example";

        /// <summary>
        /// Initializes a new instance of the <see cref="BeamPlugin"/> class.
        /// </summary>
        /// <param name="data">The <see cref="StructuresData"/> object for the plugin.</param>
        public BeamPlugin(StructuresData data)
        {
            this.Data = data;
        }

        /// <inheritdoc />
        public override bool Run(List<InputDefinition> input)
        {
            try
            {
                this.GetValuesFromDialog();

                var points = (ArrayList)input[0].GetInput();
                var point1 = points[0] as Point;
                var point2 = points[1] as Point;
                var lengthVector = new Vector(point2 - point1);

                if (this.lengthFactor > 0)
                {
                    point2 = this.lengthFactor * lengthVector + point1;
                }

                this.CreateBeam(point1, point2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Exception: " + ex);
            }

            return true;
        }

        /// <inheritdoc />
        public override List<InputDefinition> DefineInput()
        {
            var beamPicker = new Picker();
            var pointList = new List<InputDefinition>();

            var point1 = beamPicker.PickPoint();
            var point2 = beamPicker.PickPoint();

            var input1 = new InputDefinition(point1);
            var input2 = new InputDefinition(point2);

            pointList.Add(input1);
            pointList.Add(input2);

            return pointList;
        }

        /// <summary>
        /// Gets the current values from dialog.
        /// </summary>
        private void GetValuesFromDialog()
        {
            this.lengthFactor = this.Data.LengthFactor;
            this.profile = this.Data.Profile;
            if (this.IsDefaultValue(this.lengthFactor))
            {
                this.lengthFactor = 2.0;
            }

            if (this.IsDefaultValue(this.profile))
            {
                this.profile = DefaultProfileName;
            }
        }

        /// <summary>
        /// Creates a beam using two input points.
        /// </summary>
        /// <param name="point1">The first input point.</param>
        /// <param name="point2">The second input point.</param>
        private void CreateBeam(Point point1, Point point2)
        {
            var myBeam = new TSM.Beam(point1, point2)
            {
                Profile = { ProfileString = this.profile },
                Finish = "PAINT"
            };

            myBeam.Insert();
        }
    }
}
