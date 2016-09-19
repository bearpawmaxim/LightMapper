using LightMapper.Infrastructure;
using System.Collections.Generic;

namespace LightMapper
{
    public interface IMapper
    {
        #region Mappings control
        IMappingItem<SourceT, TargetT> CreateMapping<SourceT, TargetT>(bool mapFields)
            where SourceT : class
            where TargetT : class;

        void AddMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class;

        void RemoveMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class;

        IMappingItem<SourceT, TargetT> GetMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class;

        void UpdateMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class;
        #endregion
        #region Mapping
        IEnumerable<TargetT> Map<SourceT, TargetT>(IEnumerable<SourceT> input)
            where SourceT : class
            where TargetT : class;

        TargetT Map<SourceT, TargetT>(SourceT input)
            where SourceT : class
            where TargetT : class;
        #endregion
    }
}
