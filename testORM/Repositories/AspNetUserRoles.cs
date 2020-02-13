
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetUserRolesRepository : GRRepository<AspNetUserRoles>, IGRRepository<AspNetUserRoles>
{
public AspNetUserRolesRepository(IGRContext context) : base(context)
{
}
}
}
