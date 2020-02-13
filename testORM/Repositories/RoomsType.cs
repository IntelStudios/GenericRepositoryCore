
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class RoomsTypeRepository : GRRepository<RoomsType>, IGRRepository<RoomsType>
{
public RoomsTypeRepository(IGRContext context) : base(context)
{
}
}
}
