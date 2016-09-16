using LightMapper.Concrete;
using LightMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightMapper
{
    public class LightMapper : IMapper
    {
        #region Variables
        public static IMapper Instance => _instance = _instance ?? new LightMapper();

        private static IMapper _instance;
        private readonly object _locker = new object();

        private IList<IMappingItem> _mappingStore = new List<IMappingItem>();
        #endregion
        #region Mapping control
        public IMappingItem<SourceT, TargetT> CreateMapping<SourceT, TargetT>(bool mapFields)
            where SourceT : class
            where TargetT : new()
            => MappingItem<SourceT, TargetT>.CreateMapping(mapFields);

        public void AddMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : new()
        {
            try
            {
                int idx = -1;

                var existingMapping = FindMapping<SourceT, TargetT>(true) as MappingData<SourceT, TargetT>;
                if (existingMapping != null)
                {
                    idx = _mappingStore.IndexOf((IMappingItem)existingMapping);
                    _mappingStore[idx] = null;
                }

                existingMapping = mapping as MappingData<SourceT, TargetT>;
                existingMapping.CompileMapping();

                if (idx != -1) _mappingStore[idx] = existingMapping as IMappingItem<SourceT, TargetT>;
                else _mappingStore.Add(existingMapping as IMappingItem<SourceT, TargetT>);
            }
            catch (Exception e)
            {
                throw new CompilationFailedException($"{typeof(SourceT)} to {typeof(SourceT)} mapping compilation failed!", e);
            }
        }

        public void UpdateMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : new()
            => AddMapping(mapping);


        public IMappingItem<SourceT, TargetT> GetMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : new()
        {
            return FindMapping<SourceT, TargetT>(true);
        }

        public void RemoveMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : new()
        {
            var mi = FindMapping<SourceT, TargetT>();
            _mappingStore.Remove(mi);
        }

        private IMappingItem<SourceT, TargetT> FindMapping<SourceT, TargetT>(bool notThrow = false)
            where SourceT : class
            where TargetT : new()
        {
            IMappingItem mi = _mappingStore.FirstOrDefault(m => m.SourceType.Hash == typeof(SourceT).GetHashCode() && m.TargetType.Hash == typeof(TargetT).GetHashCode());
            if (mi == null && !notThrow) throw new MappingNotFoundException("Mapping of class '{0}' into '{1}' not found!", typeof(SourceT).FullName, typeof(TargetT).FullName);

            return (mi as IMappingItem<SourceT, TargetT>);
        }
        #endregion
        #region Mapping
        public TargetT Map<SourceT, TargetT>(SourceT input)
            where SourceT : class
            where TargetT : new()
        {
            var mi = FindMapping<SourceT, TargetT>();

            return mi.Map(input);
        }

        public IEnumerable<TargetT> Map<SourceT, TargetT>(IEnumerable<SourceT> input)
            where SourceT : class
            where TargetT : new()
        {
            var mi = FindMapping<SourceT, TargetT>();

            return mi.Map(input);
        }
        #endregion
    }
}
