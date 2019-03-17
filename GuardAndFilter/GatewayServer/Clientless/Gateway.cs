using Framework;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Filter
{
    public class Gateway
    {
        public byte[] Buffer;
        public Socket Socket;
        public Security Security;
        public Clientless Clientless;
        public bool Exit;
        public bool Success;
        public static iniFile cfg = new iniFile("config/settings.ini");
        public static iniFile language = new iniFile("config/language.ini");

        public Gateway(Clientless _clientless)
        {
            Socket = null;
            Security = new Security();
            Buffer = new byte[4096];
            Clientless = _clientless;
            Exit = false;
            Success = false;

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(FilterMain.gateway_remote, FilterMain.gateway_fport, new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }

            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Connecting with:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
            Logger.WriteLine(Logger.LogLevel.EventBot, $"Connecting with:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
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
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
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
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }
        }

        void RecieveCallback(IAsyncResult ar)
        {
            int Recieved = 0;
            try { Recieved = Socket.EndReceive(ar); }
            catch (Exception Ex)
            {
                if (!Exit)
                {
                    Exit = true;
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
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
                        //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
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
                        foreach (Packet packet in packets)
                        {
                            if (Handler.Gateway(Clientless, packet) == Handler.ReturnType.Break)
                            {
                                Logger.WriteLine("opcode: " + packet.Opcode + " raw:" + packet.ReadUInt8());
                                break;
                            }

                        }
                    }
                }
                catch (Exception Ex)
                {
                    if (!Exit)
                    {
                        Exit = true;
                        //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
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
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
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
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: {Ex.ToString()}");
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"{Ex.ToString()}");
                    Disconnect();
                }
            }
        }

        public void Disconnect(int send = 0)
        {
            if (!Success) Clientless.DC = true;

            if (send == 0)
            {
                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Disconnected:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
                Logger.WriteLine(Logger.LogLevel.EventBot, $"Disconnected:[ StrUserID: {Clientless.Username} ] [ CharName16: {Clientless.Character} ]");
            }

            if (Clientless.Mode == ClientlessMode.Gateway)
                Clientless.Mode = ClientlessMode.None;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                //Socket.Close();
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
    }
}
