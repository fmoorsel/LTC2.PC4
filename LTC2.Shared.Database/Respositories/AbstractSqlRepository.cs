using LTC2.Shared.Database.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Shared.Database.Respositories
{
    public interface ISqlRepositoryParameter
    {
        string ColumnName { get; set; }
        object Value { get; set; }
    }

    public class DefaultSqlRepositoryParameter : ISqlRepositoryParameter
    {
        public string ColumnName { get; set; }
        public object Value { get; set; }
    }

    public class SqlRepositoryParameterList : List<ISqlRepositoryParameter>
    {
        public ISqlRepositoryParameter AddWithValue(string columnName, object value)
        {
            var param = new DefaultSqlRepositoryParameter() { ColumnName = columnName, Value = value };
            Add(param);
            return param;
        }
    }

    public class DbParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public string TypeName { get; set; }

        public ParameterDirection Direction { get; set; }

        public SqlDbType DbType { get; set; }

        public DbParameter(string name, object value) : this(name, value, null)
        {
        }

        public DbParameter(string name, object value, string typeName)
        {
            Name = name;
            Value = value;
            TypeName = typeName;
        }
    }
    public class AbstractSqlRepository : ISqlRepository
    {
        public string DbConnectionString { get; set; }

        protected async Task<T> GetRecordAsync<T>(string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            var records = await GetRecordsAsync(query, rowMapper, parameters);
            return records.FirstOrDefault();
        }

        protected async Task<T> GetRecordAsync<T>(SqlConnection sqlConnection, string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            var records = await GetRecordsAsync(sqlConnection, query, rowMapper, parameters);
            return records.FirstOrDefault();
        }

        protected async Task<List<T>> GetRecordsAsync<T>(string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                return await GetRecordsAsync(sqlConnection, query, rowMapper, parameters);
            }
        }

        protected async Task<List<T>> GetRecordsAsync<T>(SqlConnection sqlConnection, string query, IRowMapper<T> rowMapper, IDictionary<string, object> parameters = null)
            where T : class
        {
            var records = new List<T>();

            using (var sqlCommand = new SqlCommand(query, sqlConnection))
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

        protected async Task<List<T>> GetRecordsByStoredProcedureAsync<T>(string storedProcedure, IRowMapper<T> rowMapper, List<DbParameter> parameters = null)
            where T : class
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                return await GetRecordsByStoredProcedureAsync(sqlConnection, storedProcedure, rowMapper, parameters);
            }
        }

        protected List<T> GetRecordsByStoredProcedure<T>(string storedProcedure, IRowMapper<T> rowMapper, List<DbParameter> parameters = null)
            where T : class
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                sqlConnection.Open();

                return GetRecordsByStoredProcedure(sqlConnection, storedProcedure, rowMapper, parameters);
            }
        }

        protected List<T> GetRecordsByStoredProcedure<T>(SqlConnection sqlConnection, string storedProcedure, IRowMapper<T> rowMapper, List<DbParameter> parameters = null)
            where T : class
        {
            var records = new List<T>();

            using (var sqlCommand = new SqlCommand(storedProcedure, sqlConnection))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var dbParameter in parameters)
                    {
                        var parameter = sqlCommand.Parameters.AddWithValue(dbParameter.Name, dbParameter.Value);

                        if (dbParameter.Value is DataTable)
                        {
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = dbParameter.TypeName;
                        }
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
        protected async Task<List<T>> GetRecordsByStoredProcedureAsync<T>(SqlConnection sqlConnection, string storedProcedure, IRowMapper<T> rowMapper, List<DbParameter> parameters = null)
            where T : class
        {
            var records = new List<T>();

            using (var sqlCommand = new SqlCommand(storedProcedure, sqlConnection))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var dbParameter in parameters)
                    {
                        var parameter = sqlCommand.Parameters.AddWithValue(dbParameter.Name, dbParameter.Value);

                        if (dbParameter.Value is DataTable)
                        {
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = dbParameter.TypeName;
                        }
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

        protected async Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                return await ExecuteNonQueryAsync(sqlConnection, query, parameters);
            }
        }

        protected async Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object> parameters, SqlTransaction transaction)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                return await ExecuteNonQueryAsync(sqlConnection, query, parameters, transaction);
            }
        }

        protected async Task<int> ExecuteNonQueryAsync(SqlConnection sqlConnection, string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqlCommand(query, sqlConnection))
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

        protected async Task<int> ExecuteNonQueryAsync(SqlConnection sqlConnection, string query, IDictionary<string, object> parameters, SqlTransaction transaction)
        {
            using (var sqlCommand = new SqlCommand(query, sqlConnection, transaction))
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

        protected async Task<T> ExecuteScalarAsync<T>(string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                return (T)await ExecuteScalarAsync<T>(sqlConnection, query, null, parameters);
            }
        }

        protected async Task<T> ExecuteScalarAsync<T>(SqlConnection sqlConnection, string query, SqlTransaction transaction, IDictionary<string, object> parameters = null)
        {
            using (var sqlCommand = new SqlCommand(query, sqlConnection, transaction))
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

        protected int ExecuteBatchViaDataAdapter(SqlConnection sqlConnection, string query, SqlTransaction transaction, DataTable dataTable, IParameterMapper parameterMapper)
        {
            using (var sqlCommand = new SqlCommand(query, sqlConnection, transaction))
            {
                sqlCommand.UpdatedRowSource = UpdateRowSource.None;
                parameterMapper.Map(sqlCommand.Parameters, dataTable);

                SqlDataAdapter adapter = new SqlDataAdapter();

                adapter.InsertCommand = sqlCommand;
                adapter.UpdateBatchSize = dataTable.Rows.Count;

                return adapter.Update(dataTable);
            }
        }

        protected async Task ExecuteStoredProcedureNonQueryAsync(string storedProcedure, List<DbParameter> parameters = null, DbParameter outputDbParameter = null)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();

                await ExecuteStoredProcedureNonQueryAsync(sqlConnection, storedProcedure, parameters, outputDbParameter);
            }
        }
        protected void ExecuteStoredProcedureNonQuery(string storedProcedure, List<DbParameter> parameters = null, DbParameter outputDbParameter = null)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                sqlConnection.Open();

                ExecuteStoredProcedureNonQuery(sqlConnection, storedProcedure, parameters, outputDbParameter);
            }
        }

        protected void ExecuteStoredProcedureNonQuery(SqlConnection sqlConnection, string storedProcedure, List<DbParameter> parameters = null, DbParameter outputDbParameter = null)
        {
            using (var sqlCommand = new SqlCommand(storedProcedure, sqlConnection))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var dbParameter in parameters)
                    {
                        var parameter = sqlCommand.Parameters.AddWithValue(dbParameter.Name, dbParameter.Value);

                        if (dbParameter.Value is DataTable)
                        {
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = dbParameter.TypeName;
                        }
                    }

                    if (outputDbParameter != null)
                    {
                        var parameter = sqlCommand.Parameters.Add(outputDbParameter.Name, outputDbParameter.DbType);
                        parameter.Direction = outputDbParameter.Direction;
                    }
                }

                sqlCommand.ExecuteNonQuery();

                if (outputDbParameter != null)
                {
                    outputDbParameter.Value = sqlCommand.Parameters[outputDbParameter.Name].Value;
                }
            }
        }


        protected async Task ExecuteStoredProcedureNonQueryAsync(SqlConnection sqlConnection, string storedProcedure, List<DbParameter> parameters = null, DbParameter outputDbParameter = null)
        {
            using (var sqlCommand = new SqlCommand(storedProcedure, sqlConnection))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var dbParameter in parameters)
                    {
                        var parameter = sqlCommand.Parameters.AddWithValue(dbParameter.Name, dbParameter.Value);

                        if (dbParameter.Value is DataTable)
                        {
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = dbParameter.TypeName;
                        }
                    }

                    if (outputDbParameter != null)
                    {
                        var parameter = sqlCommand.Parameters.Add(outputDbParameter.Name, outputDbParameter.DbType);
                        parameter.Direction = outputDbParameter.Direction;
                    }
                }

                await sqlCommand.ExecuteNonQueryAsync();

                if (outputDbParameter != null)
                {
                    outputDbParameter.Value = sqlCommand.Parameters[outputDbParameter.Name].Value;
                }
            }
        }

        protected async Task ExecuteStoredProcedureNonQueryInTransactionAsync(SqlConnection sqlConnection, SqlTransaction transaction, string storedProcedure, List<DbParameter> parameters = null, DbParameter outputDbParameter = null)
        {
            using (var sqlCommand = new SqlCommand(storedProcedure, sqlConnection, transaction))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var dbParameter in parameters)
                    {
                        var parameter = sqlCommand.Parameters.AddWithValue(dbParameter.Name, dbParameter.Value);

                        if (dbParameter.Value is DataTable)
                        {
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = dbParameter.TypeName;
                        }
                    }

                    if (outputDbParameter != null)
                    {
                        var parameter = sqlCommand.Parameters.Add(outputDbParameter.Name, outputDbParameter.DbType);
                        parameter.Direction = outputDbParameter.Direction;
                    }
                }

                await sqlCommand.ExecuteNonQueryAsync();

                if (outputDbParameter != null)
                {
                    outputDbParameter.Value = sqlCommand.Parameters[outputDbParameter.Name].Value;
                }
            }
        }
        protected async Task WriteToServer(string tableName, DataTable dataTable)
        {
            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                await sqlConnection.OpenAsync();
                await WriteToServer(sqlConnection, tableName, dataTable);
            }
        }

        protected async Task WriteToServer(SqlConnection sqlConnection, string tableName, DataTable dataTable)
        {
            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection))
            {
                sqlBulkCopy.DestinationTableName = tableName;
                await sqlBulkCopy.WriteToServerAsync(dataTable);
            }
        }

        /// <summary>
        /// Converts the given list of data objects into a DataTable. Make sure that the properties in the given data object are in exact order of the 
        /// columns in the corresponding table of the database. Otherwise, the data is mapped to the wrong columns.
        /// </summary>
        protected DataTable ConvertToDataTable<T>(IList<T> list)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();

            for (var i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                var propertyDescriptor = propertyDescriptorCollection[i];
                var propType = propertyDescriptor.PropertyType;

                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    table.Columns.Add(propertyDescriptor.Name, Nullable.GetUnderlyingType(propType));
                }
                else
                {
                    table.Columns.Add(propertyDescriptor.Name, propType);
                }
            }

            var values = new object[propertyDescriptorCollection.Count];

            foreach (var listItem in list)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(listItem);
                }

                table.Rows.Add(values);
            }

            return table;
        }

        protected void TryRollback(SqlTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Deadlock retry helper method which accepts an async method that accepts one argument (TArgument). This method is executed 
        /// for the given maximum number of times when the SQL server returned error code 1205 (Deadlock victim) with a delay of 50
        /// milliseconds between each try.
        /// 
        /// This method is a "copy" from the following stack overflow question:
        /// https://stackoverflow.com/questions/28814267/how-do-i-pass-async-method-as-action-or-func
        /// 
        /// The error codes returned by SQL server can be read here:
        /// All error codes: https://msdn.microsoft.com/en-us/library/cc645603.aspx
        /// Error codes 1000 - 1999: https://msdn.microsoft.com/en-us/library/cc645603.aspx
        /// Error code 1205: https://msdn.microsoft.com/en-us/library/aa337376.aspx
        /// </summary>
        public async Task DeadlockRetryHelper<TArgument>(Func<TArgument, Task> method, TArgument argument, int maxRetries = 3)
        {
            var retryCount = 0;
            var delay = 50;

            while (retryCount < maxRetries)
            {
                try
                {
                    await method(argument);
                    return;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Deadlock
                    {
                        retryCount++;

                        if (retryCount >= maxRetries)
                            throw;

                        // Wait for X milliseconds
                        Thread.Sleep(delay);

                        // Increase the delay between each retry
                        delay = delay * 5;
                    }
                    else
                        throw;
                }
            }
        }
    }

}
