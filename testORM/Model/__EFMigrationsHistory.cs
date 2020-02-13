
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "__EFMigrationsHistory")]
public class __EFMigrationsHistory
{

[GRColumnName(ColumnName = "MigrationId")]
	public string Migrationid { get; set;}

[GRColumnName(ColumnName = "ProductVersion")]
	public string Productversion { get; set;}
}

}
