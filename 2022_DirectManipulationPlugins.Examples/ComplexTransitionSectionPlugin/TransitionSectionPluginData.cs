namespace ComplexTransitionSectionPlugin
{
    using Tekla.Structures.Plugins;

    /// <summary>
    /// The <see cref="TransitionSectionPluginData"/> class provides a data structure
    /// to be used to communicate between the the plugin, the creation and manipulation
    /// features and the default user interface.
    /// </summary>
    public sealed class TransitionSectionPluginData
    {
        /// <summary>
        /// Profile to use in the transition section example.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.SectionProfile)]
        public string SectionProfile;

        /// <summary>
        /// Material for the transition section.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.Material)]
        public string Material;

        /// <summary>
        /// Finish for the transition section.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.Finish)]
        public string Finish;

        /// <summary>
        /// The rectangle width for the transition section.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.RectangleWidth)]
        public double RectangleWidth;

        /// <summary>
        /// The rectangle height for the transition section.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.RectangleHeight)]
        public double RectangleHeight;

        /// <summary>
        /// The circle radius for the transition section.
        /// </summary>
        [StructuresField(TransitionSectionPluginPropertyNames.CircleRadius)]
        public double CircleRadius;
    }
}
