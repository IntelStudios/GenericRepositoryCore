using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityPrimitiveNullRepository : GRRepository<TestEntityPrimitiveNull>, IGRRepository<TestEntityPrimitiveNull>
    {
        public TestEntityPrimitiveNullRepository(IGRContext context) : base(context)
        {
        }
    }
}
