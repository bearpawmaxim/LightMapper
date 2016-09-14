using NMapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NMapper.Concrete
{
    public delegate TargetT MapSingleDlg<SourceT, TargetT>(SourceT source, Action<SourceT, TargetT>[] actions);
    public delegate IEnumerable<TargetT> MapEnumerableDlg<SourceT, TargetT>(IEnumerable<SourceT> source, Action<SourceT, TargetT>[] actions);

    internal class MappingCompiler<SourceT, TargetT>
        //where SourceT : class
        //where TargetT : new()
    {
        private static DynamicMethod CreateDynamicMapMethod(Type sourceType, Type targetType) => new DynamicMethod("DynamicCopy", targetType, new Type[] { sourceType, typeof(Action<SourceT, TargetT>[]) }, typeof(MappingCompiler<SourceT, TargetT>), true);

        public void Compile(ref MappingData<SourceT, TargetT> mappingItem)
        {
            Type sourceType = typeof(SourceT),
                targetType = typeof(TargetT);

#if DEBUG_COMPILER
            var mAsmName = $"MappingAsm_{sourceType.Name}_{targetType.Name}.dll";

            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = "MappingAsm";
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = ab.DefineDynamicModule("MappingAsm", mAsmName);
            TypeBuilder tb = mb.DefineType("iNnerMapper", TypeAttributes.Public);

            MethodBuilder mSingle = tb.DefineMethod("MapSingle", MethodAttributes.Public | MethodAttributes.Static, typeof(TargetT), new[] { typeof(SourceT), typeof(Action<SourceT, TargetT>[]) });
            var ilGenSng = mSingle.GetILGenerator();

            MethodBuilder mCollection = tb.DefineMethod("MapEnumerable", MethodAttributes.Public | MethodAttributes.Static, typeof(IEnumerable<TargetT>), new[] { typeof(IEnumerable<SourceT>), typeof(Action<SourceT, TargetT>[]) });
            var ilGenArr = mCollection.GetILGenerator();
#else
            var mSingle = CreateDynamicMapMethod(sourceType, targetType);
            var mArray = CreateDynamicMapMethod(typeof(IEnumerable<SourceT>), typeof(IEnumerable<TargetT>));

            var ilGenSng = mSingle.GetILGenerator();
            var ilGenArr = mArray.GetILGenerator();
#endif
            EmitSingleMapping(ilGenSng, mappingItem);
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
            generator.DeclareLocal(typeof(IEnumerable<TargetT>));

            var lbl1 = generator.DefineLabel();
            var lbl2 = generator.DefineLabel();
            var lbl3 = generator.DefineLabel();
            var lbl4 = generator.DefineLabel();
            var lbl5 = generator.DefineLabel();

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Newobj, typeof(List<TargetT>).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null));
            generator.Emit(OpCodes.Stloc_0);
            generator.Emit(OpCodes.Nop);

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
            generator.Emit(OpCodes.Call, single);
            generator.Emit(OpCodes.Callvirt, typeof(List<TargetT>).GetMethod("Add"));
            generator.Emit(OpCodes.Nop);

            generator.MarkLabel(lbl2);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Callvirt, typeof(System.Collections.IEnumerator).GetMethod("MoveNext"));
            generator.Emit(OpCodes.Brtrue_S, lbl1);
            generator.Emit(OpCodes.Leave_S, lbl4);

            generator.BeginFinallyBlock();

            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Brfalse_S, lbl3);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"));
            generator.Emit(OpCodes.Nop);
            generator.MarkLabel(lbl3);
            generator.Emit(OpCodes.Endfinally);

            generator.EndExceptionBlock();

            generator.MarkLabel(lbl4);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Stloc_3);
            generator.Emit(OpCodes.Br_S, lbl5);

            generator.MarkLabel(lbl5);
            generator.Emit(OpCodes.Ldloc_3);
            generator.Emit(OpCodes.Ret);
        }

        private void EmitSingleMapping(ILGenerator generator, MappingData<SourceT, TargetT> mappingItem)
        {
            generator.DeclareLocal(typeof(TargetT));
            generator.DeclareLocal(typeof(int));
            generator.DeclareLocal(typeof(bool));
            generator.DeclareLocal(typeof(TargetT));

            var condLbl = generator.DefineLabel();
            var bodyLbl = generator.DefineLabel();
            var retLbl = generator.DefineLabel();

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Newobj, typeof(TargetT).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null));
            

            foreach (var memb in mappingItem.MappingProperties.Where(w => w.SourceAccessor != null && w.TargetAccessor != null && w.InMapping))
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldarg_0);

                if (memb.IsProperty)
                {
                    generator.Emit(OpCodes.Callvirt, (memb.SourceAccessor as PropertyInfo).GetGetMethod());
                    generator.Emit(OpCodes.Callvirt, (memb.TargetAccessor as PropertyInfo).GetSetMethod());
                    generator.Emit(OpCodes.Nop);
                }
                else
                {
                    generator.Emit(OpCodes.Ldfld, memb.SourceAccessor as FieldInfo);
                    generator.Emit(OpCodes.Stfld, memb.TargetAccessor as FieldInfo);
                }
            }
            generator.Emit(OpCodes.Stloc_0);

            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Stloc_1);
            generator.Emit(OpCodes.Br_S, condLbl);

            generator.MarkLabel(bodyLbl);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Ldelem_Ref);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, typeof(Action<SourceT, TargetT>).GetMethod("Invoke"));
            generator.Emit(OpCodes.Nop);

            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Ldc_I4_1);
            generator.Emit(OpCodes.Add);
            generator.Emit(OpCodes.Stloc_1);

            generator.MarkLabel(condLbl);

            generator.Emit(OpCodes.Ldloc_1);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldlen);
            generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Clt);
            generator.Emit(OpCodes.Stloc_2);
            generator.Emit(OpCodes.Ldloc_2);
            generator.Emit(OpCodes.Brtrue_S, bodyLbl);

            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Stloc_3);

            generator.Emit(OpCodes.Br_S, retLbl);

            generator.MarkLabel(retLbl);
            generator.Emit(OpCodes.Ldloc_3);
            generator.Emit(OpCodes.Ret);
        }
    }
}
