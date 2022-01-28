using ClosedXML.Excel;
using HaloCareCore.DAL;
using HaloCareCore.Filters;
using HaloCareCore.Models;
using HaloCareCore.Models.Reports;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaloCareCore.Controllers
{

    public class ReportManagementController : Controller
    {
        private IReportRepository _reports;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportManagementController(OH17Context context, IConfiguration configuration)
        {
            _reports = new ReportRepository(context);
        }
        // GET: ReportManagement Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: diseaseManagement - HIV & Diabetes options
        public ActionResult diseaseManagement()
        {
            return View();
        }



        // HIV Reports

        // GET: hivDashboard
        public ActionResult hivDashboard()
        {
            return View();
        }

        // GET: ClientDashboard
        public ActionResult hivClientDashboard()
        {
            return View();
        }

        // GET: RiskReport
        public ActionResult hivRiskReport()
        {
            return View();
        }

        // GET: Enrolments
        public ActionResult hivEnrolments()
        {
            /*
            var model = _reports.GetPatientCountByStatus("SELFMED","ACT");
            List<int> reparts = new List<int>();
            var patientCounts = model.Select(x => x.reportYear).Distinct();
            
            foreach (var item in model)
            {
                item.reportMonth = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Convert.ToInt16(item.reportMonth));
            }
            return View(model);
            */
            return View();
        }

        public JsonResult BarChartData()
        {

            Chart _chart = new Chart();
            _chart.labels = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "Novemeber", "December" };
            _chart.datasets = new List<Datasets>();
            List<Datasets> _dataSet = new List<Datasets>();
            _dataSet.Add(new Datasets()
            {
                label = "Current Year",
                data = new int[] { 28, 48, 40, 19, 86, 27, 90, 20, 45, 65, 34, 22 },
                backgroundColor = new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                borderColor = new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                borderWidth = "1"
            });
            _chart.datasets = _dataSet;
            return Json(_chart);
        }

        // GET: ActiveOnProgram
        public ActionResult hivActiveOnProgram()
        {
            return View();
        }

        // GET: ActiveOnMedication
        public ActionResult hivActiveOnMedication()
        {
            return View();
        }

        // GET: NotTxReady
        public ActionResult hivNotTxReady()
        {
            return View();
        }

        // GET: GenderAndAge
        public ActionResult hivGenderAndAge()
        {
            return View();
        }

        // GET: PEP&PrEP
        public ActionResult hivPepAndPrep()
        {
            return View();
        }

        // GET: CD4
        public ActionResult hivCd4()
        {
            return View();
        }

        // GET: CD4Migration
        public ActionResult hivCD4Migration()
        {
            return View();
        }

        // GET: ViralLoad
        public ActionResult hivViralLoad()
        {
            return View();
        }

        // GET: ViralLoadMigration
        public ActionResult hivViralLoadMigration()
        {
            return View();
        }

        // GET: DeactivatedPatients
        public ActionResult hivDeactivatedPatients()
        {
            return View();
        }

        // GET: Script
        public ActionResult hivRegime()
        {
            return View();
        }

        // GET: Analytics
        public ActionResult hivReportAnalytics()
        {
            return View();
        }

        // GET: patientList
        public ActionResult hivPatientList()
        {
            var model = new financialView();
            model.finList = _reports.FinancialStatusList("SELFMED");
            model.statList = _reports.FinPatientStatusReport("SELFMED");
            return View(model);
        }

        public JsonResult DonutChartData()
        {
            var statlist = _reports.FinPatientStatusReport("SELFMED");
            string[] labels = new string[statlist.Count()];
            int[] values = new int[statlist.Count()];
            int i = 0;
            foreach (var item in statlist)
            {
                labels[i] = item.strName;
                values[i] = item.strCount;
                i++;
            }

            Chart _chart = new Chart();
            _chart.labels = labels;
            _chart.datasets = new List<Datasets>();
            List<Datasets> _dataSet = new List<Datasets>();
            _dataSet.Add(new Datasets()
            {
                label = "Current Year",
                data = values,
                backgroundColor = new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                borderColor = new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                borderWidth = "1"
            });
            _chart.datasets = _dataSet;
            return Json(_chart);
        }

        [HttpPost]
        public void ExportToExcel(operationalReportViewModel model)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Demo");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = model.newEnrolmentCount;
                worksheet.Cell(currentRow, 2).Value = model.strName;
                worksheet.Cell(currentRow, 3).Value = model.strCount;
                currentRow++;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment;filename=DemoExcel.xls");
                Response.ContentType = "application/xls";
                Response.Body.WriteAsync(content);
                Response.Body.Flush();
            }
        }

        // GET: operationsReport
        public ActionResult hivOperationsReport()
        {
            return View();
        }


        // Diabetes Reports

        // GET: diabetesDashboard
        public ActionResult diabetesDashboard()
        {
            return View();
        }

        // GET: diabetesClientDashboard
        public ActionResult diabetesClientDashboard()
        {
            return View();
        }

        // GET: diabetesRiskReport
        public ActionResult diabetesRiskReport()
        {
            return View();
        }

        // GET: diabetesEnrolments
        public ActionResult diabetesEnrolments()
        {
            return View();
        }

        // GET: diabetesActiveOnProgram
        public ActionResult diabetesActiveOnProgram()
        {
            return View();
        }

        // GET: diabetesActiveOnMedication
        public ActionResult diabetesActiveOnMedication()
        {
            return View();
        }

        // GET: diabetesNotTxReady
        public ActionResult diabetesNotTxReady()
        {
            return View();
        }

        // GET: diabetesGenderAndAge
        public ActionResult diabetesGenderAndAge()
        {
            return View();
        }

        // GET: diabetesDietOralInsulin
        public ActionResult diabetesDietOralInsulin()
        {
            return View();
        }

        // GET: diabetesAveHbA1c
        public ActionResult diabetesAveHbA1c()
        {
            return View();
        }

        // GET: diabetesRiskEvaluation
        public ActionResult diabetesRiskEvaluation()
        {
            return View();
        }

        // GET: diabetesRegime
        public ActionResult diabetesRegime()
        {
            return View();
        }

        // GET: diabetesDeactivatedPatients
        public ActionResult diabetesDeactivatedPatients()
        {
            return View();
        }

        // GET: diabetesAnalytics
        public ActionResult diabetesReportAnalytics()
        {
            return View();
        }


        // GET: diabetesPatientList
        public ActionResult diabetesPatientList()
        {
            return View();
        }

        // GET: diabetesOperationsReport
        public ActionResult diabetesOperationsReport()
        {
            return View();
        }


        // GET: reportDashboard
        public ActionResult reportDashboard()
        {
            {
                return View();
            }
        }

        // GET: reportHIV
        public ActionResult reportHIV()
        {
            return View();
        }

        // GET: reportDiabetes
        public ActionResult reportDiabetes()
        {
            return View();
        }



        // GET: riskReport
        public ActionResult riskReport()
        {
            return View();
        }

        // GET: clientReport
        public ActionResult clientReport()
        {
            return View();
        }

        // GET: boardReport
        public ActionResult boardReport()
        {
            return View();
        }

        // GET: operationsDiseaseManagement
        public ActionResult operationsDiseaseManagement()
        {
            return View();
        }

        // GET: analyticsDiseaseManagement
        public ActionResult analyticsDiseaseManagement()
        {
            return View();
        }

        // GET: excutiveDashboard
        public ActionResult executiveDashboard()
        {
            return View();
        }

        // GET: performanceDashboard
        public ActionResult performanceDashboard()
        {
            return View();
        }

        // GET: caseManagerProduction
        public ActionResult caseManagerProduction()
        {
            return View();
        }

        // GET: serviceLevelAgreement
        public ActionResult serviceLevelAgreement()
        {
            return View();
        }

        // GET: productionReport
        public ActionResult productionReport()
        {
            return View();
        }

        // GET: patientList
        public ActionResult patientList()
        {
            return View();
        }

        // GET: operationsReport
        public ActionResult operationsReport()
        {
            return View();
        }

        // GET: enrolmentReport
        public ActionResult enrolmentReport()
        {
            return View();
        }

        // GET: activeOnProgram
        public ActionResult activeOnProgram()
        {
            return View();
        }

        // GET: activeOnMeds
        public ActionResult activeOnMedication()
        {
            return View();
        }


        // GET: analyticalReport
        public ActionResult analyticalReport()
        {
            return View();
        }


    }

}