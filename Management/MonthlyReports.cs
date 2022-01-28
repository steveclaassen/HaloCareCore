using HaloCareCore.DAL;
using HaloCareCore.Models.Reports;
using HaloCareCore.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Management
{
    public class MonthlyReports
    {
        private IReportRepository _reports;
        private OH17Context context;
        public MonthlyReports()
        {
            _reports = new ReportRepository(context);
        }
       
        public void GetRiskReport(string medicalAidCode)
        {
            var year = DateTime.Now.AddMonths(-1).Year;
            var month = DateTime.Now.AddMonths(-1).Month;

            var activePatientCOunt = _reports.GetActivePatientCount("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfActivePatients","",activePatientCOunt, 0);

            var activeGenderSummary = _reports.GetActiveGenderPatientCount("SELFMED");
            foreach (var item in activeGenderSummary)
            {
                InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfActivePatientsByGender", item.statusCountACTGender, Convert.ToInt32(item.actpatients.statusCountACT), 0);
            }    

            var mtcPreHaartSummary = _reports.GetMTCPHRPatientCount("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfMtcNotForOngoingAndPreHaartPatients", "", Convert.ToInt32(mtcPreHaartSummary[0].statusCountACT), 0);

            var artSummary = _reports.GetARTPatientCount("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfActiveMembersOnART", "", Convert.ToInt32(artSummary[0].statusCountACT), 0);

            var foiSummary = _reports.GetFOIPatientCount("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfFocusInterventionPatients", "", Convert.ToInt32(foiSummary[0].statusCountACT), 0);

            var deregSummary = _reports.GetPatientCountByStatus("SELFMED", "NON");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfDeRegisteredPatients", "", Convert.ToInt32(deregSummary[0].statusCountACT), 0);

            var PEPCompleteSummary = _reports.GetPatientCountByStatus("SELFMED", "PEC");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SummaryOfPepCompletePatients", "", Convert.ToInt32(PEPCompleteSummary[0].statusCountACT), 0);

            var SuspendedSummary = _reports.GetPatientCountByStatus("SELFMED", "SUS");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "SuspensionsByMonth", "", Convert.ToInt32(SuspendedSummary[0].statusCountACT), 0);

            var DeceasedSummary = _reports.GetPatientCountByStatus("SELFMED", "DEC");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "DeceasedPatientsByMonth", "", Convert.ToInt32(DeceasedSummary[0].statusCountACT), 0);

            var ResignedSummary = _reports.GetPatientCountByStatus("SELFMED", "RES");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "ResignationsByMonth", "", Convert.ToInt32(ResignedSummary[0].statusCountACT), 0);

            var CD4Count = _reports.AVGCD4Count("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "AverageCd4CountOfActivePatientsExclPreHaart", "", 0, Convert.ToDouble(CD4Count[0].avgPathCount));

            var CD4Perc = _reports.AVGCD4Perc("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "AverageCd4CountPercentageExclPreHaart", "", 0, Convert.ToDouble(CD4Count[0].avgPathCount));

            var viralLoadlt1000 = _reports.AVgViralLoadlt1000Count("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "AverageViralLoadUnder1000", "", 0, Convert.ToDouble(CD4Count[0].avgPathCount));

            var viralLoadgt1000 = _reports.AVgViralLoadgt1000Count("SELFMED");
            InsertRiskData(year, month, "HIV", "SELFMED", "", "AverageViralLoadMoreThen1000", "", 0, Convert.ToDouble(CD4Count[0].avgPathCount));
        }

        public void InsertRiskData(int year, int month, string area, string medical, string subname, string fieldname, string other, int value, double doubleint )
        {
            var model = new RiskReportingData()
            {
                id = Guid.NewGuid(),
                year = year,
                month = month,
                area = area,
                name = medical,
                subName = subname,
                fieldName = fieldname,
                other = other,
                createdDate = DateTime.Now,
                createdBy = "Monthly Reporting Service",
                valueInt = value,
                valueDouble = doubleint,
            };

            var result = _reports.InsertRiskData(model);
        }

        public void GetOperationalReport(string medicalAidCode)
        {
            var newEnrollments = _reports.OpsNewEnrolments("SELFMED");
            var PatientStatusCount = _reports.OpsPatientStatusReport("SELFMED");
            var PatientCommunicationCount = _reports.OpsPatientComsReport("SELFMED");
            var PatientEnquiriesCount = _reports.OpsPatientEnquiriesReport("SELFMED");
            var GetSmsComs = _reports.OpsPatientSmsReport("SELFMED");
            var GetSMSComsStatus = _reports.OpsPatientSmsStatusReport("SELFMED");
        }

        public void GetFinanceReport(string medicalAidCode)
        {
            var activeSummary = _reports.OpsPatientStatusReport("SELFMED");
            var PatientList = _reports.FinancialStatusList("SELFMED");
        }
    }
}