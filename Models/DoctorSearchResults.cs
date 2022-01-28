using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class DoctorSearchResults
    {
        public string DependentID { get; set; }
        public List<Doctors> doctorResults { get; set; }
    }
}