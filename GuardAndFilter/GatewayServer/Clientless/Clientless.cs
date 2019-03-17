using System;
using System.Threading;
using Framework;

namespace Filter
{
    public class Clientless
    {
        public string Username;
        public string Password;
        public string Character;
        public Gateway GW;
        public Agent AG;
        public bool DC;
        public ClientlessMode Mode;
        public static iniFile cfg = new iniFile("config/settings.ini");
        public static iniFile language = new iniFile("config/language.ini");

        public Clientless(string _username, string _password, string _character)
        {
            Username = _username;
            Password = _password;
            Character = _character;

            GW = null;
            AG = null;

            DC = true;

            Mode = ClientlessMode.None;

            Logger.WriteLine(Logger.LogLevel.EventBot, $"Added new Clientless [ {Username} ] [ {Character} ]");
            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Clientless]: Added new Clientless [ {Username} ] [ {Character} ]");

            Thread.Sleep(5000);

            // Clientless thread calling
            new Thread(() => { ClientlessThread(); }).Start();

            // Timer for reading shit.
        }

        void ClientlessThread()
        {
            while (true)
            {
                if (DC)
                {
                    try
                    {
                        if (AG != null)
                        {
                            if (!AG.Exit)
                            {
                                AG.Exit = true;
                                AG.Disconnect();
                            }
                            else
                                AG = null;
                        }
                    }
                    catch { }

                    try
                    {
                        if (GW != null)
                        {
                            if (!GW.Exit)
                            {
                                GW.Exit = true;
                                GW.Disconnect();
                            }
                            else
                                GW = null;
                        }
                    }
                    catch { }

                    if (GW == null && AG == null)
                    {
                        if (FilterMain.ENABLE_CLIENTLESS)
                        {
                            DC = false;
                            Logger.WriteLine(Logger.LogLevel.EventBot, $"Restarting Clientless [ {0} ] [ {1} ]", Username, Character);
                            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [Clientless]: Restarting Clientless [ {Username} ] [ {Character} ]");

                            GW = new Gateway(this);
                        }
                    }

                    Thread.Sleep(5000);

                    continue;
                }

                if (Mode == ClientlessMode.Gateway)
                {
                    GW.Security.Send(new Packet(0x2002));
                    GW.SendToServer();
                }
                else if (Mode == ClientlessMode.Agent)
                {
                    AG.Security.Send(new Packet(0x2002));
                    AG.SendToServer();
                }



                Thread.Sleep(8000);
            }
        }
    }

    public enum ClientlessMode
    {
        None,
        Gateway,
        Agent
    }
}
