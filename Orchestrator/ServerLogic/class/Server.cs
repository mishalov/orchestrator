using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class Server
    {
        public string Id { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{IP}:{Port}";
        }
    }
}
