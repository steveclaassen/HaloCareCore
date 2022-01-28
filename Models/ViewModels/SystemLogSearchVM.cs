﻿using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class SystemLogSearchVM
    {
        public EnquirySearch EnquirySearch { get; set; }

        public Log Log { get; set; }
        public List<SystemLogResultsVM> Logs { get; set; }

        public List<Log> Tables { get; set; }
        public string[] SelectedTable { get; set; }

        public List<Log> Columns { get; set; }
        public string[] SelectedColumn { get; set; }

        public List<Log> RecordID { get; set; }
        public string[] SelectedRecordID { get; set; }

        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        public List<QueryTypes> QueryTypes { get; set; }
        public string[] SelectedQueryTypes { get; set; }

        public List<Priorities> QueryPriorities { get; set; }
        public string[] SelectedQueryPriorities { get; set; }

        public List<Validation.ManagementStatus> ManagementStatus { get; set; }
        public string[] SelectedManagementStatus { get; set; }

        public bool filter { get; set; }
    }
}
