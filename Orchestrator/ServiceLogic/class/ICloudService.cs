using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public interface ICloudService
    {
        string Id { get; set; }
        string UserId { get; set; }
        int CountOfReplicas { get; set; }
        string Base64File { get; set; }
        string Port { get; set; }
        string DockerServiceId { get; set; } // Выдает докер
        //public abstract int Version { get; set; } // Версия сервиса, нужна для Update
        NetworkResponse Network { get; set; }
        List<Dependency> Dependencies { get; set; }
        Task<bool> Create();
        //public async Task<bool> Start();
        Task<bool> Remove();
        Task<bool> Update(string FileBase64);
        Task<bool> Reinitialize(ReinitServiceHttpParams reinitParams, SwarmService swarmService);
    }
}
