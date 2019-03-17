using Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Filter
{
    public static class Handler
    {
        public static int sender_ok = 0;
        public static Clientless peta;
        public static iniFile cfg = new iniFile("config/settings.ini");
        public static iniFile language = new iniFile("config/language.ini");

        public static ReturnType Gateway(Clientless C, Packet _pck)
        {
            ReturnType RT = ReturnType.Continue;
            switch (_pck.Opcode)
            {
                case 0x2001: RT = GATEWAY_GLOBAL_IDENTIFICATION(C, _pck); break;
                case 0xA100: RT = GATEWAY_PATCH_RESPONSE(C, _pck); break;
                case 0xA101: RT = GATEWAY_SERVERLIST_RESPONSE(C, _pck); break;
                case 0xA102: RT = GATEWAY_LOGIN_RESPONSE(C, _pck); break;
                case 0x2322: RT = GATEWAY_LOGIN_IBUV_CHALLENGE(C, _pck); break;
            }
            return RT;
        }

        public static ReturnType Agent(Clientless C, Packet _pck, string message = null)
        {
            ReturnType RT = ReturnType.Continue;
            switch (_pck.Opcode)
            {
                case 0x2001: RT = AGENT_GLOBAL_IDENTIFICATION(C, _pck); break;
                case 0xA103: RT = AGENT_LOGIN_RESPONSE(C, _pck); break;
                case 0xB007: RT = AGENT_CHARACTER_SCREEN(C, _pck); break;
                case 0x3020: RT = AGENT_CELESTIAL_POSITION(C, _pck); break;
            }
            return RT;
        }

        public static ReturnType GATEWAY_GLOBAL_IDENTIFICATION(Clientless C, Packet _pck)
        {
            try
            {
                if (_pck.ReadAscii() == "GatewayServer")
                {
                    C.Mode = ClientlessMode.Gateway;

                    Packet response = new Packet(0x6100, true, false);
                    response.WriteUInt8(FilterMain.ServerLocale);
                    response.WriteAscii("SR_Client");
                    response.WriteUInt32(FilterMain.ServerVersion);

                    C.GW.Security.Send(response);
                }

            }
            catch { }
            return ReturnType.Continue;
        }

        public static ReturnType GATEWAY_PATCH_RESPONSE(Clientless C, Packet _pck)
        {
            try
            {
                byte result = _pck.ReadUInt8();
                if (result == 1)
                {
                    // send hardware ip packet here, if nessessary...

                    C.GW.Security.Send(new Packet(0x6101, true));
                }
                else
                {
                    byte error = _pck.ReadUInt8();
                    switch (error)
                    {
                        case 2: // Client patch
                            {
                                FilterMain.ServerVersion++;
                                try
                                {
                                    cfg.IniWriteValue("CLIENTLESS", "VERSION", FilterMain.ServerVersion.ToString());
                                }
                                catch { }
                            }
                            break;
                        case 1: // Invalid client. Program will be terminated.
                        case 3: // Invalid client. Program will be terminated.
                        case 4: // The server is undergoing inspection or updates.
                        case 5: // You have to install the full version.
                        default:
                            break;
                    }

                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Invalid Version:[ {C.Username} ] [ {C.Character} ]");
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Invalid Version:[ {C.Username} ] [ {C.Character} ]");

                    if (!C.GW.Exit)
                    {
                        C.GW.Exit = true;
                        C.GW.Disconnect();
                    }

                    return ReturnType.Break;
                }

            }
            catch { }
            return ReturnType.Continue;
        }

        public static ReturnType GATEWAY_SERVERLIST_RESPONSE(Clientless C, Packet _pck)
        {
            try
            {
                Packet response = new Packet(0x6102);
                response.WriteUInt8(FilterMain.ServerLocale);
                response.WriteAscii(C.Username);
                response.WriteAscii(C.Password);
                response.WriteUInt16(FilterMain.ShardID);
                C.GW.Security.Send(response);
            }
            catch { }
            return ReturnType.Continue;
        }

        public static ReturnType GATEWAY_LOGIN_RESPONSE(Clientless C, Packet _pck)
        {
            try
            {
                byte num = _pck.ReadUInt8();
                if (num == 1)
                {
                    uint id = _pck.ReadUInt32();
                    string ip = _pck.ReadAscii();
                    ushort port = _pck.ReadUInt16();

                    C.GW.Success = true;

                    C.AG = new Agent(C, id, ip, port);

                    if (!C.GW.Exit)
                    {
                        C.GW.Exit = true;
                        C.GW.Disconnect(1);
                    }
                    return ReturnType.Break;
                }
                else if (num == 2)
                {
                    byte error = _pck.ReadUInt8();
                    switch (error)
                    {
                        case 1: // Wrong Password
                            {
                                _pck.ReadUInt8(); // Total tries
                                _pck.ReadUInt8(); //
                                _pck.ReadUInt8(); //
                                _pck.ReadUInt8(); //
                                _pck.ReadUInt8(); // Used tries

                                Logger.WriteLine(Logger.LogLevel.EventBot, $"Invalid Password:[ {C.Username} ] [ {C.Character} ]");
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Invalid Password:[ {C.Username} ] [ {C.Character} ]");

                                if (!C.GW.Exit)
                                {
                                    C.GW.Exit = true;
                                    C.GW.Disconnect();
                                }

                                break;
                            }
                        case 2: // Blocked
                            {
                                byte type = _pck.ReadUInt8();
                                switch (type)
                                {
                                    case 1: // Blocked login
                                        {
                                            _pck.ReadAscii(); // Reason
                                            break;
                                        }
                                    case 2: // chat? idk
                                    case 3: // trade? idk
                                    case 4: // inspect? idk
                                        break;
                                }

                                Logger.WriteLine(Logger.LogLevel.EventBot, $"Account Blocked:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Account Blocked:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");

                                if (!C.GW.Exit)
                                {
                                    C.GW.Exit = true;
                                    C.GW.Disconnect();
                                }
                                break;
                            }
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            {
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Login Error:[Num: {error} ] [StrUserID: {C.Username} ] [CharName16: {C.Character} ]");
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"Login Error:[Num: {error} ] [StrUserID: {C.Username} ] [CharName16: {C.Character} ]");
                            }
                            break;
                        case 13: // GM PRIV IP
                            {
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"GM IP missing in [{FilterMain.DATABASE_ACC}].[dbo].[_PrivilegedIP], add {FilterMain.gateway_local} and restart GatewayServer.exe");
                            }
                            break;
                        case 8: // Failed to connect to server because access to the current IP has exceeded its limit.
                        case 10: // Only adults over the age of 18 are allowed to connect to server.
                        case 11: // Only users over the age of 12 are allowed to connect to the server.
                        case 12: // Adults over the age of 18 are not allowed to connect to the Teen server.
                        default:
                            {
                                Logger.WriteLine(Logger.LogLevel.EventBot, $"Login Error:[ Num: {error} ] [ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");
                                //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [GatewayServer]: Login Error:[Num: {error} ] [StrUserID: {C.Username} ] [CharName16: {C.Character} ]");

                                if (!C.GW.Exit)
                                {
                                    C.GW.Exit = true;
                                    C.GW.Disconnect();
                                }
                                break;
                            }
                    }
                }

                if (!C.GW.Exit)
                {
                    C.GW.Exit = true;
                    C.GW.Disconnect();
                }

            }
            catch { }

            return ReturnType.Break;
        }

        public static ReturnType GATEWAY_LOGIN_IBUV_CHALLENGE(Clientless C, Packet _pck)
        {
            try
            {
                Packet response = new Packet(0x6323);
                response.WriteAscii(FilterMain.gateway_captcha);
                C.GW.Security.Send(response);
            }
            catch { }
            return ReturnType.Continue;
        }

        public static ReturnType AGENT_GLOBAL_IDENTIFICATION(Clientless C, Packet _pck)
        {
            try
            {
                if (_pck.ReadAscii() != "GatewayServer")
                {
                    C.Mode = ClientlessMode.Agent;

                    Packet response = new Packet(0x6103);

                    response.WriteUInt32(C.AG.LoginID);
                    response.WriteAscii(C.Username);
                    response.WriteAscii(C.Password);
                    response.WriteUInt8(FilterMain.ServerLocale);
                    response.WriteUInt32(0);
                    response.WriteUInt16(0);

                    C.AG.Security.Send(response);
                }
            }
            catch { }

            return ReturnType.Continue;
        }

        public static ReturnType AGENT_LOGIN_RESPONSE(Clientless C, Packet _pck)
        {
            try
            {
                if (_pck.ReadUInt8() == 1)
                {
                    Packet response = new Packet(0x7007);
                    response.WriteUInt8((byte)2);
                    C.AG.Security.Send(response);
                }
                else
                {
                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Agent Login Error:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");
                    //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: Agent Login Error:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");

                    if (!C.AG.Exit)
                    {
                        C.AG.Exit = true;
                        C.AG.Disconnect();
                    }

                    return ReturnType.Break;
                }
            }
            catch { }

            return ReturnType.Continue;
        }

        public static ReturnType AGENT_CHARACTER_SCREEN(Clientless C, Packet _pck)
        {
            try
            {
                if (_pck.ReadUInt8() == 2) // character listening
                {
                    if (_pck.ReadUInt8() == 1) // result
                    {
                        bool CharFound = false;

                        byte charCount = _pck.ReadUInt8();
                        if (charCount > 0)
                        {
                            for (int i = 0; i < charCount; i++)
                            {
                                uint CharID = _pck.ReadUInt32();
                                string CharName = _pck.ReadAscii();

                                if (CharName == C.Character)
                                {
                                    CharFound = true;

                                    Thread.Sleep(2000);

                                    Packet packet = new Packet(0x7001);
                                    packet.WriteAscii(CharName);
                                    C.AG.Security.Send(packet);

                                    break;
                                }
                                else
                                {
                                    uint num8;
                                    byte num9;
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt8();
                                    _pck.ReadUInt64();
                                    _pck.ReadUInt16();
                                    _pck.ReadUInt16();
                                    _pck.ReadUInt16();
                                    _pck.ReadUInt32();
                                    _pck.ReadUInt32();
                                    if (_pck.ReadUInt8() == 1)
                                    {
                                        _pck.ReadUInt32();
                                    }
                                    _pck.ReadUInt16();
                                    _pck.ReadUInt8();
                                    byte num6 = _pck.ReadUInt8();
                                    int num7 = 0;
                                    while (num7 < num6)
                                    {
                                        num8 = _pck.ReadUInt32();
                                        num9 = _pck.ReadUInt8();
                                        num7++;
                                    }
                                    byte num10 = _pck.ReadUInt8();
                                    for (num7 = 0; num7 < num10; num7++)
                                    {
                                        num8 = _pck.ReadUInt32();
                                        num9 = _pck.ReadUInt8();
                                    }
                                }
                            }
                        }

                        if (!CharFound)
                        {
                            if(charCount >= 4)
                            {
                                if (!C.GW.Exit)
                                {
                                    C.GW.Exit = true;
                                    C.GW.Disconnect();
                                    Logger.WriteLine(Logger.LogLevel.EventBot, $"Too many characters on {C.Username} account, please remove some!");
                                }
                            }

                            C.AG.CreatingCharacter = true;

                            Logger.WriteLine(Logger.LogLevel.EventBot, $"Creating Character:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");
                            //FilterMain.startup_list.Add($"9[{DateTime.UtcNow}] [AgentServer]: Creating Character:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");

                            Thread.Sleep(3000);

                            Packet response = new Packet(0x7007);
                            response.WriteUInt8(0x01); // 1 ?
                            response.WriteAscii(C.Character); //name

                            response.WriteUInt32(1907); //refobjid
                            response.WriteUInt8(0x22); //height
                            response.WriteUInt32(3640); //itemid
                            response.WriteUInt32(3641); //itemid
                            response.WriteUInt32(3642); //itemid
                            response.WriteUInt32(3636); //itemid

                            C.AG.Security.Send(response);
                        }
                    }
                }
                else if (_pck.ReadUInt8() == 1 && C.AG.CreatingCharacter) // character listening
                {
                    Task.Run(async () => await sqlCon.exec($"EXEC [{FilterMain.DATABASE}].[dbo].[_EventBotFirst] '{C.Character}'"));
                    Packet response = new Packet(0x7007);
                    response.WriteUInt8(0x02); // 1 ?
                    C.AG.Security.Send(response);
                    C.AG.CreatingCharacter = false;
                }
            }
            catch { }

            return ReturnType.Continue;
        }

        public static ReturnType AGENT_CELESTIAL_POSITION(Clientless C, Packet _pck)
        {
            try
            {
                CharStrings.UniqueID = _pck.ReadUInt32();
                C.AG.Security.Send(new Packet(0x3012));
                //Logger.WriteLine(//Logger.LogLevel.EventBot, $"[AgentServer]: Spawned as:[ StrUserID: {C.Username} ] [ CharName16: {C.Character} ]");
            }
            catch { }
            return ReturnType.Continue;
        }

        public enum ReturnType
        {
            Continue, Break
        }
    }
}
