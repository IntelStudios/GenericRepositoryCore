
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetUserClaimsRepository : GRRepository<AspNetUserClaims>, IGRRepository<AspNetUserClaims>
{
public AspNetUserClaimsRepository(IGRContext context) : base(context)
{
}
}
}
