using LightMapper.Infrastructure;
using System.Collections.Generic;

namespace LightMapper
{
    /// <summary>IMapper interface</summary>
    public interface IMapper
    {
        #region Mappings control
        /// <summary>Returns a new instance of IMappingItem&lt;SourceT, TargetT&gt;</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <param name="mapFields">Include class fields in mapping or nor</param>
        /// <param name="caseSensitive">Use case sensitiv member search or not</param>
        /// <returns>New unregistered instance of IMappingItem&lt;SourceT, TargetT&gt;</returns>
        IMappingItem<SourceT, TargetT> CreateMapping<SourceT, TargetT>(bool mapFields, bool caseSensitive = true)
            where SourceT : class
            where TargetT : class;
        /// <summary>Registers a mapping in mapper</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <param name="mapping">Instance of IMappingItem&lt;SourceT, TargetT&gt;</param>
        void AddMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class;
        /// <summary>Removes a mapping from mapper</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        void RemoveMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class;
        /// <summary>Returns registered mapping by it's source and target type</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <returns>Instance of IMappingItem&lt;SourceT, TargetT&gt;</returns>
        IMappingItem<SourceT, TargetT> GetMapping<SourceT, TargetT>()
            where SourceT : class
            where TargetT : class;
        /// <summary>Registers a mapping again</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <param name="mapping">Instance of IMappingItem&lt;SourceT, TargetT&gt;</param>
        void UpdateMapping<SourceT, TargetT>(IMappingItem<SourceT, TargetT> mapping)
            where SourceT : class
            where TargetT : class;
        #endregion
        #region Mapping
        /// <summary>Performs mapping from SourctT into TargetT</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <param name="input">Instance of SourceT object</param>
        /// <returns>Mapped TargetT object</returns>
        IEnumerable<TargetT> Map<SourceT, TargetT>(IEnumerable<SourceT> input)
            where SourceT : class
            where TargetT : class;
        /// <summary>Performs mapping of SourceT objects collection into TargetT objects collection</summary>
        /// <typeparam name="SourceT">Mapping source type</typeparam>
        /// <typeparam name="TargetT">Mapping target type</typeparam>
        /// <param name="input">IEnumerable of SourceT objects</param>
        /// <returns>Mapped IEnumerable of TargetT objects</returns>
        TargetT Map<SourceT, TargetT>(SourceT input)
            where SourceT : class
            where TargetT : class;
        #endregion
    }
}
