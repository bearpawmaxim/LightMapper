using LightMapper.Concrete;
using System;
using System.Collections.Generic;

namespace LightMapper.Infrastructure
{
    /// <summary>Internal mapping specific data store</summary>
    /// <typeparam name="SourceT">Mapping source type</typeparam>
    /// <typeparam name="TargetT">Mapping target type</typeparam>
    public class MappingData<SourceT, TargetT>
    {
        internal IList<MappingProperty> MappingProperties { get; set; }
        /// <summary>Internal storage of explicit actions</summary>
        public Dictionary<Action<SourceT, TargetT>, ExplicitOrders> ExplicitActions { get; set; }
        internal MapperActivator<SourceT, TargetT> Activator { get; set; }

        internal void CompileMapping()
        {
            MapperActivator<SourceT, TargetT> outActivator;
            var mc = new MappingCompiler<SourceT, TargetT>();
            mc.Compile(this, out outActivator);

            Activator = outActivator;
        }
    }
}
