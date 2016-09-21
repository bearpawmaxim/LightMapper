using LightMapper.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LightMapper.Concrete
{
    internal delegate TargetT MapSingleDlg<SourceT, TargetT>(SourceT source, IEnumerable<Action<SourceT, TargetT>> beforeActions, IEnumerable<Action<SourceT, TargetT>> afterActions);
    internal delegate IEnumerable<TargetT> MapEnumerableDlg<SourceT, TargetT>(IEnumerable<SourceT> source, IEnumerable<Action<SourceT, TargetT>> beforeActions, IEnumerable<Action<SourceT, TargetT>> afterActions);

    internal class MappingCompiler<SourceT, TargetT>
    {
        private static DynamicMethod CreateDynamicMapMethod(Type sourceType, Type targetType) => new DynamicMethod("DynamicCopy", targetType, new Type[] { sourceType, typeof(IEnumerable<Action<SourceT, TargetT>>), typeof(IEnumerable<Action<SourceT, TargetT>>) }, typeof(MappingCompiler<SourceT, TargetT>), true);

        public void Compile(ref MappingData<SourceT, TargetT> mappingItem)
        {
            Type sourceType = typeof(SourceT),
                targetType = typeof(TargetT);

#if DEBUG_COMPILER
            var mAsmName = $"MappingAsm_{sourceType.Name}_{targetType.Name}.dll";

            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = "MappingAsm";
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = ab.DefineDynamicModule("MappingAsmMod", mAsmName);
            TypeBuilder tb = mb.DefineType("LightMapperMappings", TypeAttributes.Public);

            MethodBuilder mExplicit = tb.DefineMethod("ExecuteExplicit", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new[] { typeof(IEnumerable<Action<SourceT, TargetT>>), typeof(SourceT), typeof(TargetT) });
            var ilGenExpl = mExplicit.GetILGenerator();

            MethodBuilder mSingle = tb.DefineMethod("MapSingle", MethodAttributes.Public | MethodAttributes.Static, typeof(TargetT), new[] { typeof(SourceT), typeof(IEnumerable<Action<SourceT, TargetT>>), typeof(IEnumerable<Action<SourceT, TargetT>>) });
            var ilGenSng = mSingle.GetILGenerator();

            MethodBuilder mCollection = tb.DefineMethod("MapEnumerable", MethodAttributes.Public | MethodAttributes.Static, typeof(IEnumerable<TargetT>), new[] { typeof(IEnumerable<SourceT>), typeof(IEnumerable<Action<SourceT, TargetT>>), typeof(IEnumerable<Action<SourceT, TargetT>>) });
            var ilGenArr = mCollection.GetILGenerator();
#else
            var mExplicit = new DynamicMethod("ExecuteExplicit", typeof(void), new[] { typeof(IEnumerable<Action<SourceT, TargetT>>), sourceType, targetType }, typeof(MappingCompiler<SourceT, TargetT>), true);
            var mSingle = CreateDynamicMapMethod(sourceType, targetType);
            var mArray = CreateDynamicMapMethod(typeof(IEnumerable<SourceT>), typeof(IEnumerable<TargetT>));

            var ilGenExpl = mExplicit.GetILGenerator();
            var ilGenSng = mSingle.GetILGenerator();
            var ilGenArr = mArray.GetILGenerator();
#endif
            EmitExplicitExecutor(ilGenExpl);
            EmitSingleMapping(ilGenSng, mappingItem, mExplicit);
            EmitCollectionMapping(ilGenArr, mappingItem, mSingle);

#if DEBUG_COMPILER
            tb.CreateType();
            ab.Save(mAsmName);

            mappingItem.MapSingleMethod = tb.GetMethod("MapSingle").CreateDelegate(typeof(MapSingleDlg<SourceT, TargetT>)) as MapSingleDlg<SourceT, TargetT>;
            mappingItem.MapCollectionMethod = tb.GetMethod("MapEnumerable").CreateDelegate(typeof(MapEnumerableDlg<SourceT, TargetT>)) as MapEnumerableDlg<SourceT, TargetT>;
#else
            mappingItem.MapSingleMethod = mSingle.CreateDelegate(typeof(MapSingleDlg<SourceT, TargetT>)) as MapSingleDlg<SourceT, TargetT>;
            mappingItem.MapCollectionMethod = mArray.CreateDelegate(typeof(MapEnumerableDlg<SourceT, TargetT>)) as MapEnumerableDlg<SourceT, TargetT>;
#endif
        }

        private void EmitCollectionMapping(ILGenerator generator, MappingData<SourceT, TargetT> mappingItem, MethodInfo single)
        {
            generator.DeclareLocal(typeof(List<TargetT>));
            generator.DeclareLocal(typeof(IEnumerator<SourceT>));
            generator.DeclareLocal(typeof(SourceT));
            generator.DeclareLocal(typeof(int));

            var lbl1 = generator.DefineLabel();
            var lbl2 = generator.DefineLabel();
            var lbl3 = generator.DefineLabel();
            var lbl4 = generator.DefineLabel();

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, typeof(ICollection).GetProperty("Count").GetGetMethod());
            generator.Emit(OpCodes.Stloc_3);

            generator.Emit(OpCodes.Ldloc_3);
            generator.Emit(OpCodes.Newobj, typeof(List<TargetT>).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int) }, null));
            generator.Emit(OpCodes.Stloc_0);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, typeof(IEnumerable<SourceT>).GetMethod("GetEnumerator"));
            generator.Emit(OpCodes.Stloc_1);

            generator.BeginExceptionBlock();

            generator.Emit(OpCodes.Br_S, lbl2);

            generator.MarkLabel(lbl1);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Callvirt, typeof(IEnumerator<SourceT>).GetProperty("Current").GetGetMethod());
            generator.Emit(OpCodes.Stloc_2);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ldloc_2);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Call, single);
            generator.Emit(OpCodes.Callvirt, typeof(List<TargetT>).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(TargetT) }, null));
            generator.MarkLabel(lbl2);
            generator.Emit(OpCodes.Ldloc_1);

            generator.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
            generator.Emit(OpCodes.Brtrue_S, lbl1);

            generator.BeginFinallyBlock();

            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Brfalse_S, lbl3);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"));
            generator.MarkLabel(lbl3);

            generator.EndExceptionBlock();

            generator.MarkLabel(lbl4);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);
        }

        private void EmitExplicitExecutor(ILGenerator generator)
        {
            generator.DeclareLocal(typeof(IEnumerator<Action<SourceT, TargetT>>));
            var lbl1 = generator.DefineLabel();
            var lbl2 = generator.DefineLabel();
            var lbl3 = generator.DefineLabel();
            var lbl4 = generator.DefineLabel();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, typeof(IEnumerable<Action<SourceT, TargetT>>).GetMethod("GetEnumerator"));
            generator.Emit(OpCodes.Stloc_0);

            generator.BeginExceptionBlock();
            generator.Emit(OpCodes.Br_S, lbl2);

            generator.MarkLabel(lbl1);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, typeof(IEnumerator<Action<SourceT, TargetT>>).GetProperty("Current").GetGetMethod());

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Callvirt, typeof(Action<SourceT, TargetT>).GetMethod("Invoke"));

            generator.MarkLabel(lbl2);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
            generator.Emit(OpCodes.Brtrue_S, lbl1);

            generator.BeginFinallyBlock();

            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Brfalse_S, lbl3);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"));
            generator.MarkLabel(lbl3);

            generator.EndExceptionBlock();

            generator.MarkLabel(lbl4);
            generator.Emit(OpCodes.Ret);
        }

        private void EmitSingleMapping(ILGenerator generator, MappingData<SourceT, TargetT> mappingItem, MethodInfo @explicit)
        {
            generator.DeclareLocal(typeof(TargetT));

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Newobj, typeof(TargetT).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null));
            generator.Emit(OpCodes.Stloc_0);

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Call, @explicit);

            foreach (var mp in mappingItem.MappingProperties.Where(w => w.InMapping && w.SourceAccessor != null && w.TargetAccessor != null))
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_0);

                if (mp.SourceAccessor.MemberType == MemberTypes.Property)
                    generator.Emit(OpCodes.Callvirt, (mp.SourceAccessor as PropertyInfo).GetGetMethod());
                else
                    generator.Emit(OpCodes.Ldfld, mp.SourceAccessor as FieldInfo);

                if (mp.TargetAccessor.MemberType == MemberTypes.Property)
                    generator.Emit(OpCodes.Callvirt, (mp.TargetAccessor as PropertyInfo).GetSetMethod());
                else
                    generator.Emit(OpCodes.Stfld, mp.TargetAccessor as FieldInfo);
            }

            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Call, @explicit);

            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);
        }
    }
}
