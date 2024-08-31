using LTC2.Shared.Database.Interfaces;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.SpatiaLiteRepository.Repositories
{
    public abstract class AbstractSqliteRepository
    {
        protected SqliteConnection _connection;

        public string DbConnectionString { get; set; }

        public AbstractSqliteRepository()
        {
        }

        public abstract void Open();

        public virtual void Close()
        {
            _connection?.Close();
        }

        protected async Task<List<T>> GetRecordsAsync<T>(SqliteConnection sqlConnection, string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            var records = new List<T>();

            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                using (var sqlReader = await sqlCommand.ExecuteReaderAsync())
                {
                    while (await sqlReader.ReadAsync())
                    {
                        var record = rowMapper.Map(sqlReader);

                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                }
            }

            return records;
        }

        protected List<T> GetRecords<T>(SqliteConnection sqlConnection, string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            var records = new List<T>();

            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                using (var sqlReader = sqlCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var record = rowMapper.Map(sqlReader);

                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                }
            }

            return records;
        }

        protected async Task<int> ExecuteNonQueryAsync(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        protected int ExecuteNonQuery(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return sqlCommand.ExecuteNonQuery();
            }
        }

        protected async Task<int> ExecuteNonQueryAsync(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters, SqliteTransaction transaction)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection, transaction))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        protected int ExecuteNonQuery(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters, SqliteTransaction transaction)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection, transaction))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return sqlCommand.ExecuteNonQuery();
            }
        }

        protected async Task<T> ExecuteScalarAsync<T>(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return (T)await sqlCommand.ExecuteScalarAsync();
            }
        }

        protected T ExecuteScalar<T>(SqliteConnection sqlConnection, string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return (T)sqlCommand.ExecuteScalar();
            }
        }

        protected async Task<T> ExecuteScalarAsync<T>(SqliteConnection sqlConnection, string query, SqliteTransaction transaction, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection, transaction))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return (T)await sqlCommand.ExecuteScalarAsync();
            }
        }

        protected T ExecuteScalar<T>(SqliteConnection sqlConnection, string query, SqliteTransaction transaction, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqliteCommand(query, sqlConnection, transaction))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return (T)sqlCommand.ExecuteScalar();
            }
        }


    }
}
