using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class AssignmentSearch
    {
        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Option")]
        public string MedicalOption { get; set; }

        [DisplayName("Member number")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("ID/Auth #")]
        public string IDNumber { get; set; }

        [DisplayName("Patient name")]
        public string PatientName { get; set; }

        [DisplayName("Program code")]
        public string ProgramCode { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Patient status")]
        public string ManagementStatus { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("Item code")]
        public string AssignmentItemCode { get; set; }

        [DisplayName("Assignment type")]
        public string AssignmentType { get; set; }

        [DisplayName("Item type")]
        public string AssignmentItemType { get; set; }
        
        [DisplayName("Tasks closed")]
        public int AssignmentTasksClosed { get; set; }
        public int AssignmentTasksCount { get; set; }

        [DisplayName("Assignment ID")]
        public int AssignmentID { get; set; }

        [DisplayName("Assignment status")]
        public string AssignmentStatus{ get; set; }

        [DisplayName("Assignment age")]
        public string AssignmentAge { get; set; }

        [DisplayName("Elapsed time")]
        public string ElapsedTime { get; set; }
        public string AssignmentItemProgram { get; set; }

        //--
        public Guid DependantID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid AssignmentProgramID { get; set; }

        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        public bool Active { get; set; }

    }
}
