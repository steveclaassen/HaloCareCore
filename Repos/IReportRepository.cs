using HaloCareCore.Models.Reports;
using HaloCareCore.Models.Service;
using System;
using System.Collections.Generic;

namespace HaloCareCore.Repos
{
    public interface IReportRepository : IDisposable
    {
        int GetActivePatientCount(string schemeCode);
        List<ACTPatients> GetPatientCountByStatus(string schemeCode, string status);
        List<ACTGENPatients> GetActiveGenderPatientCount(string schemeCode);
        List<ACTPatients> GetMTCPHRPatientCount(string schemeCode);
        List<ACTPatients> GetARTPatientCount(string schemeCode);
        List<ACTPatients> GetFOIPatientCount(string schemeCode);
        List<AVGPathCount> AVGCD4Count(string schemeCode);
        List<AVGPathCount> AVGCD4Perc(string schemeCode);
        List<AVGPathCount> AVgViralLoadlt1000Count(string schemeCode);
        List<AVGPathCount> AVgViralLoadgt1000Count(string schemeCode);
        List<AVGPathCount> NewEnrolments(string schemeCode);
        RiskReportViewModel GetRiskVRViewCount(string schemeCode);
        RiskReportViewModel GetRiskCDViewCount(string schemeCode);
        ServiceResult InsertRiskData(RiskReportingData model);

        operationalReportViewModel OpsNewEnrolments(string schemeCode);
        List<operationalReportViewModel> OpsPatientStatusReport(string schemeCode);
        List<operationalReportViewModel> OpsPatientComsReport(string schemeCode);
        List<operationalReportViewModel> OpsPatientEnquiriesReport(string schemeCode);
        List<operationalReportViewModel> OpsPatientSmsReport(string schemeCode);
        List<operationalReportViewModel> OpsPatientSmsStatusReport(string schemeCode);

        List<financialReport> FinancialStatusList(string schemeCode);
        List<operationalReportViewModel> FinPatientStatusReport(string schemeCode);
    }
}