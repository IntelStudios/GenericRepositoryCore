using GenericRepository.Models;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Tests
{
    [TestClass]
    public class EntityJoinTypesTest
    {
        static string dbBaseName = "xeelo-tests-gr-cross-model";
        static string dbName = null;

        [ClassInitialize]
        public static void TestInit(TestContext context)
        {
            dbName = TestUtils.InitializeDatabase(dbBaseName);
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            TestUtils.DeleteDatabase(dbName);
        }

        [TestMethod]
        public async Task Get_Entitities_Join_Inner()
        {
            TestEntityJoiningType1Repository gr1 = TestUtils.GetTestEntityJoiningType1Repository(dbName);
            TestEntityJoiningType2Repository gr2 = TestUtils.GetTestEntityJoiningType2Repository(dbName);

            GRJoinedList result = await gr1.GRAll()
                .GRInnerJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToJoinedListAsync();

            Assert.IsTrue(result.Count == 5, "Wrong number of results was returned.");

            foreach (var item in result)
            {
                TestEntityJoiningType1 e1 = item.Get<TestEntityJoiningType1>();
                TestEntityJoiningType2 e2 = item.Get<TestEntityJoiningType2>();

                Assert.IsTrue(e1 != null, "Entity1 is null, but it shouldn't.");
                Assert.IsTrue(e2 != null, "Entity2 is null, but it shouldn't.");

                Assert.IsTrue(e1.TestEntityJoiningType2ID == e2.TestEntityJoiningType2ID, "Items were not joined correctly.");

                Assert.IsTrue(e1.TestEntityJoiningType1Name == string.Format("Name1 {0}", e1.TestEntityJoiningType1ID), "Items was not extracted correctly.");
                Assert.IsTrue(e2.TestEntityJoiningType2Name == string.Format("Name2 {0}", e2.TestEntityJoiningType2ID), "Items was not extracted correctly.");
            }
        }

        [TestMethod]
        public async Task Get_Entitities_Join_Left()
        {
            TestEntityJoiningType1Repository gr1 = TestUtils.GetTestEntityJoiningType1Repository(dbName);
            TestEntityJoiningType2Repository gr2 = TestUtils.GetTestEntityJoiningType2Repository(dbName);

            GRJoinedList result = await gr1.GRAll()
                .GRLeftJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToJoinedListAsync();

            Assert.IsTrue(result.Count == 10, "Wrong number of results was returned.");

            foreach (var item in result)
            {
                TestEntityJoiningType1 e1 = item.Get<TestEntityJoiningType1>();
                TestEntityJoiningType2 e2 = item.Get<TestEntityJoiningType2>();

                Assert.IsTrue(e1 != null, "Entity1 is null, but it shouldn't.");

                if (e1.TestEntityJoiningType2ID > 10)
                {
                    Assert.IsTrue(e2 == null, "Entity2 is not null, but it should.");
                } else
                {
                    Assert.IsTrue(e2 != null, "Entity2 is null, but it shouldn't.");
                    Assert.IsTrue(e1.TestEntityJoiningType2ID == e2.TestEntityJoiningType2ID, "Items were not joined correctly.");
                    Assert.IsTrue(e2.TestEntityJoiningType2Name == string.Format("Name2 {0}", e2.TestEntityJoiningType2ID), "Items was not extracted correctly.");
                }

                Assert.IsTrue(e1.TestEntityJoiningType1Name == string.Format("Name1 {0}", e1.TestEntityJoiningType1ID), "Items was not extracted correctly.");
            }
        }

        [TestMethod]
        public async Task Get_Entitities_Join_Right()
        {
            TestEntityJoiningType1Repository gr1 = TestUtils.GetTestEntityJoiningType1Repository(dbName);
            TestEntityJoiningType2Repository gr2 = TestUtils.GetTestEntityJoiningType2Repository(dbName);

            GRJoinedList result = 
                await gr1
                .GRAll()
                .GRRightJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToJoinedListAsync();

            Assert.IsTrue(result.Count == 10, "Wrong number of results was returned.");

            foreach (GRJoinedListItem item in result)
            {
                TestEntityJoiningType1 e1 = item.Get<TestEntityJoiningType1>();
                TestEntityJoiningType2 e2 = item.Get<TestEntityJoiningType2>();

                Assert.IsTrue(e2 != null, "Entity2 is null, but it shouldn't.");
                Assert.IsTrue(e2.TestEntityJoiningType2Name == string.Format("Name2 {0}", e2.TestEntityJoiningType2ID), "Items was not extracted correctly.");

                if (e2.TestEntityJoiningType2ID % 2 == 1)
                {
                    Assert.IsTrue(e1 == null, "Entity2 is not null, but it should.");
                }
                else
                {
                    Assert.IsTrue(e2 != null, "Entity2 is null, but it shouldn't.");
                    Assert.IsTrue(e1.TestEntityJoiningType2ID == e2.TestEntityJoiningType2ID, "Items were not joined correctly.");
                    Assert.IsTrue(e2.TestEntityJoiningType2Name == string.Format("Name2 {0}", e2.TestEntityJoiningType2ID), "Items was not extracted correctly.");
                }
            }
        }

        [TestMethod]
        public async Task Get_Entitities_Join_Outer()
        {
            TestEntityJoiningType1Repository gr1 = TestUtils.GetTestEntityJoiningType1Repository(dbName);
            TestEntityJoiningType2Repository gr2 = TestUtils.GetTestEntityJoiningType2Repository(dbName);

            GRJoinedList result =
                await gr1
                .GRAll()
                .GRFullOuterJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToJoinedListAsync();

            Assert.IsTrue(result.Count == 15, "Wrong number of results was returned.");

            foreach (GRJoinedListItem item in result)
            {
                TestEntityJoiningType1 e1 = item.Get<TestEntityJoiningType1>();
                TestEntityJoiningType2 e2 = item.Get<TestEntityJoiningType2>();

                Assert.IsTrue(e1 != null || e2 != null, "Both entities cannot be null.");

                if (e1 != null && e2 != null)
                {
                    Assert.IsTrue(e1.TestEntityJoiningType2ID == e2.TestEntityJoiningType2ID, "Items were not joined correctly.");
                }

                if (e1 != null)
                {
                    Assert.IsTrue(e1.TestEntityJoiningType1Name == string.Format("Name1 {0}", e1.TestEntityJoiningType1ID), "Item1 was not extracted correctly.");
                }

                if (e2 != null)
                {
                    Assert.IsTrue(e2.TestEntityJoiningType2Name == string.Format("Name2 {0}", e2.TestEntityJoiningType2ID), "Item2 was not extracted correctly.");
                }
            }
        }
    }
}
