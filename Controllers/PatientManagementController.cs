using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace HaloCareCore.Controllers
{
    public class PatientManagementController : Controller
    {
        private IMemberRepository _member;
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PatientManagementController(OH17Context context, IConfiguration configuration)
        {
            _member = new MemberRepository(configuration, context);
            _admin = new AdminRepository(context, configuration);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DoctorSearch(string DependentID)
        {
            var model = new DoctorSearchVars();
            if (DependentID != null) { model.dependentID = DependentID; }

            return View(model);
        }
        [HttpPost]
        public ActionResult DoctorSearch(DoctorSearchVars model)
        {
            var result = new DoctorSearchResults();
            result.doctorResults = new List<Doctors>();
            if (model.dependentID != null) { result.DependentID = model.dependentID; }

            result.doctorResults = _admin.SearchDoctors(model);
            return View("DoctorSearchResult", result);
        }

        public ActionResult DoctorSearchResult(List<Doctors> result)
        {
            return View(result);
        }

        public ActionResult DetailedDoctorView(Guid id)
        {
            var model = _admin.GetDoctorInformation(id);
            return View(model);
        }

        public ActionResult AddDoctorToProfile(string docID, string depId, string doctorname, Guid? pro)
        {
            var doctorH = new PatientDoctorHistory()
            {
                doctorId = new Guid(docID),
                createdBy = User.Identity.Name,
                createdDate = DateTime.Now,
                effectiveDate = DateTime.Now,
                active = true,
                dependantId = new Guid(depId),
                doctorName = doctorname,
                ProgramID = pro,
            };
            var respo = _member.InsertDoctorHistory(doctorH);
            return RedirectToAction("patientDashboard", "Member", new { DependentID = depId, pro = pro }); //hcare-1048
        }

        //------------------------------------------------------------------------------------ advanced-search  ----------------------------------------------------------------------------------->

        public ActionResult SearchResults(searchResultVM results)
        {
            return View(results);
        }

        public ActionResult AdvancedSearch()
        {
            //HCare-1250
            ViewBag.statusCode = new SelectList(_member.GetManagementStatuses().Where(x => x.active == true), "statusCode", "statusName"); //HCare-1052
            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "medicalAidCode", "Name");
            ViewBag.programID = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            ViewBag.sex = new SelectList(_member.GetSex(), "sex", "sexName");
            ViewBag.caseManager = new SelectList(_admin.GetCaseManagers(), "caseManagerNo", "cmFullName");
            return View();
        }

        [HttpPost]
        public ActionResult AdvancedSearch(AdvancedSearchView model)
        {
            //if (Request.Query["statusCode"].ToString() != null) { model.statusCode = Request.Query["statusCode"].ToString(); }
            //if (Request.Query["medicalAidID"].ToString() != null) { model.medAidId = Request.Query["medicalAidID"].ToString(); }
            //if (Request.Query["programID"].ToString() != null) { model.programCode = Request.Query["programID"].ToString(); }
            //if (Request.Query["sex"].ToString() != null) { model.sex = Request.Query["sex"].ToString(); }
            //if (Request.Query["caseManager"].ToString() != null) { model.caseManager = Request.Query["caseManager"].ToString(); }
            //if (Request.Query["riskRating"].ToString() != null) { model.riskRating = Request.Query["riskRating"].ToString(); }
            //if (model.dateTo != null) { model.dateTo = Convert.ToDateTime(model.dateTo?.ToString("yyyy-MM-dd 23:59:59")); } //HCare-1078

            var results = new searchResultVM();
            results.advancedSearchResultModels = _member.GetAdvancedSearchresults(model);
            ViewBag.Total = results.advancedSearchResultModels.Count();

            results.advancedSearchResultModels = results.advancedSearchResultModels.Take(1000).ToList();
            results.advancedSearchView = model;

            return View("SearchResults", results);
        }

        //HCare-643
        public ActionResult ExportAdvancedSearchNew(searchResultVM model)
        {
            model.advancedSearchResultModels = _member.GetAdvancedSearchresults(model.advancedSearchView);

            ExcelPackage excel = new ExcelPackage();
            var workSheet1 = excel.Workbook.Worksheets.Add("Results");
            workSheet1.Cells[1, 1].LoadFromCollection(model.advancedSearchResultModels, true);
            workSheet1.Row(1).Style.Font.Bold = true;

            workSheet1.DeleteColumn(1); //DependantID //HCARE-684
            workSheet1.DeleteColumn(1); //medicalAidID
            workSheet1.DeleteColumn(1); //programId
            workSheet1.DeleteColumn(5); //Patient First Name (UPPERCASE)
            workSheet1.DeleteColumn(6); //Patient Lastname (UPPERCASE)
            workSheet1.DeleteColumn(28); //managment-status-code

            workSheet1.Column(1).Width = 20; //member-number
            workSheet1.Column(3).Width = 20; //id-number
            workSheet1.Column(4).Width = 28; //medical-scheme
            workSheet1.Column(5).Width = 28; //member-name
            workSheet1.Column(6).Width = 28; //member-surname

            workSheet1.Column(7).Style.Numberformat.Format = "dd-mm-yyyy"; //date-of-birth
            workSheet1.Column(7).Width = 15; //date-of-birth

            workSheet1.Column(8).Width = 15; //scheme-option

            //program-information
            workSheet1.Column(9).Width = 15; //program-code
            workSheet1.Column(10).Width = 15; //program-icd-10
            workSheet1.Column(11).Style.Numberformat.Format = "dd-mm-yyyy"; //program-start-date
            workSheet1.Column(11).Width = 20; //program-start-date
            workSheet1.Column(12).Style.Numberformat.Format = "dd-mm-yyyy"; //program-end-date
            workSheet1.Column(12).Width = 20; //program-end-date

            //patient-status 
            workSheet1.Column(13).Width = 28; //patient-status //hcare-1314

            //management-status
            workSheet1.Column(14).Width = 28; //management-status
            workSheet1.Column(15).Style.Numberformat.Format = "dd-mm-yyyy"; //ms-effective-date
            workSheet1.Column(15).Width = 15; //ms-effective-date
            workSheet1.Column(16).Style.Numberformat.Format = "dd-mm-yyyy";  //ms-end-date //hcare-651
            workSheet1.Column(16).Width = 15; //ms-end-date
            workSheet1.Column(17).Width = 15; //ms-modified-by
            workSheet1.Column(18).Style.Numberformat.Format = "dd-mm-yyyy"; //ms-modified-date
            workSheet1.Column(18).Width = 15; //ms-modified-date

            //doctor-inforamtion
            workSheet1.Column(19).Width = 15; //doctor-bhf#
            workSheet1.Column(20).Width = 20; //doctor-name
            workSheet1.Column(21).Width = 20; //doctor-surname
            workSheet1.Column(22).Width = 20; //doctor-email
            workSheet1.Column(23).Width = 20; //doctor-mobile

            workSheet1.Column(24).Width = 25; //last-laboratory-lab

            workSheet1.Column(25).Width = 25; //dependant-case-manager

            workSheet1.Column(26).Width = 15; //dependant-mobile
            workSheet1.Column(27).Width = 15; //dependant-telephone
            workSheet1.Column(28).Style.Numberformat.Format = "dd-mm-yyyy"; //dependant-created-date
            workSheet1.Column(28).Width = 15; //dependant-created-date

            //risk-rating
            workSheet1.Column(29).Width = 20;//risk-rating-history
            workSheet1.Column(30).Width = 20;//rr-history-priority
            workSheet1.Column(31).Style.Numberformat.Format = "dd-mm-yyyy"; //rr-history-effective-date
            workSheet1.Column(31).Width = 20; //rr-history-effective-date
            workSheet1.Column(32).Width = 20; //risk-rating-current
            workSheet1.Column(33).Width = 20; //rr-current-priority
            workSheet1.Column(34).Style.Numberformat.Format = "dd-mm-yyyy"; //rr-current-effective-date
            workSheet1.Column(34).Width = 20; //rr-current-effective-date
            workSheet1.Column(35).Width = 20; //rr-overall-rating
            workSheet1.Column(36).Width = 20; //rr-overall-priority

            //hba1c
            workSheet1.Column(37).Width = 15; //hba1c
            workSheet1.Column(38).Style.Numberformat.Format = "dd-mm-yyyy"; //hba1c-effective-date
            workSheet1.Column(38).Width = 15; //hba1c-effective-date

            //viral-load
            workSheet1.Column(39).Width = 15; //viral-load
            workSheet1.Column(40).Style.Numberformat.Format = "dd-mm-yyyy"; //viral-load-effective-date
            workSheet1.Column(40).Width = 15; //viral-load-effective-date

            //cervical-cancer
            workSheet1.Column(41).Width = 15; //cervical-cancer //hcare-956
            workSheet1.Column(42).Style.Numberformat.Format = "dd-mm-yyyy"; //cervical-cancer-effective-date //hcare-956
            workSheet1.Column(42).Width = 15; //cervical-cancer-effective-date //hcare-956
            workSheet1.Column(43).Width = 20; //cervical-cancer-results //hcare-956

            //frail-care 
            workSheet1.Column(44).Width = 15; //frail-care //hcare-931
            workSheet1.Column(45).Width = 20; //frail-care-assistance //hcare-931
            workSheet1.Column(46).Width = 20; //frail-care-comment //hcare-931

            //tb-screen //HCare-1275
            workSheet1.Column(47).Width = 15; //tb-screen
            workSheet1.Column(48).Width = 20; //tb-screen-result
            workSheet1.Column(49).Style.Numberformat.Format = "dd-mm-yyyy"; //tb-start-date
            workSheet1.Column(49).Width = 20; //tb-start-date
            workSheet1.Column(50).Style.Numberformat.Format = "dd-mm-yyyy"; //tb-end-date
            workSheet1.Column(50).Width = 20; //tb-end-date

            //mental-health
            workSheet1.Column(51).Width = 20; //mh-suicide-current
            workSheet1.Column(52).Width = 20; //mh-suicide-history
            workSheet1.Column(53).Width = 20; //mh-depression-current
            workSheet1.Column(54).Width = 20; //mh-depression-history
            workSheet1.Column(55).Width = 20; //mh-dsm5-current
            workSheet1.Column(56).Width = 20; //mh-dsm5-history

            //doctor-referral
            workSheet1.Column(57).Width = 15; //doctor-referral
            workSheet1.Column(58).Style.Numberformat.Format = "dd-mm-yyyy"; //doctor-referral-date
            workSheet1.Column(58).Width = 15; //doctor-referral-date
            workSheet1.Column(59).Width = 15; //doctor-referral-from //hcare-1144

            //hiv-stage
            workSheet1.Column(60).Width = 15; //stage-code //hcare-1014 

            //insulin-dependance
            workSheet1.Column(61).Width = 15; //insulin-check //hcare-673 
            workSheet1.Column(62).Style.Numberformat.Format = "dd-mm-yyyy"; //insulin-effective-date
            workSheet1.Column(63).Width = 15; //oral-injectable-check //hcare-673 
            workSheet1.Column(64).Style.Numberformat.Format = "dd-mm-yyyy"; //oral-injectable-effective-date
            workSheet1.Column(65).Width = 15; //dietary-check //hcare-673 
            workSheet1.Column(66).Style.Numberformat.Format = "dd-mm-yyyy"; //dietary-effective-date

            //state-enrolled
            workSheet1.Column(67).Width = 15; //hcare-863

            //employer-code
            workSheet1.Column(68).Width = 15; //employer-code //hcare-1373
            workSheet1.Column(69).Width = 25; //employer-code-description //hcare-1373
            workSheet1.Column(70).Width = 15; // EmailOptOut


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=advanced-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);
            }

        }

        //hidden on 19-02-2021 for clean-up purposes
        //public ActionResult aSearchExtract(searchResultVM model)
        //{
        //    var sb = new StringBuilder();

        //    var aSearch = _member.GetAdvancedSearchresults(model.advancedSearchView);

        //    var grid = new System.Web.UI.WebControls.GridView();
        //    grid.DataSource = model;
        //    grid.DataBind();
        //    Response.Clear();
        //    Response.Headers.Add("content-disposition", "attachment; filename=advancedSearch.xls");
        //    Response.ContentType = "application/vnd.ms-excel";
        //    StringWriter sw = new StringWriter();
        //    System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
        //    grid.RenderControl(htw);
        //    Response.WriteAsync(sw.ToString());
        //    Response.StatusCode = StatusCodes.Status200OK;
        //    return View(model);
        //}

        //public ActionResult ExportAdvancedSearch(searchResultVM model)
        //{
        //    var sb = new StringBuilder();

        //    model.advancedSearchResultModels = _member.GetAdvancedSearchresults(model.advancedSearchView);

        //    var grid = new System.Web.UI.WebControls.GridView();
        //    grid.DataSource = model.advancedSearchResultModels;
        //    grid.DataBind();

        //    Response.Clear();
        //    Response.Buffer = true;
        //    Response.Headers.Add("content-disposition", "attachment; filename=SearchResults.xls");
        //    
        //    Response.ContentType = "application/vnd.ms-excel";
        //    StringWriter sw = new StringWriter();
        //    System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
        //    grid.RenderControl(htw);
        //    Response.WriteAsync(sw.ToString());
        //    Response.StatusCode = StatusCodes.Status200OK;
        //    return View(model);
        //}

        //------------------------------------------------------------------------------------ assignment-search  ----------------------------------------------------------------------------------->

        #region assignment-search hcare-1129
        public ActionResult AssignmentSearch()
        {
            var model = new AssignmentSearchVM();
            model.AssignmentSearches = new List<AssignmentSearch>();

            model.filter = true;

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();
            model.AssignmentItemTypes = _member.GetAssignmentItemTypes().Where(x => x.active == true).ToList();
            model.AssignmentItemTypes = model.AssignmentItemTypes.Where(x => x.diabetes == true || x.hiv == true || x.mental == true).ToList(); //Hcare-995

            return View(model);

        }

        [HttpPost]
        public ActionResult AssignmentSearch(AssignmentSearchVM model)
        {

            #region form-dropdowns
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetPrograms().Where(x => x.Active == true).ToList();
            model.AssignmentItemTypes = _member.GetAssignmentItemTypes().Where(x => x.active == true).ToList();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var assignmentitem = String.Empty;

            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (model.SelectedAssignmentTypes != null) { assignmentitem = String.Join(",", model.SelectedAssignmentTypes); } else { model.SelectedAssignmentTypes = new string[] { }; }

            #region date-variables
            var fromDate = "";
            var toDate = "";

            if (!String.IsNullOrEmpty(Request.Query["FromDate"]))
            {
                fromDate = Convert.ToDateTime(Request.Query["FromDate"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                //fromDate = "01 Jan 2018 00:00:00";
                fromDate = new DateTime(2018, 1, 1).ToString("dd-MMM-yyyy 00:00:00"); //HCare-1129
            }
            if (!String.IsNullOrEmpty(Request.Query["ToDate"]))
            {
                toDate = Convert.ToDateTime(Request.Query["ToDate"]).ToString("dd-MMM-yyyy 23:59:59"); //HCare-1129
            }
            else
            {
                toDate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59"); //HCare-1129
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["assignmentitems"] = assignmentitem;
            TempData["fromdate"] = fromDate;
            TempData["todate"] = toDate;
            #endregion

            model.filter = false;
            model.AssignmentSearches = _member.GetAssignmentSearchResults(medicalaid, program, assignmentitem, fromDate, toDate);

            return View(model);
        }

        public ActionResult ExportAssignmentToExcel(AssignmentSearchVM model)
        {
            var medicalaids = Request.Query["medicalaids"].ToString();
            var programs = Request.Query["programs"].ToString();
            var assignmentitems = Request.Query["assignmentitems"].ToString();
            var fromdate = Request.Query["fromdate"].ToString();
            var todate = Request.Query["todate"].ToString();

            model.AssignmentSearches = _member.GetAssignmentSearchResults(medicalaids, programs, assignmentitems, fromdate, todate);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Scheme");
            table.Columns.Add("Option");
            table.Columns.Add("Member number");
            table.Columns.Add("Dep code");
            table.Columns.Add("ID/Auth #");
            table.Columns.Add("Patient name");
            table.Columns.Add("Program");
            table.Columns.Add("Patient status");
            table.Columns.Add("Effective date");
            table.Columns.Add("Item type code");
            table.Columns.Add("Assignment type");
            table.Columns.Add("Item type description");
            table.Columns.Add("Task closed count");
            table.Columns.Add("Assignment ID");
            table.Columns.Add("Assignment status");
            table.Columns.Add("Assignment age");
            table.Columns.Add("Ellapsed time");

            foreach (var line in model.AssignmentSearches)
            {
                DataRow row = table.NewRow();
                row["Scheme"] = line.MedicalScheme;
                row["Option"] = line.MedicalOption;
                row["Member number"] = line.MembershipNumber;
                row["Dep code"] = line.DependantCode;
                row["ID/Auth #"] = line.IDNumber;
                row["Patient name"] = line.PatientName;
                row["Program"] = line.Program;
                row["Patient status"] = line.ManagementStatus;
                row["Effective date"] = line.EffectiveDate.Value.ToString("dd-MM-yyyy");
                row["Item type code"] = line.AssignmentItemCode;
                row["Assignment type"] = line.AssignmentType;
                row["Item type description"] = line.AssignmentItemType;
                row["Task closed count"] = line.AssignmentTasksClosed + "/" + line.AssignmentTasksCount;
                row["Assignment ID"] = line.AssignmentID;
                row["Assignment status"] = line.AssignmentStatus;
                row["Assignment age"] = line.AssignmentAge;
                row["Ellapsed time"] = line.ElapsedTime;

                table.Rows.Add(row);
            }

            DataView view = new DataView(table);

            #endregion
            #region excel
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Cells["A1"].LoadFromDataTable(table, true);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=assignment-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }

        public ActionResult FillAssignmentItem(Guid pro)
        {
            var options = _member.GetAssignmentItemTypesByProgram(pro);
            options = options.Where(x => x.diabetes == true || x.hiv == true || x.mental == true).ToList(); //Hcare-995
            return Json(options);
        }

        #endregion

        //------------------------------------------------------------------------------------ query-report  ----------------------------------------------------------------------------------->
        public ActionResult SearchQueries()
        {
            ViewBag.queryType = new SelectList(_member.GetQueryTypes(), "queryDescription", "queryDescription");
            ViewBag.medicalAid = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "Name", "Name"); //HCare-680(Enquiry search enhanced)
            ViewBag.memberStatus = new SelectList(_member.GetManagementStatus().Where(x => x.statusCode != "DEA").Where(x => x.statusCode != "DEC").Where(x => x.statusCode != "RES").Where(x => x.statusCode != "PEC").Where(x => x.statusCode != "PRC"), "statusName", "statusName"); //HCare-680(Enquiry search enhanced)
            ViewBag.queryPriority = new SelectList(_member.GetPriorities().Where(x => x.active == true), "priorityName", "priorityName"); //HCare-680(Enquiry search enhanced)

            var model = _member.GetActiveSQueries().Take(50); //HCare-619(Remove 'inactive' schemes from the Open enquiry view)

            return View(model);
        }
        [HttpPost]
        public ActionResult SearchQueries(List<EnquiriesSearch> model)
        {
            var querytype = Request.Query["queryType"].ToString();
            var member = Request.Query["membershipno"].ToString();
            var medicalaid = Request.Query["medicalAid"].ToString();
            var memberStatus = Request.Query["memberStatus"].ToString();
            var queryPriority = Request.Query["queryPriority"].ToString();
            var queryStatus = Request.Query["queryStatus"].ToString();

            model = _member.GetSearchQueriesResults(querytype, member, medicalaid, memberStatus, queryPriority, queryStatus);

            ViewBag.queryType = new SelectList(_member.GetQueryTypes(), "queryDescription", "queryDescription", querytype);
            ViewBag.medicalAid = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "Name", "Name"); //HCare-680(Enquiry search enhanced)
            ViewBag.memberStatus = new SelectList(_member.GetManagementStatus().Where(x => x.statusCode != "DEA").Where(x => x.statusCode != "DEC").Where(x => x.statusCode != "RES").Where(x => x.statusCode != "PEC").Where(x => x.statusCode != "PRC"), "statusName", "statusName"); //HCare-680(Enquiry search enhanced)
            ViewBag.queryPriority = new SelectList(_member.GetPriorities().Where(x => x.active == true), "priorityName", "priorityName"); //HCare-680(Enquiry search enhanced)

            return View(model);
        }

        //HCare-680
        public ActionResult ExportQueriesToExcel(List<EnquiriesSearch> model)
        {
            if (Request.Query["queryType"].ToString() != null || Request.Query["membershipno"].ToString() != null || Request.Query["medicalAid"].ToString() != null || Request.Query["memberStatus"].ToString() != null || Request.Query["queryPriority"].ToString() != null || Request.Query["queryStatus"].ToString() != null)
            {
                var querytype = Request.Query["queryType"].ToString();
                var member = Request.Query["membershipno"].ToString();
                var medicalaid = Request.Query["medicalAid"].ToString();
                var memberStatus = Request.Query["memberStatus"].ToString();
                var queryPriority = Request.Query["queryPriority"].ToString();
                var queryStatus = Request.Query["queryStatus"].ToString();

                model = _member.GetSearchQueriesResults(querytype, member, medicalaid, memberStatus, queryPriority, queryStatus);

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells[1, 1].LoadFromCollection(_member.GetSearchQueriesResults(querytype, member, medicalaid, memberStatus, queryPriority, queryStatus), true);

                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.DeleteColumn(2); //Medical scheme Status
                workSheet.DeleteColumn(5); //Full name (UPPERCASE)
                //workSheet.DeleteColumn(10); //Query Effective date
                workSheet.DeleteColumn(26); //Dependant ID

                workSheet.Column(11).Style.Numberformat.Format = "dd-mm-yyyy";  //Column K - Effective date
                workSheet.Column(16).Style.Numberformat.Format = "dd-mm-yyyy";  //Column P - Created date
                workSheet.Column(22).Style.Numberformat.Format = "dd-mm-yyyy";  //Column V - Modified date
                workSheet.Column(24).Style.Numberformat.Format = "dd-mm-yyyy";  //Column X - Resolution date
                //workSheet.Column(26).Style.Numberformat.Format = "hh:mm";  //Column Z - Query Age

                //column-width
                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();
                workSheet.Column(8).Width = 30;
                workSheet.Column(9).AutoFit();
                workSheet.Column(10).Width = 30;
                workSheet.Column(11).Width = 30;
                workSheet.Column(12).Width = 20;
                workSheet.Column(13).Width = 20;
                workSheet.Column(14).AutoFit();
                workSheet.Column(15).AutoFit();
                workSheet.Column(16).Width = 15;
                workSheet.Column(17).AutoFit();
                workSheet.Column(18).AutoFit();
                workSheet.Column(19).AutoFit();
                workSheet.Column(20).Width = 15;
                workSheet.Column(21).AutoFit();
                workSheet.Column(22).AutoFit();
                workSheet.Column(23).Width = 30;
                workSheet.Column(24).AutoFit();
                workSheet.Column(25).Width = 30;
                workSheet.Column(26).AutoFit();
                workSheet.Column(27).AutoFit();
                workSheet.Column(28).Width = 30;


                //column-style
                workSheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MedicalAid
                workSheet.Column(1).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MedicalAid
                workSheet.Column(2).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MemberNumber
                workSheet.Column(2).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MemberNumber
                workSheet.Column(8).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescriptionCode
                workSheet.Column(8).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescriptionCode
                workSheet.Column(9).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescription
                workSheet.Column(9).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescription
                workSheet.Column(17).Style.Fill.PatternType = ExcelFillStyle.Solid;  //Priority
                workSheet.Column(17).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //Priority
                workSheet.Column(18).Style.Fill.PatternType = ExcelFillStyle.Solid; //Status
                workSheet.Column(18).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee")); //Status
                workSheet.Column(27).Style.Fill.PatternType = ExcelFillStyle.Solid;  //ManagmentStatus
                workSheet.Column(27).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //ManagmentStatus

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=querySearch.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
            }
            else
            {

                var fullResult = _member.GetAllQueries();

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells[1, 1].LoadFromCollection(fullResult, true);

                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.DeleteColumn(2); //Medical scheme Status
                workSheet.DeleteColumn(5); //Full name (UPPERCASE)
                //workSheet.DeleteColumn(10); //Query Effective date
                workSheet.DeleteColumn(26); //Dependant ID

                workSheet.Column(11).Style.Numberformat.Format = "dd-mm-yyyy";  //Column K - Effective date
                workSheet.Column(16).Style.Numberformat.Format = "dd-mm-yyyy";  //Column P - Created date
                workSheet.Column(22).Style.Numberformat.Format = "dd-mm-yyyy";  //Column V - Modified date
                workSheet.Column(24).Style.Numberformat.Format = "dd-mm-yyyy";  //Column X - Resolution date
                //workSheet.Column(26).Style.Numberformat.Format = "hh:mm";  //Column Z - Query Age

                //column-width
                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();
                workSheet.Column(8).Width = 30;
                workSheet.Column(9).AutoFit();
                workSheet.Column(10).Width = 30;
                workSheet.Column(11).Width = 30;
                workSheet.Column(12).Width = 20;
                workSheet.Column(13).Width = 20;
                workSheet.Column(14).AutoFit();
                workSheet.Column(15).AutoFit();
                workSheet.Column(16).Width = 15;
                workSheet.Column(17).AutoFit();
                workSheet.Column(18).AutoFit();
                workSheet.Column(19).AutoFit();
                workSheet.Column(20).Width = 15;
                workSheet.Column(21).AutoFit();
                workSheet.Column(22).AutoFit();
                workSheet.Column(23).Width = 30;
                workSheet.Column(24).AutoFit();
                workSheet.Column(25).Width = 30;
                workSheet.Column(26).AutoFit();
                workSheet.Column(27).AutoFit();
                workSheet.Column(28).Width = 30;


                //column-style
                workSheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MedicalAid
                workSheet.Column(1).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MedicalAid
                workSheet.Column(2).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MemberNumber
                workSheet.Column(2).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MemberNumber
                workSheet.Column(8).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescriptionCode
                workSheet.Column(8).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescriptionCode
                workSheet.Column(9).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescription
                workSheet.Column(9).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescription
                workSheet.Column(17).Style.Fill.PatternType = ExcelFillStyle.Solid;  //Priority
                workSheet.Column(17).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //Priority
                workSheet.Column(18).Style.Fill.PatternType = ExcelFillStyle.Solid; //Status
                workSheet.Column(18).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee")); //Status
                workSheet.Column(27).Style.Fill.PatternType = ExcelFillStyle.Solid;  //ManagmentStatus
                workSheet.Column(27).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //ManagmentStatus

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=querySearch.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
            }
        }


        public ActionResult ListAssignments()
        {
            var model = _member.GetAllActiveAssignments();
            return View(model);
        }

        public ActionResult EditQuery(int id)
        {
            var model = _member.GetQueryById(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult EditQuery(Queries model)
        {
            var results = _member.UpdateQuery(model);
            ViewBag.queryType = new SelectList(_member.GetQueryTypes(), "queryTypes", "queryDescription");
            return View("SearchQueries");
        }

        public ActionResult QueryDetail(int id)
        {
            var model = _member.GetQueryById(id);
            return View(model);
        }

        public ActionResult RecentEnrol()
        {
            var model = _member.GetRecentEnrollments();
            return View(model);
        }

        public ActionResult searchManagement()
        {
            var user = User.Identity.Name;
            var userinformation = _admin.GetUserByUsername(user);
            ViewBag.UserSearchManagment = userinformation.SearchManagement;

            return View();
        }

        //------------------------------------------------------------------------------------ production-report  ----------------------------------------------------------------------------------->
        public ActionResult ProductionView()
        {
            //var viewModel = new ProductionSearchView();
            //viewModel.assignmentItemTaskTypes = _member.GetAssignmentTaskTypes();

            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "Name", "Name");
            ViewBag.users = new SelectList(_admin.GetUsers().Where(x => x.Active == true), "username", "userFullName");
            ViewBag.assignmentItems = new SelectList(_member.GetAssignmentTaskTypes(), "taskDescription", "taskDescription");

            return View();
        }
        [HttpPost]
        public ActionResult ProductionView(List<ProductionSearchView> model)
        {
            var medicalaid = Request.Query["medicalAidID"].ToString();
            var users = Request.Query["users"].ToString().ToLower().ToString();
            var filter = Request.Query["assignmentItems"].ToString();
            var fromDate = "";
            var toDate = "";

            if (!String.IsNullOrEmpty(Request.Query["dateFrom"]))
            {
                fromDate = Convert.ToDateTime(Request.Query["dateFrom"]).ToString();
            }
            else
            {
                fromDate = "01 Jan 2019 00:00:00";
            }
            if (!String.IsNullOrEmpty(Request.Query["dateTo"]))
            {
                toDate = Convert.ToDateTime(Request.Query["dateTo"]).ToString();
            }
            else
            {
                toDate = DateTime.Now.ToString();
            }

            //var assignmentItems = Request.Query["assignmentItems"].ToString();

            model = _member.GetProductionSearchResults(medicalaid, users, fromDate, toDate, filter);

            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "Name", "Name");
            ViewBag.users = new SelectList(_admin.GetUsers().Where(x => x.Active == true), "username", "userFullName");
            ViewBag.assignmentItems = new SelectList(_member.GetAssignmentTaskTypes(), "taskDescription", "taskDescription");

            return View(model);

        }
        [HttpPost]
        public ActionResult ExportProductionViewToExcel(List<ProductionSearchView> model)
        {
            if (Request.Query["medicalAidID"].ToString() != null || Request.Query["users"].ToString() != null || Request.Query["assignmentItems"].ToString() != null)
            {
                var medicalaid = Request.Query["medicalAidID"].ToString();
                var users = Request.Query["users"].ToString().ToLower().ToString();
                var filter = Request.Query["assignmentItems"].ToString();
                var fromDate = "";
                var toDate = "";

                if (!String.IsNullOrEmpty(Request.Query["dateFrom"]))
                {
                    fromDate = Convert.ToDateTime(Request.Query["dateFrom"]).ToString();
                }
                else
                {
                    fromDate = "01 Jan 2019 00:00:00";
                }
                if (!String.IsNullOrEmpty(Request.Query["dateTo"]))
                {
                    toDate = Convert.ToDateTime(Request.Query["dateTo"]).ToString();
                }
                else
                {
                    toDate = DateTime.Now.ToString();
                }

                model = _member.GetProductionSearchResults(medicalaid, users, fromDate, toDate, filter);

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells[1, 1].LoadFromCollection(_member.GetProductionSearchResults(medicalaid, users, fromDate, toDate, filter), true);

                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.DeleteColumn(1); //Dependant ID

                workSheet.Column(3).Style.Numberformat.Format = "dd-mm-yyyy hh:mm:ss";  //Column C - Effective date
                workSheet.Column(4).Style.Numberformat.Format = "dd-mm-yyyy hh:mm:ss";  //Column D - Created date
                workSheet.Column(6).Style.Numberformat.Format = "dd-mm-yyyy hh:mm:ss";  //Column F - Modified date

                //column-width
                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();
                workSheet.Column(8).Width = 30;
                workSheet.Column(9).AutoFit();
                workSheet.Column(10).Width = 30;
                workSheet.Column(11).Width = 30;
                workSheet.Column(12).Width = 20;
                workSheet.Column(13).Width = 20;
                workSheet.Column(14).AutoFit();
                workSheet.Column(15).AutoFit();
                workSheet.Column(16).Width = 15;
                workSheet.Column(17).AutoFit();
                workSheet.Column(18).AutoFit();
                workSheet.Column(19).AutoFit();
                workSheet.Column(20).Width = 15;
                workSheet.Column(21).AutoFit();
                workSheet.Column(22).AutoFit();
                workSheet.Column(23).Width = 30;
                workSheet.Column(24).AutoFit();
                workSheet.Column(25).Width = 30;
                workSheet.Column(26).AutoFit();
                workSheet.Column(27).AutoFit();
                workSheet.Column(28).Width = 30;

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=ProductionReport.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
            }
            else
            {

                var fullResult = _member.GetAllQueries();

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells[1, 1].LoadFromCollection(fullResult, true);

                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.DeleteColumn(2); //Medical scheme Status
                workSheet.DeleteColumn(5); //Full name (UPPERCASE)
                //workSheet.DeleteColumn(10); //Query Effective date
                workSheet.DeleteColumn(26); //Dependant ID

                workSheet.Column(11).Style.Numberformat.Format = "dd-mm-yyyy";  //Column K - Effective date
                workSheet.Column(16).Style.Numberformat.Format = "dd-mm-yyyy";  //Column P - Created date
                workSheet.Column(22).Style.Numberformat.Format = "dd-mm-yyyy";  //Column V - Modified date
                workSheet.Column(24).Style.Numberformat.Format = "dd-mm-yyyy";  //Column X - Resolution date
                //workSheet.Column(26).Style.Numberformat.Format = "hh:mm";  //Column Z - Query Age

                //column-width
                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();
                workSheet.Column(8).Width = 30;
                workSheet.Column(9).AutoFit();
                workSheet.Column(10).Width = 30;
                workSheet.Column(11).Width = 30;
                workSheet.Column(12).Width = 20;
                workSheet.Column(13).Width = 20;
                workSheet.Column(14).AutoFit();
                workSheet.Column(15).AutoFit();
                workSheet.Column(16).Width = 15;
                workSheet.Column(17).AutoFit();
                workSheet.Column(18).AutoFit();
                workSheet.Column(19).AutoFit();
                workSheet.Column(20).Width = 15;
                workSheet.Column(21).AutoFit();
                workSheet.Column(22).AutoFit();
                workSheet.Column(23).Width = 30;
                workSheet.Column(24).AutoFit();
                workSheet.Column(25).Width = 30;
                workSheet.Column(26).AutoFit();
                workSheet.Column(27).AutoFit();
                workSheet.Column(28).Width = 30;


                //column-style
                workSheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MedicalAid
                workSheet.Column(1).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MedicalAid
                workSheet.Column(2).Style.Fill.PatternType = ExcelFillStyle.Solid;  //MemberNumber
                workSheet.Column(2).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //MemberNumber
                workSheet.Column(8).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescriptionCode
                workSheet.Column(8).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescriptionCode
                workSheet.Column(9).Style.Fill.PatternType = ExcelFillStyle.Solid;  //QueryDescription
                workSheet.Column(9).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //QueryDescription
                workSheet.Column(17).Style.Fill.PatternType = ExcelFillStyle.Solid;  //Priority
                workSheet.Column(17).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //Priority
                workSheet.Column(18).Style.Fill.PatternType = ExcelFillStyle.Solid; //Status
                workSheet.Column(18).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee")); //Status
                workSheet.Column(27).Style.Fill.PatternType = ExcelFillStyle.Solid;  //ManagmentStatus
                workSheet.Column(27).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#eeeeee"));  //ManagmentStatus

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=querySearch.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
            }
        }

        //------------------------------------------------------------------------------------ pathology-report  ----------------------------------------------------------------------------------->
        public ActionResult PathologySearch() //HCare-815
        {
            var model = new PathologySearchVM();
            model.PathologyFields = _member.GetPathologyFields();
            model.PathologySearches = new List<PathologySearch>();

            model.PathologyFieldsList = _member.GetPathologyFields().Where(x => x.active == true).ToList();
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();

            return View(model);

        }
        [HttpPost]
        public ActionResult PathologySearch(PathologySearchVM model) //HCare-815
        {
            var pathField = _member.GetPathologyFields();
            var displayFields = model.SelectedPathologyFields;
            ViewBag.displayFields = displayFields;

            #region form-dropdowns
            model.PathologyFieldsList = _member.GetPathologyFields().Where(x => x.active == true).ToList();
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var pathologyfield = String.Empty;

            //var medicalaid = String.Join(",", model.SelectedMedicalAids);
            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (model.SelectedPathologyFields != null) { pathologyfield = String.Join(",", model.SelectedPathologyFields); } else { model.SelectedPathologyFields = new string[] { }; }

            #region date-variables
            var fromDate = "";
            var toDate = "";

            if (!String.IsNullOrEmpty(Request.Query["FromDate"]))
            {
                fromDate = Convert.ToDateTime(Request.Query["FromDate"]).ToString("dd-MMM-yyyy 00:00:00"); //HCare-1129
            }
            else
            {
                //fromDate = "01 Jan 2018 00:00:00";
                fromDate = new DateTime(2018, 1, 1).ToString("dd-MMM-yyyy 00:00:00"); //HCare-1129
            }
            if (!String.IsNullOrEmpty(Request.Query["ToDate"]))
            {
                toDate = Convert.ToDateTime(Request.Query["ToDate"]).ToString("dd-MMM-yyyy 23:59:59"); //HCare-1129
            }
            else
            {
                toDate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59"); //HCare-1129
            }
            #endregion
            #endregion

            model.PathologySearches = _member.GetPathologySearchResults(medicalaid, program, pathologyfield, fromDate, toDate);
            model.PathologyFields = pathField.Where(c => displayFields.Contains(c.DisplayName)).ToList();

            #region hidden-fields
            TempData["pathologyfields"] = pathologyfield;
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["fromdate"] = fromDate;
            TempData["todate"] = toDate;
            #endregion

            return View(model);
        }

        public ActionResult ExportPathologyToExcel(PathologySearchVM model) //HCare-815
        {
            if (model.SelectedMedicalAids != null || model.SelectedPrograms != null || model.SelectedPathologyFields != null)
            {
                var pathologyfields = Request.Query["pathologyfields"].ToString();
                var medicalaids = Request.Query["medicalaids"].ToString();
                var programs = Request.Query["programs"].ToString();
                var fromdate = Request.Query["fromdate"].ToString();
                var todate = Request.Query["todate"].ToString();

                string[] pathField = pathologyfields.Split(',');
                var pf = pathField.ToList();

                var pfields = _member.GetPathologyFields();

                model.PathologySearches = _member.GetPathologySearchResults(medicalaids, programs, pathologyfields, fromdate, todate);
                model.PathologyFields = pfields.Where(c => pf.Contains(c.DisplayName)).ToList();

                #region data-table
                DataTable table = new DataTable();
                table.Columns.Add("Medical scheme");
                table.Columns.Add("Member#");
                table.Columns.Add("Dependant code");
                table.Columns.Add("ID/Auth");
                table.Columns.Add("First name");
                table.Columns.Add("Last name");
                table.Columns.Add("Pathology lab");
                table.Columns.Add("Created by");
                table.Columns.Add("Created date");
                foreach (var item in pf)
                {
                    table.Columns.Add(item.ToString());
                    table.Columns.Add(item.ToString() + " EffectiveDate");
                }
                foreach (var line in model.PathologySearches)
                {
                    DataRow row = table.NewRow();

                    //Map all the values in the columns
                    row["Medical scheme"] = line.MedicalAidScheme;
                    row["Member#"] = line.MembershipNumber;
                    row["Dependant code"] = line.DependantCode;
                    row["ID/Auth"] = line.IDNumber;
                    row["First name"] = line.FirstName;
                    row["Last name"] = line.LastName;
                    row["Pathology lab"] = line.PathologyLab;
                    row["Created by"] = line.CreatedBy;
                    row["Created date"] = line.EffectiveDate.ToString("dd-MM-yyyy");
                    foreach (var item in model.PathologyFields)
                    {
                        row[item.DisplayName] = line.GetType().GetProperty(item.DisplayName.Replace(" ", "")).GetValue(line);
                        if (line.GetType().GetProperty(item.DisplayName.Replace(" ", "")).GetValue(line) != null)
                        {
                            row[item.DisplayName + " EffectiveDate"] = Convert.ToDateTime(line.GetType().GetProperty(item.DisplayName.Replace(" ", "") + "EffectiveDate").GetValue(line)).ToString("dd-MM-yyyy");
                        }
                    }
                    table.Rows.Add(row);
                }

                DataView view = new DataView(table);

                #endregion
                #region excel
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells["A1"].LoadFromDataTable(table, true);
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();


                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=pathology-search.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
                #endregion
            }
            else
            {

                var fullResult = _member.GetActivePathologyResults();

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells[1, 1].LoadFromCollection(fullResult, true);

                workSheet.Row(1).Style.Font.Bold = true;

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=pathology-search.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
            }
        }

        #region hcare-1119 Communication log search 

        public ActionResult CommunicationLogSearch()
        {
            List<SelectListItem> typesOfCommunication = new List<SelectListItem>();
            typesOfCommunication.Add(new SelectListItem() { Text = "Email", Value = "Email" });
            typesOfCommunication.Add(new SelectListItem() { Text = "SMS", Value = "SMS" });
            typesOfCommunication.Add(new SelectListItem() { Text = "Enquiries", Value = "Enquiries" });
            typesOfCommunication.Add(new SelectListItem() { Text = "Attachments", Value = "Attachments" });
            typesOfCommunication.Add(new SelectListItem() { Text = "Diabetic diary", Value = "Diabetic diary" });
            typesOfCommunication.Add(new SelectListItem() { Text = "Notes", Value = "Notes" });

            ViewBag.Value = new SelectList(typesOfCommunication, "Value", "Text");
            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids().Where(x => x.Active == true), "medicalAidCode", "Name");


            return View();
        }

        [HttpPost]
        public ActionResult CommunicationLogSearch(CommunicationLogVM model)
        {
            if (Request.Query["medicalAidID"].ToString() != null) { model.Scheme = Request.Query["medicalAidID"].ToString(); }
            if (Request.Query["medicalAidID"].ToString() != null) { model.TypeOfCommunication = Request.Query["Value"].ToString(); }
            model.dateTo = Convert.ToDateTime(model.dateTo.ToString("yyyy-MM-dd 23:59:59"));       

            var results = new CommunicationLogResultsVM();
            results.communicationLogVMs = _member.getAllCommunicationLog(model);
            results.CommunicationLogVM = model;

            return View("CommunicationLogSearchResults", results);
        }
        public ActionResult CommunicationLogSearchResults(CommunicationLogResultsVM model)
        {
            return View(model);
        }
        public ActionResult ExportCommunicationLog(CommunicationLogResultsVM model)
        {
            if (model.CommunicationLogVM.Scheme == null)
                model.CommunicationLogVM.Scheme = "";


            model.communicationLogVMs = _member.getAllCommunicationLog(model.CommunicationLogVM);


            ExcelPackage excel = new ExcelPackage();
            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = "Member Number";
            workSheet.Cells["B1"].Value = "Profile Number";
            workSheet.Cells["C1"].Value = "Depandant Code";
            workSheet.Cells["D1"].Value = "Account";
            workSheet.Cells["E1"].Value = "Type Of Communication";
            workSheet.Cells["F1"].Value = "Description";
            workSheet.Cells["G1"].Value = "Detail";
            workSheet.Cells["H1"].Value = "Date Sent";
            workSheet.Cells["I1"].Value = "Communication Sent To";
            int row = 2;
            foreach (var item in model.communicationLogVMs)
            {

                workSheet.Cells[string.Format("A{0}", row)].Value = item.MemberNumber;
                workSheet.Cells[string.Format("B{0}", row)].Value = item.ProfileNumber;
                workSheet.Cells[string.Format("C{0}", row)].Value = item.DepandantCode;
                workSheet.Cells[string.Format("D{0}", row)].Value = item.Scheme;
                workSheet.Cells[string.Format("E{0}", row)].Value = item.TypeOfCommunication;
                workSheet.Cells[string.Format("F{0}", row)].Value = item.description;
                if (item.Detail != null)
                { workSheet.Cells[string.Format("G{0}", row)].Value = Regex.Replace(item.Detail, @"<[^>]+>|&nbsp;", "").Trim(); }
                else { workSheet.Cells[string.Format("G{0}", row)].Value = item.Detail; }
                workSheet.Cells[string.Format("H{0}", row)].Value = item.DateSent.ToString("dd-MM-yyyy H:mm"); ;
                workSheet.Cells[string.Format("I{0}", row)].Value = item.CommunicationSentTo;
                row++;
            }


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=CommunicationLogSearch.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);
            }


        }

        #endregion

        //------------------------------------------------------------------------------------ risk-rating-report  ----------------------------------------------------------------------------------->
        #region risk-search hcare-1138
        public ActionResult RiskRatingSearch()
        {
            var model = new RiskSearchVM();
            model.RiskSearches = new List<RiskSearch>();

            model.filter = true;

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();
            model.RiskRating = _member.GetRiskRatingTypes().Where(x => x.Active == true).OrderBy(x => x.RiskPriority).ToList();

            return View(model);

        }

        [HttpPost]
        public ActionResult RiskRatingSearch(RiskSearchVM model)
        {
            #region form-dropdowns
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetPrograms().Where(x => x.Active == true).ToList();
            model.RiskRating = _member.GetRiskRatingTypes().Where(x => x.Active == true).ToList();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var riskrating = String.Empty;

            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (model.SelectedRiskRating != null) { riskrating = String.Join(",", model.SelectedRiskRating); } else { model.SelectedRiskRating = new string[] { }; }

            var programCode = _member.GetPrograms().Where(x => x.programID.ToString() == program).Select(x => x.code).FirstOrDefault();
            program = programCode;

            #region date-variables
            var fromDate = "";
            var toDate = "";

            if (!String.IsNullOrEmpty(Request.Query["FromDate"]))
            {
                fromDate = Convert.ToDateTime(Request.Query["FromDate"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                fromDate = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00");
            }
            if (!String.IsNullOrEmpty(Request.Query["ToDate"]))
            {
                toDate = Convert.ToDateTime(Request.Query["ToDate"]).ToString("dd-MMM-yyyy 23:59:59");
            }
            else
            {
                toDate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59");
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["riskrating"] = riskrating;
            TempData["fromdate"] = fromDate;
            TempData["todate"] = toDate;
            #endregion

            model.filter = false;
            model.RiskSearches = _member.GetRiskRatingSearchResults(medicalaid, program, riskrating, fromDate, toDate);

            return View(model);
        }

        public ActionResult RiskRatingToExcel(RiskSearchVM model)
        {
            var medicalaid = Request.Query["medicalaids"].ToString();
            var program = Request.Query["programs"].ToString();
            var riskrating = Request.Query["riskrating"].ToString();
            var fromdate = Request.Query["fromdate"].ToString();
            var todate = Request.Query["todate"].ToString();

            model.RiskSearches = _member.GetRiskRatingSearchResults(medicalaid, program, riskrating, fromdate, todate);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Scheme");
            table.Columns.Add("Program");
            table.Columns.Add("ICD-10");
            table.Columns.Add("Patient name");
            table.Columns.Add("ID/Auth");
            table.Columns.Add("Member number");
            table.Columns.Add("Dep code");
            table.Columns.Add("Birth date");
            table.Columns.Add("Effective date");
            table.Columns.Add("Gender");
            table.Columns.Add(">=Age65");
            table.Columns.Add("Management status");
            table.Columns.Add("Risk rating");
            table.Columns.Add("Hypo risk");
            table.Columns.Add("Diabetic retinopathy");
            table.Columns.Add("Cardiomyopathy");
            table.Columns.Add("CAD");
            table.Columns.Add("CCF");
            table.Columns.Add("Dysrhythmia");
            table.Columns.Add("Chronic renal failure");
            table.Columns.Add("Amputation");
            table.Columns.Add("Hyperlipidaemia");
            table.Columns.Add("Hypertension");
            table.Columns.Add("Hypoglycaemic unaware");
            table.Columns.Add("Diabetic diary");
            table.Columns.Add("Smoker");
            table.Columns.Add("Tuberculosis");
            table.Columns.Add("Cancer");
            table.Columns.Add("eGFR");
            table.Columns.Add("eGFR effective");
            table.Columns.Add("HbA1c");
            table.Columns.Add("HbA1c effective");
            table.Columns.Add("Viral load");
            table.Columns.Add("Viral load effective");
            table.Columns.Add("CD4 count");
            table.Columns.Add("CD4 count effective");
            table.Columns.Add("CD4 percentage");
            table.Columns.Add("CD4 percentage effective");

            foreach (var line in model.RiskSearches)
            {
                DataRow row = table.NewRow();
                row["Scheme"] = line.MedicalScheme;
                row["Program"] = line.Program;
                row["ICD-10"] = line.ICD10;
                row["Patient name"] = line.PatientName;
                row["ID/Auth"] = line.IDNumber;
                row["Member number"] = line.MembershipNumber;
                row["Dep code"] = line.DependantCode;
                if (!String.IsNullOrEmpty(line.BirthDate.ToString())) { row["Birth date"] = line.BirthDate.Value.ToString("dd-MM-yyyy"); } else { row["Birth date"] = "NULL"; }
                row["Gender"] = line.Gender;
                row[">=Age65"] = line.Age65;
                row["Management status"] = line.ManagementStatus;
                if (!String.IsNullOrEmpty(line.EffectiveDate.ToString())) { row["Effective date"] = line.EffectiveDate.Value.ToString("dd-MM-yyyy"); } else { row["Effective date"] = "NULL"; }
                row["Risk rating"] = line.RiskRating;
                row["Hypo risk"] = line.HypoRisk;
                row["Diabetic retinopathy"] = line.DiabeticRetinopathy;
                row["Cardiomyopathy"] = line.Cardiomyopathy;
                row["CAD"] = line.CAD;
                row["CCF"] = line.CCF;
                row["Dysrhythmia"] = line.Dysrhythmia;
                row["Chronic renal failure"] = line.ChronicRenalFailure;
                row["Amputation"] = line.Amputation;
                row["Hyperlipidaemia"] = line.Hyperlipidaemia;
                row["Hypertension"] = line.Hypertension;
                row["Hypoglycaemic unaware"] = line.HypoglycaemicUnaware;
                row["Diabetic diary"] = line.DiabeticDiary;
                row["Smoker"] = line.Smoker;
                row["Tuberculosis"] = line.Tuberculosis;
                row["Cancer"] = line.Cancer;
                row["eGFR"] = line.eGFR;
                row["eGFR effective"] = line.eGFREffectiveDate;
                row["HbA1c"] = line.HbA1c;
                row["HbA1c effective"] = line.HbA1cEffectiveDate;
                row["Viral load"] = line.ViralLoad;
                row["Viral load effective"] = line.ViralLoadEffectiveDate;
                row["CD4 count"] = line.CD4Count;
                row["CD4 count effective"] = line.CD4CountEffectiveDate;
                row["CD4 percentage"] = line.CD4Percentage;
                row["CD4 percentage effective"] = line.CD4PercentageEffectiveDate;

                table.Rows.Add(row);
            }

            DataView view = new DataView(table);

            #endregion
            #region excel
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Cells["A1"].LoadFromDataTable(table, true);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=risk-rating-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }

        #endregion

        //------------------------------------------------------------------------------------ management-status-report  ----------------------------------------------------------------------------------->
        #region management-status-search hcare-1267
        public ActionResult ManagementStatusSearch()
        {
            var model = new ManagementStatusSearchVM();
            model.ManagementStatusSearches = new List<ManagementStatusSearch>();

            model.filter = true;

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();

            return View(model);

        }

        [HttpPost]
        public ActionResult ManagementStatusSearch(ManagementStatusSearchVM model)
        {
            #region form-dropdowns
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetPrograms().Where(x => x.Active == true).ToList();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var managementstatus_current = String.Empty;
            var managementstatus_previous = String.Empty;

            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (!String.IsNullOrEmpty(Request.Query["management-status-current"])) { managementstatus_current = String.Join(",", Request.Query["management-status-current"]); }
            if (!String.IsNullOrEmpty(Request.Query["management-status-previous"])) { managementstatus_previous = String.Join(",", Request.Query["management-status-previous"]); }

            var programCode = _member.GetPrograms().Where(x => x.programID.ToString() == program).Select(x => x.code).FirstOrDefault();
            program = programCode;

            #region date-variables
            var fromdate_current = "";
            var todate_current = "";

            if (!String.IsNullOrEmpty(Request.Query["from-date-current"]))
            {
                fromdate_current = Convert.ToDateTime(Request.Query["from-date-current"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                fromdate_current = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00");
            }
            if (!String.IsNullOrEmpty(Request.Query["to-date-current"]))
            {
                todate_current = Convert.ToDateTime(Request.Query["to-date-current"]).ToString("dd-MMM-yyyy 23:59:59");
            }
            else
            {
                todate_current = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59");
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["managementstatus_current"] = managementstatus_current;
            TempData["managementstatus_previous"] = managementstatus_previous;
            TempData["fromdate_current"] = fromdate_current;
            TempData["todate_current"] = todate_current;
            #endregion

            model.filter = false;
            model.ManagementStatusSearches = _member.GetManagementStatusSearchResults(medicalaid, program, managementstatus_current, fromdate_current, todate_current, managementstatus_previous);

            return View(model);
        }

        public ActionResult ManagementStatusToExcel(ManagementStatusSearchVM model)
        {
            var medicalaid = Request.Query["medicalaids"].ToString();
            var program = Request.Query["programs"].ToString();
            var managementstatus_current = Request.Query["managementstatus_current"].ToString();
            var managementstatus_previous = Request.Query["managementstatus_previous"].ToString();
            var fromdate_current = Request.Query["fromdate_current"].ToString();
            var todate_current = Request.Query["todate_current"].ToString();

            model.ManagementStatusSearches = _member.GetManagementStatusSearchResults(medicalaid, program, managementstatus_current, fromdate_current, todate_current, managementstatus_previous);

            if (!String.IsNullOrEmpty(managementstatus_previous))
            {
                #region data-table-both
                DataTable table2 = new DataTable();
                table2.Columns.Add("Scheme");
                table2.Columns.Add("Program");
                table2.Columns.Add("Program start date");
                table2.Columns.Add("Patient name");
                table2.Columns.Add("ID/Auth #");
                table2.Columns.Add("Member number");
                table2.Columns.Add("Dep code");
                table2.Columns.Add("Previous management status");
                table2.Columns.Add("Previous MS start date");
                table2.Columns.Add("Previous MS end date");
                table2.Columns.Add("Current management status");
                table2.Columns.Add("Current MS start date");
                table2.Columns.Add("Current MS end date");

                foreach (var line in model.ManagementStatusSearches)
                {
                    DataRow row = table2.NewRow();
                    row["Scheme"] = line.MedicalScheme;
                    row["Program"] = line.Program;
                    row["Program start date"] = line.ProgramStartDate.Value.ToString("dd-MM-yyyy"); //hcare-1267-correction
                    row["Patient name"] = line.PatientName;
                    row["ID/Auth #"] = line.IDNumber;
                    row["Member number"] = line.MembershipNumber;
                    row["Dep code"] = line.DependantCode;
                    row["Previous management status"] = line.ManagementStatusPrevious;
                    if (!String.IsNullOrEmpty(line.ManagementStatusPrevious_StartDate.ToString())) { row["Previous MS start date"] = line.ManagementStatusPrevious_StartDate.Value.ToString("dd-MM-yyyy"); }
                    if (!String.IsNullOrEmpty(line.ManagementStatusPrevious_EndDate.ToString())) { row["Previous MS end date"] = line.ManagementStatusPrevious_EndDate.Value.ToString("dd-MM-yyyy"); }
                    row["Current management status"] = line.ManagementStatusCurrent;
                    if (!String.IsNullOrEmpty(line.ManagementStatusCurrent_StartDate.ToString())) { row["Current MS start date"] = line.ManagementStatusCurrent_StartDate.Value.ToString("dd-MM-yyyy"); }
                    if (!String.IsNullOrEmpty(line.ManagementStatusCurrent_EndDate.ToString())) { row["Current MS end date"] = line.ManagementStatusCurrent_EndDate.Value.ToString("dd-MM-yyyy"); }

                    table2.Rows.Add(row);
                }

                DataView view = new DataView(table2);

                #endregion
                #region excel
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells["A1"].LoadFromDataTable(table2, true);
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=management-status-search.xlsx"); //hcare-1267-correction
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
                #endregion
            }
            else
            {
                #region data-table-both
                DataTable table1 = new DataTable();
                table1.Columns.Add("Scheme");
                table1.Columns.Add("Program");
                table1.Columns.Add("Program start date");
                table1.Columns.Add("Patient name");
                table1.Columns.Add("Member number");
                table1.Columns.Add("Dep code");
                table1.Columns.Add("Current management status");
                table1.Columns.Add("Current MS start date");
                table1.Columns.Add("Current MS end date");

                foreach (var line in model.ManagementStatusSearches)
                {
                    DataRow row = table1.NewRow();
                    row["Scheme"] = line.MedicalScheme;
                    row["Program"] = line.Program;
                    row["Program start date"] = line.ProgramStartDate.Value.ToString("dd-MM-yyyy"); //hcare-1267-correction
                    row["Patient name"] = line.PatientName;
                    row["Member number"] = line.MembershipNumber;
                    row["Dep code"] = line.DependantCode;
                    row["Current management status"] = line.ManagementStatusCurrent;
                    if (!String.IsNullOrEmpty(line.ManagementStatusCurrent_StartDate.ToString())) { row["Current MS start date"] = line.ManagementStatusCurrent_StartDate.Value.ToString("dd-MM-yyyy"); }
                    if (!String.IsNullOrEmpty(line.ManagementStatusCurrent_EndDate.ToString())) { row["Current MS end date"] = line.ManagementStatusCurrent_EndDate.Value.ToString("dd-MM-yyyy"); }

                    table1.Rows.Add(row);
                }

                DataView view = new DataView(table1);

                #endregion
                #region excel
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Results");
                workSheet.Cells["A1"].LoadFromDataTable(table1, true);
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("content-disposition", "attachment;  filename=management-status-search.xlsx"); //hcare-1267-correction
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.Body);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
                    return View(model);

                }
                #endregion
            }
        }

        #endregion

        //------------------------------------------------------------------------------------ production-report  ----------------------------------------------------------------------------------->
        #region production-search hcare-1289
        public ActionResult ProductionSearch()
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);
            if (user.SearchManagement == true)
            {
                var model = new ProductionSearchVM();
                model.ProductionSearches = new List<ProductionResultsVM>();

                model.filter = true;

                model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
                model.Programs = _member.GetAllowedProgramsPerUserList(user.userID).ToList();
                model.Users = _admin.GetUsers().Where(x => x.Active == true).ToList();
                model.WorkItems = _member.GetProductionWorkItems();

                return View(model);
            }
            else

                return RedirectToAction("searchManagement", "PatientManagement", null);

        }
        [HttpPost]
        public ActionResult ProductionSearch(ProductionSearchVM model)
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);

            #region form-dropdowns
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedProgramsPerUserList(user.userID).ToList();
            model.Users = _admin.GetUsers().Where(x => x.Active == true).ToList();
            model.WorkItems = _member.GetProductionWorkItems();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var users = String.Empty;
            var workitems = String.Empty;

            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (model.SelectedUsers != null) { users = String.Join(",", model.SelectedUsers); } else { model.SelectedUsers = new string[] { }; }
            if (model.SelectedWorkItems != null) { workitems = String.Join(",", model.SelectedWorkItems); } else { model.SelectedWorkItems = new string[] { }; }

            #region date-variables
            var fromdate = "";
            var todate = "";

            if (!String.IsNullOrEmpty(Request.Query["from-date"]))
            {
                fromdate = Convert.ToDateTime(Request.Query["from-date"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                fromdate = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00");
            }
            if (!String.IsNullOrEmpty(Request.Query["to-date"]))
            {
                todate = Convert.ToDateTime(Request.Query["to-date"]).ToString("dd-MMM-yyyy 23:59:59");
            }
            else
            {
                todate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59");
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["users"] = users;
            TempData["workitems"] = workitems;
            TempData["fromdate"] = fromdate;
            TempData["todate"] = todate;
            #endregion

            model.filter = false;
            model.ProductionSearches = _member.GetProductionSearchResults(medicalaid, program, users, workitems, fromdate, todate);

            return View(model);
        }
        public ActionResult ProductionToExcel(ProductionSearchVM model)
        {
            var medicalaid = Request.Query["medicalaids"].ToString();
            var program = Request.Query["programs"].ToString();
            var users = Request.Query["users"].ToString();
            var workitems = Request.Query["workitems"].ToString();
            var fromdate = Request.Query["fromdate"].ToString();
            var todate = Request.Query["todate"].ToString();

            model.ProductionSearches = _member.GetProductionSearchResults(medicalaid, program, users, workitems, fromdate, todate);

            #region data-table-both
            DataTable table = new DataTable();
            table.Columns.Add("Scheme");
            table.Columns.Add("Program");
            table.Columns.Add("User name");
            table.Columns.Add("Created date");
            table.Columns.Add("Work item");
            table.Columns.Add("Date from");
            table.Columns.Add("Date to");
            table.Columns.Add("ID/Auth");
            table.Columns.Add("Member number");
            table.Columns.Add("Dependant code");
            table.Columns.Add("Member name");

            foreach (var line in model.ProductionSearches)
            {
                DataRow row = table.NewRow();
                row["Scheme"] = line.MedicalScheme;
                row["Program"] = line.Program;
                row["User name"] = line.UserName;
                if (!String.IsNullOrEmpty(line.CreatedDate.ToString())) { row["Created date"] = line.CreatedDate.Value.ToString("dd-MM-yyyy hh:mm:ss"); }
                row["Work item"] = line.WorkItem;
                if (!String.IsNullOrEmpty(line.DateFrom.ToString())) { row["Date from"] = line.DateFrom.Value.ToString("dd-MM-yyyy"); }
                if (!String.IsNullOrEmpty(line.DateTo.ToString())) { row["Date to"] = line.DateTo.Value.ToString("dd-MM-yyyy"); }
                row["ID/Auth"] = line.MemberID;
                row["Member number"] = line.MemberNumber;
                row["Dependant code"] = line.DependantCode;
                row["Member name"] = line.MemberName;

                table.Rows.Add(row);
            }

            DataView view = new DataView(table);

            #endregion
            #region excel
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Cells["A1"].LoadFromDataTable(table, true);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=production-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion

        }

        #endregion

        //------------------------------------------------------------------------------------- enquiry-report  ------------------------------------------------------------------------------------->
        #region enquiry-search hcare-874
        public ActionResult EnquirySearch()
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);

            var model = new EnquirySearchVM();
            model.EnquirySearches = new List<EnquiryResultsVM>();

            model.filter = true;

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedProgramsPerUserList(user.userID).ToList();
            model.ManagementStatus = _member.GetManagementStatus();
            model.QueryPriorities = _member.GetPriorities();
            model.QueryTypes = _member.GetQueryTypes();

            return View(model);

        }
        [HttpPost]
        public ActionResult EnquirySearch(EnquirySearchVM model)
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);

            #region form-dropdowns
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedProgramsPerUserList(user.userID).ToList();
            model.ManagementStatus = _member.GetManagementStatus();
            model.QueryPriorities = _member.GetPriorities();
            model.QueryTypes = _member.GetQueryTypes();
            #endregion
            #region form-variables
            var medicalaid = String.Empty;
            var program = String.Empty;
            var managmentstatus = String.Empty;
            var querypriorities = String.Empty;
            var querytypes = String.Empty;

            if (model.SelectedMedicalAids != null) { medicalaid = String.Join(",", model.SelectedMedicalAids); } else { model.SelectedMedicalAids = new string[] { }; }
            if (model.SelectedPrograms != null) { program = String.Join(",", model.SelectedPrograms); } else { model.SelectedPrograms = new string[] { }; }
            if (model.SelectedManagementStatus != null) { managmentstatus = String.Join(",", model.SelectedManagementStatus); } else { model.SelectedManagementStatus = new string[] { "" }; }
            if (model.SelectedQueryTypes != null) { querytypes = String.Join(",", model.SelectedQueryTypes); } else { model.SelectedQueryTypes = new string[] { "" }; }
            if (model.SelectedQueryPriorities != null) { querypriorities = String.Join(",", model.SelectedQueryPriorities); } else { model.SelectedQueryPriorities = new string[] { "" }; }

            #region date-variables
            var fromdate = "";
            var todate = "";

            if (!String.IsNullOrEmpty(Request.Query["from-date"]))
            {
                fromdate = Convert.ToDateTime(Request.Query["from-date"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                fromdate = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00");
            }
            if (!String.IsNullOrEmpty(Request.Query["to-date"]))
            {
                todate = Convert.ToDateTime(Request.Query["to-date"]).ToString("dd-MMM-yyyy 23:59:59");
            }
            else
            {
                todate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59");
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["medicalaids"] = medicalaid;
            TempData["programs"] = program;
            TempData["managmentstatus"] = managmentstatus;
            TempData["querypriorities"] = querypriorities;
            TempData["querytypes"] = querytypes;
            TempData["fromdate"] = fromdate;
            TempData["todate"] = todate;
            #endregion

            model.filter = false;
            model.EnquirySearches = _member.GetEnquirySearchResults(medicalaid, program, managmentstatus, querypriorities, querytypes, fromdate, todate);

            return View(model);
        }
        public ActionResult EnquiryToExcel(EnquirySearchVM model)
        {
            var medicalaid = Request.Query["medicalaids"].ToString();
            var program = Request.Query["programs"].ToString();
            var managmentstatus = Request.Query["managmentstatus"].ToString();
            var querypriorities = Request.Query["querypriorities"].ToString();
            var querytypes = Request.Query["querytypes"].ToString();
            var fromdate = Request.Query["fromdate"].ToString();
            var todate = Request.Query["todate"].ToString();

            model.EnquirySearches = _member.GetEnquirySearchResults(medicalaid, program, managmentstatus, querypriorities, querytypes, fromdate, todate);

            #region data-table-both
            DataTable table = new DataTable();
            table.Columns.Add("Date range");
            table.Columns.Add("Scheme");
            table.Columns.Add("Program");
            table.Columns.Add("Patient name");
            table.Columns.Add("ID/Auth #");
            table.Columns.Add("Member number");
            table.Columns.Add("Dep code");
            table.Columns.Add("Enquiry source");
            table.Columns.Add("Enquiry type");
            table.Columns.Add("Enquiry priority");
            table.Columns.Add("Enquiry effective date");
            table.Columns.Add("Enquiry comment");
            table.Columns.Add("Follow up");

            foreach (var line in model.EnquirySearches)
            {
                DataRow row = table.NewRow();
                row["Date range"] = line.DateFrom.Value.ToString("dd/MM/yyyy") + " - " + line.DateTo.Value.ToString("dd/MM/yyyy");
                row["Scheme"] = line.MedicalScheme;
                if (line.Program != null) { row["Program"] = line.Program; } else { row["Program"] = "N/A"; }
                row["Patient name"] = line.MemberName;
                row["ID/Auth #"] = line.MemberID;
                row["Member number"] = line.MemberNumber;
                row["Dep code"] = line.DependantCode;
                row["Enquiry source"] = line.EnquirySource;
                row["Enquiry type"] = line.EnquiryType;
                row["Enquiry priority"] = line.EnquiryPriority;
                if (!String.IsNullOrEmpty(line.EnquiryEffectiveDate.ToString())) { row["Enquiry effective date"] = line.EnquiryEffectiveDate.Value.ToString("dd/MM/yyyy"); }
                row["Enquiry comment"] = line.EnquiryComment;
                row["Follow up"] = line.FollowUp;

                table.Rows.Add(row);
            }

            DataView view = new DataView(table);

            #endregion
            #region excel
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Cells["A1"].LoadFromDataTable(table, true);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=enquiry-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);
            }
            #endregion

        }
        #endregion

        //------------------------------------------------------------------------------------- system-log-report  ------------------------------------------------------------------------------------->
        #region hcare-1442: system-log-search
        public ActionResult SystemLogSearch()
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);

            var model = new SystemLogSearchVM();
            model.Logs = new List<SystemLogResultsVM>();
            model.filter = true;
            model.Tables = _admin.GetSystemLogTables();
            ViewBag.Post = "";

            return View(model);

        }
        [HttpPost]
        public ActionResult SystemLogSearch(SystemLogSearchVM model)
        {
            var user = _admin.GetUserByUsername(User.Identity.Name);
            
            model.Tables = _admin.GetSystemLogTables();

            #region form-variables
            var table = String.Empty;
            var column = String.Empty;
            var recordID = String.Empty;
            var currentValue = String.Empty;

            if (model.SelectedTable != null) { table = String.Join(",", model.SelectedTable); }
            if (model.SelectedColumn != null) { column = String.Join(",", model.SelectedColumn); }
            if (!String.IsNullOrEmpty(Request.Query["record-id"])) { recordID = Request.Query["record-id"]; }
            if (!String.IsNullOrEmpty(Request.Query["current-value"])) { currentValue = Request.Query["current-value"]; }

            #region date-variables
            var createdDate = "";

            if (!String.IsNullOrEmpty(Request.Query["created-date"]))
            {
                createdDate = Convert.ToDateTime(Request.Query["created-date"]).ToString("dd MMM yyyy 00:00:00");
            }
            else
            {
                createdDate = Convert.ToDateTime(DateTime.Today.AddYears(-1)).ToString("dd MMM yyyy 00:00:00");
            }
            #endregion
            #endregion
            #region hidden-fields
            TempData["table"] = table;
            TempData["column"] = column;
            TempData["recordid"] = recordID;
            TempData["currentvalue"] = currentValue;
            TempData["createddate"] = createdDate;
            #endregion

            model.filter = false;
            model.Logs = _admin.GetSystemLogSearchResults(table, column, recordID, currentValue, createdDate);

            ViewBag.Post = "return";

            return View(model);
        }
        public ActionResult SystemLogToExcel(SystemLogSearchVM model)
        {
            var table = Request.Query["table"].ToString();
            var column = Request.Query["column"].ToString();
            var recordID = Request.Query["recordid"].ToString();
            var currentValue = Request.Query["currentvalue"].ToString();
            var createdDate = Request.Query["createddate"].ToString();

            model.Logs = _admin.GetSystemLogSearchResults(table, column, recordID, currentValue, createdDate);

            #region data-table-both
            DataTable datatable = new DataTable();
            datatable.Columns.Add("RecordID");
            datatable.Columns.Add("EventType");
            datatable.Columns.Add("Table");
            datatable.Columns.Add("Column");
            datatable.Columns.Add("CurrentValue");
            datatable.Columns.Add("CreatedDate");

            foreach (var line in model.Logs)
            {
                DataRow row = datatable.NewRow();
                row["RecordID"] = line.RecordID;
                row["EventType"] = line.EventType;
                row["Table"] = line.TableName;
                row["Column"] = line.ColumnName;
                row["CurrentValue"] = line.CurrentValue;
                row["CreatedDate"] = line.CreatedDate.Value.ToString("dd/MM/yyyy");

                datatable.Rows.Add(row);
            }

            DataView view = new DataView(datatable);

            #endregion
            #region excel
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Results");
            workSheet.Cells["A1"].LoadFromDataTable(datatable, true);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=system-log-search.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.Body);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);
            }
            #endregion

        }

        public ActionResult GetLogTableColumns(string table)
        {
            var options = _admin.GetSystemLogColumns(table);

            return Json(options);
        }
        public ActionResult GetLogTableRecordID(string table)
        {
            var options = _admin.GetSystemLogRecords(table);

            return Json(options);
        }
        #endregion

    }
}