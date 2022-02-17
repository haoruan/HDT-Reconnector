using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

namespace HDT_Reconnect
{
    public class Reconnect
    {
        private const uint connectionPort = 3724;
        private Iphlpapi.MIB_TCPROW_OWNER_PID hsTcpRow;
        public Reconnect()
        {
            List<Iphlpapi.MIB_TCPROW_OWNER_PID> tcprows = Iphlpapi.GetAllTCPConnections();
            foreach (Iphlpapi.MIB_TCPROW_OWNER_PID tcprow in tcprows)
            {
                if (tcprow.ProcessId == 0 && tcprow.RemotePort == connectionPort)
                {
                    hsTcpRow = 
                }
            }
        }

        public void Disconnect()
        {

        }

        public bool IsReconnected()
        {
            return false;
        }
    }
}
