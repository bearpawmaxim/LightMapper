using LightMapper.Concrete;
using System;
using System.Collections.Generic;

namespace LightMapper.Infrastructure
{
    public class MappingData<SourceT, TargetT>
    {
        internal IList<MappingProperty> MappingProperties { get; set; }
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

        internal Dictionary<Action<PSourceT, PTargetT>, ExplicitOrders> CastExplicit<PSourceT, PTargetT>()
        {
            Dictionary<Action<PSourceT, PTargetT>, ExplicitOrders> ret = new Dictionary<Action<PSourceT, PTargetT>, ExplicitOrders>();

            foreach (var act in ExplicitActions)
                ret.Add(act.Key as Action<PSourceT, PTargetT>, act.Value);

            return ret;
        }
    }
}
