
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "sysdiagrams")]
public class sysdiagrams
{

[GRColumnName(ColumnName = "name")]
	public string Name { get; set;}

[GRColumnName(ColumnName = "principal_id")]
	public int Principal_id { get; set;}
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "diagram_id")]
	public int Diagram_id { get; set;}

[GRColumnName(ColumnName = "version")]
	public int? Version { get; set;}

[GRColumnName(ColumnName = "definition")]
	public unknown (165)? Definition { get; set;}
}

}
