
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class RoomsFunctionRepository : GRRepository<RoomsFunction>, IGRRepository<RoomsFunction>
{
public RoomsFunctionRepository(IGRContext context) : base(context)
{
}
}
}
