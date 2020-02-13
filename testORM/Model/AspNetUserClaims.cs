
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetUserClaims")]
public class AspNetUserClaims
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "UserId")]
	public string Userid { get; set;}

[GRColumnName(ColumnName = "ClaimType")]
	public string Claimtype { get; set;}

[GRColumnName(ColumnName = "ClaimValue")]
	public string Claimvalue { get; set;}
}

}
