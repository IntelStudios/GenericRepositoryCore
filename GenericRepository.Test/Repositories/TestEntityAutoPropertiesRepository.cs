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
    public class TestEntityAutoPropertiesRepository : GRRepository<TestEntityAutoProperties>, IGRRepository<TestEntityAutoProperties>
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
        
        public TestEntityAutoPropertiesRepository(IGRContext context) : base(context)
        {
            
        }

        public async Task<TestEntityJoining> CrossGRGetAsync(int id)
        {
            return await GRGetAsync<TestEntityJoining>(id);
        }

        public async Task UpdateAsync(TestEntityJoining entity)
        {
            await GRUpdate<TestEntityJoining>(entity).GRExecuteAsync();
        }

        public async Task<List<TestEntityJoining>> CrossWhere()
        {
            return await GRWhere<TestEntityJoining>(e => e.TestEntityJoiningID > 50).GRToListAsync();
        }
    }
}
