using LightMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LightMapper.Concrete
{
    public sealed class MappingItem<SourceT, TargetT> : MappingData<SourceT, TargetT>, IMappingItem<SourceT, TargetT>
        where SourceT : class
        where TargetT : class
    {
        #region Variables
        public MappingTypeInfo SourceType { get; private set; }
        public MappingTypeInfo TargetType { get; private set; }
        #endregion

        internal static IMappingItem<SourceT, TargetT> CreateMapping(bool mapFields)
        {
            Type sourceType = typeof(SourceT),
                targetType = typeof(TargetT);

            int sHash = sourceType.GetHashCode(),
                tHash = targetType.GetHashCode();

            int? sBaseHash = sourceType.BaseType == typeof(object) ? new int?() : sourceType.BaseType.GetHashCode(),
                tBaseHash = targetType.BaseType == typeof(object) ? new int?() : targetType.BaseType.GetHashCode();

            var _mappingProps = new List<MappingProperty>();

            PropertyInfo[] sourcePi = sourceType.GetProperties();
            PropertyInfo[] targetPi = targetType.GetProperties();

            foreach (PropertyInfo spi in sourcePi)
            {
                var tpi = targetPi.FirstOrDefault(t => t.Name.Equals(spi.Name) && t.PropertyType.Equals(spi.PropertyType));
                var ntype = Nullable.GetUnderlyingType(spi.PropertyType);

                _mappingProps.Add(new MappingProperty(spi, tpi, true, ntype?.FullName ?? spi.PropertyType.FullName, ntype != null, tpi != null));
            }

            foreach (PropertyInfo tpi in targetPi.Except(sourcePi, a => a.Name))
            {
                var ntype = Nullable.GetUnderlyingType(tpi.PropertyType);
                _mappingProps.Add(new MappingProperty(null, tpi, true, ntype?.FullName ?? tpi.PropertyType.FullName, ntype != null, false));
            }

            FieldInfo[] sourceFi = sourceType.GetFields();
            FieldInfo[] targetFi = targetType.GetFields();

            foreach (FieldInfo sfi in sourceFi)
            {
                var tfi = targetFi.FirstOrDefault(t => t.Name.Equals(sfi.Name) && t.FieldType.Equals(sfi.FieldType));
                var ntype = Nullable.GetUnderlyingType(sfi.FieldType);

                _mappingProps.Add(new MappingProperty(sfi, tfi, false, ntype?.FullName ?? sfi.FieldType.FullName, ntype != null, mapFields && tfi != null));
            }

            foreach (FieldInfo tfi in targetFi.Except(sourceFi, a => a.Name))
            {
                var ntype = Nullable.GetUnderlyingType(tfi.FieldType);
                _mappingProps.Add(new MappingProperty(null, tfi, false, ntype?.FullName ?? tfi.FieldType.FullName, ntype != null, false));
            }

            return new MappingItem<SourceT, TargetT>
            {
                SourceType = new MappingTypeInfo { Type = sourceType, Hash = sHash, BaseHash = sBaseHash },
                TargetType = new MappingTypeInfo { Type = targetType, Hash = tHash, BaseHash = tBaseHash },
                ExplicitActions = new Dictionary<Action<SourceT, TargetT>, ExplicitOrders>(),
                MappingProperties = _mappingProps
            };
        }

        #region Explicit/Exclude/Include
        public IMappingItem<SourceT, TargetT> Explicit(Action<SourceT, TargetT> action, ExplicitOrders executionOrder)
        {
            if (ExplicitActions.Keys.FirstOrDefault(f => ActionsEqual(f, action)) == null)
                ExplicitActions.Add(action, executionOrder);

            return this;
        }

        private bool ActionsEqual<T1, T2>(Action<T1, T2> firstAction, Action<T1, T2> secondAction)
        {
            if (firstAction.Target != secondAction.Target) return false;

            var firstMethodBody = firstAction.Method.GetMethodBody().GetILAsByteArray();
            var secondMethodBody = secondAction.Method.GetMethodBody().GetILAsByteArray();

            if (firstMethodBody.Length != secondMethodBody.Length) return false;

            for (var i = 0; i < firstMethodBody.Length; i++)
            {
                if (firstMethodBody[i] != secondMethodBody[i]) return false;
            }

            return true;
        }

        public IMappingItem<SourceT, TargetT> ExplicitField(Expression<Func<TargetT, object>> target, Expression<Func<SourceT, object>> source)
        {
            string tAccName = ExpressionOperations.GetMemberName(target);
            if (tAccName == null) throw new ArgumentException($"Incorrect lambda-expression {target.ToString()} for TargetT({typeof(TargetT).Name})!");

            string sAccName = ExpressionOperations.GetMemberName(source);
            if (sAccName == null) throw new ArgumentException($"Incorrect lambda-expression {source.ToString()} for SourceT({typeof(SourceT).Name})!");

            MemberInfo tMi = typeof(TargetT).GetMember(tAccName)[0],
                sMi = typeof(SourceT).GetMember(sAccName)[0];

            var tMp = MappingProperties.FirstOrDefault(w => w.TargetAccessor == tMi);
            if (tMp == null) throw new ArgumentException($"Property or field {tAccName} not found in TargetT({typeof(TargetT).Name})");

            var sMp = MappingProperties.FirstOrDefault(w => w.SourceAccessor == sMi);
            if (sMp == null) throw new ArgumentException($"Property or field {sAccName} not found in SourceT({typeof(SourceT).Name})");

            if (!tMp.Type.Equals(sMp.Type)) throw new ArgumentException($"TargetT and SourceT type are not equal ({tMp.Type} != {sMp.Type})");
            tMp.SetSourceAccessor(sMi, sMi.MemberType == MemberTypes.Property);

            return this;
        }

        public IMappingItem<SourceT, TargetT> Exclude(Expression<Func<TargetT, object>> expression) => SetMappingPropertyState(expression, false);
        public IMappingItem<SourceT, TargetT> Include(Expression<Func<TargetT, object>> expression) => SetMappingPropertyState(expression, true);

        private IMappingItem<SourceT, TargetT> SetMappingPropertyState(Expression<Func<TargetT, object>> expression, bool state)
        {
            string propName = ExpressionOperations.GetMemberName(expression);
            if (propName == null) throw new ArgumentException($"Incorrect lambda-expression '{expression.ToString()}'!");

            MemberInfo tMi = typeof(TargetT).GetMember(propName)[0];

            MappingProperty mp = MappingProperties.FirstOrDefault(w => w.TargetAccessor == tMi);
            if (mp == null) throw new ArgumentException($"Lambda-expression error '{expression.ToString()}'\r\nProperty '{propName}' not found in class '{typeof(TargetT).ToString()}'!");

            mp.InMapping = state;

            return this;
        }
        #endregion
    }
}
