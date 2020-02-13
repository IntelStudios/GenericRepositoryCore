
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetUsersRepository : GRRepository<AspNetUsers>, IGRRepository<AspNetUsers>
{
public AspNetUsersRepository(IGRContext context) : base(context)
{
}
}
}
