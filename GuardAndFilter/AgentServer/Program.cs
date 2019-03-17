using Filter.NetEngine;
using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Filter
{
    class Program
    {
        #region Console pool thread
        static void ConsolePoolThread()
        {
            while (true)
            {
                string cmd = Console.ReadLine().ToLower();

                if (cmd == "/clear")
                {
                    Console.Clear();
                    Logger.WriteLine("/clear was successfull");
                }

                if(cmd == "/help")
                {
                    Console.Clear();
                    Logger.WriteLine(Logger.LogLevel.Debug, "Command list\n------------------------------------------------------------------------------------------\n" +
                        "/resetbot - Resets all detected bot users\n" +
                        "/reload - Reloads filter configuartion files\n" +
                        "/status - Shows information about filter (amount of exploits blocked, etc)\n" +
                        "/clear - Clears the console window\n" +
                        "/help - This message box, duh? :S\n" +
                        "------------------------------------------------------------------------------------------");
                }

                if (cmd == "/resetbot")
                {
                    FilterMain.Last_login.Clear();
                    FilterMain.BOT_LIST.Clear();
                    Logger.WriteLine("/resetbot was successfull");
                }

                if (cmd == "/reload")
                {
                    Config.LoadEverything();
                    Program.LoadOpcodes();
                    Program.LoadGMs();
                    Program.LoadBYPASS();
                    Program.LoadCAFE();
                    Program.Insult();
                    Program.LoadRegions();
                    Program.LoadFortress();
                    Program.LoadCities();
                    Program.LoadBlockedSkills();
                    Program.ZerkRegions();
                    Program.PartyRegions();
                    Program.EventRegions();
                    Program.BotRegions();
                    Program.EventBlockedSkills();
                    Program.FortressBlockedSkills();
                    Program.JobBlockedSkills();
                    Program.PvpBlockedSkills();
                    Program.BotBlockedSkills();
                    Program.LoadEventbot();
                    Logger.WriteLine("/reload was successfull");
                }

                if(cmd == "/status")
                {
                    Console.Clear();

                    Logger.WriteLine(Logger.LogLevel.Debug,
                        $"Protected since {FilterMain.protection_started}\n" +
                        $"------------------------------------------------------------------------------------------\n" +
                        $"Blocked DoS attacks = {FilterMain.blocked_agent_dosattacks}\n" +
                        $"Blocked DDoS attacks = {FilterMain.blocked_agent_mikeboty}\n" +
                        $"Blocked SQL injects = {FilterMain.blocked_agent_sqlinject}\n" +
                        $"Blocked exploits = {FilterMain.blocked_agent_exploits}\n" +
                        $"Blocked packet injects = {FilterMain.blocked_agent_injects}\n" +
                        $"Blocked packet floods = {FilterMain.blocked_agent_packetflood}\n" +
                        $"Blocked char_screen floods = {FilterMain.blocked_agent_packetflood_charscreen}\n" +
                        $"Amount of executed SQL querries = {FilterMain.total_sql_queries}\n" +
                        $"Total visited users = {FilterMain.total_visited_users}\n" +
                        $"Daily visited users = {FilterMain.daily_visited_users}\n" +
                        $"Botting players = {FilterMain.bots_found}\n" +
                        $"------------------------------------------------------------------------------------------"
                        );
                }

                if(cmd == "/settings")
                {

                }

                // Avoid 100% cpu usage.
                Thread.Sleep(1);
            }
        }
        #endregion

        #region Read OPCODES
        public static void LoadOpcodes()
        {
            FilterMain.BAD_Opcodes.Clear();
            try
            {
                FilterMain.BAD_Opcodes.Clear(); // Remove old
                // Check if badopcodes exists.
                if (File.Exists("config/exploit.txt"))
                {
                    foreach (string line in File.ReadAllLines("config/exploit.txt"))
                    {
                        // Read opcode
                        string[] split = line.Split(',');
                        string opcode = split[0].ToLower();
                        FilterMain.BAD_Opcodes.Add(opcode, split[1]);
                    }
                }
                else
                {
                    Logger.WriteLine(Logger.LogLevel.Warning, "Error loading exploit opcodes, you should close program!");
                }
            }
            catch
            {
                Logger.WriteLine(Logger.LogLevel.Warning, "Error loading opcodes, you should close program.");
            }
        }
        #endregion

        #region LoadGMs
        public static void LoadGMs()
        {
            FilterMain.GMs.Clear(); // Remove old

            foreach (string line in File.ReadAllLines("config/gmaccounts.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add ban
                    FilterMain.GMs.Add(line);
                }
            }
        }
        #endregion

        #region Load CAFE rules
        public static void LoadCAFE()
        {
            FilterMain.CAFE.Clear(); // Remove old

            foreach (string line in File.ReadAllLines("config/cafe.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add ban
                    FilterMain.CAFE.Add(line);
                }
            }
        }
        #endregion

        #region Load BYPASS rules
        public static void LoadBYPASS()
        {
            FilterMain.BYPASS.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("config/bypass.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add ban
                    FilterMain.BYPASS.Add(line);
                }
            }
        }
        #endregion

        #region Insultwords
        // Insult words
        public static void Insult()
        {
            FilterMain.badwords.Clear(); // Remove old

            // Check if the file exist
            foreach (string line in File.ReadAllLines("config/badwords.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add region
                    FilterMain.badwords.Add(line.ToLower());
                }
            }
        }
        #endregion

        #region Load all regions
        public static void LoadRegions()
        {
            FilterMain.town_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/towns.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add region
                    FilterMain.town_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load all regions
        public static void LoadFortress()
        {
            FilterMain.fortress_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/fortress.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add region
                    FilterMain.fortress_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load all cities
        public static void LoadCities()
        {
            FilterMain.city_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/cities.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add cities
                    FilterMain.city_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load all BlockedSkills
        public static void LoadBlockedSkills()
        {
            FilterMain.BlockedSkills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/BlockedSkills.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.BlockedSkills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.TOWN_BLOCKED_SKILLS = false;
            if (FilterMain.BlockedSkills.Count > 0)
            {
                FilterMain.TOWN_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load zerk regions
        public static void ZerkRegions()
        {
            FilterMain.zerk_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/zerk.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add cities
                    FilterMain.zerk_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load Party regions
        public static void PartyRegions()
        {
            FilterMain.party_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/party.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add cities
                    FilterMain.party_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load Bot regions
        public static void BotRegions()
        {
            FilterMain.bot_region_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/bot.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add cities
                    FilterMain.bot_region_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load Event regions
        public static void EventRegions()
        {
            FilterMain.event_region_list.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("regions/event.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add cities
                    FilterMain.event_region_list.Add(short.Parse(line));
                }
            }
        }
        #endregion

        #region Load Event blocked skills
        public static void EventBlockedSkills()
        {
            FilterMain.event_region_skills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/event.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.event_region_skills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.EVENT_BLOCKED_SKILLS = false;
            if (FilterMain.event_region_skills.Count > 0)
            {
                FilterMain.EVENT_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load Fortress blocked skills
        public static void FortressBlockedSkills()
        {
            FilterMain.fortress_region_skills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/fortress.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.fortress_region_skills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.FORTRESS_BLOCKED_SKILLS = false;
            if (FilterMain.fortress_region_skills.Count > 0)
            {
                FilterMain.FORTRESS_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load Job blocked skills
        public static void JobBlockedSkills()
        {
            FilterMain.job_region_skills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/job.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.job_region_skills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.JOB_BLOCKED_SKILLS = false;
            if (FilterMain.job_region_skills.Count > 0)
            {
                FilterMain.JOB_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load Pvp blocked skills
        public static void PvpBlockedSkills()
        {
            FilterMain.pvp_region_skills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/pvp.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.pvp_region_skills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.PVP_BLOCKED_SKILLS = false;
            if (FilterMain.pvp_region_skills.Count > 0)
            {
                FilterMain.PVP_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load BOT blocked skills
        public static void BotBlockedSkills()
        {
            FilterMain.bot_region_skills.Clear(); // Remove old
            foreach (string line in File.ReadAllLines("skills/bot.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add blocked skill
                    FilterMain.bot_region_skills.Add(Convert.ToInt32(line));
                }
            }

            FilterMain.BOT_BLOCKED_SKILLS = false;
            if (FilterMain.bot_region_skills.Count > 0)
            {
                FilterMain.BOT_BLOCKED_SKILLS = true;
            }
        }
        #endregion

        #region Load eventbot rules
        public static void LoadEventbot()
        {
            FilterMain.names.Clear();

            foreach (string line in File.ReadAllLines("config/eventbot.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    if (!FilterMain.names.Contains(line))
                    {
                        // Add ban
                        FilterMain.names.Add(line);
                    }
                }
            }
        }
        #endregion

        #region MD5 hash
        public static string MD5_Encode(String str_encode)
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str_encode));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        #endregion

        #region Base64 Encode
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        #endregion

        #region ModuelLogger
        static void ModuelCrashChecker()
        {
            while (true)
            {
                Thread.Sleep(5000);

                if (!File.Exists("config/opcodes.ini")) continue;
                iniFile cfg = new iniFile("config/opcodes.ini");

                foreach (KeyValuePair<ushort, Int64> item in FilterMain.ALL_Opcodes.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value))
                {
                    cfg.IniWriteValue("OPCODES", $"0x{item.Key.ToString("x4")}", $"{item.Value}[{DateTime.UtcNow}]");
                }
            }
        }
        #endregion

        #region Main string shit
        static void Main(string[] args)
        {
            Console.Title = "GiaFilter | " + System.Diagnostics.Process.GetCurrentProcess().ProcessName + " Priv Build {" + typeof(Program).Assembly.GetName().Version + "}";

            #region Load everything that is needed.
            Config.LoadEverything();
            Program.LoadOpcodes();
            Program.LoadGMs();
            Program.LoadBYPASS();
            Program.LoadCAFE();
            Program.Insult();
            Program.LoadRegions();
            Program.LoadFortress();
            Program.LoadCities();
            Program.LoadBlockedSkills();
            Program.ZerkRegions();
            Program.PartyRegions();
            Program.EventRegions();
            Program.BotRegions();
            Program.EventBlockedSkills();
            Program.FortressBlockedSkills();
            Program.JobBlockedSkills();
            Program.PvpBlockedSkills();
            Program.BotBlockedSkills();
            Program.LoadEventbot();
            #endregion

            #region Check server protection
            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                var currentProfiles = fwPolicy2.CurrentProfileTypes;

                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    int port = Convert.ToInt32(rule.LocalPorts);

                    if (!rule.LocalPorts.Contains(FilterMain.agent_mports.ToString()))
                    {
                        Logger.WriteLine(Logger.LogLevel.Warning, $"Your server is not protected, please close port {FilterMain.agent_mports} in Windows Firewall!");
                    }
                }
            }
            catch { }
            #endregion

            #region Start agent server
            AsyncServer AgentServer = new AsyncServer();
            AgentServer.Start(FilterMain.agent_listen, FilterMain.agent_fports, AsyncServer.E_ServerType.AgentServer);
            #endregion

            #region REVERSE ENGINEERING MASTER
            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_CurStatus] {FilterMain.agent_mports}"));
            #endregion

            #region Start threading
            new Thread(ConsolePoolThread).Start();
            new Thread(ModuelCrashChecker).Start();
            #endregion
        }
        #endregion
    }
}
