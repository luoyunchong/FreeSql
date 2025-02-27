﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.DynamicEntity;
using Newtonsoft.Json;
using Xunit;

namespace FreeSql.Tests.DynamicEntity
{
    public class DynamicEntityTest
    {
        private static IFreeSql fsql = new FreeSqlBuilder().UseConnectionString(DataType.Sqlite,
                "data source=:memory:")
            .UseMonitorCommand(d => Console.WriteLine(d.CommandText)).Build();

        [Fact]
        public void NormalTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("NormalUsers")
                .Property("Id", typeof(string))
                .Property("Name", typeof(string))
                .Property("Address", typeof(string))
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Id"] = Guid.NewGuid().ToString(),
                ["Address"] = "北京市"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void AttributeTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("AttributeUsers",
                    new TableAttribute() { Name = "T_Attribute_User" },
                    new IndexAttribute("Name_Index1", "Name", false))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Address", typeof(string),
                    new ColumnAttribute() { StringLength = 150, Position = 3 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Address"] = "北京市"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            var insertId = fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteIdentity();
            var select = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void SuperClassTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("Roles", new TableAttribute() { Name = "T_Role" },
                    new IndexAttribute("Name_Index2", "Name", false))
                .Extend(typeof(BaseModel))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void SuperClassVirtualOverrideTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("Role_VirtualOverride",
                    new TableAttribute() { Name = "T_Role_VirtualOverride" },
                    new IndexAttribute("Name_Index2", "Name", false))
                .Extend(typeof(BaseModelOverride))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Operators", typeof(string), true) //重写 virtual 属性
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin",
                ["Operators"] = "manager"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void SuperClassBaseModelAbstractTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("Role_AbstractOverride",
                    new TableAttribute() { Name = "T_Role_AbstractOverride" },
                    new IndexAttribute("Name_Index2", "Name", false))
                .Extend(typeof(BaseModelAbstract))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Operators", typeof(string), true) //重写 abstract 属性
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin",
                ["Operators"] = "manager"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void SuperClassBaseModelAbstractAndVirtualTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("Role_AbstractAndVirtualOverride",
                    new TableAttribute() { Name = "Role_AbstractAndVirtualOverride" },
                    new IndexAttribute("Name_Index2", "Name", false))
                .Extend(typeof(BaseModelAbstractAndVirtual))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Operators", typeof(string), true) //重写 abstract 属性
                .Property("Operators2", typeof(string), true) //重写 virtual 属性
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin",
                ["Operators"] = "manager",
                ["Operators2"] = "manager2"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void DefaultValueTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("NormalUsers")
                .Property("Id", typeof(string))
                .Property("Age", typeof(int), false, 12)
                .Property("Longs", typeof(long), false, 16666)
                .Property("Dates", typeof(DateTime), false, "2023-05-15")
                .Property("Name", typeof(char), false, '我')
                .Property("Address", typeof(bool), false, false) //设置默认值
                .Property("Money", typeof(double), false, 265421.02) //设置默认值
                .Property("MoneyFloat", typeof(float), false, 26543.02) //设置默认值
                .Property("MoneyDecimal", typeof(decimal), true, 2663.12560) //设置默认值
                .Build();
         
            var dict = new Dictionary<string, object>
            {
                ["Id"] = Guid.NewGuid().ToString()
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
            var objects = fsql.Select<object>().AsType(table.Type).ToList();
        }
    }

    public class BaseModel
    {
        [Column(Position = 99)] public DateTime UpdateTime { get; set; }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson { get; set; }
    }

    public class BaseModelOverride
    {
        [Column(Position = 99)] public DateTime UpdateTime { get; set; }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson { get; set; }

        public virtual string Operators { get; set; }
    }

    public abstract class BaseModelAbstract
    {
        [Column(Position = 99)] public DateTime UpdateTime { get; set; }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson { get; set; }

        public abstract string Operators { get; set; }
    }

    public abstract class BaseModelAbstractAndVirtual
    {
        [Column(Position = 99)] public DateTime UpdateTime { get; set; }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson { get; set; }

        public abstract string Operators { get; set; }


        public virtual string Operators2 { get; set; }
    }
}