using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using TestTTframework.Models;

namespace TestTTframework.Repositories
{
    class RoleRepository: GRRepository<Role>, IGRRepository<Role>
    {
        public RoleRepository(IGRContext context): base(context) 
        {

        }
    }
}
