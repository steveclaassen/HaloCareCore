using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class CommunicationController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CommunicationController(OH17Context context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);
            _webHostEnvironment = webHostEnvironment;
        }

        public ActionResult ImportComminication()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    IFormFileCollection files = Request.Form.Files;
                    for (int i = 0; i < files.Count(); i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Form.Files[i].FileName);  

                        IFormFile file = files[i];
                        string fname;
                        fname = file.FileName;

                        // Get the complete folder path and store the file inside it.  
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        var filePath = Path.Combine(webRootPath, "~/Uploads/",fname);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            file.CopyToAsync(stream);
                        }

                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        [HttpPost]
        public ActionResult Import()
        {
            List<string> erorrows = new List<string>();

            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    IFormFileCollection files = Request.Form.Files;

                    for (int i = 0; i < files.Count(); i++)
                    {

                        IFormFile excelfile = files[i];

                        if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
                        {

                            string fileName = excelfile.FileName;
                            string fileContentType = excelfile.ContentType;
                            byte[] fileBytes = new byte[excelfile.Length];
                            //var data = excelfile.InputStream.Read(fileBytes, 0, Convert.ToInt32(excelfile.Length));

                            using (var package = new ExcelPackage(excelfile.OpenReadStream()))
                            {
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;
                                for (int row = 2; row <= noOfRow; row++)
                                {
                                    if (noOfCol == 11)
                                    {
                                        try
                                        {
                                            ImportCommunicationModel comunication = new ImportCommunicationModel();

                                            if (workSheet.Cells[row, 1].Value != null)
                                                comunication.Scheme = workSheet.Cells[row, 1].Value.ToString().Trim();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Account field is required");
                                                continue;
                                            }
                                            if (workSheet.Cells[row, 2].Value != null)
                                                comunication.MemberNumber = workSheet.Cells[row, 2].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Member Number field is required");
                                                continue;
                                            }

                                            if (workSheet.Cells[row, 3].Value != null)
                                                comunication.Depcode = workSheet.Cells[row, 3].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Dependent code field is required");
                                                continue;
                                            }

                                            if (workSheet.Cells[row, 4].Value != null)
                                                comunication.ProfileNumber = workSheet.Cells[row, 4].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Profile number field is required");
                                                continue;
                                            }
                                            if (workSheet.Cells[row, 5].Value != null)
                                                comunication.Type = workSheet.Cells[row, 5].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Communication type field is required");
                                                continue;
                                            }
                                            if (workSheet.Cells[row, 6].Value != null)
                                                comunication.Description = workSheet.Cells[row, 6].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Description communication field is required");
                                                continue;
                                            }

                                            if (workSheet.Cells[row, 7].Value != null)
                                                comunication.NoteType = workSheet.Cells[row, 7].Value.ToString();
                                            else
                                            {
                                                comunication.NoteType = "";
                                            }
                                            if (workSheet.Cells[row, 8].Value != null)
                                                comunication.Details = workSheet.Cells[row, 8].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Details field is required");
                                                continue;
                                            }
                                            if (workSheet.Cells[row, 9].Value != null)
                                                comunication.messageTo = workSheet.Cells[row, 9].Value.ToString();
                                            else
                                            {
                                                comunication.messageTo = "";
                                            }
                                            if (workSheet.Cells[row, 10].Value != null)
                                            {
                                                try
                                                {

                                                    comunication.DateSent = DateTime.ParseExact(workSheet.Cells[row, 10].Value.ToString(), "dd/MM/yyyy", null);  //Convert.ToDateTime(DateTime.FromOADate(double.Parse(workSheet.Cells[row, 10].Value.ToString())).ToString());

                                                }
                                                catch (Exception ex)
                                                {
                                                    erorrows.Add("Row " + row + " Incorrect date format | error message" + ex.Message);
                                                    continue;
                                                }
                                            }

                                            else
                                            {
                                                erorrows.Add("Row " + row + " Date sent field is required");
                                                continue;
                                            }
                                            if (workSheet.Cells[row, 11].Value != null)
                                                comunication.Program = workSheet.Cells[row, 11].Value.ToString();
                                            else
                                            {
                                                erorrows.Add("Row " + row + " Program field is required");
                                                continue;
                                            }

                                            comunication.Createdby = User.Identity.Name;


                                            #region validation
                                            var MemberModel = _member.GetMemberByMembershipNo(comunication.MemberNumber);

                                            if (MemberModel == null)
                                            {
                                                erorrows.Add("Row " + row + " Member number " + comunication.MemberNumber + " is not valid");
                                                continue;
                                            }
                                            var depandentModel = _member.GetDependantByMembershipDepCodeAidCode(comunication.MemberNumber, comunication.Depcode, comunication.Scheme).FirstOrDefault();
                                            if (depandentModel == null)
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " is not valid for Dep code " + comunication.Depcode + " and Account " + comunication.Scheme);
                                                continue;
                                            }

                                            comunication.DepedentID = depandentModel.DependantID;

                                            var MedicalModel = _member.GetMedicalAidByName(comunication.Scheme);

                                            if (MedicalModel == null)
                                            {
                                                erorrows.Add("Row " + row + " Account " + comunication.Scheme + " is not a valid");
                                                continue;
                                            }

                                            if (MemberModel.medicalAidID != MedicalModel.MedicalAidID)
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Dep code " + comunication.Depcode + " is not valid for " + comunication.Scheme + " Account");
                                                continue;
                                            }

                                            if (string.IsNullOrEmpty(_member.GetDependants(MemberModel.memberID, comunication.Depcode)))
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " is not valid for Dep code " + comunication.Depcode);
                                                continue;
                                            }
                                            if (comunication.Type.ToLower().Contains("notes") && string.IsNullOrEmpty(comunication.NoteType))
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Communication Type " + comunication.Type + " Note Type is a required field");
                                                continue;
                                            }

                                            if (!String.IsNullOrEmpty(comunication.NoteType))
                                            {
                                                if (!_member.GetNoteTypes().Where(x => x.noteDescription.Contains(comunication.NoteType)).Any())
                                                {
                                                    erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Notes type " + comunication.NoteType + "it is not valid");
                                                    continue;
                                                }
                                            }
                                            if ((comunication.Type.ToLower().Contains("sms") || comunication.Type.ToLower().Contains("email")))
                                            {
                                                if (String.IsNullOrEmpty(comunication.messageTo))
                                                {
                                                    erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Communication sent to is Required for communication type " + comunication.Type);
                                                    continue;
                                                }
                                            }
                                            if (comunication.Type.ToLower().Contains("diabetic diary") && !comunication.Program.ToLower().Contains("diabetes"))
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Diabetic diary only applicable to diabetes program only");
                                                continue;
                                            }
                                            comunication.ProgramId = _member.GetPrograms().Where(x => x.ProgramName == comunication.Program).Select(x => x.programID).FirstOrDefault();

                                            if (comunication.ProgramId == Guid.Empty)
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " is not in " + comunication.Program);
                                                continue;
                                            }
                                            if (comunication.Type.ToLower().Contains("sms") && comunication.messageTo.Substring(0, 1) != "0")
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Invalid Number format (Phone Number starting with 0 expected)");
                                                continue;
                                            }
                                            if (comunication.Type.ToLower().Contains("email") && !comunication.messageTo.Contains("@"))
                                            {
                                                erorrows.Add("Row " + row + " Member No " + comunication.MemberNumber + " Invalid Email format");
                                                continue;
                                            }

                                            #endregion

                                            var res = _member.ImportCommunication(comunication);

                                            if (!res)
                                                erorrows.Add("Row " + row + " Member number" + comunication.MemberNumber + " Invalid communication Type");
                                        }
                                        catch (Exception ex)
                                        {
                                            return Json(ex);
                                        }

                                    }
                                    else
                                    {
                                        erorrows.Add("Row " + row + " Incorrect number of columns");
                                    }
                                }

                            }

                        }
                        else
                        {
                            return Json(" Incorrect file fomrat");
                        }
                    }
                    if (erorrows.Count() == 0)
                        return Json(" File Uploaded Successfully!");
                    return Json(erorrows);

                }
                catch (Exception ex)
                {
                    return Json(" Internal Server error" + ex);
                }
            }
            else
            {
                return Json(" No files selected.");
            }


        }
    }
}