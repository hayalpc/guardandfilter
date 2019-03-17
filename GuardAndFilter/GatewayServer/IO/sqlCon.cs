#pragma warning disable
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Filter
{
    public static class sqlCon
    {
        private static string _Filter = $"Server={FilterMain.sql_host};Database={FilterMain.DATABASE};User ID={FilterMain.sql_user};Password={FilterMain.sql_pass};MultipleActiveResultSets=True";
        private static string _Shard = $"Server={FilterMain.sql_host};Database={FilterMain.DATABASE_SHARD};User ID={FilterMain.sql_user};Password={FilterMain.sql_pass};MultipleActiveResultSets=True";
        private static string _Account = $"Server={FilterMain.sql_host};Database={FilterMain.DATABASE_ACC};User ID={FilterMain.sql_user};Password={FilterMain.sql_pass};MultipleActiveResultSets=True";
        private static string _Log = $"Server={FilterMain.sql_host};Database={FilterMain.DATABASE_LOG};User ID={FilterMain.sql_user};Password={FilterMain.sql_pass};MultipleActiveResultSets=True";


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
            query = query.Replace("'", string.Empty);
            query = query.Replace("\"", string.Empty);
            return query;
        }
        #endregion

        #region Execute query
        public static async Task exec(string query)
        {
            try
            {
                using (var con = new SqlConnection(_Filter))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        await command.ExecuteNonQueryAsync(); //1min l
                    }
                }
            }
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function Exec({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function exec({query}), exception catched : {Ex.ToString()}");
            }
        }
        #endregion

        #region Producer int return
        public static async Task<Int64> prod_int2(string query)
        {
            Int64 value = 0;
            try
            {
                using (var con = new SqlConnection(_Filter))
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
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_int2({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Producer int return
        public static async Task<int> prod_int(string query)
        {
            int value = 0;
            try
            {
                using (var con = new SqlConnection(_Filter))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            read.Read();
                            value = (int)read.GetInt32(0); //no command?
                        }
                    }
                }
            }
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_int({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Producer string return
        public static async Task<string> prod_string(string query)
        {
            string value = string.Empty;
            try
            {
                using (var con = new SqlConnection(_Filter))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            read.Read();
                            value = read.GetString(0); //no command?
                        }
                    }
                }
            }
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function prod_string({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function prod_string({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Return int value
        public static async Task<int> GetInt(string query)
        {
            int value = 0;
            try
            {
                using (var con = new SqlConnection(_Filter))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.CommandType = CommandType.Text;
                        value = (int)await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Return string value
        public static async Task<string> GetString(string query)
        {
            string value = string.Empty;
            try
            {
                using (var con = new SqlConnection(_Filter))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.CommandType = CommandType.Text;
                        value = (string)await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (SqlException Ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] SQL Error -> Function GetInt({query}), exception catched : {Ex.ToString()}");
                Logger.WriteLine(Logger.LogLevel.Debug, $"SQL Error -> Function GetString({query}), exception catched : {Ex.ToString()}");
            }
            return value;
        }
        #endregion

        #region Read Royale bugged
        public static async Task Read_Royale()
        {
            try
            {
                using (var con = new SqlConnection(_Shard))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT CharName16 FROM {FilterMain.DATABASE_LOG}.._PlayersLeft WHERE BotName16 = '{FilterMain.Charname}'", con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            while (await read.ReadAsync())
                            {
                                try
                                {
                                    string CharName16 = read["CharName16"].ToString();
                                    FilterMain.BATTLE_ROYALE_BUGGED.Add(CharName16);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region Read ItemData for Eventbot
        public static async Task Read_ItemData()
        {
            try
            {
                using (var con = new SqlConnection(_Shard))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT ID, CodeName128, ObjName128 FROM {FilterMain.DATABASE_SHARD}.dbo._RefObjCommon WHERE CodeName128 LIKE 'ITEM_%' AND Service = 1", con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            while (await read.ReadAsync())
                            {
                                try
                                {
                                    uint ItemID = Convert.ToUInt32(read["ID"]);
                                    string CodeName128 = read["CodeName128"].ToString();
                                    string ItemName128 = read["ObjName128"].ToString();

                                    if (ItemID != 0 && CodeName128 != string.Empty && ItemName128 != string.Empty)
                                    {
                                        CharStrings.Items_Info.itemsidlist.Add(ItemID);
                                        CharStrings.Items_Info.itemstypelist.Add(CodeName128);
                                        CharStrings.Items_Info.itemsnamelist.Add(ItemName128);

                                        CharStrings.Items_Info.itemslevellist.Add(1);
                                        CharStrings.Items_Info.items_maxlist.Add(1);
                                        CharStrings.Items_Info.itemsdurabilitylist.Add(1);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }

            }
            catch
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading items from _RefObjCommon, check DATABASE_SHARD in settings.ini");
                Logger.WriteLine(Logger.LogLevel.Error, "Error loading items from _RefObjCommon, check DATABASE_SHARD in settings.ini");
            }
        }
        #endregion

        #region Read Mobs for Eventbot
        public static async Task Read_Monsters()
        {
            try
            {
                using (var con = new SqlConnection(_Shard))
                {
                    await con.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT ID, CodeName128, ObjName128 FROM {FilterMain.DATABASE_SHARD}.._RefObjCommon WHERE CodeName128 NOT like 'ITEM_%' AND Service = 1", con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync())
                        {
                            while (await read.ReadAsync())
                            {
                                try
                                {
                                    uint ItemID = Convert.ToUInt32(read["ID"]);
                                    string CodeName128 = read["CodeName128"].ToString();
                                    string ItemName128 = read["ObjName128"].ToString();

                                    if (ItemID != 0 && CodeName128 != string.Empty && ItemName128 != string.Empty)
                                    {
                                        CharStrings.Mobs_Info.mobsidlist.Add(ItemID);
                                        CharStrings.Mobs_Info.mobstypelist.Add(CodeName128);
                                        CharStrings.Mobs_Info.mobsnamelist.Add(ItemName128);

                                        CharStrings.Mobs_Info.mobslevellist.Add(1);
                                        CharStrings.Mobs_Info.mobshplist.Add(1);
                                        CharStrings.Mobs_Info.mobsifuniquelist.Add("1");
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                        }
                    }
                }

            }
            catch
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading monsters from _RefObjCommon, check DATABASE_SHARD in settings.ini");
                Logger.WriteLine(Logger.LogLevel.Error, "Error loading items from _RefObjCommon, check DATABASE_SHARD in settings.ini");
            }
        }
        #endregion
    }
}