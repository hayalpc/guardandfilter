using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace Filter
{
    sealed class FilterMain
    {
        public static bool BATTLE_ROYALE_MODE = false;

        public static List<string> BATTLE_ROYALE = new List<string>();
        public static List<string> BATTLE_ROYALE_ALIVE = new List<string>();
        public static bool BATTLE_ROYALE_SHRINK_AREA = false;
        public static List<string> BATTLE_ROYALE_CHEATERS = new List<string>();


        #region Funny information

        public static Int64 blocked_agent_injects = 0;

        public static Int64 blocked_agent_mikeboty = 0;

        public static Int64 blocked_agent_dosattacks = 0;
        public static Int64 blocked_agent_exploits = 0;

        public static Int64 blocked_agent_sqlinject = 0;
        public static Int64 total_agent_sql_nullexception = 0;
        public static Int64 total_agent_sql_deadlock = 0;

        public static Int64 blocked_agent_packetflood = 0;
        public static Int64 blocked_agent_packetflood_charscreen = 0;

        public static Int64 total_sql_queries = 0;
        public static Int64 total_visited_users = 0;
        public static Int64 daily_visited_users = 0;
        public static Int64 bots_found = 0;

        public static DateTime protection_started = DateTime.Now;
        #endregion


        public static List<ushort> Server_opcodes = new List<ushort>();
        public static List<ushort> Server_whitelisted = new List<ushort>();
        public static bool debug_mike = false;
        public static List<string> names = new List<string>();
        public static Hashtable Last_login = new Hashtable();

        public static void DiscordWebHook(string message)
        {
            /*
            try
            {
                string url = "https://discordapp.com/api/webhooks/376369777825939477/Q-aBi7V5ZXCMYtqrswiPsCodw-znll0XV8vnnggO3KBtYzmSXSewE5VdI5tS0tFmH8Fq";
                string sender = "KRYLFILTER AgentFilter";
                using (var client = new WebClient())
                {
                    var data = new NameValueCollection();

                    data["content"] = message;
                    data["username"] = sender;
                    data["avatar_url"] = "https://i.imgur.com/Xu9Stsu.jpg";
                    var response = client.UploadValues(url, "POST", data);
                }
            }
            catch
            {
                //Logger.WriteLine(Logger.LogLevel.Debug, "Error sending DiscordWebHook()");
            }*/
        }

        #region SQL settings
        public static string sql_host = "(local)";
        public static string sql_user = "sa";
        public static string sql_pass = "KRYLFITLER";
        public static string sql_db = "FILTER";
        public static string DATABASE_LOG = "SRO_VT_LOG";
        #endregion

        #region Agent settings *NEW*
        /*
            All agent settings here (:
        */
        public static string FILES = "vSRO";
        public static string LANG = "en";
        /*
            RULES:
            0 = default;
            1 = continue;
            2 = disconnect;
            3 = firewall block;
        */
        public static int RULE = 2;
        public static int dMaxBytesPerSec_Agent = 1000;
        public static string agent_remote = "127.0.0.1";
        public static string agent_listen = "127.0.0.1";
        public static int agent_fports = 15884;
        public static int agent_mports = 1338;
        public static List<string> ip_list_a = new List<string>();
        public static Hashtable BAD_Opcodes = new Hashtable();
        public static Dictionary<ushort, Int64> ALL_Opcodes = new Dictionary<ushort, Int64>();

        public static int PACKET_COUNT = 50;
        public static int PACKET_RESET = 500;
        public static int FLOOD_COUNT = 30;
        public static List<string> BYPASS = new List<string>();
        public static List<string> CAFE = new List<string>();
        public static List<short> town_list = new List<short>();
        public static List<short> fortress_list = new List<short>();
        public static List<short> city_list = new List<short>();
        public static List<short> bot_region_list = new List<short>();
        public static List<int> BlockedSkills = new List<int>();
        public static uint[] jobcave_teleport = {164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181};

        public static List<short> zerk_list = new List<short>();
        public static List<short> party_list = new List<short>();
        public static List<short> event_region_list = new List<short>();
        public static List<int> event_region_skills = new List<int>();
        public static List<int> fortress_region_skills = new List<int>();
        public static List<int> job_region_skills = new List<int>();
        public static List<int> pvp_region_skills = new List<int>();
        public static List<int> bot_region_skills = new List<int>();
        #endregion

        #region SQL CONFIGURATION
        public static int PC_LIMIT = 0;
        public static int BA_PC_LIMIT = 0;
        public static int CTF_PC_LIMIT = 0;
        public static int JOB_PC_LIMIT = 0;
        public static bool REWARD_PC_LIMIT = false;
        public static int IP_LIMIT = 0;
        public static int CAFE_LIMIT = 0;
        public static int PLUS_LIMIT = 0;
        public static int GUILD_LIMIT = 0;
        public static int UNION_LIMIT = 0;

        public static bool ACADEMY_CREATE_DISABLED = false;
        public static bool ACADEMY_INVITE_DISABLED = false;
        public static bool ACADEMY_JOIN_DISABLED = false;
        public static bool ACADEMY_ACCEPT_DISABLED = false;
        public static bool ACADEMY_BAN_DISABLED = false;
        public static bool ACADEMY_LEAVE_DISABLED = false;

        public static int ARENA_REGISTER_LEVEL = 0;
        public static int CTF_REGISTER_LEVEL = 0;

        public static int GLOBAL_LEVEL = 0;
        public static int GLOBAL_DELAY = 0;

        public static int REVERSE_LEVEL = 0;
        public static int REVERSE_DELAY = 0;
        public static bool REVERSE_JOB_DISABLED = false;

        public static int RESCURRENT_LEVEL = 0;
        public static int RESCURRENT_DELAY = 0;
        public static bool RESCURRENT_JOB_DISABLED = false;

        public static int STALL_LEVEL = 0;
        public static int STALL_DELAY = 0;

        public static int EXCHANGE_LEVEL = 0;
        public static int EXCHANGE_DELAY = 0;

        public static int ZERK_LEVEL = 0;
        public static int ZERK_DELAY = 0;

        public static int RESTART_DELAY = 0;
        public static int EXIT_DELAY = 0;

        public static bool GM_LOGIN_ONLY = false;
        public static bool GM_START_VISIBLE = false;
        public static bool GM_MOBKILL_PROTECTION = false;
        public static bool GM_WRITE_WHITE = true;

        public static string WELCOME_MSG = string.Empty;

        public static int TRADE_BUG_DELAY = 30;
        public static int ATTACK_BUG_DELAY = 5;

        public static bool BOT_CONTROLL = false;
        public static bool BOT_CONNECTION = true;
        public static List<string> BOT_LIST = new List<string>();

        public static bool BOT_FORTRESS_ARENA = true;
        public static bool BOT_BATTLE_ARENA = true;
        public static bool BOT_CTF_ARENA = true;
        public static bool BOT_JOB_TEMPLE = true;
        public static bool BOT_WATER_TEMPLE = true;

        public static bool BOT_PVP = true;
        public static bool BOT_TRACE = true;
        public static bool BOT_EXCHANGE = true;
        public static bool BOT_STALL = true;
        public static bool BOT_PARTY = true;
        public static bool BOT_JOBBING = true;

        public static bool BOT_ALCHEMY_ELIXIR = true;
        public static bool BOT_ALCHEMY_STONE = true;

        public static int BOT_MAX_PLAYTIME_DAY = 0;
        public static int BOT_MAX_PLAYTIME_WEEK = 0;
        public static int BOT_MAX_PLAYTIME_WEEKEND = 0;

        public static Int32 BOT_MAX_EXP_DAY = 0;
        public static Int32 BOT_MAX_EXP_WEEK = 0;
        public static Int32 BOT_MAX_EXP_WEEKEND = 0;

        public static bool JOB_ADVANCED = false;
        public static bool JOB_TRACE = false;
        public static bool JOB_ZERK = false;

        public static bool REWARDPERHOUR_ENABLED = false;
        public static bool REWARDPERHOUR_INFORMPLAYER = false;
        public static string REWARDPERHOUR_ITEMID = "silk";
        public static int REWARDPERHOUR_OPTLEVEL = 1;

        public static bool LOG_ALLCHAT = false;
        public static bool LOG_GMCHAT = false;
        public static bool LOG_PMCHAT = false;
        public static bool LOG_PARTYCHAT = false;
        public static bool LOG_GUILDCHAT = false;
        public static bool LOG_ACADEMYCHAT = false;
        public static bool LOG_UNIONCHAT = false;
        public static bool LOG_GLOBALCHAT = false;

        public static bool PHBOT_LOCK = false;
        public static bool PM_TICKET = false;

        public static bool ITEM_LOCK = false;

        public static bool ITEM_LOCK_UP_STR = false;
        public static bool ITEM_LOCK_UP_INT = false;
        public static bool ITEM_LOCK_UP_SKILL = false;

        public static bool ITEM_LOCK_EXCHANGE = false;
        public static bool ITEM_LOCK_STALL = false;
        public static bool ITEM_LOCK_STORAGE = false;
        public static bool ITEM_LOCK_BUY_ITEM = false;
        public static bool ITEM_LOCK_SELL_ITEM = false;
        public static bool ITEM_LOCK_DROP_STUFF = false;

        public static bool ITEM_LOCK_GLOBAL = false;
        public static bool ITEM_LOCK_AVATAR = false;
        public static bool ITEM_LOCK_ALCHEMY = false;
        public static bool ITEM_LOCK_ALCHEMY_STONE = false;
        public static bool ITEM_LOCK_ALCHEMY_DISSAMBLE = false;

        public static bool ITEM_LOCK_GUILD_KICK = false;
        public static bool ITEM_LOCK_GUILD_DISBAND = false;
        public static bool ITEM_LOCK_GUILD_LEAVE = false;
        public static bool ITEM_LOCK_GUILD_DONATE_GP = false;
        public static bool ITEM_LOCK_GUILD_PROMOTE = false;
        public static bool ITEM_LOCK_GUILD_STORAGE = false;

        public static int ITEM_LOCK_MAX_FAIL = 3;

        public static bool WICKED_STALL_PRICE = false;
        public static bool ALEXUS_GOLD_COIN = false;

        public static bool FORTRESS_TRACE = false;
        public static bool FORTRESS_ZERK = false;
        public static bool FORTRESS_BERSERKPOT = false;
        public static bool FORTRESS_RESURRECTIONSCROLL = false;

        public static bool JOB_BERSERKPOT = false;

        public static bool PVP_ZERK = false;
        public static bool PVP_BERSERKPOT = false;

        public static bool FORTRESS_BLOCKED_SKILLS = false;
        public static bool PVP_BLOCKED_SKILLS = false;
        public static bool TOWN_BLOCKED_SKILLS = false;
        public static bool EVENT_BLOCKED_SKILLS = false;
        public static bool JOB_BLOCKED_SKILLS = false;
        public static bool BOT_BLOCKED_SKILLS = false;

        #endregion

        #region GENERAL SETTINGS
        public static List<string> GMs = new List<string>();
        public static List<string> badwords = new List<string>();
        #endregion
    }
}