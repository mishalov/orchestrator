using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Orchestrator
{
    public static class ServerLogic
    {
        public static List<Server> Servers = new List<Server>();

        public static SwarmInspectResponse SwarmManagerInfo = null;

        public static async Task JoinAllWorkersAsync()
        {
            var a = await Client.Docker.Swarm.InspectSwarmAsync();
            if (Servers.Count == 0) throw new Exception("Have no registred servers to join! this.Servers.length == 0");
            if (SwarmManagerInfo == null) throw new Exception("Server Logic is not initialized! Use ServerLogic.Init()");
            foreach (Server _s in Servers)
            {
                SwarmManagerNode swarmInfo = new SwarmManagerNode("192.168.56.102", "2377", SwarmManagerInfo.JoinTokens.Worker);
                try
                {
                    string url = $"{ _s.ToString()}/api/server";
                    string body = JsonConvert.SerializeObject(swarmInfo);
                    Logger.Log($"Requesting to {url}...");
                    Logger.Log(body);
                    StringContent httpContent = new StringContent(body, Encoding.UTF8, "application/json");
                    var answer = await Client.Http.PostAsync(url, httpContent);
                    Logger.Success(JsonConvert.SerializeObject(answer));
                }
                catch (HttpRequestException e)
                {
                    Logger.Fail($"{e.Message} : {e.InnerException.Message}");
                }
            }
        }

        public static async Task Init(List<Server> _Servers)
        {
            SwarmManagerInfo = await Client.Docker.Swarm.InspectSwarmAsync();
            Servers = _Servers;
        }
    }
}
