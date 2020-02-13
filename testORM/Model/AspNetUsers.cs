
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetUsers")]
public class AspNetUsers
{

[GRColumnName(ColumnName = "Id")]
	public string Id { get; set;}

[GRColumnName(ColumnName = "UserName")]
	public string Username { get; set;}

[GRColumnName(ColumnName = "NormalizedUserName")]
	public string Normalizedusername { get; set;}

[GRColumnName(ColumnName = "Email")]
	public string Email { get; set;}

[GRColumnName(ColumnName = "NormalizedEmail")]
	public string Normalizedemail { get; set;}

[GRColumnName(ColumnName = "EmailConfirmed")]
	public bool Emailconfirmed { get; set;}

[GRColumnName(ColumnName = "PasswordHash")]
	public string Passwordhash { get; set;}

[GRColumnName(ColumnName = "SecurityStamp")]
	public string Securitystamp { get; set;}

[GRColumnName(ColumnName = "ConcurrencyStamp")]
	public string Concurrencystamp { get; set;}

[GRColumnName(ColumnName = "PhoneNumber")]
	public string Phonenumber { get; set;}

[GRColumnName(ColumnName = "PhoneNumberConfirmed")]
	public bool Phonenumberconfirmed { get; set;}

[GRColumnName(ColumnName = "TwoFactorEnabled")]
	public bool Twofactorenabled { get; set;}

[GRColumnName(ColumnName = "LockoutEnd")]
	public unknown (43)? Lockoutend { get; set;}

[GRColumnName(ColumnName = "LockoutEnabled")]
	public bool Lockoutenabled { get; set;}

[GRColumnName(ColumnName = "AccessFailedCount")]
	public int Accessfailedcount { get; set;}

[GRColumnName(ColumnName = "HouseId")]
	public int? Houseid { get; set;}
}

}
