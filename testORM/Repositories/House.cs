
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using System.Collections.Generic;
using System.Text;
using testORM.Model;

namespace testORM.Repositories
{

public class HouseRepository : GRRepository<House>, IGRRepository<House>
{
public HouseRepository(IGRContext context) : base(context)
{
}
}
}
