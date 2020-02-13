using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using TestTTframework.Models;

namespace TestTTframework.Repositories
{
    class UserRepository : GRRepository<User>, IGRRepository<User>
    {
        public UserRepository(IGRContext context) : base(context)
        {

        }

    }
}
