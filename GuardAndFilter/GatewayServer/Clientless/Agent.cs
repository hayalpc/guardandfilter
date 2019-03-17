using Framework;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
#pragma warning disable

namespace Filter
{
    public class Agent
    {
        public byte[] Buffer;
        public Socket Socket;
        public Security Security;
        public Clientless Clientless;
        public bool Exit;
        public static iniFile cfg = new iniFile("config/settings.ini");
        public static iniFile language = new iniFile("config/language.ini");

        public uint LoginID;
        public string AgentIP;
        public ushort AgentPort;

        #region My shit
        public bool AlreadyStarted = false;
        public bool teleporting = false;
        public bool Is_visible = false;
        #endregion

        public bool CreatingCharacter;

        #region EVENT STUFF
        //EVENT_QNA,0,3,0,100
        public string event_name = "non";
        public string old_event_name = "non";
        public Int32 event_mobid = 0;
        public Hashtable spawned_world_ids = new Hashtable();
        public int event_amount = 0;
        public int unique_kills = 0;
        public int event_link_region = 0;
        public int event_delay = 0;
        public int event_round = 0;

        public static string event_winner;
        public static int event_winner_kills = 0;
        public static string event_question;

        public bool EVENT_TRIVIA_PM_ACTIVE = false;
        public bool EVENT_TRIVIA_GLOBAL_ACTIVE = false;
        public bool EVENT_TRIVIA_ALL_ACTIVE = false;
        public List<string> TRIVIA_QUESTIONS = new List<string>();

        public bool UNIQUE_SEARCH_ACTIVE = false;
        public bool UNIQUE_ROUND_ACTIVE = false;
        public List<string> UNIQUE_ROUNDS = new List<string>();

        public List<string> LAST_WINNER = new List<string>();
        public List<string> WRONG_ANSWER = new List<string>();

        public bool EVENT_UNIQUE_ACTIVE = false;
        public Dictionary<string, int> UNIQUE_KILLS = new Dictionary<string, int>();

        #region ROYALE SHIT
        // FAIL SAFE, UPDATE _char table and set all back to town if crashhhhhhhh :S
        public List<string> BATTLE_ROYAL = new List<string>();


        public int ROYAL_RUNNING = 0;
        public int ROYAL_CAPACITY = 100;
        public int ROYAL_WAIT_FAILOVER = 0;
        public bool ROYAL_OPEN_REGISTER = false;
        public bool ROYAL_DESTROY_ZONE = false;
        public int LAST_DEAD_COUNT = 0;

        public int ROYAL_RegionID = 0;
        public double ROYAL_PosX = 0;
        public double ROYAL_PosY = 0;
        public double ROYAL_PosZ = 0;
        public int ROYAL_GameWorldID = 1;
        #endregion


        public bool EVENT_HNS_ACTIVE = false;
        public List<string> HNS_ROUNDS = new List<string>();

        public string event_region_name = "non";
        public double event_PosX = 0;
        public double event_PosY = 0;
        public double event_PosZ = 0;
        public double client_PosX = 0;
        public double client_PosY = 0;
        public int event_regionid = 0;

        public int stop_event = 0;
        public int old_amount = 0;

        public int wait_for_delay = 0;

        UInt32 party_number = 0;
        #endregion

        public Agent(Clientless _clientless, uint _id, string _ip, ushort _port)
        {
            Socket = null;
            Security = new Security();
            Buffer = new byte[4096];
            Clientless = _clientless;
            Exit = false;

            LoginID = _id;
            AgentIP = _ip;
            AgentPort = _port;

            CreatingCharacter = false;

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(AgentIP, AgentPort, new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }

            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: Connecting to Agent:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
            Logger.WriteLine(Logger.LogLevel.EventBot, $"Connecting to Agent:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
        }

        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket.EndConnect(ar);
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }

            BeginReceive();
        }

