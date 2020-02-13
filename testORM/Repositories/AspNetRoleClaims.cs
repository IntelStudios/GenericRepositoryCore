
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetRoleClaimsRepository : GRRepository<AspNetRoleClaims>, IGRRepository<AspNetRoleClaims>
{
public AspNetRoleClaimsRepository(IGRContext context) : base(context)
{
}
}
}
