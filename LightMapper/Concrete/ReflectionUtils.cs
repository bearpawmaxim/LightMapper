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
                var tpi = targetPi.FirstOrDefault(t => t.Name.Equals(spi.Name) && (t.PropertyType.Equals(spi.PropertyType) || t.IsEnumConversion(spi)));

                _mappingProps.Add(new MappingProperty(spi, tpi, tpi != null));
            }

            foreach (PropertyInfo tpi in targetPi.Except(sourcePi, a => a.Name))
                _mappingProps.Add(new MappingProperty(null, tpi, false));
        }

        internal static void ProcessFieldInfo(bool mapFields, List<MappingProperty> _mappingProps, FieldInfo[] sourceFi, FieldInfo[] targetFi)
        {
            foreach (FieldInfo sfi in sourceFi)
            {
                var tfi = targetFi.FirstOrDefault(t => t.Name.Equals(sfi.Name) && (t.FieldType.Equals(sfi.FieldType) || t.IsEnumConversion(sfi)));

                _mappingProps.Add(new MappingProperty(sfi, tfi, mapFields && tfi != null));
            }

            foreach (FieldInfo tfi in targetFi.Except(sourceFi, a => a.Name))
                _mappingProps.Add(new MappingProperty(null, tfi, false));
        }

        internal static bool IsEnumConversion(this MemberInfo target, MemberInfo source)
        {
            Type targetType = target.MemberType == MemberTypes.Property ? (target as PropertyInfo).PropertyType : (target as FieldInfo).FieldType,
                sourceType = source.MemberType == MemberTypes.Property ? (source as PropertyInfo).PropertyType : (source as FieldInfo).FieldType;

            if ((targetType.IsEnum && Enum.GetUnderlyingType(targetType).Equals(sourceType))
                || (sourceType.IsEnum && Enum.GetUnderlyingType(sourceType).Equals(targetType)))
                return true;

            return false;
        }
    }
}
