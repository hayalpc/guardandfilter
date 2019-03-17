using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Filter
{
    public static class sqlCon
    {
        private static string _sqlConnection = $"Server={FilterMain.sql_host};Database={FilterMain.sql_db};User ID={FilterMain.sql_user};Password={FilterMain.sql_pass};MultipleActiveResultSets=True";

        /*
        Notes to future mike:
        In C# when coding SQL use
        using(var con = new SqlConnection(string))
        {

        }
        And you do not need to call con.Close();
        */

        #region Clean query
        public static string clean(string query)
        {
            if (query.Contains("'") || query.Contains("\""))
            {
                FilterMain.blocked_agent_sqlinject++;
            }

            query = query.Replace("'", string.Empty);
            query = query.Replace("\"", string.Empty);
            return query;
        }
        #endregion

        #region Execute query
        public static async Task exec(string query)
        {
            FilterMain.total_sql_queries++;
            try
            {
                using (var con = new SqlConnection(_sqlConnection))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        await command.ExecuteNonQueryAsync(); //1min l
                    }
                }
            }
            catch (NullReferenceException)
            {
                FilterMain.total_agent_sql_nullexception++;
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function exec({query}), exception catched : Null exception");
                return;
            }
            catch (SqlException Ex)
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
        }
        #endregion

        #region Producer int return
        public static async Task<Int64> prod_int2(string query)
        {
            FilterMain.total_sql_queries++;
            Int64 value = 0;
            try
            {
                using (var con = new SqlConnection(_sqlConnection))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            await read.ReadAsync();
                            value = Convert.ToInt64(read.GetValue(0)); //no command?
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                FilterMain.total_agent_sql_nullexception++;
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_int({query}), exception catched : Null exception");
                return 0;
            }
            catch (SqlException Ex)
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Producer int return
        public static async Task<int> prod_int(string query)
        {
            FilterMain.total_sql_queries++;
            int value = 0;
            try
            {
                using (var con = new SqlConnection(_sqlConnection))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            await read.ReadAsync();
                            value = (int)read.GetInt32(0); //no command?
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                FilterMain.total_agent_sql_nullexception++;
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_int({query}), exception catched : Null exception");
                return 0;
            }
            catch (SqlException Ex)
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Producer string return
        public static async Task<string> prod_string(string query)
        {
            FilterMain.total_sql_queries++;
            string value = string.Empty;
            try
            {
                using (var con = new SqlConnection(_sqlConnection))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            await read.ReadAsync();
                            value = read.GetString(0); //no command?
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                FilterMain.total_agent_sql_nullexception++;
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_string({query}), exception catched : Null exception");
                return "";
            }
            catch (SqlException Ex)
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Return string value
        public static async Task<string> GetString(string query)
        {
            FilterMain.total_sql_queries++;
            string value = string.Empty;
            try
            {
                using (var con = new SqlConnection(_sqlConnection))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.CommandType = CommandType.Text;
                        value = (string)await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (NullReferenceException)
            {
                FilterMain.total_agent_sql_nullexception++;
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetString({query}), exception catched : Null exception");
                return "";
            }
            catch (SqlException Ex)
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion
    }
}