using ClosedXML.Excel;
using HaloCareCore.DAL;
using HaloCareCore.Extensions;
using HaloCareCore.Helpers;
using HaloCareCore.Management;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace HaloCareCore.Controllers
{
    public class ClinicalController : Controller
    {
        private IMemberRepository _member;
        private IClinicalRepository _clinical;
        private IAdminRepository _admin;
        private IMedicalAidRepository _medical;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClinicalController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, OH17Context context)
        {
            _member = new MemberRepository(configuration, context);
            _clinical = new ClinicalRepository(context, configuration);
            _admin = new AdminRepository(context, configuration);
            _medical = new MedicalAidRepository(configuration, context);//HCare-1197
        }

        // GET: Clinical
        public ActionResult Index()
        {
            return View();
        }

        // GET: Clinical/OpenAssignments
        public ActionResult OpenAssignments()
        {
            var model = new AssignmentsListView();
            model.openAssignments = _clinical.GetOpenAssignmentsFull(500);
            model.inProgressAssignments = _clinical.GetInProgressAssignments();
            model.closedAssignments = _clinical.GetClosedAssignments();
            model.opencount = model.openAssignments.Count() + 1;
            model.inprogresscount = model.inProgressAssignments.Count() + 1;
            model.closedcount = model.closedAssignments.Count() + 1;
            return View(model);
        }

        [HttpPost]
        public ActionResult OpenAssignments(AssignmentsListView model)
        {
            int newCount = model.pageRowCount + 50;
            model.openAssignments = _clinical.GetOpenAssignments(newCount);
            model.inProgressAssignments = _clinical.GetInProgressAssignments();
            model.closedAssignments = _clinical.GetClosedAssignments();
            model.opencount = model.openAssignments.Count() + 1;
            model.inprogresscount = model.inProgressAssignments.Count() + 1;
            model.closedcount = model.closedAssignments.Count() + 1;
            ModelState.Remove("pageRowCount");
            model.pageRowCount = newCount;
            return View(model);
        }

        public ActionResult CompactAssignments()
        {
            var model = new CompactAssignmentsListView();
            model.openAssignments = _clinical.GetCompactOpenAssignments();
            //model.opencount = _clinical.GetOpenAssignments().Count() + 1;
            //model.inProgressAssignments = _clinical.GetCompactInProgressAssignments();
            model.closedAssignments = _clinical.GetCompactClosedAssignments();
            //model.closedcount = _clinical.GetClosedAssignments().Count() + 1;

            return View(model);
        }

        public ActionResult ClosedAssignments()
        {
            var model = _clinical.GetClosedAssignments();
            return View(model);
        }

        // GET: Clinical/PostponedAssignments
        public ActionResult PostponedAssignments()
        {
            var model = _clinical.GetPostponedAssignments();
            return View(model);
        }

        // GET: Clinical/DocumentAssignments
        public ActionResult DocumentAssignments()
        {
            var model = _clinical.GetDocumentAssignments();
            return View(model);
        }

        // GET: Clinical/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult testPathology(Guid DependentID)
        {
            ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes(), "id", "pathologyType");
            Pathology model = new Pathology();
            model.effectiveDate = DateTime.Now;
            model.dependentID = DependentID;
            return View(model);

        }

        public ActionResult CreatePathology(Guid DependentID)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            //HCare-834
            if (Request.Query["pt"].ToString() != null)
            {
                ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == Request.Query["pt"]), "id", "pathologyType");
            }
            else
            {
                ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");//HCARE-837
            }

            ViewBag.programId = programCode;
            ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");


            var pathology = new Pathology();
            var attachment = new Attachments();
            var model = new pathologyAttachment()
            {
                pathology = pathology,
                attachment = attachment
            };

            model.pathology.effectiveDate = DateTime.Now;
            model.pathology.dependentID = DependentID;
            model.attachment.dependentID = DependentID;

            return View(model);
        }

        [HttpPost]
        public ActionResult CreatePathology(pathologyAttachment model)
        {
            var fileAttached = Request.Form.Files["Pathfile"];
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            ViewBag.programId = Request.Query["pro"];

            if (Request.Query["chkNoAttachment"].ToString().ToLower().Contains("true") || fileAttached.HasFile())
            {
                var labreference = _member.GetPathology(model.pathology.dependentID).Where(x => x.labReferenceNo == model.pathology.labReferenceNo).ToList();
                if (labreference.Count() == 0)
                {
                    #region pathology-insert
                    //HCare-674
                    model.pathology.createdDate = DateTime.Now;
                    model.pathology.createdBy = User.Identity.Name;
                    model.pathology.labReferenceNo = Request.Query["pathology.labReferenceNo"];
                    model.pathology.labName = Request.Query["laboratories"];
                    model.pathology.pathologyType = Request.Query["pathologyType"];
                    #region decimal-insert //HCare-1050
                    if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Count"])) model.pathology.CD4Count = decimal.Parse(Request.Query["pathology.CD4Count"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Percentage"])) model.pathology.CD4Percentage = decimal.Parse(Request.Query["pathology.CD4Percentage"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.viralLoad"])) model.pathology.viralLoad = decimal.Parse(Request.Query["pathology.viralLoad"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.haemoglobin"])) model.pathology.haemoglobin = decimal.Parse(Request.Query["pathology.haemoglobin"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.bilirubin"])) model.pathology.bilirubin = decimal.Parse(Request.Query["pathology.bilirubin"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.totalCholestrol"])) model.pathology.totalCholestrol = decimal.Parse(Request.Query["pathology.totalCholestrol"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.hdl"])) model.pathology.hdl = decimal.Parse(Request.Query["pathology.hdl"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.ldl"])) model.pathology.ldl = decimal.Parse(Request.Query["pathology.ldl"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.triglycerides"])) model.pathology.triglycerides = decimal.Parse(Request.Query["pathology.triglycerides"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.glucose"])) model.pathology.glucose = decimal.Parse(Request.Query["pathology.glucose"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.hba1c"])) model.pathology.hba1c = decimal.Parse(Request.Query["pathology.hba1c"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.alt"])) model.pathology.alt = decimal.Parse(Request.Query["pathology.alt"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.ast"])) model.pathology.ast = decimal.Parse(Request.Query["pathology.ast"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.urea"])) model.pathology.urea = decimal.Parse(Request.Query["pathology.urea"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.creatinine"])) model.pathology.creatinine = decimal.Parse(Request.Query["pathology.creatinine"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.eGfr"])) model.pathology.eGfr = decimal.Parse(Request.Query["pathology.eGfr"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.mauCreatRatio"])) model.pathology.mauCreatRatio = decimal.Parse(Request.Query["pathology.mauCreatRatio"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.FEV1"])) model.pathology.FEV1 = decimal.Parse(Request.Query["pathology.FEV1"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.Eosinophylia"])) model.pathology.Eosinophylia = decimal.Parse(Request.Query["pathology.Eosinophylia"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.ucreatinine"])) model.pathology.ucreatinine = decimal.Parse(Request.Query["pathology.ucreatinine"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.salbumin"])) model.pathology.salbumin = decimal.Parse(Request.Query["pathology.salbumin"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.ualbuminuria"])) model.pathology.ualbuminuria = decimal.Parse(Request.Query["pathology.ualbuminuria"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.systolicBP"])) model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(Request.Query["pathology.diastolicBP"])) model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                    #endregion
                    #region effective-date-insert hcare-1159
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["effectiveDate"]);
                    if (Request.Query["pathology.CD4Count"] == "") { model.pathology.CD4CounteffectiveDate = null; } else { model.pathology.CD4CounteffectiveDate = Convert.ToDateTime(Request.Query["CD4CounteffectiveDate"]); }
                    if (Request.Query["pathology.CD4Percentage"] == "") { model.pathology.CD4PercentageeffectiveDate = null; } else { model.pathology.CD4PercentageeffectiveDate = Convert.ToDateTime(Request.Query["CD4PercentageeffectiveDate"]); }
                    if (Request.Query["pathology.viralLoad"] == "") { model.pathology.viralLoadeffectiveDate = null; } else { model.pathology.viralLoadeffectiveDate = Convert.ToDateTime(Request.Query["viralLoadeffectiveDate"]); }
                    if (Request.Query["pathology.haemoglobin"] == "") { model.pathology.haemoglobineffectiveDate = null; } else { model.pathology.haemoglobineffectiveDate = Convert.ToDateTime(Request.Query["haemoglobineffectiveDate"]); }
                    if (Request.Query["pathology.bilirubin"] == "") { model.pathology.bilirubineffectiveDate = null; } else { model.pathology.bilirubineffectiveDate = Convert.ToDateTime(Request.Query["bilirubineffectiveDate"]); }
                    if (Request.Query["pathology.totalCholestrol"] == "") { model.pathology.totalCholestroleffectiveDate = null; } else { model.pathology.totalCholestroleffectiveDate = Convert.ToDateTime(Request.Query["totalCholestroleffectiveDate"]); }
                    if (Request.Query["pathology.hdl"] == "") { model.pathology.hdleffectiveDate = null; } else { model.pathology.hdleffectiveDate = Convert.ToDateTime(Request.Query["hdleffectiveDate"]); }
                    if (Request.Query["pathology.ldl"] == "") { model.pathology.ldleffectiveDate = null; } else { model.pathology.ldleffectiveDate = Convert.ToDateTime(Request.Query["ldleffectiveDate"]); }
                    if (Request.Query["pathology.triglycerides"] == "") { model.pathology.triglycerideseffectiveDate = null; } else { model.pathology.triglycerideseffectiveDate = Convert.ToDateTime(Request.Query["triglycerideseffectiveDate"]); }
                    if (Request.Query["pathology.glucose"] == "") { model.pathology.glucoseeffectiveDate = null; } else { model.pathology.glucoseeffectiveDate = Convert.ToDateTime(Request.Query["glucoseeffectiveDate"]); }
                    if (Request.Query["pathology.hba1c"] == "") { model.pathology.hba1ceffectiveDate = null; } else { model.pathology.hba1ceffectiveDate = Convert.ToDateTime(Request.Query["hba1ceffectiveDate"]); }
                    if (Request.Query["pathology.alt"] == "") { model.pathology.alteffectiveDate = null; } else { model.pathology.alteffectiveDate = Convert.ToDateTime(Request.Query["alteffectiveDate"]); }
                    if (Request.Query["pathology.ast"] == "") { model.pathology.asteffectiveDate = null; } else { model.pathology.asteffectiveDate = Convert.ToDateTime(Request.Query["asteffectiveDate"]); }
                    if (Request.Query["pathology.urea"] == "") { model.pathology.ureaeffectiveDate = null; } else { model.pathology.ureaeffectiveDate = Convert.ToDateTime(Request.Query["ureaeffectiveDate"]); }
                    if (Request.Query["pathology.creatinine"] == "") { model.pathology.creatinineeffectiveDate = null; } else { model.pathology.creatinineeffectiveDate = Convert.ToDateTime(Request.Query["creatinineeffectiveDate"]); }
                    if (Request.Query["pathology.eGfr"] == "") { model.pathology.eGfreffectiveDate = null; } else { model.pathology.eGfreffectiveDate = Convert.ToDateTime(Request.Query["eGfreffectiveDate"]); }
                    if (Request.Query["pathology.mauCreatRatio"] == "") { model.pathology.mauCreatRatioeffectiveDate = null; } else { model.pathology.mauCreatRatioeffectiveDate = Convert.ToDateTime(Request.Query["mauCreatRatioeffectiveDate"]); }
                    if (Request.Query["pathology.FEV1"] == "") { model.pathology.FEV1effectiveDate = null; } else { model.pathology.FEV1effectiveDate = Convert.ToDateTime(Request.Query["FEV1effectiveDate"]); }
                    if (Request.Query["pathology.Eosinophylia"] == "") { model.pathology.EosinophyliaeffectiveDate = null; } else { model.pathology.EosinophyliaeffectiveDate = Convert.ToDateTime(Request.Query["EosinophyliaeffectiveDate"]); }
                    if (Request.Query["pathology.hivEliza"] == "") { model.pathology.hivElizaeffectiveDate = null; } else { model.pathology.hivElizaeffectiveDate = Convert.ToDateTime(Request.Query["hivElizaeffectiveDate"]); }
                    if (Request.Query["pathology.ucreatinine"] == "") { model.pathology.ucreatinineeffectiveDate = null; } else { model.pathology.ucreatinineeffectiveDate = Convert.ToDateTime(Request.Query["ucreatinineeffectiveDate"]); }
                    if (Request.Query["pathology.normGtt"] == "") { model.pathology.normGtteffectiveDate = null; } else { model.pathology.normGtteffectiveDate = Convert.ToDateTime(Request.Query["normGtteffectiveDate"]); }
                    if (Request.Query["pathology.abnGtt"] == "") { model.pathology.abnGtteffectiveDate = null; } else { model.pathology.abnGtteffectiveDate = Convert.ToDateTime(Request.Query["abnGtteffectiveDate"]); }
                    if (Request.Query["pathology.salbumin"] == "") { model.pathology.salbumineffectiveDate = null; } else { model.pathology.salbumineffectiveDate = Convert.ToDateTime(Request.Query["salbumineffectiveDate"]); }
                    if (Request.Query["pathology.ualbuminuria"] == "") { model.pathology.ualbuminuriaeffectiveDate = null; } else { model.pathology.ualbuminuriaeffectiveDate = Convert.ToDateTime(Request.Query["ualbuminuriaeffectiveDate"]); }
                    if (Request.Query["pathology.diastolicBP"] == "" && Request.Query["pathology.systolicBP"] == "") { model.pathology.BPeffectiveDate = null; } else { model.pathology.BPeffectiveDate = Convert.ToDateTime(Request.Query["BPeffectiveDate"]); }
                    #endregion

                    model.pathology.comment = Request.Query["pathology.comment"];
                    model.pathology.active = true;
                    #region HCare-742 future-date
                    if (model.pathology.effectiveDate > DateTime.Today || model.pathology.CD4CounteffectiveDate > DateTime.Today || model.pathology.CD4PercentageeffectiveDate > DateTime.Today || model.pathology.viralLoadeffectiveDate > DateTime.Today ||
                        model.pathology.haemoglobineffectiveDate > DateTime.Today || model.pathology.totalCholestroleffectiveDate > DateTime.Today || model.pathology.hdleffectiveDate > DateTime.Today || model.pathology.ldleffectiveDate > DateTime.Today ||
                        model.pathology.triglycerideseffectiveDate > DateTime.Today || model.pathology.glucoseeffectiveDate > DateTime.Today || model.pathology.hba1ceffectiveDate > DateTime.Today || model.pathology.alteffectiveDate > DateTime.Today ||
                        model.pathology.asteffectiveDate > DateTime.Today || model.pathology.ureaeffectiveDate > DateTime.Today || model.pathology.creatinineeffectiveDate > DateTime.Today || model.pathology.eGfreffectiveDate > DateTime.Today ||
                        model.pathology.mauCreatRatioeffectiveDate > DateTime.Today || model.pathology.FEV1effectiveDate > DateTime.Today || model.pathology.EosinophyliaeffectiveDate > DateTime.Today || model.pathology.hivElizaeffectiveDate > DateTime.Today ||
                        model.pathology.ucreatinineeffectiveDate > DateTime.Today || model.pathology.normGtteffectiveDate > DateTime.Today || model.pathology.abnGtteffectiveDate > DateTime.Today || model.pathology.salbumineffectiveDate > DateTime.Today ||
                        model.pathology.ualbuminuriaeffectiveDate > DateTime.Today || model.pathology.BPeffectiveDate > DateTime.Today)
                    {
                        ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
                        ViewBag.futuredateError = "Date cannot be set to future date";
                        return View(model);
                    }
                    #endregion

                    var result = _member.InsertPathology(model.pathology);
                    #endregion
                    #region clinical-rules
                    if (result.Success)
                    {
                        var paths = _member.GetPathology(model.pathology.dependentID);
                        if (paths.Count() >= 1)//HCare-976
                        {
                            var clinicalRulesEngine = new ClinicalRulesEngine();
                            clinicalRulesEngine.CheckPathology(model.pathology.dependentID, model.pathology.pathologyID);
                            //clinicalRulesEngine.CoMorbidsRules(paths);

                            //var res = _member.GetEnrolmentStep(model.pathology.dependentID);
                            //if (res != null)
                            //{
                            //    res.pathologyCaptured = true;
                            //    var update = _member.UpdateEnrolmentStep(res);
                            //}
                        }
                    }
                    #endregion
                    #region attachment
                    if (model.attachment != null)
                    {
                        model.attachment.attachmentName = "Pathology Attachment";
                        model.attachment.attachmentType = "002";
                        model.attachment.createdDate = DateTime.Now;
                        model.attachment.Active = true;

                        var file = Request.Form.Files["Pathfile"];
                        if (file.HasFile())
                        {
                            string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                            string filename = Path.GetFileName(Request.Form.Files["Pathfile"].FileName.Replace("<", "").Replace(">", ""));
                            var filePath = Path.Combine(path, model.attachment.dependentID + "_" + filename);
                            using (var stream = System.IO.File.Create(filePath))
                            {
                                file.CopyToAsync(stream);
                            }

                            model.attachment.Link = model.attachment.dependentID + "_" + filename;
                            model.attachment.createdBy = User.Identity.Name;
                            _member.InsertAttachment(model.attachment);
                        }
                    }
                    #endregion

                    return new RedirectResult(Url.Action("PatientClinical_Pathology", "Member", new { DependentID = model.pathology.dependentID, pro = Request.Query["pro"] }));
                }
                else
                {
                    ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");
                    ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
                    ViewBag.Message = "This lab reference number already exists!";
                    return View(model);
                }
            }
            else
            {
                ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
                ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");
                ViewBag.attachmentError = "If pathology entry does not require an attachment, please check the No attachment box";
                return View(model);
            }

        }

        public ActionResult _InsertText(Guid DependentID, int id, int taskId, Guid? pro)
        {
            var model = new ComsViewModel();
            model.smsMessages = new SmsMessages();



            var contacts = _member.GetContact(DependentID);

            if (contacts != null)
            {
                if (string.IsNullOrEmpty(contacts.cell)) { model.smsMessages.cell_no = contacts.ISeriesCell; } else { model.smsMessages.cell_no = contacts.cell; }
            }
            //var task = _clinical.GetTaskView(taskId);

            //if (!String.IsNullOrEmpty(task.templateID))
            //{
            //    model.smsMessages.message = _clinical.GetSmsTemplate(Convert.ToInt16(task.templateID));
            //    model.smsMessages.messageID = Convert.ToInt16(task.templateID);
            //}

            model.smsMessages.dependantID = DependentID;
            model.smsMessages.effectiveDate = DateTime.Now;

            //HCare-1043
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var det = _member.GetMemberByDependantID(DependentID);

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID); //HCare-1380
            var program = _admin.GetProgramById(new Guid(pro.ToString())); //HCare-1380
            ViewBag.ReferenceID = id;
            ViewBag.TaskID = taskId;
            ViewBag.MedicalAid = medicalaid.Name; //HCare-1380
            ViewBag.ProgramID = program.programID; //HCare-1380
            ViewBag.Program = program.ProgramName; //HCare-1380
            ViewBag.UserID = User.Identity.Name; //hcare-1380

            var templates = _member.GetSmsMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//hcare-1380
            ViewBag.smsTemplate = new SelectList(templates, "templateId", "title"); //hcare-1380

            return View(model);
        }

        [HttpPost]
        public ActionResult _InsertText(ComsViewModel model)
        {
            model.smsMessages.programId = new Guid(Request.Query["programid"]); //hcare-1378
            model.smsMessages.templateID = Convert.ToInt16(Request.Query["sms-template"]); //hcare-1289
            model.smsMessages.createdBy = User.Identity.Name;
            model.smsMessages.createdDate = DateTime.Now;
            model.smsMessages.message = Request.Query["sms-text-message"];
            model.smsMessages.status = "Queued";
            if (String.IsNullOrEmpty(Request.Query["smsMessages.effectiveDate"])) { model.smsMessages.effectiveDate = DateTime.Now; } else { model.smsMessages.effectiveDate = Convert.ToDateTime(Request.Query["smsMessages.effectiveDate"]); }
            model.smsMessages.ReferenceNumber = Request.Query["referenceid"];
            model.smsMessages.Active = true;

            _member.InsertSMS(model.smsMessages);
            //_member.InsertText_HCDWL(model.dependantID.ToString(), model.cell_no, model.effectiveDate.ToString("dd MMM yyyy hh:mm:ss"), model.templateID, model.message, model.status, model.ReferenceNumber, model.programId.ToString(), model.createdBy, model.createdDate.ToString("dd MMM yyyy hh:mm:ss"));

            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskid"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["referenceid"], pro = Request.Query["programid"] });
        }

        public ActionResult _InsertEmail(Guid DependentID, int id, int taskId, Guid? pro)
        {
            var model = new ComsViewModel();
            model.emailMessages = new Emails();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.pro = pro; //hcare-1289

            model.dependant = _member.GetDependantDetails(DependentID, null);

            var contacts = _member.GetContact(DependentID);

            if (contacts != null) { model.emailMessages.emailTo = contacts.email; }
            var task = _clinical.GetTaskView(taskId);
            if (!String.IsNullOrEmpty(task.templateID)) { model.emailMessages.emailMassage = _clinical.GetEmailTemplate(Convert.ToInt16(task.templateID)); }
            model.emailMessages.dependantID = DependentID;
            model.emailMessages.effectivedate = DateTime.Now;

            //HCare-1197
            var det = _member.GetMemberByDependantID(DependentID);
            ViewBag.schemeSubject = _medical.GetMedicalAid(det.medicalAidID).medicalAidSettings.schemeSubject;
            var emailsubjectView = _member.GetDependentDetails(DependentID, pro);
            ViewBag.MemberNo = emailsubjectView.member.membershipNo;
            ViewBag.Dep = emailsubjectView.dependent.dependentCode;
            ViewBag.Surname = emailsubjectView.dependent.lastName;
            ViewBag.Intial = emailsubjectView.dependent.initials;

            #region hcare-1380
            var emailTemplates = _member.GetEmailMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//HCare-1098
            ViewBag.emailTemplate = new SelectList(emailTemplates, "templateId", "title");

            model.EmailHistory = _member.GetEmails(DependentID, pro);
            model.AttachmentTemplates = _admin.GetAttachmentTemplateByAccount(det.medicalAidID, new Guid(pro.ToString())); //HCare-1380
            model.EmailAttachmentHistories = _admin.GetEmailAttachmentByDependantID(DependentID, new Guid(pro.ToString()), DateTime.Today); //HCare-1380

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID); //HCare-1380
            var program = _admin.GetProgramById(new Guid(pro.ToString())); //HCare-1380
            ViewBag.DependantID = DependentID; //HCare-1380
            ViewBag.MedicalAid = medicalaid.Name; //HCare-1380
            ViewBag.MedicalAidID = medicalaid.MedicalAidID; //hcare-1380
            ViewBag.ProgramID = program.programID; //HCare-1380
            ViewBag.Program = program.ProgramName; //HCare-1380
            ViewBag.UserID = User.Identity.Name; //hcare-1380
            ViewBag.MemberName = model.dependant.firstName_UC + " " + model.dependant.lastName_UC; //hcare-1384

            model.EmailHeader = _admin.GetEmailLayoutByAccount(det.medicalAidID, new Guid(pro.ToString())).Where(x => x.LayoutType.ToLower() == "header").ToList(); //hcare-1384
            model.EmailFooter = _admin.GetEmailLayoutByAccount(det.medicalAidID, new Guid(pro.ToString())).Where(x => x.LayoutType.ToLower() == "footer").ToList(); //hcare-1384

            if (model.EmailHeader.Count != 0) { ViewBag.Header = model.EmailHeader[0].Id; } else { ViewBag.Header = null; } //hcare-1384
            if (model.EmailFooter.Count != 0) { ViewBag.Footer = model.EmailFooter[0].Id; } else { ViewBag.Footer = null; } //hcare-1384

            #endregion

            return View(model);
        }

        [HttpGet]
        public ActionResult _Call(Guid DependentID, int id, int taskId, Guid? pro)
        {
            var cont = new Contact();
            cont = _member.GetContact(DependentID);
            var drcontant = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == pro).ToList(); //HCare-1386
            var member = _member.GetDependentDetails(DependentID, null);
            Guid? programCode;
            if (Request.Query["pro"].ToString() == null)
            {
                programCode = _member.GetPrograms().Select(x => x.programID).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            }

            ViewBag.pro = programCode;
            var model = new GeneralContactViewModel();
            model.PatientName = member.dependent.firstName + " " + member.dependent.lastName;
            model.membershipNo = member.member.membershipNo;
            model.IdNumber = member.dependent.idNumber;

            if (member.contact != null)
            {
                model.ContactID = member.contact.ContactID;
            }

            if (cont != null)
            {
                model.contactName = cont.contactName;
                model.contactNumber = cont.landLine;
                model.cell = cont.cell;
                model.email = cont.email;
                model.landLine = cont.landLine;
                model.workNo = cont.workNo;
                model.fax = cont.fax;

            }
            if (drcontant.Count > 0)
            {
                model.drPracticeNo = drcontant[0].practiceNo;
                model.drContactName = drcontant[0].doctorName;
                model.drContactNumber = drcontant[0].tel;
                model.drcell = drcontant[0].cell;
            }
            ViewBag.task = taskId;
            ViewBag.id = id;
            return View(model);
        }

        [HttpPost]
        public ActionResult _Call()
        {
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";
            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _ChangeStatus(Guid DependentID, int id, int taskId, Guid? pro)
        {
            //HCare-1259
            var model = new PatientManagementStatusHistory();
            var viewmodel = new ManagementStatusVM();
            viewmodel.dependantId = DependentID;

            var managementStatusses = _member.GetManagementStatus().Where(x => x.active == true);
            model.dependantId = DependentID;
            ViewBag.task = taskId;
            ViewBag.id = id;
            Guid? programCode;
            if (Request.Query["pro"].ToString() == null)
            {
                programCode = _member.GetPrograms().Select(x => x.programID).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
                var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                var det = _member.GetMemberByDependantID(DependentID);
                managementStatusses = _member.GetManagementStatusesByMedicalAid(det.medicalAidID).Where(x => x.programCode == programcode);
            }

            ViewBag.pro = programCode;
            var progcode = _admin.GetProgramById(new Guid(programCode.ToString()));
            viewmodel.ManagementStatusVMs = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == progcode.code).ToList();

            var memberinformation = _member.GetMemberByDependantID(DependentID);
            var laststatus = _member.GetManagmentStatusInformation(DependentID).Where(x => x.active == true).Where(x => x.programCode == progcode.code).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var PreClosedStatus = _member.GetManagmentStatusInformation(DependentID).Where(x => x.active == true).Where(x => x.programCode == progcode.code).OrderByDescending(x => x.effectiveDate).Skip(1).FirstOrDefault();
            var statushistory = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == progcode.code).Where(x => x.active == true).ToList();
            var statusesses = _member.GetManagementStatusesByMedicalAid(memberinformation.medicalAidID).Where(x => x.programCode == progcode.code);

            if (laststatus != null)
            {
                var memberStatuss = _member.GetManagementStatusByDependentId(DependentID, progcode.programID).Where(x => x.active == true).ToList();
                var enrolmentStep = statusesses.Where(m => memberStatuss.Any(c => c.managementStatusCode.Equals(m.statusName))).Select(m => m.enrolmentStage).Max(); //if it breaks here, check the account's managment status list for the scheme and option
                if (enrolmentStep == 0)
                {
                    statusesses = statusesses.Where(x => x.enrolmentStage != 2 && x.enrolmentStage != 3);
                }
                else if (enrolmentStep == 1)
                {
                    statusesses = statusesses.Where(x => x.enrolmentStage != 1 && x.enrolmentStage != 3);
                }
                else if (enrolmentStep == 2)
                {
                    statusesses = statusesses.Where(x => x.enrolmentStage != 2 && x.enrolmentStage != 1);
                }
                else
                {
                    statusesses = statusesses.Where(x => x.enrolmentStage != 1 && x.enrolmentStage != 2 && x.enrolmentStage != 3);
                }

                foreach (var history in statushistory)
                {
                    if (history.managementStatusCode == "ERD" || history.managementStatusCode == "ERH" || history.managementStatusCode == "ERM")
                    {
                        if (User.HasRole("6. Global user") || User.HasRole("5. Super user"))
                        {
                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode), "statusCode", "statusName"); //checks what the current status is and removes from the dropdown menu to avoid duplicating
                        }
                        else
                        {
                            if (laststatus.codeStatus.ToLower() == "o")
                            {
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1259-correction
                            }
                            else if (laststatus.codeStatus.ToLower() == "c")
                            {
                                if (PreClosedStatus.codeStatus.ToLower() == "o")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode).Where(x => x.programCode == progcode.code).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                                }
                                else if (PreClosedStatus.codeStatus.ToLower() == "p")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode).Where(x => x.programCode == progcode.code).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                                }
                            }
                            else
                            {
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                            }
                        }
                    }
                    else
                    {
                        ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode != laststatus.managementStatusCode), "statusCode", "statusName");
                    }
                }
            }
            else
            {
                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode == "ERD" || x.statusCode == "ERH" || x.statusCode == "ERM"), "statusCode", "statusName");
            }

            return View(viewmodel);
        }
        [HttpPost]
        public ActionResult _ChangeStatus(PatientManagementStatusHistory model, Guid? pro)
        {
            var programcode = _member.GetManagementStatusByCode().Where(x => x.statusCode == Request.Query["managementStatus"].ToString()).Select(x => x.programCode).FirstOrDefault();
            var lastStatus = _member.GetManagmentStatusInformation(model.dependantId).Where(x => x.active == true).Where(x => x.endDate == null).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var lastStatus_Closed = _member.GetManagmentStatusInformation(model.dependantId).Where(x => x.active == true).FirstOrDefault();
            model.managementStatusCode = Request.Query["managementStatus"].ToString();
            model.createdBy = User.Identity.Name;

            if (lastStatus == null)
            {
                ServiceResult result = _member.InsertManagementStatus(model);
            }
            else if (lastStatus.managementStatusCode != model.managementStatusCode)//HCare-755 (Validation check for Management Status create)
            {
                if (lastStatus != null)
                {
                    var statusUpdate = new PatientManagementStatusHistory()
                    {
                        id = lastStatus.id,
                        managementStatusCode = lastStatus.managementStatusCode,
                        createdBy = lastStatus.createdBy,
                        createdDate = lastStatus.createdDate,
                        active = lastStatus.active,
                        modifiedBy = User.Identity.Name,
                        modifiedDate = DateTime.Now,
                        effectiveDate = lastStatus.effectiveDate,
                    };

                    if (lastStatus_Closed.codeStatus.ToLower() != "c") //HCare-786(This leaves the previous date alone if the status was closed. Allows for time lapse as per HCare-786)
                    {
                        statusUpdate.endDate = model.effectiveDate.Value.AddDays(-1);
                    }

                    var results = _member.UpdateManagementStatus(statusUpdate);
                }

                //HCare-792(Create assignment when management status is changed to DM Enrolment)
                if (model.managementStatusCode == "ERD" || model.managementStatusCode == "ERH")
                {
                    var assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = model.dependantId,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open"
                        },
                    };
                    if (model.managementStatusCode == "ERD")
                    {
                        assignment.assignmentItemType = "NEWRT";
                        assignment.newAssignment.Instruction = "Management status updated to Enrolment Diabetes";
                    }
                    if (model.managementStatusCode == "ERH") //HCare-948
                    {
                        assignment.assignmentItemType = "NEWR";
                        assignment.newAssignment.Instruction = "Management status updated to Enrolment HIV";
                    }

                    var assignExists = _member.GetAssignment(model.dependantId, assignment.assignmentItemType);
                    if (assignExists == null)
                    {
                        var assignmentInsert = _member.InsertAssignment(assignment);

                        var assignmentTask = new AssignmentItemTasks();
                        assignmentTask.assignmentItemID = assignment.itemID;
                        var tasks = _member.GetTaskRequirements(assignment.assignmentItemType);
                        foreach (var tas in tasks)
                        {
                            assignmentTask.taskTypeId = tas.taskType;
                            assignmentTask.requirementId = tas.requirementId;
                            _member.InsertTask(assignmentTask);
                        }
                    }
                }

                //HCare-669 (Default end date to effective date when status is set to Patient resigned/Deactivated/Deceased)
                var Status_Closed = _member.GetManagmentStatusByCode(model.managementStatusCode);
                if (Status_Closed.codeStatus.ToLower() == "c")
                {
                    model.endDate = model.effectiveDate;

                }

                ServiceResult result = _member.InsertManagementStatus(model);

            }

            #region assignment-task
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            var res = _clinical.UpdateTask(task);
            #endregion

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        [HttpPost]
        
        public ActionResult _InsertEmail(ComsViewModel model, Guid? pro)
        {

            var dependantID = Request.Query["e-dependantid"];
            var programID = Request.Query["e-programid"];
            var medicalaidID = Request.Query["e-medicalaidid"];

            var emailhistory = _admin.GetEmailAttachmentByDependantID(new Guid(dependantID), new Guid(programID), DateTime.Today);

            #region email-insert
            model.emailMessages.createdDate = DateTime.Now;
            model.emailMessages.effectivedate = DateTime.Now;
            model.emailMessages.createdBy = User.Identity.Name;
            model.emailMessages.programId = new Guid(pro.ToString()); //HCare-1254
            model.emailMessages.status = "Pending";

            _member.InsertEmail(model.emailMessages);

            #region hcare-1384: email-layouts
            string root = AppDomain.CurrentDomain.BaseDirectory + "uploads//templates//layout//";

            string header = null;
            string footer = null;
            string h_dimensions = null;
            string f_dimensions = null;

            string header_image = "default-space.jpg";
            string header_height = "0";
            string header_width = "0";

            string footer_image = "default-space.jpg";
            string footer_height = "0";
            string footer_width = "0";

            var layoutinformation = _admin.GetEmailLayoutByAccount(new Guid(medicalaidID), new Guid(programID)); //hcare-1384

            foreach (var item in layoutinformation)
            {
                if (item.LayoutType.ToLower().Contains("header"))
                {
                    header_image = item.FileName;
                    header_height = item.LayoutHeight;
                    header_width = item.LayoutWidth;
                    break;
                }
            }
            foreach (var item in layoutinformation)
            {
                if (item.LayoutType.ToLower().Contains("footer"))
                {
                    footer_image = item.FileName;
                    footer_height = item.LayoutHeight;
                    footer_width = item.LayoutWidth;
                    break;
                }
            }

            header = root + header_image;
            h_dimensions = "height='" + header_height + "', width='" + header_width + "'";
            footer = root + footer_image;
            f_dimensions = "height='" + footer_height + "', width='" + footer_width + "'";
            #endregion

            #endregion
            #region email-attachment-insert
            if (emailhistory.Count > 0)
            {
                var attachments = "";
                foreach (var item in emailhistory)
                {
                    var attachment = _admin.GetAttachmentTemplateByID(item.TemplateID);
                    attachments += attachment.FileName + ',';

                    item.ModifiedBy = User.Identity.Name;
                    item.ModifiedDate = DateTime.Now;
                    item.EmailID = model.emailMessages.emailId;
                    item.Sent = true;

                    _admin.UpdateEmailAttachmentHistory(item);

                }
                attachments = attachments.TrimEnd(',');

                var emailsend = SendEmail(model.emailMessages.emailTo, model.emailMessages.cc, model.emailMessages.subject, header, h_dimensions, model.emailMessages.emailMassage, footer, f_dimensions, attachments, medicalaidID);
                #region hcare-1448: update-email
                var email = _member.GetMemberEmailByID(model.emailMessages.emailId);
                if (emailsend.ToLower() == "sent")
                {
                    if (email != null)
                    {
                        email.status = "Sent";
                        _member.UpdateEmailStatus(email);
                    }
                }
                else
                {
                    email.status = "Error: " + emailsend;
                    _member.UpdateEmailStatus(email);
                }
                #endregion
            }
            else
            {
                var emailsend = SendEmail(model.emailMessages.emailTo, model.emailMessages.cc, model.emailMessages.subject, header, h_dimensions, model.emailMessages.emailMassage, footer, f_dimensions, "", medicalaidID);
                #region hcare-1448: update-email
                var email = _member.GetMemberEmailByID(model.emailMessages.emailId);
                if (emailsend.ToLower() == "sent")
                {
                    if (email != null)
                    {
                        email.status = "Sent";
                        _member.UpdateEmailStatus(email);
                    }
                }
                else
                {
                    email.status = "Error: " + emailsend;
                    _member.UpdateEmailStatus(email);
                }
                #endregion
            }
            #endregion

            //update-task
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = pro }); //hcare-1289
        }

        public string SendEmail(string to, string copy, string subject, string h_image, string h_dimensions, string body, string f_image, string f_dimensions, string attachments, string medicalaidID) //hcare-1380 //hcare-1384
        {
            try
            {
                string htmlBody = string.Format("<html><body><img src=\"cid:Header\" {1}/><br />{0}<br /><img src=\"cid:Footer\" {2}/></body></html>", body, h_dimensions, f_dimensions);
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.Default, System.Net.Mime.MediaTypeNames.Text.Html);

                // Create a LinkedResource object for each embedded image
                LinkedResource header = new LinkedResource(h_image, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                header.ContentId = "Header";
                avHtml.LinkedResources.Add(header);

                LinkedResource footer = new LinkedResource(f_image, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                footer.ContentId = "Footer";
                avHtml.LinkedResources.Add(footer);

                var from = "";
                var maSettings = _admin.GetMedicalAidSettings(new Guid(medicalaidID));
                if (maSettings != null) { if (!String.IsNullOrEmpty(maSettings.email)) { from = maSettings.email; } else { from = "support@halocare.co.za"; } } else { from = "support@halocare.co.za"; } //hcare-1380: correction

                using (MailMessage mm = new MailMessage(from, to))
                {
                    if (!String.IsNullOrEmpty(copy)) { mm.CC.Add(copy); }
                    mm.Subject = subject;
                    mm.Body = body;
                    string[] files = Directory.GetFiles(Server.MapPath("~/uploads\\templates\\attachments"));
                    if (!String.IsNullOrEmpty(attachments))
                    {
                        foreach (string file in files)
                        {
                            foreach (string attachment in attachments.Split(','))
                            {
                                if (file.Contains(attachment))
                                {
                                    string fileName = Path.GetFileName(file);
                                    mm.Attachments.Add(new Attachment(file));
                                    break;
                                }
                            }
                        }
                    }
                    mm.IsBodyHtml = true;
                    mm.AlternateViews.Add(avHtml);

                    var smtp = new SmtpClient("192.168.60.20");
                    smtp.Send(mm);
                }

                return "sent";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public ActionResult _ViewPathology(Guid DependentID, int id, Guid? pro)
        {
            Pathology model = new Pathology();
            model = _member.GetPathologyById(id);
            return View(model);
        }

        public ActionResult _InsertPathology(Guid DependentID, int id, int taskId, Guid? pro) //HCare-1062
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            ViewBag.programId = programCode;

            ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.code.Substring(0, 3)), "id", "pathologyType");
            ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");

            ViewBag.task = taskId;
            ViewBag.id = id;
            Pathology model = new Pathology();
            model.effectiveDate = DateTime.Now;
            model.dependentID = DependentID;
            return View(model);
        }

        [HttpPost]
        public ActionResult _InsertPathology(Pathology model, Guid? pro) //HCare-1062
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            ViewBag.programId = programCode;

            var id = Convert.ToInt32(Request.Query["id"].ToString());
            var taskID = Convert.ToInt32(Request.Query["taskId"].ToString());

            if (!String.IsNullOrEmpty(Request.Query["logid"].ToString()))
            {
                var logid = Convert.ToInt32(Request.Query["logid"].ToString());
                var logs = _member.GetAssignmentItemLogs(logid);
                logs.captured = true;
                logs.modifiedBy = User.Identity.Name;
                logs.modifiedDate = DateTime.Now;
                var res = _member.UpdateAssignmentLog(logs);
                var item = _clinical.GetAssignmentItemById(logs.assignmentItemID);
                item.capturedId = model.pathologyID;
                item.modifiedBy = User.Identity.Name;
                item.modifiedDate = DateTime.Now;
                var ares = _clinical.UpdateAssignmentItem(item);
            }

            var labreference = _member.GetPathology(model.dependentID).Where(x => x.labReferenceNo == model.labReferenceNo).ToList();
            if (labreference.Count() == 0)
            {
                #region pathology-insert
                //HCare-674
                model.createdDate = DateTime.Now;
                model.createdBy = User.Identity.Name;
                model.labReferenceNo = Request.Query["labReferenceNo"];
                model.labName = Request.Query["laboratories"];
                model.pathologyType = Request.Query["pathologyType"];
                #region decimal-insert //HCare-1050
                if (!String.IsNullOrEmpty(Request.Query["CD4Count"])) model.CD4Count = decimal.Parse(Request.Query["CD4Count"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["CD4Percentage"])) model.CD4Percentage = decimal.Parse(Request.Query["CD4Percentage"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["viralLoad"])) model.viralLoad = decimal.Parse(Request.Query["viralLoad"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["haemoglobin"])) model.haemoglobin = decimal.Parse(Request.Query["haemoglobin"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["bilirubin"])) model.bilirubin = decimal.Parse(Request.Query["bilirubin"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["totalCholestrol"])) model.totalCholestrol = decimal.Parse(Request.Query["totalCholestrol"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["hdl"])) model.hdl = decimal.Parse(Request.Query["hdl"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["ldl"])) model.ldl = decimal.Parse(Request.Query["ldl"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["triglycerides"])) model.triglycerides = decimal.Parse(Request.Query["triglycerides"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["glucose"])) model.glucose = decimal.Parse(Request.Query["glucose"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["hba1c"])) model.hba1c = decimal.Parse(Request.Query["hba1c"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["alt"])) model.alt = decimal.Parse(Request.Query["alt"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["ast"])) model.ast = decimal.Parse(Request.Query["ast"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["urea"])) model.urea = decimal.Parse(Request.Query["urea"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["creatinine"])) model.creatinine = decimal.Parse(Request.Query["creatinine"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["eGfr"])) model.eGfr = decimal.Parse(Request.Query["eGfr"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["mauCreatRatio"])) model.mauCreatRatio = decimal.Parse(Request.Query["mauCreatRatio"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["FEV1"])) model.FEV1 = decimal.Parse(Request.Query["FEV1"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["Eosinophylia"])) model.Eosinophylia = decimal.Parse(Request.Query["Eosinophylia"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["ucreatinine"])) model.ucreatinine = decimal.Parse(Request.Query["ucreatinine"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["salbumin"])) model.salbumin = decimal.Parse(Request.Query["salbumin"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["ualbuminuria"])) model.ualbuminuria = decimal.Parse(Request.Query["ualbuminuria"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["systolicBP"])) model.systolicBP = decimal.Parse(Request.Query["systolicBP"], CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(Request.Query["diastolicBP"])) model.diastolicBP = decimal.Parse(Request.Query["diastolicBP"], CultureInfo.InvariantCulture);
                #endregion
                #region effecitveDates
                if (Request.Query["CD4Count"] == "")
                    model.CD4CounteffectiveDate = null;

                if (Request.Query["CD4Percentage"] == "")
                    model.CD4PercentageeffectiveDate = null;

                if (Request.Query["viralLoad"] == "")
                    model.viralLoadeffectiveDate = null;

                if (Request.Query["haemoglobin"] == "")
                    model.haemoglobineffectiveDate = null;

                if (Request.Query["bilirubin"] == "")
                    model.bilirubineffectiveDate = null;

                if (Request.Query["totalCholestrol"] == "")
                    model.totalCholestroleffectiveDate = null;

                if (Request.Query["hdl"] == "")
                    model.hdleffectiveDate = null;

                if (Request.Query["ldl"] == "")
                    model.ldleffectiveDate = null;

                if (Request.Query["triglycerides"] == "")
                    model.triglycerideseffectiveDate = null;

                if (Request.Query["glucose"] == "")
                    model.glucoseeffectiveDate = null;

                if (Request.Query["hba1c"] == "")
                    model.hba1ceffectiveDate = null;

                if (Request.Query["alt"] == "")
                    model.alteffectiveDate = null;

                if (Request.Query["ast"] == "")
                    model.asteffectiveDate = null;

                if (Request.Query["urea"] == "")
                    model.ureaeffectiveDate = null;

                if (Request.Query["creatinine"] == "")
                    model.creatinineeffectiveDate = null;

                if (Request.Query["eGfr"] == "")
                    model.eGfreffectiveDate = null;

                if (Request.Query["mauCreatRatio"] == "")
                    model.mauCreatRatioeffectiveDate = null;

                if (Request.Query["FEV1"] == "")
                    model.FEV1effectiveDate = null;

                if (Request.Query["Eosinophylia"] == "")
                    model.EosinophyliaeffectiveDate = null;

                if (Request.Query["hivEliza"] == "")
                    model.hivElizaeffectiveDate = null;

                if (Request.Query["ucreatinine"] == "")
                    model.ucreatinineeffectiveDate = null;

                if (Request.Query["normGtt"] == "")
                    model.normGtteffectiveDate = null;

                if (Request.Query["abnGtt"] == "")
                    model.abnGtteffectiveDate = null;

                if (Request.Query["salbumin"] == "")
                    model.salbumineffectiveDate = null;

                if (Request.Query["ualbuminuria"] == "")
                    model.ualbuminuriaeffectiveDate = null;

                if (Request.Query["diastolicBP"] == "" && Request.Query["systolicBP"] == "")
                    model.BPeffectiveDate = null;
                #endregion
                model.comment = Request.Query["comment"];
                model.active = true;
                #region HCare-742 future-date
                if (model.effectiveDate > DateTime.Today || model.CD4CounteffectiveDate > DateTime.Today || model.CD4PercentageeffectiveDate > DateTime.Today || model.viralLoadeffectiveDate > DateTime.Today ||
                    model.haemoglobineffectiveDate > DateTime.Today || model.totalCholestroleffectiveDate > DateTime.Today || model.hdleffectiveDate > DateTime.Today || model.ldleffectiveDate > DateTime.Today ||
                    model.triglycerideseffectiveDate > DateTime.Today || model.glucoseeffectiveDate > DateTime.Today || model.hba1ceffectiveDate > DateTime.Today || model.alteffectiveDate > DateTime.Today ||
                    model.asteffectiveDate > DateTime.Today || model.ureaeffectiveDate > DateTime.Today || model.creatinineeffectiveDate > DateTime.Today || model.eGfreffectiveDate > DateTime.Today ||
                    model.mauCreatRatioeffectiveDate > DateTime.Today || model.FEV1effectiveDate > DateTime.Today || model.EosinophyliaeffectiveDate > DateTime.Today || model.hivElizaeffectiveDate > DateTime.Today ||
                    model.ucreatinineeffectiveDate > DateTime.Today || model.normGtteffectiveDate > DateTime.Today || model.abnGtteffectiveDate > DateTime.Today || model.salbumineffectiveDate > DateTime.Today ||
                    model.ualbuminuriaeffectiveDate > DateTime.Today || model.BPeffectiveDate > DateTime.Today)
                {
                    ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
                    ViewBag.futuredateError = "Date cannot be set to future date";
                    return View(model);
                }
                #endregion

                var result = _member.InsertPathology(model);
                #endregion
                #region clinical-rules
                if (result.Success)
                {
                    var paths = _member.GetPathology(model.dependentID);
                    if (paths.Count() >= 1)//HCare-976
                    {
                        var clinicalRulesEngine = new ClinicalRulesEngine();
                        clinicalRulesEngine.CheckPathology(model.dependentID, model.pathologyID);

                    }
                }
                #endregion

                //update assignmentitemtask
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";

                var tres = _clinical.UpdateTask(task);

                return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro });
            }
            else
            {
                ViewBag.id = id;
                ViewBag.task = taskID;

                ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
                ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");

                ModelState.AddModelError("labReferenceNo", "This lab reference number already exists!");

                return View(model);
            }



        }

        //public ActionResult _InsertPathology(Pathology model, Guid? pro)
        //{
        //    var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
        //    ViewBag.programId = programCode;
        //    var id = Convert.ToInt32(Request.Query["id"].ToString());
        //    var taskID = Convert.ToInt32(Request.Query["taskId"].ToString());

        //    if (!String.IsNullOrEmpty(Request.Query["logid"].ToString()))
        //    {
        //        var logid = Convert.ToInt32(Request.Query["logid"].ToString());
        //        var logs = _member.GetAssignmentItemLogs(logid);
        //        logs.captured = true;
        //        logs.modifiedBy = User.Identity.Name;
        //        logs.modifiedDate = DateTime.Now;
        //        var res = _member.UpdateAssignmentLog(logs);
        //        var item = _clinical.GetAssignmentItemById(logs.assignmentItemID);
        //        item.capturedId = model.pathologyID;
        //        item.modifiedBy = User.Identity.Name;
        //        item.modifiedDate = DateTime.Now;
        //        var ares = _clinical.UpdateAssignmentItem(item);
        //    }
        //    try
        //    {
        //        var pathos = _member.GetPathology(model.dependentID).Where(x => x.labReferenceNo == model.labReferenceNo).ToList();
        //        if (pathos.Count() == 0)
        //        {
        //            //HCare-674
        //            model.labReferenceNo = Request.Query["labReferenceNo"];
        //            model.pathologyType = Request.Query["pathologyType"];
        //            model.labName = Request.Query["laboratories"];
        //            model.createdBy = User.Identity.Name;
        //            model.createdDate = DateTime.Now;

        //            #region pathology fields
        //            if (Request.Query["CD4Count"] == "")
        //                model.CD4CounteffectiveDate = null;

        //            if (Request.Query["CD4Percentage"] == "")
        //                model.CD4PercentageeffectiveDate = null;

        //            if (Request.Query["viralLoad"] == "")
        //                model.viralLoadeffectiveDate = null;

        //            if (Request.Query["haemoglobin"] == "")
        //                model.haemoglobineffectiveDate = null;

        //            if (Request.Query["bilirubin"] == "")
        //                model.bilirubineffectiveDate = null;

        //            if (Request.Query["totalCholestrol"] == "")
        //                model.totalCholestroleffectiveDate = null;

        //            if (Request.Query["hdl"] == "")
        //                model.hdleffectiveDate = null;

        //            if (Request.Query["ldl"] == "")
        //                model.ldleffectiveDate = null;

        //            if (Request.Query["triglycerides"] == "")
        //                model.triglycerideseffectiveDate = null;

        //            if (Request.Query["glucose"] == "")
        //                model.glucoseeffectiveDate = null;

        //            if (Request.Query["hba1c"] == "")
        //                model.hba1ceffectiveDate = null;

        //            if (Request.Query["alt"] == "")
        //                model.alteffectiveDate = null;

        //            if (Request.Query["ast"] == "")
        //                model.asteffectiveDate = null;

        //            if (Request.Query["urea"] == "")
        //                model.ureaeffectiveDate = null;

        //            if (Request.Query["creatinine"] == "")
        //                model.creatinineeffectiveDate = null;

        //            if (Request.Query["eGfr"] == "")
        //                model.eGfreffectiveDate = null;

        //            if (Request.Query["mauCreatRatio"] == "")
        //                model.mauCreatRatioeffectiveDate = null;

        //            if (Request.Query["FEV1"] == "")
        //                model.FEV1effectiveDate = null;

        //            if (Request.Query["Eosinophylia"] == "")
        //                model.EosinophyliaeffectiveDate = null;

        //            if (Request.Query["hivEliza"] == "")
        //                model.hivElizaeffectiveDate = null;

        //            if (Request.Query["ucreatinine"] == "")
        //                model.ucreatinineeffectiveDate = null;

        //            if (Request.Query["normGtt"] == "")
        //                model.normGtteffectiveDate = null;

        //            if (Request.Query["abnGtt"] == "")
        //                model.abnGtteffectiveDate = null;

        //            if (Request.Query["salbumin"] == "")
        //                model.salbumineffectiveDate = null;

        //            if (Request.Query["ualbuminuria"] == "")
        //                model.ualbuminuriaeffectiveDate = null;

        //            if (Request.Query["diastolicBP"] == "" && Request.Query["systolicBP"] == "")
        //                model.BPeffectiveDate = null;

        //            #endregion
        //            model.comment = Request.Query["comment"];
        //            model.active = true;

        //            //HCare-742
        //            if (model.effectiveDate > DateTime.Today || model.CD4CounteffectiveDate > DateTime.Today || model.CD4PercentageeffectiveDate > DateTime.Today || model.viralLoadeffectiveDate > DateTime.Today ||
        //                model.haemoglobineffectiveDate > DateTime.Today || model.totalCholestroleffectiveDate > DateTime.Today || model.hdleffectiveDate > DateTime.Today || model.ldleffectiveDate > DateTime.Today ||
        //                model.triglycerideseffectiveDate > DateTime.Today || model.glucoseeffectiveDate > DateTime.Today || model.hba1ceffectiveDate > DateTime.Today || model.alteffectiveDate > DateTime.Today ||
        //                model.asteffectiveDate > DateTime.Today || model.ureaeffectiveDate > DateTime.Today || model.creatinineeffectiveDate > DateTime.Today || model.eGfreffectiveDate > DateTime.Today ||
        //                model.mauCreatRatioeffectiveDate > DateTime.Today || model.FEV1effectiveDate > DateTime.Today || model.EosinophyliaeffectiveDate > DateTime.Today || model.hivElizaeffectiveDate > DateTime.Today ||
        //                model.ucreatinineeffectiveDate > DateTime.Today || model.normGtteffectiveDate > DateTime.Today || model.abnGtteffectiveDate > DateTime.Today || model.salbumineffectiveDate > DateTime.Today ||
        //                model.ualbuminuriaeffectiveDate > DateTime.Today || model.BPeffectiveDate > DateTime.Today)
        //            {
        //                TempData["msg"] = "<script>alert('Pathology not captured due to future dates being set');</script>";
        //                return RedirectToAction("AssignmentDetails", "Clinical", new { id = id });
        //            }

        //            var result = _member.InsertPathology(model);

        //            if (result.Success)
        //            {
        //                var paths = _member.GetPathology(model.dependentID);
        //                if (paths.Count() > 1)
        //                {
        //                    var clinicalRulesEngine = new ClinicalRulesEngine();
        //                    clinicalRulesEngine.CheckPathology(model.dependentID, model.pathologyID);
        //                    clinicalRulesEngine.CoMorbidsRules(paths);

        //                    var res = _member.GetEnrolmentStep(model.dependentID);
        //                    if (res != null)
        //                    {
        //                        res.pathologyCaptured = true;
        //                        var update = _member.UpdateEnrolmentStep(res);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.id = id;
        //            ViewBag.task = taskID;

        //            ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
        //            ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");

        //            ModelState.AddModelError("labReferenceNo", "This lab reference number already exists!");

        //            return View(model);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes().Where(x => x.id == programCode.Substring(0, 3)), "id", "pathologyType");
        //        ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name");

        //        return View(model);
        //    }

        //    //update assignmentitemtask
        //    var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
        //    task.closed = true;
        //    task.closedBy = User.Identity.Name;
        //    task.closedDate = DateTime.Now;
        //    task.status = "Closed";

        //    var tres = _clinical.UpdateTask(task);

        //    return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"] });

        //}

        [HttpPost]
        public ActionResult InsertAttachment(AssignmentFullView model, Guid? pro)
        {
            var appid = Convert.ToInt32(Request.Query["appid"].ToString());
            var id = Convert.ToInt32(Request.Query["id"].ToString());
            var logid = Convert.ToInt32(Request.Query["logid"].ToString());
            model.attachment.attachmentType = Request.Query["attType"].ToString();
            var file = Request.Form.Files["file"];
            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                string filename = Path.GetFileName(Request.Form.Files["file"].FileName);
               
                var filePath = Path.Combine(path, model.attachment.dependentID + "_" + filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.attachment.Link = model.attachment.dependentID + "_" + filename;
                model.attachment.createdBy = User.Identity.Name;
                _member.InsertAttachment(model.attachment);
                var logs = _member.GetAssignmentItemLogs(appid);
                logs.attached = true;
                logs.modifiedBy = User.Identity.Name;
                logs.modifiedDate = DateTime.Now;
                var res = _member.UpdateAssignmentLog(logs);
                var item = _clinical.GetAssignmentItemById(logs.assignmentItemID);
                item.documentName = model.attachment.Link;
                item.modifiedBy = User.Identity.Name;
                item.modifiedDate = DateTime.Now;
                var ares = _clinical.UpdateAssignmentItem(item);
            }
            //update assignmentitemtask
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            var tres = _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = id });

        }


        // GET: Clinical/Edit/5
        public ActionResult EditAssignment(int id)
        {
            var model = _clinical.GetAssignmentById(id);
            return View(model);
        }

        public ActionResult AssignmentDetails(int id, Guid? pro)
        {
            var modell = new AssignmentFullView();

            modell.assignment = _clinical.GetAssignmentById(id);
            var items = _clinical.GetAssignmentItemsById(id);
            if (items.Count != 0)
            {
                foreach (var item in items)
                {
                    modell.tasks = new List<TaskViewModel>();
                    var itemCode = _member.GetAssignmentItemTypes().Where(x => x.itemDescription.Trim() == item.itemType.Trim()).Select(x => x.assignmentItemType).FirstOrDefault();
                    modell.tasks = _clinical.GetTasks(item.id, itemCode);
                    var logs = _member.GetAssignmentItemLogs(Convert.ToInt32(item.id));
                    if (logs == null)
                    {
                        var log = new AssignmentItemLogs()
                        {
                            assignmentItemID = Convert.ToInt32(item.id),
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            active = true,
                        };
                        var result = _member.InsertAssignmentLog(log);
                    }
                }
            }
            modell.attachment = new Attachments();
            modell.attachment.dependentID = modell.assignment.dependentID;
            modell.items = _clinical.GetAsItemViewById(id);
            modell.AcknowledgementQuestions = _admin.GetAcknowledgementQuestions();

            //HCare-1192
            modell.programs = new Programs();
            modell.programs.ProgramName = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();

            return View(modell);
        }

        [HttpPost]
        public ActionResult AssignmentDetails(Assignments model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.comment = model.assignmentType + " - closed";
            var result = _clinical.UpdateAssignment(model);

            return RedirectToAction("patientAssignments", "Member", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult PostPoneAssignment(int id)
        {
            var model = _clinical.GetAssignmentById(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditAssignment(Assignments model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.postponeToDate = DateTime.Now;
            model.status = "In Progress";
            var result = _clinical.UpdateAssignment(model);
            return RedirectToAction("OpenAssignments");
        }

        [HttpPost]
        public ActionResult PostPoneAssignment(Assignments model)
        {

            if (!String.IsNullOrEmpty(Request.Query["pro"]))
                model.programId = new Guid(Request.Query["pro"]);

            model.modifiedDate = DateTime.Now;
            model.Active = false;
            model.status = "PostPoned";
            model.postpone = true;
            model.modifiedBy = User.Identity.Name;
            var result = _clinical.UpdateAssignment(model);

            //create enquiry with assignment reference number
            var enq = new Queries()
            {
                query = model.postponementReason,
                createdBy = User.Identity.Name,
                effectiveDate = DateTime.Now,
                createdDate = DateTime.Now,
                Active = true,
                ReferenceNumber = model.assignmentID.ToString(),
                dependentID = model.dependentID,
                queryStatus = true,
                enquiryBy = "POR",
                queryType = "ASSI",
                querySubject = "Assignment",
                programId = model.programId
            };

            _member.InsertQuery(enq);

            return RedirectToAction("AssignmentSummary", "Member", new { DependentID = model.dependentID });
        }


        [HttpPost]
        public ActionResult CloseAssignment(AssignmentFullView model, string submitButton, Guid? pro = null)
        {
            switch (submitButton)
            {
                case "Complete":
                    model.assignment.modifiedDate = DateTime.Now;
                    model.assignment.Active = false;
                    model.assignment.status = "closed";
                    if (model.items != null)
                    {
                        model.assignment.comment = model.items[0].itemTypeName + " - closed";
                    }
                    if (String.IsNullOrEmpty(model.assignment.comment))
                    {
                        model.assignment.comment = "No Comment Added";
                    }
                    model.assignment.modifiedBy = User.Identity.Name;
                    var result = _clinical.UpdateAssignment(model.assignment);

                    //create enquiry with assignment reference number
                    var enq = new Queries()
                    {
                        query = model.assignment.comment,
                        Owner = _admin.GetUserByUsername(User.Identity.Name).userID.ToString(),
                        createdBy = User.Identity.Name,
                        effectiveDate = DateTime.Now,
                        createdDate = DateTime.Now,
                        Active = true,
                        ReferenceNumber = model.assignment.assignmentID.ToString(),
                        dependentID = model.assignment.dependentID,
                        queryStatus = true,
                        enquiryBy = "POR",
                        queryType = "ASSI",
                        querySubject = "Assignment",
                        priority = "LOW", //hcare-874
                        programId = new Guid(Request.Query["pro"]), //hcare-874
                    };

                    _member.InsertQuery(enq);

                    return RedirectToAction("EnquiryFullView", "Member", new { depId = model.assignment.dependentID, QueryID = enq.queryID, pro = pro });

                case "PostPone":
                    model.assignment.modifiedDate = DateTime.Now;
                    model.assignment.Active = false;
                    model.assignment.status = "PostPoned";
                    model.assignment.postpone = true;
                    model.assignment.modifiedBy = User.Identity.Name;
                    var results = _clinical.UpdateAssignment(model.assignment);
                    if (String.IsNullOrEmpty(model.assignment.comment))
                    {
                        model.assignment.comment = "No Comment Added";
                    }
                    //create enquiry with assignment reference number
                    var enqi = new Queries()
                    {
                        query = model.assignment.comment,
                        Owner = _admin.GetUserByUsername(User.Identity.Name).userID.ToString(),
                        createdBy = User.Identity.Name,
                        effectiveDate = DateTime.Now,
                        createdDate = DateTime.Now,
                        Active = true,
                        ReferenceNumber = model.assignment.assignmentID.ToString(),
                        dependentID = model.assignment.dependentID,
                        queryStatus = true,
                        enquiryBy = "POR",
                        queryType = "ASSI",
                        querySubject = "Assignment",
                        priority = "LOW", //hcare-874
                        programId = new Guid(Request.Query["pro"]), //hcare-874
                    };

                    _member.InsertQuery(enqi);

                    return RedirectToAction("AssignmentSummary", "Member", new { DependentID = model.assignment.dependentID, pro = pro });
            }
            return RedirectToAction("AssignmentSummary", "Member", new { DependentID = model.assignment.dependentID, pro = pro });
        }

        public ActionResult OutstandingPathologies()
        {
            var model = _admin.GetOutstandingPathologiesView();
            return View(model);
        }


        public ActionResult ExportQueriesToExcel()
        {
            var model = _admin.GetOutstandingPathologiesView();
         
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("outstandingAuthorisations");
                var currentRow = 1;
                for (int i = 0; i < model.Count; i++)
                {
                    {

                        worksheet.Cell(currentRow, 1).Value = model[i].membershipNo;
                        worksheet.Cell(currentRow, 2).Value = model[i].dependantCode;
                        worksheet.Cell(currentRow, 3).Value = model[i].schemeName;
                        worksheet.Cell(currentRow, 4).Value = model[i].effectiveDate;
                        worksheet.Cell(currentRow, 5).Value = model[i].dependantID;
                        worksheet.Cell(currentRow, 6).Value = model[i].scriptId;
                        worksheet.Cell(currentRow, 7).Value = model[i].createdDate;
                        worksheet.Cell(currentRow, 8).Value = model[i].overdue;
                        worksheet.Cell(currentRow, 9).Value = model[i].memStatus;
                        currentRow++;
                    }
                }
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment;filename=OutstandingPathologies.xls");
                Response.ContentType = "application/xls";
                Response.Body.WriteAsync(content);
                Response.Body.Flush();

                return View(model);
        }

        public ActionResult openAssignmentsExcel()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetCompactOpenAssignments();
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=newAssignments_compact.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        public ActionResult openAssignmentsExcelFullx()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetOpenAssignmentsFull(0);
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=newAssignments_full.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        //HCare-925
        public ActionResult openAssignmentsExcelFull()
        {
            var model = _clinical.GetOpenAssignmentsFull(0); //HCare-941
            var modelRows = _clinical.GetOpenAssignmentsFull(0).Count() + 1;

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("FullView");
            workSheet.Cells[1, 1].LoadFromCollection(model, true);

            #region formatting(background-color)
            using (ExcelRange rng = workSheet.Cells[1, 1, 100, 50])
            {
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
            }
            #endregion

            #region formatting(table-border)
            using (ExcelRange rng = workSheet.Cells[1, 1, modelRows, 21])
            {
                rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                rng.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
                rng.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
                rng.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
                rng.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);

                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            }
            #endregion

            #region formatting(column-format)
            workSheet.Row(1).Style.Font.Bold = true;
            //-->>
            workSheet.DeleteColumn(3); //DependantID
            workSheet.DeleteColumn(7); //Patient name UPPERCASE
                                       //-->>
            workSheet.Column(1).Width = 15;
            workSheet.Column(2).Width = 15;
            workSheet.Column(3).Width = 40;
            workSheet.Column(4).Width = 40;
            workSheet.Column(5).Width = 40;
            workSheet.Column(6).AutoFit();//HCare-941
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).Width = 40;
            workSheet.Column(9).Width = 20;
            workSheet.Column(10).AutoFit();
            workSheet.Column(11).Width = 20;
            workSheet.Column(12).Width = 20;
            workSheet.Column(12).Style.Numberformat.Format = "dd-mm-yyyy";
            workSheet.Column(13).AutoFit();
            workSheet.Column(14).Width = 20;
            workSheet.Column(15).Width = 20;
            workSheet.Column(16).Width = 20;
            workSheet.Column(17).AutoFit();
            workSheet.Column(18).Width = 20;
            //-->>
            #endregion

            using (var memoryStream = new MemoryStream())
            {

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=assignments-full-view.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);
            }

        }

        public ActionResult ipAssignmentsExcel()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetCompactInProgressAssignments();
            var grid = new GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=activeAssignments_compact.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        public ActionResult ipAssignmentsExcelFull()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetInProgressAssignments();
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=activeAssignments_full.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        public ActionResult closedAssignmentsExcel()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetCompactClosedAssignments();
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=closedAssignments_compact.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        public ActionResult closedAssignmentsExcelFull()
        {
            var sb = new StringBuilder();
            var model = _clinical.GetClosedAssignments();
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=closedAssignments_full.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        [HttpGet]
        public ActionResult _PatientDisclaimer(Guid DependentID, int id, int taskId, string templateID, Guid? pro)
        {
            var model = new DisclaimerVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            model.DisclaimerQuestions = _admin.GetDisclaimerQuestions();
            model.AcknowledgementQuestions = _admin.GetAcknowledgementQuestions();
            model.Dependant = _member.GetDependantByDependantID(DependentID);
            model.ContactVM = _member.GetContactByDependentID(DependentID);
            model.NextOFKin = _member.GetNextOfKinByDependantID(DependentID);
            model.PMOC = _admin.GetListofPMOC().Where(x => x.active == true).ToList();

            if (model.NextOFKin != null)
            {
                if (!String.IsNullOrEmpty(model.NextOFKin.FirstName)) { ViewBag.nokFirstName = model.NextOFKin.FirstName; } else { ViewBag.nokFirstName = null; }
                if (!String.IsNullOrEmpty(model.NextOFKin.LastName)) { ViewBag.nokLastName = model.NextOFKin.LastName; } else { ViewBag.nokLastName = null; }
                if (!String.IsNullOrEmpty(model.NextOFKin.Telephone)) { ViewBag.nokTelephone = model.NextOFKin.Telephone; } else { ViewBag.nokTelephone = null; }
                if (!String.IsNullOrEmpty(model.NextOFKin.Email)) { ViewBag.nokEmail = model.NextOFKin.Email; } else { ViewBag.nokEmail = null; }
                if (!String.IsNullOrEmpty(model.NextOFKin.Relation)) { ViewBag.nokRelation = model.NextOFKin.Relation; } else { ViewBag.nokRelation = null; }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult _PatientDisclaimer(DisclaimerVM model)
        {
            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var questions = _admin.GetDisclaimerQuestions();
            var acknowledgement = _admin.GetAcknowledgementQuestions();
            var msupdate = false;
            var dependantid = new Guid(Request.Query["DependantID"]);

            for (int i = 1; i < questions.Count + 1; i++)
            {
                var disclaimer = new DisclaimerResponse();
                disclaimer.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                disclaimer.DependantID = dependantid;
                disclaimer.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                disclaimer.CreatedDate = DateTime.Now;
                disclaimer.CreatedBy = User.Identity.Name;
                disclaimer.Question = "Q" + i;
                if (Request.Query["disclaimer-yes-Q" + i].ToString().ToLower().Contains("true")) { disclaimer.Yes = true; disclaimer.No = false; } else { disclaimer.Yes = false; disclaimer.No = true; }
                if (Request.Query["disclaimer-no-Q" + i].ToString().ToLower().Contains("true")) { disclaimer.Yes = false; disclaimer.No = true; if (i != 6) { msupdate = true; } } else { disclaimer.Yes = true; disclaimer.No = false; }
                if (Request.Query["disclaimer-no-Q" + i].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["DisclaimerResponse.Comment"]))) { disclaimer.Comment = Request.Query["DisclaimerResponse.Comment"]; }
                disclaimer.Program = programcode.code;
                disclaimer.Active = true;

                var insert = _admin.InsertDisclaimerResponse(disclaimer);
            }
            for (int i = 1; i < acknowledgement.Count + 1; i++)
            {
                if (Request.Query["acknowledgement-yes-A" + i].ToString().ToLower().Contains("true"))
                {
                    var disclaimer = new DisclaimerResponse();
                    disclaimer.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                    disclaimer.DependantID = dependantid;
                    disclaimer.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                    disclaimer.CreatedDate = DateTime.Now;
                    disclaimer.CreatedBy = User.Identity.Name;
                    disclaimer.Question = "A" + i;
                    if (Request.Query["acknowledgement-yes-A" + i].ToString().ToLower().Contains("true")) { disclaimer.Yes = true; disclaimer.No = false; } else { disclaimer.Yes = false; disclaimer.No = true; }
                    disclaimer.Comment = null;
                    disclaimer.Program = programcode.code;
                    disclaimer.Active = true;

                    var insert = _admin.InsertDisclaimerResponse(disclaimer);
                }
            }

            #region hcare-1363: popia-update
            model.ContactVM = _member.GetContactByDependentID(dependantid);
            if (model.ContactVM.contacts != null)
            {
                model.ContactVM.contacts.modifiedBy = User.Identity.Name;
                model.ContactVM.contacts.modifiedDate = DateTime.Now;
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.contactName"])) { model.ContactVM.contacts.contactName = Request.Query["ContactVM.contacts.contactName"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.landLine"])) { model.ContactVM.contacts.landLine = Request.Query["ContactVM.contacts.landLine"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.cell"])) { model.ContactVM.contacts.cell = Request.Query["ContactVM.contacts.cell"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.workNo"])) { model.ContactVM.contacts.workNo = Request.Query["ContactVM.contacts.workNo"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.email"])) { model.ContactVM.contacts.email = Request.Query["ContactVM.contacts.email"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.fax"])) { model.ContactVM.contacts.fax = Request.Query["ContactVM.contacts.fax"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.contacts.preferredMethodOfContact"])) { model.ContactVM.contacts.preferredMethodOfContact = Request.Query["ContactVM.contacts.preferredMethodOfContact"]; }
                _member.UpdateContact(model.ContactVM.contacts);

                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.line1"])) { model.ContactVM.address.line1 = Request.Query["ContactVM.address.line1"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.line2"])) { model.ContactVM.address.line2 = Request.Query["ContactVM.address.line2"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.line3"])) { model.ContactVM.address.line3 = Request.Query["ContactVM.address.line3"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.province"])) { model.ContactVM.address.province = Request.Query["ContactVM.address.province"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.city"])) { model.ContactVM.address.city = Request.Query["ContactVM.address.city"]; }
                if (!String.IsNullOrEmpty(Request.Query["ContactVM.address.zip"])) { model.ContactVM.address.zip = Request.Query["ContactVM.address.zip"]; }
                _member.UpdateAddress(model.ContactVM.address);

            }
            else
            {
                model.ContactVM.contacts.createdBy = User.Identity.Name;
                model.ContactVM.contacts.createdDate = DateTime.Now;
                model.ContactVM.contacts.contactName = Request.Query["ContactVM.contacts.contactName"];
                model.ContactVM.contacts.landLine = Request.Query["ContactVM.contacts.landLine"];
                model.ContactVM.contacts.cell = Request.Query["ContactVM.contacts.cell"];
                model.ContactVM.contacts.workNo = Request.Query["ContactVM.contacts.workNo"];
                model.ContactVM.contacts.email = Request.Query["ContactVM.contacts.email"];
                model.ContactVM.contacts.fax = Request.Query["ContactVM.contacts.fax"];
                model.ContactVM.contacts.preferredMethodOfContact = Request.Query["ContactVM.contacts.preferredMethodOfContact"];
                model.ContactVM.contacts.Active = true;
                _member.InsertContact(model.ContactVM.contacts);

                model.ContactVM.address.createdBy = User.Identity.Name;
                model.ContactVM.address.createdDate = DateTime.Now;
                model.ContactVM.address.line1 = Request.Query["ContactVM.address.line1"];
                model.ContactVM.address.line2 = Request.Query["ContactVM.address.line2"];
                model.ContactVM.address.line3 = Request.Query["ContactVM.address.line3"];
                model.ContactVM.address.province = Request.Query["ContactVM.address.province"];
                model.ContactVM.address.city = Request.Query["ContactVM.address.city"];
                model.ContactVM.address.zip = Request.Query["ContactVM.address.zip"];
                model.ContactVM.address.Active = true;
                _member.InsertAddress(model.ContactVM.address);
            }

            model.NextOFKin = _member.GetNextOfKinByDependantID(dependantid);
            if (model.NextOFKin != null)
            {
                model.NextOFKin.ModifiedBy = User.Identity.Name;
                model.NextOFKin.ModifiedDate = DateTime.Now;
                model.NextOFKin.EffectiveDate = DateTime.Now;
                model.NextOFKin.FirstName = Request.Query["FirstName"];
                model.NextOFKin.LastName = Request.Query["LastName"];
                model.NextOFKin.Telephone = Request.Query["Telephone"];
                model.NextOFKin.Email = Request.Query["Email"];
                model.NextOFKin.Relation = Request.Query["Relation"];

                _member.UpdateNextOfKin(model.NextOFKin);
            }
            else
            {
                var nok = new NextOFKin();
                nok.Id = Guid.NewGuid();
                nok.DependantID = dependantid;
                nok.CreatedBy = User.Identity.Name;
                nok.CreatedDate = DateTime.Now;
                nok.EffectiveDate = DateTime.Now;
                nok.FirstName = Request.Query["FirstName"];
                nok.LastName = Request.Query["LastName"];
                nok.Telephone = Request.Query["Telephone"];
                nok.Email = Request.Query["Email"];
                nok.Relation = Request.Query["Relation"];
                nok.ProgramID = programcode.programID;
                nok.Active = true;

                _member.InsertNextOfKin(nok);
            }
            #endregion

            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";
            _clinical.UpdateTask(task);

            #region ms-update + assignment close 
            var memberdetail = _member.GetMemberByDependantID(dependantid);
            var medicalaiddetail = _member.GetMedicalAidById(memberdetail.medicalAidID);
            var disclaimerrequired = medicalaiddetail.disclaimer;
            if (msupdate == true && disclaimerrequired == true)
            {
                #region last-status update
                var laststatus = _member.GetManagmentStatusInformation(dependantid).Where(x => x.active == true).Where(x => x.programCode == programcode.code).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                var laststatus_closed = _member.GetManagmentStatusInformation(dependantid).Where(x => x.active == true).FirstOrDefault();
                var statushistory = _member.GetManagmentStatusInformation(dependantid).Where(x => x.active == true).ToList();

                var statusupdate = new PatientManagementStatusHistory()
                {
                    id = laststatus.id,
                    managementStatusCode = laststatus.managementStatusCode,
                    createdBy = laststatus.createdBy,
                    createdDate = laststatus.createdDate,
                    active = laststatus.active,
                    modifiedBy = User.Identity.Name,
                    modifiedDate = DateTime.Now,
                    effectiveDate = laststatus.effectiveDate,
                };

                if (laststatus_closed.codeStatus.ToLower() != "c")
                {
                    statusupdate.endDate = DateTime.Now.AddDays(-1);
                }
                foreach (var status in statushistory)
                {
                    if (status.programCode == programcode.code && status.endDate == null & status.codeStatus.ToLower() != "c")
                    {
                        statusupdate.endDate = DateTime.Now.AddDays(-1);
                    }
                }
                var results = _member.UpdateManagementStatus(statusupdate);
                #endregion
                #region new-ms-insert
                PatientManagementStatusHistory patientStatus = new PatientManagementStatusHistory();
                patientStatus.createdDate = DateTime.Now;
                patientStatus.dependantId = new Guid(Request.Query["DependantID"]);
                patientStatus.active = true;
                patientStatus.createdBy = User.Identity.Name;
                patientStatus.createdDate = DateTime.Now;
                patientStatus.effectiveDate = DateTime.Now;

                var scheme = _member.GetMedicalAidByDependantID(dependantid); //hcare-1363
                var optout = _admin.GetManagementStatusList().Where(x => x.statusName.ToLower().Replace(" ", "").Contains("optout")).Where(x => x.programCode == programcode.code).Select(x => x.statusCode).FirstOrDefault(); //hcare-1363
                var programrefusal = _admin.GetManagementStatusList().Where(x => x.statusName.ToLower().Replace(" ", "").Contains("programrefusal")).Where(x => x.programCode == programcode.code).Select(x => x.statusCode).FirstOrDefault(); //hcare-1363
                if (scheme.MedicalAidCode.ToLower().Contains("bestmed")) { patientStatus.managementStatusCode = optout; } else { patientStatus.managementStatusCode = programrefusal; } // hcare-1363

                _member.InsertManagementStatus(patientStatus);
                #endregion
                #region close-assignment
                var assignmentID = Convert.ToInt32(Request.Query["id"]);
                var assignmentdetail = _member.GetAssignment(assignmentID);

                assignmentdetail.Active = false;
                assignmentdetail.comment = "Disclaimer not signed";
                assignmentdetail.status = "Closed";

                var assignmentupdate = _member.UpdateAssignment(assignmentdetail);

                #endregion

                return RedirectToAction("patientAssignments", "Member", new { DependentID = new Guid(Request.Query["DependantID"]), pro = Request.Query["pro"] });
            }
            #endregion


            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }



        public ActionResult _PatientQuestionnaire(Guid DependentID, int id, int taskId, string templateID)
        {
            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            var model = _clinical.GetQuestionnaireList(templateID);
            return View(model);
        }

        [HttpPost]
        public ActionResult _PatientQuestionnaire()
        {
            var result = Request.Query;
            var model = _clinical.GetQuestionnaireList(result["template"]);
            var qResponse = new PatientQuestionnaireResponse();

            foreach (var task in model)
            {
                qResponse.TaskId = Convert.ToInt32(Request.Query["taskId"]);
                qResponse.DependantId = new Guid(result["DependantID"]);
                qResponse.TemplateId = task.QuestionnaireTemplateID;

                //if (result[task.QuestionnaireTemplateID.ToString()].ToUpper().Contains("ON"))
                //{
                //    qResponse.Confirmed = true;
                //}
                //else
                //{
                //    qResponse.Confirmed = false;
                //}


                //if (task.Answer == true)
                //{
                //    if (result[task.QuestionnaireTemplateID.ToString()].ToUpper().Contains("TRUE"))
                //    {
                //        qResponse.Answer = true;
                //    }
                //    else
                //    {
                //        qResponse.Answer = false;
                //    }
                //}

                if (task.Answer == true)
                {
                    if (result[task.QuestionnaireTemplateID.ToString()].ToString().ToUpper().Contains("TRUE"))
                    {
                        qResponse.Answer = true;
                    }
                    else if (result[task.QuestionnaireTemplateID.ToString()].ToString().ToUpper().Contains("FALSE"))
                    {
                        qResponse.Answer = false;
                    }
                }
                else
                {
                    qResponse.Answer = false;
                }


                if (Request.Query["disclaimerCheckbox"].ToString().ToUpper().Contains("TRUE"))
                {
                    qResponse.Confirmed = true;
                }
                else
                {
                    qResponse.Confirmed = false;
                }


                qResponse.CreatedBy = User.Identity.Name;
                qResponse.CreatedDate = DateTime.Now;
                qResponse.Active = true;



                var insertResult = _clinical.InsertPatientQuestionnaireResponse(qResponse);

            }


            //update task
            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);


            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"] });
        }

        //HIV Questionnaire GET
        [HttpGet]
        public ActionResult _DoctorQuestionnaire(Guid DependentID, int id, int taskId, string templateID)
        {
            var model = new DoctorQuestionnaireViewModel();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;

            model.coMorbidTypes = _member.GetCoMorbids().ToList();

            return View(model);
        }

        //HIV Questionnaire POST
        [HttpPost]
        public ActionResult _DoctorQuestionnaire(DoctorQuestionnaireViewModel model)
        {

            //ARV History
            model.MedicationHistories = new List<MedicationHistory>();

            for (int i = 0; i < 10; i++)
            {
                if (!String.IsNullOrEmpty(Request.Query["Treatment" + i]))
                {
                    model.MedicationHistories.Add(new MedicationHistory()
                    {
                        productName = Request.Query["Treatment" + i],
                        comment = Request.Query["Comment" + i],
                        dependantId = new Guid(Request.Query["DependantID"]),
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        active = true,

                    });
                }
                else
                    continue;
            }

            foreach (var history in model.MedicationHistories)
            {
                var medHistory = _member.InsertMedicationHistory(history);
            }

            //CoMorbid Conditions
            model.comormidConditions = new List<CoMormidConditions>();
            if (!Request.Query["comorbids_NA"].ToString().Contains("true"))
            {

                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["cmCondition" + i]))
                    {
                        model.comormidConditions.Add(new CoMormidConditions()
                        {
                            coMorbidId = Convert.ToInt32(Request.Query["cmCondition" + i]),
                            effectiveDate = Convert.ToDateTime(Request.Query["cmDateDiagnosed" + i]),
                            CoMorbidTreatement = Convert.ToString(Request.Query["cmTreatement" + i]),
                            dependantID = new Guid(Request.Query["DependantID"]),
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            active = true,
                        });
                    }
                    else
                        continue;
                }

                foreach (var history in model.comormidConditions)
                {
                    var coMorbHistory = _member.InsertComorbidCondition(history);
                }
            }
            //Hospitalisations
            model.HospitalAuths = new List<HospitalizationAuths>();
            if (!Request.Query["hospitalisationDDate_NA"].ToString().Contains("true"))
            {
                var memberinfo = _member.GetDependentDetails(new Guid(Request.Query["DependantID"]), null);

                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["hzDiagDate" + i]))
                    {
                        model.HospitalAuths.Add(new HospitalizationAuths()
                        {
                            createdDate = Convert.ToDateTime(Request.Query["hzDiagDate" + i]),
                            authType = Convert.ToString(Request.Query["hzDiagnosis" + i]),
                            membershipNo = memberinfo.member.membershipNo,
                            dependantCode = memberinfo.dependent.dependentCode,
                            medicalAid = memberinfo.MedicalAids[0].medicalAidCode,

                        });
                    }
                    else
                        continue;
                }

                foreach (var history in model.HospitalAuths)
                {
                    var hZHistory = _member.InsertHospitalizationAuths(history);
                }
            }
            //tBSocialHistory
            if (!Request.Query["hospitalisations_NA"].ToString().Contains("true"))
            {
                model.drquestionnaireResultList = new List<DoctorQuestionnaireResults>();

                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["tbDateDiagnosedFrom" + i]))
                    {
                        model.drquestionnaireResultList.Add(new DoctorQuestionnaireResults()
                        {
                            tbTreatmentStartDate = Convert.ToDateTime(Request.Query["tbDateDiagnosedFrom" + i]),
                            tbTreatmentEndDate = Convert.ToDateTime(Request.Query["tbDateDiagnosedTo" + i]),
                            dependentID = new Guid(Request.Query["DependantID"]),
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            Active = true,
                        });
                    }
                    else
                        continue;
                }

                foreach (var history in model.drquestionnaireResultList)
                {
                    var tBHistory = _member.InsertDrQuestionnaire(history);
                }

                //Insert clinical questionnaire results
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;

                var clinQuestionnaire = _member.InsertClinicalHistoryQuestionnaire(model.questionnaire);

                //insert dr question results
                model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
                model.drquestionnaireResults.createdDate = DateTime.Now;
                model.drquestionnaireResults.createdBy = User.Identity.Name;
                model.drquestionnaireResults.Active = true;

                var drQuestionnaireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);

                //insert allergies results
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.active = true;

                var allergies = _member.InsertAllergy(model.allergy);

            }
            //insert clinical exam results
            model.clinicalexam.effectiveDate = DateTime.Now;
            model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
            model.clinicalexam.createdBy = User.Identity.Name;
            model.clinicalexam.createdDate = DateTime.Now;

            if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))

            {
                model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
            }

            var clinicalExam = _member.InsertClinicalExam(model.clinicalexam);

            //insert path results
            model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
            model.pathology.effectiveDate = DateTime.Now;
            model.pathology.createdDate = DateTime.Now;
            model.pathology.createdBy = User.Identity.Name;
            model.pathology.active = true;
            model.pathology.pathologyType = "GEN";
            model.pathology.labName = "Questionnaire";
            model.pathology.labReferenceNo = "Questionnaire";

            var pathology = _member.InsertPathology(model.pathology);

            //insert program results
            //model.programHistory.dependantId = new Guid(Request.Query["DependantID"]);
            //model.programHistory.createdDate = DateTime.Now;
            //model.programHistory.createdBy = User.Identity.Name;
            //model.programHistory.active = true;
            //model.programHistory.programCode = "HIV";

            //var program = _member.InsertProgramHistory(model.programHistory);

            //update task
            var taskss = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            taskss.closed = true;
            taskss.closedBy = User.Identity.Name;
            taskss.closedDate = DateTime.Now;
            taskss.status = "Closed";

            var taskResult = _clinical.UpdateTask(taskss);

            //apply checks to see whats missing
            if (model.drquestionnaireResults.diagnosisDate == null || model.programHistory.effectiveDate == null || model.clinicalexam.weight == 0)
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "ICQA",

                };
                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var tasks = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in tasks)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            //check for folow ups
            if (Request.Query["comorbids_NA"].ToString().Contains("true") || Request.Query["hospitalisations_NA"].ToString().Contains("true")
                || Request.Query["comorbids_NA"].ToString().Contains("true"))
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var tasks = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in tasks)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"] });
        }

        //=============================================================================== FULL Diabetes Questionnaire ===============================================================================//

        [HttpGet]
        public ActionResult _DiabetesQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode, Guid? pro)
        {
            var model = new DoctorQuestionnaireViewModel();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.dependantCode = dependantCode;
            ViewBag.membershipNo = membershipNo;

            Guid? programCode;
            if (Request.Query["pro"].ToString() == null)
            {
                programCode = _member.GetPrograms().Select(x => x.programID).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            }

            ViewBag.pro = programCode;

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;

            model.ScriptItems = _member.GetScriptItems(DependentID);
            model.dependent = _member.GetDependantByDependantID(DependentID);
            model.member = _member.GetMemberByDependantID(DependentID);
            model.contact = _member.GetContact(DependentID);

            model.clinicalexams = _member.GetClinicalExam(DependentID);
            model.podiatries = _member.GetPodiatryResults(DependentID);
            model.programDiagnoses = _member.GetDiagnosisResults(DependentID);

            model.questionnaires = new List<ClinicalHistoryQuestionaire>();
            model.questionnaires = _member.GetClinicalHistory(DependentID);

            model.patientDisclaimers = new List<PatientDisclaimer>();
            model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(DependentID);

            model.coMorbidTypes = _member.GetCoMorbids().ToList();
            model.ComorbidViews = _member.GetComorbidItems(DependentID, pro);

            model.allergies = new List<Allergies>();
            model.allergies = _member.GetAllergies(DependentID);

            model.autoNeropathies = new List<AutoNeropathy>();
            model.autoNeropathies = _member.GetAutoNeroResults(DependentID);

            model.visions = new List<Vision>();
            model.visions = _member.GetVisionResults(DependentID);

            model.hypoglycaemias = new List<Hypoglycaemia>();
            model.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);

            model.questionnaireOthers = new List<QuestionnaireOther>();
            model.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);

            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == pro).ToList();
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                    model.doctorContact = _member.GetDrInformation(doctorh[0].doctorId);
                }
            }

            var memberinfo = _member.GetDependentDetails(DependentID, null);

            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);

            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");
            #region comorbid-loop
            ViewBag.cmlist1 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist2 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist3 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist4 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist5 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist6 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist7 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist8 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist9 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.cmlist10 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            #endregion

            return View(model);
        }

        [HttpPost]
        public ActionResult _DiabetesQuestionnaire(DoctorQuestionnaireViewModel model)
        {

            //=========================================================================================================================================================================//
            //                                                                        DoctorQuestionnaireResults                                                                       // 
            //=========================================================================================================================================================================//

            model.drquestionnaireResults = new DoctorQuestionnaireResults();

            model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
            model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.Active = true;

            //=========================================================================================================================================================================//
            //                                                                           Clincal History Information                                                                   // 
            //=========================================================================================================================================================================//

            if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.clinicalexam = new Clinical();

                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))

                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }
            else if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.clinicalexam = new Clinical();

                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);

                if (model.clinicalexam.height == 0 && model.clinicalexam.weight == 0)
                    model.clinicalexam.followUp = true;
                else
                    model.clinicalexam.followUp = false;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                model.clinicalexam.active = true;

            }
            else if (Request.Query["Clinical_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.height = 0;
                model.clinicalexam.weight = 0;
                model.clinicalexam.clinicalComment = "Entry ignored";
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = false;

            }
            else
            {
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }
                else if (model.clinicalexam.height == 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight == 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = 0;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }

            var clinicalExam = _member.InsertClinicalExam(model.clinicalexam);
            model.drquestionnaireResults.ClinicalHistoryID = model.clinicalexam.id;

            //=========================================================================================================================================================================//
            //                                                                                 Pathology Insert                                                                        // 
            //=========================================================================================================================================================================//

            if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = "N/A";
                model.pathology.comment = "Diabetes questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = "N/A";
                model.pathology.comment = "Diabetes questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["Clinical_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "-";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = "N/A";
                model.pathology.comment = "Entry ignored - Diabetes questionnaire";
                model.pathology.active = false;
            }
            else if ((model.pathology.diastolicBP != null && model.pathology.systolicBP != null) || (model.pathology.diastolicBP != 0 && model.pathology.systolicBP != 0))
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = "N/A";
                if (model.pathology.systolicBP == 0 && model.pathology.diastolicBP == 0) //HCare-1063
                {
                    model.pathology.comment = "Entry ignored - Diabetes questionnaire";
                    model.pathology.active = false;
                }
                else
                {
                    model.pathology.comment = "Diabetes questionnaire";
                    model.pathology.active = true;
                }

            }
            else if (model.pathology.diastolicBP == 0 && model.pathology.systolicBP == 0)
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = "N/A";
                model.pathology.comment = "Diabetes questionnaire";
                model.pathology.diastolicBP = 0;
                model.pathology.systolicBP = 0;
                model.pathology.active = false;

            }

            var pathology = _member.InsertPathology(model.pathology);

            //=========================================================================================================================================================================//
            //                                                                                 Diagnosis Insert                                                                        // 
            //=========================================================================================================================================================================//


            if (Request.Query["diagnosis_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.PatientDiagnosis = new PatientDiagnosis();

                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.ProgramCode = "DIABD";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;

                if (!String.IsNullOrEmpty(Request.Query["PatientDiagnosis.EffectiveDate"])) { model.PatientDiagnosis.EffectiveDate = Convert.ToDateTime(Request.Query["PatientDiagnosis.EffectiveDate"]); }
                else { model.PatientDiagnosis.EffectiveDate = null; }

                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment = (Request.Query["PatientDiagnosis.generalComments"]);

                if (String.IsNullOrEmpty(Request.Query["PatientDiagnosis.EffectiveDate"])) { model.PatientDiagnosis.FollowUp = true; }
                else { model.PatientDiagnosis.FollowUp = false; }

                model.PatientDiagnosis.Active = true;
                model.PatientDiagnosis.Medication = Request.Query["medication-registration"]; //hcare-863
                model.PatientDiagnosis.EffectiveDate = new DateTime(model.PatientDiagnosis.EffectiveDate.Value.Year, model.PatientDiagnosis.EffectiveDate.Value.Month, model.PatientDiagnosis.EffectiveDate.Value.Day, model.PatientDiagnosis.CreatedDate.Hour, model.PatientDiagnosis.CreatedDate.Minute, model.PatientDiagnosis.CreatedDate.Second); //hcare-863

                var program = _member.GetPrograms().Where(x => x.code == model.PatientDiagnosis.ProgramCode).FirstOrDefault(); //hcare-863
                model.PatientDiagnosis.ICD10Code = program.icd10code; //hcare-863

            }
            else if (Request.Query["diagnosis_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.PatientDiagnosis = new PatientDiagnosis();

                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.ProgramCode = "-";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment= "Entry ignored";
                model.PatientDiagnosis.FollowUp = false;
                model.PatientDiagnosis.Active = false;
                model.PatientDiagnosis.Medication = Request.Query["medication-registration"]; //hcare-863
                model.PatientDiagnosis.EffectiveDate = new DateTime(model.PatientDiagnosis.EffectiveDate.Value.Year, model.PatientDiagnosis.EffectiveDate.Value.Month, model.PatientDiagnosis.EffectiveDate.Value.Day, model.PatientDiagnosis.CreatedDate.Hour, model.PatientDiagnosis.CreatedDate.Minute, model.PatientDiagnosis.CreatedDate.Second);

                var program = _member.GetPrograms().Where(x => x.code == model.PatientDiagnosis.ProgramCode).FirstOrDefault(); //hcare-863
                model.PatientDiagnosis.ICD10Code = program.icd10code; //hcare-863
            }
            else if (model.PatientDiagnosis.EffectiveDate != null || model.PatientDiagnosis.Comment!= null)
            {
                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.ProgramCode = "DIABD";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment= (Request.Query["PatientDiagnosis.generalComments"]);
                model.PatientDiagnosis.FollowUp = false;
                model.PatientDiagnosis.Active = true;
                model.PatientDiagnosis.Medication = Request.Query["medication-registration"]; //hcare-863
                model.PatientDiagnosis.EffectiveDate = new DateTime(model.PatientDiagnosis.EffectiveDate.Value.Year, model.PatientDiagnosis.EffectiveDate.Value.Month, model.PatientDiagnosis.EffectiveDate.Value.Day, model.PatientDiagnosis.CreatedDate.Hour, model.PatientDiagnosis.CreatedDate.Minute, model.PatientDiagnosis.CreatedDate.Second);

                var program = _member.GetPrograms().Where(x => x.code == model.PatientDiagnosis.ProgramCode).FirstOrDefault(); //hcare-863
                model.PatientDiagnosis.ICD10Code = program.icd10code; //hcare-863
            }

            var diagnosisDate = _member.InsertDiagnosisResults(model.PatientDiagnosis);

            //=========================================================================================================================================================================//
            //                                                                            CoMorbid Conditions                                                                          // 
            //=========================================================================================================================================================================//

            model.comormidConditions = new List<CoMormidConditions>(); //HCare-607

            if ((Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true")) && (!String.IsNullOrEmpty(Request.Query["cmlist1"]))) //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["cmlist" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["cmlist" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "DIABD";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);

                        if (!String.IsNullOrEmpty(Request.Query["cmlist" + i]))
                            comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        else
                            comorb.coMorbidId = 99998;

                        if (!String.IsNullOrEmpty(Request.Query["ci-comorbidEffectiveDate" + i]))
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["ci-comorbidEffectiveDate" + i]);
                        else
                            comorb.effectiveDate = null;

                        if (!String.IsNullOrEmpty(Request.Query["ci-comorbidEndDate" + i]))
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["ci-comorbidEndDate" + i]);
                        else
                            comorb.treatementEndDate = null;

                        if (String.IsNullOrEmpty(Request.Query["comorbids" + i]))
                            comorb.followUp = true;
                        else
                            comorb.followUp = false;

                        comorb.active = true;

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 99998,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    effectiveDate = null,
                    treatementEndDate = null,
                    dependantID = new Guid(Request.Query["DependantID"]),
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    generalComments = (Request.Query["comormidCondition.generalComments"]),
                    followUp = true,
                    active = true,

                });
            }
            else if (Request.Query["CoMorb_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 0,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    effectiveDate = null,
                    treatementEndDate = null,
                    dependantID = new Guid(Request.Query["DependantID"]),
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    generalComments = "Entry ignored",
                    followUp = false,
                    active = false,

                });
            }
            else if (Request.Query["comorbid_CI-false"].ToString().ToLower().Contains("true"))
            {
                //model.comormidConditions.Add(new CoMormidConditions()
                //{
                //    coMorbidId = 99999,
                //    effectiveDate = DateTime.Now,
                //    treatementEndDate = null,
                //    dependantID = new Guid(Request.Query["DependantID"]),
                //    createdDate = DateTime.Now,
                //    createdBy = User.Identity.Name,
                //    programType = "Diabetes",
                //    generalComments = (Request.Query["comormidCondition.generalComments"]),
                //    followUp = false,
                //    active = true,

                //});
            }
            else //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["cmlist" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["cmlist" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "DIABD";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);
                        comorb.active = true;

                        if (!String.IsNullOrEmpty(Request.Query["ci-comorbidEffectiveDate" + i]))
                        {
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["ci-comorbidEffectiveDate" + i]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["ci-comorbidEndDate" + i]))
                        {
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["ci-comorbidEndDate" + i]);
                        }

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }

            }


            foreach (var history in model.comormidConditions)
            {
                var coMorbHistory = _member.InsertComorbidCondition(history);
            }

            //=========================================================================================================================================================================//
            //                                                                             Medication History                                                                          // 
            //=========================================================================================================================================================================//

            model.MedicationHistories = new List<MedicationHistory>();

            if (Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["diabetesMedProduct1"])))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]))
                    {
                        var medication = new MedicationHistory();

                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]))
                            medication.productName = Request.Query["diabetesMedProduct" + i];
                        else
                            medication.productName = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedComment" + i]))
                            medication.comment = Request.Query["diabetesMedComment" + i];
                        else
                            medication.comment = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";

                        if (String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]) && String.IsNullOrEmpty(Request.Query["diabetesMedComment" + i]))
                            medication.followUp = true;
                        else
                            medication.followUp = false;

                        medication.active = true;

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true") && Request.Query["ci_MedicationNaive-true"].ToString().ToLower().Contains("true"))
            {
                for (int i = 0; i < 1; i++)
                {
                    if (String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate" + i]))
                    {
                        var medication = new MedicationHistory();

                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]))
                            medication.productName = Request.Query["diabetesMedProduct" + i];
                        else
                            medication.productName = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedComment" + i]))
                            medication.comment = Request.Query["diabetesMedComment" + i];
                        else
                            medication.comment = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";

                        if (String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]) && String.IsNullOrEmpty(Request.Query["diabetesMedComment" + i]))
                            medication.followUp = true;
                        else
                            medication.followUp = false;

                        medication.active = true;

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.MedicationHistories.Add(new MedicationHistory()
                {
                    dependantId = new Guid(Request.Query["DependantID"]),
                    Id = Convert.ToInt32(Request.Query["id"]),
                    createdDate = DateTime.Now,
                    nappiCode = "-",
                    productName = "Follow up",
                    directions = "Follow up",
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    comment = "Follow up",
                    followUp = true,
                    active = true,

                    QuestionnaireID = model.drquestionnaireResults.Id,

                });
            }
            else if (Request.Query["ci_MedicationNaive-false"].ToString().ToLower().Contains("true"))
            {
                model.MedicationHistories.Add(new MedicationHistory()
                {
                    dependantId = new Guid(Request.Query["DependantID"]),
                    Id = Convert.ToInt32(Request.Query["id"]),
                    createdDate = DateTime.Now,
                    nappiCode = "-",
                    productName = "None",
                    directions = "None",
                    startDate = null,
                    endDate = null,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    comment = "None",
                    followUp = false,
                    active = true,

                    QuestionnaireID = model.drquestionnaireResults.Id,

                });
            }
            else if (Request.Query["Medication_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.MedicationHistories.Add(new MedicationHistory()
                {
                    dependantId = new Guid(Request.Query["DependantID"]),
                    Id = Convert.ToInt32(Request.Query["id"]),
                    createdDate = DateTime.Now,
                    nappiCode = "-",
                    productName = null,
                    directions = null,
                    startDate = null,
                    endDate = null,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    comment = "Entry ignored",
                    followUp = false,
                    active = false,

                    QuestionnaireID = model.drquestionnaireResults.Id,

                });
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesMedProduct" + i]))
                    {
                        var medication = new MedicationHistory();
                        medication.productName = Request.Query["diabetesMedProduct" + i];
                        medication.comment = Request.Query["diabetesMedComment" + i];
                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;
                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";
                        medication.active = true;

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }

            foreach (var history in model.MedicationHistories)
            {
                var medHistory = _member.InsertMedicationHistory(history);
            }

            //=========================================================================================================================================================================//
            //                                                                                  Allergy Insert                                                                         // 
            //=========================================================================================================================================================================//

            if (Request.Query["Allergy_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.allergy = new Allergies();

                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.Allergy = "Follow up";
                else
                    model.allergy.Allergy = Request.Query["allergy.Allergy"];
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);

                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.followUp = true;
                else
                    model.allergy.followUp = false;

                model.allergy.active = true;
            }
            else if (Request.Query["Allergy_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.Allergy = null;
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";
                model.allergy.generalComments = "Entry ignored";
                model.allergy.followUp = false;
                model.allergy.active = false;

            }
            else if (Request.Query["allergies_ci-true"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.Allergy = Request.Query["allergy.Allergy"];
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                model.allergy.followUp = false;
                model.allergy.active = true;

            }
            else if (Request.Query["allergies_ci-false"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.Allergy = "None";
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                model.allergy.followUp = false;
                model.allergy.active = true;

            }

            var allergies = _member.InsertAllergy(model.allergy);
            model.drquestionnaireResults.AllergyID = model.allergy.id;

            //=========================================================================================================================================================================//
            //                                                                               Social History                                                                            // 
            //=========================================================================================================================================================================//

            if (Request.Query["SocialHistory_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire = new ClinicalHistoryQuestionaire();

                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "DIABD";

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.drinker = true;
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                }
                else if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.NrDrinks = "0";
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                {
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                }

                if (String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]) || String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]) || String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.followUp = true;
                }
                else
                {
                    model.questionnaire.followUp = false;
                }

                model.questionnaire.active = true;
            }
            else if (Request.Query["SocialHistory_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire = new ClinicalHistoryQuestionaire();

                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.NoCigs = null;
                model.questionnaire.smokingYears = null;
                model.questionnaire.NrDrinks = null;
                model.questionnaire.programType = "DIABD";
                model.questionnaire.socialComment = "Entry ignored";
                model.questionnaire.followUp = false;
                model.questionnaire.active = false;

            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "DIABD";

                //Insert socialHistory questionnaire results
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = "0";
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdDate = DateTime.Now;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdBy = User.Identity.Name;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))

                    model.questionnaire.followUp = false;
                model.questionnaire.active = true;

            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("false"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.smoker = false;
                model.questionnaire.NoCigs = 0;
                model.questionnaire.smokingYears = 0;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = "0";
                model.questionnaire.programType = "DIABD";
                model.questionnaire.socialComment = (Request.Query["questionnaire.socialComment"]);
                model.questionnaire.followUp = false;
                model.questionnaire.active = true;
            }

            var clinicalQuestion = _member.InsertClinicalHistoryQuestionnaire(model.questionnaire);
            model.drquestionnaireResults.SocialHistoryID = model.questionnaire.id;

            //=========================================================================================================================================================================//
            //                                                                                  Vision                                                                                 // 
            //=========================================================================================================================================================================//

            if (Request.Query["Vision_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.vision = new Vision();

                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;

                if (Request.Query["vision.vision"].ToString() != null)
                    model.vision.vision = Request.Query["vision.vision"].ToString();
                if (Request.Query["vision.vision"] == "")
                    model.vision.vision = "• Follow up";

                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                if (Request.Query["vision.eyeTreatment"].ToString() == null)
                    model.vision.eyeTreatment = "• Follow up";

                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                if (Request.Query["vision.generalComments"].ToString() == null)
                    model.vision.generalComments = null;

                model.vision.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["vision.vision"]) || String.IsNullOrEmpty(Request.Query["vision.eyeTreatment"]))
                {
                    model.vision.followUp = true;
                }
                else
                {
                    model.vision.followUp = false;
                }

                model.vision.active = true;

            }
            else if (Request.Query["Vision_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.vision = new Vision();

                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                model.vision.vision = null;
                model.vision.eyeTreatment = null;
                model.vision.generalComments = "Entry ignored";
                model.vision.programType = "DIABD";
                model.vision.followUp = false;
                model.vision.active = false;

            }
            else
            {
                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                if (Request.Query["vision.vision"].ToString() != null)
                {
                    model.vision.vision = Request.Query["vision.vision"].ToString();
                    model.vision.visionCheck = true;
                }
                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                {
                    model.vision.eyeTreatmentCheck = true;
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                }
                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                model.vision.programType = "DIABD";
                model.vision.followUp = false;
                model.vision.active = true;

            }

            var visionResults = _member.InsertVisionResults(model.vision);
            model.drquestionnaireResults.VisionID = model.vision.VisionID;

            //=========================================================================================================================================================================//
            //                                                                                  Podiatry                                                                               // 
            //=========================================================================================================================================================================//

            if (Request.Query["Podiatry_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.podiatry = new Podiatry();

                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                //HCare-759
                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                else if (Request.Query["podiatry.amputationComment"] == "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = false;
                }
                else if (Request.Query["podiatry.amputationComment"].ToString() == null)
                {
                    model.podiatry.amputationComment = "• Follow up";
                    model.podiatry.amputationCheck = false;
                }

                if (Request.Query["podiatry.amputationReason"].ToString() != null)
                    model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();
                if (Request.Query["podiatry.amputationReason"].ToString() == null)
                    model.podiatry.amputationReason = "• Follow up";

                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
                if (Request.Query["podiatry.podiatryLopsComment"].ToString() == null)
                    model.podiatry.podiatryLopsComment = "• Follow up";

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() == null)
                    model.podiatry.podiatryDeformityComment = "• Follow up";

                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() == null)
                    model.podiatry.podiatryPerArterialDiseaseComment = "• Follow up";

                if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();

                if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() == null)
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "• Follow up";

                if (Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString() != null)
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                if (Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString() == null)
                    model.podiatry.podiatryWoundAffectedLeg = "• Follow up";

                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["podiatry.amputationComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.amputationReason"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryLopsComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryDeformityComment"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryPerArterialDiseaseComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryWoundAffectedLeg"]))
                {
                    model.podiatry.followUp = true;
                }
                else
                {
                    model.podiatry.followUp = false;
                }

                model.podiatry.active = true;
            }
            else if (Request.Query["Podiatry_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.podiatry = new Podiatry();

                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                model.podiatry.amputationCheck = false; //HCare-759
                model.podiatry.amputationComment = null;
                model.podiatry.amputationReason = null;
                model.podiatry.podiatryLopsComment = null;
                model.podiatry.podiatryDeformityComment = null;
                model.podiatry.podiatryPerArterialDiseaseComment = null;
                model.podiatry.podiatryWoundComment = null;
                model.podiatry.podiatryPerArterialDiseaseAffectedLeg = null;
                model.podiatry.podiatryWoundAffectedLeg = null;
                model.podiatry.generalComments = "Entry ignored";

                model.podiatry.programType = "DIABD";
                model.podiatry.followUp = false;
                model.podiatry.active = false;
            }
            else
            {
                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                else if (Request.Query["podiatry.amputationComment"] == "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = false;
                }
                else if (Request.Query["podiatry.amputationComment"].ToString() == null)
                {
                    model.podiatry.amputationComment = "• Follow up";
                    model.podiatry.amputationCheck = false;
                }

                if (Request.Query["podiatry.amputationReason"].ToString() != null)
                    model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();

                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                {
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
                    model.podiatry.podiatryLopsCheck = true;
                }

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                {
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                    model.podiatry.podiatryDeformityCheck = true;
                }

                if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();

                if (Request.Query["arterial_ci-true"].ToString().ToLower().Contains("true"))
                {
                    if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                        model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
                    model.podiatry.podiatryPerArterialDiseaseCheck = true;
                }
                else if (Request.Query["arterial_ci-false"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "-";
                    model.podiatry.podiatryPerArterialDiseaseCheck = false;
                }

                if (Request.Query["wounds_ci-true"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                    model.podiatry.podiatryWoundCheck = true;
                }
                else if (Request.Query["wounds_ci-false"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = "-";
                    model.podiatry.podiatryWoundCheck = false;
                }

                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";
                model.podiatry.followUp = false;
                model.podiatry.active = true;
            }

            var podiatryResults = _member.InsertPodiatryResults(model.podiatry);
            model.drquestionnaireResults.PodiatryID = model.podiatry.PodiatryID;

            //=========================================================================================================================================================================//
            //                                                                               Hypoglycaemia                                                                             // 
            //=========================================================================================================================================================================//

            if (Request.Query["Hypoglycaemia_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.hypoglycaemia = new Hypoglycaemia();

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdBy = User.Identity.Name;
                model.hypoglycaemia.createdDate = DateTime.Now;

                if (Request.Query["symptoms_ci-true"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;



                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() == null || Request.Query["hypoglycaemia.glucoseReading"] == "")
                    model.hypoglycaemia.glucoseReading = "• Follow up";

                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() == null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = "• Follow up";

                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() == null || Request.Query["hypoglycaemia.hypoglycaemiaAssistance"] == "")
                    model.hypoglycaemia.hypoglycaemiaAssistance = "• Follow up";

                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() != null)
                    model.hypoglycaemia.emergencyRoomVisits = Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString();
                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() == null || Request.Query["hypoglycaemia.emergencyRoomVisits"] == "")
                    model.hypoglycaemia.emergencyRoomVisits = "• Follow up";

                if (Request.Query["hypoglycaemia.insulinUseCheck"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.insulinUseCheck = true;

                if (Request.Query["hypoglycaemia.sulfonylureaUseCheck"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.sulfonylureaUseCheck = true;

                if (Request.Query["hypoglycaemia.ckdStageCheck"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.ckdStageCheck = true;

                if (Request.Query["hypoglycaemia.patientAgeCheck"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.patientAgeCheck = true;

                model.hypoglycaemia.programType = "DIABD";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();
                if (Request.Query["hypoglycaemia.generalComments"].ToString() == null)
                    model.hypoglycaemia.generalComments = "• Follow up";

                if (String.IsNullOrEmpty(Request.Query["hypoglycaemia.emergencyRoomVisits"]) || String.IsNullOrEmpty(Request.Query["hypoglycaemia.glucoseReading"]) ||
                    String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"]) || String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaAssistance"]))
                {
                    model.hypoglycaemia.followUp = true;
                }
                else
                {
                    model.hypoglycaemia.followUp = false;
                }

                model.hypoglycaemia.active = true;
            }
            else if (Request.Query["Hypoglycaemia_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.hypoglycaemia = new Hypoglycaemia();

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdBy = User.Identity.Name;
                model.hypoglycaemia.createdDate = DateTime.Now;

                model.hypoglycaemia.hypoglycaemiaComment = null;
                model.hypoglycaemia.emergencyRoomVisits = null;
                model.hypoglycaemia.glucoseReading = null;
                model.hypoglycaemia.hypoglycaemiaSymptomsComment = null;
                model.hypoglycaemia.hypoglycaemiaAssistance = null;
                model.hypoglycaemia.generalComments = "Entry ignored";

                model.hypoglycaemia.programType = "DIABD";
                model.hypoglycaemia.followUp = false;
                model.hypoglycaemia.active = false;
            }
            else
            {
                if (Request.Query["symptoms_ci-true"].ToString().ToLower().Contains("true"))
                {
                    model.hypoglycaemia.hypoglycaemiaCheck = true;
                }

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdDate = DateTime.Now;
                model.hypoglycaemia.createdBy = User.Identity.Name;

                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                {
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
                }
                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaComment = Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString();
                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() != null)
                    model.hypoglycaemia.emergencyRoomVisits = Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString();

                model.hypoglycaemia.programType = "DIABD";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();

                model.hypoglycaemia.followUp = false;
                model.hypoglycaemia.active = true;

            }

            var hyperGResults = _member.InsertHypoglycaemiaResults(model.hypoglycaemia);
            model.drquestionnaireResults.HypoglycaemiaID = model.hypoglycaemia.HypoglycaemiaID;

            //=========================================================================================================================================================================//
            //                                                                              Hospitalisations                                                                           // 
            //=========================================================================================================================================================================//

            model.HospitalAuths = new List<HospitalizationAuths>();

            var memberinfo = _member.GetDependentDetails(new Guid(Request.Query["DependantID"]), null);

            if (Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation_ci1"]))) //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation_ci" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();

                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation_ci" + i]))
                            hospitalisation.authType = Convert.ToString(Request.Query["diabetesHospitalisation_ci" + i]);
                        else
                            hospitalisation.authType = "• Follow up";

                        hospitalisation.programType = "DIABD";

                        if (!String.IsNullOrEmpty(Request.Query["HospitalAuth.generalComments"]))
                            hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);

                        if ((String.IsNullOrEmpty(Request.Query["diabetesHospitalisation_ci" + i])) && (String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_ci" + i])))
                            hospitalisation.followUp = true;
                        else
                            hospitalisation.followUp = false;

                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_ci" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_ci" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    createdDate = DateTime.Now,
                    authType = "Follow up",
                    programType = "DIABD",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = true,
                    Active = true,


                });
            }
            else if (Request.Query["Hospitalisation_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    createdDate = DateTime.Now,
                    authType = null,
                    programType = "DIABD",
                    generalComments = "Entry ignored",
                    followUp = false,
                    Active = false,

                });
            }
            else //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation_ci" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();
                        hospitalisation.authType = Convert.ToString(Request.Query["diabetesHospitalisation_ci" + i]);
                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;
                        hospitalisation.programType = "DIABD";
                        hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);
                        hospitalisation.followUp = false;
                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_ci" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_ci" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }

            if (Request.Query["hospitalized-false"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    createdDate = DateTime.Now,
                    authType = "None",
                    programType = "DIABD",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = false,
                    Active = false,
                });
            }


            foreach (var history in model.HospitalAuths)
            {
                var hZHistory = _member.InsertHospitalizationAuths(history);
            }

            //=========================================================================================================================================================================//
            //                                                                               AutoNeropathy                                                                             // 
            //=========================================================================================================================================================================//

            if (Request.Query["AutoNero_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.autoNeropathy = new AutoNeropathy();

                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.AutoNeropathyID = Convert.ToInt32(Request.Query["id"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.createdBy = User.Identity.Name;
                model.autoNeropathy.createdDate = DateTime.Now;

                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() != null)
                    model.autoNeropathy.autoNeuroSympComment = Request.Query["autoNeropathy.autoNeuroSympComment"].ToString();
                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() == null)
                    model.autoNeropathy.autoNeuroSympComment = "• Follow up";

                if (Request.Query["autoNeropathy.generalComments"].ToString() != null)
                    model.autoNeropathy.generalComments = Request.Query["autoNeropathy.generalComments"].ToString();
                if (Request.Query["autoNeropathy.generalComments"].ToString() == null)
                    model.autoNeropathy.generalComments = "• Follow up";

                model.autoNeropathy.programType = "DIABD";

                if ((String.IsNullOrEmpty(Request.Query["autoNeropathy.autoNeuroSympComment"])))
                    model.autoNeropathy.followUp = true;
                else
                    model.autoNeropathy.followUp = false;

                model.autoNeropathy.active = true;
            }
            else if (Request.Query["AutoNero_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.createdDate = DateTime.Now;
                model.autoNeropathy.createdBy = User.Identity.Name;
                model.autoNeropathy.autoNeuroSympComment = null;
                model.autoNeropathy.generalComments = "Entry ignored";
                model.autoNeropathy.programType = "DIABD";
                model.autoNeropathy.followUp = false;
                model.autoNeropathy.active = false;
            }
            else
            {
                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.createdDate = DateTime.Now;
                model.autoNeropathy.createdBy = User.Identity.Name;

                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() != null)
                    model.autoNeropathy.autoNeuroSympComment = Request.Query["autoNeropathy.autoNeuroSympComment"].ToString();

                if (Request.Query["autoNeropathy.generalComments"].ToString() != null)
                    model.autoNeropathy.generalComments = Request.Query["autoNeropathy.generalComments"].ToString();

                model.autoNeropathy.programType = "DIABD";
                model.autoNeropathy.followUp = false;
                model.autoNeropathy.active = true;
            }

            var autoNeroResults = _member.InsertAutoNeroResults(model.autoNeropathy);
            model.drquestionnaireResults.AutoNeropathyID = model.autoNeropathy.AutoNeropathyID;

            //=========================================================================================================================================================================//
            //                                                                                     Other                                                                               // 
            //=========================================================================================================================================================================//

            if (Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;
                #region occupation
                if (Request.Query["questionnaireOther.occupation"].ToString() != null)
                    model.questionnaireOther.occupation = Request.Query["questionnaireOther.occupation"].ToString();
                if (Request.Query["questionnaireOther.shiftWorker"].ToString() != null)
                    model.questionnaireOther.shiftWorker = Request.Query["questionnaireOther.shiftWorker"].ToString();
                #endregion
                #region drug-abuse
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.recDrugsLastUsed"].ToString()))
                {
                    model.questionnaireOther.recDrugs = true;
                    model.questionnaireOther.recDrugsLastUsed = Convert.ToDateTime(Request.Query["questionnaireOther.recDrugsLastUsed"]);
                }
                else
                {
                    model.questionnaireOther.recDrugs = false;
                }
                #endregion
                #region lypohypertrophy
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.lypohypertrophy"].ToString()))
                {
                    model.questionnaireOther.lypohypertrophyCheck = true;
                    model.questionnaireOther.lypohypertrophy = Request.Query["questionnaireOther.lypohypertrophy"].ToString();
                }
                else
                {
                    model.questionnaireOther.lypohypertrophyCheck = false;
                }
                #endregion
                #region depression
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                {
                    model.questionnaireOther.depression = true;
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();
                }
                else
                {
                    model.questionnaireOther.depression = false;
                }
                #endregion
                #region tb-screen
                //HCare-966
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.TBScreenResult = null;

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentStartDate"].ToString())) { model.questionnaireOther.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentStartDate"]); model.questionnaireOther.tbDiagnosed = true; }
                else { model.questionnaireOther.tbDiagnosed = false; }
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentEndDate"].ToString())) { model.questionnaireOther.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentEndDate"]); }
                #endregion
                #region pregnant
                //HCare-968
                model.questionnaireOther.TreatingDoctor = null;
                model.questionnaireOther.isDoctorAware = false;
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString()))
                {
                    model.questionnaireOther.Pregnant = true;
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString())) { model.questionnaireOther.EDD = Convert.ToDateTime(Request.Query["questionnaireOther.EDD"]); }
                }
                else
                {
                    model.questionnaireOther.Pregnant = false;
                }
                #endregion
                #region frail-care
                //HCare-930
                model.questionnaireOther.hasKidneyInfection = false;
                model.questionnaireOther.frailCareCheck = false; //HCare-930
                model.questionnaireOther.frailCare = "• None"; //HCare-930
                model.questionnaireOther.frailCareComment = null; //HCare-930
                #endregion
                #region blood-test
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;
                #endregion
                #region follow-up
                model.questionnaireOther.followUp = true;
                #endregion
                #region comments
                if (Request.Query["questionnaireOther.generalComments"].ToString() != null)
                    model.questionnaireOther.generalComments = Request.Query["questionnaireOther.generalComments"].ToString();
                #endregion
                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.active = true;
            }
            else if (Request.Query["Other_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();

                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.Pregnant = false; //HCare-968
                model.questionnaireOther.TreatingDoctor = null; //HCare-968
                model.questionnaireOther.isDoctorAware = false; //HCare-968
                model.questionnaireOther.hasKidneyInfection = false;
                model.questionnaireOther.frailCareCheck = false; //HCare-930
                model.questionnaireOther.frailCare = "• None"; //HCare-930
                model.questionnaireOther.frailCareComment = null; //HCare-930
                model.questionnaireOther.generalComments = "Entry ignored - Diabetes questionnaire";
                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = false;

            }
            else
            {
                model.questionnaireOther = new QuestionnaireOther();
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;
                #region occupation
                if (Request.Query["questionnaireOther.occupation"].ToString() != null)
                    model.questionnaireOther.occupation = Request.Query["questionnaireOther.occupation"].ToString();
                if (Request.Query["questionnaireOther.shiftWorker"].ToString() != null)
                    model.questionnaireOther.shiftWorker = Request.Query["questionnaireOther.shiftWorker"].ToString();
                #endregion
                #region drug-abuse
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.recDrugsLastUsed"].ToString()))
                {
                    model.questionnaireOther.recDrugs = true;
                    model.questionnaireOther.recDrugsLastUsed = Convert.ToDateTime(Request.Query["questionnaireOther.recDrugsLastUsed"]);
                }
                else
                {
                    model.questionnaireOther.recDrugs = false;
                }
                #endregion
                #region lypohypertrophy
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.lypohypertrophy"].ToString()))
                {
                    model.questionnaireOther.lypohypertrophyCheck = true;
                    model.questionnaireOther.lypohypertrophy = Request.Query["questionnaireOther.lypohypertrophy"].ToString();
                }
                else
                {
                    model.questionnaireOther.lypohypertrophyCheck = false;
                }
                #endregion
                #region depression
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                {
                    model.questionnaireOther.depression = true;
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();
                }
                else
                {
                    model.questionnaireOther.depression = false;
                }
                #endregion
                #region tb-screen
                //HCare-966
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.TBScreenResult = null;

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentStartDate"].ToString())) { model.questionnaireOther.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentStartDate"]); model.questionnaireOther.tbDiagnosed = true; }
                else { model.questionnaireOther.tbDiagnosed = false; }
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentEndDate"].ToString())) { model.questionnaireOther.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentEndDate"]); }
                #endregion
                #region pregnant
                //HCare-968
                model.questionnaireOther.TreatingDoctor = null;
                model.questionnaireOther.isDoctorAware = false;
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString()))
                {
                    model.questionnaireOther.Pregnant = true;
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString())) { model.questionnaireOther.EDD = Convert.ToDateTime(Request.Query["questionnaireOther.EDD"]); }
                }
                else
                {
                    model.questionnaireOther.Pregnant = false;
                }
                #endregion
                #region frail-care
                //HCare-930
                model.questionnaireOther.hasKidneyInfection = false;
                model.questionnaireOther.frailCareCheck = false; //HCare-930
                model.questionnaireOther.frailCare = "• None"; //HCare-930
                model.questionnaireOther.frailCareComment = null; //HCare-930
                #endregion
                #region blood-test
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;
                #endregion
                #region follow-up
                model.questionnaireOther.followUp = false;
                #endregion
                #region comments
                if (Request.Query["questionnaireOther.generalComments"].ToString() != null)
                    model.questionnaireOther.generalComments = Request.Query["questionnaireOther.generalComments"].ToString();
                #endregion
                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.active = true;

            }

            var questionOtherResults = _member.InsertQuestionnaireOtherResults(model.questionnaireOther);
            model.drquestionnaireResults.QuestionnaireOtherID = model.questionnaireOther.QuestionnaireOtherID;

            //=========================================================================================================================================================================//
            //                                                                              DrQuestionnaire INSERT                                                                     // 
            //=========================================================================================================================================================================//

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);


            //=========================================================================================================================================================================//
            //                                                                                    Task Update                                                                          // 
            //=========================================================================================================================================================================//

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            //check for follow ups to create new Assignment

            if (
                Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["diagnosis_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Allergy_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["SocialHistory_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Vision_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Podiatry_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["AutoNero_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Hypoglycaemia_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true")

            )
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Diabetes questionnaire follow up",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in taskss)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        //=============================================================================== NEW Diabetes Questionnaire ===============================================================================//

        [HttpGet]
        public ActionResult _halocareDiabetesQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode, Guid? pro)
        {
            var model = new DoctorQuestionnaireViewModel();

            Guid? programCode;
            if (Request.Query["pro"].ToString() == null)
            {
                programCode = _member.GetPrograms().Select(x => x.programID).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            }

            ViewBag.pro = programCode;

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.dependantCode = dependantCode;
            ViewBag.membershipNo = membershipNo;

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;

            model.ScriptItems = _member.GetScriptItems(DependentID);
            model.dependent = _member.GetDependantByDependantID(DependentID);
            model.member = _member.GetMemberByDependantID(DependentID);
            model.contact = _member.GetContact(DependentID);

            model.allergies = new List<Allergies>();
            model.allergies = _member.GetAllergies(DependentID);

            model.coMorbidTypes = _member.GetCoMorbids().ToList();
            model.ComorbidViews = _member.GetComorbidItems(DependentID, pro);

            model.patientDisclaimers = new List<PatientDisclaimer>();
            model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(DependentID);

            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == pro).ToList(); //HCare-1386
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                    model.doctorContact = _member.GetDrInformation(doctorh[0].doctorId);
                }
            }

            var memberinfo = _member.GetDependentDetails(DependentID, null);

            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);

            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");
            #region comorbid-loop
            ViewBag.comorbid1 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid2 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid3 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid4 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid5 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid6 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid7 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid8 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid9 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbid10 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            #endregion


            return View(model);
        }

        [HttpPost]
        public ActionResult _halocareDiabetesQuestionnaire(DoctorQuestionnaireViewModel model)
        {

            //=========================================================================================================================================================================//
            //                                                                        DoctorQuestionnaireResults                                                                       // 
            //=========================================================================================================================================================================//

            model.drquestionnaireResults = new DoctorQuestionnaireResults();

            model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
            model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.Active = true;

            //=========================================================================================================================================================================//
            //                                                                           Clincal History Information                                                                   // 
            //=========================================================================================================================================================================//


            if (Request.Query["clinicalexam-followUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.clinicalexam = new Clinical();

                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))

                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }
            else if (Request.Query["clinicalexam-followUp"].ToString().ToLower().Contains("true"))
            {
                model.clinicalexam = new Clinical();
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);

                if (model.clinicalexam.height == 0 && model.clinicalexam.weight == 0)
                    model.clinicalexam.followUp = true;
                else
                    model.clinicalexam.followUp = false;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                model.clinicalexam.active = true;

            }
            else
            {
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "DIABD";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }
                else if (model.clinicalexam.height == 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight == 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = 0;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }

            var clinicalExam = _member.InsertClinicalExam(model.clinicalexam);
            model.drquestionnaireResults.ClinicalHistoryID = model.clinicalexam.id;



            //=========================================================================================================================================================================//
            //                                                                                 Pathology Insert                                                                        // 
            //=========================================================================================================================================================================//

            if (Request.Query["clinicalexam-followUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "DIABD";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Diabetes questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["clinicalexam-followUp"].ToString().ToLower().Contains("true"))
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "DIAB";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Diabetes questionnaire follow-up";
                model.pathology.active = true;
            }
            else if ((model.pathology.diastolicBP != null && model.pathology.systolicBP != null) || (model.pathology.diastolicBP != 0 && model.pathology.systolicBP != 0))
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "DIABD";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                if (model.pathology.systolicBP == 0 && model.pathology.diastolicBP == 0) //HCare-1063
                {
                    model.pathology.comment = "Entry ignored - Diabetes questionnaire";
                    model.pathology.active = false;
                }
                else
                {
                    model.pathology.comment = "Diabetes questionnaire";
                    model.pathology.active = true;
                }
            }
            else if (model.pathology.diastolicBP == 0 && model.pathology.systolicBP == 0)
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Entry ignored - Diabetes questionnaire";
                model.pathology.diastolicBP = 0;
                model.pathology.systolicBP = 0;
                model.pathology.active = false;
            }

            var pathology = _member.InsertPathology(model.pathology);

            //=========================================================================================================================================================================//
            //                                                                                 Diagnosis Insert                                                                        // 
            //=========================================================================================================================================================================//

            if (Request.Query["PatientDiagnosis-followUp"].ToString().ToLower().Contains("true"))
            {
                model.PatientDiagnosis = new PatientDiagnosis();

                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.ProgramCode = "DIABD";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;

                if (!String.IsNullOrEmpty(Request.Query["PatientDiagnosis.EffectiveDate"]))
                    model.PatientDiagnosis.EffectiveDate = Convert.ToDateTime(Request.Query["PatientDiagnosis.EffectiveDate"]);
                else
                    model.PatientDiagnosis.EffectiveDate = null;

                if (!String.IsNullOrEmpty(Request.Query["PatientDiagnosis.generalComments"]))
                    model.PatientDiagnosis.Comment= (Request.Query["PatientDiagnosis.generalComments"]);
                else
                    model.PatientDiagnosis.Comment= null;

                if (String.IsNullOrEmpty(Request.Query["PatientDiagnosis.EffectiveDate"]))
                    model.PatientDiagnosis.FollowUp = true;
                else
                    model.PatientDiagnosis.FollowUp = false;

                model.PatientDiagnosis.Active = true;

            }
            else if (model.PatientDiagnosis.EffectiveDate != null || model.PatientDiagnosis.Comment!= null)
            {
                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.ProgramCode = "DIABD";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment= (Request.Query["PatientDiagnosis.generalComments"]);
                model.PatientDiagnosis.FollowUp = false;
                model.PatientDiagnosis.Active = true;

            }

            var diagnosisDate = _member.InsertDiagnosisResults(model.PatientDiagnosis);

            //=========================================================================================================================================================================//
            //                                                                            CoMorbid Conditions                                                                          // 
            //=========================================================================================================================================================================//

            model.comormidConditions = new List<CoMormidConditions>(); //HCare-607

            if (Request.Query["comorbid_followUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["comorbid1"]))) //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["comorbid" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["comorbid" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "DIABD";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);

                        if (!String.IsNullOrEmpty(Request.Query["comorbid" + i]))
                            comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        else
                            comorb.coMorbidId = 99998;

                        if (!String.IsNullOrEmpty(Request.Query["d-comorbidEffectiveDate" + i]))
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["d-comorbidEffectiveDate" + i]);
                        else
                            comorb.effectiveDate = null;

                        if (!String.IsNullOrEmpty(Request.Query["d-comorbidEndDate" + i]))
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["d-comorbidEndDate" + i]);
                        else
                            comorb.treatementEndDate = null;

                        if (String.IsNullOrEmpty(Request.Query["comorbid" + i]))
                            comorb.followUp = true;
                        else
                            comorb.followUp = false;

                        comorb.active = true;

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["comorbid_followUp"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 99998,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    effectiveDate = null,
                    treatementEndDate = null,
                    dependantID = new Guid(Request.Query["DependantID"]),
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    generalComments = (Request.Query["comormidCondition.generalComments"]),
                    followUp = true,
                    active = true,

                });
            }
            else if (Request.Query["comorbid-false"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 99999,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    effectiveDate = DateTime.Now,
                    treatementEndDate = null,
                    dependantID = new Guid(Request.Query["DependantID"]),
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    generalComments = (Request.Query["comormidCondition.generalComments"]),
                    followUp = false,
                    active = false,

                });
            }
            else //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["comorbid" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["comorbid" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "DIABD";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);
                        comorb.active = true;

                        if (!String.IsNullOrEmpty(Request.Query["d-comorbidEffectiveDate" + i]))
                        {
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["d-comorbidEffectiveDate" + i]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["d-comorbidEndDate" + i]))
                        {
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["d-comorbidEndDate" + i]);
                        }

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }

            }

            foreach (var history in model.comormidConditions)
            {
                var coMorbHistory = _member.InsertComorbidCondition(history);
            }

            //=========================================================================================================================================================================//
            //                                                                             Medication History                                                                          // 
            //=========================================================================================================================================================================//

            model.MedicationHistories = new List<MedicationHistory>();

            if (Request.Query["medication-followUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct1"])))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]))
                    {
                        var medication = new MedicationHistory();

                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]))
                            medication.productName = Request.Query["diabetesMedicationProduct" + i];
                        else
                            medication.productName = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationComment" + i]))
                            medication.comment = Request.Query["diabetesMedicationComment" + i];
                        else
                            medication.comment = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";

                        if (String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]) && String.IsNullOrEmpty(Request.Query["diabetesMedicationComment" + i]))
                            medication.followUp = true;
                        else
                            medication.followUp = false;

                        medication.active = true;

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["medication-followUp"].ToString().ToLower().Contains("true") && Request.Query["medicationNaive-true"].ToString().ToLower().Contains("true"))
            {
                for (int i = 0; i < 1; i++)
                {
                    if (String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate" + i]))
                    {
                        var medication = new MedicationHistory();

                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]))
                            medication.productName = Request.Query["diabetesMedicationProduct" + i];
                        else
                            medication.productName = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationComment" + i]))
                            medication.comment = Request.Query["diabetesMedicationComment" + i];
                        else
                            medication.comment = "Follow up";

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";

                        if (String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]) && String.IsNullOrEmpty(Request.Query["diabetesMedicationComment" + i]))
                            medication.followUp = true;
                        else
                            medication.followUp = false;

                        medication.active = true;

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["medication-followUp"].ToString().ToLower().Contains("true"))
            {
                model.MedicationHistories.Add(new MedicationHistory()
                {
                    dependantId = new Guid(Request.Query["DependantID"]),
                    Id = Convert.ToInt32(Request.Query["id"]),
                    createdDate = DateTime.Now,
                    nappiCode = "-",
                    productName = "Follow up",
                    directions = "Follow up",
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    comment = "Follow up",
                    followUp = true,
                    active = true,

                    QuestionnaireID = model.drquestionnaireResults.Id,

                });
            }
            else if (Request.Query["medicationNaive-false"].ToString().ToLower().Contains("true"))
            {
                model.MedicationHistories.Add(new MedicationHistory()
                {
                    dependantId = new Guid(Request.Query["DependantID"]),
                    Id = Convert.ToInt32(Request.Query["id"]),
                    createdDate = DateTime.Now,
                    nappiCode = "-",
                    productName = "None",
                    directions = "None",
                    startDate = null,
                    endDate = null,
                    createdBy = User.Identity.Name,
                    programType = "DIABD",
                    comment = "None",
                    followUp = false,
                    active = true,

                    QuestionnaireID = model.drquestionnaireResults.Id,

                });
            }
            else
            {
                for (int i = 1; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesMedicationProduct" + i]))
                    {
                        var medication = new MedicationHistory();
                        medication.productName = Request.Query["diabetesMedicationProduct" + i];
                        medication.comment = Request.Query["diabetesMedicationComment" + i];
                        medication.dependantId = new Guid(Request.Query["DependantID"]);
                        medication.Id = Convert.ToInt32(Request.Query["id"]);
                        medication.createdBy = User.Identity.Name;
                        medication.createdDate = DateTime.Now;
                        medication.nappiCode = "-";
                        medication.programType = "DIABD";
                        medication.directions = "-";
                        medication.followUp = false;
                        medication.active = true;

                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.startDate"]))
                        {
                            medication.startDate = Convert.ToDateTime(Request.Query["MedicationHistory.startDate"]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["MedicationHistory.endDate"]))
                        {
                            medication.endDate = Convert.ToDateTime(Request.Query["MedicationHistory.endDate"]);
                        }

                        medication.QuestionnaireID = model.drquestionnaireResults.Id;

                        model.MedicationHistories.Add(medication);
                    }
                    else
                        continue;
                }
            }

            foreach (var history in model.MedicationHistories)
            {
                var medHistory = _member.InsertMedicationHistory(history);
            }

            //=========================================================================================================================================================================//
            //                                                                                  Allergy Insert                                                                         // 
            //=========================================================================================================================================================================//

            if (Request.Query["allergy-followUp"].ToString().ToLower().Contains("true"))
            {
                model.allergy = new Allergies();

                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);

                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.Allergy = "Follow up";
                else
                    model.allergy.Allergy = Request.Query["allergy.Allergy"];

                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["allergy.generalComments"]))
                    model.allergy.generalComments = null;
                else
                    model.allergy.generalComments = (Request.Query["allergy.generalComments"]);

                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.followUp = true;
                else
                    model.allergy.followUp = false;

                model.allergy.active = true;
            }
            else if (Request.Query["allergy.hasAllergy"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.Allergy = Request.Query["allergy.Allergy"];
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                model.allergy.followUp = false;
                model.allergy.active = true;

            }
            else if (Request.Query["allergy.hasAllergy"].ToString().ToLower().Contains("false"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.Allergy = "None";
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["allergy.generalComments"]))
                    model.allergy.generalComments = "-";
                else
                    model.allergy.generalComments = (Request.Query["allergy.generalComments"]); model.allergy.followUp = false;

                model.allergy.active = true;

            }

            var allergies = _member.InsertAllergy(model.allergy);
            model.drquestionnaireResults.AllergyID = model.allergy.id;

            //=========================================================================================================================================================================//
            //                                                                               Social History                                                                            // 
            //=========================================================================================================================================================================//

            if (Request.Query["social-followUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire = new ClinicalHistoryQuestionaire();

                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "DIABD";

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
                }

                model.questionnaire.NrDrinks = null;

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                {
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                }

                if (String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]) || String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                {
                    model.questionnaire.followUp = true;
                }
                else
                {
                    model.questionnaire.followUp = false;
                }

                model.questionnaire.active = true;
            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "DIABD";

                //Insert socialHistory questionnaire results
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdDate = DateTime.Now;

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdBy = User.Identity.Name;

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                else
                    model.questionnaire.socialComment = null;

                model.questionnaire.followUp = false;
                model.questionnaire.active = true;

            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("false"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.NoCigs = 0;
                model.questionnaire.smoker = false;
                model.questionnaire.smokingYears = 0;
                model.questionnaire.NrDrinks = "0";
                model.questionnaire.drinker = false;
                model.questionnaire.programType = "DIABD";

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                else
                    model.questionnaire.socialComment = null;

                model.questionnaire.followUp = false;
                model.questionnaire.active = true;

            }

            var clinicalQuestion = _member.InsertClinicalHistoryQuestionnaire(model.questionnaire);
            model.drquestionnaireResults.SocialHistoryID = model.questionnaire.id;

            //=========================================================================================================================================================================//
            //                                                                                  Vision                                                                                 // 
            //=========================================================================================================================================================================//

            if (Request.Query["vision-followUp"].ToString().ToLower().Contains("true"))
            {
                model.vision = new Vision();

                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                model.vision.vision = "• N/A";

                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                if (Request.Query["vision.eyeTreatment"].ToString() == null)
                    model.vision.eyeTreatment = "• Follow up";
                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                if (Request.Query["vision.generalComments"].ToString() == null)
                    model.vision.generalComments = null;

                model.vision.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["vision.eyeTreatment"]))
                {
                    model.vision.followUp = true;
                }
                else
                {
                    model.vision.followUp = false;
                }

                model.vision.active = true;

            }
            else
            {
                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                model.vision.vision = "• N/A";
                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                model.vision.programType = "DIABD";
                model.vision.followUp = false;
                model.vision.active = true;
            }

            var visionResults = _member.InsertVisionResults(model.vision);
            model.drquestionnaireResults.VisionID = model.vision.VisionID;

            //=========================================================================================================================================================================//
            //                                                                                  Podiatry                                                                               // 
            //=========================================================================================================================================================================//

            if (Request.Query["podiatries_followUp"].ToString().ToLower().Contains("true"))
            {
                model.podiatry = new Podiatry();

                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                //HCare-759
                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                else if (Request.Query["podiatry.amputationComment"] == "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = false;
                }
                else if (Request.Query["podiatry.amputationComment"].ToString() == null)
                {
                    model.podiatry.amputationComment = "• Follow up";
                    model.podiatry.amputationCheck = false;
                }

                if (Request.Query["podiatry.amputationReason"].ToString() != null)
                    model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();
                if (Request.Query["podiatry.amputationReason"].ToString() == null)
                    model.podiatry.amputationReason = "• N/A";

                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
                if (Request.Query["podiatry.podiatryLopsComment"].ToString() == null)
                    model.podiatry.podiatryLopsComment = "• Follow up";

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() == null)
                    model.podiatry.podiatryDeformityComment = "• Follow up";

                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() == null)
                    model.podiatry.podiatryPerArterialDiseaseComment = "• Follow up";

                if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();
                if (Request.Query["podiatry.podiatryWoundComment"].ToString() == null)
                    model.podiatry.podiatryWoundComment = "• N/A";

                //if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                //    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                //if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() == null)
                //    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "• Follow up";

                if (Request.Query["wounds-true"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                    model.podiatry.podiatryWoundCheck = true;
                }
                if (!Request.Query["wounds-true"].ToString().ToLower().Contains("true") && !Request.Query["wounds-false"].ToString().ToLower().Contains("true"))
                    model.podiatry.podiatryWoundAffectedLeg = "• Follow up";

                if (Request.Query["arterial-true"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                    model.podiatry.podiatryPerArterialDiseaseCheck = true;
                }

                if (!Request.Query["arterial-true"].ToString().ToLower().Contains("true") && !Request.Query["arterial-false"].ToString().ToLower().Contains("true"))
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "• Follow up";

                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["podiatry.amputationComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryLopsComment"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryDeformityComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryPerArterialDiseaseComment"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryWoundAffectedLeg"]))
                {
                    model.podiatry.followUp = true;
                }
                else
                {
                    model.podiatry.followUp = false;
                }

                model.podiatry.active = true;
            }
            else
            {
                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;
                model.podiatry.amputationReason = "• N/A";
                model.podiatry.podiatryWoundComment = "• N/A";

                //HCare-759
                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();

                //if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                //    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();
                //if (Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString() != null)
                //    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();

                if (Request.Query["wounds-true"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                    model.podiatry.podiatryWoundCheck = true;
                }

                if (Request.Query["arterial-true"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                    model.podiatry.podiatryPerArterialDiseaseCheck = true;
                }

                if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";
                model.podiatry.followUp = false;
                model.podiatry.active = true;
            }

            var podiatryResults = _member.InsertPodiatryResults(model.podiatry);
            model.drquestionnaireResults.PodiatryID = model.podiatry.PodiatryID;


            //=========================================================================================================================================================================//
            //                                                                               Hypoglycaemia                                                                             // 
            //=========================================================================================================================================================================//

            if (Request.Query["hypoG-followUp"].ToString().ToLower().Contains("true"))
            {
                model.hypoglycaemia = new Hypoglycaemia();

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdBy = User.Identity.Name;
                model.hypoglycaemia.createdDate = DateTime.Now;
                model.hypoglycaemia.hypoglycaemiaComment = "• N/A";
                model.hypoglycaemia.emergencyRoomVisits = "• N/A";

                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() == null || Request.Query["hypoglycaemia.glucoseReading"] == "")
                    model.hypoglycaemia.glucoseReading = "• Follow up";

                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() == null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = "• Follow up";

                if (Request.Query["symptoms-true"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;

                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() == null || Request.Query["hypoglycaemia.hypoglycaemiaAssistance"] == "")
                    model.hypoglycaemia.hypoglycaemiaAssistance = "• Follow up";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();
                if (Request.Query["hypoglycaemia.generalComments"].ToString() == null)
                    model.hypoglycaemia.generalComments = "• Follow up";

                model.hypoglycaemia.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["hypoglycaemia.glucoseReading"]) || String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"]) ||
                    String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaAssistance"]))
                {
                    model.hypoglycaemia.followUp = true;
                }
                else
                {
                    model.hypoglycaemia.followUp = false;
                }

                model.hypoglycaemia.active = true;
            }
            else
            {
                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdDate = DateTime.Now;
                model.hypoglycaemia.createdBy = User.Identity.Name;

                if (Request.Query["symptoms-true"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;

                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();

                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();

                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();

                if (Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString() == null)
                    model.hypoglycaemia.hypoglycaemiaComment = "• N/A";

                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() == null)
                    model.hypoglycaemia.emergencyRoomVisits = "• N/A";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();

                model.hypoglycaemia.programType = "DIABD";
                model.hypoglycaemia.followUp = false;
                model.hypoglycaemia.active = true;

            }

            var hyperGResults = _member.InsertHypoglycaemiaResults(model.hypoglycaemia);
            model.drquestionnaireResults.HypoglycaemiaID = model.hypoglycaemia.HypoglycaemiaID;

            //=========================================================================================================================================================================//
            //                                                                              Hospitalisations                                                                           // 
            //=========================================================================================================================================================================//

            model.HospitalAuths = new List<HospitalizationAuths>();

            var memberinfo = _member.GetDependentDetails(new Guid(Request.Query["DependantID"]), null);

            if (Request.Query["hospital-followUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation1"]))) //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();

                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;

                        if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation" + i]))
                            hospitalisation.authType = Convert.ToString(Request.Query["diabetesHospitalisation" + i]);
                        else
                            hospitalisation.authType = "• Follow up";

                        hospitalisation.programType = "DIABD";

                        if (!String.IsNullOrEmpty(Request.Query["HospitalAuth.generalComments"]))
                            hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);

                        if ((String.IsNullOrEmpty(Request.Query["diabetesHospitalisation" + i])) && (String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_d" + i])))
                            hospitalisation.followUp = true;
                        else
                            hospitalisation.followUp = false;

                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_d" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_d" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["hospital-followUp"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    createdDate = DateTime.Now,
                    authType = "Follow up",
                    programType = "DIABD",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = true,
                    Active = true,


                });
            }
            else //HCare-1081
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["diabetesHospitalisation" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();
                        hospitalisation.authType = Convert.ToString(Request.Query["diabetesHospitalisation" + i]);
                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;
                        hospitalisation.programType = "DIABD";
                        hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);
                        hospitalisation.followUp = false;
                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_d" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_d" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }

            if (Request.Query["hospital-false"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    createdDate = DateTime.Now,
                    authType = "None",
                    programType = "DIABD",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = false,
                    Active = false,
                });
            }

            foreach (var history in model.HospitalAuths)
            {
                var hZHistory = _member.InsertHospitalizationAuths(history);
            }

            //=========================================================================================================================================================================//
            //                                                                              DrQuestionnaire INSERT                                                                     // 
            //=========================================================================================================================================================================//

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);


            //=========================================================================================================================================================================//
            //                                                                                    Task Update                                                                          // 
            //=========================================================================================================================================================================//

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            //check for follow ups to create new Assignment

            if (
                Request.Query["clinicalexam-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["PatientDiagnosis-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["comorbid_followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["medication-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["allergy-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["social-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["vision-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["podiatries_followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["hypoG-followUp"].ToString().ToLower().Contains("true") ||
                Request.Query["hospital-followUp"].ToString().ToLower().Contains("true")

            )
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Diabetes questionnaire follow up",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in taskss)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        //================================================================================== Questionnaire_General ==================================================================================//

        public ActionResult _GeneralQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode, int script = 0)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();

            ViewBag.programId = programCode;

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;

            var model = new DoctorQuestionnaireViewModel();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.dependantCode = dependantCode;
            ViewBag.membershipNo = membershipNo;

            model.ScriptItems = new List<ScriptViewModel>();
            model.ScriptItems = _member.GetGeneralScriptItems(DependentID);
            model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program == program).ToList();
            model.dependent = _member.GetDependantByDependantID(DependentID);
            model.member = _member.GetMemberByDependantID(DependentID);
            model.contact = _member.GetContact(DependentID);

            model.clinicalexams = _member.GetClinicalExam(DependentID);

            model.questionnaires = new List<ClinicalHistoryQuestionaire>();
            model.questionnaires = _member.GetClinicalHistory(DependentID);

            model.patientDisclaimers = new List<PatientDisclaimer>();
            model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(DependentID);

            model.comormidConditions = _member.CoMorbid_Validation(DependentID);
            model.coMorbidTypes = _member.GetCoMorbids().ToList();
            model.ComorbidViews = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));

            model.comorbidConditions = _member.GetCoMorbids_Excluded(DependentID);

            model.allergies = new List<Allergies>();
            model.allergies = _member.GetAllergies(DependentID);

            model.questionnaireOthers = new List<QuestionnaireOther>();
            model.questionnaireOthers = _member.GetOtherResults(DependentID);

            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == new Guid(Request.Query["pro"])).ToList();
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                    model.doctorContact = _member.GetDrInformation(doctorh[0].doctorId);
                }
            }

            var memberinfo = _member.GetDependentDetails(DependentID, null);

            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);

            //-->>

            var scriptVM = new ScriptCreationViewModel();
            var scripts = new Scripts();

            scriptVM.script = scripts;
            scriptVM.script.dependentID = DependentID;
            scriptVM.script.effectiveDate = DateTime.Now;
            scriptVM.script.scriptID = script;

            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");
            ViewBag.coMorbids = new SelectList(_member.GetCoMorbids(), "id", "icd10Condition");
            //ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName");
            ViewBag.programType = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            #region program-loop
            //We need to do it this way until a better solution presents itself - Discussed with Steve 24 October 2019 08:30
            ViewBag.programType1 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType2 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType3 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType4 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType5 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType6 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType7 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType8 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType9 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            ViewBag.programType10 = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");
            #endregion
            #region comorbid-loop
            ViewBag.comorbids1 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids2 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids3 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids4 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids5 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids6 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids7 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids8 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids9 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            ViewBag.comorbids10 = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "mappingCode", "condition");
            #endregion

            return View(model);
        }

        [HttpPost]
        public ActionResult _GeneralQuestionnaire(DoctorQuestionnaireViewModel model)
        {

            //=========================================================================================================================================================================//
            //                                                                        DoctorQuestionnaireResults                                                                       // 
            //=========================================================================================================================================================================//

            model.drquestionnaireResults = new DoctorQuestionnaireResults();

            model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
            model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.Active = true;

            //=========================================================================================================================================================================//
            //                                                                           Clincal History Information                                                                   // 
            //=========================================================================================================================================================================//

            if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.clinicalexam = new Clinical();

                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "GEN";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))

                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }
            else if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.clinicalexam = new Clinical();
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.clinicalComment = "Follow up";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.programType = "GEN";
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);

                if (model.clinicalexam.height == 0 && model.clinicalexam.weight == 0)
                    model.clinicalexam.followUp = true;
                else
                    model.clinicalexam.followUp = false;

                if (Request.Query["clinicalexam.effectiveDate"].ToString() != null)
                    model.clinicalexam.effectiveDate = Convert.ToDateTime(Request.Query["clinicalexam.effectiveDate"]);
                else
                    model.clinicalexam.effectiveDate = null;

                model.clinicalexam.active = true;

            }
            else if (Request.Query["Clinical_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.programType = "GEN";
                model.clinicalexam.height = 0;
                model.clinicalexam.weight = 0;
                model.clinicalexam.clinicalComment = "Entry ignored - General questionnaire";
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = false;

            }
            else
            {
                model.clinicalexam.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
                model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
                model.clinicalexam.createdDate = DateTime.Now;
                model.clinicalexam.createdBy = User.Identity.Name;
                model.clinicalexam.programType = "GEN";
                model.clinicalexam.height = decimal.Parse(Request.Query["clinicalexam.height"], CultureInfo.InvariantCulture);//HCare-1050            
                model.clinicalexam.weight = decimal.Parse(Request.Query["clinicalexam.weight"], CultureInfo.InvariantCulture);//HCare-1050
                model.clinicalexam.clinicalComment = (Request.Query["clinicalexam.clinicalComment"]);
                model.clinicalexam.followUp = false;
                model.clinicalexam.active = true;

                if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }
                else if (model.clinicalexam.height == 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight == 0 || model.clinicalexam.weight.Equals(null))
                {
                    model.clinicalexam.bmi = 0;
                    var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
                    model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
                }

            }

            var clinicalExam = _member.InsertClinicalExam(model.clinicalexam);
            model.drquestionnaireResults.ClinicalHistoryID = model.clinicalexam.id;

            //=========================================================================================================================================================================//
            //                                                                                 Pathology Insert                                                                        // 
            //=========================================================================================================================================================================//

            if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") && model.clinicalexam.weight != 0 && model.clinicalexam.height != 0 && model.clinicalexam.clinicalComment != "")
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "General questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "General questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["Clinical_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Entry ignored - General questionnaire";
                model.pathology.active = false;
            }
            else if ((model.pathology.diastolicBP != null && model.pathology.systolicBP != null) || (model.pathology.diastolicBP != 0 && model.pathology.systolicBP != 0))
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.BPeffectiveDate = model.pathology.effectiveDate; //HCare-777
                model.pathology.systolicBP = decimal.Parse(Request.Query["pathology.systolicBP"], CultureInfo.InvariantCulture);
                model.pathology.diastolicBP = decimal.Parse(Request.Query["pathology.diastolicBP"], CultureInfo.InvariantCulture);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                if (model.pathology.systolicBP == 0 && model.pathology.diastolicBP == 0) //HCare-1063
                {
                    model.pathology.comment = "Entry ignored - General questionnaire";
                    model.pathology.active = false;
                }
                else
                {
                    model.pathology.comment = "General questionnaire";
                    model.pathology.active = true;
                }

            }
            else if (model.pathology.diastolicBP == 0 && model.pathology.systolicBP == 0)
            {
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.effectiveDate"]);
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "GEN";
                model.pathology.labName = "N/A";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Entry ignored - General questionnaire";
                model.pathology.diastolicBP = 0;
                model.pathology.systolicBP = 0;
                model.pathology.active = false;

            }

            var pathology = _member.InsertPathology(model.pathology);

            //=========================================================================================================================================================================//
            //                                                                            CoMorbid Conditions                                                                          // 
            //=========================================================================================================================================================================//

            model.comormidConditions = new List<CoMormidConditions>(); //HCare-607

            if ((Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true")) && (!String.IsNullOrEmpty(Request.Query["comorbids1"])))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["comorbids" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["comorbids" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "GEN";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);

                        if (!String.IsNullOrEmpty(Request.Query["comorbids" + i]))
                            comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        else
                            comorb.coMorbidId = 99998;

                        if (!String.IsNullOrEmpty(Request.Query["diabetes-comorbidEffectiveDate" + i]))
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["diabetes-comorbidEffectiveDate" + i]);
                        else
                            comorb.effectiveDate = null;

                        if (!String.IsNullOrEmpty(Request.Query["diabetes-comorbidEndDate" + i]))
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["diabetes-comorbidEndDate" + i]);
                        else
                            comorb.treatementEndDate = null;

                        if (String.IsNullOrEmpty(Request.Query["comorbids" + i]))
                            comorb.followUp = true;
                        else
                            comorb.followUp = false;

                        comorb.active = true;

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 99998,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    dependantID = new Guid(Request.Query["DependantID"]),
                    effectiveDate = null,
                    treatementEndDate = null,
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "GEN",
                    generalComments = (Request.Query["comormidCondition.generalComments"]),
                    followUp = true,
                    active = true,

                });
            }
            else if (Request.Query["CoMorb_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.comormidConditions.Add(new CoMormidConditions()
                {
                    coMorbidId = 0,
                    questionnaireID = new Guid(Request.Query["questionnaireID"]),
                    dependantID = new Guid(Request.Query["DependantID"]),
                    effectiveDate = null,
                    treatementEndDate = null,
                    createdDate = DateTime.Now,
                    createdBy = User.Identity.Name,
                    programType = "GEN",
                    generalComments = "Entry ignored - General questionnaire",
                    followUp = false,
                    active = false,

                });
            }
            else if (Request.Query["comorbid_CI-false"].ToString().ToLower().Contains("true"))
            {
                //model.comormidConditions.Add(new CoMormidConditions()
                //{
                //    coMorbidId = 99999,
                //    effectiveDate = DateTime.Now,
                //    treatementEndDate = null,
                //    dependantID = new Guid(Request.Query["DependantID"]),
                //    createdDate = DateTime.Now,
                //    createdBy = User.Identity.Name,
                //    programType = "GEN",
                //    generalComments = (Request.Query["comormidCondition.generalComments"]),
                //    followUp = false,
                //    active = true,

                //});
                if (!String.IsNullOrEmpty(Request.Query["comormidCondition.generalComments"]))
                {
                    model.comormidConditions.Add(new CoMormidConditions()
                    {
                        generalComments = (Request.Query["comormidCondition.generalComments"]),
                        questionnaireID = new Guid(Request.Query["questionnaireID"]),
                        programType = "General",
                        followUp = false,
                        active = true,
                    });
                }

            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["comorbids" + i]))
                    {
                        var comorb = new CoMormidConditions();
                        var cminformation = _member.GetCMByMappingCode(Request.Query["comorbids" + i]);
                        comorb.id = Convert.ToInt32(Request.Query["id" + i]);
                        comorb.coMorbidId = Convert.ToInt32(cminformation.id);
                        comorb.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        comorb.dependantID = new Guid(Request.Query["DependantID"]);
                        comorb.createdDate = DateTime.Now;
                        comorb.createdBy = User.Identity.Name;
                        comorb.programType = "GEN";
                        comorb.generalComments = (Request.Query["comormidCondition.generalComments"]);
                        comorb.active = true;

                        if (!String.IsNullOrEmpty(Request.Query["diabetes-comorbidEffectiveDate" + i]))
                        {
                            comorb.effectiveDate = Convert.ToDateTime(Request.Query["diabetes-comorbidEffectiveDate" + i]);
                        }
                        if (!String.IsNullOrEmpty(Request.Query["diabetes-comorbidEndDate" + i]))
                        {
                            comorb.treatementEndDate = Convert.ToDateTime(Request.Query["diabetes-comorbidEndDate" + i]);
                        }

                        model.comormidConditions.Add(comorb);
                    }
                    else
                        continue;
                }

            }

            foreach (var history in model.comormidConditions)
            {
                var coMorbHistory = _member.InsertComorbidCondition(history);
            }

            //=========================================================================================================================================================================//
            //                                                                             Medication History                                                                          // 
            //=========================================================================================================================================================================//

            //model.MedicationHistories = new List<MedicationHistory>();

            model.theScriptItems = new List<ScriptItems>();
            model.Scripts = new List<Scripts>();

            if (Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true") && Request.Query["ci_MedicationNaive_true"].ToString().ToLower().Contains("true"))
            {
                //-->>First add the ScriptID
                int scriptId = 0;
                var newScript = new Scripts();
                newScript.dependentID = new Guid(Request.Query["DependantID"]);
                newScript.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                newScript.createdDate = DateTime.Now;
                newScript.createdBy = User.Identity.Name;
                newScript.effectiveDate = DateTime.Now;
                newScript.repeats = 0;
                newScript.scriptType = "General";
                newScript.doctorID = null;
                newScript.practiceNo = null;
                newScript.Status = "TBA";
                newScript.active = true;
                scriptId = _member.InsertScript(newScript);

                //-->>Then add the medication to the scriptItems table using the scriptID
                for (int i = 0; i < 11; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalMedProduct" + i]))
                    {
                        var scriptItem = new ScriptItems();

                        scriptItem.scriptID = newScript.scriptID;
                        scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["generalMedProduct" + i].ToString()).nappiCode;
                        scriptItem.icd10code = Request.Query["programType" + i];
                        scriptItem.directions = Request.Query["generalMedNote" + i];
                        scriptItem.fromDate = Convert.ToDateTime(Request.Query["generalMedStartDate" + i]);
                        scriptItem.toDate = Convert.ToDateTime("2039-12-31");
                        scriptItem.quantity = "0";
                        scriptItem.itemRepeats = 0;
                        scriptItem.createdBy = User.Identity.Name;
                        scriptItem.createdDate = DateTime.Now;
                        scriptItem.modifiedBy = null;
                        scriptItem.active = true;
                        scriptItem.itemStatus = "Not yet authorised";
                        scriptItem.comment = "General questionnaire";

                        var insert = _member.InsertScriptItem(scriptItem);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true"))
            {
                for (int i = 0; i < 11; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalMedProduct1"]))
                    {
                        //-->>First add the ScriptID
                        int scriptId = 0;
                        var newScript = new Scripts();
                        newScript.dependentID = new Guid(Request.Query["DependantID"]);
                        newScript.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        newScript.createdDate = DateTime.Now;
                        newScript.createdBy = User.Identity.Name;
                        newScript.effectiveDate = DateTime.Now;
                        newScript.repeats = 0;
                        newScript.scriptType = "General";
                        newScript.doctorID = null;
                        newScript.practiceNo = null;
                        newScript.Status = "TBA";
                        newScript.active = true;
                        scriptId = _member.InsertScript(newScript);

                        //-->>Then add the medication to the scriptItems table using the scriptID
                        var scriptItem = new ScriptItems();

                        scriptItem.scriptID = newScript.scriptID;
                        scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["generalMedProduct" + i].ToString()).nappiCode;
                        scriptItem.icd10code = Request.Query["programType" + i];
                        scriptItem.fromDate = Convert.ToDateTime(Request.Query["generalMedStartDate" + i]);
                        scriptItem.toDate = Convert.ToDateTime("2039-12-31");
                        scriptItem.directions = "-";
                        scriptItem.quantity = "0";
                        scriptItem.itemRepeats = 0;
                        scriptItem.createdBy = User.Identity.Name;
                        scriptItem.createdDate = DateTime.Now;
                        scriptItem.modifiedBy = null;
                        scriptItem.active = true;
                        scriptItem.itemStatus = "Not yet authorised";
                        scriptItem.comment = "General questionnaire";

                        var insert = _member.InsertScriptItem(scriptItem);

                    }
                    else
                        continue;
                }

            }
            else if (Request.Query["ci_MedicationNaive_false"].ToString().ToLower().Contains("true"))
            {
                for (int i = 0; i < 11; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalMedProduct1"]))
                    {
                        //-->>First add the ScriptID
                        int scriptId = 0;
                        var newScript = new Scripts();
                        newScript.dependentID = new Guid(Request.Query["DependantID"]);
                        newScript.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        newScript.createdDate = DateTime.Now;
                        newScript.createdBy = User.Identity.Name;
                        newScript.effectiveDate = DateTime.Now;
                        newScript.repeats = 0;
                        newScript.scriptType = "General";
                        newScript.doctorID = null;
                        newScript.practiceNo = null;
                        newScript.Status = "TBA";
                        newScript.active = true;
                        scriptId = _member.InsertScript(newScript);

                        //-->>Then add the medication to the scriptItems table using the scriptID
                        var scriptItem = new ScriptItems();

                        scriptItem.scriptID = newScript.scriptID;
                        scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["generalMedProduct" + i].ToString()).nappiCode;
                        scriptItem.icd10code = Request.Query["programType" + i];
                        scriptItem.fromDate = Convert.ToDateTime(Request.Query["generalMedStartDate" + i]);
                        scriptItem.toDate = Convert.ToDateTime("2039-12-31");
                        scriptItem.directions = "-";
                        scriptItem.quantity = "0";
                        scriptItem.itemRepeats = 0;
                        scriptItem.createdBy = User.Identity.Name;
                        scriptItem.createdDate = DateTime.Now;
                        scriptItem.modifiedBy = null;
                        scriptItem.active = true;
                        scriptItem.itemStatus = "Not yet authorised";
                        scriptItem.comment = "General questionnaire";

                        var insert = _member.InsertScriptItem(scriptItem);

                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Medication_Confirmed"].ToString().ToLower().Contains("true")) //Entry should be ignored
            {
                for (int i = 0; i < 11; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalMedProduct1"]))
                    {
                        //-->>First add the ScriptID
                        int scriptId = 0;
                        var newScript = new Scripts();
                        newScript.dependentID = new Guid(Request.Query["DependantID"]);
                        newScript.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                        newScript.createdDate = DateTime.Now;
                        newScript.createdBy = User.Identity.Name;
                        newScript.effectiveDate = DateTime.Now;
                        newScript.repeats = 0;
                        newScript.scriptType = "General";
                        newScript.doctorID = null;
                        newScript.practiceNo = null;
                        newScript.Status = "TBA";
                        newScript.active = true;
                        scriptId = _member.InsertScript(newScript);

                        //-->>Then add the medication to the scriptItems table using the scriptID
                        var scriptItem = new ScriptItems();

                        scriptItem.scriptID = newScript.scriptID;
                        scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["generalMedProduct" + i].ToString()).nappiCode;
                        scriptItem.icd10code = Request.Query["programType" + i];
                        scriptItem.fromDate = Convert.ToDateTime(Request.Query["generalMedStartDate" + i]);
                        scriptItem.toDate = Convert.ToDateTime("2039-12-31");
                        scriptItem.directions = "-";
                        scriptItem.quantity = "0";
                        scriptItem.itemRepeats = 0;
                        scriptItem.createdBy = User.Identity.Name;
                        scriptItem.createdDate = DateTime.Now;
                        scriptItem.modifiedBy = null;
                        scriptItem.active = true;
                        scriptItem.itemStatus = "Not yet authorised";
                        scriptItem.comment = "General questionnaire";

                        var insert = _member.InsertScriptItem(scriptItem);

                    }
                    else
                        continue;
                }
            }
            else
            {
                //-->>First add the ScriptID
                int scriptId = 0;
                var newScript = new Scripts();
                newScript.dependentID = new Guid(Request.Query["DependantID"]);
                newScript.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                newScript.createdDate = DateTime.Now;
                newScript.createdBy = User.Identity.Name;
                newScript.effectiveDate = DateTime.Now;
                newScript.repeats = 0;
                newScript.scriptType = "General";
                newScript.doctorID = null;
                newScript.practiceNo = null;
                newScript.Status = "TBA";
                newScript.active = true;
                scriptId = _member.InsertScript(newScript);

                //-->>Then add the medication to the scriptItems table using the scriptID
                for (int i = 0; i < 11; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalMedProduct" + i]))
                    {
                        var scriptItem = new ScriptItems();

                        scriptItem.scriptID = newScript.scriptID;
                        scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["generalMedProduct" + i].ToString()).nappiCode;
                        scriptItem.icd10code = Request.Query["programType" + i];
                        scriptItem.directions = Request.Query["generalMedNote" + i];
                        scriptItem.fromDate = Convert.ToDateTime(Request.Query["generalMedStartDate" + i]);
                        scriptItem.toDate = Convert.ToDateTime("2039-12-31");
                        scriptItem.quantity = "0";
                        scriptItem.itemRepeats = 0;
                        scriptItem.createdBy = User.Identity.Name;
                        scriptItem.createdDate = DateTime.Now;
                        scriptItem.modifiedBy = null;
                        scriptItem.active = true;
                        scriptItem.itemStatus = "Not yet authorised";
                        scriptItem.comment = "General questionnaire";

                        var insert = _member.InsertScriptItem(scriptItem);

                    }
                    else
                        continue;
                }
            }

            //=========================================================================================================================================================================//
            //                                                                                  Allergy Insert                                                                         // 
            //=========================================================================================================================================================================//

            if (Request.Query["Allergy_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.allergy = new Allergies();

                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.Allergy = "Follow up";
                else
                    model.allergy.Allergy = Request.Query["allergy.Allergy"];
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "GEN";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);

                if (String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                    model.allergy.followUp = true;
                else
                    model.allergy.followUp = false;

                model.allergy.active = true;
            }
            else if (Request.Query["Allergy_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.Allergy = null;
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "GEN";
                model.allergy.generalComments = "Entry ignored - General questionnaire";
                model.allergy.followUp = false;
                model.allergy.active = false;

            }
            else if (Request.Query["allergies_ci-true"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.Allergy = Request.Query["allergy.Allergy"];
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "GEN";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                model.allergy.followUp = false;
                model.allergy.active = true;

            }
            else if (Request.Query["allergies_ci-false"].ToString().ToLower().Contains("true"))
            {
                model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                model.allergy.Allergy = "None";
                model.allergy.createdDate = DateTime.Now;
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.programType = "GEN";
                model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                model.allergy.followUp = false;
                model.allergy.active = false;

                //if (!String.IsNullOrEmpty(Request.Query["allergy.Allergy"]))
                //{
                //    model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
                //    model.allergy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                //    model.allergy.id = Convert.ToInt32(Request.Query["id"]);
                //    model.allergy.Allergy = Request.Query["allergy.Allergy"];
                //    model.allergy.createdDate = DateTime.Now;
                //    model.allergy.createdBy = User.Identity.Name;
                //    model.allergy.programType = "GEN";
                //    model.allergy.generalComments = (Request.Query["allergy.generalComments"]);
                //    model.allergy.followUp = false;
                //    model.allergy.active = true;

                //}

            }

            var allergies = _member.InsertAllergy(model.allergy);
            model.drquestionnaireResults.AllergyID = model.allergy.id;

            //=========================================================================================================================================================================//
            //                                                                               Social History                                                                            // 
            //=========================================================================================================================================================================//

            if (Request.Query["SocialHistory_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire = new ClinicalHistoryQuestionaire();

                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "GEN";

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                {
                    model.questionnaire.smoker = true;
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.drinker = true;
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                }
                else if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.NrDrinks = "0";
                }

                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                {
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                }

                if (String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]) || String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]) || String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                {
                    model.questionnaire.followUp = true;
                }
                else
                {
                    model.questionnaire.followUp = false;
                }

                model.questionnaire.active = true;
            }
            else if (Request.Query["SocialHistory_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire = new ClinicalHistoryQuestionaire();

                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.NoCigs = null;
                model.questionnaire.smokingYears = null;
                model.questionnaire.NrDrinks = null;
                model.questionnaire.programType = "GEN";
                model.questionnaire.socialComment = "Entry ignored - General questionnaire";
                model.questionnaire.followUp = false;
                model.questionnaire.active = false;

            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("true"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.programType = "GEN";

                //Insert socialHistory questionnaire results
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
                    model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = "0";
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdDate = DateTime.Now;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
                    model.questionnaire.createdBy = User.Identity.Name;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
                    model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))

                    model.questionnaire.followUp = false;
                model.questionnaire.active = true;

            }
            else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("false"))
            {
                model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaire.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
                model.questionnaire.createdDate = DateTime.Now;
                model.questionnaire.createdBy = User.Identity.Name;
                model.questionnaire.smoker = false;
                model.questionnaire.NoCigs = 0;
                model.questionnaire.smokingYears = 0;
                if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
                if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
                    model.questionnaire.NrDrinks = "0";
                model.questionnaire.programType = "GEN";
                model.questionnaire.socialComment = (Request.Query["questionnaire.socialComment"]);
                model.questionnaire.followUp = false;
                model.questionnaire.active = true;
            }

            var clinicalQuestion = _member.InsertClinicalHistoryQuestionnaire(model.questionnaire);
            model.drquestionnaireResults.SocialHistoryID = model.questionnaire.id;

            //=========================================================================================================================================================================//
            //                                                                              Hospitalisations                                                                           // 
            //=========================================================================================================================================================================//

            model.HospitalAuths = new List<HospitalizationAuths>();

            var memberinfo = _member.GetDependentDetails(new Guid(Request.Query["DependantID"]), null);

            if (Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true") && (!String.IsNullOrEmpty(Request.Query["generalHospitalisation1"])))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalHospitalisation" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();

                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;

                        if (!String.IsNullOrEmpty(Request.Query["generalHospitalisation" + i]))
                            hospitalisation.authType = Convert.ToString(Request.Query["generalHospitalisation" + i]);
                        else
                            hospitalisation.authType = "• Follow up";

                        hospitalisation.programType = "GEN";

                        if (!String.IsNullOrEmpty(Request.Query["HospitalAuth.generalComments"]))
                            hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);

                        if ((String.IsNullOrEmpty(Request.Query["generalHospitalisation" + i])) && (String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_gen" + i])))
                            hospitalisation.followUp = true;
                        else
                            hospitalisation.followUp = false;

                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_gen" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_gen" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }
            else if (Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    createdDate = DateTime.Now,
                    authType = "Follow up",
                    programType = "GEN",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = true,
                    Active = true,

                    questionnaireID = new Guid(Request.Query["questionnaireID"]),

                });
            }
            else if (Request.Query["Hospitalisation_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    createdDate = DateTime.Now,
                    authType = null,
                    programType = "GEN",
                    generalComments = "Entry ignored - General questionnaire",
                    followUp = false,
                    Active = false,

                    questionnaireID = new Guid(Request.Query["questionnaireID"]),

                });
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!String.IsNullOrEmpty(Request.Query["generalHospitalisation" + i]))
                    {
                        var hospitalisation = new HospitalizationAuths();
                        hospitalisation.authType = Convert.ToString(Request.Query["generalHospitalisation" + i]);
                        hospitalisation.createdDate = DateTime.Now;
                        hospitalisation.membershipNo = memberinfo.member.membershipNo;
                        hospitalisation.dependantCode = memberinfo.dependent.dependentCode;
                        hospitalisation.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;
                        hospitalisation.programType = "GEN";
                        hospitalisation.generalComments = (Request.Query["HospitalAuth.generalComments"]);
                        hospitalisation.followUp = false;
                        hospitalisation.Active = true;
                        hospitalisation.questionnaireID = new Guid(Request.Query["questionnaireID"]);

                        if (!String.IsNullOrEmpty(Request.Query["hospitalisationsAdminDate_gen" + i]))
                        {
                            hospitalisation.actualAdminDate = Convert.ToDateTime(Request.Query["hospitalisationsAdminDate_gen" + i]);
                        }

                        model.HospitalAuths.Add(hospitalisation);
                    }
                    else
                        continue;
                }
            }

            if (Request.Query["hospitalized-false"].ToString().ToLower().Contains("true"))
            {
                model.HospitalAuths.Add(new HospitalizationAuths()
                {
                    membershipNo = memberinfo.member.membershipNo,
                    dependantCode = memberinfo.dependent.dependentCode,
                    medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
                    createdDate = DateTime.Now,
                    authType = "None",
                    programType = "GEN",
                    generalComments = (Request.Query["HospitalAuth.generalComments"]),
                    followUp = false,
                    Active = false,

                    //questionnaireID = new Guid(Request.Query["questionnaireID"]),

                });
            }

            foreach (var history in model.HospitalAuths)
            {
                var hZHistory = _member.InsertHospitalizationAuths(history);
            }

            //=========================================================================================================================================================================//
            //                                                                                     Other                                                                               // 
            //=========================================================================================================================================================================//

            if (Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;

                #region occupation
                if (Request.Query["questionnaireOther.occupation"].ToString() != null)
                    model.questionnaireOther.occupation = Request.Query["questionnaireOther.occupation"].ToString();
                if (Request.Query["questionnaireOther.shiftWorker"].ToString() != null)
                    model.questionnaireOther.shiftWorker = Request.Query["questionnaireOther.shiftWorker"].ToString();
                #endregion
                #region drug-abuse
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.recDrugsLastUsed"].ToString()))
                {
                    model.questionnaireOther.recDrugs = true;
                    model.questionnaireOther.recDrugsLastUsed = Convert.ToDateTime(Request.Query["questionnaireOther.recDrugsLastUsed"]);
                }
                else
                {
                    model.questionnaireOther.recDrugs = false;
                }
                #endregion
                #region tb-screen
                //HCare-966
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.TBScreenDate"].ToString()))
                {
                    model.questionnaireOther.TBScreen = true;
                    model.questionnaireOther.TBScreenDate = Convert.ToDateTime(Request.Query["questionnaireOther.TBScreenDate"]);
                    model.questionnaireOther.TBScreenResult = Request.Query["questionnaireOther.TBScreenResult"];
                }
                else
                {
                    model.questionnaireOther.TBScreen = false;
                }

                if (Request.Query["questionnaireOther.TBScreenResult"].ToString().ToLower() == "positive") { model.questionnaireOther.tbDiagnosed = true; }
                else { model.questionnaireOther.tbDiagnosed = false; }

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentStartDate"].ToString())) { model.questionnaireOther.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentStartDate"]); }
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentEndDate"].ToString())) { model.questionnaireOther.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentEndDate"]); }
                #endregion
                #region pregnant
                //HCare-968
                if (Request.Query["pregnant-yes"].ToString().ToLower().Contains("true"))
                {
                    model.questionnaireOther.Pregnant = true;
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString())) { model.questionnaireOther.EDD = Convert.ToDateTime(Request.Query["questionnaireOther.EDD"]); }
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.TreatingDoctor"].ToString())) { model.questionnaireOther.TreatingDoctor = Request.Query["questionnaireOther.TreatingDoctor"]; }
                    if (Request.Query["aware-yes"].ToString().ToLower().Contains("true")) { model.questionnaireOther.isDoctorAware = true; }
                    else { model.questionnaireOther.isDoctorAware = false; }
                }
                else
                {
                    model.questionnaireOther.Pregnant = false;
                }
                #endregion
                #region frail-care
                //HCare-930
                if (Request.Query["questionnaireOther.frailCare"].ToString() != null)
                {
                    model.questionnaireOther.frailCare = Request.Query["questionnaireOther.frailCare"].ToString();
                    model.questionnaireOther.frailCareCheck = true;
                }
                else if (Request.Query["questionnaireOther.frailCare"].ToString() == null)
                {
                    model.questionnaireOther.frailCare = "• None";
                    model.questionnaireOther.frailCareCheck = false;
                }
                if (!String.IsNullOrEmpty(Request.Query["questionnaireOther.frailCareComment"].ToString()))
                {
                    model.questionnaireOther.frailCareComment = Request.Query["questionnaireOther.frailCareComment"];
                }
                else
                {
                    model.questionnaireOther.frailCareComment = null;
                }
                #endregion
                #region blood-test
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.bloodTestFrequency"].ToString())) { model.questionnaireOther.bloodTestFrequency = Request.Query["questionnaireOther.bloodTestFrequency"].ToString(); }
                else { model.questionnaireOther.bloodTestFrequency = null; }

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.bloodTestEffectiveDate"].ToString())) { model.questionnaireOther.bloodTestEffectiveDate = Convert.ToDateTime(Request.Query["questionnaireOther.bloodTestEffectiveDate"]); }
                else { model.questionnaireOther.bloodTestEffectiveDate = null; }
                #endregion
                #region follow-up
                model.questionnaireOther.followUp = true;
                #endregion
                #region comments
                if (Request.Query["questionnaireOther.generalComments"].ToString() != null)
                    model.questionnaireOther.generalComments = Request.Query["questionnaireOther.generalComments"].ToString();
                #endregion
                model.questionnaireOther.programType = "GEN";
                model.questionnaireOther.active = true;
            }
            else if (Request.Query["Other_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();

                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.Pregnant = false; //HCare-968
                model.questionnaireOther.TreatingDoctor = null; //HCare-968
                model.questionnaireOther.isDoctorAware = false; //HCare-968
                model.questionnaireOther.hasKidneyInfection = false;
                model.questionnaireOther.frailCareCheck = false; //HCare-930
                model.questionnaireOther.frailCare = "• None"; //HCare-930
                model.questionnaireOther.frailCareComment = null; //HCare-930
                model.questionnaireOther.generalComments = "Entry ignored - General questionnaire";
                model.questionnaireOther.programType = "GEN";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = false;

            }
            else
            {
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.createdDate = DateTime.Now;
                #region occupation
                if (Request.Query["questionnaireOther.occupation"].ToString() != null)
                    model.questionnaireOther.occupation = Request.Query["questionnaireOther.occupation"].ToString();
                if (Request.Query["questionnaireOther.shiftWorker"].ToString() != null)
                    model.questionnaireOther.shiftWorker = Request.Query["questionnaireOther.shiftWorker"].ToString();
                #endregion
                #region drug-abuse
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.recDrugsLastUsed"].ToString()))
                {
                    model.questionnaireOther.recDrugs = true;
                    model.questionnaireOther.recDrugsLastUsed = Convert.ToDateTime(Request.Query["questionnaireOther.recDrugsLastUsed"]);
                }
                else
                {
                    model.questionnaireOther.recDrugs = false;
                }
                #endregion
                #region tb-screen
                //HCare-966
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.TBScreenDate"].ToString()))
                {
                    model.questionnaireOther.TBScreen = true;
                    model.questionnaireOther.TBScreenDate = Convert.ToDateTime(Request.Query["questionnaireOther.TBScreenDate"]);
                    model.questionnaireOther.TBScreenResult = Request.Query["questionnaireOther.TBScreenResult"];
                }
                else
                {
                    model.questionnaireOther.TBScreen = false;
                }

                if (Request.Query["questionnaireOther.TBScreenResult"].ToString().ToLower() == "positive") { model.questionnaireOther.tbDiagnosed = true; }
                else { model.questionnaireOther.tbDiagnosed = false; }

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentStartDate"].ToString())) { model.questionnaireOther.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentStartDate"]); }
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.tbTreatmentEndDate"].ToString())) { model.questionnaireOther.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentEndDate"]); }
                #endregion
                #region pregnant
                //HCare-968
                if (Request.Query["pregnant-yes"].ToString().ToLower().Contains("true"))
                {
                    model.questionnaireOther.Pregnant = true;
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.EDD"].ToString())) { model.questionnaireOther.EDD = Convert.ToDateTime(Request.Query["questionnaireOther.EDD"]); }
                    if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.TreatingDoctor"].ToString())) { model.questionnaireOther.TreatingDoctor = Request.Query["questionnaireOther.TreatingDoctor"]; }
                    if (Request.Query["aware-yes"].ToString().ToLower().Contains("true")) { model.questionnaireOther.isDoctorAware = true; }
                    else { model.questionnaireOther.isDoctorAware = false; }
                }
                else
                {
                    model.questionnaireOther.Pregnant = false;
                }
                #endregion
                #region frail-care
                //HCare-930
                if (Request.Query["questionnaireOther.frailCare"].ToString() != null)
                {
                    model.questionnaireOther.frailCare = Request.Query["questionnaireOther.frailCare"].ToString();
                    model.questionnaireOther.frailCareCheck = true;
                }
                else if (Request.Query["questionnaireOther.frailCare"].ToString() == null)
                {
                    model.questionnaireOther.frailCare = "• None";
                    model.questionnaireOther.frailCareCheck = false;
                }
                if (!String.IsNullOrEmpty(Request.Query["questionnaireOther.frailCareComment"].ToString()))
                {
                    model.questionnaireOther.frailCareComment = Request.Query["questionnaireOther.frailCareComment"];
                }
                else
                {
                    model.questionnaireOther.frailCareComment = null;
                }
                #endregion
                #region blood-test
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.bloodTestFrequency"].ToString())) { model.questionnaireOther.bloodTestFrequency = Request.Query["questionnaireOther.bloodTestFrequency"].ToString(); }
                else { model.questionnaireOther.bloodTestFrequency = null; }

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.bloodTestEffectiveDate"].ToString())) { model.questionnaireOther.bloodTestEffectiveDate = Convert.ToDateTime(Request.Query["questionnaireOther.bloodTestEffectiveDate"]); }
                else { model.questionnaireOther.bloodTestEffectiveDate = null; }
                #endregion
                #region follow-up
                if ((String.IsNullOrEmpty(Request.Query["questionnaireOther.occupation"])) || (String.IsNullOrEmpty(Request.Query["questionnaireOther.shiftWorker"])) || (String.IsNullOrEmpty(Request.Query["questionnaireOther.lypohypertrophy"])) || (String.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"])))
                {
                    model.questionnaireOther.followUp = true;
                }
                else
                {
                    model.questionnaireOther.followUp = false;
                }
                #endregion
                #region comments
                if (Request.Query["questionnaireOther.generalComments"].ToString() != null)
                    model.questionnaireOther.generalComments = Request.Query["questionnaireOther.generalComments"].ToString();
                #endregion
                model.questionnaireOther.programType = "GEN";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = true;

            }

            var questionOtherResults = _member.InsertQuestionnaireOtherResults(model.questionnaireOther);
            model.drquestionnaireResults.QuestionnaireOtherID = model.questionnaireOther.QuestionnaireOtherID;

            //=========================================================================================================================================================================//
            //                                                                              DrQuestionnaire INSERT                                                                     // 
            //=========================================================================================================================================================================//

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);


            //=========================================================================================================================================================================//
            //                                                                                    Task Update                                                                          // 
            //=========================================================================================================================================================================//

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            //check for follow ups to create new Assignment

            if (
                Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Medication_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Allergy_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["SocialHistory_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Hospitalisation_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true")

            )
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "General questionnaire follow up",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };
                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in taskss)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        //================================================================================== Questionnaire_HIV ==================================================================================//

        public ActionResult _HIVQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            ViewBag.programId = programCode;

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;

            var model = new DoctorQuestionnaireViewModel();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.dependantCode = dependantCode;
            ViewBag.membershipNo = membershipNo;

            model.ScriptItems = _member.GetHIVScriptItems(DependentID);
            model.dependent = _member.GetDependantByDependantID(DependentID);
            model.member = _member.GetMemberByDependantID(DependentID);
            model.contact = _member.GetContact(DependentID);

            model.programDiagnoses = _member.GetHIVDiagnosisResults(DependentID);

            model.otherMedicalHistories = new List<OtherMedicalHistory>();
            model.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);

            model.questionnaireOthers = new List<QuestionnaireOther>();
            model.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);

            model.hivQuestionnaireOthers = new List<QuestionnaireOther>();
            model.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);

            model.pathologies = new List<Pathology>();
            model.pathologies = _member.GetPathology(DependentID);
            model.hivPathologies = new List<Pathology>();
            model.hivPathologies = _member.GetHIVPathology(DependentID);

            var memberinfo = _member.GetDependentDetails(DependentID, null);

            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");

            return View(model);
        }

        [HttpPost]
        public ActionResult _HIVQuestionnaire(DoctorQuestionnaireViewModel model)
        {
            var dependantID = new Guid(Request.Query["DependantID"]);
            var dependantInfo = _member.GetDependantByDependantID(dependantID);

            //=========================================================================================================================================================================//
            //                                                                        DoctorQuestionnaireResults                                                                       // 
            //=========================================================================================================================================================================//

            model.drquestionnaireResults = new DoctorQuestionnaireResults();

            model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
            model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.Active = true;

            //=========================================================================================================================================================================//
            //                                                                               DIAGNOSIS/DIAGNOSE                                                                        // 
            //=========================================================================================================================================================================//

            if (model.PatientDiagnosis.EffectiveDate != null)
            {
                model.PatientDiagnosis = new PatientDiagnosis();
                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.ProgramCode = "HIVPR";
                model.PatientDiagnosis.ProgramDescription = "HIV";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.EffectiveDate = Convert.ToDateTime(Request.Query["PatientDiagnosis.EffectiveDate"]);
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment= (Request.Query["PatientDiagnosis.generalComments"]);
                model.PatientDiagnosis.FollowUp = false;
                model.PatientDiagnosis.Active = true;
                model.PatientDiagnosis.Medication = Request.Query["medication-registration"]; //hcare-863
                model.PatientDiagnosis.EffectiveDate = new DateTime(model.PatientDiagnosis.EffectiveDate.Value.Year, model.PatientDiagnosis.EffectiveDate.Value.Month, model.PatientDiagnosis.EffectiveDate.Value.Day, model.PatientDiagnosis.CreatedDate.Hour, model.PatientDiagnosis.CreatedDate.Minute, model.PatientDiagnosis.CreatedDate.Second); //hcare-863

                var program = _member.GetPrograms().Where(x => x.code == model.PatientDiagnosis.ProgramCode).FirstOrDefault(); //hcare-863
                model.PatientDiagnosis.ICD10Code = program.icd10code; //hcare-863
            }

            var diagnosisDate = _member.InsertDiagnosisResults(model.PatientDiagnosis);

            //=========================================================================================================================================================================//
            //                                                                OTHER MEDICAL HISTORY/ANDER MEDIESE GESKIEDENIS                                                          // 
            //=========================================================================================================================================================================//

            if (Request.Query["OtherMedicalHistory_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.otherMedicalHistory = new OtherMedicalHistory();
                model.otherMedicalHistory.dependentID = new Guid(Request.Query["DependantID"]);
                model.otherMedicalHistory.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.otherMedicalHistory.Id = Convert.ToInt32(Request.Query["id"]);
                model.otherMedicalHistory.createdBy = User.Identity.Name;
                model.otherMedicalHistory.createdDate = DateTime.Now;

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.respiratoryTractEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.respiratoryTract = true;
                    model.otherMedicalHistory.respiratoryTractEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.respiratoryTractEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.respiratoryTractComment"].ToString()))
                {
                    model.otherMedicalHistory.respiratoryTractComment = Request.Query["otherMedicalHistory.respiratoryTractComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.pneumoniaEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.pneumonia = true;
                    model.otherMedicalHistory.pneumoniaEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.pneumoniaEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.pneumoniaComment"].ToString()))
                {
                    model.otherMedicalHistory.pneumoniaComment = Request.Query["otherMedicalHistory.pneumoniaComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.meningitisEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.meningitis = true;
                    model.otherMedicalHistory.meningitisEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.meningitisEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.meningitisComment"].ToString()))
                {
                    model.otherMedicalHistory.meningitisComment = Request.Query["otherMedicalHistory.meningitisComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.lymphadenopathyEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.lymphadenopathy = true;
                    model.otherMedicalHistory.lymphadenopathyEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.lymphadenopathyEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.lymphadenopathyComment"].ToString()))
                {
                    model.otherMedicalHistory.lymphadenopathyComment = Request.Query["otherMedicalHistory.lymphadenopathyComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.diarrhoeaEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.diarrhoea = true;
                    model.otherMedicalHistory.diarrhoeaEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.diarrhoeaEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.diarrhoeaComment"].ToString()))
                {
                    model.otherMedicalHistory.diarrhoeaComment = Request.Query["otherMedicalHistory.diarrhoeaComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.bladderInfectionEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.bladderInfection = true;
                    model.otherMedicalHistory.bladderInfectionEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.bladderInfectionEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.bladderInfectionComment"].ToString()))
                {
                    model.otherMedicalHistory.bladderInfectionComment = Request.Query["otherMedicalHistory.bladderInfectionComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.weightLossEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.weightLoss = true;
                    model.otherMedicalHistory.weightLossEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.weightLossEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.weightLossComment"].ToString()))
                {
                    model.otherMedicalHistory.weightLossComment = Request.Query["otherMedicalHistory.weightLossComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cancerEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.cancer = true;
                    model.otherMedicalHistory.cancerEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.cancerEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cancerComment"].ToString()))
                {
                    model.otherMedicalHistory.cancerComment = Request.Query["otherMedicalHistory.cancerComment"];
                }

                if (dependantInfo.sex == "F" || dependantInfo.sex == null)
                {
                    if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cervicalCancerEffectiveDate"].ToString()))
                    {
                        model.otherMedicalHistory.cervicalCancer = true;
                        model.otherMedicalHistory.cervicalCancerEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.cervicalCancerEffectiveDate"]);
                    }
                    if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cervicalCancerComment"].ToString()))
                    {
                        model.otherMedicalHistory.cervicalCancerComment = Request.Query["otherMedicalHistory.cervicalCancerComment"];
                    }
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.immunizationEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.immunization = true;
                    model.otherMedicalHistory.immunizationEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.immunizationEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.immunizationComment"].ToString()))
                {
                    model.otherMedicalHistory.immunizationComment = Request.Query["otherMedicalHistory.immunizationComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.generalComments"].ToString()))
                {
                    model.otherMedicalHistory.generalComments = Request.Query["otherMedicalHistory.generalComments"];
                }

                model.otherMedicalHistory.programType = "HIVPR";
                model.otherMedicalHistory.followUp = true;
                model.otherMedicalHistory.active = true;
            }
            else if (Request.Query["OtherMedicalHistory_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.otherMedicalHistory = new OtherMedicalHistory();

                model.otherMedicalHistory.dependentID = new Guid(Request.Query["DependantID"]);
                model.otherMedicalHistory.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.otherMedicalHistory.Id = Convert.ToInt32(Request.Query["id"]);
                model.otherMedicalHistory.createdBy = User.Identity.Name;
                model.otherMedicalHistory.createdDate = DateTime.Now;
                model.otherMedicalHistory.generalComments = "Entry ignored - HIV questionnaire";
                model.otherMedicalHistory.programType = "HIVPR";
                model.otherMedicalHistory.respiratoryTract = false;
                model.otherMedicalHistory.pneumonia = false;
                model.otherMedicalHistory.meningitis = false;
                model.otherMedicalHistory.lymphadenopathy = false;
                model.otherMedicalHistory.diarrhoea = false;
                model.otherMedicalHistory.bladderInfection = false;
                model.otherMedicalHistory.weightLoss = false;
                model.otherMedicalHistory.cancer = false;
                model.otherMedicalHistory.cervicalCancer = false;
                model.otherMedicalHistory.immunization = false;

                model.otherMedicalHistory.followUp = false;
                model.otherMedicalHistory.active = false;

            }
            else
            {
                model.otherMedicalHistory = new OtherMedicalHistory();
                model.otherMedicalHistory.dependentID = new Guid(Request.Query["DependantID"]);
                model.otherMedicalHistory.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.otherMedicalHistory.Id = Convert.ToInt32(Request.Query["id"]);
                model.otherMedicalHistory.createdBy = User.Identity.Name;
                model.otherMedicalHistory.createdDate = DateTime.Now;

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.respiratoryTractEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.respiratoryTract = true;
                    model.otherMedicalHistory.respiratoryTractEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.respiratoryTractEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.respiratoryTractComment"].ToString()))
                {
                    model.otherMedicalHistory.respiratoryTractComment = Request.Query["otherMedicalHistory.respiratoryTractComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.pneumoniaEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.pneumonia = true;
                    model.otherMedicalHistory.pneumoniaEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.pneumoniaEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.pneumoniaComment"].ToString()))
                {
                    model.otherMedicalHistory.pneumoniaComment = Request.Query["otherMedicalHistory.pneumoniaComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.meningitisEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.meningitis = true;
                    model.otherMedicalHistory.meningitisEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.meningitisEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.meningitisComment"].ToString()))
                {
                    model.otherMedicalHistory.meningitisComment = Request.Query["otherMedicalHistory.meningitisComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.lymphadenopathyEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.lymphadenopathy = true;
                    model.otherMedicalHistory.lymphadenopathyEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.lymphadenopathyEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.lymphadenopathyComment"].ToString()))
                {
                    model.otherMedicalHistory.lymphadenopathyComment = Request.Query["otherMedicalHistory.lymphadenopathyComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.diarrhoeaEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.diarrhoea = true;
                    model.otherMedicalHistory.diarrhoeaEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.diarrhoeaEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.diarrhoeaComment"].ToString()))
                {
                    model.otherMedicalHistory.diarrhoeaComment = Request.Query["otherMedicalHistory.diarrhoeaComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.bladderInfectionEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.bladderInfection = true;
                    model.otherMedicalHistory.bladderInfectionEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.bladderInfectionEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.bladderInfectionComment"].ToString()))
                {
                    model.otherMedicalHistory.bladderInfectionComment = Request.Query["otherMedicalHistory.bladderInfectionComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.weightLossEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.weightLoss = true;
                    model.otherMedicalHistory.weightLossEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.weightLossEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.weightLossComment"].ToString()))
                {
                    model.otherMedicalHistory.weightLossComment = Request.Query["otherMedicalHistory.weightLossComment"];
                }

                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cancerEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.cancer = true;
                    model.otherMedicalHistory.cancerEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.cancerEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cancerComment"].ToString()))
                {
                    model.otherMedicalHistory.cancerComment = Request.Query["otherMedicalHistory.cancerComment"];
                }

                if (dependantInfo.sex == "F" || dependantInfo.sex == null)
                {
                    if (!String.IsNullOrEmpty(Request.Query["otherMedicalHistory.cervicalCancerEffectiveDate"].ToString()))
                    {
                        model.otherMedicalHistory.cervicalCancer = true;
                        model.otherMedicalHistory.cervicalCancerEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.cervicalCancerEffectiveDate"]);
                    }
                    if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.cervicalCancerComment"].ToString()))
                    {
                        model.otherMedicalHistory.cervicalCancerComment = Request.Query["otherMedicalHistory.cervicalCancerComment"];
                    }
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.immunizationEffectiveDate"].ToString()))
                {
                    model.otherMedicalHistory.immunization = true;
                    model.otherMedicalHistory.immunizationEffectiveDate = Convert.ToDateTime(Request.Query["otherMedicalHistory.immunizationEffectiveDate"]);
                }
                if (!string.IsNullOrEmpty(Request.Query["otherMedicalHistory.immunizationComment"].ToString()))
                {
                    model.otherMedicalHistory.immunizationComment = Request.Query["otherMedicalHistory.immunizationComment"];
                }

                model.otherMedicalHistory.generalComments = Request.Query["otherMedicalHistory.generalComments"];
                model.otherMedicalHistory.programType = "HIVPR";

                model.otherMedicalHistory.followUp = false;
                model.otherMedicalHistory.active = true;

            }

            var OtherMedicalHistoryResults = _member.InsertOtherMedicalHistory(model.otherMedicalHistory);

            //=========================================================================================================================================================================//
            //                                                                 MONITORING BLOOD TESTS / MONTERING BLOEDTOETSE                                                          // 
            //=========================================================================================================================================================================//

            if (Request.Query["hivClinical_FollowUp"].ToString().ToLower().Contains("true") && model.pathology.CD4Count != 0 && model.pathology.viralLoad != 0 && model.pathology.CD4Percentage != 0)
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;

                //HCare-1050
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Count"]) || Request.Query["pathology.CD4Count"] != "0")
                {
                    model.pathology.CD4Count = decimal.Parse(Request.Query["pathology.CD4Count"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4CounteffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.viralLoad"]) || Request.Query["pathology.viralLoad"] != "0")
                {
                    model.pathology.viralLoad = decimal.Parse(Request.Query["pathology.viralLoad"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.viralLoadeffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Percentage"]) || Request.Query["pathology.CD4Percentage"] != "0")
                {
                    model.pathology.CD4Percentage = decimal.Parse(Request.Query["pathology.CD4Percentage"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4PercentageeffectiveDate"]);
                }
                model.pathology.pathologyType = "HIVPR";
                model.pathology.labName = "HIV Questionnaire";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "HIV Questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["hivClinical_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.createdDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Count"]) || Request.Query["pathology.CD4Count"] != "0")
                {
                    model.pathology.CD4Count = decimal.Parse(Request.Query["pathology.CD4Count"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4CounteffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.viralLoad"]) || Request.Query["pathology.viralLoad"] != "0")
                {
                    model.pathology.viralLoad = decimal.Parse(Request.Query["pathology.viralLoad"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.viralLoadeffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Percentage"]) || Request.Query["pathology.CD4Percentage"] != "0")
                {
                    model.pathology.CD4Percentage = decimal.Parse(Request.Query["pathology.CD4Percentage"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4PercentageeffectiveDate"]);
                }
                model.pathology.pathologyType = "HIVPR";
                model.pathology.labName = "HIV Questionnaire";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "HIV Questionnaire follow-up";
                model.pathology.active = true;
            }
            else if (Request.Query["hivClinical_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.effectiveDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;
                model.pathology.pathologyType = "HIVPR";
                model.pathology.labName = "HIV Questionnaire";
                model.pathology.labReferenceNo = Request.Query["taskId"];
                model.pathology.comment = "Entry ignored - HIV questionnaire";
                model.pathology.active = false;
            }
            else
            {
                model.pathology.dependentID = new Guid(Request.Query["DependantID"]);
                model.pathology.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.pathology.createdDate = DateTime.Now;
                model.pathology.createdBy = User.Identity.Name;
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Count"]) || Request.Query["pathology.CD4Count"] != "0")
                {
                    model.pathology.CD4Count = decimal.Parse(Request.Query["pathology.CD4Count"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4CounteffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.viralLoad"]) || Request.Query["pathology.viralLoad"] != "0")
                {
                    model.pathology.viralLoad = decimal.Parse(Request.Query["pathology.viralLoad"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.viralLoadeffectiveDate"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["pathology.CD4Percentage"]) || Request.Query["pathology.CD4Percentage"] != "0")
                {
                    model.pathology.CD4Percentage = decimal.Parse(Request.Query["pathology.CD4Percentage"], CultureInfo.InvariantCulture);
                    model.pathology.effectiveDate = Convert.ToDateTime(Request.Query["pathology.CD4PercentageeffectiveDate"]);
                }
                model.pathology.pathologyType = "HIVPR";
                model.pathology.labName = "HIV Questionnaire";
                model.pathology.labReferenceNo = Request.Query["taskId"];

                if (model.pathology.CD4Count == 0 && model.pathology.viralLoad == 0 && model.pathology.CD4Percentage == 0)
                {
                    model.pathology.comment = "Entry ignored - HIV questionnaire";
                    model.pathology.active = false;
                }
                else
                {
                    model.pathology.comment = (Request.Query["pathology.comment"]);
                    model.pathology.active = true;
                }

            }

            var pathology = _member.InsertPathology(model.pathology);

            //=========================================================================================================================================================================//
            //                                                                               GENERAL/ALGEMEEN                                                                          // 
            //=========================================================================================================================================================================//

            if (Request.Query["hivOther_FollowUp"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.generalComments = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                {
                    model.questionnaireOther.depression = true;
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();
                }
                else { model.questionnaireOther.depression = false; }
                model.questionnaireOther.programType = "HIVPR";
                if (String.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"])) { model.questionnaireOther.followUp = true; }
                else { model.questionnaireOther.followUp = false; }
                model.questionnaireOther.active = true;
            }
            else if (Request.Query["hivOther_Confirmed"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();

                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;
                model.questionnaireOther.depression = false;
                model.questionnaireOther.depressionComment = null;
                model.questionnaireOther.generalComments = "Entry ignored";
                model.questionnaireOther.programType = "HIVPR";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = false;

            }
            else
            {
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.generalComments = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;

                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                {
                    model.questionnaireOther.depression = true;
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();
                }
                else
                {
                    model.questionnaireOther.depression = false;
                }

                model.questionnaireOther.programType = "HIVPR";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = true;

            }

            var questionOtherResults = _member.InsertQuestionnaireOtherResults(model.questionnaireOther);
            model.drquestionnaireResults.QuestionnaireOtherID = model.questionnaireOther.QuestionnaireOtherID;

            //=========================================================================================================================================================================//
            //                                                                              DrQuestionnaire INSERT                                                                     // 
            //=========================================================================================================================================================================//

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);

            //=========================================================================================================================================================================//
            //                                                                                    Task Update                                                                          // 
            //=========================================================================================================================================================================//

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            //check for follow ups to create new Assignment

            if (
                Request.Query["OtherMedicalHistory_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["hivClinical_FollowUp"].ToString().ToLower().Contains("true") ||
                Request.Query["hivOther_FollowUp"].ToString().ToLower().Contains("true")
            )
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "HIV questionnaire follow up",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in taskss)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        //================================================================================== Questionnaire_Diabetes ==================================================================================//

        public ActionResult _newDiabetesQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode) //HCare-832
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.programID).FirstOrDefault();
            ViewBag.programId = programCode;

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;

            var model = new DoctorQuestionnaireViewModel();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.dependantCode = dependantCode;
            ViewBag.membershipNo = membershipNo;
            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");

            model.programDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
            model.ScriptItems = _member.GetDiabetesScriptItems(DependentID);

            model.autoNeropathies = new List<AutoNeropathy>();
            model.autoNeropathies = _member.GetAutoNeroResults(DependentID);

            model.visions = new List<Vision>();
            model.visions = _member.GetVisionResults(DependentID);

            model.podiatries = new List<Podiatry>();
            model.podiatries = _member.GetPodiatryResults(DependentID);

            model.hypoglycaemias = new List<Hypoglycaemia>();
            model.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);

            model.diabQuestionnaireOthers = new List<QuestionnaireOther>();
            model.diabQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);

            return View(model);
        }

        [HttpPost]
        public ActionResult _newDiabetesQuestionnaire(DoctorQuestionnaireViewModel model) //HCare-832
        {
            var dependantID = new Guid(Request.Query["DependantID"]);
            var dependantInfo = _member.GetDependantByDependantID(dependantID);

            //====================================================================== doctor-questionnaire-results =======================================================================//

            model.drquestionnaireResults = new DoctorQuestionnaireResults();

            model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
            model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.Active = true;

            //================================================================================ diagnosis ================================================================================//

            if (model.PatientDiagnosis.EffectiveDate != null)
            {
                model.PatientDiagnosis = new PatientDiagnosis();
                model.PatientDiagnosis.DependantID = new Guid(Request.Query["DependantID"]);
                model.PatientDiagnosis.QuestionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.PatientDiagnosis.Id = Convert.ToInt32(Request.Query["id"]);
                model.PatientDiagnosis.ProgramCode = "DIABD";
                model.PatientDiagnosis.ProgramDescription = "Diabetes";
                model.PatientDiagnosis.CreatedDate = DateTime.Now;
                model.PatientDiagnosis.EffectiveDate = Convert.ToDateTime(Request.Query["PatientDiagnosis.EffectiveDate"]);
                model.PatientDiagnosis.CreatedBy = User.Identity.Name;
                model.PatientDiagnosis.Comment= (Request.Query["PatientDiagnosis.generalComments"]);
                model.PatientDiagnosis.FollowUp = false;
                model.PatientDiagnosis.Active = true;
                model.PatientDiagnosis.Medication = Request.Query["medication-registration"]; //hcare-863
                model.PatientDiagnosis.EffectiveDate = new DateTime(model.PatientDiagnosis.EffectiveDate.Value.Year, model.PatientDiagnosis.EffectiveDate.Value.Month, model.PatientDiagnosis.EffectiveDate.Value.Day, model.PatientDiagnosis.CreatedDate.Hour, model.PatientDiagnosis.CreatedDate.Minute, model.PatientDiagnosis.CreatedDate.Second); //hcare-863

                var program = _member.GetPrograms().Where(x => x.code == model.PatientDiagnosis.ProgramCode).FirstOrDefault(); //hcare-863
                model.PatientDiagnosis.ICD10Code = program.icd10code; //hcare-863
            }

            var diagnosisDate = _member.InsertDiagnosisResults(model.PatientDiagnosis);

            //================================================================================ vision ================================================================================//

            if (Request.Query["d-optometry-followup"].ToString().ToLower().Contains("true"))
            {
                model.vision = new Vision();

                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;

                if (Request.Query["vision.vision"].ToString() != null)
                    model.vision.vision = Request.Query["vision.vision"].ToString();
                if (Request.Query["vision.vision"] == "")
                    model.vision.vision = "• Follow up";

                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                if (Request.Query["vision.eyeTreatment"].ToString() == null)
                    model.vision.eyeTreatment = "• Follow up";

                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                if (Request.Query["vision.generalComments"].ToString() == null)
                    model.vision.generalComments = null;

                model.vision.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["vision.vision"]) || String.IsNullOrEmpty(Request.Query["vision.eyeTreatment"]))
                {
                    model.vision.followUp = true;
                }
                else
                {
                    model.vision.followUp = false;
                }

                model.vision.active = true;

            }
            else if (Request.Query["d-optometry-ignore"].ToString().ToLower().Contains("true"))
            {
                model.vision = new Vision();

                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                model.vision.vision = null;
                model.vision.eyeTreatment = null;
                model.vision.generalComments = "Entry ignored";
                model.vision.programType = "DIABD";
                model.vision.followUp = false;
                model.vision.active = false;

            }
            else
            {
                model.vision.dependentID = new Guid(Request.Query["DependantID"]);
                model.vision.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
                model.vision.createdBy = User.Identity.Name;
                model.vision.createdDate = DateTime.Now;
                if (Request.Query["vision.vision"].ToString() != null)
                {
                    model.vision.vision = Request.Query["vision.vision"].ToString();
                    model.vision.visionCheck = true;
                }
                if (Request.Query["vision.eyeTreatment"].ToString() != null)
                {
                    model.vision.eyeTreatmentCheck = true;
                    model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
                }
                if (Request.Query["vision.generalComments"].ToString() != null)
                    model.vision.generalComments = Request.Query["vision.generalComments"].ToString();
                model.vision.programType = "DIABD";
                model.vision.followUp = false;
                model.vision.active = true;

            }

            var visionResults = _member.InsertVisionResults(model.vision);
            model.drquestionnaireResults.VisionID = model.vision.VisionID;

            //================================================================================ podiatry ================================================================================//

            if (Request.Query["d-podiatry-followup"].ToString().ToLower().Contains("true"))
            {
                model.podiatry = new Podiatry();

                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                else if (Request.Query["podiatry.amputationComment"] == "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = false;
                }
                else if (Request.Query["podiatry.amputationComment"].ToString() == null)
                {
                    model.podiatry.amputationComment = "• Follow up";
                    model.podiatry.amputationCheck = false;
                }

                if (Request.Query["podiatry.amputationReason"].ToString() != null)
                    model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();
                if (Request.Query["podiatry.amputationReason"].ToString() == null)
                    model.podiatry.amputationReason = "• Follow up";

                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
                if (Request.Query["podiatry.podiatryLopsComment"].ToString() == null)
                    model.podiatry.podiatryLopsComment = "• Follow up";

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() == null)
                    model.podiatry.podiatryDeformityComment = "• Follow up";

                if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"].ToString() == null)
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "• Follow up";

                if (Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString() != null)
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                if (Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString() == null)
                    model.podiatry.podiatryWoundAffectedLeg = "• Follow up";

                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                    model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
                if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() == null)
                    model.podiatry.podiatryPerArterialDiseaseComment = "• Follow up";

                if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();

                if (Request.Query["podiatry.WoundInfection"].ToString() != null)
                    model.podiatry.WoundInfection = Request.Query["podiatry.WoundInfection"].ToString();

                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";

                if (String.IsNullOrEmpty(Request.Query["podiatry.amputationComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.amputationReason"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryLopsComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryDeformityComment"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryPerArterialDiseaseComment"]) || String.IsNullOrEmpty(Request.Query["podiatry.podiatryPerArterialDiseaseAffectedLeg"]) ||
                    String.IsNullOrEmpty(Request.Query["podiatry.podiatryWoundAffectedLeg"]))
                {
                    model.podiatry.followUp = true;
                }
                else
                {
                    model.podiatry.followUp = false;
                }

                model.podiatry.active = true;
            }
            else if (Request.Query["d-podiatry-ignore"].ToString().ToLower().Contains("true"))
            {
                model.podiatry = new Podiatry();

                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                model.podiatry.amputationCheck = false; //HCare-759
                model.podiatry.amputationComment = null;
                model.podiatry.amputationReason = null;
                model.podiatry.podiatryLopsComment = null;
                model.podiatry.podiatryDeformityComment = null;
                model.podiatry.podiatryPerArterialDiseaseComment = null;
                model.podiatry.podiatryWoundComment = null;
                model.podiatry.podiatryPerArterialDiseaseAffectedLeg = null;
                model.podiatry.podiatryWoundAffectedLeg = null;
                model.podiatry.generalComments = "Entry ignored";

                model.podiatry.programType = "DIABD";
                model.podiatry.followUp = false;
                model.podiatry.active = false;
            }
            else
            {
                model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
                model.podiatry.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.podiatry.createdDate = DateTime.Now;
                model.podiatry.createdBy = User.Identity.Name;

                if (Request.Query["podiatry.amputationComment"].ToString() != null && Request.Query["podiatry.amputationComment"] != "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = true;
                }
                else if (Request.Query["podiatry.amputationComment"] == "• None/Geen")
                {
                    model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
                    model.podiatry.amputationCheck = false;
                }
                else if (Request.Query["podiatry.amputationComment"].ToString() == null)
                {
                    model.podiatry.amputationComment = "• Follow up";
                    model.podiatry.amputationCheck = false;
                }

                if (Request.Query["podiatry.amputationReason"].ToString() != null)
                    model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();

                if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
                {
                    model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
                    model.podiatry.podiatryLopsCheck = true;
                }

                if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
                {
                    model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
                    model.podiatry.podiatryDeformityCheck = true;
                }

                if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
                    model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();

                if (Request.Query["art-disease-yes"].ToString().ToLower().Contains("true"))
                {
                    if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
                        model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
                    model.podiatry.podiatryPerArterialDiseaseCheck = true;
                }
                else if (Request.Query["art-disease-no"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryPerArterialDiseaseAffectedLeg = "-";
                    model.podiatry.podiatryPerArterialDiseaseCheck = false;
                }

                if (Request.Query["wound-yes"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = Request.Query["podiatry.podiatryWoundAffectedLeg"].ToString();
                    model.podiatry.podiatryWoundCheck = true;
                }
                else if (Request.Query["wound-no"].ToString().ToLower().Contains("true"))
                {
                    model.podiatry.podiatryWoundAffectedLeg = "-";
                    model.podiatry.podiatryWoundCheck = false;
                }

                if (Request.Query["podiatry.generalComments"].ToString() != null)
                    model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();

                model.podiatry.programType = "DIABD";
                model.podiatry.followUp = false;
                model.podiatry.active = true;
            }

            var podiatryResults = _member.InsertPodiatryResults(model.podiatry);
            model.drquestionnaireResults.PodiatryID = model.podiatry.PodiatryID;

            //================================================================================ auto-nero ================================================================================//

            if (Request.Query["d-autonero-followup"].ToString().ToLower().Contains("true"))
            {
                model.autoNeropathy = new AutoNeropathy();

                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.AutoNeropathyID = Convert.ToInt32(Request.Query["id"]);
                model.autoNeropathy.createdBy = User.Identity.Name;
                model.autoNeropathy.createdDate = DateTime.Now;

                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() != null)
                    model.autoNeropathy.autoNeuroSympComment = Request.Query["autoNeropathy.autoNeuroSympComment"].ToString();
                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() == null)
                    model.autoNeropathy.autoNeuroSympComment = "• Follow up";

                if (Request.Query["autoNeropathy.generalComments"].ToString() != null)
                    model.autoNeropathy.generalComments = Request.Query["autoNeropathy.generalComments"].ToString();
                if (Request.Query["autoNeropathy.generalComments"].ToString() == null)
                    model.autoNeropathy.generalComments = "• Follow up";

                model.autoNeropathy.programType = "DIABD";

                if ((String.IsNullOrEmpty(Request.Query["autoNeropathy.autoNeuroSympComment"])))
                    model.autoNeropathy.followUp = true;
                else
                    model.autoNeropathy.followUp = false;

                model.autoNeropathy.active = true;
            }
            else if (Request.Query["d-autonero-ignore"].ToString().ToLower().Contains("true"))
            {
                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.createdDate = DateTime.Now;
                model.autoNeropathy.createdBy = User.Identity.Name;
                model.autoNeropathy.autoNeuroSympComment = null;
                model.autoNeropathy.generalComments = "Entry ignored";
                model.autoNeropathy.programType = "DIABD";
                model.autoNeropathy.followUp = false;
                model.autoNeropathy.active = false;
            }
            else
            {
                model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
                model.autoNeropathy.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.autoNeropathy.createdDate = DateTime.Now;
                model.autoNeropathy.createdBy = User.Identity.Name;

                if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() != null)
                    model.autoNeropathy.autoNeuroSympComment = Request.Query["autoNeropathy.autoNeuroSympComment"].ToString();

                if (Request.Query["autoNeropathy.generalComments"].ToString() != null)
                    model.autoNeropathy.generalComments = Request.Query["autoNeropathy.generalComments"].ToString();

                model.autoNeropathy.programType = "DIABD";
                model.autoNeropathy.followUp = false;
                model.autoNeropathy.active = true;
            }

            var autoNeroResults = _member.InsertAutoNeroResults(model.autoNeropathy);
            model.drquestionnaireResults.AutoNeropathyID = model.autoNeropathy.AutoNeropathyID;

            //============================================================================== hypoglycaemia ==============================================================================//

            if (Request.Query["d-hypoglycaemia-followup"].ToString().ToLower().Contains("true"))
            {
                model.hypoglycaemia = new Hypoglycaemia();

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
                model.hypoglycaemia.createdBy = User.Identity.Name;
                model.hypoglycaemia.createdDate = DateTime.Now;

                if (Request.Query["hypog-symptoms-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;
                if (Request.Query["hypog-symptoms-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = false;

                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() == null || Request.Query["hypoglycaemia.glucoseReading"] == "")
                    model.hypoglycaemia.glucoseReading = "• Follow up";

                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() == null)
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = "• Follow up";

                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() == null || Request.Query["hypoglycaemia.hypoglycaemiaAssistance"] == "")
                    model.hypoglycaemia.hypoglycaemiaAssistance = "• Follow up";

                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() != null)
                    model.hypoglycaemia.emergencyRoomVisits = Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString();
                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() == null || Request.Query["hypoglycaemia.emergencyRoomVisits"] == "")
                    model.hypoglycaemia.emergencyRoomVisits = "• Follow up";

                if (Request.Query["insulin-use-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.insulinUseCheck = true;
                if (Request.Query["insulin-use-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.insulinUseCheck = false;

                if (Request.Query["sulfonylurea-use-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.sulfonylureaUseCheck = true;
                if (Request.Query["sulfonylurea-use-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.sulfonylureaUseCheck = false;

                if (Request.Query["ckd-stage-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.ckdStageCheck = true;
                if (Request.Query["ckd-stage-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.ckdStageCheck = false;

                if (Request.Query["patient-age-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.patientAgeCheck = true;
                if (Request.Query["patient-age-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.patientAgeCheck = false;

                model.hypoglycaemia.programType = "DIABD";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();
                if (Request.Query["hypoglycaemia.generalComments"].ToString() == null)
                    model.hypoglycaemia.generalComments = "• Follow up";

                if (String.IsNullOrEmpty(Request.Query["hypoglycaemia.emergencyRoomVisits"]) || String.IsNullOrEmpty(Request.Query["hypoglycaemia.glucoseReading"]) ||
                    String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"]) || String.IsNullOrEmpty(Request.Query["hypoglycaemia.hypoglycaemiaAssistance"]))
                {
                    model.hypoglycaemia.followUp = true;
                }
                else
                {
                    model.hypoglycaemia.followUp = false;
                }

                model.hypoglycaemia.active = true;
            }
            else if (Request.Query["d-hypoglycaemia-ignore"].ToString().ToLower().Contains("true"))
            {
                model.hypoglycaemia = new Hypoglycaemia();

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
                model.hypoglycaemia.createdBy = User.Identity.Name;
                model.hypoglycaemia.createdDate = DateTime.Now;

                model.hypoglycaemia.hypoglycaemiaComment = null;
                model.hypoglycaemia.emergencyRoomVisits = null;
                model.hypoglycaemia.glucoseReading = null;
                model.hypoglycaemia.hypoglycaemiaSymptomsComment = null;
                model.hypoglycaemia.hypoglycaemiaAssistance = null;
                model.hypoglycaemia.generalComments = "Entry ignored";

                model.hypoglycaemia.programType = "DIABD";
                model.hypoglycaemia.followUp = false;
                model.hypoglycaemia.active = false;
            }
            else
            {
                if (Request.Query["hypog-symptoms-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaCheck = true;
                if (Request.Query["hypog-symptoms-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.hypoglycaemiaCheck = false;

                model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
                model.hypoglycaemia.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.hypoglycaemia.createdDate = DateTime.Now;
                model.hypoglycaemia.createdBy = User.Identity.Name;

                if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
                    model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
                {
                    model.hypoglycaemia.hypoglycaemiaSymptomsCheck = true;
                    model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
                }
                if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
                if (Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString() != null)
                    model.hypoglycaemia.hypoglycaemiaComment = Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString();
                if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() != null)
                    model.hypoglycaemia.emergencyRoomVisits = Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString();

                if (Request.Query["insulin-use-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.insulinUseCheck = true;
                if (Request.Query["insulin-use-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.insulinUseCheck = false;

                if (Request.Query["sulfonylurea-use-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.sulfonylureaUseCheck = true;
                if (Request.Query["sulfonylurea-use-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.sulfonylureaUseCheck = false;

                if (Request.Query["ckd-stage-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.ckdStageCheck = true;
                if (Request.Query["ckd-stage-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.ckdStageCheck = false;

                if (Request.Query["patient-age-yes"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.patientAgeCheck = true;
                if (Request.Query["patient-age-no"].ToString().ToLower().Contains("true"))
                    model.hypoglycaemia.patientAgeCheck = false;

                model.hypoglycaemia.programType = "DIABD";

                if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
                    model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();

                model.hypoglycaemia.followUp = false;
                model.hypoglycaemia.active = true;

            }

            var hyperGResults = _member.InsertHypoglycaemiaResults(model.hypoglycaemia);
            model.drquestionnaireResults.HypoglycaemiaID = model.hypoglycaemia.HypoglycaemiaID;

            //================================================================================= general =================================================================================//

            if (Request.Query["d-general-followup"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.generalComments = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;

                if (Request.Query["depression-yes"].ToString().ToLower().Contains("true"))
                    model.questionnaireOther.depression = true;
                if (Request.Query["depression-no"].ToString().ToLower().Contains("true"))
                    model.questionnaireOther.depression = false;
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();

                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.followUp = true;
                model.questionnaireOther.active = true;
            }
            else if (Request.Query["d-general-ignore"].ToString().ToLower().Contains("true"))
            {
                model.questionnaireOther = new QuestionnaireOther();

                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;
                model.questionnaireOther.depression = false;
                model.questionnaireOther.depressionComment = null;
                model.questionnaireOther.generalComments = "Entry ignored";
                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = false;

            }
            else
            {
                model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
                model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
                model.questionnaireOther.questionnaireID = new Guid(Request.Query["questionnaireID"]);
                model.questionnaireOther.createdDate = DateTime.Now;
                model.questionnaireOther.createdBy = User.Identity.Name;
                model.questionnaireOther.occupation = null;
                model.questionnaireOther.shiftWorker = null;
                model.questionnaireOther.generalComments = null;
                model.questionnaireOther.recDrugs = false;
                model.questionnaireOther.recDrugsLastUsed = null;
                model.questionnaireOther.TBScreen = false;
                model.questionnaireOther.TBScreenResult = null;
                model.questionnaireOther.TBScreenDate = null;
                model.questionnaireOther.tbDiagnosed = false;
                model.questionnaireOther.tbTreatmentStartDate = null;
                model.questionnaireOther.tbTreatmentEndDate = null;
                model.questionnaireOther.Pregnant = false;
                model.questionnaireOther.EDD = null;
                model.questionnaireOther.frailCareCheck = false;
                model.questionnaireOther.frailCare = "None";
                model.questionnaireOther.frailCareComment = null;
                model.questionnaireOther.bloodTestFrequency = null;
                model.questionnaireOther.bloodTestEffectiveDate = null;

                if (Request.Query["depression-yes"].ToString().ToLower().Contains("true"))
                    model.questionnaireOther.depression = true;
                if (Request.Query["depression-no"].ToString().ToLower().Contains("true"))
                    model.questionnaireOther.depression = false;
                if (!string.IsNullOrEmpty(Request.Query["questionnaireOther.depressionComment"].ToString()))
                    model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();

                model.questionnaireOther.programType = "DIABD";
                model.questionnaireOther.followUp = false;
                model.questionnaireOther.active = true;

            }

            var questionOtherResults = _member.InsertQuestionnaireOtherResults(model.questionnaireOther);
            model.drquestionnaireResults.QuestionnaireOtherID = model.questionnaireOther.QuestionnaireOtherID;

            //========================================================================= dr-questionnaire-insert =========================================================================//

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);

            //=============================================================================== task-update ===============================================================================//

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            //check for follow ups to create new Assignment
            if (
                Request.Query["d-optometry-followup"].ToString().ToLower().Contains("true") ||
                Request.Query["d-podiatry-followup"].ToString().ToLower().Contains("true") ||
                Request.Query["d-autonero-followup"].ToString().ToLower().Contains("true") ||
                Request.Query["d-hypoglycaemia-followup"].ToString().ToLower().Contains("true") ||
                Request.Query["d-general-followup"].ToString().ToLower().Contains("true")
            )
            {
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = new Guid(Request.Query["DependantID"]),
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Diabetes questionnaire follow up",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType);
                if (assignExists == null)
                {
                    var results = _member.InsertAssignment(assignment);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignment.itemID;

                    var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                    foreach (var tas in taskss)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        //================================================================================== Questionnaire_Diabetes ==================================================================================//

        public ActionResult _NewBornQuestionnaire(Guid DependentID, int id, int taskId, string templateID, string membershipNo, string dependantCode) //HCare-968
        {
            var model = new DoctorQuestionnaireViewModel();

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;
            ViewBag.dependantID = DependentID;
            ViewBag.templateID = templateID;
            ViewBag.programID = Request.Query["pro"];
            ViewBag.uniqueID = id;
            ViewBag.taskID = taskId;

            model.NewBorns = _member.GetNewBornResults(DependentID);

            return View(model);
        }

        [HttpPost]
        public ActionResult _NewBornQuestionnaire(DoctorQuestionnaireViewModel model) //HCare-968 //hcare-1451
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["nb-program-id"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["nb-dependant-id"]);
            var uniqueid = Convert.ToInt32(Request.Query["nb-unique-id"]);
            var questionnaireid = new Guid(Request.Query["nb-questionnaire-id"]);

            model.drquestionnaireResults = new DoctorQuestionnaireResults();
            model.drquestionnaireResults.dependentID = dependantid;
            model.drquestionnaireResults.Id = uniqueid;
            model.drquestionnaireResults.createdBy = User.Identity.Name;
            model.drquestionnaireResults.createdDate = DateTime.Now;
            model.drquestionnaireResults.Active = true;

            var dependantInfo = _member.GetDependantByDependantID(dependantid);

            model.NewBorn = new NewBorn();
            model.NewBorn.Id = uniqueid;
            model.NewBorn.DependantID = dependantid;
            model.NewBorn.QuestionnaireID = questionnaireid;
            model.NewBorn.CreatedBy = User.Identity.Name;
            model.NewBorn.CreatedDate = DateTime.Now;

            if (Request.Query["nb-concern"].ToString().ToLower().Contains("yes"))
            {
                model.NewBorn.hasConcern = true;
                model.NewBorn.Concern = Request.Query["newborn-concern"];
            }
            if (Request.Query["nb-breastfeeding"].ToString().ToLower().Contains("yes"))
            {
                model.NewBorn.isBreastfeeding = true;
                model.NewBorn.Breastfeeding = Request.Query["newborn-breastfeeding"];
            }
            if (Request.Query["nb-medication"].ToString().ToLower().Contains("yes"))
            {
                model.NewBorn.isOnMedication = true;
                model.NewBorn.Medication = Request.Query["newborn-medication"];
            }
            if (Request.Query["nb-hiv-test"].ToString().ToLower().Contains("yes"))
            {
                model.NewBorn.hivTest = true;
                model.NewBorn.HIVTestComment = Request.Query["newborn-hiv-test"];
            }
            if (Request.Query["nb-hiv-result"].ToString().ToLower().Contains("yes"))
            {
                model.NewBorn.hivResults = true;
                model.NewBorn.HIVResultsComment = Request.Query["newborn-hiv-result"];
            }

            model.NewBorn.GeneralComments = "Newborn questionnaire";
            model.NewBorn.Program = program.code;
            model.NewBorn.FollowUp = false;
            model.NewBorn.Active = true;

            var newBornResults = _member.InsertNewBornResults(model.NewBorn);
            model.drquestionnaireResults.NewBornID = model.NewBorn.Id;

            var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);

            var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["nb-task-id"]));

            tasks.closed = true;
            tasks.closedBy = User.Identity.Name;
            tasks.closedDate = DateTime.Now;
            tasks.status = "Closed";

            var taskResult = _clinical.UpdateTask(tasks);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["nb-unique-id"], pro = Request.Query["nb-program-id"] });
        }

        public ActionResult _DSM5Questionnaire(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1123
        {
            var model = new MentalHealthVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult _DSM5Questionnaire(MentalHealthVM model) //HCare-1123 //HCare-1205
        {
            if (Request.Query["dsm5-follow-up"].ToString().ToLower().Contains("true"))
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region dsm5-insert
                var dsm5 = new MH_DSM5ResponseNEW();
                dsm5.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                dsm5.DependantID = dependantid;
                dsm5.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                dsm5.CreatedDate = DateTime.Now;
                dsm5.CreatedBy = User.Identity.Name;

                if (Request.Query["abuse-yes"].ToString().ToLower().Contains("true")) { dsm5.SubstanceAbuse = true; } else { dsm5.SubstanceAbuse = false; }
                if (!String.IsNullOrEmpty(Request.Query["depression-one-text"]))
                {
                    dsm5.DepressionOne = Request.Query["depression-one-text"];
                    dsm5.DepressionOneSC = Convert.ToInt32(Request.Query["depression-one-score"]);
                }
                else { dsm5.DepressionOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["depression-two-text"]))
                {
                    dsm5.DepressionTwo = Request.Query["depression-two-text"];
                    dsm5.DepressionTwoSC = Convert.ToInt32(Request.Query["depression-two-score"]);
                }
                else { dsm5.DepressionTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["anger-one-text"]))
                {
                    dsm5.AngerOne = Request.Query["anger-one-text"];
                    dsm5.AngerOneSC = Convert.ToInt32(Request.Query["anger-one-score"]);
                }
                else { dsm5.AngerOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["mania-one-text"]))
                {
                    dsm5.ManiaOne = Request.Query["mania-one-text"];
                    dsm5.ManiaOneSC = Convert.ToInt32(Request.Query["mania-one-score"]);
                }
                else { dsm5.ManiaOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["mania-two-text"]))
                {
                    dsm5.ManiaTwo = Request.Query["mania-two-text"];
                    dsm5.ManiaTwoSC = Convert.ToInt32(Request.Query["mania-two-score"]);
                }
                else { dsm5.ManiaTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["anxiety-one-text"]))
                {
                    dsm5.AnxietyOne = Request.Query["anxiety-one-text"];
                    dsm5.AnxietyOneSC = Convert.ToInt32(Request.Query["anxiety-one-score"]);
                }
                else { dsm5.AnxietyOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["anxiety-two-text"]))
                {
                    dsm5.AnxietyTwo = Request.Query["anxiety-two-text"];
                    dsm5.AnxietyTwoSC = Convert.ToInt32(Request.Query["anxiety-two-score"]);
                }
                else { dsm5.AnxietyTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["anxiety-three-text"]))
                {
                    dsm5.AnxietyThree = Request.Query["anxiety-three-text"];
                    dsm5.AnxietyThreeSC = Convert.ToInt32(Request.Query["anxiety-three-score"]);
                }
                else { dsm5.AnxietyThree = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["somatic-one-text"]))
                {
                    dsm5.SomaticOne = Request.Query["somatic-one-text"];
                    dsm5.SomaticOneSC = Convert.ToInt32(Request.Query["somatic-one-score"]);
                }
                else { dsm5.SomaticOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["somatic-two-text"]))
                {
                    dsm5.SomaticTwo = Request.Query["somatic-two-text"];
                    dsm5.SomaticTwoSC = Convert.ToInt32(Request.Query["somatic-two-score"]);
                }
                else { dsm5.SomaticTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["suicide-one-text"]))
                {
                    dsm5.SuicidalOne = Request.Query["suicide-one-text"];
                    dsm5.SuicidalOneSC = Convert.ToInt32(Request.Query["suicide-one-score"]);
                }
                else { dsm5.SuicidalOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["psychosis-one-text"]))
                {
                    dsm5.PsychosisOne = Request.Query["psychosis-one-text"];
                    dsm5.PsychosisOneSC = Convert.ToInt32(Request.Query["psychosis-one-score"]);
                }
                else { dsm5.PsychosisOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["psychosis-two-text"]))
                {
                    dsm5.PsychosisTwo = Request.Query["psychosis-two-text"];
                    dsm5.PsychosisTwoSC = Convert.ToInt32(Request.Query["psychosis-two-score"]);
                }
                else { dsm5.PsychosisTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["sleep-one-text"]))
                {
                    dsm5.SleepOne = Request.Query["sleep-one-text"];
                    dsm5.SleepOneSC = Convert.ToInt32(Request.Query["sleep-one-score"]);
                }
                else { dsm5.SleepOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["memory-one-text"]))
                {
                    dsm5.MemoryOne = Request.Query["memory-one-text"];
                    dsm5.MemoryOneSC = Convert.ToInt32(Request.Query["memory-one-score"]);
                }
                else { dsm5.MemoryOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["behaviour-one-text"]))
                {
                    dsm5.BehaviourOne = Request.Query["behaviour-one-text"];
                    dsm5.BehaviourOneSC = Convert.ToInt32(Request.Query["behaviour-one-score"]);
                }
                else { dsm5.BehaviourOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["behaviour-two-text"]))
                {
                    dsm5.BehaviourTwo = Request.Query["behaviour-two-text"];
                    dsm5.BehaviourTwoSC = Convert.ToInt32(Request.Query["behaviour-two-score"]);
                }
                else { dsm5.BehaviourTwo = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["dissociation-one-text"]))
                {
                    dsm5.DissociationOne = Request.Query["dissociation-one-text"];
                    dsm5.DissociationOneSC = Convert.ToInt32(Request.Query["dissociation-one-score"]);
                }
                else { dsm5.DissociationOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["personality-one-text"]))
                {
                    dsm5.PersonalityOne = Request.Query["personality-one-text"];
                    dsm5.PersonalityOneSC = Convert.ToInt32(Request.Query["personality-one-score"]);
                }
                else { dsm5.PersonalityOne = "Follow up"; }
                if (!String.IsNullOrEmpty(Request.Query["personality-two-text"]))
                {
                    dsm5.PersonalityTwo = Request.Query["personality-two-text"];
                    dsm5.PersonalityTwoSC = Convert.ToInt32(Request.Query["personality-two-score"]);
                }
                else { dsm5.PersonalityTwo = "Follow up"; }

                dsm5.TotalScore = 0;
                dsm5.Outcome = "Follow up";
                dsm5.Comment = "DSM5 questionnaire assignment - Follow up";
                dsm5.Program = programcode.code;
                dsm5.FollowUp = true;
                dsm5.Active = true;
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region follow-up-assignment
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = dependantid,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Mental health follow up required - Schizophrenia",
                        programId = programcode.programID,

                    },

                    assignmentItemType = "FUAQ",

                };
                var results = _member.InsertAssignment(assignment);

                var taskitem = new AssignmentItemTasks();
                taskitem.assignmentItemID = assignment.itemID;

                var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                foreach (var tas in taskss)
                {
                    taskitem.taskTypeId = tas.taskType;
                    taskitem.requirementId = tas.requirementId;
                    _member.InsertTask(taskitem);
                }
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (dsm5.SuicidalOneSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: DSM5/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region risk-rating
                if (dsm5.SuicidalOneSC != 0)
                {
                    var risk = new PatientRiskRatingHistory();
                    risk.effectiveDate = DateTime.Now;
                    risk.dependantID = dependantid;
                    risk.createdDate = DateTime.Now;
                    risk.createdBy = User.Identity.Name;
                    risk.programType = programcode.code;
                    risk.active = true;

                    risk.RiskId = "R";
                    risk.reason = "DSM5 questionnaire assignment - Suicidal";
                    _member.InsertRiskRating(risk);
                    dsm5.RiskID = risk.id;

                    #region case-manager-assignmnet hcare-1176
                    if (risk.RiskId.ToUpper() == "R")
                    {
                        var CM_assignment = new AssignmentsView()
                        {
                            newAssignment = new Assignments()
                            {
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependentID = dependantid,
                                Active = true,
                                effectiveDate = DateTime.Now,
                                assignmentType = "INTER",
                                status = "Open",
                                Instruction = "Risk rating: Red - Assign case manager",
                                programId = programcode.programID,
                            },

                            assignmentItemType = "CMAN",
                        };
                        var CM_Insert = _member.InsertAssignment(CM_assignment);

                        var CM_taskitem = new AssignmentItemTasks();
                        CM_taskitem.assignmentItemID = CM_assignment.itemID;

                        var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                        foreach (var xtask in DRA_task)
                        {
                            CM_taskitem.taskTypeId = xtask.taskType;
                            CM_taskitem.requirementId = xtask.requirementId;
                            _member.InsertTask(CM_taskitem);
                        }
                    }
                    #endregion
                }

                #endregion
                var insert = _admin.InsertNEWDSM5Results(dsm5);
            }
            else
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);
                var totalScore = 0;
                #region dsm5-insert
                var dsm5 = new MH_DSM5ResponseNEW();
                dsm5.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                dsm5.DependantID = dependantid;
                dsm5.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                dsm5.CreatedDate = DateTime.Now;
                dsm5.CreatedBy = User.Identity.Name;

                if (Request.Query["abuse-yes"].ToString().ToLower().Contains("true")) { dsm5.SubstanceAbuse = true; } else { dsm5.SubstanceAbuse = false; }
                if (!String.IsNullOrEmpty(Request.Query["depression-one-text"]))
                {
                    dsm5.DepressionOne = Request.Query["depression-one-text"];
                    dsm5.DepressionOneSC = Convert.ToInt32(Request.Query["depression-one-score"]);
                    totalScore = dsm5.DepressionOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["depression-two-text"]))
                {
                    dsm5.DepressionTwo = Request.Query["depression-two-text"];
                    dsm5.DepressionTwoSC = Convert.ToInt32(Request.Query["depression-two-score"]);
                    totalScore = dsm5.DepressionTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["anger-one-text"]))
                {
                    dsm5.AngerOne = Request.Query["anger-one-text"];
                    dsm5.AngerOneSC = Convert.ToInt32(Request.Query["anger-one-score"]);
                    totalScore = dsm5.AngerOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["mania-one-text"]))
                {
                    dsm5.ManiaOne = Request.Query["mania-one-text"];
                    dsm5.ManiaOneSC = Convert.ToInt32(Request.Query["mania-one-score"]);
                    totalScore = dsm5.ManiaOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["mania-two-text"]))
                {
                    dsm5.ManiaTwo = Request.Query["mania-two-text"];
                    dsm5.ManiaTwoSC = Convert.ToInt32(Request.Query["mania-two-score"]);
                    totalScore = dsm5.ManiaTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["anxiety-one-text"]))
                {
                    dsm5.AnxietyOne = Request.Query["anxiety-one-text"];
                    dsm5.AnxietyOneSC = Convert.ToInt32(Request.Query["anxiety-one-score"]);
                    totalScore = dsm5.AnxietyOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["anxiety-two-text"]))
                {
                    dsm5.AnxietyTwo = Request.Query["anxiety-two-text"];
                    dsm5.AnxietyTwoSC = Convert.ToInt32(Request.Query["anxiety-two-score"]);
                    totalScore = dsm5.AnxietyTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["anxiety-three-text"]))
                {
                    dsm5.AnxietyThree = Request.Query["anxiety-three-text"];
                    dsm5.AnxietyThreeSC = Convert.ToInt32(Request.Query["anxiety-three-score"]);
                    totalScore = dsm5.AnxietyThreeSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["somatic-one-text"]))
                {
                    dsm5.SomaticOne = Request.Query["somatic-one-text"];
                    dsm5.SomaticOneSC = Convert.ToInt32(Request.Query["somatic-one-score"]);
                    totalScore = dsm5.SomaticOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["somatic-two-text"]))
                {
                    dsm5.SomaticTwo = Request.Query["somatic-two-text"];
                    dsm5.SomaticTwoSC = Convert.ToInt32(Request.Query["somatic-two-score"]);
                    totalScore = dsm5.SomaticTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["suicide-one-text"]))
                {
                    dsm5.SuicidalOne = Request.Query["suicide-one-text"];
                    dsm5.SuicidalOneSC = Convert.ToInt32(Request.Query["suicide-one-score"]);
                    totalScore = dsm5.SuicidalOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["psychosis-one-text"]))
                {
                    dsm5.PsychosisOne = Request.Query["psychosis-one-text"];
                    dsm5.PsychosisOneSC = Convert.ToInt32(Request.Query["psychosis-one-score"]);
                    totalScore = dsm5.PsychosisOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["psychosis-two-text"]))
                {
                    dsm5.PsychosisTwo = Request.Query["psychosis-two-text"];
                    dsm5.PsychosisTwoSC = Convert.ToInt32(Request.Query["psychosis-two-score"]);
                    totalScore = dsm5.PsychosisTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["sleep-one-text"]))
                {
                    dsm5.SleepOne = Request.Query["sleep-one-text"];
                    dsm5.SleepOneSC = Convert.ToInt32(Request.Query["sleep-one-score"]);
                    totalScore = dsm5.SleepOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["memory-one-text"]))
                {
                    dsm5.MemoryOne = Request.Query["memory-one-text"];
                    dsm5.MemoryOneSC = Convert.ToInt32(Request.Query["memory-one-score"]);
                    totalScore = dsm5.MemoryOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["behaviour-one-text"]))
                {
                    dsm5.BehaviourOne = Request.Query["behaviour-one-text"];
                    dsm5.BehaviourOneSC = Convert.ToInt32(Request.Query["behaviour-one-score"]);
                    totalScore = dsm5.BehaviourOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["behaviour-two-text"]))
                {
                    dsm5.BehaviourTwo = Request.Query["behaviour-two-text"];
                    dsm5.BehaviourTwoSC = Convert.ToInt32(Request.Query["behaviour-two-score"]);
                    totalScore = dsm5.BehaviourTwoSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["dissociation-one-text"]))
                {
                    dsm5.DissociationOne = Request.Query["dissociation-one-text"];
                    dsm5.DissociationOneSC = Convert.ToInt32(Request.Query["dissociation-one-score"]);
                    totalScore = dsm5.DissociationOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["personality-one-text"]))
                {
                    dsm5.PersonalityOne = Request.Query["personality-one-text"];
                    dsm5.PersonalityOneSC = Convert.ToInt32(Request.Query["personality-one-score"]);
                    totalScore = dsm5.PersonalityOneSC + totalScore;
                }

                if (!String.IsNullOrEmpty(Request.Query["personality-two-text"]))
                {
                    dsm5.PersonalityTwo = Request.Query["personality-two-text"];
                    dsm5.PersonalityTwoSC = Convert.ToInt32(Request.Query["personality-two-score"]);
                    totalScore = dsm5.PersonalityTwoSC + totalScore;
                }

                dsm5.TotalScore = totalScore;
                #region outcome
                if (dsm5.SuicidalOneSC > 0)
                {
                    dsm5.Outcome = "Severe";
                }
                else if (totalScore < 1)
                {
                    dsm5.Outcome = "Normal";
                }
                else if (totalScore <= 2)
                {
                    dsm5.Outcome = "Mild";
                }
                else if (totalScore >= 3 && totalScore <= 5)
                {
                    dsm5.Outcome = "Moderate";
                }
                else if (totalScore >= 6 && totalScore <= 8)
                {
                    dsm5.Outcome = "Severe";
                }
                #endregion
                dsm5.Comment = "DSM5 questionnaire assignment";
                dsm5.Program = programcode.code;
                dsm5.FollowUp = false;
                dsm5.Active = true;
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (dsm5.SuicidalOneSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: DSM5/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region risk-rating
                if (dsm5.SuicidalOneSC != 0)
                {
                    var risk = new PatientRiskRatingHistory();
                    risk.effectiveDate = DateTime.Now;
                    risk.dependantID = dependantid;
                    risk.createdDate = DateTime.Now;
                    risk.createdBy = User.Identity.Name;
                    risk.programType = programcode.code;
                    risk.active = true;

                    risk.RiskId = "R";
                    risk.reason = "DSM5 questionnaire assignment - Suicidal";
                    _member.InsertRiskRating(risk);
                    dsm5.RiskID = risk.id;

                    #region case-manager-assignmnet hcare-1176
                    if (risk.RiskId.ToUpper() == "R")
                    {
                        var CM_assignment = new AssignmentsView()
                        {
                            newAssignment = new Assignments()
                            {
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependentID = dependantid,
                                Active = true,
                                effectiveDate = DateTime.Now,
                                assignmentType = "INTER",
                                status = "Open",
                                Instruction = "Risk rating: Red - Assign case manager",
                                programId = programcode.programID,
                            },

                            assignmentItemType = "CMAN",
                        };
                        var CM_Insert = _member.InsertAssignment(CM_assignment);

                        var CM_taskitem = new AssignmentItemTasks();
                        CM_taskitem.assignmentItemID = CM_assignment.itemID;

                        var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                        foreach (var xtask in DRA_task)
                        {
                            CM_taskitem.taskTypeId = xtask.taskType;
                            CM_taskitem.requirementId = xtask.requirementId;
                            _member.InsertTask(CM_taskitem);
                        }
                    }
                    #endregion
                }
                #endregion
                #region dsm5-scoring + riskrating hcare-1206 + hcare-1207
                var dsm5score = new MH_DSM5Score();
                dsm5score.EffectiveDate = DateTime.Now;
                dsm5score.DependantID = dependantid;
                dsm5score.CreatedDate = DateTime.Now;
                dsm5score.CreatedBy = User.Identity.Name;
                dsm5score.Program = programcode.code;
                dsm5score.Active = true;

                var history = _admin.GetDSM5ScoreHistory(dependantid);
                var riskHistory = _member.GetPatientRiskRating(dependantid);

                if (history.Count > 0)
                {
                    var lastDSM5Score = history.OrderByDescending(x => x.EffectiveDate).First();
                    if (lastDSM5Score.Score < 25)
                    {
                        if (dsm5.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Stable";

                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = new PatientRiskRatingHistory();
                                        risk.effectiveDate = DateTime.Now;
                                        risk.dependantID = dependantid;
                                        risk.createdDate = DateTime.Now;
                                        risk.createdBy = User.Identity.Name;
                                        risk.programType = programcode.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        dsm5score.RiskID = risk.id;
                                        dsm5.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(dsm5score);

                        }
                        else if ((dsm5.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (dsm5.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = programcode.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                dsm5.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Degression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo <= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Progression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo >= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                    }
                    if (lastDSM5Score.Score >= 25 && lastDSM5Score.Score <= 55)
                    {
                        if (dsm5.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Stable";

                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = new PatientRiskRatingHistory();
                                        risk.effectiveDate = DateTime.Now;
                                        risk.dependantID = dependantid;
                                        risk.createdDate = DateTime.Now;
                                        risk.createdBy = User.Identity.Name;
                                        risk.programType = programcode.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        dsm5score.RiskID = risk.id;
                                        dsm5.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if ((dsm5.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (dsm5.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = programcode.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                dsm5.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Degression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo <= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Progression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo >= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                    }
                    if (lastDSM5Score.Score > 55)
                    {
                        if (dsm5.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Stable";
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                dsm5score.Score = dsm5.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = new PatientRiskRatingHistory();
                                        risk.effectiveDate = DateTime.Now;
                                        risk.dependantID = dependantid;
                                        risk.createdDate = DateTime.Now;
                                        risk.createdBy = User.Identity.Name;
                                        risk.programType = programcode.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        dsm5score.RiskID = risk.id;
                                        dsm5.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if ((dsm5.TotalScore <= ((lastDSM5Score.Score * 4) / 100) + lastDSM5Score.Score) && (dsm5.TotalScore >= ((lastDSM5Score.Score * -4) / 100) + lastDSM5Score.Score))
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Stable";

                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = programcode.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                dsm5.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore >= ((lastDSM5Score.Score * 5) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Degression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo <= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                        else if (dsm5.TotalScore <= ((lastDSM5Score.Score * -5) / 100) + lastDSM5Score.Score)
                        {
                            dsm5score.Score = dsm5.TotalScore;
                            dsm5score.Reason = "Progression";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                var riskratingTypes = _member.GetRiskRatingTypes();

                                var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                if (lastRRinfo >= 2)
                                {
                                    var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = programcode.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    dsm5.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(dsm5score);
                        }
                    }
                }
                else
                {
                    dsm5score.Score = dsm5.TotalScore;
                    dsm5score.Reason = "Baseline";

                    _admin.InsertDSM5Score(dsm5score);
                }
                #endregion
                dsm5.DSM5ScoreID = dsm5score.Id;


                var insert = _admin.InsertNEWDSM5Results(dsm5);
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _SchizophreniaQuestionnaire(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1124
        {
            var model = new MentalHealthVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult _SchizophreniaQuestionnaire(MentalHealthVM model) //HCare-1124
        {

            if (Request.Query["schizophrenia-follow-up"].ToString().ToLower().Contains("true"))
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region schizophrenia-insert
                var schizophrenia = new MH_SchizophreniaResponse();
                schizophrenia.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                schizophrenia.DependantID = dependantid;
                schizophrenia.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                schizophrenia.CreatedDate = DateTime.Now;
                schizophrenia.CreatedBy = User.Identity.Name;

                if (!String.IsNullOrEmpty(Request.Query["depression-text"]))
                {
                    schizophrenia.Depression = Request.Query["depression-text"];
                    schizophrenia.DepressionSC = Convert.ToInt32(Request.Query["depression-score"]);
                }
                else { schizophrenia.Depression = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["hoplessness-text"]))
                {
                    schizophrenia.Hopelessness = Request.Query["hoplessness-text"];
                    schizophrenia.HopelessnessSC = Convert.ToInt32(Request.Query["hoplessness-score"]);
                }
                else { schizophrenia.Hopelessness = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["self-depreciation-text"]))
                {
                    schizophrenia.SelfDepreciation = Request.Query["self-depreciation-text"];
                    schizophrenia.SelfDepreciationSC = Convert.ToInt32(Request.Query["self-depreciation-score"]);
                }
                else { schizophrenia.SelfDepreciation = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["guilty-ideas-text"]))
                {
                    schizophrenia.GuiltyIdeas = Request.Query["guilty-ideas-text"];
                    schizophrenia.GuiltyIdeasSC = Convert.ToInt32(Request.Query["guilty-ideas-score"]);
                }
                else { schizophrenia.GuiltyIdeas = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["pathological-guilt-text"]))
                {
                    schizophrenia.PathologicalGuilt = Request.Query["pathological-guilt-text"];
                    schizophrenia.PathologicalGuiltSC = Convert.ToInt32(Request.Query["pathological-guilt-score"]);
                }
                else { schizophrenia.PathologicalGuilt = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["morning-depresion-text"]))
                {
                    schizophrenia.MorningDepression = Request.Query["morning-depresion-text"];
                    schizophrenia.MorningDepressionSC = Convert.ToInt32(Request.Query["morning-depresion-score"]);
                }
                else { schizophrenia.MorningDepression = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["early-wakening-text"]))
                {
                    schizophrenia.EarlyWakening = Request.Query["early-wakening-text"];
                    schizophrenia.EarlyWakeningSC = Convert.ToInt32(Request.Query["early-wakening-score"]);
                }
                else { schizophrenia.EarlyWakening = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["suicide-text"]))
                {
                    schizophrenia.Suicidal = Request.Query["suicide-text"];
                    schizophrenia.SuicidalSC = Convert.ToInt32(Request.Query["suicide-score"]);
                }
                else { schizophrenia.Suicidal = "Follow up"; }

                if (Request.Query["MH_SchizophreniaResponse.SuicidalCMT"].ToString() != null)
                {
                    schizophrenia.SuicidalCMT = Request.Query["MH_SchizophreniaResponse.SuicidalCMT"];
                }

                if (!String.IsNullOrEmpty(Request.Query["observed-depression-text"]))
                {
                    schizophrenia.ObservedDepression = Request.Query["observed-depression-text"];
                    schizophrenia.ObservedDepressionSC = Convert.ToInt32(Request.Query["observed-depression-score"]);
                }
                else { schizophrenia.ObservedDepression = "Follow up"; }

                schizophrenia.TotalScore = 0;
                schizophrenia.Outcome = "Follow up";
                schizophrenia.Comment = "Schizophrenia questionnaire assignment - Follow up";
                schizophrenia.Program = programcode.code;
                schizophrenia.FollowUp = true;
                schizophrenia.Active = true;
                #endregion
                #region risk-rating hcare-1151
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.RiskId = "-";
                risk.reason = "Schizophrenia questionnaire assignment - follow-up";
                risk.active = false;

                _member.InsertRiskRating(risk);

                schizophrenia.RiskID = risk.id;
                var insert = _admin.InsertSchizophreniaResults(schizophrenia);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region follow-up-assignment
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = dependantid,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Mental health follow up required - Schizophrenia",
                        programId = programcode.programID,

                    },

                    assignmentItemType = "FUAQ",

                };
                var results = _member.InsertAssignment(assignment);

                var taskitem = new AssignmentItemTasks();
                taskitem.assignmentItemID = assignment.itemID;

                var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                foreach (var tas in taskss)
                {
                    taskitem.taskTypeId = tas.taskType;
                    taskitem.requirementId = tas.requirementId;
                    _member.InsertTask(taskitem);
                }
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (schizophrenia.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Schizophrenia/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }
            else
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);
                var totalScore = 0;

                #region schizophrenia-insert
                var schizophrenia = new MH_SchizophreniaResponse();
                schizophrenia.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                schizophrenia.DependantID = dependantid;
                schizophrenia.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                schizophrenia.CreatedDate = DateTime.Now;
                schizophrenia.CreatedBy = User.Identity.Name;

                if (Request.Query["depression-text"].ToString() != null)
                {
                    schizophrenia.Depression = Request.Query["depression-text"];
                    schizophrenia.DepressionSC = Convert.ToInt32(Request.Query["depression-score"]);
                    totalScore = schizophrenia.DepressionSC + totalScore;
                }

                if (Request.Query["hoplessness-text"].ToString() != null)
                {
                    schizophrenia.Hopelessness = Request.Query["hoplessness-text"];
                    schizophrenia.HopelessnessSC = Convert.ToInt32(Request.Query["hoplessness-score"]);
                    totalScore = schizophrenia.HopelessnessSC + totalScore;
                }

                if (Request.Query["self-depreciation-text"].ToString() != null)
                {
                    schizophrenia.SelfDepreciation = Request.Query["self-depreciation-text"];
                    schizophrenia.SelfDepreciationSC = Convert.ToInt32(Request.Query["self-depreciation-score"]);
                    totalScore = schizophrenia.SelfDepreciationSC + totalScore;
                }

                if (Request.Query["guilty-ideas-text"].ToString() != null)
                {
                    schizophrenia.GuiltyIdeas = Request.Query["guilty-ideas-text"];
                    schizophrenia.GuiltyIdeasSC = Convert.ToInt32(Request.Query["guilty-ideas-score"]);
                    totalScore = schizophrenia.GuiltyIdeasSC + totalScore;
                }

                if (Request.Query["pathological-guilt-text"].ToString() != null)
                {
                    schizophrenia.PathologicalGuilt = Request.Query["pathological-guilt-text"];
                    schizophrenia.PathologicalGuiltSC = Convert.ToInt32(Request.Query["pathological-guilt-score"]);
                    totalScore = schizophrenia.PathologicalGuiltSC + totalScore;
                }

                if (Request.Query["morning-depresion-text"].ToString() != null)
                {
                    schizophrenia.MorningDepression = Request.Query["morning-depresion-text"];
                    schizophrenia.MorningDepressionSC = Convert.ToInt32(Request.Query["morning-depresion-score"]);
                    totalScore = schizophrenia.MorningDepressionSC + totalScore;
                }

                if (Request.Query["early-wakening-text"].ToString() != null)
                {
                    schizophrenia.EarlyWakening = Request.Query["early-wakening-text"];
                    schizophrenia.EarlyWakeningSC = Convert.ToInt32(Request.Query["early-wakening-score"]);
                    totalScore = schizophrenia.EarlyWakeningSC + totalScore;
                }

                if (Request.Query["suicide-text"].ToString() != null)
                {
                    schizophrenia.Suicidal = Request.Query["suicide-text"];
                    schizophrenia.SuicidalSC = Convert.ToInt32(Request.Query["suicide-score"]);
                    totalScore = schizophrenia.SuicidalSC + totalScore;
                }

                if (Request.Query["MH_SchizophreniaResponse.SuicidalCMT"].ToString() != null)
                {
                    schizophrenia.SuicidalCMT = Request.Query["MH_SchizophreniaResponse.SuicidalCMT"];
                }

                if (Request.Query["observed-depression-text"].ToString() != null)
                {
                    schizophrenia.ObservedDepression = Request.Query["observed-depression-text"];
                    schizophrenia.ObservedDepressionSC = Convert.ToInt32(Request.Query["observed-depression-score"]);
                    totalScore = schizophrenia.ObservedDepressionSC + totalScore;
                }

                schizophrenia.TotalScore = totalScore;
                #region outcome
                if (schizophrenia.SuicidalSC > 0)
                {
                    schizophrenia.Outcome = "Severe";
                }
                else if (totalScore < 1)
                {
                    schizophrenia.Outcome = "Normal";
                }
                else if (totalScore >= 1 && totalScore <= 9)
                {
                    schizophrenia.Outcome = "Mild";
                }
                else if (totalScore >= 10 && totalScore <= 18)
                {
                    schizophrenia.Outcome = "Moderate";
                }
                else if (totalScore >= 19 && totalScore <= 27)
                {
                    schizophrenia.Outcome = "Severe";
                }
                #endregion
                schizophrenia.Comment = "Schizophrenia questionnaire assignment";
                schizophrenia.Program = programcode.code;
                schizophrenia.Active = true;

                #endregion
                #region risk-rating
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.active = true;

                if (schizophrenia.SuicidalSC != 0)
                {
                    risk.RiskId = "R";
                    risk.reason = "Schizophrenia questionnaire assignment - Suicidal";
                }
                else if (totalScore <= 9)
                {
                    risk.RiskId = "G";
                    risk.reason = "Schizophrenia questionnaire assignment";
                }
                else if (totalScore >= 10 && totalScore <= 18)
                {
                    risk.RiskId = "Y";
                    risk.reason = "Schizophrenia questionnaire assignment";
                }
                else if (totalScore >= 19)
                {
                    risk.RiskId = "R";
                    risk.reason = "Schizophrenia questionnaire assignment";
                }

                _member.InsertRiskRating(risk);

                schizophrenia.RiskID = risk.id;
                var insert = _admin.InsertSchizophreniaResults(schizophrenia);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (schizophrenia.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Schizophrenia/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _BipolarQuestionnaire(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1125
        {
            var model = new MentalHealthVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult _BipolarQuestionnaire(MentalHealthVM model) //HCare-1125
        {
            if (Request.Query["bipolar-follow-up"].ToString().ToLower().Contains("true"))
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region bipolar-insert
                var bipolar = new MH_BipolarResponse();
                bipolar.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                bipolar.DependantID = dependantid;
                bipolar.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                bipolar.CreatedDate = DateTime.Now;
                bipolar.CreatedBy = User.Identity.Name;

                if (!String.IsNullOrEmpty(Request.Query["bipolar-depression-text"]))
                {
                    bipolar.Depression = Request.Query["bipolar-depression-text"];
                    bipolar.DepressionSC = Convert.ToInt32(Request.Query["bipolar-depression-score"]);
                }
                else { bipolar.Depression = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-insomnia-text"]))
                {
                    bipolar.Insomnia = Request.Query["bipolar-insomnia-text"];
                    bipolar.InsomniaSC = Convert.ToInt32(Request.Query["bipolar-insomnia-score"]);
                }
                else { bipolar.Insomnia = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-appetite-text"]))
                {
                    bipolar.Appetite = Request.Query["bipolar-appetite-text"];
                    bipolar.AppetiteSC = Convert.ToInt32(Request.Query["bipolar-appetite-score"]);
                }
                else { bipolar.Appetite = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-social-engagement-text"]))
                {
                    bipolar.SocialEngagement = Request.Query["bipolar-social-engagement-text"];
                    bipolar.SocialEngagementSC = Convert.ToInt32(Request.Query["bipolar-social-engagement-score"]);
                }
                else { bipolar.SocialEngagement = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-activity-text"]))
                {
                    bipolar.Activity = Request.Query["bipolar-activity-text"];
                    bipolar.ActivitySC = Convert.ToInt32(Request.Query["bipolar-activity-score"]);
                }
                else { bipolar.Activity = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-motivation-text"]))
                {
                    bipolar.Motivation = Request.Query["bipolar-motivation-text"];
                    bipolar.MotivationSC = Convert.ToInt32(Request.Query["bipolar-motivation-score"]);
                }
                else { bipolar.Motivation = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-concentration-text"]))
                {
                    bipolar.Concentration = Request.Query["bipolar-concentration-text"];
                    bipolar.ConcentrationSC = Convert.ToInt32(Request.Query["bipolar-concentration-score"]);
                }
                else { bipolar.Concentration = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-anxiety-text"]))
                {
                    bipolar.Anxiety = Request.Query["bipolar-anxiety-text"];
                    bipolar.AnxietySC = Convert.ToInt32(Request.Query["bipolar-anxiety-score"]);
                }
                else { bipolar.Anxiety = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-pleasure-text"]))
                {
                    bipolar.Pleasure = Request.Query["bipolar-pleasure-text"];
                    bipolar.PleasureSC = Convert.ToInt32(Request.Query["bipolar-pleasure-score"]);
                }
                else { bipolar.Pleasure = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-emotion-text"]))
                {
                    bipolar.Emotion = Request.Query["bipolar-emotion-text"];
                    bipolar.EmotionSC = Convert.ToInt32(Request.Query["bipolar-emotion-score"]);
                }
                else { bipolar.Emotion = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-worthlessness-text"]))
                {
                    bipolar.Worthlessness = Request.Query["bipolar-worthlessness-text"];
                    bipolar.WorthlessnessSC = Convert.ToInt32(Request.Query["bipolar-worthlessness-score"]);
                }
                else { bipolar.Worthlessness = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-helplessness-text"]))
                {
                    bipolar.Helplessness = Request.Query["bipolar-helplessness-text"];
                    bipolar.HelplessnessSC = Convert.ToInt32(Request.Query["bipolar-helplessness-score"]);
                }
                else { bipolar.Helplessness = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-suicide-text"]))
                {
                    bipolar.Suicidal = Request.Query["bipolar-suicide-text"];
                    bipolar.SuicidalSC = Convert.ToInt32(Request.Query["bipolar-suicide-score"]);
                }
                else { bipolar.Suicidal = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["MH_BipolarResponse.SuicidalCMT"]))
                {
                    bipolar.SuicidalCMT = Request.Query["MH_BipolarResponse.SuicidalCMT"];
                }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-guilt-text"]))
                {
                    bipolar.Guilt = Request.Query["bipolar-guilt-text"];
                    bipolar.GuiltSC = Convert.ToInt32(Request.Query["bipolar-guilt-score"]);
                }
                else { bipolar.Guilt = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-psychotic-text"]))
                {
                    bipolar.Psychotic = Request.Query["bipolar-psychotic-text"];
                    bipolar.PsychoticSC = Convert.ToInt32(Request.Query["bipolar-psychotic-score"]);
                }
                else { bipolar.Psychotic = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-irritability-text"]))
                {
                    bipolar.Irritability = Request.Query["bipolar-irritability-text"];
                    bipolar.IrritabilitySC = Convert.ToInt32(Request.Query["bipolar-irritability-score"]);
                }
                else { bipolar.Irritability = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-lability-text"]))
                {
                    bipolar.Lability = Request.Query["bipolar-lability-text"];
                    bipolar.LabilitySC = Convert.ToInt32(Request.Query["bipolar-lability-score"]);
                }
                else { bipolar.Lability = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-inc-motor-drive-text"]))
                {
                    bipolar.IncMotorDrive = Request.Query["bipolar-inc-motor-drive-text"];
                    bipolar.IncMotorDriveSC = Convert.ToInt32(Request.Query["bipolar-inc-motor-drive-score"]);
                }
                else { bipolar.IncMotorDrive = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-inc-speech-text"]))
                {
                    bipolar.IncSpeech = Request.Query["bipolar-inc-speech-text"];
                    bipolar.IncSpeechSC = Convert.ToInt32(Request.Query["bipolar-inc-speech-score"]);
                }
                else { bipolar.IncSpeech = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["bipolar-agitation-text"]))
                {
                    bipolar.Agitation = Request.Query["bipolar-agitation-text"];
                    bipolar.AgitationSC = Convert.ToInt32(Request.Query["bipolar-agitation-score"]);
                }
                else { bipolar.Agitation = "Follow up"; }

                bipolar.TotalScore = 0;
                bipolar.Outcome = "Follow up";
                bipolar.Comment = "Bipolar questionnaire assignment - Follow up";
                bipolar.Program = programcode.code;
                bipolar.FollowUp = true;
                bipolar.Active = true;
                #endregion
                #region risk-rating hcare-1151
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.RiskId = "-";
                risk.reason = "Bipolar questionnaire assignment - follow-up";
                risk.active = false;

                _member.InsertRiskRating(risk);

                bipolar.RiskID = risk.id;
                var insert = _admin.InsertBipolarResults(bipolar);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region follow-up-assignment
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = dependantid,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Mental health follow up required - Bipolar",
                        programId = programcode.programID,

                    },

                    assignmentItemType = "FUAQ",

                };
                var results = _member.InsertAssignment(assignment);

                var taskitem = new AssignmentItemTasks();
                taskitem.assignmentItemID = assignment.itemID;

                var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                foreach (var tas in taskss)
                {
                    taskitem.taskTypeId = tas.taskType;
                    taskitem.requirementId = tas.requirementId;
                    _member.InsertTask(taskitem);
                }
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (bipolar.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Bipolar/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }
            else
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);
                var totalScore = 0;

                #region bipolar-insert
                var bipolar = new MH_BipolarResponse();
                bipolar.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                bipolar.DependantID = dependantid;
                bipolar.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                bipolar.CreatedDate = DateTime.Now;
                bipolar.CreatedBy = User.Identity.Name;

                if (Request.Query["bipolar-depression-text"].ToString() != null)
                {
                    bipolar.Depression = Request.Query["bipolar-depression-text"];
                    bipolar.DepressionSC = Convert.ToInt32(Request.Query["bipolar-depression-score"]);
                    totalScore = bipolar.DepressionSC + totalScore;
                }
                if (Request.Query["bipolar-insomnia-text"].ToString() != null)
                {
                    bipolar.Insomnia = Request.Query["bipolar-insomnia-text"];
                    bipolar.InsomniaSC = Convert.ToInt32(Request.Query["bipolar-insomnia-score"]);
                    totalScore = bipolar.InsomniaSC + totalScore;
                }
                if (Request.Query["bipolar-appetite-text"].ToString() != null)
                {
                    bipolar.Appetite = Request.Query["bipolar-appetite-text"];
                    bipolar.AppetiteSC = Convert.ToInt32(Request.Query["bipolar-appetite-score"]);
                    totalScore = bipolar.AppetiteSC + totalScore;
                }
                if (Request.Query["bipolar-social-engagement-text"].ToString() != null)
                {
                    bipolar.SocialEngagement = Request.Query["bipolar-social-engagement-text"];
                    bipolar.SocialEngagementSC = Convert.ToInt32(Request.Query["bipolar-social-engagement-score"]);
                    totalScore = bipolar.SocialEngagementSC + totalScore;
                }
                if (Request.Query["bipolar-activity-text"].ToString() != null)
                {
                    bipolar.Activity = Request.Query["bipolar-activity-text"];
                    bipolar.ActivitySC = Convert.ToInt32(Request.Query["bipolar-activity-score"]);
                    totalScore = bipolar.ActivitySC + totalScore;
                }
                if (Request.Query["bipolar-motivation-text"].ToString() != null)
                {
                    bipolar.Motivation = Request.Query["bipolar-motivation-text"];
                    bipolar.MotivationSC = Convert.ToInt32(Request.Query["bipolar-motivation-score"]);
                    totalScore = bipolar.MotivationSC + totalScore;
                }
                if (Request.Query["bipolar-concentration-text"].ToString() != null)
                {
                    bipolar.Concentration = Request.Query["bipolar-concentration-text"];
                    bipolar.ConcentrationSC = Convert.ToInt32(Request.Query["bipolar-concentration-score"]);
                    totalScore = bipolar.ConcentrationSC + totalScore;
                }
                if (Request.Query["bipolar-anxiety-text"].ToString() != null)
                {
                    bipolar.Anxiety = Request.Query["bipolar-anxiety-text"];
                    bipolar.AnxietySC = Convert.ToInt32(Request.Query["bipolar-anxiety-score"]);
                    totalScore = bipolar.AnxietySC + totalScore;
                }
                if (Request.Query["bipolar-pleasure-text"].ToString() != null)
                {
                    bipolar.Pleasure = Request.Query["bipolar-pleasure-text"];
                    bipolar.PleasureSC = Convert.ToInt32(Request.Query["bipolar-pleasure-score"]);
                    totalScore = bipolar.PleasureSC + totalScore;
                }
                if (Request.Query["bipolar-emotion-text"].ToString() != null)
                {
                    bipolar.Emotion = Request.Query["bipolar-emotion-text"];
                    bipolar.EmotionSC = Convert.ToInt32(Request.Query["bipolar-emotion-score"]);
                    totalScore = bipolar.EmotionSC + totalScore;
                }
                if (Request.Query["bipolar-worthlessness-text"].ToString() != null)
                {
                    bipolar.Worthlessness = Request.Query["bipolar-worthlessness-text"];
                    bipolar.WorthlessnessSC = Convert.ToInt32(Request.Query["bipolar-worthlessness-score"]);
                    totalScore = bipolar.WorthlessnessSC + totalScore;
                }
                if (Request.Query["bipolar-helplessness-text"].ToString() != null)
                {
                    bipolar.Helplessness = Request.Query["bipolar-helplessness-text"];
                    bipolar.HelplessnessSC = Convert.ToInt32(Request.Query["bipolar-helplessness-score"]);
                    totalScore = bipolar.HelplessnessSC + totalScore;
                }
                if (Request.Query["bipolar-suicide-text"].ToString() != null)
                {
                    bipolar.Suicidal = Request.Query["bipolar-suicide-text"];
                    bipolar.SuicidalSC = Convert.ToInt32(Request.Query["bipolar-suicide-score"]);
                    totalScore = bipolar.SuicidalSC + totalScore;
                }
                if (Request.Query["MH_BipolarResponse.SuicidalCMT"].ToString() != null)
                {
                    bipolar.SuicidalCMT = Request.Query["MH_BipolarResponse.SuicidalCMT"];
                }
                if (Request.Query["bipolar-guilt-text"].ToString() != null)
                {
                    bipolar.Guilt = Request.Query["bipolar-guilt-text"];
                    bipolar.GuiltSC = Convert.ToInt32(Request.Query["bipolar-guilt-score"]);
                    totalScore = bipolar.GuiltSC + totalScore;
                }
                if (Request.Query["bipolar-psychotic-text"].ToString() != null)
                {
                    bipolar.Psychotic = Request.Query["bipolar-psychotic-text"];
                    bipolar.PsychoticSC = Convert.ToInt32(Request.Query["bipolar-psychotic-score"]);
                    totalScore = bipolar.PsychoticSC + totalScore;
                }
                if (Request.Query["bipolar-irritability-text"].ToString() != null)
                {
                    bipolar.Irritability = Request.Query["bipolar-irritability-text"];
                    bipolar.IrritabilitySC = Convert.ToInt32(Request.Query["bipolar-irritability-score"]);
                    totalScore = bipolar.IrritabilitySC + totalScore;
                }
                if (Request.Query["bipolar-lability-text"].ToString() != null)
                {
                    bipolar.Lability = Request.Query["bipolar-lability-text"];
                    bipolar.LabilitySC = Convert.ToInt32(Request.Query["bipolar-lability-score"]);
                    totalScore = bipolar.LabilitySC + totalScore;
                }
                if (Request.Query["bipolar-inc-motor-drive-text"].ToString() != null)
                {
                    bipolar.IncMotorDrive = Request.Query["bipolar-inc-motor-drive-text"];
                    bipolar.IncMotorDriveSC = Convert.ToInt32(Request.Query["bipolar-inc-motor-drive-score"]);
                    totalScore = bipolar.IncMotorDriveSC + totalScore;
                }
                if (Request.Query["bipolar-inc-speech-text"].ToString() != null)
                {
                    bipolar.IncSpeech = Request.Query["bipolar-inc-speech-text"];
                    bipolar.IncSpeechSC = Convert.ToInt32(Request.Query["bipolar-inc-speech-score"]);
                    totalScore = bipolar.IncSpeechSC + totalScore;
                }
                if (Request.Query["bipolar-agitation-text"].ToString() != null)
                {
                    bipolar.Agitation = Request.Query["bipolar-agitation-text"];
                    bipolar.AgitationSC = Convert.ToInt32(Request.Query["bipolar-agitation-score"]);
                    totalScore = bipolar.AgitationSC + totalScore;
                }

                bipolar.TotalScore = totalScore;
                #region outcome
                if (bipolar.SuicidalSC > 0)
                {
                    bipolar.Outcome = "Severe";
                }
                else if (totalScore < 1)
                {
                    bipolar.Outcome = "Normal";
                }
                else if (totalScore >= 1 && totalScore <= 20)
                {
                    bipolar.Outcome = "Mild";
                }
                else if (totalScore >= 21 && totalScore <= 40)
                {
                    bipolar.Outcome = "Moderate";
                }
                else if (totalScore >= 41 && totalScore <= 60)
                {
                    bipolar.Outcome = "Severe";
                }
                #endregion
                bipolar.Comment = "Bipolar questionnaire assignment";
                bipolar.Program = programcode.code;
                bipolar.Active = true;

                #endregion
                #region risk-rating
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.active = true;

                if (bipolar.SuicidalSC > 0)
                {
                    risk.RiskId = "R";
                    risk.reason = "Bipolar questionnaire assignment - Suicidal";
                }
                else if (totalScore <= 20)
                {
                    risk.RiskId = "G";
                    risk.reason = "Bipolar questionnaire assignment";
                }
                else if (totalScore >= 21 && totalScore <= 40)
                {
                    risk.RiskId = "Y";
                    risk.reason = "Bipolar questionnaire assignment";
                }
                else if (totalScore >= 41)
                {
                    risk.RiskId = "R";
                    risk.reason = "Bipolar questionnaire assignment";
                }

                _member.InsertRiskRating(risk);

                bipolar.RiskID = risk.id;
                var insert = _admin.InsertBipolarResults(bipolar);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (bipolar.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Bipolar/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }
            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _DepressionQuestionnaire(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1126
        {
            var model = new MentalHealthVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult _DepressionQuestionnaire(MentalHealthVM model) //HCare-1126
        {
            if (Request.Query["depression-follow-up"].ToString().ToLower().Contains("true"))
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region depression-insert
                var depression = new MH_DepressionResponse();
                depression.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                depression.DependantID = dependantid;
                depression.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                depression.CreatedDate = DateTime.Now;
                depression.CreatedBy = User.Identity.Name;

                if (!String.IsNullOrEmpty(Request.Query["depression-depression-text"]))
                {
                    depression.Depression = Request.Query["depression-depression-text"];
                    depression.DepressionSC = Convert.ToInt32(Request.Query["depression-depression-score"]);
                }
                else { depression.Depression = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-guilt-text"]))
                {
                    depression.Guilt = Request.Query["depression-guilt-text"];
                    depression.GuiltSC = Convert.ToInt32(Request.Query["depression-guilt-score"]);
                }
                else { depression.Guilt = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-suicide-text"]))
                {
                    depression.Suicidal = Request.Query["depression-suicide-text"];
                    depression.SuicidalSC = Convert.ToInt32(Request.Query["depression-suicide-score"]);
                }
                else { depression.Suicidal = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-insomnia-early-night-text"]))
                {
                    depression.InsomniaEarlyNight = Request.Query["depression-insomnia-early-night-text"];
                    depression.InsomniaEarlyNightSC = Convert.ToInt32(Request.Query["depression-insomnia-early-night-score"]);
                }
                else { depression.InsomniaEarlyNight = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-insomnia-middle-night-text"]))
                {
                    depression.InsomniaMiddleNight = Request.Query["depression-insomnia-middle-night-text"];
                    depression.InsomniaMiddleNightSC = Convert.ToInt32(Request.Query["depression-insomnia-middle-night-score"]);
                }
                else { depression.InsomniaMiddleNight = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-insomnia-early-morning-text"]))
                {
                    depression.InsomniaEarlyMorning = Request.Query["depression-insomnia-early-morning-text"];
                    depression.InsomniaEarlyMorningSC = Convert.ToInt32(Request.Query["depression-insomnia-early-morning-score"]);
                }
                else { depression.InsomniaEarlyMorning = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-activity-text"]))
                {
                    depression.Activity = Request.Query["depression-activity-text"];
                    depression.ActivitySC = Convert.ToInt32(Request.Query["depression-activity-score"]);
                }
                else { depression.Activity = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-psychomotor-text"]))
                {
                    depression.Psychomotor = Request.Query["depression-psychomotor-text"];
                    depression.PsychomotorSC = Convert.ToInt32(Request.Query["depression-psychomotor-score"]);
                }
                else { depression.Psychomotor = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-agitation-text"]))
                {
                    depression.Agitation = Request.Query["depression-agitation-text"];
                    depression.AgitationSC = Convert.ToInt32(Request.Query["depression-agitation-score"]);
                }
                else { depression.Agitation = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-anxiety-psychic-text"]))
                {
                    depression.AnxietyPsychic = Request.Query["depression-anxiety-psychic-text"];
                    depression.AnxietyPsychicSC = Convert.ToInt32(Request.Query["depression-anxiety-psychic-score"]);
                }
                else { depression.AnxietyPsychic = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-anxiety-somatic-text"]))
                {
                    depression.AnxietySomatic = Request.Query["depression-anxiety-somatic-text"];
                    depression.AnxietySomaticSC = Convert.ToInt32(Request.Query["depression-anxiety-somatic-score"]);
                }
                else { depression.AnxietySomatic = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-somatic-sym-gastro-text"]))
                {
                    depression.SomaticSymGastro = Request.Query["depression-somatic-sym-gastro-text"];
                    depression.SomaticSymGastroSC = Convert.ToInt32(Request.Query["depression-somatic-sym-gastro-score"]);
                }
                else { depression.SomaticSymGastro = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-somatic-sym-general-text"]))
                {
                    depression.SomaticSymGeneral = Request.Query["depression-somatic-sym-general-text"];
                    depression.SomaticSymGeneralSC = Convert.ToInt32(Request.Query["depression-somatic-sym-general-score"]);
                }
                else { depression.SomaticSymGeneral = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-sexology-text"]))
                {
                    depression.Sexology = Request.Query["depression-sexology-text"];
                    depression.SexologySC = Convert.ToInt32(Request.Query["depression-sexology-score"]);
                }
                else { depression.Sexology = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-hypochondriasis-text"]))
                {
                    depression.Hypochondriasis = Request.Query["depression-hypochondriasis-text"];
                    depression.HypochondriasisSC = Convert.ToInt32(Request.Query["depression-hypochondriasis-score"]);
                }
                else { depression.Hypochondriasis = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-weightloss-text"]))
                {
                    depression.WeightLoss = Request.Query["depression-weightloss-text"];
                    depression.WeightLossSC = Convert.ToInt32(Request.Query["depression-weightloss-score"]);
                }
                else { depression.WeightLoss = "Follow up"; }

                if (!String.IsNullOrEmpty(Request.Query["depression-insight-text"]))
                {
                    depression.Insight = Request.Query["depression-insight-text"];
                    depression.InsightSC = Convert.ToInt32(Request.Query["depression-insight-score"]);
                }
                else { depression.Insight = "Follow up"; }

                depression.TotalScore = 0;
                depression.Outcome = "Follow up";
                depression.Comment = "Depression questionnaire assignment - Follow up";
                depression.Program = programcode.code;
                depression.FollowUp = true;
                depression.Active = true;
                #endregion
                #region risk-rating hcare-1151
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.RiskId = "-";
                risk.reason = "Depression questionnaire assignment - follow-up";
                risk.active = false;

                _member.InsertRiskRating(risk);

                depression.RiskID = risk.id;
                var insert = _admin.InsertDepressionResults(depression);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region follow-up-assignment
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = dependantid,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Mental health follow up required - Depression",
                        programId = programcode.programID,

                    },

                    assignmentItemType = "FUAQ",

                };
                var results = _member.InsertAssignment(assignment);

                var taskitem = new AssignmentItemTasks();
                taskitem.assignmentItemID = assignment.itemID;

                var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                foreach (var tas in taskss)
                {
                    taskitem.taskTypeId = tas.taskType;
                    taskitem.requirementId = tas.requirementId;
                    _member.InsertTask(taskitem);
                }
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (depression.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Depression/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }
            else
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);
                var totalScore = 0;

                #region depression-insert
                var depression = new MH_DepressionResponse();
                depression.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                depression.DependantID = dependantid;
                depression.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                depression.CreatedDate = DateTime.Now;
                depression.CreatedBy = User.Identity.Name;

                if (Request.Query["depression-depression-text"].ToString() != null)
                {
                    depression.Depression = Request.Query["depression-depression-text"];
                    depression.DepressionSC = Convert.ToInt32(Request.Query["depression-depression-score"]);
                    totalScore = depression.DepressionSC + totalScore;
                }
                if (Request.Query["depression-guilt-text"].ToString() != null)
                {
                    depression.Guilt = Request.Query["depression-guilt-text"];
                    depression.GuiltSC = Convert.ToInt32(Request.Query["depression-guilt-score"]);
                    totalScore = depression.GuiltSC + totalScore;
                }
                if (Request.Query["depression-suicide-text"].ToString() != null)
                {
                    depression.Suicidal = Request.Query["depression-suicide-text"];
                    depression.SuicidalSC = Convert.ToInt32(Request.Query["depression-suicide-score"]);
                    totalScore = depression.SuicidalSC + totalScore;
                }
                if (Request.Query["depression-insomnia-early-night-text"].ToString() != null)
                {
                    depression.InsomniaEarlyNight = Request.Query["depression-insomnia-early-night-text"];
                    depression.InsomniaEarlyNightSC = Convert.ToInt32(Request.Query["depression-insomnia-early-night-score"]);
                    totalScore = depression.InsomniaEarlyNightSC + totalScore;
                }
                if (Request.Query["depression-insomnia-middle-night-text"].ToString() != null)
                {
                    depression.InsomniaMiddleNight = Request.Query["depression-insomnia-middle-night-text"];
                    depression.InsomniaMiddleNightSC = Convert.ToInt32(Request.Query["depression-insomnia-middle-night-score"]);
                    totalScore = depression.InsomniaMiddleNightSC + totalScore;
                }
                if (Request.Query["depression-insomnia-early-morning-text"].ToString() != null)
                {
                    depression.InsomniaEarlyMorning = Request.Query["depression-insomnia-early-morning-text"];
                    depression.InsomniaEarlyMorningSC = Convert.ToInt32(Request.Query["depression-insomnia-early-morning-score"]);
                    totalScore = depression.InsomniaEarlyMorningSC + totalScore;
                }
                if (Request.Query["depression-activity-text"].ToString() != null)
                {
                    depression.Activity = Request.Query["depression-activity-text"];
                    depression.ActivitySC = Convert.ToInt32(Request.Query["depression-activity-score"]);
                    totalScore = depression.ActivitySC + totalScore;
                }
                if (Request.Query["depression-psychomotor-text"].ToString() != null)
                {
                    depression.Psychomotor = Request.Query["depression-psychomotor-text"];
                    depression.PsychomotorSC = Convert.ToInt32(Request.Query["depression-psychomotor-score"]);
                    totalScore = depression.PsychomotorSC + totalScore;
                }
                if (Request.Query["depression-agitation-text"].ToString() != null)
                {
                    depression.Agitation = Request.Query["depression-agitation-text"];
                    depression.AgitationSC = Convert.ToInt32(Request.Query["depression-agitation-score"]);
                    totalScore = depression.AgitationSC + totalScore;
                }
                if (Request.Query["depression-anxiety-psychic-text"].ToString() != null)
                {
                    depression.AnxietyPsychic = Request.Query["depression-anxiety-psychic-text"];
                    depression.AnxietyPsychicSC = Convert.ToInt32(Request.Query["depression-anxiety-psychic-score"]);
                    totalScore = depression.AnxietyPsychicSC + totalScore;
                }
                if (Request.Query["depression-anxiety-somatic-text"].ToString() != null)
                {
                    depression.AnxietySomatic = Request.Query["depression-anxiety-somatic-text"];
                    depression.AnxietySomaticSC = Convert.ToInt32(Request.Query["depression-anxiety-somatic-score"]);
                    totalScore = depression.AnxietySomaticSC + totalScore;
                }
                if (Request.Query["depression-somatic-sym-gastro-text"].ToString() != null)
                {
                    depression.SomaticSymGastro = Request.Query["depression-somatic-sym-gastro-text"];
                    depression.SomaticSymGastroSC = Convert.ToInt32(Request.Query["depression-somatic-sym-gastro-score"]);
                    totalScore = depression.SomaticSymGastroSC + totalScore;
                }
                if (Request.Query["depression-somatic-sym-general-text"].ToString() != null)
                {
                    depression.SomaticSymGeneral = Request.Query["depression-somatic-sym-general-text"];
                    depression.SomaticSymGeneralSC = Convert.ToInt32(Request.Query["depression-somatic-sym-general-score"]);
                    totalScore = depression.SomaticSymGeneralSC + totalScore;
                }
                if (Request.Query["depression-sexology-text"].ToString() != null)
                {
                    depression.Sexology = Request.Query["depression-sexology-text"];
                    depression.SexologySC = Convert.ToInt32(Request.Query["depression-sexology-score"]);
                    totalScore = depression.SexologySC + totalScore;
                }
                if (Request.Query["depression-hypochondriasis-text"].ToString() != null)
                {
                    depression.Hypochondriasis = Request.Query["depression-hypochondriasis-text"];
                    depression.HypochondriasisSC = Convert.ToInt32(Request.Query["depression-hypochondriasis-score"]);
                    totalScore = depression.HypochondriasisSC + totalScore;
                }
                if (Request.Query["depression-weightloss-text"].ToString() != null)
                {
                    depression.WeightLoss = Request.Query["depression-weightloss-text"];
                    depression.WeightLossSC = Convert.ToInt32(Request.Query["depression-weightloss-score"]);
                    totalScore = depression.WeightLossSC + totalScore;
                }
                if (Request.Query["depression-insight-text"].ToString() != null)
                {
                    depression.Insight = Request.Query["depression-insight-text"];
                    depression.InsightSC = Convert.ToInt32(Request.Query["depression-insight-score"]);
                    totalScore = depression.InsightSC + totalScore;
                }

                depression.TotalScore = totalScore;
                #region outcome
                if (depression.SuicidalSC > 0)
                {
                    depression.Outcome = "Severe";
                }
                else if (totalScore < 8)
                {
                    depression.Outcome = "Clinical remission - Normal";
                }
                else if (totalScore >= 8 && totalScore <= 13)
                {
                    depression.Outcome = "Mild depression";
                }
                else if (totalScore >= 14 && totalScore <= 18)
                {
                    depression.Outcome = "Moderate depression";
                }
                else if (totalScore >= 19 && totalScore <= 23)
                {
                    depression.Outcome = "Severe depression";
                }
                else if (totalScore > 23)
                {
                    depression.Outcome = "Very severe depression – urgent referral";
                }
                #endregion
                depression.Comment = "Depression questionnaire assignment";
                depression.Program = programcode.code;
                depression.Active = true;

                #endregion
                #region risk-rating
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = programcode.code;
                risk.active = true;

                if (depression.SuicidalSC > 0)
                {
                    risk.RiskId = "R";
                    risk.reason = "Depression questionnaire assignment - Suicidal";
                }
                else if (totalScore < 14)
                {
                    risk.RiskId = "G";
                    risk.reason = "Depression  questionnaire assignment";
                }
                else if (totalScore >= 14 && totalScore <= 18)
                {
                    risk.RiskId = "Y";
                    risk.reason = "Depression  questionnaire assignment";
                }
                else if (totalScore >= 19)
                {
                    risk.RiskId = "R";
                    risk.reason = "Depression  questionnaire assignment";
                }

                _member.InsertRiskRating(risk);

                depression.RiskID = risk.id;
                var insert = _admin.InsertDepressionResults(depression);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region dr-referral-assignmnet hcare-1137
                if (depression.SuicidalSC > 0)
                {
                    var DRA_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Refer to Health practitioner: Depression/Suicidal",
                            programId = programcode.programID,

                        },

                        assignmentItemType = "DREF",
                    };
                    var DRA_Insert = _member.InsertAssignment(DRA_assignment);

                    var DRA_taskitem = new AssignmentItemTasks();
                    DRA_taskitem.assignmentItemID = DRA_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(DRA_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        DRA_taskitem.taskTypeId = xtask.taskType;
                        DRA_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(DRA_taskitem);
                    }
                }
                #endregion
                #region case-manager-assignmnet hcare-1176
                if (risk.RiskId.ToUpper() == "R")
                {
                    var CM_assignment = new AssignmentsView()
                    {
                        newAssignment = new Assignments()
                        {
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                            dependentID = dependantid,
                            Active = true,
                            effectiveDate = DateTime.Now,
                            assignmentType = "INTER",
                            status = "Open",
                            Instruction = "Risk rating: Red - Assign case manager",
                            programId = programcode.programID,
                        },

                        assignmentItemType = "CMAN",
                    };
                    var CM_Insert = _member.InsertAssignment(CM_assignment);

                    var CM_taskitem = new AssignmentItemTasks();
                    CM_taskitem.assignmentItemID = CM_assignment.itemID;

                    var DRA_task = _member.GetTaskRequirements(CM_assignment.assignmentItemType);

                    foreach (var xtask in DRA_task)
                    {
                        CM_taskitem.taskTypeId = xtask.taskType;
                        CM_taskitem.requirementId = xtask.requirementId;
                        _member.InsertTask(CM_taskitem);
                    }
                }
                #endregion
            }
            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _DoctorReferral(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1136
        {
            var model = new MentalHealthVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;

            model.DoctorReferrals = new List<DoctorReferral>();
            model.DoctorReferrals = _member.GetDoctorReferralResults(DependentID);

            //HCare-1175
            ViewBag.referalID = new SelectList(_member.getReferralByDepandent(DependentID).ToString().Split(',').ToList());

            return View(model);
        }

        [HttpPost]
        public ActionResult _DoctorReferral(MentalHealthVM model) //HCare-1136
        {
            if (Request.Query["dr-referral-follow-up"].ToString().ToLower().Contains("true"))
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region drReferral-insert
                var drReferral = new DoctorReferral();
                drReferral.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                drReferral.DependantID = dependantid;
                drReferral.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                drReferral.CreatedDate = DateTime.Now;
                drReferral.CreatedBy = User.Identity.Name;

                //HCare-1175
                if (Request.Query["referalID"].ToString() != null) { drReferral.Referral = Request.Query["referalID"].ToString(); }

                if (!String.IsNullOrEmpty(Request.Query["referral-date"]))
                {
                    drReferral.ReferralDate = Convert.ToDateTime(Request.Query["referral-date"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["DoctorReferral.Comment"]))
                {
                    drReferral.Comment = Request.Query["DoctorReferral.Comment"];
                }

                drReferral.Program = programcode.code;
                drReferral.FollowUp = true;
                drReferral.Active = true;

                var insert = _member.InsertDoctorReferral(drReferral);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
                #region follow-up-assignment
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = dependantid,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Follow up required - Doctor Referral",
                        programId = new Guid(Request.Query["pro"])
                    },

                    assignmentItemType = "FUAQ",

                };
                var results = _member.InsertAssignment(assignment);

                var taskitem = new AssignmentItemTasks();
                taskitem.assignmentItemID = assignment.itemID;

                var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

                foreach (var tas in taskss)
                {
                    taskitem.taskTypeId = tas.taskType;
                    taskitem.requirementId = tas.requirementId;
                    _member.InsertTask(taskitem);
                }
                #endregion
            }
            else
            {
                var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
                var dependantid = new Guid(Request.Query["DependantID"]);

                #region drReferral-insert
                var drReferral = new DoctorReferral();
                drReferral.TaskID = Convert.ToInt32(Request.Query["taskId"]);
                drReferral.DependantID = dependantid;
                drReferral.AssignmentID = Convert.ToInt32(Request.Query["id"]);
                drReferral.CreatedDate = DateTime.Now;
                drReferral.CreatedBy = User.Identity.Name;

                //HCare-1175
                if (Request.Query["referalID"].ToString() != null) { drReferral.Referral = Request.Query["referalID"].ToString(); }

                if (!String.IsNullOrEmpty(Request.Query["referral-date"]))
                {
                    drReferral.ReferralDate = Convert.ToDateTime(Request.Query["referral-date"]);
                }
                if (!String.IsNullOrEmpty(Request.Query["DoctorReferral.Comment"]))
                {
                    drReferral.Comment = Request.Query["DoctorReferral.Comment"];
                }

                drReferral.Program = programcode.code;
                drReferral.Active = true;

                var insert = _member.InsertDoctorReferral(drReferral);
                #endregion
                #region task-update
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                task.closed = true;
                task.closedBy = User.Identity.Name;
                task.closedDate = DateTime.Now;
                task.status = "Closed";
                _clinical.UpdateTask(task);
                #endregion
            }
            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _AllocateCaseManager(Guid DependentID, int id, int taskId, string templateID, Guid? pro) //HCare-1176
        {
            var model = new CaseManagerVM();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.templateID = templateID;
            ViewBag.DependantID = DependentID;
            ViewBag.pro = pro;
            var program = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            var dependant = _member.GetDependentDetails(DependentID, pro);

            //ViewBag.Users = new SelectList(_admin.GetUsersList(), "userID", "userFullName");
            ViewBag.Users = new SelectList(_admin.GetAllowedUsersList(dependant.medicalAid.MedicalAidID, program.programID).Where(x => x.Active == true), "userID", "userFullName"); //HCare-1176

            model.UserMemberHistories = new List<UserMemberHistory>();
            model.UserMemberHistories = _admin.GetUserMemberHistory(DependentID, program.programID);

            return View(model);
        }

        [HttpPost]
        public ActionResult _AllocateCaseManager(CaseManagerVM model) //HCare-1176
        {
            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);

            #region cm-insert
            var cm = new UserMemberHistory();
            cm.DependantID = dependantid;
            cm.TaskID = Convert.ToInt32(Request.Query["taskId"]);
            cm.AssignmentID = Convert.ToInt32(Request.Query["id"]);
            cm.EffectiveDate = DateTime.Now;
            cm.CreatedDate = DateTime.Now;
            cm.CreatedBy = User.Identity.Name;
            cm.Program = programcode.code;

            if (!String.IsNullOrEmpty(Request.Query["Users"]))
            {
                cm.UserID = new Guid(Request.Query["Users"]);
                var user = _admin.GetUserById(cm.UserID);
                cm.UserFullName = user.userFullName;
            }
            if (!String.IsNullOrEmpty(Request.Query["UserMemberHistory.Comment"]))
            {
                cm.Comment = Request.Query["UserMemberHistory.Comment"];
            }

            cm.Active = true;

            var insert = _admin.InsertUserMemberHistory(cm);
            #endregion
            #region task-update
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";
            _clinical.UpdateTask(task);
            #endregion

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = Request.Query["pro"] });
        }

        public ActionResult _DoctorEmailAssignment(Guid DependentID, int id, int taskId, Guid? pro)
        {
            var model = new ComsViewModel();
            model.emailMessages = new Emails();

            ViewBag.task = taskId;
            ViewBag.id = id;
            ViewBag.pro = pro; //hcare-1289

            model.dependant = _member.GetDependantDetails(DependentID, null);

            var contacts = _member.GetContact(DependentID);

            //if (contacts != null) { model.emailMessages.emailTo = contacts.email; }
            var task = _clinical.GetTaskView(taskId);
            if (!String.IsNullOrEmpty(task.templateID)) { model.emailMessages.emailMassage = _clinical.GetEmailTemplate(Convert.ToInt16(task.templateID)); }
            model.emailMessages.dependantID = DependentID;
            model.emailMessages.effectivedate = DateTime.Now;

            //HCare-1197
            var det = _member.GetMemberByDependantID(DependentID);
            ViewBag.schemeSubject = _medical.GetMedicalAid(det.medicalAidID).medicalAidSettings.schemeSubject;
            var emailsubjectView = _member.GetDependentDetails(DependentID, pro);
            ViewBag.MemberNo = emailsubjectView.member.membershipNo;
            ViewBag.Dep = emailsubjectView.dependent.dependentCode;
            ViewBag.Surname = emailsubjectView.dependent.lastName;
            ViewBag.Intial = emailsubjectView.dependent.initials;

            #region hcare-1380
            var emailTemplates = _member.GetEmailMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//HCare-1098
            ViewBag.Templates = new SelectList(emailTemplates, "templateId", "title");

            model.EmailHistory = _member.GetEmails(DependentID, pro);
            model.AttachmentTemplates = _admin.GetAttachmentTemplateByAccount(det.medicalAidID, new Guid(pro.ToString())); //HCare-1380
            model.EmailAttachmentHistories = _admin.GetEmailAttachmentByDependantID(DependentID, new Guid(pro.ToString()), DateTime.Today); //HCare-1380

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID); //HCare-1380
            var program = _admin.GetProgramById(new Guid(pro.ToString())); //HCare-1380
            ViewBag.DependantID = DependentID; //HCare-1380
            ViewBag.MedicalAid = medicalaid.Name; //HCare-1380
            ViewBag.MedicalAidID = medicalaid.MedicalAidID; //hcare-1380
            ViewBag.ProgramID = program.programID; //HCare-1380
            ViewBag.Program = program.ProgramName; //HCare-1380
            ViewBag.UserID = User.Identity.Name; //hcare-1380
            ViewBag.MemberName = model.dependant.firstName_UC + " " + model.dependant.lastName_UC; //hcare-1384

            model.EmailHeader = _admin.GetEmailLayoutByAccount(det.medicalAidID, new Guid(pro.ToString())).Where(x => x.LayoutType.ToLower() == "header").ToList(); //hcare-1384
            model.EmailFooter = _admin.GetEmailLayoutByAccount(det.medicalAidID, new Guid(pro.ToString())).Where(x => x.LayoutType.ToLower() == "footer").ToList(); //hcare-1384

            if (model.EmailHeader.Count != 0) { ViewBag.Header = model.EmailHeader[0].Id; } else { ViewBag.Header = null; } //hcare-1384
            if (model.EmailFooter.Count != 0) { ViewBag.Footer = model.EmailFooter[0].Id; } else { ViewBag.Footer = null; } //hcare-1384
            #endregion

            #region hcare-1391
            var doctorinformation = _member.GetPatientDoctorHistory(DependentID); //hcare-1391
            if (doctorinformation != null)
            {
                var doctor = _admin.GetDoctorByDoctorID(doctorinformation.doctorId); //hcare-1391
                ViewBag.DoctorEmail = doctor.EmailAddress; //hcare-1391
                ViewBag.DoctorName = doctor.DoctorName; //hcare-1391
                ViewBag.DoctorPractice = doctor.PracticeNumber; //hcare-1391
            }
            #endregion

            return View(model);
        }

        [HttpPost]
        
        public ActionResult _DoctorEmailAssignment(ComsViewModel model, Guid? pro)
        {
            var dependantID = Request.Query["e2-dependantid"];
            var programID = Request.Query["e2-programid"];
            var medicalaidID = Request.Query["e2-medicalaidid"];

            var emailhistory = _admin.GetEmailAttachmentByDependantID(new Guid(dependantID), new Guid(programID), DateTime.Today);

            #region email-insert
            model.emailMessages.emailTo = Request.Query["doctor-email-to"];
            model.emailMessages.cc = Request.Query["doctor-email-cc"];
            model.emailMessages.createdDate = DateTime.Now;
            model.emailMessages.effectivedate = DateTime.Now;
            model.emailMessages.createdBy = User.Identity.Name;
            model.emailMessages.programId = new Guid(pro.ToString()); //HCare-1254
            model.emailMessages.status = "Pending";

            _member.InsertEmail(model.emailMessages);

            #region hcare-1384: email-layouts
            string root = AppDomain.CurrentDomain.BaseDirectory + "uploads//templates//layout//";

            string header = null;
            string footer = null;
            string h_dimensions = null;
            string f_dimensions = null;

            string header_image = "default-space.jpg";
            string header_height = "0";
            string header_width = "0";

            string footer_image = "default-space.jpg";
            string footer_height = "0";
            string footer_width = "0";

            var layoutinformation = _admin.GetEmailLayoutByAccount(new Guid(medicalaidID), new Guid(programID)); //hcare-1384

            foreach (var item in layoutinformation)
            {
                if (item.LayoutType.ToLower().Contains("header"))
                {
                    header_image = item.FileName;
                    header_height = item.LayoutHeight;
                    header_width = item.LayoutWidth;
                    break;
                }
            }
            foreach (var item in layoutinformation)
            {
                if (item.LayoutType.ToLower().Contains("footer"))
                {
                    footer_image = item.FileName;
                    footer_height = item.LayoutHeight;
                    footer_width = item.LayoutWidth;
                    break;
                }
            }

            header = root + header_image;
            h_dimensions = "height='" + header_height + "', width='" + header_width + "'";
            footer = root + footer_image;
            f_dimensions = "height='" + footer_height + "', width='" + footer_width + "'";
            #endregion

            #endregion
            #region email-attachment-insert
            if (emailhistory.Count > 0)
            {
                var attachments = "";
                foreach (var item in emailhistory)
                {
                    var attachment = _admin.GetAttachmentTemplateByID(item.TemplateID);
                    attachments += attachment.FileName + ',';

                    item.ModifiedBy = User.Identity.Name;
                    item.ModifiedDate = DateTime.Now;
                    item.EmailID = model.emailMessages.emailId;
                    item.Sent = true;

                    _admin.UpdateEmailAttachmentHistory(item);

                }
                attachments = attachments.TrimEnd(',');

                var emailsend = SendEmail(model.emailMessages.emailTo, model.emailMessages.cc, model.emailMessages.subject, header, h_dimensions, model.emailMessages.emailMassage, footer, f_dimensions, attachments, medicalaidID);
                #region hcare-1448: update-email
                var email = _member.GetMemberEmailByID(model.emailMessages.emailId);
                if (emailsend.ToLower() == "sent")
                {
                    if (email != null)
                    {
                        email.status = "Sent";
                        _member.UpdateEmailStatus(email);
                    }
                }
                else
                {
                    email.status = "Error: " + emailsend;
                    _member.UpdateEmailStatus(email);
                }
                #endregion
            }
            else
            {
                var emailsend = SendEmail(model.emailMessages.emailTo, model.emailMessages.cc, model.emailMessages.subject, header, h_dimensions, model.emailMessages.emailMassage, footer, f_dimensions, "", medicalaidID);
                #region hcare-1448: update-email
                var email = _member.GetMemberEmailByID(model.emailMessages.emailId);
                if (emailsend.ToLower() == "sent")
                {
                    if (email != null)
                    {
                        email.status = "Sent";
                        _member.UpdateEmailStatus(email);
                    }
                }
                else
                {
                    email.status = "Error: " + emailsend;
                    _member.UpdateEmailStatus(email);
                }
                #endregion
            }
            #endregion

            //update-task
            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"], pro = pro }); //hcare-1289
        }

        public ActionResult _WelcomeText(Guid DependentID, int id, int taskId, Guid? pro) //hcare-1445
        {
            var model = new ComsViewModel();
            model.smsMessages = new SmsMessages();

            var contacts = _member.GetContact(DependentID);
            if (contacts != null) { if (string.IsNullOrEmpty(contacts.cell)) { model.smsMessages.cell_no = contacts.ISeriesCell; } else { model.smsMessages.cell_no = contacts.cell; } }

            model.smsMessages.dependantID = DependentID;
            model.smsMessages.effectiveDate = DateTime.Now;

            //HCare-1043
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var det = _member.GetMemberByDependantID(DependentID);

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID); //HCare-1380
            var program = _admin.GetProgramById(new Guid(pro.ToString())); //HCare-1380
            ViewBag.ReferenceID = id;
            ViewBag.TaskID = taskId;
            ViewBag.MedicalAid = medicalaid.Name; //HCare-1380
            ViewBag.ProgramID = program.programID; //HCare-1380
            ViewBag.Program = program.ProgramName; //HCare-1380
            ViewBag.UserID = User.Identity.Name; //hcare-1380

            var templates = _member.GetSmsMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//hcare-1380
            ViewBag.smsTemplate = new SelectList(templates, "templateId", "title"); //hcare-1380

            return View(model);
        }
        [HttpPost]
        public ActionResult _WelcomeText(ComsViewModel model) //hcare-1445
        {
            model.smsMessages.programId = new Guid(Request.Query["w-program-id"]); //hcare-1378
            model.smsMessages.templateID = Convert.ToInt16(Request.Query["w-sms-template"]); //hcare-1289
            model.smsMessages.createdBy = User.Identity.Name;
            model.smsMessages.createdDate = DateTime.Now;
            model.smsMessages.message = Request.Query["w-sms-text-message"];
            model.smsMessages.status = "Queued";
            if (String.IsNullOrEmpty(Request.Query["smsMessages.effectiveDate"])) { model.smsMessages.effectiveDate = DateTime.Now; } else { model.smsMessages.effectiveDate = Convert.ToDateTime(Request.Query["smsMessages.effectiveDate"]); }
            model.smsMessages.ReferenceNumber = Request.Query["w-reference-id"];
            model.smsMessages.Active = true;

            _member.InsertSMS(model.smsMessages);
            //_member.InsertText_HCDWL(model.dependantID.ToString(), model.cell_no, model.effectiveDate.ToString("dd MMM yyyy hh:mm:ss"), model.templateID, model.message, model.status, model.ReferenceNumber, model.programId.ToString(), model.createdBy, model.createdDate.ToString("dd MMM yyyy hh:mm:ss"));

            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["w-task-id"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";

            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["w-reference-id"], pro = Request.Query["w-program-id"] });
        }
        [HttpPost]
        public ActionResult _QuestionnairePregnant(PregnantVM model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["program-id"])).FirstOrDefault();
            var questions = _admin.GetDisclaimerQuestions();
            var acknowledgement = _admin.GetAcknowledgementQuestions();
            var dependantid = new Guid(Request.Query["dependant-id"]);

            model.Pregnant = new QuestionnaireOther();
            model.Pregnant.dependentID = new Guid(Request.Query["dependant-id"]);
            model.Pregnant.questionnaireID = new Guid(Request.Query["questionnaire-id"]);
            model.Pregnant.QuestionnaireOtherID = Convert.ToInt32(Request.Query["unique-id"]);
            model.Pregnant.createdBy = User.Identity.Name;
            model.Pregnant.createdDate = DateTime.Now;

            #region pregnant
            if (!string.IsNullOrEmpty(Request.Query["Pregnant.EDD"].ToString()))
            {
                model.Pregnant.Pregnant = true;
                if (!string.IsNullOrEmpty(Request.Query["Pregnant.EDD"].ToString())) { model.Pregnant.EDD = Convert.ToDateTime(Request.Query["Pregnant.EDD"]); }
                if (!string.IsNullOrEmpty(Request.Query["Pregnant.TreatingDoctor"].ToString())) { model.Pregnant.TreatingDoctor = Request.Query["Pregnant.TreatingDoctor"]; }
                if (Request.Query["doctor-aware"].ToString().ToLower().Contains("yes")) { model.Pregnant.isDoctorAware = true; } else { model.Pregnant.isDoctorAware = false; }
            }
            #endregion

            #region other
            model.Pregnant.occupation = null;
            model.Pregnant.shiftWorker = null;
            model.Pregnant.recDrugs = false;
            model.Pregnant.recDrugsLastUsed = null;
            model.Pregnant.TBScreen = false;
            model.Pregnant.TBScreenDate = null;
            model.Pregnant.TBScreenResult = null;
            model.Pregnant.tbDiagnosed = false;
            model.Pregnant.tbTreatmentStartDate = null;
            model.Pregnant.tbTreatmentEndDate = null;
            model.Pregnant.frailCare = null;
            model.Pregnant.frailCareCheck = false;
            model.Pregnant.frailCareComment = null;
            model.Pregnant.bloodTestFrequency = null;
            model.Pregnant.bloodTestEffectiveDate = null;
            model.Pregnant.followUp = false;
            model.Pregnant.generalComments = null;
            model.Pregnant.programType = program.code;
            model.Pregnant.active = true;
            #endregion

            _member.InsertQuestionnaireOtherResults(model.Pregnant);

            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["task-id"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";
            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["unique-id"], pro = Request.Query["program-id"] });
        }
        [HttpGet]
        public ActionResult _QuestionnairePregnant(Guid DependentID, int id, int taskId, string templateID, Guid? pro)
        {
            var model = new PregnantVM();

            var questionnaireID = Guid.NewGuid();
            ViewBag.questionnaireID = questionnaireID;
            ViewBag.dependantID = DependentID;
            ViewBag.templateID = templateID;
            ViewBag.programID = pro;
            ViewBag.uniqueID = id;
            ViewBag.taskID = taskId;

            model.Dependant = _member.GetDependantByDependantID(DependentID);
            model.Pregnancy = _member.GetPregnancyResults(DependentID);

            return View(model);
        }

        #region OLD_RemoveLater
        //[HttpPost]
        //public ActionResult _DiabetesQuestionnaire_OLD(DoctorQuestionnaireViewModel model)
        //{

        //    //=========================================================================================================================================================================//
        //    //                                                                            CoMorbid Conditions                                                                          // 
        //    //=========================================================================================================================================================================//

        //    model.comormidConditions = new List<CoMormidConditions>();//HCare-607

        //    for (int i = 0; i < 10; i++)
        //    {
        //        if (!String.IsNullOrEmpty(Request.Query["cmCondition_diab" + i]))
        //        {
        //            var comorbid = new CoMormidConditions();
        //            comorbid.id = Convert.ToInt32(Request.Query["id" + i]);
        //            comorbid.coMorbidId = Convert.ToInt32(Request.Query["cmCondition_diab" + i]);
        //            comorbid.dependantID = new Guid(Request.Query["DependantID"]);
        //            comorbid.createdDate = DateTime.Now;
        //            comorbid.createdBy = User.Identity.Name;
        //            comorbid.programType = "Diabetes";
        //            comorbid.generalComments = (Request.Query["comormidCondition.generalComments"]);
        //            comorbid.active = true;

        //            if (!String.IsNullOrEmpty(Request.Query["cmDateDiagnosed_diab" + i]))
        //            {
        //                comorbid.effectiveDate = Convert.ToDateTime(Request.Query["cmDateDiagnosed_diab" + i]);
        //            }
        //            if (!String.IsNullOrEmpty(Request.Query["cmTreatement_diab" + i]))
        //            {
        //                comorbid.treatementEndDate = Convert.ToDateTime(Request.Query["cmTreatement_diab" + i]);
        //            }

        //            model.comormidConditions.Add(comorbid);
        //        }
        //        else
        //            continue;
        //    }
        //    if (Request.Query["CoMorb_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.comormidConditions.Add(new CoMormidConditions()
        //        {
        //            coMorbidId = 1780,
        //            effectiveDate = null,
        //            treatementEndDate = null,
        //            dependantID = new Guid(Request.Query["DependantID"]),
        //            createdDate = DateTime.Now,
        //            createdBy = User.Identity.Name,
        //            programType = "Diabetes",
        //            generalComments = (Request.Query["comormidCondition.generalComments"]),
        //            followUp = true,
        //            active = true,

        //        });
        //    }
        //    else if (!Request.Query["ComorbidView.hasCoMorbid"].ToString().ToLower().Contains("true"))
        //    {
        //        model.comormidConditions.Add(new CoMormidConditions()
        //        {
        //            coMorbidId = 1781,
        //            effectiveDate = null,
        //            treatementEndDate = null,
        //            dependantID = new Guid(Request.Query["DependantID"]),
        //            createdDate = DateTime.Now,
        //            createdBy = User.Identity.Name,
        //            programType = "Diabetes",
        //            generalComments = (Request.Query["comormidCondition.generalComments"]),
        //            followUp = false,
        //            active = true,

        //        });
        //    }

        //    foreach (var history in model.comormidConditions)
        //    {
        //        var coMorbHistory = _member.InsertComorbidCondition(history);
        //    }

        //    //=========================================================================================================================================================================//
        //    //                                                                                 TB History                                                                              // 
        //    //=========================================================================================================================================================================//

        //    model.drquestionnaireResultList = new List<DoctorQuestionnaireResults>();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        if (!String.IsNullOrEmpty(Request.Query["tbDateDiagnosedFrom_diab" + i]))
        //        {
        //            model.drquestionnaireResultList.Add(new DoctorQuestionnaireResults()
        //            {
        //                tbTreatmentStartDate = Convert.ToDateTime(Request.Query["tbDateDiagnosedFrom_diab" + i]),
        //                tbTreatmentEndDate = Convert.ToDateTime(Request.Query["tbDateDiagnosedTo_diab" + i]),
        //                dependentID = new Guid(Request.Query["DependantID"]),
        //                createdBy = User.Identity.Name,
        //                createdDate = DateTime.Now,
        //                Active = true,
        //            });
        //        }
        //        else
        //            continue;
        //    }

        //    foreach (var history in model.drquestionnaireResultList)
        //    {
        //        var tBHistory = _member.InsertDrQuestionnaire(history);
        //    }

        //    //=========================================================================================================================================================================//
        //    //                                                                        DoctorQuestionnaireResults                                                                       // 
        //    //=========================================================================================================================================================================//

        //    //if (!Request.Query["MedicationHistory.treatmentNaive"].ToString().ToLower().Contains("true") || (!Request.Query["allergy.hasAllergy"].ToString().ToLower().Contains("true") || (!Request.Query["HospitalAuth.hasBeenHospitalised"].ToString().ToLower().Contains("true") || (!Request.Query["podiatry_FollowUp"].ToString().ToLower().Contains("true") || (!Request.Query["AutoNero_FollowUp"].ToString().ToLower().Contains("true") || (!Request.Query["Vision_FollowUp"].ToString().ToLower().Contains("true")) || (!Request.Query["Hypoglycaemia_FollowUp"].ToString().ToLower().Contains("true")) || (!Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true")))))))
        //    //{
        //    //    //Insert DrQuestionnaire entry
        //    //    model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
        //    //    model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
        //    //    model.drquestionnaireResults.createdDate = DateTime.Now;
        //    //    model.drquestionnaireResults.createdBy = User.Identity.Name;
        //    //    model.drquestionnaireResults.Active = true;

        //    //}
        //    //else
        //    //{
        //    model.drquestionnaireResults = new DoctorQuestionnaireResults();

        //    model.drquestionnaireResults.dependentID = new Guid(Request.Query["DependantID"]);
        //    model.drquestionnaireResults.Id = Convert.ToInt32(Request.Query["id"]);
        //    model.drquestionnaireResults.createdDate = DateTime.Now;
        //    model.drquestionnaireResults.createdBy = User.Identity.Name;
        //    model.drquestionnaireResults.Active = true;
        //    //}

        //    //=========================================================================================================================================================================//
        //    //                                                                                  Podiatry                                                                               // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["podiatry_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.podiatry.createdDate = DateTime.Now;
        //        model.podiatry.createdBy = User.Identity.Name;
        //        model.podiatry.programType = "Diabetes";
        //        model.podiatry.active = true;

        //        if (Request.Query["podiatry.amputationComment"].ToString() != null)
        //            model.podiatry.amputationComment = Request.Query["podiatry.amputationComment"].ToString();
        //        if (Request.Query["podiatry.amputationReason"].ToString() != null)
        //            model.podiatry.amputationReason = Request.Query["podiatry.amputationReason"].ToString();
        //        if (Request.Query["podiatry.podiatryLopsComment"].ToString() != null)
        //            model.podiatry.podiatryLopsComment = Request.Query["podiatry.podiatryLopsComment"].ToString();
        //        if (Request.Query["podiatry.podiatryDeformityComment"].ToString() != null)
        //            model.podiatry.podiatryDeformityComment = Request.Query["podiatry.podiatryDeformityComment"].ToString();
        //        if (Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString() != null)
        //            model.podiatry.podiatryPerArterialDiseaseComment = Request.Query["podiatry.podiatryPerArterialDiseaseComment"].ToString();
        //        if (Request.Query["podiatry.podiatryWoundComment"].ToString() != null)
        //            model.podiatry.podiatryWoundComment = Request.Query["podiatry.podiatryWoundComment"].ToString();
        //        if (Request.Query["podiatry.generalComments"].ToString() != null)
        //            model.podiatry.generalComments = Request.Query["podiatry.generalComments"].ToString();
        //    }
        //    else
        //    {
        //        model.podiatry = new Podiatry();

        //        model.podiatry.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.podiatry.createdDate = DateTime.Now;
        //        model.podiatry.createdBy = User.Identity.Name;
        //        model.podiatry.PodiatryID = Convert.ToInt32(Request.Query["id"]);
        //        model.podiatry.amputationComment = "Follow up";
        //        model.podiatry.amputationReason = "Follow up";
        //        model.podiatry.podiatryLopsComment = "Follow up";
        //        model.podiatry.podiatryDeformityComment = "Follow up";
        //        model.podiatry.podiatryPerArterialDiseaseComment = "Follow up";
        //        model.podiatry.podiatryWoundComment = "Follow up";
        //        model.podiatry.generalComments = "Follow up";
        //        model.podiatry.programType = "Diabetes";
        //        model.podiatry.active = true;
        //    }

        //    var podiatryResults = _member.InsertPodiatryResults(model.podiatry);
        //    model.drquestionnaireResults.PodiatryID = model.podiatry.PodiatryID;

        //    //=========================================================================================================================================================================//
        //    //                                                                               AutoNeropathy                                                                             // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["AutoNero_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.autoNeropathy.createdDate = DateTime.Now;
        //        model.autoNeropathy.createdBy = User.Identity.Name;
        //        model.autoNeropathy.programType = "Diabetes";
        //        model.autoNeropathy.active = true;

        //        if (Request.Query["autoNeropathy.autoNeuroSympComment"].ToString() != null)
        //            model.autoNeropathy.autoNeuroSympComment = Request.Query["autoNeropathy.autoNeuroSympComment"].ToString();
        //        if (Request.Query["autoNeropathy.generalComments"].ToString() != null)
        //            model.autoNeropathy.generalComments = Request.Query["autoNeropathy.generalComments"].ToString();

        //    }
        //    else
        //    {
        //        model.autoNeropathy = new AutoNeropathy();

        //        model.autoNeropathy.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.autoNeropathy.AutoNeropathyID = Convert.ToInt32(Request.Query["id"]);
        //        model.autoNeropathy.createdBy = User.Identity.Name;
        //        model.autoNeropathy.createdDate = DateTime.Now;
        //        model.autoNeropathy.autoNeuroSympComment = "Follow up";
        //        model.autoNeropathy.generalComments = "Follow up";
        //        model.autoNeropathy.programType = "Diabetes";
        //        model.autoNeropathy.active = true;
        //    }

        //    var autoNeroResults = _member.InsertAutoNeroResults(model.autoNeropathy);
        //    model.drquestionnaireResults.AutoNeropathyID = model.autoNeropathy.AutoNeropathyID;

        //    //=========================================================================================================================================================================//
        //    //                                                                                  Vision                                                                                 // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["Vision_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.vision.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.vision.createdDate = DateTime.Now;
        //        model.vision.createdBy = User.Identity.Name;
        //        model.vision.programType = "Diabetes";
        //        model.vision.active = true;

        //        if (Request.Query["vision.vision"].ToString() != null)
        //            model.vision.vision = Request.Query["vision.vision"].ToString();
        //        if (Request.Query["vision.eyeTreatment"].ToString() != null)
        //            model.vision.eyeTreatment = Request.Query["vision.eyeTreatment"].ToString();
        //        if (Request.Query["vision.generalComments"].ToString() != null)
        //            model.vision.generalComments = Request.Query["vision.generalComments"].ToString();

        //    }
        //    else
        //    {
        //        model.vision = new Vision();

        //        model.vision.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.vision.VisionID = Convert.ToInt32(Request.Query["id"]);
        //        model.vision.createdBy = User.Identity.Name;
        //        model.vision.createdDate = DateTime.Now;
        //        model.vision.vision = "Follow up";
        //        model.vision.eyeTreatment = "Follow up";
        //        model.vision.generalComments = "Follow up";
        //        model.vision.programType = "Diabetes";
        //        model.vision.active = true;

        //    }

        //    var visionResults = _member.InsertVisionResults(model.vision);
        //    model.drquestionnaireResults.VisionID = model.vision.VisionID;

        //    //=========================================================================================================================================================================//
        //    //                                                                               Hypoglycaemia                                                                             // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["Hypoglycaemia_FollowUp"].ToString().ToLower().Contains("true"))

        //    {
        //        model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.hypoglycaemia.createdDate = DateTime.Now;
        //        model.hypoglycaemia.createdBy = User.Identity.Name;
        //        model.hypoglycaemia.programType = "Diabetes";
        //        model.hypoglycaemia.active = true;

        //        if (Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString() != null)
        //            model.hypoglycaemia.hypoglycaemiaComment = Request.Query["hypoglycaemia.hypoglycaemiaComment"].ToString();
        //        if (Request.Query["hypoglycaemia.glucoseReading"].ToString() != null)
        //            model.hypoglycaemia.glucoseReading = Request.Query["hypoglycaemia.glucoseReading"].ToString();
        //        if (Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString() != null)
        //            model.hypoglycaemia.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemia.hypoglycaemiaSymptomsComment"].ToString();
        //        if (Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString() != null)
        //            model.hypoglycaemia.hypoglycaemiaAssistance = Request.Query["hypoglycaemia.hypoglycaemiaAssistance"].ToString();
        //        if (Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString() != null)
        //            model.hypoglycaemia.emergencyRoomVisits = Request.Query["hypoglycaemia.emergencyRoomVisits"].ToString();
        //        if (Request.Query["hypoglycaemia.insulinUseCheck"].ToString() != null)
        //            model.hypoglycaemia.insulinUseCheck = Convert.ToBoolean(Request.Query["hypoglycaemia.insulinUseCheck"]);
        //        if (Request.Query["hypoglycaemia.sulfonylureaUseCheck"].ToString() != null)
        //            model.hypoglycaemia.sulfonylureaUseCheck = Convert.ToBoolean(Request.Query["hypoglycaemia.sulfonylureaUseCheck"]);
        //        if (Request.Query["hypoglycaemia.ckdStageCheck"].ToString() != null)
        //            model.hypoglycaemia.ckdStageCheck = Convert.ToBoolean(Request.Query["hypoglycaemia.ckdStageCheck"]);
        //        if (Request.Query["hypoglycaemia.patientAgeCheck"].ToString() != null)
        //            model.hypoglycaemia.patientAgeCheck = Convert.ToBoolean(Request.Query["hypoglycaemia.patientAgeCheck"]);
        //        if (Request.Query["hypoglycaemia.generalComments"].ToString() != null)
        //            model.hypoglycaemia.generalComments = Request.Query["hypoglycaemia.generalComments"].ToString();

        //    }
        //    else
        //    {
        //        model.hypoglycaemia = new Hypoglycaemia();

        //        model.hypoglycaemia.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.hypoglycaemia.HypoglycaemiaID = Convert.ToInt32(Request.Query["id"]);
        //        model.hypoglycaemia.createdBy = User.Identity.Name;
        //        model.hypoglycaemia.createdDate = DateTime.Now;
        //        model.hypoglycaemia.hypoglycaemiaComment = "Follow up";
        //        model.hypoglycaemia.glucoseReading = "Follow up";
        //        model.hypoglycaemia.hypoglycaemiaSymptomsComment = "Follow up";
        //        model.hypoglycaemia.hypoglycaemiaAssistance = "Follow up";
        //        model.hypoglycaemia.emergencyRoomVisits = "Follow up";
        //        model.hypoglycaemia.generalComments = "Follow up";
        //        model.hypoglycaemia.programType = "Diabetes";
        //        model.hypoglycaemia.active = true;
        //    }

        //    var hyperGResults = _member.InsertHypoglycaemiaResults(model.hypoglycaemia);
        //    model.drquestionnaireResults.HypoglycaemiaID = model.hypoglycaemia.HypoglycaemiaID;

        //    //=========================================================================================================================================================================//
        //    //                                                                                     Other                                                                               // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true"))

        //    {
        //        model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.questionnaireOther.createdDate = DateTime.Now;
        //        model.questionnaireOther.createdBy = User.Identity.Name;
        //        model.questionnaireOther.programType = "Diabetes";
        //        model.questionnaireOther.active = true;


        //        if (Request.Query["questionnaireOther.occupation"].ToString() != null)
        //            model.questionnaireOther.occupation = Request.Query["questionnaireOther.occupation"].ToString();
        //        if (Request.Query["questionnaireOther.shiftWorker"].ToString() != null)
        //            model.questionnaireOther.shiftWorker = Request.Query["questionnaireOther.shiftWorker"].ToString();
        //        if (Request.Query["questionnaireOther.lypohypertrophy"].ToString() != null)
        //            model.questionnaireOther.lypohypertrophy = Request.Query["questionnaireOther.lypohypertrophy"].ToString();
        //        if (Request.Query["questionnaireOther.depressionComment"].ToString() != null)
        //            model.questionnaireOther.depressionComment = Request.Query["questionnaireOther.depressionComment"].ToString();
        //        if (Request.Query["questionnaireOther.tbTreatmentStartDate"].ToString() != null)
        //            model.questionnaireOther.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentStartDate"]);
        //        if (Request.Query["questionnaireOther.tbTreatmentEndDate"].ToString() != null)
        //            model.questionnaireOther.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["questionnaireOther.tbTreatmentEndDate"]);
        //        if (Request.Query["questionnaireOther.generalComments"].ToString() != null)
        //            model.questionnaireOther.generalComments = Request.Query["questionnaireOther.generalComments"].ToString();

        //    }
        //    else
        //    {
        //        model.questionnaireOther = new QuestionnaireOther();

        //        model.questionnaireOther.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.questionnaireOther.QuestionnaireOtherID = Convert.ToInt32(Request.Query["id"]);
        //        model.questionnaireOther.createdBy = User.Identity.Name;
        //        model.questionnaireOther.createdDate = DateTime.Now;
        //        model.questionnaireOther.occupation = "Follow up";
        //        model.questionnaireOther.shiftWorker = "Follow up";
        //        model.questionnaireOther.lypohypertrophy = "Follow up";
        //        model.questionnaireOther.depressionComment = "Follow up";
        //        model.questionnaireOther.generalComments = "Follow up";
        //        model.questionnaireOther.programType = "Diabetes";
        //        model.questionnaireOther.active = true;

        //    }

        //    var questionOtherResults = _member.InsertQuestionnaireOtherResults(model.questionnaireOther);
        //    model.drquestionnaireResults.QuestionnaireOtherID = model.questionnaireOther.QuestionnaireOtherID;

        //    //=========================================================================================================================================================================//
        //    //                                                                               Social History                                                                            // 
        //    //=========================================================================================================================================================================//

        //    if (Request.Query["SocialHistory_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.questionnaire = new ClinicalHistoryQuestionaire();

        //        model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
        //        model.questionnaire.createdDate = DateTime.Now;
        //        model.questionnaire.createdBy = User.Identity.Name;
        //        model.questionnaire.NoCigs = null;
        //        model.questionnaire.smokingYears = null;
        //        model.questionnaire.NrDrinks = null;
        //        model.questionnaire.programType = "Diabetes";
        //        model.questionnaire.socialComment = (Request.Query["questionnaire.socialComment"]);
        //        model.questionnaire.followUp = true;
        //        model.questionnaire.active = true;
        //    }
        //    else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("true"))
        //    {
        //        model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
        //        model.questionnaire.createdDate = DateTime.Now;
        //        model.questionnaire.createdBy = User.Identity.Name;
        //        model.questionnaire.programType = "Diabetes";

        //        //Insert socialHistory questionnaire results
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
        //            model.questionnaire.NoCigs = Convert.ToInt32(Request.Query["questionnaire.NoCigs"]);
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.smokingYears"]))
        //            model.questionnaire.smokingYears = Convert.ToInt32(Request.Query["questionnaire.smokingYears"]);
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
        //            model.questionnaire.NrDrinks = Request.Query["questionnaire.NrDrinks"].ToString();
        //        if (String.IsNullOrEmpty(Request.Query["questionnaire.NrDrinks"]))
        //            model.questionnaire.NrDrinks = "0";
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
        //            model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
        //            model.questionnaire.createdDate = DateTime.Now;
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))
        //            model.questionnaire.createdBy = User.Identity.Name;
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.socialComment"]))
        //            model.questionnaire.socialComment = Request.Query["questionnaire.socialComment"].ToString();
        //        if (!String.IsNullOrEmpty(Request.Query["questionnaire.NoCigs"]))

        //            model.questionnaire.followUp = false;
        //        model.questionnaire.active = true;

        //    }
        //    else if (Request.Query["questionnaire.smoker"].ToString().ToLower().Contains("false"))
        //    {
        //        model.questionnaire.dependentID = new Guid(Request.Query["DependantID"]);
        //        model.questionnaire.id = Convert.ToInt32(Request.Query["id"]);
        //        model.questionnaire.createdDate = DateTime.Now;
        //        model.questionnaire.createdBy = User.Identity.Name;
        //        model.questionnaire.NoCigs = 0;
        //        model.questionnaire.smokingYears = 0;
        //        model.questionnaire.NrDrinks = "0";
        //        model.questionnaire.programType = "Diabetes";
        //        model.questionnaire.socialComment = (Request.Query["questionnaire.socialComment"]);
        //        model.questionnaire.followUp = false;
        //        model.questionnaire.active = true;

        //    }

        //    var clinicalQuestion = _member.InsertClinicalHistoryQuestionnaire(model.questionnaire);
        //    model.drquestionnaireResults.SocialHistoryID = model.questionnaire.id;

        //    //=========================================================================================================================================================================//
        //    //                                                                           Clincal History Information                                                                   // 
        //    //=========================================================================================================================================================================//

        //    if (!Request.Query["Clinical_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
        //        model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
        //        model.clinicalexam.createdDate = DateTime.Now;
        //        model.clinicalexam.createdBy = User.Identity.Name;
        //        model.clinicalexam.programType = "Diabetes";
        //        model.clinicalexam.active = true;

        //        if (model.clinicalexam.height != 0 || model.clinicalexam.height.Equals(null) && model.clinicalexam.weight != 0 || model.clinicalexam.weight.Equals(null))

        //        {
        //            model.clinicalexam.bmi = (model.clinicalexam.weight / model.clinicalexam.height) / model.clinicalexam.height;
        //            var sq = ((model.clinicalexam.height * 100) * model.clinicalexam.weight) / 3600;
        //            model.clinicalexam.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
        //        }

        //    }
        //    else
        //    {
        //        model.clinicalexam = new Clinical();

        //        model.clinicalexam.dependantID = new Guid(Request.Query["DependantID"]);
        //        model.clinicalexam.id = Convert.ToInt32(Request.Query["id"]);
        //        model.clinicalexam.createdDate = DateTime.Now;
        //        model.clinicalexam.createdBy = User.Identity.Name;
        //        model.clinicalexam.effectiveDate = DateTime.Now;
        //        model.clinicalexam.clinicalComment = "Follow up";
        //        model.clinicalexam.height = 0;
        //        model.clinicalexam.weight = 0;
        //        model.clinicalexam.programType = "Diabetes";
        //        model.clinicalexam.active = true;

        //    }

        //    var clinicalExam = _member.InsertClinicalExam(model.clinicalexam);
        //    model.drquestionnaireResults.ClinicalHistoryID = model.clinicalexam.id;

        //    //=========================================================================================================================================================================//
        //    //                                                                                 Pathology Insert                                                                        // 
        //    //=========================================================================================================================================================================//

        //    model.pathology.dependentID = new Guid(Request.Query["DependantID"]);

        //    model.pathology.effectiveDate = DateTime.Now;
        //    model.pathology.createdDate = DateTime.Now;
        //    model.pathology.createdBy = User.Identity.Name;
        //    model.pathology.pathologyType = "GEN";
        //    model.pathology.labName = "N/A";
        //    model.pathology.labReferenceNo = "N/A";
        //    model.pathology.comment = "Diabetes Questionnaire";
        //    model.pathology.active = true;

        //    var pathology = _member.InsertPathology(model.pathology);

        //    //=========================================================================================================================================================================//
        //    //                                                                                  Allergy Insert                                                                         // 
        //    //=========================================================================================================================================================================//

        //    if (Request.Query["allergy.hasAllergy"].ToString().ToLower().Contains("true"))
        //    {
        //        model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
        //        model.allergy.id = Convert.ToInt32(Request.Query["id"]);
        //        model.allergy.Allergy = Request.Query["allergy.Allergy"];
        //        model.allergy.createdDate = DateTime.Now;
        //        model.allergy.createdBy = User.Identity.Name;
        //        model.allergy.programType = "Diabetes";
        //        model.allergy.active = true;

        //    }
        //    else if (Request.Query["allergy.hasAllergy"].ToString().ToLower().Contains("false"))
        //    {
        //        model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
        //        model.allergy.id = Convert.ToInt32(Request.Query["id"]);
        //        model.allergy.Allergy = "None";
        //        model.allergy.createdDate = DateTime.Now;
        //        model.allergy.createdBy = User.Identity.Name;
        //        model.allergy.programType = "Diabetes";
        //        model.allergy.active = true;

        //    }
        //    else if (Request.Query["allergy.hasAllergy"].ToString().Contains("followup"))
        //    {
        //        model.allergy = new Allergies();

        //        model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
        //        model.allergy.id = Convert.ToInt32(Request.Query["id"]);
        //        model.allergy.Allergy = "Follow up";
        //        model.allergy.createdDate = DateTime.Now;
        //        model.allergy.createdBy = User.Identity.Name;
        //        model.allergy.programType = "Diabetes";
        //        model.allergy.active = true;
        //    }
        //    else
        //    {
        //        model.allergy = new Allergies();

        //        model.allergy.dependantId = new Guid(Request.Query["DependantID"]);
        //        model.allergy.id = Convert.ToInt32(Request.Query["id"]);
        //        model.allergy.Allergy = "None";
        //        model.allergy.createdDate = DateTime.Now;
        //        model.allergy.createdBy = User.Identity.Name;
        //        model.allergy.programType = "Diabetes";
        //        model.allergy.active = true;
        //    }

        //    var allergies = _member.InsertAllergy(model.allergy);
        //    model.drquestionnaireResults.AllergyID = model.allergy.id;

        //    var drQuestionnarireResults = _member.InsertDrQuestionnaire(model.drquestionnaireResults);

        //    //=========================================================================================================================================================================//
        //    //                                                                             Medication History                                                                          // 
        //    //=========================================================================================================================================================================//

        //    model.MedicationHistories = new List<MedicationHistory>();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        if (!String.IsNullOrEmpty(Request.Query["diabetesTreatment" + i]))
        //        {
        //            model.MedicationHistories.Add(new MedicationHistory()
        //            {
        //                productName = Request.Query["diabetesTreatment" + i],
        //                comment = Request.Query["diabetesComment" + i],
        //                dependantId = new Guid(Request.Query["DependantID"]),
        //                createdBy = User.Identity.Name,
        //                createdDate = DateTime.Now,
        //                nappiCode = "-",
        //                directions = "-",
        //                programType = "Diabetes",
        //                active = true,

        //                QuestionnaireID = model.drquestionnaireResults.Id,

        //            });
        //        }
        //        else
        //            continue;
        //    }

        //    if (Request.Query["MedicationHistory.treatmentNaive"].ToString().Contains("followup"))
        //    {
        //        model.MedicationHistories.Add(new MedicationHistory()
        //        {
        //            dependantId = new Guid(Request.Query["DependantID"]),
        //            createdDate = DateTime.Now,
        //            nappiCode = "-",
        //            productName = "-",
        //            comment = "Follow up",
        //            directions = "-",
        //            createdBy = User.Identity.Name,
        //            programType = "Diabetes",
        //            active = true,

        //            QuestionnaireID = model.drquestionnaireResults.Id,

        //        });
        //    }

        //    else if (Request.Query["MedicationHistory.treatmentNaive"].ToString().ToLower().Contains("false"))
        //    {
        //        model.MedicationHistories.Add(new MedicationHistory()
        //        {
        //            dependantId = new Guid(Request.Query["DependantID"]),
        //            createdDate = DateTime.Now,
        //            nappiCode = "None",
        //            productName = "None",
        //            comment = "None",
        //            directions = "None",
        //            createdBy = User.Identity.Name,
        //            programType = "Diabetes",
        //            active = true,

        //            QuestionnaireID = model.drquestionnaireResults.Id,

        //        });
        //    }

        //    foreach (var history in model.MedicationHistories)
        //    {
        //        var medHistory = _member.InsertMedicationHistory(history);
        //    }

        //    //=========================================================================================================================================================================//
        //    //                                                                              Hospitalisations                                                                           // 
        //    //=========================================================================================================================================================================//

        //    model.HospitalAuths = new List<HospitalizationAuths>();

        //    var memberinfo = _member.GetDependentDetails(new Guid(Request.Query["DependantID"]));

        //    for (int i = 0; i < 10; i++)
        //    {
        //        if (!String.IsNullOrEmpty(Request.Query["hzDiagDate_diab" + i]))
        //        {
        //            model.HospitalAuths.Add(new HospitalizationAuths()
        //            {
        //                actualAdminDate = Convert.ToDateTime(Request.Query["hzDiagDate_diab" + i]),
        //                authType = Convert.ToString(Request.Query["hzDiagnosis_diab" + i]),
        //                createdDate = DateTime.Now,
        //                membershipNo = memberinfo.member.membershipNo,
        //                dependantCode = memberinfo.dependent.dependentCode,
        //                medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
        //                programType = "Diabetes",

        //                questionnaireID = model.drquestionnaireResults.Id,
        //            });
        //        }
        //        else
        //            continue;
        //    }

        //    if (Request.Query["HospitalAuth.hasBeenHospitalised"].ToString().Contains("followup"))
        //    {
        //        model.HospitalAuths.Add(new HospitalizationAuths()
        //        {
        //            membershipNo = memberinfo.member.membershipNo,
        //            dependantCode = memberinfo.dependent.dependentCode,
        //            medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
        //            createdDate = DateTime.Now,
        //            authType = "Follow up",
        //            programType = "Diabetes",

        //            questionnaireID = model.drquestionnaireResults.Id,
        //        });
        //    }

        //    else if (Request.Query["HospitalAuth.hasBeenHospitalised"].ToString().ToLower().Contains("false"))
        //    {
        //        model.HospitalAuths.Add(new HospitalizationAuths()
        //        {
        //            membershipNo = memberinfo.member.membershipNo,
        //            dependantCode = memberinfo.dependent.dependentCode,
        //            medicalAid = memberinfo.MedicalAids[0].medicalAidCode,
        //            createdDate = DateTime.Now,
        //            authType = "None",
        //            programType = "Diabetes",

        //            questionnaireID = model.drquestionnaireResults.Id,

        //        });
        //    }

        //    foreach (var history in model.HospitalAuths)
        //    {
        //        var hZHistory = _member.InsertHospitalizationAuths(history);
        //    }



        //    //=========================================================================================================================================================================//
        //    //                                                                                     Notes                                                                               // 
        //    //=========================================================================================================================================================================//

        //    //model.Notes = new List<Notes>();

        //    //model.Note.dependentID = new Guid(Request.Query["DependantID"]);
        //    //model.Note.noteType = Request.Query["NoteTypeID"].ToString();
        //    //if (model.Note.noteType == "INO")
        //    //{
        //    //    model.Note.special = true;
        //    //}
        //    //model.Note.createdBy = User.Identity.Name;
        //    //model.Note.effectiveDate = DateTime.Now;
        //    //var DependantID = _member.InsertNote(model.Note);


        //    //=========================================================================================================================================================================//
        //    //                                                                                    Task Update                                                                          // 
        //    //=========================================================================================================================================================================//

        //    var tasks = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));

        //    tasks.closed = true;
        //    tasks.closedBy = User.Identity.Name;
        //    tasks.closedDate = DateTime.Now;
        //    tasks.status = "Closed";

        //    var taskResult = _clinical.UpdateTask(tasks);

        //    //check for folow ups
        //    if (Request.Query["HospitalAuth.hasBeenHospitalised"].ToString().Contains("true") || Request.Query["MedicationHistory.treatmentNaive"].ToString().Contains("true") || Request.Query["Other_FollowUp"].ToString().ToLower().Contains("true")
        //        || Request.Query["allergy.hasAllergy"].ToString().Contains("true") || Request.Query["Clinical_FollowUp"].ToString().Contains("true") || Request.Query["questionnaire.socialRecordFollowUp"].ToString().ToLower().Contains("true")
        //        || Request.Query["Hypoglycaemia_FollowUp"].ToString().ToLower().Contains("true"))
        //    {
        //        var assignment = new AssignmentsView()
        //        {
        //            newAssignment = new Assignments()
        //            {
        //                createdBy = User.Identity.Name,
        //                createdDate = DateTime.Now,
        //                dependentID = new Guid(Request.Query["DependantID"]),
        //                Active = true,
        //                effectiveDate = DateTime.Now,
        //                assignmentType = "INTER",
        //                status = "Open",
        //                Instruction = "Diabetes questionnaire follow up",
        //            },

        //            assignmentItemType = "FUAQ",

        //        };
        //        var results = _member.InsertAssignment(assignment);

        //        var task = new AssignmentItemTasks();
        //        task.assignmentItemID = assignment.itemID;

        //        var taskss = _member.GetTaskRequirements(assignment.assignmentItemType);

        //        foreach (var tas in taskss)
        //        {
        //            task.taskTypeId = tas.taskType;
        //            task.requirementId = tas.requirementId;
        //            _member.InsertTask(task);
        //        }
        //    }

        //    return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"] });
        //}

        //public ActionResult doctorQuestionnaire_PDF (DoctorQuestionnaireViewModel model)
        //{

        //    try
        //    {

        //        Paragraph heading = new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 2f, Font.BOLD));
        //        heading.SpacingBefore = 0f;
        //        heading.SpacingAfter = 200f;
        //        doc1.Add(heading);
        //        if (model.address != null)
        //        {
        //            ColumnText ct = new ColumnText(cb);
        //            Phrase line1 = new Phrase(model.address.line1 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line1, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            Phrase line2 = new Phrase(model.address.line2 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line2, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            Phrase line3 = new Phrase(model.address.line3 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line3, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            Phrase line4 = new Phrase(model.address.city + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line4, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            Phrase line5 = new Phrase(model.address.province + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line5, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            Phrase line6 = new Phrase(model.address.zip + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //            ct.SetSimpleColumn(line6, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
        //            ct.Go();
        //        }
        //        ColumnText cr = new ColumnText(cb);
        //        Phrase date = new Phrase("Date: " + DateTime.Now.ToShortDateString() + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(date, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase Member = new Phrase("Member No: " + model.membershipno + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(Member, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase surname = new Phrase("Surname: " + model.surname + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(surname, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase Name = new Phrase("Patient: " + model.name + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(Name, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase ContactNo = new Phrase("Mobile: " + model.contact.cell + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(ContactNo, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase TrDoctor = new Phrase("Doctor: " + model.drName + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(TrDoctor, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase Dependent = new Phrase("Dependent: " + model.dependentCode + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(Dependent, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase Dob = new Phrase("Date of Birth: " + model.dateOfBirth + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(Dob, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
        //        Phrase Option = new Phrase("Option: " + model.schemeName + "\n" + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
        //        cr.SetSimpleColumn(Option, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);

        //        cr.Go();

        //        Paragraph par1 = new Paragraph(@"Dear " + model.name + " " + model.surname, new Font(Font.FontFamily.HELVETICA, 10f));
        //        par1.SpacingBefore = 40f;
        //        par1.SpacingAfter = 9f;
        //        doc1.Add(par1);
        //        Paragraph par2 = new Paragraph(@"Your prescription has been received, reviewed and authorised according to clinical guidelines and current medical scheme rules. ", new Font(Font.FontFamily.HELVETICA, 8f));
        //        par2.SpacingAfter = 9f;
        //        doc1.Add(par2);
        //        Paragraph par3 = new Paragraph(@"Please ensure that you take your medication exactly as prescribed by your doctor. Six monthly follow-up pathology test results are recommended to monitor your medical condition on an ongoing basis. Remember to notify HaloCare of any change in medication script or dose.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        par3.SpacingAfter = 9f;
        //        doc1.Add(par3);

        //        PdfPTable table1 = new PdfPTable(1);
        //        PdfPCell cell = new PdfPCell(new Phrase("Authorisation Details", new Font(Font.FontFamily.HELVETICA, 10f)));
        //        cell.HorizontalAlignment = 1;
        //        table1.AddCell(cell);
        //        table1.WidthPercentage = 95;
        //        table1.SpacingAfter = 9f;
        //        doc1.Add(table1);

        //        PdfPTable table2 = new PdfPTable(7);
        //        table2.AddCell(new Phrase("Nappi/Tariff Code", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("Product Name", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("Quantity", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("Directions", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("From Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("To Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        table2.AddCell(new Phrase("Comment", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
        //        cell.HorizontalAlignment = 1;
        //        table2.WidthPercentage = 95;
        //        table2.SpacingAfter = 9f;
        //        foreach (var item in model.scriptItems)
        //        {
        //            table2.AddCell(new Phrase(item.nappiCode, new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase(item.productName, new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase(item.quantity.ToString(), new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase(item.directions, new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase(item.fromDate.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase(item.toDate.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
        //            table2.AddCell(new Phrase("", new Font(Font.FontFamily.HELVETICA, 8f)));
        //        }
        //        doc1.Add(table2);

        //        Paragraph note = new Paragraph("Please note:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
        //        doc1.Add(note);
        //        Paragraph noted = new Paragraph("All medication authorisations are subject to clinical review and approval. Medication claims payment is subject to medical scheme rules and medicine formulary.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        noted.SpacingAfter = 9f;
        //        doc1.Add(noted);

        //        Paragraph nam = new Paragraph("Renewal or change of medication authorisation.", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
        //        doc1.Add(nam);
        //        Paragraph det = new Paragraph("Prescriptions and relevant pathology results to be forwarded to documents@halocare.co.za or faxed to 086 570 2523.", new Font(Font.FontFamily.HELVETICA, 8f));

        //        doc1.Add(det);

        //        Paragraph rendet = new Paragraph("Weight must be noted on prescription for all members up to 13years of age or <40kg.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        rendet.SpacingAfter = 15f;
        //        doc1.Add(rendet);

        //        Paragraph disclaimer = new Paragraph("Disclaimer:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
        //        doc1.Add(disclaimer);
        //        Paragraph disdet = new Paragraph("1. This authorisation is based on clinical guidelines and scheme rules. Authorisation is not a guarantee of payment.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdet);
        //        Paragraph disdett = new Paragraph("2.The payment of claims is subject to the validity of the membership, the Scheme rules and the available benefits as on the date when the service was rendered.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdett);
        //        Paragraph disdet1 = new Paragraph("3.The authorisation is only valid for the dependant listed in this authorisation schedule and is granted based on the information available at the time.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdet1);
        //        Paragraph disdet2 = new Paragraph("4.The authorisation is only valid for the time period indicated and a new prescription is required to renew the authorisation. ", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdet2);
        //        Paragraph disdett3 = new Paragraph("5.Any changes to the approved items must be communicated to the Halocare Managed Care and failure to do so may result in non-payment of medication.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdett3);
        //        Paragraph disdet11 = new Paragraph("6.In the event of an option change, it remains the responsibility of the member to confirm how the option change will affect the authorisation.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdet11);
        //        Paragraph disdet4 = new Paragraph("7.Medication is restricted to formularies (medicine lists), clinical entry criteria and disease management protocols where applicable.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(disdet4);
        //        Paragraph disdett5 = new Paragraph("8.Medication requires a prescription from a person legally entitled to prescribe as well as the relevant ICD 10 diagnosis code, visible and valid date and signature of the prescriber, before it will be considered for benefits.", new Font(Font.FontFamily.HELVETICA, 8f));
        //        disdett5.SpacingAfter = 10f;
        //        doc1.Add(disdett5);

        //        Paragraph s = new Paragraph("Halocare Managed Care contact details", new Font(Font.FontFamily.HELVETICA, 7f));
        //        doc1.Add(s);
        //        Paragraph sig2 = new Paragraph("Tel: 086 570 2523", new Font(Font.FontFamily.HELVETICA, 7f));
        //        doc1.Add(sig2);
        //        Paragraph sig3 = new Paragraph("Email: info@halocare.co.za", new Font(Font.FontFamily.HELVETICA, 7f));
        //        sig3.SpacingAfter = 10f;
        //        doc1.Add(sig3);

        //        Paragraph reg = new Paragraph("Kind Regards", new Font(Font.FontFamily.HELVETICA, 8f));
        //        doc1.Add(reg);
        //        Paragraph sig = new Paragraph("Halocare Managed Care Team", new Font(Font.FontFamily.HELVETICA, 8f));
        //        sig.SpacingAfter = 9f;
        //        doc1.Add(sig);
        //        doc1.Close();


        //        var att = new Attachments();
        //        att.dependentID = depID;
        //        att.createdBy = User.Identity.Name;
        //        att.attachmentType = "LET";
        //        att.attachmentName = filename;
        //        att.Link = att.attachmentName;
        //        att.createdBy = User.Identity.Name;
        //        _member.InsertAttachment(att);

        //        authl.AuthLinkID = att.attachmentID;

        //        var authlet = _member.InsertAuthLetter(authl);

        //        if (authl.sentVia == "Email")
        //        {
        //            var list = new List<int>();
        //            foreach (var row in model.attachments)
        //            {
        //                if (Request.Query[row.attachmentID.ToString()].ToString() != null)
        //                {
        //                    var checkIt = Request.Query[row.attachmentID.ToString()].ToString();
        //                    if (checkIt == "true,false")
        //                    {
        //                        list.Add(row.attachmentID);
        //                    }
        //                }
        //            }

        //            var email = new Management.EmailService();
        //            string htmlmail = @"<style>
        //                        table {
        //                            width: 100%;
        //                        }

        //                        .header {
        //                            background-color: #1f4e79;
        //                            color: #ffffff;
        //                        }

        //                        .authorizationletter {
        //                            font-family: Calibri !important;
        //                            font-size: 10px !important;
        //                        }
        //                    </style>
        //    <h4>Dear" + authl.sentTo + @"</h4>
        //    <hr />
        //    <br />
        //    Please find the authorization letter for " + model.name + @" " + model.surname + @". 
        //    < br /><br />
        //    Managed Care contact details:<br />
        //    Tel: 0860 143 258<br />
        //    Email: info @halocare.co.za<br />
        //    <br />
        //    Kind regards<br />
        //    HaloCare Team<br />
        //    Managed Care
        //</div>";
        //            var sendmail = email.SendEmail(authl.sentTo, "Authorisation Letter " + model.name + " " + model.surname + " " + model.membershipno + DateTime.Now.ToShortDateString(), htmlmail, true,
        //                path + att.attachmentName, att.attachmentName, list.ToArray());
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        string filePath = @"d:\data\Logs\HC\Error.txt";
        //        using (StreamWriter writer = new StreamWriter(filePath, true))
        //        {
        //            writer.WriteLine("Auth Error: " + Environment.NewLine + "Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
        //               "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
        //            writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
        //        }
        //        return View(model);
        //    }

        //    return RedirectToAction("patientDashboard", new { DependentID = depID });
        //}
        #endregion

    }
}
