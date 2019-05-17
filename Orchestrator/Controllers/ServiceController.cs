using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Orchestrator.Controllers
{
    [ApiController]
    public class ServiceController : ControllerBase
    {

        [HttpPost("api/[controller]")]
        public async Task<JsonResult> CreateService([FromBody] CreateServiceHttpParams attr)
        {
            if (attr.Type == "node")
            {
                Logger.Log("Creating Node Js service");
                Logger.Log(JsonConvert.SerializeObject(attr.Dependencies));
                try
                {
                    ICloudService service = ServiceLogic.CreateCloudService(attr.Id, attr.UserId, attr.FileBase64, attr.CountOfReplicas, attr.Dependencies, attr.Type);
                    bool success = await service.Create();
                    ServiceLogic.Services.Add(service);
                    return new JsonResult(service);
                }
                catch (Exception e)
                {
                    Logger.Fail(e.Message);
                }
            }
            throw new Exception("Unknown type!");
        }

        [HttpPost("api/[controller]/{id}")]
        public async Task<JsonResult> UpdateService(string id, [FromBody] UpdateServiceHttpParams attr)
        {
            Logger.Log("Update service query. Service Id " + id);
            try
            {
                Logger.Log($"Updating service : {id} - to change, but have : {JsonConvert.SerializeObject(ServiceLogic.Services.Select(el => el.Id))}");
                ICloudService service = ServiceLogic.Services.Find(el => el.Id == id);
                Logger.Log($"Found service {service.Id} : {service.DockerServiceId}");
                bool success = await service.Update(attr.FileBase64);
                return new JsonResult(success);
            }
            catch (Exception e)
            {
                Logger.Fail(e.Message);
                return new JsonResult(e.Message);
            }
        }

        [HttpDelete("api/[controller]/{id}")]
        public async Task<JsonResult> DeleteService(string id)
        {
            try
            {
                await ServiceLogic.Services.Find(service => service.Id == id).Remove();
                return new JsonResult(true);
            }
            catch (Exception e)
            {
                return new JsonResult(e.Message);
            }
        }

        [HttpGet("api/[controller]")]
        public async Task<JsonResult> getIdsOfServicesAlive()
        {
            Logger.Log("Get services list...");
            IEnumerable<SwarmService> servicesList = await Client.Docker.Swarm.ListServicesAsync();
            return new JsonResult(servicesList.Select(service => service.ID).ToList());
        }

        private async Task<bool> makeReinitAsync(ReinitServiceHttpParams reinit, List<ReinitServiceHttpParams> attr, List<SwarmService> servicesList)
        {
            SwarmService swarmService = servicesList.Find(service => service.ID == reinit.DockerServiceId);
            Logger.Log($"Reinit service  : {reinit.Id}, {reinit.DockerServiceId}, serviceList : {JsonConvert.SerializeObject(servicesList.Select(el => el.ID))}");
            string dockerName = $"{reinit.UserId}-{reinit.Id}";
            if (swarmService != null)
            {
                ICloudService service = ServiceLogic.CreateCloudService(reinit.Type);
                await service.Reinitialize(reinit, swarmService);
                ServiceLogic.Services.Add(service);

            }
            else
            {
                ICloudService service = ServiceLogic.CreateCloudService(reinit.Id, reinit.UserId, reinit.FileBase64, reinit.CountOfReplicas, reinit.Dependencies, reinit.Type);
                ServiceLogic.Services.Add(service);
                await service.Create();
            }
            return true;
        }

        [HttpGet("api/[controller]/reinit")]
        public async Task<JsonResult> Reinitialize([FromBody] List<ReinitServiceHttpParams> attr)
        {
            try
            {
                List<SwarmService> servicesList = (await Client.Docker.Swarm.ListServicesAsync()).ToList();
                var tasks = attr.Select(reinit => makeReinitAsync(reinit, attr, servicesList)).ToArray();
                Task.WaitAll(tasks);
                return new JsonResult(true);
            }
            catch (Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
    }
}