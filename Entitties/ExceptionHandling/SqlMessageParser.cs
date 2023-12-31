﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using RTools_NTS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MahtaKala.Entities.ExceptionHandling
{


    public interface ISqlErrorParser
    {
        int ErrorCode { get; }

        string Translate(string message);
    }
    public static class SqlErrorParsers
    {
        static SqlErrorParsers()
        {
            SqlErrorParsers.Register(new SqlConstraintErrorParser());
        }

        public static void SetDataContext(DataContext context)
        {
            var contextType = typeof(DataContext);
            var entityTypes = contextType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0])
                .ToList();
            foreach (var t in entityTypes)
            {
                var name = context.Model.FindEntityType(t).GetTableName();
                tablesMap.Add(name, t);
            }
        }

        static Dictionary<string, Type> tablesMap = new Dictionary<string, Type>();
        static Dictionary<int, ISqlErrorParser> parsers = new Dictionary<int, ISqlErrorParser>();

        public static string GetTableDisplayName(string tableName)
        {
            return DataContext.GetEntityTitle(tablesMap[tableName]);
        }

        public static string GetFieldDisplayName(string tableName, string fieldName)
        {
            var type = tablesMap[tableName];
            var prop = type.GetProperty(fieldName);
            var nameAttr = prop.GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();
            return nameAttr != null ? nameAttr.Name : fieldName;
        }

        public static void Register(ISqlErrorParser parser)
        {
            parsers.Add(parser.ErrorCode, parser);
        }
        public static string TranslateMessage(SqlException ex)
        {
            foreach (SqlError err in ex.Errors)
            {
                if (parsers.TryGetValue(err.Number, out var parser))
                {
                    return parser.Translate(err.Message);
                }
            }
            return ex.Message;
        }

        public static string[] OperationMessages =
        {
            "درج",
            "بروز رسانی",
            "حذف"
        };
    }

    public class SqlConstraintErrorParser : ISqlErrorParser
    {
        private static readonly Regex regex;
        static SqlConstraintErrorParser()
        {
            regex = new Regex("The (?<op>INSERT|UPDATE|DELETE) statement conflicted with the (?<constraint>.+?) constraint \"(?<name>.+?)\". The conflict occurred in database \".+?\", table \"(?<table>.+?)\", column '(?<col>.+?)'.", RegexOptions.Compiled);
        }

        public int ErrorCode => 547;

        static readonly string[] fkErrorMessages =
        {
            "مقدار داده شده برای جدول '{0}' فیلد '{1}' در جدول '{2}' وجود ندارد", //Insert
            "مقدار داده شده برای جدول '{0}' فیلد '{1}' در جدول '{2}' وجود ندارد", //Update
            "امکان حذف اطلاعات مورد نظر از جدول '{2}' وجود ندارد. این اطلاعات در فیلد '{1}' در جدول '{2}' استفاده شده است"  //Delete
        };

        public string Translate(string error)
        {
            var match = regex.Match(error);
            if (!match.Success)
            {
                return error;
            }

            var op = match.Groups["op"].Value;
            var name = match.Groups["name"].Value;
            var table = match.Groups["table"].Value;
            var column = match.Groups["col"].Value;
            var constraint = match.Groups["constraint"].Value;
            var ope = (SqlOperation)Enum.Parse(typeof(SqlOperation), op);
            var opStr = SqlErrorParsers.OperationMessages[(int)ope];
            string msg;
            if (name.StartsWith("FK_"))
            {
                msg = GetFKMessage(name, ope);

            }
            else
                msg = error;

            return $"خطای {opStr} اطلاعات. {msg}";
        }

        private string GetFKMessage(string name, SqlOperation ope)
        {
            var parts = name.Split('_');
            var fkTable = SqlErrorParsers.GetTableDisplayName(parts[1]);
            var fkField = SqlErrorParsers.GetFieldDisplayName(parts[1], parts[3]);
            var pkTable = SqlErrorParsers.GetTableDisplayName(parts[2]);

            return string.Format(fkErrorMessages[(int)ope], fkTable, fkField, pkTable);
        }
    }

    public enum SqlOperation
    {
        INSERT,
        UPDATE,
        DELETE
    }
}
