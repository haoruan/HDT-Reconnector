using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;

using HDT_Reconnector.Native;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector
{
    public class Reconnector
    {
        private const string hsName = "Hearthstone";

        public const string ReconnectString = "Reconnect";
        public const string DisconnectedString = "Disconnected";

        public CONNECTION_STATUS Status { get; set; } = CONNECTION_STATUS.CONNECTED;

        public enum CONNECTION_STATUS
        {
            DISCONNECTED,
            CONNECTED
        }

        public void ResumeConnect()
        {
            Log.Info("Reconnecting...");
        }

        public int Disconnect(string addr, ushort port)
        {
            (Iphlpapi.MIB_TCPROW hsTcpRow, int err) = GetReconnectTcp(addr, port);

            if (err != 0)
            {
                Log.Info("Can't find the server connection, abort disconnection");
                return 1;
            }

            hsTcpRow.state = (uint)Iphlpapi.MIB_TCP_STATE.MIB_TCP_STATE_DELETE_TCB;

            IntPtr hsTcpRowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Iphlpapi.MIB_TCPROW)));
            Marshal.StructureToPtr(hsTcpRow, hsTcpRowPtr, false);

            Iphlpapi.SetTcpErrorCode ret = (Iphlpapi.SetTcpErrorCode)Iphlpapi.SetTcpEntry(hsTcpRowPtr);

            Marshal.FreeHGlobal(hsTcpRowPtr);

            switch (ret)
            {
                case Iphlpapi.SetTcpErrorCode.NO_ERROR:
                    Log.Info("Disconnect successfully");
                    Status = CONNECTION_STATUS.DISCONNECTED;
                    return 0;
                case Iphlpapi.SetTcpErrorCode.ERROR_ACCESS_DENIED:
                    Log.Error("Access denied");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_INVALID_PARAMETER:
                    Log.Error("Invalid parameter");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_ELEVATED:
                    Log.Error("Not elevated");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_SUPPORTED:
                    Log.Error("Not supported");
                    return 1;
                default:
                    Log.Error("Other errors");
                    return 1;
            }
        }

        private Process[] GetHsProcess()
        {
            return Process.GetProcessesByName(hsName);
        }

        private (Iphlpapi.MIB_TCPROW, int) GetReconnectTcp(string addr, ushort port)
        {
            Process[] hsProcesses = GetHsProcess();
            List<Iphlpapi.MIB_TCPROW_OWNER_MODULE> tcprows = Iphlpapi.GetAllTCPConnections();
            HashSet<uint> hsPids = new HashSet<uint>();
            Iphlpapi.MIB_TCPROW hsTcpRow = new Iphlpapi.MIB_TCPROW();

            foreach (Process hsProcess in hsProcesses)
            {
                hsPids.Add((uint)hsProcess.Id);
            }

            for (int i = tcprows.Count - 1; i >= 0; i--)
            {
                if (hsPids.Contains(tcprows[i].ProcessId))
                {
                    int remotePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(tcprows[i].remotePort, 0));
                    string remoteAddr = IPAddress.Parse(tcprows[i].RemoteAddress.ToString()).ToString();

                    if (remotePort == port && remoteAddr == addr)
                    {
                        Log.Info(String.Format("TCP connection: {0}:{1}", remoteAddr, remotePort));

                        hsTcpRow.localAddr = tcprows[i].localAddr;
                        hsTcpRow.localPort = tcprows[i].localPort;
                        hsTcpRow.remoteAddr = tcprows[i].remoteAddr;
                        hsTcpRow.remotePort = tcprows[i].remotePort;

                        return (hsTcpRow, 0);
                    }
                }
            }

            return (hsTcpRow, 1);
        }
    }
}
