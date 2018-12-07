using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityJoiningType1Repository : GRRepository<TestEntityJoiningType1>, IGRRepository<TestEntityJoiningType1>
    {
        public TestEntityJoiningType1Repository(IGRContext context) : base(context)
        {
        }
    }

    public class TestEntityJoiningType2Repository : GRRepository<TestEntityJoiningType2>, IGRRepository<TestEntityJoiningType2>
    {
        public TestEntityJoiningType2Repository(IGRContext context) : base(context)
        {
        }
    }

    public class TestEntityJoiningType3Repository : GRRepository<TestEntityJoiningType3>, IGRRepository<TestEntityJoiningType3>
    {
        public TestEntityJoiningType3Repository(IGRContext context) : base(context)
        {
        }
    }

    public class TestEntityJoiningType4Repository : GRRepository<TestEntityJoiningType4>, IGRRepository<TestEntityJoiningType4>
    {
        public TestEntityJoiningType4Repository(IGRContext context) : base(context)
        {
        }
    }
}
