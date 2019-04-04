using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Orchestrator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpPost]
        public async Task<JsonResult> AddServerWorker(Server[] swarmNodes)
        {
            try
            {
                await ServerLogic.Init(swarmNodes.ToList());
                await ServerLogic.JoinAllWorkersAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new JsonResult("Выполнилось");
        }
    }
}