using System;
using System.Linq.Expressions;
using System.Security.Permissions;

namespace LightMapper.Concrete
{
    internal delegate MapperTemplate<SourceT, TargetT> MapperActivator<SourceT, TargetT>();

    internal class MapperActivator
    {
        public static MapperActivator<SourceT, TargetT> GetActivator<SourceT, TargetT>(Type t)
        {
            var ctor = t.GetConstructor(new Type[0]);

            NewExpression newExp = Expression.New(ctor);
            LambdaExpression lambda = Expression.Lambda(typeof(MapperActivator<SourceT, TargetT>), newExp);

            MapperActivator<SourceT, TargetT> compiled = (MapperActivator<SourceT, TargetT>)lambda.Compile();
            return compiled;
        }
    }
}
