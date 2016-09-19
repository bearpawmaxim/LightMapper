using NUnit.Framework;
using LightMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightMapper.Tests.Infrastructure;
using LightMapper.Infrastructure;

namespace LightMapper.Tests
{
    [TestFixture]
    public class LightMapperTests
    {
        private IMapper _mapper;
        private List<SourceClass> _sourceList;
        private List<SourceClassSuccessor> _successorList;

        [SetUp]
        public void SetUp()
        {
            _mapper = new LightMapper();
            var mappingItem = _mapper.CreateMapping<SourceClass, TargetClass>(true)
                .Explicit((s, t) => t.DiffField2 = s.DiffField1, ExplicitOrders.AfterMap)
                .Explicit((s, t) => t.DiffProp2 = s.DiffProp1, ExplicitOrders.BeforeMap);
            _mapper.AddMapping(mappingItem);

            var mappingItem2 = _mapper.CreateMapping<SourceClassSuccessor, TargetClassSuccessor>(true);
            _mapper.AddMapping(mappingItem2);

            _sourceList = SeedSourceData(100000);
            _successorList = SeedSourceSuccessonData(100000);
        }

        #region Simple mapping
        [Test]
        public void SingleMapTest()
        {
            var src = _sourceList.Last();
            var ret = _mapper.Map<SourceClass, TargetClass>(src);

            Assert.IsNotNull(ret);

            AssertClassesEqual(src, ret, false);
        }

        [Test]
        public void CollectionMapTest()
        {
            var ret = _mapper.Map<SourceClass, TargetClass>(_sourceList);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Any());

            for (int i = 0; i < _sourceList.Count(); i ++)
            {
                AssertClassesEqual(_sourceList[i], (ret as IList<TargetClass>)[i], false);
            }
        }

        [Test]
        public void SingleBaseExplicitTest()
        {
            var src = _successorList.First();

            var ret = _mapper.Map<SourceClassSuccessor, TargetClassSuccessor>(src);

            AssertClassesEqual(src, ret, true);
        }

        [Test]
        public void CollectionBaseExplicitTest()
        {
            var ret = _mapper.Map<SourceClassSuccessor, TargetClassSuccessor>(_successorList);

            for (int i = 0; i < _successorList.Count(); i++)
            {
                AssertClassesEqual(_successorList[i], (ret as List<TargetClassSuccessor>)[i], true);
            }
        }

        #endregion
        #region Private methods
        private List<SourceClass> SeedSourceData(int count)
        {
            var lst = new List<SourceClass>();

            for (int i = 0; i < count; i++)
            {
                lst.Add(new SourceClass
                {
                    BoolProp = i % 2 == 0,
                    DateTimeProp = DateTime.Now.AddDays(-1).AddHours(i),
                    GuidField = Guid.NewGuid(),
                    IntProp = i + 2000,
                    StringField = $"Some string #{i}",
                    TimeSpanProp = DateTime.Now.TimeOfDay,

                    DiffField1 = new Random(i).Next(1, 1000000),
                    DiffProp1 = false,

                    NullProp = i % 2 == 0 ? new DateTime?(DateTime.Now) : null,
                    NullField = i % 2 != 0 ? new TimeSpan?(DateTime.Now.TimeOfDay) : null
                });
            }

            return lst;
        }

        private List<SourceClassSuccessor> SeedSourceSuccessonData(int count)
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
                    NullField = i % 2 != 0 ? new TimeSpan?(DateTime.Now.TimeOfDay) : null
                });
            }

            return lst;
        }

        private void AssertClassesEqual(SourceClass src, TargetClass trg, bool withDiffMembers)
        {
            Assert.AreEqual(trg.BoolProp, src.BoolProp, "Field/Property BoolProp arent equal!");
            Assert.AreEqual(trg.DateTimeProp, src.DateTimeProp, "Field/Property DateTimeProp arent equal!");
            Assert.AreEqual(trg.GuidField, src.GuidField, "Field/Property GuidField arent equal!");
            Assert.AreEqual(trg.IntProp, src.IntProp, "Field/Property IntProp arent equal!");
            Assert.AreEqual(trg.StringField, src.StringField, "Field/Property StringField arent equal!");
            Assert.AreEqual(trg.TimeSpanProp, src.TimeSpanProp, "Field/Property TimeSpanProp arent equal!");

            if (withDiffMembers)
            {
                Assert.AreEqual(trg.DiffField2, src.DiffField1, "Field/Property DiffField1 arent equal!");
                Assert.AreEqual(trg.DiffProp2, src.DiffProp1, "Field/Property DiffProp1 arent equal!");
            }
        }
        #endregion
    }
}