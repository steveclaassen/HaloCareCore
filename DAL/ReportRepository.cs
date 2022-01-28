using HaloCareCore.Models.Reports;
using HaloCareCore.Models.Service;
using HaloCareCore.Repos;
using HaloCareCore.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.DAL
{
    public class ReportRepository : IReportRepository
    {
        private OH17Context _context;
        private PasswordManager _pwManager = new PasswordManager();

        public ReportRepository(OH17Context context)
        {
            this._context = context;
        }

        public int GetActivePatientCount(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == "FIT" || x.managementStatusCode == "ACT" || x.managementStatusCode == "REE" || x.managementStatusCode == "PRH" ||
            x.managementStatusCode == "FAS" || x.managementStatusCode == "ENR" || x.managementStatusCode == "FNA" || x.managementStatusCode == "FIO" || x.managementStatusCode == "FOP" ||
            x.managementStatusCode == "FOS" || x.managementStatusCode == "MTC" || x.managementStatusCode == "MT1" || x.managementStatusCode == "MTN" || x.managementStatusCode == "PEP" || 
            x.managementStatusCode == "PRA" || x.managementStatusCode == "PRC")

                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    member = o.dependantId
                })
                .Select(g => new ACTPatients()
                {
                    statusCountACT = g.Count().ToString(),
                    reportMonth = DateTime.Now.Month.ToString(),
                    reportYear = DateTime.Now.Year.ToString(),
                })
                   .ToList().Count();
            return grouped;
        }

        public List<ACTPatients> GetPatientCountByStatus(string schemeCode, string status)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == status)
                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    dep = o.dependantId
                })
                .Select(g => new ACTPatients()
                {
                    statusCountACT = g.Count().ToString()
                })
                   .ToList();
            return grouped;
        }
        public List<ACTPatients> GetMTCPHRPatientCount(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == "MTC" || x.managementStatusCode == "PRH")
                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    dep = o.dependantId
                })
                .Select(g => new ACTPatients()
                {
                    statusCountACT = g.Count().ToString(),
                })
                   .ToList();
            return grouped;
        }

        public List<ACTPatients> GetARTPatientCount(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == "MTC" || x.managementStatusCode == "PRH" || x.managementStatusCode == "ACT" || x.managementStatusCode == "REE" ||
            x.managementStatusCode == "FAS" || x.managementStatusCode == "MT1" || x.managementStatusCode == "PEP" || x.managementStatusCode == "PRA" || x.managementStatusCode == "PRC")
                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    dep = o.dependantId
                })
                .Select(g => new ACTPatients()
                {
                    statusCountACT = g.Count().ToString()
                })
                   .ToList();
            return grouped;
        }

        public List<ACTPatients> GetFOIPatientCount(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == "FIO" || x.managementStatusCode == "FNA" || x.managementStatusCode == "FOP"
            || x.managementStatusCode == "FOS")
                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    dep = o.dependantId
                })
                .Select(g => new ACTPatients()
                {
                    statusCountACT = g.Count().ToString()
                })
                   .ToList();
            return grouped;
        }

        public List<ACTGENPatients> GetActiveGenderPatientCount(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group new { st, d } by st.dependantId
                           into groups
                        select groups.OrderByDescending(x => x.st.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.st.managementStatusCode == "FIT" || x.st.managementStatusCode == "ACT" || x.st.managementStatusCode == "REE" || x.st.managementStatusCode == "PRH" ||
            x.st.managementStatusCode == "FAS" || x.st.managementStatusCode == "ENR" || x.st.managementStatusCode == "FNA" || x.st.managementStatusCode == "FIO" || x.st.managementStatusCode == "FOP" ||
            x.st.managementStatusCode == "FOS" || x.st.managementStatusCode == "MTC" || x.st.managementStatusCode == "MT1" || x.st.managementStatusCode == "MTN" || x.st.managementStatusCode == "PEP" ||
            x.st.managementStatusCode == "PRA" || x.st.managementStatusCode == "PRC")
                .Where(x => x.st.active == true)
                .GroupBy(o => new
                {
                    Gender = o.d.sex
                })
                .Select(g => new ACTGENPatients()
                {
                    actpatients = new ACTPatients()
                    {
                        statusCountACT = g.Count().ToString(),
                    },
                    statusCountACTGender = g.Key.Gender,

                })
                   .ToList();

            return grouped;
        }

        public List<AVGPathCount> AVGCD4Count(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group new { st, d } by st.dependentID
                           into groups
                        select groups.OrderByDescending(x => x.st.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.st.CD4Count != 0.00m)
                .Where(x => x.st.active == true)
                .Where(x => x.st.createdDate > DateTime.Now.AddMonths(-1))
                .GroupBy(o => new
                {
                    Month = o.st.createdDate.Month,
                    Year = o.st.createdDate.Year,
                })
                .Select(g => new AVGPathCount()
                {
                    actpatients = new ACTPatients()
                    {
                        reportMonth = g.Key.Month.ToString(),
                        reportYear = g.Key.Year.ToString(),
                    },
                    avgPathCount = g.Average(x => x.st.CD4Count).ToString(),

                })
                   .OrderByDescending(a => a.actpatients.reportYear)
                   .ThenBy(a => a.actpatients.reportMonth)
                   .ToList();
            return grouped;
        }

        public List<AVGPathCount> AVGCD4Perc(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group new { st, d } by st.dependentID
                           into groups
                        select groups.OrderByDescending(x => x.st.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.st.CD4Percentage != 0.00m)
                .Where(x => x.st.active == true)
                .Where(x => x.st.createdDate > DateTime.Now.AddMonths(-1))
                .GroupBy(o => new
                {
                    Month = o.st.createdDate.Month,
                    Year = o.st.createdDate.Year,
                })
                .Select(g => new AVGPathCount()
                {
                    actpatients = new ACTPatients()
                    {
                        reportMonth = g.Key.Month.ToString(),
                        reportYear = g.Key.Year.ToString(),
                    },
                    avgPathCount = g.Average(x => x.st.CD4Percentage).ToString(),

                })
                   .OrderByDescending(a => a.actpatients.reportYear)
                   .ThenBy(a => a.actpatients.reportMonth)
                   .ToList();
            return grouped;
        }

        public List<AVGPathCount> AVgViralLoadlt1000Count(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where d.createdDate < DateTime.Now.AddMonths(-6)
                        group new { st, d } by st.dependentID
                           into groups
                        select groups.OrderByDescending(x => x.st.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.st.viralLoad < 1000.00m && x.st.viralLoad != 0)
                .Where(x => x.st.active == true)
                .Where(x => x.st.createdDate > DateTime.Now.AddMonths(-1))
                .GroupBy(o => new
                {
                    Month = o.st.createdDate.Month,
                    Year = o.st.createdDate.Year,
                })
                .Select(g => new AVGPathCount()
                {
                    actpatients = new ACTPatients()
                    {
                        reportMonth = g.Key.Month.ToString(),
                        reportYear = g.Key.Year.ToString(),
                    },
                    avgPathCount = g.Average(x => x.st.viralLoad).ToString(),

                })
                   .OrderByDescending(a => a.actpatients.reportYear)
                   .ThenBy(a => a.actpatients.reportMonth)
                   .ToList();
            return grouped;
        }

        public List<AVGPathCount> AVgViralLoadgt1000Count(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where d.createdDate < DateTime.Now.AddMonths(-6)
                        group new { st, d } by st.dependentID
                           into groups
                        select groups.OrderByDescending(x => x.st.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.st.viralLoad > 1000.00m)
                .Where(x => x.st.active == true)
                .Where(x => x.st.createdDate > DateTime.Now.AddMonths(-1))
                .GroupBy(o => new
                {
                    Month = o.st.createdDate.Month,
                    Year = o.st.createdDate.Year,
                })
                .Select(g => new AVGPathCount()
                {
                    actpatients = new ACTPatients()
                    {
                        reportMonth = g.Key.Month.ToString(),
                        reportYear = g.Key.Year.ToString(),
                    },
                    avgPathCount = g.Average(x => x.st.viralLoad).ToString(),

                })
                   .OrderByDescending(a => a.actpatients.reportYear)
                   .ThenBy(a => a.actpatients.reportMonth)
                   .ToList();
            return grouped;
        }

        public RiskReportViewModel GetRiskVRViewCount(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == "SELFMED"
                        group new { st, d } by st.dependentID
                           into groups
                        let MaxPathDate = groups.Max(g => g.st.createdDate)
                        from grp in groups
                        where grp.st.createdDate == MaxPathDate
                        select groups.OrderByDescending(x => x.st.dependentID).FirstOrDefault();

            var grouped = new RiskReportViewModel()
            {
                zeroto50 = model.Where(x => x.st.viralLoad > 0 && x.st.viralLoad < 50).Count().ToString(),
                fifty1to200 = model.Where(x => x.st.viralLoad > 51 && x.st.viralLoad < 200).Count().ToString(),
                two01to350 = model.Where(x => x.st.viralLoad > 201 && x.st.viralLoad < 350).Count().ToString(),
                three51to500 = model.Where(x => x.st.viralLoad > 351 && x.st.viralLoad < 500).Count().ToString(),
                five01plus = model.Where(x => x.st.viralLoad > 500).Count().ToString()
            };
            return grouped;
        }

        public RiskReportViewModel GetRiskCDViewCount(string schemeCode)
        {
            var model = from st in _context.Pathology
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == "SELFMED"
                        group new { st, d } by st.dependentID
                           into groups
                        let MaxPathDate = groups.Max(g => g.st.createdDate)
                        from grp in groups
                        where grp.st.createdDate == MaxPathDate
                        select groups.OrderByDescending(x => x.st.dependentID).FirstOrDefault();

            var grouped = new RiskReportViewModel()
            {
                zeroto50 = model.Where(x => x.st.CD4Count > 0 && x.st.CD4Count < 50).Count().ToString(),
                fifty1to200 = model.Where(x => x.st.CD4Count > 51 && x.st.CD4Count < 200).Count().ToString(),
                two01to350 = model.Where(x => x.st.CD4Count > 201 && x.st.CD4Count < 350).Count().ToString(),
                three51to500 = model.Where(x => x.st.CD4Count > 351 && x.st.CD4Count < 500).Count().ToString(),
                five01plus = model.Where(x => x.st.CD4Count > 500).Count().ToString()
            };
            return grouped;
        }

        public List<AVGPathCount> NewEnrolments(string schemeCode)
        {
            var model = from st in _context.Dependants
                        join m in _context.Members
                        on st.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.createdDate
                           into groups
                        select groups.OrderByDescending(x => x.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.createdDate > DateTime.Now.AddMonths(-1))
                .GroupBy(o => new
                {
                    Month = o.createdDate.Month,
                    Year = o.createdDate.Year,
                })
                .Select(g => new AVGPathCount()
                {
                    actpatients = new ACTPatients()
                    {
                        reportMonth = g.Key.Month.ToString(),
                        reportYear = g.Key.Year.ToString(),
                    },
                    avgPathCount = g.Count().ToString(),

                })
                   .OrderByDescending(a => a.actpatients.reportYear)
                   .ThenBy(a => a.actpatients.reportMonth)
                   .ToList();
            return grouped;
        }

        public operationalReportViewModel OpsNewEnrolments(string schemeCode)
        {
            var now = DateTime.Now;
            var month = new DateTime(now.Year, now.Month, 1);
            var last = month.AddDays(-1).AddHours(12);
            var result = (from st in _context.Dependants
                          join m in _context.Members
                          on st.memberID equals m.memberID
                          join me in _context.MedicalAids
                          on m.medicalAidID equals me.MedicalAidID
                          where me.medicalAidCode == schemeCode
                          where st.createdDate > last
                          select st.DependantID).Count();

            return new operationalReportViewModel() { newEnrolmentCount = result};
        }

        public List<operationalReportViewModel> OpsPatientStatusReport(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                            into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.active == true)
                .GroupBy(o => new
                {
                    Status = o.managementStatusCode,
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status,
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();

            return grouped;
        }

        public List<operationalReportViewModel> OpsPatientComsReport(string schemeCode)
        {
            var model = from st in _context.Notes
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where st.createdDate.Month == DateTime.Now.Month
                        where st.createdDate.Year == DateTime.Now.Year
                        group st by st.dependentID
                            into groups
                        select groups.OrderByDescending(x => x.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.Active == true)
                .GroupBy(o => new
                {
                    Status = o.noteType,
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status,
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();

            return grouped;
        }

        public List<operationalReportViewModel> OpsPatientEnquiriesReport(string schemeCode)
        {
            var model = from st in _context.Queries
                        join d in _context.Dependants
                        on st.dependentID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where st.createdDate.Month == DateTime.Now.Month
                        where st.createdDate.Year == DateTime.Now.Year
                        group st by st.dependentID
                            into groups
                        select groups.OrderByDescending(x => x.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.Active == true)
                .GroupBy(o => new
                {
                    Status = o.queryType,
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status,
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();

            return grouped;
        }

        public List<operationalReportViewModel> OpsPatientSmsReport(string schemeCode)
        {
            var model = from st in _context.SmsMessages
                        join d in _context.Dependants
                        on st.dependantID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where st.createdDate.Month == DateTime.Now.Month
                        where st.createdDate.Year == DateTime.Now.Year
                        group st by st.dependantID
                            into groups
                        select groups.OrderByDescending(x => x.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.Active == true)
                .GroupBy(o => new
                {
                    Status = o.templateID,
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status.ToString(),
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();

            return grouped;
        }

        public List<operationalReportViewModel> OpsPatientSmsStatusReport(string schemeCode)
        {
            var model = from st in _context.SmsMessages
                        join d in _context.Dependants
                        on st.dependantID equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        where st.createdDate.Month == DateTime.Now.Month
                        where st.createdDate.Year == DateTime.Now.Year
                        group st by st.dependantID
                            into groups
                        select groups.OrderByDescending(x => x.createdDate).FirstOrDefault();

            var grouped = model.Where(x => x.Active == true)
                .GroupBy(o => new
                {
                    Status = o.status,
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status,
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();

            return grouped;
        }

        public List<operationalReportViewModel> FinPatientStatusReport(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group st by st.dependantId
                                       into groups
                        select groups.OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            var grouped = model.Where(x => x.managementStatusCode == "FIT" || x.managementStatusCode == "ACT" || x.managementStatusCode == "REE" || x.managementStatusCode == "PRH" ||
            x.managementStatusCode == "FAS" || x.managementStatusCode == "ENR" || x.managementStatusCode == "FNA" || x.managementStatusCode == "FIO" || x.managementStatusCode == "FOP" ||
            x.managementStatusCode == "FOS" || x.managementStatusCode == "MTC" || x.managementStatusCode == "MT1" || x.managementStatusCode == "MTN" || x.managementStatusCode == "PEP" ||
            x.managementStatusCode == "PRA" || x.managementStatusCode == "PRC")
                .Where(x => x.active == true)
                .GroupBy(o => new
                {
                    Status = _context.ManagementStatus.Where(x => x.statusCode == o.managementStatusCode).Select(x => x.statusName).FirstOrDefault(),
                })
                .Select(g => new operationalReportViewModel()
                {
                    strName = g.Key.Status,
                    strCount = g.Count(),
                })
                   .OrderByDescending(a => a.strName)
                   .ToList();


            return grouped;
        }

        public List<financialReport> FinancialStatusList(string schemeCode)
        {
            var model = from st in _context.PatientManagementStatusHistory
                        join d in _context.Dependants
                        on st.dependantId equals d.DependantID
                        join m in _context.Members
                        on d.memberID equals m.memberID
                        join me in _context.MedicalAids
                        on m.medicalAidID equals me.MedicalAidID
                        where me.medicalAidCode == schemeCode
                        group new { st, d, m, me } by st.dependantId
                        into groups
                        let MaxPathDate = groups.Max(g => g.st.createdDate)
                        from grp in groups
                        where grp.st.effectiveDate == MaxPathDate
                        select groups.OrderByDescending(x => x.st.dependantId).FirstOrDefault();

            var grouped = model.Where(x => x.st.managementStatusCode == "FIT" || x.st.managementStatusCode == "ACT" || x.st.managementStatusCode == "REE" || x.st.managementStatusCode == "PRH" ||
            x.st.managementStatusCode == "FAS" || x.st.managementStatusCode == "ENR" || x.st.managementStatusCode == "FNA" || x.st.managementStatusCode == "FIO" || x.st.managementStatusCode == "FOP" ||
            x.st.managementStatusCode == "FOS" || x.st.managementStatusCode == "MTC" || x.st.managementStatusCode == "MT1" || x.st.managementStatusCode == "MTN" || x.st.managementStatusCode == "PEP" ||
            x.st.managementStatusCode == "PRA" || x.st.managementStatusCode == "PRC")
                .Select(g => new financialReport()
            {
                membershipNo = g.m.membershipNo,
                dependantCode = g.d.dependentCode,
                medicalAidCode = g.me.medicalAidCode,
                status = _context.ManagementStatus.Where(x => x.statusCode == g.st.managementStatusCode).Select(x => x.statusName).FirstOrDefault(),
                }).ToList();

            return grouped;
        }

        public ServiceResult InsertRiskData(RiskReportingData model)
        {
            _context.RiskReportingData.Add(model);

            return _context.SaveChanges();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}