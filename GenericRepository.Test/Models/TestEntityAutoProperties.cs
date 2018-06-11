using GenericRepository.Attributes;
using System;

namespace GenericRepository.Test.Models
{
    [GRTableName(TableName = "TestEntityAutoPropertiesTable")]
    public class TestEntityAutoProperties
    {
        [GRAIPrimaryKey]
        public int TestEntityAutoPropertiesID { get; set; }
        [GRColumnName(ColumnName = "TestEntityAutoPropertiesName")]
        public string Name { get; set; }
        public int TestEntityAutoPropertiesOrder { get; set; }

        [GRIgnore]
        public string TestEntityAutoPropertiesDescription { get; set; }

        [GRRepositoryProperty(PropertyName = "ServerTime", Apply = GRAutoValueApply.BeforeInsert | GRAutoValueApply.BeforeUpdate, Direction = GRAutoValueDirection.In)]
        public DateTime ModifiedDate { get; set; }

        [GRRepositoryProperty(PropertyName = "UserID", Apply = GRAutoValueApply.BeforeInsert | GRAutoValueApply.BeforeUpdate)]
        public int ModifiedBy { get; set; }

        [GRInsertOnly]
        [GRRepositoryProperty(PropertyName = "ServerTime", Apply = GRAutoValueApply.BeforeInsert, Direction = GRAutoValueDirection.In)]
        public DateTime CreatedDate { get; set; }

        [GRInsertOnly]
        [GRRepositoryProperty(PropertyName = "TableName", Apply = GRAutoValueApply.BeforeInsert)]
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
