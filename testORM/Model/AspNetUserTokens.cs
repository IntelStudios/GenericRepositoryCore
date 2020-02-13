
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetUserTokens")]
public class AspNetUserTokens
{

[GRColumnName(ColumnName = "UserId")]
	public string Userid { get; set;}

[GRColumnName(ColumnName = "LoginProvider")]
	public string Loginprovider { get; set;}

[GRColumnName(ColumnName = "Name")]
	public string Name { get; set;}

[GRColumnName(ColumnName = "Value")]
	public string Value { get; set;}
}

}
