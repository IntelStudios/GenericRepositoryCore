
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class AspNetUserTokensRepository : GRRepository<AspNetUserTokens>, IGRRepository<AspNetUserTokens>
{
public AspNetUserTokensRepository(IGRContext context) : base(context)
{
}
}
}
