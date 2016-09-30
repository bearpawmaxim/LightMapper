using LightMapper.Concrete;
using LightMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightMapper
{
    /// <summary>IMapper implementation</summary>
    public class LightMapper : IMapper
    {
        #region Variables
        /// <summary>Static mapper instance</summary>
        public static IMapper Instance => _instance = _instance ?? new LightMapper();
        private static IMapper _instance;

        private readonly object _locker = new object();

        private IList<IMappingItem> _mappingStore = new List<IMappingItem>();
        #endregion
        #region Mapping control
        /// <see cref="IMapper.CreateMapping{SourceT, TargetT}(bool)"/> 
        public IMappingItem<SourceT, TargetT> CreateMapping<SourceT, TargetT>(bool mapFields)
            where SourceT : class
            where TargetT : class
            => MappingItem<SourceT, TargetT>.CreateMapping(mapFields);

        /// <see cref="IMapper.AddMapping{SourceT, TargetT}(IMappingItem{SourceT, TargetT})"/> 
        public void AddMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class
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

        /// <see cref="IMapper.UpdateMapping{SourceT, TargetT}(IMappingItem{SourceT, TargetT})"/>
        public void UpdateMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class
            => AddMapping(mapping);

        /// <see cref="IMapper.GetMapping{SourceT, TargetT}"/>
        public IMappingItem<SourceT, TargetT> GetMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class
        {
            return FindMapping<SourceT, TargetT>(true);
        }

        /// <see cref="IMapper.RemoveMapping{SourceT, TargetT}"/>
        public void RemoveMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class
        {
            var mi = FindMapping<SourceT, TargetT>();
            _mappingStore.Remove(mi);
        }

        private IMappingItem<SourceT, TargetT> FindMapping<SourceT, TargetT>(bool notThrow = false)
            where SourceT : class
            where TargetT : class
        {
            IMappingItem mi = _mappingStore.FirstOrDefault(m => m.SourceType.Hash == typeof(SourceT).GetHashCode() && m.TargetType.Hash == typeof(TargetT).GetHashCode());
            if (mi == null && !notThrow) throw new MappingNotFoundException("Mapping of class '{0}' into '{1}' not found!", typeof(SourceT).FullName, typeof(TargetT).FullName);

            return (mi as IMappingItem<SourceT, TargetT>);
        }
        #endregion
        #region Mapping
        /// <see cref="IMapper.Map{SourceT, TargetT}(SourceT)"/>
        public TargetT Map<SourceT, TargetT>(SourceT input)
            where SourceT : class
            where TargetT : class
        {
            var mi = FindMapping<SourceT, TargetT>() as MappingData<SourceT, TargetT>;

            try
            {
                var mapper = mi.CreateMapper();

                var bActions = mi.ExplicitActions.Where(w => w.Value == ExplicitOrders.BeforeMap).Select(s => s.Key).ToList();
                var aActions = mi.ExplicitActions.Where(w => w.Value == ExplicitOrders.AfterMap).Select(s => s.Key).ToList();

                return mapper.MapSingle(input, bActions, aActions, mi.ClassCtor);
            }
            catch (Exception e)
            {
                throw new MappingFailedException($"Mapping of {typeof(SourceT).Name} to {typeof(TargetT).Name} failed!", e);
            }
        }
        /// <see cref="IMapper.Map{SourceT, TargetT}(IEnumerable{SourceT})"/>
        public IEnumerable<TargetT> Map<SourceT, TargetT>(IEnumerable<SourceT> input)
            where SourceT : class
            where TargetT : class
        {
            var mi = FindMapping<SourceT, TargetT>() as MappingData<SourceT, TargetT>;

            try
            {
                var mapper = mi.CreateMapper();

                var bActions = mi.ExplicitActions.Where(w => w.Value == ExplicitOrders.BeforeMap).Select(s => s.Key).ToList();
                var aActions = mi.ExplicitActions.Where(w => w.Value == ExplicitOrders.AfterMap).Select(s => s.Key).ToList();

                IEnumerable<TargetT> ret = new List<TargetT>(input.Count());

                ret = mapper.MapCollection(input, bActions, aActions, mi.ClassCtor);
                
                return ret;
            }
            catch (Exception e)
            {
                throw new MappingFailedException($"Mapping of {typeof(SourceT).Name} to {typeof(TargetT).Name} failed!", e);
            }
        }
        #endregion
    }
}
