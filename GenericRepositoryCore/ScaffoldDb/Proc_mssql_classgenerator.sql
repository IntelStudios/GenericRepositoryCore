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
		WHEN 48 THEN 'int'		--tinyint
		WHEN 165 THEN 'byte[]'	--varbinary(max)
		WHEN 108 THEN 'decimal' --numeric(38,20) GPS
		WHEN 175 THEN 'string'	--char(3)
		WHEN 127 THEN 'long'	--bigint
		WHEN 99 THEN 'string'	--ntext
		WHEN 56 THEN 'int'		--int
		WHEN 40 THEN 'DateTime' --date
		WHEN 61 THEN 'DateTime' --datetime
		WHEN 104 THEN 'bool'	--bit
		WHEN 231 THEN 'string'	--varchar
		WHEN 167 THEN 'string'	--varchar
		WHEN 59 THEN 'float'	--real
		WHEN 41 THEN 'TimeSpan' --time(7)
		WHEN 35 THEN 'string'	--ntext
		--WHEN 106 THEN 'decimal'
		--WHEN 239 THEN 'string'
		WHEN 241 THEN 'XElement'
		ELSE 'unknown (' + CAST(@typeId AS NVARCHAR) + ')'
	END;
	IF @isNullable = 1 AND @typeId != 231 AND @typeId != 239 AND @typeId != 241 AND @typeId != 167 AND @typeId != 99 AND @typeId != 165 AND @typeId != 99 AND @typeId != 175 AND @typeId != 35
		SET @typeName = @typeName + '?'
	return @typeName + ' ';
end

GO

IF object_id(N'AddClassHeader', N'P') IS NOT NULL
    DROP procedure AddClassHeader
GO

Create procedure AddClassHeader(@p_tableName varchar(100), @a_out varchar(max) output)
as
BEGIN
	Declare @tmp varchar(max)= '';

	DECLARE @tableComment varchar(30);
	SELECT @tableComment = convert(varchar(5000), sep.value) 
	FROM 
		sys.schemas s 
		inner join sys.tables t on s.schema_id=t.schema_id
		left join sys.extended_properties sep ON 
			t.object_id = sep.major_id
			and sep.name = 'MS_Description'
			and sep.minor_id = 0
	where t.name=@p_tableName and s.name = 'dbo'
	DECLARE @a_desc varchar(5000);

	DECLARE @br char(1) = CHAR(10);
	DECLARE @tab char(1) = CHAR(9)
	set @tmp = @tmp + 'using GenericRepository.Attributes;';
	set @tmp = @tmp + @br + 'using System;';
	--	set @tmp = @tmp + @br + 'using System.Collections.Generic;';
	set @tmp = @tmp + @br + 'using System.Text;';
	set @tmp = @tmp + @br;
	set @tmp = @tmp + @br + '%namespace%';
	set @tmp = @tmp + @br + '{';

	declare @before varchar(max) = CHAR(9);
	exec dbo.GenerateComment @a_description = @tableComment, @before = @before, @a_out = @tmp output;
			
	set @tmp = @tmp + @br + @tab + '[GRTableName(TableName = "' + @p_tableName + '")]';
	set @tmp = @tmp + @br + @tab + '[Serializable]';
	set @tmp = @tmp + @br + @tab + 'public class ' + @p_tableName;
    set @tmp = @tmp + @br + @tab + '{';
	
	set @a_out = @a_out + @tmp;
END
GO

IF object_id(N'GenerateComment', N'P') IS NOT NULL
    drop procedure GenerateComment
GO

create procedure GenerateComment
(
	@a_description varchar(max),
	@before varchar(max),
	@a_out varchar(max) output	
)
as
begin
	declare @br char(1) = CHAR(10);
	declare @a_desc varchar(5000);
	if(@a_description is not null and @a_description != '')
		begin
			set @a_out = @a_out + @br + @before + '/// <summary>'
			declare c_desc cursor local for select value from STRING_SPLIT(REPLACE(REPLACE(@a_description, char(13)+char(10), ';'), char(10), ';'), ';')	--split on crlf or lf line endings
			open c_desc
			fetch next from c_desc into @a_desc
			while @@FETCH_STATUS = 0
			begin
				set @a_out = @a_out + @br + @before + '/// ' + @a_desc;			
				fetch next from c_desc into @a_desc
			end
			close c_desc
			deallocate c_desc
			set @a_out = @a_out + @br + @before + '/// </summary>'
		end
end
GO

