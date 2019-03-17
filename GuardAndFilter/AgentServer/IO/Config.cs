using System;
using System.IO;
using System.Net;
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
                    Logger.WriteLine(Logger.LogLevel.Error, "Failed to load filter, missting file(settings.ini).");
                    return;
                }
                else
                {
                    #region READ SQL settings 
                    FilterMain.sql_host = cfg.IniReadValue("SQL", "SERVER");
                    FilterMain.sql_user = cfg.IniReadValue("SQL", "LOGIN");
                    FilterMain.sql_pass = cfg.IniReadValue("SQL", "PASSWORD");
                    FilterMain.sql_db = cfg.IniReadValue("SQL", "DATABASE");
                    #endregion

                    #region Read general settings
                    // Files stuff
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

                    #region Read filter rules
                    FilterMain.PACKET_COUNT = int.Parse(cfg.IniReadValue("FILTER", "PACKET_COUNT"));
                    FilterMain.PACKET_RESET = int.Parse(cfg.IniReadValue("FILTER", "PACKET_RESET"));
                    FilterMain.FLOOD_COUNT = int.Parse(cfg.IniReadValue("FILTER", "FLOOD_DETECTION"));
                    #endregion

                    #region Read agent rule
                    // Read rules
                    if (cfg.IniReadValue("AGENT", "REMOTE_IP") != string.Empty)
                    {
                        FilterMain.agent_listen = cfg.IniReadValue("AGENT", "LISTEN_IP");
                        FilterMain.agent_remote = cfg.IniReadValue("AGENT", "REMOTE_IP");
                        FilterMain.agent_fports = int.Parse(cfg.IniReadValue("AGENT", "LISTEN_PORT"));
                        FilterMain.agent_mports = int.Parse(cfg.IniReadValue("AGENT", "REMOTE_PORT"));
                    }
                    else
                    {
                        Logger.WriteLine(Logger.LogLevel.Error, "Error detected on [AGENT] in settings.ini, please re-check and restart filter");
                    }
                    #endregion

                }
            }
            catch
            {
                Logger.WriteLine(Logger.LogLevel.Warning, "Error loading settings, please check your configuration or restart filter!");
            }

            #region SQL settings
            try
            {
                FilterMain.PC_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PC_LIMIT'")).Result);
                FilterMain.REWARD_PC_LIMIT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REWARD_PC_LIMIT'")).Result);
                FilterMain.BA_PC_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BA_PC_LIMIT'")).Result);
                FilterMain.CTF_PC_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'CTF_PC_LIMIT'")).Result);
                FilterMain.JOB_PC_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'JOB_PC_LIMIT'")).Result);
                FilterMain.IP_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'IP_LIMIT'")).Result);
                FilterMain.CAFE_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'CAFE_LIMIT'")).Result);
                FilterMain.PLUS_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PLUS_LIMIT'")).Result);
                FilterMain.GUILD_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GUILD_LIMIT'")).Result);
                FilterMain.UNION_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'UNION_LIMIT'")).Result);

                FilterMain.ACADEMY_CREATE_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_CREATE_DISABLED'")).Result);
                FilterMain.ACADEMY_INVITE_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_INVITE_DISABLED'")).Result);
                FilterMain.ACADEMY_JOIN_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_JOIN_DISABLED'")).Result);
                FilterMain.ACADEMY_ACCEPT_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_ACCEPT_DISABLED'")).Result);
                FilterMain.ACADEMY_BAN_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_BAN_DISABLED'")).Result);
                FilterMain.ACADEMY_LEAVE_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ACADEMY_LEAVE_DISABLED'")).Result);

                FilterMain.ARENA_REGISTER_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ARENA_REGISTER_LEVEL'")).Result);
                FilterMain.CTF_REGISTER_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'CTF_REGISTER_LEVEL'")).Result);

                FilterMain.GLOBAL_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GLOBAL_LEVEL'")).Result);
                FilterMain.GLOBAL_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GLOBAL_DELAY'")).Result);

                FilterMain.REVERSE_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REVERSE_LEVEL'")).Result);
                FilterMain.REVERSE_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REVERSE_DELAY'")).Result);
                FilterMain.REVERSE_JOB_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REVERSE_JOB_DISABLED'")).Result);

                FilterMain.RESCURRENT_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'RESCURRENT_LEVEL'")).Result);
                FilterMain.RESCURRENT_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'RESCURRENT_DELAY'")).Result);
                FilterMain.RESCURRENT_JOB_DISABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'RESCURRENT_JOB_DISABLED'")).Result);

                FilterMain.STALL_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'STALL_LEVEL'")).Result);
                FilterMain.STALL_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'STALL_DELAY'")).Result);

                FilterMain.EXCHANGE_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'EXCHANGE_LEVEL'")).Result);
                FilterMain.EXCHANGE_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'EXCHANGE_DELAY'")).Result);

                FilterMain.ZERK_LEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ZERK_LEVEL'")).Result);
                FilterMain.ZERK_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ZERK_DELAY'")).Result);

                FilterMain.RESTART_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'RESTART_DELAY'")).Result);
                FilterMain.EXIT_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'EXIT_DELAY'")).Result);

                FilterMain.GM_LOGIN_ONLY = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GM_LOGIN_ONLY'")).Result);
                FilterMain.GM_START_VISIBLE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GM_START_VISIBLE'")).Result);
                FilterMain.GM_MOBKILL_PROTECTION = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GM_MOBKILL_PROTECTION'")).Result);

                FilterMain.WELCOME_MSG = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'WELCOME_MSG'")).Result;

                FilterMain.TRADE_BUG_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'TRADE_BUG_DELAY'")).Result);
                if(FilterMain.TRADE_BUG_DELAY < 5)
                {
                    FilterMain.TRADE_BUG_DELAY = 5;
                }
                FilterMain.ATTACK_BUG_DELAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ATTACK_BUG_DELAY'")).Result);

                FilterMain.BOT_CONTROLL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_CONTROLL'")).Result);
                FilterMain.BOT_CONNECTION = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_CONNECTION'")).Result);

                /*
                FilterMain.BOT_FORTRESS_ARENA = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_FORTRESS_ARENA'")).Result);
                */
                FilterMain.BOT_BATTLE_ARENA = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_BATTLE_ARENA'")).Result);
                FilterMain.BOT_CTF_ARENA = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_CTF_ARENA'")).Result);
                /*
                FilterMain.BOT_JOB_TEMPLE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_JOB_TEMPLE'")).Result);
                */
                FilterMain.BOT_WATER_TEMPLE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_WATER_TEMPLE'")).Result);

                FilterMain.BOT_PVP = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_PVP'")).Result);
                FilterMain.BOT_TRACE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_TRACE'")).Result);
                FilterMain.BOT_EXCHANGE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_EXCHANGE'")).Result);
                FilterMain.BOT_STALL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_STALL'")).Result);
                FilterMain.BOT_PARTY = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_PARTY'")).Result);
                FilterMain.BOT_JOBBING = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_JOBBING'")).Result);

                FilterMain.BOT_ALCHEMY_ELIXIR = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_ALCHEMY_ELIXIR'")).Result);
                FilterMain.BOT_ALCHEMY_STONE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_ALCHEMY_STONE'")).Result);

                FilterMain.JOB_ADVANCED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'JOB_ADVANCED'")).Result);
                FilterMain.JOB_ZERK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'JOB_ZERK'")).Result);
                FilterMain.JOB_TRACE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'JOB_TRACE'")).Result);

                /*
                FilterMain.BOT_MAX_PLAYTIME_DAY = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_PLAYTIME_DAY'")).Result);
                FilterMain.BOT_MAX_PLAYTIME_WEEK = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_PLAYTIME_WEEK'")).Result);
                FilterMain.BOT_MAX_PLAYTIME_WEEKEND = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_PLAYTIME_WEEKEND'")).Result);

                FilterMain.BOT_MAX_EXP_DAY = Int32.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_EXP_DAY'")).Result);
                FilterMain.BOT_MAX_EXP_WEEK = Int32.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_EXP_WEEK'")).Result);
                FilterMain.BOT_MAX_EXP_WEEKEND = Int32.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'BOT_MAX_EXP_WEEKEND'")).Result);
                */

                FilterMain.LOG_ALLCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_ALLCHAT'")).Result);
                FilterMain.LOG_GMCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_GMCHAT'")).Result);
                FilterMain.LOG_PMCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_PMCHAT'")).Result);
                FilterMain.LOG_PARTYCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_PARTYCHAT'")).Result);
                FilterMain.LOG_GUILDCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_GUILDCHAT'")).Result);
                FilterMain.LOG_ACADEMYCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_ACADEMYCHAT'")).Result);
                FilterMain.LOG_UNIONCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_UNIONCHAT'")).Result);
                FilterMain.LOG_GLOBALCHAT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'LOG_GLOBALCHAT'")).Result);

                FilterMain.REWARDPERHOUR_ENABLED = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REWARDPERHOUR_ENABLED'")).Result);
                FilterMain.REWARDPERHOUR_INFORMPLAYER = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REWARDPERHOUR_INFORMPLAYER'")).Result);
                FilterMain.REWARDPERHOUR_ITEMID = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REWARDPERHOUR_ITEMID'")).Result;
                FilterMain.REWARDPERHOUR_OPTLEVEL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'REWARDPERHOUR_OPTLEVEL'")).Result);

                FilterMain.PHBOT_LOCK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PHBOT_LOCK'")).Result);
                FilterMain.PM_TICKET = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PM_TICKET'")).Result);

                FilterMain.ITEM_LOCK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK'")).Result);
                FilterMain.ITEM_LOCK_MAX_FAIL = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_MAX_FAIL'")).Result);

                FilterMain.ITEM_LOCK_UP_STR = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_UP_STR'")).Result);
                FilterMain.ITEM_LOCK_UP_INT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_UP_INT'")).Result);
                FilterMain.ITEM_LOCK_UP_SKILL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_UP_SKILL'")).Result);

                FilterMain.ITEM_LOCK_EXCHANGE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_EXCHANGE'")).Result);
                FilterMain.ITEM_LOCK_STALL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_STALL'")).Result);
                FilterMain.ITEM_LOCK_STORAGE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_STORAGE'")).Result);
                FilterMain.ITEM_LOCK_BUY_ITEM = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_BUY_ITEM'")).Result);
                FilterMain.ITEM_LOCK_SELL_ITEM = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_SELL_ITEM'")).Result);
                FilterMain.ITEM_LOCK_DROP_STUFF = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_DROP_STUFF'")).Result);


                FilterMain.ITEM_LOCK_GLOBAL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GLOBAL'")).Result);
                FilterMain.ITEM_LOCK_AVATAR = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_AVATAR'")).Result);
                FilterMain.ITEM_LOCK_ALCHEMY = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_ALCHEMY'")).Result);
                FilterMain.ITEM_LOCK_ALCHEMY_STONE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_ALCHEMY_STONE'")).Result);
                FilterMain.ITEM_LOCK_ALCHEMY_DISSAMBLE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_ALCHEMY_DISSAMBLE'")).Result);

                FilterMain.ITEM_LOCK_GUILD_KICK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_KICK'")).Result);
                FilterMain.ITEM_LOCK_GUILD_DISBAND = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_DISBAND'")).Result);
                FilterMain.ITEM_LOCK_GUILD_LEAVE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_LEAVE'")).Result);
                FilterMain.ITEM_LOCK_GUILD_DONATE_GP = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_DONATE_GP'")).Result);
                FilterMain.ITEM_LOCK_GUILD_PROMOTE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_PROMOTE'")).Result);
                FilterMain.ITEM_LOCK_GUILD_STORAGE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ITEM_LOCK_GUILD_STORAGE'")).Result);

                FilterMain.WICKED_STALL_PRICE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'WICKED_STALL_PRICES'")).Result);

                FilterMain.ALEXUS_GOLD_COIN = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'ALEXUS_GOLD_COIN'")).Result);


                FilterMain.FORTRESS_TRACE = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'FORTRESS_TRACE'")).Result);
                FilterMain.FORTRESS_ZERK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'FORTRESS_ZERK'")).Result);
                FilterMain.FORTRESS_BERSERKPOT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'FORTRESS_BERSERKPOT'")).Result);
                FilterMain.FORTRESS_RESURRECTIONSCROLL = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'FORTRESS_RESURRECTIONSCROLL'")).Result);

                FilterMain.JOB_BERSERKPOT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'JOB_BERSERKPOT'")).Result);

                FilterMain.PVP_ZERK = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PVP_ZERK'")).Result);
                FilterMain.PVP_BERSERKPOT = bool.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'PVP_BERSERKPOT'")).Result);


            }
            catch (Exception ex)
            {
                Logger.WriteLine(Logger.LogLevel.Error, ex.ToString());
                return;
            }
            #endregion
        }
    }
}
