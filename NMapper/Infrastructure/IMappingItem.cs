using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NMapper.Infrastructure
{
    public interface IMappingItem
    {
        MappingTypeInfo SourceType { get; }
        MappingTypeInfo TargetType { get; }
    }

    public interface IMappingItem<SourceT, TargetT> : IMappingItem
        where SourceT : class
        where TargetT : new()
    {
        #region Explicit/Exclude/Include
        IMappingItem<SourceT, TargetT> Explicit(Action<SourceT, TargetT> action, ExplicitOrders executionOrder);
        IMappingItem<SourceT, TargetT> ExplicitField(Expression<Func<TargetT, object>> target, Expression<Func<SourceT, object>> source);
        IMappingItem<SourceT, TargetT> Exclude(Expression<Func<TargetT, object>> expression);
        IMappingItem<SourceT, TargetT> Include(Expression<Func<TargetT, object>> expression);
        #endregion
        #region Mapping
        IEnumerable<TargetT> Map(IEnumerable<SourceT> input);
        TargetT Map(SourceT input);
        #endregion
    }
}
