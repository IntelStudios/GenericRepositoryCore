
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class sysdiagramsRepository : GRRepository<sysdiagrams>, IGRRepository<sysdiagrams>
{
public sysdiagramsRepository(IGRContext context) : base(context)
{
}
}
}
