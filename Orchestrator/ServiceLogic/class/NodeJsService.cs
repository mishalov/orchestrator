using Docker.DotNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class NodeJsService : ICloudService
    {
        public string DockerServiceId { get; set; } // Выдает докер
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Base64File { get; set; }
        public int CountOfReplicas { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public string FilePath { get; set; }
        public string Port { get; set; }
        public NetworkResponse Network { get; set; }

        public string Name
        {
            get
            {
                return $"{UserId}-{Id}";
            }
        }

        public ImagesListResponse Image()
        {
            return Client.Images.Where(image => image.RepoTags.IndexOf("node:8") != -1).First();
        }

        private string CombineProgram(string Id, string UserId, string FileBase64)
        {
            byte[] data = Convert.FromBase64String(FileBase64);
            string decodedString = Encoding.UTF8.GetString(data);
            string programDirectory = FileWorker.makeDirectoryName(Id, UserId);
            if (Directory.Exists(programDirectory))
            {
                Directory.Delete(programDirectory, true);
            }
            Directory.CreateDirectory(programDirectory);
            FileWorker.Copy("/home/orchestrator/orchestrator/ServerTemplates/NodeJs", programDirectory);
            System.IO.File.WriteAllText($"{programDirectory}/program.js", decodedString);
            if (!Directory.Exists(programDirectory))
            {
                throw new Exception($"File doesnt exists! UserId : {UserId}, FilePath : {FilePath} ");
            }
            string initialBashScript = File.ReadAllText($"{programDirectory}/build.sh");
            Logger.Log($"Old bash script: {initialBashScript}");
            this.Dependencies.ForEach(el =>
            {
                Logger.Log($"serializing: {JsonConvert.SerializeObject(el)}");
                initialBashScript = $"npm install {el.name}@{el.version} && " + initialBashScript;
            });
            File.WriteAllText($"{programDirectory}/build.sh", initialBashScript);
            Logger.Log($"New bash script: {initialBashScript}");
            return programDirectory;
        }

        public NodeJsService(string Id, string UserId, string FileBase64, int countOfReplicas, List<Dependency> dependencies)
        {
            if (UserId == "" || FileBase64 == "" || Id == "")
            {
                throw new Exception($"One of the initial fields is empty! UserId : {UserId}, Id : {Id}, also check FileBase64 ");
            }
            this.CountOfReplicas = countOfReplicas;
            this.Dependencies = dependencies;
            this.FilePath = CombineProgram(Id, UserId, FileBase64);
            this.UserId = UserId;
            this.Id = Id;
        }

        public NodeJsService()
        {

        }

        public async Task<bool> Create()
        {
            var networkParams = new NetworksCreateParameters()
            {
                Name = Name,
                Driver = "overlay"
            };
            var networkList = await Client.Docker.Networks.ListNetworksAsync();
            var existingNetwork = networkList.Where(el => el.Name == Name).ToList();
            NetworkResponse network = null;
            if (existingNetwork.Count == 0)
            {
                Logger.Log($"Network for service {Name} is existing!");
                var createdNetwork = await Client.Docker.Networks.CreateNetworkAsync(networkParams);
                network = await Client.Docker.Networks.InspectNetworkAsync(createdNetwork.ID);
            }
            else
            {
                network = existingNetwork.First();
            }

            Network = network;

            Logger.Success($"Use network with Name : {network.Name}, count of replicas {CountOfReplicas}");
            NetworkAttachmentConfig networkConfig = new NetworkAttachmentConfig()
            {
                Target = network.ID
            };



            Port = (3000 + ServiceLogic.Services.Count).ToString();
            Logger.Success($"Predicted port is {Port}");
            var serviceParams = new ServiceCreateParameters()
            {
                Service = new ServiceSpec()
                {
                    Name = Name,
                    Networks = new List<NetworkAttachmentConfig>() { networkConfig },
                    EndpointSpec = new EndpointSpec()
                    {
                        Ports = new List<PortConfig>()
                        {
                            new PortConfig()
                            {
                                Protocol="tcp",
                                PublishedPort = uint.Parse(Port),
                                TargetPort = uint.Parse(Port)
                            }
                        }
                    },
                    Mode = new ServiceMode()
                    {
                        Replicated = new ReplicatedService()
                        {
                            Replicas = (ulong)(CountOfReplicas)
                        }
                    },
                    TaskTemplate = new TaskSpec()
                    {
                        ContainerSpec = new ContainerSpec()
                        {
                            Image = Image().ID,
                            Env = new List<string>() { $"PORT={Port}" },
                            Hosts = new List<string>() { $"{Port}:${Port}" },
                            Mounts = new List<Mount>
                            {
                                new Mount()
                                {
                                    Target="/home/node/",
                                    Source = $"{this.FilePath}",
                                }
                            },
                            Dir = "/home/node/",
                            Command = new List<string>() { "/bin/bash", "build.sh" },
                        },
                    }
                }
            };
            try
            {
                var service = await Client.Docker.Swarm.CreateServiceAsync(serviceParams);
                DockerServiceId = service.ID;
                Logger.Success($"Service {service.ID} is Up!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Fail($"Cant create the container: {e.Message}");
                return false;
            }
        }

        public bool Start()
        {
            if (Id == "")
            {
                throw new Exception("Cant start container: ID is Empty");
            }
            try
            {
                Logger.Success($"Container is UP!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Fail($"Container could not get run! {e.Message}");
                return false;
            }
        }

        public async Task<bool> Remove()
        {
            try
            {
                await Client.Docker.Swarm.RemoveServiceAsync(DockerServiceId);
                await Client.Docker.Networks.DeleteNetworkAsync(Network.ID);
                Logger.Success($"Service {DockerServiceId} was successfully removed");
                return true;

            }
            catch (Exception e)
            {
                Logger.Fail($"Cant remove service {DockerServiceId} : {e.Message}");
                return false;
            }
        }
        public async Task<bool> Update(string FileBase64)
        {
            try
            {
                Logger.Log($"updating service id : {Id} with DockerId {DockerServiceId}");
                this.FilePath = CombineProgram(Id, UserId, FileBase64);
                var serviceInfo = await Client.Docker.Swarm.InspectServiceAsync(DockerServiceId);
                var Image = this.Image();
                Logger.Log($"Service info gotten! Name is : {Name}, Version is: {serviceInfo.Version.Index}, Network ID is {Network.ID}, Image Id : {Image.ID}, Port: {Port}, this.FilePath: {this.FilePath}");
                NetworkAttachmentConfig networkConfig = new NetworkAttachmentConfig()
                {
                    Target = Network.ID
                };
                EndpointSpec endpointSpec = new EndpointSpec()
                {
                    Ports = new List<PortConfig>()
                            {
                                new PortConfig()
                                {
                                    Protocol="tcp",
                                    PublishedPort = uint.Parse(Port),
                                    TargetPort = uint.Parse(Port)
                                }
                            }
                };
                Logger.Log("Endpoint created");
                IList<Mount> mounts = new List<Mount>
                                {
                                    new Mount()
                                    {
                                        Target="/home/node/",
                                        Source = this.FilePath,
                                    }
                                };
                Logger.Log("Mounts created");
                TaskSpec taskSpec = new TaskSpec()
                {
                    ForceUpdate = 1,
                    RestartPolicy = new SwarmRestartPolicy()
                    {
                        Condition = "any",
                        MaxAttempts = 0
                    },
                    ContainerSpec = new ContainerSpec()
                    {
                        Image = Image.ID,
                        Env = new List<string>() { $"PORT={Port}" },
                        Hosts = new List<string>() { $"{Port}:{Port}" },
                        Mounts = mounts,
                        Dir = "/home/node/",
                        Command = new List<string>() { "/bin/bash", "build.sh" },
                    },
                };
                Logger.Log("Task spec created!");
                ServiceSpec serviceSpec = new ServiceSpec()
                {
                    Name = Name,
                    Networks = new List<NetworkAttachmentConfig>() { networkConfig },
                    EndpointSpec = endpointSpec,
                    TaskTemplate = taskSpec,
                };
                Logger.Log("Service spec created!");
                var serviceParams = new ServiceUpdateParameters()
                {
                    Version = Convert.ToInt64(serviceInfo.Version.Index),
                    Service = serviceSpec
                };
                Logger.Log("Configuration is ready, updating...");
                await Client.Docker.Swarm.UpdateServiceAsync(DockerServiceId, serviceParams);
                Logger.Success($"Updated successful!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Fail($"Cant update service {DockerServiceId} : {e.Message}");
                return false;
            }
        }

        public async Task<bool> Reinitialize(ReinitServiceHttpParams reinitParams, SwarmService swarmService)
        {
            try
            {
                Logger.Log("Start server reinitializing");
                this.DockerServiceId = swarmService.ID;
                Id = reinitParams.Id;
                UserId = reinitParams.UserId;
                Base64File = reinitParams.FileBase64;
                CountOfReplicas = reinitParams.CountOfReplicas;
                DockerServiceId = swarmService.ID;
                Dependencies = reinitParams.Dependencies;
                FilePath = FileWorker.makeDirectoryName(Id, UserId);
                Port = reinitParams.Port;
                Logger.Log($"{reinitParams.UserId}-{reinitParams.Id} ({DockerServiceId}): Main parameters filled successfull");
                NetworkAttachmentConfig networkAttachmentConfig = swarmService.Spec.Networks.FirstOrDefault();
                Network = await Client.Docker.Networks.InspectNetworkAsync(networkAttachmentConfig.Target);
                Logger.Log($"{reinitParams.UserId}-{reinitParams.Id}: Network created successfull");
                return true;
            }
            catch (Exception e)
            {
                Logger.Fail($"{reinitParams.UserId}-{reinitParams.Id}: {e.Message}");
                return false;
            }

        }
    }
}
