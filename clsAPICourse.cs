using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILMS
{
    class clsAPICourse
    {
        
        public string ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string DeliveryType { get; set; }
        public string DefaultRequirementType { get; set; }
        public string Credits { get; set; }
        public string Hours { get; set; }
        public string Status { get; set; }
        public DefaultDueDateSettings DDSettings;
        
    }

    class DefaultDueDateSettings
    {
        public string DefaultDueDate { get; set; }
        public string DaysAfterEnrollment { get; set; }
    }


    class curiculla {

        public string ID { get; set; }
        public string Name { get; set; }
        public DefaultDueDateSettings DefaultDueDateSettings;
    }

}
