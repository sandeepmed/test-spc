using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILMS
{
    class clsRegionApi
    {
        public string name { get; set; }
    }

    class GetRegion
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string NumberOfUsers { get; set; }
    }

    class clsDepartmentApi
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string NumberOfUsers { get; set; }
    }
}
