#pragma warning disable

using Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Filter.NetEngine
{
    sealed class AgentContext
    {
        Socket m_ClientSocket = null;
        AsyncServer.E_ServerType m_HandlerType;
        AsyncServer.delClientDisconnect m_delDisconnect;
        object m_Lock = new object();
        Socket m_ModuleSocket = null;

        // New buffers
        byte[] m_LocalBuffer = new byte[8192];
        byte[] m_RemoteBuffer = new byte[8192];

        Security m_LocalSecurity = new Security();
        Security m_RemoteSecurity = new Security();

        Thread m_TransferPoolThread = null;
        ulong m_BytesRecvFromClient = 0;
        DateTime m_StartTime = DateTime.Now;

        int anti_cheat_royale = 0;

        #region NEW SHIT
        DateTime afk_timer = DateTime.Now;
        bool is_fortress = false;
        bool is_afk = false;
        bool afk_symbol = true;
        #endregion

        #region Random strings and shiet
        int length = 0;
        string ip;
        //https://support.microsoft.com/sv-se/kb/307023
        #endregion

        #region SHARD EXPLOITS
        bool char_screen = false;
        bool user_logged_in = false;
        bool sent_weather_request = false;
        bool sent_charspawn_sucess = false;
        bool skip_shit = false;
        #endregion

        #region Anti exploit mesures
        int PACKET_COUNT = 0;
        Timer packet_timer = null;
        bool StallBlock = true;
        bool InstallNetwork = false;
        bool InStall = false;
        bool startnotice = false;
        byte pm_queue = 0;
        #endregion

        #region 0x3013
        UInt32 ID;
        int CurLevel = 0;
        int chardata_status = 1;
        bool char_job = false;
        bool job_thief = false;
        int guild_limit = 0;
        int union_limit = 0;
        string new_charname = null;
        bool is_in_bot_region = false;
        bool is_in_event_region = false;
        bool is_in_town_region = false;
        bool is_online = false;
        #endregion

        #region Char LOCK
        bool is_locked = true;
        int lock_fail_attempt = 0;
        int has_code = 0;
        #endregion

        #region Translate options
        Int64 required_level = 0;
        Int64 required_delay = 0;
        ushort last_opcode;
        #endregion

        #region 0x6103
        string user_id;
        string user_pw;
        bool one_time_shit = false;
        #endregion

        #region HWID SHITS
        string mac = "non";
        string serial = "non";
        string hwid = "non";
        #endregion

        #region 0x7001
        string charname;
        string sql_charname;
        bool charname_sent = false;
        #endregion

        #region Packet handler
        public static object locker = new object();
        #endregion

        #region RewardPerhour
        Timer GiveRewardPerHour = null;
        bool first_time = true;
        #endregion

        #region BOT_SYSTEM
        bool pvp_cape = false;
        bool disconnect_bot = false;
        #endregion

        #region ANTI CHEAT
        short selfWalkLatestRegion = -1;
        short LatestRegion = 25000;
        bool thief_pickup = true;
        #endregion

        #region Delays
        DateTime laststalltime = new DateTime();
        DateTime lastexchangetime = new DateTime();
        DateTime global_time = new DateTime();
        DateTime reverse_time = new DateTime();
        DateTime resurrection_time = new DateTime();
        DateTime lastzerktime = new DateTime();
        DateTime lastspawntime = new DateTime();
        DateTime exittime = new DateTime();
        DateTime restarttime = new DateTime();
        DateTime attacktime = new DateTime();
        DateTime bot_login = new DateTime();
        #endregion

        #region IP LIST COUNTER
        public static int Ip_count(string ip)
        {
            lock (locker)
            {
                try
                {
                    int count_ip = 0;
                    foreach (string ips in FilterMain.ip_list_a)
                    {
                        if (ips == ip)
                        {
                            count_ip++;
                        }
                    }
                    return count_ip;
                }
                catch
                {
                    return 0;
                }
            }
        }
        #endregion

        #region IS THIS AN ALLOWED REGION?
        public static bool IsAllowedRegion(short region)
        {
            lock (locker)
            {
                try
                {
                    if (FilterMain.town_list.Contains(region))
                    {
                        return false;
                    }
                    return true;
                }
                catch
                {
                    return true;
                }
            }
        }
        #endregion

        #region IS THIS AN ALLOWED REGION?
        public static bool IsFortressRegion(short region)
        {
            lock (locker)
            {
                try
                {
                    if (FilterMain.fortress_list.Contains(region))
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region IS THIS AN ALLOWED REGION?
        public static bool IsBotRegion(short region)
        {
            lock (locker)
            {
                try
                {
                    if (FilterMain.bot_region_list.Contains(region))
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region IS THIS AN ALLOWED REGION?
        public static bool IsEventRegion(short region)
        {
            lock (locker)
            {
                try
                {
                    if (FilterMain.event_region_list.Contains(region))
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region IS THIS AN ALLOWED REGION?
        public static bool IsTownRegion(short region)
        {
            lock (locker)
            {
                try
                {
                    if (FilterMain.city_list.Contains(region))
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region Badword blocker
        public static bool Badword(string word)
        {
            lock (locker)
            {
                try
                {
                    foreach (string badword in FilterMain.badwords)
                    {
                        if (word.Contains(badword))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        bool job_question = false;

        #region Bot list users
        public static bool BotList(string username)
        {
            lock (locker)
            {
                if(FilterMain.BOT_LIST.Contains(username))
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        public AgentContext(Socket ClientSocket, AsyncServer.delClientDisconnect delDisconnect)
        {
            this.m_delDisconnect = delDisconnect;
            this.m_ClientSocket = ClientSocket;
            this.m_HandlerType = AsyncServer.E_ServerType.AgentServer;
            this.m_ModuleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Register ip
            this.ip = ((IPEndPoint)(m_ClientSocket.RemoteEndPoint)).Address.ToString();

            try
            {
                Logger.WriteLine(Logger.LogLevel.Debug, $"Getting connection: {this.ip} -> {FilterMain.agent_remote}:{FilterMain.agent_mports}");
                this.m_ModuleSocket.Connect(new IPEndPoint(IPAddress.Parse(FilterMain.agent_remote), FilterMain.agent_mports));
                this.m_LocalSecurity.GenerateSecurity(true, true, true);
                this.DoRecvFromClient();
                Send(false);
            }
            catch
            {
                Logger.WriteLine(Logger.LogLevel.MikeMode, $"Remote host ({FilterMain.agent_remote}:{FilterMain.agent_mports}) is suicidal :(");
            }

            try
            {
                // IP list
                FilterMain.ip_list_a.Add(this.ip);

                #region Packet timer start
                if (this.packet_timer == null)
                {
                    this.packet_timer = new Timer(new TimerCallback(this.ResetPackets), null, 0, FilterMain.PACKET_RESET);
                }
                #endregion

                #region Flood control fix
                if (FilterMain.FLOOD_COUNT > 0)
                {
                    if (Ip_count(this.ip) > FilterMain.FLOOD_COUNT)
                    {
                        FilterMain.blocked_agent_dosattacks++;
                        Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for flooding.");
                        FirewallHandler.BlockIP(this.ip, "DoS");
                        this.DisconnectModuleSocket();
                        return;
                    }
                }
                #endregion
            }
            catch { }
        }

        #region Word replacer shit
        string GG(string word)
        {
            lock (locker)
            {
                try
                {
                    iniFile cfg = new iniFile("config/translate.ini");
                    if (cfg.IniReadValue("NOTICES", word).Length < 3) return word;

                    string new_word = cfg.IniReadValue("NOTICES", word);

                    new_word = new_word.Replace("{username}", this.user_id);
                    new_word = new_word.Replace("{charname}", this.charname);
                    new_word = new_word.Replace("{ip}", this.ip);
                    new_word = new_word.Replace("{optlevel}", FilterMain.REWARDPERHOUR_OPTLEVEL.ToString());
                    new_word = new_word.Replace("{itemid}", FilterMain.REWARDPERHOUR_ITEMID);
                    new_word = new_word.Replace("{level}", this.CurLevel.ToString());
                    new_word = new_word.Replace("{required_level}", this.required_level.ToString());
                    new_word = new_word.Replace("{delay}", this.required_delay.ToString());
                    new_word = new_word.Replace("{opcode}", this.last_opcode.ToString("X4"));

                    return new_word;
                }
                catch { return word; }
            }
        }
        #endregion

        #region GetBytesPerSecondFromClient()
        double GetBytesPerSecondFromClient()
        {
            double res = 0.0;

            TimeSpan diff = (DateTime.Now - m_StartTime);
            if (m_BytesRecvFromClient > int.MaxValue)
                m_BytesRecvFromClient = 0;

            if (m_BytesRecvFromClient > 0)
            {
                try
                {
                    unchecked
                    {
                        double div = diff.TotalSeconds;
                        if (diff.TotalSeconds < 1.0)
                            div = 1.0;
                        res = Math.Round((m_BytesRecvFromClient / div), 2);
                    }
                }
                catch
                {
                }
            }

            return res;
        }
        #endregion

        #region DisconnectModuleSocket()
        void DisconnectModuleSocket()
        {
            try
            {
                if (this.m_ModuleSocket != null)
                {
                    #region IP LIST
                    if (FilterMain.ip_list_a.Contains(this.ip))
                    {
                        FilterMain.ip_list_a.Remove(this.ip);
                    }
                    #endregion

                    #region BOT LIST
                    if (BotList(this.user_id))
                    {
                        FilterMain.BOT_LIST.Remove(this.user_id);
                    }
                    #endregion

                    if(this.charname == "BATTLEROYALE")
                    {
                        FilterMain.BATTLE_ROYALE.Clear();
                        FilterMain.BATTLE_ROYALE_ALIVE.Clear();
                        FilterMain.BATTLE_ROYALE_CHEATERS.Clear();
                        FilterMain.BATTLE_ROYALE_SHRINK_AREA = false;
                    }
                    else
                    {
                        FilterMain.BATTLE_ROYALE.Remove(this.charname);
                        FilterMain.BATTLE_ROYALE_ALIVE.Remove(this.charname);
                        FilterMain.BATTLE_ROYALE_CHEATERS.Remove(this.charname);
                    }

                    #region Packet timer
                    if (this.packet_timer != null)
                    {
                        this.packet_timer.Dispose();
                        this.packet_timer = null;
                    }
                    #endregion

                    #region Reward timer
                    if (this.GiveRewardPerHour != null)
                    {
                        this.GiveRewardPerHour.Dispose();
                        this.GiveRewardPerHour = null;
                    }
                    #endregion

                    // DISCONNECT
                    this.m_ModuleSocket.Close();
                    this.m_ModuleSocket = null;
                }
                this.m_ModuleSocket = null;
            }
            catch
            {
                Logger.WriteLine(Logger.LogLevel.Error, $"Client({this.ip}:{this.user_id}) Error(func _DisconnectModuelSocket())");
            }
        }
        #endregion

        void OnReceive_FromServer(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = m_ModuleSocket.EndReceive(iar);

                    if (nReceived != 0)
                    {
                        this.m_RemoteSecurity.Recv(m_RemoteBuffer, 0, nReceived);

                        List<Packet> RemotePackets = m_RemoteSecurity.TransferIncoming();

                        if (RemotePackets != null)
                        {
                            foreach (Packet _pck in RemotePackets)
                            {
                                ushort opcode = Convert.ToUInt16(_pck.Opcode.ToString().ToLower());

                                //Logger.WriteLine(Logger.LogLevel.Error, $"[S->C][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");

                                #region New exploit fix
                                if (!FilterMain.Server_whitelisted.Contains(opcode))
                                {
                                    if (!FilterMain.Server_opcodes.Contains(opcode))
                                    {
                                        FilterMain.Server_opcodes.Add(opcode);
                                    }
                                }
                                #endregion

                                #region Handshake
                                // Handshake
                                if (opcode == 0x5000 || opcode == 0x9000)
                                {
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x3020_CLIENT_CELESTIAL_POSITION
                                else if (opcode == 0x3020)
                                {
                                    _pck.Lock();
                                    try
                                    {
                                        this.ID = _pck.ReadUInt32();
                                        if(FilterMain.FILES.Contains("jsro"))
                                        {
                                            this.char_screen = false;
                                        }
                                    }
                                    catch { }
                                }
                                #endregion

                                #region 0xB517_CLIENT_CHANGENAME_VSRO274
                                else if (opcode == 0xb517 && FilterMain.FILES.Contains("274"))
                                {
                                    _pck.Lock();
                                    try
                                    {
                                        if (_pck.ReadUInt8() == 1)
                                        {
                                            if (_pck.ReadUInt8() == 2)
                                            {
                                                if (_pck.ReadUInt8() == 1)
                                                {
                                                    this.new_charname = sqlCon.clean(_pck.ReadAscii());
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, $"Mike fucked up with 274 parsing :(");
                                    }
                                }
                                #endregion

                                #region 0x3013_CLIENT_CHARDATA
                                else if (opcode == 0x3013)
                                {
                                    _pck.Lock();

                                    if (this.sql_charname == null) return;
                                    if (this.user_id == null) return;

                                    this.StallBlock = false;
                                    this.pvp_cape = false;
                                    this.selfWalkLatestRegion = -1;
                                    this.anti_cheat_royale = 0;
                                    FilterMain.BATTLE_ROYALE_CHEATERS.Remove(this.charname);

                                    #region AFK Timer
                                    this.afk_timer = DateTime.Now;
                                    this.is_afk = false;
                                    #endregion

                                    #region CurLevel
                                    try
                                    {
                                        this.CurLevel = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 2")).Result;
                                    }
                                    catch { }
                                    #endregion

                                    #region PC_LIMT_JOB_ARAB_PROTECT
                                    if (FilterMain.JOB_PC_LIMIT > 0)
                                    {
                                        try
                                        {
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountJOB] '{this.mac}'")).Result;
                                            if (current_count > FilterMain.JOB_PC_LIMIT)
                                            {
                                                //this.SendNotice("PC_LIMIT_JOB_SUIT_ERROR");
                                                this.SendNotice("JOB PC Limit Aşıldı!");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    #region IP_LIMIT_ARAB_PROTECT

                                    #endregion

                                    #region GET_REGION_WHEN_TELEPORT
                                    try
                                    {
                                        this.LatestRegion = Convert.ToInt16(Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 6")).Result);
                                    }
                                    catch { }
                                    #endregion

                                    #region ONE_TIME_SHIT
                                    if (!this.one_time_shit)
                                    {
                                        #region _LoginHistory
                                        try
                                        {
                                            Task.Run(async () => await sqlCon.exec($"[{FilterMain.sql_db}].[dbo].[_LoginLogs] '{this.user_id}', '{this.ip}', '{this.mac}', '{this.serial}', '{this.hwid}'"));
                                        }
                                        catch { }
                                        #endregion

                                        #region ITEM LOCK
                                        /*CLOSE
                                        if (FilterMain.ITEM_LOCK)
                                        {
                                            try
                                            {
                                                this.has_code = Task.Run(async () => await sqlCon.prod_int($"EXEC[{FilterMain.sql_db}].[dbo].[_GetLock] '{this.user_id}'")).Result;
                                                if (this.has_code == 0)
                                                {
                                                    this.is_locked = false;
                                                }
                                                else
                                                {
                                                    this.is_locked = true;
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            this.is_locked = false;
                                        }
                                        */
                                        #endregion

                                        #region RewardPerHour
                                        /*CLOSE
                                        if (this.GiveRewardPerHour == null)
                                        {
                                            this.GiveRewardPerHour = new Timer(new TimerCallback(this.RewardPerHour), null, 0, 3600000);
                                        }
                                        #endregion

                                        #region BOT_CONTROLL
                                        if (FilterMain.BOT_CONTROLL)
                                        {
                                            if (BotList(this.user_id))
                                            {
                                                try
                                                {
                                                    Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_Bot] '{this.user_id}', '{this.ip}', 'SERVER_LOCALE = 22'"));
                                                }
                                                catch { }
                                            }
                                        }
                                        */
                                        #endregion

                                        this.one_time_shit = true;
                                    }
                                    #endregion

                                    #region CHAR_JOB_MODE
                                    try
                                    {
                                        int jobbing = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 1")).Result;
                                        switch (jobbing)
                                        {
                                            case 0:
                                                {
                                                    this.char_job = false;
                                                    this.job_thief = false;
                                                }
                                                break;
                                            case 1:
                                                {
                                                    this.char_job = true;
                                                    this.job_thief = false;
                                                }
                                                break;
                                            case 2:
                                                {
                                                    this.char_job = true;
                                                    this.job_thief = true;
                                                }
                                                break;
                                            default:
                                                {
                                                    this.char_job = false;
                                                    this.job_thief = false;
                                                }
                                                break;
                                        }
                                    }
                                    catch { }
                                    #endregion

                                    #region JOB_BYPASS_FIX
                                    if (!FilterMain.BOT_JOBBING)
                                    {
                                        if (BotList(this.user_id))
                                        {
                                            this.SendNotice("Third part programs are not allowed to wear job suit, please remove it before logging in again!");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }
                                    #endregion

                                    #region GUILD_LIMIT
                                    if (FilterMain.GUILD_LIMIT > 0)
                                    {
                                        try
                                        {
                                            this.guild_limit = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 4")).Result;
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    #region UNION_LIMIT
                                    if (FilterMain.UNION_LIMIT > 0)
                                    {
                                        try
                                        {
                                            this.union_limit = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 5")).Result;
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    #region Is In Fortress region
                                    if(IsFortressRegion(this.LatestRegion))
                                    {
                                        this.is_fortress = true;
                                    }
                                    else
                                    {
                                        this.is_fortress = false;
                                    }
                                    #endregion

                                    #region Is in Bot Region
                                    if(IsBotRegion(this.LatestRegion))
                                    {
                                        this.is_in_bot_region = true;
                                    }
                                    else
                                    {
                                        this.is_in_bot_region = false;
                                    }
                                    #endregion

                                    #region Is In Event Region
                                    if(IsEventRegion(this.LatestRegion))
                                    {
                                        this.is_in_event_region = true;
                                    }
                                    else
                                    {
                                        this.is_in_event_region = false;
                                    }
                                    #endregion

                                    #region Is In City Region
                                    if(IsTownRegion(this.LatestRegion))
                                    {
                                        this.is_in_town_region = true;
                                    }
                                    else
                                    {
                                        this.is_in_town_region = false;
                                    }
                                    #endregion

                                    this.is_online = true;
                                }
                                #endregion

                                #region 0xB516_CLIENT_ENTER_PVP
                                else if (opcode == 0xb516)
                                {
                                    try
                                    {
                                        _pck.Lock();
                                        uint UserID = _pck.ReadUInt32();
                                        int cape = _pck.ReadUInt8();

                                        if (UserID == this.ID)
                                        {
                                            switch (cape)
                                            {
                                                case 0:
                                                    {
                                                        this.pvp_cape = false;
                                                    }
                                                    break;
                                                case 1:
                                                case 2:
                                                case 3:
                                                case 4:
                                                case 5:
                                                    {
                                                        this.pvp_cape = true;
                                                    }
                                                    break;

                                            }
                                        }
                                    }
                                    catch { }
                                }
                                #endregion

                                #region 0xB021_CLIENT_MOVEMENT
                                // SERVER->CLIENT (MOVEMENT)
                                else if (opcode == 0xb021)
                                {
                                    try
                                    {
                                        _pck.Lock(); // Lock
                                        UInt32 Target = _pck.ReadUInt32(); // Unique ID from player

                                        // Check if target == this.unique_ID
                                        if (Target == this.ID)
                                        {
                                            #region AFK Timer
                                            this.afk_timer = DateTime.Now;
                                            this.is_afk = false;
                                            
                                            if (this.afk_symbol)
                                            {
                                                Packet afk = new Packet(0x7402);
                                                afk.WriteUInt8((byte)0);
                                                m_RemoteSecurity.Send(afk);
                                                Send(true);

                                                this.afk_symbol = false;
                                            }
                                            
                                            #endregion

                                            byte unk = _pck.ReadUInt8();
                                            short Region = _pck.ReadInt16();
                                            int RegionL = Region.ToString().Length;

                                            // Make sure region is 5 numbers.
                                            if (RegionL >= 5 && _pck.GetBytes().Length >= 24)
                                            {
                                                // Register good.
                                                this.selfWalkLatestRegion = Region;
                                                this.LatestRegion = Region;
                                                //Logger.WriteLine(Logger.LogLevel.Debug, $"Region id: {Region}");
                                                /*CLOSE
                                                if (FilterMain.BATTLE_ROYALE_SHRINK_AREA)
                                                {
                                                    if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                                    {
                                                        if (!FilterMain.BATTLE_ROYALE_CHEATERS.Contains(this.charname))
                                                        {
                                                            if (this.LatestRegion != 23980)
                                                            {
                                                                if (this.anti_cheat_royale > 15)
                                                                {
                                                                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_DEAD] '{this.charname}'"));
                                                                    this.SendNotice("You stayed in the restricted area for too long, you're disqualified from the match.");
                                                                    FilterMain.BATTLE_ROYALE_CHEATERS.Add(this.charname);
                                                                    this.anti_cheat_royale = 0;
                                                                }
                                                                this.anti_cheat_royale++;
                                                            }
                                                        }
                                                    }
                                                }
                                                */
                                            }
                                            else
                                            {
                                                // Register false.
                                                this.selfWalkLatestRegion = -1;
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                #endregion

                                #region 0x3026_CLIENT_CHAT
                                else if (opcode == 0x3026)
                                {
                                    _pck.Lock(); // ;D
                                    byte type = _pck.ReadUInt8();
                                    // Logger.WriteLine(Logger.LogLevel.MikeMode, $"{type}");
                                    switch (type)
                                    {
                                        /*
                                            1 = LOCAL CHAT
                                            2 = PRIVATE CHAT
                                            3 = GM CHAT
                                            4 = PARTY CHAT
                                            5 = GUILD CHAT
                                            6 = GLOBAL CHAT
                                            16 = ACADEMY CHAT
                                            11 = UNION CHAT
                                        */
                                        // ALL CHAT
                                        case 1:
                                            {
                                                if(_pck.ReadUInt32() == this.ID)
                                                {
                                                    if (FilterMain.LOG_ALLCHAT)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 1"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;

                                            /*
                                                case 2:
                                                Reverse logic, Since CLIENT -> USER
                                                We cannot read who sent the MSG in SERVER side packets,
                                                We can only read who sent the MSG in CLIENT side packets.

                                                FUTURE MIKE; OQE?:)
                                            */

                                        // GM CHAT
                                        case 3:
                                            {
                                                if (_pck.ReadUInt32() == this.ID)
                                                {
                                                    if (FilterMain.LOG_GMCHAT)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 3"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;

                                        // Party CHAT
                                        case 4:
                                            {
                                                if (FilterMain.LOG_PARTYCHAT)
                                                {
                                                    string charname = _pck.ReadAscii(); // CharName16
                                                    if (charname == this.charname)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 4"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;

                                        // Guild CHAT
                                        case 5:
                                            {
                                                if (FilterMain.LOG_GUILDCHAT)
                                                {
                                                    string charname = _pck.ReadAscii(); // CharName16
                                                    if (charname == this.charname)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 5"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;
                                        
                                        // Global CHAT
                                        case 6:
                                            {
                                                if (FilterMain.LOG_GLOBALCHAT)
                                                {
                                                    string charname = _pck.ReadAscii(); // CharName16
                                                    if (charname == this.charname)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 6"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;

                                        // Academy CHAT
                                        case 16:
                                            {
                                                if (FilterMain.LOG_ACADEMYCHAT)
                                                {
                                                    string charname = _pck.ReadAscii(); // CharName16
                                                    if (charname == this.charname)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 16"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;

                                        // Union CHAT
                                        case 11:
                                            {
                                                if (FilterMain.LOG_UNIONCHAT)
                                                {
                                                    string charname = _pck.ReadAscii(); // CharName16
                                                    if (charname == this.charname)
                                                    {
                                                        string message = sqlCon.clean(_pck.ReadAscii());
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 11"));
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                                #endregion

                                #region 0xA103_GAME_LOGIN_REPLY
                                else if (opcode == 0xa103)
                                {
                                    // packet.ReadUInt8() == 1, SUCCESSFULL LOGIN REPLY
                                    if (_pck.ReadUInt8() == 1)
                                    {
                                        FilterMain.total_visited_users++;
                                        //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Client({this.ip}:{this.user_id}) are now allowed to send agent opcodes!");
                                        this.user_logged_in = true;
                                    }

                                    #region Register user in char selection screen
                                    if(this.user_logged_in)
                                    {
                                        //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Client({this.ip}:{this.user_id}) is now limited to the char selection screen restriction!");
                                        this.char_screen = true;
                                    }
                                    #endregion
                                }
                                #endregion

                                #region 0xb50e_SERVER_WEATHER_REPONSE
                                else if (opcode == 0xb50e)
                                {
                                    _pck.Lock();
                                    this.char_screen = false;

                                    //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Client({this.ip}:{this.user_id}) left char selection screen successfully!");
                                }
                                #endregion

                                #region 0xB150_CLIENT_ALCHEMY
                                else if (opcode == 0xb150)
                                {
                                    _pck.Lock();

                                    try
                                    {
                                        _pck.ReadUInt16();
                                        _pck.ReadUInt8();
                                        int slot = (int)_pck.ReadUInt8();
                                        _pck.ReadUInt64();
                                        int plus = (int)_pck.ReadUInt8();
                                        if (plus > 0)
                                        {
                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_HandlePlus] '{this.sql_charname}', {slot}, {plus}"));
                                        }
                                    }
                                    catch
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Error, "Error at 0xB150 parsing");
                                        Logger.WriteLine(Logger.LogLevel.Error, $"[S->C][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                    }
                                }
                                #endregion

                                #region 0x2113_SERVER_XTRAP_RESPONSE
                                else if (opcode == 0x2113)
                                {
                                    continue;
                                }
                                #endregion

                                /*else if(opcode == 0x30bf)
                                {
                                    _pck.Lock();

                                    if (FilterMain.ALIVE_BATTLE_ROYALE.Contains(this.charname))
                                    {
                                        UInt32 target = _pck.ReadUInt32();
                                        if (target == this.ID)
                                        {
                                            byte status = _pck.ReadUInt8();
                                            switch (status)
                                            {
                                                case 0x00:
                                                    {
                                                        byte stat = _pck.ReadUInt8();

                                                        // DEAD
                                                        if (stat == 0x02)
                                                        {
                                                            this.SendNotice($"[Battle Royale] You died in position #{FilterMain.ALIVE_BATTLE_ROYALE.Count}");
                                                            FilterMain.ALIVE_BATTLE_ROYALE.Remove(this.charname);

                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_EQUIP] '{this.charname}'"));



                                                            Packet insta_spawn = new Packet(0x3053);
                                                            insta_spawn.WriteUInt8(0x01);
                                                            m_LocalSecurity.Send(insta_spawn);
                                                            Send(false);

                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }*/

                                // Send packet
                                m_LocalSecurity.Send(_pck);
                                Send(false);
                            }
                        }
                    }
                    else
                    {
                        
                        try
                        {
                            // Abort connection
                            this.DisconnectModuleSocket();
                            this.m_TransferPoolThread.Abort();
                        }
                        catch { }

                        this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                        return;
                       
                    }
                    this.DoRecvFromServer();
                }
                catch(Exception e)
                {
                    try
                    {
                        // Abort connection
                        this.DisconnectModuleSocket();
                        this.m_TransferPoolThread.Abort();
                    }
                    catch { }

                    this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                    return;
                }
            }
        }

        #region Send(bool ToHost)
        void Send(bool ToHost)//that codes done by Excellency he fixed mbot for me
        {
            lock (m_Lock)
            {
                foreach (var p in (ToHost ? m_RemoteSecurity : m_LocalSecurity).TransferOutgoing())
                {

                    Socket ss = (ToHost ? m_ModuleSocket : m_ClientSocket);

                    ss.Send(p.Key.Buffer);
                    if (ToHost)
                    {
                        try
                        {
                            m_BytesRecvFromClient += (ulong)p.Key.Size;

                            double nBps = GetBytesPerSecondFromClient();
                            if (nBps > FilterMain.dMaxBytesPerSec_Agent)
                            {
                                try
                                {
                                    // Abort connection
                                    FilterMain.blocked_agent_dosattacks++;
                                    Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for flooding.");
                                    FirewallHandler.BlockIP(this.ip, "nBps count");
                                    this.DisconnectModuleSocket();
                                    this.m_TransferPoolThread.Abort();
                                }
                                catch { }

                                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                                return;
                            }
                        }
                        catch
                        {
                            try
                            {
                                // Abort connection
                                this.DisconnectModuleSocket();
                                this.m_TransferPoolThread.Abort();
                            }
                            catch { }

                            this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                            return;
                        }
                    }
                }
            }
        }
        #endregion

        #region Reset packet counter
        void ResetPackets(object e)
        {
            try
            {
                #region New shard exploit fix
                if (this.char_screen)
                {
                    if (this.PACKET_COUNT > 5)
                    {
                        FilterMain.blocked_agent_packetflood_charscreen++;
                        Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for sending to many packets in char screen!");
                        this.DisconnectModuleSocket();
                        return;
                    }
                }
                #endregion

                #region New BOT detection system
                if (this.PACKET_COUNT > 15)
                {
                    if (!FilterMain.BOT_LIST.Contains(this.user_id))
                    {
                        FilterMain.bots_found++;
                        try
                        {
                            Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_Bot] '{this.user_id}', '{this.ip}', 'PACKET_COUNT > 15'"));
                        }
                        catch { }
                        FilterMain.BOT_LIST.Add(this.user_id);
                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) was detected for using a bot!");
                        this.bot_login = DateTime.Now;
                    }
                }
                #endregion

                if (this.PACKET_COUNT > FilterMain.PACKET_COUNT)
                {
                    FilterMain.blocked_agent_packetflood++;
                    Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exceeding packet limit/second");
                    this.DisconnectModuleSocket();
                    return;
                }
            }
            catch { }
            this.PACKET_COUNT = 0;
        }
        #endregion

        #region Reward per hour
        void RewardPerHour(object e)
        {
            try
            {
                if (!this.first_time)
                {
                    if (FilterMain.REWARDPERHOUR_ENABLED)
                    {
                        int reward = 1;
                        if (FilterMain.REWARD_PC_LIMIT)
                        {
                            try
                            {
                                string new_user_id = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_GetRewardUsername] '{this.mac}'")).Result;
                                if (new_user_id != this.user_id)
                                {
                                    reward = 0;
                                }
                            }
                            catch { }
                        }

                        #region Reward toggle
                        if (reward > 0)
                        {
                            try
                            {
                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_RewardSystem] '{this.user_id}', '{this.CurLevel}', '{FilterMain.REWARDPERHOUR_ITEMID}', '{FilterMain.REWARDPERHOUR_OPTLEVEL}'"));
                                if (FilterMain.REWARDPERHOUR_INFORMPLAYER)
                                {
                                    this.SendNotice($"REWARD_TEXT");
                                }
                            }
                            catch { }
                        }
                        #endregion
                    }
                }
                this.first_time = false;
            }
            catch { }
        }
        #endregion

        #region ReverseString
        public String ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
        #endregion

        #region OnReceive_FromClient
        void OnReceive_FromClient(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = m_ClientSocket.EndReceive(iar);

                    if (nReceived != 0)
                    {

                        this.m_LocalSecurity.Recv(m_LocalBuffer, 0, nReceived);

                        List<Packet> ReceivedPackets = m_LocalSecurity.TransferIncoming();
                        if (ReceivedPackets != null)
                        {
                            foreach (Packet _pck in ReceivedPackets)
                            {
                                ushort opcode = Convert.ToUInt16(_pck.Opcode.ToString().ToLower());

                                //Logger.WriteLine(Logger.LogLevel.Error, $"[S->C][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                //Logger.WriteLine(Logger.LogLevel.Error, $"[{this.user_id}][{this.ip}][{opcode:X4}]");

                                #region UpdateOnlineUser
                                DateTime date = DateTime.Now;
                                int end_of_ip = Convert.ToInt16(this.ReverseString(this.ip.ToString()).Substring(0, 1));
                                int ip_ayrimi = end_of_ip % 3;
                                if (ip_ayrimi == 0)
                                {
                                    if (date.Second >= 0 && date.Second <= 20 && (date.Second % 10 == end_of_ip || date.Second % 10 == end_of_ip + 1 || date.Second % 10 == end_of_ip - 1))
                                    {
                                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_UpdateOnlineUser] '{this.charname}'"));
                                    }
                                }
                                else if (ip_ayrimi == 1)
                                {
                                    if (date.Second >= 20 && date.Second <= 40 && (date.Second % 10 == end_of_ip || date.Second % 10 == end_of_ip + 1 || date.Second % 10 == end_of_ip - 1))
                                    {
                                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_UpdateOnlineUser] '{this.charname}'"));
                                    }
                                }
                                else if (ip_ayrimi == 2)
                                {
                                    if (date.Second >= 40 && (date.Second % 10 == end_of_ip || date.Second % 10 == end_of_ip + 1 || date.Second % 10 == end_of_ip - 1))
                                    {
                                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_UpdateOnlineUser] '{this.charname}'"));
                                    }
                                }
                                #endregion

                                #region Opcode use
                                if (FilterMain.ALL_Opcodes.ContainsKey(opcode))
                                {
                                    FilterMain.ALL_Opcodes[opcode]++;
                                }
                                else
                                {
                                    FilterMain.ALL_Opcodes.Add(opcode, 1);
                                }
                                #endregion

                                #region PREVENT USERS FROM SENDING SERVER OPCODES TO AGENTSERVER
                                if (FilterMain.Server_opcodes.Contains(opcode))
                                {
                                    FilterMain.blocked_agent_injects++;
                                    this.last_opcode = opcode;
                                    this.SendNotice($"SERVER_OPCODE");
                                    //Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to send Server opcode({opcode:X4})");
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region PREVENT USER SENDING OPCODES TO AGENTSERVER BEFORE LOGIN SUCCESS
                                if (!this.user_logged_in)
                                {
                                    if (opcode != 0x5000 && opcode != 0x9000 && opcode != 0x2001 && opcode != 0x6103 && opcode != 0x2002)
                                    {
                                        FilterMain.blocked_agent_exploits++;
                                        //Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to use a Clientless exploit");
                                        //Logger.WriteLine(Logger.LogLevel.Error, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        Send(false);
                                        continue;
                                    }
                                }
                                #endregion

                                #region SHARD EXPLOIT FIX
                                if (this.char_screen)
                                {
                                    #region CLIENT_SAVE_BAR
                                    if (opcode == 0x7158)
                                    {
                                        try
                                        {
                                            switch (_pck.ReadUInt8())
                                            {
                                                case 0:
                                                    {
                                                        if (_pck.GetBytes().Length != 2)
                                                        {
                                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                            Send(false);
                                                            continue;
                                                        }

                                                        if (_pck.ReadUInt8() != 7)
                                                        {
                                                            Send(false);
                                                            continue;
                                                        }
                                                    }
                                                    break;

                                                case 1:
                                                    {
                                                        if (_pck.GetBytes().Length != 7)
                                                        {
                                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                            Send(false);
                                                            continue;
                                                        }
                                                        _pck.ReadUInt8(); // FromSlot
                                                        _pck.ReadUInt8(); // ToSlot
                                                        _pck.ReadUInt32(); // Item ID
                                                    }
                                                    break;

                                                default:
                                                    {
                                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                        Send(false);
                                                        continue;
                                                    }
                                                    break;
                                            }
                                            Send(false);
                                            continue;
                                        }

                                        catch
                                        {
                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                            Send(false);
                                            continue;
                                        }
                                    }
                                    #endregion

                                    #region Real exploit fix
                                    if (opcode != 0x2002 && opcode != 0x7001 && opcode != 0x7007 && opcode != 0x7450 && opcode != 0x2113 && opcode != 0x3012 && opcode != 0x750e)
                                    {
                                        FilterMain.blocked_agent_exploits++;
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        Send(false);
                                        continue;
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Protection stuff
                                this.length = _pck.GetBytes().Length;
                                if (opcode != 0x70ea)
                                {
                                    this.PACKET_COUNT++;
                                }
                                #endregion

                                #region CLIENT_HANDSHAKE
                                if (opcode == 0x5000 || opcode == 0x9000)
                                {
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x2001_CLIENT_PING_1
                                else if (opcode == 0x2001)
                                {
                                    this.DoRecvFromServer();

                                    //m_RemoteSecurity.Send(_pck);
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x2002_CLIENT_PING_2
                                else if (opcode == 0x2002)
                                {
                                    if (this.length > 0)
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, "0x2002 size problem, contact system admin.");
                                        Send(false);
                                        continue;
                                    }

                                    #region AFK Check
                                    if (this.is_online)
                                    {
                                        try
                                        {
                                            int afk_time = Convert.ToInt32(DateTime.Now.Subtract(this.afk_timer).TotalSeconds);
                                            if (afk_time > 60 * 3)
                                            {
                                                this.is_afk = true;
                                            }
                                            else
                                            {
                                                this.is_afk = false;
                                            }
                                        }
                                        catch { }

                                        if (this.is_afk)
                                        {
                                            Packet afk = new Packet(0x7402);
                                            afk.WriteUInt8((byte)2);
                                            m_RemoteSecurity.Send(afk);
                                            Send(true);

                                            this.afk_symbol = true;
                                        }

                                        /*#CLOSE
                                        if (FilterMain.BATTLE_ROYALE_SHRINK_AREA)
                                        {
                                            if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                            {
                                                if (!FilterMain.BATTLE_ROYALE_CHEATERS.Contains(this.charname))
                                                {
                                                    if (this.LatestRegion != 23980)
                                                    {
                                                        if (this.anti_cheat_royale > 15)
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE_LOG}].[dbo].[_ROYAL_DEAD] '{this.charname}'"));
                                                            this.SendNotice("You stayed in the restricted area for too long, you're disqualified from the match.");
                                                            FilterMain.BATTLE_ROYALE_CHEATERS.Add(this.charname);
                                                            this.anti_cheat_royale = 0;
                                                        }
                                                        this.anti_cheat_royale++;
                                                    }
                                                }
                                            }
                                        }
                                        */
                                    }
                                    #endregion

                                    /*CLOSE
                                    if (!this.sent_charspawn_sucess)
                                    {
                                        if (!FilterMain.FILES.Contains("jsro"))
                                        {
                                            Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for not sending (0x3012)");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }

                                    if (!this.sent_weather_request)
                                    {
                                        if (!FilterMain.FILES.Contains("jsro"))
                                        {
                                            Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for not sending (0x750E)");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }
                                    */

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7021_CLIENT_MOVE
                                else if (opcode == 0x7021)
                                {
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        //23980
                                        if (FilterMain.BATTLE_ROYALE.Contains(this.charname))
                                        {
                                            Send(false);
                                            continue;
                                        }

                                        if (FilterMain.BATTLE_ROYALE_CHEATERS.Contains(this.charname))
                                        {
                                            this.SendNotice("You stayed in the restrained area for too long, you're disqualified from the match.");
                                            Send(false);
                                            continue;
                                        }
                                    }
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x704c_CLIENT_HANDLER
                                else if (opcode == 0x704c)
                                {
                                  
                                    #region Other versions
                                    byte num = _pck.ReadUInt8();
                                    uint value = _pck.ReadUInt16();

                                    /*
                                    0D - InventorySlot
                                    EC - PotionType (EC: Normal | ED: ItemMall)
                                    09 - PotionID (08: HP | 09: ? | 10: MP)
                                    */
                                    switch (value)
                                    {
                                        #region Fortress Berserk potion
                                        case 0x40EC:
                                            {
                                                #region Disable berserk pot in pvp
                                                if (FilterMain.PVP_BERSERKPOT)
                                                {
                                                    if (this.pvp_cape)
                                                    {
                                                        //this.SendNotice("PVP_BERSERKPOT");
                                                        this.SendNotice("PVP Modda Zerk kullanımı kapalıdır!");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region Disable berserk pot in fortress
                                                if (FilterMain.FORTRESS_BERSERKPOT)
                                                {
                                                    if (this.is_fortress)
                                                    {
                                                        //this.SendNotice("FORTRESS_BERSERKPOT");
                                                        this.SendNotice("FORTRESS Modda Zerk kullanımı kapalıdır!");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region Disable berserk pot in job
                                                if (FilterMain.JOB_BERSERKPOT)
                                                {
                                                    if (this.char_job)
                                                    {
                                                        //this.SendNotice("JOB_BERSERKPOT");
                                                        this.SendNotice("JOB Modda Zerk kullanımı kapalıdır!");
                                                        continue;
                                                    }
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion

                                        #region Reverse scroll usage
                                        case 0x19ED://SCROLL
                                        //  case 0x19EC://PRE
                                            {
                                                #region Reverse disable in jobbing
                                                if (FilterMain.REVERSE_JOB_DISABLED)
                                                {
                                                    if (this.char_job)
                                                    {
                                                        //this.SendNotice("REVERSE_SCROLL_JOB_BLOCKED");
                                                        this.SendNotice("JOB Moddayken Reverse Scroll kullanımı kapalıdır!");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region Reverse level
                                                if (this.CurLevel < FilterMain.REVERSE_LEVEL)
                                                {
                                                    this.required_level = FilterMain.REVERSE_LEVEL;
                                                    //this.SendNotice("REVERSE_LEVEL_MESSAGE");
                                                    this.SendNotice("Reverse için leveliniz uygun değildir.");
                                                    continue;
                                                }
                                                #endregion

                                                #region Reverse delay
                                                if (FilterMain.REVERSE_DELAY > 0)
                                                {
                                                    try
                                                    {
                                                        int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.reverse_time).TotalSeconds);
                                                        if (gecensaniye < FilterMain.REVERSE_DELAY)
                                                        {
                                                            this.required_delay = (FilterMain.REVERSE_DELAY - gecensaniye);
                                                            //this.SendNotice("REVERSE_DELAY_MESSAGE");
                                                            this.SendNotice("Reverse için "+ this.required_delay + " saniye beklemelisiniz!");
                                                            continue;
                                                        }
                                                    }
                                                    catch { }
                                                    this.reverse_time = DateTime.Now;
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion

                                        #region Global chatting usage
                                        case 0x29ED:
                                        case 0x29EC:
                                            {
                                                string msg = _pck.ReadAscii();

                                                string message = msg.ToString().ToLower();

                                                if (message.Contains("'"))
                                                {
                                                    //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                    this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                    continue;
                                                }
                                                else if (message.Contains("\""))
                                                {
                                                    //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                    this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                continue;
                                                }

                                                #region ITEM LOCK
                                                if (this.is_locked && (FilterMain.ITEM_LOCK_GLOBAL))
                                                {
                                                    this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                                    continue;
                                                }
                                                #endregion

                                                #region BADWORD_BLOCKER
                                                if (Badword(msg))
                                                {
                                                    //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                    this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                    continue;
                                                }
                                                #endregion

                                                #region Global level
                                                if (this.CurLevel < FilterMain.GLOBAL_LEVEL)
                                                {
                                                    this.required_level = FilterMain.GLOBAL_LEVEL;
                                                    //this.SendNotice("GLOBAL_LEVEL_MESSAGE");
                                                    this.SendNotice("Global için leveliniz uygun değildir.");
                                                    continue;
                                                }
                                                #endregion

                                                #region Global delay
                                                if (FilterMain.GLOBAL_DELAY > 0)
                                                {
                                                    try
                                                    {
                                                        int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.global_time).TotalSeconds);
                                                        if (gecensaniye < FilterMain.GLOBAL_DELAY)
                                                        {
                                                            this.required_delay = (FilterMain.GLOBAL_DELAY - gecensaniye);
                                                            //this.SendNotice("GLOBAL_DELAY_MESSAGE");
                                                            this.SendNotice("Global için "+ this.required_delay + " saniye beklemelisiniz!");
                                                            continue;
                                                        }
                                                    }
                                                    catch { }
                                                    this.global_time = DateTime.Now;
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion

                                        #region Rescurrection scroll usage
                                        case 0x36ED:
                                        case 0x36EC:
                                            {
                                                #region Fortress resurrection scroll
                                                if (FilterMain.FORTRESS_RESURRECTIONSCROLL)
                                                {
                                                    if (this.is_fortress)
                                                    {
                                                        this.SendNotice("FORTRESS_RESURRECTIONSCROLL");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region Resurrection disable in jobbing
                                                if (FilterMain.RESCURRENT_JOB_DISABLED)
                                                {
                                                    if (this.char_job)
                                                    {
                                                        this.SendNotice("RESCURRENT_SCROLL_JOB_BLOCKED");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region Resurrection level
                                                if (this.CurLevel < FilterMain.RESCURRENT_LEVEL)
                                                {
                                                    this.required_level = FilterMain.RESCURRENT_LEVEL;
                                                    this.SendNotice("RESURRECTION_LEVEL_MESSAGE");
                                                    continue;
                                                }
                                                #endregion

                                                #region Resurrection delay
                                                if (FilterMain.RESCURRENT_DELAY > 0)
                                                {
                                                    try
                                                    {
                                                        int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.resurrection_time).TotalSeconds);
                                                        if (gecensaniye < FilterMain.RESCURRENT_DELAY)
                                                        {
                                                            this.required_level = (FilterMain.RESCURRENT_DELAY - gecensaniye);
                                                            this.SendNotice("RESURRECTION_DELAY_MESSAGE");
                                                            continue;
                                                        }
                                                    }
                                                    catch { }
                                                    this.resurrection_time = DateTime.Now;
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion

                                        #region Trade gameserver crasher fix
                                        case 0x11ED:
                                        case 0x11EC:
                                            {
                                                try
                                                {
                                                    int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.lastspawntime).TotalSeconds);
                                                    if (gecensaniye < FilterMain.TRADE_BUG_DELAY)
                                                    {
                                                        this.required_delay = (FilterMain.TRADE_BUG_DELAY - gecensaniye);
                                                        //this.SendNotice("PET_DELAY_MESSAGE");
                                                        this.SendNotice("Pet için " + this.required_delay +" saniye beklemelisiniz!");
                                                        continue;
                                                    }
                                                }
                                                catch
                                                {
                                                }

                                                this.lastspawntime = DateTime.Now;
                                                if (FilterMain.JOB_ADVANCED)
                                                {
                                                    if (this.job_thief)
                                                    {
                                                        this.thief_pickup = false;
                                                        if (!(IsAllowedRegion(this.selfWalkLatestRegion) && (this.selfWalkLatestRegion != -1)))
                                                        {
                                                            //this.SendNotice("JOB_THIEF_SPAWN_HORSE");
                                                            //this.SendNotice("JOB Moddayken pet için geçersiz bölgedesiniz!");
                                                            //continue;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Grab pet crasher fix
                                        case 0x10CD:
                                        case 0x10CC:
                                        case 0x08CD:
                                            {
                                                try
                                                {
                                                    int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.lastspawntime).TotalSeconds);
                                                    if (gecensaniye < FilterMain.TRADE_BUG_DELAY)
                                                    {
                                                        this.required_delay = (FilterMain.TRADE_BUG_DELAY - gecensaniye);
                                                        //this.SendNotice("PET_DELAY_MESSAGE");
                                                        this.SendNotice("Pet için " + this.required_delay +" saniye beklemelisiniz!");
                                                    continue;
                                                    }
                                                }
                                                catch
                                                {
                                                }

                                                this.lastspawntime = DateTime.Now;
                                            }
                                            break;
                                        #endregion

                                        #region Thief return scroll
                                        case 0x09EC:
                                        case 0x09ED:
                                            {
                                                if (FilterMain.JOB_ADVANCED)
                                                {
                                                    if (this.job_thief)
                                                    {
                                                        if (!(IsAllowedRegion(this.selfWalkLatestRegion) && (this.selfWalkLatestRegion != -1)))
                                                        {
                                                            //this.SendNotice("JOB_THIEF_USE_SCROLL");
                                                            //this.SendNotice("JOB Moddayken scroll için geçersiz bölgedesiniz!");
                                                            //continue;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        #endregion
                                        #region PILL_BUG_FIX
                                        case 0x316C:
                                        case 0x096C:
                                            {
                                                if (this.char_job)
                                                {
                                                    if (FilterMain.REVERSE_DELAY > 0)
                                                    {
                                                        try
                                                        {
                                                            int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.reverse_time).TotalSeconds);
                                                            if (gecensaniye < 8)
                                                            {
                                                                continue;
                                                            }
                                                        }
                                                        catch { }
                                                        this.reverse_time = DateTime.Now;
                                                    }
                                                }
                                            }
                                            break;
                                        #endregion
                                }
                                    #endregion
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7074_CLIENT_MAIN_ACTION
                                else if (opcode == 0x7074)
                                {
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE.Contains(this.charname))
                                        {
                                            Send(false);
                                            continue;
                                        }

                                        if (FilterMain.BATTLE_ROYALE_CHEATERS.Contains(this.charname))
                                        {
                                            this.SendNotice("You stayed in the restricted area for too long, you're disqualified from the match.");
                                            Send(false);
                                            continue;
                                        }
                                    }

                                    /*
                                        1 = Attack success
                                        2 = Cancel attack
                                    */
                                    byte action = _pck.ReadUInt8();
                                    switch (action)
                                    {
                                        case 1:
                                            {
                                                /*
                                                    1 = Normal attack
                                                    2 = Pickup / Cancel attack
                                                    3 = Trace
                                                    4 = Skill cast 
                                                */
                                                byte num = _pck.ReadUInt8();
                                                switch (num)
                                                {
                                                    #region Normal attack
                                                    case 1:
                                                        {
                                                            if (FilterMain.ATTACK_BUG_DELAY > 0)
                                                            {
                                                                this.attacktime = DateTime.Now;
                                                            }
                                                        }
                                                        break;
                                                    #endregion

                                                    #region Pickup
                                                    case 2:
                                                        {
                                                            if (FilterMain.JOB_ADVANCED)
                                                            {
                                                                /*if (this.job_thief && !(this.thief_pickup))
                                                                {
                                                                    this.SendNotice("JOB_THIEF_PICK_GOODS");
                                                                    continue;

                                                                }*/

                                                                //this.thief_pickup JOB CAVE FIX
                                                                if (this.job_thief && !(this.thief_pickup))
                                                                {
                                                                    if (!(IsAllowedRegion(this.selfWalkLatestRegion) && (this.selfWalkLatestRegion != -1)))
                                                                    {
                                                                        //this.SendNotice("JOB_THIEF_PICK_GOODS");
                                                                        //this.SendNotice("JOB Modda item almak için geçersiz bölgedesiniz!");
                                                                        //continue;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    #endregion

                                                    #region Trace
                                                    case 3:
                                                        {
                                                            if (FilterMain.BOT_LIST.Contains(this.user_id) && !(FilterMain.BOT_TRACE))
                                                            {
                                                                this.SendNotice("BOT_TRACE");
                                                                continue;
                                                            }

                                                            if (FilterMain.JOB_TRACE)
                                                            {
                                                                if (this.char_job)
                                                                {
                                                                    this.SendNotice("JOB_TRACE");
                                                                    continue;
                                                                }
                                                            }

                                                            if (FilterMain.FORTRESS_TRACE)
                                                            {
                                                                if (this.is_fortress)
                                                                {
                                                                    this.SendNotice("FORTRESS_TRACE");
                                                                    continue;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    #endregion

                                                    #region Skill cast
                                                    case 4:
                                                        {
                                                            ushort SkillID = _pck.ReadUInt16();

                                                            #region Fortress blocked skills
                                                            if (FilterMain.FORTRESS_BLOCKED_SKILLS)
                                                            {
                                                                if (this.is_fortress)
                                                                {
                                                                    if (FilterMain.fortress_region_skills.Contains(SkillID))
                                                                    {
                                                                        this.SendNotice("Kalede kullanımı yasak skill!");
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region JOB Blocked skills
                                                            if (FilterMain.JOB_BLOCKED_SKILLS)
                                                            {
                                                                if (this.char_job)
                                                                {
                                                                    if (FilterMain.job_region_skills.Contains(SkillID))
                                                                    {
                                                                        this.SendNotice("JOB Modda kullanımı yasak skill!");
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region Arena / CTF Blocked skills
                                                            if (FilterMain.TOWN_BLOCKED_SKILLS)
                                                            {
                                                                if (this.is_in_town_region) // 25000 - Jangan
                                                                {
                                                                    if (FilterMain.BlockedSkills.Contains(SkillID))
                                                                    {
                                                                        this.SendNotice("Kullanımı yasak skill!");
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region Event blocked skills
                                                            if (FilterMain.EVENT_BLOCKED_SKILLS)
                                                            {
                                                                if (this.is_in_event_region)
                                                                {
                                                                    if (FilterMain.event_region_skills.Contains(SkillID))
                                                                    {
                                                                        this.SendNotice("BLOCKED_SKILL_REGION_MESSAGE");
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region Bot blocked skills
                                                            if (FilterMain.BOT_BLOCKED_SKILLS)
                                                            {
                                                                if (FilterMain.BOT_LIST.Contains(this.user_id))
                                                                {
                                                                    if (this.is_in_bot_region)
                                                                    {
                                                                        this.SendNotice("BLOCKED_SKILL_BOT_REGION");
                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region Not even working :s
                                                            if (FilterMain.ATTACK_BUG_DELAY > 0)
                                                            {
                                                                try
                                                                {
                                                                    int timeleft = Convert.ToInt32((DateTime.Now.Subtract(this.attacktime)).TotalSeconds);
                                                                    if (timeleft < FilterMain.ATTACK_BUG_DELAY)
                                                                    {
                                                                        Send(false);
                                                                        continue;
                                                                    }
                                                                }
                                                                catch { }
                                                            }
                                                            #endregion
                                                        }
                                                        break;
                                                        #endregion
                                                }
                                            }
                                            break;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70c5_PET_MOVEMENT
                                else if (opcode == 0x70c5)
                                {
                                    if (this.job_question)
                                    {
                                        this.job_question = false;
                                    }
                                    this.selfWalkLatestRegion = -1;
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7045_CLIENT_SELECT_OBJECT
                                else if (opcode == 0x7045)
                                {
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7024_CLIENT_ANGLE_MOVE
                                else if (opcode == 0x7024)
                                {
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70a1_CLIENT_UP_SKILL
                                else if (opcode == 0x70a1)
                                {
                                    if (!FilterMain.ITEM_LOCK_UP_SKILL)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                }
                                #endregion

                                #region 0x2113_CLIENT_XTRAP
                                else if (opcode == 0x2113)
                                {
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x7158_CLIENT_SAVE_BAR
                                else if (opcode == 0x7158)
                                {
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x3053_CLIENT_STAND_UP
                                else if (opcode == 0x3053)
                                {
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                        {
                                            this.SendNotice("You are dead, wait for bot to teleport you.");
                                            continue;
                                        }
                                    }
                                }
                                #endregion

                                #region 0x70e3_CLIENT_MAKE_ALIAS
                                else if (opcode == 0x70e3)
                                {
                                    try
                                    {
                                        int derp = (FilterMain.PACKET_COUNT - 2) + this.PACKET_COUNT;
                                        if (derp >= FilterMain.PACKET_COUNT)
                                        {
                                            Send(false);
                                            continue;
                                        }

                                        _pck.ReadUInt32(); // NPC UNIQUE ID(FROM GS)
                                        byte shit = _pck.ReadUInt8();

                                        string alias = _pck.ReadAscii(); // NEW ALIAS
                                        if (alias.Contains("'") || alias.Contains("\""))
                                        {
                                            this.SendNotice("Nice try, not allowed symbol.");
                                            Send(false);
                                            continue;
                                        }
                                        else
                                        {
                                            m_RemoteSecurity.Send(_pck);
                                            Send(true);
                                            continue;
                                        }
                                    }
                                    catch
                                    {
                                        FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                        Send(false);
                                        continue;
                                    }
                                }
                                #endregion

                                #region 0x70e4_CLIENT_JOB_RANKING
                                else if (opcode == 0x70e4)
                                {
                                    try
                                    {
                                        int derp = (FilterMain.PACKET_COUNT - 2) + this.PACKET_COUNT;
                                        if (derp >= FilterMain.PACKET_COUNT)
                                        {
                                            Send(false);
                                            continue;
                                        }

                                        _pck.ReadUInt32();
                                        byte unk = _pck.ReadUInt8();
                                        byte unk2 = _pck.ReadUInt8();
                                        Logger.WriteLine(Logger.LogLevel.Debug, "0x70e4_CLIENT_JOB_RANKING: " + unk);
                                        if (unk != 1)
                                        {
                                            FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                            this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                            //Send(false);
                                            //continue;
                                        }

                                        if (unk2 > 1)
                                        {
                                            FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                            //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                            this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                            //Send(false);
                                            //continue;
                                        }
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }
                                    catch
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, "0x70e4_CLIENT_JOB_RANKING: catch ");
                                        FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                        //Send(false);
                                        //continue;
                                    }
                                }
                                #endregion

                                #region 0x70e6_CLIENT_PREVIOUS_JOB
                                else if (opcode == 0x70e6)
                                {
                                    try
                                    {
                                        int derp = (FilterMain.PACKET_COUNT - 2) + this.PACKET_COUNT;
                                        if (derp >= FilterMain.PACKET_COUNT)
                                        {
                                            Send(false);
                                            continue;
                                        }

                                        _pck.ReadUInt32();
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }
                                    catch
                                    {
                                        FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                        //Send(false);
                                        //continue;
                                    }
                                }
                                #endregion

                                #region 0x1420_BATTLE_ROYALE_PACKET
                                else if (opcode == 0x1420)
                                {
                                    string encryption_key = _pck.ReadAscii();
                                    if (encryption_key == "FUCK_YOU_YOU_UGGLY")
                                    {
                                        byte action = _pck.ReadUInt8();
                                        switch (action)
                                        {
                                            case 0x01:
                                                {
                                                    string charname = _pck.ReadAscii();
                                                    //string botchar = _pck.ReadAscii();
                                                    if (!FilterMain.BATTLE_ROYALE.Contains(charname))
                                                    {
                                                        FilterMain.BATTLE_ROYALE.Add(charname);
                                                    }

                                                    if (!FilterMain.BATTLE_ROYALE_ALIVE.Contains(charname))
                                                    {
                                                        FilterMain.BATTLE_ROYALE_ALIVE.Add(charname);
                                                    }
                                                }
                                                break;

                                            case 0x02:
                                                {
                                                    FilterMain.BATTLE_ROYALE.Clear();
                                                    FilterMain.BATTLE_ROYALE_CHEATERS.Clear();
                                                    FilterMain.BATTLE_ROYALE_SHRINK_AREA = false;
                                                }
                                                break;

                                            case 0x03:
                                                {
                                                    string charname = _pck.ReadAscii();
                                                    FilterMain.BATTLE_ROYALE.Remove(charname);
                                                    FilterMain.BATTLE_ROYALE_ALIVE.Remove(charname);
                                                    FilterMain.BATTLE_ROYALE_CHEATERS.Remove(charname);
                                                }
                                                break;
                                            case 0x04:
                                                {
                                                    string charname = _pck.ReadAscii();
                                                    FilterMain.BATTLE_ROYALE_ALIVE.Remove(charname);
                                                    FilterMain.BATTLE_ROYALE_CHEATERS.Remove(charname);
                                                }
                                                break;

                                            case 0x05:
                                                {
                                                    FilterMain.BATTLE_ROYALE_SHRINK_AREA = true;
                                                }
                                                break;

                                            case 0x06:
                                                {
                                                    FilterMain.BATTLE_ROYALE_SHRINK_AREA = false;
                                                    FilterMain.BATTLE_ROYALE_CHEATERS.Clear();
                                                }
                                                break;


                                        }
                                    }

                                    Send(false);
                                    continue;
                                }
                                #endregion

                                else if (opcode == 0x74df)
                                {
                                    //FilterMain.DiscordWebHook($"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                    this.SendNotice("Lütfen GM ile iletişime geçiniz!");
                                    Send(false);
                                    continue;
                                }

                                #region 0x6103_LOGIN_PACKET
                                else if (opcode == 0x6103)
                                {
                                    //uint32 == uint
                                    //UInt16 == short
                                    //string == ascii
                                    //UInt16 == short
                                    //string == ascii
                                    //UInt8 == byte
                                    //

                                    UInt32 uint32_1 = _pck.ReadUInt32(); //time
                                    //Logger.WriteLine(Logger.LogLevel.MikeMode, uint32_1.ToString());

                                    this.user_id = sqlCon.clean(_pck.ReadAscii().ToLower()); // Username
                                    //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Username: {sqlCon.clean(this.user_id)}");

                                    this.user_pw = sqlCon.clean(_pck.ReadAscii()); // Password
                                    //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Password: {this.user_pw}");

                                    byte locale = _pck.ReadUInt8(); // OperationType
                                    //Logger.WriteLine(Logger.LogLevel.MikeMode, $"Locale: {locale}");


                                    byte[] mac = _pck.ReadUInt8Array(6);
                                    string mac_address = "";
                                    int read_bytes = 0;
                                    int fail_count = 0;
                                    foreach (byte b in mac)
                                    {
                                        string macc = b.ToString("X2");
                                        if (macc == "00")
                                        {
                                            fail_count++;
                                        }
                                        mac_address += macc;
                                        if (read_bytes < 5)
                                        {
                                            mac_address += "-";
                                        }
                                        read_bytes++;
                                    }

                                    #region BOT_CONTROLL
                                    if (FilterMain.BOT_CONTROLL)
                                    {
                                        switch (locale)
                                        {
                                            case 22:
                                                {
                                                    if (!FilterMain.BOT_CONNECTION)
                                                    {
                                                        if (!FilterMain.GMs.Contains(this.user_id))
                                                        {
                                                            Logger.WriteLine(Logger.LogLevel.Warning, $"{this.user_id} Attemted to use third part tool to login to the server!");
                                                            this.DisconnectModuleSocket();
                                                            return;
                                                        }
                                                    }
                                                    FilterMain.bots_found++;
                                                    FilterMain.BOT_LIST.Add(this.user_id);
                                                    this.bot_login = DateTime.Now;
                                                }
                                                break;
                                            case 51:
                                                {
                                                    Packet login = new Packet(0x6103, true);
                                                    login.WriteUInt32(uint32_1);
                                                    login.WriteAscii(this.user_id);
                                                    login.WriteAscii(this.user_pw);
                                                    login.WriteUInt8(22);
                                                    login.WriteUInt8Array(mac);
                                                    m_RemoteSecurity.Send(login);
                                                    Send(true);
                                                    continue;
                                                }
                                                break;

                                            default:
                                                {
                                                    Logger.WriteLine(Logger.LogLevel.Error, $"Client disconnect, wrong locale id = {locale}");
                                                    this.DisconnectModuleSocket();
                                                    return;
                                                }
                                                break;
                                        }
                                    }
                                    #endregion

                                    #region New Bot detection system.
                                    if (FilterMain.Last_login.ContainsKey(this.user_id))
                                    {
                                        #region BOT DETECTION
                                        if (FilterMain.Last_login[this.user_id] != mac_address)
                                        {
                                            if (!FilterMain.BOT_LIST.Contains(this.user_id))
                                            {
                                                FilterMain.BOT_LIST.Add(this.user_id);
                                                FilterMain.bots_found++;
                                                //Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) was detected for using a bot!");
                                                this.bot_login = DateTime.Now;
                                                FilterMain.BOT_LIST.Add(this.user_id);
                                                try
                                                {
                                                    Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_Bot] '{this.user_id}', '{this.ip}', 'MAC_ADDRESS'"));
                                                }
                                                catch { }
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        FilterMain.Last_login.Add(this.user_id, mac_address);
                                    }
                                    #endregion

                                    #region IP_LIMIT
                                    // IP LIMIT DOUBLE CHECK
                                    if (FilterMain.IP_LIMIT > 0 && !(FilterMain.BYPASS.Contains(this.user_id)))
                                    {
                                        try
                                        {
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountIP] '{this.user_id}', '{this.ip}'")).Result;
                                            // Net CAFE ip
                                            if (FilterMain.CAFE_LIMIT > 0 && (FilterMain.CAFE.Contains(this.ip)))
                                            {

                                                if (current_count > FilterMain.CAFE_LIMIT)
                                                {
                                                    // Disconnect
                                                    this.DisconnectModuleSocket();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                // COUNT +1 BECAUSE ALWAYS 1 LESS
                                                if (current_count > FilterMain.IP_LIMIT)
                                                {
                                                    // Disconnect
                                                    this.DisconnectModuleSocket();
                                                    return;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7402_CLIENT_HELP_SYMBOL
                                else if (opcode == 0x7402)
                                {
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x70BA_CLIENT_STALL_ITEM
                                else if (opcode == 0x70ba && (FilterMain.WICKED_STALL_PRICE))
                                {
                                    byte status = _pck.ReadUInt8();
                                    switch (status)
                                    {
                                        #region Modify item in stall
                                        case 1:
                                            {
                                                this.SendNotice("STALL_WICKED_PRICES");
                                                continue;
                                            }
                                            break;
                                        #endregion

                                        #region Put item in stall.
                                        case 2:
                                            {
                                                _pck.ReadUInt8(); // Stall inventory slot
                                                int char_slot = _pck.ReadUInt8(); // Char inventory slot

                                                int item_stack = _pck.ReadUInt16(); // Item stack
                                                Int64 gold = Convert.ToInt64(_pck.ReadUInt64());
                                                gold = gold / item_stack;

                                                try
                                                {
                                                    Int64 arab = Task.Run(async () => await sqlCon.prod_int2($"EXEC [{FilterMain.sql_db}].[dbo].[_ReturnPrice] '{this.sql_charname}', {char_slot}, {gold}")).Result;
                                                    if (arab > 0)
                                                    {
                                                        this.required_level = arab;
                                                        this.SendNotice("STALL_WICKED_PRICE_OVERLIMIT");
                                                        continue;
                                                    }
                                                }
                                                catch { }

                                            }
                                            break;
                                            #endregion
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7450_UNKNOWN_CHARSCREEN
                                else if (opcode == 0x7450)
                                {
                                    if (!this.char_screen)
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7007_CHAR_SCREEN_SHIT
                                else if (opcode == 0x7007)
                                {
                                    if (!this.char_screen)
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    byte response = _pck.ReadUInt8();
                                    switch (response)
                                    {
                                        #region Create char
                                        case 1:
                                            {
                                                try
                                                {
                                                    _pck.ReadAscii(); // Charname

                                                    _pck.ReadUInt32(); // RefObjID

                                                    _pck.ReadUInt8(); // Height

                                                    _pck.ReadUInt32(); // ItemID
                                                    _pck.ReadUInt32(); // ItemID
                                                    _pck.ReadUInt32(); // ItemID
                                                    _pck.ReadUInt32(); // ItemID
                                                }
                                                catch
                                                {
                                                    FilterMain.blocked_agent_exploits++;
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                    Send(false);
                                                    continue;
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Char screen calling
                                        case 2:
                                            {
                                                if (this.length > 1)
                                                {
                                                    FilterMain.blocked_agent_exploits++;
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                    Send(false);
                                                    continue;
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Delete char by name
                                        case 3:
                                            {
                                                int name_length = _pck.ReadAscii().Length;

                                                if ((this.length - name_length) != 3)
                                                {
                                                    FilterMain.blocked_agent_exploits++;
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                    Send(false);
                                                    continue;
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Restore char by name
                                        case 4:
                                            {
                                                int name_length = _pck.ReadAscii().Length;

                                                if ((this.length - name_length) != 3)
                                                {
                                                    FilterMain.blocked_agent_exploits++;
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                    Send(false);
                                                    continue;
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Unknown
                                        case 5:
                                            {
                                                int name_length = _pck.ReadAscii().Length;

                                                if ((this.length - name_length) != 3)
                                                {
                                                    FilterMain.blocked_agent_exploits++;
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                    Send(false);
                                                    continue;
                                                }
                                            }
                                            break;
                                        #endregion

                                        #region Default
                                        default:
                                            {
                                                FilterMain.blocked_agent_exploits++;
                                                //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) attempted to use a Shard exploit");
                                                //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                                Send(false);
                                                continue;
                                            }
                                            break;
                                            #endregion
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7001_CHARNAME_PACKET
                                else if (opcode == 0x7001)
                                {
                                    if (this.charname_sent)
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    if (!this.char_screen)
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to send 0x7001 outside char screen!");
                                        Send(false);
                                        continue;
                                    }

                                    try
                                    {
                                        this.charname = sqlCon.clean(_pck.ReadAscii());
                                        if ((this.length - this.charname.Length) != 2)
                                        {
                                            Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to modify 0x7001!");
                                            Send(false);
                                            continue;
                                        }
                                        this.sql_charname = this.charname;

                                        #region [GM] SHIT
                                        if (this.charname.Contains("[") || this.charname.Contains("]"))
                                        {
                                            this.sql_charname = this.sql_charname.Replace("[", "%[%");
                                            this.sql_charname = this.sql_charname.Replace("]", "%]%");
                                            this.sql_charname = this.sql_charname.Replace(" ", string.Empty);
                                        }
                                        #endregion
                                        this.charname_sent = true;
                                    }
                                    catch
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to modify 0x7001!");
                                        Send(false);
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7060_PARTY_ADD_MEMBER
                                else if (opcode == 0x7060)
                                {
                                    #region Party region
                                    if (FilterMain.party_list.Contains(this.LatestRegion))
                                    {
                                        this.SendNotice("PARTY_EVENT_REGION_ADD");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7069_PARTY_MATCH_CREATE_REQUEST
                                else if (opcode == 0x7069)
                                {
                                    #region Party region
                                    if (FilterMain.party_list.Contains(this.LatestRegion))
                                    {
                                        this.SendNotice("PARTY_EVENT_REGION_CREATE");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x706d_PARTY_MATCH_JOIN_REQUEST
                                else if (opcode == 0x706d)
                                {

                                    #region Party region
                                    if (FilterMain.party_list.Contains(this.LatestRegion))
                                    {
                                        this.SendNotice("PARTY_EVENT_REGION_JOIN");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x750e_REQUEST_WEATHER
                                else if (opcode == 0x750e)
                                {
                                    #region Don't let them modify byte size, supose to be empty.
                                    if (this.length > 0)
                                    {
                                        FilterMain.blocked_agent_injects++;
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to modify 0x750E bytes.");
                                        Logger.WriteLine(Logger.LogLevel.Error, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    this.sent_weather_request = true;

                                    #region GM_START_VISIBLE
                                    if (FilterMain.GMs.Contains(this.user_id) && (FilterMain.GM_START_VISIBLE))
                                    {
                                        Packet Nigga = new Packet(0x7010);
                                        Nigga.WriteUInt16(14);
                                        m_RemoteSecurity.Send(Nigga);
                                        Send(false);
                                    }
                                    #endregion

                                    #region ROYLAE_ENABLE_CAPE
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE.Contains(this.charname))
                                        {
                                            Packet shit = new Packet(0x7516);
                                            shit.WriteUInt8(0x05);
                                            m_RemoteSecurity.Send(shit);
                                            Send(true);
                                        }

                                        if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                        {
                                            Packet leave_party = new Packet(0x7061);
                                            m_RemoteSecurity.Send(leave_party);
                                            Send(false);
                                        }
                                    }
                                    #endregion

                                    #region AFK system
                                    if (this.afk_symbol)
                                    {
                                        Packet afk = new Packet(0x7402);
                                        afk.WriteUInt8((byte)0);
                                        m_RemoteSecurity.Send(afk);
                                        Send(true);

                                        this.afk_symbol = false;
                                    }
                                    #endregion

                                    #region REMOVE_PARTY_IF_REGION
                                    if (FilterMain.party_list.Contains(this.LatestRegion))
                                    {
                                        Packet leave_party = new Packet(0x7061);
                                        m_RemoteSecurity.Send(leave_party);
                                        Send(false);
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7516_CLIENT_ENTER_PVP
                                else if (opcode == 0x7516)
                                {
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                        {
                                            Send(false);
                                            continue;
                                        }
                                    }

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && !(FilterMain.BOT_PVP))
                                    {
                                        this.SendNotice("BOT_ENTER_PVP");
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7517_CLIENT_NAMECHANGE_274
                                else if (opcode == 0x7517)
                                {
                                    if (!FilterMain.FILES.Contains("274"))
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    if (_pck.ReadUInt8() == 1)
                                    {
                                        if (_pck.ReadUInt8() == 3)
                                        {
                                            if (this.new_charname != null)
                                            {
                                                this.charname = this.new_charname;
                                                this.sql_charname = this.charname;

                                                #region [GM] SHIT
                                                if (this.charname.Contains("[") || this.charname.Contains("]"))
                                                {
                                                    this.sql_charname = this.sql_charname.Replace("[", "%[%");
                                                    this.sql_charname = this.sql_charname.Replace("]", "%]%");
                                                    this.sql_charname = this.sql_charname.Replace(" ", string.Empty);
                                                }
                                                #endregion

                                                this.new_charname = null;
                                            }
                                        }
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion  

                                #region 0x3012_WELCOME_MESSAGE
                                else if (opcode == 0x3012)
                                {
                                    #region Don't let them modify byte size, supose to be empty.
                                    if (this.length > 0)
                                    {
                                        FilterMain.blocked_agent_injects++;
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to modify 0x3012 bytes.");
                                        Logger.WriteLine(Logger.LogLevel.Error, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    this.sent_charspawn_sucess = true;

                                    if (!this.startnotice)
                                    {
                                        #region PC_LIMIT
                                        if (FilterMain.PC_LIMIT > 0 && !(FilterMain.names.Contains(this.user_id)))
                                        {
                                            try
                                            {
                                                this.mac = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_ReturnMac] '{this.user_id}'")).Result;
                                                this.hwid = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_ReturnHWID] '{this.user_id}'")).Result;
                                                this.serial = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_ReturnSERIAL] '{this.user_id}'")).Result;
                                                if (this.mac == "non")
                                                {
                                                    //this.SendNotice("PC_LIMIT_HARDWARE_ID_INCORRECT");
                                                    this.SendNotice("PC Limit için geçersiz kimlik bilgisi!");
                                                    this.DisconnectModuleSocket();
                                                    return;
                                                }

                                                int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountHWID] '{this.user_id}', '{this.mac}'")).Result + 1;
                                                if (current_count > FilterMain.PC_LIMIT)
                                                {
                                                    //this.SendNotice("PC_LIMIT_COUNT_MAX");
                                                    this.SendNotice("PC Limit aşıldı!");
                                                    this.DisconnectModuleSocket();
                                                    return;
                                                }
                                            }
                                            catch (Exception Ex)
                                            {
                                                Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) Error({Ex.ToString()})");
                                                //this.SendNotice("PC_LIMIT_HARDWARE_ID_INCORRECT");
                                                this.SendNotice("PC Limit için geçersiz kimlik bilgisi!");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                        }
                                        #endregion

                                        #region IP_LIMIT
                                        if (FilterMain.IP_LIMIT > 0 && !(FilterMain.BYPASS.Contains(this.user_id)))
                                        {
                                            try
                                            {
                                                int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountIP] '{this.user_id}', '{this.ip}'")).Result + 1; // +1 because current connection is 1.
                                                if (FilterMain.CAFE_LIMIT > 0 && (FilterMain.CAFE.Contains(this.ip)))
                                                {
                                                    // COUNT +1 BECAUSE ALWAYS 1 LESS
                                                    if (current_count > FilterMain.CAFE_LIMIT)
                                                    {
                                                        this.SendNotice("Cafe IP Limit aşıldı!");
                                                        // Disconnect
                                                        this.DisconnectModuleSocket();

                                                        // Continue
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    // COUNT +1 BECAUSE ALWAYS 1 LESS
                                                    if (current_count > FilterMain.IP_LIMIT)
                                                    {
                                                        //this.SendNotice("IP_LIMIT_COUNT_MAX");
                                                        this.SendNotice("IP Limit aşıldı!");
                                                        // Disconnect
                                                        this.DisconnectModuleSocket();

                                                        // Continue
                                                        return;
                                                    }
                                                }
                                            }
                                            catch (Exception Ex)
                                            {
                                                Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) Error({Ex.ToString()})");
                                                //this.SendNotice("IP_LIMIT_ERROR");
                                                this.SendNotice("IP Limit Hatası!");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                        }
                                        #endregion

                                        #region SQL stuff, log player.
                                        try
                                        {
                                            //HERE
                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_LogPlayers] '{this.user_id}', '{this.charname}', {this.ID}, '{this.ip}', '{this.mac}', '{this.serial}', '{this.hwid}', 1, {FilterMain.agent_mports}"));
                                        }
                                        catch
                                        {
                                            //this.SendNotice("PC_LIMIT_ERROR");
                                            this.SendNotice("PC Limit Hatası!");
                                            Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) had a problem at 0x3012, disconnected.");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                        #endregion

                                        if (FilterMain.WELCOME_MSG.Length > 3)
                                        {
                                            string msg = FilterMain.WELCOME_MSG;
                                            msg = msg.Replace("{charname}", this.charname);
                                            msg = msg.Replace("{user_id}", this.user_id);
                                            this.SendNotice(msg);
                                        }

                                        #region SPECIAL_MESSAGE
                                        if (FilterMain.PM_TICKET)
                                        {
                                            bool smessage = true;
                                            while (smessage)
                                            {
                                                string last_msg = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_GetTicket] '{this.sql_charname}'")).Result;
                                                if (last_msg.Length > 3)
                                                {
                                                    this.SendPMToOwn("TICKET2019", last_msg);
                                                }
                                                else
                                                {
                                                    smessage = false;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    this.startnotice = true;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70f1_CLIENT_GUILD_DISBAND
                                else if (opcode == 0x70f1)
                                {
                                    if (!FilterMain.ITEM_LOCK_GUILD_DISBAND)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7258_CLIENT_GUILD_DONATE_GP
                                else if (opcode == 0x7258)
                                {
                                    if (!FilterMain.ITEM_LOCK_GUILD_DONATE_GP)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70f4_CLIENT_GUILD_KICK
                                else if (opcode == 0x70f4)
                                {
                                    if (!FilterMain.ITEM_LOCK_GUILD_KICK)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7050_CLIENT_UP_STR
                                else if (opcode == 0x7050)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_UP_STR)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                   

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7051_CLIENT_UP_INT
                                else if (opcode == 0x7051)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_UP_INT)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70b4_CLIENT_STALL_BUY
                                else if (opcode == 0x70b4)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_STALL)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70f2_CLIENT_GUILD_LEAVE
                                else if (opcode == 0x70f2)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_GUILD_LEAVE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70fa_CLIENT_GUILD_PROMOTE
                                else if (opcode == 0x70fa)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_GUILD_PROMOTE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7104_CLIENT_GUILD_PROMOTE_2
                                else if (opcode == 0x7104)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_GUILD_PROMOTE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7250_CLIENT_GUILD_STORAGE
                                else if (opcode == 0x7250)
                                {
                                    
                                    if (!FilterMain.ITEM_LOCK_GUILD_STORAGE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7252_CLIENT_GUILD_STORAGE_2
                                else if (opcode == 0x7252)
                                {
                                    /*CLOSE
                                    if (!FilterMain.ITEM_LOCK_GUILD_STORAGE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked)
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    */
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7081_CLIENT_EXCHANGE
                                else if (opcode == 0x7081)
                                {
                                    
                                    #region Disable zerk in Royale
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                        {
                                            this.SendNotice("[Battle Royale] Exchange is disabled in Royale match");
                                            continue;
                                        }
                                    }
                                    #endregion
                                    

                                    /*CLOSE
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_EXCHANGE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    this.StallBlock = true;

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_EXCHANGE))
                                    {
                                        this.SendNotice("BOT_EXCHANGE_REQUEST");
                                        continue;
                                    }

                                    #region Exchange level
                                    if (this.CurLevel < FilterMain.EXCHANGE_LEVEL)
                                    {
                                        this.required_level = FilterMain.EXCHANGE_LEVEL;
                                        this.SendNotice("EXCHANGE_LEVEL_MESSAGE");
                                        continue;
                                    }
                                    #endregion
                                    */

                                    #region Exchange delay
                                    if (FilterMain.EXCHANGE_DELAY > 0)
                                    {
                                        try
                                        {
                                            _pck.ReadUInt8();
                                            int okuy = (int)_pck.ReadUInt8();
                                            if (okuy >= 1)
                                            {
                                                int gecensaniye = Convert.ToInt32((DateTime.Now.Subtract(this.lastexchangetime)).TotalSeconds);
                                                if (gecensaniye < FilterMain.EXCHANGE_DELAY)
                                                {
                                                    this.required_delay = (FilterMain.EXCHANGE_DELAY - gecensaniye);
                                                    //this.SendNotice("EXCHANGE_DELAY_MESSAGE");
                                                    this.SendNotice("Exchange için "+ this.required_delay + " saniye beklemelisiniz!");
                                                    continue;
                                                }
                                            }
                                        }
                                        catch { }
                                        this.lastexchangetime = DateTime.Now;
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7461_STALL_NETWORK_JSRO
                                else if (opcode == 0x7461)
                                {
                                    if (!FilterMain.FILES.Contains("jsro"))
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    this.InstallNetwork = true;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7082_EXCHANGE_ACCEPT
                                else if (opcode == 0x7082)
                                {
                                    
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_EXCHANGE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    this.StallBlock = true;

                                    if (this.InstallNetwork && FilterMain.FILES.Contains("jsro"))
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to use Stall network exploit!");
                                        this.SendNotice("JSRO_EXCHANGE_EXPLOIT");
                                        continue;
                                    }

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_EXCHANGE))
                                    {
                                        this.SendNotice("BOT_EXCHANGE_REQUEST");
                                        continue;
                                    }

                                    #region Exchange level
                                    if (this.CurLevel < FilterMain.EXCHANGE_LEVEL)
                                    {
                                        this.required_level = FilterMain.EXCHANGE_LEVEL;
                                        this.SendNotice("EXCHANGE_LEVEL_MESSAGE");
                                        continue;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7083_EXCHANGE_APPROVE
                                else if (opcode == 0x7083)
                                {
                                    
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_EXCHANGE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    this.StallBlock = true;

                                    if (this.InstallNetwork && FilterMain.FILES.Contains("jsro"))
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to use Stall network exploit!");
                                        this.SendNotice("JSRO_EXCHANGE_EXPLOIT");
                                        continue;
                                    }
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7084_CLIENT_EXCHANGE_CLOSE
                                else if (opcode == 0x7084)
                                {
                                    this.StallBlock = true;
                                    this.InstallNetwork = false;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70B1_CLIENT_STALL
                                else if (opcode == 0x70B1)
                                {
                                    
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_STALL))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    this.InStall = true;
                                    
                                    #region BOT STALL
                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_STALL))
                                    {
                                        this.SendNotice("BOT_OPEN_STALL");
                                        continue;
                                    }
                                    #endregion
                                    
                                    #region STALL BLOCK OUTSIDE TOWN
                                    if (!this.is_in_town_region)
                                    {
                                        //this.SendNotice("STALL_BLOCK_OUTSIDE_TOWN");
                                        this.SendNotice("Şehir dışında stall açı");
                                        continue;
                                    }
                                    #endregion
                                    
                                    #region Stall exploit fix
                                    if (this.StallBlock && FilterMain.FILES.Contains("vsro"))
                                    {
                                        this.SendNotice("STALL_REQUIRE_TELEPORT");
                                        continue;
                                    }
                                    #endregion

                                    #region Stall level
                                    if (this.CurLevel < FilterMain.STALL_LEVEL)
                                    {
                                        this.required_level = FilterMain.STALL_LEVEL;
                                        //this.SendNotice("STALL_LEVEL_MESSAGE");
                                        this.SendNotice("Stall için leveliniz uygun değildir!");
                                        continue;
                                    }
                                    #endregion

                                    #region Stall delay
                                    if (FilterMain.STALL_DELAY > 0)
                                    {
                                        try
                                        {
                                            int gecensaniye = Convert.ToInt32((DateTime.Now.Subtract(this.laststalltime)).TotalSeconds);
                                            if (gecensaniye < FilterMain.STALL_DELAY)
                                            {
                                                this.required_delay = (FilterMain.STALL_DELAY - gecensaniye);
                                                //this.SendNotice("STALL_DELAY_MESSAGE");
                                                this.SendNotice("Stall açmak için "+ this.required_delay + " saniye beklemelisiniz!");
                                                continue;
                                            }
                                        }
                                        catch { }
                                        this.laststalltime = DateTime.Now;

                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70b2_CLIENT_STALL_CLOSE
                                else if (opcode == 0x70b2)
                                {
                                    this.InStall = false;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70b5_CLIENT_STALL_OTHER_CLOSE
                                else if (opcode == 0x70b5)
                                {
                                    
                                    if (!FilterMain.FILES.Contains("jsro"))
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }
                                    else
                                    {
                                        if (this.InStall)
                                        {
                                            FilterMain.blocked_agent_injects++;
                                            //Logger.WriteLine(Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to use stall exploit!");
                                            Packet Rekt = new Packet(0x70b2);
                                            m_RemoteSecurity.Send(Rekt);
                                            Send(false);
                                            continue;
                                        }
                                    }
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70a7_ZERK_PACKET
                                else if (opcode == 0x70a7)
                                {
                                    
                                    #region Invisible exploit fix
                                    if (_pck.ReadUInt8() != 1)
                                    {
                                        
                                        FilterMain.blocked_agent_injects++;
                                        //Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for invisible inject");
                                        FirewallHandler.BlockIP(this.ip, "Invisible exploit");
                                        this.DisconnectModuleSocket();
                                        continue;
                                    }
                                    #endregion
                                    
                                    #region Disable zerk in Royale
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                        {
                                            this.SendNotice("[Battle Royale Berserk status is disabled in Royale");
                                            continue;
                                        }
                                    }
                                    #endregion
                                    
                                    #region Disable zerk in fortress
                                    if (FilterMain.FORTRESS_ZERK)
                                    {
                                        if (this.is_fortress)
                                        {
                                            this.SendNotice("FORTRESS_ZERK");
                                            continue;
                                        }
                                    }
                                    #endregion
                                    
                                    #region Disable zerk in pvp mode
                                    if (FilterMain.PVP_ZERK)
                                    {
                                        if (this.pvp_cape)
                                        {
                                            this.SendNotice("PVP_ZERK");
                                            continue;
                                        }
                                    }
                                    #endregion
                                    
                                    #region Disable zerk in specific regions
                                    if (FilterMain.zerk_list.Contains(this.LatestRegion))
                                    {
                                        this.SendNotice("ZERK_REGION_BLOCKED");
                                        continue;
                                    }
                                    #endregion
                                    
                                    #region Disable Zerk during JOB
                                    if (FilterMain.JOB_ZERK)
                                    {
                                        if (this.char_job)
                                        {
                                            this.SendNotice("ZERK_JOB_BLOCKED");
                                            continue;
                                        }
                                    }
                                    #endregion

                                    #region Invisible exploite   fix
                                    /*
                                    if (_pck.ReadUInt8() != 1)
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, "0x70a7_ZERK_PACKET 3");
                                        FilterMain.blocked_agent_injects++;
                                        Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for invisible inject");
                                        FirewallHandler.BlockIP(this.ip, "Invisible exploit");
                                        this.DisconnectModuleSocket();
                                        continue;
                                    }
                                    */
                                    #endregion
                                   
                                    #region Zerk level
                                    if (this.CurLevel < FilterMain.ZERK_LEVEL)
                                    {
                                        this.required_level = FilterMain.ZERK_LEVEL;
                                        this.SendNotice("ZERK_LEVEL_MESSAGE");
                                        continue;
                                    }
                                    #endregion
                                    
                                    #region Zerk delay
                                    if (FilterMain.ZERK_DELAY > 0)
                                    {
                                        try
                                        {
                                            int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.lastzerktime).TotalSeconds);
                                            if (gecensaniye < FilterMain.ZERK_DELAY)
                                            {
                                                this.required_delay = (FilterMain.ZERK_DELAY - gecensaniye);
                                                this.SendNotice("Zerk için " + this.required_delay + " saniye beklemelisiniz!");
                                                continue;
                                            }
                                        }
                                        catch { }
                                        this.lastzerktime = DateTime.Now;
                                    }
                                    #endregion
                                    
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x715F_CLIENT_PREMIUM_274
                                else if (opcode == 0x715f)
                                {
                                    #region vSRO274
                                    if (FilterMain.FILES.Contains("274"))
                                    {
                                        UInt32 unk = _pck.ReadUInt32(); // Premium ID
                                        UInt32 unk1 = _pck.ReadUInt32(); // Function ID
                                                                         // uint32 wRegionID aka warp region
                                        switch (unk1)
                                        {
                                            // Reverse scroll
                                            case 3795:
                                                {
                                                    #region Reverse disable in jobbing
                                                    if (FilterMain.REVERSE_JOB_DISABLED && (this.char_job))
                                                    {
                                                        //this.SendNotice("REVERSE_SCROLL_JOB_BLOCKED");
                                                        this.SendNotice("JOB Moddayken Reverse Scroll kullanımı kapalıdır!");
                                                        continue;
                                                    }
                                                    #endregion

                                                    #region Reverse level
                                                    if (this.CurLevel < FilterMain.REVERSE_LEVEL)
                                                    {
                                                        this.required_level = FilterMain.REVERSE_LEVEL;
                                                        this.SendNotice("REVERSE_LEVEL_MESSAGE");
                                                        continue;
                                                    }
                                                    #endregion

                                                    #region Reverse delay
                                                    if (FilterMain.REVERSE_DELAY > 0)
                                                    {
                                                        try
                                                        {
                                                            int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.reverse_time).TotalSeconds);
                                                            if (gecensaniye < FilterMain.REVERSE_DELAY)
                                                            {
                                                                this.required_delay = (FilterMain.REVERSE_DELAY - gecensaniye);
                                                                this.SendNotice("REVERSE_DELAY_MESSAGE");
                                                                continue;
                                                            }
                                                        }
                                                        catch { }
                                                        this.reverse_time = DateTime.Now;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                        }
                                    }
                                    #endregion

                                    #region Other versions
                                    else
                                    {
                                        UInt32 unk = _pck.ReadUInt32(); // Premium ID
                                        UInt32 unk1 = _pck.ReadUInt32(); // Function ID

                                        //Logger.WriteLine($"{unk1}");

                                        switch (unk1)
                                        {
                                            #region Reverse scroll
                                              case 3795:
                                                  {
                                                      #region Reverse disable in jobbing
                                                      if (FilterMain.REVERSE_JOB_DISABLED && (this.char_job))
                                                      {
                                                            this.SendNotice("JOB Moddayken Reverse Scroll kullanımı kapalıdır!");
                                                            //this.SendNotice("REVERSE_SCROLL_JOB_BLOCKED");
                                                            continue;
                                                      }
                                                      #endregion

                                                      #region Reverse level
                                                      if (this.CurLevel < FilterMain.REVERSE_LEVEL)
                                                      {
                                                          this.required_level = FilterMain.REVERSE_LEVEL;
                                                          this.SendNotice("REVERSE_LEVEL_MESSAGE");
                                                          continue;
                                                      }
                                                      #endregion

                                                      #region Reverse delay
                                                      if (FilterMain.REVERSE_DELAY > 0)
                                                      {
                                                          try
                                                          {
                                                              int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.reverse_time).TotalSeconds);
                                                              if (gecensaniye < FilterMain.REVERSE_DELAY)
                                                              {
                                                                  this.required_delay = (FilterMain.REVERSE_DELAY - gecensaniye);
                                                                  this.SendNotice("REVERSE_DELAY_MESSAGE");
                                                                  continue;
                                                              }
                                                          }
                                                          catch { }
                                                          this.reverse_time = DateTime.Now;
                                                      }
                                                      #endregion
                                                  }
                                                  break;
                                            #endregion

                                            #region Resurrection scroll
                                            case 3783:
                                                {
                                                    #region Fortress resurrection scroll
                                                    if (FilterMain.FORTRESS_RESURRECTIONSCROLL)
                                                    {
                                                        if (this.is_fortress)
                                                        {
                                                            this.SendNotice("FORTRESS_RESURRECTIONSCROLL");
                                                            continue;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Resurrection disable in jobbing
                                                    if (FilterMain.RESCURRENT_JOB_DISABLED)
                                                    {
                                                        if (this.char_job)
                                                        {
                                                            this.SendNotice("RESCURRENT_SCROLL_JOB_BLOCKED");
                                                            continue;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Resurrection level
                                                    if (this.CurLevel < FilterMain.RESCURRENT_LEVEL)
                                                    {
                                                        this.required_level = FilterMain.RESCURRENT_LEVEL;
                                                        this.SendNotice("RESURRECTION_LEVEL_MESSAGE");
                                                        continue;
                                                    }
                                                    #endregion

                                                    #region Resurrection delay
                                                    if (FilterMain.RESCURRENT_DELAY > 0)
                                                    {
                                                        try
                                                        {
                                                            int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.resurrection_time).TotalSeconds);
                                                            if (gecensaniye < FilterMain.RESCURRENT_DELAY)
                                                            {
                                                                this.required_level = (FilterMain.RESCURRENT_DELAY - gecensaniye);
                                                                this.SendNotice("RESURRECTION_DELAY_MESSAGE");
                                                                continue;
                                                            }
                                                        }
                                                        catch { }
                                                        this.resurrection_time = DateTime.Now;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                                #endregion
                                        }

                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7034_CLIENT_MOVE_ITEM
                                else if (opcode == 0x7034)
                                {
                                    byte action = _pck.ReadUInt8();
                                    switch (action)
                                    {
                                        case 27:
                                            {
                                                _pck.ReadUInt32();
                                                _pck.ReadUInt8();
                                                byte to_slot = _pck.ReadUInt8();
                                                if (to_slot >= 56)
                                                {
                                                    //Logger.WriteLine(Logger.LogLevel.Error, "You have more than 2 grab pets, this can crash your SR_GameServer.exe");
                                                    //this.SendNotice("GRAB_PET_INVENTORY_MAX");
                                                    this.SendNotice("Pet çantası doludur!");
                                                    continue;
                                                }
                                            }
                                            break;
                                        case 0:
                                            {
                                                byte slot = _pck.ReadUInt8();
                                                switch (slot)
                                                {
                                                    case 0:
                                                    case 1:
                                                    case 2:
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                    case 9:
                                                    case 10:
                                                    case 11:
                                                    case 12:
                                                        {
                                                            if (FilterMain.BATTLE_ROYALE_MODE)
                                                            {
                                                                Send(false);
                                                                continue;
                                                            }
                                                        }
                                                        break;
                                                }
                                                byte char_slot = _pck.ReadUInt8();
                                                switch (char_slot)
                                                {
                                                    case 0:
                                                    case 1:
                                                    case 2:
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                    case 9:
                                                    case 10:
                                                    case 11:
                                                    case 12:
                                                        {
                                                            if (FilterMain.BATTLE_ROYALE_MODE)
                                                            {
                                                                Send(false);
                                                                continue;
                                                            }
                                                        }
                                                        break;


                                                    case 8:
                                                        {

                                                            #region JOB BOT DISABLED
                                                            if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_JOBBING))
                                                            {
                                                                Logger.WriteLine(Logger.LogLevel.Error, "BOT_ENTER_JOBSUIT1");
                                                                this.SendNotice("BOT_ENTER_JOBSUIT");
                                                                continue;
                                                            }
                                                            #endregion

                                                            #region PC LIMIT JOB_CAMELS<3
                                                            if (FilterMain.JOB_PC_LIMIT > 0)
                                                            {
                                                                try
                                                                {
                                                                    int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountJOB] '{this.mac}'")).Result + 1;
                                                                    if (current_count > FilterMain.JOB_PC_LIMIT)
                                                                    {
                                                                        this.SendNotice("Job için PC limit aşılmıştır!");
                                                                        continue;
                                                                    }
                                                                }
                                                                catch { }
                                                            }
                                                            #endregion
                                                        }
                                                        break;
                                                }
                                            }
                                            break;

                                        // Buying items from NPC
                                        case 8:
                                            {
                                                #region ITEM LOCK
                                                if (this.is_locked && (FilterMain.ITEM_LOCK_BUY_ITEM))
                                                {
                                                    this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                                    continue;
                                                }
                                                #endregion
                                            }
                                            break;

                                        // Selling items to NPC
                                        case 9:
                                            {
                                                int char_slot = _pck.ReadUInt8();

                                                if (FilterMain.ALEXUS_GOLD_COIN)
                                                {
                                                    Int32 ItemID = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 7, {char_slot}")).Result;
                                                    if (ItemID == 24670)
                                                    {
                                                        ushort stack_count = _pck.ReadUInt16();
                                                        UInt32 unk = _pck.ReadUInt32();

                                                        if (stack_count > 1 && stack_count < 5)
                                                        {
                                                            this.SendNotice("You can only sell 1 and 1 or 5 and 5");
                                                            continue;
                                                        }

                                                        if (stack_count >= 5)
                                                        {
                                                            this.SendNotice("Selling 5 gold coins to NPC");
                                                            for (int i = 0; i < 5; i++)
                                                            {
                                                                Packet sell_shit = new Packet(0x7034);
                                                                sell_shit.WriteUInt8(0x09);
                                                                sell_shit.WriteUInt8((byte)char_slot);
                                                                sell_shit.WriteUInt16((byte)1);
                                                                sell_shit.WriteUInt32(unk);
                                                                m_RemoteSecurity.Send(sell_shit);
                                                                Send(true);
                                                            }
                                                            continue;
                                                        }
                                                    }
                                                }

                                                #region ITEM LOCK
                                                if (this.is_locked && (FilterMain.ITEM_LOCK_SELL_ITEM))
                                                {
                                                    this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                                    continue;
                                                }
                                                #endregion
                                            }
                                            break;

                                        case 7:
                                        case 10:
                                            {
                                                #region ITEM LOCK
                                                if (this.is_locked && (FilterMain.ITEM_LOCK_DROP_STUFF))
                                                {
                                                    this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                                    continue;
                                                }
                                                #endregion

                                                #region Battle royale
                                                if (FilterMain.BATTLE_ROYALE_MODE)
                                                {
                                                    if (FilterMain.BATTLE_ROYALE_ALIVE.Contains(this.charname))
                                                    {
                                                        this.SendNotice("You are not allowed to drop gold/items inside Battle Royale!");
                                                        continue;
                                                    }
                                                }
                                                #endregion

                                                #region TOWNS
                                                if (this.is_in_town_region)
                                                {
                                                    //this.SendNotice("TOWN_DROP_ITEM_MESSAGE");
                                                    this.SendNotice("Şehirde yere item atamazsınız!");
                                                    continue;
                                                }
                                                #endregion
                                            }
                                            break;
                                        //BUYING JOB GOODS
                                        case 19:
                                            {
                                                try
                                                {
                                                    if (FilterMain.PHBOT_LOCK)
                                                    {
                                                        if (!this.job_question)
                                                        {
                                                            string message = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_JobQuestion] '{this.sql_charname}'")).Result;
                                                            this.SendPMToOwn("BOT2019", message);
                                                            Send(false);
                                                            continue;
                                                        }
                                                    }
                                                }
                                                catch { }
                                            }
                                            break;
                                        //SELLING JOB GOODS
                                        case 20:
                                            {
                                                this.job_question = false;
                                            }
                                            break;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x703c_CLIENT_OPEN_STORAGE
                                else if (opcode == 0x703c)
                                {
                                    /*CLOSE
                                    if (!FilterMain.ITEM_LOCK_STORAGE)
                                    {
                                        m_RemoteSecurity.Send(_pck);
                                        Send(true);
                                        continue;
                                    }

                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_STORAGE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion
                                    */
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x34A9_AVATAR_BLUES_EXPLOIT
                                else if (opcode == 0x34a9)
                                {
                                    #region BATTLE ROYALE
                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                    {
                                        this.SendNotice("Avatar blues are disabled on this server.");
                                        continue;
                                    }
                                    #endregion

                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_AVATAR))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    string avatar_blue = _pck.ReadAscii().ToString().ToLower();
                                    if (!avatar_blue.Contains("avatar"))
                                    {
                                        FilterMain.blocked_agent_injects++;
                                        Send(false);
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x74B2_CTF_REGISTER_LEVEL_REQ
                                else if (opcode == 0x74b2)
                                {
                                    /*CLOSE
                                    #region CTF_BOT_BLOCKAGE
                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_CTF_ARENA))
                                    {
                                        this.SendNotice("BOT_ENTER_CTF");
                                        continue;
                                    }
                                    #endregion

                                    #region CTF_LEVEL_RESTRICTION
                                    if (this.CurLevel < FilterMain.CTF_REGISTER_LEVEL)
                                    {
                                        this.required_level = FilterMain.CTF_REGISTER_LEVEL;
                                        this.SendNotice("CTF_LEVEL_MESSAGE");
                                        continue;
                                    }
                                    #endregion
                                    */
                                    #region PC_LIMIT
                                    if (FilterMain.CTF_PC_LIMIT > 0)
                                    {
                                        try
                                        {
                                            #region CTF Handler
                                            //Task.Run(async () => await sqlCon.exec($"[{FilterMain.sql_db}].[dbo].[_HandleCTF] '{this.user_id}'"));
                                            #endregion

                                            #region Check limit
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountCTF] '{this.user_id}', '{this.mac}'")).Result;
                                            if (current_count > FilterMain.CTF_PC_LIMIT)
                                            {
                                                this.SendNotice("PC_LIMIT_CTF_MESSAGE");
                                                continue;
                                            }
                                            #endregion
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x74D3_ARENA_REGISTER_LEVEL_REQ
                                else if (opcode == 0x74d3)
                                {
                                    /*CLOSE
                                    #region BOT restriction
                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_BATTLE_ARENA))
                                    {
                                        this.SendNotice("BOT_ENTER_ARENA");
                                        continue;
                                    }
                                    #endregion

                                    #region Level restriction
                                    if (this.CurLevel < FilterMain.ARENA_REGISTER_LEVEL)
                                    {
                                        this.required_level = FilterMain.ARENA_REGISTER_LEVEL;
                                        this.SendNotice("ARENA_LEVEL_MESSAGE");
                                        continue;
                                    }
                                    #endregion
                                    */

                                    #region PC_LIMIT
                                    if (FilterMain.BA_PC_LIMIT > 0)
                                    {
                                        try
                                        {
                                            #region BA Handler
                                            //Task.Run(async () => await sqlCon.exec($"[{FilterMain.sql_db}].[dbo].[_HandleBA] '{this.user_id}'"));
                                            #endregion

                                            #region Check limit
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.sql_db}].[dbo].[_CountBA] '{this.user_id}', '{this.mac}'")).Result;
                                            if (current_count > FilterMain.BA_PC_LIMIT)
                                            {
                                                //this.SendNotice("PC_LIMIT_ARENA_MESSAGE");
                                                this.SendNotice("Battle Arenada PC Limit aşıldı!");
                                                continue;
                                            }
                                            #endregion
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70C6_PET_TERMINATE
                                else if (opcode == 0x70c6)
                                {
                                    //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");
                                    this.lastspawntime = DateTime.Now;
                                    this.thief_pickup = true;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70CB_PET_UNMOUNT
                                else if (opcode == 0x70CB)
                                {
                                    this.lastspawntime = DateTime.Now;

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7116_GRAB_PET_TERMINATE
                                else if (opcode == 0x7116)
                                {
                                    /*
                                    try
                                    {
                                        int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.lastspawntime).TotalSeconds);
                                        if (gecensaniye < FilterMain.TRADE_BUG_DELAY)
                                        {
                                            this.required_delay = (FilterMain.TRADE_BUG_DELAY - gecensaniye);
                                            this.SendNotice("GRABPET_DELAY_MESSAGE");
                                            continue;
                                        }
                                    }
                                    catch
                                    {
                                    }

                                    this.lastspawntime = DateTime.Now;
                                    */
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7006_CLIENT_LEAVE_CANCEL
                                else if (opcode == 0x7006)
                                {
                                    if (this.length > 0)
                                    {
                                        Send(false);
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x705A_CLIENT_TELEPORT_START
                                else if (opcode == 0x705a)
                                {
                                    /*CLOSE
                                    _pck.ReadUInt32();
                                    byte teleport_type = _pck.ReadUInt8();
                                    if (teleport_type == 2 && FilterMain.BOT_LIST.Contains(this.user_id))
                                    {
                                        if (!FilterMain.BOT_WATER_TEMPLE)
                                        {
                                            UInt32 teleport_id = _pck.ReadUInt32();
                                            if (teleport_id == 166 || teleport_id == 167)
                                            {
                                                this.SendNotice("BOT_WATER_TEMPLE");
                                                return;
                                            }
                                        }
                                    }
                                    
                                    try
                                    {
                                        int gecensaniye = Convert.ToInt32(DateTime.Now.Subtract(this.lastspawntime).TotalSeconds);
                                        if (gecensaniye < FilterMain.TRADE_BUG_DELAY)
                                        {
                                            this.required_delay = (FilterMain.TRADE_BUG_DELAY - gecensaniye);
                                            this.SendNotice("TELEPORT_DELAY_MESSAGE");
                                            continue;
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    */
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7010_GM_SECURITY
                                else if (opcode == 0x7010)
                                {
                                    uint command = _pck.ReadUInt8();
                                    string ColumName;
                                    switch (command)
                                    {
                                        case 3:
                                            {
                                                ColumName = "totown";
                                            }
                                            break;

                                        case 6:
                                            {
                                                ColumName = "loadmonster";
                                            }
                                            break;

                                        case 7:
                                            {
                                                ColumName = "makeitem";
                                            }
                                            break;

                                        case 8:
                                            {
                                                ColumName = "movetouser";
                                            }
                                            break;

                                        case 12:
                                            {
                                                ColumName = "zoe";
                                            }
                                            break;

                                        case 13:
                                            {
                                                ColumName = "ban";
                                            }
                                            break;

                                        case 14:
                                            {
                                                ColumName = "invisible";
                                            }
                                            break;

                                        case 15:
                                            {
                                                ColumName = "invincible";
                                            }
                                            break;

                                        case 17:
                                            {
                                                ColumName = "recalluser";
                                            }
                                            break;

                                        case 18:
                                            {
                                                ColumName = "recallguild";
                                            }
                                            break;

                                        case 20:
                                            {
                                                ColumName = "mobkill";
                                            }
                                            break;

                                        case 42:
                                            {
                                                ColumName = "spawnunique_loc";
                                            }
                                            break;

                                        default:
                                            {
                                                ColumName = "unkown";
                                            }
                                            break;
                                    }

                                    /*
                                        if (ColumName != "unkown")
                                        {
                                            int Permision = Global.dbmgr.ReadInt("SELECT "+ ColumName + " FROM _GM_Authorization WHERE Username = '" + username + "'");
                                            if (Permision == 1)
                                            {
                                                return PacketProcessResult.Send;
                                            }
                                            else
                                            {
                                                session.SendClientPlayerNotice(Global.GMController_Message);
                                                return PacketProcessResult.Ignore;
                                            }
                                        }
                                    */

                                    #region SECURITY
                                    if (!FilterMain.GMs.Contains(this.user_id))
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exploiting GM packet");
                                        this.SendNotice("GM_ERROR_MESSAGE");
                                        continue;
                                    }
                                    #endregion

                                    #region Prevent /mobkill 0
                                    if (FilterMain.FILES.Contains("vsro"))
                                    {
                                        //uint command = _pck.ReadUInt16();
                                        if (FilterMain.GM_MOBKILL_PROTECTION && (command == 20))
                                        {
                                            uint k_mob_id = _pck.ReadUInt32();
                                            Packet p = new Packet(0x7010);
                                            p.WriteUInt16(20);
                                            p.WriteUInt32(k_mob_id);
                                            p.WriteUInt8(1);
                                            m_RemoteSecurity.Send(p);
                                            Send(false);
                                        }
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7150_ALCHEMY_PACKET
                                else if (opcode == 0x7150)
                                {
                                    /*
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_ALCHEMY))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_ALCHEMY_ELIXIR))
                                    {
                                        this.SendNotice("BOT_ALCHEMY");
                                        continue;
                                    }
                                    */
                                    if (FilterMain.PLUS_LIMIT > 0)
                                    {
                                        if (_pck.ReadUInt8() == 2 && _pck.ReadUInt8() == 3)
                                        {
                                            _pck.ReadUInt8(); // Unknown
                                            int slot = _pck.ReadUInt8();

                                            try
                                            {
                                                int plus = Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.sql_db}].[dbo].[_MainFunctions] '{this.sql_charname}', 3, {slot}")).Result;
                                                if (plus >= FilterMain.PLUS_LIMIT)
                                                {
                                                    //this.SendNotice("ITEM_MAX_PLUS");
                                                    this.SendNotice("Maksimum artıya ulaştınız!");
                                                    continue;
                                                }
                                            }
                                            catch { }
                                        }
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7151_ALCHEMY_PACKET_STONES
                                else if (opcode == 0x7151)
                                {
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_ALCHEMY_STONE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_ALCHEMY_STONE))
                                    {
                                        this.SendNotice("BOT_ALCHEMY");
                                        continue;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7157_ALCHEMY_DISSABLE_ITEM
                                else if (opcode == 0x7157)
                                {
                                    /*CLOSE
                                    #region ITEM LOCK
                                    if (this.is_locked && (FilterMain.ITEM_LOCK_ALCHEMY_DISSAMBLE))
                                    {
                                        this.SendNotice("ITEM_LOCK_IS_LOCKED");
                                        continue;
                                    }
                                    #endregion

                                    if (FilterMain.BOT_LIST.Contains(this.user_id) && (!FilterMain.BOT_ALCHEMY_STONE))
                                    {
                                        this.SendNotice("BOT_ALCHEMY");
                                        continue;
                                    }
                                    */
                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x7005_CLIENT_ESC
                                else if (opcode == 0x7005)
                                {
                                    #region Protection
                                    if (this.length != 1)
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, $"Client({this.ip}:{this.user_id}) attempted to modify 0x7005");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    uint action = _pck.ReadUInt8();
                                    switch (action)
                                    {
                                        
                                        #region Exit delay
                                        case 1:
                                            {
                                                try
                                                {
                                                    if (FilterMain.EXIT_DELAY > 0)
                                                    {
                                                        int timeleft = Convert.ToInt32((DateTime.Now.Subtract(this.exittime)).TotalSeconds);
                                                        if (timeleft < FilterMain.EXIT_DELAY)
                                                        {
                                                            this.required_delay = (FilterMain.EXIT_DELAY - timeleft);
                                                            //this.SendNotice("EXIT_DELAY_MESSAGE");
                                                            this.SendNotice("Çıkış için "+ FilterMain.EXIT_DELAY + " saniye beklemelisiniz!");
                                                            continue;
                                                        }
                                                    }
                                                }
                                                catch { }
                                                this.exittime = DateTime.Now;
                                            }
                                            break;
                                        #endregion
                                        

                                        #region Restart delay
                                        case 2:
                                            {
                                                try
                                                {
                                                    if (FilterMain.RESTART_DELAY > 0)
                                                    {
                                                        int timeleft = Convert.ToInt32((DateTime.Now.Subtract(this.restarttime)).TotalSeconds);
                                                        if (timeleft < FilterMain.RESTART_DELAY)
                                                        {
                                                            this.required_delay = (FilterMain.RESTART_DELAY - timeleft);
                                                            //this.SendNotice("RESTART_DELAY_MESSAGE");
                                                            this.SendNotice("Restart için "+ FilterMain.EXIT_DELAY + " saniye beklemelisiniz!");
                                                            continue;
                                                        }
                                                    }
                                                }
                                                catch { }
                                                this.restarttime = DateTime.Now;
                                            }
                                            break;
                                        #endregion

                                        #region IWA fix v2
                                        default:
                                            {
                                                FilterMain.blocked_agent_exploits++;
                                                Logger.WriteLine(Logger.LogLevel.Debug, $"Client({this.ip}:{this.user_id}) attempted to use IWA exploit :)");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                            break;
                                            #endregion
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x70f3_GUILD_INVITE_PLAYER
                                else if (opcode == 0x70f3)
                                {
                                    if (FilterMain.GUILD_LIMIT > 0)
                                    {

                                        FilterMain.GUILD_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'GUILD_LIMIT'")).Result);
                                        if (this.guild_limit >= FilterMain.GUILD_LIMIT)
                                        {
                                            //this.SendNotice("GUILD_MAX_PLAYERS");
                                            this.SendNotice("Guild için maksimum kullanıcıya ulaştınız!");
                                            continue;
                                        }
                                        this.guild_limit++;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion



                                #region 0x70fb_GUILD_UNION_INVITE
                                else if (opcode == 0x70fb)
                                {
                                    if (FilterMain.UNION_LIMIT > 0)
                                    {
                                        FilterMain.UNION_LIMIT = int.Parse(Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetFILTER] 'UNION_LIMIT'")).Result);
                                        if (this.union_limit >= FilterMain.UNION_LIMIT)
                                        {
                                            //this.SendNotice("UNION_MAX_PLAYERS");
                                            this.SendNotice("Union için maksimum guild sayısına ulaştınız!");
                                            continue;
                                        }
                                        this.union_limit++;
                                    }

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region 0x705E_CLIENT_EXPLOIT 
                                /*else if (opcode == 0x705E)
                                {
                                    string message = _pck.ReadAscii();
                                    if (message.Contains("UPDATE") || message.Contains("INSERT") || message.Contains("exec") || message.Contains("DELETE") || message.Contains("TRUNCATE") ||
                                      message.Contains("'") || message.Contains("\"") || message.Contains("-") || message.Contains(";"))
                                    {
                                        Logger.WriteLine(Logger.LogLevel.Debug, $"Client({this.ip}:{this.user_id}) SQL Inject detection - "+ message);
                                        this.SendNotice("Yasak karakter kullandınız!");
                                        continue;
                                    }

                                }*/
                                #endregion

                                #region 0x7025_PRIVATE_MESSAGE_vSRO274
                                else if (opcode == 0x7025)
                                {
                                    //this.SendNotify("UIIT_MSG_BATTLEROYALE_TEST");
                                    #region vSRO274
                                    if (FilterMain.FILES.Contains("274"))
                                    {
                                        /*
                                         * VSRO 188
                                           1 = LOCAL
                                           2 = PM
                                           3 = GM
                                           4 = PARTY
                                           6 = Global
                                        */

                                        int type = _pck.ReadUInt8();
                                                   
                                        switch (type)
                                        {
                                            case 1:
                                                {
                                                    _pck.ReadUInt8(); //Skip
                                                    _pck.ReadUInt8(); //Skip
                                                    string message = _pck.ReadAscii().ToString().ToLower();
                                                    /*CLOSE
                                                    #region Item LOCK feature
                                                    if (message.Contains("!lock") && FilterMain.ITEM_LOCK)
                                                    {
                                                        if (message.Contains(" "))
                                                        {
                                                            string[] xpass = message.Split(' ');
                                                            int code;
                                                            if (int.TryParse(xpass[1].ToString(), out code))
                                                            {
                                                                if (code > 999)
                                                                {
                                                                    if (this.has_code == 0)
                                                                    {
                                                                        try
                                                                        {
                                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_CreateLock] '{this.user_id}', {code}"));
                                                                        }
                                                                        catch { }
                                                                        this.has_code = code;
                                                                        this.is_locked = true;
                                                                        this.required_delay = code;
                                                                        this.SendNotice($"ITEM_LOCK_FIRST_TIME");
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (this.has_code == code)
                                                                        {
                                                                            if (this.is_locked)
                                                                            {
                                                                                this.SendNotice("ITEM_LOCK_IS_UNLOCKED");
                                                                                this.is_locked = false;
                                                                                continue;
                                                                            }
                                                                            else
                                                                            {
                                                                                this.SendNotice("ITEM_LOCK_IS_UNLOCKED2");
                                                                                this.is_locked = true;
                                                                                continue;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            #region Log fail attempts
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_LockFail] '{this.user_id}', '{this.ip}', '{this.hwid}', '{this.mac}', {code}"));
                                                                            }
                                                                            catch { }
                                                                            #endregion

                                                                            this.lock_fail_attempt++;

                                                                            #region Handle lock fail attempts
                                                                            if (this.lock_fail_attempt >= FilterMain.ITEM_LOCK_MAX_FAIL)
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_DISCONNECT");
                                                                                this.DisconnectModuleSocket();
                                                                                return;
                                                                            }
                                                                            #endregion

                                                                            this.required_delay = this.lock_fail_attempt;
                                                                            this.required_level = FilterMain.ITEM_LOCK_MAX_FAIL;
                                                                            this.SendNotice($"ITEM_LOCK_WRONG_CODE");
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    this.SendNotice($"ITEM_LOCK_PASSWORD_LENGTH");
                                                                    continue;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                this.SendNotice("ITEM_LOCK_INTEGER");
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("ITEM_LOCK_USAGE");
                                                            continue;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Event commands, kappa :)
                                                    if (message.StartsWith("!"))
                                                    {
                                                        string new_msg = message.Replace("!", String.Empty);

                                                        if (!Directory.Exists("commands"))
                                                        {
                                                            Directory.CreateDirectory("commands");
                                                        }

                                                        if (File.Exists($"commands/{new_msg}.txt"))
                                                        {
                                                            string text = File.ReadAllText($"commands/{new_msg}.txt");
                                                            this.SendNotice(text);
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("EVENT_COMMANDS");
                                                        }

                                                        continue;
                                                    }
                                                    #endregion

                                                    #region Remove item lock
                                                    if (message.Contains("!removelock") && FilterMain.ITEM_LOCK)
                                                    {
                                                        if (message.Contains(" "))
                                                        {
                                                            string[] xpass = message.Split(' ');
                                                            int code;
                                                            if (int.TryParse(xpass[1].ToString(), out code))
                                                            {
                                                                if (xpass[1].ToString().Length == 4)
                                                                {
                                                                    if (this.has_code == 0)
                                                                    {
                                                                        this.SendNotice($"ITEM_LOCK_NOT_EXIST");
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (this.has_code == code)
                                                                        {
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_RemoveLock] '{this.user_id}'"));
                                                                            }
                                                                            catch { }
                                                                            this.SendNotice($"ITEM_LOCK_NO_SUPPORT");
                                                                            this.has_code = 0;
                                                                            this.is_locked = false;
                                                                            continue;
                                                                        }
                                                                        else
                                                                        {
                                                                            #region Log fail attempts
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_LockFail] '{this.user_id}', '{this.ip}', '{this.hwid}', '{this.mac}', {code}"));
                                                                            }
                                                                            catch { }
                                                                            #endregion

                                                                            this.lock_fail_attempt++;

                                                                            #region Handle lock fail attempts
                                                                            if (this.lock_fail_attempt >= FilterMain.ITEM_LOCK_MAX_FAIL)
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_DISCONNECT");
                                                                                this.DisconnectModuleSocket();
                                                                                return;
                                                                            }
                                                                            #endregion

                                                                            this.required_delay = this.lock_fail_attempt;
                                                                            this.required_level = FilterMain.ITEM_LOCK_MAX_FAIL;
                                                                            this.SendNotice($"ITEM_LOCK_WRONG_CODE");
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    this.SendNotice($"ITEM_LOCK_PASSWORD_LENGTH");
                                                                    continue;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                this.SendNotice("ITEM_LOCK_INTEGER");
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("ITEM_LOCK_USAGE2");
                                                            continue;
                                                        }
                                                    }

                                                    #endregion
                                                    */
                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                        continue;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                            case 2:
                                                {
                                                    _pck.ReadUInt8(); //Skip
                                                    _pck.ReadUInt8(); //Skip
                                                    string ToCharName16 = sqlCon.clean(_pck.ReadAscii());
                                                    string message = sqlCon.clean(_pck.ReadAscii().ToString().ToLower());

                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                        continue;
                                                    }
                                                    #endregion

                                                    #region LOG SYSTEM
                                                    if (FilterMain.LOG_PMCHAT)
                                                    {
                                                        try
                                                        {
                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 2, '{ToCharName16}'"));
                                                        }
                                                        catch { }
                                                    }
                                                    #endregion
                                                   
                                                }
                                                break;
                                            case 4:
                                                {
                                                    _pck.ReadUInt8(); //Skip
                                                    _pck.ReadUInt8(); //Skip
                                                    string message = _pck.ReadAscii().ToString().ToLower();

                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        //this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        this.SendNotice("Karaliste olan kelime kullanıldı!");
                                                        continue;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                        }
                                    }
                                    #endregion

                                    #region Other versions
                                    else
                                    {
                                        /*
                                         * VSRO 188
                                            1 = LOCAL CHAT
                                            2 = PRIVATE CHAT
                                            3 = GM CHAT
                                            4 = PARTY CHAT
                                            5 = GUILD CHAT
                                            11 = UNION CHAT
                                        */
                                        int type = _pck.ReadUInt8();
                                        switch (type)
                                        {
                                            case 1:
                                            case 3:
                                                {
                                                    _pck.ReadUInt8();
                                                    string message = _pck.ReadAscii().ToString().ToLower();
                                                    /*CLOSE
                                                    if (FilterMain.BATTLE_ROYALE_MODE)
                                                    {
                                                        if (message.StartsWith("!"))
                                                        {
                                                            if (message.Contains("join"))
                                                            {
                                                                SendPM("BATTLEROYALE", "!join");
                                                                Send(false);
                                                                continue;
                                                            }

                                                            else if (message.Contains("leave"))
                                                            {
                                                                SendPM("BATTLEROYALE", "!leave");
                                                                Send(false);
                                                                continue;
                                                            }
                                                        }
                                                    }

                                                    #region Item LOCK feature
                                                    if (message.Contains("!lock") && FilterMain.ITEM_LOCK)
                                                    {
                                                        if (message.Contains(" "))
                                                        {
                                                            string[] xpass = message.Split(' ');
                                                            int code;
                                                            if (int.TryParse(xpass[1].ToString(), out code))
                                                            {
                                                                if (code > 999)
                                                                {
                                                                    if (this.has_code == 0)
                                                                    {
                                                                        try
                                                                        {
                                                                            Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_CreateLock] '{this.user_id}', {code}"));
                                                                        }
                                                                        catch { }
                                                                        this.has_code = code;
                                                                        this.is_locked = true;
                                                                        this.required_delay = code;
                                                                        this.SendNotice($"ITEM_LOCK_FIRST_TIME");
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (this.has_code == code)
                                                                        {
                                                                            if (this.is_locked)
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_IS_UNLOCKED");
                                                                                this.is_locked = false;
                                                                                continue;
                                                                            }
                                                                            else
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_IS_UNLOCKED2");
                                                                                this.is_locked = true;
                                                                                continue;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            #region Log fail attempts
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_LockFail] '{this.user_id}', '{this.ip}', '{this.hwid}', '{this.mac}', {code}"));
                                                                            }
                                                                            catch { }
                                                                            #endregion

                                                                            this.lock_fail_attempt++;

                                                                            #region Handle lock fail attempts
                                                                            if (this.lock_fail_attempt >= FilterMain.ITEM_LOCK_MAX_FAIL)
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_DISCONNECT");
                                                                                this.DisconnectModuleSocket();
                                                                                return;
                                                                            }
                                                                            #endregion

                                                                            this.required_delay = this.lock_fail_attempt;
                                                                            this.required_level = FilterMain.ITEM_LOCK_MAX_FAIL;
                                                                            this.SendNotice($"ITEM_LOCK_WRONG_CODE");
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    this.SendNotice($"ITEM_LOCK_PASSWORD_LENGTH");
                                                                    continue;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                this.SendNotice("ITEM_LOCK_INTEGER");
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("ITEM_LOCK_USAGE");
                                                            continue;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Remove item lock
                                                    if (message.Contains("!removelock") && FilterMain.ITEM_LOCK)
                                                    {
                                                        if (message.Contains(" "))
                                                        {
                                                            string[] xpass = message.Split(' ');
                                                            int code;
                                                            if (int.TryParse(xpass[1].ToString(), out code))
                                                            {
                                                                if (xpass[1].ToString().Length == 4)
                                                                {
                                                                    if (this.has_code == 0)
                                                                    {
                                                                        this.SendNotice($"ITEM_LOCK_NOT_EXIST");
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (this.has_code == code)
                                                                        {
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_RemoveLock] '{this.user_id}'"));
                                                                            }
                                                                            catch { }
                                                                            this.SendNotice($"ITEM_LOCK_NO_SUPPORT");
                                                                            this.has_code = 0;
                                                                            this.is_locked = false;
                                                                            continue;
                                                                        }
                                                                        else
                                                                        {
                                                                            #region Log fail attempts
                                                                            try
                                                                            {
                                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_LockFail] '{this.user_id}', '{this.ip}', '{this.hwid}', '{this.mac}', {code}"));
                                                                            }
                                                                            catch { }
                                                                            #endregion

                                                                            this.lock_fail_attempt++;

                                                                            #region Handle lock fail attempts
                                                                            if (this.lock_fail_attempt >= FilterMain.ITEM_LOCK_MAX_FAIL)
                                                                            {
                                                                                this.SendNotice($"ITEM_LOCK_DISCONNECT");
                                                                                this.DisconnectModuleSocket();
                                                                                return;
                                                                            }
                                                                            #endregion

                                                                            this.required_delay = this.lock_fail_attempt;
                                                                            this.required_level = FilterMain.ITEM_LOCK_MAX_FAIL;
                                                                            this.SendNotice($"ITEM_LOCK_WRONG_CODE");
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    this.SendNotice($"ITEM_LOCK_PASSWORD_LENGTH");
                                                                    continue;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                this.SendNotice("ITEM_LOCK_INTEGER");
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("ITEM_LOCK_USAGE2");
                                                            continue;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Event commands, kappa :)
                                                    if (message.StartsWith("!"))
                                                    {
                                                        string new_msg = message.Replace("!", String.Empty);

                                                        if (!Directory.Exists("commands"))
                                                        {
                                                            Directory.CreateDirectory("commands");
                                                        }

                                                        if (File.Exists($"commands/{new_msg}.txt"))
                                                        {
                                                            string text = File.ReadAllText($"commands/{new_msg}.txt");
                                                            this.SendNotice(text);
                                                        }
                                                        else
                                                        {
                                                            this.SendNotice("EVENT_COMMANDS");
                                                        }

                                                        continue;
                                                    }
                                                    #endregion
                                                    */
                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        continue;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                            case 2:
                                                {
                                                    _pck.ReadUInt8(); // skip
                                                    string ToCharName16 = sqlCon.clean(_pck.ReadAscii());
                                                    string message = sqlCon.clean(_pck.ReadAscii().ToString().ToLower());

                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        continue;
                                                    }
                                                    #endregion
                                                    #region LOG SYSTEM
                                                    if (FilterMain.LOG_PMCHAT)
                                                    {
                                                        if (!message.StartsWith("!"))
                                                        {
                                                            try
                                                            {
                                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_ChatLogs] '{this.sql_charname}', '{message}', 2, '{ToCharName16}'"));
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                    #endregion

                                                    #region PM_TICKET
                                                    if (FilterMain.PM_TICKET && (ToCharName16 == "TICKET2019" || ToCharName16 == "ticket2019"))
                                                    {
                                                        try
                                                        {
                                                            if (message.StartsWith("?"))
                                                            {
                                                                bool smessage = true;
                                                                while (smessage)
                                                                {
                                                                    string last_msg = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.sql_db}].[dbo].[_GetTicket] '{this.sql_charname}'")).Result;
                                                                    if (last_msg.Length > 3)
                                                                    {
                                                                        this.SendPMToOwn("TICKET2019", last_msg);
                                                                    }
                                                                    else
                                                                    {
                                                                        smessage = false;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (message.Length > 25)
                                                                {
                                                                    message = message.Replace("'", string.Empty);
                                                                    message = message.Replace(";", string.Empty);
                                                                    message = message.Replace("\"", string.Empty);
                                                                    try
                                                                    {
                                                                        Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.sql_db}].[dbo].[_AddTicket] '{this.sql_charname}', '{message}'"));
                                                                        this.SendPMToOwn(ToCharName16, "MESAJINIZ: " + message);
                                                                        this.SendPMToOwn(ToCharName16, "Merhaba, mesajınız alınmıştır. Cevabınızı sitemizden yada TICKET2019'a ? yazarak görebilirsiniz.");
                                                                    }
                                                                    catch { }
                                                                }
                                                                else
                                                                {
                                                                    this.SendPMToOwn(ToCharName16, "Mesajınız çok kısa! Lütfen en az 25 karakter olmalıdır.");
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Logger.WriteLine(Logger.LogLevel.Error, ex.ToString());
                                                        }
                                                        Send(false);
                                                        continue;
                                                    }
                                                    #endregion


                                                    #region PHBOT_FIX
                                                    if (this.char_job && ToCharName16 == "BOT2019" && FilterMain.PHBOT_LOCK)
                                                    {
                                                        try
                                                        {
                                                            string questionAnswer = Task.Run(async () => await sqlCon.prod_string($"EXEC [{FilterMain.sql_db}].[dbo].[_GetQuestionAnswer] '{this.sql_charname}'")).Result;
                                                            if (questionAnswer == message)
                                                            {
                                                                this.SendPMToOwn("BOT2019", "Mesajı Doğru Cevapladınız! Kervanı yükleyebilirsiniz.");
                                                                this.job_question = true;
                                                            }
                                                            else
                                                            {
                                                                this.SendPMToOwn("BOT2019", "Mesajı Yanlış Cevapladınız!");
                                                                this.job_question = false;
                                                            }
                                                           
                                                        }catch(Exception ex)
                                                        {
                                                            Logger.WriteLine(Logger.LogLevel.Error, ex.ToString());
                                                        }
                                                        Send(false);
                                                        continue;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                            case 4:
                                                {
                                                    string message = _pck.ReadAscii().ToString().ToLower();
                                                    #region Blocked words
                                                    if (Badword(message))
                                                    {
                                                        this.SendNotice("BLACKLIST_WORD_MESSAGE");
                                                        continue;
                                                    }
                                                    #endregion
                                                }
                                                break;
                                        }
                                    }
                                    #endregion

                                    m_RemoteSecurity.Send(_pck);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region GAMESERVER_EXPLOIT1
                                //if (opcode == 0x3510 || opcode == 0x34B1 || opcode == 0x34D2 || opcode == 0x385F || FilterMain.BAD_Opcodes.Contains(opcode))
                                if (FilterMain.BAD_Opcodes.ContainsKey(opcode))
                                {
                                    Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exploiting: GAMESERVER_EXPLOIT1");
                                    continue;
                                }
                                #endregion
                                #region OpcodeHandler
                                else
                                {

                                    int OpcodesHandler = Opcodes.AgentServer(_pck);
                                    switch (OpcodesHandler)
                                    {
                                        case 1:
                                            {
                                                this.SendNotice("PACKET_ERROR_MESSAGE");
                                                continue;
                                            }
                                            break;
                                        case 2:
                                            {
                                                FilterMain.blocked_agent_exploits++;
                                                Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exploiting");
                                                this.SendNotice("PACKET_DISCONNECT_MESSAGE");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                            break;
                                        case 3:
                                            {
                                                FilterMain.blocked_agent_exploits++;
                                                Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was banned for exploiting");
                                                FirewallHandler.BlockIP(this.ip, "Exploiting");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                            break;
                                    }
                                  
                                }
                                #endregion

                                // Send packets
                                m_RemoteSecurity.Send(_pck);
                                Send(true);
                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            // Abort connection
                            this.DisconnectModuleSocket();
                            this.m_TransferPoolThread.Abort();
                        }
                        catch { }

                        this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                        return;
                    }


                    this.DoRecvFromClient();
                }
                catch(Exception e)
                {
                    try
                    {
                        // Abort connection
                        this.DisconnectModuleSocket();
                        this.m_TransferPoolThread.Abort();
                    }
                    catch { }

                    this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                    return;
                }
            }
        }
        #endregion

        #region Send PM
        void SendPM(string user, string message)
        {
            Packet response = new Packet(0x7025);
            response.WriteUInt8((byte)2);
            response.WriteUInt8((byte)0);
            response.WriteAscii(user);
            response.WriteAscii(message);
            m_RemoteSecurity.Send(response);
            Send(true);
        }
        #endregion   

        #region Send PM
        void SendPMToOwn(string user, string message)
        {
            Packet packet = new Packet(0x3026);
            //packet.WriteUInt8(2);
            packet.WriteUInt8((byte)2);
            packet.WriteAscii(user);
            packet.WriteAscii(message);
            m_LocalSecurity.Send(packet);
            Send(false);
        }
        #endregion

        #region Send Notice
        void SendNotice(string Message)
        {
            Packet packet = new Packet(0x3026);
            //packet.WriteUInt8(0x07);
            packet.WriteUInt8((byte)7);
            if (FilterMain.FILES.Contains("br"))
            {
                packet.WriteUnicode(this.GG(Message));
            }
            else
            {
                packet.WriteAscii(this.GG(Message));
            }
            m_LocalSecurity.Send(packet);
            Send(false);
        }
        #endregion

        #region Send notify
        void SendNotify(string str)
        {
            /* BLUE SHIT */
            Packet err = new Packet(0x300C);
            err.WriteUInt16(3100);
            err.WriteUInt8(1);
            err.WriteAscii(str);
            m_LocalSecurity.Send(err);
            Send(false);

            //Инфа с боку
            /*
            http://prntscr.com/hhndl8
            Packet err_2 = new Packet(0x300C);
            err_2.WriteUInt16(3100);
            err_2.WriteUInt8(2);
            err_2.WriteAscii(str);
            m_LocalSecurity.Send(err_2);
            Send(false);

            */
        }
        #endregion

        #region DoRecvFromServer()
        void DoRecvFromServer()
        {
            try
            {
                this.m_ModuleSocket.BeginReceive(m_RemoteBuffer, 0, m_RemoteBuffer.Length,
                SocketFlags.None,
                new AsyncCallback(OnReceive_FromServer), null);
            }
            catch
            {
                try
                {
                    // Abort connection
                    this.DisconnectModuleSocket();
                    this.m_TransferPoolThread.Abort();
                }
                catch { }

                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                return;
            }
        }
        #endregion

        #region DoRecvFromClient()
        void DoRecvFromClient()
        {
            try
            {
                this.m_ClientSocket.BeginReceive(m_LocalBuffer, 0, m_LocalBuffer.Length,
                SocketFlags.None,
                new AsyncCallback(OnReceive_FromClient), null);
            }
            catch
            {
                try
                {
                    // Abort connection
                    this.DisconnectModuleSocket();
                    this.m_TransferPoolThread.Abort();
                }
                catch { }

                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                return;
            }
        }
        #endregion
    }
}