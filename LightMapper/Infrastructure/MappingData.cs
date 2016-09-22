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
        internal MapSingleDlg<SourceT, TargetT> MapSingleMethod { get; set; }
        internal MapEnumerableDlg<SourceT, TargetT> MapCollectionMethod { get; set; }

        internal void CompileMapping()
        {
            MapSingleMethod = null;
            MapCollectionMethod = null;

            var mappingData = this;

            var mc = new MappingCompiler<SourceT, TargetT>();
            mc.Compile(ref mappingData);
        }
    }
}
