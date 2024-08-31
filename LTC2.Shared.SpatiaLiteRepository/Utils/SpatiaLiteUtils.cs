using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.IO;
using System.Reflection;

namespace LTC2.Shared.SpatiaLiteRepository.Utils
{
    public class SpatiaLiteUtils
    {
        public static SqliteConnection CreateSpatiaLiteConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);

            OpenWithSpatiaLiteLoaded(connection);

            return connection;
        }

        public static void OpenWithSpatiaLiteLoaded(SqliteConnection connection)
        {
            connection.Open();
            connection.EnableExtensions(true);

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA encoding = \"UTF-16\"";
                cmd.ExecuteNonQuery();
            }

            SpatialiteLoader.Load(connection);
        }

        private static void AddSpatiaLiteToRunPath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyFolder = assembly.Location;
            var assemblyPath = Path.GetDirectoryName(assemblyFolder);

            if (assemblyPath != null)
            {
                var pathX64 = Path.Combine("SpatiaLite", "x64");
                var pathX86 = Path.Combine("SpatiaLite", "x86");

                var runtpathExtension = Environment.Is64BitProcess ? Path.Combine(assemblyPath, pathX64) : Path.Combine(assemblyPath, pathX86);
                var runpath = Environment.GetEnvironmentVariable("PATH");

                if (runpath != null && !runpath.Contains(runtpathExtension))
                {
                    var newRunpath = $"{runpath};{runtpathExtension}";
                    Environment.SetEnvironmentVariable("PATH", newRunpath);
                }
            }
        }
    }
}
