using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrator
{
    public static class Client
    {
        public static DockerClient Docker = new DockerClientConfiguration(new System.Uri(@"unix:///var/run/docker.sock")).CreateClient();
        public static HttpClient Http = new HttpClient();
        public static IList<ImagesListResponse> Images = null;
        public static async void Init()
        {
            Images = await Docker.Images.ListImagesAsync(new ImagesListParameters()
            {
                All = true,
            });
            Logger.Log(Images.Count().ToString());
        }
    }
}
