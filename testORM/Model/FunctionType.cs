
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "FunctionType")]
public class FunctionType
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "Name")]
	public string Name { get; set;}

[GRColumnName(ColumnName = "Description")]
	public string Description { get; set;}
}

}
