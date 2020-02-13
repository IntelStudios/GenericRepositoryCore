
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "RoomsType")]
public class RoomsType
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "TypeName")]
	public string Typename { get; set;}

[GRColumnName(ColumnName = "TypeDesc")]
	public string Typedesc { get; set;}
}

}
