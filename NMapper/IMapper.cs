using NMapper.Infrastructure;
using System.Collections.Generic;

namespace NMapper
{
    public interface IMapper
    {
        #region Mappings control
        IMappingItem<SourceT, TargetT> CreateMapping<SourceT, TargetT>(bool mapFields)
            where SourceT : class
            where TargetT : new();

        void AddMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : new();

        void RemoveMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : new();

        IMappingItem<SourceT, TargetT> GetMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : new();

        void UpdateMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : new();
        #endregion
        #region Mapping
        IEnumerable<TargetT> Map<SourceT, TargetT>(IEnumerable<SourceT> input)
            where SourceT : class
            where TargetT : new();

        TargetT Map<SourceT, TargetT>(SourceT input)
            where SourceT : class
            where TargetT : new();
        #endregion
    }
}
