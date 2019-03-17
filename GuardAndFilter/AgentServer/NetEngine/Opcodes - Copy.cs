using System;
using Framework;

namespace Filter.NetEngine
{
    class Opcodes
    {
        public static object locker = new object();

        #region Badword blocker
        public static bool badword(string word)
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

        public static int AgentServer(Packet _pck)
        {
            /*
            0 = default;
            1 = continue;
            2 = disconnect;
            3 = firewall block;
            */
            ushort opcode = Convert.ToUInt16(_pck.Opcode.ToString().ToLower());

            //if (FilterMain.debug_mike)
            //{
            //    byte[] receivePacketBytes = _pck.GetBytes();
            //    Logger.WriteLine(Logger.LogLevel.MikeMode, $"[C->S][{opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(receivePacketBytes)}{Environment.NewLine}");
            //}

            #region KNOWN EXPLOITS BLOCKER
            if (FilterMain.BAD_Opcodes.ContainsKey(Convert.ToUInt16(opcode.ToString().ToLower())))
            {
                return FilterMain.RULE;
            }
            #endregion

            else
            {

                #region 0x705E_CLIENT_SQL_INJECT_FORTRESS
                if (opcode == 0x705e)
                {
                    if(FilterMain.BATTLE_ROYALE_MODE)
                    {
                        return 1;
                    }

                    UInt32 unk = _pck.ReadUInt32();
                    byte action = _pck.ReadUInt8();

                    switch(action)
                    {
                        case 26:
                            {
                                string message = _pck.ReadAscii().ToString().ToLower();
                                if (message.Contains("'"))
                                {
                                    FilterMain.blocked_agent_sqlinject++;
                                    return 1;
                                }
                                else if (message.Contains("\""))
                                {
                                    FilterMain.blocked_agent_sqlinject++;
                                    return 1;
                                }
                            }
                            break;


                    }
                }
                #endregion

                #region 0x70f9_GUILD_DESCRIPTION
                else if (opcode == 0x70f9)
                {
                    string title = _pck.ReadAscii().ToString().ToLower();
                    if (title.Length < 3) return 1;
                    string message = _pck.ReadAscii().ToString().ToLower();
                    if (message.Length < 3) return 1;

                    if (title.StartsWith(" ") || message.StartsWith(" "))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (title.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (title.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x7256_GRANT_NAME
                else if (opcode == 0x7256)
                {
                    _pck.ReadUInt32();

                    string grant_name = _pck.ReadAscii().ToString().ToLower();
                    if (grant_name.Length < 3) return 1;

                    if (grant_name.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (grant_name.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(grant_name))
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x730c_GUILD_MESSAGE
                else if (opcode == 0x730c)
                {
                    byte stat = _pck.ReadUInt8();
                    _pck.ReadUInt8();

                    string charname = _pck.ReadAscii().ToString().ToLower();
                    if (charname.Length < 3) return 1;
                    string message = _pck.ReadAscii().ToString().ToLower();
                    if (message.Length < 3) return 1;

                    if (stat == 2)
                    {
                        string message2 = _pck.ReadAscii().ToString().ToLower();
                        if (message2.Length < 3) return 1;


                        if (message2.Contains("'"))
                        {
                            FilterMain.blocked_agent_sqlinject++;
                            return 1;
                        }
                        else if (message2.Contains("\""))
                        {
                            FilterMain.blocked_agent_sqlinject++;
                            return 1;
                        }

                        if (badword(message2))
                        {
                            return 1;
                        }
                    }

                    if (charname.StartsWith(" ") || message.StartsWith(" "))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (charname.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (charname.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x7110_GUILD_WAR_CREATE
                else if (opcode == 0x7110)
                {
                    string message = _pck.ReadAscii().ToString().ToLower();
                    if (message.Length < 3) return 1;

                    if (message.StartsWith(" "))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x7309_PRIVATE_MESSAGE
                else if (opcode == 0x7309)
                {
                    string charname = _pck.ReadAscii().ToString().ToLower();
                    if (charname.Length < 3) return 1;
                    string message = _pck.ReadAscii().ToString().ToLower();
                    if (message.Length < 3) return 1;

                    if (charname.StartsWith(" ") || message.StartsWith(" "))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (charname.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (charname.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x7060_PARTY_INVITE
                else if(opcode == 0x7060)
                {
                    if(FilterMain.BATTLE_ROYALE_MODE)
                    {
                        return 1;
                    }
                }
                #endregion

                #region 0x7069_PARTY_MATCH_REGISTER
                else if (opcode == 0x7069)
                {
                    if (FilterMain.BATTLE_ROYALE_MODE)
                    {
                        return 1;
                    }

                    _pck.ReadUInt32(); // Party number
                    _pck.ReadUInt32(); // Type
                    _pck.ReadUInt8(); // Race
                    _pck.ReadUInt8(); // Purpose of party
                    byte enter = _pck.ReadUInt8(); // Enter level
                    byte level = _pck.ReadUInt8(); // Max level
                    string message = _pck.ReadAscii().ToString().ToLower(); // Title

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }

                    if (level > 200 || enter > 200) return 1;
                }
                #endregion

                #region 0x706a_PARTY_MATCH_EDIT
                else if (opcode == 0x706a)
                {
                    if (FilterMain.BATTLE_ROYALE_MODE)
                    {
                        return 1;
                    }

                    _pck.ReadUInt32(); // Party number
                    _pck.ReadUInt32(); // Type
                    _pck.ReadUInt8(); // Race
                    _pck.ReadUInt8(); // Purpose of party
                    byte enter = _pck.ReadUInt8(); // Enter level
                    byte level = _pck.ReadUInt8(); // Max level
                    string message = _pck.ReadAscii().ToString().ToLower(); // Title

                    if (message.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (message.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }

                    if (badword(message))
                    {
                        return 1;
                    }

                    if (level > 200 || enter > 200) return 1;
                }
                #endregion

                #region 0x7470_CLIENT_ACADEMY_CREATE
                else if (opcode == 0x7470 && (FilterMain.ACADEMY_CREATE_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x7472_CLIENT_ACADEMY_INVITE
                else if (opcode == 0x7472 && (FilterMain.ACADEMY_INVITE_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x747E_CLIENT_ACADEMY_JOIN
                else if (opcode == 0x747e && (FilterMain.ACADEMY_JOIN_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x347F_CLIENT_ACADEMY_ACCEPT
                else if (opcode == 0x747e && (FilterMain.ACADEMY_ACCEPT_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x7473_CLIENT_ACADEMY_BAN
                else if (opcode == 0x7473 && (FilterMain.ACADEMY_BAN_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x7474_CLIENT_ACADEMY_LEAVE
                else if (opcode == 0x7474 && (FilterMain.ACADEMY_LEAVE_DISABLED))
                {
                    return 1;
                }
                #endregion

                #region 0x7302_CLIENT_ADD_FRIEND
                else if(opcode == 0x7302)
                {
                    string name = _pck.ReadAscii();

                    if (name.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (name.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                }
                #endregion

                #region 0x730d_CLIENT_BLOCK_WHISPER
                else if(opcode == 0x730d)
                {
                    _pck.ReadUInt8();
                    string name = _pck.ReadAscii();

                    if (name.Contains("'"))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                    else if (name.Contains("\""))
                    {
                        FilterMain.blocked_agent_sqlinject++;
                        return 1;
                    }
                }
                #endregion



                else
                {
                    return 0;
                }
                return 0;
            }
        }
    }
}
