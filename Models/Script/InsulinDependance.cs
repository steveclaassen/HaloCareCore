using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Script
{
    public class InsulinDependance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Code")]
        [StringLength(3, ErrorMessage = "Code restricted to 3 characters eg. INS")]
        public string Code { get; set; }

        [Required]
        [DisplayName("Descrption")]
        public string Description { get; set; }

        [DisplayName("T-Class")]
        public string TClass { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}
