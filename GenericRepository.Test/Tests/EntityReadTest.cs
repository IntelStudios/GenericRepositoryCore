using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericRepository.Test.Models;
using GenericRepository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using GenericRepository.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GenericRepository.Helpers;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityReadTest
    {
        static string dbBaseName = "xeelo-tests-gr-read";
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
        public async Task Get_Entities_All()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await grEntities.GRAll().GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get all entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.TestEntityAutoPropertiesDescription == null, "Entity description was loaded, it should be ignored!");

                Assert.IsTrue(item.Name == string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    item.Name
                    );

                Assert.IsTrue(item.TestEntityAutoPropertiesOrder == (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    item.TestEntityAutoPropertiesOrder
                    );

                Assert.IsTrue(item.CreatedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity CreatedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.CreatedDate);

                Assert.IsTrue(item.ModifiedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity ModifiedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.ModifiedDate);

                Assert.IsTrue(item.ModifiedBy == -1,
                    "Entity name was loaded incorrectly! Entity ModifiedBy should be '{0}' instead of '{1}'.",
                    -1, item.ModifiedBy);

                Assert.IsTrue(item.CreatedBy == -1,
                    "Entity name was loaded incorrectly! Entity CreatedBy should be '{0}' instead of '{1}'.",
                    -1, item.CreatedBy);

                Assert.IsTrue(item.IsActive,
                    "Entity name was loaded incorrectly! Entity IsActive should be TRUE instead of FALSE.");
            }

            List<int> allIds = testEntities.Select(e => e.TestEntityAutoPropertiesID).ToList();
            List<string> allNames = testEntities.Select(e => e.Name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_Entities_Id_GT_50()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await grEntities.GRWhere(e => e.TestEntityAutoPropertiesID > 50).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            int expectedCount = 50;

            Assert.IsTrue(testEntities.Count == expectedCount, "Returned {0} entities instead of {1}.", testEntities.Count, expectedCount);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 50, "Wrong entity ID = {0} was returned!", item.TestEntityAutoPropertiesID);
            }
        }

        [TestMethod]
        public void Get_Entities_Id_GT_50_Count()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            int count = 0;

            try
            {
                count = grEntities.GRWhere(e => e.TestEntityAutoPropertiesID > 50).GRCountAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get count of entitites ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            int expectedCount = 50;

            Assert.IsTrue(count == expectedCount, "Returned count of {0} entities instead of {1}.", count, expectedCount);
        }

        [TestMethod]
        public void Get_Entities_All_Count()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            int count = 0;

            try
            {
                count = grEntities.GRCount();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get count of all entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }


            Assert.IsTrue(count == TestUtils.TestCollectionSize, "Returned count of {0} entities instead of {1}.", count, TestUtils.TestCollectionSize);
        }

        [TestMethod]
        public async Task Get_Entities_Id_GT_10_LT_15()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await grEntities.GRWhere(e => e.TestEntityAutoPropertiesID > 10 && e.TestEntityAutoPropertiesID < 15).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID > 10 && ID < 15 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID <= 10 || item.TestEntityAutoPropertiesID >= 10, "Wrong entity with ID = {0} was returned!", item.TestEntityAutoPropertiesID);
            }
        }

        [TestMethod]
        public async Task Get_Entities_Id_EQ_10()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await grEntities.GRWhere(e => e.TestEntityAutoPropertiesID == 10).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID == 10 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == 1, "Exactly single instance should be returned instead of {0} entities.", testEntities.Count);

            Assert.IsTrue(testEntities.Single().TestEntityAutoPropertiesID == 10, "Entity with ID = 10 should be returned instead of entitity with ID = {0}", testEntities.Single().TestEntityAutoPropertiesID);
        }

        [TestMethod]
        public void Get_Entities_Id_GT_10_FirstOrDefault()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = grEntities.GRWhere(e => e.TestEntityAutoPropertiesID > 10).GRFirstOrDefaultAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get first entitity ID > 10 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID > 10, "Entity with ID > 10 should be returned instead of entitity with ID = {0}", testEntity.TestEntityAutoPropertiesID);
        }

        [TestMethod]
        public void Get_Entities_Name_Include()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = grEntities
                    .GRWhere(e => e.TestEntityAutoPropertiesID == 10)
                    .GRInclude(e => e.Name)
                    .GRFirstOrDefaultAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with included property - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.Name != string.Empty, "Entity name should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID == 0, "Entity ID should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesOrder == 0, "Entity order should not be loaded");

            Assert.IsTrue(testEntity.CreatedDate == new DateTime(), "Entity CreatedDate should not be loaded");
            Assert.IsTrue(testEntity.ModifiedDate == new DateTime(), "Entity ModifiedDate should not be loaded");
            Assert.IsTrue(testEntity.CreatedBy == 0, "Entity CreatedBy should not be loaded");
            Assert.IsTrue(testEntity.ModifiedBy == 0, "Entity ModifiedBy should not be loaded");
            Assert.IsTrue(!testEntity.IsActive, "Entity IsActive should not be loaded");
        }

        [TestMethod]
        public void Get_Entities_Name_Excluded()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = grEntities
                    .GRWhere(e => e.TestEntityAutoPropertiesID == 10)
                    .GRExclude(e => e.Name)
                    .GRFirstOrDefaultAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with excluded property - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(string.IsNullOrEmpty(testEntity.Name), "Entity name should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID != 0, "Entity ID should be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesOrder != 0, "Entity order should be loaded");
            Assert.IsTrue(string.IsNullOrEmpty(testEntity.TestEntityAutoPropertiesDescription), "Entity description (GR ignored) should not be loaded");

            Assert.IsTrue(testEntity.CreatedDate == TestUtils.DefaultCreatedDate, "Entity CreatedDate should be loaded");
            Assert.IsTrue(testEntity.ModifiedDate == TestUtils.DefaultCreatedDate, "Entity ModifiedDate should be loaded");
            Assert.IsTrue(testEntity.CreatedBy == -1, "Entity CreatedBy should be loaded");
            Assert.IsTrue(testEntity.ModifiedBy == -1, "Entity ModifiedBy should be loaded");
            Assert.IsTrue(testEntity.IsActive, "Entity IsActive should be loaded");
        }

        [TestMethod]
        public async Task Get_Entities_Name_Ordered()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            List<TestEntityAutoProperties> entities = null;

            try
            {
                entities = await grEntities
                    .GROrderBy(e => e.Name)
                    .GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get ordered entities - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", entities.Count, TestUtils.TestCollectionSize);

            for (int i = 1; i < entities.Count; i++)
            {
                Assert.IsTrue(String.Compare(entities[i].Name, entities[i - 1].Name) != 2, "Wrong sort order");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Name_Ordered_Desc()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityAutoProperties> entities = null;

            try
            {
                entities = await grEntities
                    .GROrderByDescending(e => e.Name)
                    .GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get ordered (descending) entities - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", entities.Count, TestUtils.TestCollectionSize);

            for (int i = 1; i < entities.Count; i++)
            {
                Assert.IsTrue(String.Compare(entities[i].Name, entities[i - 1].Name) != 0, "Wrong sort order");
            }
        }

        [TestMethod]
        public async Task Get_Entities_Join()
        {
            IGRRepository<TestEntityAutoProperties> grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRExcludeAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRExcludeAll()
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRExclude(e1 => e1.Name)
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRExclude(e2 => e2.Description)
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRWhere(e1 => e1.TestEntityAutoPropertiesID == 10)
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRToJoinedListAsync();
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
            GRJoinedList list = null;

            try
            {
                list = await grEntities
                    .GRAll()
                    .GRLeftJoin<TestEntityJoining>(e1 => e1.TestEntityAutoPropertiesID, e2 => e2.TestEntityAutoPropertiesID)
                    .GRWhere(e2 => e2.TestEntityJoiningID == 10)
                    .GRToJoinedListAsync();
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
        public void Get_Entities_Binary_Stream()
        {
            IGRRepository<TestEntityBinaryStream> grEntities = TestUtils.GetTestEntityBinaryStreamRepository(dbName);
            TestEntityBinaryStream entity = null;

            try
            {
                entity = grEntities.GRGet(1);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityBinaryData != null, "Entity data stream was not loaded.");

            byte[] data = ConvertToByteArray(entity.TestEntityBinaryData);

            string value = Encoding.UTF8.GetString(data);

            Assert.IsTrue(value == TestUtils.TestSPVarbinaryData, "Binary data was corrupted.");
        }

        [TestMethod]
        public void Get_Entities_Binary_Array_Stream()
        {
            IGRRepository<TestEntityBinaryArray> grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = grEntities.GRGet(1);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityBinaryData != null, "Entity data stream was not loaded.");

            byte[] data = entity.TestEntityBinaryData;

            string value = Encoding.UTF8.GetString(data);

            Assert.IsTrue(value == TestUtils.TestSPVarbinaryData, "Binary data was corrupted.");
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Stream_Null_Condition()
        {
            IGRRepository<TestEntityBinaryStream> grEntities = TestUtils.GetTestEntityBinaryStreamRepository(dbName);
            TestEntityBinaryStream entity = null;

            try
            {
                entity = await grEntities.GRWhere(e => e.TestEntityBinaryData == null).GRFirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Array_Null_Condition()
        {
            IGRRepository<TestEntityBinaryArray> grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = await grEntities.GRWhere(e => e.TestEntityBinaryData == null).GRFirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Array_Only_With_Condition()
        {
            IGRRepository<TestEntityBinaryArray> grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = await grEntities.GRInclude(e => e.TestEntityBinaryData).GRWhere(e => e.TestEntityBinaryID == 1).GRFirstOrDefaultAsync();
                Assert.IsTrue(entity.TestEntityBinaryData != null, "Binary data was not loaded.");

            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Array_Only()
        {
            IGRRepository<TestEntityBinaryArray> grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = await grEntities.GRInclude(e => e.TestEntityBinaryData).GRFirstOrDefaultAsync();
                Assert.IsTrue(entity.TestEntityBinaryData != null, "Binary data was not loaded.");

            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }


        public static byte[] ConvertToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
