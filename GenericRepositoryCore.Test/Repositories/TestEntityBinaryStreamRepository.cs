using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;

namespace GenericRepository.Test.Repositories
{
    public class TestEntityBinaryStreamRepository : GRRepository<TestEntityBinaryStream>, IGRRepository<TestEntityBinaryStream>
    {
        public TestEntityBinaryStreamRepository(IGRContext context) : base(context)
        {
        }
    }
}
