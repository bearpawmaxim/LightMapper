using NUnit.Framework;
using NMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMapper.Tests.Infrastructure;

namespace NMapper.Tests
{
    [TestFixture]
    public class NMapperTests
    {
        private IMapper _mapper;
        private List<SourceClass> _sourceList;

        [SetUp]
        public void SetUp()
        {
            _mapper = NMapper.Instance;
            var mappingItem = _mapper.CreateMapping<SourceClass, TargetClass>(true);
            
            _mapper.AddMapping(mappingItem);
            _sourceList = SeedSourceData(100000);
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
                AssertClassesEqual(_sourceList[i], (ret as List<TargetClass>)[i], false);
            }
        }
        #endregion
        [Test]
        public void SingleMapWDiffTest()
        {
            var mi = _mapper.GetMapping<SourceClass, TargetClass>();

            mi.ExplicitField(t => t.DiffField2, s => s.DiffField1).
                ExplicitField(t => t.DiffProp2, s => s.DiffProp1);

            _mapper.UpdateMapping(mi);

            var src = _sourceList.Last();
            var ret = _mapper.Map<SourceClass, TargetClass>(src);

            Assert.IsNotNull(ret);
            AssertClassesEqual(src, ret, true);
        }

        [Test]
        public void CollectionMapWDiffTest()
        {
            var mi = _mapper.GetMapping<SourceClass, TargetClass>();
            mi.ExplicitField(t => t.DiffField2, s => s.DiffField1).
                ExplicitField(t => t.DiffProp2, s => s.DiffProp1);

            _mapper.UpdateMapping(mi);

            var ret = _mapper.Map<SourceClass, TargetClass>(_sourceList);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Any());

            for (int i = 0; i < _sourceList.Count(); i++)
            {
                AssertClassesEqual(_sourceList[i], (ret as List<TargetClass>)[i], true);
            }
        }
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

                    DiffField1 = i / .42m + i,
                    DiffProp1 = false
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