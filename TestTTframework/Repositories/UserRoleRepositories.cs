using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using TestTTframework.Models;

namespace TestTTframework.Repositories
{
    class UserRoleRepositories : GRRepository<UserRole>, IGRRepository<UserRole>
    {

        public UserRoleRepositories(IGRContext context) : base(context)
        {
        }

    }
}
