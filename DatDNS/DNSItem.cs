using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatDNS
{
    class DNSItem
    {
        [DisplayName("colName")]
        public string Name { get; set; }

        [DisplayName("Primary DNS")]
        public string PrimaryIP { get; set; }

        [DisplayName("Secondary DNS")]
        public string SecondaryIP { get; set; }
    }
}
