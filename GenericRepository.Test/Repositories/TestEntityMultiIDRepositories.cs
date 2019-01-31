using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityMultiID_1Repository : GRRepository<TestEntityMultiID_1>, IGRRepository<TestEntityMultiID_1>
    {
        public TestEntityMultiID_1Repository(IGRContext context) : base(context)
        {

        }
    }

    public class TestEntityMultiID_2Repository : GRRepository<TestEntityMultiID_2>, IGRRepository<TestEntityMultiID_2>
    {
        public TestEntityMultiID_2Repository(IGRContext context) : base(context)
        {

        }
    }

    public class TestEntityMultiID_3Repository : GRRepository<TestEntityMultiID_3>, IGRRepository<TestEntityMultiID_3>
    {
        public TestEntityMultiID_3Repository(IGRContext context) : base(context)
        {

        }
    }
}
