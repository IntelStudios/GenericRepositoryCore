
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetRoleClaims")]
public class AspNetRoleClaims
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "RoleId")]
	public string Roleid { get; set;}

[GRColumnName(ColumnName = "ClaimType")]
	public string Claimtype { get; set;}

[GRColumnName(ColumnName = "ClaimValue")]
	public string Claimvalue { get; set;}
}

}
