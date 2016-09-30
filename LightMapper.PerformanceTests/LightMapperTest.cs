using LightMapper.Infrastructure;
using LightMapper.PerformanceTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightMapper.PerformanceTests
{
    internal class LightMapperTest
    {
        IMapper _mapper;

        public void SetUp(Action<SourceClass, TargetClass> @explicit)
        {
            _mapper = LightMapper.Instance;
            var mi = _mapper.CreateMapping<SourceClass, TargetClass>(true)
                .Explicit(@explicit, ExplicitOrders.AfterMap)
                .ExplicitMember(t => t.SField, s => s.SProp)
                .SetConstructorFunc(() => new TargetClass());

            _mapper.AddMapping(mi);

            var miSucc = _mapper.CreateMapping<SourceClassSuccessor, TargetClassSuccessor>(true)
                .AddBaseExplicit(_mapper.GetMapping<SourceClass, TargetClass>());
            _mapper.AddMapping(miSucc);
        }

        public long TestSingle<SourceT, TargetT>(SourceT source, int cycleCount)
            where SourceT : class
            where TargetT : class
        {
            long total = 0;
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < cycleCount; i++)
            {
                sw.Restart();
                var ret = _mapper.Map<SourceT, TargetT>(source);
                sw.Stop();
                total += sw.ElapsedMilliseconds;
                GC.Collect();
            }

            return total;
        }

        public long TestCollection<SourceT, TargetT>(List<SourceT> source, int cycleCount)
            where SourceT : class
            where TargetT : class
        {
            long total = 0;
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < cycleCount; i++)
            {
                sw.Restart();
                var ret = _mapper.Map<SourceT, TargetT>(source);
                sw.Stop();
                total += sw.ElapsedMilliseconds;
                GC.Collect();
            }

            return total;
        }
    }
}
