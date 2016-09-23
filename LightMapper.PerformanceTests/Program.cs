using LightMapper.PerformanceTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightMapper.PerformanceTests
{
    class Program
    {
        const int SeedCount = 100000;
        const int CycleCount = 10;

        static LightMapperTest _lightMapperTest;
        static AutoMapperTest _autoMapperTest;
        static ExpressMapperTest _expressMapperTest;

        static List<SourceClass> _sourceList;
        static List<SourceClassSuccessor> _sourceSuccessorList;
        

        static void Main(string[] args)
        {
            SetUp();

            SingleMappingTest();
            SingleSuccessorMappingTest();

            CollectionMappingTest();
            CollectionSuccessorMappingTest();

            Console.ReadLine();
        }

        private static void SingleMappingTest()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mapping of SourceClass to TargetClass for {CycleCount} times");
            Console.ResetColor();
            long amTtl = _autoMapperTest.TestSingle<SourceClass, TargetClass>(_sourceList.First(), CycleCount);
            long lmTtl = _lightMapperTest.TestSingle<SourceClass, TargetClass>(_sourceList.First(), CycleCount);
            long emTtl = _expressMapperTest.TestSingle<SourceClass, TargetClass>(_sourceList.First(), CycleCount);

            SetFastestColor(amTtl, Math.Min(lmTtl, emTtl));
            Console.WriteLine($"AutoMapper:\t{amTtl}ms,\tapprox. {amTtl / CycleCount}ms/run");
            SetFastestColor(emTtl, Math.Min(lmTtl, amTtl));
            Console.WriteLine($"ExpressMapper:\t{emTtl}ms,\tapprox. {emTtl / CycleCount}ms/run");
            SetFastestColor(lmTtl, Math.Min(amTtl, emTtl));
            Console.WriteLine($"LightMapper:\t{lmTtl}ms,\tapprox. {lmTtl / CycleCount}ms/run");

            Console.WriteLine();
        }

        private static void CollectionMappingTest()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mapping of List<SourceClass>(Count = {_sourceList.Count}) to List<TargetClass> for {CycleCount} times");
            Console.ResetColor();
            long amTtl = _autoMapperTest.TestCollection<SourceClass, TargetClass>(_sourceList, CycleCount);
            long lmTtl = _lightMapperTest.TestCollection<SourceClass, TargetClass>(_sourceList, CycleCount);
            long emTtl = _expressMapperTest.TestCollection<SourceClass, TargetClass>(_sourceList, CycleCount);

            SetFastestColor(amTtl, Math.Min(lmTtl, emTtl));
            Console.WriteLine($"AutoMapper:\t{amTtl}ms,\tapprox. {amTtl / CycleCount}ms/run");
            SetFastestColor(emTtl, Math.Min(lmTtl, amTtl));
            Console.WriteLine($"ExpressMapper:\t{emTtl}ms,\tapprox. {emTtl / CycleCount}ms/run");
            SetFastestColor(lmTtl, Math.Min(amTtl, emTtl));
            Console.WriteLine($"LightMapper:\t{lmTtl}ms,\tapprox. {lmTtl / CycleCount}ms/run");
            
            Console.WriteLine();
        }

        private static void SingleSuccessorMappingTest()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mapping of SourceClassSuccessor to TargetClassSuccessor for {CycleCount} times");
            Console.ResetColor();
            long amTtl = _autoMapperTest.TestSingle<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList.First(), CycleCount);
            long lmTtl = _lightMapperTest.TestSingle<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList.First(), CycleCount);
            long emTtl = _expressMapperTest.TestSingle<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList.First(), CycleCount);

            SetFastestColor(amTtl, Math.Min(lmTtl, emTtl));
            Console.WriteLine($"AutoMapper:\t{amTtl}ms,\tapprox. {amTtl / CycleCount}ms/run");
            SetFastestColor(emTtl, Math.Min(lmTtl, amTtl));
            Console.WriteLine($"ExpressMapper:\t{emTtl}ms,\tapprox. {emTtl / CycleCount}ms/run");
            SetFastestColor(lmTtl, Math.Min(amTtl, emTtl));
            Console.WriteLine($"LightMapper:\t{lmTtl}ms,\tapprox. {lmTtl / CycleCount}ms/run");
            
            Console.WriteLine();
        }

        private static void CollectionSuccessorMappingTest()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mapping of List<SourceClassSuccessor>(Count = {_sourceSuccessorList.Count}) to List<TargetClassSuccessor> for {CycleCount} times");
            Console.ResetColor();
            long amTtl = _autoMapperTest.TestCollection<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList, CycleCount);
            long lmTtl = _lightMapperTest.TestCollection<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList, CycleCount);
            long emTtl = _expressMapperTest.TestCollection<SourceClassSuccessor, TargetClassSuccessor>(_sourceSuccessorList, CycleCount);

            SetFastestColor(amTtl, Math.Min(lmTtl, emTtl));
            Console.WriteLine($"AutoMapper:\t{amTtl}ms,\tapprox. {amTtl / CycleCount}ms/run");
            SetFastestColor(emTtl, Math.Min(lmTtl, amTtl));
            Console.WriteLine($"ExpressMapper:\t{emTtl}ms,\tapprox. {emTtl / CycleCount}ms/run");
            SetFastestColor(lmTtl, Math.Min(amTtl, emTtl));
            Console.WriteLine($"LightMapper:\t{lmTtl}ms,\tapprox. {lmTtl / CycleCount}ms/run");
            
            Console.WriteLine();
        }

        private static void SetFastestColor(long timeOne, long timeTwo) => Console.ForegroundColor = timeOne <= timeTwo ? ConsoleColor.Green : ConsoleColor.DarkGreen;

        private static void SetUp()
        {
            _sourceSuccessorList = SeedSourceSuccessonData(SeedCount);
            _sourceList = SeedSourceData();

            Console.WriteLine($"List<SourceClass> seeded with {SeedCount} items");
            Console.WriteLine($"List<SourceClassSuccessor> seeded with {SeedCount} items");
            Console.WriteLine();

            _lightMapperTest = new LightMapperTest();
            _lightMapperTest.SetUp(ExplicitAction);

            _autoMapperTest = new AutoMapperTest();
            _autoMapperTest.SetUp(ExplicitAction);

            _expressMapperTest = new ExpressMapperTest();
            _expressMapperTest.SetUp(ExplicitAction);
        }

        public static void ExplicitAction(SourceClass src, TargetClass trg)
        {
            trg.DiffProp2 = src.DiffProp1;
            trg.DiffField2 = src.DiffField1;
        }

        private static List<SourceClass> SeedSourceData()
        {
            return _sourceSuccessorList.Select(s => s as SourceClass).ToList();
        }

        private static List<SourceClassSuccessor> SeedSourceSuccessonData(int count)
        {
            var lst = new List<SourceClassSuccessor>();

            for (int i = 0; i < count; i++)
            {
                lst.Add(new SourceClassSuccessor
                {
                    BoolProp = i % 2 == 0,
                    DateTimeProp = DateTime.Now.AddDays(-1).AddHours(i),
                    GuidField = Guid.NewGuid(),
                    IntProp = i + 2000,
                    StringField = $"Some string #{i}",
                    TimeSpanProp = DateTime.Now.TimeOfDay,

                    DiffField1 = new Random(i).Next(1, 1000000),
                    DiffProp1 = false,
                    GuidSuccessorProp = Guid.NewGuid(),
                    IntSuccessorProp = new Random(i).Next(1, 1000000),

                    NullProp = i % 2 == 0 ? new DateTime?(DateTime.Now) : null,
                    NullField = i % 2 != 0 ? new TimeSpan?(DateTime.Now.TimeOfDay) : null,

                    SProp = $"Some string #{i}",
                });
            }

            return lst;
        }
    }
}
