using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public CONNECTION_STATUS Status { get; set; } = CONNECTION_STATUS.CONNECTED;

        public enum CONNECTION_STATUS
        {
            DISCONNECTED,
            CONNECTED
        }

        public void ResumeConnect(Timer timer)
        {
            timer.Change(Timeout.Infinite, 0);
            Status = CONNECTION_STATUS.CONNECTED;
            Log.Info("Reconnecting...");
        }

        public int Disconnect(string addr, ushort port)
        {
            Iphlpapi.MIB_TCPROW hsTcpRow = GetReconnectTcp(addr, port);

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
                    break;
                case Iphlpapi.SetTcpErrorCode.ERROR_INVALID_PARAMETER:
                    Log.Error("Invalid parameter");
                    break;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_ELEVATED:
                    Log.Error("Not elevated");
                    break;
                case Iphlpapi.SetTcpErrorCode.ERROR_NOT_SUPPORTED:
                    Log.Error("Not supported");
                    break;
                default:
                    Log.Error("Other errors");
                    break;
            }
            return 1;
        }

        private Process[] GetProcess(string name = null)
        {
            if (name != null)
            {
                return Process.GetProcessesByName(name);
            }

            return Process.GetProcesses();
        }

        private (Iphlpapi.MIB_TCPROW, int) GetHsTcpConnection(List<Iphlpapi.MIB_TCPROW_OWNER_MODULE> tcprows, Process[] processes, string addr, ushort port)
        {
            Dictionary<uint, string> pidToName = new Dictionary<uint, string>();
            Iphlpapi.MIB_TCPROW tcpRow = new Iphlpapi.MIB_TCPROW();

            foreach (Process process in processes)
            {
                pidToName.Add((uint)process.Id, process.ProcessName);
            }

            for (int i = tcprows.Count - 1; i >= 0; i--)
            {
                if (pidToName.ContainsKey(tcprows[i].ProcessId))
                {
                    int remotePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(tcprows[i].remotePort, 0));
                    string remoteAddr = IPAddress.Parse(tcprows[i].RemoteAddress.ToString()).ToString();

                    if (remotePort == port && remoteAddr == addr)
                    {
                        Log.Info(String.Format("Found TCP connection in {0}: {1}:{2}", pidToName[tcprows[i].ProcessId], remoteAddr, remotePort));

                        tcpRow.localAddr = tcprows[i].localAddr;
                        tcpRow.localPort = tcprows[i].localPort;
                        tcpRow.remoteAddr = tcprows[i].remoteAddr;
                        tcpRow.remotePort = tcprows[i].remotePort;

                        return (tcpRow, 0);
                    }
                }
            }

            return (tcpRow, 1);

        }

        private Iphlpapi.MIB_TCPROW GetReconnectTcp(string addr, ushort port)
        {
            List<Iphlpapi.MIB_TCPROW_OWNER_MODULE> tcprows = Iphlpapi.GetAllTCPConnections();
            Process[] hsProcesses = GetProcess(hsName);
            (Iphlpapi.MIB_TCPROW hsTcpRow, int err) = GetHsTcpConnection(tcprows, hsProcesses, addr, port);
            if (err != 0)
            {
                // Try to close the last hearthstone TCP connection.
                // In some cases (like restart hearthstone while in the game), this assumption may be failed,
                // so users need to click reconnect button again.
                DateTime lastCreateTimestamp = new DateTime(0);
                HashSet<uint> pids = new HashSet<uint>();

                foreach (Process process in hsProcesses)
                {
                    pids.Add((uint)process.Id);
                }

                for (int i = tcprows.Count - 1; i >= 0; i--)
                {
                    if (pids.Contains(tcprows[i].ProcessId))
                    {
                        if (tcprows[i].CreateTimestamp >= lastCreateTimestamp && tcprows[i].State == Iphlpapi.MIB_TCP_STATE.MIB_TCP_STATE_ESTAB)
                        {
                            hsTcpRow.localAddr = tcprows[i].localAddr;
                            hsTcpRow.localPort = tcprows[i].localPort;
                            hsTcpRow.remoteAddr = tcprows[i].remoteAddr;
                            hsTcpRow.remotePort = tcprows[i].remotePort;
                            lastCreateTimestamp = tcprows[i].CreateTimestamp;
                        }
                    }
                }

                Log.Info("Try last TCP connection in Hearthstone");
            }

            return hsTcpRow;
        }
    }
}
