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

        public string ClientUrl { get; set; }

        public int MailBoxPortNumber { get; set; }

        public string MailBoxAddress { get; set; }

        public string MailBoxHost { get; set; }

        public bool MailBoxUseSsl { get; set; }

        public string MailBoxPassword { get; set; }

        


    }
}
