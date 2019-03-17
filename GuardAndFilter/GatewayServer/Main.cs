using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
#pragma warning disable

namespace Filter
{
    sealed class FilterMain
    {
        public static string language = "ENGLISH";
        public static bool PaidUser = false;
        public static bool FilterStarted = false;
        public static List<ushort> Server_opcodes = new List<ushort>();
        public static List<ushort> Server_whitelisted = new List<ushort>();

        //public static List<string> startup_list = new List<string>();

        public static Hashtable shard_list = new Hashtable();
        public static string shard_list_return(int shard_id, int return_type)
        {
            try
            {
                if (FilterMain.shard_list.ContainsKey(shard_id))
                {
                    if (return_type > 3) return "0";
                    string search_next1 = FilterMain.shard_list[shard_id].ToString();
                    string[] search_next = search_next1.Split(',');

                    /*
                        shard_list_return(Type):
                        0 = server_name,
                        1 = online players,
                        2 = max players,
                        3 = status
                    */
                    return search_next[return_type];
                }
                return "0";
            }
            catch { }
            return "0";
        }
        //public static Timer shard_list_timer = null;
        public static bool shard_list_backend = true;

        public static string DiscordSmileys(string message)
        {
            
            message = message.Replace(":)", ":smiley:");
            message = message.Replace(":(", ":frowning:");
            message = message.Replace(":D", ":smile:");
            message = message.Replace(";)", ":wink:");
            message = message.Replace(";(", ":cry:");
            message = message.Replace(":P", ":stuck_out_tongue:");
            message = message.Replace(":O", ":open_mouth:");
            message = message.Replace(":|", ":neutral_face:");
            message = message.Replace("(y)", ":thumbsup:");
            message = message.Replace("(l)", ":hearts:");
            message = message.Replace("(u)", ":broken_heart:");
            message = message.Replace("(Y)", ":thumbsup:");
            message = message.Replace("(L)", ":hearts:");
            message = message.Replace("(U)", ":broken_heart:");
            message = message.Replace(":@", ":rage:");
            message = message.Replace("$", ":heavy_dollar_sign:");
            message = message.Replace("(chernobyl)", ":chipmunk:");
            message = message.Replace("(heidy)", ":chipmunk:");
            message = message.Replace(":S", ":confused:");

            return message;
        }

        public static void DiscordWebHook(string message, string sender, string url)
        {
            if (!FilterMain.ENABLE_WEBHOOK) return;
            if (url == "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks") return;
            try
            {
                using (var client = new WebClient())
                {
                    var data = new NameValueCollection();

                    data["content"] = message;
                    data["username"] = sender;
                    data["avatar_url"] = FilterMain.DiscordWebHook_Avatar;
                    var response = client.UploadValues(url, "POST", data);
                }
            }
            catch
            {
                Logger.WriteLine(Logger.LogLevel.Debug, "Error sending DiscordWebHook()");
            }
        }

        public static bool debug_mike = false;

        public static List<string> names = new List<string>();
        public static Hashtable mac_list = new Hashtable();
        public static Hashtable mac_encryption = new Hashtable();

        #region DiscordWebHooks
        public static string DiscordWebHook_Avatar = "https://i.imgur.com/Xu9Stsu.jpg";
        public static string DiscordWebHook_Global = "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";
        public static string DiscordWebHook_Notice = "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";
        public static string DiscordWebHook_Unique = "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";
        //public static string DiscordWebHook_Exploit = "https://discordapp.com/api/webhooks/376369777825939477/Q-aBi7V5ZXCMYtqrswiPsCodw-znll0XV8vnnggO3KBtYzmSXSewE5VdI5tS0tFmH8Fq";
        public static string DiscordWebHook_Royale = "https://discordapp.com/api/webhooks/385613500879470603/ehQJWbey8ywP1T5PZDE1xzFDra2K368yosYoYTxEPF6cytD_9qGg-3tZ6ZUqnix7culT";


        public static bool BATTLE_ROYALE = false;
        public static List<string> BATTLE_ROYALE_BUGGED = new List<string>();

        public static bool ENABLE_WEBHOOK = false;
        public static bool LIMIT_WINNERS = false;

        public static string OPENTDB = "https://opentdb.com/api.php?amount=1";
        #endregion

        #region SQL settings *NEW*
        public static string sql_host = "(local)";
        public static string sql_user = "sa";
        public static string sql_pass = "KRYLFITLER";
        public static string DATABASE = "FILTER";
        public static string DATABASE_SHARD = "SRO_VT_SHARD";
        public static string DATABASE_ACC = "SRO_VT_ACCOUNT";
        public static string DATABASE_LOG = "SRO_VT_LOG";
        #endregion

        #region Event bot
        public static bool ENABLE_CLIENTLESS = false;
        public static string Username;
        public static string Password;
        public static byte ServerLocale = 22;
        public static int ServerVersion = 188;
        public static int ShardID = 64;
        public static string Charname = "EventBOT";
        #endregion

        #region Download settings
        public static string download_ip = "127.0.0.1";
        public static int download_port = 15881;

        #endregion

        #region Gateway settings
        /*
            All gateway settings here (:
        */
        public static string gateway_remote = "127.0.0.1";
        public static string gateway_local = "127.0.0.1";
        public static int gateway_fport = 15779;
        public static int gateway_mport = 1337;

        public static string gateway_captcha = string.Empty;
        public static bool gateway_blockstatus = false;

        public static bool gateway_botdetection = false;
        public static bool gateway_gmlogin = false;

        public static List<string> gateway_accounts = new List<string>();
        public static List<string> ip_list_g = new List<string>();

        public static int packet_count = 0;
        public static int packet_reset = 500;
        public static int FLOOD_COUNT = 30;
        public static int IP_LIMIT = 0;
        public static int PC_LIMIT = 0;
        public static int CAFE_LIMIT = 0;
        public static List<string> bypass = new List<string>();
        public static List<string> cafe = new List<string>();
        public static List<string> servers = new List<string>();
        public static DateTime last_sent = DateTime.Now;
        #endregion

        #region Agent settings *NEW*
        /*
            All agent settings here (:
        */
        public static int agent_count = 0;
        public static List<string> agent_remote = new List<string>();
        public static List<string> agent_listen = new List<string>();
        public static List<int> agent_fports = new List<int>();
        public static List<int> agent_mports = new List<int>(); // Moduel ports
        #endregion

        #region GENERAL SETTINGS
        public static List<string> GMs = new List<string>();
        public static bool GM_LOGIN_ONLY = false;
        public static bool REMOTE_STATS = true;

        public static bool BOT_CONTROLL = false;
        public static bool BOT_CONNECTION = true;
        #endregion

        // SUPPORTED FILES
        public static string FILES = "vSRO188";
        /*
            RULES:
            0 = default;
            1 = continue;
            2 = disconnect;
            3 = firewall block;
        */
        public static int RULE = 2;

        // List, Dictionaries here....
        // -------------------------------------------------------------------------------------- \\
        public static Hashtable BAD_Opcodes = new Hashtable();
        // -------------------------------------------------------------------------------------- \\

        // Exploit MaxBytes
        // -------------------------------------------------------------------------------------- \\
        public static int dMaxBytesPerSec_Gateway = 1000;
        public static int dMaxBytesPerSec_Agent = 1000;
        // -------------------------------------------------------------------------------------- \\
    }
}