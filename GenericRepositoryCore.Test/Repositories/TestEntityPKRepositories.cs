using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;
using System;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityPKRepository : GRRepository<TestEntityPK>, IGRRepository<TestEntityPK>
    {
        public DateTime ServerTime
        {
            get
            {
                return new DateTime(2018, 2, 1); ;
            }
        }

        public int UserID
        {
            get
            {
                return 999;
            }
        }

        public TestEntityPKRepository(IGRContext context) : base(context)
        {
        }
    }

    public class TestEntityAIPKRepository : GRRepository<TestEntityAIPK>, IGRRepository<TestEntityAIPK>
    {
        public DateTime ServerTime
        {
            get
            {
                return new DateTime(2018, 2, 1); ;
            }
        }

        public int UserID
        {
            get
            {
                return 999;
            }
        }

        public TestEntityAIPKRepository(IGRContext context) : base(context)
        {
        }
    }

    public class TestEntityPKsRepository : GRRepository<TestEntityPKs>, IGRRepository<TestEntityPKs>
    {
        public DateTime ServerTime
        {
            get
            {
                return new DateTime(2018, 2, 1); ;
            }
        }

        public int UserID
        {
            get
            {
                return 999;
            }
        }

        public TestEntityPKsRepository(IGRContext context) : base(context)
        {
        }
    }
}
