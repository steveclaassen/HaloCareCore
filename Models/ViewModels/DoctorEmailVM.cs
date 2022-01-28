using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class DoctorEmailVM
    {
        public Guid DoctorID { get; set; }
        public string DoctorName { get; set; }
        public string PracticeNumber { get; set; }
        public string PracticeName { get; set; }
        public string EmailAddress { get; set; }
    }
}
