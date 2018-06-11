using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityBinaryArrayRepository : GRRepository<TestEntityBinaryArray>, IGRRepository<TestEntityBinaryArray>
    {
        public TestEntityBinaryArrayRepository(IGRContext context) : base(context)
        {
        }
    }
}