IF object_id(N'AddClassRepositories', N'FN') IS NOT NULL
    DROP FUNCTION AddClassRepositories
GO

Create function AddClassRepositories(@p_tableName varchar(max))
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

IF object_id(N'IsPrimaryKey', N'FN') IS NOT NULL
    DROP FUNCTION IsPrimaryKey
GO

Create function IsPrimaryKey(@p_table varchar(1000), @attribute varchar(1000))
returns int
as
BEGIN
	Declare @isKey bit = 0;
	Declare @isIdentity bit = 0;
	Declare @keyOrdinal tinyint = 0;
select
	@isKey = i.is_primary_key,
	@isIdentity = tc.is_identity,
	@keyOrdinal = ic.key_ordinal
	from 
		sys.schemas s 
		inner join sys.tables t   on s.schema_id=t.schema_id
		inner join sys.indexes i  on t.object_id=i.object_id
		inner join sys.index_columns ic on i.object_id=ic.object_id 
										and i.index_id=ic.index_id
		inner join sys.columns tc on ic.object_id=tc.object_id 
									and ic.column_id=tc.column_id
	where t.name = @p_table and tc.name = @attribute and i.is_primary_key = 1 and s.name = 'dbo'
	order by t.name, ic.key_ordinal;

	if @isIdentity = 1 begin
		return 2;
	end

	if @isKey = 1 begin
		return 1;
	end
		
	return 0;
END
GO

IF object_id(N'GetSelectProcedure', N'FN') IS NOT NULL
    drop function GetSelectProcedure
GO

Create function GetSelectProcedure(@p_table varchar(1000))
returns nvarchar(max)
as
begin
	declare @StatementCols nvarchar(max) = '';
	declare @Where nvarchar(max) = '';
	declare @HasPrimaryKey bit = 0;	 
	declare @Statement nvarchar(max) = '';
	
	select
		@HasPrimaryKey = 
			case
			when (i.is_primary_key = 1) 
				then 1
				else @HasPrimaryKey
			end,
		@Where = 
			case 
			when (i.is_primary_key = 1) 
				then @where + iif(@where != '', ' and ', '') + '[' + c.name + '] = ' + 'JSON_VALUE(@jsonId, ''$.' + c.name + ''')'
				else @Where
			end,
		@StatementCols = @StatementCols + iif(@StatementCols != '', ', ' + char(13) + char(10), '') + '			[' + c.name + ']'
	from
	sys.schemas s 
		join sys.tables t   on s.schema_id=t.schema_id
		join sys.columns c on t.object_id=c.object_id
		full outer join sys.index_columns ic on c.object_id=ic.object_id
								and c.column_id = ic.column_id
		full outer join sys.indexes i on c.object_id=i.object_id
								and i.index_id = ic.index_id
	where t.name=@p_table and s.name = 'dbo'

	set @Statement = 'create procedure [dbo].[sp' + @p_table + 'Select]
(
	@jsonId nvarchar(max) = null,
	@jsonOutput nvarchar(max) OUTPUT
)
as
'
	if @HasPrimaryKey = 1
	begin
		set @Statement = @Statement + 
'-- ====================================
-- based on primary key
-- ====================================
if @jsonId is not null
begin
	set @jsonOutput = (
		select	
'+ @StatementCols + '
		from [dbo].[' + @p_table + ']
		where
		' + @Where + '
		for json path, without_array_wrapper
	)
end
'
	end

	set @Statement = @Statement + 
'-- ====================================
-- all records
-- ====================================
'
	if @HasPrimaryKey = 1
	begin
		set @Statement = @Statement + 'else
'
	end


	set @Statement = @Statement + 
'begin
	set @jsonOutput = (
		select	
'+ @StatementCols + '
		from [dbo].[' + @p_table + ']
		for json path
	)
end
'
	return @Statement;
end
GO

IF object_id(N'GetInsertProcedure', N'FN') IS NOT NULL
    drop function GetInsertProcedure
GO

Create function GetInsertProcedure(@p_table varchar(1000))
returns nvarchar(max)
as
begin
	declare @StatementCols nvarchar(max) = '';
	declare @StatementJsonCols nvarchar(max) = '';

	--SELECT sc.name, sc.system_type_id, sc.is_nullable, sc.is_identity FROM 
	select 
		@StatementCols = @StatementCols + iif(@StatementCols != '', ', ' + char(13) + char(10), '') + '			[' + sc.name + ']',
		@StatementJsonCols = @StatementJsonCols + iif(@StatementJsonCols != '', ', ' + char(13) + char(10), '') + '			json_value(a.Value, ''$.' + sc.name + ''')'
	from 
		sys.schemas s 
		inner join sys.tables t on s.schema_id = t.schema_id
		inner join sys.columns sc on sc.object_id = t.object_id
	where t.name=@p_table and s.name = 'dbo' and sc.is_identity = 0

	declare @Statement nvarchar(max);
	set @Statement = 'create procedure [dbo].[sp' + @p_table + 'Insert]
