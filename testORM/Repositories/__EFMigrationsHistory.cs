
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class __EFMigrationsHistoryRepository : GRRepository<__EFMigrationsHistory>, IGRRepository<__EFMigrationsHistory>
{
public __EFMigrationsHistoryRepository(IGRContext context) : base(context)
{
}
}
}
