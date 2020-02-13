
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "Room")]
public class Room
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "RoomId")]
	public int Roomid { get; set;}

[GRColumnName(ColumnName = "HouseId")]
	public int Houseid { get; set;}

[GRColumnName(ColumnName = "Name")]
	public string Name { get; set;}

[GRColumnName(ColumnName = "Description")]
	public string Description { get; set;}

[GRColumnName(ColumnName = "RoomsTypeId")]
	public int Roomstypeid { get; set;}
}

}
