#pragma warning disable
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Filter.NetEngine
{
    sealed class AsyncServer
    {
        Socket m_ListenerSock = null;
        E_ServerType m_ServerType;

        ManualResetEvent m_Waiter = new ManualResetEvent(false);
        Thread m_AcceptInitThread = null;

        public enum E_ServerType : byte
        {
            AgentServer
        }

        public delegate void delClientDisconnect(ref Socket ClientSocket, E_ServerType HandlerType);

        public bool Start(string BindAddr, int nPort, E_ServerType ServType)
        {
            try
            {
                bool res = false;
                if (m_ListenerSock != null)
                {
                    throw new Exception("Trying to start server on socket which is already in use");
                }

                m_ServerType = ServType;
                m_ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //m_ListenerSock.LingerState = new LingerOption(true, 10);
                //m_ListenerSock.NoDelay = true;
                m_ListenerSock.ReceiveBufferSize = 8192;
                m_ListenerSock.ReceiveTimeout = 1000;
                m_ListenerSock.SendBufferSize = 8192;
                m_ListenerSock.SendTimeout = 1000;

                try
                {
                    Logger.WriteLine("Redirect settings for {" + BindAddr + ":" + nPort + "} was loaded!");

                    m_ListenerSock.Bind(new IPEndPoint(IPAddress.Parse(BindAddr), nPort));
                    m_ListenerSock.Listen(5);

                    m_AcceptInitThread = new Thread(AcceptInitThread);
                    m_AcceptInitThread.Start();
                }
                catch (SocketException SocketEx)
                {
                    Logger.WriteLine(Logger.LogLevel.Error, "Could not bind/listen/BeginAccept socket. Exception: {0}", SocketEx.ToString());
                }

                return res;
            }
            catch { }
            return false;
        }

        void AcceptInitThread()
        {
            try
            {
                while (m_ListenerSock != null)
                {
                    m_Waiter.Reset();
                    try
                    {
                        m_ListenerSock.BeginAccept(
                            new AsyncCallback(AcceptConnectionCallback), null
                            );
                    }
                    catch { }
                    m_Waiter.WaitOne();
                }
            }
            catch { }
        }

        //asynchronous callback on connection accepted
        void AcceptConnectionCallback(IAsyncResult iar)
        {
            Socket ClientSocket = null;

            try
            {
                //AcceptInitThread sleeps...
                m_Waiter.Set();

                iar.AsyncWaitHandle.WaitOne();

                ClientSocket = m_ListenerSock.EndAccept(iar);
                //ClientSocket.LingerState = new LingerOption(true, 10);
                //ClientSocket.NoDelay = true;
                ClientSocket.ReceiveBufferSize = 8192;
                ClientSocket.ReceiveTimeout = 1000;
                ClientSocket.SendBufferSize = 8192;
                ClientSocket.SendTimeout = 1000;

                #region Mikeboty fix
                /*try
                {
                    if (FilterMain.mikeboty_ips.Contains(ip))
                    {
                        ClientSocket.Close();
                        ClientSocket = null;
                        return;
                    }
                }
                catch { }*/
                #endregion

                #region Mikeboty fix
                /*try
                {
                    if (!FilterMain.mikeboty_ips.Contains(ip))
                    {
                        FilterMain.blocked_agent_mikeboty++;
                        byte[] tmp = new byte[2];
                        ClientSocket.Receive(tmp, 0, 2, SocketFlags.Peek);
                        if (Char.IsLetter((char)tmp[0]) || Char.IsLetter((char)tmp[1]))
                        {
                            // HTTP PROTOCOL
                            FirewallHandler.BlockIP(ip, "MikeBoty");
                            FilterMain.mikeboty_ips.Add(ip);
                            ClientSocket.Close();
                            ClientSocket = null;
                            return;
                        }
                    }
                }
                catch { }*/
                #endregion
            }

            catch (SocketException SocketEx)
            {
                return;
            }
            catch (ObjectDisposedException ObjDispEx)
            {
                Logger.WriteLine(Logger.LogLevel.Error, "AcceptConnectionCallback()::ObjectDisposedException while EndAccept. Is server shutting down ? Exception: {0}", ObjDispEx.ToString());
            }

            try
            {
                switch (m_ServerType)
                {
                    case E_ServerType.AgentServer:
                        {
                            //pass socket to agent context handler
                            new AgentContext(ClientSocket, OnClientDisconnect);
                        }
                        break;
                    default:
                        {
                            Logger.WriteLine(Logger.LogLevel.Error, "AcceptConnectionCallback()::Unknown server type");
                        }
                        break;
                }
            }
            catch (SocketException SocketEx)
            {
                Logger.WriteLine(Logger.LogLevel.Error, "AcceptConnectionCallback()::Error while starting context. Exception: {0}", SocketEx.ToString());
            }
        }

        void OnClientDisconnect(ref Socket ClientSock, E_ServerType HandlerType)
        {
            try
            {
                // Check
                if (ClientSock == null)
                {
                    return;
                }

                try
                {
                    ClientSock.Close();
                }
                catch (SocketException SocketEx)
                {
                    Logger.WriteLine(Logger.LogLevel.Error, "OnClientDisconnect()::Error closing socket. Exception: {0}", SocketEx.ToString());
                }
                catch (ObjectDisposedException ObjDispEx)
                {
                    Logger.WriteLine(Logger.LogLevel.Error, "OnClientDisconnect()::Error closing socket (socket already disposed?). Exception: {0}", ObjDispEx.ToString());
                }
                catch
                {
                    Logger.WriteLine(Logger.LogLevel.Error, "Something went wrong with Async systems.");
                }


                ClientSock = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
            catch { }
        }
    }

}
