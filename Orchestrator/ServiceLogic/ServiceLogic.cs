using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public static class ServiceLogic
    {
        public static List<ICloudService> Services = new List<ICloudService>();

        public static ICloudService CreateCloudService(string Id, string UserId, string FileBase64, int countOfReplicas, List<Dependency> dependencies, string type)
        {
            switch (type)
            {
                case "node": return new NodeJsService(Id, UserId, FileBase64, countOfReplicas, dependencies);
                default: return new NodeJsService(Id, UserId, FileBase64, countOfReplicas, dependencies);
            }
        }

        public static ICloudService CreateCloudService(string type)
        {
            switch (type)
            {
                case "node": return new NodeJsService();
                default: return new NodeJsService();
            }
        }
    }
}
