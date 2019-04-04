using Docker.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class Client
    {
        public static DockerClient Docker = new DockerClientConfiguration(new System.Uri(@"unix:///var/run/docker.sock")).CreateClient();
        public static HttpClient Http = new HttpClient();
    }
}
