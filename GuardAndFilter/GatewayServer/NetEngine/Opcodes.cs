using Framework;
using System;

namespace Filter.NetEngine
{
    class Opcodes
    {
        public static int GatewayServer(Packet _pck)
        {
            /*
            0 = default;
            1 = continue;
            2 = disconnect;
            */

            ushort opcode = Convert.ToUInt16(_pck.Opcode.ToString().ToLower());

            //if (FilterMain.debug_mike)
            //{
            //    byte[] receivePacketBytes = _pck.GetBytes();
            //    //Logger.WriteLine(Logger.LogLevel.MikeMode, $"[C->S][{_pck.Opcode:X4}][{_pck.GetBytes().Length} bytes]{(_pck.Encrypted ? "[Encrypted]" : "")}{(_pck.Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(receivePacketBytes)}{Environment.NewLine}");
            //}

            #region KNOWN EXPLOITS BLOCKER
            if (FilterMain.BAD_Opcodes.ContainsKey(opcode))
            {
                return FilterMain.RULE;
            }
            else
            {
                return 0;
            }
            #endregion

#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
