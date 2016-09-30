using LightMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LightMapper.Concrete
{
    /// <summary>IMappingItem realisation. The core of mapper</summary>
    /// <typeparam name="SourceT">Mapping source type</typeparam>
    /// <typeparam name="TargetT">Mapping target type</typeparam>
    public sealed class MappingItem<SourceT, TargetT> : MappingData<SourceT, TargetT>, IMappingItem<SourceT, TargetT>
        where SourceT : class
        where TargetT : class
    {
        #region Variables
        /// <see cref="IMappingItem.SourceType"/>
        public MappingTypeInfo SourceType { get; private set; }
        /// <see cref="IMappingItem.TargetType"/>
        public MappingTypeInfo TargetType { get; private set; }
        #endregion

        internal static IMappingItem<SourceT, TargetT> CreateMapping(bool mapFields, bool caseSensitive)
        {
            Type sourceType = typeof(SourceT),
                targetType = typeof(TargetT);

            int sHash = sourceType.GetHashCode(),
                tHash = targetType.GetHashCode();

            int? sBaseHash = sourceType.BaseType == typeof(object) ? new int?() : sourceType.BaseType.GetHashCode(),
                tBaseHash = targetType.BaseType == typeof(object) ? new int?() : targetType.BaseType.GetHashCode();

            var _mappingProps = new List<MappingProperty>();

            ReflectionUtils.ProcessPropertyInfo(_mappingProps, sourceType.GetProperties(), targetType.GetProperties(), caseSensitive);
            ReflectionUtils.ProcessFieldInfo(mapFields, _mappingProps, sourceType.GetFields(), targetType.GetFields(), caseSensitive);

            return new MappingItem<SourceT, TargetT>
            {
                SourceType = new MappingTypeInfo { Type = sourceType, Hash = sHash, BaseHash = sBaseHash },
                TargetType = new MappingTypeInfo { Type = targetType, Hash = tHash, BaseHash = tBaseHash },
                ExplicitActions = new Dictionary<Action<SourceT, TargetT>, ExplicitOrders>(),
                MappingProperties = _mappingProps
            };
        }

        /// <see cref="IMappingItem{SourceT, TargetT}.SetConstructorFunc(Func{TargetT})"/> 
        public IMappingItem<SourceT, TargetT> SetConstructorFunc(Func<TargetT> ctor)
        {
            ClassCtor = ctor;

            return this;
        }

        #region Explicit/Exclude/Include
        /// <see cref="IMappingItem{SourceT, TargetT}.Exclude(Expression{Func{TargetT, object}})"/>
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
        /// <see cref="IMappingItem{SourceT, TargetT}.ExplicitMember(Expression{Func{TargetT, object}}, Expression{Func{SourceT, object}})"/>
        public IMappingItem<SourceT, TargetT> ExplicitMember(Expression<Func<TargetT, object>> target, Expression<Func<SourceT, object>> source)
        {
            string tAccName = ExpressionOperations.GetMemberName(target);
            if (tAccName == null) throw new ArgumentException($"Incorrect lambda-expression {target.ToString()} for TargetT({typeof(TargetT).Name})!");

            string sAccName = ExpressionOperations.GetMemberName(source);
            if (sAccName == null) throw new ArgumentException($"Incorrect lambda-expression {source.ToString()} for SourceT({typeof(SourceT).Name})!");

            MemberInfo tMi = typeof(TargetT).GetMember(tAccName)[0],
                sMi = typeof(SourceT).GetMember(sAccName)[0];

            Type tType = tMi.MemberType == MemberTypes.Property ? (tMi as PropertyInfo).PropertyType : (tMi as FieldInfo).FieldType,
                sType = sMi.MemberType == MemberTypes.Property ? (sMi as PropertyInfo).PropertyType : (sMi as FieldInfo).FieldType;

            var tMp = MappingProperties.FirstOrDefault(w => w.TargetAccessor == tMi);
            if (tMp == null) throw new ArgumentException($"Property or field {tAccName} not found in TargetT({typeof(TargetT).Name})");

            var sMp = MappingProperties.FirstOrDefault(w => w.SourceAccessor == sMi);
            if (sMp == null) throw new ArgumentException($"Property or field {sAccName} not found in SourceT({typeof(SourceT).Name})");

            if (!tType.Equals(sType)) throw new ArgumentException($"TargetT and SourceT type are not equal ({tType.FullName} != {sType.FullName})");
            tMp.SetSourceAccessor(sMi);

            return this;
        }

        /// <see cref="IMappingItem{SourceT, TargetT}.Exclude(Expression{Func{TargetT, object}})"/>
        public IMappingItem<SourceT, TargetT> Exclude(Expression<Func<TargetT, object>> expression) => SetMappingPropertyState(expression, false);
        /// <see cref="IMappingItem{SourceT, TargetT}.Include(Expression{Func{TargetT, object}})"/>
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

        /// <see cref="IMappingItem{SourceT, TargetT}.AddBaseExplicit{BSourceT, BTargetT}(IMappingItem{BSourceT, BTargetT})"/>
        public IMappingItem<SourceT, TargetT> AddBaseExplicit<BSourceT, BTargetT>(IMappingItem<BSourceT, BTargetT> baseMapping)
            where BSourceT: class
            where BTargetT: class
        {
            if (!SourceType.BaseHash.HasValue) throw new ArgumentException($"TargetT type {typeof(SourceT).Name} doesn't have base type!");
            if (!TargetType.BaseHash.HasValue) throw new ArgumentException($"SourceT type {typeof(TargetT).Name} doesn't have base type!");

            foreach (var md in (baseMapping as MappingData<BSourceT, BTargetT>).ExplicitActions)
            {
                Action<SourceT, TargetT> act = md.Key as Action<SourceT, TargetT>;
                ExplicitActions.Add(act, md.Value);
            }

            return this;
        }
        #endregion
    }
}
