using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class DoctorInformation
    {
        [Key]
        public Guid DoctorInformationID { get; set; }
        public Guid doctorID { get; set; }
        public Guid contactID { get; set; }
        public Guid addressID { get; set; }
    }
}