using LightMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightMapper.Concrete
{
    internal static class ReflectionUtils
    {
        internal static void AddBaseExplcit<SourceT, TargetT>(IList<IMappingItem> mappingStore, MappingData<SourceT, TargetT> mi, Dictionary<Action<SourceT, TargetT>, ExplicitOrders> explicitActions)
        {
            var bmi = mappingStore.FirstOrDefault(f => f.SourceType.Hash == (mi as IMappingItem).SourceType.BaseHash) as IMappingItem;
            if (bmi != null)
            {
                Type t = typeof(MappingData<,>).MakeGenericType(bmi.SourceType.Type, bmi.TargetType.Type);
                var ti = Activator.CreateInstance(t);
                ti = bmi;
                var getter = ti.GetType()
                    .GetMethod("CastExplicit", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(SourceT), typeof(TargetT));

                foreach (var act in getter.Invoke(ti, null) as IDictionary<Action<SourceT, TargetT>, ExplicitOrders>)
                    explicitActions.Add(act.Key, act.Value);
            }
        }
    }
}
