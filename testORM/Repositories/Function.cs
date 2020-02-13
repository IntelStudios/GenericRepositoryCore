
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class FunctionRepository : GRRepository<Function>, IGRRepository<Function>
{
public FunctionRepository(IGRContext context) : base(context)
{
}
}
}
