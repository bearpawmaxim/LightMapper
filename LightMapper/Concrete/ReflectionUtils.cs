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

        internal static void ProcessPropertyInfo(List<MappingProperty> _mappingProps, PropertyInfo[] sourcePi, PropertyInfo[] targetPi)
        {
            foreach (PropertyInfo spi in sourcePi)
            {
                var tpi = targetPi.FirstOrDefault(t => t.Name.Equals(spi.Name) && t.PropertyType.Equals(spi.PropertyType));
                var ntype = Nullable.GetUnderlyingType(spi.PropertyType);

                _mappingProps.Add(new MappingProperty(spi, tpi, ntype?.FullName ?? spi.PropertyType.FullName, ntype != null, tpi != null));
            }

            foreach (PropertyInfo tpi in targetPi.Except(sourcePi, a => a.Name))
            {
                var ntype = Nullable.GetUnderlyingType(tpi.PropertyType);
                _mappingProps.Add(new MappingProperty(null, tpi, ntype?.FullName ?? tpi.PropertyType.FullName, ntype != null, false));
            }
        }

        internal static void ProcessFieldInfo(bool mapFields, List<MappingProperty> _mappingProps, FieldInfo[] sourceFi, FieldInfo[] targetFi)
        {
            foreach (FieldInfo sfi in sourceFi)
            {
                var tfi = targetFi.FirstOrDefault(t => t.Name.Equals(sfi.Name) && t.FieldType.Equals(sfi.FieldType));
                var ntype = Nullable.GetUnderlyingType(sfi.FieldType);

                _mappingProps.Add(new MappingProperty(sfi, tfi, ntype?.FullName ?? sfi.FieldType.FullName, ntype != null, mapFields && tfi != null));
            }

            foreach (FieldInfo tfi in targetFi.Except(sourceFi, a => a.Name))
            {
                var ntype = Nullable.GetUnderlyingType(tfi.FieldType);
                _mappingProps.Add(new MappingProperty(null, tfi, ntype?.FullName ?? tfi.FieldType.FullName, ntype != null, false));
            }
        }
    }
}
