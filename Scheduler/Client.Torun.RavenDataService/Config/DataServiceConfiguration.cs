using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Config
{
    public class DataServiceConfiguration
    {
        public int PopulationSize { get; set; }

        public string[] Urls { get; set; }

        public string Database { get; set; }

        
    }
}
