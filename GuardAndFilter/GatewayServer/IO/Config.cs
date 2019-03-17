using System;
using System.IO;
using System.Threading.Tasks;

namespace Filter
{
    class Config
    {
        public static iniFile cfg = new iniFile("config/settings.ini");

        public static void LoadEverything()
        {
            try
            {
                if (!File.Exists("config/settings.ini"))
                {
                    //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Failed to load filter, missting file(config/settings.ini).");
                    Logger.WriteLine(Logger.LogLevel.Error, "Failed to load filter, missting file(settings.ini).");
                    return;
                }
                else
                {
                    #region BACKEND settings
                    //FilterMain.shard_list_backend = bool.Parse(cfg.IniReadValue("BACKEND", "BACKEND_ENABLE"));
                    FilterMain.shard_list_backend = true;
                    #endregion

                    /* #region CLIENTLESS
                     FilterMain.ENABLE_CLIENTLESS = bool.Parse(cfg.IniReadValue("CLIENTLESS", "ENABLE_CLIENTLESS"));
                     FilterMain.Username = cfg.IniReadValue("CLIENTLESS", "USERNAME");
                     FilterMain.Password = cfg.IniReadValue("CLIENTLESS", "PASSWORD");
                     FilterMain.ServerLocale = byte.Parse(cfg.IniReadValue("CLIENTLESS", "LOCALE"));
                     FilterMain.ServerVersion = int.Parse(cfg.IniReadValue("CLIENTLESS", "VERSION"));
                     FilterMain.ShardID = int.Parse(cfg.IniReadValue("CLIENTLESS", "SHARDID"));
                     if(FilterMain.PaidUser)
                     {
                         FilterMain.Charname = cfg.IniReadValue("CLIENTLESS", "CHARNAME");
                     }
                     #endregion
                     */

                    #region Discord webhook
                    FilterMain.DiscordWebHook_Avatar = cfg.IniReadValue("CLIENTLESS", "AVATAR_URL");
                    FilterMain.DiscordWebHook_Global = cfg.IniReadValue("CLIENTLESS", "GLOBAL_MESSAGE");
                    FilterMain.DiscordWebHook_Notice = cfg.IniReadValue("CLIENTLESS", "NOTICE_MESSAGE");
                    FilterMain.DiscordWebHook_Unique = cfg.IniReadValue("CLIENTLESS", "UNIQUE_MESSAGE");
                    #endregion

                    /*#region Eventbot options
                    FilterMain.OPENTDB = cfg.IniReadValue("CLIENTLESS", "OPENTDB");
                    if(!FilterMain.OPENTDB.Contains("?amount=1"))
                    {
                        Logger.WriteLine(Logger.LogLevel.Error, "Are you retarded? DO NOT USE ?amount= BIGGER THAN 1 YOU FUCKING RETARD.");
                        FilterMain.OPENTDB = "https://opentdb.com/api.php?amount=1";
                        cfg.IniWriteValue("CLIENTLESS", "OPENTDB", "https://opentdb.com/api.php?amount=1");
                    }
                    FilterMain.ENABLE_WEBHOOK = bool.Parse(cfg.IniReadValue("CLIENTLESS", "ENABLE_WEBHOOK"));
                    FilterMain.LIMIT_WINNERS = bool.Parse(cfg.IniReadValue("CLIENTLESS", "LIMIT_WINNER"));
                  
                    #endregion*/

                    #region READ SQL settings 
                    FilterMain.sql_host = cfg.IniReadValue("SQL", "SERVER");
                    FilterMain.sql_user = cfg.IniReadValue("SQL", "LOGIN");
                    FilterMain.sql_pass = cfg.IniReadValue("SQL", "PASSWORD");
                    FilterMain.DATABASE = cfg.IniReadValue("SQL", "DATABASE");
                    if(FilterMain.DATABASE_SHARD != cfg.IniReadValue("SQL", "DATABASE_SHARD"))
                    {
                        //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Rememeber to change all SQL procedures for FILTER database when your SHARD name is different.");
                        Logger.WriteLine(Logger.LogLevel.Warning, "Rememeber to change all SQL procedures for FILTER database when your SHARD name is different.");
                    }
                    FilterMain.DATABASE_SHARD = cfg.IniReadValue("SQL", "DATABASE_SHARD");
                    if (FilterMain.DATABASE_ACC != cfg.IniReadValue("SQL", "DATABASE_ACC"))
                    {
                        //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Rememeber to change all SQL procedures for FILTER database when your ACC name is different.");
                        Logger.WriteLine(Logger.LogLevel.Warning, "Rememeber to change all SQL procedures for FILTER database when your ACC name is different.");
                    }
                    FilterMain.DATABASE_ACC = cfg.IniReadValue("SQL", "DATABASE_ACC");
                    if (FilterMain.DATABASE_LOG != cfg.IniReadValue("SQL", "DATABASE_LOG"))
                    {
                        //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Rememeber to change all SQL procedures for FILTER database when your LOG name is different.");
                        Logger.WriteLine(Logger.LogLevel.Warning, "Rememeber to change all SQL procedures for FILTER database when your LOG name is different.");
                    }
                    FilterMain.DATABASE_LOG = cfg.IniReadValue("SQL", "DATABASE_LOG");
                    #endregion

                    #region Read general settings
                    // Files stuff
                    FilterMain.language = cfg.IniReadValue("GENERAL", "LANG").ToUpper();
                    FilterMain.FILES = cfg.IniReadValue("GENERAL", "FILES").ToLower();

                    FilterMain.Server_whitelisted.Clear();
                    FilterMain.Server_opcodes.Clear();
                    string server_op = cfg.IniReadValue("GENERAL", "OPCODES").ToLower();
                    int server_count = server_op.Split(',').Length;
                    string[] server_split = server_op.Split(',');
                    for (int fakmylife = 0; fakmylife < server_count; fakmylife++)
                    {
                        ushort opcode = Convert.ToUInt16(server_split[fakmylife], 16);
                        FilterMain.Server_whitelisted.Add(opcode);
                    }

                    FilterMain.RULE = int.Parse(cfg.IniReadValue("GENERAL", "RULE"));
                    #endregion

                    #region Read download rules
                    FilterMain.download_ip = cfg.IniReadValue("DOWNLOAD", "DOWNLOAD_IP");
                    FilterMain.download_port = int.Parse(cfg.IniReadValue("DOWNLOAD", "DOWNLOAD_PORT"));
                    #endregion

                    #region Read gateway rules
                    FilterMain.gateway_remote = cfg.IniReadValue("GATEWAY", "REMOTE_IP");
                    FilterMain.gateway_mport = int.Parse(cfg.IniReadValue("GATEWAY", "REMOTE_PORT"));

                    FilterMain.gateway_local = cfg.IniReadValue("GATEWAY", "LISTEN_IP");
                    FilterMain.gateway_fport = int.Parse(cfg.IniReadValue("GATEWAY", "LISTEN_PORT"));

                    FilterMain.gateway_captcha = cfg.IniReadValue("GATEWAY", "CAPTCHA_CODE");

                    FilterMain.packet_count = int.Parse(cfg.IniReadValue("GATEWAY", "PACKET_COUNT"));
                    FilterMain.packet_reset = int.Parse(cfg.IniReadValue("GATEWAY", "PACKET_RESET"));
                    FilterMain.FLOOD_COUNT = int.Parse(cfg.IniReadValue("GATEWAY", "FLOOD_DETECTION"));
                    #endregion

                    #region Read agent rules
                    FilterMain.agent_count = int.Parse(cfg.IniReadValue("GATEWAY", "AGENT_COUNT"));

                    #region Cleaning like a bosse
                    FilterMain.agent_listen.Clear();
                    FilterMain.agent_remote.Clear();
                    FilterMain.agent_fports.Clear();
                    FilterMain.agent_mports.Clear();
                    #endregion

                    int i = 0;
                    int loaded_rules = 0;
                    if(!FilterMain.PaidUser)
                    {
                        if(FilterMain.agent_count > 1)
                        {
                            FilterMain.agent_count = 1;
                        }
                    }

                    for (i = 0; i < FilterMain.agent_count; i++)
                    {
                        // Read rules
                        if (cfg.IniReadValue("AGENT_" + i, "REMOTE_IP") != string.Empty)
                        {
                            FilterMain.agent_listen.Add(cfg.IniReadValue("AGENT_" + i, "LISTEN_IP"));
                            FilterMain.agent_remote.Add(cfg.IniReadValue("AGENT_" + i, "REMOTE_IP"));
                            FilterMain.agent_fports.Add(int.Parse(cfg.IniReadValue("AGENT_" + i, "LISTEN_PORT")));
                            FilterMain.agent_mports.Add(int.Parse(cfg.IniReadValue("AGENT_" + i, "REMOTE_PORT")));
                            loaded_rules++;
                        }
                        else
                        {
                            Logger.WriteLine(Logger.LogLevel.Error, "Agent counter is set to " + FilterMain.agent_count + " but no rules is setted in settings.ini");
                            FilterMain.agent_count = loaded_rules;
                            Logger.WriteLine(Logger.LogLevel.Warning, "New agent counter is set to " + loaded_rules + " if its wrong change it manually and restart filter.");
                        }
                    }
                    #endregion
                    //FilterMain.startup_list.Add($"[{DateTime.UtcNow}] Loaded configuration file successfully!", 1);
                }
            }
            catch(Exception ex)
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading settings, please check your configuration or restart filter!");
                Logger.WriteLine(Logger.LogLevel.Warning, ex.ToString());
                return;
            }

            #region SQL settings
            try
            {
                FilterMain.BOT_CONTROLL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'BOT_CONTROLL'")).Result);
                FilterMain.BOT_CONNECTION = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'BOT_CONNECTION'")).Result);
                FilterMain.GM_LOGIN_ONLY = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'GM_LOGIN_ONLY'")).Result);
                FilterMain.PC_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'PC_LIMIT'")).Result);
                FilterMain.IP_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'IP_LIMIT'")).Result);
                FilterMain.CAFE_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_GetFILTER] 'CAFE_LIMIT'")).Result);
                //FilterMain.startup_list.Add($"[{DateTime.UtcNow}] SQL connection was established.", 1);
            }
            catch
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading settings from SQL server.");
                Logger.WriteLine(Logger.LogLevel.Error, "Error loading settings from SQL server.");
                return;
            }
            #endregion
        }
    }
}
