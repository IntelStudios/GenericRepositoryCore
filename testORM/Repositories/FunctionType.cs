
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class FunctionTypeRepository : GRRepository<FunctionType>, IGRRepository<FunctionType>
{
public FunctionTypeRepository(IGRContext context) : base(context)
{
}
}
}