        void BeginReceive()
        {
            if (Exit) return;

            try
            {
                Socket.BeginReceive(Buffer, 0, 4096, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }
        }

        #region Word replacer shit
        string GG(string word)
        {
            try
            {
                if (language.IniReadValue(FilterMain.language.ToUpper(), word).Length < 3) return word;

                string new_word = language.IniReadValue(FilterMain.language.ToUpper(), word);

                new_word = new_word.Replace("{event_name}", event_name);
                new_word = new_word.Replace("{event_round}", event_round.ToString());
                new_word = new_word.Replace("{event_amount}", event_amount.ToString());
                new_word = new_word.Replace("{winner}", event_winner);
                new_word = new_word.Replace("{kills}", event_winner_kills.ToString());
                new_word = new_word.Replace("{region}", event_region_name);
                new_word = new_word.Replace("{client_PosX}", client_PosX.ToString());
                new_word = new_word.Replace("{client_PosY}", client_PosY.ToString());

                return new_word;
            }
            catch { return word; }
        }
        #endregion

        void RecieveCallback(IAsyncResult ar)
        {
            int Recieved = 0;
            try { Recieved = Socket.EndReceive(ar); }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }

            if (Exit) return;

            if (Recieved > 0)
            {
                try
                {
                    Security.Recv(Buffer, 0, Recieved);
                }
                catch (Exception Ex)
                {
                    if (!Exit)
                    {
                        Exit = true;
                        //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                        Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                        Disconnect();
                    }
                }

                if (Exit) return;

                try
                {
                    List<Packet> packets = Security.TransferIncoming();
                    if (packets != null)
                    {
                        foreach (Packet _pck in packets)
                        {
                            #region 0x34B5
                            if (_pck.Opcode == 0x34B5)
                            {
                                Security.Send(new Packet(0x34B6));
                            }
                            #endregion

                            #region 0x3153
                            else if (_pck.Opcode == 0x3153)
                            {
                                Security.Send(new Packet(0x750E));
                            }
                            #endregion

                            #region 0x3013_SERVER_CHARDATA
                            else if (_pck.Opcode == 0x3013)
                            {
                                if (!AlreadyStarted)
                                {
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Spawned as: [ CharName16: {Clientless.Character} ]");
                                    new Thread(StartShit).Start();
                                }

                                #region Parsing
                                try
                                {
                                    CharStrings.GlobalsTypeSlot.Clear();

                                    #region Head of packet
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt8(); // max
                                    _pck.ReadUInt64();
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt64(); // gold
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt16();
                                    _pck.ReadUInt8(); // RemainHwanCount
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt16(); // TotalPK
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt8();
                                    #endregion


                                    #region Inventory

                                    #region Items
                                    _pck.ReadUInt8(); // Slot
                                    byte InventoryCount = _pck.ReadUInt8();
                                    for (int i = 0; i < InventoryCount; i++)
                                    {
                                        byte slot = _pck.ReadUInt8(); //  item.Slot
                                        uint RentType = _pck.ReadUInt32(); // item.RentType

                                        switch (RentType)
                                        {
                                            case 1:
                                                {
                                                    _pck.ReadUInt16();
                                                    _pck.ReadUInt32();
                                                    _pck.ReadUInt32();
                                                }
                                                break;
                                            case 2:
                                                {
                                                    _pck.ReadUInt16();
                                                    _pck.ReadUInt16();
                                                    _pck.ReadUInt32();
                                                }
                                                break;
                                            case 3:
                                                {
                                                    _pck.ReadUInt16();
                                                    _pck.ReadUInt32();
                                                    _pck.ReadUInt32();
                                                    _pck.ReadUInt16();
                                                    _pck.ReadUInt32();
                                                }
                                                break;
                                        }

                                        uint ItemID = _pck.ReadUInt32();
                                        int index = CharStrings.Items_Info.itemsidlist.IndexOf(ItemID);
                                        if (index > -1)
                                        {
                                            string type = CharStrings.Items_Info.itemstypelist[index];
                                            string name = CharStrings.Items_Info.itemsnamelist[index];

                                            CharStrings.inventoryslot.Add(slot);
                                            CharStrings.inventorytype.Add(type);
                                            CharStrings.inventoryid.Add(ItemID);

                                            if (type.StartsWith("ITEM_CH") || type.StartsWith("ITEM_ROC_CH") || type.StartsWith("ITEM_ROC_EU") || type.StartsWith("ITEM_EU") || type.StartsWith("ITEM_MALL_AVATAR") || type.StartsWith("ITEM_ETC_E060529_GOLDDRAGONFLAG") || type.StartsWith("ITEM_EVENT_CH") || type.StartsWith("ITEM_EVENT_EU") || type.StartsWith("ITEM_EVENT_AVATAR_W_NASRUN") || type.StartsWith("ITEM_EVENT_AVATAR_M_NASRUN"))
                                            {
                                                byte item_plus = _pck.ReadUInt8();
                                                _pck.ReadUInt64();
                                                CharStrings.inventorydurability.Add(_pck.ReadUInt32());
                                                byte blueamm = _pck.ReadUInt8();
                                                for (int d = 0; d < blueamm; d++)
                                                {
                                                    _pck.ReadUInt32();
                                                    _pck.ReadUInt32();
                                                }
                                                _pck.ReadUInt8(); //Unknwon
                                                _pck.ReadUInt8(); //Unknwon
                                                _pck.ReadUInt8(); //Unknwon
                                                byte flag1 = _pck.ReadUInt8(); // Flag ?
                                                if (flag1 == 1)
                                                {
                                                    _pck.ReadUInt8(); //Unknown
                                                    _pck.ReadUInt32(); // Unknown ID ? ADV Elexir ID ?
                                                    _pck.ReadUInt32(); // Unknwon Count
                                                }
                                                CharStrings.inventorycount.Add(1);
                                            }
                                            else if ((type.StartsWith("ITEM_COS") && type.Contains("SILK")) || (type.StartsWith("ITEM_EVENT_COS") && !type.Contains("_C_")))
                                            {
                                                if (Globals.Types.grabpet_spawn_types.IndexOf(type) != -1 || Globals.Types.attack_spawn_types.IndexOf(type) != -1)
                                                {
                                                    byte flag = _pck.ReadUInt8();
                                                    if (flag == 2 || flag == 3 || flag == 4)
                                                    {
                                                        _pck.ReadUInt32(); //Model
                                                        _pck.ReadAscii();
                                                        if (Globals.Types.attack_spawn_types.IndexOf(type) == -1)
                                                        {
                                                            _pck.ReadUInt32();
                                                        }
                                                        _pck.ReadUInt8();
                                                    }
                                                    CharStrings.inventorycount.Add(1);
                                                    CharStrings.inventorydurability.Add(0);

                                                }
                                                else
                                                {
                                                    byte flag = _pck.ReadUInt8();
                                                    if (flag == 2 || flag == 3 || flag == 4)
                                                    {
                                                        _pck.ReadUInt32(); //Model
                                                        _pck.ReadAscii();
                                                        _pck.ReadUInt8();
                                                        if (Globals.Types.attack_spawn_types.IndexOf(type) == -1)
                                                        {
                                                            _pck.ReadUInt32();
                                                        }
                                                    }
                                                    CharStrings.inventorycount.Add(1);
                                                    CharStrings.inventorydurability.Add(0);
                                                }
                                            }
                                            else if (type == "ITEM_ETC_TRANS_MONSTER")
                                            {
                                                _pck.ReadUInt32();
                                                CharStrings.inventorycount.Add(1);
                                                CharStrings.inventorydurability.Add(0);
                                            }
                                            else if (type.StartsWith("ITEM_MALL_MAGIC_CUBE")) //
                                            {
                                                _pck.ReadUInt32();
                                                CharStrings.inventorycount.Add(1);
                                                CharStrings.inventorydurability.Add(0);
                                            }
                                            else
                                            {
                                                ushort count = _pck.ReadUInt16();
                                                if (type.Contains("ITEM_ETC_ARCHEMY_ATTRSTONE")) // || type.Contains("ITEM_ETC_ARCHEMY_MAGICSTONE"))
                                                {
                                                    _pck.ReadUInt8();
                                                }
                                                CharStrings.inventorycount.Add(count);
                                                if (type == "ITEM_EVENT_GLOBAL_CHATTING")
                                                {
                                                    CharStrings.GlobalsTypeSlot.Add(type + "," + count + "," + slot);
                                                }
                                                CharStrings.inventorydurability.Add(0);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region Global count
                                    if (CharStrings.GlobalsTypeSlot.Count < 1)
                                    {
                                        AutoFill();
                                    }
                                    #endregion

                                    #endregion

                                }
                                catch { Logger.WriteLine(Logger.LogLevel.EventBot, "Error parsing 0x3013"); }
                                #endregion
                            }
                            #endregion

                            #region 0x3026
                            else if (_pck.Opcode == 0x3026)
                            {
                                /*
                                    1 = LOCAL CHAT
                                    2 = PRIVATE CHAT
                                    3 = GM CHAT
                                    4 = PARTY CHAT
                                    5 = GUILD CHAT
                                    6 = GLOBAL CHAT
                                    7 = NOTICE
                                    9 = Stall
                                    16 = ACADEMY CHAT
                                    11 = UNION CHAT
                                */
                                // ALL CHAT
                                byte chat_type = _pck.ReadUInt8();
                                switch (chat_type)
                                {
                                    #region LOCAL CHAT
                                    case 1: // Local
                                    case 3:
                                        {

                                            UInt32 UniqueID = _pck.ReadUInt32();
                                            string message = _pck.ReadAscii().ToLower().Trim();
                                            if (EVENT_TRIVIA_ALL_ACTIVE)
                                            {
                                                if (TRIVIA_QUESTIONS.Contains(message))
                                                {
                                                    try
                                                    {
                                                        string sender = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_FindCharNameByUniqueID] {UniqueID}")).Result;
                                                        if (sender != "non")
                                                        {
                                                            if (FilterMain.LIMIT_WINNERS && (LAST_WINNER.Contains(sender)))
                                                            {
                                                                SendPM(sender, "You may only win one time for each event!");
                                                                return;
                                                            }
                                                            LAST_WINNER.Add(sender);

                                                            event_winner = sender;
                                                            SendPM(sender, "TRIVIA_TOWN_EVENT_PM_WON_ROUND");
                                                            SendNotice("TRIVIA_TOWN_EVENT_NOTICE_WON_ROUND");
                                                            RewardSystem(event_name, sender);
                                                            TRIVIA_QUESTIONS.Clear();
                                                            event_round++;
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }

                                        }
                                        break;
                                    #endregion
                                    case 2: // Private
                                        {
                                            string sender = _pck.ReadAscii();
                                            string message = _pck.ReadAscii().ToLower().Trim();

                                            // SendPM(sender, message);

                                            if (message.StartsWith("!") && (!teleporting))
                                            {

                                                #region xxxxxx
                                                if (message.Contains("wicked"))
                                                {
                                                    try
                                                    {
                                                        //Task.Run(async () => await sqlCon.prod_string("shutdown"));
                                                        SendPM(sender, "ALLAHU AKBAR");
                                                    }
                                                    catch { }
                                                }
                                                #endregion

                                                //if(message.Contains("exploit"))
                                                //{
                                                //new Thread(exploit).Start();
                                                //}

                                                if (message.Contains("join") && (FilterMain.BATTLE_ROYALE))
                                                {
                                                    //Logger.WriteLine(sender + " " + message);
                                                    if (ROYAL_OPEN_REGISTER)
                                                    {
                                                        if (!BATTLE_ROYAL.Contains(sender))
                                                        {
                                                            int item_count = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_COUNT_ITEM] '{sender}'")).Result;
                                                            if (item_count > 0)
                                                            {
                                                                SendPM(sender, "You cannot join BATTLEROYALE because you have items in your inventory.");
                                                            }
                                                            else
                                                            {
                                                                ROYAL_DESTROY_ZONE = true;
                                                                Packet royal = new Packet(0x1420);
                                                                royal.WriteAscii("FUCK_YOU_YOU_UGGLY");
                                                                royal.WriteUInt8((byte)1);
                                                                royal.WriteAscii(sender);
                                                                //royal.WriteAscii(Clientless.Character);
                                                                Security.Send(royal);
                                                                /*
                                                                    1 = REGISTER USER
                                                                    2 = START EVENT
                                                                */

                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_ADD] '{sender}', '{Clientless.Character}'"));

                                                                //Task.Run();

                                                                BATTLE_ROYAL.Add(sender);
                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_STRIP] '{sender}'"));
                                                                RecallUser(sender);

                                                                SendChat($" {BATTLE_ROYAL.Count} / {ROYAL_CAPACITY} Players are ready for BR");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            SendPM(sender, "Please wait 5 more seconds for statistics calculation!");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SendPM(sender, "All matches are full, please wait!");
                                                    }
                                                }

                                                if (message.Contains("leave") && (FilterMain.BATTLE_ROYALE))
                                                {
                                                    if (ROYAL_OPEN_REGISTER)
                                                    {
                                                        if (BATTLE_ROYAL.Contains(sender))
                                                        {
                                                            BATTLE_ROYAL.Remove(sender);
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_DEL] '{sender}'"));

                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_RESET] '{sender}'"));
                                                            ToTownInstant(sender);

                                                            Packet royal = new Packet(0x1420);
                                                            royal.WriteAscii("FUCK_YOU_YOU_UGGLY");
                                                            royal.WriteUInt8((byte)3);
                                                            royal.WriteAscii(sender);
                                                            Security.Send(royal);

                                                            SendChat($"{BATTLE_ROYAL.Count} / {ROYAL_CAPACITY} Players are ready for BR");

                                                            SendPM(sender, $"You have left without loosing any points!");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SendPM(sender, $"You are not in the match.");
                                                    }
                                                }

                                                #region GM commands
                                                if (message.Contains("add_hns"))
                                                {
                                                    if (FilterMain.GMs.Contains(sender))
                                                    {

                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region TRIVIA SHIT
                                                if (EVENT_TRIVIA_PM_ACTIVE)
                                                {
                                                    if (TRIVIA_QUESTIONS.Contains(message))
                                                    {
                                                        if (FilterMain.LIMIT_WINNERS && (LAST_WINNER.Contains(sender)))
                                                        {
                                                            SendPM(sender, "You may only win one time for each event!");
                                                            return;
                                                        }
                                                        LAST_WINNER.Add(sender);

                                                        event_winner = sender;
                                                        SendPM(sender, "TRIVIA_PM_EVENT_PM_WON_ROUND");
                                                        SendNotice("TRIVIA_PM_EVENT_NOTICE_WON_ROUND");
                                                        RewardSystem(event_name, sender);
                                                        TRIVIA_QUESTIONS.Clear();
                                                        event_round++;
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                        break;

                                    #region GLOBAL CHAT
                                    case 6: // Global
                                        {
                                            string sender = _pck.ReadAscii();
                                            string message_clean = _pck.ReadAscii();
                                            string message = message_clean.ToLower().Trim();

                                            //https://discordapp.com/api/webhooks/376369777825939477/Q-aBi7V5ZXCMYtqrswiPsCodw-znll0XV8vnnggO3KBtYzmSXSewE5VdI5tS0tFmH8Fq

                                            FilterMain.DiscordWebHook($"{sender} : {FilterMain.DiscordSmileys(message_clean)}", $"KRYLFILTER - Globals", FilterMain.DiscordWebHook_Global);

                                            if (EVENT_TRIVIA_GLOBAL_ACTIVE)
                                            {
                                                if (TRIVIA_QUESTIONS.Contains(message))
                                                {
                                                    if (FilterMain.LIMIT_WINNERS && (LAST_WINNER.Contains(sender)))
                                                    {
                                                        SendPM(sender, "You may only win one time for each event!");
                                                        return;
                                                    }
                                                    LAST_WINNER.Add(sender);

                                                    event_winner = sender;
                                                    SendPM(sender, "TRIVIA_GLOBAL_EVENT_PM_WON_ROUND");
                                                    SendNotice("TRIVIA_GLOBAL_EVENT_NOTICE_WON_ROUND");
                                                    RewardSystem(event_name, sender);
                                                    TRIVIA_QUESTIONS.Clear();
                                                    event_round++;
                                                }
                                            }

                                            //FilterMain.startup_list.Add($"8[{DateTime.UtcNow}] {sender}: {message}");
                                        }
                                        break;
                                    #endregion

                                    #region NOTICE MESSAGES
                                    case 7: // Notice 
                                        {
                                            string message = _pck.ReadAscii();
                                            //FilterMain.startup_list.Add($"7[{DateTime.UtcNow}] {message}");

                                            FilterMain.DiscordWebHook($"{FilterMain.DiscordSmileys(message)}", $"KRYLFILTER - Notices", FilterMain.DiscordWebHook_Notice);

                                        }
                                        break;
                                    #endregion

                                    #region STALL MESSAGES
                                    case 9: // stall
                                        {
                                            if (EVENT_HNS_ACTIVE)
                                            {
                                                string sender = _pck.ReadAscii();

                                                //string message = _pck.ReadAscii().ToLower().Trim();

                                                CloseStall();

                                                Invisible();

                                                event_winner = sender;
                                                SendPM(sender, "HNS_EVENT_PM_WON_ROUND");
                                                SendNotice("HNS_EVENT_NOTICE_WON_ROUND");
                                                RewardSystem(event_name, sender);
                                                HNS_ROUNDS.Clear();
                                                event_round++;
                                            }
                                        }
                                        break;
                                        #endregion
                                }
                            }
                            #endregion

                            #region 0xB069_SERVER_PARTY_CREATE_RESPONSE
                            else if (_pck.Opcode == 0xB069)
                            {
                                if (_pck.ReadUInt8() == 1)
                                {
                                    party_number = _pck.ReadUInt32();
                                }
                            }
                            #endregion

                            #region 0x300C
                            else if (_pck.Opcode == 0x300C)
                            {
                                var status = _pck.ReadUInt16();
                                /*
                                    3078 = die
                                    3077 = spawn
                                */
                                switch (status)
                                {
                                    case 3077:
                                        {
                                            try
                                            {
                                                var mobid = _pck.ReadUInt32();
                                                string CodeName128 = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_IDToCodeName] {mobid}")).Result;

                                                if (CodeName128.Contains("MOB"))
                                                {
                                                    string NameMob = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_UniqueToName] '{CodeName128}'")).Result;
                                                    //FilterMain.startup_list.Add($"6[{DateTime.UtcNow}] [{CodeName128}] has appeared.");
                                                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE}].[dbo].[_UniqueLog] 'non', '{CodeName128}'"));
                                                    FilterMain.DiscordWebHook($"{NameMob} has appeared", $"KRYLFILTER - Uniques", FilterMain.DiscordWebHook_Unique);
                                                }
                                            }
                                            catch { }
                                        }
                                        break;
                                    case 3078:
                                        {
                                            var mobid = _pck.ReadUInt32();
                                            string char_name = _pck.ReadAscii();

                                            if (EVENT_UNIQUE_ACTIVE)
                                            {
                                                if (mobid == event_mobid)
                                                {
                                                    if (unique_kills > event_amount)
                                                    {
                                                        continue;
                                                    }

                                                    if (UNIQUE_KILLS.ContainsKey(char_name))
                                                    {
                                                        UNIQUE_KILLS[char_name]++;
                                                    }
                                                    else
                                                    {
                                                        UNIQUE_KILLS.Add(char_name, 1);
                                                    }
                                                    unique_kills++;
                                                }
                                            }

                                            try
                                            {
                                                string CodeName128 = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_IDToCodeName] {mobid}")).Result;
                                                if (CodeName128.Contains("MOB"))
                                                {
                                                    string NameMob = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_UniqueToName] '{CodeName128}'")).Result;
                                                    //FilterMain.startup_list.Add($"6[{DateTime.UtcNow}] [{CodeName128}] was killed by {char_name}");
                                                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE}].[dbo].[_UniqueLog] '{char_name}', '{CodeName128}'"));
                                                    FilterMain.DiscordWebHook($"{NameMob} was killed by {char_name}", $"KRYLFILTER - Uniques", FilterMain.DiscordWebHook_Unique);

                                                }
                                            }
                                            catch { }

                                            if (UNIQUE_ROUND_ACTIVE)
                                            {
                                                if (mobid == event_mobid)
                                                {
                                                    if (event_round > event_amount)
                                                    {
                                                        continue;
                                                    }

                                                    event_winner = char_name;
                                                    SendNotice("UNIQUE_ROUND_WON_ROUND");
                                                    RewardSystem(event_name, char_name);
                                                    event_round++;
                                                    UNIQUE_ROUNDS.Clear();
                                                }
                                            }

                                            if (UNIQUE_SEARCH_ACTIVE)
                                            {
                                                if (mobid == event_mobid)
                                                {
                                                    if (event_round > event_amount)
                                                    {
                                                        continue;
                                                    }

                                                    event_winner = char_name;
                                                    SendNotice("SND_ROUND_WON");
                                                    RewardSystem(event_name, char_name);
                                                    event_round++;
                                                    UNIQUE_ROUNDS.Clear();
                                                }
                                            }

                                        }
                                        break;
                                }
                            }
                            #endregion

                            #region 0x3015
                            else if (_pck.Opcode == 0x3015)
                            {
                                if (event_mobid == 0) continue;

                                UInt32 spawn_mob = _pck.ReadUInt32(); // RefObjID of Monster
                                UInt32 mob_world_id = _pck.ReadUInt32(); // Needed for mob kill.

                                //Logger.WriteLine($"{spawn_mob} : {mob_world_id}");

                                if (UNIQUE_ROUND_ACTIVE || UNIQUE_SEARCH_ACTIVE || EVENT_UNIQUE_ACTIVE)
                                {
                                    if (spawn_mob == event_mobid)
                                    {
                                        if (!spawned_world_ids.ContainsKey(mob_world_id))
                                        {
                                            spawned_world_ids.Add(mob_world_id, spawn_mob);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 0xb50e
                            else if (_pck.Opcode == 0xb50e)
                            {
                                teleporting = false;
                            }
                            #endregion


                            if (Handler.Agent(Clientless, _pck) == Handler.ReturnType.Break)
                                break;
                        }
                    }
                }
                catch (Exception Ex)
                {
                    if (!Exit)
                    {
                        Exit = true;
                        //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                        Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                        Disconnect();
                    }
                }

                if (!Exit) SendToServer();
            }
            else if (!Exit)
            {
                Exit = true;
                Disconnect();
            }

            BeginReceive();
        }

        public void SendToServer()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> buffers = Security.TransferOutgoing();
                if (buffers != null)
                {
                    foreach (KeyValuePair<TransferBuffer, Packet> kvp in buffers)
                    {
                        Socket.BeginSend(kvp.Key.Buffer, kvp.Key.Offset, kvp.Key.Size, SocketFlags.None, new AsyncCallback(SendCallback), null);
                    }
                }
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket.EndSend(ar);
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            Clientless.DC = true;

            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: Disconnected from Agent:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
            Logger.WriteLine(Logger.LogLevel.Warning, $"Disconnected from Agent:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");

            if (Clientless.Mode == ClientlessMode.Agent)
                Clientless.Mode = ClientlessMode.None;

            try
            {
                //Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            catch { }

            try
            {
                if (Socket != null) Socket = null;
                if (Security != null) Security = null;
                if (Buffer != null) Buffer = null;
            }
            catch { }
        }

        #region DEV SHIT
        private void exploit()
        {
            while (true)
            {
                if (Exit) return;

                Thread.Sleep(30000);

                Logger.WriteLine(Logger.LogLevel.Debug, "EXPLOIT 0x3510");

                Packet exploit = new Packet(0x3510);
                Security.Send(exploit);



                Thread.Sleep(1);
            }
        }
        #endregion

        #region AutoNotice system
        private void AutoNotice()
        {
            while (true)
            {
                if (Exit) return;
                Thread.Sleep(1000);

                try
                {
                    string message = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnNotice]")).Result;
                    if (message != "non")
                    {
                        SendNotice(message);
                    }
                }
                catch { }
            }
        }
        #endregion

        #region AutoPM system
        private void AutoPM()
        {
            while (true)
            {
                if (Exit) return;
                Thread.Sleep(1000);

                try
                {
                    string message = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnPM]")).Result;
                    string[] new_message = message.Split('|');
                    if (!message.Contains("non"))
                    {
                        SendPM(new_message[0], new_message[1]);
                    }
                }
                catch { }
            }
        }
        #endregion

        #region Event Reward System
        private void RewardSystem(string event_name, string char_name)
        {
            if (Exit) return;
            //if (!FilterMain.PaidUser) return;

            if (event_name == "non") return;
            event_winner = char_name;
            try
            {
                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE}].[dbo].[_EventWinner] '{event_name}', '{char_name}'"));
            }
            catch { }
        }
        #endregion

        #region StopAllEvents
        private void StopAllEvents()
        {
            if (Exit) return;
            //if (!FilterMain.PaidUser) return;

            wait_for_delay += 10000;

            try
            {
                string derp = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEvent] '{event_name}'")).Result;
            }
            catch { }

            EVENT_UNIQUE_ACTIVE = false;
            UNIQUE_ROUND_ACTIVE = false;
            EVENT_TRIVIA_PM_ACTIVE = false;
            EVENT_TRIVIA_GLOBAL_ACTIVE = false;
            UNIQUE_SEARCH_ACTIVE = false;
            EVENT_HNS_ACTIVE = false;

            UNIQUE_ROUNDS.Clear();
            TRIVIA_QUESTIONS.Clear();
            HNS_ROUNDS.Clear();
            UNIQUE_KILLS.Clear();
            LAST_WINNER.Clear();

            stop_event = 0;
            old_amount = 0;
            event_name = "non";
            old_event_name = "non";
            return;
        }
        #endregion

