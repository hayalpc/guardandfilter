using Filter.NetEngine;
using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
#pragma warning disable

namespace Filter
{
    class Program
    {
        public static List<Clientless> ClientlessList = new List<Clientless>();
        public static string ip;

        #region Console pool thread
        static void ConsolePoolThread()
        {
            while (true)
            {
                string cmd = Console.ReadLine().ToLower();

                if (cmd == "/clear")
                {
                    Console.Clear();
                }

                if (cmd == "/reload")
                {
                    Config.LoadEverything();
                    Program.LoadOpcodes();
                    Program.LoadGMs();
                    Program.Loadbypass();
                    Program.LoadCafe();
                    Program.LoadEventbot();
                    Logger.WriteLine("Everything was reloaded.");
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
                    //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading exploit opcodes, you should close program!");
                    Logger.WriteLine(Logger.LogLevel.Warning, "Error loading exploit opcodes, you should close program!");
                }
            }
            catch
            {
                //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Error loading exploit opcodes, you should close program.");
                Logger.WriteLine(Logger.LogLevel.Warning, "Error loading opcodes, you should close program.");
            }
        }
        #endregion

        #region LoadGMs
        public static void LoadGMs()
        {
            FilterMain.GMs.Clear();

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

        #region Load cafe rules
        public static void LoadCafe()
        {
            FilterMain.cafe.Clear();

            foreach (string line in File.ReadAllLines("config/cafe.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add ban
                    FilterMain.cafe.Add(line);
                }
            }
        }
        #endregion

        #region Load bypass rules
        public static void Loadbypass()
        {
            FilterMain.bypass.Clear();

            foreach (string line in File.ReadAllLines("config/bypass.txt"))
            {
                if (line != String.Empty && line != null)
                {
                    // Add ban
                    FilterMain.bypass.Add(line);
                }
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
                    // Add ban
                    FilterMain.names.Add(line);
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

        #region Shard list sender
        static void shard_list_sender()
        {
           
        }
        #endregion

        public static void Reload()
        {
            Config.LoadEverything();
            Program.LoadOpcodes();
            Program.LoadGMs();
            Program.Loadbypass();
            Program.LoadCafe();
            Program.LoadEventbot();
        }

        public static void CloseProgram()
        {
            Environment.Exit(0);
        }

        public static bool paid()
        {
            return FilterMain.PaidUser;
        }

        #region Main string shit
        static void Main(string[] args)
        {
            Console.Title = "GiaFilter | " + System.Diagnostics.Process.GetCurrentProcess().ProcessName + " Priv Build {" + typeof(Program).Assembly.GetName().Version + "}";

            #region License system
            /*try
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        ip = client.DownloadString("http://89.160.86.243/license/ip.php");
                    }
                }
                catch { FilterMain.PaidUser = false; }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        client.Headers.Add("user-agent", Program.MD5_Encode("KRYLFILTER_" + ip));
                        string value = client.DownloadString("http://89.160.86.243/license/index.php");

                        if (value == Program.MD5_Encode("KRYLFILTER_" + ip))
                        {
                            FilterMain.PaidUser = true;
                        }
                    }
                }
                catch { FilterMain.PaidUser = false; }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        client.Headers.Add("user-agent", Program.MD5_Encode("KRYLFILTER_" + ip));
                        string value = client.DownloadString("http://89.160.86.243/index.php");
                        if (value != "Kappa")
                        {
                            FilterMain.PaidUser = false;
                        }

                    }
                }
                catch { FilterMain.PaidUser = false; }
            }
            catch { FilterMain.PaidUser = false; }*/
            #endregion
            FilterMain.PaidUser = true;

            #region Load everything that is needed.
            Config.LoadEverything();
            Program.LoadOpcodes();
            Program.LoadGMs();
            Program.Loadbypass();
            Program.LoadCafe();
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

                    if (!rule.LocalPorts.Contains(FilterMain.gateway_mport.ToString()))
                    {
                        //FilterMain.startup_list.Add($"1[{DateTime.UtcNow}] Your server is not protected, please close port {FilterMain.gateway_mport} in Windows Firewall!");
                        Logger.WriteLine(Logger.LogLevel.Warning, $"Your server is not protected, please close port {FilterMain.gateway_mport} in Windows Firewall!");
                    }
                }
            }
            catch { }
            #endregion

            #region Shard list timer
            if (FilterMain.shard_list_backend)
            {
                new Thread(shard_list_sender).Start();
            }
            #endregion

            #region Start gateway server
            AsyncServer GatewayServer = new AsyncServer();
            GatewayServer.Start(FilterMain.gateway_local, FilterMain.gateway_fport, AsyncServer.E_ServerType.GatewayServer);
            #endregion

            #region Start clientless
            if (FilterMain.ENABLE_CLIENTLESS)
            {
                sqlCon.Read_ItemData();
                sqlCon.Read_Monsters();
                if(FilterMain.BATTLE_ROYALE)
                {
                    sqlCon.Read_Royale();
                }
                ClientlessList.Add(new Clientless(FilterMain.Username, FilterMain.Password, FilterMain.Charname));
            }
            #endregion

            #region Start threading
            new Thread(ConsolePoolThread).Start();
            #endregion
        }
        #endregion
    }
}
