namespace BasicPluginFeatures
{
    using System.Collections.Generic;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;

    /// <summary>
    /// Direct Manipulation manipulation feature for the BeamPlugin class.
    /// </summary>
    /// <seealso cref="Tekla.Structures.Plugins.DirectManipulation.Features.PluginManipulationFeatureBase" />
    public class BeamPluginManipulationFeature : PluginManipulationFeatureBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeamPluginManipulationFeature"/> class.
        /// </summary>
        public BeamPluginManipulationFeature()
            : base(BasicPlugin.BeamPlugin.PluginName)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(Component component)
        {
            yield return new BeamPluginManipulationContext(component, this);
        }
    }
}
