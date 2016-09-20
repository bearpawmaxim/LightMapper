using AutoMapper;
using LightMapper.PerformanceTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMapper.PerformanceTests
{
    internal class AutoMapperTest
    {
        public void SetUp(Action<SourceClass, TargetClass> @explicit)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<SourceClass, TargetClass>()
                    .ForMember(t => t.SField, opt => opt.MapFrom(src => src.SProp))
                    .AfterMap(@explicit);

                cfg.CreateMap<SourceClassSuccessor, TargetClassSuccessor>()
                    .IncludeBase<SourceClass, TargetClass>();
            });
        }

        public long TestSingle<SourceT, TargetT>(SourceT source, int cycleCount)
        {
            long total = 0;
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < cycleCount; i++)
            {
                sw.Restart();
                var ret = Mapper.Map<SourceT, TargetT>(source);
                sw.Stop();
                total += sw.ElapsedMilliseconds;
                GC.Collect();
            }

            return total;
        }

        public long TestCollection<SourceT, TargetT>(List<SourceT> source, int cycleCount)
        {
            long total = 0;
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < cycleCount; i++)
            {
                sw.Restart();
                var ret = Mapper.Map<IEnumerable<SourceT>, IEnumerable<TargetT>>(source);
                sw.Stop();
                total += sw.ElapsedMilliseconds;
                GC.Collect();
            }

            return total;
        }
    }
}
