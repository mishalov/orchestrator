using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class SwarmManagerNode
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Token { get; set; }

        public SwarmManagerNode(string Ip, string Port, string Token)
        {
            this.Ip = Ip;
            this.Port = Port;
            this.Token = Token;
        }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }
}
