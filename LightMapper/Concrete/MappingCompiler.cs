using LightMapper.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;

namespace LightMapper.Concrete
{
    internal class MappingCompiler<SourceT, TargetT>
    {
        public void Compile(MappingData<SourceT, TargetT> mappingItem, out MapperActivator<SourceT, TargetT> outActivator)
        {
            Type sourceType = typeof(SourceT),
                targetType = typeof(TargetT);

            var mAsmName = $"MappingAsm_{sourceType.Name}_{targetType.Name}.dll";

            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName an = new AssemblyName($"MappingAsm_{sourceType.Name}_{targetType.Name}");//Assembly.GetAssembly(sourceType).GetName();
            AssemblyBuilder ab = ad.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            
            var moduleName = targetType.Module.Name;
            ModuleBuilder mb = ab.DefineDynamicModule("MappingAsm", mAsmName);

            TypeBuilder tb = mb.DefineType($"{targetType.Namespace}.Mapper_{sourceType.Name}_{targetType.Name}", TypeAttributes.Public, typeof(MapperTemplate<SourceT, TargetT>));
            
            MethodBuilder mSingle = tb.DefineMethod("MapSingle",
                MethodAttributes.Public |
                MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig,
                CallingConventions.Standard,
                typeof(TargetT),
                new[] { typeof(SourceT), typeof(List<Action<SourceT, TargetT>>), typeof(List<Action<SourceT, TargetT>>), typeof(Func<TargetT>) }
            );

            var ilGenSng = mSingle.GetILGenerator();

            EmitMapping(ilGenSng, mappingItem);
            tb.DefineMethodOverride(mSingle, typeof(MapperTemplate<SourceT, TargetT>).GetMethod("MapSingle"));

            var mapperType = tb.CreateType();
            outActivator = MapperActivator.GetActivator<SourceT, TargetT>(mapperType);
#if DEBUG_COMPILER
            ab.Save(mAsmName);
#endif
        }

        private void EmitMapping(ILGenerator generator, MappingData<SourceT, TargetT> mappingItem)
        {
            generator.DeclareLocal(typeof(TargetT));
            generator.DeclareLocal(typeof(TargetT));

            var lbl1 = generator.DefineLabel();

            generator.Emit(OpCodes.Nop);

            if (mappingItem.ClassCtor != null)
            {
                generator.Emit(OpCodes.Ldarg_S, 4);
                generator.Emit(OpCodes.Call, typeof(Func<TargetT>).GetMethod("Invoke"));
            }
            else
                generator.Emit(OpCodes.Newobj, typeof(TargetT).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null));

            generator.Emit(OpCodes.Stloc_0);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Call, typeof(MapperTemplate<SourceT, TargetT>).GetMethod("ExecuteExplicit"));
            generator.Emit(OpCodes.Nop);

            foreach (var mp in mappingItem.MappingProperties.Where(w => w.InMapping && w.SourceAccessor != null && w.TargetAccessor != null))
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_1);

                if (mp.SourceAccessor.MemberType == MemberTypes.Property)
                    generator.Emit(OpCodes.Callvirt, (mp.SourceAccessor as PropertyInfo).GetGetMethod());
                else
                    generator.Emit(OpCodes.Ldfld, mp.SourceAccessor as FieldInfo);

                if (mp.TargetAccessor.MemberType == MemberTypes.Property)
                    generator.Emit(OpCodes.Callvirt, (mp.TargetAccessor as PropertyInfo).GetSetMethod());
                else
                    generator.Emit(OpCodes.Stfld, mp.TargetAccessor as FieldInfo);
            }

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_3);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Call, typeof(MapperTemplate<SourceT, TargetT>).GetMethod("ExecuteExplicit"));
            generator.Emit(OpCodes.Nop);

            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Stloc_1);
            generator.Emit(OpCodes.Br_S, lbl1);

            generator.MarkLabel(lbl1);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Ret);
        }
    }
}
