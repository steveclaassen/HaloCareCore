using ClosedXML.Excel;
using HaloCareCore.DAL;
using HaloCareCore.Management;
using HaloCareCore.Models;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Script;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace HaloCareCore.Controllers
{
    public class ScriptsController : Controller
    {

        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IClinicalRepository _clinical;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ScriptsController(OH17Context context, IConfiguration configuration)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);
            _clinical = new ClinicalRepository(context, configuration);
        }

        public JsonResult UpdateScriptItem(ScriptItems model)
        {
            // Update model to your db 
            var item = _member.GetScriptItem(model.itemNo);
            item.toDate = model.toDate;
            item.modifiedBy = User.Identity.Name;
            item.modifiedDate = DateTime.Now;
            item.prophylaxis = model.prophylaxis;
            var result = _member.UpdateScriptItem(item);
            string message = "Failure";
            if (result.Success)
            {
                message = "Success";
            }

            return Json(message);
        }

        public ActionResult OutstandingAuthorise(string sortOrder)
        {
            ViewBag.createdDateAsc = sortOrder == "createdDate" ? "createdDate_asc" : "createdDate";
            ViewBag.effectiveDateAsc = sortOrder == "effectiveDate" ? "effectiveDate_asc" : "effectiveDate";

            var model = _admin.GetOutstandingAuthorisedScripts();

            switch (sortOrder)
            {

                case "createdDate":
                    if (sortOrder.Equals(ViewBag))
                        model = _admin.GetOutstandingAuthorisedScripts().OrderBy(c => c.createdDate).ToList();
                    else
                        model = _admin.GetOutstandingAuthorisedScripts().OrderByDescending(c => c.createdDate).ToList();
                    break;

                case "effectiveDate":
                    if (sortOrder.Equals(ViewBag))
                        model = _admin.GetOutstandingAuthorisedScripts().OrderBy(c => c.effectiveDate).ToList();
                    else
                        model = _admin.GetOutstandingAuthorisedScripts().OrderByDescending(c => c.effectiveDate).ToList();
                    break;


            }

            return View(model);
        }

        public ActionResult Recent()
        {
            var model = _admin.GetRecentScripts();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = new ScriptCreationViewModel();
            model.script = _member.GetScript(id);
            model.items = _member.GetScriptItems(id);
            model.currentScript = _member.GetScripts(model.script.dependentID)[0].scriptID;
            return View(model);
        }

        [HttpPost]
        public ActionResult Details(ScriptCreationViewModel model)
        {
            model.script.modifiedBy = User.Identity.Name;
            model.script.modifiedDate = DateTime.Now;
            var res = _member.UpdateScript(model.script);
            return new RedirectResult(Url.Action("patientClinical", "Member", new { DependentID = model.script.dependentID, @target = "1" }) + "#clin");
        }


        public ActionResult AuthoriseScript(int id)
        {
            var model = new ScriptAuthViewModel();
            model.script = _member.GetScript(id);
            model.items = _member.GetScriptItems(id);
            model.member = _member.GetDependentDetails(model.script.dependentID, null);
            model.pathology = _member.GetPathology(model.script.dependentID).Take(3).ToList();
            model.clinicals = _member.GetClinicalExam(model.script.dependentID);
            model.scriptItems = _member.GetScriptItemsMultiple(model.script.dependentID, id).Take(10).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult AuthoriseScript(ScriptAuthViewModel ScriptAuthmodel)
        {
            //return View(ScriptAuthmodel);
            try
            {
                var scriptInfo = new Scripts();
                scriptInfo = _member.GetScript(ScriptAuthmodel.script.scriptID);
                scriptInfo.firstLine = ScriptAuthmodel.script.firstLine;
                scriptInfo.secondLine = ScriptAuthmodel.script.secondLine;
                scriptInfo.prophylaxis = ScriptAuthmodel.script.prophylaxis;
                scriptInfo.resistanceTest = ScriptAuthmodel.script.resistanceTest;
                scriptInfo.salvageTherapy = ScriptAuthmodel.script.salvageTherapy;
                /*if (!ModelState.IsValid)
                {
                    if (!scriptInfo.firstLine && !scriptInfo.secondLine && !scriptInfo.prophylaxis && !scriptInfo.resistanceTest && !scriptInfo.salvageTherapy)
                    {
                        ModelState.AddModelError("Script Category", "Please select script category");
                        return View(ScriptAuthmodel);
                    }
                }*/
                scriptInfo.modifiedBy = User.Identity.Name;
                scriptInfo.modifiedDate = DateTime.Now;
                scriptInfo.active = true;


                var res = _member.UpdateScript(scriptInfo);

                ScriptAuthmodel.member = _member.GetDependentDetails(ScriptAuthmodel.script.dependentID, null);
                ScriptAuthmodel.pathology = _member.GetPathology(ScriptAuthmodel.script.dependentID).Take(3).ToList();
                ScriptAuthmodel.scriptItems = _member.GetScriptItemsMultiple(ScriptAuthmodel.script.dependentID, ScriptAuthmodel.script.scriptID);

                var doctorinfo = _admin.GetDoctor(ScriptAuthmodel.script.doctorID);
                ScriptAuthmodel.member.doctor = new Doctors();
                ScriptAuthmodel.member.doctor.practiceNo = doctorinfo.practiceNo;

                //generate auth request here
                foreach (var row in ScriptAuthmodel.items.ToList())
                {
                    if (!row.authItem)
                    {
                        ScriptAuthmodel.items.Remove(row);
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"d:\Data\Logs\HC\Error.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Auth Error: " + Environment.NewLine + "Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            if (ScriptAuthmodel.member.MedicalAids[0].medicalAidCode.Contains("MH") || ScriptAuthmodel.member.MedicalAids[0].medicalAidCode.Contains("TEST"))
            {
                _member.UpdateScriptItems(ScriptAuthmodel.items, "Authorised");
                var task = _clinical.GetTaskByComment(ScriptAuthmodel.script.scriptID.ToString());
                //if (task.comment != null)
                //{
                //    task.closedBy = User.Identity.Name;
                //    task.closedDate = DateTime.Now;
                //    task.closed = true;
                //    task.modifiedBy = User.Identity.Name;
                //    task.modifiedDate = DateTime.Now;

                //    _clinical.UpdateTask(task);
                //}
                ScriptAuthmodel.script.Status = "Active";
                ScriptAuthmodel.script.modifiedBy = User.Identity.Name;
                ScriptAuthmodel.script.modifiedDate = DateTime.Now;
                ScriptAuthmodel.script.active = true;
                _member.UpdateScript(ScriptAuthmodel.script);
                return RedirectToAction("AuthoriseScript", "Scripts", new { id = ScriptAuthmodel.script.scriptID });

            }
            else //Mediscor Auth
            {
                try
                {
                    //generate auth request here
                    var Authxml = Authorise.GetMediscorXml(ScriptAuthmodel);
                    string xml = Authxml.InnerXml.ToString();
                    int entryLineNo = Authorise.LogAuthRequest(3006, ScriptAuthmodel.script.scriptID, ScriptAuthmodel.script.dependentID, xml, "", "System");

                    XmlNode PV = new ProcessScriptAuthorisation().PatientScriptAuthorisation(xml);

                    //Response.WriteAsync(PV.OuterXml.ToString());
                    XmlDocument xdoc = new XmlDocument();

                    xdoc.LoadXml(PV.OuterXml.ToString());

                    // load xslt to do transformation
                    XslTransform xsl = new XslTransform();
                    xsl.Load(Server.MapPath("../../xml/AuthoriseScript.xslt"));
                    // load xslt arguments to load specific page from xml file
                    // this can be used if you have multiple pages
                    // in your xml file and you loading them one at a time
                    XsltArgumentList xslarg = new XsltArgumentList();

                    // get transformed results
                    StringWriter sw = new StringWriter();
                    xsl.Transform(xdoc, xslarg, sw);
                    string result = sw.ToString();

                    XmlNamespaceManager XNM = new XmlNamespaceManager(xdoc.NameTable);
                    string messages = xdoc.SelectSingleNode("//d/:rm//m:r", XNM).InnerText;


                    System.IO.File.AppendAllText(@"d:\Data\Logs\mediscor\ProcessScriptAuthorizationLog.txt"
                        , string.Format("{1}: {0}input: {3} {0}result: {4} {0}response: {2} {0}check : {5} {0}|{0}"
                            , Environment.NewLine, DateTime.Now, PV.OuterXml, xml, result, xdoc.SelectSingleNode("d/@st").Value));

                    // free up the memory of objects that are not used anymore
                    if (xdoc.SelectSingleNode("d/@st").Value != "0")
                    {
                        _member.UpdateScriptItems(ScriptAuthmodel.items, "Authorised");
                        var task = _clinical.GetTaskByComment(ScriptAuthmodel.script.scriptID.ToString());
                        if (task.comment != null)
                        {
                            task.closedBy = User.Identity.Name;
                            task.closedDate = DateTime.Now;
                            task.closed = true;
                            task.modifiedBy = User.Identity.Name;
                            task.modifiedDate = DateTime.Now;

                            _clinical.UpdateTask(task);
                        }
                        ScriptAuthmodel.script.Status = "Active";
                        ScriptAuthmodel.script.modifiedBy = User.Identity.Name;
                        ScriptAuthmodel.script.modifiedDate = DateTime.Now;
                        ScriptAuthmodel.script.active = true;
                        _member.UpdateScript(ScriptAuthmodel.script);
                        Authorise.UpdateAuthRequest(entryLineNo, xml, result, User.Identity.Name);

                        System.IO.File.AppendAllText(@"d:\Data\Logs\mediscor\ProcessScriptAuthorizationLog.txt"
                           , string.Format("{1}: {0}UpdateScriptDetails: {0}|{0}"
                               , Environment.NewLine, DateTime.Now));
                    }
                    else
                    {
                        _member.UpdateScriptItems(ScriptAuthmodel.items, "Rejected");
                        Response.WriteAsync("<span class=\"red\">Script could not be authorised at this time</span>");
                    }
                    sw.Close();
                    Response.WriteAsync(result);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(@"d:\Data\Logs\mediscor\ProcessScriptAuthorizationLog.txt"
                        , string.Format("{3}: error: {0}{1}{2}|{1}", ex.Message, Environment.NewLine, ex.StackTrace, DateTime.Now));
                }
            }
            return View(ScriptAuthmodel);
        }

        public ActionResult Outstanding()
        {
            ViewBag.statusCode = new SelectList(_member.GetManagementStatus(), "statusCode", "statusName");
            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids(), "Name", "Name");
            var model = new OutstandingsMultiView();
            model.search = new OutstandingSearchModel();
            model.outstandings = new List<OutstandingsView>();
            model.outstandings = _admin.GetOutstandingScriptsView();
            return View(model);
        }
        [HttpPost]
        public ActionResult Outstanding(OutstandingsMultiView model)
        {
            ViewBag.statusCode = new SelectList(_member.GetManagementStatus(), "statusCode", "statusName", Request.Query["statusCode"].ToString());
            ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids(), "medicalAidCode", "Name", Request.Query["medicalAidID"].ToString());
            model.search.statusCode = Request.Query["statusCode"].ToString();
            model.search.medicalAidName = Request.Query["medicalAidID"].ToString();
            model.outstandings = _admin.GetOutstandingScriptsView();
            if (!String.IsNullOrEmpty(model.search.medicalAidName))
            {
                model.outstandings = model.outstandings.Where(x => x.schemeName == model.search.medicalAidName).ToList();
            }
            if (!String.IsNullOrEmpty(model.search.statusCode))
            {
                model.outstandings = model.outstandings.Where(x => x.status == model.search.statusCode).ToList();
            }
            if (!String.IsNullOrEmpty(model.search.expiryDate.ToString()))
            {
                model.outstandings = model.outstandings.Where(x => x.expiredDate > model.search.expiryDate).ToList();
            }
            if (model.search.nrMonths != 0)
            {
                model.outstandings = model.outstandings.Where(x => x.monthsOutstanding == model.search.nrMonths.ToString()).ToList();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ExportOutstandingScriptsToExcel()
        {
            var sb = new StringBuilder();
            var model = _admin.GetOutstandingScriptsView();
            if (Request.Query["sts"].ToString() != null)
            {
                if (!String.IsNullOrEmpty(Request.Query["sts"].ToString()))
                {
                    model = model.Where(x => x.status == Request.Query["sts"].ToString()).ToList();
                }
            }
            if (Request.Query["med"].ToString() != null)
            {
                if (!String.IsNullOrEmpty(Request.Query["med"].ToString()))
                {
                    model = model.Where(x => x.schemeName == Request.Query["med"].ToString()).ToList();
                }
            }
            if (Request.Query["nr"].ToString() != null)
            {
                if (!String.IsNullOrEmpty(Request.Query["nr"].ToString()))
                {
                    model = model.Where(x => x.monthsOutstanding == Request.Query["nr"].ToString()).ToList();
                }
            }
            if (Request.Query["expiry"].ToString() != null)
            {
                if (!String.IsNullOrEmpty(Request.Query["expiry"].ToString()))
                {
                    model = model.Where(x => x.expiredDate >= Convert.ToDateTime(Request.Query["expiry"].ToString())).ToList();
                }
            }
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=OutstandingScripts.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }



        [HttpPost]
        public ActionResult OutstandingAuthorise(Scripts model)
        {
            //open view
            return View(model);
        }

        [HttpPost]
        public ActionResult Recent(Scripts model)
        {
            //redirect to script view
            return View(model);
        }

        public ActionResult outstandingAuthorisations()
        {

            var model = _admin.GetOutstandingAuthorisedScripts();
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
                Response.Headers.Add("content-disposition", "attachment;filename=outstandingAuthorisations.xls");
                Response.ContentType = "application/xls";
                Response.Body.WriteAsync(content);
                Response.Body.Flush();
            }
            return View(model);

        }



    }
}