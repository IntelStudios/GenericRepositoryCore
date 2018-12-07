using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.CRUD
{
    [TestClass]
    public class Join
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

            GRTable result = await gr1.GRAll()
                .GRInnerJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToTableAsync();

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

            GRTable result = await gr1.GRAll()
                .GRLeftJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToTableAsync();

            Assert.IsTrue(result.Count == 10, "Wrong number of results was returned.");

            foreach (var item in result)
            {
                TestEntityJoiningType1 e1 = item.Get<TestEntityJoiningType1>();
                TestEntityJoiningType2 e2 = item.Get<TestEntityJoiningType2>();

                Assert.IsTrue(e1 != null, "Entity1 is null, but it shouldn't.");

                if (e1.TestEntityJoiningType2ID > 10)
                {
                    Assert.IsTrue(e2 == null, "Entity2 is not null, but it should.");
                }
                else
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

            GRTable result =
                await gr1
                .GRAll()
                .GRRightJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToTableAsync();

            Assert.IsTrue(result.Count == 10, "Wrong number of results was returned.");

            foreach (GRTableRow item in result)
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

            GRTable result =
                await gr1
                .GRAll()
                .GRFullOuterJoin<TestEntityJoiningType2>(e1 => e1.TestEntityJoiningType2ID, e2 => e2.TestEntityJoiningType2ID)
                .GRToTableAsync();

            Assert.IsTrue(result.Count == 15, "Wrong number of results was returned.");

            foreach (GRTableRow item in result)
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

        [TestMethod]
        public async Task Get_Entities_Join()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", list.Count, TestUtils.TestCollectionSize);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e1.Name), "Name was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e1.CreatedDate == TestUtils.DefaultCreatedDate, "CreatedDate was not loaded");
                Assert.IsTrue(e1.ModifiedDate == TestUtils.DefaultCreatedDate, "ModifiedDate was not loaded");
                Assert.IsTrue(e1.CreatedBy == -1, "CreatedBy was not loaded");
                Assert.IsTrue(e1.ModifiedBy == -1, "ModifiedBy was not loaded");
                Assert.IsTrue(e1.IsActive, "IsActive was not loaded");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "TestEntity2ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "TestEntity1ID was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e2.Description), "Description was not loaded.");

                Assert.IsTrue(e1.TestEntityAutoPropertiesID == e2.TestEntityAutoPropertiesID, "TestEntity1ID != TestEntity1ID");

                Assert.IsTrue(e1.Name == string.Format(TestUtils.NameFormatString, e1.TestEntityAutoPropertiesID), "Entity was not loaded properly.");
                Assert.IsTrue(e2.Description == string.Format("Description {0}", e2.TestEntityJoiningID), "Entity was not loaded properly.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Exclude_E1_All()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRExcludeAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with excluded entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", list.Count, TestUtils.TestCollectionSize);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1 == null, "Entity was loaded.");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e2.Description), "Description was not loaded.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Exclude_E2_All()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRExcludeAll()
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with excluded entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", list.Count, TestUtils.TestCollectionSize);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e1.Name), "Name was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e1.CreatedDate == TestUtils.DefaultCreatedDate, "CreatedDate was not loaded");
                Assert.IsTrue(e1.ModifiedDate == TestUtils.DefaultCreatedDate, "ModifiedDate was not loaded");
                Assert.IsTrue(e1.CreatedBy == -1, "CreatedBy was not loaded");
                Assert.IsTrue(e1.ModifiedBy == -1, "ModifiedBy was not loaded");
                Assert.IsTrue(e1.IsActive, "IsActive was not loaded");

                Assert.IsTrue(e2 == null, "Entity was loaded.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Exclude_E1_Name()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRExclude(e1 => e1.Name)
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with excluded Entity1.Name - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", list.Count, TestUtils.TestCollectionSize);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.Name), "Name was loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e1.CreatedDate == TestUtils.DefaultCreatedDate, "CreatedDate was loaded");
                Assert.IsTrue(e1.ModifiedDate == TestUtils.DefaultCreatedDate, "ModifiedDate was loaded");
                Assert.IsTrue(e1.CreatedBy == -1, "CreatedBy was loaded");
                Assert.IsTrue(e1.ModifiedBy == -1, "ModifiedBy was loaded");
                Assert.IsTrue(e1.IsActive, "IsActive was loaded");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e2.Description), "Description was not loaded.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Exclude_E2_Description()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRExclude(e2 => e2.Description)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with excluded Description - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", list.Count, TestUtils.TestCollectionSize);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e1.Name), "Name was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e2.Description), "Description was loaded.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Where_E1_ID_EQ_10()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRWhere(e1 => e1.TestEntityAutoPropertiesID == 10)
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with where clause ID = {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == 1, "Returned {0} entities instead of 1.", list.Count);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e1.TestEntityAutoPropertiesID == 10, "ID was not loaded correctly.");

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e1.Name), "Name was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e1.CreatedDate == TestUtils.DefaultCreatedDate, "CreatedDate was loaded");
                Assert.IsTrue(e1.ModifiedDate == TestUtils.DefaultCreatedDate, "ModifiedDate was loaded");
                Assert.IsTrue(e1.CreatedBy == -1, "CreatedBy was loaded");
                Assert.IsTrue(e1.ModifiedBy == -1, "ModifiedBy was loaded");
                Assert.IsTrue(e1.IsActive, "IsActive was loaded");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e2.Description), "Description was not loaded.");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join_Where_E2_ID_EQ_10()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRTable list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRWhere(e2 => e2.TestEntityJoiningID == 10)
                    .GRToTableAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get joined entities with where clause ID = {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(list.Count == 1, "Returned {0} entities instead of 1.", list.Count);

            foreach (var item in list)
            {
                TestEntityAutoProperties e1 = item.Get<TestEntityAutoProperties>();
                TestEntityJoining e2 = item.Get<TestEntityJoining>();

                Assert.IsTrue(e2.TestEntityJoiningID == 10, "ID was not loaded correctly.");

                Assert.IsTrue(e1.TestEntityAutoPropertiesID != 0, "ID was not loaded.");
                Assert.IsTrue(e1.TestEntityAutoPropertiesOrder != 0, "Order was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e1.Name), "Name was not loaded.");
                Assert.IsTrue(string.IsNullOrEmpty(e1.TestEntityAutoPropertiesDescription), "Description was loaded.");

                Assert.IsTrue(e1.CreatedDate == TestUtils.DefaultCreatedDate, "CreatedDate was not loaded");
                Assert.IsTrue(e1.ModifiedDate == TestUtils.DefaultCreatedDate, "ModifiedDate was not loaded");
                Assert.IsTrue(e1.CreatedBy == -1, "CreatedBy was not loaded");
                Assert.IsTrue(e1.ModifiedBy == -1, "ModifiedBy was not loaded");
                Assert.IsTrue(e1.IsActive, "IsActive was not loaded");

                Assert.IsTrue(e2.TestEntityJoiningID != 0, "TestEntity2ID was not loaded.");
                Assert.IsTrue(e2.TestEntityAutoPropertiesID != 0, "TestEntity1ID was not loaded.");
                Assert.IsTrue(!string.IsNullOrEmpty(e2.Description), "Description was not loaded.");
            }
        }


        [TestMethod]
        public async Task Get_Entities_Join_Entity_With_Nullable_Property()
        {
            // SELECT t.[TestEntityJoiningType3ID] AS t_TestEntityJoiningType3ID, t.[TestEntityJoiningType3Name] AS t_TestEntityJoiningType3Name, t2.[TestEntityJoiningType4ID] AS t2_TestEntityJoiningType4ID, t2.[TestEntityJoiningType3ID] AS t2_TestEntityJoiningType3ID, t2.[TestEntityJoiningType4Value] AS t2_TestEntityJoiningType4Value FROM[TestEntityJoiningType3] AS t LEFT JOIN[TestEntityJoiningType4] as t2 on t.[TestEntityJoiningType3ID] = t2.[TestEntityJoiningType3ID] WHERE t2.[TestEntityJoiningType3ID] = 1
            // RETURNS:
            // 1   Name 1  1   1   NULL

            TestEntityJoiningType3Repository grEntities = TestUtils.GetTestEntityJoiningType3Repository(dbName);
            GRTable table = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoiningType4>(e => e.TestEntityJoiningType3ID, e => e.TestEntityJoiningType3ID)
                    .GRWhere(e => e.TestEntityJoiningType3ID == 1)
                    .GRToTableAsync();

            Assert.IsTrue(table.Count == 1, "Returned {0} entities instead of 1.", table.Count);

            GRTableRow row = table[0];

            TestEntityJoiningType3 e3 = row.Get<TestEntityJoiningType3>();
            TestEntityJoiningType4 e4 = row.Get<TestEntityJoiningType4>();

            Assert.IsTrue(e3 != null, $"TestEntityJoiningType3 was not parsed correctly.");
            Assert.IsTrue(e4 != null, $"TestEntityJoiningType4 was not parsed correctly.");

            Assert.IsTrue(e3.TestEntityJoiningType3ID == 1, "ID of TestEntityJoiningType3 was not loaded correctly.");
            Assert.IsTrue(e3.TestEntityJoiningType3Name == "Name 1", "Name of TestEntityJoiningType3 was not loaded correctly.");

            Assert.IsTrue(e4.TestEntityJoiningType4ID == 1, "ID of TestEntityJoiningType4 was not loaded correctly.");
            Assert.IsTrue(e4.TestEntityJoiningType3ID == 1, "TestEntityJoiningType3ID of TestEntityJoiningType4 was not loaded correctly.");
            Assert.IsTrue(e4.TestEntityJoiningType4Value == null, "Value of TestEntityJoiningType4 was not loaded correctly.");
        }

        [TestMethod]
        public async Task Get_Entities_Join_Nullable_Property_Null()
        {
            // SELECT t.[TestEntityJoiningType3ID] AS t_TestEntityJoiningType3ID, t.[TestEntityJoiningType3Name] AS t_TestEntityJoiningType3Name, t2.[TestEntityJoiningType4Value] AS t2_TestEntityJoiningType4Value, t2.[TestEntityJoiningType4ID] AS t2_TestEntityJoiningType4ID FROM [TestEntityJoiningType3] AS t  LEFT JOIN [TestEntityJoiningType4] as t2 on t.[TestEntityJoiningType3ID] = t2.[TestEntityJoiningType3ID] WHERE t2.[TestEntityJoiningType3ID] = 1
            // RETURNS:
            // 1	Name 1	NULL	1

            TestEntityJoiningType3Repository grEntities = TestUtils.GetTestEntityJoiningType3Repository(dbName);
            GRTable table = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoiningType4>(e => e.TestEntityJoiningType3ID, e => e.TestEntityJoiningType3ID)
                    .GRInclude(e => e.TestEntityJoiningType4Value)
                    .GRWhere(e => e.TestEntityJoiningType3ID == 1)
                    .GRToTableAsync();

            Assert.IsTrue(table.Count == 1, "Returned {0} entities instead of 1.", table.Count);

            GRTableRow row = table[0];

            TestEntityJoiningType3 e3 = row.Get<TestEntityJoiningType3>();
            TestEntityJoiningType4 e4 = row.Get<TestEntityJoiningType4>();

            Assert.IsTrue(e3 != null, $"TestEntityJoiningType3 was not parsed correctly.");
            Assert.IsTrue(e4 != null, $"TestEntityJoiningType4 was not parsed correctly.");

            Assert.IsTrue(e3.TestEntityJoiningType3ID == 1, "ID of TestEntityJoiningType3 was not loaded correctly.");
            Assert.IsTrue(e3.TestEntityJoiningType3Name == "Name 1", "Name of TestEntityJoiningType3 was not loaded correctly.");

            Assert.IsTrue(e4.TestEntityJoiningType4ID == 0, "ID of TestEntityJoiningType4 was loaded but it shouldn't.");
            Assert.IsTrue(e4.TestEntityJoiningType3ID == 0, "TestEntityJoiningType3ID of TestEntityJoiningType4 was loaded but it shouldn't.");
        }

        [TestMethod]
        public async Task Get_Entities_Join_Nullable_Property_Set()
        {
            // SELECT t.[TestEntityJoiningType3ID] AS t_TestEntityJoiningType3ID, t.[TestEntityJoiningType3Name] AS t_TestEntityJoiningType3Name, t2.[TestEntityJoiningType4Value] AS t2_TestEntityJoiningType4Value, t2.[TestEntityJoiningType4ID] AS t2_TestEntityJoiningType4ID FROM [TestEntityJoiningType3] AS t  LEFT JOIN [TestEntityJoiningType4] as t2 on t.[TestEntityJoiningType3ID] = t2.[TestEntityJoiningType3ID] WHERE t2.[TestEntityJoiningType3ID] = 2
            // RETURNS:
            // 2	Name 2	1	2

            TestEntityJoiningType3Repository grEntities = TestUtils.GetTestEntityJoiningType3Repository(dbName);
            GRTable table = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoiningType4>(e => e.TestEntityJoiningType3ID, e => e.TestEntityJoiningType3ID)
                    .GRInclude(e => e.TestEntityJoiningType4Value)
                    .GRWhere(e => e.TestEntityJoiningType3ID == 2)
                    .GRToTableAsync();

            Assert.IsTrue(table.Count == 1, "Returned {0} entities instead of 1.", table.Count);

            GRTableRow row = table[0];

            TestEntityJoiningType3 e3 = row.Get<TestEntityJoiningType3>();
            TestEntityJoiningType4 e4 = row.Get<TestEntityJoiningType4>();

            Assert.IsTrue(e3 != null, $"TestEntityJoiningType3 was not parsed correctly.");
            Assert.IsTrue(e4 != null, $"TestEntityJoiningType4 was not parsed correctly.");

            Assert.IsTrue(e3.TestEntityJoiningType3ID == 2, "ID of TestEntityJoiningType3 was not loaded correctly.");
            Assert.IsTrue(e3.TestEntityJoiningType3Name == "Name 2", "Name of TestEntityJoiningType3 was not loaded correctly.");

            Assert.IsTrue(e4.TestEntityJoiningType4ID == 0, "ID of TestEntityJoiningType4 was loaded but it shouldn't.");
            Assert.IsTrue(e4.TestEntityJoiningType3ID == 0, "TestEntityJoiningType3ID of TestEntityJoiningType4 was loaded but it shouldn't.");

            Assert.IsTrue(e4.TestEntityJoiningType4Value == 1, "Value of TestEntityJoiningType4 was not loaded correctly.");
        }

        [TestMethod]
        public async Task Get_Entities_Join_Non_Existing()
        {
            // SELECT t.[TestEntityJoiningType3ID] AS t_TestEntityJoiningType3ID, t.[TestEntityJoiningType3Name] AS t_TestEntityJoiningType3Name, t2.[TestEntityJoiningType4Value] AS t2_TestEntityJoiningType4Value, t2.[TestEntityJoiningType4ID] AS t2_TestEntityJoiningType4ID FROM [TestEntityJoiningType3] AS t  LEFT JOIN [TestEntityJoiningType4] as t2 on t.[TestEntityJoiningType3ID] = t2.[TestEntityJoiningType3ID] WHERE t.[TestEntityJoiningType3ID] = 3   
            // RETURNS:
            // 3	Name 3	NULL	NULL

            TestEntityJoiningType3Repository grEntities = TestUtils.GetTestEntityJoiningType3Repository(dbName);
            GRTable table = await grEntities
                    .GRWhere(e => e.TestEntityJoiningType3ID == 3)
                    .GRLeftJoin<TestEntityJoiningType4>(e => e.TestEntityJoiningType3ID, e => e.TestEntityJoiningType3ID)
                    .GRInclude(e => e.TestEntityJoiningType4Value)
                    .GRToTableAsync();

            Assert.IsTrue(table.Count == 1, "Returned {0} entities instead of 1.", table.Count);

            GRTableRow row = table[0];

            TestEntityJoiningType3 e3 = row.Get<TestEntityJoiningType3>();
            TestEntityJoiningType4 e4 = row.Get<TestEntityJoiningType4>();

            Assert.IsTrue(e3 != null, $"TestEntityJoiningType3 was not parsed correctly.");
            Assert.IsTrue(e4 == null, $"TestEntityJoiningType4 should be null.");

            Assert.IsTrue(e3.TestEntityJoiningType3ID == 3, "ID of TestEntityJoiningType3 was not loaded correctly.");
            Assert.IsTrue(e3.TestEntityJoiningType3Name == "Name 3", "Name of TestEntityJoiningType3 was not loaded correctly.");
        }
    }
}
