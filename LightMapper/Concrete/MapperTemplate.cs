using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace LightMapper.Concrete
{
    public abstract class MapperTemplate<SourceT, TargetT>
    {
        public abstract TargetT MapSingle(SourceT input, List<Action<SourceT,TargetT>> bActions, List<Action<SourceT, TargetT>> aActions, Func<TargetT> ctor);

        public IEnumerable<TargetT> MapCollection(IEnumerable<SourceT> input, List<Action<SourceT, TargetT>> bActions, List<Action<SourceT, TargetT>> aActions, Func<TargetT> ctor)
        {
            object locker = new object();

            int inputLen = input.Count();
            TargetT[] ret = new TargetT[input.Count()];

            Parallel.For(0, inputLen, n => { lock (locker) ret[n] = MapSingle(input.ElementAt(n), aActions, bActions, ctor); });

            return ret.ToList();
        }

        public void ExecuteExplicit(List<Action<SourceT, TargetT>> actions, SourceT source, TargetT target)
        {
            for (int i = 0; i < actions.Count; i++)
                actions[i].Invoke(source, target);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
