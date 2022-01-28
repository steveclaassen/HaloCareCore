using HaloCareCore.DAL;
using HaloCareCore.Extensions;
using HaloCareCore.Helpers;
using HaloCareCore.Management;
using HaloCareCore.Models;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using HaloCareCore.XmlModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;


namespace HaloCareCore.Controllers
{
    public class MemberController : Controller
    {
        // GET: Member
        private IMemberRepository _member;
        private IAdminRepository _admin;
        private IClinicalRepository _clinical;
        private IMedicalAidRepository _medical;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MemberController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, OH17Context context)
        {
            _member = new MemberRepository(configuration, context);
            _admin = new AdminRepository(context, configuration);
            _clinical = new ClinicalRepository(context, configuration);
            _medical = new MedicalAidRepository(configuration, context);//HCare-1197
        }

        //HCare-103
        //Added the WS call to the fillscheme function


        public ActionResult FillRest(Guid medId, Guid opt, string mem, string depcode)
        {
            var med = _member.GetMedicalAids().Where(x => x.MedicalAidID == medId).Select(x => x.medicalAidCode).FirstOrDefault();
            var claimcode = _member.GetClaimCodeByMedicalAidId(med);
            var jsonObj = new memberDetailsRequest()
            {
                memberDetails = new memberDetails()
                {
                    OriginID = "Halocare",
                    SchemeCode = claimcode,
                    FamilyId = mem,
                    DependantNo = depcode,
                    Option = "00",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(jsonObj);

            var url = "http://sectest.mediscor.co.za:1234/PUBLIC_API/WEBS1@10";
            //GET(url);
            var res = MemberValidationService.POSTREQ(url, jsonRequest);
            var result = res.DetailsResponse.result;

            var dependant = new Dependant()
            {
                firstName = result.Firstname,
                lastName = result.LastName,
                initials = result.Firstname.Substring(0, 1),
                idNumber = result.IDNumber,
                sex = result.Gender,
                title = result.Title,
                birthDate = Convert.ToDateTime(result.DateOfBirth),
            };

            if (result.Language == "E")
            {
                dependant.languageCode = "ENG";
            }
            else
            {
                dependant.languageCode = "AFR";
            }

            return Json(dependant);
        }


        public ActionResult FillScheme(Guid medId)
        {
            var options = _member.GetMedicalAidPlansByMedicalAidId(medId);
            return Json(options);
        }

        public ActionResult FillProgram(Guid medId)
        {
            var options = _member.GetAllowedProgramsPerScheme(medId);

            return Json(options);
        }


        //HCare-621
        public ActionResult FillTemplate(int tempId)
        {
            var options = _member.GetTemplateByID(tempId);
            return Json(options);
        }

        public ActionResult FillEmailTemplate(int tempId)
        {
            var options = _member.GetEmailTemplateByID(tempId);
            return Json(options);
        }

        public ActionResult AddReferenceScript(int id)
        {
            var model = _member.GetScript(id);
            var reference = new ScriptReferenceView();
            reference.script = new ScriptReferences();
            reference.attachments = new List<Attachments>();
            reference.script.reference = id;
            reference.script.dependantId = model.dependentID;
            reference.attachments = _member.GetAttachments(model.dependentID);
            return View(reference);
        }
        public ActionResult SyncSeries(Guid DependentID, Guid pro)
        {
            //HCare-745
            //pull the rest of the needed information for the WS call
            var dep = _member.GetDependantByDependantID(DependentID);
            var mem = _member.GetMemberByDependantID(DependentID);
            var med = _member.GetMedicalAids().Where(x => x.MedicalAidID == mem.medicalAidID).FirstOrDefault();
            var claimcode = _member.GetClaimCodeByMedicalAidId(med.medicalAidCode);
            var option = _member.GetPatientPlan(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();

            if (claimcode != null)
            {
                var jsonObj = new memberDetailsRequest()
                {
                    memberDetails = new memberDetails()
                    {
                        OriginID = "HALOCARE",
                        SchemeCode = claimcode,
                        FamilyId = mem.membershipNo,
                        DependantNo = dep.dependentCode,
                        Option = option,
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(jsonObj);

                var url = "https://apigw-prod.mediscor.co.za:8443/PROD/MBR_DET01";
                //GET(url);
                var res = MemberValidationService.POSTREQ(url, jsonRequest);
                if (res == null)
                {
                    return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
                }

                if (res == null) //HCare-1088
                {
                    return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
                }
                var result = res.DetailsResponse.result;

                if (result.MemberNumber == null)
                {
                    return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
                }

                var dependant = new MemberImports()
                {
                    medicalAidCode = result.SchemeName,
                    membershipNo = result.MemberNumber,
                    dependantCode = result.DependantNumber,
                    option = result.SchemeOption,
                    title = result.Title,
                    firstName = result.Firstname,
                    lastName = result.LastName,
                    gender = result.Gender,
                    language = result.Language,
                    dateOfBirth = result.DateOfBirth,
                    iDNumber = result.IDNumber,
                    addressLine1 = result.AddressLine1,
                    addressLine2 = result.AddressLine2,
                    city = result.City,
                    postalCode = result.PostalCode,
                    telephoneNumber = result.TelephoneNumber,
                    cellphone = result.Cellphone,
                    emailAddress = result.EmailAddress,
                    employerCode = result.EmployerCode,
                    createdBy = User.Identity.Name,
                    createdDate = DateTime.Now,
                    DependantId = DependentID,
                    optionStatus = result.OptionStatus,
                    memberStatus = result.MemberStatus,

                };

                //Hcare-1040
                if (med.titleSync)
                {
                    dep.firstName = result.Firstname;
                    dep.lastName = result.LastName;
                    dep.title = result.Title;

                    //HCare-1088
                    //if (!string.IsNullOrEmpty(result.IDNumber))
                    //{
                    //    dep.idNumber = result.IDNumber;
                    //}

                    dep.fullNameUC = null; //HCare-1088
                    _member.UpdateDependant(dep);
                }

                if (!string.IsNullOrEmpty(result.EligibilityStart))
                {
                    dependant.eligibilityStart = Convert.ToDateTime(result.EligibilityStart);
                }

                if (!string.IsNullOrEmpty(result.EligibilityEnd))
                {
                    dependant.eligibilityEnd = Convert.ToDateTime(result.EligibilityEnd);
                }

                _member.InsertMemberData(dependant);

                if (!string.IsNullOrEmpty(dependant.employerCode))
                {
                    var plan = _member.GetPayPoint(DependentID);
                    if (!String.IsNullOrEmpty(plan))
                    {
                        if (plan != dependant.employerCode)
                        {
                            _member.InsertPaypointHistory(new PayPointHistory()
                            {
                                active = true,
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependantId = DependentID,
                                effectiveDate = DateTime.Now,
                                planId = dependant.employerCode, /*HCare-923*/
                            });
                        }
                    }
                }

                //HCare-1318
                if (!string.IsNullOrEmpty(dependant.option))
                {
                    var plan = _member.GetPatientPlan(DependentID);
                    var planM = _member.GetMedicalAidPlans().Where(x => x.planCode == dependant.option).FirstOrDefault();
                    if (!String.IsNullOrEmpty(plan))
                    {
                        if (plan != dependant.option)
                        {
                            _member.InsertPlanHistory(new PatientPlanHistory()
                            {
                                active = true,
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependantId = DependentID,
                                effectiveDate = DateTime.Now,
                                planId = planM.Id,
                                planName = dependant.option
                            });
                        }
                    }
                }

                var info = new ComsViewModel();
                info = _member.GetAddressByDependentID(DependentID);

                //if (dep != null)
                //{
                //    if (dep.firstName.ToLower() != dependant.firstName.ToLower() || dep.lastName.ToLower() != dependant.lastName.ToLower())
                //    {
                //        dep.firstName = dependant.firstName;
                //        dep.title = dependant.title;
                //        dep.lastName = dependant.lastName;
                //        dep.modifiedBy = User.Identity.Name;
                //        dep.modifiedDate = DateTime.Now;
                //        var addressID = _member.UpdateDependant(dep);
                //    }
                //}

                if (info == null)
                {
                    var address = new Address()
                    {
                        line1 = dependant.addressLine1,
                        line2 = dependant.addressLine2,
                        city = dependant.city,
                        zip = dependant.postalCode,
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                    };

                    var addressID = _member.InsertAddress(address);
                    var meminfo = _member.GetMemberInformation(DependentID);
                    if (meminfo != null)
                    {
                        meminfo.addressID = addressID;
                        _member.UpdateMemberInformation(meminfo);
                    }
                    else
                    {
                        var minfo = new MemberInformation()
                        {
                            dependantID = DependentID,
                            addressID = addressID,
                            contactID = null,
                            memberID = mem.memberID,

                        };

                        _member.InsertMemberInformation(minfo);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(info.address.line1))
                    {
                        info.address.line1 = dependant.addressLine1;
                    }

                    if (String.IsNullOrEmpty(info.address.line2))
                    {
                        info.address.line2 = dependant.addressLine2;
                    }

                    if (String.IsNullOrEmpty(info.address.city))
                    {
                        info.address.city = dependant.city;
                    }
                    if (String.IsNullOrEmpty(info.address.zip))
                    {
                        info.address.zip = dependant.postalCode;
                    }

                    info.address.modifiedBy = User.Identity.Name;
                    info.address.modifiedDate = DateTime.Now;

                    _member.UpdateAddress(info.address);
                }

                var con = _member.GetContactByDependentID(DependentID);
                if (con == null)
                {
                    if (info == null)
                    {
                        var contact = new Contact()
                        {
                            cell = dependant.cellphone,
                            email = dependant.emailAddress,
                            landLine = dependant.telephoneNumber,
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                        };
                        //HCare 788
                        if (!string.IsNullOrEmpty(dependant.emailAddress))
                        {
                            contact.preferredMethodOfContact = "E";
                        }
                        else
                        {
                            contact.preferredMethodOfContact = "P";
                        }
                        var contactId = _member.InsertContact(contact);

                        var meminfo = _member.GetMemberInformation(DependentID);
                        if (meminfo != null)
                        {
                            meminfo.contactID = contactId;
                            _member.UpdateMemberInformation(meminfo);
                        }
                        else
                        {
                            var minfo = new MemberInformation()
                            {
                                dependantID = DependentID,
                                addressID = null,
                                contactID = contactId,
                                memberID = mem.memberID,
                            };

                            _member.InsertMemberInformation(minfo);
                        }
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(con.contacts.email))
                    {
                        con.contacts.email = dependant.emailAddress;
                    }

                    if (String.IsNullOrEmpty(con.contacts.cell))
                    {
                        con.contacts.cell = dependant.cellphone;
                    }

                    if (String.IsNullOrEmpty(con.contacts.landLine))
                    {
                        con.contacts.landLine = dependant.telephoneNumber;
                    }

                    con.contacts.modifiedBy = User.Identity.Name;
                    con.contacts.modifiedDate = DateTime.Now;

                    _member.UpdateContact(con.contacts);

                }

                var statushistory = _member.GetPatientStatus(DependentID);
                var programHistory = _member.GetProgramHistories(DependentID);

                if (programHistory.Count > 1)
                {
                    var programsLeft = programHistory.Where(x => x.programCode != programCode).ToList();
                    if (programsLeft.Count > 0)
                    {
                        foreach (var item in programsLeft)
                        {
                            var latestStatusEx = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == item.programCode).Where(x => x.active == true).ToList();
                            var programID = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.programID).FirstOrDefault();
                            if (result.MemberStatus.ToLower() != "active")
                            {
                                var history = new PatientManagementStatusHistory()
                                {
                                    dependantId = DependentID,
                                    createdDate = DateTime.Now,
                                    createdBy = User.Identity.Name,
                                    effectiveDate = dependant.eligibilityStart,
                                    endDate = DateTime.Now,
                                    active = true,
                                };

                                if (result.MemberStatus.ToLower() == "inactive")
                                {
                                    history.effectiveDate = DateTime.Now.AddDays(+1);
                                    history.endDate = DateTime.Now.AddDays(+1);
                                    var prog = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.code).FirstOrDefault();
                                    if (prog == "DIABD")
                                    {
                                        history.managementStatusCode = "DRD";
                                    }
                                    if (prog == "HIVPR")
                                    {
                                        history.managementStatusCode = "DRH";
                                    }
                                    if (prog == "MNTLH")//Hcare-1303
                                    {
                                        history.managementStatusCode = "DRM";
                                    }
                                    if (prog == "TBCUL")//Hcare-1303
                                    {
                                        history.managementStatusCode = "DRT";
                                    }
                                    if (latestStatusEx[0].codeStatus != "C")
                                    {
                                        if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRM" && latestStatusEx[0].managementStatusCode != "DRH")
                                        {
                                            _member.InsertManagementStatus(history);
                                        }

                                        if (latestStatusEx != null)
                                        {
                                            history = new PatientManagementStatusHistory()
                                            {
                                                dependantId = DependentID,
                                                createdDate = latestStatusEx[0].createdDate,
                                                createdBy = latestStatusEx[0].createdBy,
                                                effectiveDate = latestStatusEx[0].effectiveDate,
                                                endDate = DateTime.Now,
                                                active = latestStatusEx[0].active,
                                                managementStatusCode = latestStatusEx[0].managementStatusCode,
                                                id = latestStatusEx[0].id,
                                            };

                                            _member.UpdateManagementStatus(history);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dependant.eligibilityEnd < DateTime.Now)
                                {
                                    if (latestStatusEx[0].managementStatusCode != "INA")
                                    {
                                        var history = new PatientManagementStatusHistory()
                                        {
                                            dependantId = DependentID,
                                            createdDate = latestStatusEx[0].createdDate,
                                            createdBy = latestStatusEx[0].createdBy,
                                            effectiveDate = latestStatusEx[0].effectiveDate,
                                            endDate = DateTime.Now,
                                            active = latestStatusEx[0].active,
                                            managementStatusCode = latestStatusEx[0].managementStatusCode,
                                            id = latestStatusEx[0].id,
                                        };

                                        if (latestStatusEx[0].codeStatus != "C")
                                            if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRM" && latestStatusEx[0].managementStatusCode != "DRT")
                                                _member.UpdateManagementStatus(history);

                                        //HCare-745
                                        history.effectiveDate = DateTime.Now.AddDays(+1);
                                        history.endDate = DateTime.Now.AddDays(+1);
                                        var prog = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.code).FirstOrDefault();
                                        if (prog == "DIABD")
                                        {
                                            history.managementStatusCode = "DRD";
                                        }
                                        if (prog == "HIVPR")
                                        {
                                            history.managementStatusCode = "DRH";
                                        }
                                        if (prog == "MNTLH")//HCARE-1303
                                        {
                                            history.managementStatusCode = "DRM";
                                        }
                                        if (prog == "TBCUL")//Hcare-1303
                                        {
                                            history.managementStatusCode = "DRT";
                                        }
                                        if (latestStatusEx[0].codeStatus != "C")
                                        {
                                            if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRM" && latestStatusEx[0].managementStatusCode != "DRT")
                                            {
                                                history.createdBy = User.Identity.Name;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var history = new PatientManagementStatusHistory()
                                    {
                                        dependantId = DependentID,
                                        createdDate = DateTime.Now,
                                        createdBy = User.Identity.Name,
                                        effectiveDate = DateTime.Now,
                                        active = true,
                                    };


                                    if (latestStatusEx[0].codeStatus == "C" && (latestStatusEx[0].managementStatusCode == "DRH" || latestStatusEx[0].managementStatusCode == "DRD" || latestStatusEx[0].managementStatusCode != "DRM" || latestStatusEx[0].managementStatusCode != "DRT"))
                                    {
                                        var assignment = new AssignmentsView()
                                        {
                                            newAssignment = new Assignments()
                                            {
                                                createdBy = User.Identity.Name,
                                                createdDate = DateTime.Now,
                                                dependentID = DependentID,
                                                Active = true,
                                                effectiveDate = DateTime.Now,
                                                assignmentType = "INTER",
                                                status = "Open",
                                                programId = programID
                                            },

                                            assignmentItemType = "REI",

                                        };
                                        var AssignmentExists = _member.GetActivePatientAssignments(DependentID).Where(x => x.itemType == "REI").Where(x => x.program == programID).ToList();
                                        if (AssignmentExists.Count() == 0)
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

                                        var lastOpen = latestStatusEx.Where(x => x.codeStatus == "O").Where(x => x.programCode == item.programCode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                        if (lastOpen != null)
                                        {
                                            if (lastOpen.managementStatusCode != latestStatusEx[0].managementStatusCode)
                                            {
                                                var last = _member.GetManagementStatusById(lastOpen.id);
                                                history.managementStatusCode = last.managementStatusCode;
                                                history.effectiveDate = DateTime.Now;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                        else
                                        {
                                            var lastPending = latestStatusEx.Where(x => x.codeStatus == "P").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                            if (lastPending != null)
                                            {
                                                if (lastPending.managementStatusCode != latestStatusEx[0].managementStatusCode)
                                                {
                                                    var last = _member.GetManagementStatusById(lastPending.id);
                                                    history.managementStatusCode = last.managementStatusCode;
                                                    history.effectiveDate = DateTime.Now;
                                                    _member.InsertManagementStatus(history);
                                                }
                                            }
                                        }

                                    }
                                }

                            }
                        }
                    }
                }

                var latestStatus = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == programCode).Where(X => X.active == true).ToList();
                if (result != null)
                {
                    if (result.MemberStatus.ToLower() != "active")
                    {
                        var history = new PatientManagementStatusHistory()
                        {
                            dependantId = DependentID,
                            createdDate = DateTime.Now,
                            createdBy = User.Identity.Name,
                            effectiveDate = dependant.eligibilityStart,
                            endDate = DateTime.Now,
                            active = true,
                        };

                        var statushistoryLine = new PatientStatusHistory()
                        {
                            dependantId = DependentID,
                            createdDate = DateTime.Now,
                            createdBy = User.Identity.Name,
                            effectiveDate = DateTime.Now,
                            active = true,
                        };
                        if (result.MemberStatus.ToLower().Contains("waiting"))
                        {
                            statushistoryLine.statusCode = "WAI";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "WAI")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                        }
                        if (result.MemberStatus.ToLower() == "inactive")
                        {
                            statushistoryLine.statusCode = "INA";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "INA")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                            history.effectiveDate = DateTime.Now.AddDays(+1);
                            history.endDate = DateTime.Now.AddDays(+1);
                            var prog = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                            if (prog == "DIABD")
                            {
                                history.managementStatusCode = "DRD";
                            }
                            if (prog == "HIVPR")
                            {
                                history.managementStatusCode = "DRH";
                            }
                            if (prog == "TBCUL")
                            {
                                history.managementStatusCode = "DRT";
                            }
                            if (prog == "MNTLH")
                            {
                                history.managementStatusCode = "DRM";
                            }
                            if (latestStatus[0].codeStatus != "C")
                            {
                                if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRM" && latestStatus[0].managementStatusCode != "DRT")
                                {
                                    _member.InsertManagementStatus(history);
                                }

                                if (latestStatus != null)
                                {
                                    history = new PatientManagementStatusHistory()
                                    {
                                        dependantId = DependentID,
                                        createdDate = latestStatus[0].createdDate,
                                        createdBy = latestStatus[0].createdBy,
                                        effectiveDate = latestStatus[0].effectiveDate,
                                        endDate = DateTime.Now,
                                        active = latestStatus[0].active,
                                        managementStatusCode = latestStatus[0].managementStatusCode,
                                        id = latestStatus[0].id,
                                    };

                                    _member.UpdateManagementStatus(history);
                                }
                            }
                        }
                        if (result.MemberStatus.ToLower() == "suspended")
                        {
                            statushistoryLine.statusCode = "SUS";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "SUS")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                        }
                    }
                    else
                    {
                        var statushistoryLine = new PatientStatusHistory();

                        if (dependant.eligibilityEnd < DateTime.Now)
                        {
                            statushistoryLine.dependantId = DependentID;
                            statushistoryLine.createdDate = DateTime.Now;
                            statushistoryLine.createdBy = User.Identity.Name;
                            statushistoryLine.effectiveDate = DateTime.Now;//dependant.eligibilityStart;
                            statushistoryLine.active = true;
                            statushistoryLine.statusCode = "INA";

                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "INA")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }
                            if (latestStatus[0].managementStatusCode != "INA")
                            {
                                var history = new PatientManagementStatusHistory()
                                {
                                    dependantId = DependentID,
                                    createdDate = latestStatus[0].createdDate,
                                    createdBy = latestStatus[0].createdBy,
                                    effectiveDate = latestStatus[0].effectiveDate,
                                    endDate = DateTime.Now,
                                    active = latestStatus[0].active,
                                    managementStatusCode = latestStatus[0].managementStatusCode,
                                    id = latestStatus[0].id,
                                };
                                if (latestStatus[0].codeStatus != "C")
                                    if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRM" && latestStatus[0].managementStatusCode != "DRT")
                                        _member.UpdateManagementStatus(history);

                                //HCare-745
                                history.effectiveDate = DateTime.Now.AddDays(+1);
                                history.endDate = DateTime.Now.AddDays(+1);
                                var prog = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                                if (prog == "DIABD")
                                {
                                    history.managementStatusCode = "DRD";
                                }
                                if (prog == "HIVPR")
                                {
                                    history.managementStatusCode = "DRH";
                                }
                                if (prog == "TBCUL")
                                {
                                    history.managementStatusCode = "DRT";
                                }
                                if (prog == "MNTLH")
                                {
                                    history.managementStatusCode = "DRM";
                                }
                                if (latestStatus[0].codeStatus != "C")
                                {
                                    if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRT" && latestStatus[0].managementStatusCode != "DRM")
                                    {
                                        history.createdBy = User.Identity.Name;
                                        _member.InsertManagementStatus(history);
                                    }
                                }
                            }

                            //HCare-745 end
                        }
                        else
                        {
                            var statushistoryLin = new PatientStatusHistory()
                            {
                                dependantId = DependentID,
                                createdDate = DateTime.Now,
                                createdBy = User.Identity.Name,
                                effectiveDate = DateTime.Now,
                                active = true,
                                statusCode = "ACT"
                            };

                            if (statushistory == null || statushistory.statusCode != "ACT")
                                _member.InsertPatientStatus(statushistoryLin);

                            var history = new PatientManagementStatusHistory()
                            {
                                dependantId = DependentID,
                                createdDate = DateTime.Now,
                                createdBy = User.Identity.Name,
                                effectiveDate = DateTime.Now,
                                active = true,
                            };


                            if (latestStatus[0].codeStatus == "C" && (latestStatus[0].managementStatusCode == "DRH" || latestStatus[0].managementStatusCode == "DRD" || latestStatus[0].managementStatusCode == "DRT" || latestStatus[0].managementStatusCode == "DRM"))
                            {
                                var assignment = new AssignmentsView()
                                {
                                    newAssignment = new Assignments()
                                    {
                                        createdBy = User.Identity.Name,
                                        createdDate = DateTime.Now,
                                        dependentID = DependentID,
                                        Active = true,
                                        effectiveDate = DateTime.Now,
                                        assignmentType = "INTER",
                                        status = "Open",
                                        programId = pro
                                    },

                                    assignmentItemType = "REI",

                                };
                                var AssignmentExists = _member.GetActivePatientAssignments(DependentID).Where(x => x.itemType == "REI").ToList();
                                if (AssignmentExists.Count() == 0)
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
                                if (statushistory != null)
                                {
                                    var lastOpen = latestStatus.Where(x => x.codeStatus == "O").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                    if (lastOpen != null)
                                    {
                                        if (lastOpen.managementStatusCode != latestStatus[0].managementStatusCode)
                                        {
                                            var last = _member.GetManagementStatusById(lastOpen.id);
                                            history.managementStatusCode = last.managementStatusCode;
                                            history.effectiveDate = DateTime.Now;
                                            _member.InsertManagementStatus(history);
                                        }
                                    }
                                    else
                                    {
                                        var lastPending = latestStatus.Where(x => x.codeStatus == "P").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                        if (lastPending != null)
                                        {
                                            if (lastPending.managementStatusCode != latestStatus[0].managementStatusCode)
                                            {
                                                var last = _member.GetManagementStatusById(lastPending.id);
                                                history.managementStatusCode = last.managementStatusCode;
                                                history.effectiveDate = DateTime.Now;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }
                }
            }
            return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
        }

        public ActionResult SynciSeries(Guid DependentID, Guid pro)
        {
            //HCare-745
            //pull the rest of the needed information for the WS call
            var dep = _member.GetDependantByDependantID(DependentID);
            var mem = _member.GetMemberByDependantID(DependentID);
            var med = _member.GetMedicalAids().Where(x => x.MedicalAidID == mem.medicalAidID).FirstOrDefault();
            var claimcode = _member.GetClaimCodeByMedicalAidId(med.medicalAidCode);
            var option = _member.GetPatientPlan(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();

            if (claimcode != null)
            {
                var jsonObj = new memberDetailsRequest()
                {
                    memberDetails = new memberDetails()
                    {
                        OriginID = "HALOCARE",
                        SchemeCode = claimcode,
                        FamilyId = mem.membershipNo,
                        DependantNo = dep.dependentCode,
                        Option = option,
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(jsonObj);

                var url = " https://apigw-prod.mediscor.co.za:8443/PROD/MBR_DET01";
                //GET(url);
                var res = MemberValidationService.POSTREQ(url, jsonRequest);
                if (res == null)
                {
                    return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
                }

                var result = res.DetailsResponse.result;
                if (result.MemberNumber == null)
                {
                    return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
                }


                var dependant = new MemberImports()
                {
                    medicalAidCode = result.SchemeName,
                    membershipNo = result.MemberNumber,
                    dependantCode = result.DependantNumber,
                    option = result.SchemeOption,
                    title = result.Title,
                    firstName = result.Firstname,
                    lastName = result.LastName,
                    gender = result.Gender,
                    language = result.Language,
                    dateOfBirth = result.DateOfBirth,
                    iDNumber = result.IDNumber,
                    addressLine1 = result.AddressLine1,
                    addressLine2 = result.AddressLine2,
                    city = result.City,
                    postalCode = result.PostalCode,
                    telephoneNumber = result.TelephoneNumber,
                    cellphone = result.Cellphone,
                    emailAddress = result.EmailAddress,
                    employerCode = result.EmployerCode,
                    createdBy = User.Identity.Name,
                    createdDate = DateTime.Now,
                    DependantId = DependentID,
                    optionStatus = result.OptionStatus,
                    memberStatus = result.MemberStatus,
                };

                //Hcare-1040
                if (med.titleSync)
                {
                    dep.firstName = result.Firstname;
                    dep.lastName = result.LastName;
                    dep.title = result.Title;

                    //HCare-1088

                    //if (!string.IsNullOrEmpty(result.IDNumber))
                    //{
                    //    dep.idNumber = result.IDNumber;
                    //}

                    dep.fullNameUC = null; //HCare-1088
                    _member.UpdateDependant(dep);
                }

                if (!string.IsNullOrEmpty(result.EligibilityStart))
                {
                    dependant.eligibilityStart = Convert.ToDateTime(result.EligibilityStart);
                }

                if (!string.IsNullOrEmpty(result.EligibilityEnd))
                {
                    dependant.eligibilityEnd = Convert.ToDateTime(result.EligibilityEnd);
                }

                _member.InsertMemberData(dependant);

                if (!string.IsNullOrEmpty(dependant.employerCode))
                {
                    var plan = _member.GetPayPoint(DependentID);
                    if (!String.IsNullOrEmpty(plan))
                    {
                        if (plan != dependant.employerCode)
                        {
                            _member.InsertPaypointHistory(new PayPointHistory()
                            {
                                active = true,
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependantId = DependentID,
                                effectiveDate = DateTime.Now,
                                planId = dependant.employerCode, /*HCare-923*/
                            });
                        }
                    }
                }
                //HCare-1318
                if (!string.IsNullOrEmpty(dependant.option))
                {
                    var plan = _member.GetPatientPlan(DependentID);
                    var planM = _member.GetMedicalAidPlans().Where(x => x.planCode == dependant.option).FirstOrDefault();
                    if (!String.IsNullOrEmpty(plan))
                    {
                        if (plan != dependant.option)
                        {
                            _member.InsertPlanHistory(new PatientPlanHistory()
                            {
                                active = true,
                                createdBy = User.Identity.Name,
                                createdDate = DateTime.Now,
                                dependantId = DependentID,
                                effectiveDate = DateTime.Now,
                                planId = planM.Id,
                                planName = dependant.option
                            });
                        }
                    }
                }

                var info = new ComsViewModel();
                info = _member.GetAddressByDependentID(DependentID);

                //if (dep != null)
                //{
                //    if (dep.firstName.ToLower() != dependant.firstName.ToLower() || dep.lastName.ToLower() != dependant.lastName.ToLower())
                //    {
                //        dep.firstName = dependant.firstName;
                //        dep.lastName = dependant.lastName;
                //        dep.modifiedBy = User.Identity.Name;
                //        dep.modifiedDate = DateTime.Now;
                //        var addressID = _member.UpdateDependant(dep);
                //    }
                //}

                if (info == null)
                {
                    var address = new Address()
                    {
                        line1 = dependant.addressLine1,
                        line2 = dependant.addressLine2,
                        city = dependant.city,
                        zip = dependant.postalCode,
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                    };

                    var addressID = _member.InsertAddress(address);
                    var meminfo = _member.GetMemberInformation(DependentID);
                    if (meminfo != null)
                    {
                        meminfo.addressID = addressID;
                        _member.UpdateMemberInformation(meminfo);
                    }
                    else
                    {
                        var minfo = new MemberInformation()
                        {
                            dependantID = DependentID,
                            addressID = addressID,
                            contactID = null,
                            memberID = mem.memberID,

                        };

                        _member.InsertMemberInformation(minfo);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(info.address.line1))
                    {
                        info.address.line1 = dependant.addressLine1;
                    }

                    if (String.IsNullOrEmpty(info.address.line2))
                    {
                        info.address.line2 = dependant.addressLine2;
                    }

                    if (String.IsNullOrEmpty(info.address.city))
                    {
                        info.address.city = dependant.city;
                    }
                    if (String.IsNullOrEmpty(info.address.zip))
                    {
                        info.address.zip = dependant.postalCode;
                    }

                    info.address.modifiedBy = User.Identity.Name;
                    info.address.modifiedDate = DateTime.Now;

                    _member.UpdateAddress(info.address);
                }

                var con = _member.GetContactByDependentID(DependentID);
                if (con == null)
                {
                    if (info == null)
                    {
                        var contact = new Contact()
                        {
                            cell = dependant.cellphone,
                            email = dependant.emailAddress,
                            landLine = dependant.telephoneNumber,
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                        };
                        //HCare 788
                        if (!string.IsNullOrEmpty(dependant.emailAddress))
                        {
                            contact.preferredMethodOfContact = "E";
                        }
                        else
                        {
                            contact.preferredMethodOfContact = "P";
                        }
                        var contactId = _member.InsertContact(contact);

                        var meminfo = _member.GetMemberInformation(DependentID);
                        if (meminfo != null)
                        {
                            meminfo.contactID = contactId;
                            _member.UpdateMemberInformation(meminfo);
                        }
                        else
                        {
                            var minfo = new MemberInformation()
                            {
                                dependantID = DependentID,
                                addressID = null,
                                contactID = contactId,
                                memberID = mem.memberID,
                            };

                            _member.InsertMemberInformation(minfo);
                        }
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(con.contacts.email))
                    {
                        con.contacts.email = dependant.emailAddress;
                    }

                    if (String.IsNullOrEmpty(con.contacts.cell))
                    {
                        con.contacts.cell = dependant.cellphone;
                    }

                    if (String.IsNullOrEmpty(con.contacts.landLine))
                    {
                        con.contacts.landLine = dependant.telephoneNumber;
                    }

                    con.contacts.modifiedBy = User.Identity.Name;
                    con.contacts.modifiedDate = DateTime.Now;

                    _member.UpdateContact(con.contacts);

                }

                var statushistory = _member.GetPatientStatus(DependentID);
                var programHistory = _member.GetProgramHistories(DependentID);

                if (programHistory.Count > 1)
                {
                    var programsLeft = programHistory.Where(x => x.programCode != programCode).ToList();
                    if (programsLeft.Count > 0)
                    {
                        foreach (var item in programsLeft)
                        {
                            var programID = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.programID).FirstOrDefault();
                            var latestStatusEx = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == item.programCode).Where(x => x.active == true).ToList();
                            if (result.MemberStatus.ToLower() != "active")
                            {
                                var history = new PatientManagementStatusHistory()
                                {
                                    dependantId = DependentID,
                                    createdDate = DateTime.Now,
                                    createdBy = User.Identity.Name,
                                    effectiveDate = dependant.eligibilityStart,
                                    endDate = DateTime.Now,
                                    active = true,
                                };

                                if (result.MemberStatus.ToLower() == "inactive")
                                {
                                    history.effectiveDate = DateTime.Now.AddDays(+1);
                                    history.endDate = DateTime.Now.AddDays(+1);
                                    var prog = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.code).FirstOrDefault();
                                    if (prog == "DIABD")
                                    {
                                        history.managementStatusCode = "DRD";
                                    }
                                    if (prog == "HIVPR")
                                    {
                                        history.managementStatusCode = "DRH";
                                    }
                                    if (prog == "TBCUL")
                                    {
                                        history.managementStatusCode = "DRT";
                                    }
                                    if (prog == "MNTLH")
                                    {
                                        history.managementStatusCode = "DRM";
                                    }
                                    if (latestStatusEx[0].codeStatus != "C")
                                    {
                                        if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRM" && latestStatusEx[0].managementStatusCode != "DRT")
                                        {
                                            _member.InsertManagementStatus(history);
                                        }

                                        if (latestStatusEx != null)
                                        {
                                            history = new PatientManagementStatusHistory()
                                            {
                                                dependantId = DependentID,
                                                createdDate = latestStatusEx[0].createdDate,
                                                createdBy = latestStatusEx[0].createdBy,
                                                effectiveDate = latestStatusEx[0].effectiveDate,
                                                endDate = DateTime.Now,
                                                active = latestStatusEx[0].active,
                                                managementStatusCode = latestStatusEx[0].managementStatusCode,
                                                id = latestStatusEx[0].id,
                                            };

                                            _member.UpdateManagementStatus(history);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dependant.eligibilityEnd < DateTime.Now)
                                {
                                    if (latestStatusEx[0].managementStatusCode != "INA")
                                    {
                                        var history = new PatientManagementStatusHistory()
                                        {
                                            dependantId = DependentID,
                                            createdDate = latestStatusEx[0].createdDate,
                                            createdBy = latestStatusEx[0].createdBy,
                                            effectiveDate = latestStatusEx[0].effectiveDate,
                                            endDate = DateTime.Now,
                                            active = latestStatusEx[0].active,
                                            managementStatusCode = latestStatusEx[0].managementStatusCode,
                                            id = latestStatusEx[0].id,
                                        };

                                        if (latestStatusEx[0].codeStatus != "C")
                                            if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRT" && latestStatusEx[0].managementStatusCode != "DRM")
                                                _member.UpdateManagementStatus(history);

                                        //HCare-745
                                        history.effectiveDate = DateTime.Now.AddDays(+1);
                                        history.endDate = DateTime.Now.AddDays(+1);
                                        var prog = _member.GetPrograms().Where(x => x.code == item.programCode).Select(x => x.code).FirstOrDefault();
                                        if (prog == "DIABD")
                                        {
                                            history.managementStatusCode = "DRD";
                                        }
                                        if (prog == "HIVPR")
                                        {
                                            history.managementStatusCode = "DRH";
                                        }
                                        if (prog == "TBCUL")
                                        {
                                            history.managementStatusCode = "DRT";
                                        }
                                        if (prog == "MNTLH")
                                        {
                                            history.managementStatusCode = "DRM";
                                        }
                                        if (latestStatusEx[0].codeStatus != "C")
                                        {
                                            if (latestStatusEx[0].managementStatusCode != "DRH" && latestStatusEx[0].managementStatusCode != "DRD" && latestStatusEx[0].managementStatusCode != "DRM" && latestStatusEx[0].managementStatusCode != "DRT")
                                            {
                                                history.createdBy = User.Identity.Name;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var history = new PatientManagementStatusHistory()
                                    {
                                        dependantId = DependentID,
                                        createdDate = DateTime.Now,
                                        createdBy = User.Identity.Name,
                                        effectiveDate = DateTime.Now,
                                        active = true,
                                    };


                                    if (latestStatusEx[0].codeStatus == "C" && (latestStatusEx[0].managementStatusCode == "DRH" || latestStatusEx[0].managementStatusCode == "DRD" || latestStatusEx[0].managementStatusCode == "DRM" || latestStatusEx[0].managementStatusCode == "DRT"))
                                    {
                                        var assignment = new AssignmentsView()
                                        {
                                            newAssignment = new Assignments()
                                            {
                                                createdBy = User.Identity.Name,
                                                createdDate = DateTime.Now,
                                                dependentID = DependentID,
                                                Active = true,
                                                effectiveDate = DateTime.Now,
                                                assignmentType = "INTER",
                                                status = "Open",
                                                programId = programID
                                            },

                                            assignmentItemType = "REI",

                                        };
                                        var AssignmentExists = _member.GetActivePatientAssignments(DependentID).Where(x => x.itemType == "REI").Where(x => x.program == programID).ToList();
                                        if (AssignmentExists.Count() == 0)
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

                                        var lastOpen = latestStatusEx.Where(x => x.codeStatus == "O").Where(x => x.programCode == item.programCode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                        if (lastOpen != null)
                                        {
                                            if (lastOpen.managementStatusCode != latestStatusEx[0].managementStatusCode)
                                            {
                                                var last = _member.GetManagementStatusById(lastOpen.id);
                                                history.managementStatusCode = last.managementStatusCode;
                                                history.effectiveDate = DateTime.Now;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                        else
                                        {
                                            var lastPending = latestStatusEx.Where(x => x.codeStatus == "P").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                            if (lastPending != null)
                                            {
                                                if (lastPending.managementStatusCode != latestStatusEx[0].managementStatusCode)
                                                {
                                                    var last = _member.GetManagementStatusById(lastPending.id);
                                                    history.managementStatusCode = last.managementStatusCode;
                                                    history.effectiveDate = DateTime.Now;
                                                    _member.InsertManagementStatus(history);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

                var latestStatus = _member.GetManagmentStatusInformation(DependentID).Where(x => x.programCode == programCode).Where(X => X.active == true).ToList();
                if (result != null)
                {
                    if (result.MemberStatus.ToLower() != "active")
                    {
                        var history = new PatientManagementStatusHistory()
                        {
                            dependantId = DependentID,
                            createdDate = DateTime.Now,
                            createdBy = User.Identity.Name,
                            effectiveDate = dependant.eligibilityStart,
                            endDate = DateTime.Now,
                            active = true,
                        };

                        var statushistoryLine = new PatientStatusHistory()
                        {
                            dependantId = DependentID,
                            createdDate = DateTime.Now,
                            createdBy = User.Identity.Name,
                            effectiveDate = DateTime.Now,
                            active = true,
                        };
                        if (result.MemberStatus.ToLower().Contains("waiting"))
                        {
                            statushistoryLine.statusCode = "WAI";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "WAI")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                        }
                        if (result.MemberStatus.ToLower() == "inactive")
                        {
                            statushistoryLine.statusCode = "INA";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "INA")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                            history.effectiveDate = DateTime.Now.AddDays(+1);
                            history.endDate = DateTime.Now.AddDays(+1);
                            var prog = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                            if (prog == "DIABD")
                            {
                                history.managementStatusCode = "DRD";
                            }
                            if (prog == "HIVPR")
                            {
                                history.managementStatusCode = "DRH";
                            }
                            if (prog == "TBCUL")
                            {
                                history.managementStatusCode = "DRT";
                            }
                            if (prog == "MNTLH")
                            {
                                history.managementStatusCode = "DRM";
                            }
                            if (latestStatus[0].codeStatus != "C")
                            {
                                if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRM" && latestStatus[0].managementStatusCode != "DRT")
                                {
                                    _member.InsertManagementStatus(history);
                                }

                                if (latestStatus != null)
                                {
                                    history = new PatientManagementStatusHistory()
                                    {
                                        dependantId = DependentID,
                                        createdDate = latestStatus[0].createdDate,
                                        createdBy = latestStatus[0].createdBy,
                                        effectiveDate = latestStatus[0].effectiveDate,
                                        endDate = DateTime.Now,
                                        active = latestStatus[0].active,
                                        managementStatusCode = latestStatus[0].managementStatusCode,
                                        id = latestStatus[0].id,
                                    };

                                    _member.UpdateManagementStatus(history);
                                }
                            }
                        }
                        if (result.MemberStatus.ToLower() == "suspended")
                        {
                            statushistoryLine.statusCode = "SUS";
                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "SUS")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }

                        }
                    }
                    else
                    {
                        var statushistoryLine = new PatientStatusHistory();

                        if (dependant.eligibilityEnd < DateTime.Now)
                        {
                            statushistoryLine.dependantId = DependentID;
                            statushistoryLine.createdDate = DateTime.Now;
                            statushistoryLine.createdBy = User.Identity.Name;
                            statushistoryLine.effectiveDate = DateTime.Now;//dependant.eligibilityStart;
                            statushistoryLine.active = true;
                            statushistoryLine.statusCode = "INA";

                            if (statushistory != null)
                            {
                                if (statushistory.statusCode != "INA")
                                    _member.InsertPatientStatus(statushistoryLine);
                            }
                            else
                            {
                                _member.InsertPatientStatus(statushistoryLine);
                            }
                            if (latestStatus[0].managementStatusCode != "INA")
                            {
                                var history = new PatientManagementStatusHistory()
                                {
                                    dependantId = DependentID,
                                    createdDate = latestStatus[0].createdDate,
                                    createdBy = latestStatus[0].createdBy,
                                    effectiveDate = latestStatus[0].effectiveDate,
                                    endDate = DateTime.Now,
                                    active = latestStatus[0].active,
                                    managementStatusCode = latestStatus[0].managementStatusCode,
                                    id = latestStatus[0].id,
                                };
                                if (latestStatus[0].codeStatus != "C")
                                    if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRM" && latestStatus[0].managementStatusCode != "DRT")
                                        _member.UpdateManagementStatus(history);

                                //HCare-745
                                history.effectiveDate = DateTime.Now.AddDays(+1);
                                history.endDate = DateTime.Now.AddDays(+1);
                                var prog = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                                if (prog == "DIABD")
                                {
                                    history.managementStatusCode = "DRD";
                                }
                                if (prog == "HIVPR")
                                {
                                    history.managementStatusCode = "DRH";
                                }
                                if (prog == "MNTLH")
                                {
                                    history.managementStatusCode = "DRM";
                                }
                                if (prog == "TBCUL")
                                {
                                    history.managementStatusCode = "DRT";
                                }
                                if (latestStatus[0].codeStatus != "C")
                                {
                                    if (latestStatus[0].managementStatusCode != "DRH" && latestStatus[0].managementStatusCode != "DRD" && latestStatus[0].managementStatusCode != "DRM" && latestStatus[0].managementStatusCode != "DRT")
                                    {
                                        history.createdBy = User.Identity.Name;
                                        _member.InsertManagementStatus(history);
                                    }
                                }
                            }

                            //HCare-745 end
                        }
                        else
                        {
                            var statushistoryLin = new PatientStatusHistory()
                            {
                                dependantId = DependentID,
                                createdDate = DateTime.Now,
                                createdBy = User.Identity.Name,
                                effectiveDate = DateTime.Now,
                                active = true,
                                statusCode = "ACT"
                            };

                            if (statushistory == null || statushistory.statusCode != "ACT")
                                _member.InsertPatientStatus(statushistoryLin);

                            var history = new PatientManagementStatusHistory()
                            {
                                dependantId = DependentID,
                                createdDate = DateTime.Now,
                                createdBy = User.Identity.Name,
                                effectiveDate = DateTime.Now,
                                active = true,
                            };


                            if (latestStatus[0].codeStatus == "C" && (latestStatus[0].managementStatusCode == "DRH" || latestStatus[0].managementStatusCode == "DRD" || latestStatus[0].managementStatusCode == "DRT" || latestStatus[0].managementStatusCode == "DRM"))
                            {
                                var assignment = new AssignmentsView()
                                {
                                    newAssignment = new Assignments()
                                    {
                                        createdBy = User.Identity.Name,
                                        createdDate = DateTime.Now,
                                        dependentID = DependentID,
                                        Active = true,
                                        effectiveDate = DateTime.Now,
                                        assignmentType = "INTER",
                                        status = "Open",
                                        programId = pro
                                    },

                                    assignmentItemType = "REI",

                                };
                                var AssignmentExists = _member.GetActivePatientAssignments(DependentID).Where(x => x.itemType == "REI").ToList();
                                if (AssignmentExists.Count() == 0)
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
                                if (statushistory != null)
                                {
                                    var lastOpen = latestStatus.Where(x => x.codeStatus == "O").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                    if (lastOpen != null)
                                    {
                                        if (lastOpen.managementStatusCode != latestStatus[0].managementStatusCode)
                                        {
                                            var last = _member.GetManagementStatusById(lastOpen.id);
                                            history.managementStatusCode = last.managementStatusCode;
                                            history.effectiveDate = DateTime.Now;
                                            _member.InsertManagementStatus(history);
                                        }
                                    }
                                    else
                                    {
                                        var lastPending = latestStatus.Where(x => x.codeStatus == "P").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
                                        if (lastPending != null)
                                        {
                                            if (lastPending.managementStatusCode != latestStatus[0].managementStatusCode)
                                            {
                                                var last = _member.GetManagementStatusById(lastPending.id);
                                                history.managementStatusCode = last.managementStatusCode;
                                                history.effectiveDate = DateTime.Now;
                                                _member.InsertManagementStatus(history);
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }
                }
            }
            return RedirectToAction("patientDashboard", new { DependentId = DependentID, pro = pro });
        }

        [HttpPost]
        public ActionResult AddReferenceScript(ScriptReferenceView model)
        {

            model.script.createdBy = User.Identity.Name;
            //upload script
            var scriptfile = Request.Form.Files["file"];
            if (scriptfile.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                string filename = Path.GetFileName(Request.Form.Files["file"].FileName);
                model.script.attachment = model.script.reference + "_" + filename;
                var filePath = Path.Combine(path, model.script.reference + "_" + filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    scriptfile.CopyToAsync(stream);
                }

                var att = new Attachments()
                {
                    attachmentName = model.script.reference + "_" + filename,
                    Active = true,
                    createdBy = model.script.createdBy,
                    Link = model.script.attachment,
                    attachmentType = "MS1",
                    createdDate = DateTime.Now,
                    dependentID = model.script.dependantId,

                };
                _member.InsertAttachment(att);
                _member.InsertScriptReference(model.script);
                return RedirectToAction("patientClinical", new { DependentId = model.script.dependantId });
            }

            foreach (var row in model.attachments)
            {
                if (Request.Query[row.attachmentID.ToString()].ToString() != null)
                {
                    var checkIt = Request.Query[row.attachmentID.ToString()].ToString();
                    if (checkIt == "true,false")
                    {
                        model.script.attachment = row.Link;
                    }
                }
            }

            _member.InsertScriptReference(model.script);
            return RedirectToAction("patientClinical", new { DependentId = model.script.dependantId });
        }

        public ActionResult FillType(string queryType)
        {
            var options = _member.GetQueryTypesBySource(queryType);
            return Json(options);
        }

        public ActionResult FillIcd10(string code)
        {
            var options = _member.GetIcd10CodesByCode(code);
            return Json(options);
        }

        public ActionResult FillProduct(string prodName)
        {
            var options = _member.GetActiveProductsByName(prodName);
            return Json(options);
        }

        public JsonResult GetProducts(string Prefix)
        {
            var prod = _admin.GetProducts();
            var Products = (from p in prod
                            where p.productName.StartsWith(Prefix)
                            select new { p.productName, p.nappiCode });

            return Json(Products);
        }

        public ActionResult _AutoCompleteAjaxLoading(string Prefix)
        {
            var data = new[] { "item 1", "item 2", "item 3" };
            return Json(data);
        }



        public ActionResult CaseManagerHistory(Guid DependentID)
        {
            var model = _admin.GetCaseManagerHistories(DependentID);
            return View(model);
        }

        public ActionResult AddContactInformation(Guid DependentID) //HCare-692
        {
            ViewBag.depId = DependentID;
            ViewBag.pmoc = new SelectList(_admin.GetListofPMOC().Where(x => x.active == true), "pmocCode", "pmocDescription");
            return View();
        }
        [HttpPost]
        public ActionResult AddContactInformation(ComsViewModel model) //HCare-692
        {
            //Contact
            model.memberInformation.Contacts.createdDate = DateTime.Now;
            model.memberInformation.Contacts.createdBy = User.Identity.Name;
            model.memberInformation.Contacts.Active = true;

            if (model.memberInformation.Contacts.cell != null)
            {
                model.memberInformation.Contacts.cell = Request.Query["memberInformation.Contacts.cell"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesCell != null)
            {
                model.memberInformation.Contacts.ISeriesCell = Request.Query["memberInformation.Contacts.ISeriesCell"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.landLine != null)
            {
                model.memberInformation.Contacts.landLine = Request.Query["memberInformation.Contacts.landLine"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesLandLine != null)
            {
                model.memberInformation.Contacts.ISeriesLandLine = Request.Query["memberInformation.Contacts.ISeriesLandLine"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.workNo != null)
            {
                model.memberInformation.Contacts.workNo = Request.Query["memberInformation.Contacts.workNo"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesWorkNo != null)
            {
                model.memberInformation.Contacts.ISeriesWorkNo = Request.Query["memberInformation.Contacts.ISeriesWorkNo"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.fax != null)
            {
                model.memberInformation.Contacts.fax = Request.Query["memberInformation.Contacts.fax"].ToString().Replace(" ", ""); //HCare-678

            }
            model.memberInformation.Contacts.preferredMethodOfContact = Request.Query["pmoc"].ToString(); //HCare-692

            var contactResult = _member.InsertContact(model.memberInformation.Contacts);

            var oldmember = _member.GetMemberInformation(new Guid(Request.Query["depId"].ToString()));
            if (oldmember != null)
            {
                oldmember.contactID = contactResult;
                _member.UpdateMemberInformation(oldmember);
            }
            else
            {
                var meminfo = new MemberInformation();
                meminfo.dependantID = new Guid(Request.Query["depId"].ToString());
                meminfo.contactID = contactResult;
                var response = _member.InsertMemberInformation(meminfo);
            }


            //Address
            model.memberInformation.Address.createdDate = DateTime.Now;
            model.memberInformation.Address.createdBy = User.Identity.Name;
            model.memberInformation.Address.Active = true;

            var addressResult = _member.InsertAddress(model.memberInformation.Address);
            var res = _member.GetEnrolmentStep(new Guid(Request.Query["depId"].ToString()));
            if (res != null)
            {
                res.demographicCaptured = true;
                var update = _member.UpdateEnrolmentStep(res);
            }

            oldmember = _member.GetMemberInformation(new Guid(Request.Query["depId"].ToString()));
            if (oldmember != null)
            {
                oldmember.addressID = addressResult;
                _member.UpdateMemberInformation(oldmember);
            }
            else
            {
                var meminfo = new MemberInformation();
                meminfo.dependantID = new Guid(Request.Query["depId"].ToString());
                meminfo.addressID = addressResult;
                meminfo.Address.Active = true;
                var response = _member.InsertMemberInformation(meminfo);
            }

            ViewBag.pmoc = new SelectList(_admin.GetListofPMOC(), "pmocCode", "pmocDescription");

            return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["depId"].ToString()), pro = Request.Query["pro"] });

        }


        public ActionResult EditContactInformation(Guid DependentID) //HCare-692
        {
            ViewBag.pmoc = new SelectList(_admin.GetListofPMOC().Where(x => x.active == true), "pmocCode", "pmocDescription");
            var model = _member.GetContactByDependentID(DependentID);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditContactInformation(ComsViewModel model) //HCare-692
        {
            //Contact
            model.memberInformation.Contacts.modifiedBy = User.Identity.Name;
            model.memberInformation.Contacts.modifiedDate = DateTime.Now;
            model.memberInformation.Contacts.Active = true;

            if (model.memberInformation.Contacts.cell != null)
            {
                model.memberInformation.Contacts.cell = Request.Query["memberInformation.Contacts.cell"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesCell != null)
            {
                model.memberInformation.Contacts.ISeriesCell = Request.Query["memberInformation.Contacts.ISeriesCell"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.landLine != null)
            {
                model.memberInformation.Contacts.landLine = Request.Query["memberInformation.Contacts.landLine"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesLandLine != null)
            {
                model.memberInformation.Contacts.ISeriesLandLine = Request.Query["memberInformation.Contacts.ISeriesLandLine"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.workNo != null)
            {
                model.memberInformation.Contacts.workNo = Request.Query["memberInformation.Contacts.workNo"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesWorkNo != null)
            {
                model.memberInformation.Contacts.ISeriesWorkNo = Request.Query["memberInformation.Contacts.ISeriesWorkNo"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.fax != null)
            {
                model.memberInformation.Contacts.fax = Request.Query["memberInformation.Contacts.fax"].ToString().Replace(" ", ""); //HCare-678

            }
            model.memberInformation.Contacts.preferredMethodOfContact = Request.Query["memberInformation.Contacts.preferredMethodOfContact"].ToString(); //HCare-692

            var contactUpdate = _member.UpdateContact_MI(model);

            //Address
            model.memberInformation.Address.modifiedBy = User.Identity.Name;
            model.memberInformation.Address.modifiedDate = DateTime.Now;

            var result = _member.UpdateAddress_MI(model);

            ViewBag.pmoc = new SelectList(_admin.GetListofPMOC(), "pmocCode", "pmocDescription");


            return RedirectToAction("patientCommunication", new { DependentId = model.memberInformation.dependantID, pro = Request.Query["pro"] });
        }

        //public ActionResult AddContact(Guid DependentID) //HCare-598
        //{
        //    ViewBag.depId = DependentID;
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult AddContact(ComsViewModel model) //HCare-598
        //{
        //    model.memberInformation.Contacts.createdDate = DateTime.Now;
        //    model.memberInformation.Contacts.createdBy = User.Identity.Name;
        //    model.memberInformation.Contacts.Active = true;

        //    if (model.memberInformation.Contacts.cell != null)
        //    {
        //        model.memberInformation.Contacts.cell = Request.Query["memberInformation.Contacts.cell"].ToString().Replace(" ", ""); //HCare-678

        //    }
        //    if (model.memberInformation.Contacts.landLine != null)
        //    {
        //        model.memberInformation.Contacts.landLine = Request.Query["memberInformation.Contacts.landLine"].ToString().Replace(" ", ""); //HCare-678

        //    }
        //    if (model.memberInformation.Contacts.workNo != null)
        //    {
        //        model.memberInformation.Contacts.workNo = Request.Query["memberInformation.Contacts.workNo"].ToString().Replace(" ", ""); //HCare-678

        //    }
        //    if (model.memberInformation.Contacts.fax != null)
        //    {
        //        model.memberInformation.Contacts.fax = Request.Query["memberInformation.Contacts.fax"].ToString().Replace(" ", ""); //HCare-678

        //    }
        //    var result = _member.InsertContact(model.memberInformation.Contacts);
        //    if (result != null)
        //    {
        //        var oldmember = _member.GetMemberInformation(new Guid(Request.Query["depId"].ToString()));
        //        if (oldmember != null)
        //        {
        //            oldmember.contactID = result;
        //            _member.UpdateMemberInformation(oldmember);
        //        }
        //        else
        //        {
        //            var meminfo = new MemberInformation();
        //            meminfo.dependantID = new Guid(Request.Query["depId"].ToString());
        //            meminfo.contactID = result;
        //            var response = _member.InsertMemberInformation(meminfo);
        //        }
        //    }
        //    return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["depId"].ToString()) });
        //}

        //public ActionResult AddAddress(Guid DependentID)
        //{
        //    ViewBag.depId = DependentID;
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult AddAddress(ComsViewModel model)
        //{
        //    model.memberInformation.Address.createdDate = DateTime.Now;
        //    model.memberInformation.Address.createdBy = User.Identity.Name;
        //    model.memberInformation.Address.Active = true;

        //    var result = _member.InsertAddress(model.memberInformation.Address);
        //    var res = _member.GetEnrolmentStep(new Guid(Request.Query["depId"].ToString()));
        //    if (res != null)
        //    {
        //        res.demographicCaptured = true;
        //        var update = _member.UpdateEnrolmentStep(res);
        //    }

        //    if (result != null)
        //    {
        //        var oldmember = _member.GetMemberInformation(new Guid(Request.Query["depId"].ToString()));
        //        if (oldmember != null)
        //        {
        //            oldmember.addressID = result;
        //            _member.UpdateMemberInformation(oldmember);
        //        }
        //        else
        //        {
        //            var meminfo = new MemberInformation();
        //            meminfo.dependantID = new Guid(Request.Query["depId"].ToString());
        //            meminfo.addressID = result;
        //            meminfo.Address.Active = true;
        //            var response = _member.InsertMemberInformation(meminfo);
        //        }
        //    }
        //    return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["depId"].ToString()) });
        //} 

        //HCare-598
        //Add Contact from PatientCommunication
        public ActionResult PatientCommunication_Contact_Add(Guid Id)
        {
            ViewBag.depId = Id;
            return View();
        } //HCare-598

        [HttpPost]
        public ActionResult PatientCommunication_Contact_Add(ComsViewModel model) //HCare-595
        {
            model.memberInformation.Contacts.createdDate = DateTime.Now;
            model.memberInformation.Contacts.createdBy = User.Identity.Name;
            if (model.memberInformation.Contacts.cell != null)
            {
                model.memberInformation.Contacts.cell = model.memberInformation.Contacts.cell.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.landLine != null)
            {
                model.memberInformation.Contacts.landLine = model.memberInformation.Contacts.landLine.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.workNo != null)
            {
                model.memberInformation.Contacts.workNo = model.memberInformation.Contacts.workNo.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.fax != null)
            {
                model.memberInformation.Contacts.fax = model.memberInformation.Contacts.fax.Replace(" ", ""); //HCare-678
            }

            var result = _member.InsertContact(model.memberInformation.Contacts);
            //var oldmember = _member.GetMemberInformation(new Guid(Request.Query["depId"].ToString()));
            var oldmember = _member.GetMemberInformation(model.memberInformation.dependantID);
            if (oldmember != null)
            {
                oldmember.contactID = result;
                _member.UpdateMemberInformation(oldmember);
            }
            else
            {
                var meminfo = new MemberInformation();
                meminfo.dependantID = new Guid(Request.Query["depId"].ToString());
                meminfo.contactID = result;
                var response = _member.InsertMemberInformation(meminfo);
            }

            return RedirectToAction("patientDashboard", new { DependentId = new Guid(Request.Query["depId"].ToString()), pro = new Guid(Request.Query["pro"].ToString()) });

        }

        public ActionResult EditAddress(Guid DependentID)
        {
            var model = _member.GetAddressByDependentID(DependentID);
            return View(model);
        }

        //[HttpPost]
        //public ActionResult EditAddress(Address model)
        //{
        //    model.modifiedBy = User.Identity.Name;
        //    model.modifiedDate = DateTime.Now;
        //    var result = _member.UpdateAddress(model);
        //    return View(model);
        //}

        [HttpPost]
        public ActionResult EditAddress(ComsViewModel model)
        {
            model.memberInformation.Address.modifiedBy = User.Identity.Name;
            model.memberInformation.Address.modifiedDate = DateTime.Now;

            var result = _member.UpdateAddress_MI(model);

            return RedirectToAction("patientCommunication", new { DependentId = model.memberInformation.dependantID });
        }

        public ActionResult EditContact(Guid DependentID)
        {
            var model = _member.GetContactByDependentID(DependentID);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditContact(ComsViewModel model)
        {
            model.memberInformation.Contacts.modifiedBy = User.Identity.Name;
            model.memberInformation.Contacts.modifiedDate = DateTime.Now;
            model.memberInformation.Contacts.Active = true;

            if (model.memberInformation.Contacts.cell != null)
            {
                model.memberInformation.Contacts.cell.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.landLine != null)
            {
                model.memberInformation.Contacts.landLine.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.workNo != null)
            {
                model.memberInformation.Contacts.workNo.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesCell != null)
            {
                model.memberInformation.Contacts.ISeriesCell.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesLandLine != null)
            {
                model.memberInformation.Contacts.ISeriesLandLine.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.ISeriesWorkNo != null)
            {
                model.memberInformation.Contacts.ISeriesWorkNo.Replace(" ", ""); //HCare-678

            }
            if (model.memberInformation.Contacts.fax != null)
            {
                model.memberInformation.Contacts.fax.Replace(" ", ""); //HCare-678

            }
            var result = _member.UpdateContact_MI(model);

            return RedirectToAction("patientCommunication", new { DependentId = model.memberInformation.dependantID });
        }

        //Edit Contact from Task
        public ActionResult ContactDetails_Edit(Guid id)
        {
            var model = _member.GetContactById(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult ContactDetails_Edit(Contact model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            if (model.cell != null)
            {
                model.cell = Request.Query["cell"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.landLine != null)
            {
                model.landLine = Request.Query["landLine"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.workNo != null)
            {
                model.workNo = Request.Query["workNo"].ToString().Replace(" ", ""); //HCare-678

            }
            if (model.fax != null)
            {
                model.fax = Request.Query["fax"].ToString().Replace(" ", ""); //HCare-678

            }
            var result = _member.UpdateContact(model);

            var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
            task.closed = true;
            task.closedBy = User.Identity.Name;
            task.closedDate = DateTime.Now;
            task.status = "Closed";
            _clinical.UpdateTask(task);

            return RedirectToAction("AssignmentDetails", "Clinical", new { id = Request.Query["id"] });
        }

        //Edit Contact from PatientCommunication
        public ActionResult PatientCommunication_Contact_Edit(Guid id)
        {
            var model = _member.GetContact(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult PatientCommunication_Contact_Edit(ComsViewModel model) //HCare-595
        {
            if (model.memberInformation.Contacts.cell != null)
            {
                model.memberInformation.Contacts.cell = model.memberInformation.Contacts.cell.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.landLine != null)
            {
                model.memberInformation.Contacts.landLine = model.memberInformation.Contacts.landLine.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.workNo != null)
            {
                model.memberInformation.Contacts.workNo = model.memberInformation.Contacts.workNo.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.fax != null)
            {
                model.memberInformation.Contacts.fax = model.memberInformation.Contacts.fax.Replace(" ", ""); //HCare-678
            }
            if (model.memberInformation.Contacts.email != null)
            {
                model.memberInformation.Contacts.email = model.memberInformation.Contacts.email; //HCare-678
            }

            model.memberInformation.Contacts.modifiedBy = User.Identity.Name;
            model.memberInformation.Contacts.modifiedDate = DateTime.Now;

            var result = _member.UpdateContact_MI(model);
            return RedirectToAction("patientCommunication", new { DependentId = model.memberInformation.dependantID });
        }
        #region HCare-1061
        public ActionResult DiabeticDairy(Guid DependentID, Guid pro)
        {
            var model = new DiabeticDairyViewModel()
            {
                DiabeticDairy = new DiabeticDairy()
                {
                    dependentID = DependentID
                }

            };

            return View(model);
        }
        [HttpPost]
        public ActionResult DiabeticDairySave(DiabeticDairy diabeticDairy)
        {
            diabeticDairy.programId = new Guid(Request.Query["pro"]); //HCare-1254
            diabeticDairy.createdBy = User.Identity.Name;
            if (!ModelState.IsValid)
                return View("DiabeticDairy", diabeticDairy);

            if (diabeticDairy.dairyId == 0)
            {
                diabeticDairy.assignementSent = false;
                diabeticDairy.createdBy = User.Identity.Name;
                diabeticDairy.createdDate = DateTime.Now;
                var time = DateTime.Now.TimeOfDay;
                var modifiedDate = diabeticDairy.sentDate.Add(time); //HCare-1488
                diabeticDairy.sentDate = modifiedDate;
                _member.InsertDiabeticDairy(diabeticDairy);
            }
            else
            {

                diabeticDairy.modifiedBy = User.Identity.Name;
                diabeticDairy.modifiedDate = DateTime.Now;
                var time = DateTime.Now.TimeOfDay;
                var modifiedDate = diabeticDairy.sentDate.Add(time); //HCare-1488
                diabeticDairy.sentDate = modifiedDate;

                _member.UpdateDiabeticDairy(diabeticDairy);
            }


            return RedirectToAction("patientCommunication", new { DependentID = diabeticDairy.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult DiabeticDairy_Edit(int dairyId, Guid? pro)
        {
            var diabeticDairy = _member.GetDiabeticDairy().Where(x => x.dairyId == dairyId).SingleOrDefault();

            if (diabeticDairy == null)
                return NotFound();

            var viewModel = new DiabeticDairyViewModel()
            {
                DiabeticDairy = diabeticDairy
            };

            return View(viewModel);
        }
        #endregion

        public ActionResult Enquiries(Guid DependentID, Guid pro)
        {
            var list = _member.GetQueryTypes();

            var model = new Queries()
            {
                dependentID = DependentID
            };

            ViewBag.EnquiryType = new SelectList(_member.GetQueryTypes(), "code", "queryDescription");
            ViewBag.Priority = new SelectList(_member.GetPriorities(), "prioritytype", "priorityName");
            ViewBag.Assignments = new SelectList(_member.GetActiveSelectAssignments(DependentID), "assignmentID", "AssignmentType");
            ViewBag.owner = _admin.GetUsers().Select(x => new SelectListItem() { Text = x.Firstname + " " + x.Lastname, Value = x.userID.ToString() });
            return View(model);
        }

        [HttpPost]
        public ActionResult Enquiries(Queries model)
        {
            //model.querySubject = Request.Query["radEnquiry"].ToString();
            //model.enquiryBy = Request.Query["EnquiryType"].ToString();
            //if (!String.IsNullOrEmpty(Request.Query["Assignments"]))
            //    model.ReferenceNumber = Request.Query["Assignments"].ToString();
            //model.queryType = Request.Query["QueryT"].ToString();
            //model.priority = Request.Query["Priority"].ToString();
            //if (!String.IsNullOrEmpty(Request.Query["owner"]))
            //{
            //    model.Owner = Request.Query["owner"].ToString();
            //}

            //model.createdBy = User.Identity.Name;

            //_member.InsertQuery(model);

            //return RedirectToAction("patientCommunication", new { DependentID = model.dependentID });



            model.querySubject = Request.Query["radEnquiry"].ToString();
            model.enquiryBy = Request.Query["EnquiryType"].ToString();
            //HCare-1046
            model.programId = new Guid(Request.Query["pro"]);
            if (!String.IsNullOrEmpty(Request.Query["Assignments"]))
                model.ReferenceNumber = Request.Query["Assignments"].ToString();
            model.queryType = Request.Query["QueryT"].ToString();
            if (!String.IsNullOrEmpty(Request.Query["Priority"].ToString())) { model.priority = Request.Query["Priority"].ToString(); } else { model.priority = "LOW"; } //hcare-874
            if (!String.IsNullOrEmpty(Request.Query["owner"])) { model.Owner = Request.Query["owner"].ToString(); }

            model.createdBy = User.Identity.Name;

            if (!Request.Query["queryStatus"].ToString().ToLower().Contains("true"))
            {
                model.createdDate = DateTime.Now;
                model.Active = true;
                model.queryStatus = false;
                model.effectiveDate = DateTime.Now;
                model.resolutionDate = DateTime.Now;
                model.resolvedBy = User.Identity.Name;

                _member.InsertQuery(model);
            }
            else
            {
                model.createdDate = DateTime.Now;
                model.Active = true;
                model.queryStatus = true;
                model.effectiveDate = Convert.ToDateTime(Request.Query["followUpDate"]);

                _member.InsertQuery(model);
            }
            return RedirectToAction("patientCommunication", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult EnquiryFullView(Guid depId, int QueryID = 0, Guid? pro = null)
        {
            var model = new EnquiryFull()
            {
                DependantID = depId,
                enquiryList = _member.GetEnquiryFullView(QueryID, depId),
                enquiryComment = new EnquiryComments()
            };

            if (model.enquiryList.Count > 0)
            {
                foreach (var enq in model.enquiryList)
                {
                    if (!enq.dependantID.ToString().Contains("000"))
                    {
                        model.DependantID = enq.dependantID;
                    }
                    if (enq.enquiryId != 0)
                    {
                        model.enquiryComment.queryId = enq.enquiryId;
                        model.DependantID = enq.dependantID;
                    }
                }
                //model.enquiryComment.queryId = model.enquiryList[0].enquiryId;
                ViewBag.owner = new SelectList(_admin.GetUsers(), "userId", "userFullName", model.enquiryList[0].owner);

            }

            return View(model);
        }

        [HttpPost]

        public ActionResult EnquiryFullView(EnquiryFull model, string submitButton, Guid? pro = null)
        {

            switch (submitButton)
            {
                case "Close":
                    model.enquiryComment.createdBy = User.Identity.Name;
                    model.enquiryComment.active = true;
                    model.enquiryComment.effectiveDate = DateTime.Now;

                    var enquiry = _member.GetQueryById(model.enquiryComment.queryId);
                    model.enquiryComment.ReferenceNumber = enquiry.ReferenceNumber;
                    enquiry.Active = false;
                    //enquiry.createdBy = User.Identity.Name;

                    if (!string.IsNullOrEmpty(Request.Query["owner"].ToString()))
                    {
                        enquiry.Owner = Request.Query["owner"].ToString();
                    }
                    else
                    {
                        ModelState.AddModelError("Custom", "An owner is required in order to close the enquiry");
                        ViewBag.owner = new SelectList(_admin.GetUsers(), "userId", "username");
                        model.enquiryList = _member.GetEnquiryFullView(model.enquiryComment.queryId, model.DependantID);
                        return View(model);
                    }

                    var result = _member.InsetEnquiryComment(model.enquiryComment);

                    enquiry.queryStatus = false;
                    enquiry.resolvedBy = User.Identity.Name;
                    enquiry.resolutionDate = DateTime.Now;

                    var UpdateEnquiry = _member.UpdateQuery(enquiry);
                    return new RedirectResult(Url.Action("patientAssignments", "Member", new { DependentID = model.DependantID, pro = pro }));

                case "PostPone":
                    model.enquiryComment.createdBy = User.Identity.Name;
                    model.enquiryComment.active = true;
                    //model.enquiryComment.effectiveDate = DateTime.Now;
                    model.enquiryComment.effectiveDate = Convert.ToDateTime(Request.Query["enquiryComment.effectiveDate"]);
                    enquiry = _member.GetQueryById(model.enquiryComment.queryId);
                    model.enquiryComment.ReferenceNumber = enquiry.ReferenceNumber;
                    if (!string.IsNullOrEmpty(Request.Query["owner"].ToString()))
                    {
                        enquiry.Owner = Request.Query["owner"].ToString();
                    }
                    else
                    {
                        ModelState.AddModelError("Custom", "An owner is required in order to postpone the enquiry");
                        ViewBag.owner = new SelectList(_admin.GetUsers(), "userId", "username");
                        model.enquiryList = _member.GetEnquiryFullView(model.enquiryComment.queryId, model.DependantID);
                        return View(model);
                    }
                    result = _member.InsetEnquiryComment(model.enquiryComment);
                    enquiry.Active = true;
                    enquiry.queryStatus = true;
                    //enquiry.createdBy = User.Identity.Name; //HCare-735

                    enquiry.followUpDate = model.enquiryComment.effectiveDate;

                    UpdateEnquiry = _member.UpdateQuery(enquiry);
                    return new RedirectResult(Url.Action("patientAssignments", "Member", new { DependentID = model.DependantID, pro = pro }));
            }
            return RedirectToAction("patientAssignments", new { DependentID = model.DependantID, pro = pro });
        }

        public ActionResult EditQuery(int id)
        {
            var model = _member.GetQueryById(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditQuery(Queries model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            var result = _member.UpdateQuery(model);
            return RedirectToAction("Enquiries", new { DependentID = model.dependentID });
        }

        public ActionResult EditMember(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            ViewBag.iChecked = _member.GetMedicalAids().Where(x => x.MedicalAidID == model.medicalAid.MedicalAidID).Select(x => x.titleSync).FirstOrDefault(); //HCare-1088
            ViewBag.dependantType = new SelectList(_member.GetDependantType(), "dependentTypeCode", "dependentType", model.dependent.dependentType);


            return View(model);
        }

        //[HttpPost]
        //public ActionResult EditMember(EnrolmentViewModel model)
        //{
        //    model.dependent.modifiedBy = User.Identity.Name;
        //    model.dependent.modifiedDate = DateTime.Now;
        //    model.dependent.dispensingProvider = Request.Query["provider"];

        //    if (model.DProvider == null)
        //    {
        //        model.DProvider = new PatientDispensingProviderHistory();
        //    }
        //    if (Request.Query["provider"].ToString() != null)
        //    {
        //        if (!String.IsNullOrEmpty(Request.Query["provider"].ToString()))
        //        {
        //            model.DProvider.dispensingId = Convert.ToInt16(Request.Query["provider"]);
        //            PatientDispensingProviderHistory prov = new PatientDispensingProviderHistory()
        //            {
        //                createdBy = User.Identity.Name,
        //                dispensingId = model.DProvider.dispensingId,
        //                createdDate = DateTime.Now,
        //                dependantId = model.dependent.DependantID,
        //            };
        //            _member.InsertDispensingProviderHistory(prov);
        //        }
        //    }

        //    var dep = _member.GetDependantByDependantID(model.dependent.DependantID);

        //    MembershipNoHistory history = new MembershipNoHistory()
        //    {
        //        createdBy = User.Identity.Name,
        //        dependantId = model.dependent.DependantID,
        //    };

        //    if (model.dependent.dependentCode != dep.dependentCode)
        //    {
        //        history.dependantCode = dep.dependentCode;
        //    }
        //    //if depcode changed keep old one
        //    var member = _member.GetMemberByDependantID(model.dependent.DependantID);
        //    if (member.membershipNo != model.member.membershipNo || model.dependent.dependentCode != dep.dependentCode)
        //    {
        //        var memberNew = _member.GetMembers().Where(x => x.membershipNo == model.member.membershipNo).FirstOrDefault();
        //        if (memberNew != null)
        //        {
        //            var deps = _member.GetDependants(memberNew.memberID, model.dependent.dependentCode);
        //            if (deps != "0")
        //            {
        //                var models = _member.GetDependentDetails(model.dependent.DependantID);

        //                ViewBag.medicalAid = new SelectList(_member.GetMedicalAids().Where(x => x.clCarrier != true), "medicalAidId", "Name", models.member.medicalAidID);
        //                ViewBag.dependantType = new SelectList(_member.GetDependantType(), "dependentTypeCode", "dependentType", models.dependent.dependentType);
        //                ViewBag.DemographicCode = new SelectList(_member.GetDemographic(), "demographicCode", "demographicName", models.dependent.demographic);
        //                ViewBag.sex = new SelectList(_member.GetSex(), "sex", "sexName", models.dependent.sex);
        //                ViewBag.language = new SelectList(_member.GetLanguage(), "languageName", "languageCode", models.dependent.languageCode);
        //                ViewBag.originationID = new SelectList(_member.GetOrigin(), "OriginID", "originName", models.dependent.originationId);

        //                if (models.DProvider != null)
        //                {
        //                    ViewBag.provider = new SelectList(_member.GetProviders(), "id", "name", models.DProvider.dispensingId);
        //                }
        //                else
        //                {
        //                    ViewBag.provider = new SelectList(_member.GetProviders(), "id", "name");
        //                }

        //                ViewBag.memberNumber = "Member# & Dep code already exists!";
        //                return View(models);
        //            }
        //        }
        //    }

        //    history.MembershipNo = member.membershipNo;
        //    if (member.membershipNo != model.member.membershipNo || model.dependent.dependentCode != dep.dependentCode)
        //    {
        //        //Insert into memberhistory
        //        _member.InsertMembershipHistory(history);
        //    }

        //    ServiceResult result = _member.UpdateDependant(model.dependent);
        //    model.member.medicalAidID = member.medicalAidID;
        //    if (member.membershipNo != model.member.membershipNo || member.medicalAidID != model.member.medicalAidID)
        //    {
        //        model.member.modifiedBy = User.Identity.Name;
        //        model.member.modifiedDate = DateTime.Now;

        //        _member.UpdateMembers(model.member);
        //    }

        //    return RedirectToAction("patientDashboard", new { DependentID = model.dependent.DependantID, pro = Request.Query["pro"] });
        //}


        [HttpPost]
        public ActionResult EditMember(EnrolmentViewModel model)
        {

            //------------------------------------------------------------------------------------- dependant-record -------------------------------------------------------------------------------------//
            model.dependent.modifiedBy = User.Identity.Name;
            model.dependent.modifiedDate = DateTime.Now;
            model.dependent.dispensingProvider = Request.Query["provider"];


            #region membership-history
            var dep = _member.GetDependantByDependantID(model.dependent.DependantID);
            MembershipNoHistory history = new MembershipNoHistory()
            {
                createdBy = User.Identity.Name,
                dependantId = model.dependent.DependantID,
            };

            if (model.dependent.dependentCode != dep.dependentCode)
            {
                history.dependantCode = dep.dependentCode;
            }
            //if depcode changed keep old one
            var member = _member.GetMemberByDependantID(model.dependent.DependantID);
            if (member.membershipNo != model.member.membershipNo || model.dependent.dependentCode != dep.dependentCode)
            {
                var memberNew = _member.GetMemberByMembershipNo(model.member.membershipNo);
                if (memberNew != null)
                {
                    var deps = _member.GetDependants(memberNew.memberID, model.dependent.dependentCode);
                    if (deps != "0")
                    {
                        var models = _member.GetDependentDetails(model.dependent.DependantID, null);

                        ViewBag.medicalAid = new SelectList(_member.GetMedicalAids().Where(x => x.clCarrier != true), "medicalAidId", "Name", models.member.medicalAidID);
                        ViewBag.dependantType = new SelectList(_member.GetDependantType(), "dependentTypeCode", "dependentType", models.dependent.dependentType);
                        ViewBag.DemographicCode = new SelectList(_member.GetDemographic(), "demographicCode", "demographicName", models.dependent.demographic);
                        ViewBag.sex = new SelectList(_member.GetSex(), "sex", "sexName", models.dependent.sex);
                        ViewBag.language = new SelectList(_member.GetLanguage(), "languageName", "languageCode", models.dependent.languageCode);
                        ViewBag.originationID = new SelectList(_member.GetOrigin(), "OriginID", "originName", models.dependent.originationId);


                        ViewBag.memberNumber = "Member# & Dep code already exists!";
                        return View(models);
                    }
                }
            }

            history.MembershipNo = member.membershipNo;
            if (member.membershipNo != model.member.membershipNo || model.dependent.dependentCode != dep.dependentCode)
            {
                //Insert into memberhistory
                _member.InsertMembershipHistory(history);
            }
            #endregion

            ServiceResult result = _member.UpdateDependant(model.dependent);

            model.member.medicalAidID = member.medicalAidID;
            if (member.membershipNo != model.member.membershipNo || member.medicalAidID != model.member.medicalAidID)
            {
                model.member.modifiedBy = User.Identity.Name;
                model.member.modifiedDate = DateTime.Now;

                _member.UpdateMembers(model.member);
            }

            return RedirectToAction("patientDashboard", new { DependentID = model.dependent.DependantID, pro = Request.Query["pro"] });
        }

        public ActionResult ViewMember(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            var enrol = _member.GetEnrolmentStep(DependentID);
            if (enrol == null)
            {
                model.hasSteps = false;
            }
            else
            {
                if (!enrol.hasClinical && !enrol.hasDemographic && !enrol.hasPathology && !enrol.hasScript)
                {
                    model.hasSteps = false;
                }
                else if (!enrol.clinicalCaptured && enrol.hasClinical)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.demographicCaptured && enrol.hasDemographic)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.pathologyCaptured && enrol.hasPathology)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.scriptCaptured && enrol.hasScript)
                {
                    model.hasSteps = true;
                }
                else
                {
                    model.hasSteps = false;
                }
            }
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);
            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID);
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID);
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            return View(model);
        }

        public ActionResult _ProfileInformation(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);
            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID);
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID);
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            return PartialView("~/Views/Member/Partials/_ProfileInformation.cshtml", model);
        }

        public ActionResult _ProfileInformation_viewMember(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);
            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID);
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID);
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            return PartialView("~/Views/Member/Partials/_ProfileInformation_viewMember.cshtml", model);
        }

        public ActionResult _profileBar(Guid DependentID, Guid pro)
        {
            var model = _member.GetDependentDetails(DependentID, null);

            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID, pro);

            if (model.hypoglymiaRiskHistory != null)
            {
                model.HypoRisk = ", HypoRisk: " + model.hypoglymiaRiskHistory.HypoRisk.ToString() +
                                    ", Insulin: " + model.hypoglymiaRiskHistory.Insulin.ToString() +
                                    ", Sulphonylureas: " + model.hypoglymiaRiskHistory.Sulphonylureas.ToString() +
                                    ", Glucose: " + model.hypoglymiaRiskHistory.Glucose.ToString() +
                                    ", Renal: " + model.hypoglymiaRiskHistory.Renal.ToString() +
                                    ", Hypo: " + model.hypoglymiaRiskHistory.hypo.ToString();
            }

            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetDependentsById(DependentID, pro);
            if (model.statuss.Count == 0)
            {
                model.statuss = model.statuss.Take(1).ToList();
            }
            else
            {
                model.statuss = model.statuss.Take(2).ToList();
            }
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();

            var doctorh = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == pro).ToList();
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            return PartialView("~/Views/Member/Partials/_profileBar.cshtml", model);
        }

        public ActionResult _profileBar_edit(Guid DependentID, Guid? pro)
        {
            var model = _member.GetDependentDetails(DependentID, pro);
            var program = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var hb = _member.GetHba1cByPatient(DependentID);
            if (hb.Count > 0)
            {
                model.hba1c = hb[0].hba1c;
                model.hbEffective = hb[0].hba1ceffectiveDate;
            }
            else
            {
                model.hba1c = null;
                model.hbEffective = null;
            }

            model.programcode = programcode;

            //Staging is for HIV Patients only 
            model.PatientStagingHistory = new PatientStagingHistory();
            if (programcode == "HIVPR")
                model.PatientStagingHistory.stageCode = _member.GetPatientStagingHistoryByDependant(DependentID); //HCare-1014

            model.programs = new List<PatientProgramHistory>();

            var programhistory = _member.GetPatientProgramHistory(DependentID, program.programID);
            var mhdiagnosis = _member.GetMHDiagnosis(DependentID);

            if (programcode.ToLower() == "mntlh")
            {
                //model.programs = _member.GetMHDiagnosisHistory(DependentID, program.programID);
                model.programHistories = _member.Get_MH_Diagnosis_History(DependentID, program.programID); //hcare-1203
            }
            else
            {
                //model.programs = _member.GetPatientProgramHistory(DependentID, program.programID);
                model.programHistories = _member.Get_Program_History(DependentID, program.programID); //hcare-1203
            }



            //#region HCare-1203 - update :: disabled from 01 November 2021 (maybe before)
            //if (pro == null)
            //{
            //    var programCheck = _member.GetPatientProgramHistory(DependentID);
            //    var subProgramCheck = _member.GetPatientProgramSubHistory(DependentID);
            //    if (programCheck.Count != 0 && subProgramCheck.Count != 0)
            //    {
            //        model.programs = _member.GetProgramHistory(DependentID);
            //        model.SubPrograms = null;
            //    }
            //    else if (programCheck.Count == 0 && subProgramCheck.Count != 0)
            //    {
            //        model.programs = _member.GetPatientProgramHistory(DependentID);
            //        model.SubPrograms = _member.GetPatientProgramSubHistory(DependentID);
            //    }
            //    else if (programCheck.Count != 0 && subProgramCheck.Count == 0)
            //    {
            //        model.programs = _member.GetPatientProgramHistory(DependentID);
            //        model.SubPrograms = _member.GetPatientProgramSubHistory(DependentID);
            //    }
            //    else if (programCheck.Count == 0 && subProgramCheck.Count == 0)
            //    {
            //        model.programs = null;
            //        model.SubPrograms = null;
            //    }
            //}
            //else
            //{
            //    //model.programs = _member.GetPatientProgramHistory(DependentID, new Guid(pro.ToString()));
            //    //model.SubPrograms = _member.GetPatientProgramSubHistory(DependentID, new Guid(pro.ToString()));

            //    var programCheck = _member.GetPatientProgramHistory(DependentID, new Guid(pro.ToString()));
            //    var subProgramCheck = _member.GetPatientProgramSubHistory(DependentID, new Guid(pro.ToString()));
            //    if (programCheck.Count != 0 && subProgramCheck.Count != 0)
            //    {
            //        model.programs = _member.GetValidProgramHistory(DependentID, new Guid(pro.ToString()));
            //        model.SubPrograms = null;
            //    }
            //    else if (programCheck.Count == 0 && subProgramCheck.Count != 0)
            //    {
            //        model.programs = _member.GetPatientProgramHistory(DependentID, new Guid(pro.ToString()));
            //        model.SubPrograms = _member.GetPatientProgramSubHistory(DependentID, new Guid(pro.ToString()));
            //    }
            //    else if (programCheck.Count != 0 && subProgramCheck.Count == 0)
            //    {
            //        model.programs = _member.GetPatientProgramHistory(DependentID, new Guid(pro.ToString()));
            //        model.SubPrograms = _member.GetPatientProgramSubHistory(DependentID, new Guid(pro.ToString()));
            //    }
            //    else if (programCheck.Count == 0 && subProgramCheck.Count == 0)
            //    {
            //        model.programs = null;
            //        model.SubPrograms = null;
            //    }
            //}
            //if (programcode.ToLower() == "mntlh")
            //{ 

            //}
            //    #endregion

            //model.MentalHealthDiagnoses = _member.GetMentalHealthHistoryByDependant(DependentID, program.programID);
            //model.PatientProgramHistories = _member.GetMHDiagnosisHistory_pb(DependentID, program.programID); // hcare-1203

            if (model.hypoglymiaRiskHistory != null)
                model.HypoRisk = ", HypoRisk: " + model.hypoglymiaRiskHistory.HypoRisk.ToString() +
                                    ", Insulin: " + model.hypoglymiaRiskHistory.Insulin.ToString() +
                                    ", Sulphonylureas: " + model.hypoglymiaRiskHistory.Sulphonylureas.ToString() +
                                    ", Glucose: " + model.hypoglymiaRiskHistory.Glucose.ToString() +
                                    ", Renal: " + model.hypoglymiaRiskHistory.Renal.ToString() +
                                    ", Hypo: " + model.hypoglymiaRiskHistory.hypo.ToString();

            model.statuss = new List<PatientManagementStatusHistory>();
            if (pro == null)
            {
                model.statuss = _member.GetDependentsById(DependentID, null);
            }
            else
            {
                model.statuss = _member.GetDependentsById(DependentID, new Guid(pro.ToString()));
            }

            if (model.statuss.Count == 0)
            {
                model.statuss = model.statuss.Take(1).ToList();
            }
            else if (model.statuss.Count > 2)
            {
                model.statuss = model.statuss.ToList();
            }
            else
            {
                model.statuss = model.statuss.Take(2).ToList();
            }
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();

            var doctorh = _member.GetDoctorHistory(DependentID).Where(x => x.ProgramId == pro).ToList(); //HCare-1181 //HCare-1386
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            model.hospitalAuths = _member.GetGeneralHospitalizationAuths(model.member.membershipNo, model.dependent.dependentCode);

            model.UserMemberHistories = new List<UserMemberHistory>();
            model.UserMemberHistories = _admin.GetUserMemberHistory(DependentID, program.programID).Where(x => x.Active == true).OrderByDescending(x => x.EffectiveDate).ToList(); //HCare-1176

            var user = User.Identity.Name;
            var userInfo = _admin.GetUserByUsername(user);
            ViewBag.UserName = user;

            model.wfQueue = _member.GetUserWorkflowTask(DependentID);
            model.QuestionnaireOther = _member.GetTuberculosisResults(DependentID); //HCare-1276

            model.StateEnrolled = _member.GetPatientDiagnosisResult(DependentID, program.code); //hcare-863

            #region hcare-1374: popup-template
            var scheme = _member.GetPatientPlanByDependant(DependentID);
            var schemeOption = _member.GetMedicalAidPlansList().Where(x => x.PlanCode == scheme.planName).FirstOrDefault();

            if (scheme != null)
            {
                model.PopUpTemplates = _admin.GetPopUpTemplateByMemberInformation(model.medicalAid.MedicalAidID, schemeOption.PlanName, program.programID);
                model.PopUp = new PopUpTemplateVM();

                if (model.PopUpTemplates.Count != 0)
                {
                    foreach (var pop in model.PopUpTemplates)
                    {
                        var ma = _member.GetMedicalAidById(new Guid(pop.selectedSchemes));
                        if (pop.Type.ToLower() == "long") { model.PopUp.program_pop_type = "true"; model.PopUp.medicalAid = ma.Name; model.PopUp.option = schemeOption.PlanName; model.PopUp.programName = program.ProgramName; model.PopUp.program_pop_title = pop.Title; model.PopUp.program_pop_template = pop.Template; model.PopUp.programName = pro.ToString(); }
                        if (pop.Type.ToLower() == "short") { model.PopUp.profile_pop_type = "true"; model.PopUp.medicalAid = ma.Name; model.PopUp.option = schemeOption.PlanName; model.PopUp.programName = program.ProgramName; model.PopUp.profile_pop_title = pop.Title; model.PopUp.profile_pop_template = pop.Template; model.PopUp.programName = pro.ToString(); }
                    }
                }

                var memberrecord = _member.GetMemberRecordByDependantID(DependentID, program.programID, user);
                if (memberrecord != null && memberrecord.CreatedDate.ToString("dd MM yyyy") == DateTime.Today.ToString("dd MM yyyy") && memberrecord.ProgramID == program.programID)
                {
                    model.PopUp.program_pop_checkbox = memberrecord.ProgramPopUp.ToString();
                    model.PopUp.profile_pop_checkbox = memberrecord.ProfilePopUp.ToString();
                }
            }
            #endregion


            return PartialView("~/Views/Member/Partials/_profileBar_edit.cshtml", model);
        }

        public ActionResult _profileBar_MemberFile(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);

            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);


            return PartialView("~/Views/Member/Partials/_profileBar_MemberFile.cshtml", model);
        }

        public ActionResult _member_profile_bar(Guid DependentID, Guid? pro)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            ViewBag.riskRatingID = model.RiskRating;

            model.programs = new List<PatientProgramHistory>();
            if (pro == null)
            {
                model.programs = _member.GetProgramHistory(DependentID);
            }
            else
            {
                model.programs = _member.GetProgramHistory(DependentID, new Guid(pro.ToString()));
            }

            if (model.hypoglymiaRiskHistory != null)
                model.HypoRisk = ", HypoRisk: " + model.hypoglymiaRiskHistory.HypoRisk.ToString() +
                                    ", Insulin: " + model.hypoglymiaRiskHistory.Insulin.ToString() +
                                    ", Sulphonylureas: " + model.hypoglymiaRiskHistory.Sulphonylureas.ToString() +
                                    ", Glucose: " + model.hypoglymiaRiskHistory.Glucose.ToString() +
                                    ", Renal: " + model.hypoglymiaRiskHistory.Renal.ToString() +
                                    ", Hypo: " + model.hypoglymiaRiskHistory.hypo.ToString();

            model.statuss = new List<PatientManagementStatusHistory>();
            if (pro == null)
            {
                model.statuss = _member.GetDependentsById(DependentID, null);
            }
            else
            {
                model.statuss = _member.GetDependentsById(DependentID, new Guid(pro.ToString()));
            }

            if (model.statuss.Count == 0)
            {
                model.statuss = model.statuss.Take(1).ToList();
            }
            else if (model.statuss.Count > 2)
            {
                model.statuss = model.statuss.ToList();
            }
            else
            {
                model.statuss = model.statuss.Take(2).ToList();
            }
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();

            var doctorh = _member.GetDrHistory(DependentID).Where(x => x.ProgramId == pro).ToList(); //HCare-1386
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                }
            }
            return PartialView("~/Views/Member/Partials/_member_profile_bar.cshtml", model);
        }



        public ActionResult EditNote(int NoteId)
        {
            var model = _member.GetNote(NoteId);
            ViewBag.NoteType = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription", model.noteType);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditNote(Notes model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.noteType = Request.Query["noteType"].ToString();
            var result = _member.UpdateNote(model);
            return new RedirectResult(Url.Action("patientCommunication", "Member", new { DependentID = model.dependentID, pro = Request.Query["pro"] }));
        }

        public ActionResult ViewNote(Guid DependentID)
        {
            var model = _member.GetNotes(DependentID);
            if (model.Count() != 0)
            {
                return View(model[0]);
            }
            else
            {
                return RedirectToAction("patientCommunication", new { DependentID = DependentID });
            }
        }

        public ActionResult PathologyDetails(int Id)
        {
            var model = _member.GetPathologyById(Id);
            return View(model);
        }

        public ActionResult EditPathology(int Id)
        {
            var model = _member.GetPathologyById(Id);
            ViewBag.pathologyType = new SelectList(_member.GetPathologyTypes(), "id", "pathologyType", model.pathologyType);
            ViewBag.labName = new SelectList(_member.GetLaboratories(), "labName", "labName", model.labName);
            ViewBag.laboratories = new SelectList(_admin.GetListOfLaboratories().Where(x => x.active == true), "name", "name", model.labName);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPathology(Pathology model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.labName = Request.Query["laboratories"].ToString();
            model.pathologyType = Request.Query["pathologyType"].ToString();

            #region decimal-update //HCare-1050
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

            var result = _member.UpdatePathology(model);
            return new RedirectResult(Url.Action("PatientClinical_Pathology", "Member", new { DependentID = model.dependentID, pro = Request.Query["pro"] }));
        }

        public ActionResult EditScriptItem(int itemId)
        {
            var model = _member.GetScriptItem(itemId);
            ViewBag.ItemName = _admin.GetProduct(model.nappiCode).productName;
            ViewBag.depId = _member.GetScript(model.scriptID).dependentID;
            return View(model);
        }

        public ActionResult ViewScriptItem(int itemId)
        {
            var model = _member.GetScriptItem(itemId);
            ViewBag.ItemName = _admin.GetProduct(model.nappiCode).productName;
            ViewBag.depId = _member.GetScript(model.scriptID).dependentID;
            return View(model);
        }

        [HttpPost]
        public ActionResult EditScriptItem(ScriptItems model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            var result = _member.UpdateScriptItem(model);
            return RedirectToAction("AuthoriseScript", "Scripts", new { id = model.scriptID });
        }

        //GET
        public ActionResult ManageStatus(Guid DependentID, Guid pro)
        {
            var model = _member.GetManagementStatusByDependentId(DependentID, pro).OrderByDescending(x => x.createdDate).OrderByDescending(x => x.effectiveDate);
            return View(model);
        }

        public ActionResult fillCode(string codeID)
        {
            var options = _member.GetManagementStatus(codeID);
            return Json(options);
        }

        [HttpGet]
        public ActionResult SetManagementStatus(Guid DependentID, Guid pro)
        {
            TempData["dependantid"] = DependentID;
            TempData["programid"] = pro;

            var det = _member.GetMemberByDependantID(DependentID);
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var statusesses = _member.GetManagementStatusesByMedicalAid(det.medicalAidID).Where(x => x.programCode == programcode);
            var last_closed_status = _member.GetManagmentStatusInformation(DependentID).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var PreClosedStatus = _member.GetManagmentStatusInformation(DependentID).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).Skip(1).FirstOrDefault();

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID);
            TempData["medicalaid"] = medicalaid.medicalAidCode;

            //HCare-919 add these two functionality to check status that already allocated to the user N.B super user able to see both
            var memberStatuss = _member.GetManagementStatusByDependentId(DependentID, pro).Where(x => x.active == true).ToList();
            var enrolmentStep = statusesses.Where(m => memberStatuss.Any(c => c.managementStatusCode.Equals(m.statusName))).Select(m => m.enrolmentStage).Max(); //if it breaks here, check the account's managment status list for the scheme and option

            // flitering by enrolment on dropdown April 2020
            // var enrolmentStep = statusesses.Where(x => x.statusCode == last_closed_status.managementStatusCode).Select(x => x.enrolmentStage).FirstOrDefault();
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

            if (last_closed_status != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (User.HasRole("6. Global user") || User.HasRole("5. Super user")) //if in an OPEN status and SUPERUSER is logged in, show full status list
                    {
                        // flitering april 2020
                        var test = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");

                        return View();
                    }
                    else
                    {
                        if (last_closed_status.codeStatus.ToLower() == "o")
                        {
                            //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "p"), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, ONLY closed //hcare-1223
                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                        }
                        else if (last_closed_status.codeStatus.ToLower() == "c")
                        {
                            //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "c"), "statusCode", "statusName"); //if current status is closed, you can change to pending and open status but NOT closed //hcare-1223
                            if (PreClosedStatus.codeStatus.ToLower() == "o")
                            {
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                            }
                            else if (PreClosedStatus.codeStatus.ToLower() == "p")
                            {
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                            }
                        }
                        else
                        {
                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                        }
                        return View();
                    }
                }
            }

            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusCode == "ERD" || x.statusCode == "ERH").Where(x => x.programCode == programcode), "statusCode", "statusName");
            return View();
        }

        [HttpPost]
        public ActionResult SetManagementStatus(PatientManagementStatusHistory model)
        {
            var DependantID = new Guid(Request.Query["dependantid"]);
            var pro = new Guid(Request.Query["programid"]);

            var programcode = _member.GetManagementStatusByCode().Where(x => x.statusCode == Request.Query["managementStatus"].ToString()).Select(x => x.programCode).FirstOrDefault();
            //HCare-1065 - removed the enddate check
            var det = _member.GetMemberByDependantID(DependantID);
            var medicalaid = _member.GetMedicalAidById(det.medicalAidID);
            var dependantinfo = _member.GetDependantByDependantID(DependantID);
            var statusHistory = _member.GetManagmentStatusInformation(DependantID).Where(x => x.active == true).ToList();
            var statushistory = _member.GetManagementStatusByDependent(DependantID, pro).Where(x => x.active == true).ToList();
            var statusesses = _member.GetManagementStatusesByMedicalAid(det.medicalAidID).Where(x => x.programCode == programcode);
            var lastStatus_Closed = _member.GetManagmentStatusInformation(DependantID).Where(x => x.active == true).FirstOrDefault();
            var lastStatus = _member.GetManagmentStatusInformation(DependantID).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var LastStatus_x = _member.GetManagmentStatusInformation(DependantID).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var PreClosedStatus = _member.GetManagmentStatusInformation(DependantID).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).Skip(1).FirstOrDefault();

            model.managementStatusCode = Request.Query["managementStatus"].ToString();
            model.dependantId = DependantID;
            model.createdBy = User.Identity.Name;

            #region enrolment-status-check HCare-991
            if (model.managementStatusCode == "ERD" || model.managementStatusCode == "ERH" || model.managementStatusCode == "ERM")
            {
                foreach (var item in statushistory)
                {
                    if (item.managementStatusCode == "ERD" || item.managementStatusCode == "ERH" || model.managementStatusCode == "ERM")
                    {
                        //IF in an OPEN status and SUPERUSER is logged in, show full status list
                        if (User.HasRole("6. Global user") || User.HasRole("5. Super user"))
                        {
                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName");
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                            ViewBag.Message = "Enrolment status already exists.";
                            //HCare-1189
                            TempData["dependantid"] = DependantID;
                            TempData["programid"] = pro;
                            TempData["medicalaid"] = medicalaid.medicalAidCode;

                            return View(model);
                        }
                        //ELSE if in OPEN status and CONSULTANT is logged in, don't show Pending status list
                        else
                        {
                            if (lastStatus_Closed.codeStatus.ToLower() == "o")
                            {
                                //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "p"), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, ONLY closed
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                                ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");

                            }
                            else if (lastStatus_Closed.codeStatus.ToLower() == "c")
                            {
                                //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "c"), "statusCode", "statusName"); //if current status is closed, you can change to pending and open status but NOT closed
                                if (PreClosedStatus.codeStatus.ToLower() == "o")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                                }
                                else if (PreClosedStatus.codeStatus.ToLower() == "p")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                                }
                                ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                            }
                            else
                            {
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                                ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                            }
                            ViewBag.Message = "Enrolment status already exists.";
                            //HCare-1189
                            TempData["dependantid"] = DependantID;
                            TempData["programid"] = pro;
                            TempData["medicalaid"] = medicalaid.medicalAidCode;

                            return View(model);
                        }
                    }
                }
            }
            #endregion

            if (lastStatus != null)
            {
                #region hcare-1067 dep-created-date-validation
                if (model.effectiveDate <= dependantinfo.createdDate)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        if (User.HasRole("6. Global user") || User.HasRole("5. Super user")) //if in an OPEN status and SUPERUSER is logged in, show full status list
                        {
                            ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                            ModelState.AddModelError("effectiveDate", "Dependant was not yet created - Created date " + dependantinfo.createdDate.ToString("dd-MM-yyyy"));

                            TempData["dependantid"] = DependantID;
                            TempData["programid"] = pro;
                            TempData["medicalaid"] = medicalaid.medicalAidCode;

                            return View(model);
                        }
                        else
                        {
                            if (LastStatus_x.codeStatus.ToLower() == "o")
                            {
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                            }
                            else if (LastStatus_x.codeStatus.ToLower() == "c")
                            {
                                if (PreClosedStatus.codeStatus.ToLower() == "o")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                                }
                                else if (PreClosedStatus.codeStatus.ToLower() == "p")
                                {
                                    ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                                }
                            }
                            else
                            {
                                ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                            }

                            ModelState.AddModelError("effectiveDate", "Dependant was not yet created - Created date " + dependantinfo.createdDate.ToString("dd-MM-yyyy"));

                            TempData["dependantid"] = DependantID;
                            TempData["programid"] = pro;
                            TempData["medicalaid"] = medicalaid.medicalAidCode;

                            return View(model);
                        }
                    }

                    //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName", model.managementStatusCode);
                    //ModelState.AddModelError("effectiveDate", "Dependant was not yet created - Created date " + dependantinfo.createdDate.ToString("dd-MM-yyyy"));

                    //TempData["dependantid"] = DependantID;
                    //TempData["programid"] = pro;
                    //TempData["medicalaid"] = medicalaid.medicalAidCode;

                    //return View(model);
                }
                #endregion
                #region hcare-852 date validation
                //-->> HCare-852 (Date collection validation)
                foreach (var status in statusHistory.Where(x => x.programCode == programcode))
                {
                    if (model.effectiveDate <= status.endDate || model.effectiveDate <= status.effectiveDate)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (User.HasRole("6. Global user") || User.HasRole("5. Super user")) //if in an OPEN status and SUPERUSER is logged in, show full status list
                            {
                                // flitering april 2020
                                var test = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                                ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                                TempData["dependantid"] = DependantID;
                                TempData["programid"] = pro;
                                TempData["medicalaid"] = medicalaid.medicalAidCode;

                                return View(model);
                            }
                            else
                            {
                                if (LastStatus_x.codeStatus.ToLower() == "o")
                                {
                                    ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                                }
                                else if (LastStatus_x.codeStatus.ToLower() == "c")
                                {
                                    if (PreClosedStatus.codeStatus.ToLower() == "o")
                                    {
                                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                                    }
                                    else if (PreClosedStatus.codeStatus.ToLower() == "p")
                                    {
                                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                                    }
                                }
                                else
                                {
                                    ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                                }

                                ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                                TempData["dependantid"] = DependantID;
                                TempData["programid"] = pro;
                                TempData["medicalaid"] = medicalaid.medicalAidCode;

                                return View(model);
                            }
                        }

                        //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                        //ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                        ////HCare-1189
                        //TempData["dependantid"] = DependantID;
                        //TempData["programid"] = pro;
                        //TempData["medicalaid"] = medicalaid.medicalAidCode;

                        //return View(model);
                    }
                }
                //-->>
                #endregion
                #region hcare-755 last status update 
                //-->> HCare-755 (setting the last status)
                if (lastStatus.managementStatusCode != model.managementStatusCode)
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
                        sentToCl = lastStatus.sentToCl,
                    };

                    //-->> HCare-786(This leaves the previous date alone if the status was closed. Allows for time lapse as per HCare-786)
                    if (lastStatus_Closed.codeStatus.ToLower() != "c")
                    {
                        statusUpdate.endDate = model.effectiveDate.Value.AddDays(-1);
                    }
                    //-->> This will ensure that the enddate is the same as the effective date if the last status is closed and a new status is added thereafter (HCare-1162)
                    if (lastStatus_Closed.codeStatus.ToLower() == "c")
                    {
                        statusUpdate.endDate = statusUpdate.effectiveDate;
                    }
                    //-->>

                    //-->> HCare-852 (Setting end date )
                    foreach (var status in statusHistory)
                    {
                        if (status.programCode == programcode && status.endDate == null & status.codeStatus.ToLower() != "c")
                        {
                            statusUpdate.endDate = model.effectiveDate.Value.AddDays(-1);
                        }
                    }
                    //-->>
                    var results = _member.UpdateMSRecord(statusUpdate);
                }
                //-->>
                #endregion
                #region duplicate validation
                //This is for the duplicate check - HCare-713 (Management Status changes)
                if (lastStatus.managementStatusCode != model.managementStatusCode)
                {
                    #region hcare-792 enrollment assignment
                    //-->> HCare-792(Create assignment when management status is changed to DM Enrolment)
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
                                status = "Open",
                                programId = pro
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

                        var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType, pro);
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
                    //-->>
                    #endregion
                    #region hcare-669 closed status
                    //HCare-669 (Default end date to effective date when status is set to Patient resigned/Deactivated/Deceased)
                    var Status_Closed = _member.GetManagmentStatusByCode(model.managementStatusCode);
                    if (Status_Closed.codeStatus.ToLower() == "c")
                    {
                        model.endDate = model.effectiveDate;
                    }
                    #endregion
                    #region firstManagmentStatusCheck
                    //CHECK if last status is a PENDING! THEN update firstopen status from model
                    //CHECK if last status is closed AND there is an OPEN status after! THEN update firstopen status from model
                    #endregion

                    ServiceResult result = _member.InsertManagementStatus(model);
                    return RedirectToAction("patientDashboard", "Member", new { DependentID = model.dependantId, pro = pro });
                }
                else
                {
                    //IF in an OPEN status and SUPERUSER is logged in, show full status list
                    if (User.HasRole("6. Global user") || User.HasRole("5. Super user"))
                    {
                        ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName");
                        ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                        ViewBag.Message = "Management status is already set to this value!";

                        //HCare-1189
                        TempData["dependantid"] = DependantID;
                        TempData["programid"] = pro;
                        TempData["medicalaid"] = medicalaid.medicalAidCode;

                        return View(model);
                    }
                    //ELSE if in OPEN status and CONSULTANT is logged in, don't show Pending status list
                    else
                    {
                        if (LastStatus_x.codeStatus.ToLower() == "o")
                        {

                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                        }
                        else if (LastStatus_x.codeStatus.ToLower() == "c")
                        {
                            if (PreClosedStatus.codeStatus.ToLower() == "o")
                            {
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                            }
                            else if (PreClosedStatus.codeStatus.ToLower() == "p")
                            {
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                            }
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                        }
                        else
                        {
                            ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                        }
                        ViewBag.Message = "Management status is already set to this value!";
                        //HCare-1189
                        TempData["dependantid"] = DependantID;
                        TempData["programid"] = pro;
                        TempData["medicalaid"] = medicalaid.medicalAidCode;

                        return View(model);
                    }
                }
                #endregion
            }
            else
            {
                #region hcare-852 date validation
                //-->> HCare-852 (Date collection validation)
                foreach (var status in statusHistory.Where(x => x.programCode == programcode))
                {
                    if (model.effectiveDate <= status.endDate || model.effectiveDate <= status.effectiveDate)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (User.HasRole("6. Global user") || User.HasRole("5. Super user")) //if in an OPEN status and SUPERUSER is logged in, show full status list
                            {
                                // flitering april 2020
                                var test = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                                ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                                ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                                TempData["dependantid"] = DependantID;
                                TempData["programid"] = pro;
                                TempData["medicalaid"] = medicalaid.medicalAidCode;

                                return View(model);
                            }
                            else
                            {
                                if (LastStatus_x.codeStatus.ToLower() == "o")
                                {
                                    ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() == "o" || x.statusType.ToLower().ToString() == "c" || x.statusName.ToLower().ToString().Replace(" ", "").Contains("optout") || x.statusName.ToLower().ToString().Contains("refusal")), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, except for opt-out/programme refusal. //HCare-1223
                                }
                                else if (LastStatus_x.codeStatus.ToLower() == "c")
                                {
                                    if (PreClosedStatus.codeStatus.ToLower() == "o")
                                    {
                                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that open, the user will only be able to select the same open status. //HCare-1223
                                    }
                                    else if (PreClosedStatus.codeStatus.ToLower() == "p")
                                    {
                                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true).Where(x => x.programCode == programcode).Where(x => x.statusCode.ToLower().ToString().Replace(" ", "").Contains(PreClosedStatus.managementStatusCode.ToLower())), "statusCode", "statusName");  //if previous status was closed and the status before that pending, the user will only be able to select the same pending status. //HCare-1223
                                    }
                                }
                                else
                                {
                                    ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                                }

                                ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                                TempData["dependantid"] = DependantID;
                                TempData["programid"] = pro;
                                TempData["medicalaid"] = medicalaid.medicalAidCode;

                                return View(model);
                            }
                        }

                        //ViewBag.managementStatus = new SelectList(statusesses.Where(x => x.active == true).Where(x => x.programCode == programcode), "statusCode", "statusName");
                        //ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";
                        ////HCare-1189
                        //TempData["dependantid"] = DependantID;
                        //TempData["programid"] = pro;
                        //TempData["medicalaid"] = medicalaid.medicalAidCode;

                        //return View(model);
                    }
                }
                //-->>
                #endregion
                #region hcare-792 enrollment assignment
                //-->> HCare-792(Create assignment when management status is changed to DM Enrolment)
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
                            status = "Open",
                            programId = pro
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

                    var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType, pro);
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
                //-->>
                #endregion

                ServiceResult result = _member.InsertManagementStatus(model);
                return RedirectToAction("patientDashboard", "Member", new { DependentID = model.dependantId, pro = pro });
            }

        }


        public ActionResult DependantCreatedDateCheck(Guid dependantID)
        {
            var options = _member.GetDependantByDependantID(dependantID);

            var createddate = options.createdDate.ToString("yyyy-MM-dd");

            return Json(options);
        }

        public ActionResult ManagementStatusDateRangeValidation(Guid dependantID, DateTime effectiveDate, Guid programID)
        {
            var mshistory = _member.GetManagmentStatusInformation(dependantID).Where(x => x.active == true).ToList();
            var program = _admin.GetProgramById(programID);
            var DateRangeAllowed = true;

            #region hcare-852 date validation
            //-->> HCare-852 (Date collection validation)
            foreach (var status in mshistory.Where(x => x.programCode == program.code))
            {
                if (effectiveDate <= status.endDate || effectiveDate <= status.effectiveDate)
                {
                    DateRangeAllowed = false;
                }
            }

            #endregion

            return Json(DateRangeAllowed);
        }

        public ActionResult ComorbidDateRangeValidation(Guid dependantID, Guid programID, DateTime effectiveDate, string endDate, string mappingCode)
        {
            var cminformation = _member.GetCMByMappingCode(mappingCode);
            var cmhistory = _member.GetComorbidInformation(dependantID);
            var program = _admin.GetProgramById(programID);
            var daterangeallowed = true;
            DateTime? treatmentEndDate = null;
            if (!String.IsNullOrEmpty(endDate)) { treatmentEndDate = Convert.ToDateTime(endDate); }

            #region overlapping-date-validation
            if (treatmentEndDate != null)
            {
                foreach (var item in cmhistory)
                {
                    bool startDateOverlap = effectiveDate >= item.effectiveDate && effectiveDate <= item.endDate;
                    bool startEndDateOverlap = item.effectiveDate <= treatmentEndDate && effectiveDate <= item.endDate;

                    if (!String.IsNullOrEmpty(item.effectiveDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
                    {
                        if ((cminformation.mappingDescription == item.Condition) && (cminformation.ICD10Code == item.icd10))
                        {
                            if (!String.IsNullOrEmpty(effectiveDate.ToString()) && !String.IsNullOrEmpty(treatmentEndDate.ToString()))
                            {
                                if (startEndDateOverlap == true)
                                {
                                    daterangeallowed = false;
                                }
                            }
                            else if (!String.IsNullOrEmpty(effectiveDate.ToString()) && String.IsNullOrEmpty(treatmentEndDate.ToString()))
                            {
                                if (startDateOverlap == true)
                                {
                                    daterangeallowed = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in cmhistory)
                {
                    if (item != null)
                    {
                        bool startDateOverlap = effectiveDate >= item.effectiveDate && effectiveDate <= item.endDate;
                        bool startEndDateOverlap = item.effectiveDate <= treatmentEndDate && effectiveDate <= item.endDate;

                        if (!String.IsNullOrEmpty(item.effectiveDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
                        {
                            if ((cminformation.mappingDescription == item.Condition) && (cminformation.ICD10Code == item.icd10))
                            {
                                if (startDateOverlap == true)
                                {
                                    daterangeallowed = false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return Json(daterangeallowed);
        }



        [HttpPost]
        public ActionResult SetManagementStatusXX(PatientManagementStatusHistory model)
        {
            var depId = new Guid(Request.Query["DependentID"]);
            var programcode = _member.GetManagementStatusByCode().Where(x => x.statusCode == Request.Query["managementStatus"].ToString()).Select(x => x.programCode).FirstOrDefault();
            var pro = _member.GetPrograms().Where(x => x.code == programcode).Select(x => x.programID).FirstOrDefault();
            var lastStatus = _member.GetManagmentStatusInformation(depId).Where(x => x.active == true).Where(x => x.endDate == null).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            var lastStatus_Closed = _member.GetManagmentStatusInformation(depId).Where(x => x.active == true).FirstOrDefault();
            var statusHistory = _member.GetManagmentStatusInformation(depId).Where(x => x.active == true).ToList();

            model.managementStatusCode = Request.Query["managementStatus"].ToString();
            model.dependantId = depId;
            model.createdBy = User.Identity.Name;

            if (lastStatus != null) //if the patient does not have any status
            {
                #region done
                //-->> HCare-852
                foreach (var status in statusHistory.Where(x => x.programCode == programcode))
                {
                    if (model.effectiveDate <= status.endDate || model.effectiveDate <= status.effectiveDate)
                    {
                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName");
                        ViewBag.Message = "Date range has been used in Start End date collection, please review patient status history.";

                        return View(model);
                    }
                }
                //-->>

                //-->> HCare-755 (Validation check for Management Status create)
                if (lastStatus.managementStatusCode != model.managementStatusCode)
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

                    //-->> HCare-786(This leaves the previous date alone if the status was closed. Allows for time lapse as per HCare-786)
                    if (lastStatus_Closed.codeStatus.ToLower() != "c")

                    {
                        statusUpdate.endDate = model.effectiveDate.Value.AddDays(-1);
                    }
                    //-->>

                    //-->> HCare-852
                    foreach (var status in statusHistory)
                    {
                        if (status.programCode == programcode && status.endDate == null & status.codeStatus.ToLower() != "c")
                        {
                            statusUpdate.endDate = model.effectiveDate.Value.AddDays(-1);
                        }
                    }
                    //-->>

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
                            status = "Open",
                            programId = pro
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

                    var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignment.assignmentItemType, pro);
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

                #endregion

                //This is for the duplicate check - HCare-713 (Management Status changes)
                if (lastStatus.managementStatusCode != model.managementStatusCode)
                {
                    ServiceResult result = _member.InsertManagementStatus(model);
                    return RedirectToAction("patientDashboard", "Member", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
                }
                else
                {
                    //IF in an OPEN status and SUPERUSER is logged in, show full status list
                    if (User.HasRole("6. Global user") || User.HasRole("5. Super user"))
                    {
                        ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName");
                        ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");
                        ViewBag.Message = "Management status is already set to this value!";

                        return View(model);
                    }
                    //ELSE if in OPEN status and CONSULTANT is logged in, don't show Pending status list
                    else
                    {
                        if (lastStatus_Closed.codeStatus.ToLower() == "o")
                        {
                            ViewBag.managementStatus = new SelectList(_member.GetManagementStatus_noPendingStatus().Where(x => x.active == true), "statusCode", "statusName"); //if current status is open, you cannot change to pending status, ONLY closed
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");

                        }
                        else if (lastStatus_Closed.codeStatus.ToLower() == "c")
                        {
                            ViewBag.managementStatus = new SelectList(_member.GetManagementStatus_noClosedStatus().Where(x => x.active == true), "statusCode", "statusName"); //if current status is closed, you can change to pending and open status but NOT closed
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");

                        }
                        else
                        {
                            ViewBag.managementStatus = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName"); //if current status is NOT open or closed, you will have access to all statuses in the list
                            ViewBag.reason = new SelectList(_member.GetManagementStatusReasons().Where(x => x.active == true), "reason", "reason");

                        }
                        ViewBag.Message = "Management status is already set to this value!";
                        return View(model);
                    }
                }

            }
            else //if this is the first entry management status
            {
                ServiceResult result = _member.InsertManagementStatus(model);
                return RedirectToAction("patientDashboard", "Member", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
            }
        }

        public ActionResult AddNote(Guid DependentID, Guid? pro)
        {
            var model = new Notes();
            model.dependentID = DependentID;
            ViewBag.NoteTypeID = new SelectList(_member.GetNoteTypes(), "noteType", "noteDescription");
            ViewBag.programID = pro; //hcare-1156
            model.effectiveDate = DateTime.Now;
            return View(model);
        }

        public ActionResult Hba1cHistory(Guid DependentID)
        {
            var model = new List<Hba1cRangeHistory>();
            model = _member.GetHba1cRangeHistory(DependentID);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddNote(Notes model)
        {
            try
            {
                model.noteType = Request.Query["NoteTypeID"].ToString();
                model.programId = new Guid(Request.Query["pro"]); //hcare-1156
                if (model.noteType == "INO") { model.special = true; }
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.effectiveDate = DateTime.Now;

                var insert = _member.InsertNote(model);

                return new RedirectResult(Url.Action("patientCommunication", "Member", new { DependentID = model.dependentID, pro = Request.Query["pro"] }));
            }
            catch (Exception)
            {
                return View(model);
            }
        }

        public ActionResult Enrol()
        {
            var model = new EnrolmentViewModel();
            model.dependent = new Dependant();
            model.member = new Member();
            model.dependent.createdBy = User.Identity.Name;
            model.member.createdBy = User.Identity.Name;
            model.dependentType = _member.GetDependantType();
            model.MedicalAids = _member.GetMedicalAids();
            model.languages = _member.GetLanguage();
            model.Origins = _member.GetOrigin();
            model.gender = _member.GetSex();
            model.dependent.demographic = "UNK";

            ViewBag.program = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            return View(model);
        }

        // POST: Member/Create
        [HttpPost]
        public ActionResult Enrol(EnrolmentViewModel model)
        {
            try
            {
                //------------------------------------------------------------------------------------- create member record -------------------------------------------------------------------------------------//

                Member member = model.member;
                model.MedicalAids = _member.GetMedicalAids();
                var result = _member.GetDependantByMembershipDepCodeAidCode(model.member.membershipNo, model.dependent.dependentCode, model.MedicalAids.Where(x => x.MedicalAidID == member.medicalAidID).Select(x => x.medicalAidCode).FirstOrDefault());
                if (result.Count() != 0)
                {
                    model.dependent.createdBy = User.Identity.Name;
                    model.dependentType = _member.GetDependantType();
                    model.MedicalAids = _member.GetMedicalAids();
                    model.languages = _member.GetLanguage();
                    model.Origins = _member.GetOrigin();
                    model.gender = _member.GetSex();

                    ViewBag.program = new SelectList(_member.GetPrograms(), "code", "ProgramName");
                    ViewBag.Message = "Member already exists in database";

                    return View(model);
                }

                var memberID = _member.GetMembers(model.member.membershipNo.ToString(), model.member.medicalAidID);
                if (memberID == "0")
                {
                    member.createdBy = User.Identity.Name;
                    member.createdDate = DateTime.Now;
                    member.active = true;
                    member.memberID = _member.InsertMembers(member);
                }
                else
                {
                    member.memberID = new Guid(memberID);
                }

                //------------------------------------------------------------------------------------ create dependant record -----------------------------------------------------------------------------------//

                Dependant dependent = new Dependant();
                dependent = model.dependent;
                var dependentID = _member.GetDependants(member.memberID, dependent.dependentCode);
                if (dependentID == "0")
                {
                    dependent.memberID = member.memberID;
                    dependent.demographic = "UNK";
                    dependent.createdBy = User.Identity.Name;
                    dependent.createdDate = DateTime.Now;
                    dependent.Active = true;
                    dependent.DependantID = _member.InsertDependant(dependent);
                }
                else
                {
                    ModelState.AddModelError("Custom", "Dependant already loaded on the system");
                    ViewBag.medicalAidID = new SelectList(_member.GetMedicalAids(), "medicalAidID", "Name", Request.Query["medicalAidID"].ToString());
                    model.MedicalAids = _member.GetMedicalAids();
                    model.Origins = _member.GetOrigin();
                    model.languages = _member.GetLanguage();
                    model.dependentType = _member.GetDependantType();
                    model.gender = _member.GetSex();
                    ViewBag.program = new SelectList(_member.GetPrograms(), "code", "ProgramName");

                    return View(model);
                }

                //----------------------------------------------------------------------------------- create medical-aid record ----------------------------------------------------------------------------------//

                PatientPlanHistory plan = new PatientPlanHistory();
                plan.dependantId = model.dependent.DependantID;
                if (Request.Query["schemeOption"].ToString() != null)
                {
                    plan.planId = new Guid(Request.Query["schemeOption"].ToString());
                    plan.createdBy = User.Identity.Name;
                    plan.createdDate = DateTime.Now;
                    plan.effectiveDate = DateTime.Now;
                    var results = _member.InsertPlanHistory(plan);
                }

                //------------------------------------------------------------------------------------- create program record ------------------------------------------------------------------------------------//

                PatientProgramHistory patientProgram = new PatientProgramHistory();
                patientProgram.dependantId = dependent.DependantID;
                patientProgram.programCode = Request.Query["program"].ToString();
                patientProgram.createdDate = DateTime.Now;
                patientProgram.createdBy = User.Identity.Name;
                patientProgram.effectiveDate = DateTime.Now;
                patientProgram.active = true;
                _member.InsertProgramHistory(patientProgram);

                var programcode = _member.GetPrograms().Where(x => x.code == patientProgram.programCode).FirstOrDefault();

                //-------------------------------------------------------------------------------- create management status record ------------------------------------------------------------------------------//

                PatientManagementStatusHistory patientStatus = new PatientManagementStatusHistory();
                patientStatus.createdDate = DateTime.Now;
                patientStatus.dependantId = dependent.DependantID;
                patientStatus.active = true;
                patientStatus.createdBy = User.Identity.Name;
                patientStatus.createdDate = DateTime.Now;
                patientStatus.effectiveDate = DateTime.Now;

                if (patientProgram.programCode == "DIABD")
                {
                    patientStatus.managementStatusCode = "ERD";
                }
                else if (patientProgram.programCode == "HIVPR")
                {
                    patientStatus.managementStatusCode = "ERH";
                }
                _member.InsertManagementStatus(patientStatus);



                //----------------------------------------------------------------------------------- create assignment record ---------------------------------------------------------------------------------//

                var assignments = new AssignmentsView();
                assignments.newAssignment = new Assignments();
                assignments.newAssignment.Active = true;
                assignments.newAssignment.createdBy = User.Identity.Name;
                assignments.newAssignment.createdDate = DateTime.Now;
                assignments.newAssignment.effectiveDate = DateTime.Now;
                assignments.newAssignment.assignmentType = "INTER";
                assignments.newAssignment.comment = "For follow up after enrolment";
                assignments.newAssignment.dependentID = dependent.DependantID;

                if (patientProgram.programCode == "DIABD")
                {
                    assignments.assignmentItemType = "NEWRT";
                }
                else if (patientProgram.programCode == "HIVPR")
                {
                    assignments.assignmentItemType = "NEWR";
                }

                var assignExists = _member.GetAssignment(new Guid(Request.Query["DependantID"]), assignments.assignmentItemType);
                if (assignExists == null)
                {
                    var ids = _member.InsertAssignment(assignments);

                    var task = new AssignmentItemTasks();
                    task.assignmentItemID = assignments.itemID;

                    var tasks = _member.GetTaskRequirements(assignments.assignmentItemType);

                    foreach (var tas in tasks)
                    {
                        task.taskTypeId = tas.taskType;
                        task.requirementId = tas.requirementId;
                        _member.InsertTask(task);
                    }
                }

                return RedirectToAction("patientDashboard", new { DependentID = model.dependent.DependantID, pro = programcode.programID });
            }
            catch (Exception)
            {
                model.MedicalAids = _member.GetMedicalAids();
                model.Origins = _member.GetOrigin();
                model.languages = _member.GetLanguage();
                model.dependentType = _member.GetDependantType();
                model.gender = _member.GetSex();
                ViewBag.program = new SelectList(_member.GetPrograms(), "code", "ProgramName");

                return View(model);
            }
        }

        // GET: Member/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Member/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult ProgramHistory(Guid DependentID, Guid pro)
        {
            var program = _admin.GetProgramById(pro);
            var model = new ProgramHistoryViewModel();

            #region hcare-1203
            var programhistory = _member.GetPatientProgramHistory(DependentID, pro);
            var mhdiagnosis = _member.GetMHDiagnosis(DependentID);

            if (program.code.ToLower() == "mntlh")
            {
                model.programHistories = _member.Get_MH_Program_Diagnosis_History(DependentID, program.programID);
            }
            //Hcare-1297
            else
            {
                var histories = new List<PatientProgramVM>();
                histories.Add(new PatientProgramVM()
                {
                    Id = programhistory[0].id,
                    DependantID = programhistory[0].dependantId,
                    ProgramCode = programhistory[0].programCode,
                    EffectiveDate = programhistory[0].effectiveDate,
                    EndDate = programhistory[0].endDate,
                    ICD10Code = programhistory[0].icd10Code,
                    ConditionCode = programhistory[0].conditionCode,
                    Comment = programhistory[0].Comment,
                    CreatedBy = programhistory[0].createdBy,
                    CreatedDate = programhistory[0].createdDate,
                    ModifiedBy = programhistory[0].modifiedBy,
                    ModifiedDate = programhistory[0].modifiedDate,
                    Active = programhistory[0].active
                });

                model.programHistories = histories;
            }

            model.diagnosisHistories = _member.GetPatientProgramChangeHistory(DependentID, pro);

            #endregion

            return View(model);
        }

        public ActionResult AddProgram(Guid DependentID)
        {
            var model = new PatientProgramHistory();
            ViewBag.program = new SelectList(_member.GetPrograms().Where(x => x.ProgramName != "General"), "code", "ProgramName");
            ViewBag.icd10code = new SelectList(_member.GetPrograms(), "icd10Code", "ProgramName");
            model.dependantId = DependentID;
            model.effectiveDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProgram(PatientProgramHistory model)
        {
            model.createdBy = User.Identity.Name;
            model.programCode = Request.Query["program"].ToString();
            var result = _member.InsertProgramHistory(model);
            return new RedirectResult(Url.Action("patientDashboard", "Member", new { DependentID = model.dependantId }));
        }

        public ActionResult Search(string SearchVar)  //HCare-1250 //hcare-1337
        {
            var model = new List<MemberSearchViewModel>();
            if (!string.IsNullOrEmpty(Request.Query["SearchVar"]))
            {
                model = _member.GetDependents(Request.Query["SearchVar"].ToString().Trim()).Take(50).ToList();
                TempData["searchValue"] = Request.Query["SearchVar"];
                return View("Searchresult", model);
            }

            return View("Searchresult", model);
        }

        public ActionResult patientExtract()
        {
            var sb = new StringBuilder();
            var model = _member.GetPatients();
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=patientExtract.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        public ActionResult AssignmentSummary(Guid DependentID)
        {
            var model = new PatientSummaryView();

            model.assignments = new AssignmentsView()
            {
                newAssignments = _member.GetNewAssignments(DependentID),
                activeAssignments = _member.GetActiveOnlyAssignments(DependentID),
                closedAssignments = _member.GetClosedAssignments(DependentID).Take(5).ToList(),
                newAssignment = new Assignments() { dependentID = DependentID },
            };

            model.pathologies = new List<Pathology>();
            model.pathologies = _member.GetPathology(DependentID).Take(3).ToList();
            var script = _member.GetScripts(DependentID);
            if (script.Count != 0)
            {
                int scriptno = Convert.ToInt32(script.FirstOrDefault().scriptID);
                model.ScriptItems = _member.GetScriptItems(scriptno);
            }
            return View(model);
        }

        public ActionResult AddScript(Guid DependentID, int script = 0)
        {
            var model = new ScriptCreationViewModel();
            var scripts = new Scripts();
            var scriptItems = new ScriptViewModel();
            if (script != 0)
            {
                scripts = _member.GetScript(script);
                model.items = _member.GetScriptItems(scripts.scriptID);
            }

            model.script = scripts;
            model.script.dependentID = DependentID;
            model.script.scriptID = script;

            ViewBag.doctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
            return View(model);
        }



        public ActionResult _ScriptItem(int scriptID)
        {
            var model = new ScriptCreationViewModel();
            model.items = _member.GetScriptItems(scriptID);

            return PartialView("~/Views/Member/Partials/_ScriptItem.cshtml", model);
        }

        [HttpPost]
        public JsonResult _AddAssignmentAjax(AssignmentsView model)
        {
            model.newAssignment.assignmentType = Request.Query["assignmentType"].ToString();
            model.assignmentItemType = Request.Query["assignmentItemType"].ToString();
            model.newAssignment.programId = new Guid(Request.Query["pro"]);
            model.newAssignment.createdBy = User.Identity.Name;

            ServiceResult result = _member.InsertAssignment(model);

            var task = new AssignmentItemTasks();
            task.assignmentItemID = model.itemID;

            return Json(model.itemID);
        }
        [HttpPost]
        public JsonResult _AddAssignmentItemAjax(AssignmentsView model)
        {
            model.tasktype = Request.Query["AssignmentTask"].ToString();
            model.taskRequirement = Convert.ToInt32(Request.Query["AssignmentReq"]);

            var task = new AssignmentItemTasks();
            task.assignmentItemID = model.itemID;
            task.createdBy = User.Identity.Name;
            return Json(model.itemID);
        }

        public ActionResult _AddScript(Guid DependentID, int id, int taskId, int script = 0)
        {
            ViewBag.icd10 = new SelectList(_member.GetIcd10Codes(), "icd10CodeID", "icd10CodeID");
            ViewBag.task = taskId;
            ViewBag.id = id;
            var model = new ScriptCreationViewModel();
            var scripts = new Scripts();
            var scriptItems = new ScriptViewModel();
            if (script != 0)
            {
                scripts = _member.GetScript(script);
                model.items = _member.GetScriptItems(scripts.scriptID);
            }

            model.script = scripts;
            model.script.dependentID = DependentID;
            model.script.effectiveDate = DateTime.Now;
            model.script.scriptID = script;

            ViewBag.doctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
            return View(model);
        }

        [HttpPost]
        public JsonResult _AddScriptAjax(ScriptCreationViewModel model)
        {
            int scriptId = 0;
            model.script.scriptType = "GEN";
            var id = Convert.ToInt32(Request.Query["id"].ToString());
            //var logid = Convert.ToInt32(Request.Query["logid"].ToString());
            var doc = _admin.GetDoctor(model.doctor.doctor.drLastName, model.script.practiceNo);

            if (doc != null)
            {
                model.script.doctorID = doc.doctorID;
                model.script.createdBy = User.Identity.Name;
                scriptId = _member.InsertScript(model.script);
                var res = _member.GetEnrolmentStep(model.script.dependentID);
                if (res != null)
                {
                    res.scriptCaptured = true;
                    var update = _member.UpdateEnrolmentStep(res);
                }
                //update assignmentitemtask
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                if (task != null)
                {
                    task.closed = true;
                    task.closedBy = User.Identity.Name;
                    task.closedDate = DateTime.Now;
                    task.status = "Closed";

                    var tres = _clinical.UpdateTask(task);
                }
                return Json(model.script);
            }
            else
            {
                model.doctor.doctor.practiceNo = model.script.practiceNo;
                model.doctor.doctor.drFirstName = "";
                model.doctor.doctor.drLastName = model.doctor.doctor.drLastName;
                model.doctor.doctor.createdBy = User.Identity.Name;
                model.script.createdBy = User.Identity.Name;
                var res = _admin.InsertDoctor(model.doctor.doctor);
                model.script.doctorID = res;
                scriptId = _member.InsertScript(model.script);
                var rres = _member.GetEnrolmentStep(model.script.dependentID);
                if (rres != null)
                {
                    rres.scriptCaptured = true;
                    var update = _member.UpdateEnrolmentStep(rres);
                }

                //update assignmentitemtask
                var task = _clinical.GetTask(Convert.ToInt32(Request.Query["taskId"]));
                if (task != null)
                {
                    task.closed = true;
                    task.closedBy = User.Identity.Name;
                    task.closedDate = DateTime.Now;
                    task.status = "Closed";

                    var tres = _clinical.UpdateTask(task);
                }
                return Json(model.script);
            }
        }

        public ActionResult _AddScriptItem(int script, int? member)
        {
            ScriptItems model = new ScriptItems();
            if (member != null)
                model.modifiedBy = "1";

            model.scriptID = script;
            ViewBag.icd10 = new SelectList(_member.GetIcd10Codes(), "icd10CodeID", "icd10CodeID");
            return View(model);
        }

        [HttpPost]
        public ActionResult _AddScriptItem(ScriptItems model)
        {
            var member = model.modifiedBy;
            model.modifiedBy = null;
            model.nappiCode = Request.Query["proName"].ToString();
            model.icd10code = Request.Query["icd10"].ToString();
            model.itemStatus = "Not yet authorised";
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.fromDate = DateTime.Now;
            model.toDate = Convert.ToDateTime("2039-12-31");
            var script = _member.GetScript(model.scriptID);
            var depId = script.dependentID;
            model.fromDate = script.effectiveDate;
            var result = _member.InsertScriptItem(model);
            if (member != null)
            {
                return RedirectToAction("patientClinical", new { DependentID = depId });
            }
            else
            {
                return RedirectToAction("AddScript", new { DependentID = depId, script = model.scriptID });
            }
        }

        [HttpPost]
        public JsonResult _AddScriptItemAjax(ScriptCreationViewModel model)
        {
            var nap = "";
            if (Request.Query["prodctName"].ToString() != null)
            {
                nap = Request.Query["prodctName"].ToString();
            }
            else if (Request.Query["proName"].ToString() != null)
            {
                nap = Request.Query["proName"].ToString();
            }
            var item = new ScriptItems()
            {
                nappiCode = nap,
                icd10code = Request.Query["icd10"].ToString(),
                itemStatus = "Not yet authorised",
                directions = model.item.directions,
                quantity = model.item.quantity,
                createdBy = User.Identity.Name,
                createdDate = DateTime.Now,
                fromDate = DateTime.Now,
                toDate = Convert.ToDateTime("2039-12-31"),
                scriptID = Convert.ToInt32(Request.Query["scriptsID"]),
            };
            var result = _member.InsertScriptItem(item);
            model.item = _member.GetScriptItems(item.scriptID).Where(x => x.itemNo == item.itemNo).FirstOrDefault();
            model.items = _member.GetScriptItems(item.scriptID);
            return Json(model);
        }

        public JsonResult UpdateScriptItem(ScriptItems model)
        {
            // Update model to your db 
            var item = _member.GetScriptItem(model.itemNo);
            item.toDate = model.toDate;
            item.fromDate = model.fromDate;
            item.active = model.active;
            item.quantity = model.quantity;
            item.directions = model.directions;
            item.modifiedBy = User.Identity.Name;
            item.modifiedDate = DateTime.Now;
            var result = _member.UpdateScriptItem(item);
            string message = "Failure";
            if (result.Success)
            {
                message = "Success";
            }

            return Json(message);
        }

        [HttpPost]
        public ActionResult AddScript(ScriptCreationViewModel model)
        {
            int scriptId = 0;
            model.script.scriptType = "GEN";

            var doc = _admin.GetDoctor(model.doctor.doctor.drLastName, model.script.practiceNo);

            if (doc != null)
            {
                model.script.doctorID = doc.doctorID;
                model.script.createdBy = User.Identity.Name;
                scriptId = _member.InsertScript(model.script);
                var res = _member.GetEnrolmentStep(model.script.dependentID);
                if (res != null)
                {
                    res.scriptCaptured = true;
                    var update = _member.UpdateEnrolmentStep(res);
                }

                return RedirectToAction("AddScript", new { DependentID = model.script.dependentID, script = scriptId });
            }
            else
            {
                model.doctor.doctor.practiceNo = model.script.practiceNo;
                model.doctor.doctor.drFirstName = "";
                model.doctor.doctor.drLastName = "";
                model.doctor.doctor.createdBy = User.Identity.Name;
                model.script.createdBy = User.Identity.Name;
                var res = _admin.InsertDoctor(model.doctor.doctor);
                model.script.doctorID = res;
                scriptId = _member.InsertScript(model.script);
                var rres = _member.GetEnrolmentStep(model.script.dependentID);
                if (rres != null)
                {
                    rres.scriptCaptured = true;
                    var update = _member.UpdateEnrolmentStep(rres);
                }
                return RedirectToAction("AddScript", new { DependentID = model.script.dependentID, script = scriptId });
            }
        }

        [HttpGet]
        public ActionResult GetProductInfo(string product)
        {
            var result = _admin.GetProductByName(product);
            return Json(result);
        }

        public ActionResult LoadComsPartial(Guid DependentID)
        {
            var model = new ComsViewModel();
            model.queries = new Queries();
            model.queriess = new List<Queries>();
            model.smsMessages = new SmsMessages();
            model.emailMessages = new Emails();
            model.address = new Address();
            model.contacts = new Contact();
            model.notes = new List<Notes>();
            model.assignments = new List<Assignments>();

            model.contacts = _member.GetContact(DependentID);
            if (model.contacts != null)
            {
                if (!String.IsNullOrEmpty(model.contacts.cell))
                    model.smsMessages.cell_no = model.contacts.cell;
                else
                    model.smsMessages.cell_no = model.contacts.ISeriesCell;

                model.emailMessages.emailTo = model.contacts.email;
            }
            else
            {
                model.contacts = new Contact();
            }
            model.emailMessages.dependantID = DependentID;
            model.address = _member.GetAddress(DependentID);
            if (model.address == null)
            {
                model.address = new Address();
            }
            model.queries.dependentID = DependentID;
            model.smsMessages.dependantID = DependentID;
            model.smsMessages.effectiveDate = DateTime.Now;
            model.emailMessages.effectivedate = DateTime.Now;
            model.notes = _member.GetNotes(DependentID);
            model.queriess = _member.GetQueriesByDependant(DependentID);
            ViewBag.Assignments = new SelectList(_member.GetActiveSelectAssignments(DependentID), "assignmentID", "AssignmentType");
            ViewBag.EnquiryType = new SelectList(_member.GetQueryTypes(), "queryTypes", "queryDescription");
            ViewBag.Priority = new SelectList(_member.GetPriorities(), "prioritytype", "priorityName");
            var templates = _member.GetTemplates();
            ViewBag.templates = new SelectList(templates, "templateId", "template");
            return PartialView("~/Views/Member/Partials/_Communications.cshtml", model);
        }

        public ActionResult LoadProfilePartial(Guid dependentId)
        {
            var model = _member.GetProfile(dependentId);
            model.attachment = new Attachments();
            model.attachment.dependentID = dependentId;
            model.patientPlanHistory = new PatientPlanHistory();
            model.patientPlanHistory.dependantId = dependentId;
            model.payPointHistory = new PayPointHistory();
            model.payPointHistory.dependantId = dependentId;
            ViewBag.attachmentType = new SelectList(_member.GetAttachmentTypes(), "attachmentType", "typeDescription");
            ViewBag.benefitPlan = new SelectList(model.patientPlans, "Id", "Name");
            ViewBag.paypoints = new SelectList(model.paypoints, "Id", "Name");
            model.attachments = _member.GetAttachments(dependentId);
            model.authLetters = _member.GetAuthorizationLetters(dependentId);

            return PartialView("~/Views/Member/Partials/_PatientProfile.cshtml", model);
        }

        public ActionResult _StepsRequired(Guid dependentId)
        {
            var model = _member.GetDependentDetails(dependentId, null);

            return PartialView("~/Views/Member/Partials/_StepsRequired.cshtml", model.enroll);
        }

        [HttpPost]
        public ActionResult InsertPlanHistory(PatientProfileViewModel model)
        {
            model.patientPlanHistory.planId = new Guid(Request.Query["benefitPlan"].ToString());
            model.patientPlanHistory.createdDate = DateTime.Now;
            model.patientPlanHistory.effectiveDate = DateTime.Now;
            var results = _member.InsertPlanHistory(model.patientPlanHistory);
            return RedirectToAction("patientProfile", new { DependentID = model.patientPlanHistory.dependantId });
        }

        public ActionResult LoadAssignmentsPartial(Guid dependentId)
        {
            ViewBag.AssignmentType = new SelectList(_member.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
            ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes(), "assignmentItemType", "itemDescription");
            ViewBag.AssignmentTask = new SelectList(_member.GetAssignmentTaskTypes(), "taskid", "taskDescription");
            ViewBag.AssignmentReq = new SelectList(_member.GetTaskRequirement(), "id", "requirementText");
            var model = new AssignmentsView()
            {
                activeAssignments = _member.GetActiveAssignments(dependentId),
                closedAssignments = _member.GetClosedAssignments(dependentId),
                newAssignment = new Assignments() { dependentID = dependentId },
            };

            return PartialView("~/Views/Member/Partials/_Assignments.cshtml", model);
        }


        [HttpPost]
        public ActionResult AddAssignment(AssignmentsView model)
        {
            model.newAssignment.assignmentType = Request.Query["assignmentType"].ToString();

            if (User.Identity.IsAuthenticated)
            {
                if (User.HasRole("6. Global user") || (User.HasRole("5. Super user")) || (User.HasRole("4. Advisor")))
                {
                    model.assignmentItemType = Request.Query["assignmentItemType"].ToString().Trim();
                }
                else
                {
                    model.assignmentItemType = Request.Query["AssignmentItemType_agent"].ToString().Trim();
                }
            }

            model.newAssignment.createdBy = User.Identity.Name;
            model.newAssignment.programId = new Guid(Request.Query["pro"]);
            var assignExists = _member.GetAssignment(new Guid(Request.Query["newAssignment.dependentID"]), model.assignmentItemType, model.newAssignment.programId);
            if (assignExists == null)
            {
                ServiceResult result = _member.InsertAssignment(model);

                var task = new AssignmentItemTasks();
                task.assignmentItemID = model.itemID;

                var tasks = _member.GetTaskRequirements(model.assignmentItemType);

                foreach (var tas in tasks)
                {
                    task.taskTypeId = tas.taskType;
                    task.requirementId = tas.requirementId;
                    _member.InsertTask(task);
                }
            }


            return RedirectToAction("patientAssignments", new { DependentID = model.newAssignment.dependentID, pro = Request.Query["pro"] });
        }

        //NAMIBIAN AUTH LETTER
        [HttpPost]
        public ActionResult NAMAuthLetter(AuthLetterView model)
        {
            var doctor = Request.Query["Doctor"].ToString();
            if (doctor == "true,false")
            {
                model.authletter.sentTo += ", " + model.drEmail;
            }
            var patient = Request.Query["Patient"].ToString();
            if (patient == "true,false")
            {
                model.authletter.sentTo += ", " + model.contact.email;
            }
            var dispenser = Request.Query["DispensingProvider"].ToString();
            if (dispenser == "true,false")
            {
                model.authletter.sentTo += ", " + model.dipensingEmail;
            }
            model.dependantID = model.authletter.dependantID;
            var depID = model.authletter.dependantID;
            var authl = new AuthorizationLetters();
            authl.createdBy = User.Identity.Name;
            authl.dependantID = depID;
            authl.sentTo = model.authletter.sentTo;
            authl.sentVia = model.authletter.sentVia;
            model.address = new Address();
            model.contact = new Contact();
            model.scriptItems = new List<ScriptViewModel>();
            model.dependantID = depID;
            model.scriptItems = _member.GetScriptItems(model.scriptNo);
            authl.scriptNo = model.scriptNo;
            model.medaidpic = String.Format("/Content/Images/2017Scheme/{0}", model.schemeCode);
            model.referenceNo = ValidationHelpers.GenerateUniqueCode(model.scriptNo);
            authl.AuthLetterID = model.referenceNo;
            //model.medaidpic = String.Format("file://OH17/Content/Images/2017Scheme/{0}", model.schemeCode);
            //HtmlToPdf Renderer = new HtmlToPdf();
            try
            {
                var doc1 = new Document();
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads\\";
                string imagepath = Server.MapPath("~/Content/Images/2017Scheme");

                var filename = "AuthLetter_" + model.membershipno + "_" + model.dependentCode + "_" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(doc1, new FileStream(Path.Combine(path, filename), FileMode.Create));
                doc1.Open();
                PdfContentByte cb = writer.DirectContent;
                Image jpg = Image.GetInstance(imagepath + "/haloCare_logo.png");
                jpg.ScalePercent(5f);
                jpg.SetAbsolutePosition(doc1.PageSize.Width - 550f, doc1.PageSize.Height - 120f);
                doc1.Add(jpg);
                Image jpeg = Image.GetInstance(imagepath + "/" + model.schemeCode);
                jpeg.ScalePercent(20f);
                jpeg.SetAbsolutePosition(doc1.PageSize.Width - 180f, doc1.PageSize.Height - 140f);
                doc1.Add(jpeg);


                Paragraph heading = new Paragraph("Authorisation", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                heading.SpacingAfter = 200f;
                doc1.Add(heading);
                if (model.address != null)
                {
                    ColumnText ct = new ColumnText(cb);
                    Phrase line1 = new Phrase(model.address.line1 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line1, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line2 = new Phrase(model.address.line2 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line2, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line3 = new Phrase(model.address.line3 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line3, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line4 = new Phrase(model.address.city + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line4, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line5 = new Phrase(model.address.province + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line5, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line6 = new Phrase(model.address.zip + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line6, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    ct.Go();
                }
                ColumnText cr = new ColumnText(cb);
                Phrase date = new Phrase("Date: " + DateTime.Now.ToShortDateString() + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(date, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Member = new Phrase("Member No: " + model.membershipno + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Member, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase surname = new Phrase("Surname: " + model.surname + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(surname, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Name = new Phrase("Patient: " + model.name + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Name, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Dependent = new Phrase("Dependant: " + model.dependentCode + "\n", new Font(Font.FontFamily.HELVETICA, 8f));//HCare-957
                cr.SetSimpleColumn(Dependent, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Dob = new Phrase("Date of Birth: " + model.dateOfBirth + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Dob, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Option = new Phrase("Option: " + model.schemeName + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Option, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                cr.Go();
                Paragraph par1 = new Paragraph(@"Dear " + model.name + " " + model.surname, new Font(Font.FontFamily.HELVETICA, 10f));
                par1.SpacingAfter = 9f;
                doc1.Add(par1);
                Paragraph par2 = new Paragraph(@"The medicine application has been received and reviewed according to current medical fund rules. Refer to your medical fund brochure for information on the different benefits, service providers and co-payments applicable.", new Font(Font.FontFamily.HELVETICA, 8f));
                par2.SpacingAfter = 9f;
                doc1.Add(par2);
                Paragraph par3 = new Paragraph(@"Please ensure that you take your medication exactly as prescribed by your doctor. It is very important to ensure that you have a valid prescription from your doctor for these medicines and that this prescription has been forwarded to Prosperity.", new Font(Font.FontFamily.HELVETICA, 8f));
                par3.SpacingAfter = 9f;
                doc1.Add(par3);

                PdfPTable table1 = new PdfPTable(1);
                PdfPCell cell = new PdfPCell(new Phrase("Regular follow-up pathology tests have also been recommended to monito4r your medical condition on an ongoing basis.", new Font(Font.FontFamily.HELVETICA, 8f)));
                cell.HorizontalAlignment = 1;
                table1.AddCell(cell);
                table1.SpacingAfter = 9f;
                doc1.Add(table1);

                PdfPTable table2 = new PdfPTable(7);
                table2.AddCell(new Phrase("Nappi/Tariff Code", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Product Name", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Quantity", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Directions", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("From Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("To Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Comment", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                cell.HorizontalAlignment = 1;
                table2.SpacingAfter = 9f;
                foreach (var item in model.scriptItems)
                {
                    table2.AddCell(new Phrase(item.nappiCode, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.productName, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.quantity.ToString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.directions, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.fromDate.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.toDate.Value.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase("", new Font(Font.FontFamily.HELVETICA, 8f)));
                }
                doc1.Add(table2);

                Paragraph note = new Paragraph("Please note:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(note);
                Paragraph noted = new Paragraph("Please notify us of any changes to the treatment script to ensure correct payment of your claim. All authorisations are subject to the availability of funds applicable to the benefit. Should the cost of your medication exceed the funds available, the amount exceeding the benefit will be for you own account.", new Font(Font.FontFamily.HELVETICA, 8f));
                noted.SpacingAfter = 9f;
                doc1.Add(noted);

                Paragraph nam = new Paragraph("Namibia reference price (NRP) is applicable to your medicines.", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(nam);
                Paragraph det = new Paragraph("NRP sets a maximum reimbursable price for a list of generically similar products with a cost lower than that of the original medicine. This means that if you select to use the original product for which a generic alternative is available or a generic alternative above NRP, you will have to pay the difference between the price of the chosen medicine and that of NRP. This is applicable to both formulary and non-formulary medicines. We encourage the use of generically similar medicines in the interest of cost-effective care.", new Font(Font.FontFamily.HELVETICA, 8f));
                det.SpacingAfter = 9f;
                doc1.Add(det);
                Paragraph nams = new Paragraph("Renewal of authorisation:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(nams);
                Paragraph dets = new Paragraph("Prescription and/or additional information can be faxed to 0866 522 647/0886 30469. Alternatively, you can email us at preauth@na.prospertiyhealth.com.", new Font(Font.FontFamily.HELVETICA, 8f));
                dets.SpacingAfter = 9f;
                doc1.Add(dets);

                Paragraph rendet = new Paragraph("Please ensure that you have the follow-up tests done by visiting your treating doctor. Arrange with the laboratory to have the results of the pathology tests forwarded to both your treating doctor and Prosperity, since this will assist us to monitor your condition. Kindly ask your treating doctor for a repeat prescription which must be forwarded to Prosperity. ", new Font(Font.FontFamily.HELVETICA, 8f));
                rendet.SpacingAfter = 15f;
                doc1.Add(rendet);

                Paragraph disclaimer = new Paragraph("Disclaimer:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(disclaimer);
                Paragraph disdet = new Paragraph("Please note that the benefit details provided above are correct at the time of printing. Any changes to your membership status, plan selection, authorisation/s or the funds clinical policies and guidelines may affect your benefits. ", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet);

                Paragraph s = new Paragraph("Yours in health", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(s);
                Paragraph sig2 = new Paragraph("Prosperity", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(sig2);

                doc1.Close();


                var att = new Attachments();
                att.dependentID = depID;
                att.createdBy = User.Identity.Name;
                att.attachmentType = "LET";
                att.attachmentName = filename;
                att.Link = att.attachmentName;
                att.createdBy = User.Identity.Name;
                _member.InsertAttachment(att);

                authl.AuthLinkID = att.attachmentID;

                var authlet = _member.InsertAuthLetter(authl);

                if (authl.sentVia == "Email")
                {
                    var list = new List<int>();
                    foreach (var row in model.attachments)
                    {
                        if (Request.Query[row.attachmentID.ToString()].ToString() != null)
                        {
                            var checkIt = Request.Query[row.attachmentID.ToString()].ToString();
                            if (checkIt == "true,false")
                            {
                                list.Add(row.attachmentID);
                            }
                        }
                    }

                    var email = new Management.EmailService();
                    string htmlmail = @"<style>
                                table {
                                    width: 100%;
                                }

                                .header {
                                    background-color: #1f4e79;
                                    color: #ffffff;
                                }

                                .authorizationletter {
                                    font-family: Calibri !important;
                                    font-size: 10px !important;
                                }
                            </style>
            <h4>Dear" + authl.sentTo + @"</h4>
            <hr />
            <br />
            Please find the authorization letter for " + model.name + @" " + model.surname + @". 
            < br /><br />
            Managed Care contact details:<br />
            Tel: 0860 143 258<br />
            Email: info @onehealth.co.za<br />
            <br />
            Kind regards<br />
            Managed Care Team<br />
        </div>";
                    var sendmail = email.SendEmail(authl.sentTo, "Authorisation Letter" + DateTime.Now.ToShortDateString(), htmlmail, true,
                        path + att.attachmentName, att.attachmentName, list.ToArray());
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
                return View(model);
            }

            return RedirectToAction("patientDashboard", new { DependentID = depID });
        }

        [HttpPost]
        public ActionResult AuthLetter(AuthLetterView model)
        {
            var doctor = Request.Query["Doctor"].ToString();
            if (doctor == "true,false")
            {
                model.authletter.sentTo += ", " + model.drEmail;
            }
            var patient = Request.Query["Patient"].ToString();
            if (patient == "true,false")
            {
                model.authletter.sentTo += ", " + model.contact.email;
            }
            var dispenser = Request.Query["DispensingProvider"].ToString();
            if (dispenser == "true,false")
            {
                model.authletter.sentTo += ", " + model.dipensingEmail;
            }
            model.dependantID = model.authletter.dependantID;
            var depID = model.authletter.dependantID;
            var authl = new AuthorizationLetters();
            authl.createdBy = User.Identity.Name;
            authl.dependantID = depID;
            authl.sentTo = model.authletter.sentTo;
            authl.sentVia = model.authletter.sentVia;
            model.address = new Address();
            model.contact = new Contact();
            model.scriptItems = new List<ScriptViewModel>();
            model.dependantID = depID;
            model.scriptItems = _member.GetScriptItems(model.scriptNo);
            authl.scriptNo = model.scriptNo;
            model.medaidpic = String.Format("/Content/Images/2017Scheme/{0}", model.schemeCode);
            model.referenceNo = ValidationHelpers.GenerateUniqueCode(model.scriptNo);
            authl.AuthLetterID = model.referenceNo;
            //model.medaidpic = String.Format("file://OH17/Content/Images/2017Scheme/{0}", model.schemeCode);
            //HtmlToPdf Renderer = new HtmlToPdf();
            try
            {
                var doc1 = new Document();
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads\\";
                string imagepath = Server.MapPath("~/Content/Images/2017Scheme");

                var filename = "AuthLetter_" + model.membershipno + "_" + model.dependentCode + "_" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(doc1, new FileStream(Path.Combine(path, filename), FileMode.Create));
                doc1.Open();
                PdfContentByte cb = writer.DirectContent;
                Image jpg = Image.GetInstance(imagepath + "/haloCare_logo.png");
                jpg.ScalePercent(4f);
                jpg.SetAbsolutePosition(doc1.PageSize.Width - 550f, doc1.PageSize.Height - 120f);
                doc1.Add(jpg);
                Image jpeg = Image.GetInstance(imagepath + "/" + model.schemeCode);
                jpeg.ScalePercent(18f);
                jpeg.SetAbsolutePosition(doc1.PageSize.Width - 180f, doc1.PageSize.Height - 140f);
                doc1.Add(jpeg);


                Paragraph heading = new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 2f, Font.BOLD));
                heading.SpacingBefore = 0f;
                heading.SpacingAfter = 200f;
                doc1.Add(heading);
                if (model.address != null)
                {
                    ColumnText ct = new ColumnText(cb);
                    Phrase line1 = new Phrase(model.address.line1 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line1, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line2 = new Phrase(model.address.line2 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line2, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line3 = new Phrase(model.address.line3 + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line3, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line4 = new Phrase(model.address.city + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line4, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line5 = new Phrase(model.address.province + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line5, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    Phrase line6 = new Phrase(model.address.zip + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                    ct.SetSimpleColumn(line6, 34, 680, 580, 317, 15, Element.ALIGN_LEFT);
                    ct.Go();
                }
                ColumnText cr = new ColumnText(cb);
                Phrase date = new Phrase("Date: " + DateTime.Now.ToShortDateString() + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(date, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Member = new Phrase("Member No: " + model.membershipno + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Member, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase surname = new Phrase("Surname: " + model.surname + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(surname, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Name = new Phrase("Patient: " + model.name + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Name, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase ContactNo = new Phrase("Mobile: " + model.contact.cell + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(ContactNo, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase TrDoctor = new Phrase("Doctor: " + model.drName + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(TrDoctor, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Dependent = new Phrase("Dependant: " + model.dependentCode + "\n", new Font(Font.FontFamily.HELVETICA, 8f));//HCare-957
                cr.SetSimpleColumn(Dependent, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Dob = new Phrase("Date of Birth: " + model.dateOfBirth + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Dob, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);
                Phrase Option = new Phrase("Option: " + model.schemeName + "\n" + "\n", new Font(Font.FontFamily.HELVETICA, 8f));
                cr.SetSimpleColumn(Option, 34, 690, 550, 317, 15, Element.ALIGN_RIGHT);

                cr.Go();

                Paragraph par1 = new Paragraph(@"Dear " + model.name + " " + model.surname, new Font(Font.FontFamily.HELVETICA, 10f));
                par1.SpacingBefore = 40f;
                par1.SpacingAfter = 9f;
                doc1.Add(par1);
                Paragraph par2 = new Paragraph(@"Your prescription has been received, reviewed and authorised according to clinical guidelines and current medical scheme rules. ", new Font(Font.FontFamily.HELVETICA, 8f));
                par2.SpacingAfter = 9f;
                doc1.Add(par2);
                Paragraph par3 = new Paragraph(@"Please ensure that you take your medication exactly as prescribed by your doctor. Six monthly follow-up pathology test results are recommended to monitor your medical condition on an ongoing basis. Remember to notify HaloCare of any change in medication script or dose.", new Font(Font.FontFamily.HELVETICA, 8f));
                par3.SpacingAfter = 9f;
                doc1.Add(par3);

                PdfPTable table1 = new PdfPTable(1);
                PdfPCell cell = new PdfPCell(new Phrase("Authorisation Details", new Font(Font.FontFamily.HELVETICA, 10f)));
                cell.HorizontalAlignment = 1;
                table1.AddCell(cell);
                table1.WidthPercentage = 95;
                table1.SpacingAfter = 9f;
                doc1.Add(table1);

                PdfPTable table2 = new PdfPTable(7);
                table2.AddCell(new Phrase("Nappi/Tariff Code", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Product Name", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Quantity", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Directions", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("From Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("To Date", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                table2.AddCell(new Phrase("Comment", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
                cell.HorizontalAlignment = 1;
                table2.WidthPercentage = 95;
                table2.SpacingAfter = 9f;
                foreach (var item in model.scriptItems)
                {
                    table2.AddCell(new Phrase(item.nappiCode, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.productName, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.quantity.ToString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.directions, new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.fromDate.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase(item.toDate.Value.ToShortDateString(), new Font(Font.FontFamily.HELVETICA, 8f)));
                    table2.AddCell(new Phrase("", new Font(Font.FontFamily.HELVETICA, 8f)));
                }
                doc1.Add(table2);

                Paragraph note = new Paragraph("Please note:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(note);
                Paragraph noted = new Paragraph("All medication authorisations are subject to clinical review and approval. Medication claims payment is subject to medical scheme rules and medicine formulary.", new Font(Font.FontFamily.HELVETICA, 8f));
                noted.SpacingAfter = 9f;
                doc1.Add(noted);

                Paragraph nam = new Paragraph("Renewal or change of medication authorisation.", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(nam);
                Paragraph det = new Paragraph("Prescriptions and relevant pathology results to be forwarded to documents@halocare.co.za or faxed to 086 570 2523.", new Font(Font.FontFamily.HELVETICA, 8f));

                doc1.Add(det);

                Paragraph rendet = new Paragraph("Weight must be noted on prescription for all members up to 13years of age or <40kg.", new Font(Font.FontFamily.HELVETICA, 8f));
                rendet.SpacingAfter = 15f;
                doc1.Add(rendet);

                Paragraph disclaimer = new Paragraph("Disclaimer:", new Font(Font.FontFamily.HELVETICA, 10f, Font.BOLD));
                doc1.Add(disclaimer);
                Paragraph disdet = new Paragraph("1. This authorisation is based on clinical guidelines and scheme rules. Authorisation is not a guarantee of payment.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet);
                Paragraph disdett = new Paragraph("2.The payment of claims is subject to the validity of the membership, the Scheme rules and the available benefits as on the date when the service was rendered.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdett);
                Paragraph disdet1 = new Paragraph("3.The authorisation is only valid for the dependant listed in this authorisation schedule and is granted based on the information available at the time.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet1);
                Paragraph disdet2 = new Paragraph("4.The authorisation is only valid for the time period indicated and a new prescription is required to renew the authorisation. ", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet2);
                Paragraph disdett3 = new Paragraph("5.Any changes to the approved items must be communicated to the Halocare Managed Care and failure to do so may result in non-payment of medication.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdett3);
                Paragraph disdet11 = new Paragraph("6.In the event of an option change, it remains the responsibility of the member to confirm how the option change will affect the authorisation.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet11);
                Paragraph disdet4 = new Paragraph("7.Medication is restricted to formularies (medicine lists), clinical entry criteria and disease management protocols where applicable.", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(disdet4);
                Paragraph disdett5 = new Paragraph("8.Medication requires a prescription from a person legally entitled to prescribe as well as the relevant ICD 10 diagnosis code, visible and valid date and signature of the prescriber, before it will be considered for benefits.", new Font(Font.FontFamily.HELVETICA, 8f));
                disdett5.SpacingAfter = 10f;
                doc1.Add(disdett5);

                Paragraph s = new Paragraph("Halocare Managed Care contact details", new Font(Font.FontFamily.HELVETICA, 7f));
                doc1.Add(s);
                Paragraph sig2 = new Paragraph("Tel: 086 570 2523", new Font(Font.FontFamily.HELVETICA, 7f));
                doc1.Add(sig2);
                Paragraph sig3 = new Paragraph("Email: info@halocare.co.za", new Font(Font.FontFamily.HELVETICA, 7f));
                sig3.SpacingAfter = 10f;
                doc1.Add(sig3);

                Paragraph reg = new Paragraph("Kind Regards", new Font(Font.FontFamily.HELVETICA, 8f));
                doc1.Add(reg);
                Paragraph sig = new Paragraph("Halocare Managed Care Team", new Font(Font.FontFamily.HELVETICA, 8f));
                sig.SpacingAfter = 9f;
                doc1.Add(sig);
                doc1.Close();


                var att = new Attachments();
                att.dependentID = depID;
                att.createdBy = User.Identity.Name;
                att.attachmentType = "LET";
                att.attachmentName = filename;
                att.Link = att.attachmentName;
                att.createdBy = User.Identity.Name;
                _member.InsertAttachment(att);

                authl.AuthLinkID = att.attachmentID;

                var authlet = _member.InsertAuthLetter(authl);

                if (authl.sentVia == "Email")
                {
                    var list = new List<int>();
                    foreach (var row in model.attachments)
                    {
                        if (Request.Query[row.attachmentID.ToString()].ToString() != null)
                        {
                            var checkIt = Request.Query[row.attachmentID.ToString()].ToString();
                            if (checkIt == "true,false")
                            {
                                list.Add(row.attachmentID);
                            }
                        }
                    }

                    var email = new Management.EmailService();
                    string htmlmail = @"<style>
                                table {
                                    width: 100%;
                                }

                                .header {
                                    background-color: #1f4e79;
                                    color: #ffffff;
                                }

                                .authorizationletter {
                                    font-family: Calibri !important;
                                    font-size: 10px !important;
                                }
                            </style>
            <h4>Dear" + authl.sentTo + @"</h4>
            <hr />
            <br />
            Please find the authorization letter for " + model.name + @" " + model.surname + @". 
            < br /><br />
            Managed Care contact details:<br />
            Tel: 0860 143 258<br />
            Email: info @halocare.co.za<br />
            <br />
            Kind regards<br />
            HaloCare Team<br />
            Managed Care
        </div>";
                    var sendmail = email.SendEmail(authl.sentTo, "Authorisation Letter " + model.name + " " + model.surname + " " + model.membershipno + DateTime.Now.ToShortDateString(), htmlmail, true,
                        path + att.attachmentName, att.attachmentName, list.ToArray());
                }

            }
            catch (Exception ex)
            {
                string filePath = @"d:\data\Logs\HC\Error.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Auth Error: " + Environment.NewLine + "Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                return View(model);
            }

            return RedirectToAction("patientDashboard", new { DependentID = depID });
        }

        [HttpPost]
        public ActionResult InsertPayPoint(PatientProfileViewModel model)
        {
            model.payPointHistory.createdBy = User.Identity.Name;
            model.payPointHistory.planId = Request.Query["paypoints"]; /*HCare-923*/
            var result = _member.InsertPaypointHistory(model.payPointHistory);
            return RedirectToAction("patientProfile", new { DependentID = model.payPointHistory.dependantId });
        }

        public ActionResult AuthoriseScript(int scriptID)
        {
            var model = new AuthorisationViewModel();
            model.script = _member.GetScript(scriptID);
            model.scriptItems = _member.GetScriptItems(scriptID);

            return View(model);
        }

        [HttpPost]
        public ActionResult AuthoriseScript(AuthorisationViewModel ScriptAuthmodel)
        {
            return null;
            /*var scriptInfo = new Scripts();
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
            }
            scriptInfo.modifiedBy = User.Identity.Name;
            scriptInfo.modifiedDate = DateTime.Now;
            scriptInfo.active = true;

            var res = _member.UpdateScript(scriptInfo);

            ScriptAuthmodel.memberinformation = _member.GetDependentDetails(ScriptAuthmodel.script.dependentID);
            var doctorinfo = _admin.GetDoctor(ScriptAuthmodel.script.doctorID);
            ScriptAuthmodel.memberinformation.doctor = new Doctors();
            ScriptAuthmodel.memberinformation.doctor.practiceNo = doctorinfo.practiceNo;
            //generate auth request here
            foreach (var row in ScriptAuthmodel.scriptItems.ToList())
            {
                if (!row.authItem)
                {
                    ScriptAuthmodel.scriptItems.Remove(row);
                }
            }

            var failed = 0;
            //if (ScriptAuthmodel.memberinformation.MedicalAids.medicalAidCode.Contains("SPEC"))
            if (1 == 0)
            {
                try
                {
                    //generate auth request here
                    var Authxml = Authorise.GenerateSpectramedNappisAuthXml(ScriptAuthmodel);
                    string xml = Authxml.InnerXml.ToString();
                    int entryLineNo = Authorise.LogAuthRequest(3006, ScriptAuthmodel.script.scriptID, ScriptAuthmodel.script.dependentID, xml, "", "System");

                    var AuthService = new KosWSService();
                    var response = AuthService.processAuth(xml);
                    Authorise.UpdateAuthRequest(entryLineNo, xml, response, "System");

                    if (response.Contains("Authorisation created successfully") || response.Contains("Authorisation updated successfully") || response.Contains("Possible Duplicate Agility auth exists"))
                    {
                        _member.UpdateScriptItems(ScriptAuthmodel.scriptItems, "Authorised");

                    }
                    else
                    {
                        _member.UpdateScriptItems(ScriptAuthmodel.scriptItems, "Rejected");
                    }
                }
                catch (Exception ex)
                {
                    var text = ex.Message;
                }
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
                    xsl.Load(Server.MapPath("../xml/AuthoriseScript.xslt"));

                    // load xslt arguments to load specific page from xml file
                    // this can be used if you have multiple pages
                    // in your xml file and you loading them one at a time
                    XsltArgumentList xslarg = new XsltArgumentList();

                    // get transformed results
                    StringWriter sw = new StringWriter();
                    xsl.Transform(xdoc, xslarg, sw);
                    string result = sw.ToString();

                    System.IO.File.AppendAllText(@"D:\Data\Logs\mediscor\ProcessScriptAuthorisationLog.txt"
                        , string.Format("{1}: {0}input: {3} {0}result: {4} {0}response: {2} {0}check : {5} {0}|{0}"
                            , Environment.NewLine, DateTime.Now, PV.OuterXml, xml, result, xdoc.SelectSingleNode("d/@st").Value));

                    // free up the memory of objects that are not used anymore
                    if (xdoc.SelectSingleNode("d/@st").Value != "0")
                    {
                        _member.UpdateScriptItems(ScriptAuthmodel.scriptItems, "Authorised");
                        Authorise.UpdateAuthRequest(entryLineNo, xml, xsl.ToString(), User.Identity.Name);

                        System.IO.File.AppendAllText(@"D:\Data\Logs\mediscor\ProcessScriptAuthorisationLog.txt"
                           , string.Format("{1}: {0}UpdateScriptDetails: {0}|{0}"
                               , Environment.NewLine, DateTime.Now));
                    }
                    else
                    {
                        failed = 1;
                        _member.UpdateScriptItems(ScriptAuthmodel.scriptItems, "Rejected");
                        Response.WriteAsync("<span class=\"red\">Script could not be authorised at this time</span>");
                    }
                    sw.Close();
                    Response.WriteAsync(result);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(@"D:\Data\Logs\mediscor\ProcessScriptAuthorisationLog.txt"
                        , string.Format("{3}: error: {0}{1}{2}|{1}", ex.Message, Environment.NewLine, ex.StackTrace, DateTime.Now));
                }
            }
            if (failed != 0)
            {
                return View(ScriptAuthmodel);
            }
            else
            {
                return RedirectToAction("AuthLetter", new { DependentID = ScriptAuthmodel.script.dependentID });
            }*/
        }

        public ActionResult AddClinicalHistory(Guid DependentID)
        {
            var exists = _member.getQuestionnaire(DependentID);
            if (exists == null)
            {
                var model = new ClinicalHistoryQuestionaire();
                model.dependentID = DependentID;
                model.DiagnosisDate = DateTime.Now;
                return View(model);
            }
            else
            {
                return RedirectToAction("AddMedication", new { DependentID = DependentID });
            }
        }

        [HttpPost]
        public ActionResult AddClinicalHistory(ClinicalHistoryQuestionaire model)
        {
            model.createdBy = User.Identity.Name;
            model.active = true;
            model.createdDate = DateTime.Now;
            model.DiagnosisDate = DateTime.Now;

            var result = _member.InsertClinicalHistoryQuestionnaire(model);
            if (result.Success)
            {
                return RedirectToAction("AddMedication", new { DependentID = model.dependentID });
            }
            else
            {
                return View(model);
            }
        }



        public ActionResult EditQuestionaire(Guid DependentID)
        {
            var exists = _member.getQuestionnaire(DependentID);
            if (exists != null)
            {
                return View(exists);
            }
            else
            {
                var model = new ClinicalHistoryQuestionaire();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditQuestionaire(ClinicalHistoryQuestionaire model)
        {
            var exists = _member.getQuestionnaire(model.dependentID);
            if (exists != null)
            {
                model.modifiedBy = User.Identity.Name;
                model.modifieddate = DateTime.Now;

                var result = _member.UpdateQuestionnaire(model);
            }
            else
            {
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;

                var result = _member.InsertClinicalHistoryQuestionnaire(model);
            }

            return RedirectToAction("patientClinical", new { DependentID = model.dependentID });

        }

        public ActionResult Medication_History(Guid DependentID, int script = 0) //HCare-1023
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();

            ViewBag.DependantID = DependentID;

            var scriptVM = new ScriptCreationViewModel();
            var scripts = new Scripts();

            scriptVM.script = scripts;
            scriptVM.script.dependentID = DependentID;
            scriptVM.script.effectiveDate = DateTime.Now;
            scriptVM.script.scriptID = script;
            scriptVM.items = _member.GetScriptItemsFull(DependentID).Where(x => x.program != programCode).ToList();
            scriptVM.DiabetesItems = _member.GetAllDiabetesScriptItems(DependentID);
            scriptVM.HIVItems = _member.GetAllHIVScriptItems(DependentID);
            scriptVM.MHItems = _member.GetAllMHScriptItems(DependentID);

            return View(scriptVM);
        }

        public ActionResult ExportGeneralMedicationHistory(ScriptCreationViewModel model) //HCare-1145
        {

            model.items = _member.GetScriptItemsFull(model.script.dependentID);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Effective date");
            table.Columns.Add("Repeats");
            table.Columns.Add("Product name");
            table.Columns.Add("NAPPI");
            table.Columns.Add("ATC Code");
            table.Columns.Add("MIMS Class description");
            table.Columns.Add("MIMS Class number");
            table.Columns.Add("MMAP Indicator");
            table.Columns.Add("MRP Indicator");
            table.Columns.Add("Pack unit of measure");
            table.Columns.Add("Generic Indicator");
            table.Columns.Add("Medicine Schedule");
            table.Columns.Add("product metric strength");
            table.Columns.Add("Authorisation outcome");
            table.Columns.Add("Quantity");
            table.Columns.Add("From date");
            table.Columns.Add("To date");
            table.Columns.Add("Item repeats");
            table.Columns.Add("Modified by");
            table.Columns.Add("Modified date");
            table.Columns.Add("Item status");
            table.Columns.Add("Strength");
            table.Columns.Add("Item #");
            table.Columns.Add("Benefit type");
            table.Columns.Add("Script ID");
            table.Columns.Add("Prescribing Dr.");
            table.Columns.Add("Auth item");
            table.Columns.Add("ICD-10");
            table.Columns.Add("Code type");
            table.Columns.Add("Prophylaxis");
            table.Columns.Add("Line note");
            table.Columns.Add("Created by");
            table.Columns.Add("Active");
            table.Columns.Add("CL item #");
            table.Columns.Add("Program");

            foreach (var line in model.items)
            {
                DataRow row = table.NewRow();
                row["Effective date"] = line.effectiveDate;
                row["Repeats"] = line.repeats;
                row["Product name"] = line.productName;
                row["NAPPI"] = line.nappiCode;
                row["ATC Code"] = line.atcCode;
                row["MIMS Class description"] = line.therapeuticClass6Desc;
                row["MIMS Class number"] = line.therapeuticClass6;
                row["MMAP Indicator"] = line.mmapIndicator;
                row["MRP Indicator"] = line.mrpIndicator;
                row["Pack unit of measure"] = line.packUOM;
                row["Generic Indicator"] = line.genericIndicator;
                row["Medicine Schedule"] = line.productSchedule;
                row["product metric strength"] = line.strengthUOM;
                row["Authorisation outcome"] = line.directions;
                row["Quantity"] = line.quantity;
                row["From date"] = line.fromDate.ToString("dd-MM-yyyy");
                row["To date"] = line.toDate.Value.ToString("dd-MM-yyyy");
                row["Item repeats"] = line.itemRepeats;
                row["Modified by"] = line.modifiedBy;
                row["Modified date"] = line.modifiedDate;
                if (line.itemStatus.ToLower() == "authorised")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "pending")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "rejected")
                { row["Item status"] = line.itemStatus; }
                row["Strength"] = line.strength;
                row["Item #"] = line.itemNo;
                row["Benefit type"] = line.benefitType;
                row["Script ID"] = line.scriptID;
                row["Prescribing Dr."] = line.prescribingDr;
                row["Auth item"] = line.authItem;
                row["ICD-10"] = line.icd10code;
                row["Code type"] = line.icd10Type;
                row["Prophylaxis"] = line.prophylaxis;
                row["Line note"] = line.comment;
                row["Created by"] = line.createdBy;
                row["Active"] = line.active;
                row["CL item #"] = line.CLItemLineNo;
                row["Program"] = line.program;

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
                Response.Headers.Add("content-disposition", "attachment;  filename=medication-history.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }
        public ActionResult ExportDiabetesMedicationHistory(ScriptCreationViewModel model) //HCare-1145
        {

            model.items = _member.GetAllDiabetesScriptItems(model.script.dependentID);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Effective date");
            table.Columns.Add("Repeats");
            table.Columns.Add("Product name");
            table.Columns.Add("NAPPI");
            table.Columns.Add("ATC Code");
            table.Columns.Add("MIMS Class description");
            table.Columns.Add("MIMS Class numbe");
            table.Columns.Add("MMAP Indicator");
            table.Columns.Add("MRP Indicator");
            table.Columns.Add("Pack unit of measure");
            table.Columns.Add("Generic Indicator");
            table.Columns.Add("Medicine Schedule");
            table.Columns.Add("product metric strength");
            table.Columns.Add("Authorisation outcome");
            table.Columns.Add("Quantity");
            table.Columns.Add("From date");
            table.Columns.Add("To date");
            table.Columns.Add("Item repeats");
            table.Columns.Add("Modified by");
            table.Columns.Add("Modified date");
            table.Columns.Add("Item status");
            table.Columns.Add("Strength");
            table.Columns.Add("Item #");
            table.Columns.Add("Benefit type");
            table.Columns.Add("Script ID");
            table.Columns.Add("Prescribing Dr.");
            table.Columns.Add("Auth item");
            table.Columns.Add("ICD-10");
            table.Columns.Add("Code type");
            table.Columns.Add("Prophylaxis");
            table.Columns.Add("Line note");
            table.Columns.Add("Created by");
            table.Columns.Add("Active");
            table.Columns.Add("CL item #");
            table.Columns.Add("Program");

            foreach (var line in model.items)
            {
                DataRow row = table.NewRow();
                row["Effective date"] = line.effectiveDate;
                row["Repeats"] = line.repeats;
                row["Product name"] = line.productName;
                row["NAPPI"] = line.nappiCode;
                row["ATC Code"] = line.atcCode;
                row["MIMS Class description"] = line.therapeuticClass6Desc;
                row["MIMS Class number"] = line.therapeuticClass6;
                row["MMAP Indicator"] = line.mmapIndicator;
                row["MRP Indicator"] = line.mrpIndicator;
                row["Pack unit of measure"] = line.packUOM;
                row["Generic Indicator"] = line.genericIndicator;
                row["Medicine Schedule"] = line.productSchedule;
                row["product metric strength"] = line.strengthUOM;
                row["Authorisation outcome"] = line.directions;
                row["Quantity"] = line.quantity;
                row["From date"] = line.fromDate.ToString("dd-MM-yyyy");
                row["To date"] = line.toDate.Value.ToString("dd-MM-yyyy");
                row["Item repeats"] = line.itemRepeats;
                row["Modified by"] = line.modifiedBy;
                row["Modified date"] = line.modifiedDate;
                if (line.itemStatus.ToLower() == "authorised")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "pending")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "rejected")
                { row["Item status"] = line.itemStatus; }
                row["Strength"] = line.strength;
                row["Item #"] = line.itemNo;
                row["Benefit type"] = line.benefitType;
                row["Script ID"] = line.scriptID;
                row["Prescribing Dr."] = line.prescribingDr;
                row["Auth item"] = line.authItem;
                row["ICD-10"] = line.icd10code;
                row["Code type"] = line.icd10Type;
                row["Prophylaxis"] = line.prophylaxis;
                row["Line note"] = line.comment;
                row["Created by"] = line.createdBy;
                row["Active"] = line.active;
                row["CL item #"] = line.CLItemLineNo;
                row["Program"] = line.program;

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
                Response.Headers.Add("content-disposition", "attachment;  filename=medication-history.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }
        public ActionResult ExportHIVMedicationHistory(ScriptCreationViewModel model) //HCare-1145
        {

            model.items = _member.GetAllHIVScriptItems(model.script.dependentID);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Effective date");
            table.Columns.Add("Repeats");
            table.Columns.Add("Product name");
            table.Columns.Add("NAPPI");
            table.Columns.Add("ATC Code");
            table.Columns.Add("MIMS Class description");
            table.Columns.Add("MIMS Class numbe");
            table.Columns.Add("MMAP Indicator");
            table.Columns.Add("MRP Indicator");
            table.Columns.Add("Pack unit of measure");
            table.Columns.Add("Generic Indicator");
            table.Columns.Add("Medicine Schedule");
            table.Columns.Add("product metric strength");
            table.Columns.Add("Authorisation outcome");
            table.Columns.Add("Quantity");
            table.Columns.Add("From date");
            table.Columns.Add("To date");
            table.Columns.Add("Item repeats");
            table.Columns.Add("Modified by");
            table.Columns.Add("Modified date");
            table.Columns.Add("Item status");
            table.Columns.Add("Strength");
            table.Columns.Add("Item #");
            table.Columns.Add("Benefit type");
            table.Columns.Add("Script ID");
            table.Columns.Add("Prescribing Dr.");
            table.Columns.Add("Auth item");
            table.Columns.Add("ICD-10");
            table.Columns.Add("Code type");
            table.Columns.Add("Prophylaxis");
            table.Columns.Add("Line note");
            table.Columns.Add("Created by");
            table.Columns.Add("Active");
            table.Columns.Add("CL item #");
            table.Columns.Add("Program");

            foreach (var line in model.items)
            {
                DataRow row = table.NewRow();
                row["Effective date"] = line.effectiveDate;
                row["Repeats"] = line.repeats;
                row["Product name"] = line.productName;
                row["NAPPI"] = line.nappiCode;
                row["ATC Code"] = line.atcCode;
                row["MIMS Class description"] = line.therapeuticClass6Desc;
                row["MIMS Class number"] = line.therapeuticClass6;
                row["MMAP Indicator"] = line.mmapIndicator;
                row["MRP Indicator"] = line.mrpIndicator;
                row["Pack unit of measure"] = line.packUOM;
                row["Generic Indicator"] = line.genericIndicator;
                row["Medicine Schedule"] = line.productSchedule;
                row["product metric strength"] = line.strengthUOM;
                row["Authorisation outcome"] = line.directions;
                row["Quantity"] = line.quantity;
                row["From date"] = line.fromDate.ToString("dd-MM-yyyy");
                row["To date"] = line.toDate.Value.ToString("dd-MM-yyyy");
                row["Item repeats"] = line.itemRepeats;
                row["Modified by"] = line.modifiedBy;
                row["Modified date"] = line.modifiedDate;
                if (line.itemStatus.ToLower() == "authorised")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "pending")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "rejected")
                { row["Item status"] = line.itemStatus; }
                row["Strength"] = line.strength;
                row["Item #"] = line.itemNo;
                row["Benefit type"] = line.benefitType;
                row["Script ID"] = line.scriptID;
                row["Prescribing Dr."] = line.prescribingDr;
                row["Auth item"] = line.authItem;
                row["ICD-10"] = line.icd10code;
                row["Code type"] = line.icd10Type;
                row["Prophylaxis"] = line.prophylaxis;
                row["Line note"] = line.comment;
                row["Created by"] = line.createdBy;
                row["Active"] = line.active;
                row["CL item #"] = line.CLItemLineNo;
                row["Program"] = line.program;

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
                Response.Headers.Add("content-disposition", "attachment;  filename=medication-history.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }

        public ActionResult ExportMHMedicationHistory(ScriptCreationViewModel model) //HCare-1183
        {

            model.items = _member.GetAllMHScriptItems(model.script.dependentID);

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Effective date");
            table.Columns.Add("Repeats");
            table.Columns.Add("Product name");
            table.Columns.Add("NAPPI");
            table.Columns.Add("ATC Code");
            table.Columns.Add("MIMS Class description");
            table.Columns.Add("MIMS Class numbe");
            table.Columns.Add("MMAP Indicator");
            table.Columns.Add("MRP Indicator");
            table.Columns.Add("Pack unit of measure");
            table.Columns.Add("Generic Indicator");
            table.Columns.Add("Medicine Schedule");
            table.Columns.Add("product metric strength");
            table.Columns.Add("Authorisation outcome");
            table.Columns.Add("Quantity");
            table.Columns.Add("From date");
            table.Columns.Add("To date");
            table.Columns.Add("Item repeats");
            table.Columns.Add("Modified by");
            table.Columns.Add("Modified date");
            table.Columns.Add("Item status");
            table.Columns.Add("Strength");
            table.Columns.Add("Item #");
            table.Columns.Add("Benefit type");
            table.Columns.Add("Script ID");
            table.Columns.Add("Prescribing Dr.");
            table.Columns.Add("Auth item");
            table.Columns.Add("ICD-10");
            table.Columns.Add("Code type");
            table.Columns.Add("Prophylaxis");
            table.Columns.Add("Line note");
            table.Columns.Add("Created by");
            table.Columns.Add("Active");
            table.Columns.Add("CL item #");
            table.Columns.Add("Program");

            foreach (var line in model.items)
            {
                DataRow row = table.NewRow();
                row["Effective date"] = line.effectiveDate;
                row["Repeats"] = line.repeats;
                row["Product name"] = line.productName;
                row["NAPPI"] = line.nappiCode;
                row["ATC Code"] = line.atcCode;
                row["MIMS Class description"] = line.therapeuticClass6Desc;
                row["MIMS Class number"] = line.therapeuticClass6;
                row["MMAP Indicator"] = line.mmapIndicator;
                row["MRP Indicator"] = line.mrpIndicator;
                row["Pack unit of measure"] = line.packUOM;
                row["Generic Indicator"] = line.genericIndicator;
                row["Medicine Schedule"] = line.productSchedule;
                row["product metric strength"] = line.strengthUOM;
                row["Authorisation outcome"] = line.directions;
                row["Quantity"] = line.quantity;
                row["From date"] = line.fromDate.ToString("dd-MM-yyyy");
                row["To date"] = line.toDate.Value.ToString("dd-MM-yyyy");
                row["Item repeats"] = line.itemRepeats;
                row["Modified by"] = line.modifiedBy;
                row["Modified date"] = line.modifiedDate;
                if (line.itemStatus.ToLower() == "authorised")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "pending")
                { row["Item status"] = line.itemStatus; }
                else if (line.itemStatus.ToLower() == "rejected")
                { row["Item status"] = line.itemStatus; }
                row["Strength"] = line.strength;
                row["Item #"] = line.itemNo;
                row["Benefit type"] = line.benefitType;
                row["Script ID"] = line.scriptID;
                row["Prescribing Dr."] = line.prescribingDr;
                row["Auth item"] = line.authItem;
                row["ICD-10"] = line.icd10code;
                row["Code type"] = line.icd10Type;
                row["Prophylaxis"] = line.prophylaxis;
                row["Line note"] = line.comment;
                row["Created by"] = line.createdBy;
                row["Active"] = line.active;
                row["CL item #"] = line.CLItemLineNo;
                row["Program"] = line.program;

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
                Response.Headers.Add("content-disposition", "attachment;  filename=medication-history.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion
        }




        public ActionResult AddMedication(Guid DependentID, int script = 0)
        {
            ViewBag.DependantID = DependentID;

            var scriptVM = new ScriptCreationViewModel();
            var scripts = new Scripts();

            scriptVM.script = scripts;
            scriptVM.script.dependentID = DependentID;
            scriptVM.script.effectiveDate = DateTime.Now;
            scriptVM.script.scriptID = script;
            scriptVM.items = _member.GetScriptItemsFull(DependentID);

            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName");
            ViewBag.programType = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");

            return View(scriptVM);
        }

        [HttpPost]
        public ActionResult AddMedication(ScriptCreationViewModel model)
        {
            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName");
            ViewBag.programType = new SelectList(_member.GetPrograms(), "icd10code", "ProgramName");

            model.theScriptItems = new List<ScriptItems>();
            model.scripts = new List<Scripts>();

            //-->>First add the script ID
            int scriptId = 0;
            var newScript = new Scripts();
            newScript.dependentID = new Guid(Request.Query["DependantID"]);
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

            //-->>Then add the script item
            var scriptItem = new ScriptItems();
            scriptItem.scriptID = newScript.scriptID;
            scriptItem.nappiCode = _admin.GetNappiCode(Request.Query["item.productName"].ToString()).nappiCode;
            scriptItem.icd10code = Request.Query["programType"];
            scriptItem.fromDate = model.item.fromDate;
            scriptItem.toDate = model.item.toDate;
            scriptItem.directions = model.item.directions;
            scriptItem.quantity = "0";
            scriptItem.itemRepeats = 0;
            scriptItem.createdBy = User.Identity.Name;
            scriptItem.createdDate = DateTime.Now;
            scriptItem.active = true;
            scriptItem.itemStatus = "Not yet authorised";
            scriptItem.comment = model.item.comment;

            var insert = _member.InsertScriptItem(scriptItem);

            var scriptVM = new ScriptCreationViewModel();
            var scripts = new Scripts();

            scriptVM.script = scripts;
            scriptVM.script.dependentID = newScript.dependentID;
            scriptVM.script.effectiveDate = DateTime.Now;
            scriptVM.items = _member.GetScriptItemsFull(newScript.dependentID);

            return RedirectToAction("AddMedication", new { DependentID = Request.Query["DependentID"], pro = Request.Query["pro"] });
        }

        //-->>HCare-892
        public ActionResult EditMedicationItem(int itemId)
        {
            var model = _member.GetScriptItem(itemId);
            ViewBag.icd10 = _member.GetScriptItem(itemId).icd10code;
            ViewBag.nappi = _member.GetScriptItem(itemId).nappiCode;
            ViewBag.ItemName = _admin.GetProduct(model.nappiCode).productName;
            ViewBag.fromDate = _member.GetScriptItem(itemId).fromDate;
            ViewBag.toDate = _member.GetScriptItem(itemId).toDate;
            ViewBag.comment = _member.GetScriptItem(itemId).comment;

            ViewBag.DependentID = _member.GetScript(model.scriptID).dependentID;
            return View(model);
        }

        [HttpPost]
        public ActionResult EditMedicationItem(ScriptItems model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            var result = _member.UpdateScriptItem(model);

            return new RedirectResult(Url.Action("PatientClinical_PatientHistory", "Member", new { DependentID = _member.GetScript(model.scriptID).dependentID, pro = Request.Query["pro"] }));
        }
        //-->>

        public ActionResult AddMeds(Guid DependentID)
        {
            var model = new ClinicalHistoryViewModel();
            model.medications = new List<MedicationHistory>();
            model.medication = new MedicationHistory();
            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName");
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            model.medications = _member.GetMedicationHistories(DependentID);
            model.medication.dependantId = DependentID;
            model.medication.startDate = DateTime.Now;
            model.medication.endDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddMeds(ClinicalHistoryViewModel model)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            model.medication.createdBy = User.Identity.Name;
            model.medication.active = true;
            model.medication.createdDate = DateTime.Now;
            model.medication.productName = Request.Query["medication.productName"].ToString();
            model.medication.nappiCode = _admin.GetNappiCode(Request.Query["medication.productName"].ToString()).nappiCode;
            model.medication.programType = Request.Query["medication.programType"];

            var result = _member.InsertMedicationHistory(model.medication);
            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName");
            model.medications = _member.GetMedicationHistories(model.medication.dependantId);
            return View(model);
        }

        public ActionResult AddAllergies(Guid DependentID)
        {
            var model = new ClinicalHistoryViewModel();
            model.allergies = new List<Allergies>();
            model.allergy = new Allergies();
            model.allergy.dependantId = DependentID;
            model.allergies = _member.GetAllergies(DependentID);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddAllergies(ClinicalHistoryViewModel model)
        {
            model.allergy.createdBy = User.Identity.Name;
            model.allergy.active = true;
            model.allergy.createdDate = DateTime.Now;

            var result = _member.InsertAllergy(model.allergy);
            model.allergies = _member.GetAllergies(model.allergy.dependantId);
            return View(model);
        }

        public ActionResult Allergy_Create(Guid DependentID) //HCare-771
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new ClinicalHistoryViewModel();
            model.allergies = new List<Allergies>();
            model.allergy = new Allergies();
            model.allergy.dependantId = DependentID;
            model.allergies = _member.GetAllergies(DependentID);
            ViewBag.program = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");

            return View(model);
        }

        [HttpPost]
        public ActionResult Allergy_Create(ClinicalHistoryViewModel model) //HCare-771
        {
            var allergy = Request.Query["allergy.Allergy"];

            if (string.IsNullOrEmpty(allergy.ToString()))
            {
                ViewBag.program = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");
                ViewBag.allergy = "Allergy field is required";
                ModelState.AddModelError("allergy.Allergy", "Allergy required!");

                return View(model);
            }
            else
            {
                model.allergy.createdBy = User.Identity.Name;
                model.allergy.active = true;
                model.allergy.createdDate = DateTime.Now;
                model.allergy.programType = "GEN";

                ViewBag.program = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");

                var result = _member.InsertAllergy(model.allergy);
                model.allergies = _member.GetAllergies(model.allergy.dependantId);

                return new RedirectResult(Url.Action("PatientClinical_PatientHistory", "Member", new { DependentID = model.allergy.dependantId, pro = Request.Query["pro"] }));
            }
        }

        public ActionResult EditAllergies(int id)
        {
            var model = _member.GetAllergyById(id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName", model.programType);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditAllergies(Allergies model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifieddate = DateTime.Now;
            var result = _member.UpdateAllergy(model);
            return new RedirectResult(Url.Action("PatientClinical_PatientHistory", "Member", new { DependentID = model.dependantId, pro = Request.Query["pro"] }));
        }

        public ActionResult DetailsAllergies(int id)
        {
            var model = _member.GetAllergyById(id);
            return View(model);
        }


        public ActionResult EditAssignment(int id)
        {
            var model = _member.GetAssignment(id);
            ViewBag.assignmentType = new SelectList(_member.GetAssignmentTypes(), "assignmentType", "assignmentDescription", model.assignmentType);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditAssignment(Assignments model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.status = "In Progress";
            var result = _member.UpdateAssignment(model);
            return RedirectToAction("patientAssignments", new { DependentID = model.dependentID });
        }

        public ActionResult loadDashboard(Guid DependentID)
        {
            EnrolmentViewModel model = new EnrolmentViewModel();
            model = _member.GetDependentDetails(DependentID, null);
            model.dependent.DependantID = DependentID;
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);
            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID);
            model.plan = _member.getPlanCode(DependentID);
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID);
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                    model.doctorContact = _member.GetDrInformation(doctorh[0].doctorId);
                }
            }

            return PartialView("~/Views/Member/Partials/_Dashboard.cshtml", model);
        }

        public ActionResult ViewDoctorHistory(Guid DependentID)
        {
            var history = _member.GetDrHistory(DependentID);
            return View(history);
        }
        public ActionResult _ViewScript(int id)
        {
            var model = new List<ScriptViewModel>();
            model = _member.GetScriptItems(id);
            return View(model);
        }

        public ActionResult LoadClinicalPartial(Guid DependentID, Guid? pro)
        {
            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, pro);
            model.DependentID = DependentID;
            model.Scripts = new List<Scripts>();
            model.ScriptItems = new List<ScriptViewModel>();
            model.Scripts = _member.GetScripts(DependentID);
            if (model.Scripts.Count != 0)
            {
                int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
                model.ScriptItems = _member.GetScriptItems(scriptno);
            }
            model.claimHistory = new ClaimsHistory();
            model.Pathologies = new List<Pathology>();
            model.Pathologies = _member.GetPathology(DependentID);
            model.rulesBroken = new List<RulesInstructions>();
            model.rulesBroken = _member.GetRulesHistory(DependentID);

            model.summary = new PatientClinicalSummaryViewModel();
            model.summary.questionaire = new ClinicalHistoryQuestionaire();
            model.summary.allergies = new List<Allergies>();
            model.summary.meds = new List<MedicationHistory>();
            model.summary.clinicalexams = new List<Clinical>();
            model.summary.questionaire = _member.getQuestionnaire(DependentID);
            model.summary.allergies = _member.GetAllergies(DependentID);
            model.summary.meds = _member.GetMedicationHistories(DependentID);
            model.summary.clinicalexams = _member.GetClinicalExam(DependentID);

            model.HospitalAuths = new List<HospitalizationAuths>();
            model.HospitalClaims = new List<HospitalClaimView>();

            var memberinfo = _member.GetDependentDetails(DependentID, null);
            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);

            model.scriptReferences = new List<ScriptReferences>();
            model.scriptReferences = _member.GetScriptReference(DependentID);
            return PartialView("~/Views/Member/Partials/_Clinical.cshtml", model);
        }

        [HttpPost]
        public ActionResult InsertAttachment(PatientProfileViewModel model)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            model.attachment.attachmentName = model.attachment.attachmentName.Replace("<", "").Replace(">", "");
            model.attachment.attachmentType = Request.Query["attachmentType"].ToString();
            var file = Request.Form.Files["file"];

            if (file.HasFile())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                string filename = Path.GetFileName(Request.Form.Files["file"].FileName.Replace("<", "").Replace(">", ""));
               
                var filePath = Path.Combine(path, model.attachment.dependentID + "_" + filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream);
                }
                model.attachment.Link = model.attachment.dependentID + "_" + filename;
                model.attachment.createdBy = User.Identity.Name;
                //HCare-1046
                model.attachment.programId = new Guid(Request.Query["pro"]);
                _member.InsertAttachment(model.attachment);
            }

            //HCare-1993
            var attachmenttype = _member.GetAttachmentTypes().Where(x => x.attachmentType == model.attachment.attachmentType).FirstOrDefault();
            if (!string.IsNullOrEmpty(attachmenttype.assignmentItemType))//HCare-1339
            {

                //if (Request.Query["attachmentType"].ToString() != "WLC")
                //{
                var assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = model.attachment.dependentID,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "DOCUM",
                        status = "Open",
                        programId = new Guid(Request.Query["pro"])
                    },

                };
                assignment.assignmentItemType = attachmenttype.assignmentItemType;
                //if (Request.Query["attachmentType"].ToString() == "QUE")
                //{
                //    assignment.assignmentItemType = "CQUE";
                //}
                //else if (model.attachment.attachmentType == "002")
                //{
                //    assignment.assignmentItemType = "DOCUP";
                //}
                //else if (model.attachment.attachmentType == "MS1") //HCare-690
                //{
                //    assignment.assignmentItemType = "DOCUS";
                //}
                //
                //if (model.attachment.attachmentType == "APP") //HCare-620
                //{
                //    if (programCode == "HIVPR")
                //    {
                //        assignment.assignmentItemType = "NEWR";
                //    }
                //    else
                //         if (programCode == "DIABD")
                //    {
                //        assignment.assignmentItemType = "NEWRT";
                //    }
                //
                //    assignment.newAssignment.Instruction = "New Application form added";
                //}
                var assignExists = _member.GetAssignment(model.attachment.dependentID, assignment.assignmentItemType, new Guid(Request.Query["pro"]));
                if (assignExists == null)
                {
                    var result = _member.InsertAssignment(assignment);

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

            return RedirectToAction("patientCommunication", new { DependentID = model.attachment.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult Edit_attachmentType(int id, Guid? pro)
        {
            var model = _member.GetAttachment(id);
            ViewBag.attachmentType = new SelectList(_member.GetAttachmentTypes(), "attachmentType", "typeDescription");
            ViewBag.pro = pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit_attachmentType(Attachments model)
        {
            var programcode = Request.Query["pro"];
            var result = _member.UpdateAttachment(model);

            return RedirectToAction("patientCommunication", new { DependentID = model.dependentID, pro = programcode });
        }

        [HttpPost]
        public ActionResult _Communications(Queries model)
        {
            model.queryType = Request.Query["QueryT"].ToString();
            if (!String.IsNullOrEmpty(Request.Query["pro"]))
                model.programId = new Guid(Request.Query["pro"]);

            model.enquiryBy = Request.Query["EnquiryType"].ToString();
            if (!String.IsNullOrEmpty(Request.Query["Assignments"]))
                model.ReferenceNumber = Request.Query["Assignments"].ToString();

            model.querySubject = Request.Query["radEnquiry"].ToString();
            model.priority = Request.Query["Priority"].ToString();
            model.createdBy = User.Identity.Name;
            _member.InsertQuery(model);
            return RedirectToAction("patientCommunication", new { DependentID = model.dependentID });
        }

        [HttpPost]
        public ActionResult InsertScriptItem(ScriptCreationViewModel model)
        {
            var scriptItem = new ScriptItems()
            {
                scriptID = model.script.scriptID,
                nappiCode = model.item.nappiCode,
                directions = model.item.directions,
                quantity = model.item.quantity,
                fromDate = model.item.fromDate,
                toDate = model.item.toDate,
                itemRepeats = model.item.itemRepeats,
            };

            scriptItem.createdBy = User.Identity.Name;
            _member.InsertScriptItem(scriptItem);

            return RedirectToAction("AddScript", new { DependentID = model.script.dependentID, script = model.script.scriptID });
        }

        [HttpPost]
        public ActionResult InsertSms(ComsViewModel model)
        {
            model.smsMessages.templateID = Convert.ToInt16(Request.Query["titles"]);
            model.smsMessages.programId = new Guid(Request.Query["pro"]); //HCare-1254
            model.smsMessages.status = "Queued";
            model.smsMessages.createdBy = User.Identity.Name;
            model.smsMessages.message = Request.Query["text-message"];
            if (String.IsNullOrEmpty(Request.Query["smsMessages.effectiveDate"])) { model.smsMessages.effectiveDate = DateTime.Now; } else { model.smsMessages.effectiveDate = Convert.ToDateTime(Request.Query["smsMessages.effectiveDate"]); }

            _member.InsertMessage(model.smsMessages);
            return RedirectToAction("patientCommunication", new { DependentID = model.smsMessages.dependantID, pro = Request.Query["pro"] });
        }

        [HttpPost]

        public ActionResult InsertEmail(ComsViewModel model, List<IFormFile> attachment, Guid pro)
        {
            //HCare - 860
            attachment.RemoveAt(attachment.Count - 1);
            model.emailMessages.createdBy = User.Identity.Name;
            model.emailMessages.programId = pro; //HCare-1254
            model.emailMessages.effectivedate.ToString("yyyy-MM-dd");
            _member.InsertEmailMessage(model.emailMessages);

            if (attachment.Count() > 0) // if there us attachements 
                _member.InsertEmailAttachment(model, attachment.Distinct().ToList());


            return RedirectToAction("patientCommunication", new { DependentID = model.emailMessages.dependantID, pro = Request.Query["pro"] });
        }

        public ActionResult SmsHistory(Guid DependentID, Guid? pro)
        {
            var model = _member.GetSmsMessages(DependentID, pro); //1254
            return View(model);
        }

        public ActionResult EmailHistory(Guid DependentID, Guid? pro)
        {
            var model = _member.GetEmails(DependentID, pro);
            return View(model);
        }

        [HttpGet]
        public ActionResult AddClinical(Guid DependentID)
        {
            var model = new Clinical();
            model.dependantID = DependentID;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddClinical(Clinical model)
        {
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.programType = "GEN";
            //HCare-1050
            if (!String.IsNullOrEmpty(Request.Query["height"])) model.height = decimal.Parse(Request.Query["height"], CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(Request.Query["weight"])) model.weight = decimal.Parse(Request.Query["weight"], CultureInfo.InvariantCulture);
            if (Request.Query["clinicalComment"].ToString() != null) { model.clinicalComment = (Request.Query["clinicalComment"]); }
            else { model.clinicalComment = "Follow up"; }
            if (model.height == 0 || model.weight == 0) { model.followUp = true; }
            else { model.followUp = false; }
            if (Request.Query["effectiveDate"].ToString() != null) { model.effectiveDate = Convert.ToDateTime(Request.Query["effectiveDate"]); }
            else { model.effectiveDate = null; }
            if (model.height != 0 || model.height.Equals(null) && model.weight != 0 || model.weight.Equals(null))
            {
                model.bmi = (model.weight / model.height) / model.height;
                var sq = ((model.height * 100) * model.weight) / 3600;
                model.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
            }
            model.active = true;

            var clinicalExam = _member.InsertClinicalExam(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
        }

        public ActionResult EditClinicalExam(int Id)
        {
            var model = _member.GetClinicalExamById(Id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            return View(model);
        }

        [HttpPost]
        public ActionResult EditClinicalExam(Clinical model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            //HCare-1050
            if (!String.IsNullOrEmpty(Request.Query["height"])) model.height = decimal.Parse(Request.Query["height"], CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(Request.Query["weight"])) model.weight = decimal.Parse(Request.Query["weight"], CultureInfo.InvariantCulture);

            if (Request.Query["effectiveDate"].ToString() != null)
                model.effectiveDate = Convert.ToDateTime(Request.Query["effectiveDate"]);
            else
                model.effectiveDate = null;

            if (model.height != 0 && model.weight != 0)
            {
                model.bmi = (model.weight / model.height) / model.height;
                var sq = ((model.height * 100) * model.weight) / 3600;
                model.bodyServiceArea = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(sq)));
            }
            var result = _member.UpdateClinical(model);

            var res = _member.GetEnrolmentStep(model.dependantID);
            if (res != null)
            {
                res.clinicalCaptured = true;
                var update = _member.UpdateEnrolmentStep(res);
            }
            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
        }

        //HCare-607
        public ActionResult DetailsClinicalExam(int Id)
        {
            var model = _member.GetClinicalExamById(Id);
            return View(model);
        }

        public ActionResult EditMedication(int Id)
        {

            var model = _member.GetMedicationHistory(Id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName", model.programType);
            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName", model.nappiCode);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditMedication(MedicationHistory model)
        {
            model.modifiedBy = User.Identity.Name;
            model.nappiCode = Request.Query["Product"].ToString();
            model.productName = _admin.GetProduct(Request.Query["Product"].ToString()).productName;
            var result = _member.UpdateMedicationHistory(model);
            return RedirectToAction("patientClinical", new { DependentID = model.dependantId });
        }
        //HCare-607
        public ActionResult DetailsMedication(int Id)
        {
            var model = _member.GetMedicationHistory(Id);
            ViewBag.Product = new SelectList(_admin.GetProducts(), "nappiCode", "productName", model.nappiCode);
            return View(model);
        }


        public ActionResult EditManagementStatus(int id, Guid pro)
        {
            var model = _member.GetManagementStatusById(id);
            var memberdetail = _member.GetMemberByDependantID(model.dependantId);
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var allowedstatus = _member.GetManagementStatusesByMedicalAid(memberdetail.medicalAidID).Where(x => x.programCode == programcode);
            var statushistory = _member.GetManagementStatusByDependent(model.dependantId, pro).Where(x => x.active == true).ToList(); //HCare-918 (corrected HCare-919)
            var enrolmentStep = allowedstatus.Where(m => statushistory.Any(c => c.managementStatusCode.Equals(m.statusCode))).Select(m => m.enrolmentStage).Max();//HCare-918 (corrected HCare-919)

            ViewBag.managementStatusCode = new SelectList(allowedstatus.Where(x => x.active == true), "statusCode", "statusName", model.managementStatusCode);
            ViewBag.pro = pro;
            ViewBag.StatusHistory = statushistory;

            var medicalaid = _member.GetMedicalAidById(memberdetail.medicalAidID);
            TempData["medicalaid"] = medicalaid.medicalAidCode;

            return View(model);
        }

        [HttpPost]
        public ActionResult EditManagementStatus(PatientManagementStatusHistory model)
        {
            var pro = new Guid(Request.Query["pro"]);
            var msdetail = _member.GetManagementStatusById(model.id);
            var memberdetail = _member.GetMemberByDependantID(model.dependantId);
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            var allowedstatus = _member.GetManagementStatusesByMedicalAid(memberdetail.medicalAidID).Where(x => x.programCode == programcode);
            var statushistory = _member.GetManagementStatusByDependent(model.dependantId, pro).Where(x => x.active == true).ToList();
            //var laststatus = _member.GetManagmentStatusInformation(model.dependantId).Where(x => x.active == true).FirstOrDefault();
            var medicalaid = _member.GetMedicalAidById(memberdetail.medicalAidID);
            var laststatus = _member.GetManagmentStatusInformation(model.dependantId).Where(x => x.active == true).Where(x => x.programCode == programcode).OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            ViewBag.StatusHistory = statushistory;
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            #region enrolment-status-check HCare-991
            if (model.managementStatusCode == "ERD" || model.managementStatusCode == "ERH")
            {
                foreach (var item in statushistory)
                {
                    if ((item.managementStatusCode == "ERD" && item.id != model.id && model.active == true) || (item.managementStatusCode == "ERH" && item.id != model.id && model.active == true)) //HCare-1101
                    {
                        //IF in an OPEN status and SUPERUSER is logged in, show full status list
                        if (User.HasRole("6. Global user") || User.HasRole("5. Super user"))
                        {
                            ViewBag.managementStatusCode = new SelectList(allowedstatus.Where(x => x.active == true), "statusCode", "statusName", msdetail.managementStatusCode);
                            ViewBag.Message = "Enrolment status already exists.";
                            ViewBag.StatusHistory = statushistory;
                            ViewBag.pro = pro;

                            return View(model);
                        }
                        //ELSE if in OPEN status and CONSULTANT is logged in, don't show Pending status list
                        else
                        {
                            if (laststatus.codeStatus.ToLower() == "o")
                            {

                                ViewBag.managementStatusCode = new SelectList(allowedstatus.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "p"), "statusCode", "statusName", msdetail.managementStatusCode); //if current status is open, you cannot change to pending status, ONLY closed
                            }
                            else if (laststatus.codeStatus.ToLower() == "c")
                            {
                                ViewBag.managementStatusCode = new SelectList(allowedstatus.Where(x => x.active == true).Where(x => x.statusType.ToLower().ToString() != "c"), "statusCode", "statusName", msdetail.managementStatusCode); //if current status is closed, you can change to pending and open status but NOT closed
                            }
                            else
                            {
                                ViewBag.managementStatusCode = new SelectList(allowedstatus.Where(x => x.active == true), "statusCode", "statusName", msdetail.managementStatusCode); //if current status is NOT open or closed, you will have access to all statuses in the list
                            }
                            ViewBag.Message = "Enrolment status already exists.";
                            ViewBag.StatusHistory = statushistory;

                            return View(model);
                        }
                    }
                }
            }
            #endregion
            #region closed-status-assignments HCare-748
            if (Configuration.GetSection("EmailSettings")["Citizen"].ToString().ToLower() == "south african")
            {
                if (model.managementStatusCode == "RES" || model.managementStatusCode == "DEA" || model.managementStatusCode == "DEC" || model.managementStatusCode == "DAM")
                {
                    var assignment = new Assignments();
                    var memberAssignments = _member.GetActiveMemberAssignments(model.dependantId);
                    foreach (var item in memberAssignments)
                    {
                        item.assignmentType = item.assignmentType;
                        item.comment = "Assignment closed due to management status";
                        item.Instruction = item.Instruction + " - Assignment closed due to management status";
                        item.status = "Closed";
                        item.Active = false;
                        var results = _member.UpdateAssignment(item);
                    }
                }
            }
            else if (Configuration.GetSection("EmailSettings")["Citizen"].ToString().ToLower() == "namibian")
            {
                if (model.managementStatusCode == "RES" || model.managementStatusCode == "DEA" || model.managementStatusCode == "DEC" || model.managementStatusCode == "DAM" || model.managementStatusCode == "DAO")
                {
                    var assignment = new Assignments();
                    var memberAssignments = _member.GetActiveMemberAssignments(model.dependantId);
                    foreach (var item in memberAssignments)
                    {
                        item.assignmentType = item.assignmentType;
                        item.comment = "Assignment closed due to management status";
                        item.Instruction = item.Instruction + " - Assignment closed due to management status";
                        item.status = "Closed";
                        item.Active = false;
                        var results = _member.UpdateAssignment(item);
                    }
                }
            }
            #endregion
            #region closed-status-start-end-dates HCare-850
            if (laststatus.codeStatus.ToString().ToLower() == "c" && laststatus.id == model.id) //HCare-1101
            {
                if (model.endDate != model.effectiveDate)
                {
                    ViewBag.managementStatusCode = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName", msdetail.managementStatusCode);
                    ViewBag.Message = "Start and End date must be the same for a CLOSED status";

                    TempData["medicalaid"] = medicalaid.medicalAidCode;
                    ViewBag.error = "Start and End date must be the same for a CLOSED status";
                    return RedirectToAction("EditManagementStatus", new { model.id, pro = pro });

                }
            }
            #endregion

            if (model.id == laststatus.id) { model.sentToCl = false; } //hcare-1265
            var result = _member.UpdateMSRecord(model); //hcare-1265
            if (result.Success && model.endDate >= model.effectiveDate)
            {
                return RedirectToAction("ManageStatus", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
            }
            else if (model.endDate != null && model.endDate < model.effectiveDate)//-->> HCare-850
            {
                ViewBag.managementStatusCode = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName", msdetail.managementStatusCode);
                ViewBag.Message = "End date cannot be less than effective date!";
                ViewBag.StatusHistory = statushistory;

                return View(model);
            }
            else if (result.Success)
            {
                return RedirectToAction("ManageStatus", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
            }
            else
            {
                ViewBag.managementStatusCode = new SelectList(_member.GetManagementStatus().Where(x => x.active == true), "statusCode", "statusName", msdetail.managementStatusCode);
                ViewBag.Message = "Managment status is required!";
                ViewBag.StatusHistory = statushistory;

                return View(model);
            }
        }

        public ActionResult ManagementStatusCheck(Guid dependantID, int id)
        {
            var options = _member.GetManagementStatusByInformation(dependantID, id);

            return Json(options);
        }

        //HCare-763
        public ActionResult DetailsManagementStatus(int id)
        {
            var model = _member.GetManagementStatusById(id);
            var det = _member.GetMemberByDependantID(model.dependantId);
            ViewBag.StatusCode = new SelectList(_member.GetManagementStatusesByMedicalAid(det.medicalAidID), "statusCode", "statusName", model.managementStatusCode);
            return View(model);
        }

        public ActionResult GetImage(string image)
        {
            var path = Path.Combine(Server.MapPath("~/uploads"), image);

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = image;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult InProgressEnrol()
        {
            var model = _member.GetInProgressEnrol();
            return View(model);
        }

        public ActionResult EditProgram(string itemId)
        {
            var model = _member.GetProgramHistoryById(Convert.ToInt32(itemId));
            ViewBag.program = new SelectList(_member.GetPrograms().Where(x => x.ProgramName != "General"), "code", "ProgramName", model.programCode);
            ViewBag.icd10code = new SelectList(_member.GetIcd10Codes(), "icd10CodeID", "icd10CodeID", model.icd10Code);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProgram(PatientProgramHistory model)
        {

            if (Request.Query["program"].ToString() != null)
                model.programCode = Request.Query["program"].ToString();
            if (Request.Query["icd10Code"].ToString() != null)
                model.icd10Code = Request.Query["icd10Code"].ToString();

            ServiceResult result = _member.UpdateProgramHistory(model);
            return RedirectToAction("patientDashboard", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
        }


        public ActionResult patientRecord(Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            var enrol = _member.GetEnrolmentStep(DependentID);
            if (enrol == null)
            {
                model.hasSteps = false;
            }
            else
            {
                if (!enrol.hasClinical && !enrol.hasDemographic && !enrol.hasPathology && !enrol.hasScript)
                {
                    model.hasSteps = false;
                }
                else if (!enrol.clinicalCaptured && enrol.hasClinical)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.demographicCaptured && enrol.hasDemographic)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.pathologyCaptured && enrol.hasPathology)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.scriptCaptured && enrol.hasScript)
                {
                    model.hasSteps = true;
                }
                else
                {
                    model.hasSteps = false;
                }
            }

            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID);
            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID);
            model.paypoint = _member.GetPayPoint(DependentID);
            model.doctor = new Doctors();
            var doctorh = _member.GetDrHistory(DependentID);
            if (doctorh != null)
            {
                if (doctorh.Count() != 0)
                {
                    model.doctor = _admin.GetDoctor(doctorh[0].doctorId);
                    model.doctorContact = _member.GetDrInformation(doctorh[0].doctorId);
                }
            }
            return View(model);
        }

        public ActionResult patientDashboard(Guid pro, Guid DependentID)
        {
            var model = _member.GetDependentDetails(DependentID, null);
            var enrol = _member.GetEnrolmentStep(DependentID);
            var program = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();

            if (enrol == null)
            {
                model.hasSteps = false;
            }
            else
            {
                if (!enrol.hasClinical && !enrol.hasDemographic && !enrol.hasPathology && !enrol.hasScript)
                {
                    model.hasSteps = false;
                }
                else if (!enrol.clinicalCaptured && enrol.hasClinical)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.demographicCaptured && enrol.hasDemographic)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.pathologyCaptured && enrol.hasPathology)
                {
                    model.hasSteps = true;
                }
                else if (!enrol.scriptCaptured && enrol.hasScript)
                {
                    model.hasSteps = true;
                }
                else
                {
                    model.hasSteps = false;
                }
            }

            //model.programID = program.programID.ToString();

            model.programs = new List<PatientProgramHistory>();
            model.programs.Add(_member.GetProgramLatest(DependentID, pro));//HCARE-1070

            model.statuss = new List<PatientManagementStatusHistory>();
            model.statuss = _member.GetManagementStatusByDependentId(DependentID, pro).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList(); //HCare-768

            model.paypoint = _member.GetPayPoint(DependentID);
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
            #region HCare-1319
            //model.membershipNoHistories = new List<MembershipNoHistory>();
            //model.membershipNoHistories = _member.GetMembershipNumberHistory(DependentID);
            #endregion

            model.memberImports = new List<MemberImports>();
            model.memberImports = _member.GetMemberImports(DependentID);

            model.UserMemberHistories = new List<UserMemberHistory>();
            model.UserMemberHistories = _admin.GetUserMemberHistory(DependentID, pro).Where(x => x.Active == true).OrderByDescending(x => x.EffectiveDate).ToList(); //HCare-1176

            var user = User.Identity.Name;
            var userInfo = _admin.GetUserByUsername(user);
            ViewBag.UserName = user;

            #region hcare-1374: popup-template
            var scheme = _member.GetPatientPlanByDependant(DependentID);
            var schemeOption = _member.GetMedicalAidPlansList().Where(x => x.PlanCode == scheme.planName || x.PlanName == scheme.planName).FirstOrDefault();

            if (scheme != null && schemeOption != null)
            {
                model.PopUpTemplates = _admin.GetPopUpTemplateByMemberInformation(model.medicalAid.MedicalAidID, schemeOption.PlanName, program.programID);
                model.PopUp = new PopUpTemplateVM();

                if (model.PopUpTemplates.Count != 0)
                {
                    foreach (var pop in model.PopUpTemplates)
                    {
                        var ma = _member.GetMedicalAidById(new Guid(pop.selectedSchemes));
                        if (pop.Type.ToLower() == "long") { model.PopUp.program_pop_type = "true"; model.PopUp.medicalAid = ma.Name; model.PopUp.option = schemeOption.PlanName; model.PopUp.programName = program.ProgramName; model.PopUp.program_pop_title = pop.Title; model.PopUp.program_pop_template = pop.Template; model.PopUp.programName = program.ProgramName.ToUpper(); }
                        if (pop.Type.ToLower() == "short") { model.PopUp.profile_pop_type = "true"; model.PopUp.medicalAid = ma.Name; model.PopUp.option = schemeOption.PlanName; model.PopUp.programName = program.ProgramName; model.PopUp.profile_pop_title = pop.Title; model.PopUp.profile_pop_template = pop.Template; model.PopUp.programName = program.ProgramName.ToUpper(); }
                    }
                }

                var memberrecord = _member.GetMemberRecordByDependantID(DependentID, program.programID, user);
                if (memberrecord != null && memberrecord.CreatedDate.ToString("dd MM yyyy") == DateTime.Today.ToString("dd MM yyyy") && memberrecord.ProgramID == program.programID)
                {
                    model.PopUp.program_pop_checkbox = memberrecord.ProgramPopUp.ToString();
                    model.PopUp.profile_pop_checkbox = memberrecord.ProfilePopUp.ToString();
                }
            }
            #endregion

            return View(model);
        }

        public ActionResult patientAssignments(Guid dependentId, Guid pro)
        {
            //HCare-995 start
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            ViewBag.programId = programCode.code;
            if (programCode.code == "DIABD")
            {
                ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes().Where(x => x.diabetes == true), "assignmentItemType", "itemDescription");
            }
            else if (programCode.code == "HIVPR")
            {
                ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes().Where(x => x.hiv == true), "assignmentItemType", "itemDescription");
            }
            else if (programCode.code == "ONCO")
            {
                ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes().Where(x => x.oncology == true), "assignmentItemType", "itemDescription");
            }
            else if (programCode.code == "MNTLH")
            {
                ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes().Where(x => x.mental == true), "assignmentItemType", "itemDescription");
            }
            else if (programCode.code == "GEN")
            {
                ViewBag.AssignmentItemType = new SelectList(_member.GetAssignmentItemTypes(), "assignmentItemType", "itemDescription");
            }
            //end
            ViewBag.AssignmentType = new SelectList(_member.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
            ViewBag.AssignmentItemType_agent = new SelectList(_member.GetAssignmentItemTypes().Where(x => x.assignmentItemType != "NEWRT").Where(x => x.assignmentItemType != "PREG"), "assignmentItemType", "itemDescription"); //HCare-699
            ViewBag.AssignmentTask = new SelectList(_member.GetAssignmentTaskTypes(), "taskid", "taskDescription");
            ViewBag.AssignmentReq = new SelectList(_member.GetTaskRequirement(), "id", "requirementText");

            var model = new AssignmentsView()
            {
                activeAssignments = _member.GetActivePatientAssignments(dependentId, pro), //19 February 2019 -- HCare-660 (Order by Descending)
                closedAssignments = _member.GetClosedPatientAssignments(dependentId, pro), //01 March 2019 -- HCare-660 (Order by Descending)
                newAssignment = new Assignments() { dependentID = dependentId },
            };

            return View(model);
        }

        public ActionResult patientCommunication(Guid DependentID, Guid pro)
        {
            ViewBag.DependantID = DependentID;

            var model = new ComsViewModel();
            model.queries = new Queries();
            model.queriess = new List<Queries>();
            model.smsMessages = new SmsMessages();
            model.emailMessages = new Emails();
            model.address = new Address();
            model.contacts = new Contact();
            model.notes = new List<Notes>();
            model.assignments = new List<Assignments>();
            model.smsMessageTemplate = new SmsMessageTemplates();
            model.memberInformation = new MemberInformation();
            model.diabeticDairy = new DiabeticDairy(); //HCare-1061
            model.diabeticDairies = new List<DiabeticDairy>();//HCare-1061

            model.MemberInfo = new MemberImports();
            model.MemberInfos = new List<MemberImports>();
            //-->>
            model.attachment = new Attachments();
            model.attachment.dependentID = DependentID;
            model.attachments = _member.GetAttachments(DependentID).Where(x => x.programId == pro || x.programId == null).ToList();//HCare-1046; //HCare-1254
            //-->>
            model.patientDisclaimers = new List<PatientDisclaimer>();
            model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(DependentID);
            //-->>
            model.authLetters = _member.GetAuthorizationLetters(DependentID);

            model.dependant = _member.GetDependantDetails(DependentID, null);
            model.contacts = _member.GetContact(DependentID);
            //model.MemberInfo = _member.GetMemberImportDetails(DependentID);
            model.MemberInfo = _member.GetMemberImports(DependentID).FirstOrDefault();
            //hcare-1233
            if (model.contacts != null)
            {
                if (string.IsNullOrEmpty(model.contacts.cell))
                    model.smsMessages.cell_no = model.contacts.ISeriesCell;
                else
                    model.smsMessages.cell_no = model.contacts.cell;

                if (string.IsNullOrEmpty(model.contacts.email))
                    model.emailMessages.emailTo = model.contacts.ISeriesEmail;
                else
                    model.emailMessages.emailTo = model.contacts.email;
            }
            else
            {
                model.contacts = new Contact();
            }

            model.memberInformation = _member.GetContact_MI(DependentID);
            if (model.memberInformation != null)
            {
                //hcare-1233
                if (string.IsNullOrEmpty(model.contacts.cell))
                    model.smsMessages.cell_no = model.contacts.ISeriesCell;
                else
                    model.smsMessages.cell_no = model.contacts.cell;

                if (string.IsNullOrEmpty(model.contacts.email))
                    model.emailMessages.emailTo = model.contacts.ISeriesEmail;
                else
                    model.emailMessages.emailTo = model.contacts.email;

            }
            else
            {
                model.memberInformation = new MemberInformation();
            }

            model.address = _member.GetAddress(DependentID);
            if (model.address == null)
            {
                model.address = new Address();
            }

            model.queries.dependentID = DependentID;
            model.smsMessages.dependantID = DependentID;
            model.smsMessages.effectiveDate = DateTime.Now;
            model.emailMessages.dependantID = DependentID;
            model.emailMessages.effectivedate = DateTime.Now;
            model.notes = _member.GetNotes(DependentID).Where(x => x.programId == pro || x.programId == null).ToList();//HCare-1046;  //HCare-1254
            model.queriess = _member.GetQueriesByDependant(DependentID).Where(x => x.programId == pro || x.programId == null).ToList();//HCare-1046; //HCare-1254 -
            model.diabeticDairies = _member.GetDiabeticDairyByDependant(DependentID).Where(x => x.programId == pro).ToList();//HCare-1061

            //HCare-1043
            var programcode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            model.programCode = programcode; //HCare-1252
            var det = _member.GetMemberByDependantID(DependentID);
            var templates = _member.GetSmsMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//HCare-1098
            var emailTemplates = _member.GetEmailMessagesByMedicalAid(det.medicalAidID, new Guid(pro.ToString()));//HCare-1098

            //HCare - 1128  for email 
            var emailsubjectView = _member.GetDependentDetails(DependentID, pro);
            ViewBag.MemberNo = emailsubjectView.member.membershipNo;
            ViewBag.Dep = emailsubjectView.dependent.dependentCode;
            ViewBag.Surname = emailsubjectView.dependent.lastName;
            ViewBag.Intial = emailsubjectView.dependent.initials;
            ViewBag.sync = emailsubjectView.medicalAid.sync;

            ViewBag.Assignments = new SelectList(_member.GetActiveSelectAssignments(DependentID), "assignmentID", "AssignmentType");
            ViewBag.EnquiryType = new SelectList(_member.GetQueryTypes(), "queryTypes", "queryDescription");
            ViewBag.Priority = new SelectList(_member.GetPriorities(), "prioritytype", "priorityName");
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            ViewBag.templates = new SelectList(templates, "templateId", "template");

            ViewBag.titles = new SelectList(templates, "templateId", "title");

            ViewBag.emailTemplates = new SelectList(emailTemplates, "templateId", "template");
            var emailTemplate = emailTemplates;
            ViewBag.emailTemplate = new SelectList(emailTemplate, "templateId", "title");
            ViewBag.emailTemplate2 = new SelectList(emailTemplate, "templateId", "title");

            #region HCare-864
            model.DisclaimerFullView = _admin.GetDisclaimerResults(DependentID, pro);
            #endregion

            ViewBag.attachmentType = new SelectList(_member.GetAttachmentTypes(), "attachmentType", "typeDescription");

            #region enquiry-modal
            ViewBag.EnquiryType = new SelectList(_member.GetQueryTypes(), "code", "queryDescription");
            ViewBag.Priority = new SelectList(_member.GetPriorities(), "prioritytype", "priorityName");
            ViewBag.Assignments = new SelectList(_member.GetActiveSelectAssignments(DependentID), "assignmentID", "AssignmentType");
            ViewBag.owner = _admin.GetUsers().Select(x => new SelectListItem() { Text = x.Firstname + " " + x.Lastname, Value = x.userID.ToString() });
            #endregion

            //hCare-1119
            model.CommunicationLog = _member.getCommunicationLog(DependentID, pro).OrderByDescending(x => x.DateSent).ThenBy(n => n.TypeOfCommunication)
            .Distinct().Select(x => new CommunicationLogVM { Detail = x.Detail, description = x.description, DateSent = x.DateSent, TypeOfCommunication = x.TypeOfCommunication, CommunicationSentTo = x.CommunicationSentTo }).ToList();
            ;


            //HCare-1197
            ViewBag.schemeSubject = _medical.GetMedicalAid(det.medicalAidID).medicalAidSettings.schemeSubject;

            model.NextOfKin = _member.GetNextOfKinInformation(DependentID); //hcare-1361
            //ViewBag.ProgramID = pro;

            model.EmailHistory = _member.GetEmails(DependentID, pro);
            model.AttachmentTemplates = _admin.GetAttachmentTemplateByAccount(det.medicalAidID, pro); //HCare-1380
            model.EmailAttachmentHistories = _admin.GetEmailAttachmentByDependantID(DependentID, pro, DateTime.Today); //HCare-1380

            var medicalaid = _member.GetMedicalAidById(det.medicalAidID); //HCare-1380
            var program = _admin.GetProgramById(pro); //HCare-1380
            ViewBag.MedicalAid = medicalaid.Name; //HCare-1380
            ViewBag.MedicalAidID = medicalaid.MedicalAidID; //hcare-1380
            ViewBag.ProgramID = program.programID; //HCare-1380
            ViewBag.Program = program.ProgramName; //HCare-1380
            ViewBag.UserID = User.Identity.Name; //hcare-1380
            ViewBag.MemberName = model.dependant.firstName_UC + " " + model.dependant.lastName_UC; //hcare-1384

            model.SMSHistory = _member.GetSmsMessages(DependentID, pro); //hcare-1378

            //model.EmailHeader = _admin.GetEmailLayoutByAccount(det.medicalAidID, pro).Where(x => x.LayoutType.ToLower() == "header").ToList(); //hcare-1384
            //model.EmailFooter = _admin.GetEmailLayoutByAccount(det.medicalAidID, pro).Where(x => x.LayoutType.ToLower() == "footer").ToList(); //hcare-1384

            //if (model.EmailHeader.Count != 0) { ViewBag.Header = model.EmailHeader[0].Id; } else { ViewBag.Header = null; } //hcare-1384
            //if (model.EmailFooter.Count != 0) { ViewBag.Footer = model.EmailFooter[0].Id; } else { ViewBag.Footer = null; } //hcare-1384

            var doctorinformation = _member.GetPatientDoctorHistory(DependentID); //hcare-1391
            if (doctorinformation != null)
            {
                var doctor = _admin.GetDoctorByDoctorID(doctorinformation.doctorId); //hcare-1391
                if (doctor != null)
                {
                    ViewBag.DoctorEmail = doctor.EmailAddress; //hcare-1391
                    ViewBag.DoctorName = doctor.DoctorName; //hcare-1391
                    ViewBag.DoctorPractice = doctor.PracticeNumber; //hcare-1391
                }
            }

            return View(model);

        }


        //public ActionResult patientCommunication(Guid DependentID, Guid pro) //before 10/07/2020
        //{
        //    var model = new ComsViewModel();
        //    model.queries = new Queries();
        //    model.queriess = new List<Queries>();
        //    model.smsMessages = new SmsMessages();
        //    model.emailMessages = new Emails();
        //    model.address = new Address();
        //    model.contacts = new Contact();
        //    model.notes = new List<Notes>();
        //    model.assignments = new List<Assignments>();
        //    model.smsMessageTemplate = new SmsMessageTemplates();
        //    model.memberInformation = new MemberInformation();
        //    model.MemberInfo = new MemberImports();
        //    model.MemberInfos = new List<MemberImports>();

        //    model.dependant = _member.GetDependantDetails(DependentID);
        //    model.contacts = _member.GetContact(DependentID);
        //    //model.MemberInfo = _member.GetMemberImportDetails(DependentID);
        //    model.MemberInfo = _member.GetMemberImports(DependentID).FirstOrDefault();
        //    if (model.contacts != null)
        //    {
        //        model.smsMessages.cell_no = model.contacts.cell;
        //        model.emailMessages.emailTo = model.contacts.email;
        //    }
        //    else
        //    {
        //        model.contacts = new Contact();
        //    }

        //    model.memberInformation = _member.GetContact_MI(DependentID);
        //    if (model.memberInformation != null)
        //    {
        //        model.smsMessages.cell_no = model.memberInformation.Contacts.cell;
        //        model.emailMessages.emailTo = model.memberInformation.Contacts.email;
        //    }
        //    else
        //    {
        //        model.memberInformation = new MemberInformation();
        //    }

        //    model.address = _member.GetAddress(DependentID);
        //    if (model.address == null)
        //    {
        //        model.address = new Address();
        //    }

        //    model.queries.dependentID = DependentID;
        //    model.smsMessages.dependantID = DependentID;
        //    model.smsMessages.effectiveDate = DateTime.Now;
        //    model.emailMessages.dependantID = DependentID;
        //    model.emailMessages.effectivedate = DateTime.Now;
        //    model.notes = _member.GetNotes(DependentID);
        //    model.queriess = _member.GetQueriesByDependant(DependentID);

        //    ViewBag.Assignments = new SelectList(_member.GetActiveSelectAssignments(DependentID), "assignmentID", "AssignmentType");
        //    ViewBag.EnquiryType = new SelectList(_member.GetQueryTypes(), "queryTypes", "queryDescription");
        //    ViewBag.Priority = new SelectList(_member.GetPriorities(), "prioritytype", "priorityName");
        //    var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
        //    var templates = _member.GetTemplates().Where(x => x.program == programCode);
        //    ViewBag.templates = new SelectList(templates, "templateId", "template");

        //    var smsTemplates = _member.GetTemplates().Where(x => x.program == programCode);
        //    ViewBag.titles = new SelectList(smsTemplates, "templateId", "title");

        //    var emailTemplates = _member.GetEmailTemplates();
        //    ViewBag.emailTemplates = new SelectList(emailTemplates, "templateId", "template");
        //    var emailTemplate = _member.GetEmailTemplates();
        //    ViewBag.emailTemplate = new SelectList(emailTemplate, "templateId", "title");

        //    return View(model);

        //}

        public ActionResult AddComorbid(Guid DependentID, Guid pro)
        {
            var model = new CoMormidConditions();
            model.dependantID = DependentID;

            ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(DependentID), "condition", "condition");

            return View(model);
        }

        [HttpPost]
        public ActionResult AddComorbid(CoMormidConditions model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.coMorbidId = Convert.ToInt32(Request.Query["icd10"]);
            model.programType = "GEN";
            model.active = true;

            var comorbidHistory = _member.GetComorbidInformation(model.dependantID).ToList();
            var comorbid = _member.GetCoMorbidExclusionsByID(model.coMorbidId);

            #region hcare-894 date-validation
            // end-date cannot precede start-date
            if (model.treatementEndDate < model.effectiveDate)
            {
                ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(model.dependantID), "condition", "condition", model.coMorbidId);
                ModelState.AddModelError("treatementEndDate", "Treatment end date cannot precede Effective date.");

                return View(model);
            }

            foreach (var item in comorbidHistory)
            {
                bool startDateOverlap = model.effectiveDate >= item.startDate && model.effectiveDate <= item.endDate;
                bool startEndDateOverlap = item.startDate <= model.treatementEndDate && model.effectiveDate <= item.endDate;

                if (!String.IsNullOrEmpty(item.startDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
                {
                    if (comorbid.mappingDescription == item.Condition || comorbid.ICD10Code == item.icd10)
                    {
                        if (!String.IsNullOrEmpty(model.effectiveDate.ToString()) && !String.IsNullOrEmpty(model.treatementEndDate.ToString()))
                        {
                            if (startEndDateOverlap == true)
                            {
                                ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(model.dependantID), "condition", "condition", model.coMorbidId);
                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

                                return View(model);
                            }
                        }
                        else if (!String.IsNullOrEmpty(model.effectiveDate.ToString()) && String.IsNullOrEmpty(model.treatementEndDate.ToString()))
                        {
                            if (startDateOverlap == true)
                            {
                                ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(model.dependantID), "condition", "condition", model.coMorbidId);
                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

                                return View(model);
                            }
                        }
                    }
                }
            }
            #endregion

            _member.InsertComorbidCondition(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
        }

        //public ActionResult EditComorbid(int id) //before 10/07/2020
        //{
        //    var viewModel = new coMorbidViewModel();
        //    viewModel.coMormidCondition = _member.GetCoMorbidByID(id);
        //    viewModel.coMorbidConditionVM = _member.GetCoMorbids_ExcludedByID(id);

        //    ViewBag.coMorbids = new SelectList(_member.GetCoMorbids().Where(x => x.active == true), "id", "icd10Condition", viewModel.coMormidCondition.coMorbidId);
        //    //ViewBag.comorb = new SelectList(_member.GetCoMorbids_Excluded(viewModel.coMormidCondition.dependantID), "coMorbidId", "icd10Condition", viewModel.coMorbidConditionVM.coMorbidId);
        //    //ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(viewModel.coMormidCondition.dependantID), "coMorbidId", "icd10Condition", viewModel.coMorbidConditionVM.coMorbidId);
        //    ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(viewModel.coMormidCondition.dependantID), "condition", "condition", viewModel.coMorbidConditionVM.Condition);
        //    ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(viewModel.coMorbidConditionVM.Condition), "id", "icd10", viewModel.coMorbidConditionVM.icd10);

        //    ViewBag.CoMorbidTreatement = new SelectList(_member.GetCoMorbid(), "id", "CoMorbidTreatement");
        //    ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

        //    return View(viewModel);

        //}

        public ActionResult EditComorbid(int id)
        {
            var viewModel = new coMorbidViewModel();
            viewModel.coMormidCondition = _member.GetCoMorbidByID(id);
            //HCare-1293
            //if (viewModel.coMormidCondition.createdDate > Convert.ToDateTime("2020-09-20"))
            //{
            if (viewModel.coMormidCondition.coMorbidId.ToString() == "99998")
            {
                viewModel.coMorbidConditionVM = _member.GetCoMorbids_ExcludedByID(id);
                ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(viewModel.coMormidCondition.dependantID), "condition", "condition", viewModel.coMorbidConditionVM.Condition);
                ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(viewModel.coMorbidConditionVM.Condition, viewModel.coMorbidConditionVM.dependantID), "id", "icd10", viewModel.coMorbidConditionVM.icd10);
            }
            else
            {
                viewModel.coMorbidConditionVM = _member.GetNewCoMorbids_ExcludedByID(id); //HCare-859
                ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(viewModel.coMormidCondition.dependantID), "condition", "condition", viewModel.coMorbidConditionVM.Condition); //HCare-859
                ViewBag.icd10 = new SelectList(_member.GetNewCoMorbidsICD10NotLinkedToDependant(viewModel.coMorbidConditionVM.Condition, viewModel.coMorbidConditionVM.dependantID), "id", "icd10", viewModel.coMorbidConditionVM.icd10);
            }
            //}
            //else
            //{
            //    viewModel.coMorbidConditionVM = _member.GetCoMorbids_ExcludedByID(id);
            //    ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(viewModel.coMormidCondition.dependantID), "condition", "condition", viewModel.coMorbidConditionVM.Condition);
            //    ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(viewModel.coMorbidConditionVM.Condition, viewModel.coMorbidConditionVM.dependantID), "id", "icd10", viewModel.coMorbidConditionVM.icd10);
            //}

            ViewBag.CoMorbidID = viewModel.coMorbidConditionVM.CMConditionId;

            //ViewBag.CoMorbidTreatement = new SelectList(_member.GetCoMorbid(), "id", "CoMorbidTreatement"); //hidden 03/02/2021
            //ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName"); //hidden 03/02/2021

            return View(viewModel);

        }

        [HttpPost]
        public ActionResult EditComorbid(coMorbidViewModel model)
        {
            var comorbidHistory = _member.GetComorbidInformation(model.coMormidCondition.dependantID).ToList();
            model.coMorbidConditionVM = _member.GetCoMorbids_ExcludedByID(model.coMormidCondition.id);

            model.coMormidCondition.modifiedDate = DateTime.Now;
            model.coMormidCondition.modifiedBy = User.Identity.Name;

            #region CoMorbidID
            if (model.coMormidCondition.coMorbidId.ToString() == "99998")
            {
                model.coMormidCondition.coMorbidId = Convert.ToInt32(Request.Query["icd10"]);
            }
            if (!String.IsNullOrEmpty(Request.Query["icd10"]))
            {
                model.coMormidCondition.coMorbidId = Convert.ToInt32(Request.Query["icd10"]);
            }
            else if (!String.IsNullOrEmpty(Request.Query["comorbidID"]))
            {
                model.coMormidCondition.coMorbidId = Convert.ToInt32(Request.Query["comorbidID"]);
            }
            #endregion
            #region hcare-894 date validation
            //HCare-1293
            //if (model.coMormidCondition.createdDate > Convert.ToDateTime("2020-09-20"))
            //{
            var cmExclusion = _member.GetCoMorbidExclusionsByID(model.coMormidCondition.coMorbidId);
            #region end-date vs start-date
            if (model.coMormidCondition.treatementEndDate < model.coMormidCondition.effectiveDate)
            {
                ViewBag.comorb = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMorbidConditionVM.Condition); //HCare-859
                ViewBag.icd10 = new SelectList(_member.GetNewCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMorbidConditionVM.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
                ModelState.AddModelError("coMormidCondition.treatementEndDate", "Treatment end date cannot precede Effective date.");

                return View(model);
            }
            #endregion
            #region overlapping-date-validation
            foreach (var item in comorbidHistory.Where(x => x.id != model.coMormidCondition.id))
            {
                bool xOverlap = model.coMormidCondition.effectiveDate >= item.startDate && model.coMormidCondition.effectiveDate <= item.endDate;
                bool yOverlap = item.startDate >= model.coMormidCondition.effectiveDate && item.startDate <= model.coMormidCondition.treatementEndDate;
                bool xyOverlap = item.startDate <= model.coMormidCondition.treatementEndDate && model.coMormidCondition.effectiveDate <= item.endDate;

                if (item.id != model.coMormidCondition.id)
                {
                    if (cmExclusion.mappingDescription == item.Condition || cmExclusion.ICD10Code == item.icd10)
                    {
                        if (!String.IsNullOrEmpty(item.startDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
                        {
                            if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) && !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
                            {
                                if (xyOverlap == true && item.id != model.coMormidCondition.id)
                                {
                                    ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
                                    ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

                                    return View(model);
                                }
                            }
                            else if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) || !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
                            {
                                if (xOverlap == true)
                                {
                                    ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
                                    ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

                                    return View(model);
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(item.startDate.ToString()) && String.IsNullOrEmpty(item.endDate.ToString()))
                        {
                            if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) && !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
                            {
                                if (yOverlap == true && item.id != model.coMormidCondition.id)
                                {
                                    ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
                                    ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

                                    return View(model);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //}
            //else
            //{
            //    var cmType = _member.GetCoMorbidTypeByID(model.coMormidCondition.coMorbidId);
            //    #region end-date vs start-date
            //    if (model.coMormidCondition.treatementEndDate < model.coMormidCondition.effectiveDate)
            //    {
            //        ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMorbidConditionVM.Condition);
            //        ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMorbidConditionVM.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
            //        ModelState.AddModelError("coMormidCondition.treatementEndDate", "Treatment end date cannot precede Effective date.");
            //
            //        return View(model);
            //    }
            //    #endregion
            //    #region overlapping-date-validation
            //    foreach (var item in comorbidHistory.Where(x => x.id != model.coMormidCondition.id))
            //    {
            //        bool xOverlap = model.coMormidCondition.effectiveDate >= item.startDate && model.coMormidCondition.effectiveDate <= item.endDate;
            //        bool yOverlap = item.startDate >= model.coMormidCondition.effectiveDate && item.startDate <= model.coMormidCondition.treatementEndDate;
            //        bool xyOverlap = item.startDate <= model.coMormidCondition.treatementEndDate && model.coMormidCondition.effectiveDate <= item.endDate;
            //
            //        if (item.id != model.coMormidCondition.id)
            //        {
            //            if (cmType.condition == item.Condition || cmType.icd10 == item.icd10)
            //            {
            //                if (!String.IsNullOrEmpty(item.startDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
            //                {
            //                    if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) && !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
            //                    {
            //                        if (xyOverlap == true && item.id != model.coMormidCondition.id)
            //                        {
            //                            ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
            //                            ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
            //                            ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";
            //
            //                            return View(model);
            //                        }
            //                    }
            //                    else if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) || !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
            //                    {
            //                        if (xOverlap == true)
            //                        {
            //                            ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
            //                            ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
            //                            ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";
            //
            //                            return View(model);
            //                        }
            //                    }
            //                }
            //                else if (!String.IsNullOrEmpty(item.startDate.ToString()) && String.IsNullOrEmpty(item.endDate.ToString()))
            //                {
            //                    if (!String.IsNullOrEmpty(model.coMormidCondition.effectiveDate.ToString()) && !String.IsNullOrEmpty(model.coMormidCondition.treatementEndDate.ToString()))
            //                    {
            //                        if (yOverlap == true && item.id != model.coMormidCondition.id)
            //                        {
            //                            ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
            //                            ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);
            //                            ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";
            //
            //                            return View(model);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    #endregion
            //}
            #endregion

            var result = _member.UpdateCoMorbidCondition(model.coMormidCondition);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.coMormidCondition.dependantID, pro = Request.Query["pro"] });

        }

        //[HttpPost] -- before 07/10/2020
        //public ActionResult EditComorbid(coMorbidViewModel model)
        //{
        //    ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

        //    var comorbidHistory = _member.GetComorbidInformation(model.coMormidCondition.dependantID).ToList();
        //    model.coMorbidConditionVM = _member.GetCoMorbids_ExcludedByID(model.coMormidCondition.id);

        //    var oldICD = Request.Query["icd"];
        //    var newICD = Request.Query["icd10"];
        //    try
        //    {
        //        model.coMormidCondition.modifiedDate = DateTime.Now;
        //        model.coMormidCondition.modifiedBy = User.Identity.Name;

        //        if (model.coMormidCondition.coMorbidId.ToString() == "99998")
        //        {
        //            model.coMormidCondition.coMorbidId = Convert.ToInt32(newICD);
        //        }

        //        if (newICD == "")
        //        {
        //            model.coMormidCondition.coMorbidId = Convert.ToInt32(oldICD);
        //        }
        //        else
        //        {
        //            model.coMormidCondition.coMorbidId = Convert.ToInt32(newICD);
        //        }

        //        #region hcare-894 date validation
        //        var comorbid = _member.GetCoMorbidTypeByID(model.coMormidCondition.coMorbidId);

        //        // end-date cannot precede start-date
        //        if (model.coMormidCondition.treatementEndDate < model.coMormidCondition.effectiveDate)
        //        {
        //            ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMorbidConditionVM.Condition);
        //            ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);

        //            ModelState.AddModelError("coMormidCondition.treatementEndDate", "Treatment end date cannot precede Effective date.");

        //            return View(model);
        //        }

        //        foreach (var item in comorbidHistory)
        //        {
        //            if (!String.IsNullOrEmpty(item.startDate.ToString()) && !String.IsNullOrEmpty(item.endDate.ToString()))
        //            {
        //                if (comorbid.condition == item.Condition || comorbid.icd10 == item.icd10)
        //                {
        //                    if (comorbid.id == item.id)
        //                    {
        //                        if (model.coMormidCondition.effectiveDate != item.startDate && model.coMormidCondition.treatementEndDate != item.endDate)
        //                        {
        //                            if ((model.coMormidCondition.effectiveDate <= item.endDate || model.coMormidCondition.effectiveDate <= item.startDate))
        //                            {
        //                                ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
        //                                ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);

        //                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

        //                                return View(model);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (model.coMormidCondition.effectiveDate != item.startDate && model.coMormidCondition.treatementEndDate != item.endDate)
        //                        {
        //                            if (model.coMormidCondition.effectiveDate <= item.endDate || model.coMormidCondition.effectiveDate <= item.startDate)
        //                            {
        //                                ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMormidCondition.coMorbidId);
        //                                ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition, model.coMormidCondition.dependantID), "id", "icd10", model.coMorbidConditionVM.icd10);

        //                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's comorbid history.";

        //                                return View(model);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        #endregion

        //        //#region hcare-894 date validation
        //        //foreach (var comorbid in comorbidHistory)
        //        //{
        //        //    if (!String.IsNullOrEmpty(comorbid.startDate.ToString()) && !String.IsNullOrEmpty(comorbid.endDate.ToString()))
        //        //    {
        //        //        if (model.coMormidCondition.coMorbidId == comorbid.coMorbidId)
        //        //        {
        //        //            if (model.coMormidCondition.effectiveDate <= comorbid.endDate || model.coMormidCondition.effectiveDate <= comorbid.startDate)
        //        //            {
        //        //                ViewBag.comorb = new SelectList(_member.GetCoMorbidsNotLinkedToDependant(model.coMormidCondition.dependantID), "condition", "condition", model.coMorbidConditionVM.Condition);
        //        //                ViewBag.icd10 = new SelectList(_member.GetCoMorbidsICD10NotLinkedToDependant(model.coMorbidConditionVM.Condition), "id", "icd10", model.coMorbidConditionVM.icd10);

        //        //                ViewBag.Message = "Date range has been used in Start End date collection, please review patient's comorbid history.";

        //        //                return View(model);
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        //#endregion

        //        var result = _member.UpdateCoMorbidCondition(model.coMormidCondition);

        //        return RedirectToAction("patientClinical", new { DependentID = model.coMormidCondition.dependantID, pro = Request.Query["pro"] });
        //    }
        //    catch
        //    {
        //        return View();
        //    }

        //}

        //[HttpPost]
        //public ActionResult EditComorbid(coMorbidViewModel model)
        //{
        //    model.coMormidCondition.modifiedBy = User.Identity.Name;
        //    model.coMormidCondition.modifiedDate = DateTime.Now;
        //    model.coMormidCondition.coMorbidId = Convert.ToInt32(Request.Query["icd10"]);

        //    var result = _member.UpdateCoMorbidCondition(model.coMormidCondition);

        //    return RedirectToAction("patientClinical", new { DependentID = model.coMormidCondition.dependantID, pro = Request.Query["pro"] });

        //    //try
        //    //{
        //    //    ViewBag.coMorbids = new SelectList(_member.GetCoMorbids().Where(x => x.active == true), "id", "icd10Condition", model.coMormidCondition.coMorbidId);
        //    //    ViewBag.comorb = new SelectList(_member.GetCoMorbids_Excluded(model.coMormidCondition.dependantID), "coMorbidId", "icd10Condition", model.coMormidCondition.coMorbidId);

        //    //    model.coMormidCondition.modifiedDate = DateTime.Now;
        //    //    model.coMormidCondition.modifiedBy = User.Identity.Name;

        //    //    if (model.coMormidCondition.coMorbidId.ToString() == "99998")
        //    //    {
        //    //        model.coMormidCondition.coMorbidId = Convert.ToInt32(Request.Query["comorb"]);
        //    //    }


        //    //    var result = _member.UpdateCoMorbidCondition(model.coMormidCondition);

        //    //    return RedirectToAction("patientClinical", new { DependentID = model.coMormidCondition.dependantID, pro = Request.Query["pro"] });
        //    //}
        //    //catch
        //    //{

        //    //    return View();
        //    //}

        //}

        public ActionResult FillICD(string condition, Guid dependantID)
        {
            var cc = _admin.GetComorbidInfoByName(condition, dependantID);

            var options = new List<CoMorbidConditionVM>();
            options = _member.GetCoMorbidsICD10NotLinkedToDependant(condition, dependantID);

            //var option = viewModel.coMormidCondition.createdDate > Convert.ToDateTime("2020-09-20") ? options = _member.GetNewCoMorbidsICD10NotLinkedToDependant(condition, dependantID) : options = _member.GetCoMorbidsICD10NotLinkedToDependant(condition, dependantID);



            return Json(options);
        }

        public ActionResult FillICD10_Add(string condition, Guid dependantID)
        {
            var options = _member.GetNewCoMorbidsICD10NotLinkedToDependant(condition, dependantID);

            return Json(options);
        }

        public ActionResult FillICD10_MH_Diagnosis(string condition, Guid dependantID)
        {
            var options = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(condition, dependantID);
            return Json(options);
        }

        //HCare-607
        public ActionResult DetailsComorbid(int id)
        {
            var viewModel = new coMorbidViewModel();
            viewModel.coMormidCondition = _member.GetCoMorbidByID(id);
            viewModel.coMorbidType = _member.GetCoMorbidTypeByID(id);
            viewModel.ComorbidConditionExclusion = _admin.GetComorbidExclusion(viewModel.coMormidCondition.coMorbidId); //HCare-859
                                                                                                                        //HCare-1293
            ViewBag.coMorbids = new SelectList(_admin.GetICD10Conditions(), "id", "ICD10Condition", viewModel.coMormidCondition.coMorbidId);

            return View(viewModel);
        }

        //[HttpGet]
        //public ActionResult patientClinical(Guid DependentID, string sortOrder, Guid? pro) //hidden on 26/04/2021
        //{
        //    var programCode = string.Empty;
        //    if (pro != null)
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
        //    }
        //    else
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
        //    }


        //    TempData["dependantID"] = DependentID;
        //    TempData["program"] = pro;

        //    var dependantInfo = _member.GetDependantByDependantID(DependentID);
        //    var memberInfo = _member.GetMemberByDependantID(DependentID);

        //    ClinicalViewModel model = new ClinicalViewModel();
        //    model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
        //    model.DependentID = DependentID;
        //    model.Scripts = new List<Scripts>();
        //    model.ScriptItems = new List<ScriptViewModel>();
        //    model.Scripts = _member.GetScripts(DependentID);
        //    if (model.Scripts.Count != 0)
        //    {
        //        int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
        //        model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //        model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
        //        model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
        //        model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
        //        model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //    }
        //    model.claimHistory = new ClaimsHistory();
        //    //-->>            
        //    model.Pathologies = new List<Pathology>();
        //    model.Pathologies = _member.GetPathology(DependentID);
        //    model.hivBloodResults = new List<Pathology>();
        //    model.hivBloodResults = _member.GetPathology(DependentID);
        //    //-->> HCare-851
        //    model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
        //    model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
        //    model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
        //    model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
        //    //-->>
        //    #region clinical-rules
        //    var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

        //    var male = false;
        //    var female = false;
        //    var rules = new List<ClinicalRules>();

        //    if (dependantInfo.sex.ToUpper().Contains("F"))
        //    {
        //        female = true;
        //        rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
        //    }
        //    else
        //    {
        //        male = true;
        //        rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
        //    }
        //    #endregion
        //    model.clinicalRules = rules;
        //    model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
        //    //-->>
        //    model.rulesBroken = new List<RulesInstructions>();
        //    model.rulesBroken = _member.GetRulesHistory(DependentID).Where(x => x.program == programCode).ToList();
        //    //-->>
        //    model.BrokenRules = _member.GetClinicalRulesHistory(DependentID).Where(x => x.ProgramCode == programCode).ToList();
        //    //-->>
        //    model.summary = new PatientClinicalSummaryViewModel();
        //    //-->>
        //    model.summary.clinicalexams = new List<Clinical>();
        //    model.summary.clinicalexams = _member.GetClinicalExam(DependentID);
        //    model.summary.generalClinicalExams = new List<Clinical>();
        //    model.summary.generalClinicalExams = _member.GetGeneralClinicalExam(DependentID);
        //    model.summary.diabetesClinicalExams = new List<Clinical>();
        //    model.summary.diabetesClinicalExams = _member.GetDiabetesClinicalExam(DependentID);
        //    model.summary.hivClinicalExams = new List<Clinical>();
        //    model.summary.hivClinicalExams = _member.GetHivClinicalExam(DependentID);
        //    //-->>
        //    model.CoMorbids = new List<ComorbidView>();
        //    model.CoMorbids = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));
        //    model.generalCoMorbids = new List<ComorbidView>();
        //    model.generalCoMorbids = _member.GetGeneralComorbidItems(DependentID, new Guid(Request.Query["pro"]));
        //    //-->>
        //    model.summary.meds = new List<MedicationHistory>();
        //    model.summary.meds = _member.GetMedicationHistories(DependentID);
        //    model.summary.generalMedications = new List<MedicationHistory>();
        //    model.summary.generalMedications = _member.GetGeneralMedicationHistories(DependentID);
        //    model.summary.hivMedications = new List<MedicationHistory>();
        //    model.summary.hivMedications = _member.GetHIVMedicationHistories(DependentID);
        //    //-->>
        //    model.summary.allergies = new List<Allergies>();
        //    model.summary.allergies = _member.GetAllergies(DependentID);
        //    model.summary.generalAllergies = new List<Allergies>();
        //    model.summary.generalAllergies = _member.GetGeneralAllergies(DependentID);
        //    //-->>
        //    model.summary.questionaire = new ClinicalHistoryQuestionaire();
        //    model.summary.questionaire = _member.getQuestionnaire(DependentID);
        //    model.summary.questionaires = _member.GetSocialHistory(DependentID);
        //    model.summary.generalQuestionaires = _member.GetGeneralSocialHistory(DependentID);
        //    //-->>
        //    var memberinfo = _member.GetDependentDetails(DependentID, null);
        //    model.HospitalClaims = new List<HospitalClaimView>();
        //    model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    model.HospitalAuths = new List<HospitalizationAuths>();
        //    model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    model.generalHospitalAuths = new List<HospitalizationAuths>();
        //    model.generalHospitalAuths = _member.GetGeneralHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    //-->>
        //    model.summary.questionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);
        //    model.summary.generalQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.generalQuestionnaireOthers = _member.GetGeneralQuestionnaireOtherResults(DependentID);
        //    model.summary.diabetesQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.diabetesQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);
        //    model.summary.hivQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);
        //    //-->>
        //    model.summary.Other = new List<QuestionnaireOther>(); //HCare-968
        //    model.summary.Other = _member.GetOtherResults(DependentID); //HCare-968
        //    model.summary.Pregnancy = new List<QuestionnaireOther>(); //HCare-968
        //    model.summary.Pregnancy = _member.GetPregnancyResults(DependentID); //HCare-968
        //                                                                        //-->>
        //    model.summary.NewBorns = new List<NewBorn>(); //HCare-968
        //    model.summary.NewBorns = _member.GetNewBornResults(DependentID); //HCare-968
        //                                                                     //-->>
        //    model.summary.programDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.programDiagnoses = _member.GetDiagnosisResults(DependentID);
        //    model.summary.diabetesProgramDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.diabetesProgramDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
        //    model.summary.hivProgramDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.hivProgramDiagnoses = _member.GetHIVDiagnosisResults(DependentID);
        //    //-->>
        //    model.summary.visions = new List<Vision>();
        //    model.summary.visions = _member.GetVisionResults(DependentID);
        //    //-->>
        //    model.summary.podiatries = new List<Podiatry>();
        //    model.summary.podiatries = _member.GetPodiatryResults(DependentID);
        //    model.summary.podiatry = new Podiatry();
        //    //-->>
        //    model.summary.autoNeropathies = new List<AutoNeropathy>();
        //    model.summary.autoNeropathies = _member.GetAutoNeroResults(DependentID);
        //    //-->>
        //    model.summary.hypoglycaemias = new List<Hypoglycaemia>();
        //    model.summary.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);
        //    //-->>
        //    model.summary.otherMedicalHistories = new List<OtherMedicalHistory>();
        //    model.summary.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);
        //    //-->>
        //    model.summary.doctorQuestionnaireResults = new List<DoctorQuestionnaireResults>();
        //    model.summary.doctorQuestionnaireResults = _member.GetDrQuesResults(DependentID);
        //    //-->>
        //    #region mental-health
        //    //-->>
        //    model.summary.MH_DSM5Responses = new List<MH_DSM5ResponseNEW>(); //HCare-1123
        //    model.summary.MH_DSM5Responses = _admin.GetNEWDSM5Results(DependentID); //HCare-1123

        //    model.summary.MH_SchizophreniaResponses = new List<MH_SchizophreniaResponse>(); //HCare-1124
        //    model.summary.MH_SchizophreniaResponses = _admin.GetSchizophreniaResults(DependentID); //HCare-1124

        //    model.summary.MH_BipolarResponses = new List<MH_BipolarResponse>(); //HCare-1125
        //    model.summary.MH_BipolarResponses = _admin.GetBipolarResults(DependentID); //HCare-1125

        //    model.summary.MH_DepressionResponses = new List<MH_DepressionResponse>(); //HCare-1126
        //    model.summary.MH_DepressionResponses = _admin.GetDepressionResults(DependentID); //HCare-1126
        //                                                                                     //-->>
        //    #endregion

        //    model.scriptReferences = new List<ScriptReferences>();
        //    model.scriptReferences = _member.GetScriptReference(DependentID);
        //    //-->>
        //    model.programs = new List<PatientProgramHistory>();
        //    model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
        //    model.programcode = programCode;
        //    //-->>
        //    model.gender = dependantInfo.sex;

        //    ViewBag.effectiveDateAsc = sortOrder == "effectiveDate" ? "effectiveDate_asc" : "effectiveDate";

        //    switch (sortOrder)
        //    {
        //        case "effectiveDate":
        //            if (sortOrder.Equals(ViewBag))
        //                model.Pathologies = _member.GetPathology(DependentID).OrderBy(c => c.effectiveDate).ToList();
        //            else
        //                model.Pathologies = _member.GetPathology(DependentID).OrderByDescending(c => c.effectiveDate).ToList();
        //            break;
        //    }

        //    model.RiskRating = new List<PatientRiskRatingHistory>();
        //    model.RiskRating = _member.GetPatientRiskRatingMultipleReasons(DependentID, pro);

        //    model.DoctorReferrals = new List<DoctorReferral>();
        //    model.DoctorReferrals = _member.GetDoctorReferralResults(DependentID);

        //    return View(model);
        //}

        public ActionResult PatientClinical_Summary(Guid DependentID, Guid? pro)
        {
            #region program-code
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }
            #endregion

            TempData["dependantID"] = DependentID;
            TempData["program"] = pro;

            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;
            //#region script-items //hcare-1313 - hidden 26/04/2021
            //model.Scripts = new List<Scripts>();
            //model.ScriptItems = new List<ScriptViewModel>();
            //model.Scripts = _member.GetScripts(DependentID);
            //if (model.Scripts.Count != 0)
            //{
            //    int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
            //    model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
            //    model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
            //    model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
            //    model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
            //    model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
            //}
            //model.claimHistory = new ClaimsHistory();
            //#endregion
            #region pathology            
            model.Pathologies = new List<Pathology>();
            model.Pathologies = _member.GetPathology(DependentID);
            model.hivBloodResults = new List<Pathology>();
            model.hivBloodResults = _member.GetPathology(DependentID);
            //-->> HCare-851
            model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
            model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
            model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
            model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
            model.TBPathologies = new List<QuestionnaireOther>();
            model.TBPathologies = _member.GetQuestionnaireOtherResults(DependentID);
            #endregion
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
            #endregion

            model.rulesBroken = new List<RulesInstructions>();
            model.rulesBroken = _member.GetRulesHistory(DependentID).Where(x => x.program == programCode).ToList();

            model.BrokenRules = _member.GetClinicalRulesHistory(DependentID).Where(x => x.ProgramCode == programCode).ToList();

            model.scriptReferences = new List<ScriptReferences>();
            model.scriptReferences = _member.GetScriptReference(DependentID);

            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
            model.programcode = programCode;

            model.RiskRating = new List<PatientRiskRatingHistory>();
            model.RiskRating = _member.GetPatientRiskRatingMultipleReasons(DependentID, pro);

            model.DoctorReferrals = new List<DoctorReferral>();
            model.DoctorReferrals = _member.GetDoctorReferralResults(DependentID);

            model.DSM5Scores = new List<MH_DSM5Score>();
            model.DSM5Scores = _admin.GetDSM5ScoreHistory(DependentID);

            var md = _member.GetPatientStagingHistoryByDependant(DependentID);

            model.patientStagingHistories = _member.GetPatientStagingAndReasonsHistorybyDependentId(DependentID);

            model.RangeHistory = _member.GetHba1cRangeHistory(DependentID).Take(4).ToList(); //HCAre-1241

            return View(model);
        }

        public ActionResult PatientClinical_Pathology(Guid DependentID, Guid? pro)
        {
            #region program-code
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }
            #endregion

            TempData["dependantID"] = DependentID;
            TempData["program"] = pro;

            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;

            #region pathology            
            model.Pathologies = new List<Pathology>();
            model.Pathologies = _member.GetPathology(DependentID);
            model.hivBloodResults = new List<Pathology>();
            model.hivBloodResults = _member.GetPathology(DependentID);
            //-->> HCare-851
            model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
            model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
            model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
            model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
            model.TBPathologies = new List<QuestionnaireOther>();
            model.TBPathologies = _member.GetQuestionnaireOtherResults(DependentID);
            #endregion
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
            #endregion

            model.programcode = programCode;

            return View(model);
        }

        //public ActionResult PatientClinical_Script(Guid DependentID, string sortOrder, Guid? pro) //hcare-1313 - hidden 26/04/2021
        //{
        //    var programCode = string.Empty;
        //    if (pro != null)
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
        //    }
        //    else
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
        //    }


        //    TempData["dependantID"] = DependentID;
        //    TempData["program"] = pro;

        //    var dependantInfo = _member.GetDependantByDependantID(DependentID);
        //    var memberInfo = _member.GetMemberByDependantID(DependentID);

        //    ClinicalViewModel model = new ClinicalViewModel();
        //    model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
        //    model.DependentID = DependentID;
        //    model.Scripts = new List<Scripts>();
        //    model.ScriptItems = new List<ScriptViewModel>();
        //    model.Scripts = _member.GetScripts(DependentID);
        //    if (model.Scripts.Count != 0)
        //    {
        //        int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
        //        model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //        model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
        //        model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
        //        model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
        //        model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //    }
        //    model.claimHistory = new ClaimsHistory();
        //    //-->>            
        //    model.Pathologies = new List<Pathology>();
        //    model.Pathologies = _member.GetPathology(DependentID);
        //    model.hivBloodResults = new List<Pathology>();
        //    model.hivBloodResults = _member.GetPathology(DependentID);
        //    //-->> HCare-851
        //    model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
        //    model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
        //    model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
        //    model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
        //    //-->>
        //    #region clinical-rules
        //    var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

        //    var male = false;
        //    var female = false;
        //    var rules = new List<ClinicalRules>();

        //    if (dependantInfo.sex.ToUpper().Contains("F"))
        //    {
        //        female = true;
        //        rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
        //    }
        //    else
        //    {
        //        male = true;
        //        rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
        //    }
        //    #endregion
        //    model.clinicalRules = rules;
        //    model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
        //    //-->>
        //    model.rulesBroken = new List<RulesInstructions>();
        //    model.rulesBroken = _member.GetRulesHistory(DependentID).Where(x => x.program == programCode).ToList();
        //    //-->>
        //    model.BrokenRules = _member.GetClinicalRulesHistory(DependentID).Where(x => x.ProgramCode == programCode).ToList();
        //    //-->>
        //    model.summary = new PatientClinicalSummaryViewModel();
        //    //-->>
        //    model.summary.clinicalexams = new List<Clinical>();
        //    model.summary.clinicalexams = _member.GetClinicalExam(DependentID);
        //    model.summary.generalClinicalExams = new List<Clinical>();
        //    model.summary.generalClinicalExams = _member.GetGeneralClinicalExam(DependentID);
        //    model.summary.diabetesClinicalExams = new List<Clinical>();
        //    model.summary.diabetesClinicalExams = _member.GetDiabetesClinicalExam(DependentID);
        //    model.summary.hivClinicalExams = new List<Clinical>();
        //    model.summary.hivClinicalExams = _member.GetHivClinicalExam(DependentID);
        //    //-->>
        //    model.CoMorbids = new List<ComorbidView>();
        //    model.CoMorbids = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));
        //    model.generalCoMorbids = new List<ComorbidView>();
        //    model.generalCoMorbids = _member.GetGeneralComorbidItems(DependentID, new Guid(Request.Query["pro"]));
        //    //-->>
        //    model.summary.meds = new List<MedicationHistory>();
        //    model.summary.meds = _member.GetMedicationHistories(DependentID);
        //    model.summary.generalMedications = new List<MedicationHistory>();
        //    model.summary.generalMedications = _member.GetGeneralMedicationHistories(DependentID);
        //    model.summary.hivMedications = new List<MedicationHistory>();
        //    model.summary.hivMedications = _member.GetHIVMedicationHistories(DependentID);
        //    //-->>
        //    model.summary.allergies = new List<Allergies>();
        //    model.summary.allergies = _member.GetAllergies(DependentID);
        //    model.summary.generalAllergies = new List<Allergies>();
        //    model.summary.generalAllergies = _member.GetGeneralAllergies(DependentID);
        //    //-->>
        //    model.summary.questionaire = new ClinicalHistoryQuestionaire();
        //    model.summary.questionaire = _member.getQuestionnaire(DependentID);
        //    model.summary.questionaires = _member.GetSocialHistory(DependentID);
        //    model.summary.generalQuestionaires = _member.GetGeneralSocialHistory(DependentID);
        //    //-->>
        //    var memberinfo = _member.GetDependentDetails(DependentID, null);
        //    model.HospitalClaims = new List<HospitalClaimView>();
        //    model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    model.HospitalAuths = new List<HospitalizationAuths>();
        //    model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    model.generalHospitalAuths = new List<HospitalizationAuths>();
        //    model.generalHospitalAuths = _member.GetGeneralHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
        //    //-->>
        //    model.summary.questionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);
        //    model.summary.generalQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.generalQuestionnaireOthers = _member.GetGeneralQuestionnaireOtherResults(DependentID);
        //    model.summary.diabetesQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.diabetesQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);
        //    model.summary.hivQuestionnaireOthers = new List<QuestionnaireOther>();
        //    model.summary.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);
        //    //-->>
        //    model.summary.Other = new List<QuestionnaireOther>(); //HCare-968
        //    model.summary.Other = _member.GetOtherResults(DependentID); //HCare-968
        //    model.summary.Pregnancy = new List<QuestionnaireOther>(); //HCare-968
        //    model.summary.Pregnancy = _member.GetPregnancyResults(DependentID); //HCare-968
        //                                                                        //-->>
        //    model.summary.NewBorns = new List<NewBorn>(); //HCare-968
        //    model.summary.NewBorns = _member.GetNewBornResults(DependentID); //HCare-968
        //                                                                     //-->>
        //    model.summary.programDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.programDiagnoses = _member.GetDiagnosisResults(DependentID);
        //    model.summary.diabetesProgramDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.diabetesProgramDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
        //    model.summary.hivProgramDiagnoses = new List<PatientDiagnosis>();
        //    model.summary.hivProgramDiagnoses = _member.GetHIVDiagnosisResults(DependentID);
        //    //-->>
        //    model.summary.visions = new List<Vision>();
        //    model.summary.visions = _member.GetVisionResults(DependentID);
        //    //-->>
        //    model.summary.podiatries = new List<Podiatry>();
        //    model.summary.podiatries = _member.GetPodiatryResults(DependentID);
        //    model.summary.podiatry = new Podiatry();
        //    //-->>
        //    model.summary.autoNeropathies = new List<AutoNeropathy>();
        //    model.summary.autoNeropathies = _member.GetAutoNeroResults(DependentID);
        //    //-->>
        //    model.summary.hypoglycaemias = new List<Hypoglycaemia>();
        //    model.summary.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);
        //    //-->>
        //    model.summary.otherMedicalHistories = new List<OtherMedicalHistory>();
        //    model.summary.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);
        //    //-->>
        //    model.summary.doctorQuestionnaireResults = new List<DoctorQuestionnaireResults>();
        //    model.summary.doctorQuestionnaireResults = _member.GetDrQuesResults(DependentID);
        //    //-->>
        //    #region mental-health
        //    //-->>
        //    model.summary.MH_DSM5Responses = new List<MH_DSM5ResponseNEW>(); //HCare-1123
        //    model.summary.MH_DSM5Responses = _admin.GetNEWDSM5Results(DependentID); //HCare-1123

        //    model.summary.MH_SchizophreniaResponses = new List<MH_SchizophreniaResponse>(); //HCare-1124
        //    model.summary.MH_SchizophreniaResponses = _admin.GetSchizophreniaResults(DependentID); //HCare-1124

        //    model.summary.MH_BipolarResponses = new List<MH_BipolarResponse>(); //HCare-1125
        //    model.summary.MH_BipolarResponses = _admin.GetBipolarResults(DependentID); //HCare-1125

        //    model.summary.MH_DepressionResponses = new List<MH_DepressionResponse>(); //HCare-1126
        //    model.summary.MH_DepressionResponses = _admin.GetDepressionResults(DependentID); //HCare-1126
        //                                                                                     //-->>
        //    #endregion

        //    model.scriptReferences = new List<ScriptReferences>();
        //    model.scriptReferences = _member.GetScriptReference(DependentID);
        //    //-->>
        //    model.programs = new List<PatientProgramHistory>();
        //    model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
        //    model.programcode = programCode;
        //    //-->>
        //    model.gender = dependantInfo.sex;

        //    ViewBag.effectiveDateAsc = sortOrder == "effectiveDate" ? "effectiveDate_asc" : "effectiveDate";

        //    switch (sortOrder)
        //    {
        //        case "effectiveDate":
        //            if (sortOrder.Equals(ViewBag))
        //                model.Pathologies = _member.GetPathology(DependentID).OrderBy(c => c.effectiveDate).ToList();
        //            else
        //                model.Pathologies = _member.GetPathology(DependentID).OrderByDescending(c => c.effectiveDate).ToList();
        //            break;
        //    }

        //    model.RiskRating = new List<PatientRiskRatingHistory>();
        //    model.RiskRating = _member.GetPatientRiskRatingMultipleReasons(DependentID, pro);

        //    model.DoctorReferrals = new List<DoctorReferral>();
        //    model.DoctorReferrals = _member.GetDoctorReferralResults(DependentID);

        //    return View(model);
        //}


        public ActionResult PatientClinical_PatientHistory(Guid DependentID, Guid? pro)
        {
            #region program-code
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }
            #endregion

            TempData["dependantID"] = DependentID;
            TempData["program"] = pro;

            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;
            #region medication
            model.Scripts = new List<Scripts>();
            model.ScriptItems = new List<ScriptViewModel>();
            model.Scripts = _member.GetScripts(DependentID);
            if (model.Scripts.Count != 0)
            {
                int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
                model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
                model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
                model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
                model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
                model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
                model.MentalHealthScriptItems = _member.GetMHScriptItems(DependentID).Where(x => x.program == programCode).ToList();
            }
            model.claimHistory = new ClaimsHistory();
            #endregion
            #region pathology + clinical rules
            model.hivBloodResults = new List<Pathology>();
            model.hivBloodResults = _member.GetPathology(DependentID);
            model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
            #endregion
            #endregion
            #region patient-history
            model.summary = new PatientClinicalSummaryViewModel();
            //-->>
            model.summary.clinicalexams = new List<Clinical>();
            model.summary.clinicalexams = _member.GetClinicalExam(DependentID);
            model.summary.generalClinicalExams = new List<Clinical>();
            model.summary.generalClinicalExams = _member.GetGeneralClinicalExam(DependentID);
            model.summary.diabetesClinicalExams = new List<Clinical>();
            model.summary.diabetesClinicalExams = _member.GetDiabetesClinicalExam(DependentID);
            model.summary.hivClinicalExams = new List<Clinical>();
            model.summary.hivClinicalExams = _member.GetHivClinicalExam(DependentID);
            //-->>
            model.CoMorbids = new List<ComorbidView>();
            model.CoMorbids = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            model.generalCoMorbids = new List<ComorbidView>();
            model.generalCoMorbids = _member.GetGeneralComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            //-->>
            model.summary.meds = new List<MedicationHistory>();
            model.summary.meds = _member.GetMedicationHistories(DependentID);
            model.summary.generalMedications = new List<MedicationHistory>();
            model.summary.generalMedications = _member.GetGeneralMedicationHistories(DependentID);
            model.summary.hivMedications = new List<MedicationHistory>();
            model.summary.hivMedications = _member.GetHIVMedicationHistories(DependentID);
            //-->>
            model.summary.allergies = new List<Allergies>();
            model.summary.allergies = _member.GetAllergies(DependentID);
            model.summary.generalAllergies = new List<Allergies>();
            model.summary.generalAllergies = _member.GetGeneralAllergies(DependentID);
            //-->>
            model.summary.questionaire = new ClinicalHistoryQuestionaire();
            model.summary.questionaire = _member.getQuestionnaire(DependentID);
            model.summary.questionaires = _member.GetSocialHistory(DependentID);
            model.summary.generalQuestionaires = _member.GetGeneralSocialHistory(DependentID);
            //-->>
            var memberinfo = _member.GetDependentDetails(DependentID, null);
            model.HospitalClaims = new List<HospitalClaimView>();
            model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.HospitalAuths = new List<HospitalizationAuths>();
            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.generalHospitalAuths = new List<HospitalizationAuths>();
            model.generalHospitalAuths = _member.GetGeneralHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            //-->>
            model.summary.questionnaireOthers = new List<QuestionnaireOther>();
            model.summary.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);
            model.summary.generalQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.generalQuestionnaireOthers = _member.GetGeneralQuestionnaireOtherResults(DependentID);
            model.summary.diabetesQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.diabetesQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);
            model.summary.hivQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);
            //-->>
            model.summary.Other = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Other = _member.GetOtherResults(DependentID); //HCare-968
            model.summary.Pregnancy = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Pregnancy = _member.GetPregnancyResults(DependentID); //HCare-968
                                                                                //-->>
            model.summary.NewBorns = new List<NewBorn>(); //HCare-968
            model.summary.NewBorns = _member.GetNewBornResults(DependentID); //HCare-968
                                                                             //-->>
            model.summary.programDiagnoses = new List<PatientDiagnosis>();
            model.summary.programDiagnoses = _member.GetDiagnosisResults(DependentID);
            model.summary.diabetesProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.diabetesProgramDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
            model.summary.hivProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.hivProgramDiagnoses = _member.GetHIVDiagnosisResults(DependentID);
            //-->>
            model.summary.visions = new List<Vision>();
            model.summary.visions = _member.GetVisionResults(DependentID);
            //-->>
            model.summary.podiatries = new List<Podiatry>();
            model.summary.podiatries = _member.GetPodiatryResults(DependentID);
            model.summary.podiatry = new Podiatry();
            //-->>
            model.summary.autoNeropathies = new List<AutoNeropathy>();
            model.summary.autoNeropathies = _member.GetAutoNeroResults(DependentID);
            //-->>
            model.summary.hypoglycaemias = new List<Hypoglycaemia>();
            model.summary.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);
            //-->>
            model.summary.otherMedicalHistories = new List<OtherMedicalHistory>();
            model.summary.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);
            //-->>
            model.summary.doctorQuestionnaireResults = new List<DoctorQuestionnaireResults>();
            model.summary.doctorQuestionnaireResults = _member.GetDrQuesResults(DependentID);
            //-->>
            #region mental-health
            //-->>
            model.summary.MH_DSM5Responses = new List<MH_DSM5ResponseNEW>(); //HCare-1203
            model.summary.MH_DSM5Responses = _admin.GetNEWDSM5Results(DependentID); //HCare-1203
            if (model.summary.MH_DSM5Responses.Count != 0)
            {
                var maximumdate = _admin.GetNEWDSM5Results(DependentID).OrderByDescending(x => x.CreatedDate).First();
                if (maximumdate != null) { ViewBag.MaxDate = maximumdate.Id; }
            }

            model.summary.MH_SchizophreniaResponses = new List<MH_SchizophreniaResponse>(); //HCare-1124
            model.summary.MH_SchizophreniaResponses = _admin.GetSchizophreniaResults(DependentID); //HCare-1124

            model.summary.MH_BipolarResponses = new List<MH_BipolarResponse>(); //HCare-1125
            model.summary.MH_BipolarResponses = _admin.GetBipolarResults(DependentID); //HCare-1125

            model.summary.MH_DepressionResponses = new List<MH_DepressionResponse>(); //HCare-1126
            model.summary.MH_DepressionResponses = _admin.GetDepressionResults(DependentID); //HCare-1126
                                                                                             //-->>
            #endregion
            #endregion

            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
            model.programcode = programCode;
            model.gender = dependantInfo.sex;
            #region medical care plan hcare-1092
            model.summary.diabetesCarePlanPathologies = new List<CarePlanPathology>();
            model.summary.diabetesCarePlanPathologies = _member.GetCarePathology(DependentID, pro);
            model.summary.clinicalAdditions = new List<ClinicalAddition>();
            model.summary.clinicalAdditions = _member.GetClinicalAdditionByClinicalID(DependentID, pro);
            model.summary.nutritionAndLifestyles = new List<NutritionAndLifestyle>();
            model.summary.nutritionAndLifestyles = _member.GetNutritionandlifestyleByDependentID(DependentID, pro);
            model.summary.medicines = new List<Medicine>();
            model.summary.medicines = _member.GetMedicinesByDependentID(DependentID, pro);
            model.summary.visits = new List<Visit>();
            model.summary.visits = _member.GetVisitByDependentID(DependentID, pro);
            model.summary.paediatrics = new List<paediatric>();
            model.summary.paediatrics = _member.GetPaediatricByDependentID(DependentID, pro);
            #endregion

            model.MHDiagnoses = new List<MHDiagnosisVM>(); //HCare-1194
            model.MHDiagnoses = _member.GetMHDiagnosis(DependentID); //HCare-1194

            return View(model);
        }

        //public ActionResult PatientClinical_ScriptHistory(Guid DependentID, Guid? pro) //hcare-1313 - hidden 26/04/2021
        //{
        //    #region program-code
        //    var programCode = string.Empty;
        //    if (pro != null)
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
        //    }
        //    else
        //    {
        //        programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
        //    }
        //    #endregion

        //    TempData["dependantID"] = DependentID;
        //    TempData["program"] = pro;

        //    var dependantInfo = _member.GetDependantByDependantID(DependentID);
        //    var memberInfo = _member.GetMemberByDependantID(DependentID);

        //    ClinicalViewModel model = new ClinicalViewModel();
        //    model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
        //    model.DependentID = DependentID;
        //    model.Scripts = new List<Scripts>();
        //    model.ScriptItems = new List<ScriptViewModel>();
        //    model.Scripts = _member.GetScripts(DependentID);
        //    if (model.Scripts.Count != 0)
        //    {
        //        int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
        //        model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //        model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
        //        model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
        //        model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
        //        model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
        //    }
        //    model.claimHistory = new ClaimsHistory();
        //    //-->>
        //    model.scriptReferences = new List<ScriptReferences>();
        //    model.scriptReferences = _member.GetScriptReference(DependentID);
        //    //-->>
        //    model.programs = new List<PatientProgramHistory>();
        //    model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
        //    model.programcode = programCode;
        //    //-->>

        //    return View(model);
        //}

        public ActionResult _PatientClinical_Diabetes(Guid DependentID, Guid? pro) //HCare-1142
        {
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }

            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;
            model.Scripts = new List<Scripts>();
            model.ScriptItems = new List<ScriptViewModel>();
            model.Scripts = _member.GetScripts(DependentID);
            if (model.Scripts.Count != 0)
            {
                int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
                model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
                model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
                model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
                model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
                model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
            }
            model.claimHistory = new ClaimsHistory();
            //-->>            
            model.Pathologies = new List<Pathology>();
            model.Pathologies = _member.GetPathology(DependentID);
            model.hivBloodResults = new List<Pathology>();
            model.hivBloodResults = _member.GetPathology(DependentID);
            //-->> HCare-851
            model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
            model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
            model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
            model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
            //-->>
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            #endregion
            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
            //-->>
            model.rulesBroken = new List<RulesInstructions>();
            model.rulesBroken = _member.GetRulesHistory(DependentID).Where(x => x.program == programCode).ToList();
            //-->>
            model.BrokenRules = _member.GetClinicalRulesHistory(DependentID).Where(x => x.ProgramCode == programCode).ToList();
            //-->>
            model.summary = new PatientClinicalSummaryViewModel();
            //-->>
            model.summary.clinicalexams = new List<Clinical>();
            model.summary.clinicalexams = _member.GetClinicalExam(DependentID);
            model.summary.generalClinicalExams = new List<Clinical>();
            model.summary.generalClinicalExams = _member.GetGeneralClinicalExam(DependentID);
            model.summary.diabetesClinicalExams = new List<Clinical>();
            model.summary.diabetesClinicalExams = _member.GetDiabetesClinicalExam(DependentID);
            model.summary.hivClinicalExams = new List<Clinical>();
            model.summary.hivClinicalExams = _member.GetHivClinicalExam(DependentID);
            //-->>
            model.CoMorbids = new List<ComorbidView>();
            model.CoMorbids = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            model.generalCoMorbids = new List<ComorbidView>();
            model.generalCoMorbids = _member.GetGeneralComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            //-->>
            model.summary.meds = new List<MedicationHistory>();
            model.summary.meds = _member.GetMedicationHistories(DependentID);
            model.summary.generalMedications = new List<MedicationHistory>();
            model.summary.generalMedications = _member.GetGeneralMedicationHistories(DependentID);
            model.summary.hivMedications = new List<MedicationHistory>();
            model.summary.hivMedications = _member.GetHIVMedicationHistories(DependentID);
            //-->>
            model.summary.allergies = new List<Allergies>();
            model.summary.allergies = _member.GetAllergies(DependentID);
            model.summary.generalAllergies = new List<Allergies>();
            model.summary.generalAllergies = _member.GetGeneralAllergies(DependentID);
            //-->>
            model.summary.questionaire = new ClinicalHistoryQuestionaire();
            model.summary.questionaire = _member.getQuestionnaire(DependentID);
            model.summary.questionaires = _member.GetSocialHistory(DependentID);
            model.summary.generalQuestionaires = _member.GetGeneralSocialHistory(DependentID);
            //-->>
            var memberinfo = _member.GetDependentDetails(DependentID, null);
            model.HospitalClaims = new List<HospitalClaimView>();
            model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.HospitalAuths = new List<HospitalizationAuths>();
            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.generalHospitalAuths = new List<HospitalizationAuths>();
            model.generalHospitalAuths = _member.GetGeneralHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            //-->>
            model.summary.questionnaireOthers = new List<QuestionnaireOther>();
            model.summary.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);
            model.summary.generalQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.generalQuestionnaireOthers = _member.GetGeneralQuestionnaireOtherResults(DependentID);
            model.summary.diabetesQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.diabetesQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);
            model.summary.hivQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);
            //-->>
            model.summary.Other = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Other = _member.GetOtherResults(DependentID); //HCare-968
            model.summary.Pregnancy = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Pregnancy = _member.GetPregnancyResults(DependentID); //HCare-968
                                                                                //-->>
            model.summary.NewBorns = new List<NewBorn>(); //HCare-968
            model.summary.NewBorns = _member.GetNewBornResults(DependentID); //HCare-968
                                                                             //-->>
            model.summary.programDiagnoses = new List<PatientDiagnosis>();
            model.summary.programDiagnoses = _member.GetDiagnosisResults(DependentID);
            model.summary.diabetesProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.diabetesProgramDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
            model.summary.hivProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.hivProgramDiagnoses = _member.GetHIVDiagnosisResults(DependentID);
            //-->>
            model.summary.visions = new List<Vision>();
            model.summary.visions = _member.GetVisionResults(DependentID);
            //-->>
            model.summary.podiatries = new List<Podiatry>();
            model.summary.podiatries = _member.GetPodiatryResults(DependentID);
            model.summary.podiatry = new Podiatry();
            //-->>
            model.summary.autoNeropathies = new List<AutoNeropathy>();
            model.summary.autoNeropathies = _member.GetAutoNeroResults(DependentID);
            //-->>
            model.summary.hypoglycaemias = new List<Hypoglycaemia>();
            model.summary.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);
            //-->>
            model.summary.otherMedicalHistories = new List<OtherMedicalHistory>();
            model.summary.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);
            //-->>
            model.summary.doctorQuestionnaireResults = new List<DoctorQuestionnaireResults>();
            model.summary.doctorQuestionnaireResults = _member.GetDrQuesResults(DependentID);
            //-->>



            model.scriptReferences = new List<ScriptReferences>();
            model.scriptReferences = _member.GetScriptReference(DependentID);
            //-->>
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
            model.programcode = programCode;
            //-->>
            model.gender = dependantInfo.sex;


            return PartialView("~/Views/Member/Partials/_PatientClinical_Diabetes.cshtml", model);

        }

        public ActionResult _PatientClinical_HIV(Guid DependentID, Guid? pro) //HCare-1142
        {
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }

            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;
            model.Scripts = new List<Scripts>();
            model.ScriptItems = new List<ScriptViewModel>();
            model.Scripts = _member.GetScripts(DependentID);
            if (model.Scripts.Count != 0)
            {
                int scriptno = Convert.ToInt32(model.Scripts.FirstOrDefault().scriptID);
                model.ScriptItems = _member.GetScriptItems(DependentID).Where(x => x.program == programCode).ToList();
                model.ScriptItemsFull = _member.GetScriptItemsFull(DependentID).Where(x => x.program == programCode).ToList(); //This is not filtering out the different medications that are set to the program. Discussed with Steve, we will look at it later...
                model.generalScriptItems = _member.GetGeneralScriptItems(DependentID).Where(x => x.program != programCode).ToList();
                model.diabetesScriptItems = _member.GetDiabetesScriptItems(DependentID)/*.Where(x => x.program == programCode)*/.ToList();
                model.hivScriptItems = _member.GetHIVScriptItems(DependentID).Where(x => x.program == programCode).ToList();
            }
            model.claimHistory = new ClaimsHistory();
            //-->>            
            model.Pathologies = new List<Pathology>();
            model.Pathologies = _member.GetPathology(DependentID);
            model.hivBloodResults = new List<Pathology>();
            model.hivBloodResults = _member.GetPathology(DependentID);
            //-->> HCare-851
            model.generalPathologies = _member.GetGeneralPathologyForDependant(DependentID);
            model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathologyForDependant(DependentID);
            model.diabetesPathologies = _member.GetDiabetesPathologyForDependant(DependentID);
            model.hivPathologies = _member.GetHIVPathologyForDependant(DependentID);
            //-->>
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            #endregion
            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);
            //-->>
            model.rulesBroken = new List<RulesInstructions>();
            model.rulesBroken = _member.GetRulesHistory(DependentID).Where(x => x.program == programCode).ToList();
            //-->>
            model.BrokenRules = _member.GetClinicalRulesHistory(DependentID).Where(x => x.ProgramCode == programCode).ToList();
            //-->>
            model.summary = new PatientClinicalSummaryViewModel();
            //-->>
            model.summary.clinicalexams = new List<Clinical>();
            model.summary.clinicalexams = _member.GetClinicalExam(DependentID);
            model.summary.generalClinicalExams = new List<Clinical>();
            model.summary.generalClinicalExams = _member.GetGeneralClinicalExam(DependentID);
            model.summary.diabetesClinicalExams = new List<Clinical>();
            model.summary.diabetesClinicalExams = _member.GetDiabetesClinicalExam(DependentID);
            model.summary.hivClinicalExams = new List<Clinical>();
            model.summary.hivClinicalExams = _member.GetHivClinicalExam(DependentID);
            //-->>
            model.CoMorbids = new List<ComorbidView>();
            model.CoMorbids = _member.GetComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            model.generalCoMorbids = new List<ComorbidView>();
            model.generalCoMorbids = _member.GetGeneralComorbidItems(DependentID, new Guid(Request.Query["pro"]));
            //-->>
            model.summary.meds = new List<MedicationHistory>();
            model.summary.meds = _member.GetMedicationHistories(DependentID);
            model.summary.generalMedications = new List<MedicationHistory>();
            model.summary.generalMedications = _member.GetGeneralMedicationHistories(DependentID);
            model.summary.hivMedications = new List<MedicationHistory>();
            model.summary.hivMedications = _member.GetHIVMedicationHistories(DependentID);
            //-->>
            model.summary.allergies = new List<Allergies>();
            model.summary.allergies = _member.GetAllergies(DependentID);
            model.summary.generalAllergies = new List<Allergies>();
            model.summary.generalAllergies = _member.GetGeneralAllergies(DependentID);
            //-->>
            model.summary.questionaire = new ClinicalHistoryQuestionaire();
            model.summary.questionaire = _member.getQuestionnaire(DependentID);
            model.summary.questionaires = _member.GetSocialHistory(DependentID);
            model.summary.generalQuestionaires = _member.GetGeneralSocialHistory(DependentID);
            //-->>
            var memberinfo = _member.GetDependentDetails(DependentID, null);
            model.HospitalClaims = new List<HospitalClaimView>();
            model.HospitalClaims = _member.GetHospitalizationClaim(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.HospitalAuths = new List<HospitalizationAuths>();
            model.HospitalAuths = _member.GetHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            model.generalHospitalAuths = new List<HospitalizationAuths>();
            model.generalHospitalAuths = _member.GetGeneralHospitalizationAuths(memberinfo.member.membershipNo, memberinfo.dependent.dependentCode);
            //-->>
            model.summary.questionnaireOthers = new List<QuestionnaireOther>();
            model.summary.questionnaireOthers = _member.GetQuestionnaireOtherResults(DependentID);
            model.summary.generalQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.generalQuestionnaireOthers = _member.GetGeneralQuestionnaireOtherResults(DependentID);
            model.summary.diabetesQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.diabetesQuestionnaireOthers = _member.GetDiabetesQuestionnaireOtherResults(DependentID);
            model.summary.hivQuestionnaireOthers = new List<QuestionnaireOther>();
            model.summary.hivQuestionnaireOthers = _member.GetHIVQuestionnaireOtherResults(DependentID);
            //-->>
            model.summary.Other = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Other = _member.GetOtherResults(DependentID); //HCare-968
            model.summary.Pregnancy = new List<QuestionnaireOther>(); //HCare-968
            model.summary.Pregnancy = _member.GetPregnancyResults(DependentID); //HCare-968
                                                                                //-->>
            model.summary.NewBorns = new List<NewBorn>(); //HCare-968
            model.summary.NewBorns = _member.GetNewBornResults(DependentID); //HCare-968
                                                                             //-->>
            model.summary.programDiagnoses = new List<PatientDiagnosis>();
            model.summary.programDiagnoses = _member.GetDiagnosisResults(DependentID);
            model.summary.diabetesProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.diabetesProgramDiagnoses = _member.GetDiabetesDiagnosisResults(DependentID);
            model.summary.hivProgramDiagnoses = new List<PatientDiagnosis>();
            model.summary.hivProgramDiagnoses = _member.GetHIVDiagnosisResults(DependentID);
            //-->>
            model.summary.visions = new List<Vision>();
            model.summary.visions = _member.GetVisionResults(DependentID);
            //-->>
            model.summary.podiatries = new List<Podiatry>();
            model.summary.podiatries = _member.GetPodiatryResults(DependentID);
            model.summary.podiatry = new Podiatry();
            //-->>
            model.summary.autoNeropathies = new List<AutoNeropathy>();
            model.summary.autoNeropathies = _member.GetAutoNeroResults(DependentID);
            //-->>
            model.summary.hypoglycaemias = new List<Hypoglycaemia>();
            model.summary.hypoglycaemias = _member.GetHypoglycaemiaResults(DependentID);
            //-->>
            model.summary.otherMedicalHistories = new List<OtherMedicalHistory>();
            model.summary.otherMedicalHistories = _member.GetOtherMedicalHistory(DependentID);
            //-->>
            model.summary.doctorQuestionnaireResults = new List<DoctorQuestionnaireResults>();
            model.summary.doctorQuestionnaireResults = _member.GetDrQuesResults(DependentID);
            //-->>



            model.scriptReferences = new List<ScriptReferences>();
            model.scriptReferences = _member.GetScriptReference(DependentID);
            //-->>
            model.programs = new List<PatientProgramHistory>();
            model.programs = _member.GetProgramHistory(DependentID).Where(x => x.active == true).ToList();
            model.programcode = programCode;
            //-->>
            model.gender = dependantInfo.sex;


            return PartialView("~/Views/Member/Partials/_PatientClinical_HIV.cshtml", model);

        }

        public ActionResult _PatientClinical_MentalHealth(Guid DependentID, Guid? pro) //HCare-1142
        {
            var programCode = string.Empty;
            if (pro != null)
            {
                programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }
            else
            {
                programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            }
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);

            ClinicalViewModel model = new ClinicalViewModel();
            model = _member.GetClinicalInfo(DependentID, new Guid(Request.Query["pro"]));
            model.DependentID = DependentID;

            model.summary = new PatientClinicalSummaryViewModel();

            model.summary.MH_DSM5Responses = new List<MH_DSM5ResponseNEW>(); //HCare-1123
            model.summary.MH_DSM5Responses = _admin.GetNEWDSM5Results(DependentID); //HCare-1123

            model.summary.MH_SchizophreniaResponses = new List<MH_SchizophreniaResponse>(); //HCare-1124
            model.summary.MH_SchizophreniaResponses = _admin.GetSchizophreniaResults(DependentID); //HCare-1124

            model.summary.MH_BipolarResponses = new List<MH_BipolarResponse>(); //HCare-1125
            model.summary.MH_BipolarResponses = _admin.GetBipolarResults(DependentID); //HCare-1125

            model.summary.MH_DepressionResponses = new List<MH_DepressionResponse>(); //HCare-1126
            model.summary.MH_DepressionResponses = _admin.GetDepressionResults(DependentID); //HCare-1126

            return PartialView("~/Views/Member/Partials/_PatientClinical_MentalHealth.cshtml", model);

        }

        public ActionResult Pathology_Search_Dependant(Guid DependentID, Guid? pro) //HCare-974
        {
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();


            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode.code).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode.code).ToList();
            }
            #endregion

            var model = new PathologySearchVM();

            ViewBag.DependentID = dependantInfo.DependantID;
            ViewBag.pro = programCode.programID;

            model.ClinicalRules = rules;
            model.PathologyFields = _member.GetPathologyFields();
            model.PathologySearches = new List<PathologySearch>();
            model.PathologySearchResults = new List<PathologySortVM>();

            model.PathologyFieldsList = _member.GetPathologyFields().Where(x => x.active == true).ToList();
            model.DependentID = DependentID;
            model.pro = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult Pathology_Search_Dependant(PathologySearchVM model) //HCare-974
        {
            var dependantInfo = _member.GetDependantByDependantID(model.DependentID);
            var memberInfo = _member.GetMemberByDependantID(model.DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            var pathField = _member.GetPathologyFields();
            var displayFields = model.SelectedPathologyFields;
            ViewBag.displayFields = displayFields;

            #region form-dropdowns
            model.PathologyFieldsList = _member.GetPathologyFields().Where(x => x.active == true).ToList();
            #endregion
            #region form-variables
            var pathologyfield = String.Empty;
            if (model.SelectedPathologyFields != null) { pathologyfield = String.Join(",", model.SelectedPathologyFields); } else { model.SelectedPathologyFields = new string[] { }; }
            #region date-variables
            var fromDate = "";
            var toDate = "";

            if (!String.IsNullOrEmpty(Request.Query["FromDate"]))
            {
                fromDate = Convert.ToDateTime(Request.Query["FromDate"]).ToString("dd-MMM-yyyy 00:00:00");
            }
            else
            {
                fromDate = new DateTime(2019, 1, 1).ToString("dd-MMM-yyyy 00:00:00"); //HCare-1129
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
            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode.code).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode.code).ToList();
            }
            model.ClinicalRules = rules;
            #endregion

            model.PathologySearchResults = _member.GetSortedPathologySearchResults(model.DependentID, pathologyfield, fromDate, toDate);
            model.PathologyFields = pathField.Where(c => displayFields.Contains(c.DisplayName)).ToList();

            if (model.PathologySearchResults.Count == 0)
            {
                ViewBag.Message = "No records found.";
            }

            #region hidden-fields
            TempData["pathologyfields"] = pathologyfield;
            TempData["fromdate"] = fromDate;
            TempData["todate"] = toDate;
            TempData["dependantid"] = model.DependentID;
            #endregion

            return View(model);
        }
        public ActionResult ExportPathologyResultsToExcel(PathologySearchVM model) //HCare-974
        {
            var pathologyfields = Request.Query["pathologyfields"].ToString();
            var fromdate = Request.Query["fromdate"].ToString();
            var todate = Request.Query["todate"].ToString();
            var dependantid = new Guid(Request.Query["dependantid"]);

            string[] pathField = pathologyfields.Split(',');
            var pf = pathField.ToList();

            var pfields = _member.GetPathologyFields();

            model.PathologySearchResults = _member.GetSortedPathologySearchResults(dependantid, pathologyfields, fromdate, todate);
            model.PathologyFields = pfields.Where(c => pf.Contains(c.DisplayName)).ToList();

            #region data-table
            DataTable table = new DataTable();
            table.Columns.Add("Pathology date");
            foreach (var item in pf)
            {
                table.Columns.Add(item.ToString());
            }
            foreach (var line in model.PathologySearchResults)
            {
                DataRow row = table.NewRow();

                //Map all the values in the columns
                row["Pathology date"] = line.InitialDate.ToString("dd-MM-yyyy");
                foreach (var item in model.PathologyFields)
                {
                    row[item.DisplayName] = line.GetType().GetProperty(item.DisplayName.Replace(" ", "")).GetValue(line);
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
                memoryStream.WriteTo(Response.OutputStream);
                Response.Body.Flush();
                Response.StatusCode = StatusCodes.Status200OK;
                return View(model);

            }
            #endregion

        }

        public ActionResult pathologyHistory_general(Guid DependentID, Guid? pro)
        {
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            ClinicalViewModel model = new ClinicalViewModel();

            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode.code).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode.code).ToList();
            }
            #endregion

            model.generalPathologies = new List<Pathology>();
            model.generalPathologies = _member.GetGeneralPathology(DependentID);
            model.clinicalRules = rules;


            return View(model);
        }

        public ActionResult pathologyHistory_hyperlipidaemia(Guid DependentID, Guid? pro)
        {
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();

            ClinicalViewModel model = new ClinicalViewModel();
            model.programcode = programCode;

            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            #endregion

            model.hyperlipidaemiaPathologies = new List<Pathology>();
            model.hyperlipidaemiaPathologies = _member.GetHyperlipidaemiaPathology(DependentID);

            model.clinicalRules = rules;
            model.clinicalRulesVM = _member.GetClinicalRulesByDependant(DependentID, programCode);

            return View(model);

        }
        public ActionResult pathologyHistory_diabetes(Guid DependentID) //HCare-974
        {
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();

            ClinicalViewModel model = new ClinicalViewModel();

            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            #endregion

            model.diabetesPathologies = new List<Pathology>();
            model.diabetesPathologies = _member.GetDiabetesPathology(DependentID);
            model.clinicalRules = rules;

            return View(model);

        }
        public ActionResult pathologyHistory_hiv(Guid DependentID)
        {
            var dependantInfo = _member.GetDependantByDependantID(DependentID);
            var memberInfo = _member.GetMemberByDependantID(DependentID);
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();

            ClinicalViewModel model = new ClinicalViewModel();

            #region clinical-rules
            var AllowedRules = _admin.GetClinicalRulesByScheme(memberInfo.medicalAidID);

            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();

            if (dependantInfo.sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == programCode).ToList();
            }
            else
            {
                male = true;
                rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == programCode).ToList();
            }
            #endregion

            model.hivPathologies = new List<Pathology>();
            model.hivPathologies = _member.GetHIVPathology(DependentID);

            model.clinicalRules = rules;

            return View(model);

        }

        public ActionResult CreateRiskRating(Guid DependentID) //HCare-1058
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new PatientRiskRatingHistory();
            model.dependantID = DependentID;
            ViewBag.riskType = new SelectList(_member.GetRiskRatingTypes(), "RiskType", "RiskName");
            return View(model);
        }
        [HttpPost]
        public ActionResult CreateRiskRating(PatientRiskRatingHistory model) //HCare-1058
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.RiskId = Request.Query["riskType"].ToString();
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.programType = programCode.code;

            if (!String.IsNullOrEmpty(Request.Query["effectiveDate"]))
            {
                DateTime effectivedate = Convert.ToDateTime(model.effectiveDate);
                model.effectiveDate = new DateTime(effectivedate.Year, effectivedate.Month, effectivedate.Day, model.createdDate.Value.Hour, model.createdDate.Value.Minute, model.createdDate.Value.Second);
            }

            DateTime date = DateTime.Now;
            TimeSpan time = new TimeSpan(36, 0, 0, 0);
            DateTime combined = date.Add(time);
            Console.WriteLine("{0:dddd}", combined);

            model.active = true;

            _member.InsertRiskRating(model);

            #region case-manager-assignmnet hcare-1176
            if (model.RiskId.ToUpper() == "R")
            {
                var CM_assignment = new AssignmentsView()
                {
                    newAssignment = new Assignments()
                    {
                        createdBy = User.Identity.Name,
                        createdDate = DateTime.Now,
                        dependentID = model.dependantID,
                        Active = true,
                        effectiveDate = DateTime.Now,
                        assignmentType = "INTER",
                        status = "Open",
                        Instruction = "Risk rating: Red - Assign case manager",
                        programId = programCode.programID,
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

            return RedirectToAction("PatientClinical_Summary", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
        }

        public ActionResult RiskRating_Edit(int id)
        {
            var model = _member.GetPatientRiskRatingByID(id);
            model.RiskRatings = _member.GetRiskRatingTypes();

            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            return View(model);
        }
        [HttpPost]
        public ActionResult RiskRating_Edit(PatientRiskRatingHistory model)
        {
            model.RiskRatings = _member.GetRiskRatingTypes();
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;
            model.reason = Request.Query["dbreason"];
            if (!String.IsNullOrEmpty(Request.Query["Comment"])) { model.Comment = Request.Query["Comment"]; }

            _member.UpdateRiskRating(model);

            return RedirectToAction("PatientClinical_Summary", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
        }

        public ActionResult RiskRating_Details(int id)
        {
            var model = _member.GetPatientRiskRatingByID(id);

            return View(model);
        }


        public ActionResult patientPathologyExporttoExcel(Guid DependentID)
        {
            var sb = new StringBuilder();
            var model = _member.GetPathology(DependentID);
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = model;
            grid.DataBind();
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment; filename=patientPathology.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.WriteAsync(sw.ToString());
            Response.StatusCode = StatusCodes.Status200OK;
            return View(model);
        }

        //public ActionResult patientProfile(Guid dependentId, Guid pro)
        //{
        //    var model = _member.GetProfile(dependentId);

        //    model.attachment = new Attachments();
        //    model.attachment.dependentID = dependentId;
        //    model.attachments = _member.GetAttachments(dependentId);

        //    model.patientPlanHistory = new PatientPlanHistory();
        //    model.patientPlanHistory.dependantId = dependentId;

        //    model.payPointHistory = new PayPointHistory();
        //    model.payPointHistory.dependantId = dependentId;

        //    model.authLetters = _member.GetAuthorizationLetters(dependentId);
        //    model.doctorQuestionnaireResults = _member.GetDoctorQuestionResultsById(dependentId);

        //    model.disclaimerViewModel = new disclaimerViewModel();
        //    model.disclaimerViewModels = _member.GetDisclaimerViewResults(dependentId);

        //    model.patientDisclaimers = new List<PatientDisclaimer>();
        //    model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(dependentId);

        //    //model.disclaimerViewModels = _member.GetDisclaimerViewDistinct(dependentId);

        //    //model.disclaimerResults = _member.GetDisclaimerViewList(dependentId);

        //    ViewBag.attachmentType = new SelectList(_member.GetAttachmentTypes(), "attachmentType", "typeDescription");
        //    ViewBag.benefitPlan = new SelectList(model.patientPlans, "Id", "Name");
        //    ViewBag.paypoints = new SelectList(model.paypoints, "Id", "Name");

        //    return View(model);
        //}

        public ActionResult patientProfile(Guid dependentId, Guid pro)
        {
            var model = _member.GetProfile(dependentId);

            //model.attachment = new Attachments();
            //model.attachment.dependentID = dependentId;
            //model.attachments = _member.GetAttachments(dependentId);

            model.patientPlanHistory = new PatientPlanHistory();
            model.patientPlanHistory.dependantId = dependentId;

            model.payPointHistory = new PayPointHistory();
            model.payPointHistory.dependantId = dependentId;

            //model.authLetters = _member.GetAuthorizationLetters(dependentId);
            model.doctorQuestionnaireResults = _member.GetDoctorQuestionResultsById(dependentId);

            model.disclaimerViewModel = new disclaimerViewModel();
            model.disclaimerViewModels = _member.GetDisclaimerViewResults(dependentId);

            //model.patientDisclaimers = new List<PatientDisclaimer>();
            //model.patientDisclaimers = _clinical.GetPatientDisclaimerResults(dependentId);

            //model.disclaimerViewModels = _member.GetDisclaimerViewDistinct(dependentId);
            //model.disclaimerResults = _member.GetDisclaimerViewList(dependentId);

            //ViewBag.attachmentType = new SelectList(_member.GetAttachmentTypes(), "attachmentType", "typeDescription");
            ViewBag.benefitPlan = new SelectList(model.patientPlans, "Id", "Name");
            ViewBag.paypoints = new SelectList(model.paypoints, "Id", "Name");

            return View(model);
        }

        [HttpGet]
        public ActionResult _DiabetesQuestionnaire_Details(int id)
        {
            var model = _member.GetFullQuestionnaireById(id);
            return View(model);
        }


        //=========================================================================== hospitalisations ===========================================================================//
        public ActionResult Hospitalisations_Create(Guid DependentID)
        {
            var model = new HospitalizationAuths();

            var memberinfo = _member.GetDependentDetails(DependentID, null);

            model.membershipNo = memberinfo.member.membershipNo;
            model.dependantCode = memberinfo.dependent.dependentCode;
            model.medicalAid = memberinfo.MedicalAids[0].medicalAidCode;

            return View(model);
        }
        [HttpPost]
        public ActionResult Hospitalisations_Create(HospitalizationAuths model)
        {

            model.createdDate = DateTime.Now;
            model.programType = "GEN";
            model.Active = true;
            var hospitalInsert = _member.InsertHospitalizationAuths(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = Request.Query["DependentID"], pro = Request.Query["pro"] });

        }

        public ActionResult Hospitalisations_Edit(int id)
        {
            var model = new HospitalAuthViewModel();

            model.hospitalizationAuth = _member.GetHospitalizationAuthByID(id);
            var dep = _member.GetDependantByMembershipDepCodeAidCode(model.hospitalizationAuth.membershipNo, model.hospitalizationAuth.dependantCode, model.hospitalizationAuth.medicalAid);
            model.dependantID = dep[0].DependantID;

            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName", model.hospitalizationAuth.programType);

            return View(model);
        }
        [HttpPost]
        public ActionResult Hospitalisations_Edit(HospitalAuthViewModel model)
        {
            try
            {
                ViewBag.programType = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");

                var result = _member.UpdateHospitalizationAuth(model.hospitalizationAuth);

                return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependantID, pro = Request.Query["pro"] });
            }
            catch
            {
                ViewBag.hospitalizationAuth.programType = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");

                return View();
            }

        }

        public ActionResult Hospitalisations_Details(int id)
        {

            var model = new HospitalAuthViewModel();
            model.hospitalizationAuth = _member.GetHospitalizationAuthByID(id);

            var dep = _member.GetDependantByMembershipDepCodeAidCode(model.hospitalizationAuth.membershipNo, model.hospitalizationAuth.dependantCode, model.hospitalizationAuth.medicalAid);

            model.dependantID = dep[0].DependantID;

            return View(model);
        }

        //=============================================================================== podiatry ===============================================================================//

        public ActionResult podiatry_Create(Guid DependentID)
        {
            var model = new Podiatry();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult podiatry_Create(Podiatry model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.programType = "DIABD";
            model.active = true;

            if (Request.Query["amputationComment"].ToString() != null)
                model.amputationComment = Request.Query["amputationComment"].ToString();
            if (Request.Query["amputationReason"].ToString() != null)
                model.amputationReason = Request.Query["amputationReason"].ToString();
            if (Request.Query["podiatryLopsComment"].ToString() != null)
                model.podiatryLopsComment = Request.Query["podiatryLopsComment"].ToString();
            if (Request.Query["podiatryDeformityComment"].ToString() != null)
                model.podiatryDeformityComment = Request.Query["podiatryDeformityComment"].ToString();
            if (Request.Query["podiatryPerArterialDiseaseComment"].ToString() != null)
                model.podiatryPerArterialDiseaseComment = Request.Query["podiatryPerArterialDiseaseComment"].ToString();
            if (Request.Query["podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                model.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatryPerArterialDiseaseAffectedLeg"].ToString();
            if (Request.Query["podiatryWoundComment"].ToString() != null)
                model.podiatryWoundComment = Request.Query["podiatryWoundComment"].ToString();
            if (Request.Query["podiatryWoundAffectedLeg"].ToString() != null)
                model.podiatryWoundAffectedLeg = Request.Query["podiatryWoundAffectedLeg"].ToString();
            if (Request.Query["programType"].ToString() != null)
                model.programType = Request.Query["programType"].ToString();
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.InsertPodiatryResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult podiatry_Edit(int id)
        {
            var model = _member.GetPodiatryResultById(id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            return View(model);
        }
        [HttpPost]
        public ActionResult podiatry_Edit(Podiatry model)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            if (Request.Query["amputaionComment_Concat"].ToString() != null)
                model.amputaionComment_Concat = Request.Query["amputaionComment_Concat"].ToString().Split(',');
            if (Request.Query["amputaionReason_Concat"].ToString() != null)
                model.amputaionReason_Concat = Request.Query["amputaionReason_Concat"].ToString().Split(',');
            if (Request.Query["podiatryLops_Concat"].ToString() != null)
                model.podiatryLops_Concat = Request.Query["podiatryLops_Concat"].ToString().Split(',');
            if (Request.Query["podiatryDeformity_Concat"].ToString() != null)
                model.podiatryDeformity_Concat = Request.Query["podiatryDeformity_Concat"].ToString().Split(',');
            if (Request.Query["PerArterialDisease_Concat"].ToString() != null)
                model.PerArterialDisease_Concat = Request.Query["PerArterialDisease_Concat"].ToString().Split(',');
            if (Request.Query["podiatryPerArterialDiseaseAffectedLeg"].ToString() != null)
                model.podiatryPerArterialDiseaseAffectedLeg = Request.Query["podiatryPerArterialDiseaseAffectedLeg"].ToString();
            if (Request.Query["podiatryWound_Concat"].ToString() != null)
                model.podiatryWound_Concat = Request.Query["podiatryWound_Concat"].ToString().Split(',');
            if (Request.Query["podiatryWoundAffectedLeg"].ToString() != null)
                model.podiatryWoundAffectedLeg = Request.Query["podiatryWoundAffectedLeg"].ToString();
            if (Request.Query["programType"].ToString() != null)
                model.programType = Request.Query["programType"].ToString();
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.UpdatePodiatryResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult podiatry_Details(int id)
        {
            var model = _member.GetPodiatryResultById(id);

            return View(model);
        }

        //================================================================================ autoNero ================================================================================//

        public ActionResult autoNero_Create(Guid DependentID)
        {
            var model = new AutoNeropathy();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult autoNero_Create(AutoNeropathy model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.programType = "DIABD";
            model.active = true;

            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.InsertAutoNeroResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult autoNero_Edit(int id)
        {
            var model = _member.GetAutoNeroResultById(id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            return View(model);
        }
        [HttpPost]
        public ActionResult autoNero_Edit(AutoNeropathy model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            if (Request.Query["autoNeuroSymptom_Concat"].ToString() != null)
                model.autoNeuroSymptom_Concat = Request.Query["autoNeuroSymptom_Concat"].ToString().Split(',');
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();


            var result = _member.UpdateAutoNeroResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult autoNero_Details(int id)
        {
            var model = _member.GetAutoNeroResultById(id);

            return View(model);
        }

        //================================================================================= vision =================================================================================//

        public ActionResult vision_Create(Guid DependentID)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new Vision();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult vision_Create(Vision model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.programType = "DIABD";
            model.active = true;

            if (Request.Query["vision"].ToString() != null)
                model.vision = Request.Query["vision"].ToString();
            if (Request.Query["eyeTreatment"].ToString() != null)
                model.eyeTreatment = Request.Query["eyeTreatment"].ToString();
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.InsertVisionResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult vision_Edit(int id)
        {
            var model = _member.GetVisionResultById(id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            return View(model);
        }
        [HttpPost]
        public ActionResult vision_Edit(Vision model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            if (Request.Query["vision_Concat"].ToString() != null)
                model.vision_Concat = Request.Query["vision_Concat"].ToString().Split(',');
            if (Request.Query["eyeTreat_Concat"].ToString() != null)
                model.eyeTreat_Concat = Request.Query["eyeTreat_Concat"].ToString().Split(',');
            if (Request.Query["programType"].ToString() != null)
                model.programType = Request.Query["programType"].ToString(); //HCare-607
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.UpdateVisionResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult vision_Details(int id)
        {
            var model = _member.GetVisionResultById(id);

            return View(model);
        }

        //============================================================================= hypoGlycaemia =============================================================================//

        public ActionResult hypoGlycaemia_Create(Guid DependentID)
        {
            var model = new Hypoglycaemia();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult hypoGlycaemia_Create(Hypoglycaemia model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.programType = "DIABD";
            model.active = true;

            if (Request.Query["hypoglycaemiaSymptomsComment"].ToString() != null)
                model.hypoglycaemiaSymptomsComment = Request.Query["hypoglycaemiaSymptomsComment"].ToString();
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.InsertHypoglycaemiaResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult hypoGlycaemia_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            var glucoseFrequency = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "• None/Geen", Value = "• None/Geen"},
                new SelectListItem() { Text = "• Daily/Daagliks", Value = "• Daily/Daagliks"},
                new SelectListItem() { Text = "• Weekly/Weekliks", Value = "• Weekly/Weekliks"},
                new SelectListItem() { Text = "• Monthly/Maandeliks", Value = "• Monthly/Maandeliks"},
                new SelectListItem() { Text = "• Follow up", Value = "• Follow up"}
            };
            ViewBag.glucose = glucoseFrequency;

            var model = _member.GetHypoglycaemiaResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult hypoGlycaemia_Edit(Hypoglycaemia model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");

            var glucoseFrequency = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "• None/Geen", Value = "• None/Geen"},
                new SelectListItem() { Text = "• Daily/Daagliks", Value = "• Daily/Daagliks"},
                new SelectListItem() { Text = "• Weekly/Weekliks", Value = "• Weekly/Weekliks"},
                new SelectListItem() { Text = "• Monthly/Maandeliks", Value = "• Monthly/Maandeliks"},
                new SelectListItem() { Text = "• Follow up", Value = "• Follow up"}
            };
            ViewBag.glucose = glucoseFrequency;

            if (Request.Query["hypoglycaemiaSymptoms_Concat"].ToString() != null)
                model.hypoglycaemiaSymptoms_Concat = Request.Query["hypoglycaemiaSymptoms_Concat"].ToString().Split(',');
            if (Request.Query["programType"].ToString() != null)
                model.programType = Request.Query["programType"].ToString(); //HCare-607
            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.UpdateHypoglycaemiaResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult hypoGlycaemia_Details(int id)
        {
            var model = _member.GetHypoglycaemiaResultById(id);

            return View(model);
        }

        //============================================================================== questionOther ==============================================================================//

        public ActionResult questionOther_Create(Guid DependentID)
        {
            var model = new QuestionnaireOther();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult questionOther_Create(QuestionnaireOther model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.active = true;

            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.InsertQuestionnaireOtherResults(model);

            return RedirectToAction("patientClinical", new { DependentID = model.dependentID });

        }

        public ActionResult questionOther_Edit(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult questionOther_Edit(QuestionnaireOther model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            if (Request.Query["generalComments"].ToString() != null)
                model.generalComments = Request.Query["generalComments"].ToString();

            var result = _member.UpdateQuestionnaireOtherResult(model);

            return RedirectToAction("patientClinical", new { DependentID = model.dependentID });

        }

        public ActionResult questionOther_Details(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }

        //================================================================================= diabetes-other =================================================================================//

        public ActionResult diabetesOther_Create(Guid DependentID)
        {
            var model = new QuestionnaireOther();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult diabetesOther_Create(QuestionnaireOther model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.occupation = null;
            model.shiftWorker = null;
            model.generalComments = null;
            model.recDrugs = false;
            model.recDrugsLastUsed = null;
            model.TBScreen = false;
            model.TBScreenResult = null;
            model.TBScreenDate = null;
            model.tbDiagnosed = false;
            model.tbTreatmentStartDate = null;
            model.tbTreatmentEndDate = null;
            model.Pregnant = false;
            model.EDD = null;
            model.frailCareCheck = false;
            model.frailCare = "None";
            model.frailCareComment = null;
            model.bloodTestFrequency = null;
            model.bloodTestEffectiveDate = null;

            if (!string.IsNullOrEmpty(Request.Query["depressionComment"].ToString()))
                model.depressionComment = Request.Query["depressionComment"].ToString();

            model.programType = "DIABD";
            model.followUp = false;
            model.active = true;

            var result = _member.InsertQuestionnaireOtherResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult diabetesOther_Edit(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult diabetesOther_Edit(QuestionnaireOther model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            model.programType = program.code;

            var result = _member.UpdateQuestionnaireOtherResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult diabetesOther_Details(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }

        //================================================================================= hivOther =================================================================================//

        public ActionResult hivOther_Create(Guid DependentID)
        {
            var model = new QuestionnaireOther();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult hivOther_Create(QuestionnaireOther model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.occupation = null;
            model.shiftWorker = null;
            model.generalComments = null;
            model.recDrugs = false;
            model.recDrugsLastUsed = null;
            model.TBScreen = false;
            model.TBScreenResult = null;
            model.TBScreenDate = null;
            model.tbDiagnosed = false;
            model.tbTreatmentStartDate = null;
            model.tbTreatmentEndDate = null;
            model.Pregnant = false;
            model.EDD = null;
            model.frailCareCheck = false;
            model.frailCare = "None";
            model.frailCareComment = null;
            model.bloodTestFrequency = null;
            model.bloodTestEffectiveDate = null;

            if (!string.IsNullOrEmpty(Request.Query["depressionComment"].ToString()))
                model.depressionComment = Request.Query["depressionComment"].ToString();

            model.programType = "HIVPR";
            model.followUp = false;
            model.active = true;

            var result = _member.InsertQuestionnaireOtherResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult hivOther_Edit(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult hivOther_Edit(QuestionnaireOther model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            model.programType = program.code;

            var result = _member.UpdateQuestionnaireOtherResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult hivOther_Details(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }

        //================================================================================ general-other ================================================================================//


        //public ActionResult generalOther_Create(Guid DependentID)
        //{
        //    var model = new QuestionnaireOther();
        //    model.dependentID = DependentID;

        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult generalOther_Create(QuestionnaireOther model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.createdBy = User.Identity.Name;
        //        model.createdDate = DateTime.Now;
        //        model.programType = "GEN";
        //        model.active = true;

        //        if (Request.Query["frailCare"].ToString() != null)
        //            model.frailCare = Request.Query["frailCare"].ToString();

        //        var result = _member.InsertQuestionnaireOtherResults(model);

        //        return RedirectToAction("patientClinical", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        //    }
        //    else
        //    {
        //        return View(model);
        //    }

        //}

        //public ActionResult generalOther_Edit(int id)
        //{
        //    var model = _member.GetQuestionnaireOtherResultById(id);
        //    //ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName", model.programType);

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult generalOther_Edit(QuestionnaireOther model)
        //{
        //    model.modifiedBy = User.Identity.Name;
        //    model.modifiedDate = DateTime.Now;

        //    if (Request.Query["frailCare_Concat"].ToString() != null)
        //        model.frailCare_Concat = Request.Query["frailCare_Concat"].ToString().Split(',');

        //    var result = _member.UpdateQuestionnaireOtherResult(model);

        //    return RedirectToAction("patientClinical", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        //}

        //public ActionResult generalOther_Details(int id)
        //{
        //    var model = _member.GetQuestionnaireOtherResultById(id);

        //    return View(model);
        //}

        public ActionResult Clinical_General_Other_Create(Guid DependentID)
        {

            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new QuestionnaireOther();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult Clinical_General_Other_Create(QuestionnaireOther model)
        {
            if (model.bloodTestEffectiveDate == DateTime.Today)
            {
                ViewBag.futuredateError = "Date cannot be set to future date";
                return RedirectToAction("Clinical_General_Other_Create");
            }
            //HCare-968
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.Pregnant = false;
            model.EDD = null;
            model.TreatingDoctor = null;
            model.depression = false;
            model.depressionComment = null;
            model.programType = "GEN";
            model.active = true;

            if (!string.IsNullOrEmpty(Request.Query["occupation"].ToString())) { model.occupation = Request.Query["occupation"]; }
            if (!string.IsNullOrEmpty(Request.Query["shiftWorker"].ToString())) { model.shiftWorker = Request.Query["shiftWorker"]; }
            if (!string.IsNullOrEmpty(Request.Query["recDrugsLastUsed"].ToString())) { model.recDrugsLastUsed = Convert.ToDateTime(Request.Query["recDrugsLastUsed"]); }
            if (!string.IsNullOrEmpty(Request.Query["TBScreenResult"].ToString())) { model.TBScreenResult = Request.Query["TBScreenResult"]; }
            if (!string.IsNullOrEmpty(Request.Query["TBScreenDate"].ToString())) { model.TBScreenDate = Convert.ToDateTime(Request.Query["TBScreenDate"]); }
            if (!string.IsNullOrEmpty(Request.Query["tbTreatmentStartDate"].ToString())) { model.tbTreatmentStartDate = Convert.ToDateTime(Request.Query["tbTreatmentStartDate"]); }
            if (!string.IsNullOrEmpty(Request.Query["tbTreatmentEndDate"].ToString())) { model.tbTreatmentEndDate = Convert.ToDateTime(Request.Query["tbTreatmentEndDate"]); }
            if (Request.Query["frailCare"].ToString() != null) { model.frailCare = Request.Query["frailCare"]; }
            if (!string.IsNullOrEmpty(Request.Query["frailCareComment"].ToString())) { model.frailCareComment = Request.Query["frailCareComment"]; }
            if (!string.IsNullOrEmpty(Request.Query["bloodTestFrequency"].ToString())) { model.bloodTestFrequency = Request.Query["bloodTestFrequency"]; }
            if (!string.IsNullOrEmpty(Request.Query["bloodTestEffectiveDate"].ToString())) { model.bloodTestEffectiveDate = Convert.ToDateTime(Request.Query["bloodTestEffectiveDate"]); }
            if (!string.IsNullOrEmpty(Request.Query["generalComments"].ToString())) { model.generalComments = Request.Query["generalComments"]; }

            var result = _member.InsertQuestionnaireOtherResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult Clinical_General_Other_Edit(int id)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult Clinical_General_Other_Edit(QuestionnaireOther model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            if (Request.Query["frailCare_Concat"].ToString() != null)
                model.frailCare_Concat = Request.Query["frailCare_Concat"].ToString().Split(',');

            var result = _member.UpdateQuestionnaireOtherResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult Clinical_General_Other_Details(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }

        //================================================================================ general-pregnant ================================================================================//
        //HCare-968
        public ActionResult Clinical_General_Pregnant_Create(Guid DependentID)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new QuestionnaireOther();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult Clinical_General_Pregnant_Create(QuestionnaireOther model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.occupation = null;
            model.shiftWorker = null;
            model.recDrugs = false;
            model.recDrugsLastUsed = null;
            model.TBScreen = false;
            model.TBScreenResult = null;
            model.TBScreenDate = null;
            model.tbDiagnosed = false;
            model.tbTreatmentStartDate = null;
            model.tbTreatmentEndDate = null;
            model.frailCareCheck = false;
            model.frailCare = "None";
            model.frailCareComment = null;
            model.bloodTestFrequency = null;
            model.bloodTestEffectiveDate = null;
            model.depression = false;
            model.depressionComment = null;
            model.programType = program.code;
            model.followUp = false;
            model.active = true;

            #region pregnant-insert
            //HCare-968
            if (model.Pregnant == true)
            {
                if (!string.IsNullOrEmpty(Request.Query["EDD"].ToString())) { model.EDD = Convert.ToDateTime(Request.Query["EDD"]); }
                if (!string.IsNullOrEmpty(Request.Query["TreatingDoctor"].ToString())) { model.TreatingDoctor = Request.Query["TreatingDoctor"]; }
            }

            if (!string.IsNullOrEmpty(Request.Query["generalComments"].ToString())) { model.generalComments = Request.Query["generalComments"]; }

            #endregion

            var result = _member.InsertQuestionnaireOtherResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult Clinical_General_Pregnant_Edit(int id)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult Clinical_General_Pregnant_Edit(QuestionnaireOther model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;
            model.programType = program.code;

            #region pregnant-insert
            //HCare-968
            if (!string.IsNullOrEmpty(Request.Query["EDD"].ToString())) { model.EDD = Convert.ToDateTime(Request.Query["EDD"]); }
            if (!string.IsNullOrEmpty(Request.Query["TreatingDoctor"].ToString())) { model.TreatingDoctor = Request.Query["TreatingDoctor"]; }
            if (!string.IsNullOrEmpty(Request.Query["generalComments"].ToString())) { model.generalComments = Request.Query["generalComments"]; }
            #endregion

            var result = _member.UpdateQuestionnaireOtherResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult Clinical_General_Pregnant_Details(int id)
        {
            var model = _member.GetQuestionnaireOtherResultById(id);

            return View(model);
        }
        //HCare-968

        //================================================================================ general-newborn ================================================================================//
        //HCare-968
        public ActionResult Clinical_General_NewBorn_Create(Guid DependentID)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new NewBorn();
            model.DependantID = DependentID;

            return View(model);
        }
        [HttpPost]

        public ActionResult Clinical_General_NewBorn_Create(NewBorn model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.CreatedDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;
            model.Program = program.code;
            model.Active = true;

            #region newborn-insert
            if (Request.Query["nb-concern"].ToString().ToLower().Contains("yes"))
            {
                model.hasConcern = true;
                model.Concern = Request.Query["newborn-concern"];
            }
            if (Request.Query["nb-breastfeeding"].ToString().ToLower().Contains("yes"))
            {
                model.isBreastfeeding = true;
                model.Breastfeeding = Request.Query["newborn-breastfeeding"];
            }
            if (Request.Query["nb-medication"].ToString().ToLower().Contains("yes"))
            {
                model.isOnMedication = true;
                model.Medication = Request.Query["newborn-medication"];
            }
            if (Request.Query["nb-hiv-test"].ToString().ToLower().Contains("yes"))
            {
                model.hivTest = true;
                model.HIVTestComment = Request.Query["newborn-hiv-test"];
            }
            if (Request.Query["nb-hiv-result"].ToString().ToLower().Contains("yes"))
            {
                model.hivResults = true;
                model.HIVResultsComment = Request.Query["newborn-hiv-result"];
            }
            if (!String.IsNullOrEmpty(Request.Query["newborn-general-comment"]))
            {
                model.GeneralComments = Request.Query["newborn-general-comment"];
            }
            #endregion

            var result = _member.InsertNewBornResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });
        }

        public ActionResult Clinical_General_NewBorn_Edit(int id) //hcare-1451
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = _member.GetNewBornResultById(id);
            if (model.hasConcern == true) { ViewBag.Concern = "Yes"; } else { ViewBag.Concern = "No"; }
            if (model.isBreastfeeding == true) { ViewBag.Breastfeeding = "Yes"; } else { ViewBag.Breastfeeding = "No"; }
            if (model.isOnMedication == true) { ViewBag.Medication = "Yes"; } else { ViewBag.Medication = "No"; }
            if (model.hivTest == true) { ViewBag.HIVTest = "Yes"; } else { ViewBag.HIVTest = "No"; }
            if (model.hivResults == true) { ViewBag.HIVResults = "Yes"; } else { ViewBag.HIVResults = "No"; }

            return View(model);
        }
        [HttpPost]
        public ActionResult Clinical_General_NewBorn_Edit(NewBorn model) //hcare-1451
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;
            model.Program = program.code;

            #region newborn-insert
            if (Request.Query["nb-concern"].ToString().ToLower().Contains("yes"))
            {
                model.hasConcern = true;
                model.Concern = Request.Query["newborn-concern"];
            }
            if (Request.Query["nb-breastfeeding"].ToString().ToLower().Contains("yes"))
            {
                model.isBreastfeeding = true;
                model.Breastfeeding = Request.Query["newborn-breastfeeding"];
            }
            if (Request.Query["nb-medication"].ToString().ToLower().Contains("yes"))
            {
                model.isOnMedication = true;
                model.Medication = Request.Query["newborn-medication"];
            }
            if (Request.Query["nb-hiv-test"].ToString().ToLower().Contains("yes"))
            {
                model.hivTest = true;
                model.HIVTestComment = Request.Query["newborn-hiv-test"];
            }
            if (Request.Query["nb-hiv-result"].ToString().ToLower().Contains("yes"))
            {
                model.hivResults = true;
                model.HIVResultsComment = Request.Query["newborn-hiv-result"];
            }
            if (!String.IsNullOrEmpty(Request.Query["newborn-general-comment"]))
            {
                model.GeneralComments = Request.Query["newborn-general-comment"];
            }
            #endregion

            var result = _member.UpdateNewBornResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });
        }

        public ActionResult Clinical_General_NewBorn_Details(int id)
        {
            var model = _member.GetNewBornResultById(id);

            return View(model);
        }
        //HCare-968

        //================================================================================ socialRecord ================================================================================//

        public ActionResult Create_socialRecord(Guid DependentID)
        {
            var model = new ClinicalHistoryQuestionaire();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult Create_socialRecord(ClinicalHistoryQuestionaire model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.active = true;
            model.programType = "GEN";

            //if (Request.Query["programType"].ToString() != null)
            //    model.programType = Request.Query["programType"].ToString(); //HCare-607
            if (Request.Query["socialComment"].ToString() != null)
                model.socialComment = Request.Query["socialComment"].ToString();

            var result = _member.InsertClinicalHistoryQuestionnaire(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult Edit_socialRecord(int id)
        {
            var model = _member.GetSocialRecord(id);
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName", model.programType);

            return View(model);

        }
        [HttpPost]
        public ActionResult Edit_socialRecord(ClinicalHistoryQuestionaire model)
        {
            try
            {
                model.modifieddate = DateTime.Now;
                model.modifiedBy = User.Identity.Name;

                if (Request.Query["programType"].ToString() != null)
                    model.programType = Request.Query["programType"].ToString(); //HCare-607
                if (Request.Query["socialComment"].ToString() != null)
                    model.socialComment = Request.Query["socialComment"].ToString();

                var result = _member.UpdateSocialRecord(model);

                return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
            }
            catch
            {

                return View();
            }

        }

        public ActionResult Details_socialRecord(int id)
        {
            var model = _member.GetSocialRecord(id);
            return View(model);

        }

        //================================================================================= disclaimer =================================================================================//


        public ActionResult disclaimer_Create(Guid DependentID)
        {
            var model = new PatientQuestionnaireResponse();
            model.DependantId = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult disclaimer_Create(PatientQuestionnaireResponse model)
        {
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.Active = true;

            var result = _member.InsertDisclaimerResults(model);

            return RedirectToAction("patientProfile", new { DependentID = model.DependantId });

        }

        public ActionResult disclaimer_Edit(int id)
        {
            var model = _member.GetDisclaimerResultsById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult disclaimer_Edit(PatientQuestionnaireResponse model)
        {
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            var result = _member.UpdateDisclaimerResults(model);

            return RedirectToAction("patientProfile", new { DependentID = model.DependantId });

        }


        //================================================================================= diagnosis =================================================================================//


        //HCare-607
        public ActionResult diagnosis_Create(Guid DependentID)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            var model = new PatientDiagnosis();
            ViewBag.program = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");
            model.DependantID = DependentID;

            model.ProgramCode = programCode.code;
            model.ProgramDescription = programCode.ProgramName;

            return View(model);
        }
        [HttpPost]
        public ActionResult diagnosis_Create(PatientDiagnosis model)
        {
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;
            model.EffectiveDate = Convert.ToDateTime(Request.Query["EffectiveDate"]);
            model.EffectiveDate = new DateTime(model.EffectiveDate.Value.Year, model.EffectiveDate.Value.Month, model.EffectiveDate.Value.Day, model.CreatedDate.Hour, model.CreatedDate.Minute, model.CreatedDate.Second); //hcare-863

            model.Comment = Request.Query["generalComments"].ToString();
            model.Active = true;
            model.Medication = Request.Query["medication-registration"].ToString(); //hcare-863

            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.ProgramCode = program.code;
            model.ProgramDescription = program.ProgramName;
            model.ICD10Code = program.icd10code;

            var result = _member.InsertDiagnosisResults(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        //HCare-607
        public ActionResult diagnosis_Edit(int id)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();

            var model = _member.GetDiagnosisById(id);
            ViewBag.programDescription = new SelectList(_member.GetPrograms(), "ProgramName", "ProgramName");

            model.ProgramCode = programCode;

            return View(model);

        }
        [HttpPost]
        public ActionResult diagnosis_Edit(PatientDiagnosis model)
        {
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (model.EffectiveDate != null) { model.EffectiveDate = Convert.ToDateTime(Request.Query["diagnosisDate"].ToString()); }
            model.EffectiveDate = new DateTime(model.EffectiveDate.Value.Year, model.EffectiveDate.Value.Month, model.EffectiveDate.Value.Day, model.ModifiedDate.Value.Hour, model.ModifiedDate.Value.Minute, model.ModifiedDate.Value.Second); //hcare-863
            if (!String.IsNullOrEmpty(Request.Query["Comment"].ToString())) { model.Comment = Request.Query["Comment"].ToString(); } else { model.Comment = null; } //hcare-863
            model.Medication = Request.Query["Medication"].ToString(); //hcare-863

            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.ProgramCode = program.code;
            model.ICD10Code = program.icd10code;

            var result = _member.UpdateDiagnosisResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        //HCare-607
        public ActionResult diagnosis_Details(int id)
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault();
            var model = _member.GetDiagnosisById(id);

            model.ProgramCode = programCode;

            return View(model);

        }

        //============================================================================= otherMedicalHistory =============================================================================//

        public ActionResult otherMedicalHistory_Create(Guid DependentID)
        {
            var model = new OtherMedicalHistory();
            model.dependentID = DependentID;

            return View(model);
        }
        [HttpPost]
        public ActionResult otherMedicalHistory_Create(OtherMedicalHistory model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.generalComments = Request.Query["generalComments"].ToString();
            model.programType = "HIVPR";
            model.active = true;

            var result = _member.InsertOtherMedicalHistory(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });

        }

        public ActionResult otherMedicalHistory_Edit(int id)
        {
            var model = _member.GetOtherMedicalHistoryById(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult otherMedicalHistory_Edit(OtherMedicalHistory model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            var result = _member.UpdateOtherMedicalHistory(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult otherMedicalHistory_Details(int id)
        {
            var model = _member.GetOtherMedicalHistoryById(id);

            return View(model);
        }

        public ActionResult MemberFileView(Guid DependentID)
        {
            var model = _member.GetDependentsBasicById(DependentID);

            return View(model);
        }

        public ActionResult MemberProgram(Guid DependentID) //HCare-1121
        {
            var model = new MemberProfileVM();
            model.MemberBasicViews = _member.GetPatientProgramHistoryById(DependentID);

            return View(model);
        }

        public ActionResult DateInformationPopUp()
        {
            return View();
        }

        #region mental-health-dsm5 hcare-1123

        public ActionResult MH_DSM5_Create(Guid DependentID, Guid pro)
        {
            var model = new MH_DSM5ResponseNEW();
            model.DependantID = DependentID;
            ViewBag.program = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_DSM5_Create(MH_DSM5ResponseNEW model)
        {
            var program = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;

            #region dsm5-insert
            model.DependantID = dependantid;
            model.TaskID = null;
            model.AssignmentID = null;
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;

            if (!String.IsNullOrEmpty(Request.Query["depression-one-text"]))
            {
                model.DepressionOne = Request.Query["depression-one-text"];
                model.DepressionOneSC = Convert.ToInt32(Request.Query["depression-one-score"]);
                totalScore = model.DepressionOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["depression-two-text"]))
            {
                model.DepressionTwo = Request.Query["depression-two-text"];
                model.DepressionTwoSC = Convert.ToInt32(Request.Query["depression-two-score"]);
                totalScore = model.DepressionTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anger-one-text"]))
            {
                model.AngerOne = Request.Query["anger-one-text"];
                model.AngerOneSC = Convert.ToInt32(Request.Query["anger-one-score"]);
                totalScore = model.AngerOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["mania-one-text"]))
            {
                model.ManiaOne = Request.Query["mania-one-text"];
                model.ManiaOneSC = Convert.ToInt32(Request.Query["mania-one-score"]);
                totalScore = model.ManiaOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["mania-two-text"]))
            {
                model.ManiaTwo = Request.Query["mania-two-text"];
                model.ManiaTwoSC = Convert.ToInt32(Request.Query["mania-two-score"]);
                totalScore = model.ManiaTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-one-text"]))
            {
                model.AnxietyOne = Request.Query["anxiety-one-text"];
                model.AnxietyOneSC = Convert.ToInt32(Request.Query["anxiety-one-score"]);
                totalScore = model.AnxietyOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-two-text"]))
            {
                model.AnxietyTwo = Request.Query["anxiety-two-text"];
                model.AnxietyTwoSC = Convert.ToInt32(Request.Query["anxiety-two-score"]);
                totalScore = model.AnxietyTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-three-text"]))
            {
                model.AnxietyThree = Request.Query["anxiety-three-text"];
                model.AnxietyThreeSC = Convert.ToInt32(Request.Query["anxiety-three-score"]);
                totalScore = model.AnxietyThreeSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["somatic-one-text"]))
            {
                model.SomaticOne = Request.Query["somatic-one-text"];
                model.SomaticOneSC = Convert.ToInt32(Request.Query["somatic-one-score"]);
                totalScore = model.SomaticOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["somatic-two-text"]))
            {
                model.SomaticTwo = Request.Query["somatic-two-text"];
                model.SomaticTwoSC = Convert.ToInt32(Request.Query["somatic-two-score"]);
                totalScore = model.SomaticTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["suicide-one-text"]))
            {
                model.SuicidalOne = Request.Query["suicide-one-text"];
                model.SuicidalOneSC = Convert.ToInt32(Request.Query["suicide-one-score"]);
                totalScore = model.SuicidalOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["psychosis-one-text"]))
            {
                model.PsychosisOne = Request.Query["psychosis-one-text"];
                model.PsychosisOneSC = Convert.ToInt32(Request.Query["psychosis-one-score"]);
                totalScore = model.PsychosisOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["psychosis-two-text"]))
            {
                model.PsychosisTwo = Request.Query["psychosis-two-text"];
                model.PsychosisTwoSC = Convert.ToInt32(Request.Query["psychosis-two-score"]);
                totalScore = model.PsychosisTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["sleep-one-text"]))
            {
                model.SleepOne = Request.Query["sleep-one-text"];
                model.SleepOneSC = Convert.ToInt32(Request.Query["sleep-one-score"]);
                totalScore = model.SleepOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["memory-one-text"]))
            {
                model.MemoryOne = Request.Query["memory-one-text"];
                model.MemoryOneSC = Convert.ToInt32(Request.Query["memory-one-score"]);
                totalScore = model.MemoryOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["behaviour-one-text"]))
            {
                model.BehaviourOne = Request.Query["behaviour-one-text"];
                model.BehaviourOneSC = Convert.ToInt32(Request.Query["behaviour-one-score"]);
                totalScore = model.BehaviourOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["behaviour-two-text"]))
            {
                model.BehaviourTwo = Request.Query["behaviour-two-text"];
                model.BehaviourTwoSC = Convert.ToInt32(Request.Query["behaviour-two-score"]);
                totalScore = model.BehaviourTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["dissociation-one-text"]))
            {
                model.DissociationOne = Request.Query["dissociation-one-text"];
                model.DissociationOneSC = Convert.ToInt32(Request.Query["dissociation-one-score"]);
                totalScore = model.DissociationOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["personality-one-text"]))
            {
                model.PersonalityOne = Request.Query["personality-one-text"];
                model.PersonalityOneSC = Convert.ToInt32(Request.Query["personality-one-score"]);
                totalScore = model.PersonalityOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["personality-two-text"]))
            {
                model.PersonalityTwo = Request.Query["personality-two-text"];
                model.PersonalityTwoSC = Convert.ToInt32(Request.Query["personality-two-score"]);
                totalScore = model.PersonalityTwoSC + totalScore;
            }

            model.TotalScore = totalScore;
            #region outcome
            if (model.SuicidalOneSC > 0)
            {
                model.Outcome = "Severe";
            }
            else if (totalScore < 1)
            {
                model.Outcome = "Normal";
            }
            else if (totalScore <= 2)
            {
                model.Outcome = "Mild";
            }
            else if (totalScore >= 3 && totalScore <= 5)
            {
                model.Outcome = "Moderate";
            }
            else if (totalScore >= 6 && totalScore <= 8)
            {
                model.Outcome = "Severe";
            }
            #endregion
            model.Comment = "Patient profile";
            model.Program = program.code;
            model.Active = true;


            #endregion
            #region dr-referral-assignmnet hcare-1137
            if (model.SuicidalOneSC > 0)
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
                        programId = program.programID,

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
            if (model.SuicidalOneSC != 0)
            {
                var risk = new PatientRiskRatingHistory();
                risk.effectiveDate = DateTime.Now;
                risk.dependantID = dependantid;
                risk.createdDate = DateTime.Now;
                risk.createdBy = User.Identity.Name;
                risk.programType = program.code;
                risk.active = true;

                risk.RiskId = "R";
                risk.reason = "DSM5 questionnaire assignment - Suicidal";
                _member.InsertRiskRating(risk);
                model.RiskID = risk.id;

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
                            programId = program.programID,
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
            dsm5score.Program = program.code;
            dsm5score.Active = true;

            var history = _admin.GetDSM5ScoreHistory(dependantid);
            var riskHistory = _member.GetPatientRiskRating(dependantid);

            if (history.Count > 0)
            {
                var lastDSM5Score = history.OrderByDescending(x => x.EffectiveDate).First();
                if (lastDSM5Score.Score < 25)
                {
                    if (model.SuicidalOneSC != 0)
                    {
                        var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                        if (latestRiskRating.RiskId == "R")
                        {
                            dsm5score.Score = model.TotalScore;
                            dsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }

                            #endregion
                        }
                        else
                        {
                            dsm5score.Score = model.TotalScore;
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion
                        }
                        _admin.InsertDSM5Score(dsm5score);

                    }
                    else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                    {
                        dsm5score.Score = model.TotalScore;
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
                            risk.programType = program.code;
                            risk.active = true;
                            risk.RiskId = latestRiskRating.RiskId;
                            risk.reason = "Patient Stable - DSM5 score";

                            _member.InsertRiskRating(risk);

                            dsm5score.RiskID = risk.id;
                            model.RiskID = risk.id;
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Degression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }

                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Progression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                }
                if (lastDSM5Score.Score >= 25 && lastDSM5Score.Score <= 55)
                {
                    if (model.SuicidalOneSC != 0)
                    {
                        var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                        if (latestRiskRating.RiskId == "R")
                        {
                            dsm5score.Score = model.TotalScore;
                            dsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }

                            #endregion
                        }
                        else
                        {
                            dsm5score.Score = model.TotalScore;
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion
                        }
                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                    {
                        dsm5score.Score = model.TotalScore;
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
                            risk.programType = program.code;
                            risk.active = true;
                            risk.RiskId = latestRiskRating.RiskId;
                            risk.reason = "Patient Stable - DSM5 score";

                            _member.InsertRiskRating(risk);

                            dsm5score.RiskID = risk.id;
                            model.RiskID = risk.id;
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Degression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }

                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Progression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                }
                if (lastDSM5Score.Score > 55)
                {
                    if (model.SuicidalOneSC != 0)
                    {
                        var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                        if (latestRiskRating.RiskId == "R")
                        {
                            dsm5score.Score = model.TotalScore;
                            dsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }

                            #endregion
                        }
                        else
                        {
                            dsm5score.Score = model.TotalScore;
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    dsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion
                        }
                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if ((model.TotalScore <= ((lastDSM5Score.Score * 4) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -4) / 100) + lastDSM5Score.Score))
                    {
                        dsm5score.Score = model.TotalScore;
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
                            risk.programType = program.code;
                            risk.active = true;
                            risk.RiskId = latestRiskRating.RiskId;
                            risk.reason = "Patient Stable - DSM5 score";

                            _member.InsertRiskRating(risk);

                            dsm5score.RiskID = risk.id;
                            model.RiskID = risk.id;
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore >= ((lastDSM5Score.Score * 5) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Degression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }

                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                    else if (model.TotalScore <= ((lastDSM5Score.Score * -5) / 100) + lastDSM5Score.Score)
                    {
                        dsm5score.Score = model.TotalScore;
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
                                risk.programType = program.code;
                                risk.active = true;

                                risk.RiskId = newRisk;
                                risk.reason = "Patient Progression - DSM5 score";
                                _member.InsertRiskRating(risk);
                                dsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                        }
                        #endregion

                        _admin.InsertDSM5Score(dsm5score);
                    }
                }
            }
            else
            {
                dsm5score.Score = model.TotalScore;
                dsm5score.Reason = "Baseline";

                _admin.InsertDSM5Score(dsm5score);
            }
            #endregion
            model.DSM5ScoreID = dsm5score.Id;
            var insert = _admin.InsertNEWDSM5Results(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult MH_DSM5_Edit(int id)
        {
            var model = _admin.GetNEWDSM5ById(id);
            ViewBag.Program = model.Program;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_DSM5_Edit(MH_DSM5ResponseNEW model)
        {
            var totalScore = 0;
            var program = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);

            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            #region dsm5-update
            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            if (!String.IsNullOrEmpty(Request.Query["depression-one-text"]))
            {
                model.DepressionOne = Request.Query["depression-one-text"];
                model.DepressionOneSC = Convert.ToInt32(Request.Query["depression-one-score"]);
                totalScore = model.DepressionOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["depression-two-text"]))
            {
                model.DepressionTwo = Request.Query["depression-two-text"];
                model.DepressionTwoSC = Convert.ToInt32(Request.Query["depression-two-score"]);
                totalScore = model.DepressionTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anger-one-text"]))
            {
                model.AngerOne = Request.Query["anger-one-text"];
                model.AngerOneSC = Convert.ToInt32(Request.Query["anger-one-score"]);
                totalScore = model.AngerOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["mania-one-text"]))
            {
                model.ManiaOne = Request.Query["mania-one-text"];
                model.ManiaOneSC = Convert.ToInt32(Request.Query["mania-one-score"]);
                totalScore = model.ManiaOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["mania-two-text"]))
            {
                model.ManiaTwo = Request.Query["mania-two-text"];
                model.ManiaTwoSC = Convert.ToInt32(Request.Query["mania-two-score"]);
                totalScore = model.ManiaTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-one-text"]))
            {
                model.AnxietyOne = Request.Query["anxiety-one-text"];
                model.AnxietyOneSC = Convert.ToInt32(Request.Query["anxiety-one-score"]);
                totalScore = model.AnxietyOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-two-text"]))
            {
                model.AnxietyTwo = Request.Query["anxiety-two-text"];
                model.AnxietyTwoSC = Convert.ToInt32(Request.Query["anxiety-two-score"]);
                totalScore = model.AnxietyTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["anxiety-three-text"]))
            {
                model.AnxietyThree = Request.Query["anxiety-three-text"];
                model.AnxietyThreeSC = Convert.ToInt32(Request.Query["anxiety-three-score"]);
                totalScore = model.AnxietyThreeSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["somatic-one-text"]))
            {
                model.SomaticOne = Request.Query["somatic-one-text"];
                model.SomaticOneSC = Convert.ToInt32(Request.Query["somatic-one-score"]);
                totalScore = model.SomaticOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["somatic-two-text"]))
            {
                model.SomaticTwo = Request.Query["somatic-two-text"];
                model.SomaticTwoSC = Convert.ToInt32(Request.Query["somatic-two-score"]);
                totalScore = model.SomaticTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["suicide-one-text"]))
            {
                model.SuicidalOne = Request.Query["suicide-one-text"];
                model.SuicidalOneSC = Convert.ToInt32(Request.Query["suicide-one-score"]);
                totalScore = model.SuicidalOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["psychosis-one-text"]))
            {
                model.PsychosisOne = Request.Query["psychosis-one-text"];
                model.PsychosisOneSC = Convert.ToInt32(Request.Query["psychosis-one-score"]);
                totalScore = model.PsychosisOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["psychosis-two-text"]))
            {
                model.PsychosisTwo = Request.Query["psychosis-two-text"];
                model.PsychosisTwoSC = Convert.ToInt32(Request.Query["psychosis-two-score"]);
                totalScore = model.PsychosisTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["sleep-one-text"]))
            {
                model.SleepOne = Request.Query["sleep-one-text"];
                model.SleepOneSC = Convert.ToInt32(Request.Query["sleep-one-score"]);
                totalScore = model.SleepOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["memory-one-text"]))
            {
                model.MemoryOne = Request.Query["memory-one-text"];
                model.MemoryOneSC = Convert.ToInt32(Request.Query["memory-one-score"]);
                totalScore = model.MemoryOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["behaviour-one-text"]))
            {
                model.BehaviourOne = Request.Query["behaviour-one-text"];
                model.BehaviourOneSC = Convert.ToInt32(Request.Query["behaviour-one-score"]);
                totalScore = model.BehaviourOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["behaviour-two-text"]))
            {
                model.BehaviourTwo = Request.Query["behaviour-two-text"];
                model.BehaviourTwoSC = Convert.ToInt32(Request.Query["behaviour-two-score"]);
                totalScore = model.BehaviourTwoSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["dissociation-one-text"]))
            {
                model.DissociationOne = Request.Query["dissociation-one-text"];
                model.DissociationOneSC = Convert.ToInt32(Request.Query["dissociation-one-score"]);
                totalScore = model.DissociationOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["personality-one-text"]))
            {
                model.PersonalityOne = Request.Query["personality-one-text"];
                model.PersonalityOneSC = Convert.ToInt32(Request.Query["personality-one-score"]);
                totalScore = model.PersonalityOneSC + totalScore;
            }

            if (!String.IsNullOrEmpty(Request.Query["personality-two-text"]))
            {
                model.PersonalityTwo = Request.Query["personality-two-text"];
                model.PersonalityTwoSC = Convert.ToInt32(Request.Query["personality-two-score"]);
                totalScore = model.PersonalityTwoSC + totalScore;
            }

            model.TotalScore = totalScore;
            #region outcome
            if (model.SuicidalOneSC > 0)
            {
                model.Outcome = "Severe";
            }
            else if (totalScore < 1)
            {
                model.Outcome = "Normal";
            }
            else if (totalScore <= 2)
            {
                model.Outcome = "Mild";
            }
            else if (totalScore >= 3 && totalScore <= 5)
            {
                model.Outcome = "Moderate";
            }
            else if (totalScore >= 6 && totalScore <= 8)
            {
                model.Outcome = "Severe";
            }
            #endregion
            model.Comment = "Patient profile - Edit";
            model.Program = program.code;
            #endregion
            #region dsm5-score + risk-rating
            var dsm5score = _admin.GetDSM5ScoreByID(Convert.ToInt32(model.DSM5ScoreID));
            if (dsm5score != null)
            {
                dsm5score.ModifiedDate = DateTime.Now;
                dsm5score.ModifiedBy = User.Identity.Name;
                dsm5score.Score = totalScore;
                dsm5score.Comment = "DSM5 questionnaire";

                if (model.Active == true)
                {
                    dsm5score.Active = true;
                }
                else
                {
                    dsm5score.Active = false;
                }

                var history = _admin.GetDSM5ScoreHistory(dependantid);
                var riskHistory = _member.GetPatientRiskRating(dependantid);

                if (history.Count > 0)
                {
                    var currentDSM5Score = _admin.GetDSM5ScoreByID(Convert.ToInt32(model.DSM5ScoreID));
                    var score = currentDSM5Score.Score;
                    var reason = currentDSM5Score.Reason;
                    var createddate = currentDSM5Score.CreatedDate;
                    var createdby = currentDSM5Score.CreatedBy;

                    var DSM5Score = history.Where(x => x.Id == model.DSM5ScoreID).First();
                    var baselineDSM5Score = history.OrderByDescending(x => x.EffectiveDate).First();
                    if (DSM5Score.Reason.ToLower().Contains("baseline"))
                    {
                        #region dsm5-score-history
                        var dsm5sh = new MH_DSM5ScoreHistory();
                        dsm5sh.DependantID = dependantid;
                        dsm5sh.CreatedDate = createddate;
                        dsm5sh.CreatedBy = createdby;
                        dsm5sh.OldScore = score;
                        dsm5sh.OldReason = reason;
                        dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                        dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                        dsm5sh.NewScore = dsm5score.Score;
                        dsm5sh.NewReason = dsm5score.Reason;
                        dsm5sh.RiskID = dsm5score.Id;
                        dsm5sh.Comment = "DSM5 edit - baseline";

                        _admin.InsertDSM5ScoreHistory(dsm5sh);
                        #endregion

                        dsm5score.Score = model.TotalScore;
                        _admin.UpdateDSM5Score(dsm5score);
                    }
                    else
                    {
                        var lastDSM5Score = history.OrderByDescending(x => x.EffectiveDate).Skip(1).First();
                        if (lastDSM5Score.Score < 25)
                        {
                            if (model.SuicidalOneSC != 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                if (latestRiskRating.RiskId == "R")
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Stable";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient stable";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion
                                }
                                else
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Degression";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient degression";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion

                                    #region risk-rating
                                    if (riskHistory.Count > 0)
                                    {
                                        var riskratingTypes = _member.GetRiskRatingTypes();

                                        var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                        if (lastRRinfo <= 2)
                                        {
                                            var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();
                                            var risk = _member.GetRiskRatingByID(model.RiskID);

                                            risk.modifiedDate = DateTime.Now;
                                            risk.modifiedBy = User.Identity.Name;
                                            risk.RiskId = newRisk;
                                            risk.reason = "Patient Degression - DSM5 score";
                                            _member.UpdateRiskRating(risk);
                                        }
                                    }

                                    #endregion
                                }

                                _admin.UpdateDSM5Score(dsm5score);

                            }
                            else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Stable";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient stable";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                var latestRiskRating = _member.GetRiskRatingByID(model.RiskID);
                                var previousRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();

                                latestRiskRating.modifiedDate = DateTime.Now;
                                latestRiskRating.modifiedBy = User.Identity.Name;
                                latestRiskRating.RiskId = previousRiskRating.RiskId;
                                latestRiskRating.reason = "Patient Stable - DSM5 score";
                                _member.UpdateRiskRating(latestRiskRating);
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient degression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();
                                        var risk = _member.GetRiskRatingByID(model.RiskID);

                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }

                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Progression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient progression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo >= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = _member.GetRiskRatingByID(model.RiskID);
                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Progression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                        }
                        if (lastDSM5Score.Score >= 25 && lastDSM5Score.Score <= 55)
                        {
                            if (model.SuicidalOneSC != 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                if (latestRiskRating.RiskId == "R")
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Stable";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient stable";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion
                                }
                                else
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Degression";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient degression";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion
                                    #region risk-rating
                                    if (riskHistory.Count > 0)
                                    {
                                        var riskratingTypes = _member.GetRiskRatingTypes();

                                        var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                        if (lastRRinfo <= 2)
                                        {
                                            var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();
                                            var risk = _member.GetRiskRatingByID(model.RiskID);

                                            risk.modifiedDate = DateTime.Now;
                                            risk.modifiedBy = User.Identity.Name;
                                            risk.RiskId = newRisk;
                                            risk.reason = "Patient Degression - DSM5 score";
                                            _member.UpdateRiskRating(risk);
                                        }
                                    }

                                    #endregion
                                }
                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Stable";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient stable";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                var latestRiskRating = _member.GetRiskRatingByID(model.RiskID);
                                var previousRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();

                                latestRiskRating.modifiedDate = DateTime.Now;
                                latestRiskRating.modifiedBy = User.Identity.Name;
                                latestRiskRating.RiskId = previousRiskRating.RiskId;
                                latestRiskRating.reason = "Patient Stable - DSM5 score";
                                _member.UpdateRiskRating(latestRiskRating);
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient degression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = _member.GetRiskRatingByID(model.RiskID);
                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }

                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Progression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient progression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo >= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = _member.GetRiskRatingByID(model.RiskID);
                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Progression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                        }
                        if (lastDSM5Score.Score > 55)
                        {
                            if (model.SuicidalOneSC != 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                if (latestRiskRating.RiskId == "R")
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Stable";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient stable";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion
                                }
                                else
                                {
                                    dsm5score.Score = model.TotalScore;
                                    dsm5score.Reason = "Degression";
                                    #region dsm5-score-history
                                    var dsm5sh = new MH_DSM5ScoreHistory();
                                    dsm5sh.DependantID = dependantid;
                                    dsm5sh.CreatedDate = createddate;
                                    dsm5sh.CreatedBy = createdby;
                                    dsm5sh.OldScore = score;
                                    dsm5sh.OldReason = reason;
                                    dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                    dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                    dsm5sh.NewScore = dsm5score.Score;
                                    dsm5sh.NewReason = dsm5score.Reason;
                                    dsm5sh.RiskID = dsm5score.Id;
                                    dsm5sh.Comment = "DSM5 edit - Patient degression";

                                    _admin.InsertDSM5ScoreHistory(dsm5sh);
                                    #endregion
                                    #region risk-rating
                                    if (riskHistory.Count > 0)
                                    {
                                        var riskratingTypes = _member.GetRiskRatingTypes();

                                        var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                        if (lastRRinfo <= 2)
                                        {
                                            var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();
                                            var risk = _member.GetRiskRatingByID(model.RiskID);

                                            risk.modifiedDate = DateTime.Now;
                                            risk.modifiedBy = User.Identity.Name;
                                            risk.RiskId = newRisk;
                                            risk.reason = "Patient Degression - DSM5 score";
                                            _member.UpdateRiskRating(risk);
                                        }
                                    }

                                    #endregion
                                }
                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if ((model.TotalScore <= ((lastDSM5Score.Score * 4) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -4) / 100) + lastDSM5Score.Score))
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Stable";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient stable";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                var latestRiskRating = _member.GetRiskRatingByID(model.RiskID);
                                var previousRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();

                                latestRiskRating.modifiedDate = DateTime.Now;
                                latestRiskRating.modifiedBy = User.Identity.Name;
                                latestRiskRating.RiskId = previousRiskRating.RiskId;
                                latestRiskRating.reason = "Patient Stable - DSM5 score";
                                _member.UpdateRiskRating(latestRiskRating);
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore >= ((lastDSM5Score.Score * 5) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Degression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient degression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).Skip(1).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo <= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo + 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = _member.GetRiskRatingByID(model.RiskID);
                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }

                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                            else if (model.TotalScore <= ((lastDSM5Score.Score * -5) / 100) + lastDSM5Score.Score)
                            {
                                dsm5score.Score = model.TotalScore;
                                dsm5score.Reason = "Progression";
                                #region dsm5-score-history
                                var dsm5sh = new MH_DSM5ScoreHistory();
                                dsm5sh.DependantID = dependantid;
                                dsm5sh.CreatedDate = createddate;
                                dsm5sh.CreatedBy = createdby;
                                dsm5sh.OldScore = score;
                                dsm5sh.OldReason = reason;
                                dsm5sh.ModifiedDate = dsm5score.ModifiedDate;
                                dsm5sh.ModifiedBy = dsm5score.ModifiedBy;
                                dsm5sh.NewScore = dsm5score.Score;
                                dsm5sh.NewReason = dsm5score.Reason;
                                dsm5sh.RiskID = dsm5score.Id;
                                dsm5sh.Comment = "DSM5 edit - Patient progression";

                                _admin.InsertDSM5ScoreHistory(dsm5sh);
                                #endregion
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();
                                    var riskratingTypes = _member.GetRiskRatingTypes();

                                    var lastRRinfo = Convert.ToInt32(riskratingTypes.Where(x => x.RiskType == latestRiskRating.RiskId).Select(x => x.RiskPriority).FirstOrDefault());

                                    if (lastRRinfo >= 2)
                                    {
                                        var newRisk = riskratingTypes.Where(x => x.RiskPriority == (lastRRinfo - 1).ToString()).Select(x => x.RiskType).FirstOrDefault();

                                        var risk = _member.GetRiskRatingByID(model.RiskID);
                                        risk.modifiedDate = DateTime.Now;
                                        risk.modifiedBy = User.Identity.Name;
                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Progression - DSM5 score";
                                        _member.UpdateRiskRating(risk);
                                    }
                                }
                                #endregion

                                _admin.UpdateDSM5Score(dsm5score);
                            }
                        }
                    }
                }
            }
            else
            {
                var newdsm5score = new MH_DSM5Score();
                newdsm5score.EffectiveDate = DateTime.Now;
                newdsm5score.DependantID = dependantid;
                newdsm5score.CreatedDate = DateTime.Now;
                newdsm5score.CreatedBy = User.Identity.Name;
                newdsm5score.Program = program.code;
                newdsm5score.Active = true;

                var history = _admin.GetDSM5ScoreHistory(dependantid);
                var riskHistory = _member.GetPatientRiskRating(dependantid);

                if (history.Count > 0)
                {
                    var lastDSM5Score = history.OrderByDescending(x => x.EffectiveDate).First();
                    if (lastDSM5Score.Score < 25)
                    {
                        if (model.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Stable";

                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = program.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Degression";
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
                                        risk.programType = program.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        newdsm5score.RiskID = risk.id;
                                        model.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(newdsm5score);

                        }
                        else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                newdsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Degression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Progression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                    }
                    if (lastDSM5Score.Score >= 25 && lastDSM5Score.Score <= 55)
                    {
                        if (model.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Stable";

                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = program.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Degression";
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
                                        risk.programType = program.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        newdsm5score.RiskID = risk.id;
                                        model.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if ((model.TotalScore <= ((lastDSM5Score.Score * 9) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -9) / 100) + lastDSM5Score.Score))
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Stable";
                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                newdsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore >= ((lastDSM5Score.Score * 10) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Degression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore <= ((lastDSM5Score.Score * -10) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Progression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                    }
                    if (lastDSM5Score.Score > 55)
                    {
                        if (model.SuicidalOneSC != 0)
                        {
                            var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                            if (latestRiskRating.RiskId == "R")
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Stable";
                                #region risk-rating
                                if (riskHistory.Count > 0)
                                {
                                    var risk = new PatientRiskRatingHistory();
                                    risk.effectiveDate = DateTime.Now;
                                    risk.dependantID = dependantid;
                                    risk.createdDate = DateTime.Now;
                                    risk.createdBy = User.Identity.Name;
                                    risk.programType = program.code;
                                    risk.active = true;
                                    risk.RiskId = latestRiskRating.RiskId;
                                    risk.reason = "Patient Stable - DSM5 score";

                                    _member.InsertRiskRating(risk);

                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }

                                #endregion
                            }
                            else
                            {
                                newdsm5score.Score = model.TotalScore;
                                newdsm5score.Reason = "Degression";
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
                                        risk.programType = program.code;
                                        risk.active = true;

                                        risk.RiskId = newRisk;
                                        risk.reason = "Patient Degression - DSM5 score";
                                        _member.InsertRiskRating(risk);
                                        newdsm5score.RiskID = risk.id;
                                        model.RiskID = risk.id;
                                    }
                                }

                                #endregion
                            }
                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if ((model.TotalScore <= ((lastDSM5Score.Score * 4) / 100) + lastDSM5Score.Score) && (model.TotalScore >= ((lastDSM5Score.Score * -4) / 100) + lastDSM5Score.Score))
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Stable";

                            #region risk-rating
                            if (riskHistory.Count > 0)
                            {
                                var latestRiskRating = riskHistory.OrderByDescending(x => x.effectiveDate).First();

                                var risk = new PatientRiskRatingHistory();
                                risk.effectiveDate = DateTime.Now;
                                risk.dependantID = dependantid;
                                risk.createdDate = DateTime.Now;
                                risk.createdBy = User.Identity.Name;
                                risk.programType = program.code;
                                risk.active = true;
                                risk.RiskId = latestRiskRating.RiskId;
                                risk.reason = "Patient Stable - DSM5 score";

                                _member.InsertRiskRating(risk);

                                newdsm5score.RiskID = risk.id;
                                model.RiskID = risk.id;
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore >= ((lastDSM5Score.Score * 5) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Degression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Degression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }

                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                        else if (model.TotalScore <= ((lastDSM5Score.Score * -5) / 100) + lastDSM5Score.Score)
                        {
                            newdsm5score.Score = model.TotalScore;
                            newdsm5score.Reason = "Progression";
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
                                    risk.programType = program.code;
                                    risk.active = true;

                                    risk.RiskId = newRisk;
                                    risk.reason = "Patient Progression - DSM5 score";
                                    _member.InsertRiskRating(risk);
                                    newdsm5score.RiskID = risk.id;
                                    model.RiskID = risk.id;
                                }
                            }
                            #endregion

                            _admin.InsertDSM5Score(newdsm5score);
                        }
                    }
                }
                else
                {
                    newdsm5score.Score = model.TotalScore;
                    newdsm5score.Reason = "Baseline";

                    _admin.InsertDSM5Score(newdsm5score);
                }
                model.DSM5ScoreID = newdsm5score.Id;
            }
            #endregion
            #region dr-referral-assignmnet hcare-1137
            if (model.SuicidalOneSC > 0)
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
                        programId = program.programID,

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

            var result = _admin.UpdateNEWDSM5Result(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult MH_DSM5_Details(int id)
        {
            var model = _admin.GetNEWDSM5ById(id);

            return View(model);
        }

        public ActionResult MH_DSM5Score_Details(int id)
        {
            DSM5ScoreVM model = _admin.GetDSM5ScoreInformation(id);
            model.DSM5ScoreHistory = _admin.GetDSM5ScoreDetails(id);
            return View(model);
        }

        #endregion

        #region diabetes-care-plan hcare-1092
        public ActionResult diabetesCarePlanPathology_Create(Guid DependentID, Guid Pro)
        {
            var model = new CarePlanPathology();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult diabetesCarePlanPathology_Create(CarePlanPathology model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertCarePathology(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult diabetesCarePlanPathology_Details(int id)
        {
            var model = _member.GetCarePathologyByID(id);
            return View(model);
        }

        public ActionResult diabetesCarePlanPathology_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetCarePathologyByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult diabetesCarePlanPathology_Edit(CarePlanPathology model)
        {

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateCarePlanPathology(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult medicine_Create(Guid DependentID, Guid Pro)
        {
            var model = new Medicine();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult medicine_Create(Medicine model)
        {
            if (Request.Query["PatientsOnInsulin"].ToString() != null)
                model.PatientsOnInsulin = Request.Query["PatientsOnInsulin"].ToString();
            if (Request.Query["changeOfRegimeHIV"].ToString() != null)
                model.changeOfRegimeHIV = Request.Query["changeOfRegimeHIV"].ToString();
            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertMedicine(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult medicine_Details(int id)
        {
            var model = _member.GetMedicineByID(id);
            return View(model);
        }

        public ActionResult medicine_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetMedicineByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult medicine_Edit(Medicine model)
        {
            if (Request.Query["PatientsOnInsulin_Concat"].ToString() != null)
                model.PatientsOnInsulin_Concat = Request.Query["PatientsOnInsulin_Concat"].ToString().Split(',');
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateMedicines(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult clinicalAdditionst_Create(Guid DependentID, Guid Pro)
        {
            var model = new ClinicalAddition();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult clinicalAdditionst_Create(ClinicalAddition model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertClinicalAddition(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult clinicalAdditionst_Edit(int id)
        {
            var model = _member.GetClinicalAdditionByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult clinicalAdditionst_Edit(ClinicalAddition model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateClinicalAddition(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult clinicalAdditionst_Details(int id)
        {
            var model = _member.GetClinicalAdditionByID(id);

            return View(model);
        }

        public ActionResult nutritionAndLifestyle_Create(Guid DependentID, Guid Pro)
        {
            var model = new NutritionAndLifestyle();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult nutritionAndLifestyle_Create(NutritionAndLifestyle model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            _member.InsertNutritionAndLifestyle(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult nutritionAndLifestyle_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetNutritionAndLifestyleByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult nutritionAndLifestyle_Edit(NutritionAndLifestyle model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateNutritionAndLifestyle(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult nutritionAndLifestyle_Details(int id)
        {
            var model = _member.GetNutritionAndLifestyleByID(id);
            return View(model);
        }
        public ActionResult visit_Create(Guid DependentID, Guid Pro)
        {
            var model = new Visit();

            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult visit_Create(Visit model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertVisit(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult visit_Edit(int id)
        {
            var model = _member.GetVisitByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult visit_Edit(Visit model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateVisit(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult visit_Details(int id)
        {
            var model = _member.GetVisitByID(id);
            return View(model);
        }

        #endregion

        #region HIV-care-plan hcare-1093
        public ActionResult HIVCarePlanPathology_Create(Guid DependentID, Guid Pro)
        {
            var model = new CarePlanPathology();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult HIVCarePlanPathology_Create(CarePlanPathology model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertCarePathology(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult HIVCarePlanPathology_Details(int id)
        {
            var model = _member.GetCarePathologyByID(id);
            return View(model);
        }

        public ActionResult HIVCarePlanPathology_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetCarePathologyByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult HIVCarePlanPathology_Edit(CarePlanPathology model)
        {

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateCarePlanPathology(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult HIVmedicine_Create(Guid DependentID, Guid Pro)
        {
            var model = new Medicine();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult HIVmedicine_Create(Medicine model)
        {
            if (Request.Query["changeOfRegimeHIV"].ToString() != null)
                model.changeOfRegimeHIV = Request.Query["changeOfRegimeHIV"].ToString();
            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertMedicine(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult HIVmedicine_Details(int id)
        {
            var model = _member.GetMedicineByID(id);
            return View(model);
        }

        public ActionResult HIVmedicine_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetMedicineByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult HIVmedicine_Edit(Medicine model)
        {
            if (Request.Query["changeOfRegimeHIV_Concat"].ToString() != null)
                model.PatientsOnInsulin_Concat = Request.Query["changeOfRegimeHIV_Concat"].ToString().Split(',');
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateMedicines(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult HIVpaediatric_Create(Guid DependentID, Guid Pro)
        {
            var model = new paediatric();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult HIVpaediatric_Create(paediatric model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.Insertpaediatric(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult HIVpaediatric_Details(int id)
        {
            var model = _member.GetPaediatricByID(id);
            return View(model);
        }

        public ActionResult HIVpaediatric_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetPaediatricByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult HIVpaediatric_Edit(paediatric model)
        {

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdatePaediatrict(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        #endregion

        #region mental-health-care-plan hcare-1192
        public ActionResult MentalHealthtPlanPathology_Create(Guid DependentID, Guid Pro)
        {
            var model = new CarePlanPathology();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MentalHealthtPlanPathology_Create(CarePlanPathology model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertCarePathology(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult MentalHealthtPlanPathology_Details(int id)
        {
            var model = _member.GetCarePathologyByID(id);
            return View(model);
        }

        public ActionResult MentalHealthtPlanPathology_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetCarePathologyByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult MentalHealthtPlanPathology_Edit(CarePlanPathology model)
        {

            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateCarePlanPathology(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult MentalHealthtPlanMedicine_Create(Guid DependentID, Guid Pro)
        {
            var model = new Medicine();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MentalHealthtPlanMedicine_Create(Medicine model)
        {
            if (Request.Query["MentalChangeOfRegime"].ToString() != null)
                model.changeOfRegimeHIV = Request.Query["MentalChangeOfRegime"].ToString();
            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertMedicine(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult MentalHealthtPlanMedicine_Details(int id)
        {
            var model = _member.GetMedicineByID(id);
            return View(model);
        }

        public ActionResult MentalHealthtPlanMedicine_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetMedicineByID(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult MentalHealthtPlanMedicine_Edit(Medicine model)
        {
            if (Request.Query["changeOfRegimeMental_Concat"].ToString() != null)
                model.PatientsOnInsulin_Concat = Request.Query["changeOfRegimeMental_Concat"].ToString().Split(',');
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateMedicines(model);


            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }
        public ActionResult MentalHealthtPlanNutritionAndLifestyle_Create(Guid DependentID, Guid Pro)
        {
            var model = new NutritionAndLifestyle();
            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }

        [HttpPost]
        public ActionResult MentalHealthtPlanNutritionAndLifestyle_Create(NutritionAndLifestyle model)
        {
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            _member.InsertNutritionAndLifestyle(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult MentalHealthtPlanNutritionAndLifestyle_Edit(int id)
        {
            ViewBag.programType = new SelectList(_member.GetPrograms(), "code", "ProgramName");
            var model = _member.GetNutritionAndLifestyleByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult MentalHealthtPlanNutritionAndLifestyle_Edit(NutritionAndLifestyle model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateNutritionAndLifestyle(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }


        public ActionResult MentalHealthtPlanNutritionAndLifestyle_Details(int id)
        {
            var model = _member.GetNutritionAndLifestyleByID(id);
            return View(model);
        }
        public ActionResult MentalHealthtPlanVisit_Create(Guid DependentID, Guid Pro)
        {
            var model = new Visit();

            model.dependentID = DependentID;
            model.programId = Pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MentalHealthtPlanVisit_Create(Visit model)
        {

            model.createdBy = User.Identity.Name; ;
            model.createdDate = DateTime.Now;
            _member.InsertVisit(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult MentalHealthtPlanVisit_Edit(int id)
        {
            var model = _member.GetVisitByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult MentalHealthtPlanVisit_Edit(Visit model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            _member.UpdateVisit(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.dependentID, pro = Request.Query["pro"] });
        }

        public ActionResult MentalHealthtPlanVisit_Details(int id)
        {
            var model = _member.GetVisitByID(id);
            return View(model);
        }
        #endregion

        #region mental-health-schizophrenia hcare-1124

        public ActionResult MH_Schizophrenia_Create(Guid DependentID, Guid pro)
        {
            var model = new MH_SchizophreniaResponse();
            model.DependantID = DependentID;
            ViewBag.program = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Schizophrenia_Create(MH_SchizophreniaResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;

            #region schizophrenia-insert
            var schizophrenia = new MH_SchizophreniaResponse();
            schizophrenia.DependantID = dependantid;
            schizophrenia.TaskID = null;
            schizophrenia.AssignmentID = null;
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

            if (Request.Query["SuicidalCMT"].ToString() != null)
            {
                schizophrenia.SuicidalCMT = Request.Query["SuicidalCMT"];
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
            schizophrenia.Comment = "Schizophrenia questionnaire - create";
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
                risk.reason = "Schizophrenia questionnaire - create (Suicidal)";
            }
            else if (totalScore <= 9)
            {
                risk.RiskId = "G";
                risk.reason = "Schizophrenia questionnaire - create";
            }
            else if (totalScore >= 10 && totalScore <= 18)
            {
                risk.RiskId = "Y";
                risk.reason = "Schizophrenia questionnaire - create";
            }
            else if (totalScore >= 19)
            {
                risk.RiskId = "R";
                risk.reason = "Schizophrenia questionnaire - create";
            }

            _member.InsertRiskRating(risk);

            schizophrenia.RiskID = risk.id;
            var insert = _admin.InsertSchizophreniaResults(schizophrenia);

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
            if (risk.RiskId == "R")
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

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult MH_Schizophrenia_Edit(int id)
        {
            var model = _admin.GetSchizophreniaById(id);
            ViewBag.Program = model.Program;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Schizophrenia_Edit(MH_SchizophreniaResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;
            var program = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();

            #region schizophrenia-update
            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            model.DependantID = dependantid;
            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;

            if (Request.Query["depression-text"].ToString() != null)
            {
                model.Depression = Request.Query["depression-text"];
                model.DepressionSC = Convert.ToInt32(Request.Query["depression-score"]);
                totalScore = model.DepressionSC + totalScore;
            }
            if (Request.Query["hopelessness-text"].ToString() != null)
            {
                model.Hopelessness = Request.Query["hopelessness-text"];
                model.HopelessnessSC = Convert.ToInt32(Request.Query["hopelessness-score"]);
                totalScore = model.HopelessnessSC + totalScore;
            }
            if (Request.Query["self-depreciation-text"].ToString() != null)
            {
                model.SelfDepreciation = Request.Query["self-depreciation-text"];
                model.SelfDepreciationSC = Convert.ToInt32(Request.Query["self-depreciation-score"]);
                totalScore = model.SelfDepreciationSC + totalScore;
            }
            if (Request.Query["guilty-ideas-text"].ToString() != null)
            {
                model.GuiltyIdeas = Request.Query["guilty-ideas-text"];
                model.GuiltyIdeasSC = Convert.ToInt32(Request.Query["guilty-ideas-score"]);
                totalScore = model.GuiltyIdeasSC + totalScore;
            }
            if (Request.Query["pathological-guilt-text"].ToString() != null)
            {
                model.PathologicalGuilt = Request.Query["pathological-guilt-text"];
                model.PathologicalGuiltSC = Convert.ToInt32(Request.Query["pathological-guilt-score"]);
                totalScore = model.PathologicalGuiltSC + totalScore;
            }
            if (Request.Query["morning-depresion-text"].ToString() != null)
            {
                model.MorningDepression = Request.Query["morning-depresion-text"];
                model.MorningDepressionSC = Convert.ToInt32(Request.Query["morning-depresion-score"]);
                totalScore = model.MorningDepressionSC + totalScore;
            }
            if (Request.Query["early-wakening-text"].ToString() != null)
            {
                model.EarlyWakening = Request.Query["early-wakening-text"];
                model.EarlyWakeningSC = Convert.ToInt32(Request.Query["early-wakening-score"]);
                totalScore = model.EarlyWakeningSC + totalScore;
            }
            if (Request.Query["suicide-text"].ToString() != null)
            {
                model.Suicidal = Request.Query["suicide-text"];
                model.SuicidalSC = Convert.ToInt32(Request.Query["suicide-score"]);
                totalScore = model.SuicidalSC + totalScore;
            }
            if (Request.Query["SuicidalCMT"].ToString() != null)
            {
                model.SuicidalCMT = Request.Query["SuicidalCMT"];
            }
            if (Request.Query["observed-depression-text"].ToString() != null)
            {
                model.ObservedDepression = Request.Query["observed-depression-text"];
                model.ObservedDepressionSC = Convert.ToInt32(Request.Query["observed-depression-score"]);
                totalScore = model.ObservedDepressionSC + totalScore;
            }

            model.TotalScore = totalScore;
            #region outcome
            if (model.SuicidalSC > 0)
            {
                model.Outcome = "Severe";
            }
            else if (totalScore < 1)
            {
                model.Outcome = "Normal";
            }
            else if (totalScore >= 1 && totalScore <= 9)
            {
                model.Outcome = "Mild";
            }
            else if (totalScore >= 10 && totalScore <= 18)
            {
                model.Outcome = "Moderate";
            }
            else if (totalScore >= 19 && totalScore <= 27)
            {
                model.Outcome = "Severe";
            }
            #endregion
            model.Comment = "Schizophrenia questionnaire - edit";
            model.Program = programcode.code;

            #endregion
            #region risk-rating
            var RiskRating = _member.GetRiskRatingByID(model.RiskID);
            RiskRating.modifiedDate = DateTime.Now;
            RiskRating.modifiedBy = User.Identity.Name;

            if (model.SuicidalSC != 0)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Schizophrenia questionnaire - edit (Suicidal)";
            }
            else if (totalScore <= 9)
            {
                RiskRating.RiskId = "G";
                RiskRating.reason = "Schizophrenia questionnaire - edit";
            }
            else if (totalScore >= 10 && totalScore <= 18)
            {
                RiskRating.RiskId = "Y";
                RiskRating.reason = "Schizophrenia questionnaire - edit";
            }
            else if (totalScore >= 19)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Schizophrenia questionnaire - edit";
            }

            if (model.Active == true)
            {
                RiskRating.active = true;
            }
            else
            {
                RiskRating.active = false;
            }

            _member.UpdateRiskRating(RiskRating);

            #endregion
            #region case-manager-assignmnet hcare-1176
            if (RiskRating.RiskId == "R")
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

            var result = _admin.UpdateSchizophreniaResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult MH_Schizophrenia_Details(int id)
        {
            var model = _admin.GetSchizophreniaById(id);

            return View(model);
        }

        #endregion

        #region mental-health-bipolar hcare-1125

        public ActionResult MH_Bipolar_Create(Guid DependentID, Guid pro)
        {
            var model = new MH_BipolarResponse();
            model.DependantID = DependentID;
            ViewBag.program = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Bipolar_Create(MH_BipolarResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;

            #region bipolar-insert
            var bipolar = new MH_BipolarResponse();
            bipolar.TaskID = null;
            bipolar.DependantID = dependantid;
            bipolar.AssignmentID = null;
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
            else if (totalScore < 20)
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
            if (risk.RiskId == "R")
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

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult MH_Bipolar_Edit(int id)
        {
            var model = _admin.GetBipolarById(id);
            ViewBag.Program = model.Program;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Bipolar_Edit(MH_BipolarResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;
            var program = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();

            #region bipolar-update
            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            model.DependantID = dependantid;
            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;

            if (Request.Query["bipolar-depression-text"].ToString() != null)
            {
                model.Depression = Request.Query["bipolar-depression-text"];
                model.DepressionSC = Convert.ToInt32(Request.Query["bipolar-depression-score"]);
                totalScore = model.DepressionSC + totalScore;
            }
            if (Request.Query["bipolar-insomnia-text"].ToString() != null)
            {
                model.Insomnia = Request.Query["bipolar-insomnia-text"];
                model.InsomniaSC = Convert.ToInt32(Request.Query["bipolar-insomnia-score"]);
                totalScore = model.InsomniaSC + totalScore;
            }
            if (Request.Query["bipolar-appetite-text"].ToString() != null)
            {
                model.Appetite = Request.Query["bipolar-appetite-text"];
                model.AppetiteSC = Convert.ToInt32(Request.Query["bipolar-appetite-score"]);
                totalScore = model.AppetiteSC + totalScore;
            }
            if (Request.Query["bipolar-social-engagement-text"].ToString() != null)
            {
                model.SocialEngagement = Request.Query["bipolar-social-engagement-text"];
                model.SocialEngagementSC = Convert.ToInt32(Request.Query["bipolar-social-engagement-score"]);
                totalScore = model.SocialEngagementSC + totalScore;
            }
            if (Request.Query["bipolar-activity-text"].ToString() != null)
            {
                model.Activity = Request.Query["bipolar-activity-text"];
                model.ActivitySC = Convert.ToInt32(Request.Query["bipolar-activity-score"]);
                totalScore = model.ActivitySC + totalScore;
            }
            if (Request.Query["bipolar-motivation-text"].ToString() != null)
            {
                model.Motivation = Request.Query["bipolar-motivation-text"];
                model.MotivationSC = Convert.ToInt32(Request.Query["bipolar-motivation-score"]);
                totalScore = model.MotivationSC + totalScore;
            }
            if (Request.Query["bipolar-concentration-text"].ToString() != null)
            {
                model.Concentration = Request.Query["bipolar-concentration-text"];
                model.ConcentrationSC = Convert.ToInt32(Request.Query["bipolar-concentration-score"]);
                totalScore = model.ConcentrationSC + totalScore;
            }
            if (Request.Query["bipolar-anxiety-text"].ToString() != null)
            {
                model.Anxiety = Request.Query["bipolar-anxiety-text"];
                model.AnxietySC = Convert.ToInt32(Request.Query["bipolar-anxiety-score"]);
                totalScore = model.AnxietySC + totalScore;
            }
            if (Request.Query["bipolar-pleasure-text"].ToString() != null)
            {
                model.Pleasure = Request.Query["bipolar-pleasure-text"];
                model.PleasureSC = Convert.ToInt32(Request.Query["bipolar-pleasure-score"]);
                totalScore = model.PleasureSC + totalScore;
            }
            if (Request.Query["bipolar-emotion-text"].ToString() != null)
            {
                model.Emotion = Request.Query["bipolar-emotion-text"];
                model.EmotionSC = Convert.ToInt32(Request.Query["bipolar-emotion-score"]);
                totalScore = model.EmotionSC + totalScore;
            }
            if (Request.Query["bipolar-worthlessness-text"].ToString() != null)
            {
                model.Worthlessness = Request.Query["bipolar-worthlessness-text"];
                model.WorthlessnessSC = Convert.ToInt32(Request.Query["bipolar-worthlessness-score"]);
                totalScore = model.WorthlessnessSC + totalScore;
            }
            if (Request.Query["bipolar-helplessness-text"].ToString() != null)
            {
                model.Helplessness = Request.Query["bipolar-helplessness-text"];
                model.HelplessnessSC = Convert.ToInt32(Request.Query["bipolar-helplessness-score"]);
                totalScore = model.HelplessnessSC + totalScore;
            }
            if (Request.Query["bipolar-suicide-text"].ToString() != null)
            {
                model.Suicidal = Request.Query["bipolar-suicide-text"];
                model.SuicidalSC = Convert.ToInt32(Request.Query["bipolar-suicide-score"]);
                totalScore = model.SuicidalSC + totalScore;
            }
            if (Request.Query["SuicidalCMT"].ToString() != null)
            {
                model.SuicidalCMT = Request.Query["MH_BipolarResponse.SuicidalCMT"];
            }
            if (Request.Query["bipolar-guilt-text"].ToString() != null)
            {
                model.Guilt = Request.Query["bipolar-guilt-text"];
                model.GuiltSC = Convert.ToInt32(Request.Query["bipolar-guilt-score"]);
                totalScore = model.GuiltSC + totalScore;
            }
            if (Request.Query["bipolar-psychotic-text"].ToString() != null)
            {
                model.Psychotic = Request.Query["bipolar-psychotic-text"];
                model.PsychoticSC = Convert.ToInt32(Request.Query["bipolar-psychotic-score"]);
                totalScore = model.PsychoticSC + totalScore;
            }
            if (Request.Query["bipolar-irritability-text"].ToString() != null)
            {
                model.Irritability = Request.Query["bipolar-irritability-text"];
                model.IrritabilitySC = Convert.ToInt32(Request.Query["bipolar-irritability-score"]);
                totalScore = model.IrritabilitySC + totalScore;
            }
            if (Request.Query["bipolar-lability-text"].ToString() != null)
            {
                model.Lability = Request.Query["bipolar-lability-text"];
                model.LabilitySC = Convert.ToInt32(Request.Query["bipolar-lability-score"]);
                totalScore = model.LabilitySC + totalScore;
            }
            if (Request.Query["bipolar-inc-motor-drive-text"].ToString() != null)
            {
                model.IncMotorDrive = Request.Query["bipolar-inc-motor-drive-text"];
                model.IncMotorDriveSC = Convert.ToInt32(Request.Query["bipolar-inc-motor-drive-score"]);
                totalScore = model.IncMotorDriveSC + totalScore;
            }
            if (Request.Query["bipolar-inc-speech-text"].ToString() != null)
            {
                model.IncSpeech = Request.Query["bipolar-inc-speech-text"];
                model.IncSpeechSC = Convert.ToInt32(Request.Query["bipolar-inc-speech-score"]);
                totalScore = model.IncSpeechSC + totalScore;
            }
            if (Request.Query["bipolar-agitation-text"].ToString() != null)
            {
                model.Agitation = Request.Query["bipolar-agitation-text"];
                model.AgitationSC = Convert.ToInt32(Request.Query["bipolar-agitation-score"]);
                totalScore = model.AgitationSC + totalScore;
            }


            model.TotalScore = totalScore;
            #region outcome
            if (model.SuicidalSC > 0)
            {
                model.Outcome = "Severe";
            }
            else if (totalScore < 1)
            {
                model.Outcome = "Normal";
            }
            else if (totalScore >= 1 && totalScore <= 20)
            {
                model.Outcome = "Mild";
            }
            else if (totalScore >= 21 && totalScore <= 40)
            {
                model.Outcome = "Moderate";
            }
            else if (totalScore >= 41 && totalScore <= 60)
            {
                model.Outcome = "Severe";
            }
            #endregion
            model.Comment = "Bipolar questionnaire - edit";
            model.Program = programcode.code;
            #endregion
            #region risk-rating
            var RiskRating = _member.GetRiskRatingByID(model.RiskID);
            RiskRating.modifiedDate = DateTime.Now;
            RiskRating.modifiedBy = User.Identity.Name;

            if (model.SuicidalSC != 0)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Bipolar questionnaire - edit (Suicidal)";
            }
            else if (totalScore <= 20)
            {
                RiskRating.RiskId = "G";
                RiskRating.reason = "Bipolar questionnaire - edit";
            }
            else if (totalScore >= 21 && totalScore <= 40)
            {
                RiskRating.RiskId = "Y";
                RiskRating.reason = "Bipolar questionnaire - edit";
            }
            else if (totalScore >= 41)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Bipolar questionnaire - edit";
            }

            if (model.Active == true)
            {
                RiskRating.active = true;
            }
            else
            {
                RiskRating.active = false;
            }

            _member.UpdateRiskRating(RiskRating);
            #endregion
            #region case-manager-assignmnet hcare-1176
            if (RiskRating.RiskId == "R")
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

            var result = _admin.UpdateBipolarResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult MH_Bipolar_Details(int id)
        {
            var model = _admin.GetBipolarById(id);

            return View(model);
        }

        #endregion

        #region mental-health-depression hcare-1126

        public ActionResult MH_Depression_Create(Guid DependentID, Guid pro)
        {
            var model = new MH_DepressionResponse();
            model.DependantID = DependentID;
            ViewBag.program = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Depression_Create(MH_DepressionResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;

            #region depression-insert
            var depression = new MH_DepressionResponse();
            depression.TaskID = null;
            depression.DependantID = dependantid;
            depression.AssignmentID = null;
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
            depression.Comment = "Depression questionnaire - create";
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
            if (risk.RiskId == "R")
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

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult MH_Depression_Edit(int id)
        {
            var model = _admin.GetDepressionById(id);
            ViewBag.Program = model.Program;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Depression_Edit(MH_DepressionResponse model)
        {
            var programcode = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);
            var totalScore = 0;
            var program = _member.GetPrograms().Where(x => x.code == Request.Query["pro"]).FirstOrDefault();

            #region depression-update
            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            model.DependantID = dependantid;
            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;

            if (Request.Query["depression-depression-text"].ToString() != null)
            {
                model.Depression = Request.Query["depression-depression-text"];
                model.DepressionSC = Convert.ToInt32(Request.Query["depression-depression-score"]);
                totalScore = model.DepressionSC + totalScore;
            }
            if (Request.Query["depression-guilt-text"].ToString() != null)
            {
                model.Guilt = Request.Query["depression-guilt-text"];
                model.GuiltSC = Convert.ToInt32(Request.Query["depression-guilt-score"]);
                totalScore = model.GuiltSC + totalScore;
            }
            if (Request.Query["depression-suicide-text"].ToString() != null)
            {
                model.Suicidal = Request.Query["depression-suicide-text"];
                model.SuicidalSC = Convert.ToInt32(Request.Query["depression-suicide-score"]);
                totalScore = model.SuicidalSC + totalScore;
            }
            if (Request.Query["depression-insomnia-early-night-text"].ToString() != null)
            {
                model.InsomniaEarlyNight = Request.Query["depression-insomnia-early-night-text"];
                model.InsomniaEarlyNightSC = Convert.ToInt32(Request.Query["depression-insomnia-early-night-score"]);
                totalScore = model.InsomniaEarlyNightSC + totalScore;
            }
            if (Request.Query["depression-insomnia-middle-night-text"].ToString() != null)
            {
                model.InsomniaMiddleNight = Request.Query["depression-insomnia-middle-night-text"];
                model.InsomniaMiddleNightSC = Convert.ToInt32(Request.Query["depression-insomnia-middle-night-score"]);
                totalScore = model.InsomniaMiddleNightSC + totalScore;
            }
            if (Request.Query["depression-insomnia-early-morning-text"].ToString() != null)
            {
                model.InsomniaEarlyMorning = Request.Query["depression-insomnia-early-morning-text"];
                model.InsomniaEarlyMorningSC = Convert.ToInt32(Request.Query["depression-insomnia-early-morning-score"]);
                totalScore = model.InsomniaEarlyMorningSC + totalScore;
            }
            if (Request.Query["depression-activity-text"].ToString() != null)
            {
                model.Activity = Request.Query["depression-activity-text"];
                model.ActivitySC = Convert.ToInt32(Request.Query["depression-activity-score"]);
                totalScore = model.ActivitySC + totalScore;
            }
            if (Request.Query["depression-psychomotor-text"].ToString() != null)
            {
                model.Psychomotor = Request.Query["depression-psychomotor-text"];
                model.PsychomotorSC = Convert.ToInt32(Request.Query["depression-psychomotor-score"]);
                totalScore = model.PsychomotorSC + totalScore;
            }
            if (Request.Query["depression-agitation-text"].ToString() != null)
            {
                model.Agitation = Request.Query["depression-agitation-text"];
                model.AgitationSC = Convert.ToInt32(Request.Query["depression-agitation-score"]);
                totalScore = model.AgitationSC + totalScore;
            }
            if (Request.Query["depression-anxiety-psychic-text"].ToString() != null)
            {
                model.AnxietyPsychic = Request.Query["depression-anxiety-psychic-text"];
                model.AnxietyPsychicSC = Convert.ToInt32(Request.Query["depression-anxiety-psychic-score"]);
                totalScore = model.AnxietyPsychicSC + totalScore;
            }
            if (Request.Query["depression-anxiety-somatic-text"].ToString() != null)
            {
                model.AnxietySomatic = Request.Query["depression-anxiety-somatic-text"];
                model.AnxietySomaticSC = Convert.ToInt32(Request.Query["depression-anxiety-somatic-score"]);
                totalScore = model.AnxietySomaticSC + totalScore;
            }
            if (Request.Query["depression-somatic-sym-gastro-text"].ToString() != null)
            {
                model.SomaticSymGastro = Request.Query["depression-somatic-sym-gastro-text"];
                model.SomaticSymGastroSC = Convert.ToInt32(Request.Query["depression-somatic-sym-gastro-score"]);
                totalScore = model.SomaticSymGastroSC + totalScore;
            }
            if (Request.Query["depression-somatic-sym-general-text"].ToString() != null)
            {
                model.SomaticSymGeneral = Request.Query["depression-somatic-sym-general-text"];
                model.SomaticSymGeneralSC = Convert.ToInt32(Request.Query["depression-somatic-sym-general-score"]);
                totalScore = model.SomaticSymGeneralSC + totalScore;
            }
            if (Request.Query["depression-sexology-text"].ToString() != null)
            {
                model.Sexology = Request.Query["depression-sexology-text"];
                model.SexologySC = Convert.ToInt32(Request.Query["depression-sexology-score"]);
                totalScore = model.SexologySC + totalScore;
            }
            if (Request.Query["depression-hypochondriasis-text"].ToString() != null)
            {
                model.Hypochondriasis = Request.Query["depression-hypochondriasis-text"];
                model.HypochondriasisSC = Convert.ToInt32(Request.Query["depression-hypochondriasis-score"]);
                totalScore = model.HypochondriasisSC + totalScore;
            }
            if (Request.Query["depression-weightloss-text"].ToString() != null)
            {
                model.WeightLoss = Request.Query["depression-weightloss-text"];
                model.WeightLossSC = Convert.ToInt32(Request.Query["depression-weightloss-score"]);
                totalScore = model.WeightLossSC + totalScore;
            }
            if (Request.Query["depression-insight-text"].ToString() != null)
            {
                model.Insight = Request.Query["depression-insight-text"];
                model.InsightSC = Convert.ToInt32(Request.Query["depression-insight-score"]);
                totalScore = model.InsightSC + totalScore;
            }

            model.TotalScore = totalScore;
            #region outcome
            if (model.SuicidalSC > 0)
            {
                model.Outcome = "Severe";
            }
            else if (totalScore < 8)
            {
                model.Outcome = "Clinical remission - Normal";
            }
            else if (totalScore >= 8 && totalScore <= 13)
            {
                model.Outcome = "Mild depression";
            }
            else if (totalScore >= 14 && totalScore <= 18)
            {
                model.Outcome = "Moderate depression";
            }
            else if (totalScore >= 19 && totalScore <= 23)
            {
                model.Outcome = "Severe depression";
            }
            else if (totalScore > 23)
            {
                model.Outcome = "Very severe depression – urgent referral";
            }
            #endregion
            model.Comment = "Depression questionnaire - edit";
            model.Program = programcode.code;
            #endregion
            #region risk-rating
            var RiskRating = _member.GetRiskRatingByID(model.RiskID);
            RiskRating.modifiedDate = DateTime.Now;
            RiskRating.modifiedBy = User.Identity.Name;

            if (model.SuicidalSC > 0)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Depression questionnaire assignment - Suicidal";
            }
            else if (totalScore < 14)
            {
                RiskRating.RiskId = "G";
                RiskRating.reason = "Depression  questionnaire assignment";
            }
            else if (totalScore >= 14 && totalScore <= 18)
            {
                RiskRating.RiskId = "Y";
                RiskRating.reason = "Depression  questionnaire assignment";
            }
            else if (totalScore >= 19)
            {
                RiskRating.RiskId = "R";
                RiskRating.reason = "Depression  questionnaire assignment";
            }

            if (model.Active == true)
            {
                RiskRating.active = true;
            }
            else
            {
                RiskRating.active = false;
            }

            _member.UpdateRiskRating(RiskRating);
            #endregion
            #region case-manager-assignmnet hcare-1176
            if (RiskRating.RiskId == "R")
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

            var result = _admin.UpdateDepressionResult(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult MH_Depression_Details(int id)
        {
            var model = _admin.GetDepressionById(id);

            return View(model);
        }

        #endregion

        #region doctor-referral hcare-1136

        public ActionResult DoctorReferral_Create(Guid DependentID, Guid pro)
        {
            var model = new DoctorReferral();
            model.DependantID = DependentID;
            ViewBag.program = pro;

            ViewBag.referalID = new SelectList(_member.getReferralByDepandent(DependentID).ToString().Split(',').ToList());
            model.referralFroms = _member.getReferralFromByDepandent(DependentID, pro).ToString().Split(',').ToList();
            ViewBag.referalFrom = new SelectList(model.referralFroms);

            return View(model);
        }
        [HttpPost]
        public ActionResult DoctorReferral_Create(DoctorReferral model)
        {
            //HCare-1175
            if (Request.Query["referalID"].ToString() != null) { model.Referral = Request.Query["referalID"].ToString(); }
            if (Request.Query["referalFrom"].ToString() != null) { model.referralFrom = Request.Query["referalFrom"].ToString(); }

            var programcode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            var dependantid = new Guid(Request.Query["DependantID"]);

            model.TaskID = null;
            model.AssignmentID = null;
            model.DependantID = dependantid;
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;

            if (!String.IsNullOrEmpty(Request.Query["referral-date"]))
            {
                model.ReferralDate = Convert.ToDateTime(Request.Query["referral-date"]);
            }
            if (!String.IsNullOrEmpty(Request.Query["Comment"]))
            {
                model.Comment = Request.Query["Comment"];
            }

            model.Program = programcode.code;
            model.Active = true;

            var insert = _member.InsertDoctorReferral(model);

            return RedirectToAction("PatientClinical_Summary", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult DoctorReferral_Edit(int id, Guid? DependentID, Guid pro)
        {
            var model = _member.GetDoctorReferralById(id);
            ViewBag.Program = model.Program;

            ViewBag.referalID = new SelectList(_member.getReferralByDepandent(DependentID).ToString().Split(',').ToList(), model.Referral);
            model.referralFroms = _member.getReferralFromByDepandent(DependentID, pro).ToString().Split(',').ToList();
            ViewBag.referalFrom = new SelectList(model.referralFroms, model.referralFrom);

            return View(model);
        }
        [HttpPost]
        public ActionResult DoctorReferral_Edit(DoctorReferral model)
        {
            var a = Request.Query.Keys;

            var dependantid = new Guid(Request.Query["DependantID"]);
            var program = _member.GetPrograms().Where(x => x.ProgramName == Request.Query["programName"]).FirstOrDefault();

            //HCare-1175
            if (Request.Query["referalID"].ToString() != null) { model.Referral = Request.Query["referalID"].ToString(); }
            if (Request.Query["referalFrom"].ToString() != null) { model.referralFrom = Request.Query["referalFrom"].ToString(); }

            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            model.DependantID = dependantid;
            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;

            if (!String.IsNullOrEmpty(Request.Query["referral-date"]))
            {
                model.ReferralDate = Convert.ToDateTime(Request.Query["referral-date"]);
            }
            if (!String.IsNullOrEmpty(Request.Query["Comment"]))
            {
                model.Comment = Request.Query["Comment"];
            }

            var result = _member.UpdateDoctorReferral(model);

            return RedirectToAction("PatientClinical_Summary", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult DoctorReferral_Details(int id)
        {
            var model = _member.GetDoctorReferralById(id);

            return View(model);
        }

        #endregion

        #region pathology-types hcare-970
        public ActionResult PathologyType_Index()
        {
            var model = _member.GetPathologyTypeList();
            return View(model);
        }
        public ActionResult PathologyType_Create()
        {
            ViewBag.Code = "";
            ViewBag.Description = "";

            return View();
        }
        [HttpPost]
        public ActionResult PathologyType_Create(PathologyTypes model)
        {
            model.id = model.id.Trim(); //hcare-1285
            model.pathologyType = model.pathologyType.Trim(); //hcare-1285
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.active = true;

            #region duplicate-check
            var validation = _member.GetPathologyTypeValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.id.ToLower() == item.id.ToLower().Trim() && model.pathologyType.ToLower() == item.pathologyType.ToLower().Trim()) { v1++; break; }
                if (model.id.ToLower() == item.id.ToLower().Trim()) { v2++; }
                if (model.pathologyType.ToLower() == item.pathologyType.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.id;
                ViewBag.Description = model.pathologyType;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.id;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.pathologyType;
                return View(model);
            }
            #endregion

            var insert = _member.InsertPathologyType(model);

            return RedirectToAction("Index", "Other");

        }

        public ActionResult PathologyType_Edit(string id)
        {
            var model = _member.GetPathologyTypeById(id);
            ViewBag.Code = "";
            ViewBag.Description = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult PathologyType_Edit(PathologyTypes model)
        {
            model.id = model.id.Trim(); //hcare-1285
            model.pathologyType = model.pathologyType.Trim(); //hcare-1285
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _member.GetPathologyTypeValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.id.ToLower() != item.id.ToLower().Trim() && model.pathologyType.ToLower() == item.pathologyType.ToLower().Trim()) { v1++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.id;
                ViewBag.Description = model.pathologyType;
                return View(model);
            }
            #endregion

            var result = _member.UpdatePathologyType(model);

            return RedirectToAction("Index", "Other");
        }

        public ActionResult PathologyType_Details(string id)
        {
            var model = _member.GetPathologyTypeById(id);
            return View(model);
        }

        public ActionResult PathologyTypeCodeCheck(string code) //HCare-956
        {
            var options = _member.GetPathologyTypeByCode(code);

            return Json(options);
        }
        public ActionResult PathologyTypeNameCheck(string name) //HCare-956
        {
            var options = _member.GetPathologyTypeByName(name);

            return Json(options);
        }
        #endregion

        #region HCare-1196 HIV staging 4

        public ActionResult CreateHIVstaging(Guid DependentID) //HCare-1058
        {
            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();
            ViewBag.programId = programCode.programID;

            var model = new PatientStagingHistory();
            model.dependantId = DependentID;


            List<SelectListItem> HIVStages = new List<SelectListItem>();
            HIVStages.Add(new SelectListItem() { Text = "Stage 4", Value = "4" });

            ViewBag.riskType = new SelectList(HIVStages, "Value", "Text");

            return View(model);
        }
        [HttpPost]
        public ActionResult CreateHIVstaging(PatientStagingHistory model)
        {

            var programCode = _member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).FirstOrDefault();

            model.stageCode = Request.Query["riskType"].ToString();

            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;

            ClinicalViewModel modell = new ClinicalViewModel();
            var results = _member.GetHIVPathologyForManualStaging(model.dependantId);
            model.cd4Count = results.CD4Count;
            model.cd4Percentage = Convert.ToInt32(results.CD4Percentage);
            model.viralLoad = results.viralLoad;

            if (!String.IsNullOrEmpty(Request.Query["effective-Date"]))
            {
                var date = Convert.ToDateTime(Request.Query["effective-Date"]).Date;
                model.effectiveDate = date.Add(DateTime.Now.TimeOfDay);
            }
            //if id is zero do the insert otherwise do update
            if (model.id == 0)
            {
                model.active = true;
                _member.InsertHIVPatientStagingHistoryManualStaging(model);
            }
            else
            {

                _member.UpdateHIVPatientStagingHistoryForManualStaging(model);
            }

            return RedirectToAction("PatientClinical_Summary", new { DependentID = model.dependantId, pro = Request.Query["pro"] });
        }

        public ActionResult HIVstagingEdit(int id, Guid pro) //HCare-1058
        {
            ViewBag.programId = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            var model = _member.GetPatientStagingHistoryById(id);


            var viewModel = new PatientStagingHistory
            {
                dependantId = model.dependantId,
                stageCode = model.stageCode,
                active = model.active,
                comment = model.comment,
                id = model.id,
                effectiveDate = model.effectiveDate

            };

            List<SelectListItem> HIVStages = new List<SelectListItem>();
            HIVStages.Add(new SelectListItem() { Text = "Stage 1", Value = "1" });
            HIVStages.Add(new SelectListItem() { Text = "Stage 2", Value = "2" });
            HIVStages.Add(new SelectListItem() { Text = "Stage 3", Value = "3" });
            HIVStages.Add(new SelectListItem() { Text = "Stage 4", Value = "4" });

            ViewBag.riskType = new SelectList(HIVStages, "Value", "Text", model.stageCode);

            return View(viewModel);
        }

        public ActionResult HIVstagingDetails(int id, int? stagingIdFK, Guid? pro)
        {
            if (id == 0)
            {
                return RedirectToAction("RiskRating_Details", new { id = stagingIdFK, pro = pro });
            }
            else
            {
                var model = _member.GetPatientStagingHistoryById(id);

                return View(model);
            }

        }
        #endregion

        #region HCare-1194 MH Diagnosis
        public ActionResult MH_Diagnosis_Create(Guid DependentID, Guid pro)
        {
            var model = new MentalHealthDiagnosis();
            model.DependantID = DependentID;
            ViewBag.Condition = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(DependentID), "mappingCode", "condition");
            ViewBag.DependantID = DependentID;
            ViewBag.Program = pro;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Diagnosis_Create(MentalHealthDiagnosis model)
        {
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.ICD10Code = Request.Query["icd10"];
            model.ConditionCode = Request.Query["Condition"];
            model.ProgramCode = "MNTLH";
            model.Comment = Request.Query["Comment"];
            model.Active = true;

            var program = _admin.GetProgrambyCode(model.ProgramCode);
            var history = _member.GetMHDiagnosis(model.DependantID);
            var condition = _member.GetICD10Info(model.ICD10Code);

            #region hcare-894 date-validation
            // end-date cannot precede start-date
            if (model.EndDate < model.EffectiveDate)
            {
                ViewBag.Condition = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID), "mappingCode", "condition");
                ModelState.AddModelError("EndDate", "Condition end date cannot precede Effective date.");
                ViewBag.Program = program.programID;
                ViewBag.MappingCode = model.ConditionCode;

                return View(model);
            }

            foreach (var item in history)
            {
                bool startDateOverlap = model.EffectiveDate >= item.EffectiveDate && model.EffectiveDate <= item.EndDate;
                bool startEndDateOverlap = item.EffectiveDate <= model.EndDate && model.EffectiveDate <= item.EndDate;

                if (!String.IsNullOrEmpty(item.EffectiveDate.ToString()) && !String.IsNullOrEmpty(item.EndDate.ToString()))
                {
                    if (condition.ICD10Code == item.ICD10Code)
                    {
                        if (!String.IsNullOrEmpty(model.EffectiveDate.ToString()) && !String.IsNullOrEmpty(model.EndDate.ToString()))
                        {
                            if (startEndDateOverlap == true)
                            {
                                ViewBag.Condition = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID), "mappingCode", "condition");
                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's diagnosis history.";
                                ViewBag.Program = program.programID;
                                ViewBag.MappingCode = model.ConditionCode;

                                return View(model);
                            }
                        }
                        else if (!String.IsNullOrEmpty(model.EffectiveDate.ToString()) && String.IsNullOrEmpty(model.EndDate.ToString()))
                        {
                            if (startDateOverlap == true)
                            {
                                ViewBag.Condition = new SelectList(_member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID), "mappingCode", "condition"); ;
                                ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's diagnosis history.";
                                ViewBag.Program = program.programID;
                                ViewBag.MappingCode = model.ConditionCode;

                                return View(model);
                            }
                        }
                    }
                }
            }
            #endregion

            _member.InsertMentalHealthDiagnosis(model);

            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });
        }

        public ActionResult MH_Diagnosis_Edit(int id)
        {
            var model = _member.GetMentalHealthDiagnosisByID(id);
            var program = _admin.GetProgrambyName(model.Program);

            model.ConditionList = _member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID);
            model.ICD10List = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(model.MappingCode, model.DependantID);
            var diagnosisinfo = _member.GetMHDiagnosisInformation(model.MappingCode, model.ICD10Code);

            ViewBag.Program = program.programID;
            ViewBag.MappingCode = model.MappingCode;
            ViewBag.Condition = diagnosisinfo.Condition;

            return View(model);
        }
        [HttpPost]
        public ActionResult MH_Diagnosis_Edit(MHDiagnosisViewModel model)
        {
            var mhdiagnosis_preEdit = _member.GetMentalHealthDiagnosisByID(model.Id);
            var mhdiagnosis = _member.GetMentalHealthDiagnosisByID(model.Id);
            var program = _admin.GetProgramById(new Guid(Request.Query["pro"]));
            var diagnosisinfo = _member.GetMHDiagnosisInformation(Request.Query["conditionCode"], model.ICD10Code);
            if (diagnosisinfo != null) { model.MappingCode = diagnosisinfo.mappingCode; }



            mhdiagnosis.ModifiedBy = User.Identity.Name;
            mhdiagnosis.ModifiedDate = DateTime.Now;
            mhdiagnosis.EffectiveDate = model.EffectiveDate;
            mhdiagnosis.EndDate = model.EndDate;
            mhdiagnosis.ICD10Code = model.ICD10Code;
            mhdiagnosis.MappingCode = model.MappingCode;
            mhdiagnosis.Program = program.code;
            if (!String.IsNullOrEmpty(model.Comment)) { mhdiagnosis.Comment = model.Comment; }
            if (model.Active == true) { mhdiagnosis.Active = true; } else { mhdiagnosis.Active = false; }

            #region hcare-894 date validation
            #region end-date cannot precede start-date
            if (mhdiagnosis.EndDate < mhdiagnosis.EffectiveDate)
            {
                model.ConditionList = _member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID);
                model.ICD10List = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(model.MappingCode, model.DependantID);
                ViewBag.Program = program.programID;
                ViewBag.MappingCode = model.MappingCode;
                ViewBag.Condition = diagnosisinfo.Condition;

                ModelState.AddModelError("EndDate", "End date cannot precede Effective date.");

                return View(model);
            }
            #endregion
            #region overlapping-date-check
            var ppshhistory = _member.GetMHDiagnosis(model.DependantID);

            foreach (var item in ppshhistory.Where(x => x.Id != model.Id))
            {
                bool xOverlap = mhdiagnosis.EffectiveDate >= item.EffectiveDate && mhdiagnosis.EffectiveDate <= item.EndDate;
                bool yOverlap = item.EffectiveDate >= mhdiagnosis.EffectiveDate && item.EffectiveDate <= mhdiagnosis.EndDate;
                bool xyOverlap = item.EffectiveDate <= mhdiagnosis.EndDate && mhdiagnosis.EffectiveDate <= item.EndDate;

                if (item.Id != mhdiagnosis.Id)
                {
                    if (mhdiagnosis.MappingCode == item.MappingCode || mhdiagnosis.ICD10Code == item.ICD10Code)
                    {
                        if (!String.IsNullOrEmpty(item.EffectiveDate.ToString()) && !String.IsNullOrEmpty(item.EndDate.ToString()))
                        {
                            if (!String.IsNullOrEmpty(mhdiagnosis.EffectiveDate.ToString()) && !String.IsNullOrEmpty(mhdiagnosis.EndDate.ToString()))
                            {
                                if (xyOverlap == true && item.Id != mhdiagnosis.Id)
                                {
                                    model.ConditionList = _member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID);
                                    model.ICD10List = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(model.MappingDescription, model.DependantID);
                                    ViewBag.Program = program.programID;
                                    ViewBag.MappingCode = mhdiagnosis.MappingCode;

                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's diagnosis history.";

                                    return View(model);
                                }
                            }
                            else if (!String.IsNullOrEmpty(mhdiagnosis.EffectiveDate.ToString()) || !String.IsNullOrEmpty(mhdiagnosis.EndDate.ToString()))
                            {
                                if (xOverlap == true)
                                {
                                    model.ConditionList = _member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID);
                                    model.ICD10List = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(model.MappingDescription, model.DependantID);
                                    ViewBag.Program = program.programID;
                                    ViewBag.MappingCode = mhdiagnosis.MappingCode;

                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's diagnosis history.";

                                    return View(model);
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(item.EffectiveDate.ToString()) && String.IsNullOrEmpty(item.EffectiveDate.ToString()))
                        {
                            if (!String.IsNullOrEmpty(mhdiagnosis.EffectiveDate.ToString()) && !String.IsNullOrEmpty(mhdiagnosis.EndDate.ToString()))
                            {
                                if (yOverlap == true && item.Id != mhdiagnosis.Id)
                                {
                                    model.ConditionList = _member.GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(model.DependantID);
                                    model.ICD10List = _member.GetNewCoMorbidsICD10NotLinkedToDependant_MH(model.MappingDescription, model.DependantID);
                                    ViewBag.Program = program.programID;
                                    ViewBag.MappingCode = mhdiagnosis.MappingCode;

                                    ViewBag.Message = "Date range has been used in the Start/End date collection, please review patient's diagnosis history.";

                                    return View(model);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #endregion

            var result = _member.UpdateMentalHealthDiagnosis(mhdiagnosis);

            if (result.Success)
            {
                #region patient-program-history-update hcare-1203
                var patientprogram = _member.GetLatestPatientProgram(model.DependantID, new Guid(Request.Query["pro"]));
                if (patientprogram.icd10Code + " " + patientprogram.conditionCode == mhdiagnosis_preEdit.ICD10Code + " " + mhdiagnosis_preEdit.MappingCode)
                {
                    _member.UpdatePatientProgramHistory(patientprogram.id, mhdiagnosis.ICD10Code, mhdiagnosis.EndDate, User.Identity.Name);
                }
                #endregion
            }



            return RedirectToAction("PatientClinical_PatientHistory", new { DependentID = model.DependantID, pro = Request.Query["pro"] });

        }

        public ActionResult MH_Diagnosis_Details(int id)
        {
            var model = _member.GetMentalHealthDiagnosisByID(id);

            return View(model);
        }
        #endregion

        #region HCare-1361 next-of-kin
        public ActionResult Create_NextOfKin(Guid DependentID, Guid ProgramID)
        {
            var model = new NextOFKin();
            model.DependantID = DependentID;
            model.ProgramID = ProgramID;

            return View(model);
        }
        [HttpPost]
        public ActionResult Create_NextOfKin(NextOFKin model)
        {
            model.Id = Guid.NewGuid();
            model.DependantID = new Guid(Request.Query["DependantID"]);
            model.ProgramID = new Guid(Request.Query["ProgramID"]);
            if (!String.IsNullOrEmpty(model.FirstName)) { model.FirstName = model.FirstName.Trim(); }
            if (!String.IsNullOrEmpty(model.LastName)) { model.LastName = model.LastName.Trim(); }
            if (!String.IsNullOrEmpty(model.Telephone)) { model.Telephone = model.Telephone.Trim(); }
            if (!String.IsNullOrEmpty(model.Email)) { model.Email = model.Email.Trim(); }
            if (!String.IsNullOrEmpty(model.Relation)) { model.Relation = model.Relation.Trim(); }

            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.EffectiveDate = DateTime.Now;
            model.Active = true;

            #region duplicate-check
            var validation = _member.GetNextOfKinValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            var v4 = 0;
            foreach (var item in validation)
            {
                if (model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone && model.Email == item.Email && model.Relation == item.Relation) { v4++; break; }
                if (model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone && model.Email == item.Email) { v3++; break; }
                if (model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone) { v2++; break; }
                if (model.FirstName == item.FirstName && model.LastName == item.LastName) { v1++; break; }
            }
            if (v4 > 0)
            {
                ViewBag.validation = "level 4";
                ViewBag.fname = model.FirstName;
                ViewBag.lname = model.LastName;
                ViewBag.telephone = model.Telephone;
                ViewBag.email = model.Email;
                ViewBag.relation = model.Relation;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.validation = "level 3";
                ViewBag.fname = model.FirstName;
                ViewBag.lname = model.LastName;
                ViewBag.telephone = model.Telephone;
                ViewBag.email = model.Email;
                ViewBag.relation = model.Relation;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.validation = "level 2";
                ViewBag.fname = model.FirstName;
                ViewBag.lname = model.LastName;
                ViewBag.telephone = model.Telephone;
                ViewBag.email = model.Email;
                ViewBag.relation = model.Relation;
                return View(model);
            }
            if (v1 > 0)
            {
                ViewBag.validation = "level 1";
                ViewBag.fname = model.FirstName;
                ViewBag.lname = model.LastName;
                ViewBag.telephone = model.Telephone;
                ViewBag.email = model.Email;
                ViewBag.relation = model.Relation;
                return View(model);
            }
            #endregion

            _member.InsertNextOfKin(model);

            return RedirectToAction("PatientCommunication", new { DependentID = model.DependantID, pro = model.ProgramID });
        }

        public ActionResult Edit_NextOfKin(Guid id)
        {
            var model = _member.GetNextOfKinByID(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_NextOfKin(NextOFKin model)
        {
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            model.EffectiveDate = DateTime.Now;

            if (!String.IsNullOrEmpty(model.FirstName)) { model.FirstName = model.FirstName.Trim(); }
            if (!String.IsNullOrEmpty(model.LastName)) { model.LastName = model.LastName.Trim(); }
            if (!String.IsNullOrEmpty(model.Telephone)) { model.Telephone = model.Telephone.Trim(); }
            if (!String.IsNullOrEmpty(model.Email)) { model.Email = model.Email.Trim(); }
            if (!String.IsNullOrEmpty(model.Relation)) { model.Relation = model.Relation.Trim(); }

            #region duplicate-check
            var validation = _member.GetNextOfKinValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            var v4 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone && model.Email == item.Email && model.Relation == item.Relation) { v4++; break; }
                if (model.Id != item.Id && model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone && model.Email == item.Email) { v3++; break; }
                if (model.Id != item.Id && model.FirstName == item.FirstName && model.LastName == item.LastName && model.Telephone == item.Telephone) { v2++; break; }
                if (model.Id != item.Id && model.FirstName == item.FirstName && model.LastName == item.LastName) { v1++; break; }
            }
            if (v4 > 0)
            {
                ViewBag.validation = "level 4";
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.validation = "level 3";
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.validation = "level 2";
                return View(model);
            }
            if (v1 > 0)
            {
                ViewBag.validation = "level 1";
                return View(model);
            }
            #endregion
            _member.UpdateNextOfKin(model);

            return RedirectToAction("PatientCommunication", new { DependentID = model.DependantID, pro = model.ProgramID });

        }

        public ActionResult Details_NextOfKin(Guid id)
        {
            var model = _member.GetNextOfKinByID(id);
            var program = _admin.GetProgramById(model.ProgramID);
            ViewBag.Program = program.ProgramName;
            return View(model);
        }

        public ActionResult NOKValidation01(string firstname, string lastname)
        {
            var options = _member.GetNOKValidation01(firstname, lastname);

            return Json(options);
        }
        public ActionResult NOKValidation02(string firstname, string lastname, string telephone)
        {
            var options = _member.GetNOKValidation02(firstname, lastname, telephone);

            return Json(options);
        }
        public ActionResult NOKValidation03(string firstname, string lastname, string telephone, string email)
        {
            var options = _member.GetNOKValidation03(firstname, lastname, telephone, email);

            return Json(options);
        }
        public ActionResult NOKValidation04(string firstname, string lastname, string telephone, string email, string relation)
        {
            var options = _member.GetNOKValidation04(firstname, lastname, telephone, email, relation);

            return Json(options);
        }
        #endregion

        public ActionResult email_details(int id) //hcare-1380
        {
            var model = _admin.GetEmailDetailByID(id);

            var history = _admin.GetEmailAttachmentHistoryByEmailID(model.Id);
            var attachments = new List<AttachmentTemplate>();

            foreach (var item in history)
            {
                var file = _admin.GetAttachmentTemplateByID(item.TemplateID);
                attachments.Add(file);
            }

            model.Attachments = attachments;

            return View(model);
        }
        public ActionResult textmessage_details(int id) //hcare-1380
        {
            var model = _admin.GetTextMessageByID(id);
            var program = _admin.GetProgramById(model.programId);
            ViewBag.Program = program.ProgramName;

            return View(model);
        }

        public ActionResult EmailAttachmentHistoryInsert(string templateid, string dependantid, string programid, bool status) //hcare-1380
        {
            var model = _admin.GetEmailAttachmentByID(new Guid(templateid), new Guid(dependantid), new Guid(programid));

            if (model != null)
            {
                if (model.Active != status)
                {
                    _admin.UpdateEmailAttachmentHistory(new EmailAttachmentHistory()
                    {
                        Id = model.Id,
                        TemplateID = model.TemplateID,
                        ModifiedBy = User.Identity.Name,
                        ModifiedDate = DateTime.Now,
                        Active = status,
                    });
                }
                return Json("update");
            }
            else
            {
                _admin.InsertEmailAttachmentHistory(new EmailAttachmentHistory()
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    TemplateID = new Guid(templateid),
                    DependantID = new Guid(dependantid),
                    ProgramID = new Guid(programid),
                    Active = status,
                });
                return Json("insert");
            }
        }
        public ActionResult EmailAttachmentHistoryDetails(string templateid) //hcare-1380
        {
            var model = _admin.GetAttachmentTemplateByID(new Guid(templateid));
            return Json(model);
        }
        public ActionResult EmailAttachmentHistoryReset(string dependantid, string programid) //hcare-1380
        {
            var model = _admin.ResetAttachmentEmailHistory(new Guid(dependantid), new Guid(programid));
            return Json(model);
        }

        [HttpPost]

        public ActionResult ProfileEmail(ComsViewModel model, Guid pro) //hcare-1380
        {
            var dependantID = Request.Query["e1-dependantid"];
            var programID = Request.Query["e1-programid"];
            var medicalaidID = Request.Query["e1-medicalaidid"];
            //var planID = Request.Query["planID"];

            var emailhistory = _admin.GetEmailAttachmentByDependantID(new Guid(dependantID), new Guid(programID), DateTime.Today);

            #region email-insert
            model.emailMessages.createdDate = DateTime.Now;
            model.emailMessages.effectivedate = DateTime.Now;
            model.emailMessages.createdBy = User.Identity.Name;
            model.emailMessages.programId = pro; //HCare-1254
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
                if (emailsend == "sent")
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



            return RedirectToAction("patientCommunication", new { DependentID = model.emailMessages.dependantID, pro = Request.Query["pro"] });
        }

        [HttpPost]

        public ActionResult DoctorEmail(ComsViewModel model, Guid pro) //hcare-1391
        {
            var dependantID = Request.Query["e2-dependantid"];
            var programID = Request.Query["e2-programid"];
            var medicalaidID = Request.Query["e2-medicalaidid"];
            var planID = Request.Query["planID"];

            var emailhistory = _admin.GetEmailAttachmentByDependantID(new Guid(dependantID), new Guid(programID), DateTime.Today);

            #region email-insert
            model.emailMessages.emailTo = Request.Query["doctor-email-to"];
            model.emailMessages.cc = Request.Query["doctor-email-cc"];
            model.emailMessages.createdDate = DateTime.Now;
            model.emailMessages.effectivedate = DateTime.Now;
            model.emailMessages.createdBy = User.Identity.Name;
            model.emailMessages.programId = pro; //HCare-1254
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

            return RedirectToAction("patientCommunication", new { DependentID = model.emailMessages.dependantID, pro = Request.Query["pro"] });
        }

        public ActionResult ProfilePopRecord(string dependantID, string programID, string username, bool check) //hcare-1374
        {
            var model = _member.GetMemberRecordByDependantID(new Guid(dependantID), new Guid(programID), username);

            if (model != null && model.CreatedDate.ToString("dd MM yyyy") == DateTime.Today.ToString("dd MM yyyy") && model.ProgramID == new Guid(programID))
            {
                model.ProfilePopUp = check;
                model.ModifiedBy = User.Identity.Name;
                model.ModifiedDate = DateTime.Now;

                _member.UpdateMemberRecord(model);

                return Json(model);
            }
            else
            {
                var insert = new MemberRecord();

                insert.Id = Guid.NewGuid();
                insert.DependantID = new Guid(dependantID);
                insert.AccessedBy = User.Identity.Name;
                insert.AccessedDate = DateTime.Now;
                insert.ProfilePopUp = check;
                insert.ProgramID = new Guid(programID);
                insert.CreatedBy = User.Identity.Name;
                insert.CreatedDate = DateTime.Now;
                insert.Active = true;

                _member.InsertMemberRecord(insert);

                return Json(model);
            }
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

        public ActionResult FillEmailLayoutByID(Guid id)
        {
            var options = _member.GetEmailLayoutByID(id);
            return Json(options);
        }

        public ActionResult EmailTemplateBuild(string templateID)
        {
            var emailTemplate = _member.GetEmailTemplateByID(Convert.ToInt32(templateID));

            var vResults = new LayoutvsEmailTemplateVM();
            vResults.EmailTemplate = emailTemplate[0].templateBody;

            return Json(vResults);
        }

        public ActionResult DoctorEmailDetails_01(string doctorname, string practicenumber, string practicename) //hcare-1391
        {
            var model = _admin.GetDoctorDetails(doctorname, practicenumber, practicename);
            return Json(model);
        }
        public ActionResult GetDoctorEmailInformation(string doctorID) //hcare-1391
        {
            var model = _admin.GetDoctorByDoctorID(new Guid(doctorID));
            return Json(model);
        }

        [HttpPost]
        public ActionResult Import(IFormFile excelfile)
        {

            List<string> erorrows = new List<string>();
            if (excelfile == null || excelfile.Length == 0)
            {
                ViewBag.Error = "Please select a excel file";
                return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["DependantID"].ToString()), pro = Request.Query["pro"] });
            }
            else
            {

                if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
                {

                    string fileName = excelfile.FileName;
                    string fileContentType = excelfile.ContentType;
                    byte[] fileBytes = new byte[excelfile.Length];
                    var data = excelfile.InputStream.Read(fileBytes, 0, Convert.ToInt32(excelfile.Length));

                    using (var package = new ExcelPackage(excelfile.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int row = 2; row <= noOfRow; row++)
                        {
                            if (noOfCol == 9)
                            {
                                if (_member.GetMemberBydependentIDAndMemberNumber(new Guid(Request.Query["DependantID"]), workSheet.Cells[row, 2].Value.ToString().Trim()))
                                {
                                    try
                                    {
                                        ImportCommunicationModel comunication = new ImportCommunicationModel();
                                        comunication.Scheme = workSheet.Cells[row, 1].Value.ToString();
                                        comunication.MemberNumber = workSheet.Cells[row, 2].Value.ToString();
                                        comunication.Depcode = workSheet.Cells[row, 3].Value.ToString();
                                        comunication.ProfileNumber = workSheet.Cells[row, 4].Value.ToString();
                                        comunication.Type = workSheet.Cells[row, 5].Value.ToString();
                                        comunication.Description = workSheet.Cells[row, 6].Value.ToString();
                                        comunication.Details = workSheet.Cells[row, 7].Value.ToString();
                                        comunication.messageTo = workSheet.Cells[row, 8].Value.ToString();
                                        comunication.DateSent = Convert.ToDateTime(DateTime.FromOADate(double.Parse(workSheet.Cells[row, 9].Value.ToString())).ToString("yyyy/MM/dd"));
                                        comunication.Createdby = User.Identity.Name;
                                        comunication.ProgramId = new Guid(Request.Query["pro"]);
                                        comunication.DepedentID = new Guid(Request.Query["DependantID"]);

                                        if (_member.GetPrograms().Where(x => x.programID == new Guid(Request.Query["pro"])).Select(x => x.code).FirstOrDefault() != "DIABD" &&
                                            comunication.Type.ToLower().Contains("diabetic diary"))
                                            erorrows.Add("Row " + row + "Diabetic diary it is only applicable on Diabetes program");
                                        var res = _member.ImportCommunication(comunication);

                                        if (!res)
                                            erorrows.Add("Row " + row + "Invalid communication Type");
                                    }
                                    catch (Exception ex)
                                    {
                                        TempData["Error"] = "Row " + row + ex;
                                    }


                                }
                                else
                                {
                                    erorrows.Add("Row " + row + " Invalid member number");
                                    ViewBag.Error = "";
                                }


                            }
                            else
                            {
                                erorrows.Add("Row " + row + " Incorrect number of columns");
                            }
                        }

                        TempData["ErrorlList"] = erorrows;

                    }
                    return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["DependantID"].ToString()), pro = Request.Query["pro"] });
                }
                else
                {
                    TempData["Error"] = "File type is incorrect <br>";
                    return RedirectToAction("patientCommunication", new { DependentId = new Guid(Request.Query["DependantID"].ToString()), pro = Request.Query["pro"] });
                }
            }


        }
    }
}
