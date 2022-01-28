using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class DoctorViewModel
    {
        public Doctors doctor { get; set; }
        public DoctorsInformation doctorsInformation { get; set; }
        public Practices practices { get; set; }
        public Address address { get; set; }
        public Contact contact { get; set; }
        public string depID { get; set; }

        public virtual DoctorTypes doctorTypes{get;set;}

        public List<Dependant> dependants { get; set; }

        public List<Language> Languages { get; set; }
        public List<Sex> Gender { get; set; }

        public Guid DependantID { get; set; }
        public Guid? ProgramID { set; get; }
    }
}