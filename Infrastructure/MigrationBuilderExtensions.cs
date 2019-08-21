using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;

namespace StripeSample.Infrastructure
{
    public static class MigrationBuilderExtensions
    {
        public static void ExecuteSql(this MigrationBuilder builder, string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", fileName);
            var sql = File.ReadAllText(path);
            builder.Sql(sql);
        }
    }
}
