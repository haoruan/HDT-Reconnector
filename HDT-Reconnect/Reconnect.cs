using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;

using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnect
{
    public class Reconnect
    {
        private const string HsName = "Hearthstone";

        public const string ReconnectString = "Reconnect";
        public const string DisconnectedString = "Disconnected";

        public CONNECTION_STATUS Status
        { get; set; }

        public enum CONNECTION_STATUS
        {
            DISCONNECTED,
            CONNECTED
        }

        public Reconnect()
        {
            Status = CONNECTION_STATUS.DISCONNECTED;
            Iphlpapi.MIB_TCPROW hsTcpRow = getReconnectTcp();
            if (hsTcpRow.remoteAddr != 0)
            {
                Status = CONNECTION_STATUS.CONNECTED;
            }
        }

        public int Disconnect()
        {
            Iphlpapi.MIB_TCPROW hsTcpRow = getReconnectTcp();

            hsTcpRow.state = (uint)Iphlpapi.MIB_TCP_STATE.MIB_TCP_STATE_DELETE_TCB;

            IntPtr hsTcpRowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Iphlpapi.MIB_TCPROW)));
            Marshal.StructureToPtr(hsTcpRow, hsTcpRowPtr, false);

            Iphlpapi.SetTcpErrorCode ret = (Iphlpapi.SetTcpErrorCode)Iphlpapi.SetTcpEntry(hsTcpRowPtr);
            switch (ret)
            {
                case Iphlpapi.SetTcpErrorCode.NO_ERROR:
                    Status = CONNECTION_STATUS.DISCONNECTED;
                    return 0;
                case Iphlpapi.SetTcpErrorCode.ERROR_ACCESS_DENIED:
                    Log.Error("Access Denied");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_INVALID_PARAMETER:
                    Log.Error("Invalid Parameter");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_ELEVATED:
                    Log.Error("Not Elevated");
                    return 1;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_SUPPORTED:
                    Log.Error("Not Supported");
                    return 1;
                default:
                    Log.Error("Other Errors");
                    return 1;
            }
        }

        private Process[] GetHsProcess()
        {
            return Process.GetProcessesByName(HsName);
        }

        private Iphlpapi.MIB_TCPROW getReconnectTcp()
        {
            Process[] hsProcesses = GetHsProcess();
            List<Iphlpapi.MIB_TCPROW_OWNER_PID> tcprows = Iphlpapi.GetAllTCPConnections();
            HashSet<uint> hsPids = new HashSet<uint>();
            Iphlpapi.MIB_TCPROW hsTcpRow = new Iphlpapi.MIB_TCPROW();

            foreach (Process hsProcess in hsProcesses)
            {
                hsPids.Add((uint)hsProcess.Id);
            }

            foreach (Iphlpapi.MIB_TCPROW_OWNER_PID tcprow in tcprows)
            {
                if (hsPids.Contains(tcprow.ProcessId))
                {
                    int remotePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(tcprow.remotePort, 0));
                    string remoteAddr = IPAddress.Parse(tcprow.RemoteAddress.ToString()).ToString();
                    Log.Info(String.Format("TCP connection: {0}:{1}", remoteAddr, remotePort));

                    if (remotePort != 443)
                    {
                        hsTcpRow.localAddr = tcprow.localAddr;
                        hsTcpRow.localPort = tcprow.localPort;
                        hsTcpRow.remoteAddr = tcprow.remoteAddr;
                        hsTcpRow.remotePort = tcprow.remotePort;
                    }
                }
            }

            return hsTcpRow;
        }
    }
}