(
	@jsonInput nvarchar(max),
	@identity nvarchar(max) OUTPUT
)
as
set nocount on
set transaction isolation level read committed
begin transaction
begin try
	if @jsonInput not like ''[[]%]''
		set @jsonInput = ''['' + @jsonInput + '']''

	if isjson(@jsonInput) = 1
	begin';

	set @Statement = @Statement + '
		insert into [dbo].[' + @p_table + '](
'+ @StatementCols + '
		)
		select
' + @StatementJsonCols + '
		from openjson(@jsonInput) as a
		set @identity = (select SCOPE_IDENTITY());
	end
	else
	begin	
		-- ====================================
		-- invalid JSON
		-- ====================================
		raiserror (''Invalid JSON format.'', 16, 1)
	end
end try
begin catch
	if @@TRANCOUNT > 0
		rollback transaction;

	declare @ErrorMessage nvarchar(4000)
    declare @ErrorSeverity int
    declare @ErrorState int

    set @ErrorMessage = error_message()
    set @ErrorSeverity = error_severity()
    set @ErrorState = error_state()

    raiserror (@ErrorMessage, 
               @ErrorSeverity, 
               @ErrorState)
end catch

if @@TRANCOUNT > 0
begin
	commit transaction
end';
	return @Statement;
end
GO

IF object_id(N'GetUpdateProcedure', N'FN') IS NOT NULL
    drop function GetUpdateProcedure
GO

Create function GetUpdateProcedure(@p_table varchar(1000))
returns nvarchar(max)
as
begin
	declare @SetStatements nvarchar(max) = '';
	declare @Where nvarchar(max) = '';
	declare @HasPrimaryKey bit = 0;
	
	select
		@HasPrimaryKey = 
			case
			when (i.is_primary_key = 1) 
				then 1
				else @HasPrimaryKey
			end,
		@Where = 
			case 
			when (i.is_primary_key = 1) 
				then @where + iif(@where != '', ' and ', '') + '[' + c.name + '] = ' + 'JSON_VALUE(@jsonInput, ''$.' + c.name + ''')'
				else @Where
			end,
		@SetStatements = 
			case
			when (i.is_primary_key is null or i.is_primary_key = 0) 
				then @SetStatements + iif(@SetStatements != '', ', ' + char(13) + char(10), '') + '			[' + c.name + '] = ' + 'JSON_VALUE(@jsonInput, ''$.' + c.name + ''')'
				else @SetStatements
			end
	from
	sys.schemas s 
		join sys.tables t   on s.schema_id=t.schema_id
		join sys.columns c on t.object_id=c.object_id
		full outer join sys.index_columns ic on c.object_id=ic.object_id
								and c.column_id = ic.column_id
		full outer join sys.indexes i on c.object_id=i.object_id
								and i.index_id = ic.index_id
	where t.name=@p_table and s.name = 'dbo';

	if @HasPrimaryKey = 0 or @SetStatements = ''
	begin
		return null;
	end;
	
	declare @Statement nvarchar(max);
	set @Statement = 'create procedure [dbo].[sp' + @p_table + 'Update]
(
	@jsonInput nvarchar(max)
)
as
set nocount on
set transaction isolation level read committed
begin transaction
begin try
	if @jsonInput not like ''[[]%]''
		set @jsonInput = ''['' + @jsonInput + '']''

	if isjson(@jsonInput) = 1
	begin
		update [dbo].[' + @p_table + '] set 
'+ @SetStatements + '
		where ' + @Where + ';
	end
	else
	begin	
		-- ====================================
		-- invalid JSON
		-- ====================================
		raiserror (''Invalid JSON format.'', 16, 1)
	end
end try
begin catch
	if @@TRANCOUNT > 0
		rollback transaction;

	declare @ErrorMessage nvarchar(4000)
    declare @ErrorSeverity int
    declare @ErrorState int

    set @ErrorMessage = ERROR_MESSAGE()
    set @ErrorSeverity = ERROR_SEVERITY()
    set @ErrorState = ERROR_STATE()

    raiserror (@ErrorMessage, 
               @ErrorSeverity, 
               @ErrorState)
