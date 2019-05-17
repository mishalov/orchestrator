using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class Dependency
    {
        public string name { get; set; }
        public string lang { get; set; }
        public string version { get; set; }
    }

    public class CreateServiceHttpParams
    {
        public string FileBase64 { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
        public int CountOfReplicas { get; set; }
        public List<Dependency> Dependencies = new List<Dependency>();
    }

    public class ReinitServiceHttpParams
    {
        public string FileBase64 { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
        public int CountOfReplicas { get; set; }
        public string DockerServiceId { get; set; }
        public string Port { get; set; }
        public List<Dependency> Dependencies = new List<Dependency>();
    }
}
