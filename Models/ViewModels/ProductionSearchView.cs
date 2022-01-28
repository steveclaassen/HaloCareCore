using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ProductionSearchView
    {

        public Guid dependantID { get; set; }

        [DisplayName("Medical scheme")]
        public string medicalAid { get; set; }

        [DisplayName("Task description")]
        public string taskDescription { get; set; }

        [DisplayName("Created date")]
        public DateTime taskCreatedDate { get; set; }

        [DisplayName("Effective date")]
        public DateTime taskEffectiveDate { get; set; }

        [DisplayName("Closed by")]
        public string closedBy { get; set; }

        [DisplayName("Closed date")]
        public DateTime? taskClosedDate { get; set; }

        [DisplayName("Assignment type")]
        public string assignmentType { get; set; }

        [DisplayName("Item type")]
        public string itemType { get; set; }

        [DisplayName("Assignment ID")]
        public int assignmentID { get; set; }

        [DisplayName("Item ID")]
        public int itemID { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Dependant code")]
        public string dependantCode { get; set; }

        [DisplayName("Status")]
        public bool status { get; set; }

        [DisplayName("Program code")]
        public string programCode { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }

        [DisplayName("Medical scheme ID")]
        public string medAidId { get; set; }

        [DisplayName("ID #")]
        public string idNumber { get; set; }

        [DisplayName("Member surname")]
        public string memberSurname { get; set; }

        [DisplayName("Date from")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateFrom { get; set; }

        [DisplayName("Date to")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateTo { get; set; }

        [DisplayName("Assignment items")]
        public string assignmentItems { get; set; }


        public List<AssignmentItemTaskTypes> assignmentItemTaskTypes { get; set; }

    }
}