end catch

if @@TRANCOUNT > 0
begin
	commit transaction
end';
	return @Statement;
end
GO

IF object_id(N'PrintDtoForDatabase', N'P') IS NOT NULL
    DROP PROCEDURE PrintDtoForDatabase
GO

create procedure PrintDtoForDatabase(
	@p_DbName nvarchar(max)
	)
as
	DECLARE c_tables CURSOR LOCAL FOR SELECT t.TABLE_NAME FROM information_schema.tables t WHERE t.TABLE_CATALOG=@p_DbName and t.TABLE_SCHEMA = 'dbo';
	Declare @counter int = -1;
	DECLARE @name varchar(1000);
	DECLARE @out nvarchar(max) = '';
	Declare @repo nvarchar(max) = '';
	Declare @selectProcedure nvarchar(max) = '';
	Declare @insertProcedure nvarchar(max) = '';
	Declare @updateProcedure nvarchar(max) = '';
	DECLARE @br char(1) = CHAR(10)
	DECLARE @tab char(1) = CHAR(9)

	DECLARE @a_name varchar(1000);
	DECLARE @a_type int;
	DECLARE @a_nullable bit;
	DECLARE @a_description varchar(5000);

	DECLARE @a_desc varchar(5000);
	
	Declare @OutputTable Table(
		TableName varchar(200),
		Class nvarchar(max),
		Repository nvarchar(max),
		SelectProcedure nvarchar(max),
		InsertProcedure nvarchar(max),
		UpdateProcedure nvarchar(max)
	);

	DECLARE @keyVersion int;
begin
	open c_tables
	FETCH NEXT FROM c_tables INTO @name
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		if(@name != '') begin
			exec dbo.AddClassHeader @p_tableName = @name, @a_out = @out output;
			set @repo = @repo + dbo.AddClassRepositories(@name);
			set @selectProcedure = dbo.GetSelectProcedure(@name);
			set @insertProcedure = dbo.GetInsertProcedure(@name);
			set @updateProcedure = dbo.GetUpdateProcedure(@name);
			end

		DECLARE c_attr CURSOR LOCAL FOR (SELECT sc.name, sc.system_type_id, sc.is_nullable, convert(varchar(5000), sep.value) FROM 
			sys.schemas s 
			inner join sys.tables t on s.schema_id = t.schema_id
			inner join sys.columns sc on sc.object_id = t.object_id
			LEFT JOIN sys.extended_properties sep on 
				t.object_id = sep.major_id
                and sc.column_id = sep.minor_id
                and sep.name = 'MS_Description'
			where t.name=@name and s.name = 'dbo');
		open c_attr
		FETCH NEXT FROM c_attr INTO @a_name,@a_type,@a_nullable,@a_description
		WHILE @@FETCH_STATUS = 0
		BEGIN
			declare @before varchar(max) = CHAR(9) + CHAR(9);
			exec dbo.GenerateComment @a_description = @a_description, @before = @before, @a_out = @out output;

			set @keyVersion = dbo.IsPrimaryKey(@name, @a_name);
			
			if(@keyVersion = 1) begin
				set @out = @out + @br + @tab + @tab + '[GRPrimaryKey]';
			end

			if(@keyVersion = 2) begin
				set @out = @out + @br + @tab + @tab + '[GRAIPrimaryKey]';
			end
			
			--set @out = @out + @br + '[GRColumnName(ColumnName = "' + @a_name + '")]';
			set @out = @out + @br + @tab + @tab + 'public ' + dbo.ConvertDataType(@a_type,@a_nullable) + @a_name + ' { get; set; }';
			--set @out = @out + @br + @tab + 'public ' + dbo.ConvertDataType(@a_type,@a_nullable) + dbo.Initcap(@a_name) + ' { get; set;}';
			FETCH NEXT FROM c_attr INTO @a_name,@a_type,@a_nullable,@a_description
		end
		close c_attr
		deallocate c_attr
		set @out = @out + @br + @tab + '}';
		set @out = @out + @br + '}';
		insert into @OutputTable values (@name, @out, @repo, @selectProcedure, @insertProcedure, @updateProcedure);
		set @repo = '';
		set @out = '';--@out + '#endclass' + @br;
		set @selectProcedure = '';
		set @insertProcedure = '';
		set @updateProcedure = '';
		FETCH NEXT FROM c_tables INTO @name
	END
	close c_tables;
	Select * from @OutputTable;
	--print @out;
	--print @repo;
end
