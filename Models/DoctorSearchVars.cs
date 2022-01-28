using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class DoctorSearchVars
    {
        [DisplayName("Dependant ID")]
        public string dependentID { get; set; }

        [DisplayName("First name")]
        public string FirstName { get; set; }

        [DisplayName("Last name")]
        public string LastName { get; set; }

        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [DisplayName("Practice name")]
        public string practiceName { get; set; }
    }
}