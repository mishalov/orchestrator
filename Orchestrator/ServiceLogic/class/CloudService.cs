using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public class CloudService
    {
        public CloudService() { }
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CountOfContainers { get; set; }
        string _type = "";
        public string Type
        {
            get
            {
                return this._type;
            }
            set
            {
                if (value != "nodejs" || value != "python")
                {
                    throw new Exception("This type of service is not supported!");
                }
            }
        }
        public string Base64File { get; set; }
    }
}