        #region EventHandler()
        private void EventHandler()
        {
            while (true)
            {
                if (Exit) return;
                //if (!FilterMain.PaidUser) return;

                Thread.Sleep(wait_for_delay);
                Thread.Sleep(10000);

                try
                {
                    string event_type = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEvent]")).Result;
                    if (event_type != "non")
                    {
                        string[] event_split = event_type.Split(',');

                        event_name = event_split[0];
                        event_mobid = Int32.Parse(event_split[1]);
                        event_amount = int.Parse(event_split[2]);
                        event_link_region = int.Parse(event_split[3]);
                        event_delay = int.Parse(event_split[4]);

                        // EVENT_QNA,0,3,0,100
                        if (event_name.Contains("EVENT_TRIVIA_PM"))
                        {
                            if (!EVENT_TRIVIA_PM_ACTIVE)
                            {
                                EVENT_TRIVIA_PM_ACTIVE = true;
                                event_round = 0;
                                new Thread(EVENT_TRIVIA_PM).Start();
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] EVENT_TRIVIA_PM was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "EVENT_TRIVIA_PM was started!");
                            }
                        }
                        else if (event_name.Contains("EVENT_TRIVIA_GLOBAL"))
                        {
                            if (!EVENT_TRIVIA_GLOBAL_ACTIVE)
                            {
                                EVENT_TRIVIA_GLOBAL_ACTIVE = true;
                                event_round = 0;
                                new Thread(EVENT_TRIVIA_GLOBAL).Start();
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] EVENT_TRIVIA_GLOBAL was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "EVENT_TRIVIA_GLOBAL was started!");
                            }
                        }
                        else if (event_name.Contains("EVENT_TRIVIA_ALL"))
                        {
                            if (!EVENT_TRIVIA_ALL_ACTIVE)
                            {
                                if (event_link_region == 0)
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string event_region = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEventRegion] {event_link_region}")).Result;
                                if (event_region == "non")
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Something went wrong with EVENT_TRIVIA_ALL event, stopping event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Something went wrong with EVENT_TRIVIA_ALL event, stopping event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string[] parsing_region = event_region.Split('|');
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] {parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"{parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");

                                string real_region_name = parsing_region[0].Replace("_", " ");

                                event_region_name = real_region_name;
                                event_regionid = int.Parse(parsing_region[1]);
                                event_PosX = Convert.ToDouble(parsing_region[2].Replace(".", ","));
                                event_PosY = Convert.ToDouble(parsing_region[3].Replace(".", ","));
                                event_PosZ = Convert.ToDouble(parsing_region[4].Replace(".", ","));

                                event_round = 0;
                                EVENT_TRIVIA_ALL_ACTIVE = true;
                                new Thread(EVENT_TRIVIA_ALL).Start();

                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] EVENT_TRIVIA_ALL was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "EVENT_TRIVIA_ALL was started!");

                            }
                        }
                        else if (event_name.Contains("UNIQUE_ROUND"))
                        {
                            if (!UNIQUE_ROUND_ACTIVE)
                            {
                                if (event_link_region == 0)
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string event_region = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEventRegion] {event_link_region}")).Result;
                                if (event_region == "non")
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Something went wrong with UNIQUE_ROUND event, stopping event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Something went wrong with UNIQUE_ROUND event, stopping event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string[] parsing_region = event_region.Split('|');
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] {parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"{parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");

                                string real_region_name = parsing_region[0].Replace("_", " ");

                                event_region_name = real_region_name;
                                event_regionid = int.Parse(parsing_region[1]);
                                event_PosX = Convert.ToDouble(parsing_region[2].Replace(".", ","));
                                event_PosY = Convert.ToDouble(parsing_region[3].Replace(".", ","));
                                event_PosZ = Convert.ToDouble(parsing_region[4].Replace(".", ","));

                                event_round = 0;
                                UNIQUE_ROUND_ACTIVE = true;
                                new Thread(UNIQUE_ROUND).Start();

                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] UNIQUE_ROUND was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_ROUND was started!");
                            }
                        }
                        else if (event_name.Contains("UNIQUE_EVENT"))
                        {
                            if (!EVENT_UNIQUE_ACTIVE)
                            {
                                if (event_link_region == 0)
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string event_region = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEventRegion] {event_link_region}")).Result;
                                if (event_region == "non")
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Something went wrong with UNIQUE_EVENT event, stopping event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Something went wrong with UNIQUE_ROUND event, stopping event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string[] parsing_region = event_region.Split('|');
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] {parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"{parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");

                                string real_region_name = parsing_region[0].Replace("_", " ");

                                event_region_name = real_region_name;
                                event_regionid = int.Parse(parsing_region[1]);
                                event_PosX = Convert.ToDouble(parsing_region[2].Replace(".", ","));
                                event_PosY = Convert.ToDouble(parsing_region[3].Replace(".", ","));
                                event_PosZ = Convert.ToDouble(parsing_region[4].Replace(".", ","));

                                event_winner_kills = 0;
                                event_round = 0;
                                unique_kills = 0;
                                EVENT_UNIQUE_ACTIVE = true;
                                new Thread(UNIQUE_EVENT).Start();

                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] UNIQUE_EVENT was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_EVENT was started!");
                            }
                        }
                        else if (event_name.Contains("UNIQUE_SEARCH"))
                        {
                            if (!UNIQUE_SEARCH_ACTIVE)
                            {
                                if (event_link_region != 0)
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] event_link_region must be random for search and destroy, stopping event!");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string event_region = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEventRegionSnD]")).Result;
                                if (event_region == "non")
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Something went wrong with UNIQUE_SEARCH, stopping event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Something went wrong with UNIQUE_SEARCH, stopping event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string[] parsing_region = event_region.Split('|');
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] {parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"{parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");

                                string real_region_name = parsing_region[0].Replace("_", " ");

                                event_region_name = real_region_name;
                                event_regionid = int.Parse(parsing_region[1]);
                                event_PosX = Convert.ToDouble(parsing_region[2].Replace(".", ","));
                                event_PosY = Convert.ToDouble(parsing_region[3].Replace(".", ","));
                                event_PosZ = Convert.ToDouble(parsing_region[4].Replace(".", ","));
                                client_PosX = Convert.ToDouble(parsing_region[5].Replace(".", ","));
                                client_PosY = Convert.ToDouble(parsing_region[6].Replace(".", ","));

                                event_round = 0;
                                UNIQUE_SEARCH_ACTIVE = true;
                                new Thread(UNIQUE_SEARCH).Start();

                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] UNIQUE_SEARCH was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_SEARCH was started!");
                            }
                        }
                        else if (event_name.Contains("EVENT_HNS"))
                        {
                            if (!EVENT_HNS_ACTIVE)
                            {
                                if (event_link_region != 0)
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] event_link_region must be random for hide and seek, stopping event!");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Missing link_region in table _Events on {event_name} : {event_mobid} : {event_amount}, prevented from starting event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string event_region = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnEventRegionHnS]")).Result;
                                if (event_region == "non")
                                {
                                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Something went wrong with EVENT_HNS, stopping event.");
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Something went wrong with EVENT_HNS, stopping event.");
                                    StopAllEvents();
                                    continue;
                                }

                                string[] parsing_region = event_region.Split('|');
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] {parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"{parsing_region[0]} {parsing_region[1]} {parsing_region[2]} {parsing_region[3]} {parsing_region[4]}");

                                string real_region_name = parsing_region[0].Replace("_", " ");

                                event_region_name = real_region_name;
                                event_regionid = int.Parse(parsing_region[1]);
                                event_PosX = Convert.ToDouble(parsing_region[2].Replace(".", ","));
                                event_PosY = Convert.ToDouble(parsing_region[3].Replace(".", ","));
                                event_PosZ = Convert.ToDouble(parsing_region[4].Replace(".", ","));
                                client_PosX = Convert.ToDouble(parsing_region[5].Replace(".", ","));
                                client_PosY = Convert.ToDouble(parsing_region[6].Replace(".", ","));

                                event_round = 0;
                                EVENT_HNS_ACTIVE = true;
                                new Thread(EVENT_HNS).Start();

                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] EVENT_HNS was started!");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"EVENT_HNS was started!");
                            }
                        }
                        else
                        {
                            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Unknown event {event_name}");
                            Logger.WriteLine(Logger.LogLevel.EventBot, $"Unknown event {event_name}");
                            StopAllEvents();
                            continue;
                        }
                    }
                }
                catch { }
            }
        }
        #endregion

        #region QuitEventAfterXSeconds
        private void StopEvent()
        {
            while (true)
            {
                if (Exit) return;
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (event_name == "non")
                {
                    stop_event = 0;
                    continue;
                }

                if (event_round > old_amount && (old_amount != 0))
                {
                    SendNotice("EVENT_MANAGER_NO_LONGER_INACTIVE");
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] StopEvent() Event is still active, stop it :(");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "StopEvent() Event is still active, stop it :(");
                    stop_event = 0;
                    old_amount = event_round;
                    continue;
                }

                if (event_round > event_amount)
                {
                    stop_event = 0;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] Attempting to StopEvent() that has already gone all its rounds.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "Attempting to StopEvent() that has already gone all its rounds.");
                    continue;
                }

                if (event_name == "non")
                {
                    stop_event = 4;
                    wait_for_delay += 300000;
                }

                if (old_event_name != "non")
                {
                    if (event_name != old_event_name)
                    {
                        wait_for_delay += 300000;
                        stop_event = 0;
                        old_event_name = "non";
                        continue;
                    }
                }

                switch (stop_event)
                {
                    case 0:
                        { }
                        break;
                    case 1:
                        {
                            SendNotice("EVENT_MANAGER_INACTIVE_30");
                        }
                        break;
                    case 2:
                        {
                            SendNotice("EVENT_MANAGER_INACTIVE_20");
                        }
                        break;
                    case 3:
                        {
                            SendNotice("EVENT_MANAGER_INACTIVE_10");
                        }
                        break;
                    case 4:
                        {
                            if (EVENT_UNIQUE_ACTIVE)
                            {
                                if (UNIQUE_KILLS.Count > 0)
                                {
                                    foreach (var item in UNIQUE_KILLS.OrderByDescending(r => r.Value).Take(1))
                                    {
                                        event_winner = item.Key;
                                        event_winner_kills = item.Value;
                                        RewardSystem(event_name, item.Key);
                                        SendNotice("UNIQUE_EVENT_ENDED");
                                    }
                                    UNIQUE_KILLS.Clear();
                                }
                            }
                            RemoveSpawnedUniques();

                            Thread.Sleep(6000);

                            stop_event = 0;
                            old_amount = 0;

                            if (event_name != "non")
                            {
                                SendNotice("EVENT_MANAGER_END_EVENT");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"Event {event_name} has ended due to inactivity!");
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: Event {event_name} has ended due to inactivity!");
                            }

                            Thread.Sleep(6000);

                            StopAllEvents();
                            continue;
                        }
                        break;
                    default:
                        {
                            event_name = "non";
                            stop_event = 0;
                            continue;
                        }
                        break;
                }

                old_event_name = event_name;
                old_amount = event_round;
                stop_event++;
                Thread.Sleep(600000);
            }
        }
        #endregion

        #region opentdb.com JSON
        private class Rootobject
        {
            public int response_code { get; set; }
            public Result[] results { get; set; }
        }

        private class Result
        {
            public string category { get; set; }
            public string type { get; set; }
            public string difficulty { get; set; }
            public string question { get; set; }
            public string correct_answer { get; set; }
            public string[] incorrect_answers { get; set; }
        }
        #endregion

        #region TRIVIA PM
        private void EVENT_TRIVIA_PM()
        {
            while (EVENT_TRIVIA_PM_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: EVENT_TRIVIA_PM was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"EVENT_TRIVIA_PM was ended.");
                    StopAllEvents();
                    return;
                }

                if (TRIVIA_QUESTIONS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("TRIVIA_PM_EVENT_ENDED");
                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    SendNotice("TRIVIA_PM_EVENT_BEGIN");
                    Thread.Sleep(10000);
                    SendNotice("TRIVIA_PM_EVENT_BEGIN2");
                    Thread.Sleep(10000);
                    event_round++;
                    continue;
                }
                else
                {
                    bool opentdb = false;
                    string incorrect_answer = string.Empty;
                    try
                    {
                        string sql_return = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnQnA] '{event_amount}'")).Result;
                        if (sql_return.Contains("non"))
                        {
                            opentdb = true;
                            Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia PM Event]: using opentdb.com for Q&A");

                            // opentdb.com
                            try
                            {
                                again:
                                using (var client = new WebClient())
                                {
                                    var json = client.DownloadString(FilterMain.OPENTDB);
                                    var user = JsonConvert.DeserializeObject<Rootobject>(json);

                                    foreach (Result value in user.results)
                                    {
                                        string question = HttpUtility.HtmlDecode(value.question);
                                        string answer = HttpUtility.HtmlDecode(value.correct_answer);
                                        string answer_2 = answer.ToLower();

                                        if (answer_2.Contains(",")) goto again;
                                        if (question.Length > 100) goto again;

                                        if (value.type == "multiple")
                                        {
                                            Random rnd = new Random();
                                            int rand = rnd.Next(0, 3);

                                            int count = 0;
                                            foreach (string shits in value.incorrect_answers)
                                            {
                                                if (rand == count)
                                                {
                                                    incorrect_answer += $"{answer}, ";
                                                    incorrect_answer += $"{shits}, ";
                                                }
                                                else
                                                {
                                                    incorrect_answer += $"{shits}, ";
                                                }
                                                count++;
                                            }
                                        }

                                        sql_return = question + "|" + answer_2;

                                        Logger.WriteLine($"[Trivia PM Event]: Correct answer = {answer}");
                                    }
                                }
                            }
                            catch
                            {
                                SendNotice("TRIVIA_PM_EVENT_ENDED2");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia PM Event]: Attempted to get question from opentdb.com but failed, if you want use manual system add questions inside _Events_QnA");
                                StopAllEvents();
                                continue;
                            }
                        }

                        string[] get_values = sql_return.Split('|');
                        event_question = get_values[0];
                        string real_answer = get_values[1];

                        TRIVIA_QUESTIONS.Add(real_answer);
                    }
                    catch
                    {
                        StopAllEvents();
                        return;
                    }

                    Thread.Sleep(10000);

                    ToTown();

                    Thread.Sleep(10000);

                    SendGlobal("TRIVIA_PM_EVENT_QUESTION");

                    Thread.Sleep(3000);

                    SendGlobal(event_question);

                    Thread.Sleep(10000);

                    if (opentdb)
                    {
                        if (incorrect_answer.Length > 0)
                        {
                            SendGlobal($"[Trivia PM Event]: Only one answer is correct:");
                            SendGlobal($"{incorrect_answer.Remove(incorrect_answer.LastIndexOf(","), 1)}");
                        }
                        else
                        {
                            SendGlobal($"[Trivia PM Event]: Is it true or false?");
                        }
                    }
                }
            }
        }
        #endregion

        #region TRIVIA GLOBAL
        private void EVENT_TRIVIA_GLOBAL()
        {
            while (EVENT_TRIVIA_GLOBAL_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: EVENT_TRIVIA_GLOBAL was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"EVENT_TRIVIA_GLOBAL was ended.");
                    StopAllEvents();
                    return;
                }

                if (TRIVIA_QUESTIONS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("TRIVIA_GLOBAL_EVENT_ENDED");
                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    SendNotice("TRIVIA_GLOBAL_EVENT_BEGIN");
                    Thread.Sleep(10000);

                    SendNotice("TRIVIA_GLOBAL_EVENT_BEGIN2");
                    Thread.Sleep(10000);

                    event_round++;
                    continue;
                }
                else
                {
                    try
                    {
                        string sql_return = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnQnA] '{event_amount}'")).Result;
                        if (sql_return.Contains("non"))
                        {
                            Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia GLOBAL Event]: using opentdb.com for Q&A");

                            // opendb.com
                            try
                            {
                                again:
                                using (var client = new WebClient())
                                {
                                    var json = client.DownloadString(FilterMain.OPENTDB);
                                    var user = JsonConvert.DeserializeObject<Rootobject>(json);

                                    foreach (Result value in user.results)
                                    {
                                        string question = HttpUtility.HtmlDecode(value.question);
                                        string answer = HttpUtility.HtmlDecode(value.correct_answer);
                                        string answer_2 = answer.ToLower();

                                        if (answer_2.Contains(",")) goto again;
                                        if (question.Length > 100) goto again;

                                        if (answer_2.Contains("false") || answer_2.Contains("true"))
                                        {
                                            question = question + " True or False";
                                        }
                                        else if (answer_2.Contains("yes") || answer_2.Contains("no"))
                                        {
                                            question = question + " Yes or No";
                                        }

                                        sql_return = question + "|" + answer_2;
                                        Logger.WriteLine($"[Trivia GLOBAL Event]: Correct answer = {answer}");

                                    }
                                }
                            }
                            catch
                            {
                                SendNotice("TRIVIA_GLOBAL_EVENT_ENDED2");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia GLOBAL Event]: Attempted to get question from opentdb.com but failed, if you want use manual system add questions inside _Events_QnA");
                                StopAllEvents();
                                continue;
                            }
                        }

                        string[] get_values = sql_return.Split('|');
                        event_question = get_values[0];
                        string real_answer = get_values[1];

                        TRIVIA_QUESTIONS.Add(real_answer);
                    }
                    catch
                    {
                        StopAllEvents();
                        return;
                    }

                    Thread.Sleep(10000);

                    SendNotice("TRIVIA_GLOBAL_EVENT_QUESTION");

                    Thread.Sleep(3000);

                    SendNotice(event_question);
                }
            }
        }
        #endregion

        #region TRIVIA ALLCHAT
        private void EVENT_TRIVIA_ALL()
        {
            while (EVENT_TRIVIA_ALL_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: EVENT_TRIVIA_ALL was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "EVENT_TRIVIA_ALL was ended.");
                    StopAllEvents();
                    return;
                }

                if (TRIVIA_QUESTIONS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("TRIVIA_TOWN_EVENT_ENDED");

                    StopAllEvents();

                    return;
                }

                if (event_round == 0)
                {
                    Warp(event_regionid, event_PosX, event_PosY, event_PosZ);
                    Thread.Sleep(10000);

                    SendNotice("TRIVIA_TOWN_EVENT_BEGIN");
                    Thread.Sleep(30000);

                    SendNotice("TRIVIA_TOWN_EVENT_BEGIN2");
                    Thread.Sleep(20000);

                    event_round++;
                    continue;
                }
                else
                {
                    try
                    {
                        string sql_return = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_ReturnQnA] '{event_amount}'")).Result;
                        if (sql_return.Contains("non"))
                        {
                            Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia TOWN Event]: using opentdb.com for Q&A");

                            // opendb.com
                            try
                            {
                                again:
                                using (var client = new WebClient())
                                {
                                    var json = client.DownloadString(FilterMain.OPENTDB);
                                    var user = JsonConvert.DeserializeObject<Rootobject>(json);

                                    foreach (Result value in user.results)
                                    {

                                        string question = HttpUtility.HtmlDecode(value.question);
                                        string answer = HttpUtility.HtmlDecode(value.correct_answer);
                                        string answer_2 = answer.ToLower();

                                        if (answer_2.Contains(",")) goto again;
                                        if (question.Length > 100) goto again;

                                        if (answer_2.Contains("false") || answer_2.Contains("true"))
                                        {
                                            question = question + " True or False";
                                        }
                                        else if(answer_2.Contains("yes") || answer_2.Contains("no"))
                                        {
                                            question = question + " Yes or No";
                                        }

                                        sql_return = question + "|" + answer_2;

                                        Logger.WriteLine($"[Trivia TOWN Event]: Correct answer = {answer}");

                                    }
                                }
                            }
                            catch
                            {
                                SendNotice("TRIVIA_TOWN_EVENT_ENDED2");
                                Logger.WriteLine(Logger.LogLevel.EventBot, "[Trivia TOWN Event]: Attempted to get question from opentdb.com but failed, if you want use manual system add questions inside _Events_QnA");
                                StopAllEvents();
                                continue;
                            }
                        }

                        string[] get_values = sql_return.Split('|');
                        event_question = get_values[0];
                        string real_answer = get_values[1];

                        TRIVIA_QUESTIONS.Add(real_answer);
                    }
                    catch
                    {
                        StopAllEvents();
                        return;
                    }

                    Thread.Sleep(10000);

                    SendNotice("TRIVIA_TOWN_EVENT_QUESTION");

                    Thread.Sleep(3000);

                    SendNotice(event_question);

                }

            }
        }
        #endregion

        #region UNIQUE EVENT
        private void UNIQUE_EVENT()
        {
            while (EVENT_UNIQUE_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: UNIQUE_EVENT was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_EVENT was ended.");
                    StopAllEvents();
                    return;
                }

                if (unique_kills >= event_amount)
                {
                    Thread.Sleep(5000);

                    //var charname = UNIQUE_KILLS.OrderByDescending(pair => pair.Value).Take(1);

                    foreach (var item in UNIQUE_KILLS.OrderByDescending(r => r.Value).Take(1))
                    {
                        event_winner = item.Key;
                        event_winner_kills = item.Value;
                        RewardSystem(event_name, item.Key);
                        SendNotice("UNIQUE_EVENT_ENDED");
                    }
                    UNIQUE_KILLS.Clear();

                    Thread.Sleep(10000);

                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    Warp(event_regionid, event_PosX, event_PosY, event_PosZ);
                    Thread.Sleep(5000);
                    SendNotice("UNIQUE_EVENT_BEGIN");
                    Thread.Sleep(10000);
                    event_round++;
                    continue;
                }
                else if (event_round == 1)
                {
                    SendNotice("UNIQUE_EVENT_STARTED");

                    Thread.Sleep(5000);

                    SpawnUnique(event_mobid, event_amount);
                    event_round = 2;
                }
            }
        }
        #endregion

        #region UNIQUE ROUND
        private void UNIQUE_ROUND()
        {
            while (UNIQUE_ROUND_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: UNIQUE_ROUND was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_ROUND was ended.");
                    StopAllEvents();
                    return;
                }

                if (UNIQUE_ROUNDS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("UNIQUE_ROUND_ENDED");

                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    Warp(event_regionid, event_PosX, event_PosY, event_PosZ);
                    Thread.Sleep(5000);
                    SendNotice("UNIQUE_ROUND_BEGIN");
                    Thread.Sleep(10000);
                    event_round++;
                    continue;
                }
                else
                {
                    UNIQUE_ROUNDS.Add(event_name);

                    Thread.Sleep(10000);

                    SpawnUnique(event_mobid, 1);
                    SendNotice("UNIQUE_ROUND_STARTED");
                }
            }
        }
        #endregion

        #region SEARCH AND DESTROY
        private void UNIQUE_SEARCH()
        {
            while (UNIQUE_SEARCH_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: UNIQUE_SEARCH was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "UNIQUE_SEARCH was ended.");
                    StopAllEvents();
                    return;
                }

                if (UNIQUE_ROUNDS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("SND_ROUND_ENDED");

                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    Warp(event_regionid, event_PosX, event_PosY, event_PosZ);
                    Thread.Sleep(5000);
                    SendNotice("SND_ROUND_BEGIN");
                    Thread.Sleep(10000);

                    SendNotice("SND_ROUND_BEGIN2");
                    Thread.Sleep(10000);

                    event_round++;
                    continue;
                }
                else
                {
                    UNIQUE_ROUNDS.Add(event_name);

                    Thread.Sleep(10000);

                    SpawnUnique(event_mobid, 1);
                    SendNotice("SND_ROUND_STARTED");
                }
            }
        }
        #endregion

        #region HIDE AND SEEK
        private void EVENT_HNS()
        {
            while (EVENT_HNS_ACTIVE)
            {
                //if (!FilterMain.PaidUser) return;
                Thread.Sleep(1000);

                if (Exit)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Event Manager]: HNS_EVENT was ended.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, "HNS_EVENT was ended.");
                    StopAllEvents();
                    return;
                }

                if (HNS_ROUNDS.Count > 0)
                {
                    continue;
                }

                if (event_round > event_amount)
                {
                    SendNotice("HNS_EVENT_ENDED");

                    StopAllEvents();
                    return;
                }

                if (event_round == 0)
                {
                    Warp(event_regionid, event_PosX, event_PosY, event_PosZ);
                    Thread.Sleep(5000);

                    SendNotice("HNS_EVENT_BEGIN");
                    Thread.Sleep(10000);

                    SendNotice("HNS_EVENT_BEGIN2");
                    Thread.Sleep(10000);

                    event_round++;
                    continue;
                }
                else
                {
                    HNS_ROUNDS.Add(event_name);

                    Thread.Sleep(10000);

                    SendNotice("HNS_EVENT_STARTED");
                    CreateStall();
                }
            }
        }
        #endregion

        #region MAIN BOT HANDLER SYSTEM
        private void StartShit()
        {
            while (true)
            {
                Thread.Sleep(10000);
                if (Exit) return;

                if (!AlreadyStarted)
                {
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: Spawned as: [ CharName16: {Clientless.Character} ]");
                    AlreadyStarted = true;

                    new Thread(AutoNotice).Start();
                    new Thread(AutoPM).Start();

                    if (!FilterMain.BATTLE_ROYALE)
                    {
                        new Thread(GoTown).Start();
                        //new Thread(Advertisment).Start();
                        new Thread(EventHandler).Start();
                        new Thread(StopEvent).Start();
                        //new Thread(exploit).Start();
                        return;
                    }
                    else
                    {
                        new Thread(UpdatePlacement).Start();
                        new Thread(HandleRoyale).Start();
                        return;
                    }

                    return;
                }
                return;
            }
        }
        #endregion

        #region SpawnUnique
        private void SpawnUnique(Int32 mobid, int amount)
        {
            if (Exit) return;

            Packet response = new Packet(0x7010);
            response.WriteUInt8(0x06);
            response.WriteUInt8(0x00);
            response.WriteUInt32(mobid);
            response.WriteUInt16(amount);
            response.WriteUInt8(0x03);
            Security.Send(response);
        }
        #endregion

        #region RemoveSpawnedUnique
        private void RemoveSpawnedUniques()
        {
            if (Exit) return;

            try
            {
                foreach (object world_id in spawned_world_ids.Keys)
                {
                    if (Exit) return;

                    Thread.Sleep(5000);

                    object mobid = spawned_world_ids[world_id];
                    string CodeName128 = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE}].[dbo].[_IDToCodeName] {mobid}")).Result;

                    #region Teleport to monster
                    Packet response = new Packet(0x7010);
                    response.WriteUInt16((byte)31);
                    response.WriteAscii(CodeName128);
                    Security.Send(response);
                    Thread.Sleep(3000);
                    #endregion

                    #region Remove monster
                    Packet response2 = new Packet(0x7010);
                    response2.WriteUInt16((byte)20);
                    response2.WriteUInt32(world_id);
                    response2.WriteUInt8(1);
                    Security.Send(response2);
                    Thread.Sleep(5000);
                    #endregion

                    //FilterMain.startup_list.Add($"6[{DateTime.UtcNow}] [{CodeName128}] was removed by {Clientless.Character}.");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"[{CodeName128}] was removed by {Clientless.Character}.");

                    wait_for_delay += 5000;
                }
            }
            catch { }

            spawned_world_ids.Clear();
            Thread.Sleep(3000);
            ToTown();
        }
        #endregion

        #region Warp
        private void Warp(int RegionID, double x, double y, double z)
        {
            if (Exit) return;

            teleporting = true;

            Thread.Sleep(3000);

            Packet response = new Packet(0x7010);
            response.WriteUInt8(0x10);
            response.WriteUInt8(0);
            response.WriteInt16(RegionID);
            response.WriteSingle(x);
            response.WriteSingle(y);
            response.WriteSingle(z);
            response.WriteInt8(1);
            response.WriteUInt8(0);
            Security.Send(response);

            Thread.Sleep(3000);

            Packet response2 = new Packet(0x704F);
            response2.WriteUInt8(0x04);
            Security.Send(response2);

            //Thread.Sleep(10500);
        }
        #endregion

        #region CreateStall
        private void CreateStall()
        {
            if (Exit) return;

            Invincible();

            Thread.Sleep(3000);

            Packet response = new Packet(0x70B1);
            response.WriteAscii("KRYLFILTER EVENT");
            Security.Send(response);

            Packet response2 = new Packet(0x70B4);
            response2.WriteUInt8((byte)6);
            response2.WriteAscii("SEND MESSAGE HERE");
            Security.Send(response2);
        }
        #endregion

        #region CloseStall
        private void CloseStall()
        {
            if (Exit) return;

            Packet response = new Packet(0x70B2);
            Security.Send(response);
        }
        #endregion

        #region Invisible
        private void Invisible()
        {
            if (Exit) return;

            if (Is_visible)
            {
                Is_visible = false;
            }
            else
            {
                Is_visible = true;
            }

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)14);
            Security.Send(response);
        }
        #endregion

        #region Invincible
        private void Invincible()
        {
            if (Exit) return;

            if (!Is_visible) Is_visible = true;

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)15);
            Security.Send(response);
        }
        #endregion

        #region ToTown
        private void ToTown(string charname = "")
        {
            if (Exit) return;

            if (string.IsNullOrEmpty(charname)) charname = Clientless.Character;

            if (charname == Clientless.Character)
            {
                teleporting = true;
            }

            Thread.Sleep(5000);

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)3);
            response.WriteAscii(charname);
            Security.Send(response);
        }
        #endregion

        #region ToTown without delay
        private void ToTownInstant(string charname = "")
        {
            if (Exit) return;
            
            if (string.IsNullOrEmpty(charname)) charname = Clientless.Character;

            if(charname == Clientless.Character)
            {
                teleporting = true;
            }

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)3);
            response.WriteAscii(charname);
            Security.Send(response);
        }
        #endregion

        #region Royale battle area
        private void GoBattle()
        {
            while (true)
            {
                Thread.Sleep(3000);
                if (Exit) return;
                teleporting = true;

                Packet response = new Packet(0x7010);
                response.WriteUInt8(0x10);
                response.WriteUInt8(0);
                response.WriteInt16(ROYAL_RegionID);
                response.WriteSingle(ROYAL_PosX);
                response.WriteSingle(ROYAL_PosY);
                response.WriteSingle(ROYAL_PosZ);
                response.WriteInt8(ROYAL_GameWorldID);
                response.WriteUInt8(0);
                Security.Send(response);

                Thread.Sleep(10000);

                Packet response2 = new Packet(0x704F);
                response2.WriteUInt8(0x04);
                Security.Send(response2);

                Thread.Sleep(5000);

                Invisible();

                Thread.Sleep(5000);

                shit_happens:
                if (FilterMain.BATTLE_ROYALE_BUGGED.Count > 0)
                {
                    foreach(string charname in FilterMain.BATTLE_ROYALE_BUGGED)
                    {
                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_STRIP] '{charname}'"));
                        Packet royal = new Packet(0x1420);
                        royal.WriteAscii("FUCK_YOU_YOU_UGGLY");
                        royal.WriteUInt8((byte)3);
                        royal.WriteAscii(charname);
                        Security.Send(royal);
                        Thread.Sleep(3000);
                        ToTownInstant(charname);
                        Thread.Sleep(3000);
                        SendPM(charname, "BATTLEROYALE was reset by Administrator, match never happened.");
                        FilterMain.BATTLE_ROYALE_BUGGED.Remove(charname);
                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_DEL] '{charname}'"));
                        goto shit_happens;
                    }
                }
                else
                {
                    Thread.Sleep(10000);
                    SendNotice("Registration for BATTLEROYALE is now open!");
                }

                new Thread(HandleDiscord).Start();
                return;
            }
        }
        #endregion

        private void UpdatePlacement()
        {
            while(true)
            {
                if (Exit) return;
                Thread.Sleep(10000);

                try
                {
                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_UPDATE_PLACEMENT]"));
                }
                catch { }
            }
        }

        private void PlayersLeft()
        {
            while(true)
            {
                if (Exit) return;
                Thread.Sleep(10000);
                if (ROYAL_OPEN_REGISTER) continue;
                int count = BATTLE_ROYAL.Count;
                if (count < 2) return;

                if (LAST_DEAD_COUNT == count) continue;

                SendNotice($"[{count}] players left");
                LAST_DEAD_COUNT = count;
            }
        }

        private void HandleDiscord()
        {
            while(true)
            {
                if (Exit) return;
                Thread.Sleep(1000);

                try
                {
                    try
                    {
                        string message = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_SendDiscord]")).Result;
                        if (message != "non")
                        {
                            FilterMain.DiscordWebHook(message, Clientless.Character, FilterMain.DiscordWebHook_Royale);
                        }
                    }
                    catch { }
                }
                catch { }
            }
        }

        private void HandleRoyale()
        {
            while(true)
            {
                if (Exit) return;

                Thread.Sleep(10000);

                try
                {
                    string royale_type = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_SETTINGS] '{Clientless.Character}'")).Result;
                    string[] type = royale_type.Split('|');

                    ROYAL_RUNNING = int.Parse(type[0]);
                    ROYAL_CAPACITY = int.Parse(type[1]);
                    ROYAL_RegionID = int.Parse(type[2]);
                    ROYAL_PosX = Convert.ToDouble(type[3].Replace(".", ","));
                    ROYAL_PosY = Convert.ToDouble(type[4].Replace(".", ","));
                    ROYAL_PosZ = Convert.ToDouble(type[5].Replace(".", ","));
                    ROYAL_GameWorldID = int.Parse(type[6]);
                }
                catch(Exception ex)
                { Logger.WriteLine(Logger.LogLevel.EventBot, ex.ToString()); }

                new Thread(RegisterHandler).Start();
                new Thread(WatchDeadPlayer).Start();
                new Thread(GoBattle).Start();
                return;
            }
        }

        private void RestrictZone()
        {
            while(true)
            {
                if (Exit) return;
                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                /*Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE) return;

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE) return;

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE) return;

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE) return;

                Thread.Sleep(60000);
                if (ROYAL_DESTROY_ZONE) return;*/


                SendNotice("The area will be restrained in 5 minutes, please proceed to the red circle.");
                Thread.Sleep(240000);

                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                SendNotice("The area will be restrained in 1 minute, proceed to the red circle!");
                Thread.Sleep(60000);

                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                SendNotice("You are not allowed outside the red circle anymore.");
                Thread.Sleep(10000);

                if (ROYAL_DESTROY_ZONE)
                {
                    Logger.WriteLine("Closed RestrictZone()");
                    return;
                }

                // RESTRAIN
                Packet start = new Packet(0x1420);
                start.WriteAscii("FUCK_YOU_YOU_UGGLY");
                start.WriteUInt8(0x05);
                Security.Send(start);

                return;
            }
        }

        private void RegisterHandler()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Exit) return;
                if (FilterMain.BATTLE_ROYALE_BUGGED.Count > 0) continue;

                int count = BATTLE_ROYAL.Count;
                if (count < 1)
                {
                    ROYAL_OPEN_REGISTER = true;
                    continue;
                }

                if (!ROYAL_OPEN_REGISTER) continue;

                if (count >= ROYAL_CAPACITY)
                {
                    ROYAL_OPEN_REGISTER = false;
                    //Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_ONGOING] '{Clientless.Character}', 1"));

                    SendNotice("Registration for BATTLEROYALE is now closed, match will start soon.");
                    Thread.Sleep(10000);

                    // START EVENT?
                    SendChat("Match will start in 30 seconds");
                    Thread.Sleep(10000);
                    SendChat("Match will start in 20 seconds");
                    Thread.Sleep(10000);
                    SendChat("Match will start in 10 seconds");
                    Thread.Sleep(10000);
                    SendNotice("The match has started!");

                    // START EVENT?
                    Packet start = new Packet(0x1420);
                    start.WriteAscii("FUCK_YOU_YOU_UGGLY");
                    start.WriteUInt8(0x02);
                    Security.Send(start);

                    ROYAL_DESTROY_ZONE = false;
                    new Thread(RestrictZone).Start();
                    new Thread(PlayersLeft).Start();
                    new Thread(WatchAlivePlayer).Start();
                }
                else
                {
                    Thread.Sleep(60000);
                    ROYAL_WAIT_FAILOVER++;
                    count = BATTLE_ROYAL.Count;
                }

                if(ROYAL_WAIT_FAILOVER >= 1) // 4 = 3 minutes
                {
                    if(count < 2)
                    {
                        SendNotice("Waiting for more players to join BR, you still got time to register.");
                        ROYAL_WAIT_FAILOVER = 0;
                        continue;
                    }

                    count = BATTLE_ROYAL.Count;
                    if (count < 2)
                    {
                        ROYAL_WAIT_FAILOVER = 0;
                        continue;
                    }

                    SendNotice("Waiting for more players to join BR, you still got time to register.");
                    Thread.Sleep(60000);

                    count = BATTLE_ROYAL.Count;
                    if (count < 2)
                    {
                        ROYAL_WAIT_FAILOVER = 0;
                        continue;
                    }

                    SendNotice("Waiting for more players to join BR, you still got time to register.");
                    Thread.Sleep(60000);

                    count = BATTLE_ROYAL.Count;
                    if (count < 2)
                    {
                        ROYAL_WAIT_FAILOVER = 0;
                        continue;
                    }

                    ROYAL_OPEN_REGISTER = false;
                    SendNotice("Registration for BATTLEROYALE is now closed, match will start soon.");
                    Thread.Sleep(10000);

                    // START EVENT?
                    SendChat("Match will start in 30 seconds");
                    Thread.Sleep(10000);
                    SendChat("Match will start in 20 seconds");
                    Thread.Sleep(10000);
                    SendChat("Match will start in 10 seconds");
                    Thread.Sleep(10000);
                    SendNotice("The match has started!");

                    // START EVENT?
                    Packet start = new Packet(0x1420);
                    start.WriteAscii("FUCK_YOU_YOU_UGGLY");
                    start.WriteUInt8(0x02);
                    Security.Send(start);

                    ROYAL_DESTROY_ZONE = false;
                    new Thread(RestrictZone).Start();
                    LAST_DEAD_COUNT = BATTLE_ROYAL.Count;
                    new Thread(PlayersLeft).Start();
                    new Thread(WatchAlivePlayer).Start();
                }
            }
        }

        private void WatchDeadPlayer()
        {
            while (true)
            {
                Thread.Sleep(5000);

                if (Exit) return;
                if (FilterMain.BATTLE_ROYALE_BUGGED.Count > 0) continue;

                shit_stuff:
                int count = BATTLE_ROYAL.Count;
                foreach (string charname in BATTLE_ROYAL)
                {
                    int alive = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_ALIVE] '{charname}'")).Result;
                    if (alive == -1) continue;

                    #region Dead people
                    if (alive == 0)
                    {
                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_CALCULATE_DEAD_SINGLE] '{charname}'"));
                        Thread.Sleep(3000);
                        ToTownInstant(charname);
                        Thread.Sleep(10000);
                        SendPM(charname, $"You died in position #{BATTLE_ROYAL.Count}");
                        Thread.Sleep(3000);
                        Packet royal = new Packet(0x1420);
                        royal.WriteAscii("FUCK_YOU_YOU_UGGLY");
                        royal.WriteUInt8((byte)3);
                        royal.WriteAscii(charname);
                        Security.Send(royal);

                        count = BATTLE_ROYAL.Count;
                        if (count == 1)
                        {
                            SendNotice("Registration for BATTLEROYALE is now open!");
                            FilterMain.DiscordWebHook($"The last player in BATTLEROYALE died was killed by the enviroment.", Clientless.Character, FilterMain.DiscordWebHook_Royale);
                        }
                        BATTLE_ROYAL.Remove(charname);

                        goto shit_stuff;
                    }
                    #endregion


                }
            }
        }

        private void WatchAlivePlayer()
        {
            while (true)
            {
                // BUG :(
                Thread.Sleep(5000);

                if (Exit) return;
                if (ROYAL_OPEN_REGISTER)
                {
                    Logger.WriteLine("WatchAlivePlayer() was destroyed.");
                    return;
                }

                int count = BATTLE_ROYAL.Count;
                if (count != 1) continue;

                foreach (string charname in BATTLE_ROYAL)
                {
                    int alive = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_ALIVE] '{charname}'")).Result;
                    #region Alive people
                    if (alive == 1)
                    {
                        #region Winner winner chicken dinner
                        ROYAL_DESTROY_ZONE = true;
                        int kills = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_CALCULATE_WINNER] '{charname}', '{Clientless.Character}'")).Result;
                        Thread.Sleep(3000);
                        ToTownInstant(charname);
                        Thread.Sleep(10000);
                        SendPM(charname, $"Winner winner chicken dinner!");
                        //SendNotice($"{charname} has claimed victory of Silkroad Royale with x kills!");
                        Thread.Sleep(3000);
                        Packet royal = new Packet(0x1420);
                        royal.WriteAscii("FUCK_YOU_YOU_UGGLY");
                        royal.WriteUInt8((byte)3);
                        royal.WriteAscii(charname);
                        Security.Send(royal);
                        SendNotice($"{charname} has claimed victory of BATTLEROYALE with {kills} kills!");
                        FilterMain.DiscordWebHook($"**{charname}** has claimed victory of **BATTLEROYALE** with **{kills}** kills!", Clientless.Character, FilterMain.DiscordWebHook_Royale);
                        Thread.Sleep(10000);
                        Packet start = new Packet(0x1420);
                        start.WriteAscii("FUCK_YOU_YOU_UGGLY");
                        start.WriteUInt8(0x06);
                        Security.Send(start);
                        BATTLE_ROYAL.Remove(charname);
                        SendNotice("Registration for BATTLEROYALE is now open!");

                        Logger.WriteLine("WatchAlivePlayer() was destroyed.");
                        return;
                        #endregion
                    }
                    #endregion
                }
                Logger.WriteLine("WatchAlivePlayer() was destroyed.");
                return;
            }
        }

        #region GoTown
        private void GoTown()
        {
            if (Exit) return;

            teleporting = true;
            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)2);
            Security.Send(response);
            return;
        }
        #endregion

        #region RecallUser
        private void RecallUser(string charname)
        {
            if (Exit) return;

            if (string.IsNullOrEmpty(charname)) charname = Clientless.Character;

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)17);
            response.WriteAscii(charname);
            Security.Send(response);
        }
        #endregion

        #region MoveToUser
        private void MoveToUser(string charname)
        {
            if (Exit) return;
            if (string.IsNullOrEmpty(charname)) charname = Clientless.Character;

            Packet response = new Packet(0x7010);
            response.WriteUInt16((byte)8);
            response.WriteAscii(charname);
            Security.Send(response);
        }
        #endregion

        #region CreateParty
        private void CreateParty()
        {
            if (Exit) return;

            Packet party = new Packet(0x7069);
            party.WriteUInt64(0x00);
            party.WriteUInt8((byte)5);
            party.WriteUInt8(0x00);
            party.WriteUInt8((byte)1);
            party.WriteUInt8((byte)255);
            party.WriteAscii("KRYLFILTER");
            Security.Send(party);
        }
        #endregion

        #region CloseParty
        private void CloseParty()
        {
            if (Exit) return;

            Packet party = new Packet(0x706B);
            party.WriteUInt32(party_number);
            Security.Send(party);
        }
        #endregion

        #region SendNotice
        private void SendNotice(string message)
        {
            if (Exit) return;

            Packet response = new Packet(0x7025);
            response.WriteUInt8((byte)7);
            response.WriteUInt8((byte)0);
            response.WriteAscii(GG(message));
            Security.Send(response);
        }
        #endregion

        #region SendPM
        private void SendPM(string user, string message)
        {
            if (Exit) return;

            Packet response = new Packet(0x7025);
            response.WriteUInt8((byte)2);
            response.WriteUInt8((byte)0);
            response.WriteAscii(user);
            response.WriteAscii(GG(message));
            Security.Send(response);
        }
        #endregion

        #region SendChat
        private void SendChat(string message)
        {
            if (Exit) return;

            Packet response = new Packet(0x7025);
            response.WriteUInt8(0x03);
            response.WriteUInt8((byte)0);
            response.WriteAscii(message);
            Security.Send(response);
        }
        #endregion

        #region AutoFill Globals
        private void AutoFill()
        {
            if (CharStrings.GlobalsTypeSlot.Count < 1)
            {
                try
                {
                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_SHARD}].[dbo].[_ADD_ITEM_EXTERN] '{Clientless.Character}', 'ITEM_EVENT_GLOBAL_CHATTING', 50, 0"));
                }
                catch { }

                Thread.Sleep(5000);

                ToTown();
            }
        }
        #endregion

        #region SendGlobals
        private void SendGlobal(string message)
        {
            if (Exit) return;

            try
            {
                if (CharStrings.GlobalsTypeSlot.Count < 1)
                {
                    AutoFill();
                    Thread.Sleep(10000);
                    SendGlobal(message);
                    return;
                }
                var random = new Random();
                int index = random.Next(CharStrings.GlobalsTypeSlot.Count);
                String GlobalSlot = CharStrings.GlobalsTypeSlot[index];
                string[] tttxt;
                string TType = string.Empty, slot = string.Empty;
                int Count = 0;
                if (GlobalSlot != "")
                {
                    tttxt = GlobalSlot.Split(',');
                    TType = tttxt[0];
                    Count = int.Parse(tttxt[1]);
                    slot = tttxt[2];
                    CharStrings.GlobalsTypeSlot.Remove(GlobalSlot);

                    Packet response = new Packet(0x704C, true);
                    response.WriteUInt8((byte)int.Parse(slot));
                    response.WriteUInt8(0xEC);
                    response.WriteUInt8(0x29);
                    response.WriteAscii(GG(message));
                    Security.Send(response);
                    Count = Count - 1;
                    CharStrings.GlobalsTypeSlot.Add(TType + "," + Count + "," + slot);
                }
                else
                {
                    AutoFill();
                    Thread.Sleep(10000);
                    SendGlobal(message);
                    return;
                }
            }
            catch
            {
                AutoFill();
                Thread.Sleep(10000);
                SendGlobal(message);
                return;
            }
        }
        #endregion
    }
}
