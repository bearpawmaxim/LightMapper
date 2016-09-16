using LightMapper.Concrete;
using System;
using System.Collections.Generic;

namespace LightMapper.Infrastructure
{
    public abstract class MappingData<SourceT, TargetT>
    {
        internal IList<MappingProperty> MappingProperties { get; set; }
        internal Dictionary<Action<SourceT, TargetT>, ExplicitOrders> ExplicitActions { get; set; }
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
