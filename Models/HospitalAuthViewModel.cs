using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class HospitalAuthViewModel
    {
        public HospitalizationAuths hospitalizationAuth { get; set; }

        public Guid dependantID { get; set; }
    }
}
