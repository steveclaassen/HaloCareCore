using HaloCareCore.DAL;
using HaloCareCore.Helpers;
using HaloCareCore.Models;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class TemplateController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IMedicalAidRepository _medical;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TemplateController(OH17Context context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this._admin = new AdminRepository(context, configuration);
            this._member = new MemberRepository(configuration, context);
            this._medical = new MedicalAidRepository(configuration, context);
            _webHostEnvironment = webHostEnvironment;
        }

        public ActionResult Index()
        {
            var viewModel = new CommunicationViewModel()
            {
                smsMessageTemplates = _admin.GetSmsMessageTemplates(),
                emailTemplates = _admin.GetemailTemplate(),
                EducationalNotes = _admin.GetEducationalNote(),
                PopUpTemplates = _admin.GetPopUpTemplates(),
                Attachments = _admin.GetAttachmentTemplateIndex(), //hcare-1380
                EmailLayouts = _admin.GetEmailLayoutIndex(), //hcare-1384
            };

            return View(viewModel);
        }

        //========================================================================= text-templates =========================================================================//
        public ActionResult Create()
        {
            var model = new SmsMessageTemplates();
            model.Languages = _admin.GetsmsLanguage();
            model.Programs = _admin.GetPrograms();

            ViewBag.smsTitle = "";
            ViewBag.Language = "";
            ViewBag.Program = "";
            ViewBag.smsTemplate = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(SmsMessageTemplates model)
        {
            model.title = model.title.Trim(); //hcare-1285
            model.template = model.template.Trim(); //hcare-1285
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetSMSTemplates(); //hcare-1298
            var program = _admin.GetPrograms().Where(x => x.code == model.program).Select(x => x.ProgramName).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.title.ToLower() == item.title.ToLower().Trim() && model.template.ToLower() == item.template.ToLower().Trim() && model.program.ToLower().Trim() == item.program.ToLower().Trim()) { v1++; break; }
                if (model.title.ToLower() == item.title.ToLower().Trim()) { v2++; }
                if (model.template.ToLower() == item.template.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTitle = model.title;
                ViewBag.smsTemplate = model.template;
                ViewBag.Program = program;

                return View(model);
            }
            if (v2 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTitle = model.title;
                return View(model);
            }
            if (v3 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTemplate = model.template;
                return View(model);
            }
            #endregion

            _admin.InsertSmsMessageTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _admin.GetSmsMessageTemplate(id);
            model.Languages = _admin.GetsmsLanguage();
            model.Programs = _admin.GetPrograms();

            ViewBag.Rule = "";
            ViewBag.smsTitle = "";
            ViewBag.Language = "";
            ViewBag.Program = "";
            ViewBag.smsTemplate = "";


            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(SmsMessageTemplates model)
        {
            model.title = model.title.Trim(); //hcare-1285
            model.template = model.template.Trim(); //hcare-1285
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            #region duplicate-check
            var validation = _admin.GetSMSTemplates(); //hcare-1298
            var program = _admin.GetPrograms().Where(x => x.code == model.program).Select(x => x.ProgramName).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.templateID != item.templateID && model.title.ToLower() == item.title.ToLower().Trim() && model.template.ToLower() == item.template.ToLower().Trim() && model.program.ToLower().Trim() == item.program.ToLower().Trim()) { v1++; break; }
                if (model.templateID != item.templateID && model.title.ToLower() == item.title.ToLower().Trim()) { v2++; }
                if (model.templateID != item.templateID && model.template.ToLower() == item.template.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTitle = model.title;
                ViewBag.smsTemplate = model.template;
                ViewBag.Program = program;

                return View(model);
            }
            if (v2 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTitle = model.title;
                return View(model);
            }
            if (v3 > 0)
            {
                model.Languages = _admin.GetsmsLanguage();
                model.Programs = _admin.GetPrograms();
                ViewBag.smsTemplate = model.template;
                return View(model);
            }
            #endregion

            _admin.UpdateSmsTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var model = _admin.GetSmsMessageTemplate(id);
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var model = _admin.GetSmsMessageTemplate(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Delete(SmsMessageTemplates model)
        {
            try
            {
                model.Active = false;
                model.modifiedBy = User.Identity.Name;
                var result = _admin.UpdateSmsTemplate(model);

                return RedirectToAction("Index");//HCAre-1200
            }
            catch
            {
                return View();
            }
        }

        public ActionResult TextTitleCheck(string name) //HCare-956
        {
            var options = _admin.GetTextTemplateByTitle(name);

            return Json(options);
        }
        public ActionResult TextMessageCheck(string template) //HCare-956
        {
            var options = _admin.GetTextTemplateByMessage(template);

            return Json(options);
        }

        //========================================================================= email-templates =========================================================================//
        public ActionResult Create_EmailTemplates()
        {
            var model = new EmailTemplates();
            model.Programs = _admin.GetPrograms();

            ViewBag.emailTitle = "";
            ViewBag.Program = "";
            ViewBag.emailTemplate = "";
            return View(model);
        }
        [HttpPost]
        
        public ActionResult Create_EmailTemplates(EmailTemplates model)
        {
            model.title = model.title.Trim(); //hcare-1285
            if (String.IsNullOrEmpty(model.templateBody))
            {
                model.Programs = _admin.GetPrograms();
                ViewBag.emailBody = "Body cannot be empty!";
                return View(model);
            }
            model.templateBody = model.templateBody.Trim(); //hcare-1285
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetEmailTemplates(); //hcare-1298
            var program = _admin.GetPrograms().Where(x => x.code == model.program).Select(x => x.ProgramName).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            foreach (var item in validation)
            {
                if (model.title.ToLower() == item.title.ToLower().Trim()) { v1++; }
                if (model.templateBody.ToLower() == item.templateBody.ToLower().Trim()) { v2++; break; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms();
                ViewBag.emailTitle = model.title;
                return View(model);
            }
            if (v2 > 0)
            {
                model.Programs = _admin.GetPrograms();
                ViewBag.emailTemplate = model.template;
                return View(model);
            }
            #endregion

            _admin.InsertEmailTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit_EmailTemplates(int id)
        {

            var Viewmodel = _admin.GetEmailTemplateByID(id);
            Viewmodel.Programs = _admin.GetPrograms();

            return View(Viewmodel);
        }
        [HttpPost]
        
        public ActionResult Edit_EmailTemplates(EmailTemplates model)
        {
            model.title = model.title.Trim(); //hcare-1285
            model.templateBody = model.templateBody.Trim(); //hcare-1285
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            if (String.IsNullOrEmpty(model.templateBody))
            {
                model.Programs = _admin.GetPrograms();
                ViewBag.emailBody = "Body cannot be empty!";
                return View(model);
            }

            #region duplicate-check
            var validation = _admin.GetEmailTemplates(); //hcare-1298
            var program = _admin.GetPrograms().Where(x => x.code == model.program).Select(x => x.ProgramName).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            foreach (var item in validation)
            {
                if (model.templateID != item.templateID && model.title.ToLower() == item.title.ToLower().Trim()) { v1++; }
                if (model.templateID != item.templateID && model.templateBody.ToLower() == item.templateBody.ToLower().Trim()) { v2++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.Program = model.program;
                ViewBag.emailTitle = model.title;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Program = model.program;
                ViewBag.emailTemplate = model.template;
                return View(model);
            }
            #endregion

            var result = _admin.UpdateEmailTemplate(model);

            return RedirectToAction("Index");//HCAre-1200
        }

        public ActionResult Details_EmailTemplates(int id)
        {
            var model = _admin.GetEmailTemplateByID(id);
            return View(model);
        }

        public ActionResult uploadPartial()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var appData = Path.Combine(webRootPath,"~/Content/Images/uploads");
            var images = Directory.GetFiles(appData).Select(x => new ImagesViewModel
            {
                Url = Url.Content("/images/uploads/" + Path.GetFileName(x))
            });
            return View(images);
        }
        public void uploadnow(IFormFile upload)
        {
            if (upload != null)
            {
                string ImageName = upload.FileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "~/Images/uploads",ImageName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    upload.CopyToAsync(stream);
                }
            }

        }

        //========================================================================= pop-up-templates =========================================================================//

        public ActionResult PopUpTemplates_Create() //hcare-1374
        {
            var model = new PopUpTemplate();

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Options = new List<MedicalAidPlans>();
            model.Programs = _admin.GetPrograms().Where(x => x.Active == true).ToList();

            return View(model);
        }
        [HttpPost]
        
        public ActionResult PopUpTemplates_Create(PopUpTemplate model) //hcare-1374
        {
            model.Id = Guid.NewGuid();
            model.Type = Request.Query["popup-type"];
            if (!String.IsNullOrEmpty(model.Title)) { model.Title = model.Title.Trim(); }
            model.selectedSchemes = Request.Query["selectedSchemes"];
            model.selectedOptions = Request.Query["selectedOptions"];
            model.selectedPrograms = string.Join(",", Request.Query["selectedPrograms"].ToString().Split(',').OrderBy(s => s));


            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetPopUpTemplateList().OrderBy(x=>x.selectedPrograms).ToList();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Type.ToLower().Trim() == item.Type.ToLower().Trim() && model.Title.ToLower().Trim() == item.Title.ToLower().Trim() && model.Template.ToLower().Trim() == item.Template.ToLower().Trim() && model.selectedSchemes == item.selectedSchemes && model.selectedOptions == item.selectedOptions && model.selectedPrograms == item.selectedPrograms) { v1++; }
            }
            if (v1 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
                model.Options = new List<MedicalAidPlans>();
                model.Programs = _admin.GetPrograms().Where(x => x.Active == true).ToList();
                ViewBag.Validation = "duplicate";
                
                return View(model);
            }
            #endregion

            _admin.InsertPopUpTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult PopUpTemplates_Edit(Guid id) //hcare-1374
        {
            //var model = _admin.GetPopUpTemplateDetails(id);
            var model = _admin.GetPopUpTemplateByID(id);

            model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
            model.Options = _member.GetMedicalAidPlans();
            model.Programs = _admin.GetPrograms().Where(x => x.Active == true).ToList();

            #region hidden-fields
            TempData["scheme"] = model.selectedSchemes;
            TempData["options"] = model.selectedOptions;
            TempData["programs"] = model.selectedPrograms;
            #endregion
            return View(model);
        }
        [HttpPost]
        
        public ActionResult PopUpTemplates_Edit(PopUpTemplate model) //hcare-1374
        {
            model.Type = Request.Query["popup-type"];
            if (!String.IsNullOrEmpty(model.Title)) { model.Title = model.Title.Trim(); }
            if (!String.IsNullOrEmpty(Request.Query["Template"])) { model.Template = Request.Query["Template"].ToString().Trim(); }

            model.selectedSchemes = Request.Query["selectedSchemes"];
            model.selectedOptions = Request.Query["selectedOptions"];
            model.selectedPrograms = string.Join(",", Request.Query["selectedPrograms"].ToString().Split(',').OrderBy(s => s));

            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            #region duplicate-check
            var validation = _admin.GetPopUpTemplateList().OrderBy(x => x.selectedPrograms).ToList();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.Type.ToLower().Trim() == item.Type.ToLower().Trim() && model.Title.ToLower().Trim() == item.Title.ToLower().Trim() && model.Template.ToLower().Trim() == item.Template.ToLower().Trim() && model.selectedSchemes == item.selectedSchemes && model.selectedOptions == item.selectedOptions && model.selectedPrograms == item.selectedPrograms) { v1++; }
            }
            if (v1 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids().Where(x => x.Active == true).ToList();
                model.Options = _member.GetMedicalAidPlans();
                model.Programs = _admin.GetPrograms().Where(x => x.Active == true).ToList();

                #region hidden-fields
                TempData["scheme"] = model.selectedSchemes;
                TempData["options"] = model.selectedOptions;
                TempData["programs"] = model.selectedPrograms;
                #endregion

                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion

            _admin.UpdatePopUpTemplate(model);

            return RedirectToAction("Index");
        }

        //======================================================================= email-attachments =======================================================================//
        public ActionResult Attachment_Create() //hcare-1380
        {
            var model = new AttachmentTemplate();
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();

            return View(model);
        }
        [HttpPost]
        public ActionResult Attachment_Create(AttachmentTemplate model, IFormFile file) //hcare-1380
        {
            model.Id = Guid.NewGuid();
            model.AttachmentName = model.AttachmentName.Trim();
            model.AttachmentExtension = Request.Query["attachment-type"];
            model.AttachmentType = Request.Query["attachment-type-text"];
            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.Active = true;

            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads\\templates\\attachments";
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                string filename = Path.GetFileName(Request.Form.Files["file"].FileName.Replace("<", "").Replace(">", ""));
               
                var filePath = Path.Combine(path, filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.Root = path;
                model.FileName = filename;
            }

            #region duplicate-check
            var validation = _admin.GetAttachmentTemplateValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.AttachmentName.ToLower() == item.AttachmentName.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion
            _admin.InsertAttachmentTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult Attachment_Edit(Guid id) //hcare-1380
        {
            var model = _admin.GetAttachmentTemplateByID(id);
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();

            ViewBag.attachmenttype = model.AttachmentType;
            ViewBag.attachmentextension = model.AttachmentExtension;

            return View(model);
        }
        [HttpPost]
        public ActionResult Attachment_Edit(AttachmentTemplate model, IFormFile file) //hcare-1380
        {
            var existing = _admin.GetAttachmentTemplateByID(model.Id);

            model.AttachmentName = model.AttachmentName.Trim();
            model.AttachmentExtension = Request.Query["attachment-type"];
            model.AttachmentType = Request.Query["attachment-type-text"];
            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads\\templates\\attachments";
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                string filename = Path.GetFileName(Request.Form.Files["file"].FileName.Replace("<", "").Replace(">", ""));
               
                var filePath = Path.Combine(path, filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.Root = path;
                model.FileName = filename;
            }
            else
            {
                model.Root = existing.Root;
                model.FileName = existing.FileName;
            }

            #region duplicate-check
            var validation = _admin.GetAttachmentTemplateValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.AttachmentName.ToLower() == item.AttachmentName.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                ViewBag.Validation = "duplicate";
                ViewBag.attachmenttype = model.AttachmentType;
                ViewBag.attachmentextension = model.AttachmentExtension;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateAttachmentTemplate(model);

            return RedirectToAction("Index");
        }

        public ActionResult Attachment_Details(Guid id) //hcare-1380
        {
            var model = _admin.GetAttachmentTemplateDetails(id);

            return View(model);
        }

        public ActionResult GetAttachment(string attachment) //hcare-1380
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "~/uploads\\templates\\attachments", attachment);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = attachment;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        

        public ActionResult PopUpTemplates_Details(Guid id) //hcare-1374
        {
            var model = _admin.GetPopUpTemplateDetails(id);
            return View(model);
        }

        public ActionResult FillScheme(Guid medId)
        {
            var options = _member.GetMedicalAidPlansByMedicalAidId(medId);
            return Json(options);
        }
        public ActionResult AttachmentNameValidation(string name) //hcare-1380
        {
            var options = _admin.GetTemplateByName(name);

            return Json(options);
        }

        //========================================================================= email-layouts =========================================================================//
        public ActionResult Email_Layout_Create() //hcare-1384
        {
            var model = new EmailLayout();
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();

            return View(model);
        }
        [HttpPost]
        public ActionResult Email_Layout_Create(EmailLayout model, IFormFile file) //hcare-1384
        {
            model.Id = Guid.NewGuid();
            model.Description = model.Description.Trim();
            model.LayoutType = Request.Query["layout-type"];
            model.LayoutSize = Request.Query["layout-size"];
            if (model.LayoutSize.ToLower().Contains("small")) { model.LayoutHeight = "150"; model.LayoutWidth = "1024"; }
            else if (model.LayoutSize.ToLower().Contains("medium")) { model.LayoutHeight = "300"; model.LayoutWidth = "1024"; }
            else if (model.LayoutSize.ToLower().Contains("large")) { model.LayoutHeight = "450"; model.LayoutWidth = "1024"; }

            model.AttachmentType = "JPEG";
            model.FileExtension = ".jpg";

            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.Active = true;

            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads//templates//layout";
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                string filename = Path.GetFileName(Request.Form.Files["file"].FileName.Replace(" ", "-").Replace(".png", ".jpg").TrimStart().TrimEnd().ToLower()); //hcare-1384: update
                
                var filePath = Path.Combine(path, filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.Root = path;
                model.FileName = filename;
            }

            #region duplicate-check
            var validation = _admin.GetEmailLayoutValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Description.ToLower() == item.Description.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion


            _admin.InsertEmailLayout(model);

            return RedirectToAction("Index");
        }

        public ActionResult Email_Layout_Edit(Guid id) //hcare-1384
        {
            var model = _admin.GetEmailLayoutByID(id);
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();

            ViewBag.type = model.LayoutType;
            ViewBag.attachmenttype = model.AttachmentType;
            ViewBag.extension = model.FileExtension;
            ViewBag.size = char.ToUpper(model.LayoutSize[0]) + model.LayoutSize.Substring(1) + " (" + model.LayoutHeight + " x " + model.LayoutWidth + ")";
            ViewBag.type = char.ToUpper(model.LayoutType[0]) + model.LayoutType.Substring(1);

            return View(model);
        }
        [HttpPost]
        public ActionResult Email_Layout_Edit(EmailLayout model, IFormFile file) //hcare-1384
        {
            var existing = _admin.GetEmailLayoutByID(model.Id);

            model.Description = model.Description.Trim();
            model.LayoutType = Request.Query["layout-type"];
            model.LayoutSize = Request.Query["layout-size"];
            if (model.LayoutSize.ToLower().Contains("small")) { model.LayoutHeight = "150"; model.LayoutWidth = "1024"; }
            else if (model.LayoutSize.ToLower().Contains("medium")) { model.LayoutHeight = "300"; model.LayoutWidth = "1024"; }
            else if (model.LayoutSize.ToLower().Contains("large")) { model.LayoutHeight = "450"; model.LayoutWidth = "1024"; }

            model.AttachmentType = "JPEG";
            model.FileExtension = ".jpg";

            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads//templates//layout";
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                string filename = Path.GetFileName(Request.Form.Files["file"].FileName.Replace(" ", "-").Replace(".png", ".jpg").TrimStart().TrimEnd().ToLower()); //hcare-1384: update
               
                var filePath = Path.Combine(path, filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.Root = path;
                model.FileName = filename;
            }
            else
            {
                model.Root = existing.Root;
                model.FileName = existing.FileName;
            }

            #region duplicate-check
            var validation = _admin.GetEmailLayoutValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.Description.ToLower() == item.Description.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                ViewBag.Validation = "duplicate";
                ViewBag.type = model.LayoutType;
                ViewBag.extension = model.FileExtension;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateEmailLayout(model);

            return RedirectToAction("Index");
        }

        public ActionResult Email_Layout_Details(Guid id) //hcare-1384
        {
            var model = _admin.GetEmailLayoutDetails(id);

            return View(model);
        }


        public ActionResult GetEmailLayoutAttachment(string attachment) //hcare-1384
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "~/uploads//templates//layout", attachment);
 
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = attachment;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult EmailLayoutNameValidation(string name) //hcare-1384
        {
            var options = _admin.GetEmailLayoutByName(name);

            return Json(options);
        }
    }
}
