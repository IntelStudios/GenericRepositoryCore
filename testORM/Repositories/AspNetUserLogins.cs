
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetUserLoginsRepository : GRRepository<AspNetUserLogins>, IGRRepository<AspNetUserLogins>
{
public AspNetUserLoginsRepository(IGRContext context) : base(context)
{
}
}
}
