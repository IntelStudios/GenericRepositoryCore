
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetRoles")]
public class AspNetRoles
{

[GRColumnName(ColumnName = "Id")]
	public string Id { get; set;}

[GRColumnName(ColumnName = "Name")]
	public string Name { get; set;}

[GRColumnName(ColumnName = "NormalizedName")]
	public string Normalizedname { get; set;}

[GRColumnName(ColumnName = "ConcurrencyStamp")]
	public string Concurrencystamp { get; set;}
}

}
