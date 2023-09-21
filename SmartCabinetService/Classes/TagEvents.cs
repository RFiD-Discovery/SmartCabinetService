using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCabinetService.Classes
{
    public class TagEvents
    {
        public string readerName { get; set; }
        public string mac { get; set; }
        public List<TagRead> tag_reads { get; set; }

        public class TagRead
        {
            public string epc { get; set; }
            public object firstSeenTimestamp { get; set; }
            public int antennaPort { get; set; }
            public string antennaZone { get; set; }
            public double peakRssi { get; set; }
            public double txPower { get; set; }
        }
    }
}
