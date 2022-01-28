using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class PatientDisclaimer
    {
        [Key]
        public int PatientDisclaimerId { get; set; }

        [DisplayName("Dependant ID")]
        public Guid DependentID { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Signed acknowledgement")]
        public bool SignedAcknowledgement { get; set; }

        [DisplayName("Telephonic acknowledgement")]
        public bool TelephonicAcknowledgement { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}
