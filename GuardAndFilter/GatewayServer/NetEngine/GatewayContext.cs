#pragma warning disable
using Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Filter.NetEngine
{
    sealed class GatewayContext
    {
        Socket m_ClientSocket = null;
        AsyncServer.E_ServerType m_HandlerType;
        AsyncServer.delClientDisconnect m_delDisconnect;
        object m_Lock = new object();
        Socket m_ModuleSocket = null;

        /*
        Old buffers
        byte[] m_LocalBuffer = new byte[8192];
        byte[] m_RemoteBuffer = new byte[8192];
        */

        // New buffers
        byte[] m_LocalBuffer = new byte[8192];
        byte[] m_RemoteBuffer = new byte[8192];

        Security m_LocalSecurity = new Security();
        Security m_RemoteSecurity = new Security();

        Thread m_TransferPoolThread = null;
        ulong m_BytesRecvFromClient = 0;
        DateTime m_StartTime = DateTime.Now;

        #region All dem shiets
        int sent_id = 0;
        int sent_list = 0;
        int length = 0;
        public static object locker = new object();
        string ip;
        bool packet_handler = false;
        string user_id;
        string user_pw;
        int fail_login = 0;
        int players = 0;
        #endregion

        #region HWID SHITS
        string mac = "non";
        string serial = "non";
        string hwid = "non";
        bool hwid_sent = false;
        int bypass_xx = 0;
        int first_opcode_recieved = 0;
        #endregion

        #region shit_shit_shit
        int players_online = 0;
        int max_players = 0;
        string server_name = "non";
        int login_status = 0;

        #endregion

        #region Anti exploit mesures
        int packet_count;
        Timer packet_timer = null;
        #endregion

        // Count IP list
        public static int Ip_count(string ip)
        {
            lock (locker)
            {
                try
                {
                    int count_ip = 0;

                    foreach (string ips in FilterMain.ip_list_g)
                    {
                        if (ips == ip)
                        {
                            count_ip++;
                        }
                    }
                    return count_ip + 1;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public GatewayContext(Socket ClientSocket, AsyncServer.delClientDisconnect delDisconnect)
        {
            this.m_delDisconnect = delDisconnect;
            this.m_ClientSocket = ClientSocket;
            this.m_HandlerType = AsyncServer.E_ServerType.GatewayServer;
            this.m_ModuleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ip = ((IPEndPoint)(m_ClientSocket.RemoteEndPoint)).Address.ToString();

            try
            {
                this.m_ModuleSocket.Connect(new IPEndPoint(IPAddress.Parse(FilterMain.gateway_remote), FilterMain.gateway_mport));
                this.m_LocalSecurity.GenerateSecurity(true, true, true);
                this.DoRecvFromClient();
                Send(false);
            }
            catch
            {
                //Logger.WriteLine(//Logger.LogLevel.MikeMode, $"Remote host ({FilterMain.gateway_remote}:{FilterMain.gateway_mport}) is suicidal :(");
            }

            // IP list
            FilterMain.ip_list_g.Add(this.ip);

            #region Packet timer start
            if (this.packet_timer == null)
            {
                this.packet_timer = new Timer(new TimerCallback(this.ResetPackets), null, 0, FilterMain.packet_reset);
            }
            #endregion

            #region Flood control fix
            if (FilterMain.FLOOD_COUNT > 0)
            {
                if (Ip_count(this.ip) > FilterMain.FLOOD_COUNT)
                {
                    //Logger.WriteLine(//Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for flooding.");
                    FirewallHandler.BlockIP(this.ip, "DoS");
                    this.DisconnectModuleSocket();
                    return;
                }
            }
            #endregion
        }

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

        void DisconnectModuleSocket()
        {
            try
            {
                if (this.m_ModuleSocket != null)
                {
                    #region IP LIST
                    if (FilterMain.ip_list_g.Contains(this.ip))
                    {
                        FilterMain.ip_list_g.Remove(this.ip);
                    }
                    #endregion

                    if (this.packet_timer != null)
                    {
                        this.packet_timer.Dispose();
                        this.packet_timer = null;
                    }

                    // DISCONNECT
                    this.m_ModuleSocket.Close();
                    this.m_ModuleSocket = null;
                }
                // NULL
                this.m_ModuleSocket = null;
            }
            catch
            {
                //Logger.WriteLine(//Logger.LogLevel.Error, $"Client({this.ip}:{this.user_id}) Error(Error(func _DisconnectModuelSocket())");
            }
        }

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

                                #region Login packet
                                // Login packet
                                else if (opcode == 0xa102)
                                {
                                    // Host
                                    string src_host;
                                    int src_port;
                                    byte res = _pck.ReadUInt8();

                                    // Derp
                                    if (res == 1)
                                    {
                                        int index = 0;
                                        uint id = _pck.ReadUInt32();
                                        src_host = _pck.ReadAscii();
                                        src_port = _pck.ReadUInt16();

                                        Packet new_pck = new Packet(0xA102, true);
                                        new_pck.WriteUInt8(res);
                                        new_pck.WriteUInt32(id);

                                        try
                                        {
                                            /*
                                                Future mike:
                                                PLAYER -> CONNECT -> GET INFO FROM MODUELPORT(SRC_PORT) TO FILTER PORT.
                                            */
                                            index = FilterMain.agent_mports.IndexOf(src_port);
                                            if (index > -1)
                                            {
                                                Logger.WriteLine(Logger.LogLevel.MikeMode, $"Chooosing {FilterMain.agent_listen[index]}:{FilterMain.agent_fports[index]} - {src_port}  to connect with!");
                                                new_pck.WriteAscii(FilterMain.agent_listen[index]);
                                                new_pck.WriteUInt16(FilterMain.agent_fports[index]);
                                                new_pck.WriteUInt32((uint)0);
                                                new_pck.Lock();
                                                m_LocalSecurity.Send(new_pck);
                                                Send(false);
                                                continue;
                                            }
                                            else
                                            {
                                                Logger.WriteLine(Logger.LogLevel.Error, "Agent bindings wrong, re-check config/settings.ini");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                        }

                                        catch
                                        {
                                            Logger.WriteLine(Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) couldn't connect to server, wrong port binding or trying to exploit?");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }

                                    }
                                }
                                #endregion

                                #region 0xA100_DOWNLOAD_SPOOF
                                else if (opcode == 0xa100 && (FilterMain.download_ip != "127.0.0.1"))
                                {
                                    byte result = _pck.ReadUInt8();
                                    if (result == 0x02)
                                    {
                                        byte ErrorCode = _pck.ReadUInt8();
                                        if (ErrorCode == 0x02)
                                        {
                                            string ip = _pck.ReadAscii(); // ServerIP
                                            ushort port = _pck.ReadUInt16(); // ServerPort
                                            UInt32 version = _pck.ReadUInt32(); // Version
                                            byte flag = _pck.ReadUInt8(); // Flag

                                            Packet spoof = new Packet(0xA100, false, true);
                                            spoof.WriteUInt8(result);
                                            spoof.WriteUInt8(ErrorCode);
                                            spoof.WriteAscii(FilterMain.download_ip); // Spoofing part
                                            spoof.WriteUInt16(FilterMain.download_port); // Spoofing part
                                            spoof.WriteUInt32(version); // Version
                                            spoof.WriteUInt8(flag);
                                            while (flag == 0x01)
                                            {
                                                UInt32 FileID = _pck.ReadUInt32();
                                                string FileName = _pck.ReadAscii();
                                                string FilePath = _pck.ReadAscii();
                                                UInt32 FileLen = _pck.ReadUInt32();
                                                byte unk = _pck.ReadUInt8(); // Packed, no idea.
                                                flag = _pck.ReadUInt8();

                                                spoof.WriteUInt32(FileID);
                                                spoof.WriteAscii(FileName);
                                                spoof.WriteAscii(FilePath);
                                                spoof.WriteUInt32(FileLen);
                                                spoof.WriteUInt8(unk);
                                                spoof.WriteUInt8(flag);
                                            }
                                            m_LocalSecurity.Send(spoof);
                                            Send(false);
                                            continue;
                                        }
                                    }
                                }
                                #endregion

                                #region Captcha remover
                                // Captcha remover, trololo!
                                else if (opcode == 0x2322 && (FilterMain.gateway_captcha.Length > 0))
                                {
                                    Packet captchapacket = new Packet(0x6323, false);
                                    captchapacket.WriteAscii(FilterMain.gateway_captcha);
                                    m_RemoteSecurity.Send(captchapacket);
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                #region Online players
                                else if (opcode == 0xa101)
                                {
                                    int shard_id = 64;
                                    string server_name;
                                    int max_players = 1000;
                                    int status = 0;
                                    byte flag = 0x00;
                                    int shard_count = 0;

                                    #region Global list
                                    flag = _pck.ReadUInt8();
                                    while (flag == 0x01)
                                    {
                                        _pck.ReadUInt8();
                                        _pck.ReadAscii();

                                        flag = _pck.ReadUInt8();
                                    }
                                    #endregion

                                    #region Shard list
                                    flag = _pck.ReadUInt8();
                                    while(flag == 0x01)
                                    {
                                        shard_id = _pck.ReadUInt16();
                                        server_name = _pck.ReadAscii(); // Shard Name
                                        players = _pck.ReadUInt16();
                                        max_players = _pck.ReadUInt16(); // Max Players
                                        status = _pck.ReadUInt8(); // Status
                                        _pck.ReadUInt8();
                                        flag = _pck.ReadUInt8();

                                        try
                                        {
                                            if (FilterMain.shard_list.ContainsKey(shard_id))
                                            {
                                                FilterMain.shard_list[shard_id] = $"{server_name},{players},{max_players},{status}";
                                            }
                                            else
                                            {
                                                FilterMain.shard_list.Add(shard_id, $"{server_name},{players},{max_players},{status}");
                                            }
                                        }
                                        catch { }
                                    }
                                    #endregion
                                }
                                #endregion

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
                    DoRecvFromServer();
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


        void Send(bool ToHost)//that codes done by Excellency he fixed mbot for me
        {
            lock (m_Lock)
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
                            if (nBps > FilterMain.dMaxBytesPerSec_Gateway)
                            {
                                //Logger.WriteLine(//Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) disconnected for flooding.");
                                FirewallHandler.BlockIP(this.ip, "nBps count");

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

        #region Reset packet counter
        void ResetPackets(object e)
        {
            try
            {
                if (this.packet_count > FilterMain.packet_count)
                {
                    //Logger.WriteLine(//Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exceeding packet limit/second");
                    this.DisconnectModuleSocket();
                    return;
                }
            }
            catch { }
            this.packet_count = 0;
        }
        #endregion

        void OnReceive_FromClient(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = m_ClientSocket.EndReceive(iar);

                    if (nReceived != 0)
                    {
                        m_LocalSecurity.Recv(m_LocalBuffer, 0, nReceived);

                        List<Packet> ReceivedPackets = m_LocalSecurity.TransferIncoming();
                        if (ReceivedPackets != null)
                        {
                            foreach (Packet _pck in ReceivedPackets)
                            {
                                ushort opcode = Convert.ToUInt16(_pck.Opcode.ToString().ToLower());

                                //Logger.WriteLine(Logger.LogLevel.Warning, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(_pck.GetBytes())}{Environment.NewLine}");

                                #region New exploit fix
                                if (FilterMain.Server_opcodes.Contains(opcode))
                                {
                                    //Logger.WriteLine(//Logger.LogLevel.Notify, $"Client({this.ip}:{this.user_id}) attempted to send Server opcode({opcode:X4})");
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region Protection stuff
                                this.length = _pck.GetBytes().Length;
                                this.packet_count++;
                                this.packet_handler = true;
                                #endregion

                                #region Ignore handshake 
                                // Ignore handshake
                                if (opcode == 0x5000 || opcode == 0x9000)
                                {
                                    this.packet_handler = false;
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x2001_CLIENT_FLOOD_PROTECTION
                                // Filter against flood
                                else if (opcode == 0x2001)
                                {
                                    this.packet_handler = false;
                                    this.DoRecvFromServer();

                                    #region Exploit fix
                                    if (this.length != 12)
                                    {
                                        //Logger.WriteLine(//Logger.LogLevel.Error, $"Client({this.ip}:{this.user_id}) tried to exploit 0x2001");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region 0x2002_CLIENT_PING_PROTECTION
                                else if (opcode == 0x2002)
                                {
                                    this.packet_handler = false;
                                    if (this.length != 0)
                                    {
                                        //Logger.WriteLine(//Logger.LogLevel.Error, $"Client({this.ip}:{this.user_id}) tried to exploit 0x2002");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                }
                                #endregion

                                #region 0x6102_CLIENT_LOGIN_RESPONSE
                                else if (opcode == 0x6102)
                                {
                                    this.packet_handler = false;

                                    // Check shit
                                    byte locale = _pck.ReadUInt8();
                                    this.user_id = sqlCon.clean(_pck.ReadAscii().ToLower());
                                    this.user_pw = sqlCon.clean(_pck.ReadAscii());
                                    ushort ServerID = _pck.ReadUInt16();
                                    int is_gm = 0;

                                    if (!FilterMain.PaidUser)
                                    {
                                        if (players >= 501)
                                        {
                                            // Send client error
                                            Packet new_packet = new Packet(0xA102, false);
                                            new_packet.WriteUInt8(0x02);
                                            new_packet.WriteUInt8(5); // PC LIMIT ERROR
                                            m_LocalSecurity.Send(new_packet);
                                            Send(false);

                                            continue;
                                        }
                                    }

                                    #region Block login
                                    if (FilterMain.GM_LOGIN_ONLY)
                                    {
                                        if (!FilterMain.GMs.Contains(this.user_id))
                                        {
                                            Packet new_packet = new Packet(0xA102, false);
                                            new_packet.WriteUInt8(0x02);
                                            new_packet.WriteUInt8(12); // PC LIMIT ERROR
                                            m_LocalSecurity.Send(new_packet);
                                            Send(false);
                                            continue;
                                        }
                                    }
                                    #endregion

                                    #region Rewrite (locale) in login packet
                                    if (FilterMain.BOT_CONTROLL)
                                    {
                                        if(locale == 22)
                                        {
                                            if (!FilterMain.BOT_CONNECTION)
                                            {
                                                if (!FilterMain.GMs.Contains(this.user_id))
                                                {
                                                    // Send client error
                                                    Packet new_packet = new Packet(0xA102, false);
                                                    new_packet.WriteUInt8(0x02);
                                                    new_packet.WriteUInt8(11); // PC LIMIT ERROR
                                                    m_LocalSecurity.Send(new_packet);
                                                    Send(false);

                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region New anti exploit(gateway)
                                    if (this.sent_list != 1 || this.sent_id != 1)
                                    {
                                        // Byebye
                                        //Logger.WriteLine(//Logger.LogLevel.Error, $"Client({this.ip}:{this.user_id}) tried to exploit 0x6102");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    #region IP LIMIT
                                    if ((FilterMain.IP_LIMIT > 0) && !(FilterMain.bypass.Contains(this.user_id)))
                                    {
                                        try
                                        {
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.DATABASE}].[dbo].[_CountIP] '{this.user_id}', '{this.ip}'")).Result + 1; // +1 because current connection is 1.
                                            if (FilterMain.CAFE_LIMIT > 0 && (FilterMain.cafe.Contains(this.ip)))
                                            {
                                                // COUNT +1 BECAUSE ALWAYS 1 LESS
                                                if (current_count > FilterMain.CAFE_LIMIT)
                                                {
                                                    // Send client ERROR
                                                    Packet new_packet = new Packet(0xA102, false);
                                                    new_packet.WriteUInt8(0x02);
                                                    new_packet.WriteUInt8(8); // ip limit error
                                                    m_LocalSecurity.Send(new_packet);
                                                    Send(false);

                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                // COUNT +1 BECAUSE ALWAYS 1 LESS
                                                if (current_count > FilterMain.IP_LIMIT)
                                                {
                                                    // Send client ERROR
                                                    Packet new_packet = new Packet(0xA102, false);
                                                    new_packet.WriteUInt8(0x02);
                                                    new_packet.WriteUInt8(8); // ip limit error
                                                    m_LocalSecurity.Send(new_packet);
                                                    Send(false);

                                                    continue;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    #region Login status
                                    try
                                    {
                                        this.login_status = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.DATABASE}].[dbo].[_CheckLogin] '{this.user_id}', '{Program.MD5_Encode(this.user_pw)}'")).Result;
                                    }
                                    catch { }
                                    #endregion

                                    #region PC LIMIT
                                    if (FilterMain.PC_LIMIT > 0 && !(FilterMain.names.Contains(this.user_id)))
                                    {
                                        #region Log bypassers
                                        if (this.bypass_xx > 0 && (this.login_status > 0))
                                        {
                                            try
                                            {
                                                Task.Run(async () => await sqlCon.prod_int($"EXEC [{FilterMain.DATABASE}].[dbo].[_HwidBypass] '{this.user_id}', '{this.ip}', '{this.mac}', 'Bypassed dll #{this.bypass_xx}'"));
                                            }
                                            catch { }
                                        }
                                        #endregion

                                        #region Try get hwid by StrUserID
                                        if (this.hwid_sent && (this.login_status > 0))
                                        {
                                            try
                                            {
                                                Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE}].[dbo].[_UpdateHWID] '{this.user_id}', '{this.ip}', '{this.mac}', '{this.serial}', '{this.hwid}'"));
                                            }
                                            catch {
                                                Logger.WriteLine(Logger.LogLevel.Error, $"_try_UpdateHWID sent from {this.user_id}");
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                this.mac = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.DATABASE}].[dbo].[_ReturnMac] '{this.user_id}'")).Result;
                                                this.hwid = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.DATABASE}].[dbo].[_ReturnHWID] '{this.user_id}'")).Result;
                                                this.serial = Task.Run(async () => await sqlCon.prod_string($"[{FilterMain.DATABASE}].[dbo].[_ReturnSERIAL] '{this.user_id}'")).Result;
                                                if (this.mac == "non" && this.login_status > 0)
                                                {
                                                    Logger.WriteLine(Logger.LogLevel.Error, $"No HWID sent from {this.user_id}");
                                                    continue;
                                                }
                                            }
                                            catch {
                                                Logger.WriteLine(Logger.LogLevel.Error, $"try catch HWID sent from {this.user_id}");
                                            }
                                        }
                                        #endregion

                                        #region HWID count
                                        try
                                        {
                                            int current_count = Task.Run(async () => await sqlCon.prod_int($"[{FilterMain.DATABASE}].[dbo].[_CountHWID] '{this.user_id}', '{this.mac}'")).Result + 1; // +1 because current connection is 1.
                                            if (current_count > FilterMain.PC_LIMIT)
                                            {
                                                // Send client error
                                                Packet new_packet = new Packet(0xA102, false);
                                                new_packet.WriteUInt8(0x02);
                                                new_packet.WriteUInt8(10); // PC LIMIT ERROR
                                                m_LocalSecurity.Send(new_packet);
                                                Send(false);
                                                continue;
                                            }
                                        }
                                        catch { }
                                        #endregion
                                    }
                                    #endregion

                                    if (FilterMain.BOT_CONTROLL)
                                    {
                                        if (locale == 51)
                                        {
                                            Packet login = new Packet(0x6102, true);
                                            login.WriteUInt8(22);
                                            login.WriteAscii(this.user_id);
                                            login.WriteAscii(this.user_pw);
                                            login.WriteUInt16(ServerID);
                                            m_RemoteSecurity.Send(login);
                                            Send(true);
                                            continue;
                                        }
                                    }
                                }
                                #endregion

                                #region 0x6100_CLIENT_GLOBAL_RESPONSE_ID
                                else if (opcode == 0x6100)
                                {
                                    this.packet_handler = false;
                                    this.sent_id = 1;
                                }
                                #endregion

                                #region 0x6101_CLIENT_SERVER_LIST
                                else if (opcode == 0x6101)
                                {
                                    this.packet_handler = false;
                                    this.sent_list = 1;
                                }
                                #endregion

                                #region 0x1420_CLIENT_PC_LIMIT_vSRO188
                                else if (opcode == 0x1420 && (FilterMain.FILES.Contains("188")))
                                {
                                    for (int i = 0; i<76; i++)
                                    {
                                        byte lel = _pck.ReadUInt8();
                                        if(lel != 255)
                                        {
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }

                                    this.hwid = sqlCon.clean(_pck.ReadAscii().Trim());
                                    this.mac = sqlCon.clean(_pck.ReadAscii().Trim().ToUpper());
                                    this.serial = sqlCon.clean(_pck.ReadAscii().Trim());
                                    string date = _pck.ReadAscii().Trim();

                                    #region Fail check #1
                                    if (this.mac.Length != 17)
                                    {
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    #region Fail check #2
                                    for (int i = 0; i < 6; i++)
                                    {
                                        if (this.mac.Split('-')[i].Length != 2)
                                        {
                                            Logger.WriteLine("lel #2");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }
                                    #endregion

                                    #region Fail check #3
                                    if (this.mac.Split('-').Length != 6)
                                    {
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    #endregion

                                    if(!date.Contains("2019"))
                                    {
                                        Logger.WriteLine("CLIENT_PC_LIMIT #4");
                                        this.DisconnectModuleSocket();
                                        return;
                                    }
                                    else
                                    {
                                        string new_date = date.Replace("2019", "1991");
                                        string enc = Program.MD5_Encode("_HAYALPC_" + this.mac + new_date);
                                        if (enc != this.hwid)
                                        {
                                            Logger.WriteLine($"Wrong encryption key "+ this.mac);
                                            //Logger.WriteLine($"If you are using EdxLoader remove MultiClient or you will face this problem.");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                        else
                                        {
                                            Logger.WriteLine($"Mac:" + this.mac);
                                        }
                                    }

                                    for (int i = 0; i < 38; i++)
                                    {
                                        byte lel = _pck.ReadUInt8();
                                        if (lel != 255)
                                        {
                                            Logger.WriteLine("lel");
                                            this.DisconnectModuleSocket();
                                            return;
                                        }
                                    }

                                    this.hwid_sent = true;
                                    Send(false);
                                    continue;
                                }
                                #endregion

                                #region OpcodeHandler
                                if (this.packet_handler)
                                {
                                    int OpcodesHandler = Opcodes.GatewayServer(_pck);
                                    switch (OpcodesHandler)
                                    {
                                        /*
                                        0 = nothing;
                                        1 = prevent;
                                        2 = disconnect;
                                        3 = ban;
                                        */

                                        case 1:
                                            {
                                                continue;
                                            }
                                            break;

                                        case 2:
                                            {
                                                //Logger.WriteLine(//Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was kicked for exploiting");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                            break;

                                        case 3:
                                            {
                                                //Logger.WriteLine(//Logger.LogLevel.Warning, $"Client({this.ip}:{this.user_id}) was banned for exploiting");
                                                FirewallHandler.BlockIP(this.ip, "Exploiting");
                                                this.DisconnectModuleSocket();
                                                return;
                                            }
                                            break;
                                    }
                                }
                                #endregion

                                #region Clear logs
                                /*
                                if (m_LastPackets.Count > 100)
                                {
                                    m_LastPackets.Clear();
                                }

                                Packet CopyOfPacket = _pck;
                                m_LastPackets.Enqueue(CopyOfPacket);
                                */
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
    }
}
