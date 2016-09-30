using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LightMapper.Infrastructure
{
    /// <summary>Base for <see cref="IMappingItem{SourceT, TargetT}"/>. Provides an ability to store <see cref="IMappingItem{SourceT, TargetT}"/> in List</summary>
    public interface IMappingItem
    {
        /// <summary>Source type info</summary>
        MappingTypeInfo SourceType { get; }
        /// <summary>Target type info</summary>
        MappingTypeInfo TargetType { get; }
    }

    /// <summary>IMappingItem interface</summary>
    /// <typeparam name="SourceT">Mapping source type</typeparam>
    /// <typeparam name="TargetT">Mapping target type</typeparam>
    public interface IMappingItem<SourceT, TargetT> : IMappingItem
        where SourceT : class
        where TargetT : class
    {
        /// <summary>Defines custom constructor Func for TargetT type</summary>
        /// <param name="constructor">Constructor function</param>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> SetConstructorFunc(Func<TargetT> ctor);
        #region Explicit/Exclude/Include
        /// <summary>Defines an explicit action which will be executed during the mapping process</summary>
        /// <param name="action">Action{SourceT, TargetT} to execute</param>
        /// <param name="executionOrder">Execution order (before or after mapping)</param>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> Explicit(Action<SourceT, TargetT> action, ExplicitOrders executionOrder);
        /// <summary>Defines an explicit mapping for field/property</summary>
        /// <param name="target">TargetT field/property t =&gt; t.TargetPropName</param>
        /// <param name="source">SourceT field/property s =&gt; s.SourcePropName</param>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> ExplicitMember(Expression<Func<TargetT, object>> target, Expression<Func<SourceT, object>> source);
        /// <summary>
        /// Excludes field/property from mapping. Influences every mapping scenario (standart, explicit field, explicit action)
        /// </summary>
        /// <param name="expression">TargetT field/property. Ex. t =&gt; t.TargetPropName</param>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> Exclude(Expression<Func<TargetT, object>> expression);
        /// <summary>Includes field/property into mapping</summary>
        /// <param name="expression">TargetT field/property. Ex. t =&gt; t.TargetPropName</param>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> Include(Expression<Func<TargetT, object>> expression);
        /// <summary>
        /// Adds base class explicit actions (if exists)
        /// </summary>
        /// <typeparam name="BSourceT">Base to source {SourceT} type ({SourceT} inherits {BSourceT})</typeparam>
        /// <typeparam name="BTargetT">Base to target {TargetT} type ({TargetT} inherits {BTargetT})</typeparam>
        /// <returns>IMappingItem{SourceT, TargetT}</returns>
        IMappingItem<SourceT, TargetT> AddBaseExplicit<BSourceT, BTargetT>(IMappingItem<BSourceT, BTargetT> baseMapping)
            where BSourceT : class
            where BTargetT : class;
        #endregion
    }
}
