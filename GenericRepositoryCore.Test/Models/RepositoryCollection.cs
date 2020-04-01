using GenericRepository.Interfaces;
using GenericRepository.Test.Repositories;

namespace GenericRepository.Test.Models
{
    public class RepositoryCollection
    {
        public TestEntityAutoPropertiesRepository TestEntityAutoPropertiesRepository { get; set; }
        public TestEntityBinaryStreamRepository TestEntityBinaryStreamRepository { get; set; }
        public TestEntityBinaryArrayRepository TestEntityBinaryArrayRepository { get; set; }
        public TestEntityPKRepository TestEntityPKRepository { get; set; }
        public TestEntityAIPKRepository TestEntityAIPKRepository { get; set; }
        public TestEntityPKsRepository TestEntityPKsRepository { get; set; }
        public TestEntityPrimitiveNullRepository TestEntityPrimitiveNullRepository { get; set; }
        public IGRContext Context { get; set; }
    }
}
