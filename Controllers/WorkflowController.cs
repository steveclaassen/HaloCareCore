using HaloCareCore.DAL;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class WorkflowController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowController(OH17Context context, IConfiguration configuration)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);

        }
        protected override void Dispose(bool disposing)
        {
            _admin.Dispose();
        }

        #region workflow hcare-1134
        public ActionResult Settings()
        {
            var viewModel = new wfSettingsVM()
            {
                wfViewModels = _admin.GetWorkFlowSettings().OrderByDescending(x => x.Active).ThenBy(x => x.MedicalScheme).ToList(),
                wfQueueVMs = _admin.GetWFUserQueue(),
            };

            return View(viewModel);
        }

        public ActionResult Create()
        {
            var model = new wfSettingsVM();
            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();
            model.ManagementStatuses = _member.GetManagementStatus().Where(x => x.active == true).ToList();
            model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.Active == true).ToList();
            model.AssignmentItemTypes = _member.GetAssignmentItems().Where(x => x.active == true).ToList();

            var UserName = User.Identity.Name;
            var user = _admin.GetUserByUsername(UserName);
            TempData["userid"] = user.userID;

            ViewBag.Queue = "";
            #region pathology-field-loop
            ViewBag.pathology1 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology2 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology3 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology4 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology5 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology6 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology7 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology8 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology9 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology10 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            #endregion

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(wfSettingsVM model)
        {
            #region duplicate-check
            var validation = _admin.GetWFSettings(); //hcare-1298
            var medicalaidID = new Guid(Request.Query["SelectedMedicalAids"]);
            var programID = new Guid(Request.Query["SelectedPrograms"]);
            var managementStatus = Request.Query["SelectedManagementStatus"];
            var riskrating = Request.Query["SelectedRiskRating"];
            var assignment = Request.Query["SelectedAssignments"];
            DateTime? fromdate = null;
            DateTime? todate = null;
            if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { fromdate = Convert.ToDateTime(Request.Query["FromDate"]); }
            if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { todate = Convert.ToDateTime(Request.Query["ToDate"]); }

            var vcount = 0;
            foreach (var entry in validation)
            {
                if ((medicalaidID == entry.MedicalAidID)
                    && (programID == entry.ProgramID)
                    && (managementStatus == entry.ManagementStatus)
                    && (riskrating == entry.RiskRating)
                    && (assignment == entry.Assignment)
                    && (fromdate == entry.FromDate)
                    && (todate == entry.ToDate)
                    ) { vcount++; }
            }
            if (vcount > 0)
            {
                #region variables
                model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
                model.Programs = _member.GetAllowedPrograms().Where(x => x.Active == true).ToList();
                model.ManagementStatuses = _member.GetManagementStatus().Where(x => x.active == true).ToList();
                model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.Active == true).ToList();
                model.AssignmentItemTypes = _member.GetAssignmentItems().Where(x => x.active == true).ToList();

                var UserName = User.Identity.Name;
                var user = _admin.GetUserByUsername(UserName);
                TempData["userid"] = user.userID;

                var queueinformation = _admin.GetWorkFlowSettingInformation(medicalaidID, programID, managementStatus, riskrating);

                #region pathology-field-loop
                ViewBag.pathology1 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology2 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology3 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology4 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology5 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology6 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology7 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology8 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology9 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology10 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                #endregion
                #endregion

                ViewBag.Queue = queueinformation.QueueName;

                return View(model);
            }

            #endregion

            var queueid = Guid.NewGuid();
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!String.IsNullOrEmpty(Request.Query["pathology" + i]))
                {
                    var wfsetting = new wfSettings();
                    wfsetting.QueueID = queueid;
                    wfsetting.QueueName = Request.Query["wfSetting.QueueName"].ToString().Trim(); //hcare-1285
                    wfsetting.MedicalAidID = new Guid(Request.Query["SelectedMedicalAids"]);
                    wfsetting.ProgramID = new Guid(Request.Query["SelectedPrograms"]);
                    wfsetting.ManagementStatus = Request.Query["SelectedManagementStatus"];
                    if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { wfsetting.FromDate = Convert.ToDateTime(Request.Query["FromDate"]); }
                    if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { wfsetting.ToDate = Convert.ToDateTime(Request.Query["ToDate"]); }
                    wfsetting.RiskRating = Request.Query["SelectedRiskRating"];
                    wfsetting.Assignment = Request.Query["SelectedAssignments"];
                    wfsetting.Instruction = Request.Query["wfSetting.Instruction"].ToString().Replace("'", "").Trim(); //hcare-1285
                    wfsetting.CreatedDate = DateTime.Now;
                    wfsetting.CreatedBy = User.Identity.Name;
                    wfsetting.Active = true;
                    if (!String.IsNullOrEmpty(Request.Query["pathology" + i])) { wfsetting.PathologyField = Request.Query["pathology" + i]; }
                    if (!String.IsNullOrEmpty(Request.Query["less-" + i])) { wfsetting.Less = Convert.ToDecimal(Request.Query["less-" + i]); }
                    if (!String.IsNullOrEmpty(Request.Query["greater-" + i])) { wfsetting.Greater = Convert.ToDecimal(Request.Query["greater-" + i]); }

                    var insert = _admin.InsertWFSetting(wfsetting);
                    count++;
                }
            }
            if (count == 0)
            {
                var wfsetting = new wfSettings();
                wfsetting.QueueID = queueid;
                wfsetting.QueueName = Request.Query["wfSetting.QueueName"].ToString().Trim(); //hcare-1285
                wfsetting.MedicalAidID = new Guid(Request.Query["SelectedMedicalAids"]);
                wfsetting.ProgramID = new Guid(Request.Query["SelectedPrograms"]);
                wfsetting.ManagementStatus = Request.Query["SelectedManagementStatus"];
                if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { wfsetting.FromDate = Convert.ToDateTime(Request.Query["FromDate"]); }
                if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { wfsetting.ToDate = Convert.ToDateTime(Request.Query["ToDate"]); }
                wfsetting.RiskRating = Request.Query["SelectedRiskRating"];
                wfsetting.Assignment = Request.Query["SelectedAssignments"];
                wfsetting.Instruction = Request.Query["wfSetting.Instruction"].ToString().Replace("'", "").Trim(); //hcare-1285;
                wfsetting.CreatedDate = DateTime.Now;
                wfsetting.CreatedBy = User.Identity.Name;
                wfsetting.Active = true;

                var insert = _admin.InsertWFSetting(wfsetting);
            }

            return RedirectToAction("Settings");
        }

        public ActionResult Edit(Guid id)
        {
            wfViewModel model = _admin.GetWorkflowSettingsById(id);
            var queues = _admin.GetWorkflowSettingsByQueue(id);

            model.PathologyFields = _admin.GetWorkflowSettingsByQueue(id);
            var user = _admin.GetUserByUsername(User.Identity.Name);

            ViewBag.MedicalAids = new SelectList(_member.GetMedicalAidsByUser(user.userID), "MedicalAidID", "Name", model.MedicalAidID);
            ViewBag.Programs = new SelectList(_member.GetAllowedProgramsPerUser(model.MedicalAidID, user.userID), "programID", "ProgramName", model.ProgramID);
            ViewBag.ManagementStatuses = new SelectList(_member.GetManagementStatus(), "statusCode", "statusName", model.ManagementStatus);
            ViewBag.RiskRatingTypes = new SelectList(_member.GetRiskRatingTypes(), "RiskName", "RiskName", model.RiskRating);
            ViewBag.AssignmentItemTypes = new SelectList(_member.GetAssignmentItemTypesForUser(user.userID), "assignmentItemType", "itemDescription", model.AssignmentItemType);

            TempData["userid"] = user.userID;
            if (!String.IsNullOrEmpty(model.RiskRating)) { TempData["riskratingfields"] = model.RiskRating; } else { ViewBag.riskrating = "no-risk"; TempData["riskratingfields"] = "no-risk"; }
            if (!String.IsNullOrEmpty(model.ManagementStatus)) { TempData["mStatusField"] = model.ManagementStatus; } else { ViewBag.mStatusField = "no-status"; TempData["mStatusField"] = "no-status"; }
            if (!String.IsNullOrEmpty(model.AssignmentItemType)) { TempData["assignmentField"] = model.AssignmentItemType; } else { ViewBag.assignmentField = "no-assignment"; TempData["assignmentField"] = "no-assignment"; }

            ViewBag.PathologyField = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            TempData["pathologyfields"] = model.PathologyField;

            #region pathology-field-loop
            ViewBag.pathology1 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology2 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology3 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology4 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology5 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology6 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology7 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology8 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology9 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            ViewBag.pathology10 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
            #endregion

            return View(model);

        }
        [HttpPost]
        public ActionResult Edit(wfViewModel model)
        {
            //REQUIREMENTS: the post needs the temp fields if you are posting back to the model. 

            var queueid = new Guid(Request.Query["QueueID"]);
            model = _admin.GetWorkflowSettingsById(queueid);
            model.PathologyFields = _admin.GetWorkflowSettingsByQueue(queueid);

            #region duplicate-check
            var validation = _admin.GetWFSettings(); //hcare-1298
            var queueID = new Guid(Request.Query["QueueID"]);
            var medicalaidID = new Guid(Request.Query["MedicalAids"]);
            var programID = new Guid(Request.Query["Programs"]);
            string managementStatus = null; if (!String.IsNullOrEmpty(Request.Query["ManagementStatuses"])) { managementStatus = Request.Query["ManagementStatuses"]; }
            string riskrating = null; if (!String.IsNullOrEmpty(Request.Query["RiskRatingTypes"])) { riskrating = Request.Query["RiskRatingTypes"]; }
            string assignment = null; if (!String.IsNullOrEmpty(Request.Query["AssignmentItemTypes"])) { assignment = Request.Query["AssignmentItemTypes"]; }
            DateTime? fromdate = null; if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { fromdate = Convert.ToDateTime(Request.Query["FromDate"]); }
            DateTime? todate = null; if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { todate = Convert.ToDateTime(Request.Query["ToDate"]); }

            var vcount = 0;
            foreach (var entry in validation)
            {
                if ((queueID != entry.QueueID)
                    && (medicalaidID == entry.MedicalAidID)
                    && (programID == entry.ProgramID)
                    && (managementStatus == entry.ManagementStatus)
                    && (riskrating == entry.RiskRating)
                    && (assignment == entry.Assignment)
                    && (fromdate == entry.FromDate)
                    && (todate == entry.ToDate)
                    ) { vcount++; }
            }
            if (vcount > 0)
            {
                #region variables
                var user = _admin.GetUserByUsername(User.Identity.Name);
                ViewBag.MedicalAids = new SelectList(_member.GetMedicalAidsByUser(user.userID), "MedicalAidID", "Name", model.MedicalAidID);
                ViewBag.Programs = new SelectList(_member.GetAllowedProgramsPerUser(model.MedicalAidID, user.userID), "programID", "ProgramName", model.ProgramID);
                ViewBag.ManagementStatuses = new SelectList(_member.GetManagementStatus(), "statusCode", "statusName", model.ManagementStatus);
                ViewBag.RiskRatingTypes = new SelectList(_member.GetRiskRatingTypes(), "RiskName", "RiskName", model.RiskRating);
                ViewBag.AssignmentItemTypes = new SelectList(_member.GetAssignmentItemTypesForUser(user.userID), "assignmentItemType", "itemDescription", model.AssignmentItemType);

                TempData["userid"] = user.userID;
                if (!String.IsNullOrEmpty(model.RiskRating)) { TempData["riskratingfields"] = model.RiskRating; } else { ViewBag.riskrating = "no-risk"; TempData["riskratingfields"] = "no-risk"; }
                if (!String.IsNullOrEmpty(model.ManagementStatus)) { TempData["mStatusField"] = model.ManagementStatus; } else { ViewBag.mStatusField = "no-status"; TempData["mStatusField"] = "no-status"; }
                if (!String.IsNullOrEmpty(model.AssignmentItemType)) { TempData["assignmentField"] = model.AssignmentItemType; } else { ViewBag.assignmentField = "no-assignment"; TempData["assignmentField"] = "no-assignment"; }

                ViewBag.PathologyField = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                TempData["pathologyfields"] = model.PathologyField;

                var queueinformation = _admin.GetWorkFlowSettingInformation(medicalaidID, programID, managementStatus, riskrating);

                #region pathology-field-loop
                ViewBag.pathology1 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology2 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology3 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology4 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology5 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology6 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology7 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology8 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology9 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                ViewBag.pathology10 = new SelectList(_member.GetPathologyFields().Where(x => x.PathologyField != "hivEliza").Where(x => x.PathologyField != "normGtt").Where(x => x.PathologyField != "abnGtt"), "PathologyField", "DisplayName");
                #endregion
                #endregion

                ViewBag.Queue = queueinformation.QueueName;

                return View(model);
            }

            #endregion

            var delete = _admin.DeleteWFSetting(queueid);
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!String.IsNullOrEmpty(Request.Query["old-pathology-" + i]))
                {
                    var wfsetting = new wfSettings();
                    wfsetting.QueueID = queueid;
                    wfsetting.QueueName = Request.Query["QueueName"].ToString().Trim(); //hcare-1285
                    wfsetting.MedicalAidID = new Guid(Request.Query["MedicalAids"]);
                    wfsetting.ProgramID = new Guid(Request.Query["Programs"]);
                    wfsetting.ManagementStatus = Request.Query["ManagementStatuses"];
                    if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { wfsetting.FromDate = Convert.ToDateTime(Request.Query["FromDate"]); }
                    if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { wfsetting.ToDate = Convert.ToDateTime(Request.Query["ToDate"]); }
                    if (!String.IsNullOrEmpty(Request.Query["RiskRatingTypes"])) { wfsetting.RiskRating = Request.Query["RiskRatingTypes"]; } else { wfsetting.RiskRating = null; }
                    if (!String.IsNullOrEmpty(Request.Query["AssignmentItemTypes"])) { wfsetting.Assignment = Request.Query["AssignmentItemTypes"].ToString().Replace("Select,", ""); } else { wfsetting.Assignment = null; }
                    wfsetting.Instruction = Request.Query["Instruction"].ToString().Replace("'", "").Trim(); //hcare-1285
                    wfsetting.CreatedDate = model.CreatedDate;
                    wfsetting.CreatedBy = model.CreatedBy;
                    wfsetting.ModifiedDate = DateTime.Now;
                    wfsetting.ModifiedBy = User.Identity.Name;
                    if (Request.Query["Active"].ToString().ToLower().Contains("true")) { wfsetting.Active = true; } else { wfsetting.Active = false; }
                    if (!String.IsNullOrEmpty(Request.Query["old-pathology-" + i])) { wfsetting.PathologyField = Request.Query["old-pathology-" + i]; }
                    if (!String.IsNullOrEmpty(Request.Query["less-" + i])) { wfsetting.Less = Convert.ToDecimal(Request.Query["less-" + i]); }
                    if (!String.IsNullOrEmpty(Request.Query["greater-" + i])) { wfsetting.Greater = Convert.ToDecimal(Request.Query["greater-" + i]); }

                    var insert = _admin.InsertWFSetting(wfsetting);
                    count++;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                if (!String.IsNullOrEmpty(Request.Query["new-pathology-" + i]))
                {
                    var wfsetting = new wfSettings();
                    wfsetting.QueueID = queueid;
                    wfsetting.QueueName = Request.Query["QueueName"].ToString().Trim(); //hcare-1285
                    wfsetting.MedicalAidID = new Guid(Request.Query["MedicalAids"]);
                    wfsetting.ProgramID = new Guid(Request.Query["Programs"]);
                    wfsetting.ManagementStatus = Request.Query["ManagementStatuses"];
                    if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { wfsetting.FromDate = Convert.ToDateTime(Request.Query["FromDate"]); }
                    if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { wfsetting.ToDate = Convert.ToDateTime(Request.Query["ToDate"]); }
                    if (!String.IsNullOrEmpty(Request.Query["RiskRatingTypes"])) { wfsetting.RiskRating = Request.Query["RiskRatingTypes"]; } else { wfsetting.RiskRating = null; }
                    if (!String.IsNullOrEmpty(Request.Query["AssignmentItemTypes"])) { wfsetting.Assignment = Request.Query["AssignmentItemTypes"].ToString().Replace("Select,", ""); } else { wfsetting.Assignment = null; }
                    wfsetting.Instruction = Request.Query["Instruction"].ToString().Replace("'", "").Trim(); //hcare-1285
                    wfsetting.CreatedDate = model.CreatedDate;
                    wfsetting.CreatedBy = model.CreatedBy;
                    wfsetting.ModifiedDate = DateTime.Now;
                    wfsetting.ModifiedBy = User.Identity.Name;
                    if (Request.Query["Active"].ToString().ToLower().Contains("true")) { wfsetting.Active = true; } else { wfsetting.Active = false; }
                    if (!String.IsNullOrEmpty(Request.Query["new-pathology-" + i])) { wfsetting.PathologyField = Request.Query["new-pathology-" + i]; }
                    if (!String.IsNullOrEmpty(Request.Query["new-less-" + i])) { wfsetting.Less = Convert.ToDecimal(Request.Query["new-less-" + i]); }
                    if (!String.IsNullOrEmpty(Request.Query["new-greater-" + i])) { wfsetting.Greater = Convert.ToDecimal(Request.Query["new-greater-" + i]); }

                    var insert = _admin.InsertWFSetting(wfsetting);
                    count++;
                }
                else
                    continue;
            }

            if (count == 0)
            {
                var wfsetting = new wfSettings();
                wfsetting.QueueID = queueid;
                wfsetting.QueueName = Request.Query["QueueName"].ToString().Trim(); //hcare-1285
                wfsetting.MedicalAidID = new Guid(Request.Query["MedicalAids"]);
                wfsetting.ProgramID = new Guid(Request.Query["Programs"]);
                wfsetting.ManagementStatus = Request.Query["ManagementStatuses"];
                if (!String.IsNullOrEmpty(Request.Query["FromDate"])) { wfsetting.FromDate = Convert.ToDateTime(Request.Query["FromDate"]); }
                if (!String.IsNullOrEmpty(Request.Query["ToDate"])) { wfsetting.ToDate = Convert.ToDateTime(Request.Query["ToDate"]); }
                if (!String.IsNullOrEmpty(Request.Query["RiskRatingTypes"])) { wfsetting.RiskRating = Request.Query["RiskRatingTypes"]; } else { wfsetting.RiskRating = null; }
                if (!String.IsNullOrEmpty(Request.Query["AssignmentItemTypes"])) { wfsetting.Assignment = Request.Query["AssignmentItemTypes"].ToString().Replace("Select,", ""); } else { wfsetting.Assignment = null; }
                wfsetting.Instruction = Request.Query["Instruction"].ToString().Replace("'", "").Trim(); //hcare-1285
                wfsetting.CreatedDate = model.CreatedDate;
                wfsetting.CreatedBy = model.CreatedBy;
                wfsetting.ModifiedDate = DateTime.Now;
                wfsetting.ModifiedBy = User.Identity.Name;
                if (Request.Query["Active"].ToString().ToLower().Contains("true")) { wfsetting.Active = true; } else { wfsetting.Active = false; }

                var insert = _admin.InsertWFSetting(wfsetting);
            }



            var active = false;
            var wfuserqueue = _admin.GetUserQueueListByQueueID(queueid);
            if (wfuserqueue != null)
            {
                if (Request.Query["Active"].ToString().ToLower().Contains("true")) { active = true; } else { active = false; }
                if (active == false) { _admin.DeleteWFUserQueue(wfuserqueue.QueueID); }
            }

            return RedirectToAction("Settings");
        }

        public ActionResult Details(Guid id)
        {
            wfViewModel model = _admin.GetWorkflowSettingDetails(id);
            model.PathologyFields = _admin.GetWorkflowSettingsByQueue(id);

            return View(model);
        }

        public ActionResult FillSchemeForUser(Guid userID)
        {
            var options = _member.GetMedicalAidsByUser(userID);

            return Json(options);
        }
        public ActionResult FillProgramForUser(Guid medID, Guid userID)
        {
            var options = _member.GetAllowedProgramsPerUser(medID, userID);

            return Json(options);
        }
        public ActionResult FillProgramsForMedicalAid(Guid medID)
        {
            var options = _member.GetAllowedProgramsPerScheme(medID);

            return Json(options);
        }
        public ActionResult FillManagementStatus(Guid medID, Guid proID)
        {
            var options = _member.GetManagementStatusesByMAandPro(medID, proID);

            return Json(options);
        }
        public ActionResult FillAssignmentsForUser(Guid userID)
        {
            var options = _member.GetAssignmentItemTypesForUser(userID);

            return Json(options);
        }

        public ActionResult UserWorkflow(string UserName)
        {
            var user = _admin.GetUserByUsername(UserName);
            //var wfSetting = _admin.GetWFSettings();

            //foreach (var wf in wfSetting)
            //{
            //    var queueID = wf.QueueID;
            //    var userQueue = _admin.GetWFUserQueueByQueueID(wf.QueueID);

            //    var medicalscheme = "";
            //    var program = "";
            //    var managementStatus = "";
            //    var fromDate = "";
            //    var toDate = "";
            //    var riskRating = "";
            //    var assignment = "";
            //    var instruction = "";

            //    medicalscheme = wf.MedicalAidID.ToString();
            //    program = wf.ProgramID.ToString();
            //    managementStatus = wf.ManagementStatus;
            //    if (wf.FromDate != null) { fromDate = wf.FromDate.Value.ToString("dd-MMM-yyyy 00:00:00"); } else { fromDate = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00"); }
            //if (wf.ToDate != null) { toDate = wf.ToDate.Value.ToString("dd-MMM-yyyy 23:59:59"); } else { toDate = DateTime.Now.ToString("dd-MMM-yyyy 23:59:59"); }
            //    riskRating = wf.RiskRating;
            //    assignment = wf.Assignment;
            //    instruction = wf.Instruction;

            //if (userQueue != null) { var insert = _member.InsertWFQueue(queueID, medicalscheme, program, managementStatus, fromDate, toDate, riskRating, assignment, instruction); }

            //}

            var index = _member.GetUserWorkflowIndex(user.userID);

            var viewModel = new WorkflowUserVM()
            {
                wfUsers = _member.GetUWFList(user.userID),
                wfQueues = _member.GetUserWorkflowIndex(user.userID),
            };


            return View(viewModel);
        }

        public ActionResult WorkflowTask(WorkflowUser model)
        {
            model.DependantID = new Guid(Request.Query["dependantid"]);
            model.Id = Convert.ToInt32(Request.Query["id"]); //hcare-1311
            #region work-flow-log
            var wflog = new wfUserLog();
            wflog.QueueID = new Guid(Request.Query["queueid"]);
            wflog.SchemeID = new Guid(Request.Query["medicalaidid"]);
            wflog.ProgramID = new Guid(Request.Query["programid"]);
            wflog.UserID = new Guid(Request.Query["userid"]);
            wflog.DependantID = new Guid(Request.Query["dependantid"]);
            wflog.DependantCode = Request.Query["dependantcode"];
            wflog.Title = Request.Query["title"];
            wflog.FirstName = Request.Query["firstname"];
            wflog.LastName = Request.Query["lastname"];
            wflog.MembershipNumber = Request.Query["membernumber"];
            wflog.ManagementStatus = Request.Query["mstatus"];
            wflog.RiskRating = Request.Query["riskrating"];
            wflog.Instruction = Request.Query["instruction"];

            if (Request.Query["status"].ToString().ToLower() == "open")
            {
                wflog.Status = "Open";
                wflog.ModifiedBy = User.Identity.Name;
                wflog.ModifiedDate = DateTime.Now;
            }
            if (Request.Query["status"].ToString().ToLower() == "in progress")
            {
                wflog.Status = "In progress";
                wflog.ModifiedBy = User.Identity.Name;
                wflog.ModifiedDate = DateTime.Now;
            }
            if (Request.Query["status"].ToString().ToLower() == "closed")
            {
                wflog.Status = "Closed";
                wflog.ModifiedBy = User.Identity.Name;
                wflog.ModifiedDate = DateTime.Now;
            }
            wflog.Active = true;

            var insert = _member.InsertUserWorkflowLog(wflog);
            #endregion
            #region work-flow-queue
            var workflowuser = _member.GetUserWFById(model.Id, wflog.UserID, wflog.SchemeID, wflog.ProgramID, wflog.ManagementStatus, wflog.MembershipNumber, wflog.DependantCode, wflog.RiskRating); //hcare-1311
            if (Request.Query["status"].ToString().ToLower() == "open")
            {
                workflowuser.Status = "Open";
                var update = _member.UpdateWorkflowUser(workflowuser);
                return new RedirectResult(Url.Action("MemberProgram", "Member", new { DependentID = model.DependantID }));
            }
            if (Request.Query["status"].ToString().ToLower() == "in progress")
            {
                workflowuser.Status = "In progress";
                var update = _member.UpdateWorkflowUser(workflowuser);
                return new RedirectResult(Url.Action("MemberProgram", "Member", new { DependentID = model.DependantID }));
            }
            if (Request.Query["status"].ToString().ToLower() == "closed")
            {
                workflowuser.Status = "Closed";
                var update = _member.UpdateWorkflowUser(workflowuser);
                return new RedirectResult(Url.Action("UserWorkflow", "Workflow", new { UserName = User.Identity.Name }));
            }
            #endregion
            return View();
        }

        public ActionResult Create_UserQueue()
        {
            var model = new wfSettingsVM();
            model.Users = _admin.GetUsersList().Where(x => x.Active == true).ToList();
            model.Queues = _admin.GetUserQueues().Where(x => x.Active == true).ToList();
            ViewBag.MedicalAidError = "";
            ViewBag.ProgramError = "";
            ViewBag.AssignmentError = "";
            ViewBag.User = "";
            ViewBag.Queue = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Create_UserQueue(wfSettingsVM model)
        {
            var userqueue = new wfUserQueue();

            userqueue.UserID = new Guid(Request.Query["SelectedUser"]);
            userqueue.QueueID = new Guid(Request.Query["SelectedQueues"]);
            userqueue.CreatedDate = DateTime.Now;
            userqueue.CreatedBy = User.Identity.Name;
            userqueue.Active = true;

            var queueinformation = _admin.GetUserQueues().Where(x => x.QueueID == userqueue.QueueID).FirstOrDefault();
            var usermedicalaid = _member.GetMedicalAidsByUser(userqueue.UserID);
            var userprograms = _member.GetAllowedProgramsPerUser(queueinformation.MedicalAidID, userqueue.UserID);
            var usermanagementstatus = _member.GetManagementStatusesByMAandPro(queueinformation.MedicalAidID, queueinformation.ProgramID);
            var userassignments = _member.GetAssignmentItemTypesForUser(userqueue.UserID);

            var medicalaid = _member.GetMedicalAidById(queueinformation.MedicalAidID);
            var program = _member.GetPrograms().Where(x => x.programID == queueinformation.ProgramID).FirstOrDefault();
            var assignment = _member.GetAssignmentItems().Where(x => x.assignmentItemType == queueinformation.Assignment).FirstOrDefault();
            var user = _member.GetUserNameByUserID(userqueue.UserID);
            var queue = _member.GetQueueNameByQueueID(userqueue.QueueID);

            #region duplicate-check
            var validation = _admin.GetUserQueueList();
            var vcount = 0;
            foreach (var entry in validation)
            {
                if ((userqueue.UserID == entry.UserID) && (userqueue.QueueID == entry.QueueID)) { vcount++; }
            }
            if (vcount > 0)
            {
                model.Users = _admin.GetUsersList().Where(x => x.Active == true).ToList();
                model.Queues = _admin.GetUserQueues().Where(x => x.Active == true).ToList();

                ViewBag.User = user.Firstname + " " + user.Lastname;
                ViewBag.Queue = queue.QueueName;

                return View(model);
            }

            #endregion
            #region medical-aid-check
            var macount = 0;
            foreach (var ma in usermedicalaid)
            {
                if (ma.MedicalAidID == queueinformation.MedicalAidID) { macount++; }
            }
            if (macount == 0)
            {
                model.Users = _admin.GetUsersList().Where(x => x.Active == true).ToList();
                model.Queues = _admin.GetUserQueues().Where(x => x.Active == true).ToList();
                ViewBag.MedicalAidError = medicalaid.Name;

                return View(model);
            }
            #endregion
            #region program-check
            var pcount = 0;
            foreach (var p in userprograms)
            {
                if (p.programID == queueinformation.ProgramID) { pcount++; }
            }
            if (pcount == 0)
            {
                model.Users = _admin.GetUsersList().Where(x => x.Active == true).ToList();
                model.Queues = _admin.GetUserQueues().Where(x => x.Active == true).ToList();
                ViewBag.ProgramError = program.ProgramName;

                return View(model);
            }
            #endregion
            #region assignment-check
            var acount = 0;
            foreach (var aa in userassignments)
            {
                if (queue.Assignment != null)
                {
                    string[] qAssignment = queue.Assignment.Split(',');
                    foreach (string a in qAssignment)
                    {
                        if (aa.assignmentItemType == a) { acount++; }
                    }
                }
            }
            if (!String.IsNullOrEmpty(queueinformation.Assignment) && acount == 0)
            {
                model.Users = _admin.GetUsersList().Where(x => x.Active == true).ToList();
                model.Queues = _admin.GetUserQueues().Where(x => x.Active == true).ToList();
                ViewBag.AssignmentError = assignment.itemDescription;

                return View(model);
            }
            #endregion


            var insert = _admin.InsertWFUserQueue(userqueue);

            return RedirectToAction("Settings");
        }

        public ActionResult Edit_UserQueue(int id)
        {
            var model = _admin.GetWFUserQueueById(id);

            ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", model.UserID);
            ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", model.QueueID);
            ViewBag.MedicalAidError = "";
            ViewBag.ProgramError = "";
            ViewBag.AssignmentError = "";
            ViewBag.UserID = "";
            ViewBag.User = "";
            ViewBag.Queue = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_UserQueue(wfQueueVM model)
        {
            var userqueue = new wfUserQueue();

            userqueue.Id = Convert.ToInt32(Request.Query["Id"]);
            userqueue.UserID = new Guid(Request.Query["Users"]);
            userqueue.QueueID = new Guid(Request.Query["Queues"]);
            if (Request.Query["Active"].ToString().ToLower().Contains("true")) { userqueue.Active = true; } else { userqueue.Active = false; }

            userqueue.ModifiedDate = DateTime.Now;
            userqueue.ModifiedBy = User.Identity.Name;

            var queueinformation = _admin.GetUserQueues().Where(x => x.QueueID == userqueue.QueueID).FirstOrDefault();
            var usermedicalaid = _member.GetMedicalAidsByUser(userqueue.UserID);
            var userprograms = _member.GetAllowedProgramsPerUser(queueinformation.MedicalAidID, userqueue.UserID);
            var usermanagementstatus = _member.GetManagementStatusesByMAandPro(queueinformation.MedicalAidID, queueinformation.ProgramID);
            var userassignments = _member.GetAssignmentItemTypesForUser(userqueue.UserID);

            var medicalaid = _member.GetMedicalAidById(queueinformation.MedicalAidID);
            var program = _member.GetPrograms().Where(x => x.programID == queueinformation.ProgramID).FirstOrDefault();
            var assignment = _member.GetAssignmentItems().Where(x => x.assignmentItemType == queueinformation.Assignment).FirstOrDefault();
            var user = _member.GetUserNameByUserID(userqueue.UserID);
            var queue = _member.GetQueueNameByQueueID(userqueue.QueueID);

            #region duplicate-check
            var validation = _admin.GetUserQueueList();
            var vcount = 0;
            foreach (var entry in validation)
            {
                if ((model.Id != entry.Id) && (userqueue.UserID == entry.UserID) && (userqueue.QueueID == entry.QueueID)) { vcount++; }
            }
            if (vcount > 0)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", model.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", model.QueueID);
                ViewBag.User = user.Firstname + " " + user.Lastname;
                ViewBag.Queue = queue.QueueName;

                return View(model);
            }
            #endregion
            #region user-rights 
            if (usermedicalaid == null)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", userqueue.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", userqueue.QueueID);
                ViewBag.MedicalAidError = medicalaid.Name;
                ViewBag.UserID = userqueue.UserID;

                return View(model);
            }
            if (userprograms == null)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", userqueue.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", userqueue.QueueID);
                ViewBag.ProgramError = program.ProgramName;
                ViewBag.UserID = userqueue.UserID;

                return View(model);
            }
            #endregion
            #region medical-aid-check
            var macount = 0;
            foreach (var ma in usermedicalaid)
            {
                if (ma.MedicalAidID == queueinformation.MedicalAidID) { macount++; }
            }
            if (macount == 0)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", userqueue.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", userqueue.QueueID);
                ViewBag.MedicalAidError = medicalaid.Name;
                ViewBag.UserID = userqueue.UserID;

                return View(model);
            }
            #endregion
            #region program-check
            var pcount = 0;
            foreach (var p in userprograms)
            {
                if (p.programID == queueinformation.ProgramID) { pcount++; }
            }
            if (pcount == 0)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", userqueue.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", userqueue.QueueID);
                ViewBag.ProgramError = program.ProgramName;
                ViewBag.UserID = userqueue.UserID;

                return View(model);
            }
            #endregion
            #region assignment-check
            var acount = 0;
            foreach (var aa in userassignments)
            {
                if (queue.Assignment != null)
                {
                    string[] qAssignment = queue.Assignment.Split(',');
                    foreach (string a in qAssignment)
                    {
                        if (aa.assignmentItemType == a) { acount++; }
                    }
                }
            }
            if (!String.IsNullOrEmpty(queueinformation.Assignment) && acount == 0)
            {
                ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName", userqueue.UserID);
                ViewBag.Queues = new SelectList(_admin.GetUserQueues(), "QueueID", "QueueName", userqueue.QueueID);
                ViewBag.AssignmentError = assignment.itemDescription;
                ViewBag.UserID = userqueue.UserID;

                return View(model);
            }
            #endregion

            var wfqueue = _member.GetWFQueueByUserAndQueue(model.UserID, model.QueueID);
            if (wfqueue.Count > 0)
            {
                var uqInfo = _member.GetUserQueueInfo(model.UserID, model.QueueID);

                if ((uqInfo.UserID == userqueue.UserID && uqInfo.QueueID != userqueue.QueueID) || (userqueue.Active == false))
                {
                    _member.RemoveWFQueueFromUser(model.UserID, model.QueueID);
                }
                else if ((uqInfo.UserID != userqueue.UserID && uqInfo.QueueID != userqueue.QueueID))
                {
                    _member.RemoveWFQueueFromUser(model.UserID, model.QueueID);
                }
                else if (uqInfo.UserID != userqueue.UserID && uqInfo.QueueID == userqueue.QueueID)
                {
                    _member.UpdateWFQueueForUser(model.UserID, model.QueueID, userqueue.UserID);
                }
            }

            var update = _admin.UpdateWFUserQueue(userqueue);

            return RedirectToAction("Settings");
        }

        public ActionResult Details_UserQueue(int id)
        {
            var model = _admin.GetWFUserQueueById(id);

            return View(model);
        }

        #endregion

    }
}