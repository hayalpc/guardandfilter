#pragma warning disable
using System.Net;
using System.Net.Sockets;

namespace Filter.NetEngine
{
    sealed class HwidContext
    {
        Socket m_ClientSocket = null;
        AsyncServer.E_ServerType m_HandlerType;
        object m_Lock = new object();
        Socket m_ModuleSocket = null;
        string mac = "non";
        string ip;
        string encryption;

        public HwidContext(Socket ClientSocket)
        {
            this.m_ClientSocket = ClientSocket;
            this.m_HandlerType = AsyncServer.E_ServerType.HwidServer;
            this.m_ModuleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ip = ((IPEndPoint)(m_ClientSocket.RemoteEndPoint)).Address.ToString();

            try
            {
                byte[] test = new byte[100];
                ClientSocket.Receive(test, 0, 100, SocketFlags.Peek);

                string shit = System.Text.ASCIIEncoding.ASCII.GetString(test);
                string shit_2 = shit.Trim();
                shit_2 = shit_2.Replace(" ", string.Empty);

                #region Make sure that request is GET
                if (!shit_2.StartsWith("GET"))
                {
                    Logger.WriteLine("Debug #1");
                    this.Disconnect();
                    return;
                }
                #endregion

                #region First line
                string[] shit_3 = shit_2.Split('?');
                string shit_4 = shit_3[1];
                if (!shit_4.Contains("hwid"))
                {
                    Logger.WriteLine("Debug #2");
                    this.Disconnect();
                    return;
                }
                shit_4 = shit_4.Replace("hwid=", string.Empty);
                this.mac = shit_4.Substring(0, 18).ToUpper();
                #endregion

                #region Make sure it ends with ;
                if (this.mac.EndsWith(";"))
                {
                    this.mac = this.mac.Replace(";", string.Empty);
                }
                else
                {
                    Logger.WriteLine("Debug #3");
                    this.Disconnect();
                    return;
                }
                #endregion

                #region Second line
                string[] aids_1 = shit_2.Split('&');
                string aids_2 = aids_1[1];
                if (!aids_2.Contains("enc"))
                {
                    Logger.WriteLine("Debug #4");
                    this.Disconnect();
                    return;
                }
                aids_2 = aids_2.Replace("enc=", string.Empty);
                this.encryption = aids_2.Substring(0, 21);
                #endregion

                #region Make sure it ends with ;
                if (this.encryption.EndsWith(";"))
                {
                    this.encryption = this.encryption.Replace(";", string.Empty);
                }
                else
                {
                    Logger.WriteLine("Debug #5");
                    this.Disconnect();
                    return;
                }
                #endregion

                #region Mac protection
                if (this.mac.Length != 17)
                {
                    Logger.WriteLine("Debug #6");
                    this.Disconnect();
                    return;
                }

                for (int i = 0; i < 6; i++)
                {
                    if (this.mac.Split('-')[i].Length != 2)
                    {
                        Logger.WriteLine("Debug #7");
                        this.Disconnect();
                        return;
                    }
                }

                if (this.mac.Split('-').Length != 6)
                {
                    Logger.WriteLine("Debug #8");
                    this.Disconnect();
                    return;
                }

                if(this.encryption.Length != 20)
                {
                    Logger.WriteLine("Debug #9");
                    this.Disconnect();
                    return;
                }
                #endregion

                #region END
                if (FilterMain.mac_list.ContainsKey(this.ip))
                {
                    FilterMain.mac_list[this.ip] = this.mac;
                }
                else
                {
                    FilterMain.mac_list.Add(this.ip, this.mac);
                }

                if (FilterMain.mac_encryption.ContainsKey(this.mac))
                {
                    FilterMain.mac_encryption[this.mac] = this.encryption;
                }
                else
                {
                    FilterMain.mac_encryption.Add(this.mac, this.encryption);
                }
                #endregion
            }
            catch { }

            this.Disconnect();
            return;
        }

        void Disconnect()
        {
            try
            {
                if (this.m_ModuleSocket != null)
                {
                    this.m_ModuleSocket.Close();
                    this.m_ModuleSocket = null;
                }
                this.m_ModuleSocket = null;
            }
            catch { }
        }
    }
}
