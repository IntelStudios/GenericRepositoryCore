
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetUserLogins")]
public class AspNetUserLogins
{

[GRColumnName(ColumnName = "LoginProvider")]
	public string Loginprovider { get; set;}

[GRColumnName(ColumnName = "ProviderKey")]
	public string Providerkey { get; set;}

[GRColumnName(ColumnName = "ProviderDisplayName")]
	public string Providerdisplayname { get; set;}

[GRColumnName(ColumnName = "UserId")]
	public string Userid { get; set;}
}

}
