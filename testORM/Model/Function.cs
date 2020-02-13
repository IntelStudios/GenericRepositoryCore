
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "Function")]
public class Function
{
[GRAIPrimaryKey]

[GRColumnName(ColumnName = "Id")]
	public int Id { get; set;}

[GRColumnName(ColumnName = "IdHouse")]
	public int Idhouse { get; set;}

[GRColumnName(ColumnName = "IdFunctionType")]
	public int Idfunctiontype { get; set;}

[GRColumnName(ColumnName = "ReadPlcValueName")]
	public string Readplcvaluename { get; set;}

[GRColumnName(ColumnName = "WritePlcValueName")]
	public string Writeplcvaluename { get; set;}

[GRColumnName(ColumnName = "IsReadPlcValue")]
	public bool? Isreadplcvalue { get; set;}

[GRColumnName(ColumnName = "IsWritePlcValue")]
	public bool? Iswriteplcvalue { get; set;}
}

}
