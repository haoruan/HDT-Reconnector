using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace HDT_Reconnector.LogHandler
{
    internal class ConnectionLogHandler
    {
        private readonly Regex GotoGameServer = new Regex(@"Network\.GotoGameServer.*address=[ ]*(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})");
        private readonly Regex ClientConnection = new Regex(@"after the connection has been disconnected");
        public void Handle(string line, ReconnectPanel reconnectPanel)
        {
            var match = GotoGameServer.Match(line);
            if(match.Success)
            {
                reconnectPanel.RemoteAddr = Utils.ConvertFromIpAddressToInteger(match.Groups[1].Value);
                reconnectPanel.RemotePort = UInt16.Parse(match.Groups[2].Value);
            }

            match = ClientConnection.Match(line);
            if(match.Success)
            {
                reconnectPanel.IsGameDisconnected = true;
            }
        }
    }
}
