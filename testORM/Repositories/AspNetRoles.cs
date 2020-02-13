
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetRolesRepository : GRRepository<AspNetRoles>, IGRRepository<AspNetRoles>
{
public AspNetRolesRepository(IGRContext context) : base(context)
{
}
}
}
