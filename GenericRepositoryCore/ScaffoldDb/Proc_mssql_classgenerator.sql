

CREATE FUNCTION Initcap (@Str varchar(max))
RETURNS varchar(max) AS
BEGIN
  DECLARE @Result varchar(max)
  SET @Str = LOWER(@Str) + ' '
  SET @Result = ''
  WHILE 1=1
  BEGIN
    IF PATINDEX('% %',@Str) = 0 BREAK
    SET @Result = @Result + UPPER(Left(@Str,1))+
    SubString  (@Str,2,CharIndex(' ',@Str)-1)
    SET @Str = SubString(@Str,
      CharIndex(' ',@Str)+1,Len(@Str))
  END
  SET @Result = Left(@Result,Len(@Result))
  RETURN @Result
END 

GO

create function ConvertDataType(@typeId int, @isNullable bit)
returns varchar(max)
as
BEGIN
	DECLARE @typeName varchar(max);
	SET @typeName =
	CASE @typeId
		WHEN 56 THEN 'int'
		WHEN 40 THEN 'DateTime'
		WHEN 61 THEN 'DateTime'
		WHEN 104 THEN 'bool'
		WHEN 106 THEN 'decimal'
		WHEN 231 THEN 'string'
		WHEN 167 THEN 'string'
		WHEN 239 THEN 'string'
		WHEN 241 THEN 'XElement'
		ELSE 'unknown (' + CAST(@typeId AS NVARCHAR) + ')'
	END;
	IF @isNullable = 1 AND @typeId != 231 AND @typeId != 239 AND @typeId != 241 AND @typeId != 167
		SET @typeName = @typeName + '?'
	return @typeName + ' ';
end

GO

Create or alter function AddClassHeader(@p_tableName varchar(100))
returns varchar(max)
as
BEGIN
	Declare @tmp varchar(max)= '';
	DECLARE @br char(1) = CHAR(10);
	set @tmp = @tmp + @br + 'using GenericRepository.Attributes;';
		set @tmp = @tmp + @br + 'using System;';
		set @tmp = @tmp + @br + 'using System.Collections.Generic;';
		set @tmp = @tmp + @br + 'using System.Text;';
		set @tmp = @tmp + @br;
		set @tmp = @tmp + @br + '%namespace%';
		set @tmp = @tmp + @br + '{';
		set @tmp = @tmp + @br;
		set @tmp = @tmp + @br + '[GRTableName(TableName = "' + @p_tableName + '")]';
		set @tmp = @tmp + @br + 'public class ' + @p_tableName;
        set @tmp = @tmp + @br + '{';
	return @tmp;
END
GO
Create or alter function AddClassRepositories(@p_tableName varchar(max))
returns varchar(max)
as
BEGIN
	Declare @tmp varchar(max)= '';
	DECLARE @br char(1) = CHAR(10);
	set @tmp = @tmp + @br + 'using GenericRepository.Interfaces;';
		set @tmp = @tmp + @br + 'using GenericRepository.Repositories;';
		set @tmp = @tmp + @br + 'using System.Collections.Generic;';
		set @tmp = @tmp + @br + 'using System.Text;';
		set @tmp = @tmp + @br + '%usingToModel%';
		set @tmp = @tmp + @br;
		
		set @tmp = @tmp + @br + '%namespace%';
		set @tmp = @tmp + @br + '{';
		set @tmp = @tmp + @br;
		set @tmp = @tmp + @br + 'public class ' + @p_tableName + 'Repository' + ' : GRRepository<' + @p_tableName + '>, IGRRepository<'+ @p_tableName +'>';
        set @tmp = @tmp + @br + '{';

		set @tmp = @tmp + @br + 'public ' + @p_tableName + 'Repository(IGRContext context) : base(context)';
		set @tmp = @tmp + @br + '{';
		set @tmp = @tmp + @br + '}';
		set @tmp = @tmp + @br + '}';
		set @tmp = @tmp + @br + '}';
		set @tmp = @tmp + @br;
		
	return @tmp;
END

GO

Create or alter function IsPrimaryKey(@p_tableWithAttribute nvarchar(max))
returns bit
as
BEGIN
	Declare @isKey bit = 0;
	Declare @tmpPK nvarchar(max);

select 
	@tmpPK = Concat(t.name,'.', tc.name) 
	from 
		sys.schemas s 
		inner join sys.tables t   on s.schema_id=t.schema_id
		inner join sys.indexes i  on t.object_id=i.object_id
		inner join sys.index_columns ic on i.object_id=ic.object_id 
										and i.index_id=ic.index_id
		inner join sys.columns tc on ic.object_id=tc.object_id 
									and ic.column_id=tc.column_id
	where i.is_primary_key=1 and  tc.is_identity = 1
	and Concat(t.name,'.', tc.name) = @p_tableWithAttribute
	order by t.name, ic.key_ordinal;

	if(@tmpPK is not null)
		set @IsKey = 1;
		
	return @IsKey
END

GO
create or alter  procedure PrintDtoForDatabase(
	@p_DbName nvarchar(max)
	)
as
	DECLARE c_tables CURSOR LOCAL FOR SELECT table_name FROM information_schema.tables WHERE table_catalog=@p_DbName;
	Declare @counter int = -1;
	DECLARE @name varchar(1000);
	DECLARE @out nvarchar(max) = '';
	Declare @repo nvarchar(max) = '';
	DECLARE @br char(1) = CHAR(10)
	DECLARE @tab char(1) = CHAR(9)

	DECLARE @a_name varchar(1000);
	DECLARE @a_type int;
	DECLARE @a_nullable bit;
	
	Declare @OutputTable Table(
	TableName varchar(200),
	Class nvarchar(max),
	Repository nvarchar(max)
	);

begin
	open c_tables
	FETCH NEXT FROM c_tables INTO @name
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		if(@name != '') begin
			set @out = @out + dbo.AddClassHeader(@name);
			set @repo = @repo + dbo.AddClassRepositories(@name);
			end

		DECLARE c_attr CURSOR LOCAL FOR SELECT cols.name, cols.system_type_id, cols.is_nullable FROM sys.columns cols JOIN sys.tables tbl ON cols.object_id = tbl.object_id where tbl.name=@name;
		open c_attr
		FETCH NEXT FROM c_attr INTO @a_name,@a_type,@a_nullable
		WHILE @@FETCH_STATUS = 0
		BEGIN
			
			if(dbo.IsPrimaryKey(Concat(@name, '.',  @a_name)) = 1)
				set @out = @out + @br + '[GRAIPrimaryKey]';
			set @out = @out + @br;
			set @out = @out + @br + '[GRColumnName(ColumnName = "' + @a_name + '")]';
			set @out = @out + @br + @tab + 'public ' + dbo.ConvertDataType(@a_type,@a_nullable) + dbo.Initcap(@a_name) + ' { get; set;}';
			FETCH NEXT FROM c_attr INTO @a_name,@a_type,@a_nullable

		end
		close c_attr
		deallocate c_attr
		set @out = @out + @br + '}' + @br;
		set @out = @out + @br + '}' + @br;
		insert into @OutputTable values (@name, @out, @repo);
		set @repo = '';
		set @out = '';--@out + '#endclass' + @br;
		FETCH NEXT FROM c_tables INTO @name

	END
	close c_tables;
	Select * from @OutputTable;
	--print @out;
	--print @repo;
end



