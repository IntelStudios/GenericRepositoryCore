
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class RoomRepository : GRRepository<Room>, IGRRepository<Room>
{
public RoomRepository(IGRContext context) : base(context)
{
}
}
}
