
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "RoomsFunction")]
public class RoomsFunction
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "FunctionId")]
	public int Functionid { get; set;}

[GRColumnName(ColumnName = "RoomId")]
	public int Roomid { get; set;}
}

}
