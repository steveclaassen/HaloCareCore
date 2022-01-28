using Dapper;
using HaloCareCore.Helpers;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HaloCareCore.DAL
{
    public class MemberRepository : IMemberRepository
    {
        private OH17Context _context;
        private IAdminRepository _admin;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private readonly IConfiguration Configuration;

        public MemberRepository(IConfiguration configuration, OH17Context context)
        {
            this._context = context;
            _admin = new AdminRepository(context, configuration);

        }

        public List<EnquiryFullViewModel> GetEnquiryFullView(int queryID, Guid depID)
        {
            var results = (from q in _context.Queries
                           where q.queryID == queryID
                           where q.dependentID == depID
                           select new EnquiryFullViewModel()
                           {
                               dependantID = q.dependentID,
                               enquiryId = q.queryID,
                               effectiveDate = q.effectiveDate,
                               enquiryRefNumber = q.queryID.ToString(),
                               TextInformation = q.query,
                               enquiryBy = _context.QueryTypes.Where(x => x.code == q.enquiryBy).Select(x => x.queryDescription).FirstOrDefault(),
                               enquiryTypeName = _context.EnquiryTypes.Where(x => x.enquiryType == q.queryType).Select(x => x.enquiryName).FirstOrDefault(),
                               createdBy = q.createdBy,
                               status = q.queryStatus ? "Open" : "Closed",
                               owner = _context.Users.Where(x => x.userID.ToString() == q.Owner).Select(x => x.username).FirstOrDefault(),
                           }).ToList();

            var messages = (from s in _context.SmsMessages
                            where s.ReferenceNumber == queryID.ToString()
                            where s.dependantID == depID
                            select new EnquiryFullViewModel()
                            {
                                dependantID = s.dependantID,
                                effectiveDate = s.effectiveDate,
                                enquiryRefNumber = s.ReferenceNumber,
                                TextInformation = s.message,
                                createdBy = s.createdBy,
                                status = s.status,
                            }).ToList();

            foreach (var message in messages)
            {
                results.Add(message);
            }

            var emails = (from s in _context.Emails
                          where s.ReferenceNumber == queryID.ToString()
                          where s.dependantID == depID
                          select new EnquiryFullViewModel()
                          {
                              dependantID = s.dependantID,
                              effectiveDate = s.effectivedate,
                              enquiryRefNumber = s.ReferenceNumber,
                              TextInformation = s.emailMassage,
                              createdBy = s.createdBy,
                              status = s.status
                          }).ToList();

            foreach (var email in emails)
            {
                results.Add(email);
            }
            var comments = (from s in _context.EnquiryComments
                            where s.queryId == queryID
                            select new EnquiryFullViewModel()
                            {
                                effectiveDate = s.effectiveDate,
                                enquiryRefNumber = s.ReferenceNumber,
                                TextInformation = s.comment,
                                createdBy = s.createdBy,
                                status = s.active.ToString()
                            }).ToList();

            foreach (var comment in comments)
            {
                results.Add(comment);
            }

            return results.OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<MedicalAidPlans> GetPlans(Guid MedicalAidId)
        {
            return _context.MedicalAidPlans.Where(x => x.medicalAidId == MedicalAidId).ToList();
        }

        public List<Programs> GetPrograms()
        {
            return _context.Program.Where(x => x.Active == true).OrderBy(x => x.ProgramName).ToList();
        }

        public List<Programs> GetAllowedPrograms()
        {
            var results = _context.Program.OrderByDescending(x => x.Active == true).ThenBy(x => x.ProgramName).ToList();
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var programlist = rights.accessList.Select(x => x.programId).ToList();
            var filteredRes = (from r in results
                               where programlist.Contains(r.programID)
                               select r).ToList();
            return filteredRes;

        }

        public List<Programs> GetAllowedProgramsPerScheme(Guid medId)
        {
            var programs = _context.Program.OrderByDescending(x => x.Active == true).ThenBy(x => x.ProgramName).ToList();
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var programlist = rights.accessList.Where(x => x.medicalAidId == medId).Select(x => x.programId).ToList();

            var filteredRes = (from r in programs
                               where programlist.Contains(r.programID)
                               select r).ToList();

            return filteredRes;

        }

        public List<Programs> GetAllowedProgramsPerUser(Guid medId, Guid userID)
        {
            var programs = _context.Program.OrderByDescending(x => x.Active == true).ThenBy(x => x.ProgramName).ToList();
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.userID == userID).Select(x => x.userID).FirstOrDefault());
            if (rights != null)
            {

                var programlist = rights.accessList.Where(x => x.medicalAidId == medId).Select(x => x.programId).ToList();

                var filteredRes = (from r in programs
                                   where programlist.Contains(r.programID)
                                   select r).ToList();

                return filteredRes;
            }
            else
            {
                return null;
            }

        }

        public List<Programs> GetAllowedProgramsPerUserList(Guid userID) //hcare-1289
        {
            string sql = string.Format(@"select distinct p.ProgramName, p.programID
                                        from UserProgramAccess upa
                                        inner join Programs p on upa.programId = p.programID
                                        inner join MedicalAid ma on upa.medicalAidId = ma.MedicalAidID
                                        where upa.userId = '{0}' 
                                        and upa.Active = 1 
                                        and ma.Active = 1", userID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var program = (List<Programs>)db.Query<Programs>(sql, null, commandTimeout: 500);
                db.Close();
                return program;
            }
        }

        public List<MemberBasicViewModel> GetPatientProgramHistoryById(Guid dependantid) //HCare-1121
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var results = (from d in _context.Dependants
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join pp in _context.PatientProgramHistory on d.DependantID equals pp.dependantId into ppo
                           from pp in ppo.DefaultIfEmpty()
                           join p in _context.Program on pp.programCode equals p.code into po
                           from p in po.DefaultIfEmpty()
                           where d.DependantID == dependantid
                           select new MemberBasicViewModel
                           {
                               MedicalAidID = ma.MedicalAidID,
                               dependantId = d.DependantID,
                               membershipNo = m.membershipNo,
                               dependentCode = d.dependentCode,
                               programID = p.programID,
                               programCode = pp.programCode,
                               Scheme = ma.Name,
                               programName = p.ProgramName,
                           }).ToList();


            var progList = rights.accessList.Where(x => x.medicalAidId == results[0].MedicalAidID).Select(x => x.programId).ToList();
            var include = new List<MemberBasicViewModel>();
            var exclude = new List<MemberBasicViewModel>();
            foreach (var program in results)
            {
                var remove = true;
                if (progList.Contains(program.programID))
                {
                    remove = false;
                }
                if (remove) { exclude.Add(program); }
                else { include.Add(program); }
            }

            List<MemberBasicViewModel> distinctList = include.GroupBy(p => p.programID).Select(g => g.First()).ToList();

            return distinctList;

        }

        public List<PatientProgramHistory> GetPatientProgramHistory()
        {
            return _context.PatientProgramHistory.Where(x => x.active == true).OrderBy(x => x.icd10Code).ToList();
        }

        public PatientProgramHistory GetPatientProgramHistoryByID(int id)
        {
            return _context.PatientProgramHistory.Where(x => x.id == id).First();
        }

        public List<AccessServices> GetAccessServices()
        {
            return _context.AccessServices.Where(x => x.Active == true).OrderBy(x => x.name).ToList();
        }

        public List<AssignmentTypes> GetAssignmentTypes()
        {
            return _context.AssignmentTypes.Where(x => x.active == true).ToList();
        }

        public List<AssignmentItemTypes> GetAssignmentItemTypes()
        {
            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            var results = _context.AssignmentItemTypes.Where(x => x.active == true).ToList();
            results = (from r in results
                       where allowedItems.Contains(r.assignmentItemType)
                       select r).ToList();

            return results;
        }

        public List<AssignmentItemTypes> GetAssignmentItemTypesForUser(Guid userID)
        {
            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            var results = _context.AssignmentItemTypes.Where(x => x.active == true).ToList();
            results = (from r in results
                       where allowedItems.Contains(r.assignmentItemType)
                       select r).ToList();

            return results.OrderBy(x => x.itemDescription).ToList();
        }

        public List<AssignmentItemTypes> GetAssignmentItems()
        {
            return _context.AssignmentItemTypes.Where(x => x.active == true).ToList();
        }

        public Users GetUserNameByUserID(Guid userID)
        {
            return _context.Users.Where(x => x.userID == userID).FirstOrDefault();
        }

        public wfSettings GetQueueNameByQueueID(Guid queueID)
        {
            return _context.wfSettings.Where(x => x.QueueID == queueID).FirstOrDefault();
        }

        //public List<AssignmentItemTypes> GetAssignmentItemTypesByProgram(Guid pro)
        //{
        //    var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
        //    var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();
        //    //var program = _context.Program.Where(x => x.programID == pro).FirstOrDefault();

        //    var results = _context.AssignmentItemTypes.Where(x => x.active == true).ToList();
        //    results = (from r in results
        //               where allowedItems.Contains(r.assignmentItemType)
        //               //where r.program == program.code || r.program == null
        //               //where r.program.Contains(program.code) || r.program == null
        //               select r).ToList();

        //    #region selected-programs
        //    var programinfo = _context.Program.Where(x => x.programID == pro).FirstOrDefault();
        //        results = (from r in results
        //                   where r.program == null || programinfo.code == r.program
        //                   select r).ToList();
        //    #endregion

        //    return results;
        //}

        public List<AssignmentItemTypes> GetAssignmentItemTypesByProgram(Guid pro)
        {
            var programinfo = _context.Program.Where(x => x.programID == pro).FirstOrDefault();
            var results = _context.AssignmentItemTypes.Where(x => x.active == true).Where(x => x.program == programinfo.code || x.program == null).ToList();

            return results;
        }

        public List<AssignmentItemTaskTypes> GetAssignmentTaskTypes()
        {
            var results = _context.AssignmentItemTaskTypes.Where(x => x.active == true).ToList();
            return results;
        }

        public List<TaskTypeRequirements> GetTaskRequirement()
        {
            return _context.TaskTypeRequirements.Where(x => x.active == true).ToList();
        }

        public List<TaskRequirementItemLinking> GetTaskRequirements(string itemType)
        {
            return _context.TaskRequirementItemLinking.Where(x => x.itemId == itemType).Where(x => x.active == true).ToList();
        }

        public List<NoteTypes> GetNoteTypes()
        {
            return _context.NoteTypes.Where(x => x.active == true).OrderBy(x => x.noteDescription).ToList();
        }

        public List<PatientManagementStatusHistory> GrabManagementStatusByDependantID(Guid dependantID)
        {
            var result = _context.PatientManagementStatusHistory.Where(x => x.dependantId == dependantID).ToList();

            return result;
        }

        public ManagementStatusVM GetManagmentStatusByCode(string code)
        {
            var results = (from pm in _context.ManagementStatus
                           where pm.statusCode == code
                           select new ManagementStatusVM
                           {
                               codeStatus = pm.statusType,
                           }).FirstOrDefault();

            return results;
        }

        public List<ManagementStatusVM> GetManagmentStatusInformation(Guid dependantID)
        {
            var results = (from pm in _context.PatientManagementStatusHistory
                           join d in _context.Dependants on pm.dependantId equals d.DependantID
                           join m in _context.Members on d.memberID equals m.memberID
                           join ms in _context.ManagementStatus on pm.managementStatusCode equals ms.statusCode

                           where pm.dependantId == dependantID
                           select new ManagementStatusVM
                           {
                               id = pm.id,
                               dependantId = pm.dependantId,
                               endDate = pm.endDate,
                               createdBy = pm.createdBy,
                               createdDate = pm.createdDate,
                               modifiedBy = pm.modifiedBy,
                               modifiedDate = pm.modifiedDate,
                               active = pm.active,
                               effectiveDate = pm.effectiveDate,
                               managementStatusName = ms.statusName,
                               managementStatusCode = pm.managementStatusCode,
                               codeStatus = ms.statusType,
                               programCode = ms.programCode,
                               sentToCl = pm.sentToCl,

                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }


        public List<PatientManagementStatusHistory> GetManagementStatusByDependentId(Guid dependantID)
        {
            var responses = _context.PatientManagementStatusHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.effectiveDate).ToList();
            foreach (var response in responses)
            {
                response.managementStatusCode = _context.ManagementStatus.Where(x => x.statusCode == response.managementStatusCode).Select(x => x.statusName).FirstOrDefault();
            }

            return responses;
        }
        public List<PatientManagementStatusHistory> GetManagementStatusByDependent(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = (from pm in _context.PatientManagementStatusHistory
                           join d in _context.ManagementStatus on pm.managementStatusCode equals d.statusCode
                           where pm.dependantId == dependantID
                           where d.programCode == programCode
                           select pm).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public ManagementStatusVM GetManagementStatusByInformation(Guid dependantID, int id)
        {
            var results = (from pm in _context.PatientManagementStatusHistory
                           join ms in _context.ManagementStatus on pm.managementStatusCode equals ms.statusCode
                           where pm.dependantId == dependantID
                           where pm.id == id
                           select new ManagementStatusVM
                           {
                               dependantId = pm.dependantId,
                               managementStatusCode = pm.managementStatusCode,
                               codeStatus = ms.statusType.ToLower(),
                               effectiveDate = pm.effectiveDate,
                               endDate = pm.endDate,
                               active = pm.active,
                               programCode = ms.programCode

                           }).FirstOrDefault();

            return results;
        }

        public List<PatientManagementStatusHistory> GetManagementStatusByDependentId(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = (from pm in _context.PatientManagementStatusHistory
                           join d in _context.ManagementStatus on pm.managementStatusCode equals d.statusCode
                           where pm.dependantId == dependantID
                           where d.programCode == programCode
                           select pm).OrderByDescending(x => x.effectiveDate).ToList();

            foreach (var result in results)
            {
                result.managementStatusCode = _context.ManagementStatus.Where(x => x.statusCode == result.managementStatusCode).Select(x => x.statusName).FirstOrDefault();
            }

            return results;
        }

        public PatientManagementStatusHistory GetManagementStatusById(int id)
        {
            var result = _context.PatientManagementStatusHistory.Where(x => x.id == id).OrderByDescending(x => x.effectiveDate).FirstOrDefault();

            return result;
        }

        public DoctorQuestionnaireResults GetQuestionnaireById(int id)
        {
            var result = _context.DoctorQuestionnaireResults.Where(x => x.Active == true).Where(x => x.Id == id).OrderByDescending(x => x.createdDate).FirstOrDefault();

            return result;
        }

        public DoctorQuestionnaireViewModel GetFullQuestionnaireById(int id)
        {
            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join mi in _context.MemberInformation
                           on d.DependantID equals mi.dependantID
                           join a in _context.Address
                           on mi.addressID equals a.addressID
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID
                           join ce in _context.Clinical
                           on d.DependantID equals ce.dependantID
                           join mh in _context.MedicationHistory
                           on d.DependantID equals mh.dependantId
                           join qr in _context.DoctorQuestionnaireResults
                           on d.DependantID equals qr.dependentID
                           where qr.Id == id
                           select new DoctorQuestionnaireViewModel
                           {
                               clinicalexam = ce,
                               MedicationHistory = mh,

                           }).FirstOrDefault();

            return results;
        }

        public ServiceResult UpdateManagementStatus(PatientManagementStatusHistory model)
        {
            var oldstatus = _context.PatientManagementStatusHistory.Where(x => x.id == model.id).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            oldstatus.managementStatusCode = model.managementStatusCode;
            oldstatus.modifiedDate = model.modifiedDate;
            oldstatus.modifiedBy = model.modifiedBy;
            oldstatus.effectiveDate = model.effectiveDate;
            oldstatus.endDate = model.endDate;
            oldstatus.active = model.active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateMSRecord(PatientManagementStatusHistory model) //hcare-1265
        {
            var oldstatus = _context.PatientManagementStatusHistory.Where(x => x.id == model.id).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            oldstatus.managementStatusCode = model.managementStatusCode;
            oldstatus.modifiedDate = model.modifiedDate;
            oldstatus.modifiedBy = model.modifiedBy;
            oldstatus.effectiveDate = model.effectiveDate;
            oldstatus.endDate = model.endDate;
            oldstatus.sentToCl = model.sentToCl;
            oldstatus.active = model.active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateManagementStatusCode(ManagementStatus model)
        {
            var oldstatus = _context.ManagementStatus.Where(x => x.statusCode == model.statusCode).FirstOrDefault();
            oldstatus.statusName = model.statusName;
            oldstatus.modifiedBy = model.modifiedBy;
            oldstatus.modifiedDate = model.modifiedDate;
            oldstatus.active = model.active;
            oldstatus.statusType = model.statusType;
            oldstatus.programCode = model.programCode;
            return _context.SaveChanges();
        }

        public ServiceResult InsertManagementStatus(PatientManagementStatusHistory model)
        {
            model.active = true;
            model.createdDate = DateTime.Now;
            DateTime now = Convert.ToDateTime(model.effectiveDate);
            model.effectiveDate = new DateTime(now.Year, now.Month, now.Day, model.createdDate.Hour, model.createdDate.Minute, model.createdDate.Second, model.createdDate.Millisecond);
            _context.PatientManagementStatusHistory.Add(model);
            return SaveResult();
        }

        public ServiceResult InsertManagementStatusCode(ManagementStatus model)
        {
            model.active = true;
            _context.ManagementStatus.Add(model);
            return SaveResult();
        }

        public ManagementStatus GetMStatusByCode(string code) //HCare-956
        {
            return _context.ManagementStatus.Where(x => x.statusCode == code).FirstOrDefault();
        }

        public ManagementStatus GetMStatusByName(string description) //HCare-956
        {
            return _context.ManagementStatus.Where(x => x.statusName == description).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }


        public ServiceResult InsertAssignment(AssignmentsView model)
        {
            model.newAssignment.createdDate = DateTime.Now;
            model.newAssignment.Active = true;
            model.newAssignment.status = "Open";
            _context.Assignments.Add(model.newAssignment);
            SaveResult();

            var assignmentitem = new AssignmentItems()
            {
                assignmentId = model.newAssignment.assignmentID,
                itemType = model.assignmentItemType,
                createdDate = DateTime.Now,
                createdBy = model.newAssignment.createdBy,
                active = true,
            };
            _context.AssignmentItems.Add(assignmentitem);
            Save();
            model.itemID = assignmentitem.id;
            return SaveResult();
        }

        public ServiceResult InsertTask(AssignmentItemTasks task)
        {
            task.createdDate = DateTime.Now;
            task.active = true;
            task.effectiveDate = DateTime.Now;
            task.status = "Open";

            _context.AssignmentItemTasks.Add(task);
            return SaveResult();
        }

        public List<Queries> GetQueriesByDependant(Guid dependantID)
        {
            var model = _context.Queries.Where(x => x.dependentID == dependantID).OrderByDescending(x => x.effectiveDate).ToList();
            foreach (var item in model)
            {
                item.enquiryBy = _context.QueryTypes.Where(x => x.code == item.enquiryBy).Select(x => x.queryDescription).FirstOrDefault();
                item.queryType = _context.EnquiryTypes.Where(x => x.enquiryType == item.queryType).Select(x => x.enquiryName).FirstOrDefault();
            }
            return model;
        }
        //HCare=1061
        public List<DiabeticDairy> GetDiabeticDairyByDependant(Guid DependantID)
        {
            return _context.DiabeticDairy.Where(x => x.dependentID == DependantID).ToList();
        }



        public PatientProgramHistory GetProgramLatest(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderBy(x => x.effectiveDate).ToList();
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
                result.effectiveDate = results[0].effectiveDate;
            }
            return results.OrderByDescending(x => x.createdDate).ToList()[0];
        }

        public PatientProgramHistory GetLatestPatientProgram(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).FirstOrDefault();

            return results;
        }

        public List<PatientProgramHistory> GetProgramHistory(Guid dependantID)
        {
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.createdDate).ToList();
            foreach (var result in results)
            {
                //HCare-1122
                if (result.programCode == "MNTLH")
                {
                    var mntlCodes = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Select(x => x.icd10Code).ToList();
                    foreach (var code in mntlCodes)
                    {
                        result.icd10Code = result.icd10Code + ", " + code;
                    }
                }
                result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramHistory> GetProgramHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).Take(1).ToList();
            foreach (var result in results)
            {
                var subhistories = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).ToList();
                foreach (var subhistory in subhistories)
                {
                    result.icd10Code = result.icd10Code + ", " + subhistory.icd10Code;
                }
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramHistory> GetValidProgramHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).Where(x => x.endDate > DateTime.Today || x.endDate == null).OrderByDescending(x => x.createdDate).Take(1).ToList();
            foreach (var result in results)
            {
                var subhistories = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).Where(x => x.endDate > DateTime.Today).OrderByDescending(x => x.createdDate).ToList();
                foreach (var subhistory in subhistories)
                {
                    result.icd10Code = result.icd10Code + ", " + subhistory.icd10Code;
                }
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results.OrderByDescending(x => x.effectiveDate).ToList();//hcare-1070
        }

        public List<PatientProgramHistory> GetMHDiagnosisHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = new List<PatientProgramHistory>();
            if (programCode.ToLower() == "mntlh")
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).OrderByDescending(x => x.createdDate).Take(1).ToList();
            }
            else
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).Where(x => x.endDate > DateTime.Today || x.endDate == null).OrderByDescending(x => x.createdDate).Take(1).ToList();
            }
            foreach (var result in results)
            {
                var mhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today).OrderByDescending(x => x.CreatedDate).ToList();
                foreach (var mh in mhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.icd10Code + ' ' + result.conditionCode).ToList())
                {
                    result.icd10Code = result.icd10Code + ", " + mh.ICD10Code;
                }
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramVM> Get_MH_Diagnosis_History(Guid dependantID, Guid Id) //hcare-1203
        {
            var programcode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = new List<PatientProgramVM>();

            results = (from mhd in _context.MentalHealthDiagnoses
                       where mhd.DependantID == dependantID
                       where mhd.ProgramCode == programcode

                       select new PatientProgramVM
                       {
                           Id = mhd.Id,
                           DependantID = mhd.DependantID,
                           ProgramCode = mhd.ProgramCode,
                           EffectiveDate = mhd.EffectiveDate,
                           EndDate = mhd.EndDate,
                           ICD10Code = mhd.ICD10Code,
                           ConditionCode = mhd.ConditionCode,
                           Comment = mhd.Comment,
                           CreatedBy = mhd.CreatedBy,
                           CreatedDate = mhd.CreatedDate,
                           ModifiedBy = mhd.ModifiedBy,
                           ModifiedDate = mhd.ModifiedDate,
                           Active = mhd.Active,

                       }).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today || x.EndDate == null).OrderByDescending(x => x.CreatedDate).Take(1).ToList();

            var activemhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programcode).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today || x.EndDate == null).OrderByDescending(x => x.CreatedDate).ToList();
            var allmhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programcode).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

            if (activemhdiagnosis.Count == 0)
            {
                results = (from mhd in _context.MentalHealthDiagnoses
                           where mhd.DependantID == dependantID
                           where mhd.ProgramCode == programcode

                           select new PatientProgramVM
                           {
                               Id = mhd.Id,
                               DependantID = mhd.DependantID,
                               ProgramCode = mhd.ProgramCode,
                               EffectiveDate = mhd.EffectiveDate,
                               EndDate = mhd.EndDate,
                               ICD10Code = mhd.ICD10Code,
                               ConditionCode = mhd.ConditionCode,
                               Comment = mhd.Comment,
                               CreatedBy = mhd.CreatedBy,
                               CreatedDate = mhd.CreatedDate,
                               ModifiedBy = mhd.ModifiedBy,
                               ModifiedDate = mhd.ModifiedDate,
                               Active = mhd.Active,


                           }).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).Take(1).ToList();

                foreach (var result in results)
                {
                    foreach (var mh in allmhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.ICD10Code + ' ' + result.ConditionCode))
                    {
                        var modifiedDate = allmhdiagnosis.OrderByDescending(t => t.ModifiedDate).Select(x => x.ModifiedDate).First();
                        var lastModifiedDate = modifiedDate.Value.ToString("dd-MM-yyyy");
                        var mhModifiedDate = mh.ModifiedDate.Value.ToString("dd-MM-yyyy");
                        if (mhModifiedDate == lastModifiedDate)
                        {
                            result.ICD10Code = result.ICD10Code + ", " + mh.ICD10Code;
                        }
                    }
                    result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                }
            }
            else
            {
                foreach (var result in results)
                {
                    foreach (var mh in activemhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.ICD10Code + ' ' + result.ConditionCode))
                    {
                        result.ICD10Code = result.ICD10Code + ", " + mh.ICD10Code;
                    }
                    result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                }
            }

            return results;

        }
        public List<PatientProgramVM> Get_Program_History(Guid dependantID, Guid Id) //hcare-1203
        {
            var programcode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();

            var results = new List<PatientProgramVM>();

            var pphistory = (from pph in _context.PatientProgramHistory
                             where pph.dependantId == dependantID
                             where pph.programCode == programcode
                             where pph.active == true
                             //where pph.endDate >= DateTime.Today || pph.endDate == null

                             select new PatientProgramVM
                             {
                                 Id = pph.id,
                                 DependantID = pph.dependantId,
                                 ProgramCode = pph.programCode,
                                 EffectiveDate = pph.effectiveDate,
                                 EndDate = pph.endDate,
                                 ICD10Code = pph.icd10Code,
                                 ConditionCode = pph.conditionCode,
                                 Comment = pph.Comment,
                                 CreatedBy = pph.createdBy,
                                 CreatedDate = pph.createdDate,
                                 ModifiedBy = pph.modifiedBy,
                                 ModifiedDate = pph.modifiedDate,
                                 Active = pph.active,

                             }).OrderByDescending(x => x.CreatedDate).ToList();

            var createdDate = pphistory.OrderByDescending(t => t.CreatedDate).Select(x => x.CreatedDate).First();

            if (pphistory.Count > 1)
            {
                var open = 0;
                var closed = 0;

                foreach (var result in pphistory)
                {
                    if (result.EndDate >= DateTime.Today || result.EndDate == null) { open++; }
                    else { closed++; }
                }
                if (open > 0)
                {
                    foreach (var result in pphistory.Where(x => x.EndDate >= DateTime.Now || x.EndDate == null))
                    {
                        results.Add(result);
                        result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                        break;
                    }
                }
                if (open == 0 && closed > 0)
                {
                    foreach (var result in pphistory.Where(x => x.EndDate < DateTime.Now && x.CreatedDate == createdDate))
                    {
                        results.Add(result);
                        result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                        break;
                    }
                }
            }
            else
            {
                foreach (var result in pphistory)
                {
                    results.Add(result);
                    result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                }
            }

            return results;

        }
        public List<PatientProgramVM> Get_MH_Program_Diagnosis_History(Guid dependantID, Guid Id) //hcare-1203
        {
            var programcode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = new List<PatientProgramVM>();

            results = (from mhd in _context.MentalHealthDiagnoses
                       join pph in _context.PatientProgramHistory on mhd.ICD10Code equals pph.icd10Code
                       where pph.dependantId == dependantID
                       where pph.programCode == programcode

                       select new PatientProgramVM
                       {
                           Id = pph.id,
                           DependantID = pph.dependantId,
                           ProgramCode = pph.programCode,
                           EffectiveDate = pph.effectiveDate,
                           EndDate = pph.endDate,
                           ICD10Code = mhd.ICD10Code,
                           ConditionCode = mhd.ConditionCode,
                           Comment = mhd.Comment,
                           CreatedBy = pph.createdBy,
                           CreatedDate = pph.createdDate,
                           ModifiedBy = mhd.ModifiedBy,
                           ModifiedDate = mhd.ModifiedDate,
                           Active = pph.active,

                       }).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).Take(1).ToList();

            var mhresults = (from mhd in _context.MentalHealthDiagnoses
                             where mhd.DependantID == dependantID
                             where mhd.ProgramCode == programcode

                             select new PatientProgramVM
                             {
                                 Id = mhd.Id,
                                 DependantID = mhd.DependantID,
                                 ProgramCode = mhd.ProgramCode,
                                 EffectiveDate = mhd.EffectiveDate,
                                 EndDate = mhd.EndDate,
                                 ICD10Code = mhd.ICD10Code,
                                 ConditionCode = mhd.ConditionCode,
                                 Comment = mhd.Comment,
                                 CreatedBy = mhd.CreatedBy,
                                 CreatedDate = mhd.CreatedDate,
                                 ModifiedBy = mhd.ModifiedBy,
                                 ModifiedDate = mhd.ModifiedDate,
                                 Active = mhd.Active,

                             }).Where(x => x.Active == true).Where(x => x.EndDate >= DateTime.Today || x.EndDate == null).OrderByDescending(x => x.CreatedDate).Take(1).ToList();

            var activemhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programcode).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today || x.EndDate == null).OrderByDescending(x => x.CreatedDate).ToList();
            var allmhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programcode).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

            if (activemhdiagnosis.Count == 0)
            {
                results = (from mhd in _context.MentalHealthDiagnoses
                           join pph in _context.PatientProgramHistory on mhd.ICD10Code equals pph.icd10Code
                           where mhd.DependantID == dependantID
                           where mhd.ProgramCode == programcode

                           select new PatientProgramVM
                           {
                               Id = pph.id,
                               DependantID = pph.dependantId,
                               ProgramCode = pph.programCode,
                               EffectiveDate = pph.effectiveDate,
                               EndDate = pph.endDate,
                               ICD10Code = mhd.ICD10Code,
                               ConditionCode = mhd.ConditionCode,
                               Comment = mhd.Comment,
                               CreatedBy = pph.createdBy,
                               CreatedDate = pph.createdDate,
                               ModifiedBy = mhd.ModifiedBy,
                               ModifiedDate = mhd.ModifiedDate,
                               Active = pph.active,

                           }).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).Take(1).ToList();

                var i_icd10code = results[0].ICD10Code; //hcare-1203-amendment
                var i_condiditioncode = results[0].ConditionCode; //hcare-1203-amendment

                foreach (var result in results)
                {
                    foreach (var mh in allmhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != i_icd10code + ' ' + i_condiditioncode))
                    {
                        var modifiedDate = allmhdiagnosis.OrderByDescending(t => t.ModifiedDate).Select(x => x.ModifiedDate).First();
                        var lastModifiedDate = modifiedDate.Value.ToString("dd-MM-yyyy");
                        var mhModifiedDate = mh.ModifiedDate.Value.ToString("dd-MM-yyyy");
                        if (mhModifiedDate == lastModifiedDate)
                        {
                            result.ICD10Code = result.ICD10Code + ", " + mh.ICD10Code;
                        }
                    }
                    result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                }
            }
            else
            {
                foreach (var mh in mhresults)
                {
                    results[0].ICD10Code = mh.ICD10Code;
                    results[0].ConditionCode = mh.ConditionCode;
                }

                foreach (var result in results)
                {
                    foreach (var mh in activemhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.ICD10Code + ' ' + result.ConditionCode))
                    {
                        result.ICD10Code = result.ICD10Code + ", " + mh.ICD10Code;
                    }
                    result.ProgramCode = _context.Program.Where(x => x.code == programcode).Select(x => x.ProgramName).FirstOrDefault();
                }
            }

            return results;

        }


        public List<PatientProgramHistory> GetMHDiagnosisHistory_pb(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = new List<PatientProgramHistory>();
            if (programCode.ToLower() == "mntlh")
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).OrderByDescending(x => x.createdDate).Take(1).ToList();
            }
            else
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).Where(x => x.endDate > DateTime.Today || x.endDate == null).OrderByDescending(x => x.createdDate).Take(1).ToList();
            }
            foreach (var result in results)
            {
                var mhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today).OrderByDescending(x => x.CreatedDate).ToList();
                foreach (var mh in mhdiagnosis.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.icd10Code + ' ' + result.conditionCode).ToList())
                {
                    result.icd10Code = result.icd10Code + ", " + mh.ICD10Code;
                }
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramHistory> GetPatientProgramHistory(Guid dependantID)
        {
            var results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.createdDate).Where(x => x.endDate > DateTime.Today || x.endDate == null).Where(x => x.active == true).ToList();
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramHistory> GetPatientProgramHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = new List<PatientProgramHistory>();
            if (programCode.ToLower() == "mntlh")
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            }
            else
            {
                results = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            }
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            //HCare-1297
            results[0].effectiveDate = results[results.Count - 1].effectiveDate;
            results[0].createdDate = results[results.Count - 1].createdDate;
            return results.Take(1).ToList();
        }

        public List<PatientProgramSubHistory> GetPatientProgramSubHistory(Guid dependantID)
        {
            var results = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.endDate > DateTime.Today || x.endDate == null).Where(x => x.active == true).ToList();
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramSubHistory> GetPatientProgramSubHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).Where(x => x.endDate > DateTime.Today || x.endDate == null).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public List<PatientProgramSubHistory> GetProgramSubHistory(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).ToList();
            //foreach (var result in results)
            //{
            //    result.icd10Code = _context.ComorbidConditionExclusions.Where(x => x.ICD10Code == result.icd10Code).Select(x => x.mappingDescription).FirstOrDefault();
            //}
            return results;
        }


        public List<PatientHistoryVM> GetProgramChangesHistory(Guid dependantID, Guid Id) //HCare-1195
        {
            var vmResult = new List<PatientHistoryVM>();
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var progHistory = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).ToList();
            var single = new PatientHistoryVM();
            single = new PatientHistoryVM(); //HCare-1203
            foreach (var result in progHistory)
            {
                if (result.endDate > DateTime.Now || result.endDate == null)
                {
                    single.effectiveDate = result.effectiveDate.Value;
                    single.icdAdded = result.icd10Code;
                }
                else if (result.modifiedBy != null)//Hcare-1195
                {
                    single.effectiveDate = result.effectiveDate.Value;
                    single.icdUpdated = result.icd10Code;
                }
                else
                {
                    single.effectiveDate = result.endDate.Value;
                    single.icdRemoved = result.icd10Code;
                }
                vmResult.Add(single);

            }
            var subs = _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).ToList();
            foreach (var result in subs)
            {
                single = new PatientHistoryVM(); //HCare-1203
                if (result.endDate > DateTime.Now || result.endDate == null)
                {
                    single.effectiveDate = result.effectiveDate.Value;
                    single.icdAdded = result.icd10Code;
                }
                else if (result.modifiedBy != null) //Hcare-1195
                {
                    single.effectiveDate = result.effectiveDate.Value;
                    single.icdUpdated = result.icd10Code;
                }
                else
                {
                    single.effectiveDate = result.effectiveDate.Value;
                    single.icdRemoved = result.icd10Code;
                }
                vmResult.Add(single);

            }
            return vmResult.OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<PatientHistoryVM> GetPatientProgramChangeHistory(Guid dependantID, Guid Id) //HCare-1203
        {
            var vmResult = new List<PatientHistoryVM>();

            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var progHistory = _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).Where(x => x.programCode == programCode).OrderByDescending(x => x.createdDate).Where(x => x.active == true).ToList();

            var single = new PatientHistoryVM();
            if (programCode == "MNTLH")
            {


                var mhdiagnosis = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).OrderByDescending(x => x.CreatedDate).ToList();

                var mhdX = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).ToList();
                foreach (var result in mhdiagnosis)
                {
                    single = new PatientHistoryVM();
                    if (result.EndDate < DateTime.Now)
                    {
                        single.effectiveDate = result.EffectiveDate.Value;
                        single.icdRemoved = result.ICD10Code;
                    }
                    else if (result.ModifiedDate != null)
                    {
                        single.effectiveDate = result.EffectiveDate.Value;
                        single.icdUpdated = result.ICD10Code;
                    }
                    else if (result.EndDate == null || result.EndDate >= DateTime.Now)
                    {
                        single.effectiveDate = result.EffectiveDate.Value;
                        single.icdAdded = result.ICD10Code;
                    }
                    vmResult.Add(single);
                }
            }
            else
            {
                foreach (var result in progHistory)
                {
                    single = new PatientHistoryVM();
                    if (result.endDate < DateTime.Now)
                    {
                        single.effectiveDate = result.effectiveDate.Value;
                        single.icdRemoved = result.icd10Code;
                    }
                    else if (result.modifiedBy != null)
                    {
                        single.effectiveDate = result.effectiveDate.Value;
                        single.icdUpdated = result.icd10Code;
                    }
                    else if (result.endDate == null || result.endDate >= DateTime.Now)
                    {
                        single.effectiveDate = result.effectiveDate.Value;
                        single.icdAdded = result.icd10Code;
                    }

                    vmResult.Add(single);
                }
            }

            return vmResult;
        }

        public List<PatientProgramHistory> GetProgramHistories(Guid dependantID)
        {
            return _context.PatientProgramHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.createdDate).ToList();
        }

        public List<MembershipNoHistory> GetMembershipNumberHistory(Guid dependantID)
        {
            return _context.MembershipNoHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.createdDate).ToList();
        }
        //public List<MemberImports> GetMemberImports(Guid dependantID)
        //{
        //    return _context.MemberImports.Where(x => x.DependantId == dependantID).OrderByDescending(x => x.createdDate).ToList();

        //}

        public List<MemberImports> GetMemberImports(Guid dependantID)
        {
            var results = _context.MemberImports.Where(x => x.DependantId == dependantID).OrderByDescending(x => x.createdDate).ToList();
            foreach (var result in results)
            {
                result.language = _context.Language.Where(x => x.iSeriesLanguageCode == result.language).Select(x => x.languageCode).FirstOrDefault();
            }

            return results;
        }

        public MemberImports GetMemberImportDetails(Guid depID)
        {
            var results = _context.MemberImports.Where(x => x.DependantId == depID).FirstOrDefault();

            results.language = _context.Language.Where(x => x.iSeriesLanguageCode == results.language).Select(x => x.languageCode).FirstOrDefault();

            return results;
        }

        public PatientStatusHistory GetPatientStatus(Guid depID)
        {
            return _context.PatientStatusHistory.Where(x => x.dependantId == depID).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
        }

        public ServiceResult InsertPatientStatus(PatientStatusHistory model)
        {
            model.active = true;
            _context.PatientStatusHistory.Add(model);
            return SaveResult();
        }

        public Member GetMemberByDependantID(Guid DepID)
        {
            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           where d.DependantID == DepID
                           select m).FirstOrDefault();
            return results;
        }

        public List<ManagementStatus> GetManagementStatusesByMedicalAid(Guid MedId)
        {
            var results = _context.ManagementStatus.Where(x => x.active == true).ToList();
            var valids = _context.AccountManagementStatuses.Where(x => x.SchemeId == MedId).Where(x => x.Active == true).Select(x => x.statusCode).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.statusCode)
                               select r).ToList();
            return filteredRes;
        }

        public List<ManagementStatus> GetManagementStatusesByMAandPro(Guid medID, Guid proID)
        {
            var program = _context.Program.Where(x => x.programID == proID).FirstOrDefault();

            var results = _context.ManagementStatus.Where(x => x.active == true).Where(x => x.programCode == program.code || x.programCode == null).ToList();
            var valids = _context.AccountManagementStatuses.Where(x => x.SchemeId == medID).Where(x => x.Active == true).Select(x => x.statusCode).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.statusCode)
                               select r).ToList();
            return filteredRes;
        }
        //Hcare-1043
        public List<SmsMessageTemplates> GetSmsMessagesByMedicalAid(Guid MedId, Guid pro)
        {
            var results = _context.SmsMessageTemplates.Where(x => x.Active == true).ToList();
            var valids = _context.AccountTextTemplates.Where(x => x.SchemeId == MedId).Where(x => x.Active == true).Where(x => x.ProgramId == pro).Select(x => x.templateId).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.templateID)
                               select r).ToList();
            return filteredRes;
        }
        //Hcare-1043
        public List<EmailTemplates> GetEmailMessagesByMedicalAid(Guid MedId, Guid pro)
        {
            var results = _context.EmailTemplates.Where(x => x.Active == true).ToList();
            var valids = _context.AccountEmailTemplates.Where(x => x.SchemeId == MedId).Where(x => x.Active == true).Where(x => x.ProgramId == pro).Select(x => x.templateId).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.templateID)
                               select r).ToList();
            return filteredRes;
        }

        //public EnrolmentViewModel GetDependentDetails(Guid dependantID)
        //{
        //    var results = _context.ManagementStatus.Where(x => x.active == true).ToList();
        //    var valids = _context.AccountManagementStatuses.Where(x => x.SchemeId == MedId).Where(x => x.Active == true).Select(x=>x.statusCode).ToList();
        //    var filteredRes = (from r in results
        //                       where valids.Contains(r.statusCode)
        //                       select r).ToList();
        //    return filteredRes;
        //}

        public EnrolmentViewModel GetDependentDetails(Guid dependantID, Guid? pro)
        {

            var programcode = "";
            if (pro != null)
            {
                programcode = _context.Program.Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
            }

            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join mi in _context.MemberInformation
                           on d.DependantID equals mi.dependantID into mInfo
                           from mi in mInfo.DefaultIfEmpty()
                           join a in _context.Address
                           on mi.addressID equals a.addressID into ad
                           from a in ad.DefaultIfEmpty()
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID into co
                           from c in co.DefaultIfEmpty()
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           join hr in _context.HypoglymiaRiskHistory
                           on d.DependantID equals hr.dependantID into hrInfo
                           from hr in hrInfo.DefaultIfEmpty()
                           where d.DependantID == dependantID
                           select new EnrolmentViewModel
                           {
                               member = m,
                               dependent = d,
                               address = a,
                               contact = c,
                               memberInformation = mi,
                               MedicalAids = _context.MedicalAids.Where(x => x.MedicalAidID == m.medicalAidID).ToList(),
                               medicalAid = _context.MedicalAids.Where(x => x.MedicalAidID == m.medicalAidID).FirstOrDefault(),
                               Notes = _context.Notes.Where(x => x.dependentID == d.DependantID).OrderByDescending(x => x.effectiveDate).Where(x => x.special == true).Where(x => x.expiryDate > DateTime.Now).FirstOrDefault(),
                               CaseManagers = _context.CaseManagerHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.createdDate).FirstOrDefault(),
                               enroll = _context.EnrolmentStepsMonitor.Where(x => x.dependantId == d.DependantID).FirstOrDefault(),
                               plan = _context.PatientPlanHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.createdDate).Select(x => x.planName).FirstOrDefault(),
                               RiskRating = _context.PatientRiskRatingHistory.Where(x => x.dependantID == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.RiskId).FirstOrDefault(), //HCare-695 //HCare-1158
                               RiskReason = _context.PatientRiskRatingHistory.Where(x => x.dependantID == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.reason).FirstOrDefault(), //HCare-695 //HCare-1158 
                               RiskProgram = programcode, //HCare-1033
                               hypoglymiaRiskHistory = hr,
                               patientStatusName = _context.PatientStatus.Where(p => p.Code == (_context.PatientStatusHistory.Where(x => x.dependantId == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.statusCode).FirstOrDefault())).Select(p => p.Name).FirstOrDefault(), //HCARE-869
                               program = _context.PatientProgramHistory.Where(x => x.dependantId == d.DependantID).FirstOrDefault(),
                               memberLanguage = _context.Language.Where(x => x.languageName == d.languageCode).Select(x => x.languageCode).FirstOrDefault(),
                               memberGender = _context.Sex.Where(x => x.sex == d.sex).Select(x => x.sexName).FirstOrDefault(),
                               memberDependantType = _context.DependantType.Where(x => x.dependentTypeCode == d.dependentType).Select(x => x.dependentType).FirstOrDefault(),
                               //medicalAid = _context.MedicalAids.Where(x => x.Active == true).FirstOrDefault(),
                               //doctor = _context.Doctors.Where(x => x.doctorID == mi.doctorID).OrderBy(x => x.createdDate).FirstOrDefault(),

                           }).FirstOrDefault();
            if (pro != null)
            {
                results.RiskRating = _context.PatientRiskRatingHistory.Where(x => x.dependantID == results.dependent.DependantID).Where(x => x.programType == programcode).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.RiskId).FirstOrDefault(); //HCare-695 //HCare-1158 
                results.RiskReason = _context.PatientRiskRatingHistory.Where(x => x.dependantID == results.dependent.DependantID).Where(x => x.programType == programcode).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.reason).FirstOrDefault(); //HCare-695 //HCare-1158
            }


            if (results.CaseManagers != null)
                results.CaseManagers.caseManagerId = _context.CaseManagers.Where(x => x.caseManagerNo == results.CaseManagers.caseManagerId).Select(x => x.caseManagerName + " " + x.caseManagerSurname).FirstOrDefault();

            return results;
        }
        public EnrolmentViewModel GetDependentMedicalAid(Guid dependantID, Guid pro)
        {

            var programcode = "";
            programcode = _context.Program.Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();


            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join mi in _context.MemberInformation on d.DependantID equals mi.dependantID into mInfo
                           from mi in mInfo.DefaultIfEmpty()
                           join a in _context.Address on mi.addressID equals a.addressID into ad
                           from a in ad.DefaultIfEmpty()
                           join c in _context.Contacts on mi.contactID equals c.ContactID into co
                           from c in co.DefaultIfEmpty()
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join hr in _context.HypoglymiaRiskHistory on d.DependantID equals hr.dependantID into hrInfo
                           from hr in hrInfo.DefaultIfEmpty()
                           where d.DependantID == dependantID
                           select new EnrolmentViewModel
                           {
                               member = m,
                               dependent = d,
                               address = a,
                               contact = c,
                               memberInformation = mi,
                               MedicalAids = _context.MedicalAids.Where(x => x.MedicalAidID == m.medicalAidID).ToList(),
                               medicalAid = _context.MedicalAids.Where(x => x.MedicalAidID == m.medicalAidID).FirstOrDefault(),
                               Notes = _context.Notes.Where(x => x.dependentID == d.DependantID).OrderByDescending(x => x.effectiveDate).Where(x => x.special == true).Where(x => x.expiryDate > DateTime.Now).FirstOrDefault(),
                               CaseManagers = _context.CaseManagerHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.createdDate).FirstOrDefault(),
                               enroll = _context.EnrolmentStepsMonitor.Where(x => x.dependantId == d.DependantID).FirstOrDefault(),
                               plan = _context.PatientPlanHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.createdDate).Select(x => x.planName).FirstOrDefault(),
                               RiskRating = _context.PatientRiskRatingHistory.Where(x => x.dependantID == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.RiskId).FirstOrDefault(), //HCare-695 //HCare-1158
                               RiskReason = _context.PatientRiskRatingHistory.Where(x => x.dependantID == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.reason).FirstOrDefault(), //HCare-695 //HCare-1158 
                               RiskProgram = programcode, //HCare-1033
                               hypoglymiaRiskHistory = hr,
                               patientStatusName = _context.PatientStatus.Where(p => p.Code == (_context.PatientStatusHistory.Where(x => x.dependantId == d.DependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.statusCode).FirstOrDefault())).Select(p => p.Name).FirstOrDefault(), //HCARE-869
                               program = _context.PatientProgramHistory.Where(x => x.dependantId == d.DependantID).FirstOrDefault(),
                               memberLanguage = _context.Language.Where(x => x.languageName == d.languageCode).Select(x => x.languageCode).FirstOrDefault(),
                               memberGender = _context.Sex.Where(x => x.sex == d.sex).Select(x => x.sexName).FirstOrDefault(),
                               memberDependantType = _context.DependantType.Where(x => x.dependentTypeCode == d.dependentType).Select(x => x.dependentType).FirstOrDefault(),

                           }).FirstOrDefault();

            return results;
        }

        public List<DoctorHistoryView> GetDrHistory(Guid dependantID)
        {
            var results = (from d in _context.PatientDoctorHistory
                           join doc in _context.Doctors on d.doctorId equals doc.doctorID
                           join di in _context.DoctorsInformation on d.doctorId equals di.DoctorID into mInfo
                           from di in mInfo.DefaultIfEmpty()
                           where d.dependantId == dependantID
                           select new DoctorHistoryView
                           {
                               id = d.id,
                               dependantId = d.dependantId,
                               doctorName = d.doctorName,
                               doctorId = doc.doctorID,
                               tel = di.WorkNumber, //HCare-1291
                               email = di.Email,
                               effectiveDate = d.effectiveDate,
                               cell = di.MobileNumber,
                               practiceNo = doc.practiceNo,
                               ProgramId = d.ProgramID //HCare-1386
                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public List<DoctorHistoryView> GetDoctorHistory(Guid dependantID)
        {
            var results = (from d in _context.PatientDoctorHistory
                           join doc in _context.Doctors on d.doctorId equals doc.doctorID
                           join di in _context.DoctorsInformation on d.doctorId equals di.DoctorID into mInfo
                           from di in mInfo.DefaultIfEmpty()
                           where d.dependantId == dependantID
                           select new DoctorHistoryView
                           {
                               id = d.id,
                               dependantId = d.dependantId,
                               doctorName = d.doctorName,
                               doctorId = doc.doctorID,
                               tel = di.WorkNumber, //HCare-1291
                               email = di.Email,
                               effectiveDate = d.effectiveDate,
                               cell = di.MobileNumber,
                               practiceNo = doc.practiceNo,
                               ProgramId = d.ProgramID //HCare-1386
                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public EnrolmentViewModel GetDependentDetails(string membershipno)
        {
            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join mi in _context.MemberInformation
                           on d.DependantID equals mi.dependantID
                           join a in _context.Address
                           on mi.addressID equals a.addressID
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID
                           join l in _context.Language
                           on d.languageCode equals l.languageCode
                           where m.membershipNo == membershipno
                           select new EnrolmentViewModel
                           {
                               member = m,
                               dependent = d,
                               address = a,
                               contact = c,
                               memberInformation = mi,
                               language = l,

                           }).FirstOrDefault();

            return results;
        }

        public List<AdvancedSearchResultModel> GetAdvancedSearchresults(AdvancedSearchView model)
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();


            string sql = @" SET DATEFORMAT YMD
                            SELECT d.DependantID, ma.medicalAidID, pr.programId, m.membershipNo as membershipNumber, d.dependentCode as DependantCode, d.firstName as memberName, d.lastName as memberSurname, 
							d.idNumber, ma.medicalAidCode, pr.code as program,                
                            (SELECT(CASE 
                            	-->> hcare-1299
                            	-->> program-check: is program mental health 
                            	WHEN (exists (SELECT top 1 * FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND ms.programCode = 'MNTLH' ORDER BY createdDate DESC))  
                            		THEN (CASE WHEN (exists(select * from MentalHealthDiagnosis where DependantID = d.DependantID and (EndDate >= GETDATE() or EndDate is NULL)))
                            		THEN (SELECT SUBSTRING((select ',' + ICD10Code as 'data()' from MentalHealthDiagnosis where DependantID = d.DependantID and programCode = 'MNTLH' and (EndDate >= GETDATE() or EndDate is NULL)  FOR XML PATH('') ), 2 , 9999))
                            		ELSE (SELECT SUBSTRING((select ',' + ICD10Code as 'data()' from MentalHealthDiagnosis where DependantID = d.DependantID and programCode = 'MNTLH' and convert(varchar(10), ModifiedDate, 120) = (select MAX(convert(varchar(10), ModifiedDate, 120)) from MentalHealthDiagnosis where DependantID = d.DependantID) FOR XML PATH('') ), 2 , 9999)) END)
                            	-->> hcare-1299
                            	-->> hcare-1140
                            	-->> program-check: not mental health: check-1: does the history contain a date greater than today
                            	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode))  
                            		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode)
                            	-->> check-2: does the history contain a date less than today
                            	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode  = ms.programCode ORDER BY createdDate DESC)) --hcare-1203
                            		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode = ms.programCode ORDER BY createdDate DESC) --hcare-1203
                            	-->> check-3: does the history contain a NULL end date
                            	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode))  
                            		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode )
                            	ELSE (SELECT top 1 icd10Code FROM PatientProgramHistory WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) END)) [icd10Code],
                            	-->> hcare-1140
                            		
									pph.planName as schemeOption,
                            	--(SELECT top 1 planName FROM PatientPlanHistory  WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) AS schemeOption,

                            	-->> hcare-1314
								(SELECT(CASE 
                            	WHEN (exists (SELECT top 1 ps.Name from PatientStatusHistory psh inner join PatientStatus ps on psh.statusCode = ps.Code WHERE psh.active = 1 AND psh.dependantId = d.DependantID order by psh.effectiveDate desc))
                            		THEN (SELECT top 1 ps.Name from PatientStatusHistory psh inner join PatientStatus ps on psh.statusCode = ps.Code WHERE psh.active = 1 AND psh.dependantId = d.DependantID order by psh.effectiveDate desc)
                            	ELSE ('') END)) [PatientStatus],
                            	-->> hcare-1314

                                ms.statusName as memberStatus, dr.practiceNo as drBHFNumber, dr.drFirstName as drName, dr.drLastName as drSurname, 
                                (SELECT top 1 labName FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID ORDER BY effectiveDate DESC) AS latestPathologyLab,
                                c.cell, c.landLine as tel, d.createdDate, s.effectiveDate as statusEffectiveDate, s.managementStatusCode as statusCode, (SELECT MIN(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId AND pr.code = programCode AND active = 1) as programStartDate,--HCare-1070
                                d.birthDate, s.modifiedBy as statusModifiedBy, s.modifiedDate as statusModifiedDate, s.endDate as StatusEndDate, 
                            
                            	-->hcare-1103 / hcare-1181
								di.Email as drEmail, di.MobileNumber as drCell,
                            	--(SELECT distinct top 1 Email FROM DoctorsInformation WHERE DoctorID = (SELECT distinct top 1 dv.DoctorID FROM [dbo].DoctorsInformation dv WHERE Dri.doctorId=dv.doctorID)) as drEmail,
                            	--(SELECT distinct top 1 MobileNumber FROM DoctorsInformation WHERE DoctorID = (SELECT distinct top 1 dv.DoctorID FROM [dbo].DoctorsInformation dv WHERE Dri.doctorId=dv.doctorID)) as drCell,
                            						
                                -->> hcare-1033 / hcare-1249
                            	(SELECT top 1 RiskName FROM RiskRatingTypes  WHERE RiskType = (SELECT TOP 1 RiskId From (SELECT TOP 2 * From PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND programType = pr.code ORDER BY effectiveDate))[RiskRatingHistory],--hcare-1074
                            	(SELECT top 1 RiskPriority FROM RiskRatingTypes  WHERE RiskType = (SELECT TOP 1 RiskId From (SELECT TOP 2 * From PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND programType = pr.code ORDER BY effectiveDate))[RiskPriorityHistory],--hcare-1074
                            	(SELECT TOP 1 effectiveDate From (SELECT TOP 2 * From PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND programType = pr.code ORDER BY effectiveDate)[RiskEffectiveHistory], --hcare-1074
                            
                            	(SELECT top 1 RiskName FROM RiskRatingTypes  WHERE RiskType = (SELECT top 1 RiskId FROM PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC)) [RiskRatingCurrent], --hcare-1074
                            	(SELECT top 1 RiskPriority FROM RiskRatingTypes  WHERE RiskType = (SELECT top 1 RiskId FROM PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC))[RiskPriorityCurrent], --hcare-1074
                            	(SELECT top 1 effectiveDate FROM PatientRiskRatingHistory  WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) [RiskEffectiveCurrent], --hcare-1074
                            	
                                (SELECT(CASE 
                            	-->> CHECK 1 - does the history contain a High risk(R)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'R' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))
                            			THEN ('RED')
                            		-->> CHECK 2 - does the history contain a Medium risk(Y)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'Y' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))  
                            			THEN ('YELLOW')
                            		-->> CHECK 3 - does the history contain a Low risk(G)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'G' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))  
                            			THEN ('GREEN')
                            		ELSE NULL END)) [OverallRating],
                            	(SELECT(CASE 
                            		-->> CHECK 1 - does the history contain a High risk(R)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'R' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))  
                            			THEN (SELECT top 1 RiskPriority FROM RiskRatingTypes WHERE active = 1 AND RiskType = 'R')
                            		-->> CHECK 2 - does the history contain a Medium risk(Y)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'Y' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))  
                            			THEN (SELECT top 1 RiskPriority FROM RiskRatingTypes  WHERE active = 1 AND RiskType = 'Y')
                            		-->> CHECK 3 - does the history contain a Low risk(G)
                            		WHEN (exists (select RiskId from PatientRiskRatingHistory where dependantID = d.DependantID and RiskId = 'G' and programType IN (select programCode from PatientProgramHistory where active = 1 and dependantId = d.DependantID) and active = 1 and effectiveDate in (select max(effectiveDate) from PatientRiskRatingHistory where dependantID = d.DependantID group by programType)))  
                            			THEN (SELECT top 1 RiskPriority FROM RiskRatingTypes  WHERE active = 1 AND RiskType = 'G')
                            		ELSE NULL END)) [OverallPriority],
                            	-->> hcare-1033 / hcare-1249

                            						(SELECT top 1 hba1c FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY hba1ceffectiveDate DESC) AS hba1c, --hcare-1322
                            		                (SELECT top 1 hba1ceffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY hba1ceffectiveDate DESC) AS hba1ceffectiveDate, --hcare-1322
                            		                (SELECT top 1 viralLoad FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY viralLoadeffectiveDate DESC) AS viralLoad, --hcare-1322
                            		                (SELECT top 1 viralLoadeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY viralLoadeffectiveDate DESC) AS viralLoadeffectiveDate, --hcare-1322
                                                    
													omh.cervicalCancer as cervicalCancer, omh.cervicalCancerComment as cervicalCancerComment, omh.cervicalCancerEffectiveDate as cervicalCancerEffectiveDate, qo.TBScreen as TBScreened, 
													qo.frailCareCheck as frailCareCheck, qo.frailCare as frailCare, qo.frailCareComment as frailCareComment,
													
													--(SELECT top 1 cervicalCancer FROM OtherMedicalHistory  WHERE active = 1 AND dependentID = d.DependantID AND pr.code = 'HIVPR' ORDER BY cervicalCancerEffectiveDate DESC) AS cervicalCancer,
                            						--(SELECT top 1 cervicalCancerComment FROM OtherMedicalHistory  WHERE active = 1 AND dependentID = d.DependantID AND pr.code = 'HIVPR' ORDER BY cervicalCancerEffectiveDate DESC) AS cervicalCancerComment,
                            						--(SELECT top 1 cervicalCancerEffectiveDate FROM OtherMedicalHistory WHERE active = 1 AND dependentID = d.DependantID AND pr.code = 'HIVPR' ORDER BY cervicalCancerEffectiveDate DESC) AS cervicalCancerEffectiveDate,
                            						--(SELECT top 1 TBScreen FROM QuestionnaireOther  WHERE active = 1 AND dependentID = d.DependantID ORDER BY createdDate DESC) AS TBScreened,
                            						--(SELECT top 1 frailCareCheck FROM QuestionnaireOther  WHERE active = 1 AND dependentID = d.DependantID ORDER BY createdDate DESC) AS frailCareCheck, --HCare-931
                                                    --(SELECT top 1 frailCare FROM QuestionnaireOther  WHERE active = 1 AND dependentID = d.DependantID ORDER BY createdDate DESC) AS frailCare, --HCare-931
                                                    --(SELECT top 1 frailCareComment FROM QuestionnaireOther  WHERE active = 1 AND dependentID = d.DependantID ORDER BY createdDate DESC) AS frailCareComment, --HCare-931

                            						-->> hcare-1135
                            						-->> suicide-risk-current
                            						(SELECT(CASE 
                            							-->> CHECK 1 - schizophrenia questionaire - created date
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 2 - bipolar questionaire - created date
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_BipolarResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_BipolarResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 3 - depression questionaire - created date
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								ELSE (SELECT '') END)) [SuicideRiskCurrent], 
                            						-->>
                            						-->> suicide-risk-history
                            						(SELECT(CASE 
                            							-->> CHECK 1 - schizophrenia questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_SchizophreniaResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_SchizophreniaResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            							-->> CHECK 2 - bipolar questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_BipolarResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_BipolarResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            							-->> CHECK 3 - depression questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_DepressionResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 SuicidalSC From (select Top 2 * from MH_DepressionResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            								---->>
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 1 - Question 1 bipolar questionaire
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								-->> CHECK 1 - Question 1 depression questionaire
                            							WHEN (exists (SELECT top 1 SuicidalSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 SuicidalSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								---->>
                            							ELSE (SELECT '') END))[SuicideRiskHistory],
                            						-->>
                            						-->> depression-current
                            						(SELECT(CASE 
                            							-->> CHECK 1 - schizophrenia questionaire - created date
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 2 - bipolar questionaire - created date
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 3 - depression questionaire - created date
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								ELSE (SELECT '') END)) [DepressionCurrent], 
                            						-->>
                            						-->> suicide-risk-history
                            						(SELECT(CASE 
                            							-->> CHECK 1 - schizophrenia questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_SchizophreniaResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate  FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_SchizophreniaResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            							-->> CHECK 2 - bipolar questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_BipolarResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_BipolarResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            							-->> CHECK 3 - depression questionaire - selecting where the created date is less than the latest created date over the three tables
                            							WHEN (exists (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_DepressionResponse  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X)))
                            							THEN (SELECT TOP 1 DepressionSC From (select Top 2 * from MH_DepressionResponse  ORDER BY CreatedDate DESC) x  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode 
                            								AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT CreatedDate AS MaxDate  FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) 
                            								union SELECT CreatedDate AS MaxDate FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)
                            								union SELECT CreatedDate AS MaxDate FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate < (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X)) AS X))
                            								---->>
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_SchizophreniaResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            							-->> CHECK 1 - Question 1 bipolar questionaire
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_BipolarResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								-->> CHECK 1 - Question 1 depression questionaire
                            							WHEN (exists (SELECT top 1 DepressionSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 DepressionSC FROM MH_DepressionResponse  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND createdDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_SchizophreniaResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_BipolarResponse WHERE Active = 1 UNION SELECT MAX(CreatedDate) AS MaxDate FROM MH_DepressionResponse WHERE Active = 1) AS X) ORDER BY CreatedDate DESC)
                            								---->>
                            							ELSE (SELECT '') END))[DepressionHistory],
                            						-->>
                            						-->> DSM5-current
                            						(SELECT(CASE 
                            							WHEN (exists (SELECT top 1 TotalScore FROM MH_DSM5Response WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_DSM5Response) AS X) ORDER BY CreatedDate DESC))  
                            								THEN (SELECT top 1 TotalScore FROM  MH_DSM5Response  WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode AND CreatedDate = (SELECT MAX(MaxDate) FROM (SELECT MAX(CreatedDate) AS MaxDate FROM MH_DSM5Response) AS X) ORDER BY CreatedDate DESC)
                            							ELSE (SELECT '') END)) [DSM5Current],
                            						-->>
                            						-->> DSM5-history
                            						(SELECT(CASE 
                            							WHEN (exists (SELECT TOP 1 TotalScore From (select Top 2 * from MH_DSM5Response  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode ORDER BY CreatedDate))  
                            								THEN (SELECT TOP 1 TotalScore From (select Top 2 * from MH_DSM5Response  ORDER BY CreatedDate DESC) x WHERE Active = 1 AND DependantID = d.DependantID AND Program = ms.programCode ORDER BY CreatedDate)
                            							ELSE (SELECT '') END)) [DSM5History],
                            						-->>							
                            						-->> Doctor Referral
                            						(SELECT(CASE WHEN (exists (SELECT TOP 1 Active From DoctorReferral  WHERE Active = 1 AND DependantID = d.DependantID AND Program = pr.code ORDER BY CreatedDate DESC)) THEN ('yes') ELSE (SELECT 'no') END)) [DrReferral],
                            						(SELECT TOP 1 ReferralDate From DoctorReferral  WHERE  Active = 1 AND DependantID = d.DependantID AND Program = pr.code ORDER BY CreatedDate DESC) [DrReferralDate],
                                                    -->> hcare-1144	
                            						(SELECT TOP 1 referralFrom From DoctorReferral  WHERE  Active = 1 AND DependantID = d.DependantID AND Program = pr.code ORDER BY CreatedDate DESC) [ReferralFrom],
                            						
                                                    -->> hcare-1014
                            						(SELECT TOP 1 stageCode FROM PatientStagingHistory p INNER JOIN PatientProgramHistory ph ON PH.dependantId = p.DependantID WHERE p.dependantId=d.DependantID AND ph.programCode = 'HIVPR' ORDER BY P.createdDate DESC) stageCode,
                                                    
                                                    -->> hcare-863
													(SELECT(CASE WHEN (exists (SELECT TOP 1 Medication From PatientDiagnosis WHERE Active = 1 AND dependentID = d.DependantID AND programCode = pr.code AND Medication like 'state' ORDER BY diagnosisDate DESC)) THEN ('yes') ELSE (SELECT 'no') END)) [StateEnrolled],
													
                            						-->> hcare-1373
													pp.planId [EmployerCode], ed.EmployerDescription [EmployerCodeDescription]


                                FROM Member m 
                                INNER JOIN Dependant d ON m.memberID = d.memberID
                                INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                            	LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                            		AND (s.endDate > getdate()
                            		OR s.endDate is null
                            		OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))--hcare-1171
                            		AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
                                    --AND s.createdDate in (Select MAX(ss.createdDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
                            		AND s.active = 1
                                LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                            	LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                            	LEFT OUTER JOIN PatientDoctorHistory Dri ON d.DependantID = Dri.dependantId
                                AND Dri.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientDoctorHistory  WHERE dependantId = d.DependantID AND Active = 1)
                            	LEFT OUTER JOIN Doctors dr ON Dri.doctorId = dr.doctorID
                            	LEFT OUTER JOIN MemberInformation mi ON d.DependantID = mi.dependantID
                            	LEFT OUTER JOIN Contact c ON mi.contactID = c.ContactID
                            	LEFT OUTER JOIN Address a ON mi.addressID = a.addressID
								-->hcare-1402
								LEFT OUTER JOIN PayPointHistory pp ON d.DependantID = pp.dependantId --using the max id because there are multiples using the same effective date
								AND pp.active = 1 
								AND pp.id = (SELECT MAX(id) FROM PayPointHistory WHERE dependantId = d.DependantID AND active = 1) 
								LEFT OUTER JOIN EmployerMaster ed ON pp.planId = ed.EmployerCode
								AND ed.active = 1 
								LEFT OUTER JOIN OtherMedicalHistory omh on d.DependantID = omh.dependentID
								AND omh.active = 1
								AND pr.code = 'HIVPR'
								AND omh.cervicalCancerEffectiveDate = (SELECT MAX(cervicalCancerEffectiveDate) FROM OtherMedicalHistory where dependentID = d.DependantID and active = 1 and pr.code = 'HIVPR')
								LEFT OUTER JOIN QuestionnaireOther qo on d.DependantID = qo.dependentID
								AND qo.active = 1 
								AND qo.createdDate = (SELECT MAX(createdDate) FROM QuestionnaireOther where dependentID = d.DependantID and active = 1)
								LEFT OUTER JOIN PatientPlanHistory pph on d.DependantID = pph.dependantId --using the max id because there are multiples using the same effective date
								AND pph.active = 1
								AND pph.id = (SELECT MAX(id) FROM PatientPlanHistory where dependantId = d.DependantID and active = 1)
								LEFT OUTER JOIN DoctorsInformation di on Dri.doctorId = di.DoctorID 
								AND di.Active = 1
								AND dri.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientDoctorHistory where dependantId = d.DependantID and active = 1 and doctorId = di.DoctorID)

								WHERE ma.Active = 1
                                AND m.active = 1
                                AND d.Active = 1";

            //if (!String.IsNullOrEmpty(model.medAidId))  //hcare-1402 // this slows the query down more!
            //{
            //    var medaids = "'" + model.medAidId.Replace(",", "','") + "'";
            //    sql = sql + string.Format(" AND ma.medicalAidCode IN ({0})", medaids);
            //}

            if (!String.IsNullOrEmpty(model.membershipNumber))
            {
                sql = sql + string.Format(" AND m.membershipNo LIKE '%{0}%'", model.membershipNumber);
            }

            if (!String.IsNullOrEmpty(model.dateFrom.ToString()))
            {
                //Hcare-1069
                sql = sql + string.Format(" AND s.effectiveDate >= '{0}'", model.dateFrom);
            }
            if (!String.IsNullOrEmpty(model.dateTo.ToString()))
            {
                //Hcare-1069
                sql = sql + string.Format(" AND s.effectiveDate <= '{0}'", model.dateTo);
            }

            if (model.memberName != null)
            {
                sql = sql + string.Format(" AND d.firstName LIKE '%{0}%'", model.memberName);
            }
            if (model.memberSurname != null)
            {
                sql = sql + string.Format(" AND d.lastName LIKE '%{0}%'", model.memberSurname);
            }
            if (model.idNumber != null)
            {
                sql = sql + string.Format(" AND d.idNumber LIKE '%{0}%'", model.idNumber);
            }
            if (model.cell != null)
            {
                sql = sql + string.Format(" AND c.cell LIKE '%{0}%'", model.cell);
            }
            if (model.telNo != null)
            {
                sql = sql + string.Format(" AND c.landLine LIKE '%{0}%'", model.telNo);
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = db.Query<AdvancedSearchResultModel>(sql, null, commandTimeout: 10000).ToList();
                db.Close();

                results = (from r in results
                           where medaidlist.Contains(r.medicalAidID)
                           select r).ToList();

                var freshResponse = new List<AdvancedSearchResultModel>();

                foreach (var med in medaidlist)
                {
                    var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                    var filteredRes = (from r in results
                                       where r.medicalAidID == med
                                       where progs.Contains(r.programId)
                                       select r).ToList();

                    freshResponse.AddRange(filteredRes);
                }

                results = freshResponse.Distinct().ToList();

                if (!String.IsNullOrEmpty(model.medAidId))
                {
                    string[] medaids = model.medAidId.Split(',');
                    results = (from r in results
                               where medaids.Contains(r.medicalAidCode)
                               select r).ToList();
                }

                if (!String.IsNullOrEmpty(model.programCode))
                {
                    string[] programs = model.programCode.Split(',');
                    results = (from r in results
                               where programs.Contains(r.program)
                               select r).ToList();
                }

                if (!String.IsNullOrEmpty(model.statusCode))
                {
                    string[] status = model.statusCode.Split(',');
                    results = (from r in results
                               where status.Contains(r.statusCode)
                               select r).ToList();
                }

                if (!String.IsNullOrEmpty(model.riskRating) && model.riskRating != ",N")
                {
                    string[] risk = model.riskRating.Split(',');
                    results = (from r in results
                               where risk.Contains(r.RiskRatingCurrent)
                               select r).ToList();
                }

                if (model.riskRating == ",N")
                {
                    string[] risk = model.riskRating.Split(',');
                    results = (from r in results
                               select r).Where(x => x.RiskRatingCurrent != "G").Where(x => x.RiskRatingCurrent != "R").Where(x => x.RiskRatingCurrent != "Y").ToList();
                }

                if (model.doctorName != null)
                {
                    results = results.Where(x => x.drName.ToLower().Contains(model.doctorName.ToLower())).ToList();
                }
                if (model.doctorSurname != null)
                {
                    results = results.Where(x => x.drSurname.ToLower().Contains(model.doctorSurname.ToLower())).ToList();
                }
                if (model.practiceNo != null)
                {
                    results = results.Where(x => x.drBHFNumber.ToLower().Contains(model.practiceNo.ToLower().Trim())).ToList();
                }

                return results.OrderByDescending(x => x.createdDate).OrderBy(x => x.DependantCode).ToList();
            }
        }





        public List<AssignmentSearch> GetAssignmentSearchResults(string medicalaid = "", string program = "", string assignmentitem = "", string fromDate = "", string toDate = "") //HCare-1129
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedMedicalAidList = rights.accessList.Select(x => x.medicalAidId).ToList();


            //DateTime dtFrom = Convert.ToDateTime(fromDate);
            //DateTime dtTo = Convert.ToDateTime(toDate);

            string sql = string.Format(@"DECLARE @x INT
                                        SELECT @x = DATEDIFF(s, a.createdDate, GETDATE())
                                        FROM Assignments a
                                        
                                        SELECT a.assignmentID[AssignmentID], a.Active[Active], ast.assignmentDescription[AssignmentType], ma.Name[MedicalScheme], m.membershipNo[MembershipNumber], d.idNumber [IDNumber], d.dependentCode [DependantCode], 
                                        d.firstName + ' ' + d.lastName [PatientName],a.effectiveDate [EffectiveDate], d.DependantID [DependantID], pr.code[ProgramCode], pr.ProgramName [Program], ph.planName[MedicalOption], 
                                        (SELECT TOP 1 msx.statusName FROM PatientManagementStatusHistory phx INNER JOIN ManagementStatus msx on phx.managementStatusCode = msx.statusCode where phx.dependantId = d.DependantID AND msx.programCode = pr.code AND phx.active = 1 ORDER BY phx.effectiveDate DESC) [ManagementStatus],
                                        DATEDIFF(day, a.effectiveDate, getDate())[AssignmentAge], a.status[AssignmentStatus], ait.itemDescription[AssignmentItemType], (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) [AssignmentTasksClosed], 
                                        (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id) [AssignmentTasksCount], ma.MedicalAidID[MedicalAidID],
                                        										
                                        CONVERT(VARCHAR, DATEDIFF(dd, a.createdDate, GETDATE())) + ' Days '
                                        + CONVERT(VARCHAR, DATEDIFF(hh, a.createdDate, GETDATE()) % 24) + ' Hours '
                                        + CONVERT(VARCHAR, DATEDIFF(mi, a.createdDate, GETDATE()) % 60) + ' Minutes '
                                        + CONVERT(VARCHAR, DATEPART(ss, DATEADD(s, @x, CONVERT(DATETIME2, '0001-01-01')))) + ' Seconds' [ElapsedTime],
                                        ait.assignmentItemType[AssignmentItemCode], a.programId AS assignmentProgramID, ait.program[AssignmentItemProgram]
                                        
                                        FROM Assignments a
                                        INNER JOIN Dependant d ON a.dependentID = d.DependantID
                                        INNER JOIN Member m ON d.memberID = m.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
                                        --LEFT OUTER JOIN PatientProgramHistory pp ON d.DependantID = pp.dependantId
                                        LEFT OUTER JOIN Programs pr ON a.programId = pr.programID
                                        LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
                                        	AND ph.id = (SELECT top 1 id FROM PatientPlanHistory WHERE d.DependantID = dependantId ORDER BY createdDate DESC)
                                        LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
                                            AND ps.createdDate = (SELECT MAX(createdDate) FROM PatientManagementStatusHistory WHERE d.DependantID = ps.dependantId)
                                        LEFT OUTER JOIN ManagementStatus ms ON ps.managementStatusCode = ms.statusCode
                                        INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
                                        INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
                                        WHERE a.Active = 1
                                        AND ma.Active = 1
                                        AND a.effectiveDate between '{0}' AND '{1}'
										AND ma.MedicalAidID = '{2}' -- addition made to HCare-1129
                                        ", fromDate, toDate, medicalaid);


            sql = sql + string.Format(" ORDER BY a.createdDate ASC");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<AssignmentSearch>)db.Query<AssignmentSearch>(sql, null, commandTimeout: 10000);
                db.Close();

                #region allowed-medical-aids
                results = (from r in results
                           where allowedMedicalAidList.Contains(r.MedicalAidID)
                           select r).ToList();
                #endregion
                #region selected-medical-aids
                if (!String.IsNullOrEmpty(medicalaid))
                {
                    string[] medaids = medicalaid.Split(',');
                    results = (from r in results
                               where medaids.Contains(r.MedicalAidID.ToString())
                               select r).ToList();
                }
                #endregion
                #region selected-programs
                if (!String.IsNullOrEmpty(program))
                {
                    string[] programs = program.Split(',');
                    results = (from r in results
                               where r.AssignmentProgramID == Guid.Empty || programs.Contains(r.AssignmentProgramID.ToString())
                               select r).ToList();
                }

                var programinfo = _context.Program.Where(x => x.programID.ToString() == program).FirstOrDefault();
                if (!String.IsNullOrEmpty(program))
                {
                    results = (from r in results
                               where r.AssignmentItemProgram == null || programinfo.code == r.AssignmentItemProgram
                               select r).ToList();
                }
                #endregion
                #region selected-assignment-items
                if (!String.IsNullOrEmpty(assignmentitem))
                {
                    string[] assignmentitems = assignmentitem.Split(',');
                    results = (from r in results
                               where assignmentitem.Contains(r.AssignmentItemType)
                               select r).ToList();
                }
                #endregion

                return results.OrderByDescending(x => x.EffectiveDate).OrderBy(x => x.DependantID).ThenBy(x => x.DependantCode).ToList();
            }
        }


        public List<Products> GetActiveProducts()
        {
            return _context.Products.Where(x => x.Active == true).OrderBy(x => x.productName).ToList();
        }

        public List<Products> GetActiveProductsByName(string prodname)
        {
            var results = _context.Products.Where(x => x.Active == true).Where(x => x.productName.Contains(prodname) || x.nappiCode.Contains(prodname)).OrderBy(x => x.productName).ToList();
            foreach (var item in results)
            {
                item.productName = item.productName + " - Nappi " + item.nappiCode + " - packsize: " + item.packSize;
            }
            return results;
        }

        public ScriptItems GetScriptItem(int itemNo)
        {
            return _context.ScriptItems.Where(x => x.itemNo == itemNo).FirstOrDefault();
        }

        public ServiceResult UpdateScriptItem(ScriptItems model)
        {
            var old = _context.ScriptItems.Where(x => x.itemNo == model.itemNo).FirstOrDefault();
            old.itemRepeats = model.itemRepeats;
            old.modifiedDate = model.modifiedDate;
            old.modifiedBy = model.modifiedBy;
            old.quantity = model.quantity;
            old.prophylaxis = model.prophylaxis;
            old.active = model.active;
            old.directions = model.directions;
            old.toDate = model.toDate;
            old.comment = model.comment;
            old.benefitType = model.benefitType;

            return _context.SaveChanges();
        }

        public List<ScriptViewModel> GetScriptItems(int scriptNo)
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts
                           on d.scriptID equals s.scriptID
                           join p in _context.Products
                           on d.nappiCode equals p.nappiCode
                           where d.scriptID == scriptNo && p.Active == true
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }
        public List<ScriptViewModel> GetScriptItems(Guid DepId)
        {

            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts
                           on d.scriptID equals s.scriptID
                           join p in _context.Products
                           on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           where s.dependentID == DepId
                           where d.active == true
                           where d.toDate > DateTime.Now
                           //where d.itemStatus != "Rejected" Hcare-1114
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }
        public List<ScriptViewModel> GetGeneralScriptItems(Guid DepId) //HCare-1023
        {

            var yesterday = DateTime.Now.AddDays(-1);

            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts
                           on d.scriptID equals s.scriptID
                           join p in _context.Products
                           on d.nappiCode equals p.nappiCode
                           where s.dependentID == DepId
                           where d.toDate > yesterday
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).OrderByDescending(x => x.fromDate).ToList();



            return results;
        }
        public List<ScriptViewModel> GetScriptItemsFull(Guid DepId) //HCare-1023
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts
                           on d.scriptID equals s.scriptID
                           join p in _context.Products
                           on d.nappiCode equals p.nappiCode
                           where s.dependentID == DepId
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()

                           }).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }

        public List<ScriptViewModel> GetDiabetesScriptItems(Guid DepId) //HCare-1023
        {
            var yesterday = DateTime.Now.AddDays(-1);

            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           where d.toDate > yesterday
                           //where d.itemStatus != "Rejected" Hcare-1114
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("DIABD")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }
        public List<ScriptViewModel> GetAllDiabetesScriptItems(Guid DepId) //HCare-1023
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("DIABD")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }

        public List<ScriptViewModel> GetHIVScriptItems(Guid DepId) //HCare-1023
        {
            var yesterday = DateTime.Now.AddDays(-1);

            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           where d.toDate > yesterday
                           //where d.itemStatus != "Rejected" //Hcare-1114
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("HIVPR")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }
        public List<ScriptViewModel> GetAllHIVScriptItems(Guid DepId) //HCare-1023
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("HIVPR")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }

        public List<ScriptViewModel> GetMHScriptItems(Guid DepId) //HCare-1183
        {
            var yesterday = DateTime.Now.AddDays(-1);

            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           where d.toDate > yesterday
                           //where d.itemStatus != "Rejected" HCare-1114
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("MNTLH")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }
        public List<ScriptViewModel> GetAllMHScriptItems(Guid DepId) //HCare-1183
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts on d.scriptID equals s.scriptID
                           join p in _context.Products on d.nappiCode equals p.nappiCode into pd
                           from p in pd.DefaultIfEmpty()
                           join i in _context.Icd10Codes on d.icd10code equals i.icd10CodeID
                           where s.dependentID == DepId
                           where d.active == true
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               prophylaxis = d.prophylaxis,
                               icd10code = d.icd10code,
                               active = d.active,
                               comment = d.comment,
                               createdBy = d.createdBy,
                               CLItemLineNo = d.CLItemLineNo,
                               icd10Type = i.subcode,
                               program = _context.Icd10Codes.Where(x => x.Active == true).Where(x => x.icd10CodeID == d.icd10code).Select(x => x.subcode).FirstOrDefault()
                           }).Where(x => x.icd10Type.ToUpper().Contains("MNTLH")).OrderByDescending(x => x.fromDate).ToList();

            return results;
        }

        public List<ScriptViewModel> GetScriptItemsMultiple(Guid DepID, int scriptID)
        {
            var results = (from d in _context.ScriptItems
                           join s in _context.Scripts
                           on d.scriptID equals s.scriptID
                           join p in _context.Products
                           on d.nappiCode equals p.nappiCode
                           where s.dependentID == DepID
                           where d.scriptID != scriptID
                           select new ScriptViewModel
                           {
                               scriptID = s.scriptID.ToString(),
                               scriptType = s.scriptType,
                               prescribingDr = _context.Doctors.Where(x => x.practiceNo == s.practiceNo).Select(x => x.drLastName).FirstOrDefault(),
                               effectiveDate = d.createdDate,
                               itemRepeats = d.itemRepeats,
                               productName = p.productName,
                               quantity = d.quantity,
                               directions = d.directions,
                               itemStatus = d.itemStatus,
                               fromDate = d.fromDate,
                               toDate = d.toDate,
                               modifiedBy = d.modifiedBy,
                               modifiedDate = d.modifiedDate,
                               nappiCode = d.nappiCode,
                               itemNo = d.itemNo,
                               benefitType = d.benefitType,
                               active = d.active
                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public string getPlanCode(Guid dependantID)
        {
            return _context.MedicalAidPlans.Where(p => p.planCode == (_context.PatientPlanHistory.OrderByDescending(x => x.createdDate).Where(x => x.dependantId == dependantID).Select(pip => pip.planName).FirstOrDefault())).Select(x => x.Name).FirstOrDefault();
        }

        public string GetClaimCode(string planname)
        {
            return _context.MedicalAidPlans.OrderByDescending(x => x.createdDate).Where(x => x.Name == planname).Select(x => x.claimCode).FirstOrDefault();
        }

        public string GetClaimCodeByMedicalAidId(string medcode)
        {
            return _context.MedicalAidClaimCode.Where(x => x.medicalAidCode == medcode).Select(x => x.claimCode).FirstOrDefault();
        }

        public List<MedicalAidClaimCode> GetMedicalAidClaimCode() //hcare-1298
        {
            return _context.MedicalAidClaimCode.Where(x => x.Active == true).ToList();
        }


        public string GetPatientPlan(Guid depID)
        {
            return _context.PatientPlanHistory.Where(x => x.dependantId == depID).OrderByDescending(x => x.createdDate).Select(x => x.planName).FirstOrDefault();
        }
        public PatientPlanHistory GetPatientPlanByDependant(Guid dependantID) //hcare-1374
        {
            return _context.PatientPlanHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.createdDate).FirstOrDefault();
        }

        public string GetServicePlanCode(string planname)
        {
            return _context.MedicalAidPlans.OrderByDescending(x => x.createdDate).Where(x => x.Name == planname).Select(x => x.servicePlanCode).FirstOrDefault();
        }

        public ClinicalViewModel GetClinicalInfo(Guid depID, Guid? pro)
        {
            var results = new ClinicalViewModel();
            //add claims information
            //HERE
            //HCare-834
            var paths = _context.Pathology.Where(x => x.dependentID == depID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
            if (pro != null)
            {
                var programCode = GetPrograms().Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();

                //paths = paths.Where(x => x.pathologyType == programCode.Substring(0, 3)).ToList();

                if (paths.Count > 0)
                {
                    results.hasPathology = true;
                    if (programCode.ToUpper() == "HIVPR")
                    {
                        paths = paths.Where(x => x.CD4CounteffectiveDate != null || x.viralLoadeffectiveDate != null).ToList();
                        if (paths.Count > 0)
                        {
                            if (paths[0].CD4CounteffectiveDate != null)
                            {
                                results.lastPathologyDate = paths[0].CD4CounteffectiveDate;
                                results.nextPathologyDate = Convert.ToDateTime(paths[0].CD4CounteffectiveDate).AddMonths(6);

                                if (Convert.ToDateTime(paths[0].CD4CounteffectiveDate).AddMonths(6) < DateTime.Now)
                                {
                                    results.isPathologyDue = true;
                                    results.pathologyBehind = DateTime.Now.DayOfYear - Convert.ToDateTime(paths[0].CD4CounteffectiveDate).DayOfYear;
                                }
                                else
                                {
                                    results.isPathologyDue = false;
                                    results.pathologyBehind = 0;
                                }
                            }
                            else
                            {
                                if (paths[0].viralLoadeffectiveDate != null)
                                {
                                    results.lastPathologyDate = paths[0].viralLoadeffectiveDate;
                                    results.nextPathologyDate = Convert.ToDateTime(paths[0].viralLoadeffectiveDate).AddMonths(6);

                                    if (Convert.ToDateTime(paths[0].viralLoadeffectiveDate).AddMonths(6) < DateTime.Now)
                                    {
                                        results.isPathologyDue = true;
                                        results.pathologyBehind = DateTime.Now.DayOfYear - Convert.ToDateTime(paths[0].viralLoadeffectiveDate).DayOfYear;
                                    }
                                    else
                                    {
                                        results.isPathologyDue = false;
                                        results.pathologyBehind = 0;
                                    }
                                }
                            }
                        }
                    }
                    else if (programCode.ToUpper() == "DIABD")
                    {
                        paths = paths.Where(x => x.hba1ceffectiveDate != null).ToList();
                        if (paths.Count > 0)
                        {
                            if (paths[0].hba1ceffectiveDate != null)
                            {
                                results.lastPathologyDate = paths[0].hba1ceffectiveDate;
                                results.nextPathologyDate = Convert.ToDateTime(paths[0].hba1ceffectiveDate).AddMonths(6);

                                if (Convert.ToDateTime(paths[0].hba1ceffectiveDate).AddMonths(6) < DateTime.Now)
                                {
                                    results.isPathologyDue = true;
                                    results.pathologyBehind = DateTime.Now.DayOfYear - Convert.ToDateTime(paths[0].hba1ceffectiveDate).DayOfYear;
                                }
                                else
                                {
                                    results.isPathologyDue = false;
                                    results.pathologyBehind = 0;
                                }
                            }
                        }
                    }
                }

            }


            var scripts = _context.Scripts.Where(x => x.dependentID == depID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();

            if (scripts.Count != 0)
            {
                results.scriptCreatedBy = scripts[0].createdBy;
                results.scriptEffectiveDate = scripts[0].effectiveDate;
                results.scriptRepeats = scripts[0].repeats;
                if (scripts[0].effectiveDate.AddMonths(scripts[0].repeats) < DateTime.Now)
                {
                    results.requiresNewScript = true;
                }
            }

            results.clinicalExams = _context.Clinical.Where(x => x.active == true).Where(x => x.dependantID == depID).OrderByDescending(x => x.effectiveDate).ToList();
            return results;
        }

        public string GetPayPoint(Guid depId)
        {
            #region HCare-923

            var results = (from p in _context.PayPointHistory
                           join e in _context.EmployerMasters
                           on p.planId equals e.EmployerCode

                           where p.dependantId == depId

                           select new EmployerMasterViewModel
                           {
                               EmployerDescription = e.EmployerDescription,
                               effectiveDate = p.effectiveDate,
                               active = p.active

                           }).OrderByDescending(x => x.effectiveDate).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.EmployerDescription).FirstOrDefault();
            #endregion

            //  _context.PayPointHistory.Where(x => x.dependantId == depId).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).Select(x => x.planName).FirstOrDefault();

            return results;
        }

        public PatientProfileViewModel GetProfile(Guid dependentId)

        {
            var results = new PatientProfileViewModel();
            results.assignments = _context.Assignments.Where(x => x.dependentID == dependentId).Take(5).ToList();
            results.patientPlanHistories = _context.PatientPlanHistory.Where(x => x.dependantId == dependentId).ToList();
            results.payPointHistories = _context.PayPointHistory.Where(x => x.dependantId == dependentId).ToList();

            var dependant = _context.Dependants.Where(x => x.DependantID == dependentId).FirstOrDefault();
            var idnumber = dependant.idNumber;
            var memberid = dependant.memberID;
            var idCheck = ValidationHelpers.GetControlDigit(idnumber);
            if (idCheck == -0)
                results.isValidId = false;
            else
                results.isValidId = true;

            results.queries = _context.Queries.Where(x => x.dependentID == dependentId).OrderByDescending(x => x.effectiveDate).Take(5).ToList();
            var script = _context.Scripts.Where(x => x.dependentID == dependentId).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            if (script != null)
            {
                results.scriptEffectiveDate = script.effectiveDate;
                results.scriptRepeats = script.repeats;

                if (script.effectiveDate.AddMonths(6) < DateTime.Now)
                {
                    results.requiresNewScript = true;
                }
                else
                {
                    results.requiresNewScript = false;
                }
            }

            var pathology = _context.Pathology.Where(x => x.dependentID == dependentId).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            if (pathology != null)
            {
                results.lastPathologyDate = pathology.effectiveDate;
                results.nextPathologyDate = pathology.effectiveDate.AddMonths(6);

                if (pathology.effectiveDate.AddMonths(6) < DateTime.Now)
                {
                    results.isPathologyDue = true;
                    results.pathologyBehind = DateTime.Now.Month - pathology.effectiveDate.AddMonths(6).Month;
                }
                else
                {
                    results.isPathologyDue = false;
                    results.pathologyBehind = 0;
                }
            }

            var medicalAidId = _context.Members.Where(x => x.memberID == memberid).Select(x => x.medicalAidID).FirstOrDefault();
            results.patientPlans = _context.MedicalAidPlans.Where(x => x.medicalAidId == medicalAidId).Where(x => x.active == true).ToList();
            results.paypoints = _context.PayPoints.Where(x => x.medicalAidId == medicalAidId).Where(x => x.active == true).ToList();

            return results;
        }

        public List<MemberSearchViewModel> GetDependents(string membershipno) //HCare760
        {

            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            membershipno = membershipno.ToUpper();
            if (membershipno != "")
            {
                string sql = string.Format(@"SELECT ma.MedicalAidID, pr.programID, m.memberID, m.membershipNo, d.DependantID, d.dependentCode, d.initials,d.firstName, d.lastName, d.idNumber, d.birthDate,d.sex, ma.Name as scheme,
                                                    ms.statusName as patientStatus, ms.statusCode
                                            FROM Member m 
                                            INNER JOIN Dependant d ON m.memberID = d.memberID
                                            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
						                    LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
						                    	AND (s.endDate > getdate()
						                    	OR s.endDate is null
						                    	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))--hcare-1171
						                    	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate)[effectiveDate] FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
                                                --AND s.createdDate in (Select MAX(ss.createdDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
                                                AND s.active = 1
                                            LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode
                                            LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                            WHERE ma.Active = 1
                                            AND m.active = 1
                                            AND d.Active = 1
                                            AND (m.membershipNo LIKE '%{0}%'
                                            OR d.firstName LIKE '%{0}%'
                                            OR d.lastName LIKE '%{0}%')", membershipno);

                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                {

                    db.Open();
                    var ourCustomers = (List<MemberSearchViewModel>)db.Query<MemberSearchViewModel>(sql, null, commandTimeout: 500);
                    db.Close();

                    var freshResponse = new List<MemberSearchViewModel>();
                    foreach (var med in medaidlist)
                    {
                        var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                        var filteredRes = (from r in ourCustomers
                                           where r.MedicalAidID == med
                                           where progs.Contains(r.programID)
                                           select r).ToList();

                        freshResponse.AddRange(filteredRes);
                    }

                    ourCustomers = freshResponse.Distinct().ToList();
                    return ourCustomers.ToList();
                }
            }
            else
            {
                string sql = string.Format(@"SELECT ma.MedicalAidID, pr.programID, m.memberID, m.membershipNo, d.DependantID, d.dependentCode, d.initials,d.firstName, d.lastName, d.idNumber, d.birthDate,d.sex, ma.Name as scheme,
                                                    ms.statusName as patientStatus, ms.statusCode
                                            FROM Member m 
                                            INNER JOIN Dependant d ON m.memberID = d.memberID
                                            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                            LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                            AND (s.endDate > getdate()
                                            OR s.endDate is null
                                            OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType = 'C' AND active = 1) 
                                            AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE active = 1 AND dependantId = d.DependantID)))
                                            AND s.active = 1
                                            LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode
                                            LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                            WHERE ma.Active = 1
                                            AND m.active = 1
                                            AND d.Active = 1", membershipno);

                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                {
                    db.Open();
                    var ourCustomers = (List<MemberSearchViewModel>)db.Query<MemberSearchViewModel>(sql, null, commandTimeout: 500);
                    db.Close();

                    var freshResponse = new List<MemberSearchViewModel>();
                    foreach (var med in medaidlist)
                    {
                        var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                        var filteredRes = (from r in ourCustomers
                                           where r.MedicalAidID == med
                                           where progs.Contains(r.programID)
                                           select r).ToList();

                        freshResponse.AddRange(filteredRes);
                    }

                    ourCustomers = freshResponse;
                    return ourCustomers.ToList();
                }
            }
        }

        public List<PatientManagementStatusHistory> GetDependentsById(Guid depId, Guid? id)
        {
            string sql = string.Format(@"SELECT s.id, s.dependantId, s.endDate, s.createdBy, s.createdDate, s.modifiedBy, s.modifiedDate, s.active, ms.statusName as managementStatusCode, s.effectiveDate, s.ManagementStatus_statusCode, s.sentToCl
                                            FROM Member m 
                                            INNER JOIN Dependant d ON m.memberID = d.memberID
                                            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
						                    LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
						                    	AND (s.endDate > getdate()
						                    	OR s.endDate is null
						                    	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))--hcare-1171
						                    	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate)[effectiveDate] FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
						                    	AND s.active = 1
                                            INNER JOIN ManagementStatus ms ON s.managementStatusCode = ms.statusCode
                                            INNER JOIN Programs pr ON ms.programCode = pr.code                                          
                                            WHERE ma.Active = 1
                                                AND m.active = 1
                                                AND d.Active = 1
                                                AND d.DependantID = '{0}'", depId);
            if (id != null)
            {
                sql = sql + string.Format("AND pr.programId='{0}'", id);
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<PatientManagementStatusHistory>)db.Query<PatientManagementStatusHistory>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public MemberBasicViewModel GetDependentsBasicById(Guid depId)
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());

            string sql = string.Format(@"SELECT ma.MedicalAidID, d.dependantId, m.membershipNo, d.dependentCode, d.firstName, d.lastName, d.title, d.birthDate, d.idNumber as IDNo, ma.Name as Scheme, pph.planName as [Option],
											ph.planName as EmployerGroup, p.programID, pp.programCode
										    FROM Member m 
                                            INNER JOIN Dependant d ON m.memberID = d.memberID
                                            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
											LEFT OUTER JOIN PatientPlanHistory pph ON d.DependantID = pph.dependantId
											AND pph.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE active = 1 AND dependantId = d.DependantID)
											LEFT OUTER JOIN PayPointHistory ph ON d.DependantID = ph.dependantId
											AND ph.effectiveDate = (SELECT MAX(effectiveDate) FROM PayPointHistory WHERE active = 1 AND dependantId = d.DependantID)
											LEFT OUTER JOIN PatientProgramHistory pp ON d.DependantID = pp.dependantId
											LEFT OUTER JOIN Programs p ON pp.programCode = p.code
                                            WHERE ma.Active = 1
                                            AND m.active = 1
                                            AND d.Active = 1
                                            AND d.DependantID = '{0}'", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<MemberBasicViewModel>)db.Query<MemberBasicViewModel>(sql, null, commandTimeout: 500);
                db.Close();
                var progList = rights.accessList.Where(x => x.medicalAidId == ourCustomers[0].MedicalAidID).Select(x => x.programId).ToList();
                foreach (var cust in ourCustomers)
                {
                    if (progList.Contains(cust.programID))
                    {
                        if (cust.programCode.ToLower() == "hivpr")
                        {
                            ourCustomers[0].hiv = true;
                            ourCustomers[0].hivK = cust.programID;
                        }

                        if (cust.programCode.ToLower() == "diabd")
                        {
                            ourCustomers[0].diab = true;
                            ourCustomers[0].diabK = cust.programID;
                        }
                    }
                }
                return ourCustomers[0];
            }
        }

        public List<Dependant> GetRecentEnrollments()
        {
            var datefrom = DateTime.Now.AddMonths(-1).Date;
            var results = (from d in _context.Dependants
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           where d.createdDate >= datefrom
                           select d
                          ).OrderByDescending(x => x.createdDate).OrderBy(x => x.dependentCode).ToList();

            return results;
        }

        public List<Dependant> GetInProgressEnrol()
        {
            var datefrom = DateTime.Now.AddMonths(-2).Date;
            string sql = string.Format(@"SELECT d.*
                                        FROM Dependant d
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE dependantId = d.DependantID AND active = 1)
                                        WHERE (s.managementStatusCode = 'DME'
                                        OR s.managementStatusCode is null)
                                        AND d.createdDate >= '{0}'
                                        ORDER BY d.createdDate DESC", datefrom);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Dependant>)db.Query<Dependant>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public PatientProgramHistory GetProgramHistoryById(int id)
        {
            return _context.PatientProgramHistory.Where(x => x.id == id).FirstOrDefault();
        }

        public List<ManagementStatus> GetManagementStatus()
        {
            var results = _context.ManagementStatus.OrderByDescending(x => x.active == true).ThenByDescending(x => x.statusType.ToLower() == "o").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "p").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "c").ThenBy(x => x.programCode).ThenBy(x => x.statusName).ToList();
            //foreach (var res in results) // hcare-1279
            //{
            //    res.statusType = _context.StatusTypes.Where(x => x.TypeCode == res.statusType).Select(x => x.TypeName).FirstOrDefault();
            //}
            return results;
        }
        public List<ManagementStatus> GetManagementStatusValidation()
        {
            return _context.ManagementStatus.Where(x => x.active == true).ToList();
        }

        public List<ManagementStatus> GetManagementStatuses()
        {
            var results = _context.ManagementStatus.OrderByDescending(x => x.active == true).ThenByDescending(x => x.statusType.ToLower() == "o").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "p").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "c").ThenBy(x => x.programCode).ThenBy(x => x.statusName).ToList();

            return results;
        }

        public List<ManagementStatus> GetManagementStatusList()
        {
            var results = _context.ManagementStatus.OrderByDescending(x => x.active == true).ThenByDescending(x => x.statusType.ToLower() == "o").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "p").ThenBy(x => x.programCode).ThenByDescending(x => x.statusType.ToLower() == "c").ThenBy(x => x.programCode).ThenBy(x => x.statusName).ToList();
            foreach (var res in results)
            {
                res.programCode = _context.Program.Where(x => x.code == res.programCode).Select(x => x.ProgramName).FirstOrDefault();
                res.statusType = _context.StatusTypes.Where(x => x.TypeCode == res.statusType).Select(x => x.TypeName).FirstOrDefault();
            }
            return results;
        }
        public List<ManagementStatus> GetManagementStatusByCode()
        {
            var results = _context.ManagementStatus.OrderByDescending(x => x.active == true).ThenByDescending(x => x.statusType.ToLower() == "o").ThenByDescending(x => x.statusType.ToLower() == "p").ThenByDescending(x => x.statusType.ToLower() == "c").ThenBy(x => x.statusName).ToList();

            return results;
        }


        public List<ManagementStatus> GetManagementStatus_noPendingStatus()
        {
            var results = _context.ManagementStatus.OrderBy(x => x.statusName).Where(x => x.statusType.ToLower().ToString() != "p").ToList();
            foreach (var res in results)
            {
                res.statusType = _context.StatusTypes.Where(x => x.TypeCode == res.statusType).Select(x => x.TypeName).FirstOrDefault();
            }

            return results;
        }

        public List<ManagementStatus> GetManagementStatus_noClosedStatus()
        {
            var results = _context.ManagementStatus.OrderBy(x => x.statusName).Where(x => x.statusType.ToLower().ToString() != "c").ToList();
            foreach (var res in results)
            {
                res.statusType = _context.StatusTypes.Where(x => x.TypeCode == res.statusType).Select(x => x.TypeName).FirstOrDefault();
            }

            return results;
        }

        public ManagementStatus GetManagementStatus(string statusCode)
        {
            return _context.ManagementStatus.Where(x => x.statusCode.ToLower() == statusCode.ToLower()).FirstOrDefault();
        }

        public ManagementStatus GetManagementStatusDescription(string statusName)
        {
            return _context.ManagementStatus.Where(x => x.statusName.ToLower() == statusName.ToLower()).FirstOrDefault();
        }

        public List<ClinicalRules> GetClinicalRules()
        {
            return _context.ClinicalRules.Where(x => x.active == true).ToList();
        }



        public List<DoctorQuestionnaireResults> GetDrQuesResults(Guid depId)
        {
            return _context.DoctorQuestionnaireResults.Where(x => x.dependentID == depId).ToList();
        }

        public List<ScriptReferences> GetScriptReference(Guid depId)
        {
            return _context.ScriptReferences.Where(x => x.active == true).Where(x => x.dependantId == depId).ToList();
        }

        public Clinical GetClinicalExamById(int Id)
        {
            return _context.Clinical.Where(x => x.id == Id).FirstOrDefault();
        }

        public List<QueryTypes> GetQueryTypes()
        {
            return _context.QueryTypes.Where(x => x.active == true).ToList();
        }

        public List<EnquiryTypes> GetQueryTypesBySource(string source)
        {
            return _context.EnquiryTypes.Where(x => x.source.ToLower().Contains(source.ToLower())).Where(x => x.active == true).ToList();
        }

        public List<Priorities> GetPriorities()
        {
            return _context.Priorities.Where(x => x.active == true).ToList();
        }

        public List<Icd10Codes> GetIcd10Codes()
        {
            return _context.Icd10Codes.OrderBy(x => x.icd10CodeID).ToList();
        }

        //public List<Icd10Codes> GetIcd10CodesByCode(string code)
        //{
        //    string name = _context.Program.Where(x => x.code == code).Select(x => x.ProgramName).FirstOrDefault();
        //    return _context.Icd10Codes.Where(x => x.codeType == name).OrderBy(x => x.icd10CodeID).ToList();
        //}

        public List<Icd10Codes> GetIcd10CodesByCode(string code)
        {
            string name = _context.Program.Where(x => x.code == code).Select(x => x.ProgramName).FirstOrDefault();
            return _context.Icd10Codes.Where(x => x.codeType == name).OrderBy(x => x.icd10CodeID).ToList();
        }

        public List<MedicalAid> GetMedicalAids()
        {
            var results = _context.MedicalAids.OrderByDescending(x => x.Active == true).ThenByDescending(x => x.clCarrier == true).ThenBy(x => x.Name).ToList();
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            var filteredRes = (from r in results
                               where medaidlist.Contains(r.MedicalAidID)
                               select r).ToList();
            return filteredRes;

        }

        public List<MedicalAid> GetMedicalAidsByUser(Guid userID)
        {
            var results = _context.MedicalAids.OrderByDescending(x => x.Active == true).ThenByDescending(x => x.clCarrier == true).ThenBy(x => x.Name).ToList();
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.userID == userID).Select(x => x.userID).FirstOrDefault());
            if (rights != null)
            {
                var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                var filteredRes = (from r in results
                                   where medaidlist.Contains(r.MedicalAidID)
                                   select r).ToList();
                return filteredRes;
            }
            else
            {
                return null;
            }
        }

        public List<MedicalAidAccount> GetMedicalAidAccounts()
        {
            return _context.MedicalAidAccounts.OrderBy(x => x.Code).Where(x => x.Active == true).ToList();
        }

        public List<MedicalAidPlans> GetMedicalAidPlans()
        {
            return _context.MedicalAidPlans.Where(x => x.active == true).OrderBy(x => x.Name).ToList();
        }

        public List<MedicalAidPlans> GetMedicalAidPlansByMedicalAidId(Guid medId)
        {
            return _context.MedicalAidPlans.Where(x => x.active == true).Where(x => x.medicalAidId == medId).OrderBy(x => x.Name).ToList();
        }

        public List<Origin> GetOrigin()
        {
            return _context.Origins.OrderBy(x => x.originName).ToList();
        }

        //public List<Language> GetLanguage()
        //{
        //    return _context.Language.OrderBy(x => x.languageName).ToList();
        //}

        public List<Language> GetLanguage()
        {
            var languages = _context.Language.OrderBy(x => x.languageName).ToList();
            foreach (var language in languages)
            {
                language.languageCode = _context.Language.Where(x => x.languageName == language.languageName).Select(x => x.languageCode).FirstOrDefault();
            }

            return languages;
        }

        public List<Demographic> GetDemographic()
        {
            return _context.Demographic.OrderBy(x => x.demographicName).ToList();
        }

        public List<DependantType> GetDependantType()
        {
            return _context.DependantType.OrderBy(x => x.dependentType).ToList();
        }

        public List<SmsMessageTemplates> GetTemplates()
        {
            return _context.SmsMessageTemplates.Where(x => x.Active == true).OrderBy(x => x.templateID).ToList();
        }

        public List<SmsMessageTemplates> GetTemplateByID(int tempId)
        {
            return _context.SmsMessageTemplates.Where(x => x.Active == true).Where(x => x.templateID == tempId).OrderBy(x => x.template).ToList();
            //return _context.SmsMessageTemplates.Where(x => x.title.ToLower().Contains(templateTitle.ToLower())).Where(x => x.Active == true).ToList();
        }

        public List<EmailTemplates> GetEmailTemplates()
        {
            return _context.EmailTemplates.Where(x => x.Active == true).OrderBy(x => x.templateID).ToList();
        }

        public List<EmailTemplates> GetEmailTemplateByID(int tempId)
        {
            return _context.EmailTemplates.Where(x => x.Active == true).Where(x => x.templateID == tempId).OrderBy(x => x.template).ToList();
        }

        public List<Sex> GetSex()
        {
            return _context.Sex.Where(x => x.active == true).OrderBy(x => x.sexName).ToList();
        }

        public IEnumerable<Dependant> GetPatients()
        {
            return _context.Dependants.OrderByDescending(x => x.createdDate).ToList();
        }

        public IEnumerable<Member> GetMembers()
        {

            return _context.Members.OrderByDescending(x => x.createdDate).ToList();
        }

        public Member GetMemberByMembershipNo(string membershipNo)
        {
            return _context.Members.Where(x => x.membershipNo == membershipNo).FirstOrDefault();
        }


        public Member GetMembers(Guid memberId)
        {
            return _context.Members.Find(memberId);
        }

        public List<SmsMessages> GetSmsMessages(Guid depId, Guid? pro) //1254
        {
            return _context.SmsMessages.Where(x => x.dependantID == depId).Where(x => x.programId == pro).OrderByDescending(x => x.effectiveDate).ThenByDescending(x => x.createdDate).ToList();
        }

        //1254
        public List<Emails> GetEmails(Guid depId, Guid? pro)
        {
            return _context.Emails.Where(x => x.dependantID == depId).Where(x => x.programId == pro).OrderByDescending(x => x.effectivedate).ToList();
        }

        public List<Notes> GetNotes(Guid DepID)
        {
            var notes = _context.Notes.Where(x => x.dependentID == DepID).OrderByDescending(x => x.effectiveDate).ToList();
            foreach (var note in notes)
            {
                note.noteType = _context.NoteTypes.Where(x => x.noteType == note.noteType).Select(x => x.noteDescription).FirstOrDefault();
            }

            return notes;
        }

        public Notes GetNote(int Id)
        {
            return _context.Notes.Where(x => x.noteID == Id).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
        }

        public ServiceResult UpdateNote(Notes model)
        {
            var old = _context.Notes.Where(x => x.noteID == model.noteID).FirstOrDefault();
            old.noteType = model.noteType;
            old.note = model.note;
            old.special = model.special;
            old.expiryDate = model.expiryDate;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public List<Pathology> GetPathology(Guid DepID)
        {
            return _context.Pathology.Where(x => x.dependentID == DepID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }
        public Pathology GetPathologyById(int Id)
        {
            return _context.Pathology.Where(x => x.pathologyID == Id).Where(x => x.active == true).FirstOrDefault();
        }

        public List<Pathology> GetGeneralPathology(Guid DepID) //HCare-1063
        {
            string sql = string.Format(@"SELECT *
                                        FROM Pathology
                                        WHERE dependentID = '{0}'
                                        AND (systolicBP IS NOT NULL
                                        OR diastolicBP IS NOT NULL
                                        OR ucreatinine IS NOT NULL
                                        OR ualbuminuria IS NOT NULL
                                        OR bilirubin IS NOT NULL
                                        OR ast IS NOT NULL
                                        OR alt IS NOT NULL
                                        OR urea IS NOT NULL
                                        OR creatinine IS NOT NULL
                                        OR eGfr IS NOT NULL
                                        OR mauCreatRatio IS NOT NULL
                                        OR FEV1 IS NOT NULL
                                        OR Eosinophylia IS NOT NULL
                                        OR haemoglobin IS NOT NULL)
										AND (systolicBP not like '0.00'
                                        OR diastolicBP not like '0.00'
                                        OR ucreatinine not like '0.00'
                                        OR ualbuminuria not like '0.00'
                                        OR bilirubin not like '0.00'
                                        OR ast not like '0.00'
                                        OR alt not like '0.00'
                                        OR urea not like '0.00'
                                        OR creatinine not like '0.00'
                                        OR eGfr not like '0.00'
                                        OR mauCreatRatio not like '0.00'
                                        OR FEV1 not like '0.00'
                                        OR Eosinophylia not like '0.00'
                                        OR haemoglobin not like '0.00')
                                        AND Active = 1", DepID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetHyperlipidaemiaPathology(Guid DepID) //HCare-1063
        {
            string sql = string.Format(@"SELECT *
	                                    FROM Pathology
	                                    WHERE dependentID = '{0}'
	                                    AND (totalCholestrol IS NOT NULL
	                                    OR triglycerides IS NOT NULL
	                                    OR ldl IS NOT NULL
	                                    OR hdl IS NOT NULL)
										AND (totalCholestrol not like '0.00'
	                                    OR triglycerides not like '0.00'
	                                    OR ldl not like '0.00'
	                                    OR hdl not like '0.00')
										AND Active = 1", DepID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetDiabetesPathology(Guid DepID) //HCare-1063
        {
            string sql = string.Format(@"SELECT *
                                        FROM Pathology
                                        WHERE dependentID = '{0}'
                                        AND (hba1c IS NOT NULL
                                        OR normGtt IS NOT NULL
                                        OR abnGtt IS NOT NULL
                                        OR glucose IS NOT NULL
                                        OR ucreatinine IS NOT NULL
                                        OR ualbuminuria IS NOT NULL
                                        OR bilirubin IS NOT NULL
                                        OR ast IS NOT NULL
                                        OR alt IS NOT NULL
                                        OR urea IS NOT NULL
                                        OR creatinine IS NOT NULL
                                        OR eGfr IS NOT NULL
                                        OR mauCreatRatio IS NOT NULL)
										AND (hba1c not like '0.00'
                                        OR normGtt not like '0'
                                        OR abnGtt not like '0'
                                        OR glucose not like '0.00'
                                        OR ucreatinine not like '0.00'
                                        OR ualbuminuria not like '0.00'
                                        OR bilirubin not like '0.00'
                                        OR ast not like '0.00'
                                        OR alt not like '0.00'
                                        OR urea not like '0.00'
                                        OR creatinine not like '0.00'
                                        OR eGfr not like '0.00'
                                        OR mauCreatRatio not like '0.00')
										AND Active = 1", DepID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetHIVPathology(Guid DepID) //HCare-1063
        {
            string sql = string.Format(@"SELECT *
	                                    FROM Pathology
	                                    WHERE dependentID = '{0}'
	                                    AND (hivEliza IS NOT NULL
	                                    OR CD4Count IS NOT NULL
	                                    OR CD4Percentage IS NOT NULL
	                                    OR viralLoad IS NOT NULL)
										AND (hivEliza not like '0'
	                                    OR CD4Count not like '0.00'
	                                    OR CD4Percentage not like '0.00'
	                                    OR viralLoad not like '0.00')
										AND Active = 1", DepID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<Pathology> GetHba1cByPatient(Guid depId) //HCare-1242
        {
            string sql = string.Format(@"SELECT top 1 hba1c, hba1ceffectiveDate FROM Pathology
                                                WHERE dependentID = '{0}'
                                                AND hba1c is not NULL 
                                                AND hba1c not like '0.00' 
                                                AND Active = 1
                                                ORDER BY hba1ceffectiveDate DESC", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<Pathology> GetGeneralPathologyForDependant(Guid depId) //HCare-1063 //HCare-1219
        {
            string sql = string.Format(@"SELECT 
                                        (select top 1 systolicBP from Pathology where BPeffectiveDate = (select MAX(BPeffectiveDate) from Pathology WHERE systolicBP is not NULL AND systolicBP not like '0.00' AND dependentID = '{0}' AND Active = 1 AND systolicBP != 0.00) AND systolicBP is not NULL AND systolicBP not like '0.00' AND dependentID = '{0}' AND Active = 1) AS systolicBP,
                                        (select top 1 diastolicBP from Pathology where BPeffectiveDate = (select MAX(BPeffectiveDate) from Pathology WHERE diastolicBP is not NULL AND diastolicBP not like '0.00' AND dependentID = '{0}' AND Active = 1 AND diastolicBP != 0.00) AND diastolicBP is not NULL AND diastolicBP not like '0.00' AND dependentID = '{0}' AND Active = 1) AS diastolicBP,
                                        (select top 1 BPeffectiveDate from Pathology where BPeffectiveDate= (select MAX(BPeffectiveDate) from Pathology WHERE BPeffectiveDate is not NULL AND systolicBP not like '0.00' AND systolicBP != 0.00 AND diastolicBP != 0.00 AND dependentID = '{0}' AND Active = 1) AND BPeffectiveDate is not NULL AND systolicBP not like '0.00' AND systolicBP != 0.00 AND diastolicBP != 0.00 AND dependentID = '{0}' AND Active = 1) AS BPeffectiveDate,
                                        (select top 1 ucreatinine from Pathology where ucreatinineeffectiveDate = (select MAX(ucreatinineeffectiveDate) from Pathology WHERE ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ucreatinine,
                                        (select top 1 ucreatinineeffectiveDate from Pathology where ucreatinineeffectiveDate= (select MAX(ucreatinineeffectiveDate) from Pathology WHERE ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ucreatinineeffectiveDate,
                                        (select top 1 ualbuminuria from Pathology where ualbuminuriaeffectiveDate= (select MAX(ualbuminuriaeffectiveDate) from Pathology WHERE ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ualbuminuria,
                                        (select top 1 ualbuminuriaeffectiveDate from Pathology where ualbuminuriaeffectiveDate= (select MAX(ualbuminuriaeffectiveDate) from Pathology WHERE ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ualbuminuriaeffectiveDate,
                                        (select top 1 bilirubin from Pathology where bilirubineffectiveDate= (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS bilirubin,
                                        (select top 1 bilirubineffectiveDate from Pathology where bilirubineffectiveDate= (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS bilirubineffectiveDate,
                                        (select top 1 ast from Pathology where asteffectiveDate= (select MAX(asteffectiveDate) from Pathology WHERE ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ast,
                                        (select top 1 asteffectiveDate from Pathology where asteffectiveDate= (select MAX(asteffectiveDate) from Pathology WHERE ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AS asteffectiveDate,
                                        (select top 1 alt from Pathology where alteffectiveDate= (select MAX(alteffectiveDate) from Pathology WHERE alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AND alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AS alt,
                                        (select top 1 alteffectiveDate from Pathology where alteffectiveDate= (select MAX(alteffectiveDate) from Pathology WHERE alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AND alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AS alteffectiveDate,
                                        (select top 1 urea from Pathology where ureaeffectiveDate= (select MAX(ureaeffectiveDate) from Pathology WHERE urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AND urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AS urea,
                                        (select top 1 ureaeffectiveDate from Pathology where ureaeffectiveDate= (select MAX(ureaeffectiveDate) from Pathology WHERE urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AND urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ureaeffectiveDate,
                                        (select top 1 creatinine from Pathology where creatinineeffectiveDate= (select MAX(creatinineeffectiveDate) from Pathology WHERE creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS creatinine,
                                        (select top 1 creatinineeffectiveDate from Pathology where creatinineeffectiveDate= (select MAX(creatinineeffectiveDate) from Pathology WHERE creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS creatinineeffectiveDate,
                                        (select top 1 eGfr from Pathology where eGfreffectiveDate= (select MAX(eGfreffectiveDate) from Pathology WHERE eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AND eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AS eGfr,
                                        (select top 1 eGfreffectiveDate from Pathology where eGfreffectiveDate= (select MAX(eGfreffectiveDate) from Pathology WHERE eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AND eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AS eGfreffectiveDate,
                                        (select top 1 mauCreatRatio from Pathology where mauCreatRatioeffectiveDate= (select MAX(mauCreatRatioeffectiveDate) from Pathology WHERE mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AND mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AS mauCreatRatio,
                                        (select top 1 mauCreatRatioeffectiveDate from Pathology where mauCreatRatioeffectiveDate= (select MAX(mauCreatRatioeffectiveDate) from Pathology WHERE mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AND mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AS mauCreatRatioeffectiveDate,
                                        (select top 1 FEV1 from Pathology where FEV1effectiveDate= (select MAX(FEV1effectiveDate) from Pathology WHERE FEV1 is not NULL AND FEV1 not like '0.00' AND dependentID = '{0}' AND Active = 1) AND FEV1 is not NULL AND FEV1 not like '0.00' AND dependentID = '{0}' AND Active = 1) AS FEV1,
                                        (select top 1 FEV1effectiveDate from Pathology where FEV1effectiveDate= (select MAX(FEV1effectiveDate) from Pathology WHERE FEV1 is not NULL AND FEV1 not like '0.00' AND dependentID = '{0}' AND Active = 1) AND FEV1 is not NULL AND FEV1 not like '0.00' AND dependentID = '{0}' AND Active = 1) AS FEV1effectiveDate,
                                        (select top 1 Eosinophylia from Pathology where EosinophyliaeffectiveDate= (select MAX(EosinophyliaeffectiveDate) from Pathology WHERE Eosinophylia is not NULL AND Eosinophylia not like '0.00' AND dependentID = '{0}' AND Active = 1) AND Eosinophylia is not NULL AND Eosinophylia not like '0.00' AND dependentID = '{0}' AND Active = 1) AS Eosinophylia,
                                        (select top 1 EosinophyliaeffectiveDate from Pathology where EosinophyliaeffectiveDate= (select MAX(EosinophyliaeffectiveDate) from Pathology WHERE Eosinophylia is not NULL AND Eosinophylia not like '0.00' AND dependentID = '{0}' AND Active = 1) AND Eosinophylia is not NULL AND Eosinophylia not like '0.00'AND dependentID = '{0}' AND Active = 1) AS EosinophyliaeffectiveDate,
                                        (select top 1 haemoglobin from Pathology where haemoglobineffectiveDate= (select MAX(haemoglobineffectiveDate) from Pathology WHERE haemoglobin is not NULL AND haemoglobin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND haemoglobin is not NULL AND haemoglobin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS haemoglobin,
                                        (select top 1 haemoglobineffectiveDate from Pathology where haemoglobineffectiveDate= (select MAX(haemoglobineffectiveDate) from Pathology WHERE haemoglobin is not NULL AND haemoglobin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND haemoglobin is not NULL AND haemoglobin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS haemoglobineffectiveDate", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetHyperlipidaemiaPathologyForDependant(Guid depId) //HCare-1063 //HCare-1219
        {
            string sql = string.Format(@"SELECT 
                                        (select top 1 totalCholestrol from Pathology where totalCholestroleffectiveDate = (select MAX(totalCholestroleffectiveDate) from Pathology WHERE totalCholestrol is not NULL AND totalCholestrol not like '0.00' AND dependentID = '{0}' AND Active = 1) AND totalCholestrol is not NULL AND totalCholestrol not like '0.00' AND dependentID = '{0}' AND Active = 1) AS totalCholestrol,
                                        (select top 1 totalCholestroleffectiveDate from Pathology where totalCholestroleffectiveDate = (select MAX(totalCholestroleffectiveDate) from Pathology WHERE totalCholestrol is not NULL AND totalCholestrol not like '0.00' AND dependentID = '{0}' AND Active = 1) AND totalCholestrol is not NULL AND totalCholestrol not like '0.00' AND dependentID = '{0}' AND Active = 1) AS totalCholestroleffectiveDate,
                                        (select top 1 triglycerides from Pathology where triglycerideseffectiveDate = (select MAX(triglycerideseffectiveDate) from Pathology WHERE triglycerides is not NULL AND triglycerides not like '0.00' AND dependentID = '{0}' AND Active = 1) AND triglycerides is not NULL AND triglycerides not like '0.00' AND dependentID = '{0}' AND Active = 1) AS triglycerides,
                                        (select top 1 triglycerideseffectiveDate from Pathology where triglycerideseffectiveDate = (select MAX(triglycerideseffectiveDate) from Pathology WHERE triglycerides is not NULL AND triglycerides not like '0.00' AND dependentID = '{0}' AND Active = 1) AND triglycerides is not NULL AND triglycerides not like '0.00' AND dependentID = '{0}' AND Active = 1) AS triglycerideseffectiveDate,
                                        (select top 1 ldl from Pathology where ldleffectiveDate = (select MAX(ldleffectiveDate) from Pathology WHERE ldl is not NULL AND ldl not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ldl is not NULL AND ldl not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ldl,
                                        (select top 1 ldleffectiveDate from Pathology where ldleffectiveDate = (select MAX(ldleffectiveDate) from Pathology WHERE ldl is not NULL AND ldl not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ldl is not NULL AND ldl not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ldleffectiveDate,
                                        (select top 1 hdl from Pathology where hdleffectiveDate = (select MAX(hdleffectiveDate) from Pathology WHERE hdl is not NULL AND hdl not like '0.00' AND dependentID = '{0}' AND Active = 1) AND hdl is not NULL AND hdl not like '0.00' AND dependentID = '{0}' AND Active = 1) AS hdl,
                                        (select top 1 hdleffectiveDate from Pathology where hdleffectiveDate= (select MAX(hdleffectiveDate) from Pathology WHERE hdl is not NULL AND hdl not like '0.00' AND dependentID = '{0}' AND Active = 1) AND hdl is not NULL AND hdl not like '0.00' AND dependentID = '{0}' AND Active = 1) AS hdleffectiveDate", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetDiabetesPathologyForDependant(Guid depId) //HCare-1063 //HCare-1219
        {
            string sql = string.Format(@"SELECT 
                                        (select top 1 hba1c from Pathology where hba1ceffectiveDate = (select MAX(hba1ceffectiveDate) from Pathology WHERE hba1c is not NULL AND hba1c not like '0.00' AND dependentID = '{0}' AND Active = 1) AND hba1c is not NULL AND hba1c not like '0.00' AND dependentID = '{0}' AND Active = 1) AS hba1c,
                                        (select top 1 hba1ceffectiveDate from Pathology where hba1ceffectiveDate = (select MAX(hba1ceffectiveDate) from Pathology WHERE hba1c is not NULL AND hba1c not like '0.00' AND dependentID = '{0}' AND Active = 1) AND hba1c is not NULL AND hba1c not like '0.00' AND dependentID = '{0}' AND Active = 1) AS hba1ceffectiveDate,
                                        (select top 1 normGtt from Pathology where normGtteffectiveDate = (select MAX(normGtteffectiveDate) from Pathology WHERE normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND Active = 1) AND normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND Active = 1) AS normGtt,
                                        (select top 1 normGtteffectiveDate from Pathology where normGtteffectiveDate = (select MAX(normGtteffectiveDate) from Pathology WHERE normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND Active = 1) AND normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND Active = 1) AS normGtteffectiveDate,
                                        (select top 1 abnGtt from Pathology where abnGtteffectiveDate = (select MAX(abnGtteffectiveDate) from Pathology WHERE abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND Active = 1) AND abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND Active = 1) AS abnGtt,
                                        (select top 1 abnGtteffectiveDate from Pathology where abnGtteffectiveDate = (select MAX(abnGtteffectiveDate) from Pathology WHERE abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND Active = 1) AND abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND Active = 1) AS abnGtteffectiveDate,                                        
                                        (select top 1 glucose from Pathology where glucoseeffectiveDate = (select MAX(glucoseeffectiveDate) from Pathology WHERE glucose is not NULL AND glucose not like '0.00' AND dependentID = '{0}' AND Active = 1) AND glucose is not NULL AND glucose not like '0.00' AND dependentID = '{0}' AND Active = 1) AS glucose,
                                        (select top 1 glucoseeffectiveDate from Pathology where glucoseeffectiveDate = (select MAX(glucoseeffectiveDate) from Pathology WHERE glucose is not NULL AND glucose not like '0.00' AND dependentID = '{0}' AND Active = 1) AND glucose is not NULL AND glucose not like '0.00' AND dependentID = '{0}' AND Active = 1) AS glucoseeffectiveDate,
                                        (select top 1 ucreatinine from Pathology where ucreatinineeffectiveDate = (select MAX(ucreatinineeffectiveDate) from Pathology WHERE ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ucreatinine,
                                        (select top 1 ucreatinineeffectiveDate from Pathology where ucreatinineeffectiveDate= (select MAX(ucreatinineeffectiveDate) from Pathology WHERE ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ucreatinineeffectiveDate,
                                        (select top 1 ualbuminuria from Pathology where ualbuminuriaeffectiveDate= (select MAX(ualbuminuriaeffectiveDate) from Pathology WHERE ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ualbuminuria,
                                        (select top 1 ualbuminuriaeffectiveDate from Pathology where ualbuminuriaeffectiveDate= (select MAX(ualbuminuriaeffectiveDate) from Pathology WHERE ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ualbuminuriaeffectiveDate,
                                        (select top 1 bilirubin from Pathology where bilirubineffectiveDate= (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS bilirubin,
                                        (select top 1 bilirubineffectiveDate from Pathology where bilirubineffectiveDate= (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AND bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND Active = 1) AS bilirubineffectiveDate,
                                        (select top 1 ast from Pathology where asteffectiveDate= (select MAX(asteffectiveDate) from Pathology WHERE ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ast,
                                        (select top 1 asteffectiveDate from Pathology where asteffectiveDate= (select MAX(asteffectiveDate) from Pathology WHERE ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AND ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND Active = 1) AS asteffectiveDate,
                                        (select top 1 alt from Pathology where alteffectiveDate= (select MAX(alteffectiveDate) from Pathology WHERE alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AND alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AS alt,
                                        (select top 1 alteffectiveDate from Pathology where alteffectiveDate= (select MAX(alteffectiveDate) from Pathology WHERE alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AND alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND Active = 1) AS alteffectiveDate,
                                        (select top 1 urea from Pathology where ureaeffectiveDate= (select MAX(ureaeffectiveDate) from Pathology WHERE urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AND urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AS urea,
                                        (select top 1 ureaeffectiveDate from Pathology where ureaeffectiveDate= (select MAX(ureaeffectiveDate) from Pathology WHERE urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AND urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND Active = 1) AS ureaeffectiveDate,
                                        (select top 1 creatinine from Pathology where creatinineeffectiveDate= (select MAX(creatinineeffectiveDate) from Pathology WHERE creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS creatinine,
                                        (select top 1 creatinineeffectiveDate from Pathology where creatinineeffectiveDate= (select MAX(creatinineeffectiveDate) from Pathology WHERE creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AND creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND Active = 1) AS creatinineeffectiveDate,
                                        (select top 1 eGfr from Pathology where eGfreffectiveDate= (select MAX(eGfreffectiveDate) from Pathology WHERE eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AND eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AS eGfr,
                                        (select top 1 eGfreffectiveDate from Pathology where eGfreffectiveDate= (select MAX(eGfreffectiveDate) from Pathology WHERE eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AND eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND Active = 1) AS eGfreffectiveDate,
                                        (select top 1 mauCreatRatio from Pathology where mauCreatRatioeffectiveDate= (select MAX(mauCreatRatioeffectiveDate) from Pathology WHERE mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AND mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AS mauCreatRatio,
                                        (select top 1 mauCreatRatioeffectiveDate from Pathology where mauCreatRatioeffectiveDate= (select MAX(mauCreatRatioeffectiveDate) from Pathology WHERE mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AND mauCreatRatio is not NULL AND mauCreatRatio not like '0.00' AND dependentID = '{0}' AND Active = 1) AS mauCreatRatioeffectiveDate", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<Pathology> GetHIVPathologyForDependant(Guid depId) //HCare-1063 //HCare-1219
        {
            string sql = string.Format(@"SELECT 
                                        (select top 1 hivEliza from Pathology where hivElizaeffectiveDate = (select MAX(hivElizaeffectiveDate) from Pathology WHERE hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND Active = 1) AND hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND Active = 1) AS hivEliza,
                                        (select top 1 hivElizaeffectiveDate from Pathology where hivElizaeffectiveDate = (select MAX(hivElizaeffectiveDate) from Pathology WHERE hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND Active = 1) AND hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND Active = 1) AS hivElizaeffectiveDate,
                                        (select top 1 CD4Count from Pathology where CD4CounteffectiveDate = (select MAX(CD4CounteffectiveDate) from Pathology WHERE CD4Count is not NULL AND CD4Count not like '0.00' AND dependentID = '{0}' AND Active = 1) AND CD4Count is not NULL AND CD4Count not like '0.00' AND dependentID = '{0}' AND Active = 1) AS CD4Count,
                                        (select top 1 CD4CounteffectiveDate from Pathology where CD4CounteffectiveDate = (select MAX(CD4CounteffectiveDate) from Pathology WHERE CD4Count is not NULL AND CD4Count not like '0.00' AND dependentID = '{0}' AND Active = 1) AND CD4Count is not NULL AND CD4Count not like '0.00' AND dependentID = '{0}' AND Active = 1) AS CD4CounteffectiveDate,
                                        (select top 1 CD4Percentage from Pathology where CD4PercentageeffectiveDate = (select MAX(CD4PercentageeffectiveDate) from Pathology WHERE CD4Percentage is not NULL AND CD4Percentage not like '0.00' AND dependentID = '{0}' AND Active = 1) AND CD4Percentage is not NULL AND CD4Percentage not like '0.00' AND dependentID = '{0}' AND Active = 1) AS CD4Percentage,
                                        (select top 1 CD4PercentageeffectiveDate from Pathology where CD4PercentageeffectiveDate = (select MAX(CD4PercentageeffectiveDate) from Pathology WHERE CD4Percentage is not NULL AND CD4Percentage not like '0.00' AND dependentID = '{0}' AND Active = 1) AND CD4Percentage is not NULL AND CD4Percentage not like '0.00' AND dependentID = '{0}' AND Active = 1) AS CD4PercentageeffectiveDate,
                                        (select top 1 viralLoad from Pathology where viralLoadeffectiveDate = (select MAX(viralLoadeffectiveDate) from Pathology WHERE viralLoad is not NULL AND viralLoad not like '0.00' AND dependentID = '{0}' AND Active = 1) AND viralLoad is not NULL AND viralLoad not like '0.00' AND dependentID = '{0}' AND Active = 1) AS viralLoad,
                                        (select top 1 viralLoadeffectiveDate from Pathology where viralLoadeffectiveDate= (select MAX(viralLoadeffectiveDate) from Pathology WHERE viralLoad is not NULL AND viralLoad not like '0.00' AND dependentID = '{0}' AND Active = 1) AND viralLoad is not NULL AND viralLoad not like '0.00' AND dependentID = '{0}' AND Active = 1) AS viralLoadeffectiveDate", depId);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public ServiceResult UpdatePathology(Pathology model)
        {

            var old = GetPathologyById(model.pathologyID);
            //Hcare-1214
            if (model.active)
            {
                if (old.CD4Count != model.CD4Count ||
                old.CD4Percentage != model.CD4Percentage ||
                old.viralLoad != model.viralLoad ||
                old.haemoglobin != model.haemoglobin ||
                old.bilirubin != model.bilirubin ||
                old.totalCholestrol != model.totalCholestrol ||
                old.hdl != model.hdl ||
                old.ldl != model.ldl ||
                old.triglycerides != model.triglycerides ||
                old.glucose != model.glucose ||
                old.hba1c != model.hba1c ||
                old.alt != model.alt ||
                old.ast != model.ast ||
                old.urea != model.urea ||
                old.creatinine != model.creatinine ||
                old.eGfr != model.eGfr ||
                old.mauCreatRatio != model.mauCreatRatio ||
                old.FEV1 != model.FEV1 ||
                old.Eosinophylia != model.Eosinophylia ||
                old.hivEliza != model.hivEliza ||
                old.ucreatinine != model.ucreatinine ||
                old.normGtt != model.normGtt ||
                old.abnGtt != model.abnGtt ||
                old.salbumin != model.salbumin ||
                old.ualbuminuria != model.ualbuminuria ||
                old.systolicBP != model.systolicBP ||
                old.diastolicBP != model.diastolicBP)
                    old.sentToCL = 0;
            }
            //HCare-1412

            old.effectiveDate = model.effectiveDate;
            old.labName = model.labName;
            old.labReferenceNo = model.labReferenceNo;
            old.labTelNo = model.labTelNo;
            old.pathologyType = model.pathologyType;
            old.CD4Count = model.CD4Count;
            old.CD4CounteffectiveDate = model.CD4CounteffectiveDate;
            old.CD4Percentage = model.CD4Percentage;
            old.CD4PercentageeffectiveDate = model.CD4PercentageeffectiveDate;
            old.viralLoad = model.viralLoad;
            old.viralLoadeffectiveDate = model.viralLoadeffectiveDate;
            old.haemoglobin = model.haemoglobin;
            old.haemoglobineffectiveDate = model.haemoglobineffectiveDate;
            old.bilirubin = model.bilirubin;
            old.bilirubineffectiveDate = model.bilirubineffectiveDate;
            old.totalCholestrol = model.totalCholestrol;
            old.totalCholestroleffectiveDate = model.totalCholestroleffectiveDate;
            old.hdl = model.hdl;
            old.hdleffectiveDate = model.hdleffectiveDate;
            old.ldl = model.ldl;
            old.ldleffectiveDate = model.ldleffectiveDate;
            old.triglycerides = model.triglycerides;
            old.triglycerideseffectiveDate = model.triglycerideseffectiveDate;
            old.glucose = model.glucose;
            old.glucoseeffectiveDate = model.glucoseeffectiveDate;
            old.hba1c = model.hba1c;
            old.hba1ceffectiveDate = model.hba1ceffectiveDate;
            old.alt = model.alt;
            old.alteffectiveDate = model.alteffectiveDate;
            old.ast = model.ast;
            old.asteffectiveDate = model.asteffectiveDate;
            old.urea = model.urea;
            old.ureaeffectiveDate = model.ureaeffectiveDate;
            old.creatinine = model.creatinine;
            old.creatinineeffectiveDate = model.creatinineeffectiveDate;
            old.eGfr = model.eGfr;
            old.eGfreffectiveDate = model.eGfreffectiveDate;
            old.mauCreatRatio = model.mauCreatRatio;
            old.mauCreatRatioeffectiveDate = model.mauCreatRatioeffectiveDate;
            old.FEV1 = model.FEV1;
            old.FEV1effectiveDate = model.FEV1effectiveDate;
            old.Eosinophylia = model.Eosinophylia;
            old.EosinophyliaeffectiveDate = model.EosinophyliaeffectiveDate;
            old.hivEliza = model.hivEliza;
            old.hivElizaeffectiveDate = model.hivElizaeffectiveDate;
            old.ucreatinine = model.ucreatinine;
            old.ucreatinineeffectiveDate = model.ucreatinineeffectiveDate;
            old.normGtt = model.normGtt;
            old.normGtteffectiveDate = model.normGtteffectiveDate;
            old.abnGtt = model.abnGtt;
            old.abnGtteffectiveDate = model.abnGtteffectiveDate;
            old.salbumin = model.salbumin;
            old.salbumineffectiveDate = model.salbumineffectiveDate;
            old.ualbuminuria = model.ualbuminuria;
            old.ualbuminuriaeffectiveDate = model.ualbuminuriaeffectiveDate;
            old.systolicBP = model.systolicBP;
            old.diastolicBP = model.diastolicBP;
            old.BPeffectiveDate = model.BPeffectiveDate;
            old.comment = model.comment;
            old.active = model.active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            //HCare-1214



            if (!old.active)
            {
                var rules = _context.PatientClinicalRulesHistory.Where(x => x.pathologyID == old.pathologyID).ToList();

                if (rules.Count() != 0)
                {
                    foreach (var rule in rules)
                    {
                        var res = UpdateClinicalRules(rule);
                    }
                }
                var ass = _context.Assignments.Where(x => x.pathologyID == old.pathologyID).ToList();

                if (ass.Count() != 0)
                {
                    foreach (var assi in ass)
                    {
                        assi.Active = false;
                        var res = UpdateAssignment(assi);
                    }
                }
            }

            return _context.SaveChanges();

        }

        public ServiceResult UpdateClinicalRules(PatientClinicalRulesHistory model)
        {
            var old = _context.PatientClinicalRulesHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.active = false;
            return _context.SaveChanges();
        }

        public string GetMembers(string membership_no, Guid medicalaidID)
        {
            var member = _context.Members.Where(x => x.membershipNo == membership_no).
                Where(x => x.medicalAidID == medicalaidID).
                Select(x => x.memberID).FirstOrDefault();
            if (String.IsNullOrEmpty(member.ToString()) || member.ToString().Contains("0000"))
            {
                return "0";
            }
            else
            {
                return member.ToString();
            }
        }

        public string GetDependants(Guid memberID, string dep_code)
        {
            var dependant = _context.Dependants.Where(x => x.memberID == memberID).Where(x => x.dependentCode == dep_code).Select(x => x.DependantID).FirstOrDefault();
            if (String.IsNullOrEmpty(dependant.ToString()) || dependant.ToString().Contains("0000"))
            {
                return "0";
            }
            else
            {
                return dependant.ToString();
            }
        }

        public List<Dependant> GetDependantByMembershipDepCodeAidCode(string membershipNo, string DepCode, string medAidCode)
        {
            var results = (from m in _context.Members
                           join d in _context.Dependants
                           on m.memberID equals d.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           where m.membershipNo == membershipNo
                           where d.dependentCode == DepCode
                           where ma.medicalAidCode == medAidCode
                           select d).ToList();

            return results;
        }

        public string GetDependantByIDNo(string idno)
        {
            var dependant = _context.Dependants.Where(x => x.idNumber == idno).Select(x => x.DependantID).FirstOrDefault();
            if (String.IsNullOrEmpty(dependant.ToString()) || dependant.ToString().Contains("0000"))
            {
                return "0";
            }
            else
            {
                return dependant.ToString();
            }
        }

        public Guid InsertMembers(Member member)
        {
            member.memberID = Guid.NewGuid();
            _context.Members.Add(member);
            Save();
            return member.memberID;
        }

        public ServiceResult InsertComorbidCondition(CoMormidConditions model)
        {
            _context.CoMorbidConditions.Add(model);
            return SaveResult();
        }
        public ServiceResult UpdatePatientProgramSubHistory(PatientProgramSubHistory model)
        {
            var old = _context.PatientProgramSubHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.conditionCode = model.conditionCode;
            old.icd10Code = model.icd10Code;
            old.Comment = model.Comment;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.programCode = model.programCode;
            old.effectiveDate = model.effectiveDate;
            old.endDate = model.endDate;
            old.active = model.active;

            return SaveResult();
        }

        public List<MentalHealthDiagnosis> GetMentalHealthHistoryByDependant(Guid dependantID, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).Where(x => x.Active == true).OrderBy(x => x.CreatedDate).Take(1).ToList();
            foreach (var result in results)
            {
                var mhrecords = _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).Where(x => x.Active == true).Where(x => x.EndDate > DateTime.Today || x.EndDate == null).OrderBy(x => x.CreatedDate).ToList();
                foreach (var mh in mhrecords.Where(x => x.ICD10Code + ' ' + x.ConditionCode != result.ICD10Code + ' ' + result.ConditionCode))
                {
                    result.ICD10Code = result.ICD10Code + ", " + mh.ICD10Code;
                }
                result.ProgramCode = _context.Program.Where(x => x.code == programCode).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        public ServiceResult InsertMentalHealthDiagnosis(MentalHealthDiagnosis model)
        {
            _context.MentalHealthDiagnoses.Add(model);
            return SaveResult();
        }
        public ServiceResult UpdateMentalHealthDiagnosis(MHDiagnosisViewModel model)
        {
            var old = _context.MentalHealthDiagnoses.Where(x => x.Id == model.Id).FirstOrDefault();
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.EffectiveDate = model.EffectiveDate;
            old.EndDate = model.EndDate;
            old.ICD10Code = model.ICD10Code;
            old.ConditionCode = model.MappingCode;
            old.Comment = model.Comment;
            old.ProgramCode = model.Program;
            old.Active = model.Active;

            return SaveResult();
        }
        public List<PatientProgramSubHistory> GetPatientProgramSubHistoryList(Guid dependantID)
        {
            return _context.PatientProgramSubHistory.Where(x => x.dependantId == dependantID).ToList();
        }
        public List<CoMorbidConditionVM> GetPatientProgramHistoryList_Both(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.mappingDescription[Condition], ct.mappingCode[mappingCode]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE (ct.mappingCode NOT IN(SELECT ppsh.conditionCode FROM PatientProgramSubHistory ppsh WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.mappingCode NOT IN(SELECT ppsh.conditionCode FROM PatientProgramHistory ppsh WHERE ppsh.dependantID ='{0}' AND ppsh.programCode = 'MNTLH' AND ppsh.endDate is NULL AND ppsh.active = 1))
                                        AND (ct.mappingDescription NOT IN(SELECT distinct cme.mappingDescription FROM ComorbidConditionExclusions cme INNER JOIN PatientProgramSubHistory ppsh on cme.mappingCode = ppsh.conditionCode WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.mappingDescription NOT IN(SELECT distinct cme.mappingDescription FROM ComorbidConditionExclusions cme INNER JOIN PatientProgramHistory ppsh on cme.mappingCode = ppsh.conditionCode WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1))", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }



        public PatientProgramSubHistory GetPatientProgramSubHistoryByID(int id)
        {
            var result = _context.PatientProgramSubHistory.Where(x => x.id == id).FirstOrDefault();
            result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
            //result.conditionCode = _context.ComorbidConditionExclusions.Where(x => x.mappingCode == result.conditionCode).Select(x => x.mappingDescription).FirstOrDefault();

            return result;
        }

        public MHDiagnosisViewModel GetMentalHealthDiagnosisByID(int id)
        {
            var results = (from mhd in _context.MentalHealthDiagnoses
                           join cme in _context.ComorbidConditionExclusions on mhd.ConditionCode equals cme.mappingCode
                           join p in _context.Program on mhd.ProgramCode equals p.code
                           where mhd.ProgramCode == "MNTLH"
                           where mhd.Id == id

                           select new MHDiagnosisViewModel
                           {
                               Id = mhd.Id,
                               DependantID = mhd.DependantID,
                               EffectiveDate = mhd.EffectiveDate,
                               EndDate = mhd.EndDate,
                               CreatedBy = mhd.CreatedBy,
                               CreatedDate = mhd.CreatedDate,
                               ModifiedBy = mhd.ModifiedBy,
                               ModifiedDate = mhd.ModifiedDate,
                               ICD10Code = mhd.ICD10Code,
                               Comment = mhd.Comment,
                               MappingCode = cme.mappingCode,
                               MappingDescription = cme.mappingDescription,
                               Program = p.ProgramName,
                               Active = mhd.Active,

                           }).FirstOrDefault();

            return results;
        }

        public CoMormidConditions GetCoMorbidByID(int id)
        {
            return _context.CoMorbidConditions.Where(x => x.id == id).Where(x => x.active == true).FirstOrDefault();
        }
        public int GetCoMorbidByName(string name)
        {
            return _context.CoMorbidTypes.Where(x => x.condition == name).Where(x => x.active == true).Select(x => x.id).FirstOrDefault();
        }

        public ServiceResult UpdateCoMorbidCondition(CoMormidConditions model)
        {
            var old = _context.CoMorbidConditions.Where(x => x.id == model.id).FirstOrDefault();
            old.coMorbidId = model.coMorbidId;
            old.CoMorbidTreatement = model.CoMorbidTreatement;
            old.effectiveDate = model.effectiveDate;
            old.treatementEndDate = model.treatementEndDate;
            old.modifiedDate = model.modifiedDate;
            old.modifiedBy = model.modifiedBy;
            old.programType = model.programType; //HCare-607
            old.generalComments = model.generalComments; //HCare-607
            old.followUp = model.followUp; //HCare-607
            old.active = model.active;

            return _context.SaveChanges();
        }

        public ServiceResult InsertPlanHistory(PatientPlanHistory model)
        {
            model.planName = _context.MedicalAidPlans.Where(x => x.Id == model.planId).Select(x => x.Name).FirstOrDefault();
            model.active = true;
            _context.PatientPlanHistory.Add(model);
            return SaveResult();
        }

        public ServiceResult InsertPaypointHistory(PayPointHistory model)
        {
            model.createdDate = DateTime.Now;
            //model.planName = _context.PayPoints.Where(x => x.Id == model.planId).Select(x => x.Name).FirstOrDefault();
            model.planName = _context.EmployerMasters.Where(x => x.EmployerCode == model.planId).Select(x => x.EmployerCode).FirstOrDefault();  /*HCare-923 Force to remerge*/
            if (string.IsNullOrEmpty(model.planName))
            {
                model.planName = model.planId.ToString();
            }
            _context.PayPointHistory.Add(model);
            return SaveResult();
        }

        public ServiceResult InsetEnquiryComment(EnquiryComments model)
        {
            model.createdDate = DateTime.Now;
            _context.EnquiryComments.Add(model);
            return SaveResult();
        }

        public ServiceResult InsertDoctorHistory(PatientDoctorHistory model)
        {
            _context.PatientDoctorHistory.Add(model);
            return SaveResult();
        }

        public ServiceResult InsertAuthorization(Authorisations model)
        {
            model.createdBy = "User Here";
            model.createdDate = DateTime.Now;
            _context.Authorisations.Add(model);
            return SaveResult();
        }

        public ServiceResult InsertClinicalRule(PatientClinicalRulesHistory model)
        {
            model.createdBy = "Clinical Rules";
            model.createdDate = DateTime.Now;
            _context.PatientClinicalRulesHistory.Add(model);
            return SaveResult();

        }

        public Guid InsertNote(Notes note)
        {
            note.createdDate = DateTime.Now;
            note.Active = true;
            _context.Notes.Add(note);
            Save();
            return note.dependentID;
        }

        public void InsertQuery(Queries model)
        {
            //model.createdDate = DateTime.Now;
            //model.Active = true;
            //model.queryStatus = true;
            //model.effectiveDate = DateTime.Now;

            _context.Queries.Add(model);
            Save();
        }

        public void InsertMessage(SmsMessages model)
        {
            model.createdDate = DateTime.Now;
            model.Active = true;

            _context.SmsMessages.Add(model);
            Save();
        }

        public ServiceResult InsertText_HCDWL(string depdendantID, string mobileNo, string effectiveDate, int template, string message, string status, string reference, string programID, string createdBy, string createdDate)
        {
            string sql = string.Format(@"INSERT INTO [dbo].[SMSMessages]
                                            ([DependantID]
                                            ,[MobileNumber]
                                            ,[EffectiveDate]
                                            ,[TemplateID]
                                            ,[Message]
                                            ,[Status]
                                            ,[ReferenceNumber]
                                            ,[ResponseString]
                                            ,[AttuneID]
                                            ,[AttuneMessageStatus]
                                            ,[AttuneDeliveryDate]
                                            ,[ProgramID]
                                            ,[CreatedBy]
                                            ,[CreatedDate]
                                            ,[ModifiedBy]
                                            ,[ModifiedDate]
                                            ,[Active])
                                        VALUES
                                            ('{0}','{1}','{2}','{3}','{4}','{5}','{6}', NULL, NULL, NULL, NULL, '{7}', '{8}', '{9}', NULL, NULL, 1)
                                            ", depdendantID, mobileNo, effectiveDate, template, message, status, reference, programID, createdBy, createdDate);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("Warehouse")))
            {
                db.Open();
                db.Execute(sql, null);
                db.Close();

                return null;
            }
        }

        public void InsertEmailMessage(Emails model)
        {
            model.createdDate = DateTime.Now;
            model.status = "Queued";
            _context.Emails.Add(model);
            Save();
        }

        //HCare-860
        #region HCare-860
        public void InsertEmailAttachment(ComsViewModel model, List<IFormFile> file)
        {
            string serverDir = Configuration.GetSection("AppSettings")["dir"].ToString();
            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            string directory = Directory.CreateDirectory(@"" + serverDir + model.emailMessages.emailId).ToString();

            foreach (var fileName in file)
            {
                IFormFile _file = fileName;
                var nameFile = Path.GetFileName(_file.FileName);
                string _path = Path.Combine(serverDir + directory, model.emailMessages.emailId + nameFile);

                using (var stream = new FileStream(_path, FileMode.Create))
                {
                    fileName.CopyToAsync(stream);
                }

                AddAttachmentPath(model.emailMessages.emailId, _path);

            }
        }
        // HCare-860
        public void AddAttachmentPath(int emailId, string FilePath)
        {
            EmailAttachments _emailAttachments = new EmailAttachments();
            _emailAttachments.EmailID = emailId;
            _emailAttachments.FilePath = FilePath;
            _context.EmailAttachments.Add(_emailAttachments);
            Save();
        }
        #endregion

        public Guid InsertDependant(Dependant dependent)
        {
            dependent.DependantID = Guid.NewGuid();
            _context.Dependants.Add(dependent);
            Save();
            return dependent.DependantID;
        }

        public Guid InsertMemberData(MemberImports member)
        {
            member.Id = Guid.NewGuid();
            _context.MemberImports.Add(member);
            Save();
            return member.Id;
        }

        public ServiceResult UpdateMemberData(MemberImports member)
        {
            var oldData = _context.MemberImports.Where(x => x.DependantId == member.DependantId).FirstOrDefault();
            oldData.membershipNo = member.membershipNo;
            oldData.dependantCode = member.dependantCode;
            oldData.employerCode = member.employerCode;
            oldData.firstName = member.firstName;
            oldData.lastName = member.lastName;
            oldData.title = member.title;
            oldData.medicalAidCode = member.medicalAidCode;
            oldData.iDNumber = member.iDNumber;
            oldData.language = member.language;
            oldData.gender = member.gender;
            oldData.option = member.option;
            oldData.dateOfBirth = member.dateOfBirth;
            oldData.emailAddress = member.emailAddress;
            oldData.addressLine1 = member.addressLine1;
            oldData.addressLine2 = member.addressLine2;
            oldData.cellphone = member.cellphone;
            oldData.city = member.city;
            oldData.postalCode = member.postalCode;
            oldData.optionStatus = member.optionStatus;

            return _context.SaveChanges(); ;
        }

        public Guid InsertContact(Contact contact)
        {
            contact.ContactID = Guid.NewGuid();
            contact.Active = true;
            _context.Contacts.Add(contact);
            Save();
            return contact.ContactID;
        }

        public Guid InsertContact_MI(ComsViewModel model)
        {
            model.memberInformation.Contacts.ContactID = Guid.NewGuid();
            _context.Contacts.Add(model.memberInformation.Contacts);
            Save();
            return model.memberInformation.Contacts.ContactID;
        }

        public Guid InsertAddress(Address address)
        {
            address.addressID = Guid.NewGuid();
            _context.Address.Add(address);
            Save();
            return address.addressID;
        }

        public Guid InsertMemberInformation(MemberInformation meminfo)
        {
            meminfo.MemberInformationID = Guid.NewGuid();
            _context.MemberInformation.Add(meminfo);
            Save();
            return meminfo.MemberInformationID;
        }

        public ServiceResult InsertPathology(Pathology pathology)
        {
            //HCare-741
            DateTime effective = Convert.ToDateTime(pathology.effectiveDate);
            pathology.effectiveDate = new DateTime(effective.Year, effective.Month, effective.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);

            if (pathology.CD4CounteffectiveDate != null)
            {
                DateTime CD4Count = Convert.ToDateTime(pathology.CD4CounteffectiveDate);
                pathology.CD4CounteffectiveDate = new DateTime(CD4Count.Year, CD4Count.Month, CD4Count.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.CD4PercentageeffectiveDate != null)
            {
                DateTime CD4Percent = Convert.ToDateTime(pathology.CD4PercentageeffectiveDate);
                pathology.CD4PercentageeffectiveDate = new DateTime(CD4Percent.Year, CD4Percent.Month, CD4Percent.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.viralLoadeffectiveDate != null)
            {
                DateTime viralLoad = Convert.ToDateTime(pathology.viralLoadeffectiveDate);
                pathology.viralLoadeffectiveDate = new DateTime(viralLoad.Year, viralLoad.Month, viralLoad.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.haemoglobineffectiveDate != null)
            {
                DateTime haemoglobine = Convert.ToDateTime(pathology.haemoglobineffectiveDate);
                pathology.haemoglobineffectiveDate = new DateTime(haemoglobine.Year, haemoglobine.Month, haemoglobine.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.bilirubineffectiveDate != null)
            {
                DateTime bilirubin = Convert.ToDateTime(pathology.bilirubineffectiveDate);
                pathology.bilirubineffectiveDate = new DateTime(bilirubin.Year, bilirubin.Month, bilirubin.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.totalCholestroleffectiveDate != null)
            {
                DateTime totalCholestrol = Convert.ToDateTime(pathology.totalCholestroleffectiveDate);
                pathology.totalCholestroleffectiveDate = new DateTime(totalCholestrol.Year, totalCholestrol.Month, totalCholestrol.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.hdleffectiveDate != null)
            {
                DateTime hdl = Convert.ToDateTime(pathology.hdleffectiveDate);
                pathology.hdleffectiveDate = new DateTime(hdl.Year, hdl.Month, hdl.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.ldleffectiveDate != null)
            {
                DateTime ldl = Convert.ToDateTime(pathology.ldleffectiveDate);
                pathology.ldleffectiveDate = new DateTime(ldl.Year, ldl.Month, ldl.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.triglycerideseffectiveDate != null)
            {
                DateTime triglycerides = Convert.ToDateTime(pathology.triglycerideseffectiveDate);
                pathology.triglycerideseffectiveDate = new DateTime(triglycerides.Year, triglycerides.Month, triglycerides.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.glucoseeffectiveDate != null)
            {
                DateTime glucose = Convert.ToDateTime(pathology.glucoseeffectiveDate);
                pathology.glucoseeffectiveDate = new DateTime(glucose.Year, glucose.Month, glucose.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.hba1ceffectiveDate != null)
            {
                DateTime hbA1c = Convert.ToDateTime(pathology.hba1ceffectiveDate);
                pathology.hba1ceffectiveDate = new DateTime(hbA1c.Year, hbA1c.Month, hbA1c.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.alteffectiveDate != null)
            {
                DateTime alt = Convert.ToDateTime(pathology.alteffectiveDate);
                pathology.alteffectiveDate = new DateTime(alt.Year, alt.Month, alt.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.asteffectiveDate != null)
            {
                DateTime ast = Convert.ToDateTime(pathology.asteffectiveDate);
                pathology.asteffectiveDate = new DateTime(ast.Year, ast.Month, ast.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.ureaeffectiveDate != null)
            {
                DateTime urea = Convert.ToDateTime(pathology.ureaeffectiveDate);
                pathology.ureaeffectiveDate = new DateTime(urea.Year, urea.Month, urea.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.creatinineeffectiveDate != null)
            {
                DateTime creatinine = Convert.ToDateTime(pathology.creatinineeffectiveDate);
                pathology.creatinineeffectiveDate = new DateTime(creatinine.Year, creatinine.Month, creatinine.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.eGfreffectiveDate != null)
            {
                DateTime eGfr = Convert.ToDateTime(pathology.eGfreffectiveDate);
                pathology.eGfreffectiveDate = new DateTime(eGfr.Year, eGfr.Month, eGfr.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.mauCreatRatioeffectiveDate != null)
            {
                DateTime mauCreatRatio = Convert.ToDateTime(pathology.mauCreatRatioeffectiveDate);
                pathology.mauCreatRatioeffectiveDate = new DateTime(mauCreatRatio.Year, mauCreatRatio.Month, mauCreatRatio.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.BPeffectiveDate != null)
            {
                DateTime BP = Convert.ToDateTime(pathology.BPeffectiveDate);
                pathology.BPeffectiveDate = new DateTime(BP.Year, BP.Month, BP.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.FEV1effectiveDate != null)
            {
                DateTime FEV1 = Convert.ToDateTime(pathology.FEV1effectiveDate);
                pathology.FEV1effectiveDate = new DateTime(FEV1.Year, FEV1.Month, FEV1.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.EosinophyliaeffectiveDate != null)
            {
                DateTime Eosinophylia = Convert.ToDateTime(pathology.EosinophyliaeffectiveDate);
                pathology.EosinophyliaeffectiveDate = new DateTime(Eosinophylia.Year, Eosinophylia.Month, Eosinophylia.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.hivElizaeffectiveDate != null)
            {
                DateTime hivEliza = Convert.ToDateTime(pathology.hivElizaeffectiveDate);
                pathology.hivElizaeffectiveDate = new DateTime(hivEliza.Year, hivEliza.Month, hivEliza.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.normGtteffectiveDate != null)
            {
                DateTime normGtt = Convert.ToDateTime(pathology.normGtteffectiveDate);
                pathology.normGtteffectiveDate = new DateTime(normGtt.Year, normGtt.Month, normGtt.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.abnGtteffectiveDate != null)
            {
                DateTime abnGtt = Convert.ToDateTime(pathology.abnGtteffectiveDate);
                pathology.abnGtteffectiveDate = new DateTime(abnGtt.Year, abnGtt.Month, abnGtt.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.ucreatinineeffectiveDate != null)
            {
                DateTime ucreatinine = Convert.ToDateTime(pathology.ucreatinineeffectiveDate);
                pathology.ucreatinineeffectiveDate = new DateTime(ucreatinine.Year, ucreatinine.Month, ucreatinine.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.salbumineffectiveDate != null)
            {
                DateTime salbumin = Convert.ToDateTime(pathology.salbumineffectiveDate);
                pathology.salbumineffectiveDate = new DateTime(salbumin.Year, salbumin.Month, salbumin.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }
            if (pathology.ualbuminuriaeffectiveDate != null)
            {
                DateTime ualbuminuria = Convert.ToDateTime(pathology.ualbuminuriaeffectiveDate);
                pathology.ualbuminuriaeffectiveDate = new DateTime(ualbuminuria.Year, ualbuminuria.Month, ualbuminuria.Day, pathology.createdDate.Hour, pathology.createdDate.Minute, pathology.createdDate.Second);
            }

            _context.Pathology.Add(pathology);
            return _context.SaveChanges();
        }

        public int InsertScript(Scripts script)
        {
            script.createdDate = DateTime.Now;
            script.active = true;
            script.Status = "TBA";
            _context.Scripts.Add(script);
            Save();

            return script.scriptID;
        }
        public ServiceResult InsertScriptItem(ScriptItems items)
        {
            items.active = true;
            _context.ScriptItems.Add(items);
            return _context.SaveChanges();
        }
        public ServiceResult InsertScriptReference(ScriptReferences model)
        {
            model.active = true;
            _context.ScriptReferences.Add(model);
            model.createdDate = DateTime.Now;
            return _context.SaveChanges();
        }

        public void InsertAttachment(Attachments attachment)
        {
            attachment.createdDate = DateTime.Now;
            attachment.Active = true;
            _context.Attachments.Add(attachment);
            Save();
        }

        public ServiceResult InsertProgramHistory(PatientProgramHistory model)
        {
            model.endDate = DateTime.Now.AddYears(20);
            model.createdDate = DateTime.Now;
            model.active = true;
            _context.PatientProgramHistory.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult InsertProgram(Programs model)
        {
            _context.Program.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult InsertAuthLetter(AuthorizationLetters letter)
        {
            letter.createdDate = DateTime.Now;
            letter.Active = true;
            _context.AuthorizationLetters.Add(letter);
            return _context.SaveChanges();
        }

        public void DeleteMembers(Guid memberId)
        {
            Member member = GetMembers(memberId);
            _context.Members.Remove(member);
            Save();
        }

        public void UpdateMembers(Member model)
        {
            Member member = GetMembers(model.memberID);
            member.modifiedBy = model.modifiedBy;
            member.modifiedDate = model.modifiedDate;
            member.membershipNo = model.membershipNo;
            member.medicalAidID = model.medicalAidID;

            _context.SaveChanges();
        }

        public void UpdateMemberInformation(MemberInformation member)
        {
            var oldmember = _context.MemberInformation.Where(x => x.dependantID == member.dependantID).FirstOrDefault();
            oldmember.addressID = member.addressID;
            oldmember.dependantID = member.dependantID;
            oldmember.memberID = member.memberID;
            oldmember.programID = member.programID;
            oldmember.doctorID = member.doctorID;
            oldmember.caseManagerID = member.caseManagerID;
            oldmember.contactID = member.contactID;
            oldmember.AllowText = member.AllowText;

            _context.SaveChanges();
        }

        public ServiceResult UpdateAddress(Address model)
        {
            var old = _context.Address.Where(x => x.addressID == model.addressID).FirstOrDefault();
            if (old != null)
            {
                old.addressID = model.addressID;
                old.line1 = model.line1;
                old.line2 = model.line2;
                old.line3 = model.line3;
                old.province = model.province;
                old.city = model.city;
                old.zip = model.zip;
                old.modifiedBy = model.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.Active = model.Active;

            }
            else
            {
                model.Active = true;
                model.createdBy = model.modifiedBy;
                model.createdDate = DateTime.Now;
                _context.Address.Add(model);
            }
            return _context.SaveChanges();
        }

        public ServiceResult UpdateAddress_MI(ComsViewModel model)
        {
            var old = _context.Address.Where(x => x.addressID == model.memberInformation.addressID).FirstOrDefault();
            if (old != null)
            {
                //old.addressID = model.memberInformation.Address.addressID;
                old.line1 = model.memberInformation.Address.line1;
                old.line2 = model.memberInformation.Address.line2;
                old.line3 = model.memberInformation.Address.line3;
                old.province = model.memberInformation.Address.province;
                old.city = model.memberInformation.Address.city;
                old.zip = model.memberInformation.Address.zip;
                old.modifiedBy = model.memberInformation.Address.modifiedBy;
                old.modifiedDate = DateTime.Now;

            }
            else
            {
                model.memberInformation.Address.createdBy = model.memberInformation.Address.modifiedBy;
                model.memberInformation.Address.createdDate = DateTime.Now;
                _context.Address.Add(model.memberInformation.Address);
            }
            return _context.SaveChanges();
        }

        public ServiceResult UpdateContact(Contact model)
        {
            var old = _context.Contacts.Where(x => x.ContactID == model.ContactID).FirstOrDefault();
            if (old != null)
            {
                old.ContactID = model.ContactID;
                old.contactName = model.contactName;
                old.cell = model.cell;
                old.email = model.email;
                old.workNo = model.workNo;
                old.landLine = model.landLine;
                old.fax = model.fax;
                old.modifiedBy = model.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.Active = model.Active;
            }
            else
            {
                model.Active = true;
                model.createdBy = model.modifiedBy;
                model.createdDate = DateTime.Now;
                _context.Contacts.Add(model);
            }

            return _context.SaveChanges();
        }

        public ServiceResult UpdateContact_MI(ComsViewModel model)
        {
            var old = _context.Contacts.Where(x => x.ContactID == model.memberInformation.contactID).FirstOrDefault();
            if (old != null)
            {
                old.contactName = model.memberInformation.Contacts.contactName;
                old.cell = model.memberInformation.Contacts.cell;
                old.email = model.memberInformation.Contacts.email;
                old.workNo = model.memberInformation.Contacts.workNo;
                old.landLine = model.memberInformation.Contacts.landLine;
                old.ISeriesCell = model.memberInformation.Contacts.ISeriesCell;
                old.ISeriesEmail = model.memberInformation.Contacts.ISeriesEmail;
                old.ISeriesWorkNo = model.memberInformation.Contacts.ISeriesWorkNo;
                old.ISeriesLandLine = model.memberInformation.Contacts.ISeriesLandLine;
                old.fax = model.memberInformation.Contacts.fax;
                old.modifiedBy = model.memberInformation.Contacts.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.preferredMethodOfContact = model.memberInformation.Contacts.preferredMethodOfContact;
                old.Active = model.memberInformation.Contacts.Active;
            }
            else
            {
                model.memberInformation.Contacts.Active = true;
                model.memberInformation.Contacts.createdBy = model.memberInformation.Contacts.modifiedBy;
                model.memberInformation.Contacts.createdDate = DateTime.Now;
                _context.Contacts.Add(model.memberInformation.Contacts);
            }

            return _context.SaveChanges();
        }

        public ServiceResult UpdateScriptItems(List<ScriptViewModel> items, string status)
        {
            var scriptItem = new ScriptItems();
            foreach (var item in items)
            {
                scriptItem = _context.ScriptItems.Where(x => x.itemNo == item.itemNo).FirstOrDefault();
                scriptItem.itemStatus = status;
                _context.SaveChanges();
            }
            return _context.SaveChanges();
        }

        public ServiceResult UpdateScript(Scripts model)
        {
            var old = _context.Scripts.Where(x => x.scriptID == model.scriptID).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.firstLine = model.firstLine;
            old.secondLine = model.secondLine;
            old.salvageTherapy = model.salvageTherapy;
            old.prophylaxis = model.prophylaxis;
            old.resistanceTest = model.resistanceTest;
            old.effectiveDate = model.effectiveDate;
            old.active = model.active;
            old.Status = model.Status;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateProgramHistory(PatientProgramHistory model)
        {
            var old = _context.PatientProgramHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.effectiveDate = model.effectiveDate;
            //old.endDate = model.endDate;
            //old.icd10Code = model.icd10Code;
            //old.programCode = model.programCode;
            //old.active = model.active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdatePatientProgramHistory(int id, string icd10, DateTime? enddate, string user)
        {
            var old = _context.PatientProgramHistory.Where(x => x.id == id).FirstOrDefault();
            old.icd10Code = icd10;
            old.endDate = enddate;
            old.modifiedBy = user;
            old.modifiedDate = DateTime.Now;

            return _context.SaveChanges();
        }

        public ServiceResult UpdatePreviousPatientProgramHistory(PatientProgramHistory model)
        {
            var old = _context.PatientProgramHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.active = false;
            old.Comment = "User stopped diagnosis.";

            return _context.SaveChanges();
        }

        public ServiceResult UpdateLastPatientProgramHistory(PatientProgramHistory model)
        {
            var old = _context.PatientProgramHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.active = true;
            old.Comment = "User stopped last diagnosis.";

            return _context.SaveChanges();
        }

        public ServiceResult UpdateDependant(Dependant member)
        {
            //HCare - 1088
            var dependent = _context.Dependants.Where(x => x.DependantID == member.DependantID).FirstOrDefault();

            var isNumeric = int.TryParse(member.idNumber, out int n); //HCare-1066
            #region idNumber
            if (member.firstName != null)
            {
                member.initials = member.firstName.FirstOrDefault().ToString();//HCare-1088
                if (member.idNumber.Length >= 12 && isNumeric == true)
                {
                    var decade = 1900;
                    if (Convert.ToInt16(member.idNumber.Substring(0, 2)) < 19)
                    {
                        decade = 2000;
                    }
                    DateTime birtdate = new DateTime(decade + Convert.ToInt16(member.idNumber.Substring(0, 2)), Convert.ToInt16(member.idNumber.Substring(2, 2)), Convert.ToInt16(member.idNumber.Substring(4, 2)));
                    dependent.birthDate = birtdate;
                }
                #endregion
                dependent.idNumber = member.idNumber;
                dependent.dependentCode = member.dependentCode;
                dependent.icd10Code = member.icd10Code;
                dependent.initials = member.initials;
                dependent.lastName = member.lastName;
                dependent.firstName = member.firstName;
                dependent.isSACitizen = member.isSACitizen;
                dependent.isPrinciple = member.isPrinciple;
                dependent.dispensingProvider = member.dispensingProvider;
                if (!string.IsNullOrEmpty(member.demographic)) dependent.demographic = member.demographic;
                if (!string.IsNullOrEmpty(member.dependentType)) dependent.dependentType = member.dependentType;
                dependent.sex = member.sex;
                dependent.title = member.title;
                dependent.originationId = member.originationId;
                dependent.languageCode = member.languageCode;
                dependent.optOut = member.optOut;
                dependent.Active = member.Active;
                dependent.modifiedBy = member.modifiedBy;//1255
                dependent.modifiedDate = member.modifiedDate;//1255

                return _context.SaveChanges();
            }
            else
            {
                if (member.idNumber.Length >= 12 && isNumeric == true)
                {
                    var decade = 1900;
                    if (Convert.ToInt16(member.idNumber.Substring(0, 2)) < 19)
                    {
                        decade = 2000;
                    }
                    DateTime birtdate = new DateTime(decade + Convert.ToInt16(member.idNumber.Substring(0, 2)), Convert.ToInt16(member.idNumber.Substring(2, 2)), Convert.ToInt16(member.idNumber.Substring(4, 2)));
                    dependent.birthDate = birtdate;
                }
                dependent.idNumber = member.idNumber;
                var res = _context.SaveChanges();
                return res;

            }
        }

        public ServiceResult InsertMembershipHistory(MembershipNoHistory model)
        {
            model.createdDate = DateTime.Now;
            model.changeDate = DateTime.Now;
            model.active = true;

            _context.MembershipNoHistory.Add(model);
            return _context.SaveChanges();
        }

        public List<AssignmentView> GetNewAssignments(Guid depID)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == depID

                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               itemType = ai.itemType
                           }).ToList();

            for (int i = results.Count() - 1; i >= 0; i--)
            {
                var result = results[i];
                var taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == result.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count();
                if (taskClosedCount > 0)
                {
                    results.Remove(result);
                }
            }

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public List<AssignmentView> GetActiveOnlyAssignments(Guid dependentId)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == dependentId

                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               itemType = ai.itemType
                           }).ToList();

            for (int i = results.Count() - 1; i >= 0; i--)
            {
                var result = results[i];
                var taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == result.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count();
                if (taskClosedCount == 0)
                {
                    results.Remove(result);
                }
            }


            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public List<AssignmentView> GetActiveAssignments(Guid dependentId)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               itemType = ai.itemType
                           }).ToList();


            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public List<AssignmentView> GetActivePatientAssignments(Guid dependentId)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == dependentId
                           where a.Active == true
                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               createdDate = a.createdDate, //10 April 2019 -- HCare-660 (Order by Descending)
                               itemType = ai.itemType
                           }).OrderByDescending(x => x.createdDate).ToList();


            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;

        }

        public List<AssignmentView> GetActivePatientAssignments(Guid dependentId, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == dependentId
                           where (ait.program == programCode || ait.program == null)
                           where (a.programId == Id || a.programId == null)
                           where a.Active == true
                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               itemType = ait.assignmentItemType,
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               createdDate = a.createdDate, //10 April 2019 -- HCare-660 (Order by Descending)
                           }).OrderByDescending(x => x.createdDate).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();
            return results;

        }

        public List<AssignmentView> GetClosedPatientAssignments(Guid dependentId)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.Active == false
                           where a.dependentID == dependentId

                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               comment = a.comment,
                               closedBy = a.closedBy,
                               closedDate = a.closedDate,
                               createdDate = a.createdDate,
                               modifiedDate = a.modifiedDate, //HCare-660 (Order by Descending)
                               itemType = ai.itemType
                           }).OrderByDescending(x => x.modifiedDate).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();
            return results;
        }

        public List<AssignmentView> GetClosedPatientAssignments(Guid dependentId, Guid Id)
        {
            var programCode = _context.Program.Where(x => x.programID == Id).Select(x => x.code).FirstOrDefault();
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where (ait.program == programCode || ait.program == null)
                           where (a.programId == Id || a.programId == null)
                           where a.Active == false
                           where a.dependentID == dependentId

                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               itemType = ait.assignmentItemType,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               comment = a.comment,
                               closedBy = a.closedBy,
                               closedDate = a.closedDate,
                               createdDate = a.createdDate,
                               modifiedDate = a.modifiedDate, //HCare-660 (Order by Descending)
                           }).OrderByDescending(x => x.modifiedDate).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public List<Assignments> GetActiveSelectAssignments(Guid dependentId)
        {
            var results = _context.Assignments.Where(x => x.dependentID == dependentId).Where(x => x.Active == true).OrderByDescending(x => x.effectiveDate).ToList();
            foreach (var result in results)
            {
                result.assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == result.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault();
                result.assignmentType = result.assignmentType + " " + result.assignmentID;
            }
            return results;
        }

        public List<Assignments> GetActiveMemberAssignments(Guid dependentId)
        {
            var results = _context.Assignments.Where(x => x.dependentID == dependentId).Where(x => x.Active == true).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public List<CaseManagers> GetCaseManagers()
        {
            var results = _context.CaseManagers.Where(x => x.Active == true).OrderByDescending(x => x.caseManagerName).ToList();
            return results;
        }

        public List<Assignments> GetAllActiveAssignments()
        {
            return _context.Assignments.Where(x => x.Active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<AssignmentView> GetClosedAssignments(Guid dependentId)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.Active == false
                           where a.dependentID == dependentId

                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               comment = a.comment,
                               closedBy = a.closedBy,
                               closedDate = a.closedDate

                           }).ToList();
            return results;
        }

        public MemberInformation GetMemberInformation(Guid DependentID)
        {
            return _context.MemberInformation.Where(x => x.dependantID == DependentID).FirstOrDefault();
        }

        public MemberInformation GetMemberInformation_MI(Guid DependentID)
        {
            return _context.MemberInformation.Where(x => x.dependantID == DependentID).FirstOrDefault();
        }

        public Address GetAddress(Address address)
        {
            var Address = _context.Address.Where(x => x.line1 == address.line1 && x.line2 == address.line2 && x.line3 == address.line3).FirstOrDefault();
            return Address;
        }

        public Address GetAddressById(Guid Id)
        {
            var Address = _context.Address.Where(x => x.addressID == Id).FirstOrDefault();
            return Address;
        }

        public Contact GetContactById(Guid Id)
        {
            var Contact = _context.Contacts.Where(x => x.ContactID == Id).FirstOrDefault();
            return Contact;
        }

        public MemberInformation GetContactById_MI(Guid Id)
        {
            var Contact = _context.MemberInformation.Where(x => x.Contacts.ContactID == Id).FirstOrDefault();
            return Contact;
        }

        public ComsViewModel GetContactByDependentID(Guid DependentID)
        {
            var results = (from d in _context.Dependants
                           join mi in _context.MemberInformation
                           on d.DependantID equals mi.dependantID
                           join a in _context.Address
                           on mi.addressID equals a.addressID
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID
                           where d.DependantID == DependentID
                           select new ComsViewModel
                           {
                               address = a,
                               contacts = c,
                               memberInformation = mi

                           }).FirstOrDefault();

            return results;
        }

        public ComsViewModel GetAddressByDependentID(Guid DependentID)
        {
            var results = (from d in _context.Dependants
                           join mi in _context.MemberInformation
                           on d.DependantID equals mi.dependantID
                           join a in _context.Address
                           on mi.addressID equals a.addressID
                           where d.DependantID == DependentID
                           select new ComsViewModel
                           {
                               address = a,
                               memberInformation = mi

                           }).FirstOrDefault();

            return results;
        }

        public List<AttachmentTypes> GetAttachmentTypes()
        {
            return _context.AttachmentTypes.Where(x => x.active == true).OrderBy(x => x.typeDescription).ToList();
        }

        public string GetAttachmentType(string attachmenttype)
        {
            return _context.AttachmentTypes.Where(x => x.active == true).Where(x => x.attachmentType == attachmenttype).Select(x => x.typeDescription).FirstOrDefault();
        }

        public List<Attachments> GetAttachments(Guid dependentID)
        {
            var results = _context.Attachments.Where(x => x.dependentID == dependentID).Where(x => x.Active == true).OrderByDescending(x => x.createdDate).ToList();
            foreach (var result in results)
            {
                result.attachmentType = _context.AttachmentTypes.Where(x => x.attachmentType == result.attachmentType).Select(x => x.typeDescription).FirstOrDefault();
            }

            return results;
        }

        public Attachments GetAttachment(int id)
        {
            var result = _context.Attachments.Where(x => x.attachmentID == id).FirstOrDefault();
            if (result == null)
                return null;

            return result;
        }

        public ServiceResult UpdateAttachment(Attachments model)
        {
            var old = _context.Attachments.Where(x => x.attachmentID == model.attachmentID).FirstOrDefault();

            old.attachmentName = model.attachmentName;
            old.attachmentType = model.attachmentType;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public Address GetAddress(Guid depID)
        {
            var results = (from d in _context.Address
                           join m in _context.MemberInformation
                           on d.addressID equals m.addressID
                           where m.dependantID == depID
                           select d
                          ).FirstOrDefault();

            return results;
        }

        public Contact GetContact(Contact contact)
        {
            var Contact = _context.Contacts.Where(x => x.contactName == contact.contactName).Where(x => x.cell == contact.cell).Where(x => x.email == contact.email).FirstOrDefault();
            return Contact;
        }

        //public Contact GetContact(Guid depId)
        //{
        //    var results = (from d in _context.Contacts 
        //                   join m in _context.MemberInformation
        //                   on d.ContactID equals m.contactID
        //                   where m.dependantID == depId
        //                   select d
        //                  ).FirstOrDefault();

        //    results.preferredMethodOfContact = _context.preferredMethodOfContacts.Where(x => x.pmocCode == results.preferredMethodOfContact).Select(x => x.pmocDescription).FirstOrDefault();

        //    return results;
        //}

        public Contact GetContact(Guid depId)
        {
            var results = (from mi in _context.MemberInformation
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID into contactInfo
                           from c in contactInfo.DefaultIfEmpty()
                           where mi.dependantID == depId
                           select c
                          ).FirstOrDefault();

            if (results != null)
            {
                results.preferredMethodOfContact = _context.preferredMethodOfContacts.Where(x => x.pmocCode == results.preferredMethodOfContact).Select(x => x.pmocDescription).FirstOrDefault();
            }
            return results;
        }

        public MemberInformation GetContact_MI(Guid depId)
        {
            var results = (from mi in _context.MemberInformation
                           join c in _context.Contacts
                           on mi.contactID equals c.ContactID
                           where mi.dependantID == depId
                           select mi

                          ).FirstOrDefault();

            return results;
        }

        public Contact GetDrInformation(Guid doctorId)
        {
            var result = (from d in _context.Doctors
                          join di in _context.DoctorInformation
                          on d.doctorID equals di.doctorID into din
                          from di in din.DefaultIfEmpty()
                          join c in _context.Contacts
                          on di.contactID equals c.ContactID into co
                          from c in co.DefaultIfEmpty()
                          where d.doctorID == doctorId
                          select c)
                          .FirstOrDefault();

            return result;

        }

        public List<Scripts> GetScripts(Guid DepID)
        {
            return _context.Scripts.Where(x => x.dependentID == DepID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public Scripts GetScript(int scriptID)
        {
            return _context.Scripts.Where(x => x.scriptID == scriptID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
        }

        public List<ScriptTypes> GetScriptTypes()
        {
            return _context.ScriptTypes.Where(x => x.active == true).ToList();
        }

        public List<AuthorizationLetters> GetAuthorizationLetters(Guid dependentId)
        {
            return _context.AuthorizationLetters.Where(x => x.Active == true).Where(x => x.dependantID == dependentId).OrderByDescending(x => x.createdDate).ToList();
        }

        public List<DoctorQuestionnaireResults> GetDoctorQuestionResultsById(Guid dependentId)
        {
            return _context.DoctorQuestionnaireResults.Where(x => x.Active == true).Where(x => x.dependentID == dependentId).OrderByDescending(x => x.createdDate).ToList();
        }


        public AssignmentItemLogs GetAssignmentItemLogs(int id)
        {
            return _context.AssignmentItemLogs.Where(x => x.assignmentItemID == id).FirstOrDefault();
        }

        public ServiceResult InsertAssignmentLog(AssignmentItemLogs model)
        {
            model.active = true;
            _context.AssignmentItemLogs.Add(model);
            var result = SaveResult();

            return result;
        }

        public ServiceResult UpdateAssignmentLog(AssignmentItemLogs model)
        {
            var old = _context.AssignmentItemLogs.Where(x => x.id == model.id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.captured = model.captured;
            old.followedUp = model.followedUp;
            old.attached = model.attached;

            var result = SaveResult();

            return result;
        }

        public List<CoMorbidTypes> GetCoMorbids()
        {
            return _context.CoMorbidTypes.Where(x => x.active == true).ToList();
        }
        public List<string> GetCoMorbidList()
        {
            return _context.CoMorbidTypes.Where(x => x.active == true).Select(x => x.condition).Distinct().ToList();
        }

        public List<CoMorbidConditionVM> GetCoMorbids_Excluded(Guid depId)
        {
            string sql = string.Format(@"SELECT ct.id[coMorbidId], ct.condition + ' - ' + ct.icd10[icd10Condition]
                                        FROM CoMorbidTypes ct
                                        WHERE ct.id not in(
                                        SELECT coMorbidId
                                        FROM CoMormidConditions
                                        WHERE dependantID ='{0}'
                                        AND treatementEndDate is NULL
                                        );", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        //Returns all but the conditions which show in the history
        public List<CoMorbidConditionVM> GetCoMorbidsNotLinkedToDependant(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.condition FROM CoMorbidTypes ct
                                        WHERE ct.condition NOT IN(SELECT ct.condition FROM CoMorbidTypes ct
                                        INNER JOIN CoMormidConditions cc on ct.id = cc.coMorbidId
                                        WHERE dependantID ='{0}' AND (treatementEndDate is NULL OR treatementEndDate > GETDATE()) AND cc.active = 1)", depId);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.mappingDescription[Condition] , ct.mappingCode[mappingCode]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingDescription NOT IN(SELECT ct.mappingDescription FROM ComorbidConditionExclusions ct
                                        INNER JOIN CoMormidConditions cc on ct.id = cc.coMorbidId
                                        WHERE cc.dependantID ='{0}' AND (cc.treatementEndDate is NULL OR cc.treatementEndDate > GETDATE()) AND cc.active = 1)
                                        AND ct.Active = 1", depId);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.mappingDescription[Condition], ct.mappingCode[mappingCode]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE (ct.mappingCode NOT IN(SELECT mhd.ConditionCode FROM MentalHealthDiagnosis mhd WHERE mhd.DependantID ='{0}' AND (mhd.EndDate is NULL OR mhd.EndDate > GETDATE()) AND mhd.Active = 1 AND mhd.ProgramCode = 'MNTLH')
                                        AND ct.mappingDescription NOT IN(SELECT distinct cme.mappingDescription FROM ComorbidConditionExclusions cme INNER JOIN MentalHealthDiagnosis mhd on cme.mappingCode = mhd.ConditionCode WHERE mhd.DependantID ='{0}' AND (mhd.EndDate is NULL OR mhd.EndDate > GETDATE()) AND mhd.Active = 1))
                                        AND ct.Active = 1
                                        AND ct.formularycode IN ('Q23','Q26','L08','L13')", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis_ProgramHistory(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.mappingDescription[Condition], ct.mappingCode[mappingCode]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingCode NOT IN(SELECT ppsh.conditionCode FROM PatientProgramHistory ppsh WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.mappingDescription NOT IN(SELECT distinct cme.mappingDescription FROM ComorbidConditionExclusions cme INNER JOIN PatientProgramHistory ppsh on cme.mappingCode = ppsh.conditionCode WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.Active = 1", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis_SubHistory(Guid depId)
        {
            string sql = string.Format(@"SELECT distinct ct.mappingDescription[Condition], ct.mappingCode[mappingCode]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingCode NOT IN(SELECT ppsh.conditionCode FROM PatientProgramSubHistory ppsh WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.mappingDescription NOT IN(SELECT distinct cme.mappingDescription FROM ComorbidConditionExclusions cme INNER JOIN PatientProgramSubHistory ppsh on cme.mappingCode = ppsh.conditionCode WHERE ppsh.dependantID ='{0}' AND ppsh.endDate is NULL AND ppsh.active = 1)
                                        AND ct.Active = 1", depId);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        //ICD-10 that checks the history and removes anything already selected
        public List<CoMorbidConditionVM> GetCoMorbidsICD10NotLinkedToDependant(string condition, Guid dependantID)
        {
            string sql = string.Format(@"SELECT ct.id, ct.condition, ct.icd10
                                        FROM CoMorbidTypes ct
                                        WHERE ct.condition = '{0}'
                                        AND ct.icd10 not in(
                                        SELECT distinct ct.icd10
                                        FROM CoMorbidTypes ct
                                        INNER JOIN CoMormidConditions cc on ct.id = cc.coMorbidId
                                        WHERE dependantID ='{1}' AND (treatementEndDate is NULL OR treatementEndDate > GETDATE()) AND cc.active = 1)", condition, dependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant(string condition, Guid dependantID)
        {
            string sql = string.Format(@"SELECT ct.id, ct.mappingDescription[Condition], ct.ICD10Code[icd10]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingDescription = '{0}'
                                        AND ct.ICD10Code not in(
                                        SELECT distinct ct.ICD10Code
                                        FROM ComorbidConditionExclusions ct
                                        INNER JOIN CoMormidConditions cc on ct.id = cc.coMorbidId
                                        WHERE dependantID ='{1}' AND (treatementEndDate is NULL OR treatementEndDate > GETDATE()) AND cc.active = 1)
                                        AND ct.Active = 1", condition, dependantID);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH(string condition, Guid dependantID)
        {
            string sql = string.Format(@"SELECT ct.id, ct.mappingDescription[Condition], ct.ICD10Code[icd10]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingCode = '{0}'
                                        AND ct.Active = 1
                                        AND ct.ICD10Code not in (SELECT distinct mhd.icd10Code FROM MentalHealthDiagnosis mhd WHERE mhd.DependantID ='{1}' AND mhd.ProgramCode = 'MNTLH' AND mhd.EndDate is NULL AND mhd.Active = 1)", condition, dependantID);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }
        public List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH_ProgramHistory(string condition, Guid dependantID)
        {
            string sql = string.Format(@"SELECT ct.id, ct.mappingDescription[Condition], ct.ICD10Code[icd10]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingCode = '{0}'
                                        AND ct.ICD10Code not in (
                                        SELECT distinct ppsh.icd10Code
                                        FROM PatientProgramHistory ppsh
                                        WHERE ppsh.dependantId ='{1}' 
										AND ppsh.endDate is NULL AND 
										ppsh.active = 1)", condition, dependantID);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH_SubHistory(string condition, Guid dependantID)
        {
            string sql = string.Format(@"SELECT ct.id, ct.mappingDescription[Condition], ct.ICD10Code[icd10]
                                        FROM ComorbidConditionExclusions ct
                                        WHERE ct.mappingCode = '{0}'
                                        AND ct.ICD10Code not in (
                                        SELECT distinct ppsh.icd10Code
                                        FROM PatientProgramSubHistory ppsh
                                        WHERE ppsh.dependantId ='{1}' 
										AND ppsh.endDate is NULL AND 
										ppsh.active = 1)", condition, dependantID);
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        //public List<CoMorbidConditionVM> GetComorbidInformation(Guid dependantID)
        //{
        //    string sql = string.Format(@"SELECT cc.id, cc.dependantID, cc.coMorbidId [CMConditionId], ct.id[CMTypeID], ct.condition[Condition], ct.icd10[icd10],ct.condition + ' - ' + ct.icd10[icd10Condition], cc.effectiveDate[startDate], cc.treatementEndDate[endDate]
        //                                FROM CoMormidConditions cc
        //                                INNER JOIN CoMorbidTypes ct ON cc.coMorbidId = ct.id
        //                                WHERE cc.dependantID = '{0}'", dependantID);

        //    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        db.Open();
        //        var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
        //        db.Close();
        //        return ourCustomers;
        //    }
        //}

        public List<CoMorbidConditionVM> GetComorbidInformation(Guid dependantID)
        {
            var comorbidConditions = (from cm in _context.CoMorbidConditions
                                      where cm.dependantID == dependantID
                                      where cm.active == true
                                      where cm.followUp == false //hcare-1138: correction

                                      select new CoMorbidConditionVM
                                      {
                                          id = cm.id,
                                          CMConditionId = cm.coMorbidId,
                                          createdDate = cm.createdDate,
                                          effectiveDate = cm.effectiveDate,
                                          endDate = cm.treatementEndDate,
                                          programCode = cm.programType,

                                      }).ToList();

            var results = new List<CoMorbidConditionVM>();

            foreach (var result in comorbidConditions)
            {
                //HCare-1293
                //if (result.createdDate > Convert.ToDateTime("2020-09-20"))
                //{
                var cmlist1 = (from cm in _context.CoMorbidConditions
                               join cme in _context.ComorbidConditionExclusions on cm.coMorbidId equals cme.id
                               where cm.dependantID == dependantID
                               where cme.id == result.CMConditionId
                               where cm.id == result.id
                               where cm.active == true

                               select new CoMorbidConditionVM
                               {
                                   id = cm.id,
                                   CMConditionId = cm.coMorbidId,
                                   CMTypeID = cme.id,
                                   Condition = cme.mappingDescription,
                                   icd10 = cme.ICD10Code,
                                   icd10Condition = cme.mappingDescription + " - " + cme.ICD10Code,
                                   createdDate = cm.createdDate,
                                   effectiveDate = cm.effectiveDate,
                                   endDate = cm.treatementEndDate,
                                   programCode = cm.programType,

                               }).FirstOrDefault();

                results.Add(cmlist1);

                //}
                //else
                //{
                //    var cmlist2 = (from cm in _context.CoMorbidConditions
                //                   join ct in _context.CoMorbidTypes on cm.coMorbidId equals ct.id
                //                   where cm.dependantID == dependantID
                //                   where ct.id == result.CMConditionId
                //                   where cm.id == result.id
                //                   where cm.active == true
                //
                //                   select new CoMorbidConditionVM
                //                   {
                //                       id = cm.id,
                //                       CMConditionId = cm.coMorbidId,
                //                       CMTypeID = ct.id,
                //                       Condition = ct.condition,
                //                       icd10 = ct.icd10,
                //                       icd10Condition = ct.condition + " - " + ct.icd10,
                //                       createdDate = cm.createdDate,
                //                       effectiveDate = cm.effectiveDate,
                //                       endDate = cm.treatementEndDate,
                //                       programCode = cm.programType,
                //
                //                   }).FirstOrDefault();
                //
                //    results.Add(cmlist2);
                //
                //}
            }

            return results;
        }
        public List<CoMorbidConditionVM> GetNewComorbidInformation(Guid dependantID)
        {
            string sql = string.Format(@"SELECT cc.id, cc.dependantID, cc.coMorbidId [CMConditionId], cme.id[CMTypeID], cme.mappingDescription[Condition], cme.ICD10Code [icd10],cme.mappingDescription + ' - ' + cme.ICD10Code[icd10Condition], cc.effectiveDate[startDate], cc.treatementEndDate[endDate]
                                        FROM CoMormidConditions cc
                                        INNER JOIN ComorbidConditionExclusions cme ON cc.coMorbidId = cme.id
                                        WHERE cc.dependantID = '{0}'", dependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CoMorbidConditionVM>)db.Query<CoMorbidConditionVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }


        //public List<CoMorbidTypes> GetCoMorbidsICD10NotLinkedToDependant(string condition)
        //{
        //    string icd = _context.CoMorbidTypes.Where(x => x.condition == condition).Select(x => x.condition).FirstOrDefault();
        //    return _context.CoMorbidTypes.Where(x => x.icd10 == icd).OrderBy(x => x.icd10).ToList();
        //}




        public CoMorbidConditionVM GetCoMorbids_ExcludedByID(int id)
        {
            var results = (from cc in _context.CoMorbidConditions
                           join ct in _context.CoMorbidTypes on cc.coMorbidId equals ct.id
                           where cc.id == id
                           select new CoMorbidConditionVM
                           {
                               id = cc.id,
                               CMConditionId = ct.id,
                               dependantID = cc.dependantID,
                               icd10Condition = ct.condition + " - " + ct.icd10,
                               icd10 = ct.icd10,
                               Condition = ct.condition
                           }).FirstOrDefault();

            return results;
        }

        public CoMorbidConditionVM GetNewCoMorbids_ExcludedByID(int id)
        {
            var results = (from cc in _context.CoMorbidConditions
                           join ct in _context.ComorbidConditionExclusions on cc.coMorbidId equals ct.id
                           where cc.id == id
                           select new CoMorbidConditionVM
                           {
                               id = cc.id,
                               CMConditionId = ct.id,
                               dependantID = cc.dependantID,
                               icd10Condition = ct.mappingDescription + " - " + ct.ICD10Code,
                               icd10 = ct.ICD10Code,
                               Condition = ct.mappingDescription,
                               effectiveDate = cc.effectiveDate

                           }).FirstOrDefault();

            return results;
        }

        public CoMorbidConditionVM GetMHDiagnosisInformation(string code, string icd10)
        {
            var results = (from cme in _context.ComorbidConditionExclusions
                           where cme.mappingCode == code
                           where cme.ICD10Code == icd10

                           select new CoMorbidConditionVM
                           {
                               mappingCode = cme.mappingCode,
                               Condition = cme.mappingDescription,

                           }).FirstOrDefault();

            return results;
        }

        public CoMorbidConditionVM GetNewCoMorbids_ExcludedByConditionCode(string code)
        {
            var results = (from cc in _context.CoMorbidConditions
                           join ct in _context.ComorbidConditionExclusions on cc.coMorbidId equals ct.id
                           where ct.mappingCode == code

                           select new CoMorbidConditionVM
                           {
                               id = cc.id,
                               CMConditionId = ct.id,
                               dependantID = cc.dependantID,
                               icd10Condition = ct.mappingDescription + " - " + ct.ICD10Code,
                               icd10 = ct.ICD10Code,
                               Condition = ct.mappingDescription,
                               effectiveDate = cc.effectiveDate,
                               mappingCode = ct.mappingCode

                           }).FirstOrDefault();

            return results;
        }

        public CoMorbidTypes GetCoMorbidTypeByID(int id)
        {
            return _context.CoMorbidTypes.Where(x => x.id == id).Where(x => x.active == true).FirstOrDefault();
        }

        public ComorbidConditionExclusions GetCoMorbidExclusionsByID(int id)
        {
            return _context.ComorbidConditionExclusions.Where(x => x.id == id).Where(x => x.Active == true).FirstOrDefault();
        }

        public ComorbidConditionExclusions GetICD10Info(string id)
        {
            //var icd10id = Convert.ToInt32(id);
            return _context.ComorbidConditionExclusions.Where(x => x.ICD10Code == id).Where(x => x.Active == true).FirstOrDefault();
        }
        public ComorbidConditionExclusions GetCMByMappingCode(string mappingCode)
        {
            return _context.ComorbidConditionExclusions.Where(x => x.mappingCode == mappingCode).Where(x => x.Active == true).FirstOrDefault();
        }
        public List<CoMormidConditions> GetCoMorbid()
        {
            return _context.CoMorbidConditions.Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }

        public List<CoMorbidClinicalRulesView> GetCoMorbidRules(string ruletype)
        {
            var results = (from c in _context.CoMorbidTypeRules
                           join r in _context.CoMorbidClinicalRules
                           on c.ruleID equals r.id
                           where c.ruleType == ruletype
                           select new CoMorbidClinicalRulesView
                           {
                               ruleType = c.ruleType,
                               ruleID = c.ruleID,
                               ruleName = r.ruleName,
                               greater = r.greater,
                               less = r.less,
                               pathologyField = r.pathologyField,
                           }).ToList();

            return results;
        }

        public List<CoMorbidClinicalRules> GetCoMorbidRules()
        {
            return _context.CoMorbidClinicalRules.Where(x => x.active == true).ToList();
        }

        public List<RulesInstructions> GetRulesHistory(Guid dependantID) //hcare-1344
        {
            var results = (from r in _context.PatientClinicalRulesHistory
                           join c in _context.ClinicalRules
                           on r.ruleBroken equals c.ruleName
                           where r.dependentId == dependantID
                           where r.active == true
                           select new RulesInstructions
                           {
                               ruleName = c.ruleName,
                               RuleBroken = r.ruleBroken,
                               messages = _context.ClinicalMessageInstructions.Where(x => x.ruleMessage == r.ruleBroken).Where(x => x.active == true).ToList(),
                               effectiveDate = r.createdDate,
                               program = c.ruleType,
                               RuleInstructions = c.Instruction,

                           }).OrderByDescending(x => x.effectiveDate).ToList();

            return results;
        }

        public List<ClinicalRulesVM> GetClinicalRulesHistory(Guid dependantID) //HCare-1116 //hcare-1344
        {
            string sql = string.Format(@"SELECT r.ruleBroken[RuleBroken], ci.instruction[Message], r.createdDate[CreatedDate], c.ruleType[ProgramCode], r.pathFieldName, r.pathFieldValue, c.Instruction[Instruction], c.ruleName[RuleName], r.pathologyID[PathologyID]
                                            FROM PatientClinicalRulesHistory r
                                            INNER JOIN ClinicalRules c ON r.ruleBroken IN(c.gtMessage, c.ltMessage)
                                            AND c.id = ( SELECT MAX(id) FROM ClinicalRules WHERE gtMessage = r.ruleBroken OR ltMessage = r.ruleBroken)
                                            LEFT OUTER JOIN ClinicalMessageInstructions ci ON r.ruleBroken = ci.ruleMessage
											AND ci.id = ( SELECT MAX(id) FROM ClinicalMessageInstructions WHERE ruleMessage = r.ruleBroken)
                                            WHERE r.dependentID = '{0}'", dependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<ClinicalRulesVM>)db.Query<ClinicalRulesVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<ClinicalRulesVM> GetClinicalRulesByDependant(Guid dependantID, string pro)
        {
            string sql = string.Format(@"SELECT crh.dependentId[dependantID], cr.id[ClinicalRulesID], crh.pathologyID[PathologyID], cr.ruleName[RuleName], cr.greater[Greater], cr.gtMessage[GtMessage], cr.less[Less], cr.ltMessage[LtMessage], cr.pathologyField[PathologyField], cr.ruleType[ProgramCode]
                                        FROM PatientClinicalRulesHistory crh
                                        INNER JOIN ClinicalRules cr ON crh.ruleBroken IN(cr.gtMessage, cr.ltMessage)
                                        
                                        WHERE crh.dependentID = '{0}' AND cr.ruleType = '{1}'", dependantID, pro);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<ClinicalRulesVM>)db.Query<ClinicalRulesVM>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<Queries> GetActiveQueries()
        {
            return _context.Queries.Where(x => x.Active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<EnquiriesSearch> GetAllQueries() //HCare-680(Enquiry search enhanced)
        {
            var result = (from q in _context.Queries
                          join d in _context.Dependants
                          on q.dependentID equals d.DependantID
                          join m in _context.Members
                          on d.memberID equals m.memberID
                          join ma in _context.MedicalAids
                          on m.medicalAidID equals ma.MedicalAidID
                          join pr in _context.Priorities
                          on q.priority equals pr.prioritytype
                          join qt in _context.QueryTypes
                          on q.enquiryBy equals qt.code
                          where ma.Active == true
                          where q.Active == true
                          select new EnquiriesSearch()
                          {
                              medicalAidId = m.medicalAidID,
                              effectiveDate = q.effectiveDate,
                              queryCode = q.queryType,
                              enquiryBy = q.enquiryBy,
                              queryDescription = qt.queryDescription,
                              queryStatus = q.queryStatus,
                              queryPriority = pr.priorityName,
                              querySubject = q.querySubject,
                              query = q.query,
                              querySolution = q.querySolution,
                              createdBy = q.createdBy,
                              createdDate = q.createdDate,
                              modifiedBy = q.modifiedBy,
                              modifiedDate = q.modifiedDate,
                              resolvedBy = q.resolvedBy,
                              resolutionDate = q.resolutionDate,
                              comment = q.comment,
                              memberName = d.firstName + " " + d.lastName,
                              membershipNo = m.membershipNo,
                              dependantCode = d.dependentCode,
                              queryId = q.queryID,
                              dependantId = q.dependentID,
                              memberStatus = _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.statusName).FirstOrDefault(),
                              programID = _context.Program.Where(o => o.code == _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.programCode).FirstOrDefault()).Select(o => o.programID).FirstOrDefault(),
                              medicalAid = ma.Name,
                              queryReference = q.ReferenceNumber,
                              IdNumber = d.idNumber

                          }).OrderByDescending(x => x.effectiveDate).ToList();
            //HCare-619
            result = result.Where(x => x.memberStatus != "Deceased").Where(x => x.memberStatus != "Deactivated").Where(x => x.memberStatus != "Patient Resigned").Where(x => x.memberStatus != "PEP Complete").Where(x => x.memberStatus != "PreP Complete").ToList();

            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            result = (from r in result
                      where medaidlist.Contains(r.medicalAidId)
                      select r).ToList();

            var progs = rights.accessList.Select(x => x.programId).ToList();
            result = (from r in result
                      where progs.Contains(r.programID)
                      select r).ToList();

            return result;
        }

        public List<Queries> GetQueryStatus()
        {
            string sql = string.Format(@"SELECT DISTINCT q.queryStatus
                            FROM Queries q");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<Queries>)db.Query<Queries>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }
        }

        public List<EnquiriesSearch> GetActiveSQueries()
        {
            var results = (from q in _context.Queries
                           join d in _context.Dependants on q.dependentID equals d.DependantID
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join pr in _context.Priorities on q.priority equals pr.prioritytype into prInfo
                           from pr in prInfo.DefaultIfEmpty()
                           join qt in _context.QueryTypes on q.enquiryBy equals qt.code
                           where q.Active == true
                           where ma.Active == true //HCare-619
                           select new EnquiriesSearch()
                           {
                               medicalAid = ma.Name,
                               medicalAidId = ma.MedicalAidID,
                               medicalAidStatus = ma.Active,
                               effectiveDate = q.effectiveDate,
                               queryCode = _context.EnquiryTypes.Where(p => p.enquiryType == _context.Queries.Where(x => x.queryType == q.queryType).Select(x => x.queryType).FirstOrDefault()).Select(p => p.enquiryName).FirstOrDefault(),
                               enquiryBy = q.enquiryBy,
                               queryDescription = qt.queryDescription,
                               //queryDescription = _context.Queries.Where(p => p.queryType == _context.QueryTypes.Where(x => x.code == q.queryType).Select(x => x.queryDescription).FirstOrDefault()).Select(p => p.queryType).FirstOrDefault(),
                               querySubject = q.querySubject,
                               query = q.query,
                               querySolution = q.querySolution,
                               createdBy = q.createdBy,
                               createdDate = q.createdDate,
                               modifiedBy = q.modifiedBy,
                               modifiedDate = q.modifiedDate,
                               resolvedBy = q.resolvedBy,
                               resolutionDate = q.resolutionDate,
                               comment = q.comment,
                               memberName = d.firstName + " " + d.lastName,
                               membershipNo = m.membershipNo,
                               dependantCode = d.dependentCode,
                               queryId = q.queryID,
                               dependantId = q.dependentID,
                               queryStatus = q.queryStatus,
                               queryReference = q.ReferenceNumber,
                               queryPriority = pr.priorityName,
                               memberStatus = _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.statusName).FirstOrDefault(),
                               programID = _context.Program.Where(o => o.code == _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.programCode).FirstOrDefault()).Select(o => o.programID).FirstOrDefault(),
                           }).OrderByDescending(x => x.effectiveDate).ToList();
            //HCare-619
            results = results.Where(x => x.memberStatus != "Deceased").Where(x => x.memberStatus != "Deactivated").Where(x => x.memberStatus != "Patient Resigned").Where(x => x.memberStatus != "PEP Complete").Where(x => x.memberStatus != "PreP Complete").ToList();


            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            var filteredResponse = (from r in results
                                    where medaidlist.Contains(r.medicalAidId)
                                    select r).ToList();

            var freshResponse = new List<EnquiriesSearch>();

            foreach (var med in medaidlist)
            {
                var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                var filteredRes = (from r in filteredResponse
                                   where r.medicalAidId == med
                                   where progs.Contains(r.programID)
                                   select r).ToList();

                freshResponse.AddRange(filteredRes);
            }

            results = freshResponse.Distinct().ToList();

            return results;

        }

        public List<AdvancedSearchResultModel> GetFilteredAdSearch()
        {
            string sql = string.Format(@"SELECT d.DependantID AS dependantID, m.membershipNo as membershipNumber, d.dependentCode AS DependantCode, d.firstName As memberName,
		                                        d.lastName as memberSurname, d.idNumber, ma.medicalAidCode, p.programCode AS program, pp.planName as schemeOption,
		                                        ms.statusName as memberStatus, dr.practiceNo AS drBHFNumber, dr.drFirstName as drName, dr.drLastName AS drSurname,
		                                        drc.email AS drEmail, pa.labName as latestPathologyLab, c.cell, c.landLine AS tel, d.createdDate, s.effectiveDate as StatusEffectiveDate,
		                                        ms.statusCode
                                        FROM Member m 
                                        INNER JOIN Dependant d ON m.memberID = d.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        LEFT OUTER JOIN MemberInformation mi ON d.DependantID = mi.dependantID
                                        LEFT OUTER JOIN PatientDoctorHistory di ON d.DependantID = di.dependantId
                                        AND di.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientDoctorHistory WHERE dependantId = d.DependantID AND active = 1)
                                        LEFT OUTER JOIN Doctors dr on di.doctorId = dr.doctorID
                                        LEFT OUTER JOIN DoctorInformation dri ON di.doctorId = dri.doctorID
                                        LEFT OUTER JOIN Address dra ON dri.addressID = dra.addressID
                                        LEFT OUTER JOIN Contact drc ON dri.contactID = drc.ContactID
                                        LEFT OUTER JOIN Address a ON mi.addressID = a.addressID
                                        LEFT OUTER JOIN Contact c ON mi.contactID = c.ContactID
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE dependantId = d.DependantID AND active = 1)
                                        LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
                                        AND p.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE dependantId = d.DependantID AND active = 1)
                                        LEFT OUTER JOIN PatientPlanHistory pp ON d.DependantID = pp.dependantId
                                        AND pp.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE dependantId = d.DependantID AND active = 1)
                                        LEFT OUTER JOIN ManagementStatus ms ON s.managementStatusCode = ms.statusCode
                                        LEFT OUTER JOIN Pathology pa ON d.DependantID = pa.dependentID
                                        AND pa.effectiveDate = (SELECT MAX(po.effectiveDate) FROM Pathology po WHERE po.dependentID = d.DependantID AND po.active = 1)
                                        WHERE m.active = 1
                                        AND d.Active = 1
                                        ORDER BY d.createdDate DESC");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<AdvancedSearchResultModel>)db.Query<AdvancedSearchResultModel>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }

            //var results = (from d in _context.Dependants
            //               join m in _context.Members
            //               on d.memberID equals m.memberID
            //               join med in _context.MedicalAids
            //               on m.medicalAidID equals med.MedicalAidID
            //               join mi in _context.MemberInformation
            //               on d.DependantID equals mi.dependantID into mInfo
            //               from mi in mInfo.DefaultIfEmpty()
            //               join dr in _context.Doctors
            //               on mi.doctorID equals dr.doctorID into doc
            //               from dr in doc.DefaultIfEmpty()
            //               join dri in _context.DoctorInformation
            //               on dr.doctorID equals dri.doctorID into docinfo
            //               from dri in docinfo.DefaultIfEmpty()
            //               join a in _context.Address
            //               on mi.addressID equals a.addressID into add
            //               from a in add.DefaultIfEmpty()
            //               join c in _context.Contacts
            //               on mi.contactID equals c.ContactID into con
            //               from c in con.DefaultIfEmpty()
            //               where m.active == true
            //               select new AdvancedSearchResultModel
            //               {
            //                   dependantID = d.DependantID,
            //                   membershipNumber = m.membershipNo,
            //                   DependantCode = d.dependentCode,
            //                   memberName = d.firstName,
            //                   memberSurname = d.lastName,
            //                   idNumber = d.idNumber,
            //                   medicalAidCode = med.medicalAidCode,
            //                   program = _context.PatientProgramHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault(),
            //                   schemeOption = _context.PatientPlanHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.planName).FirstOrDefault(),
            //                   memberStatus = _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.statusName).FirstOrDefault(),
            //                   drBHFNumber = dr.practiceNo,
            //                   drName = dr.drFirstName,
            //                   drSurname = dr.drLastName,
            //                   drEmail = _context.Contacts.Where(x => x.ContactID == dri.contactID).Select(x => x.email).FirstOrDefault(),
            //                   latestPathologyLab = _context.Pathology.Where(x => x.dependentID == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.labName).FirstOrDefault(),
            //                   cell = c.cell,
            //                   tel = c.landLine,
            //                   createdDate = d.createdDate,
            //                   StatusEffectiveDate = _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.effectiveDate).FirstOrDefault(),
            //                   statusCode = _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault(),
            //               }).OrderByDescending(x => x.createdDate).ToList();
            //
            //return results;

        }

        public List<EnquiriesSearch> GetSearchQueriesResults(string querytype = "", string membershipNo = "", string medicalAid = "", string mStatus = "", string queryPriority = "", string queryStatus = "")
        {
            var result = (from q in _context.Queries
                          join d in _context.Dependants on q.dependentID equals d.DependantID
                          join m in _context.Members on d.memberID equals m.memberID
                          join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                          join pr in _context.Priorities on q.priority equals pr.prioritytype into prInfo
                          from pr in prInfo.DefaultIfEmpty()
                          join qt in _context.QueryTypes on q.enquiryBy equals qt.code
                          where q.Active == true
                          where ma.Active == true //HCare-619
                          select new EnquiriesSearch()
                          {
                              medicalAidId = ma.MedicalAidID,
                              effectiveDate = q.effectiveDate,
                              queryCode = _context.EnquiryTypes.Where(p => p.enquiryType == _context.Queries.Where(x => x.queryType == q.queryType).Select(x => x.queryType).FirstOrDefault()).Select(p => p.enquiryName).FirstOrDefault(),
                              queryDescription = qt.queryDescription,
                              queryPriority = pr.priorityName,
                              querySubject = q.querySubject,
                              query = q.query,
                              queryStatus = q.queryStatus,
                              querySolution = q.querySolution,
                              createdBy = q.createdBy,
                              createdDate = q.createdDate,
                              modifiedBy = q.modifiedBy,
                              modifiedDate = q.modifiedDate,
                              resolvedBy = q.resolvedBy,
                              resolutionDate = q.resolutionDate,
                              comment = q.comment,
                              memberName = d.firstName + " " + d.lastName,
                              membershipNo = m.membershipNo,
                              dependantCode = d.dependentCode,
                              enquiryBy = q.enquiryBy,
                              queryId = q.queryID,
                              dependantId = q.dependentID,
                              memberStatus = _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.statusName).FirstOrDefault(),
                              medicalAid = ma.Name, //HCare-680(Enquiry search enhanced)
                              queryReference = q.ReferenceNumber,
                              IdNumber = d.idNumber,
                              programID = _context.Program.Where(o => o.code == _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.programCode).FirstOrDefault()).Select(o => o.programID).FirstOrDefault()
                          }).OrderByDescending(x => x.effectiveDate).ToList();


            if (!String.IsNullOrEmpty(querytype))
            {
                result = result.Where(x => x.queryDescription == querytype).ToList();
            }

            if (!String.IsNullOrEmpty(membershipNo))
            {
                result = result.Where(x => x.membershipNo.ToLower() == membershipNo.ToLower()).ToList();
            }

            if (!String.IsNullOrEmpty(medicalAid)) //HCare-680(Enquiry search enhanced)
            {
                result = result.Where(x => x.medicalAid == medicalAid).ToList();
            }

            if (!String.IsNullOrEmpty(mStatus)) //HCare-680(Enquiry search enhanced)
            {
                result = result.Where(x => x.memberStatus == mStatus).ToList();
            }

            if (!String.IsNullOrEmpty(queryPriority)) //HCare-680(Enquiry search enhanced)
            {
                result = result.Where(x => x.queryPriority == queryPriority).ToList();
            }

            if (!String.IsNullOrEmpty(queryStatus))
            {
                if (queryStatus == "1") //HCare-680(Enquiry search enhanced)
                {
                    result = result.Where(x => x.queryStatus == true).OrderBy(x => x.effectiveDate).ToList();
                }
                else if (queryStatus == "0") //HCare-680(Enquiry search enhanced)
                {
                    result = result.Where(x => x.queryStatus == false).OrderBy(x => x.effectiveDate).ToList();
                }
            }
            //HCare-619
            result = result.Where(x => x.memberStatus != "Deceased").Where(x => x.memberStatus != "Deactivated").Where(x => x.memberStatus != "Patient Resigned").Where(x => x.memberStatus != "PEP Complete").Where(x => x.memberStatus != "PreP Complete").ToList();

            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            var filteredResponse = (from r in result
                                    where medaidlist.Contains(r.medicalAidId)
                                    select r).ToList();

            var freshResponse = new List<EnquiriesSearch>();

            foreach (var med in medaidlist)
            {
                var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                var filteredRes = (from r in filteredResponse
                                   where r.medicalAidId == med
                                   where progs.Contains(r.programID)
                                   select r).ToList();

                freshResponse.AddRange(filteredRes);
            }

            result = freshResponse.Distinct().ToList();

            return result;
        }

        public List<ProductionSearchView> GetProductionSearchResults(string medicalaid = "", string users = "", string fromDate = "", string toDate = "", string filter = "")
        {
            //string[] MedicalAids = medicalaid.Split(',');

            DateTime dtFrom = Convert.ToDateTime(fromDate);
            DateTime dtTo = Convert.ToDateTime(toDate);

            var results = (from it in _context.AssignmentItemTasks
                           join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id
                           join a in _context.Assignments on ai.assignmentId equals a.assignmentID
                           join d in _context.Dependants on a.dependentID equals d.DependantID
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join aty in _context.AssignmentTypes on a.assignmentType equals aty.assignmentType
                           join ait in _context.AssignmentItemTypes on ai.itemType equals ait.assignmentItemType
                           join att in _context.AssignmentItemTaskTypes on it.taskTypeId equals att.taskId
                           join ttr in _context.TaskTypeRequirements on it.requirementId equals ttr.id
                           //where MedicalAids.Contains(ma.Name)
                           //where Users.Contains(it.closedBy)
                           //where Filter.Contains(att.taskDescription)
                           where dtFrom <= it.closedDate && dtTo >= it.closedDate

                           select new ProductionSearchView()
                           {
                               medicalAid = ma.Name,
                               taskDescription = att.taskDescription,
                               taskCreatedDate = it.createdDate,
                               taskEffectiveDate = it.effectiveDate,
                               taskClosedDate = it.closedDate,
                               closedBy = it.closedBy.ToLower(),
                               assignmentType = aty.assignmentDescription,
                               itemType = ait.itemDescription,
                               assignmentID = ai.assignmentId,
                               itemID = ai.id,
                               memberName = d.firstName + " " + d.lastName,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               status = it.closed,

                           }).ToList();

            if (!String.IsNullOrEmpty(medicalaid))
            {
                string[] MedicalAids = medicalaid.Split(',');

                results = (from r in results
                           where MedicalAids.Contains(r.medicalAid)
                           select r).ToList();
            }

            if (!String.IsNullOrEmpty(users))
            {
                string[] Users = users.Split(',');

                results = (from r in results
                           where Users.Contains(r.closedBy)
                           select r).ToList();
            }

            if (!String.IsNullOrEmpty(filter))
            {
                string[] Filter = filter.Split(',');

                results = (from r in results
                           where Filter.Contains(r.taskDescription)
                           select r).ToList();
            }


            return results;

        }

        public List<PathologySearch> GetActivePathologyResults()
        {
            DateTime dtFrom = DateTime.Now.AddYears(-1);
            DateTime dtTo = DateTime.Now;

            var results = (from d in _context.Dependants
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join p in _context.Pathology on d.DependantID equals p.dependentID into pInfo
                           from p in pInfo.DefaultIfEmpty()
                           where dtFrom <= p.effectiveDate && dtTo >= p.effectiveDate

                           select new PathologySearch()
                           {
                               DependantID = d.DependantID,
                               MedicalAidScheme = ma.Name,
                               MembershipNumber = m.membershipNo,
                               DependantCode = d.dependentCode,
                               EffectiveDate = p.effectiveDate,
                               CreatedBy = p.createdBy,
                               CreatedDate = p.createdDate,
                               PathologyLab = p.labName,
                               FirstName = d.firstName,
                               LastName = d.lastName,
                               CD4count = p.CD4Count,
                               CD4percentage = p.CD4Percentage,
                               Viralload = p.viralLoad,
                               Haemoglobin = p.haemoglobin,
                               Bilirubin = p.bilirubin,
                               TotalCholestrol = p.totalCholestrol,
                               HDL = p.hdl,
                               LDL = p.ldl,
                               Triglycerides = p.triglycerides,
                               Glucose = p.glucose,
                               HbA1c = p.hba1c,
                               ALT = p.alt,
                               AST = p.ast,
                               Urea = p.urea,
                               Creatinine = p.creatinine,
                               eGFR = p.eGfr,
                               UAlbumintoCreatratio = p.mauCreatRatio,
                               systolicBP = p.systolicBP,
                               diastolicBP = p.diastolicBP,
                               FEV1 = p.FEV1,
                               Eosinophylia = p.Eosinophylia,
                               HIVElisa = p.hivEliza,
                               NormalGTT = p.normGtt,
                               AbnormalGTT = p.abnGtt,
                               UCreatinine = p.ucreatinine,
                               SAlbumin = p.salbumin,
                               UAlbuminuria = p.ualbuminuria,


                           }).OrderByDescending(x => x.EffectiveDate).ToList();

            return results;

        }

        //public List<PathologySearch> GetPathologySearchResults(string medicalaid = "", string program = "", string pathologyfield = "", string fromDate = "", string toDate = "")
        //{
        //    DateTime dtFrom = Convert.ToDateTime(fromDate);
        //    DateTime dtTo = Convert.ToDateTime(toDate);

        //    var results = (from d in _context.Dependants
        //                   join m in _context.Members on d.memberID equals m.memberID
        //                   join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
        //                   join p in _context.Pathology on d.DependantID equals p.dependentID into pInfo
        //                   from p in pInfo.DefaultIfEmpty()
        //                       //where dtFrom <= p.effectiveDate && dtTo >= p.effectiveDate
        //                   where (dtFrom <= p.CD4CounteffectiveDate && dtTo >= p.CD4CounteffectiveDate) ||
        //                   (dtFrom <= p.CD4PercentageeffectiveDate && dtTo >= p.CD4PercentageeffectiveDate) ||
        //                   (dtFrom <= p.viralLoadeffectiveDate && dtTo >= p.viralLoadeffectiveDate)

        //                   select new PathologySearch()
        //                   {
        //                       DependantID = d.DependantID,
        //                       PathologyID = p.pathologyID,
        //                       MedicalAidScheme = ma.Name,
        //                       MembershipNumber = m.membershipNo,
        //                       DependantCode = d.dependentCode,
        //                       EffectiveDate = p.effectiveDate,
        //                       CreatedBy = p.createdBy,
        //                       CreatedDate = p.createdDate,
        //                       PathologyLab = p.labName,
        //                       FirstName = d.firstName,
        //                       LastName = d.lastName,
        //                       ProgramName = p.pathologyType,
        //                       CD4Count = p.CD4Count,
        //                       CD4CountEffectiveDate = p.CD4CounteffectiveDate,
        //                       CD4Percentage = p.CD4Percentage,
        //                       CD4PercentageEffectiveDate = p.CD4PercentageeffectiveDate,
        //                       viralLoad = p.viralLoad,
        //                       ViralLoadEffectiveDate = p.viralLoadeffectiveDate,
        //                       haemoglobin = p.haemoglobin,
        //                       bilirubin = p.bilirubin,
        //                       totalCholestrol = p.totalCholestrol,
        //                       hdl = p.hdl,
        //                       ldl = p.ldl,
        //                       triglycerides = p.triglycerides,
        //                       glucose = p.glucose,
        //                       hba1c = p.hba1c,
        //                       alt = p.alt,
        //                       ast = p.ast,
        //                       urea = p.urea,
        //                       creatinine = p.creatinine,
        //                       eGfr = p.eGfr,
        //                       mauCreatRatio = p.mauCreatRatio,
        //                       systolicBP = p.systolicBP,
        //                       diastolicBP = p.diastolicBP,
        //                       FEV1 = p.FEV1,
        //                       Eosinophylia = p.Eosinophylia,
        //                       hivEliza = p.hivEliza,
        //                       normGtt = p.normGtt,
        //                       abnGtt = p.abnGtt,
        //                       ucreatinine = p.ucreatinine,
        //                       salbumin = p.salbumin,
        //                       ualbuminuria = p.ualbuminuria,

        //                   }).OrderByDescending(p => p.EffectiveDate).ToList();

        //    if (!String.IsNullOrEmpty(medicalaid))
        //    {
        //        results = (from r in results
        //                   where medicalaid.Contains(r.MedicalAidScheme)
        //                   select r).ToList();
        //    }
        //    if (!String.IsNullOrEmpty(program))
        //    {
        //        results = (from r in results
        //                   where program.Contains(r.ProgramName)
        //                   select r).ToList();
        //    }
        //    if (!String.IsNullOrEmpty(pathologyfield))
        //    {
        //        string[] pathField = pathologyfield.Split(',');
        //        var pf = pathField.ToList();
        //        var toKeep = new List<PathologySearch>();
        //        var toRemove = new List<PathologySearch>();
        //        foreach (var r in results)
        //        {
        //            //set remove = true, assume they are all empty
        //            var remove = true;
        //            foreach (var item in pf)
        //            {
        //                //If anyone is found not to be empty, then don't remove it
        //                if (r.GetType().GetProperty(item).GetValue(r) != null)
        //                {
        //                    remove = false;
        //                    break;
        //                }
        //            }

        //            if (remove) { toRemove.Add(r); }
        //            else { toKeep.Add(r); }
        //        }
        //        results = toKeep;
        //    }

        //    return results;
        //}

        public List<PathologySearch> GetPathologySearchResults(string medicalaid = "", string program = "", string pathologyfield = "", string fromDate = "", string toDate = "")
        {
            DateTime dtFrom = Convert.ToDateTime(fromDate);
            DateTime dtTo = Convert.ToDateTime(toDate);
            var fromdate_string = Convert.ToDateTime(fromDate).ToString("dd MMM yyyy"); //hcare-1396
            var todate_string = Convert.ToDateTime(toDate).ToString("dd MMM yyyy"); //hcare-1396

            string sql = string.Format(@" SELECT ma.Name[MedicalAidScheme], ma.MedicalAidID, m.membershipNo[MembershipNumber], d.dependentCode[DependantCode], d.idNumber[IDNumber], d.firstName[FirstName], d.lastName[LastName], p.effectiveDate[EffectiveDate], p.labName[PathologyLab],
                            p.createdBy[CreatedBy], p.createdDate[CreatedDate], p.pathologyType[ProgramName], p.CD4Count[CD4count], p.CD4CounteffectiveDate[CD4countEffectiveDate], p.CD4Percentage[CD4percentage], p.CD4PercentageeffectiveDate[CD4percentageEffectiveDate],
                            p.viralLoad[Viralload], p.viralLoadeffectiveDate[ViralloadEffectiveDate], p.haemoglobin[Haemoglobin], p.haemoglobineffectiveDate[HaemoglobinEffectiveDate], p.totalCholestrol[TotalCholestrol], p.totalCholestroleffectiveDate[TotalCholestrolEffectiveDate], 
                            p.hdl[HDL], p.hdleffectiveDate[HDLEffectiveDate], p.ldl[LDL], p.ldleffectiveDate[LDLEffectiveDate], p.triglycerides[Triglycerides], p.triglycerideseffectiveDate[TriglyceridesEffectiveDate], p.glucose[Glucose], p.glucoseeffectiveDate[GlucoseEffectiveDate], 
                            p.hba1c[HbA1c], p.hba1ceffectiveDate[HbA1cEffectiveDate], p.alt[ALT], p.alteffectiveDate[ALTEffectiveDate], p.ast[AST], p.asteffectiveDate[ASTEffectiveDate], p.eGfr[eGFR], p.eGfreffectiveDate[eGFREffectiveDate], p.bilirubin[Bilirubin], 
                            p.bilirubineffectiveDate[BilirubinEffectiveDate], p.mauCreatRatio[UAlbumintoCreatratio], p.mauCreatRatioeffectiveDate[UAlbumintoCreatratioEffectiveDate], p.systolicBP[systolicBP],p.BPeffectiveDate[systolicBPEffectiveDate], p.diastolicBP[diastolicBP], p.BPeffectiveDate[diastolicBPEffectiveDate], 
                            p.FEV1, p.FEV1effectiveDate, p.Eosinophylia, p.EosinophyliaeffectiveDate, p.hivEliza[HIVElisa], p.hivElizaeffectiveDate[HIVElisaEffectiveDate], p.normGtt[NormalGTT], p.normGtteffectiveDate[NormalGTTEffectiveDate], p.abnGtt[AbnormalGTT], p.abnGtteffectiveDate[AbnormalGTTEffectiveDate], 
                            p.ucreatinine[UCreatinine], p.ucreatinineeffectiveDate[UCreatinineEffectiveDate], p.salbumin[SAlbumin], p.salbumineffectiveDate[SAlbuminEffectiveDate], p.ualbuminuria[UAlbuminuria], p.ualbuminuriaeffectiveDate[UAlbuminuriaEffectiveDate], 
                            p.urea[Urea], p.ureaeffectiveDate[UreaEffectiveDate], p.creatinine[Creatinine], p.creatinineeffectiveDate[CreatinineEffectiveDate]
                            
                            FROM Dependant d
                            INNER JOIN Member m on m.memberID = d.memberID
                            INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                            LEFT OUTER JOIN Pathology p on d.DependantID = p.dependentID
                            WHERE m.active = 1
                            AND ma.Active = 1
                            AND p.Active = 1
                            AND (
                                CD4CounteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR CD4PercentageeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR viralLoadeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR haemoglobineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR bilirubineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR totalCholestroleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hdleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ldleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR triglycerideseffectiveDate BETWEEN '{0}' AND '{1}'
                                OR glucoseeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hba1ceffectiveDate BETWEEN '{0}' AND '{1}'
                                OR alteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR asteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ureaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR creatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR eGfreffectiveDate BETWEEN '{0}' AND '{1}'
                                OR mauCreatRatioeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR BPeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR FEV1effectiveDate BETWEEN '{0}' AND '{1}'
                                OR EosinophyliaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hivElizaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR normGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR abnGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ucreatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR salbumineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ualbuminuriaeffectiveDate BETWEEN '{0}' AND '{1}'
                            )", fromdate_string, todate_string);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<PathologySearch>)db.Query<PathologySearch>(sql, null, commandTimeout: 500);
                db.Close();

                if (!String.IsNullOrEmpty(medicalaid))
                {
                    results = (from r in results
                               where medicalaid.Contains(r.MedicalAidID.ToString())
                               select r).ToList();
                }
                else
                {
                    var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                    var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                    results = (from r in results
                               where medaidlist.Contains(r.MedicalAidID)
                               select r).ToList();
                    return results;
                }
                if (!String.IsNullOrEmpty(program))
                {
                    results = (from r in results
                               where program.Contains(r.ProgramName)
                               select r).ToList();
                }
                //if (!String.IsNullOrEmpty(program))
                //{
                //    var allowedprograms = GetAllowedPrograms();

                //    string[] programs = program.Replace(" ", "").Split(',');
                //    var p = programs.ToList();
                //    var toKeep = new List<PathologySearch>();
                //    var toRemove = new List<PathologySearch>();
                //    foreach (var r in results)
                //    {
                //        //set remove = true, assume they are all empty
                //        var remove = true;
                //        foreach (var item in p)
                //        {
                //            //If anyone is found not to be empty, then don't remove it
                //            if (item.pathol)
                //            {
                //                remove = false;
                //                break;
                //            }
                //        }

                //        if (remove) { toRemove.Add(r); }
                //        else { toKeep.Add(r); }
                //    }
                //    results = toKeep;
                //}

                if (!String.IsNullOrEmpty(pathologyfield))
                {
                    string[] pathField = pathologyfield.Replace(" ", "").Split(',');
                    var pf = pathField.ToList();
                    var toKeep = new List<PathologySearch>();
                    var toRemove = new List<PathologySearch>();
                    foreach (var r in results)
                    {
                        //set remove = true, assume they are all empty
                        var remove = true;
                        foreach (var item in pf)
                        {
                            //If anyone is found not to be empty, then don't remove it
                            if (r.GetType().GetProperty(item).GetValue(r) != null)
                            {
                                remove = false;
                                break;
                            }
                        }

                        if (remove) { toRemove.Add(r); }
                        else { toKeep.Add(r); }
                    }
                    results = toKeep;
                }

                foreach (var item in results)
                {
                    if (dtFrom <= item.CD4countEffectiveDate && dtTo >= item.CD4countEffectiveDate) { }
                    else { item.CD4count = null; }
                    if (dtFrom <= item.CD4percentageEffectiveDate && dtTo >= item.CD4percentageEffectiveDate) { }
                    else { item.CD4percentage = null; }
                    if (dtFrom <= item.ViralloadEffectiveDate && dtTo >= item.ViralloadEffectiveDate) { }
                    else { item.Viralload = null; }
                    if (dtFrom <= item.HaemoglobinEffectiveDate && dtTo >= item.HaemoglobinEffectiveDate) { }
                    else { item.Haemoglobin = null; }
                    if (dtFrom <= item.BilirubinEffectiveDate && dtTo >= item.BilirubinEffectiveDate) { }
                    else { item.Bilirubin = null; }
                    if (dtFrom <= item.TotalCholestrolEffectiveDate && dtTo >= item.TotalCholestrolEffectiveDate) { }
                    else { item.TotalCholestrol = null; }
                    if (dtFrom <= item.HDLEffectiveDate && dtTo >= item.HDLEffectiveDate) { }
                    else { item.HDL = null; }
                    if (dtFrom <= item.LDLEffectiveDate && dtTo >= item.LDLEffectiveDate) { }
                    else { item.LDL = null; }
                    if (dtFrom <= item.TriglyceridesEffectiveDate && dtTo >= item.TriglyceridesEffectiveDate) { }
                    else { item.Triglycerides = null; }
                    if (dtFrom <= item.GlucoseEffectiveDate && dtTo >= item.GlucoseEffectiveDate) { }
                    else { item.Glucose = null; }
                    if (dtFrom <= item.HbA1cEffectiveDate && dtTo >= item.HbA1cEffectiveDate) { }
                    else { item.HbA1c = null; }
                    if (dtFrom <= item.ALTEffectiveDate && dtTo >= item.ALTEffectiveDate) { }
                    else { item.ALT = null; }
                    if (dtFrom <= item.ASTEffectiveDate && dtTo >= item.ASTEffectiveDate) { }
                    else { item.AST = null; }
                    if (dtFrom <= item.UreaEffectiveDate && dtTo >= item.UreaEffectiveDate) { }
                    else { item.Urea = null; }
                    if (dtFrom <= item.CreatinineEffectiveDate && dtTo >= item.CreatinineEffectiveDate) { }
                    else { item.Creatinine = null; }
                    if (dtFrom <= item.eGFREffectiveDate && dtTo >= item.eGFREffectiveDate) { }
                    else { item.eGFR = null; }
                    if (dtFrom <= item.UAlbumintoCreatratioEffectiveDate && dtTo >= item.UAlbumintoCreatratioEffectiveDate) { }
                    else { item.UAlbumintoCreatratio = null; }
                    if (dtFrom <= item.systolicBPEffectiveDate && dtTo >= item.systolicBPEffectiveDate) { }
                    else { item.systolicBP = null; }
                    if (dtFrom <= item.diastolicBPEffectiveDate && dtTo >= item.diastolicBPEffectiveDate) { }
                    else { item.diastolicBP = null; }
                    if (dtFrom <= item.FEV1EffectiveDate && dtTo >= item.FEV1EffectiveDate) { }
                    else { item.FEV1 = null; }
                    if (dtFrom <= item.EosinophyliaEffectiveDate && dtTo >= item.EosinophyliaEffectiveDate) { }
                    else { item.Eosinophylia = null; }
                    if (dtFrom <= item.HIVElisaEffectiveDate && dtTo >= item.HIVElisaEffectiveDate) { }
                    else { item.HIVElisa = null; }
                    if (dtFrom <= item.NormalGTTEffectiveDate && dtTo >= item.NormalGTTEffectiveDate) { }
                    else { item.NormalGTT = null; }
                    if (dtFrom <= item.AbnormalGTTEffectiveDate && dtTo >= item.AbnormalGTTEffectiveDate) { }
                    else { item.AbnormalGTT = null; }
                    if (dtFrom <= item.UCreatinineEffectiveDate && dtTo >= item.UCreatinineEffectiveDate) { }
                    else { item.UCreatinine = null; }
                    if (dtFrom <= item.SAlbuminEffectiveDate && dtTo >= item.SAlbuminEffectiveDate) { }
                    else { item.SAlbumin = null; }
                    if (dtFrom <= item.UAlbuminuriaEffectiveDate && dtTo >= item.UAlbuminuriaEffectiveDate) { }
                    else { item.UAlbuminuria = null; }
                }

                return results.OrderByDescending(x => x.EffectiveDate).OrderBy(x => x.DependantID).ThenBy(x => x.DependantCode).ToList();
            }
        }
        public List<PathologySearch> GetDependantPathologySearchResults(Guid DependantID, string pathologyfield = "", string fromDate = "", string toDate = "")
        {
            DateTime dtFrom = Convert.ToDateTime(fromDate);
            DateTime dtTo = Convert.ToDateTime(toDate);

            string sql = string.Format(@" SELECT ma.Name[MedicalAidScheme],m.membershipNo[MembershipNumber],d.DependantID[DependantID], d.dependentCode[DependantCode], d.idNumber[IDNumber], d.firstName[FirstName], d.lastName[LastName], p.pathologyID[PathologyID], p.effectiveDate[EffectiveDate], p.labName[PathologyLab],
                            p.createdBy[CreatedBy], p.createdDate[CreatedDate], p.pathologyType[ProgramName], p.CD4Count[CD4count], p.CD4CounteffectiveDate[CD4countEffectiveDate], p.CD4Percentage[CD4percentage], p.CD4PercentageeffectiveDate[CD4percentageEffectiveDate],
                            p.viralLoad[Viralload], p.viralLoadeffectiveDate[ViralloadEffectiveDate], p.haemoglobin[Haemoglobin], p.haemoglobineffectiveDate[HaemoglobinEffectiveDate], p.totalCholestrol[TotalCholestrol], p.totalCholestroleffectiveDate[TotalCholestrolEffectiveDate], 
                            p.hdl[HDL], p.hdleffectiveDate[HDLEffectiveDate], p.ldl[LDL], p.ldleffectiveDate[LDLEffectiveDate], p.triglycerides[Triglycerides], p.triglycerideseffectiveDate[TriglyceridesEffectiveDate], p.glucose[Glucose], p.glucoseeffectiveDate[GlucoseEffectiveDate], 
                            p.hba1c[HbA1c], p.hba1ceffectiveDate[HbA1cEffectiveDate], p.alt[ALT], p.alteffectiveDate[ALTEffectiveDate], p.ast[AST], p.asteffectiveDate[ASTEffectiveDate], p.eGfr[eGFR], p.eGfreffectiveDate[eGFREffectiveDate], p.bilirubin[Bilirubin], 
                            p.bilirubineffectiveDate[BilirubinEffectiveDate], p.mauCreatRatio[UAlbumintoCreatratio], p.mauCreatRatioeffectiveDate[UAlbumintoCreatratioEffectiveDate], p.systolicBP[systolicBP],p.BPeffectiveDate[systolicBPEffectiveDate], p.diastolicBP[diastolicBP], p.BPeffectiveDate[diastolicBPEffectiveDate], 
                            p.FEV1, p.FEV1effectiveDate, p.Eosinophylia, p.EosinophyliaeffectiveDate, p.hivEliza[HIVElisa], p.hivElizaeffectiveDate[HIVElisaEffectiveDate], p.normGtt[NormalGTT], p.normGtteffectiveDate[NormalGTTEffectiveDate], p.abnGtt[AbnormalGTT], p.abnGtteffectiveDate[AbnormalGTTEffectiveDate], 
                            p.ucreatinine[UCreatinine], p.ucreatinineeffectiveDate[UCreatinineEffectiveDate], p.salbumin[SAlbumin], p.salbumineffectiveDate[SAlbuminEffectiveDate], p.ualbuminuria[UAlbuminuria], p.ualbuminuriaeffectiveDate[UAlbuminuriaEffectiveDate], 
                            p.urea[Urea], p.ureaeffectiveDate[UreaEffectiveDate], p.creatinine[Creatinine], p.creatinineeffectiveDate[CreatinineEffectiveDate]
                            
                            FROM Dependant d
                            INNER JOIN Member m on m.memberID = d.memberID
                            INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                            LEFT OUTER JOIN Pathology p on d.DependantID = p.dependentID
                            WHERE d.DependantID = '{2}'
                            AND m.active = 1
                            AND ma.Active = 1
                            AND p.Active = 1

                            AND (
                                CD4CounteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR CD4PercentageeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR viralLoadeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR haemoglobineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR bilirubineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR totalCholestroleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hdleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ldleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR triglycerideseffectiveDate BETWEEN '{0}' AND '{1}'
                                OR glucoseeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hba1ceffectiveDate BETWEEN '{0}' AND '{1}'
                                OR alteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR asteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ureaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR creatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR eGfreffectiveDate BETWEEN '{0}' AND '{1}'
                                OR mauCreatRatioeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR BPeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR FEV1effectiveDate BETWEEN '{0}' AND '{1}'
                                OR EosinophyliaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hivElizaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR normGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR abnGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ucreatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR salbumineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ualbuminuriaeffectiveDate BETWEEN '{0}' AND '{1}'
                            )", dtFrom, dtTo, DependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = db.Query<PathologySearch>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                if (!String.IsNullOrEmpty(pathologyfield))
                {
                    string[] pathField = pathologyfield.Replace(" ", "").Split(',');
                    var pf = pathField.ToList();
                    var toKeep = new List<PathologySearch>();
                    var toRemove = new List<PathologySearch>();
                    foreach (var r in results)
                    {
                        //set remove = true, assume they are all empty
                        var remove = true;
                        foreach (var item in pf)
                        {
                            //If anyone is found not to be empty, then don't remove it
                            if (r.GetType().GetProperty(item).GetValue(r) != null)
                            {
                                remove = false;
                                break;
                            }
                        }

                        if (remove) { toRemove.Add(r); }
                        else { toKeep.Add(r); }
                    }
                    results = toKeep;
                }

                foreach (var item in results)
                {
                    if (dtFrom <= item.CD4countEffectiveDate && dtTo >= item.CD4countEffectiveDate) { }
                    else { item.CD4count = null; }
                    if (dtFrom <= item.CD4percentageEffectiveDate && dtTo >= item.CD4percentageEffectiveDate) { }
                    else { item.CD4percentage = null; }
                    if (dtFrom <= item.ViralloadEffectiveDate && dtTo >= item.ViralloadEffectiveDate) { }
                    else { item.Viralload = null; }
                    if (dtFrom <= item.HaemoglobinEffectiveDate && dtTo >= item.HaemoglobinEffectiveDate) { }
                    else { item.Haemoglobin = null; }
                    if (dtFrom <= item.BilirubinEffectiveDate && dtTo >= item.BilirubinEffectiveDate) { }
                    else { item.Bilirubin = null; }
                    if (dtFrom <= item.TotalCholestrolEffectiveDate && dtTo >= item.TotalCholestrolEffectiveDate) { }
                    else { item.TotalCholestrol = null; }
                    if (dtFrom <= item.HDLEffectiveDate && dtTo >= item.HDLEffectiveDate) { }
                    else { item.HDL = null; }
                    if (dtFrom <= item.LDLEffectiveDate && dtTo >= item.LDLEffectiveDate) { }
                    else { item.LDL = null; }
                    if (dtFrom <= item.TriglyceridesEffectiveDate && dtTo >= item.TriglyceridesEffectiveDate) { }
                    else { item.Triglycerides = null; }
                    if (dtFrom <= item.GlucoseEffectiveDate && dtTo >= item.GlucoseEffectiveDate) { }
                    else { item.Glucose = null; }
                    if (dtFrom <= item.HbA1cEffectiveDate && dtTo >= item.HbA1cEffectiveDate) { }
                    else { item.HbA1c = null; }
                    if (dtFrom <= item.ALTEffectiveDate && dtTo >= item.ALTEffectiveDate) { }
                    else { item.ALT = null; }
                    if (dtFrom <= item.ASTEffectiveDate && dtTo >= item.ASTEffectiveDate) { }
                    else { item.AST = null; }
                    if (dtFrom <= item.UreaEffectiveDate && dtTo >= item.UreaEffectiveDate) { }
                    else { item.Urea = null; }
                    if (dtFrom <= item.CreatinineEffectiveDate && dtTo >= item.CreatinineEffectiveDate) { }
                    else { item.Creatinine = null; }
                    if (dtFrom <= item.eGFREffectiveDate && dtTo >= item.eGFREffectiveDate) { }
                    else { item.eGFR = null; }
                    if (dtFrom <= item.UAlbumintoCreatratioEffectiveDate && dtTo >= item.UAlbumintoCreatratioEffectiveDate) { }
                    else { item.UAlbumintoCreatratio = null; }
                    if (dtFrom <= item.systolicBPEffectiveDate && dtTo >= item.systolicBPEffectiveDate) { }
                    else { item.systolicBP = null; }
                    if (dtFrom <= item.diastolicBPEffectiveDate && dtTo >= item.diastolicBPEffectiveDate) { }
                    else { item.diastolicBP = null; }
                    if (dtFrom <= item.FEV1EffectiveDate && dtTo >= item.FEV1EffectiveDate) { }
                    else { item.FEV1 = null; }
                    if (dtFrom <= item.EosinophyliaEffectiveDate && dtTo >= item.EosinophyliaEffectiveDate) { }
                    else { item.Eosinophylia = null; }
                    if (dtFrom <= item.HIVElisaEffectiveDate && dtTo >= item.HIVElisaEffectiveDate) { }
                    else { item.HIVElisa = null; }
                    if (dtFrom <= item.NormalGTTEffectiveDate && dtTo >= item.NormalGTTEffectiveDate) { }
                    else { item.NormalGTT = null; }
                    if (dtFrom <= item.AbnormalGTTEffectiveDate && dtTo >= item.AbnormalGTTEffectiveDate) { }
                    else { item.AbnormalGTT = null; }
                    if (dtFrom <= item.UCreatinineEffectiveDate && dtTo >= item.UCreatinineEffectiveDate) { }
                    else { item.UCreatinine = null; }
                    if (dtFrom <= item.SAlbuminEffectiveDate && dtTo >= item.SAlbuminEffectiveDate) { }
                    else { item.SAlbumin = null; }
                    if (dtFrom <= item.UAlbuminuriaEffectiveDate && dtTo >= item.UAlbuminuriaEffectiveDate) { }
                    else { item.UAlbuminuria = null; }
                }

                return results.OrderByDescending(x => x.EffectiveDate).OrderBy(x => x.DependantID).ThenBy(x => x.DependantCode).ToList();
            }
        }

        public List<PathologySortVM> GetSortedPathologySearchResults(Guid DependantID, string pathologyfield = "", string fromDate = "", string toDate = "") //HCare-974
        {
            DateTime dtFrom = Convert.ToDateTime(fromDate);
            DateTime dtTo = Convert.ToDateTime(toDate);

            string sql = string.Format(@" SELECT ma.Name[MedicalAidScheme],m.membershipNo[MembershipNumber],d.DependantID[DependantID], d.dependentCode[DependantCode], d.idNumber[IDNumber], d.firstName[FirstName], d.lastName[LastName], p.pathologyID[PathologyID], p.effectiveDate[EffectiveDate], p.labName[PathologyLab],
                            p.createdBy[CreatedBy], p.createdDate[CreatedDate], p.pathologyType[ProgramName], p.CD4Count[CD4count], p.CD4CounteffectiveDate[CD4countEffectiveDate], p.CD4Percentage[CD4percentage], p.CD4PercentageeffectiveDate[CD4percentageEffectiveDate],
                            p.viralLoad[Viralload], p.viralLoadeffectiveDate[ViralloadEffectiveDate], p.haemoglobin[Haemoglobin], p.haemoglobineffectiveDate[HaemoglobinEffectiveDate], p.totalCholestrol[TotalCholestrol], p.totalCholestroleffectiveDate[TotalCholestrolEffectiveDate], 
                            p.hdl[HDL], p.hdleffectiveDate[HDLEffectiveDate], p.ldl[LDL], p.ldleffectiveDate[LDLEffectiveDate], p.triglycerides[Triglycerides], p.triglycerideseffectiveDate[TriglyceridesEffectiveDate], p.glucose[Glucose], p.glucoseeffectiveDate[GlucoseEffectiveDate], 
                            p.hba1c[HbA1c], p.hba1ceffectiveDate[HbA1cEffectiveDate], p.alt[ALT], p.alteffectiveDate[ALTEffectiveDate], p.ast[AST], p.asteffectiveDate[ASTEffectiveDate], p.eGfr[eGFR], p.eGfreffectiveDate[eGFREffectiveDate], p.bilirubin[Bilirubin], 
                            p.bilirubineffectiveDate[BilirubinEffectiveDate], p.mauCreatRatio[UAlbumintoCreatratio], p.mauCreatRatioeffectiveDate[UAlbumintoCreatratioEffectiveDate], p.systolicBP[systolicBP],p.BPeffectiveDate[systolicBPEffectiveDate], p.diastolicBP[diastolicBP], p.BPeffectiveDate[diastolicBPEffectiveDate], 
                            p.FEV1, p.FEV1effectiveDate, p.Eosinophylia, p.EosinophyliaeffectiveDate, p.hivEliza[HIVElisa], p.hivElizaeffectiveDate[HIVElisaEffectiveDate], p.normGtt[NormalGTT], p.normGtteffectiveDate[NormalGTTEffectiveDate], p.abnGtt[AbnormalGTT], p.abnGtteffectiveDate[AbnormalGTTEffectiveDate], 
                            p.ucreatinine[UCreatinine], p.ucreatinineeffectiveDate[UCreatinineEffectiveDate], p.salbumin[SAlbumin], p.salbumineffectiveDate[SAlbuminEffectiveDate], p.ualbuminuria[UAlbuminuria], p.ualbuminuriaeffectiveDate[UAlbuminuriaEffectiveDate], 
                            p.urea[Urea], p.ureaeffectiveDate[UreaEffectiveDate], p.creatinine[Creatinine], p.creatinineeffectiveDate[CreatinineEffectiveDate]
                            
                            FROM Dependant d
                            INNER JOIN Member m on m.memberID = d.memberID
                            INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                            LEFT OUTER JOIN Pathology p on d.DependantID = p.dependentID
                            WHERE d.DependantID = '{2}'
                            AND m.active = 1
                            AND ma.Active = 1
                            AND p.Active = 1
                            AND (
                                CD4CounteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR CD4PercentageeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR viralLoadeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR haemoglobineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR bilirubineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR totalCholestroleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hdleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ldleffectiveDate BETWEEN '{0}' AND '{1}'
                                OR triglycerideseffectiveDate BETWEEN '{0}' AND '{1}'
                                OR glucoseeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hba1ceffectiveDate BETWEEN '{0}' AND '{1}'
                                OR alteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR asteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ureaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR creatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR eGfreffectiveDate BETWEEN '{0}' AND '{1}'
                                OR mauCreatRatioeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR BPeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR FEV1effectiveDate BETWEEN '{0}' AND '{1}'
                                OR EosinophyliaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR hivElizaeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR normGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR abnGtteffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ucreatinineeffectiveDate BETWEEN '{0}' AND '{1}'
                                OR salbumineffectiveDate BETWEEN '{0}' AND '{1}'
                                OR ualbuminuriaeffectiveDate BETWEEN '{0}' AND '{1}'
                            )", fromDate, toDate, DependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = db.Query<PathologySortVM>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                if (!String.IsNullOrEmpty(pathologyfield))
                {
                    string[] pathField = pathologyfield.Replace(" ", "").Split(',');
                    var pf = pathField.ToList();
                    var toKeep = new List<PathologySortVM>();
                    var toRemove = new List<PathologySortVM>();
                    foreach (var r in results)
                    {
                        //set remove = true, assume they are all empty
                        var remove = true;
                        foreach (var item in pf)
                        {
                            //If anyone is found not to be empty, then don't remove it
                            if (r.GetType().GetProperty(item) != null)
                            {
                                remove = false;
                                break;
                            }
                        }

                        if (remove) { toRemove.Add(r); }
                        else { toKeep.Add(r); }
                    }
                    results = toKeep;
                }

                foreach (var item in results)
                {
                    if (dtFrom <= item.CD4countEffectiveDate && dtTo >= item.CD4countEffectiveDate) { }
                    else { item.CD4count = null; }
                    if (dtFrom <= item.CD4percentageEffectiveDate && dtTo >= item.CD4percentageEffectiveDate) { }
                    else { item.CD4percentage = null; }
                    if (dtFrom <= item.ViralloadEffectiveDate && dtTo >= item.ViralloadEffectiveDate) { }
                    else { item.Viralload = null; }
                    if (dtFrom <= item.HaemoglobinEffectiveDate && dtTo >= item.HaemoglobinEffectiveDate) { }
                    else { item.Haemoglobin = null; }
                    if (dtFrom <= item.BilirubinEffectiveDate && dtTo >= item.BilirubinEffectiveDate) { }
                    else { item.Bilirubin = null; }
                    if (dtFrom <= item.TotalCholestrolEffectiveDate && dtTo >= item.TotalCholestrolEffectiveDate) { }
                    else { item.TotalCholestrol = null; }
                    if (dtFrom <= item.HDLEffectiveDate && dtTo >= item.HDLEffectiveDate) { }
                    else { item.HDL = null; }
                    if (dtFrom <= item.LDLEffectiveDate && dtTo >= item.LDLEffectiveDate) { }
                    else { item.LDL = null; }
                    if (dtFrom <= item.TriglyceridesEffectiveDate && dtTo >= item.TriglyceridesEffectiveDate) { }
                    else { item.Triglycerides = null; }
                    if (dtFrom <= item.GlucoseEffectiveDate && dtTo >= item.GlucoseEffectiveDate) { }
                    else { item.Glucose = null; }
                    if (dtFrom <= item.HbA1cEffectiveDate && dtTo >= item.HbA1cEffectiveDate) { }
                    else { item.HbA1c = null; }
                    if (dtFrom <= item.ALTEffectiveDate && dtTo >= item.ALTEffectiveDate) { }
                    else { item.ALT = null; }
                    if (dtFrom <= item.ASTEffectiveDate && dtTo >= item.ASTEffectiveDate) { }
                    else { item.AST = null; }
                    if (dtFrom <= item.UreaEffectiveDate && dtTo >= item.UreaEffectiveDate) { }
                    else { item.Urea = null; }
                    if (dtFrom <= item.CreatinineEffectiveDate && dtTo >= item.CreatinineEffectiveDate) { }
                    else { item.Creatinine = null; }
                    if (dtFrom <= item.eGFREffectiveDate && dtTo >= item.eGFREffectiveDate) { }
                    else { item.eGFR = null; }
                    if (dtFrom <= item.UAlbumintoCreatratioEffectiveDate && dtTo >= item.UAlbumintoCreatratioEffectiveDate) { }
                    else { item.UAlbumintoCreatratio = null; }
                    if (dtFrom <= item.systolicBPEffectiveDate && dtTo >= item.systolicBPEffectiveDate) { }
                    else { item.systolicBP = null; }
                    if (dtFrom <= item.diastolicBPEffectiveDate && dtTo >= item.diastolicBPEffectiveDate) { }
                    else { item.diastolicBP = null; }
                    if (dtFrom <= item.FEV1EffectiveDate && dtTo >= item.FEV1EffectiveDate) { }
                    else { item.FEV1 = null; }
                    if (dtFrom <= item.EosinophyliaEffectiveDate && dtTo >= item.EosinophyliaEffectiveDate) { }
                    else { item.Eosinophylia = null; }
                    if (dtFrom <= item.HIVElisaEffectiveDate && dtTo >= item.HIVElisaEffectiveDate) { }
                    else { item.HIVElisa = null; }
                    if (dtFrom <= item.NormalGTTEffectiveDate && dtTo >= item.NormalGTTEffectiveDate) { }
                    else { item.NormalGTT = null; }
                    if (dtFrom <= item.AbnormalGTTEffectiveDate && dtTo >= item.AbnormalGTTEffectiveDate) { }
                    else { item.AbnormalGTT = null; }
                    if (dtFrom <= item.UCreatinineEffectiveDate && dtTo >= item.UCreatinineEffectiveDate) { }
                    else { item.UCreatinine = null; }
                    if (dtFrom <= item.SAlbuminEffectiveDate && dtTo >= item.SAlbuminEffectiveDate) { }
                    else { item.SAlbumin = null; }
                    if (dtFrom <= item.UAlbuminuriaEffectiveDate && dtTo >= item.UAlbuminuriaEffectiveDate) { }
                    else { item.UAlbuminuria = null; }
                }

                #region date-sort hcare-974
                if (!String.IsNullOrEmpty(pathologyfield))
                {
                    var pathologyfields = pathologyfield.Replace(" ", "").Split(',').ToList();
                    var datelist = new List<DateTime>();

                    foreach (var result in results)
                    {
                        foreach (var item in pathologyfields)
                        {
                            PropertyInfo propertyInfo = result.GetType().GetProperty(item + "EffectiveDate");
                            object date = propertyInfo.GetValue(result, null);

                            if (date != null)
                            {
                                datelist.Add(Convert.ToDateTime(date));
                            }
                        }
                    }

                    datelist.Sort((a, b) => b.CompareTo(a)); // sort-descending

                    var distinctdates = datelist.GroupBy(x => x.Date).Select(x => x.Key).ToList();

                    var pathologysort = new List<PathologySortVM>();

                    foreach (var date in distinctdates)
                    {
                        pathologysort.Add(new PathologySortVM()
                        {
                            DependantID = DependantID,
                            InitialDate = date,
                            Viralload = results.Where(x => x.ViralloadEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Viralload).FirstOrDefault(),
                            CD4count = results.Where(x => x.CD4countEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.CD4count).FirstOrDefault(),
                            CD4percentage = results.Where(x => x.CD4percentageEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.CD4percentage).FirstOrDefault(),
                            Haemoglobin = results.Where(x => x.HaemoglobinEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Haemoglobin).FirstOrDefault(),
                            Bilirubin = results.Where(x => x.BilirubinEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Bilirubin).FirstOrDefault(),
                            TotalCholestrol = results.Where(x => x.TotalCholestrolEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.TotalCholestrol).FirstOrDefault(),
                            HDL = results.Where(x => x.HDLEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.HDL).FirstOrDefault(),
                            LDL = results.Where(x => x.LDLEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.LDL).FirstOrDefault(),
                            Triglycerides = results.Where(x => x.TriglyceridesEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Triglycerides).FirstOrDefault(),
                            Glucose = results.Where(x => x.GlucoseEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Glucose).FirstOrDefault(),
                            HbA1c = results.Where(x => x.HbA1cEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.HbA1c).FirstOrDefault(),
                            ALT = results.Where(x => x.ALTEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.ALT).FirstOrDefault(),
                            AST = results.Where(x => x.ASTEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.AST).FirstOrDefault(),
                            Urea = results.Where(x => x.UreaEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Urea).FirstOrDefault(),
                            Creatinine = results.Where(x => x.CreatinineEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Creatinine).FirstOrDefault(),
                            eGFR = results.Where(x => x.eGFREffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.eGFR).FirstOrDefault(),
                            UAlbumintoCreatratio = results.Where(x => x.UAlbumintoCreatratioEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.UAlbumintoCreatratio).FirstOrDefault(),
                            systolicBP = results.Where(x => x.systolicBPEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.systolicBP).FirstOrDefault(),
                            diastolicBP = results.Where(x => x.diastolicBPEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.diastolicBP).FirstOrDefault(),
                            FEV1 = results.Where(x => x.FEV1EffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.FEV1).FirstOrDefault(),
                            Eosinophylia = results.Where(x => x.EosinophyliaEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.Eosinophylia).FirstOrDefault(),
                            HIVElisa = results.Where(x => x.HIVElisaEffectiveDate.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.HIVElisa).FirstOrDefault(),
                            NormalGTT = results.Where(x => x.NormalGTTEffectiveDate.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.NormalGTT).FirstOrDefault(),
                            AbnormalGTT = results.Where(x => x.AbnormalGTTEffectiveDate.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.AbnormalGTT).FirstOrDefault(),
                            UCreatinine = results.Where(x => x.UCreatinineEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.UCreatinine).FirstOrDefault(),
                            SAlbumin = results.Where(x => x.SAlbuminEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.SAlbumin).FirstOrDefault(),
                            UAlbuminuria = results.Where(x => x.UAlbuminuriaEffectiveDate?.ToString("dd/MM/yyyy") == date.ToString("dd/MM/yyyy")).Select(x => x.UAlbuminuria).FirstOrDefault(),
                        });
                    }

                    return pathologysort;

                }
                #endregion

                return results.OrderByDescending(x => x.InitialDate).ToList();
            }
        }

        public List<Queries> GetSearchQueries(string querytype, string membershipNo = "")
        {
            var result = _context.Queries.ToList();

            if (querytype != "")
            {
                result = result.Where(x => x.enquiryBy.ToLower() == querytype.ToLower()).ToList();
            }

            if (!String.IsNullOrEmpty(membershipNo))
            {
                var deps = (from r in result
                            join d in _context.Dependants
                            on r.dependentID equals d.DependantID
                            join m in _context.Members
                            on d.memberID equals m.memberID
                            where m.membershipNo.Contains(membershipNo)
                            select r).OrderBy(x => x.effectiveDate).ToList();
                return deps;
            }
            return result;
        }

        public ServiceResult InsertEnrolmentMonitor(EnrolmentStepsMonitor model)
        {
            _context.EnrolmentStepsMonitor.Add(model);
            var result = SaveResult();

            return result;
        }

        public EnrolmentStepsMonitor GetEnrolmentStep(Guid depId)
        {
            return _context.EnrolmentStepsMonitor.Where(x => x.dependantId == depId).FirstOrDefault();
        }

        public ServiceResult UpdateEnrolmentStep(EnrolmentStepsMonitor model)
        {
            var old = _context.EnrolmentStepsMonitor.Where(x => x.dependantId == model.dependantId).FirstOrDefault();
            old.clinicalCaptured = model.clinicalCaptured;
            old.hasClinical = model.hasClinical;
            old.hasDemographic = model.hasDemographic;
            old.demographicCaptured = model.demographicCaptured;
            old.hasScript = model.hasScript;
            old.scriptCaptured = model.scriptCaptured;
            old.hasPathology = model.hasPathology;
            old.active = model.active;
            old.pathologyCaptured = model.pathologyCaptured;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateClinical(Clinical model)
        {
            var old = _context.Clinical.Where(x => x.id == model.id).FirstOrDefault();
            old.effectiveDate = model.effectiveDate;
            old.height = model.height;
            old.weight = model.weight;
            old.bmi = model.bmi;
            old.bodyServiceArea = model.bodyServiceArea;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.programType = model.programType;
            old.clinicalComment = model.clinicalComment;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();
        }

        public ServiceResult InsertClinicalExam(Clinical model)
        {
            _context.Clinical.Add(model);

            return _context.SaveChanges();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public ServiceResult SaveResult()
        {
            return _context.SaveChanges();
        }

        public ServiceResult UpdateCaseManager(CaseManagers model)
        {
            var old = _context.CaseManagers.Where(x => x.caseManagerNo == model.caseManagerNo).FirstOrDefault();
            old.caseManagerName = model.caseManagerName;
            old.caseManagerSurname = model.caseManagerSurname;
            old.extension = model.extension;
            old.workNo = model.workNo;
            old.email = model.email;
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateQuery(Queries model)
        {
            var old = _context.Queries.Where(x => x.queryID == model.queryID).FirstOrDefault();
            old.querySolution = model.querySolution;
            old.resolutionDate = model.resolutionDate;
            old.resolvedBy = model.resolvedBy;
            old.priority = model.priority;
            old.comment = model.comment;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;
            old.queryStatus = model.queryStatus;
            old.Owner = model.Owner;
            old.effectiveDate = model.effectiveDate;
            return _context.SaveChanges();
        }



        public List<Pathology> GetLaboratories()
        {
            string sql = string.Format(@"SELECT DISTINCT labName
                                        FROM Pathology");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var pathologyLab = (List<Pathology>)db.Query<Pathology>(sql, null, commandTimeout: 500);
                db.Close();
                return pathologyLab;
            }
        }

        public List<PathologyFields> GetPathologyFields()
        {
            return _context.pathologyFields.Where(x => x.active == true).OrderByDescending(x => x.PathologyField).ToList();
        }

        public Queries GetQueryById(int Id)
        {
            return _context.Queries.Where(x => x.queryID == Id).FirstOrDefault();
        }

        public AssignmentView GetAssignment(Guid depID, string itemType)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == depID
                           where ai.itemType == itemType
                           where a.Active == true
                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               itemType = ai.itemType,
                               program = a.programId //hcare-1112
                           }).FirstOrDefault();

            return results;
        }

        public AssignmentView GetAssignment(Guid depID, string itemType, Guid? pro)
        {
            var results = (from a in _context.Assignments
                           join ai in _context.AssignmentItems
                           on a.assignmentID equals ai.assignmentId
                           join ait in _context.AssignmentItemTypes
                           on ai.itemType equals ait.assignmentItemType
                           where a.dependentID == depID
                           where ai.itemType == itemType
                           where a.programId == pro
                           where a.Active == true
                           select new AssignmentView()
                           {
                               assignmentID = a.assignmentID,
                               effectiveDate = a.effectiveDate,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               AssignmentItemType = ait.itemDescription,
                               createdBy = a.createdBy,
                               Instruction = a.Instruction,
                               itemType = ai.itemType,
                               program = a.programId //hcare-1112

                           }).FirstOrDefault();

            return results;
        }


        public Assignments GetAssignment(int Id)
        {
            return _context.Assignments.Where(x => x.assignmentID == Id).FirstOrDefault();
        }

        public ServiceResult UpdateAssignment(Assignments model)
        {
            var old = _context.Assignments.Where(x => x.assignmentID == model.assignmentID).FirstOrDefault();
            old.assignmentType = model.assignmentType;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;
            old.comment = model.comment;
            old.postpone = model.postpone;
            old.postponementReason = model.postponementReason;
            old.postponeToDate = model.postponeToDate;
            old.status = model.status;

            return _context.SaveChanges();
        }



        public ServiceResult InsertMedicationHistory(MedicationHistory model)
        {
            _context.MedicationHistory.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult InsertDrQuestionnaire(DoctorQuestionnaireResults model)
        {
            _context.DoctorQuestionnaireResults.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateMedicationHistory(MedicationHistory model)
        {
            var old = _context.MedicationHistory.Where(x => x.Id == model.Id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifieddate = DateTime.Now;
            old.comment = model.comment;
            old.directions = model.directions;
            old.endDate = model.endDate;
            old.startDate = model.startDate;
            old.nappiCode = model.nappiCode;
            old.productName = model.productName;
            old.programType = model.programType;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();
        }




        //================================================================================ Disclaimer ================================================================================//

        public PatientQuestionnaireResponse GetDisclaimerResultsById(int id)
        {
            return _context.PatientQuestionnaireResponses.Where(x => x.PatientQuestionnaireResponseID == id).FirstOrDefault();
        }

        public List<PatientQuestionnaireResponse> GetDisclaimerResults(Guid depId)
        {

            return _context.PatientQuestionnaireResponses.Where(x => x.DependantId == depId).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<disclaimerViewModel> GetDisclaimerViewResults(Guid depId)
        {
            var latest = _context.PatientQuestionnaireResponses.Where(x => x.Active == true).Where(x => x.DependantId == depId).OrderByDescending(x => x.CreatedDate).Select(x => x.TaskId).FirstOrDefault();

            var result = (from pq in _context.PatientQuestionnaireResponses
                          join qt in _context.QuestionnaireTemplates
                          on pq.TemplateId equals qt.QuestionnaireTemplateID
                          where pq.TaskId == latest
                          where pq.DependantId == depId
                          select new disclaimerViewModel()
                          {
                              DependantId = pq.DependantId,
                              QuestionnaireTemplateID = qt.QuestionnaireTemplateID,
                              TaskId = pq.TaskId,
                              TemplateId = pq.TemplateId,
                              TemplateName = qt.TemplateName,
                              questionNo = qt.QuestionNo,
                              Question = qt.Question,
                              Answer = pq.Answer,
                              Confirmed = pq.Confirmed,
                              CreatedDate = pq.CreatedDate,
                              CreatedBy = pq.CreatedBy,
                              ModifiedDate = pq.ModifiedDate,
                              ModifiedBy = pq.ModifiedBy,
                              yesNo = qt.Answer,


                          }).OrderBy(x => x.TemplateId).ToList();

            return result;
        }

        public List<disclaimerViewModel> GetDisclaimerViewDistinct(Guid depId)
        {
            var latest = _context.PatientQuestionnaireResponses.Where(x => x.Active == true).Where(x => x.DependantId == depId).OrderByDescending(x => x.CreatedDate).OrderBy(x => x.TaskId).FirstOrDefault();

            var result = (from pq in _context.PatientQuestionnaireResponses
                          join qt in _context.QuestionnaireTemplates
                          on pq.TemplateId equals qt.QuestionnaireTemplateID
                          where pq.TaskId == latest.TaskId
                          where pq.DependantId == depId
                          select new disclaimerViewModel()
                          {
                              DependantId = pq.DependantId,
                              TaskId = pq.TaskId,
                              TemplateName = qt.TemplateName,
                              TemplateId = pq.TemplateId,


                          }).OrderBy(x => x.TaskId).ToList();

            return result;
        }

        public List<PatientQuestionnaireResponse> GetDisclaimerViewList(Guid depId)
        {
            var list = _context.PatientQuestionnaireResponses.Where(x => x.DependantId == depId).Where(x => x.Active == true).Distinct().ToList();

            return list;

        }

        //public List<disclaimerViewModel> GetDisclaimerViewDistinct(Guid depId)
        //{
        //    string sql = string.Format(@"SELECT DISTINCT qt.TemplateName, pq.TaskId, pq.DependantId
        //                    FROM PatientQuestionnaireResponse pq
        //                    JOIN QuestionnaireTemplate qt ON pq.TemplateId = qt.QuestionnaireTemplateID", depId);

        //    using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SchoolContext"].ConnectionString))
        //    {
        //        db.Open();
        //        var ourCustomers = (List<disclaimerViewModel>)db.Query<disclaimerViewModel>(sql,null,commandTimeout:500);
        //        db.Close();
        //        return ourCustomers;
        //    }
        //}

        public ServiceResult InsertDisclaimerResults(PatientQuestionnaireResponse model)
        {
            _context.PatientQuestionnaireResponses.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateDisclaimerResults(PatientQuestionnaireResponse model)
        {
            var old = _context.PatientQuestionnaireResponses.Where(x => x.PatientQuestionnaireResponseID == model.PatientQuestionnaireResponseID).FirstOrDefault();

            old.Answer = model.Answer;
            old.ModifiedBy = model.ModifiedBy;

            return _context.SaveChanges();

        }

        public Dependant GetDependantByDependantID(Guid depID)
        {
            return _context.Dependants.Where(x => x.DependantID == depID).FirstOrDefault();
        }


        public Dependant GetDependantDetails(Guid depID, Guid? pro)
        {
            var results = _context.Dependants.Where(x => x.DependantID == depID).FirstOrDefault();

            results.languageCode = _context.Language.Where(x => x.languageName == results.languageCode).Select(x => x.languageCode).FirstOrDefault();

            return results;
        }

        //================================================================================ Risk Rating ================================================================================//

        public List<PatientRiskRatingHistory> GetPatientRiskRating(Guid dependantID)
        {
            var results = _context.PatientRiskRatingHistory.Where(x => x.dependantID == dependantID).Where(x => x.active == true).ToList();

            return results;
        }

        public PatientRiskRatingHistory GetPatientRiskRatingByID(int id)
        {
            var results = _context.PatientRiskRatingHistory.Where(x => x.id == id).Where(x => x.active == true).FirstOrDefault();
            results.RiskId = _context.RiskRatingTypes.Where(x => x.RiskType == results.RiskId).Select(x => x.RiskName).FirstOrDefault();

            return results;
        }

        public List<HypoglymiaRiskHistory> GetHypoRiskRating(Guid dependantID)
        {
            var results = _context.HypoglymiaRiskHistory.Where(x => x.dependantID == dependantID).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
            return results;
        }

        public List<RiskRatingTypes> GetRiskRatingTypes()
        {
            return _context.RiskRatingTypes.Where(x => x.Active == true).OrderBy(x => x.RiskPriority).ToList();
        }

        public List<PatientRiskRatingHistory> GetPatientRiskRatingMultipleReasons(Guid dependantID, Guid? pro)
        {
            var results = new List<PatientRiskRatingHistory>();
            if (pro != null)
            {
                var programcode = _context.Program.Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                results = _context.PatientRiskRatingHistory.Where(x => x.dependantID == dependantID).Where(x => x.active == true).Where(x => x.programType == programcode).ToList();
            }
            else
            {
                results = _context.PatientRiskRatingHistory.Where(x => x.dependantID == dependantID).Where(x => x.active == true).ToList();
            }

            foreach (var result in results)
            {
                var risk = GetHypoRiskRating(dependantID);
                if (risk.Count() > 0)
                {
                    var hypoRiskReason = " -- HypoRisk Reason = Insulin: " + risk[0].Insulin + " HypoRisk: " + risk[0].HypoRisk + " Sulpohonylureas: " + risk[0].Sulphonylureas + " Glucose: " + risk[0].Glucose + " Renal: " + risk[0].Renal + " Hypo: " + risk[0].HypoRisk;
                    result.reason = result.reason + hypoRiskReason;

                }
                result.RiskId = _context.RiskRatingTypes.Where(x => x.RiskType == result.RiskId).Select(x => x.RiskName).FirstOrDefault();
            }

            return results.OrderByDescending(x => x.effectiveDate).ToList(); //HCare-1204
        }

        public PatientRiskRatingHistory GetRiskRatingByID(int? id)
        {
            return _context.PatientRiskRatingHistory.Where(x => x.id == id).FirstOrDefault();
        }

        //========================================================================= Management Status Reasons =========================================================================//

        public List<ManagementStatus_DeactivatedReasons> GetManagementStatusReasons()
        {
            return _context.managementStatus_DeactivatedReasons.OrderByDescending(x => x.reason).OrderByDescending(x => x.active == true).ToList();

        }

        public ManagementStatus_DeactivatedReasons GetManagementStatusReasonByReason(string reason)
        {
            return _context.managementStatus_DeactivatedReasons.Where(x => x.reason.ToLower() == reason.ToLower()).FirstOrDefault();
        }

        public ManagementStatus_DeactivatedReasons GetManagementStatusReasonByName(string name)
        {
            return _context.managementStatus_DeactivatedReasons.Where(x => x.name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public ServiceResult InsertManagementStatusReasons(ManagementStatus_DeactivatedReasons model)
        {
            _context.managementStatus_DeactivatedReasons.Add(model);
            return _context.SaveChanges();
        }

        public ManagementStatus_DeactivatedReasons GetManagementStatusReasonByID(int id)
        {
            return _context.managementStatus_DeactivatedReasons.Where(x => x.id == id).FirstOrDefault();
        }

        public ServiceResult UpdateManagementStatusReasons(ManagementStatus_DeactivatedReasons model)
        {
            var old = _context.managementStatus_DeactivatedReasons.Where(x => x.id == model.id).FirstOrDefault();
            old.name = model.name;
            old.reason = model.reason;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.active = model.active;
            return _context.SaveChanges();
        }

        //============================================================== OTHER MEDICAL HISTORY/ANDER MEDIESE GESKIEDENIS ==============================================================//

        public OtherMedicalHistory GetOtherMedicalHistoryById(int id)
        {
            return _context.otherMedicalHistories.Where(x => x.Id == id).FirstOrDefault();
        }
        public List<OtherMedicalHistory> GetOtherMedicalHistory(Guid depId)
        {
            return _context.otherMedicalHistories.Where(x => x.dependentID == depId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }
        public ServiceResult InsertOtherMedicalHistory(OtherMedicalHistory model)
        {
            _context.otherMedicalHistories.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateOtherMedicalHistory(OtherMedicalHistory model)
        {
            var old = _context.otherMedicalHistories.Where(x => x.Id == model.Id).FirstOrDefault();

            old.respiratoryTract = model.respiratoryTract;
            old.respiratoryTractEffectiveDate = model.respiratoryTractEffectiveDate;
            old.respiratoryTractComment = model.respiratoryTractComment;
            old.pneumonia = model.pneumonia;
            old.pneumoniaEffectiveDate = model.pneumoniaEffectiveDate;
            old.pneumoniaComment = model.pneumoniaComment;
            old.meningitis = model.meningitis;
            old.meningitisEffectiveDate = model.meningitisEffectiveDate;
            old.meningitisComment = model.meningitisComment;
            old.lymphadenopathy = model.lymphadenopathy;
            old.lymphadenopathyEffectiveDate = model.lymphadenopathyEffectiveDate;
            old.lymphadenopathyComment = model.lymphadenopathyComment;
            old.diarrhoea = model.diarrhoea;
            old.diarrhoeaEffectiveDate = model.diarrhoeaEffectiveDate;
            old.diarrhoeaComment = model.diarrhoeaComment;
            old.bladderInfection = model.bladderInfection;
            old.bladderInfectionEffectiveDate = model.bladderInfectionEffectiveDate;
            old.bladderInfectionComment = model.bladderInfectionComment;
            old.weightLoss = model.weightLoss;
            old.weightLossEffectiveDate = model.weightLossEffectiveDate;
            old.weightLossComment = model.weightLossComment;
            old.cancer = model.cancer;
            old.cancerEffectiveDate = model.cancerEffectiveDate;
            old.cancerComment = model.cancerComment;
            old.cervicalCancer = model.cervicalCancer;
            old.cervicalCancerEffectiveDate = model.cervicalCancerEffectiveDate;
            old.cervicalCancerComment = model.cervicalCancerComment;
            old.immunization = model.immunization;
            old.immunizationEffectiveDate = model.immunizationEffectiveDate;
            old.immunizationComment = model.immunizationComment;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.generalComments = model.generalComments;
            old.programType = model.programType;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();

        }

        //================================================================================== Clinical ==================================================================================//

        public List<Clinical> GetClinicalExam(Guid depId)
        {
            var results = _context.Clinical.Where(x => x.dependantID == depId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public List<Clinical> GetGeneralClinicalExam(Guid depId)
        {
            var results = _context.Clinical.Where(x => x.dependantID == depId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            //foreach (var res in results)
            //{
            //    res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            //}

            return results;
        }
        public List<Clinical> GetDiabetesClinicalExam(Guid depId)
        {
            var results = _context.Clinical.Where(x => x.dependantID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes").OrderByDescending(x => x.createdDate).ToList();
            //foreach (var res in results)
            //{
            //    res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            //}

            return results;
        }
        public List<Clinical> GetHivClinicalExam(Guid depId)
        {
            var results = _context.Clinical.Where(x => x.dependantID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "HIVPR" || x.programType.ToLower() == "hiv").OrderByDescending(x => x.createdDate).ToList();
            //foreach (var res in results)
            //{
            //    res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            //}

            return results;
        }

        //================================================================================== CoMorbids ==================================================================================//

        public List<ComorbidView> GetComorbidItems(Guid depID, Guid? pro = null)
        {
            var model = _context.CoMorbidConditions.Where(x => x.dependantID == depID).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();

            var comorbid = new List<ComorbidView>();
            foreach (var item in model)
            {
                comorbid.Add(new ComorbidView()
                {
                    diagnosisDate = item.diagnosisDate,
                    CoMorbidTreatement = item.CoMorbidTreatement,
                    treatementEndDate = item.treatementEndDate,
                    effectiveDate = item.effectiveDate,
                    //HCare-1293
                    ICD10Code = _context.ComorbidConditionExclusions.Where(x => x.id == item.coMorbidId).Select(x => x.ICD10Code).FirstOrDefault(),// : _context.CoMorbidTypes.Where(x => x.id == item.coMorbidId).Select(x => x.icd10).FirstOrDefault(),
                    coMorbidId = _context.ComorbidConditionExclusions.Where(x => x.id == item.coMorbidId).Select(x => x.mappingDescription).FirstOrDefault(),// : _context.CoMorbidTypes.Where(x => x.id == item.coMorbidId).Select(x => x.condition).FirstOrDefault(),

                    createdDate = item.createdDate,
                    id = _context.CoMorbidConditions.Where(x => x.id == item.id).Select(x => x.id).FirstOrDefault(),
                    programType = item.programType,
                    generalComments = item.generalComments,
                    followUp = item.followUp,


                });
            }
            //HCare-1031
            if (pro != null)
            {
                var progcode = _context.Program.Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                var icd10s = _context.PatientProgramHistory.Where(x => x.programCode == progcode).Where(x => x.dependantId == depID).Select(x => x.icd10Code).Distinct().ToList();
                //var progs = _context.PatientProgramHistory.Where(x => x.dependantId == depID).Where(x => x.programCode != progcode).ToList();

                comorbid = (from r in comorbid
                            where !icd10s.Contains(r.ICD10Code)
                            select r).ToList();

            }

            return comorbid;
        }
        public List<ComorbidView> GetGeneralComorbidItems(Guid depID, Guid? pro = null)
        {
            var model = _context.CoMorbidConditions.Where(x => x.dependantID == depID).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();

            var comorbid = new List<ComorbidView>();
            foreach (var item in model)
            {
                comorbid.Add(new ComorbidView()
                {
                    diagnosisDate = item.diagnosisDate,
                    CoMorbidTreatement = item.CoMorbidTreatement,
                    treatementEndDate = item.treatementEndDate,
                    effectiveDate = item.effectiveDate,
                    //HCare-1293
                    ICD10Code = _context.ComorbidConditionExclusions.Where(x => x.id == item.coMorbidId).Select(x => x.ICD10Code).FirstOrDefault(),// : _context.CoMorbidTypes.Where(x => x.id == item.coMorbidId).Select(x => x.icd10).FirstOrDefault(),
                    coMorbidId = _context.ComorbidConditionExclusions.Where(x => x.id == item.coMorbidId).Select(x => x.mappingDescription).FirstOrDefault(),// : _context.CoMorbidTypes.Where(x => x.id == item.coMorbidId).Select(x => x.condition).FirstOrDefault(),

                    createdDate = item.createdDate,
                    id = _context.CoMorbidConditions.Where(x => x.id == item.id).Select(x => x.id).FirstOrDefault(),
                    //programType = item.programType,
                    programType = _context.Program.Where(x => x.code == item.programType).Select(x => x.ProgramName).FirstOrDefault(),
                    generalComments = item.generalComments,
                    followUp = item.followUp,
                });
            }
            //HCare-1031
            if (pro != null)
            {
                var progcode = _context.Program.Where(x => x.programID == pro).Select(x => x.code).FirstOrDefault();
                var icd10s = _context.PatientProgramHistory.Where(x => x.programCode == progcode).Where(x => x.dependantId == depID).Select(x => x.icd10Code).Distinct().ToList();
                var icd10Subs = _context.PatientProgramSubHistory.Where(x => x.programCode == progcode).Where(x => x.dependantId == depID).Select(x => x.icd10Code).ToList();

                comorbid = (from r in comorbid
                            where !icd10s.Contains(r.ICD10Code)
                            select r).ToList();

                comorbid = (from r in comorbid
                            where !icd10Subs.Contains(r.ICD10Code)
                            select r).ToList();

            }

            return comorbid;
        }


        public List<ComorbidView> GetComorbidConditions()
        {
            var results = (from cc in _context.CoMorbidConditions
                           join ct in _context.CoMorbidTypes on cc.coMorbidId equals ct.id
                           select new ComorbidView
                           {


                           }).ToList();

            return results;
        }

        public List<CoMormidConditions> CoMorbid_Validation(Guid depID)
        {
            return _context.CoMorbidConditions.Where(x => x.dependantID == depID).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }

        public List<CoMormidConditions> GetCoMormidConditions()
        {
            return _context.CoMorbidConditions.Where(x => x.active == true).ToList();
        }


        //============================================================================ Medication History ============================================================================//

        //============================================================================= Medication History =============================================================================//

        public MedicationHistory GetMedicationHistory(int Id)
        {
            return _context.MedicationHistory.Where(x => x.Id == Id).FirstOrDefault();
        }
        public List<MedicationHistory> GetMedicationHistory()
        {
            return _context.MedicationHistory.Where(x => x.active == true).ToList();
        }
        public List<MedicationHistory> GetMedicationHistories(Guid DepId)
        {
            return _context.MedicationHistory.Where(x => x.dependantId == DepId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }
        public List<MedicationHistory> GetGeneralMedicationHistories(Guid DepId)
        {
            var results = _context.MedicationHistory.Where(x => x.dependantId == DepId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "GEN" || x.programType.ToLower() == "general" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public List<MedicationHistory> GetHIVMedicationHistories(Guid DepId)
        {
            var results = _context.MedicationHistory.Where(x => x.dependantId == DepId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "HIVPR" || x.programType.ToLower() == "hiv" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }

        //================================================================================= Allergies =================================================================================//

        public List<Allergies> GetAllergies(Guid DepId)
        {
            return _context.Allergies.Where(x => x.dependantId == DepId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }
        public List<Allergies> GetGeneralAllergies(Guid DepId)
        {
            var results = _context.Allergies.Where(x => x.dependantId == DepId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public Allergies GetAllergyById(int Id)
        {
            return _context.Allergies.Where(x => x.id == Id).FirstOrDefault();
        }
        public Allergies GetAllergyByName(string allergy)
        {
            return _context.Allergies.Where(x => x.Allergy.ToLower() == allergy.ToLower()).FirstOrDefault();
        }
        public ServiceResult InsertAllergy(Allergies model)
        {
            _context.Allergies.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateAllergy(Allergies model)
        {
            var old = _context.Allergies.Where(x => x.id == model.id).FirstOrDefault();
            old.Allergy = model.Allergy;
            old.modifiedBy = model.modifiedBy;
            old.modifieddate = model.modifieddate;
            old.programType = model.programType;
            old.generalComments = model.generalComments;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();
        }

        //============================================================================== Social History ==============================================================================//

        public ClinicalHistoryQuestionaire getQuestionnaire(Guid Id)
        {
            return _context.ClinicalHistoryQuestionaire.Where(x => x.dependentID == Id).Where(x => x.active == true).FirstOrDefault();
        }
        public ClinicalHistoryQuestionaire GetSocialRecord(int id)
        {
            return _context.ClinicalHistoryQuestionaire.Where(x => x.id == id).FirstOrDefault();
        }
        public List<ClinicalHistoryQuestionaire> GetClinicalHistory(Guid DepId)
        {
            return _context.ClinicalHistoryQuestionaire.Where(x => x.dependentID == DepId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }
        public List<ClinicalHistoryQuestionaire> GetSocialHistory(Guid DepId)
        {
            return _context.ClinicalHistoryQuestionaire.Where(x => x.dependentID == DepId).OrderByDescending(x => x.createdDate).Where(x => x.active == true).ToList();
        }
        public List<ClinicalHistoryQuestionaire> GetGeneralSocialHistory(Guid DepId)
        {
            var results = _context.ClinicalHistoryQuestionaire.Where(x => x.dependentID == DepId).OrderByDescending(x => x.createdDate).Where(x => x.active == true).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertClinicalHistoryQuestionnaire(ClinicalHistoryQuestionaire model)
        {
            _context.ClinicalHistoryQuestionaire.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateQuestionnaire(ClinicalHistoryQuestionaire model)
        {
            var old = _context.ClinicalHistoryQuestionaire.Where(x => x.id == model.id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifieddate = model.modifieddate;
            old.DiagnosisDate = model.DiagnosisDate;
            old.smoker = model.smoker;
            old.NoCigs = model.NoCigs;
            old.drinker = model.drinker;
            old.NrDrinks = model.NrDrinks;
            old.active = model.active;

            return _context.SaveChanges();
        }
        public ServiceResult UpdateSocialRecord(ClinicalHistoryQuestionaire model)
        {
            var old = _context.ClinicalHistoryQuestionaire.Where(x => x.id == model.id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifieddate = model.modifieddate;
            old.DiagnosisDate = model.DiagnosisDate;
            old.smoker = model.smoker;
            old.NoCigs = model.NoCigs;
            old.smokingYears = model.smokingYears; //HCare-607
            old.drinker = model.drinker;
            old.NrDrinks = model.NrDrinks;
            old.programType = model.programType; //HCare-607
            old.socialComment = model.socialComment;
            old.followUp = model.followUp; //HCare-607
            old.active = model.active;

            return _context.SaveChanges();
        }

        //============================================================================= Hospitalisations =============================================================================//

        public List<HospitalizationAuths> GetHospitalizationAuths(string membershipNo, string dependantCode)
        {
            //return _context.HospitalizationAuths.Where(x => x.membershipNo == membershipNo).Where(x => x.dependantCode == dependantCode).Where(x=>x.authType != null).OrderByDescending(x => x.createdDate).ToList();
            var results = _context.HospitalizationAuths.Where(x => x.membershipNo == membershipNo).Where(x => x.dependantCode == dependantCode).Where(x => x.authType != null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public List<HospitalizationAuths> GetGeneralHospitalizationAuths(string membershipNo, string dependantCode)
        {
            var results = _context.HospitalizationAuths.Where(x => x.membershipNo == membershipNo).Where(x => x.dependantCode == dependantCode).Where(x => x.authType != null).Where(x => x.Active == true).OrderByDescending(x => x.createdDate).ToList();
            return results;
        }
        public ServiceResult InsertHospitalizationAuths(HospitalizationAuths model)
        {
            _context.HospitalizationAuths.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult InsertRiskRating(PatientRiskRatingHistory model)
        {
            _context.PatientRiskRatingHistory.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateRiskRating(PatientRiskRatingHistory model) //HCare-1151
        {
            var old = _context.PatientRiskRatingHistory.Where(x => x.id == model.id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.effectiveDate = model.effectiveDate;
            old.programType = model.programType;
            old.reason = model.reason;
            old.Comment = model.Comment;
            old.active = model.active;

            return _context.SaveChanges();
        }
        public HospitalizationAuths GetHospitalizationAuthByID(int id)
        {
            return _context.HospitalizationAuths.Where(x => x.id == id).FirstOrDefault();
        }
        public ServiceResult UpdateHospitalizationAuth(HospitalizationAuths model)
        {
            var old = _context.HospitalizationAuths.Where(x => x.id == model.id).FirstOrDefault();
            old.actualAdminDate = model.actualAdminDate;
            old.authType = model.authType;
            old.programType = model.programType; //HCare-607
            old.generalComments = model.generalComments; //HCare-607
            old.followUp = model.followUp; //HCare-607
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public List<HospitalizationClaims> GetHospitalizationClaims(string membershipNo, string dependantCode)
        {
            return _context.HospitalizationClaims.Where(x => x.membershipNo == membershipNo).Where(x => x.dependantCode == dependantCode).OrderByDescending(x => x.dischargeDate).ToList();
        }
        public List<HospitalClaimView> GetHospitalizationClaim(string membershipNo, string dependantCode)
        {
            //HCare-1325

            var results = (from q in _context.HospitalizationClaims
                           join m in _context.HospitalizationClaimEvents
                             on q.id equals m.refNo
                           join d in _context.Doctors on q.provider equals d.practiceNo
                           where q.membershipNo == membershipNo
                           where q.dependantCode == dependantCode
                           where !m.diagnosis.Equals(string.Empty) || !m.finalDiagnosis.Equals(string.Empty)
                           select new HospitalClaimView()
                           {
                               adminDate = q.adminDate,
                               dischargeDate = q.dischargeDate,
                               diagnosis = m.diagnosis,
                               finalDiagnosis = m.finalDiagnosis, //HCare-1208
                               procedureCode = m.procedureCode,
                               provider = q.provider,
                               vendor = q.vendor,
                               Fullname = d.title + " " + d.drFirstName + " " + d.drLastName

                           }).OrderByDescending(x => x.dischargeDate).ToList();


            //var results = new List<HospitalClaimView>(); //HCare-1208

            //string sql = String.Format(@"SELECT * FROM (SELECT   q.adminDate, q.dischargeDate, m.diagnosis, m.finalDiagnosis, 
            //                                            m.procedureCode,  q.provider, q.vendor, CONCAT( D.title,' ',' ' ,D.drFirstName, ' ' , D.drLastName) as Fullname,
            //                                            ROW_NUMBER() OVER(PARTITION BY adminDate ORDER BY dischargeDate desc) AS RowNumber
            //                                            FROM HospitalizationClaims q
            //                                            INNER join HospitalizationClaimEvents m on q.id = m.refNo
            //                                            LEFT OUTER JOIN Doctors D ON q.provider =D.practiceNo
            //                                            WHERE q.membershipNo = '{0}' and q.dependantCode = '{1}'
            //                                            AND (m.diagnosis != '' or  m.finalDiagnosis != '')
            //                                             ) as a
            //                             WHERE a.RowNumber = 1", membershipNo, dependantCode);

            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            //{
            //    db.Open();
            //    results = db.Query<HospitalClaimView>(sql, null, commandTimeout: 10000).ToList();
            //    db.Close();
            //}

            return results;
        }
        public List<HospitalizationClaimEvents> GetClaimEvents(string claimsNo)
        {
            return _context.HospitalizationClaimEvents.Where(x => x.claimNo == claimsNo).OrderByDescending(x => x.createddate).ToList();
        }
        public List<HospitalizationAuthEvents> GetAuthEvents(string epiAuth)
        {
            return _context.HospitalizationAuthEvents.Where(x => x.epiAuth == epiAuth).OrderByDescending(x => x.createddate).ToList();
        }

        //============================================================================== Question Other ==============================================================================//

        public QuestionnaireOther GetQuestionnaireOtherResultById(int id)
        {
            return _context.QuestionnaireOthers.Where(x => x.QuestionnaireOtherID == id).FirstOrDefault();
        }
        public List<QuestionnaireOther> GetQuestionnaireOtherResults(Guid depId)
        {
            return _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }
        public QuestionnaireOther GetTuberculosisResults(Guid depId)
        {
            var results = _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.TBScreenResult.ToLower() == "positive").Where(x => x.followUp != true).OrderByDescending(x => x.createdDate).FirstOrDefault();

            if (results != null && (results.tbTreatmentEndDate >= DateTime.Today || results.tbTreatmentEndDate == null)) { return results; }

            return null;
        }

        public List<QuestionnaireOther> GetGeneralQuestionnaireOtherResults(Guid depId)
        {
            var results = _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public List<QuestionnaireOther> GetDiabetesQuestionnaireOtherResults(Guid depId)
        {
            return _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
        }
        public List<QuestionnaireOther> GetHIVQuestionnaireOtherResults(Guid depId)
        {
            return _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "HIVPR" || x.programType.ToLower() == "hiv" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
        }

        public ServiceResult InsertQuestionnaireOtherResults(QuestionnaireOther model)
        {
            _context.QuestionnaireOthers.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateQuestionnaireOtherResult(QuestionnaireOther model)
        {
            var old = _context.QuestionnaireOthers.Where(x => x.QuestionnaireOtherID == model.QuestionnaireOtherID).FirstOrDefault();

            old.occupation = model.occupation;
            old.shiftWorkCheck = model.shiftWorkCheck;
            old.shiftWorker = model.shiftWorker;
            old.lypohypertrophyCheck = model.lypohypertrophyCheck;
            old.lypohypertrophy = model.lypohypertrophy;
            old.lypohypertrophyDate = model.lypohypertrophyDate;
            old.recDrugs = model.recDrugs;
            old.recDrugsLastUsed = model.recDrugsLastUsed;
            old.depression = model.depression;
            old.depressionComment = model.depressionComment;
            old.TBScreen = model.TBScreen;
            old.TBScreenDate = model.TBScreenDate;
            old.TBScreenResult = model.TBScreenResult;
            old.tbDiagnosed = model.tbDiagnosed;
            old.tbTreatmentStartDate = model.tbTreatmentStartDate;
            old.tbTreatmentEndDate = model.tbTreatmentEndDate;
            old.Pregnant = model.Pregnant;
            old.EDD = model.EDD;
            old.TreatingDoctor = model.TreatingDoctor;
            old.isDoctorAware = model.isDoctorAware;
            old.hasKidneyInfection = model.hasKidneyInfection;
            //hcare-930
            old.frailCareCheck = model.frailCareCheck;
            old.frailCare = model.frailCare;
            old.frailCareComment = model.frailCareComment;
            //hcare-930
            old.bloodTestFrequency = model.bloodTestFrequency;
            old.bloodTestEffectiveDate = model.bloodTestEffectiveDate;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.generalComments = model.generalComments;
            old.programType = model.programType;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();

        }

        //============================================================================= general-other ===============================================================================//
        public List<QuestionnaireOther> GetOtherResults(Guid DependantID) //HCare-968
        {
            string sql = string.Format(@"SELECT *
                                        FROM QuestionnaireOther
                                        WHERE dependentID = '{0}'
                                        AND (COALESCE (occupation, '') <> ''
                                        OR shiftWorkCheck = 1
                                        OR lypohypertrophy = 1
                                        OR recDrugs = 1
                                        OR hasKidneyInfection = 1
                                        OR frailCareCheck = 1
                                        OR TBScreen = 1
                                        OR COALESCE (bloodTestFrequency, '') <> ''
                                        OR followUp = 1)
                                        AND active = 1", DependantID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = db.Query<QuestionnaireOther>(sql, null, commandTimeout: 500).ToList();
                db.Close();
                foreach (var res in results)
                {
                    res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
                }
                return results;
            }
        }

        //================================================================================ pregnancy ================================================================================//
        public List<QuestionnaireOther> GetPregnancyResults(Guid depId) //HCare-968
        {
            return _context.QuestionnaireOthers.Where(x => x.dependentID == depId).Where(x => x.Pregnant == true).Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        }

        //================================================================================= new-born =================================================================================//

        public NewBorn GetNewBornResultById(int id)
        {
            return _context.NewBorns.Where(x => x.Id == id).FirstOrDefault();
        }
        public List<NewBorn> GetNewBornResults(Guid depId)
        {
            var results = _context.NewBorns.Where(x => x.DependantID == depId).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();
            foreach (var res in results)
            {
                res.Program = _context.Program.Where(x => x.code == res.Program).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertNewBornResults(NewBorn model)
        {
            _context.NewBorns.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateNewBornResult(NewBorn model)
        {
            var old = _context.NewBorns.Where(x => x.Id == model.Id).FirstOrDefault();

            old.hasConcern = model.hasConcern;
            old.Concern = model.Concern;
            old.isBreastfeeding = model.isBreastfeeding;
            old.Breastfeeding = model.Breastfeeding;
            old.isOnMedication = model.isOnMedication;
            old.Medication = model.Medication;
            old.hivTest = model.hivTest; //hcare-1451
            old.HIVTestComment = model.HIVTestComment; //hcare-1451
            old.hivResults = model.hivResults; //hcare-1451
            old.HIVResultsComment = model.HIVResultsComment; //hcare-1451
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Program = model.Program;
            old.GeneralComments = model.GeneralComments;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;

            return _context.SaveChanges();

        }






        //================================================================================ Diagnosis ================================================================================//

        public PatientDiagnosis GetDiagnosisById(int id)
        {
            return _context.PatientDiagnoses.Where(x => x.Id == id).FirstOrDefault();
        }
        public List<PatientDiagnosis> GetDiagnosisPrograms()
        {
            return _context.PatientDiagnoses.Where(x => x.Active == true).OrderBy(x => x.ProgramDescription).ToList();
        }
        public List<PatientDiagnosis> GetDiagnosisResults(Guid depId)
        {
            return _context.PatientDiagnoses.Where(x => x.DependantID == depId).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();
        }
        public List<PatientDiagnosis> GetDiabetesDiagnosisResults(Guid depId)
        {
            var results = _context.PatientDiagnoses.Where(x => x.DependantID == depId).Where(x => x.Active == true).Where(x => x.ProgramCode.ToUpper() == "DIABD" || x.ProgramCode == null).OrderByDescending(x => x.CreatedDate).ToList();
            return results;
        }
        public List<PatientDiagnosis> GetHIVDiagnosisResults(Guid depId)
        {
            var results = _context.PatientDiagnoses.Where(x => x.DependantID == depId).Where(x => x.Active == true).Where(x => x.ProgramCode.ToUpper() == "HIVPR" || x.ProgramCode == null).OrderByDescending(x => x.CreatedDate).ToList();
            return results;
        }

        public ServiceResult InsertDiagnosisResults(PatientDiagnosis model)
        {
            _context.PatientDiagnoses.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateDiagnosisResult(PatientDiagnosis model)
        {
            var old = _context.PatientDiagnoses.Where(x => x.Id == model.Id).FirstOrDefault();

            old.EffectiveDate = model.EffectiveDate;
            old.ProgramCode = model.ProgramCode;
            old.ProgramDescription = model.ProgramDescription;
            old.ICD10Code = model.ICD10Code;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Comment = model.Comment;
            old.Medication = model.Medication; //hcare-863
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public PatientDiagnosis GetPatientDiagnosisResult(Guid dependantID, string programCode) //hcare-863
        {
            var result = _context.PatientDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == programCode).Where(x => x.Active == true).OrderByDescending(x => x.EffectiveDate).FirstOrDefault();
            return result;

        }

        //================================================================================= Vision =================================================================================//

        public Vision GetVisionResultById(int id)
        {
            return _context.Visions.Where(x => x.VisionID == id).FirstOrDefault();
        }
        public List<Vision> GetVisionResults(Guid depId)
        {
            var results = _context.Visions.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertVisionResults(Vision model)
        {
            _context.Visions.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateVisionResult(Vision model)
        {
            var old = _context.Visions.Where(x => x.VisionID == model.VisionID).FirstOrDefault();

            old.vision = model.vision;
            old.visionCheck = model.visionCheck;
            old.eyeTreatment = model.eyeTreatment;
            old.eyeTreatmentCheck = model.eyeTreatmentCheck;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.programType = model.programType; //HCare-607
            old.generalComments = model.generalComments;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();

        }

        //================================================================================ Podiatry ================================================================================//

        public Podiatry GetPodiatryResultById(int id)
        {
            return _context.Podiatries.Where(x => x.PodiatryID == id).FirstOrDefault();
        }
        public List<Podiatry> GetPodiatryResults(Guid depId)
        {
            var results = _context.Podiatries.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertPodiatryResults(Podiatry model)
        {
            _context.Podiatries.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdatePodiatryResult(Podiatry model)
        {
            var old = _context.Podiatries.Where(x => x.PodiatryID == model.PodiatryID).FirstOrDefault();

            old.amputationCheck = model.amputationCheck; //HCare-753
            old.amputationComment = model.amputationComment;
            old.amputationReason = model.amputationReason;
            old.podiatryLopsComment = model.podiatryLopsComment;
            old.podiatryDeformityComment = model.podiatryDeformityComment;
            old.podiatryPerArterialDiseaseComment = model.podiatryPerArterialDiseaseComment;
            old.podiatryWoundComment = model.podiatryWoundComment;
            old.WoundInfection = model.WoundInfection;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.programType = model.programType;
            old.generalComments = model.generalComments;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();

        }

        //============================================================================= AutoNeropathy =============================================================================//

        public AutoNeropathy GetAutoNeroResultById(int id)
        {
            return _context.AutoNeropathies.Where(x => x.AutoNeropathyID == id).FirstOrDefault();
        }
        public List<AutoNeropathy> GetAutoNeroResults(Guid depId)
        {
            var results = _context.AutoNeropathies.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertAutoNeroResults(AutoNeropathy model)
        {
            _context.AutoNeropathies.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateAutoNeroResult(AutoNeropathy model)
        {
            var old = _context.AutoNeropathies.Where(x => x.AutoNeropathyID == model.AutoNeropathyID).FirstOrDefault();

            old.autoNeuroSympComment = model.autoNeuroSympComment;
            old.programType = model.programType;
            old.generalComments = model.generalComments;
            old.followUp = model.followUp;
            old.active = model.active;


            return _context.SaveChanges();

        }


        //============================================================================= Hypoglycaemia =============================================================================//

        public Hypoglycaemia GetHypoglycaemiaResultById(int id)
        {
            return _context.Hypoglycaemias.Where(x => x.HypoglycaemiaID == id).FirstOrDefault();
        }
        public List<Hypoglycaemia> GetHypoglycaemiaResults(Guid depId)
        {
            var results = _context.Hypoglycaemias.Where(x => x.dependentID == depId).Where(x => x.active == true).Where(x => x.programType.ToUpper() == "DIABD" || x.programType.ToLower() == "diabetes" || x.programType == null).OrderByDescending(x => x.createdDate).ToList();
            foreach (var res in results)
            {
                res.programType = _context.Program.Where(x => x.code == res.programType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }
        public ServiceResult InsertHypoglycaemiaResults(Hypoglycaemia model)
        {
            _context.Hypoglycaemias.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateHypoglycaemiaResult(Hypoglycaemia model)
        {
            var old = _context.Hypoglycaemias.Where(x => x.HypoglycaemiaID == model.HypoglycaemiaID).FirstOrDefault();

            old.glucoseReading = model.glucoseReading;
            old.hypoglycaemiaComment = model.hypoglycaemiaComment;
            old.hypoglycaemiaSymptomsComment = model.hypoglycaemiaSymptomsComment;
            old.hypoglycaemiaAssistance = model.hypoglycaemiaAssistance;
            old.emergencyRoomVisits = model.emergencyRoomVisits;
            old.insulinUseCheck = model.insulinUseCheck;
            old.sulfonylureaUseCheck = model.sulfonylureaUseCheck;
            old.ckdStageCheck = model.ckdStageCheck;
            old.patientAgeCheck = model.patientAgeCheck;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.programType = model.programType; //HCare-607
            old.generalComments = model.generalComments;
            old.followUp = model.followUp;
            old.active = model.active;

            return _context.SaveChanges();

        }




        //========================================================================= medical-scheme-settings =========================================================================//


        public List<MedicalAid> GetMedicalAidList()
        {
            return _context.MedicalAids.OrderByDescending(x => x.Active == true).ThenByDescending(x => x.clCarrier == true).ThenBy(x => x.Name).ToList();
        }

        public List<MedicalAidVM> GetMedicalAidPlansList()
        {
            var results = (from mp in _context.MedicalAidPlans
                           join ma in _context.MedicalAids on mp.medicalAidId equals ma.MedicalAidID
                           select new MedicalAidVM
                           {
                               Id = mp.Id,
                               MedicalAidId = mp.medicalAidId,
                               MedicalAidName = ma.Name,
                               PlanCode = mp.planCode,
                               PlanName = mp.Name,
                               CreatedBy = mp.createdBy,
                               CreatedDate = mp.createdDate,
                               ModifiedBy = mp.modifiedBy,
                               ModifiedDate = mp.modifieddate,
                               Active = mp.active

                           }).OrderBy(x => x.MedicalAidName).ToList();

            return results;
        }

        public List<MedicalAidVM> GetMedicalAidPlanValidation()
        {
            var results = (from mp in _context.MedicalAidPlans
                           join ma in _context.MedicalAids on mp.medicalAidId equals ma.MedicalAidID
                           where mp.active == true
                           select new MedicalAidVM
                           {
                               Id = mp.Id,
                               MedicalAidId = mp.medicalAidId,
                               MedicalAidName = ma.Name,
                               PlanCode = mp.planCode,
                               PlanName = mp.Name,
                               CreatedBy = mp.createdBy,
                               CreatedDate = mp.createdDate,
                               ModifiedBy = mp.modifiedBy,
                               ModifiedDate = mp.modifieddate,
                               Active = mp.active

                           }).OrderBy(x => x.MedicalAidName).ToList();

            return results;
        }


        public List<MedicalAidVM> GetAllowedMedicalAidPlan() //hcare-1288
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();

            var medicalaidplans = (from mp in _context.MedicalAidPlans
                                   join ma in _context.MedicalAids on mp.medicalAidId equals ma.MedicalAidID
                                   select new MedicalAidVM
                                   {
                                       Id = mp.Id,
                                       MedicalAidId = mp.medicalAidId,
                                       MedicalAidName = ma.Name,
                                       PlanCode = mp.planCode,
                                       PlanName = mp.Name,
                                       CreatedBy = mp.createdBy,
                                       CreatedDate = mp.createdDate,
                                       ModifiedBy = mp.modifiedBy,
                                       ModifiedDate = mp.modifieddate,
                                       Active = mp.active

                                   }).OrderBy(x => x.MedicalAidName).ToList();

            var results = (from r in medicalaidplans
                           where medaidlist.Contains(r.MedicalAidId)
                           select r).ToList();

            return results;
        }

        public MedicalAid GetMedicalAidById(Guid id)
        {
            return _context.MedicalAids.Where(x => x.MedicalAidID == id).FirstOrDefault();
        }

        public MedicalAidPlans GetMedicalAidPlanById(Guid id)
        {
            return _context.MedicalAidPlans.Where(x => x.Id == id).FirstOrDefault();
        }

        public List<MedicalAidVM> GetMedicalAidPlanDetails(Guid id)
        {
            var results = (from mp in _context.MedicalAidPlans
                           join ma in _context.MedicalAids on mp.medicalAidId equals ma.MedicalAidID
                           where mp.Id == id

                           select new MedicalAidVM
                           {
                               Id = mp.Id,
                               MedicalAidId = mp.medicalAidId,
                               MedicalAidName = ma.Name,
                               PlanCode = mp.planCode,
                               PlanName = mp.Name,
                               CreatedBy = mp.createdBy,
                               CreatedDate = mp.createdDate,
                               ModifiedBy = mp.modifiedBy,
                               ModifiedDate = mp.modifieddate,
                               Active = mp.active

                           }).OrderBy(x => x.MedicalAidName).ToList();

            return results;
        }

        public MedicalAidVM GetMedicalAidPlanDetail(Guid id)
        {
            var results = (from mp in _context.MedicalAidPlans
                           join ma in _context.MedicalAids on mp.medicalAidId equals ma.MedicalAidID
                           where mp.Id == id

                           select new MedicalAidVM
                           {
                               Id = mp.Id,
                               MedicalAidId = mp.medicalAidId,
                               MedicalAidName = ma.Name,
                               PlanCode = mp.planCode,
                               PlanName = mp.Name,
                               CreatedBy = mp.createdBy,
                               CreatedDate = mp.createdDate,
                               ModifiedBy = mp.modifiedBy,
                               ModifiedDate = mp.modifieddate,
                               Active = mp.active

                           }).OrderBy(x => x.MedicalAidName).FirstOrDefault();

            return results;
        }

        public ServiceResult InsertSchemeOption(MedicalAidPlans model)
        {
            _context.MedicalAidPlans.Add(model);
            return _context.SaveChanges();

        }

        public ServiceResult UpdateSchemeOption(MedicalAidVM model)
        {
            var old = _context.MedicalAidPlans.Where(x => x.Id == model.Id).FirstOrDefault();

            old.medicalAidId = model.MedicalAidId;
            old.planCode = model.PlanCode;
            old.Name = model.PlanName;
            old.modifiedBy = model.ModifiedBy;
            old.modifieddate = model.ModifiedDate;
            old.active = model.Active;

            return _context.SaveChanges();
        }

        #region HCare-1061
        public ServiceResult InsertDiabeticDairy(DiabeticDairy model)
        {
            _context.DiabeticDairy.Add(model);
            return _context.SaveChanges();
        }
        public List<DiabeticDairy> GetDiabeticDairy()
        {
            return _context.DiabeticDairy.ToList();
        }
        public ServiceResult UpdateDiabeticDairy(DiabeticDairy model)
        {
            var old = _context.DiabeticDairy.Where(x => x.dairyId == model.dairyId).FirstOrDefault();
            old.notes = model.notes;
            old.sentDate = model.sentDate;
            old.returnedDate = model.returnedDate;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.SingedEducationalNote = model.SingedEducationalNote;

            return _context.SaveChanges();
        }
        #endregion

        #region doctor-referral hcare-1136
        public ServiceResult InsertDoctorReferral(DoctorReferral model)
        {
            _context.DoctorReferrals.Add(model);
            return _context.SaveChanges();
        }

        public DoctorReferral GetDoctorReferralById(int id)
        {
            var result = _context.DoctorReferrals.Where(x => x.Id == id).FirstOrDefault();
            result.Program = _context.Program.Where(x => x.code == result.Program).Select(x => x.ProgramName).FirstOrDefault();
            return result;
        }

        public ServiceResult UpdateDoctorReferral(DoctorReferral model)
        {
            var old = _context.DoctorReferrals.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.ReferralDate = model.ReferralDate;
            old.Comment = model.Comment;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;
            old.referralFrom = model.referralFrom;
            return _context.SaveChanges();
        }

        public List<DoctorReferral> GetDoctorReferralResults(Guid dependentid)
        {
            var results = _context.DoctorReferrals.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).ToList();
            foreach (var res in results)
            {
                res.Program = _context.Program.Where(x => x.code == res.Program).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;


        }

        #endregion

        #region pathology-types hcare-970
        public ServiceResult InsertPathologyType(PathologyTypes model)
        {
            _context.PathologyTypes.Add(model);
            return _context.SaveChanges();
        }

        public PathologyTypes GetPathologyTypeById(string id)
        {
            return _context.PathologyTypes.Where(x => x.id == id).FirstOrDefault();
        }

        public ServiceResult UpdatePathologyType(PathologyTypes model)
        {
            var old = _context.PathologyTypes.Where(x => x.id == model.id).FirstOrDefault();

            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.pathologyType = model.pathologyType;
            old.active = model.active;
            return _context.SaveChanges();
        }
        public List<PathologyTypes> GetPathologyTypes()
        {
            return _context.PathologyTypes.Where(x => x.active == true).ToList();
        }

        public List<PathologyTypes> GetPathologyTypeList()
        {
            return _context.PathologyTypes.OrderByDescending(x => x.active == true).ThenBy(x => x.pathologyType).ToList();

        }

        public List<PathologyTypes> GetPathologyTypeValidation()
        {
            return _context.PathologyTypes.Where(x => x.active == true).ToList(); //hcare-1298
        }

        public PathologyTypes GetPathologyTypeByCode(string code)
        {
            return _context.PathologyTypes.Where(x => x.id == code).FirstOrDefault();
        }

        public PathologyTypes GetPathologyTypeByName(string name)
        {
            return _context.PathologyTypes.Where(x => x.pathologyType == name).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        #endregion

        #region Diabetes Care Plan hcare-1092
        public ServiceResult InsertCarePathology(CarePlanPathology model)
        {
            _context.CarePlanPathologies.Add(model);

            var res = _context.SaveChanges();
            return res;
        }
        public List<CarePlanPathology> GetCarePathology(Guid depId, Guid? pro)
        {
            var results = (from x in _context.CarePlanPathologies
                           where x.dependentID == depId
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public CarePlanPathology GetCarePathologyByID(int id)
        {
            return _context.CarePlanPathologies.Where(x => x.PathologyID == id).FirstOrDefault();
        }

        public ServiceResult UpdateCarePlanPathology(CarePlanPathology model)
        {

            var old = _context.CarePlanPathologies.Where(x => x.PathologyID == model.PathologyID).FirstOrDefault();
            old.Average_glucose_readings = model.Average_glucose_readings;
            old.generalComments = model.generalComments;
            old.HomeGlucoseTesting = model.HomeGlucoseTesting;
            old.Hypoglycemia = model.Hypoglycemia;
            old.Glucose_diary = model.Glucose_diary;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;

            return _context.SaveChanges();

        }
        public List<ClinicalAddition> GetClinicalAdditionByClinicalID(Guid depId, Guid? pro)
        {
            var results = (from x in _context.clinicalAdditions
                           where x.dependentID == depId
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public ServiceResult InsertClinicalAddition(ClinicalAddition model)
        {
            _context.clinicalAdditions.Add(model);

            var res = _context.SaveChanges();
            return res;
        }
        public ServiceResult UpdateClinicalAddition(ClinicalAddition model)
        {
            var old = _context.clinicalAdditions.Where(x => x.clinicalAdditionID == model.clinicalAdditionID).FirstOrDefault();
            old.hospitalisations = model.hospitalisations;
            old.newConditionDiagnosed = model.newConditionDiagnosed;
            old.immunisation = model.immunisation;
            old.generalComments = model.generalComments;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = DateTime.Now;

            return _context.SaveChanges();
        }
        public ClinicalAddition GetClinicalAdditionByID(int id)
        {
            return _context.clinicalAdditions.Where(x => x.clinicalAdditionID == id).FirstOrDefault();
        }

        public List<NutritionAndLifestyle> GetNutritionandlifestyleByDependentID(Guid depId, Guid? pro)
        {
            var results = (from x in _context.nutritionAndLifestyles
                           where x.dependentID == depId
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public NutritionAndLifestyle GetNutritionAndLifestyleByID(int id)
        {
            return _context.nutritionAndLifestyles.Where(x => x.nutritionAndLifestyleID == id).FirstOrDefault();
        }

        public ServiceResult InsertNutritionAndLifestyle(NutritionAndLifestyle model)
        {
            _context.nutritionAndLifestyles.Add(model);

            var res = _context.SaveChanges();
            return res;
        }
        public ServiceResult UpdateNutritionAndLifestyle(NutritionAndLifestyle model)
        {
            var old = _context.nutritionAndLifestyles.Where(x => x.nutritionAndLifestyleID == model.nutritionAndLifestyleID).FirstOrDefault();
            old.dietHistory = model.dietHistory;
            old.healthyEatingHabits = model.healthyEatingHabits;
            old.exerciseAndActivity = model.exerciseAndActivity;
            old.weightManagementAddressed = model.weightManagementAddressed;
            old.generalComments = model.generalComments;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;
            return _context.SaveChanges();
        }

        public List<Visit> GetVisitByDependentID(Guid depId, Guid? pro)
        {
            var results = (from x in _context.visits
                           where x.dependentID == depId
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public ServiceResult InsertVisit(Visit model)
        {
            _context.visits.Add(model);

            var res = _context.SaveChanges();
            return res;
        }
        public ServiceResult UpdateVisit(Visit model)
        {
            var old = _context.visits.Where(x => x.visitID == model.visitID).FirstOrDefault();
            old.FollowUpDoctor = model.FollowUpDoctor;
            old.FollowUpClinic = model.FollowUpClinic;
            old.FollowUpHaloCare = model.FollowUpHaloCare;
            old.OtherService = model.OtherService;
            old.generalComments = model.generalComments;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;
            return _context.SaveChanges();
        }
        public Visit GetVisitByID(int id)
        {
            return _context.visits.Where(x => x.visitID == id).FirstOrDefault();
        }

        public ServiceResult InsertMedicine(Medicine model)
        {

            _context.medicines.Add(model);
            var res = _context.SaveChanges();
            return res;
        }
        public ServiceResult UpdateMedicines(Medicine model)
        {
            var old = _context.medicines.Where(x => x.MedicineID == model.MedicineID).FirstOrDefault();
            old.Adherence = model.Adherence;
            old.ChangeofRegime = model.ChangeofRegime;
            old.PatientsOnInsulin = model.PatientsOnInsulin;
            old.generalComments = model.generalComments;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;

            return _context.SaveChanges();
        }
        public List<Medicine> GetMedicinesByDependentID(Guid deptID, Guid? pro)
        {
            var results = (from x in _context.medicines
                           where x.dependentID == deptID
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public Medicine GetMedicineByID(int id)
        {

            return _context.medicines.Where(x => x.MedicineID == id).FirstOrDefault();
        }


        public ServiceResult Insertpaediatric(paediatric model)
        {

            _context.paediatrics.Add(model);
            var res = _context.SaveChanges();
            return res;
        }
        public ServiceResult UpdatePaediatrict(paediatric model)
        {
            var old = _context.paediatrics.Where(x => x.paediatricID == model.paediatricID).FirstOrDefault();
            old.currentWeight = model.currentWeight;
            old.RxDoseIn_lineWithWeight = model.RxDoseIn_lineWithWeight;
            old.syrupVsTablets_ableToSwallow = model.syrupVsTablets_ableToSwallow;
            old.statusAwareness = model.statusAwareness;
            old.generalComments = model.generalComments;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;

            return _context.SaveChanges();
        }
        public List<paediatric> GetPaediatricByDependentID(Guid depId, Guid? pro)
        {
            var results = (from x in _context.paediatrics
                           where x.dependentID == depId
                           && x.programId == pro
                           orderby x.createdDate descending
                           select x).Take(2);

            return results.ToList();
        }
        public paediatric GetPaediatricByID(int id)
        {

            return _context.paediatrics.Where(x => x.paediatricID == id).FirstOrDefault();
        }

        #endregion

        #region hCare-1014
        public string GetPatientStagingHistoryByDependant(Guid dependantID)
        {
            var result = _context.PatientStagingHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.stageCode).Take(1).SingleOrDefault();//HCare-1014
            return result;
        }
        //This method will only display the manual added record
        public PatientStagingHistory GetPatientStagingHistoryById(int id)
        {

            var results = _context.PatientStagingHistory.Where(x => x.id == id).SingleOrDefault();

            return results;
        }
        #endregion

        //HCare-1196
        public IEnumerable<PatientStagingHistoryViewModel> GetPatientStagingAndReasonsHistorybyDependentId(Guid dep)
        {
            var results = new List<PatientStagingHistoryViewModel>();

            var res = (from p in _context.PatientRiskRatingHistory
                       join r in _context.PatientStagingHistory on p.patientStagingHistoryId equals r.id
                       where (p.dependantID == dep && p.active == true)
                       select new PatientStagingHistoryViewModel
                       {
                           id = r.id,
                           effectiveDate = p.effectiveDate,
                           HivStaging = r.stageCode,
                           Reason = p.reason,
                           CreatedBy = r.createdBy,
                           StagingFk = p.id,
                           RiskingActive = p.active
                       }).ToList().OrderByDescending(x => x.effectiveDate)

                       .Concat(
                                (from p in _context.PatientStagingHistory
                                 where p.dependantId == dep && p.active == true && p.createdBy != "Rating App"
                                 select new PatientStagingHistoryViewModel
                                 {
                                     id = p.id,
                                     effectiveDate = p.effectiveDate,
                                     HivStaging = p.stageCode,
                                     Reason = p.comment,
                                     CreatedBy = p.createdBy,
                                     StagingFk = 0,
                                     RiskingActive = p.active
                                 }).ToList().OrderByDescending(x => x.effectiveDate)
                                );


            //string sql = String.Format(@"SET DATEFORMAT YMD   
            //SELECT id[id],dependantId[dependentID], stageCode[HivStaging],createdDate[effectiveDate],reason[Reason],createdBy[CreatedBy] 
            //                                    FROM (SELECT  ps.id,ps.dependantId, ps.stageCode,ps.createdDate,ps.createdBy,
            //(SELECT TOP 1 reason FROM PatientRiskRatingHistory WHERE dependantID = ps.dependantId AND createdDate LIKE ps.createdDate ) AS reason,
            //                                    ROW_NUMBER() OVER(PARTITION BY ps.id  order by ps. createdDate) AS RowNumber
            //                                 FROM PatientStagingHistory ps
            //                                    INNER JOIN PatientRiskRatingHistory pr on ps.dependantId = pr.dependantID
            //                                 WHERE ps.dependantId = '{0}'  AND pr.active = 1 AND ps.active = 1
            //                                    AND pr.programType = 'HIVPR'
            //                                 ) AS a
            //                                 WHERE a.RowNumber = 1", dep);

            //using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            //{
            //    db.Open();
            //    results = db.Query<PatientStagingHistoryViewModel>(sql, null, commandTimeout: 10000).OrderByDescending(x => x.effectiveDate).Take(3).ToList();
            //    db.Close();
            //}


            return res;

        }

        //Hcare-1241
        public List<Hba1cRangeHistory> GetHba1cRangeHistory(Guid dependantID)
        {
            return _context.Hba1cRangeHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.effectiveDate).ToList();//HCare-1014
        }

        #region hcare- 1119
        public List<CommunicationLogVM> getCommunicationLog(Guid dependentID, Guid Pro)
        {
            IEnumerable<CommunicationLogVM> list = new List<CommunicationLogVM>();
            IEnumerable<CommunicationLogVM> tempList = new List<CommunicationLogVM>();


            var results = list.Concat((from s in _context.SmsMessages
                                       join t in _context.SmsMessageTemplates on s.templateID equals t.templateID
                                       where s.dependantID == dependentID && s.importSMSSubject == null
                                       && s.programId == Pro
                                       orderby s.effectiveDate descending
                                       select new CommunicationLogVM
                                       {
                                           Detail = s.message,
                                           description = t.template,
                                           DateSent = s.effectiveDate,
                                           TypeOfCommunication = "SMS",
                                           CommunicationSentTo = s.cell_no
                                       })).Concat((from s in _context.SmsMessages
                                                   where s.importSMSSubject != null && s.dependantID == dependentID
                                                   && s.programId == Pro
                                                   orderby s.effectiveDate descending
                                                   select new CommunicationLogVM
                                                   {
                                                       Detail = s.message,
                                                       description = s.importSMSSubject,
                                                       DateSent = s.effectiveDate,
                                                       TypeOfCommunication = "SMS",
                                                       CommunicationSentTo = s.cell_no
                                                   })).
                                      Concat((from e in _context.Emails
                                              where e.dependantID == dependentID && e.programId == Pro
                                              orderby e.effectivedate descending
                                              select new CommunicationLogVM
                                              {
                                                  Detail = e.emailMassage,
                                                  description = e.subject,
                                                  TypeOfCommunication = "Email",
                                                  DateSent = e.effectivedate,
                                                  CommunicationSentTo = e.emailTo
                                              })).
                                      Concat((from q in _context.Queries
                                              join qt in _context.EnquiryTypes on q.queryType equals qt.enquiryType
                                              where q.dependentID == dependentID && q.programId == Pro
                                              orderby q.effectiveDate descending
                                              select new CommunicationLogVM
                                              {
                                                  Detail = q.query,
                                                  description = qt.enquiryName,
                                                  TypeOfCommunication = "Enquiries",
                                                  DateSent = q.effectiveDate,
                                                  CommunicationSentTo = ""

                                              })).
                                 Concat((from n in _context.DiabeticDairy
                                         where n.dependentID == dependentID && n.ImportSubject == null
                                         && n.programId == Pro
                                         orderby n.sentDate descending
                                         select new CommunicationLogVM
                                         {
                                             Detail = n.notes,
                                             description = "Diabetic diary",
                                             TypeOfCommunication = "Diabetic diary",
                                             DateSent = n.sentDate,
                                             CommunicationSentTo = ""

                                         })).
                                       Concat((from n in _context.DiabeticDairy
                                               where n.dependentID == dependentID && n.ImportSubject != null
                                               && n.programId == Pro
                                               orderby n.sentDate descending
                                               select new CommunicationLogVM
                                               {
                                                   Detail = n.notes,
                                                   description = n.ImportSubject,
                                                   TypeOfCommunication = "Diabetic diary",
                                                   DateSent = n.sentDate,
                                                   CommunicationSentTo = ""

                                               })).

                                      Concat((from n in _context.Notes
                                              join nt in _context.NoteTypes on n.noteType equals nt.noteType
                                              where n.dependentID == dependentID && n.importNotesSubjet == null
                                              && n.programId == Pro
                                              orderby n.effectiveDate descending
                                              select new CommunicationLogVM
                                              {
                                                  Detail = n.note,
                                                  description = nt.noteDescription,
                                                  TypeOfCommunication = "Notes",
                                                  DateSent = n.effectiveDate,
                                                  CommunicationSentTo = ""

                                              })).
                                           Concat((from n in _context.Notes
                                                   where n.dependentID == dependentID && n.importNotesSubjet != null
                                                   && n.programId == Pro
                                                   orderby n.effectiveDate descending
                                                   select new CommunicationLogVM
                                                   {

                                                       Detail = n.note,
                                                       description = n.importNotesSubjet,
                                                       TypeOfCommunication = "Notes",
                                                       DateSent = n.effectiveDate,
                                                       CommunicationSentTo = ""

                                                   })).

                                        Concat((from n in _context.Attachments
                                                join nt in _context.AttachmentTypes on n.attachmentType equals nt.attachmentType
                                                where n.dependentID == dependentID
                                                && n.programId == Pro
                                                orderby n.createdDate descending
                                                select new CommunicationLogVM
                                                {

                                                    Detail = n.attachmentName,
                                                    description = nt.typeDescription,
                                                    TypeOfCommunication = "Atttachment",
                                                    DateSent = n.createdDate,
                                                    CommunicationSentTo = ""

                                                }));
            //HCare-1119

            string sql = String.Format(@"SET DATEFORMAT YMD 
									   SELECT s.message[Detail],'No description'[description],s.effectiveDate[DateSent],
                                                'SMS'[TypeOfCommunication], s.cell_no[CommunicationSentTo] FROM  SmsMessages s 
                                                WHERE (s.templateID = 0 and s.dependantID = '{0}' and [importSMSSubject] is null and s.programId = '{1}')", dependentID, Pro);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                tempList = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 10000).ToList();
                db.Close();
            }
            var returnList = results.Concat(tempList);

            return returnList.ToList();

        }
        public bool GetMemberBydependentIDAndMemberNumber(Guid depID, string memberNumber)
        {
            var res = (from m in _context.Members
                       join d in _context.Dependants on m.memberID equals d.memberID
                       where m.membershipNo == memberNumber && d.DependantID == depID
                       select new { m.membershipNo }).Any();
            return res;
        }
        public bool ImportCommunication(ImportCommunicationModel importCommunicationModel)
        {
            var results = false;
            var Executable = false;

            var time = DateTime.Now.TimeOfDay;
            var modifiedDate = importCommunicationModel.DateSent.Add(time); //HCare-1488
            importCommunicationModel.DateSent = modifiedDate;

            if (importCommunicationModel.Type.ToLower().Contains("email"))
            {

                var email = new Emails
                {
                    createdDate = importCommunicationModel.DateSent,
                    subject = importCommunicationModel.Description,
                    emailMassage = importCommunicationModel.Details,
                    createdBy = "HCare bulk upload",
                    programId = importCommunicationModel.ProgramId,
                    dependantID = importCommunicationModel.DepedentID,
                    effectivedate = importCommunicationModel.DateSent,
                    status = "Sent",
                    emailTo = importCommunicationModel.messageTo



                };

                Executable = true;
                if (!_context.Emails.Where(x => x.effectivedate.Date == email.effectivedate.Date && x.subject == email.subject &&
               x.emailMassage == email.emailMassage && x.programId == email.programId && x.dependantID == email.dependantID).Any())
                {
                    _context.Emails.Add(email);
                    var res = _context.SaveChanges();
                    results = res.Success;
                }

            }
            if (importCommunicationModel.Type.ToLower().Contains("sms"))
            {
                var sms = new SmsMessages
                {
                    createdDate = DateTime.Now,
                    importSMSSubject = importCommunicationModel.Description,
                    message = importCommunicationModel.Details,
                    createdBy = "HCare bulk upload",
                    programId = importCommunicationModel.ProgramId,
                    dependantID = importCommunicationModel.DepedentID,
                    status = "Sent",
                    effectiveDate = importCommunicationModel.DateSent,
                    cell_no = importCommunicationModel.messageTo,
                    Active = true

                };


                Executable = true;

                if (!_context.SmsMessages.Where(x => x.effectiveDate.Date == sms.effectiveDate.Date && x.importSMSSubject == sms.importSMSSubject &&
                x.message == sms.message && x.programId == sms.programId && x.dependantID == sms.dependantID).Any())
                {
                    _context.SmsMessages.Add(sms);
                    var res = _context.SaveChanges();
                    results = res.Success;
                }

            }
            if (importCommunicationModel.Type.ToLower().Contains("notes"))
            {
                var notes = new Notes
                {
                    effectiveDate = importCommunicationModel.DateSent,
                    note = importCommunicationModel.Details,
                    importNotesSubjet = importCommunicationModel.Description,
                    createdBy = "HCare bulk upload",
                    programId = importCommunicationModel.ProgramId,
                    createdDate = DateTime.Now,
                    dependentID = importCommunicationModel.DepedentID,
                    noteType = importCommunicationModel.NoteType,
                    Active = true

                };

                Executable = true;

                if (!_context.Notes.Where(x => x.effectiveDate.Date == notes.effectiveDate.Date && x.note == notes.note && x.importNotesSubjet == notes.importNotesSubjet && x.programId == notes.programId && x.dependentID == notes.dependentID).Any())
                {
                    _context.Notes.Add(notes);
                    var res = _context.SaveChanges();
                    results = res.Success;

                }


            }
            if (importCommunicationModel.Type.ToLower().Contains("diabetic diary"))
            {
                var diabetic = new DiabeticDairy
                {
                    createdDate = DateTime.Now,
                    ImportSubject = importCommunicationModel.Description,
                    notes = importCommunicationModel.Details,
                    createdBy = "HCare bulk upload",
                    programId = importCommunicationModel.ProgramId,
                    dependentID = importCommunicationModel.DepedentID,
                    sentDate = importCommunicationModel.DateSent,


                };

                Executable = true;

                if (!_context.DiabeticDairy.Where(x => x.sentDate.Date == diabetic.sentDate.Date && x.ImportSubject == diabetic.ImportSubject &&
                x.notes == diabetic.notes && x.programId == diabetic.programId && x.dependentID == diabetic.dependentID).Any())
                {
                    _context.DiabeticDairy.Add(diabetic);
                    var res = _context.SaveChanges();
                    results = res.Success;
                }

            }

            return Executable;
        }

        public List<CommunicationLogVM> getAllCommunicationLog(CommunicationLogVM model)
        {

            List<CommunicationLogVM> list = new List<CommunicationLogVM>();
            try
            {
                var medicalAid = new List<string>();

                if (!model.Scheme.Equals(""))
                    model.Scheme.Remove(0, 1);
                medicalAid = model.Scheme.Split(',').ToList();

                if (!string.IsNullOrEmpty(model.TypeOfCommunication))
                {
                    if (model.TypeOfCommunication.Contains("SMS"))
                    {
                        if (medicalAid.Count() == 1)
                        {
                            string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode], M.membershipNo[MemberNumber], D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														S.message[Detail], T.template[description],
														S.createdDate[DateSent],'SMS'[TypeOfCommunication],s.cell_no[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN SmsMessages S ON D.DependantID = S.dependantID
                                                        INNER JOIN SmsMessageTemplates T ON S.templateID = T.templateID
                                                        WHERE S.effectiveDate BETWEEN '{0}' AND '{1}'
                                                        ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                            // SMS with no template        //HCare-1119
                            string sql2 = string.Format(@"SET DATEFORMAT YMD
													    SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],d.idNumber[ProfileNumber]
                                                        ,ma.medicalAidCode[Scheme],
                                                        s.message[Detail],'No description'[description],s.createdDate[DateSent],
                                                        'SMS'[TypeOfCommunication],s.cell_no[CommunicationSentTo] FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.templateID = 0 AND s.[importSMSSubject] is null
                                                                AND S.effectiveDate BETWEEN '{0}' AND '{1}'
                                                                ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql2, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                            string sql3 = string.Format(@"SET DATEFORMAT YMD
											           SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],d.idNumber[ProfileNumber]
                                                        ,ma.medicalAidCode[Scheme],
                                                        s.message[Detail],s.importSMSSubject[description],s.effectiveDate[DateSent],
                                                        'SMS'[TypeOfCommunication],s.cell_no[CommunicationSentTo] FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.[importSMSSubject] is not null
                                                                AND S.effectiveDate BETWEEN '{0}' AND '{1}'
                                                                ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql3, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                        }
                        else
                        {
                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode], M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],S.message[Detail], 
														T.template[description],S.createdDate[DateSent],
														'SMS'[TypeOfCommunication],s.cell_no[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN SmsMessages S ON D.DependantID = S.dependantID
                                                        INNER JOIN SmsMessageTemplates T ON S.templateID = T.templateID
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}'
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                                // SMS with no template        //HCare-1119
                                string sql2 = string.Format(@"		SET DATEFORMAT YMD
                                                        SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],
														d.idNumber[ProfileNumber] ,ma.medicalAidCode[Scheme], 
														s.message[Detail],'No description'[description],s.createdDate[DateSent], 
														'SMS'[TypeOfCommunication], s.cell_no[CommunicationSentTo] 
                                                        FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.templateID = 0 
                                                        AND MA.medicalAidCode = '{0}' AND s.[importSMSSubject] is  null
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}'
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql2, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }

                                string sql3 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],
														d.idNumber[ProfileNumber] ,ma.medicalAidCode[Scheme], 
														s.message[Detail],s.[importSMSSubject][description],
														s.effectiveDate[DateSent],'SMS'[TypeOfCommunication],s.cell_no[CommunicationSentTo] 
                                                        FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.[importSMSSubject] is not null
                                                        AND MA.medicalAidCode = '{0}'
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}' and s.importSMSSubject is not null
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql3, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }

                        }

                    }
                    if (model.TypeOfCommunication.Contains("Email"))
                    {
                        if (medicalAid.Count() == 1)
                        {



                            string sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
															MA.medicalAidCode[Scheme],E.emailMassage[Detail],E.subject[description],
															'EMAIL'[TypeOfCommunication],E.effectivedate[DateSent],E.emailTo[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Emails E ON D.DependantID = E.dependantID
                                                            WHERE E.effectivedate BETWEEN '{0}' AND '{1}'", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                        }
                        else
                        {
                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
															D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],E.emailMassage[Detail],
															E.subject[description],'EMAIL'[TypeOfCommunication],E.effectivedate[DateSent],E.emailTo[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Emails E ON D.DependantID = E.dependantID
                                                            WHERE MA.medicalAidCode = '{0}'
                                                            AND E.effectivedate BETWEEN '{1}' AND '{2}'	", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }


                        }

                    }
                    if (model.TypeOfCommunication.Contains("Enquiries"))
                    {
                        if (medicalAid.Count() == 1)
                        {


                            string sql = string.Format(@"SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],Q.query[Detail],
														QT.enquiryName[description],'Enquiry'[TypeOfCommunication]
														,Q.effectivedate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Queries Q ON D.DependantID = Q.dependentID
                                                        INNER JOIN [EnquiryTypes] QT ON Q.queryType = QT.[enquiryType]
                                                        WHERE Q.effectivedate BETWEEN '{0}' AND '{1}'", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                        }
                        else
                        {
                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
															D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],Q.query[Detail],
															QT.enquiryName[description],'Enquiry'[TypeOfCommunication],
															Q.effectivedate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Queries Q ON D.DependantID = Q.dependentID
                                                            INNER JOIN [EnquiryTypes] QT ON Q.queryType = QT.[enquiryType]

                                                            WHERE MA.medicalAidCode = '{0}'
                                                            AND Q.effectivedate BETWEEN '{1}' AND '{2}'", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }
                        }

                    }
                    if (model.TypeOfCommunication.Contains("Diabetic diary"))
                    {
                        if (medicalAid.Count() == 1)
                        {
                            string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														dB.notes[Detail],'Diabetic diary'[description],'Diabetic diary'[TypeOfCommunication],
														dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE  dB.ImportSubject IS  NULL AND dB.sentDate BETWEEN '{0}' AND '{1}'", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                            string sql2 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														dB.notes[Detail],dB.ImportSubject[description],'Diabetic diary'[TypeOfCommunication],
														dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE dB.sentDate BETWEEN '{0}' AND '{1}' AND dB.ImportSubject IS NOT NULL ", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql2, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }
                        }
                        else
                        {

                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														dB.notes[Detail],'Diabetic diary'[description],'Diabetic diary'[TypeOfCommunication],
														dB.sentDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE  MA.medicalAidCode = '{0}' AND dB.ImportSubject IS NULL
                                                        AND dB.sentDate BETWEEN '{1}' AND '{2}'", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }

                                string sql2 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														dB.notes[Detail],dB.ImportSubject[description],
														'Diabetic diary'[TypeOfCommunication],dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE  MA.medicalAidCode = '{0}'
                                                        AND dB.sentDate BETWEEN '{1}' AND '{2}' and dB.ImportSubject is not null", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql2, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }

                        }

                    }

                    if (model.TypeOfCommunication.Contains("Notes"))
                    {
                        if (medicalAid.Count() == 1)
                        {
                            string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],N.note[Detail],
														NT.noteDescription  [description],'Notes'[TypeOfCommunication],
														N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        INNER JOIN NoteTypes NT ON N.noteType = NT.noteType
                                                        WHERE N.effectiveDate BETWEEN '{0}' AND '{1}' and [importNotesSubjet] is null", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                            string sql1 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],N.note[Detail],
														N.[importNotesSubjet][description],'Notes'[TypeOfCommunication],
														N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID

                                                        WHERE N.effectiveDate BETWEEN '{0}' AND '{1}' and [importNotesSubjet] is not null", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql1, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }
                        }
                        else
                        {
                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],N.note[Detail],NT.noteDescription  [description],'Notes'[TypeOfCommunication],
                                                        N.effectiveDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        INNER JOIN NoteTypes NT ON N.noteType = NT.noteType
                                                        WHERE MA.medicalAidCode = '{0}' 
                                                        AND N.effectiveDate BETWEEN '{1}' AND '{2}' and N.importNotesSubjet is  null", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }

                                string sql1 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],N.note[Detail],N.[importNotesSubjet][description],'Notes'[TypeOfCommunication],
                                                        N.effectiveDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        WHERE MA.medicalAidCode = '{0}' 
                                                        AND N.effectiveDate BETWEEN '{1}' AND '{2}' and N.importNotesSubjet is not null", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql1, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }
                        }

                    }
                    if (model.TypeOfCommunication.Contains("Attachments"))
                    {
                        if (medicalAid.Count() == 1)
                        {
                            string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],A.attachmentName[Detail],NT.typeDescription  [description],'Attachments'[TypeOfCommunication],
                                                        A.createdDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        INNER JOIN AttachmentTypes NT ON A.attachmentType = NT.attachmentType
                                                        WHERE A.createdDate BETWEEN '{0}' AND '{1}' and A.ImportAttachmentSubject is  null", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                            string sql1 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
                                                            A.attachmentName[Detail],A.[ImportAttachmentSubject][description],
                                                            'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        WHERE A.createdDate BETWEEN '{0}' AND '{1}' and A.ImportAttachmentSubject is not null", model.dateFrom, model.dateTo);

                            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db.Open();
                                var results = db.Query<CommunicationLogVM>(sql1, null, commandTimeout: 500).ToList();
                                db.Close();

                                list.AddRange(results);
                            }

                        }
                        else
                        {
                            foreach (var schem in medicalAid)
                            {
                                string sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],A.attachmentName[Detail],NT.typeDescription  [description],
                                                        'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo]  FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        INNER JOIN AttachmentTypes NT ON A.attachmentType = NT.attachmentType
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND A.createdDate BETWEEN '{1}' AND '{2}' and A.ImportAttachmentSubject is  null", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }

                                string sql1 = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],A.attachmentName[Detail],A.[ImportAttachmentSubject][description],
                                                        'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                      
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND A.createdDate BETWEEN '{1}' AND '{2}' and A.ImportAttachmentSubject is not null", schem, model.dateFrom, model.dateTo);

                                using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db.Open();
                                    var results = db.Query<CommunicationLogVM>(sql1, null, commandTimeout: 500).ToList();
                                    db.Close();

                                    list.AddRange(results);
                                }
                            }
                        }

                    }

                }
                else
                    if (medicalAid.Count() != 1)
                {
                    string sql = "";
                    foreach (var schem in medicalAid)
                    {
                        if (string.IsNullOrEmpty(schem))
                            continue;

                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode], M.membershipNo[MemberNumber], 
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],S.message[Detail],
														T.template[description],
                                                        S.createdDate[DateSent],'SMS'[TypeOfCommunication], S.cell_no[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN SmsMessages S ON D.DependantID = S.dependantID
                                                        INNER JOIN SmsMessageTemplates T ON S.templateID = T.templateID
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}'
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }

                        //sms with no templates        //HCare-1119
                        sql = string.Format(@"SET DATEFORMAT YMD
                                                      SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],
													  d.idNumber[ProfileNumber] ,ma.medicalAidCode[Scheme],
													  s.message[Detail],'No description'[description],s.createdDate[DateSent],
													  'SMS'[TypeOfCommunication],S.cell_no[CommunicationSentTo]
                                                        FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.templateID = 0 
                                                        AND MA.medicalAidCode = '{0}'
                                                        AND s.[importSMSSubject] is  null
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}'
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }
                        //IMPORT
                        sql = string.Format(@"		SET DATEFORMAT YMD
                                                      SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],
													  d.idNumber[ProfileNumber] ,ma.medicalAidCode[Scheme], 
													  s.message[Detail],s.[importSMSSubject][description],
													  s.effectiveDate[DateSent],  'SMS'[TypeOfCommunication], S.cell_no[CommunicationSentTo]
                                                        FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.[importSMSSubject] is not null
                                                        AND MA.medicalAidCode = '{0}'
                                                        AND S.effectiveDate BETWEEN '{1}' AND '{2}'
                                                        ORDER BY S.effectiveDate DESC", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }

                        sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
															D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
															E.emailMassage[Detail],E.subject[description],'EMAIL'[TypeOfCommunication],
															E.effectivedate[DateSent], E.emailTo[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Emails E ON D.DependantID = E.dependantID
                                                            WHERE MA.medicalAidCode = '{0}'
                                                            AND E.effectivedate BETWEEN '{1}' AND '{2}'", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }


                        sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                            D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],Q.query[Detail],QT.enquiryName[description],'Enquiry'[TypeOfCommunication],
                                                            Q.effectivedate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Queries Q ON D.DependantID = Q.dependentID
                                                            INNER JOIN [EnquiryTypes] QT ON Q.queryType = QT.[enquiryType]

                                                            WHERE MA.medicalAidCode = '{0}'
                                                            AND Q.effectivedate BETWEEN '{1}' AND '{2}'", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }




                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                         D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],dB.notes[Detail],'Diabetic diary'[description],
                                                        'Diabetic diary'[TypeOfCommunication],dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE  MA.medicalAidCode = '{0}'
                                                        AND dB.sentDate BETWEEN '{1}' AND '{2}' and dB.ImportSubject is null", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }
                        //import
                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],dB.notes[Detail],dB.ImportSubject[description],'Diabetic diary'[TypeOfCommunication],
                                                        dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE  MA.medicalAidCode = '{0}'
                                                        AND dB.sentDate BETWEEN '{1}' AND '{2}' and dB.ImportSubject is not null", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }


                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],N.note[Detail],NT.noteDescription  [description],'Notes'[TypeOfCommunication],
                                                        N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        INNER JOIN NoteTypes NT ON N.noteType = NT.noteType
                                                        WHERE MA.medicalAidCode = '{0}' 
                                                        AND N.effectiveDate BETWEEN '{1}' AND '{2}' and N.importNotesSubjet is null", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }

                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],N.note[Detail],N.[importNotesSubjet][description],
                                                        'Notes'[TypeOfCommunication],N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        WHERE MA.medicalAidCode = '{0}' 
                                                        AND N.effectiveDate BETWEEN '{1}' AND '{2}' and N.importNotesSubjet is not null ", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }


                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],A.attachmentName[Detail],NT.typeDescription[description],
                                                        'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        INNER JOIN AttachmentTypes NT ON A.attachmentType = NT.attachmentType
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND A.createdDate BETWEEN '{1}' AND '{2}' and A.[ImportAttachmentSubject] is null", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }
                        //import
                        sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],A.attachmentName[Detail],A.[ImportAttachmentSubject][description],'Attachments'[TypeOfCommunication],
                                                        A.createdDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        WHERE MA.medicalAidCode = '{0}'
                                                        AND A.createdDate BETWEEN '{1}' AND '{2}' and A.[ImportAttachmentSubject] is not null", schem, model.dateFrom, model.dateTo);

                        using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                        {
                            db.Open();
                            var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                            db.Close();

                            list.AddRange(results);
                        }

                    }
                }
                else
                {
                    string sql = "";
                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode], M.membershipNo[MemberNumber], D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],S.message[Detail], T.template[description],S.createdDate[DateSent],
                                                        'SMS'[TypeOfCommunication],S.cell_no[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN SmsMessages S ON D.DependantID = S.dependantID
                                                        INNER JOIN SmsMessageTemplates T ON S.templateID = T.templateID
                                                        WHERE S.effectiveDate BETWEEN '{0}' AND '{1}'
                                                        ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }

                    // SMS WITH NO TEMP        //HCare-1119

                    sql = string.Format(@"SET DATEFORMAT YMD
                                                       SELECT d.dependentCode[DepandantCode],m.membershipNo[MemberNumber],
													   d.idNumber[ProfileNumber] ,ma.medicalAidCode[Scheme], 
													   s.message[Detail],'No description'[description],s.createdDate[DateSent],
													   'SMS'[TypeOfCommunication],S.cell_no[CommunicationSentTo]
                                                        FROM Dependant d
                                                        INNER JOIN Member m on d.memberID = m.memberID
                                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                                                        INNER JOIN SmsMessages s on d.DependantID = s.dependantID
                                                        WHERE  s.templateID = 0
                                                        AND S.effectiveDate BETWEEN '{0}' AND '{1}' and S.[importSMSSubject] is null
                                                        ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }
                    //import
                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode], M.membershipNo[MemberNumber], 
														D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
														S.message[Detail], S.[importSMSSubject][description],
														S.effectiveDate[DateSent],'SMS'[TypeOfCommunication],S.cell_no[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN SmsMessages S ON D.DependantID = S.dependantID
                                                       
                                                        WHERE S.effectiveDate BETWEEN '{0}' AND '{1}' and S.[importSMSSubject] is not null
                                                        ORDER BY S.effectiveDate DESC", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }


                    sql = string.Format(@"SET DATEFORMAT YMD
                                                            SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
															D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],
															E.emailMassage[Detail],E.subject[description],
															'EMAIL'[TypeOfCommunication],E.effectivedate[DateSent],E.emailTo[CommunicationSentTo] FROM Dependant D
                                                            INNER JOIN Member M ON D.memberID = M.memberID
                                                            INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                            INNER JOIN Emails E ON D.DependantID = E.dependantID
                                                            WHERE E.effectivedate BETWEEN '{0}' AND '{1}'", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }





                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],Q.query[Detail],QT.enquiryName[description],
                                                        'Enquiry'[TypeOfCommunication],Q.effectivedate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Queries Q ON D.DependantID = Q.dependentID
                                                        INNER JOIN [EnquiryTypes] QT ON Q.queryType = QT.[enquiryType]

                                                        WHERE Q.effectivedate BETWEEN '{0}' AND '{1}'", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }



                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],dB.notes[Detail],'Diabetic diary'[description],
                                                        'Diabetic diary'[TypeOfCommunication],dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE dB.sentDate BETWEEN '{0}' AND '{1}' dB.[ImportSubject] is null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }
                    //import
                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],dB.notes[Detail],dB.[ImportSubject][description],'Diabetic diary'[TypeOfCommunication],
                                                        dB.sentDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN DiabeticDairy dB ON D.DependantID = dB.dependentID
                                                        WHERE dB.sentDate BETWEEN '{0}' AND '{1}' and dB.[ImportSubject] is not null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }




                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],N.note[Detail],NT.noteDescription  [description],
                                                        'Notes'[TypeOfCommunication],N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                        INNER JOIN NoteTypes NT ON N.noteType = NT.noteType
                                                        WHERE N.effectiveDate BETWEEN '{0}' AND '{1}' and N.[importNotesSubjet] is null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }

                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],D.idNumber[ProfileNumber],
                                                        MA.medicalAidCode[Scheme],N.note[Detail],N.importNotesSubjet  [description],'Notes'[TypeOfCommunication],
                                                        N.effectiveDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Notes N ON D.DependantID = N.dependentID
                                                       
                                                        WHERE N.effectiveDate BETWEEN '{0}' AND '{1}' and N.[importNotesSubjet] is not null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }

                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],A.attachmentName[Detail],NT.typeDescription  [description],
                                                        'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                        INNER JOIN AttachmentTypes NT ON A.attachmentType = NT.attachmentType
                                                        WHERE A.createdDate BETWEEN '{0}' AND '{1}' and A.[ImportAttachmentSubject] is null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }
                    //import
                    sql = string.Format(@"SET DATEFORMAT YMD
                                                        SELECT D.dependentCode[DepandantCode],M.membershipNo[MemberNumber],
                                                        D.idNumber[ProfileNumber],MA.medicalAidCode[Scheme],A.attachmentName[Detail],A.[ImportAttachmentSubject]  [description],
                                                        'Attachments'[TypeOfCommunication],A.createdDate[DateSent],''[CommunicationSentTo] FROM Dependant D
                                                        INNER JOIN Member M ON D.memberID = M.memberID
                                                        INNER JOIN MedicalAid MA ON M.medicalAidID = MA.MedicalAidID
                                                        INNER JOIN Attachments A ON D.DependantID = A.dependentID
                                                     
                                                        WHERE A.createdDate BETWEEN '{0}' AND '{1}' and A.[ImportAttachmentSubject] is not null", model.dateFrom, model.dateTo);

                    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db.Open();
                        var results = db.Query<CommunicationLogVM>(sql, null, commandTimeout: 500).ToList();
                        db.Close();

                        list.AddRange(results);
                    }

                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return list.OrderByDescending(x => x.DateSent).ToList();

        }


        public MedicalAid GetMedicalAidByName(string name)
        {
            return _context.MedicalAids.Where(x => x.medicalAidCode == name).FirstOrDefault();
        }
        #endregion

        #region HCare-1175
        public List<Referral> getReferral()
        {
            var result = _context.referrals.Where(x => x.active == true).ToList();

            return result;
        }

        public string getReferralByMedicalAid(Guid medAid)
        {
            List<string> list = new List<string>();

            var results = (from m in _context.MedicalAids
                           where m.MedicalAidID == medAid
                           select m.Referral
                           ).ToList();
            if (results[0] == null)
                results[0] = string.Empty;

            return results[0].ToString();

        }

        public string getReferralByDepandent(Guid? dep)
        {
            List<string> list = new List<string>();
            var results = (from d in _context.Dependants
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           where d.DependantID == dep
                           select ma.Referral
                           ).ToList();
            if (results[0] == null)
                results[0] = string.Empty;

            return results[0].ToString();

        }
        #endregion

        #region HCARE-1144
        public List<ReferralFrom> getReferralFrom()
        {
            var result = _context.referralFroms.Where(x => x.active == true).ToList();

            return result;
        }
        public string getReferralFromByDepandent(Guid? dep, Guid pro)
        {

            List<string> list = new List<string>();
            var results = (from d in _context.Dependants
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAidPrograms on m.medicalAidID.ToString() equals ma.medicalAidId
                           join p in _context.Program on ma.program equals p.code
                           where d.DependantID == dep && p.programID == pro
                           select ma.referralFrom
                           ).ToList();
            if (results[0] == null)
                results[0] = string.Empty;

            return results[0].ToString();


        }

        #endregion

        public List<RiskSearch> GetRiskRatingSearchResults(string medicalaid = "", string program = "", string riskrating = "", string fromDate = "", string toDate = "") //HCare-1138
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedMedicalAidList = rights.accessList.Select(x => x.medicalAidId).ToList();


            DateTime dtFrom = Convert.ToDateTime(fromDate);
            DateTime dtTo = Convert.ToDateTime(toDate);

            string sql1 = string.Format(@"SELECT pph.dependantId
                                        FROM Dependant d
                                        INNER JOIN Member m on d.memberID = m.memberID
                                        INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
            						    INNER JOIN PatientProgramHistory pph on d.DependantID = pph.dependantId 

                                        WHERE d.DependantID IN (SELECT rr.dependantID FROM PatientRiskRatingHistory rr where rr.active = 1 and rr.programType = '{1}')
                                        AND ma.Active = 1
                                        AND ma.MedicalAidID = '{0}'
            						    AND pph.programCode = '{1}'
                                        ", medicalaid, program);

            using (IDbConnection db1 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db1.Open();
                var programCheck = (List<RiskSearch>)db1.Query<RiskSearch>(sql1, null, commandTimeout: 5000);
                db1.Close();

                if (programCheck.Count != 0)
                {
                    string concatenation1 = string.Empty;
                    foreach (var item in programCheck)
                    {
                        concatenation1 += "'" + item.DependantID + "',";
                    }
                    string sql2 = string.Format(@"SET DATEFORMAT YMD
                                                    SELECT d.dependantId, ma.Name[MedicalScheme], pr.programID[ProgramID], pr.code, pr.ProgramName[Program], d.firstName + ' ' + d.lastName[PatientName], d.idNumber[IDNumber], m.membershipNo[MembershipNumber], 
                                                    d.dependentCode[DependantCode], d.birthDate[BirthDate], s.managementStatusCode as statusCode, ms.statusName[ManagementStatus], 
                                                    
													--#region gender
													(SELECT sexName from Sex where sex = (select sex from Dependant where dependantId = d.DependantID)) [Gender],
													--#endregion
													--#region program-icd10-code
													(SELECT(CASE 
													-->> CHECK 1 - does the history contain a date greater than today
													WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode))  
														THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode)
													-->> CHECK 2 - does the history contain a date less than today
													WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode  = ms.programCode))  
														THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode = ms.programCode)
													-->> CHECK 3 - does the history contain a NULL end date
													WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode))  
														THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode )
													ELSE (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) END)) [ICD10],
													--#endregion

                                                    --#region age
                                                    (SELECT(CASE WHEN (exists (SELECT 'yes' where (SELECT DATEDIFF(MONTH,d.birthDate,GETDATE())/12) >= 65)) THEN ('yes') ELSE 'no' END)) [Age65],
                                                    --#endregion
                                                    --#region risk-rating
                                                    (SELECT top 1 effectiveDate FROM PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code AND effectiveDate between '{3}' AND '{4}' ORDER BY effectiveDate DESC) [EffectiveDate], --hcare-1138-correction
                                                    (SELECT top 1 RiskType FROM RiskRatingTypes WHERE RiskType = (SELECT top 1 RiskId FROM PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code AND effectiveDate between '{3}' AND '{4}' ORDER BY effectiveDate DESC)) [RiskID], --hcare-1138-correction
                                                    (SELECT top 1 RiskName FROM RiskRatingTypes WHERE RiskType = (SELECT top 1 RiskId FROM PatientRiskRatingHistory WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code AND effectiveDate between '{3}' AND '{4}' ORDER BY effectiveDate DESC)) [RiskRating], --hcare-1138-correction
                                                    (SELECT top 1 reason FROM PatientRiskRatingHistory  WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code AND effectiveDate between '{3}' AND '{4}' ORDER BY effectiveDate DESC) [RiskReason], --hcare-1138-correction   
                                                    --#endregion
                                                    --#region pathology
                                                    (SELECT top 1 eGfr FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFR,
                                                    (SELECT top 1 eGfreffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFREffectiveDate,
                                                    (SELECT top 1 hba1c FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1c,
                                                    (SELECT top 1 hba1ceffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1ceffectiveDate,
                                                    (SELECT top 1 viralLoad FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoad,
                                                    (SELECT top 1 viralLoadeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoadeffectiveDate,
                                                    (SELECT top 1 CD4Count FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4Count,
                                                    (SELECT top 1 CD4CounteffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4CountEffectiveDate,
                                                    (SELECT top 1 CD4Percentage FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4Percentage,
                                                    (SELECT top 1 CD4PercentageeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4PercentageEffectiveDate,
                                                    --#endregion
                                                    --#region comorbids
													(SELECT(CASE 
	                                                    WHEN (exists (select glucose from pathology where dependentID = d.DependantID and active = 1 and glucose between 0.01 and 4.0 and pathologyID = (select top 1 pathologyID from Pathology where dependentID = d.DependantID and active = 1 ORDER BY effectiveDate DESC)))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [HypoRisk],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'DTP028' AND ICD10Code in ('E10.3','E10.8','E11.3','E11.8','E12.3','E13.3','E14.3'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [DiabeticRetinopathy],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL005' AND ICD10Code in ('I25.5','I42.0','I42.1','I42.2','I42.3','I42.4','I42.5','I42.6','I42.7','I42.8','I42.9'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Cardiomyopathy],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL027' AND ICD10Code in ('I20.0','I20.1','I20.8','I20.9','I25.0','I25.1','I25.2','I25.3','I25.4','I25.5','I25.6','I25.8','I25.9'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [CAD],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL004' AND ICD10Code in ('I11.0','I13.0','I13.2','I50.0','I50.1','I50.9'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [CCF],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL013' AND icd10Code in ('I47.2','I48.0','I48.1','I48.2','I48.3','I48.4','I48.9'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Dysrhythmia],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL007' AND ICD10Code in ('I12.0','I13.1','I13.2','N03.0','N03.1','N03.2','N03.3','N03.4','N03.5','N03.6','N03.7','N03.8','N03.9','N11.0','N11.1','N11.8','N11.9','N18.1','N18.2','N18.3','N18.4','N18.5','N18.9','N25.0','O10.2','O10.3'))))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [ChronicRenalFailure],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%hyperlipidaemia%')))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Hyperlipidaemia],
                                                    (SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription = 'hypertension')))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Hypertension],
													(SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%tuberculosis%')))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Tuberculosis],
													(SELECT(CASE 
                                                    	WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%kaposi sarcoma%')))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Cancer],
                                                    --#endregion
                                                    --#region other
													(SELECT(CASE 
                                                    	WHEN (exists (select * from Podiatry where dependentID = d.DependantID and active = 1 and (programType like '%diab%' or programType = '' or programType is null) and amputationCheck = 1))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Amputation],
													(SELECT(CASE 
                                                    	WHEN (exists (select * from Hypoglycaemia where dependentID = d.dependantId and active = 1 and hypoglycaemiaSymptomsComment like '%none%' and glucoseReading not like '%none%'))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [HypoglycaemicUnaware],
													(SELECT(CASE 
                                                    	WHEN (exists (select * from DiabeticDairy where dependentID = d.DependantID))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [DiabeticDiary],
													(SELECT(CASE 
                                                    	WHEN (exists (select * from ClinicalHistoryQuestionaire where dependentID = d.DependantID and active = 1 and smoker = 1))  
                                                    		THEN ('yes')
                                                    	ELSE 'no' END)) [Smoker]
                                                    --#endregion
                                                    
                                                    FROM Dependant d 
                                                    INNER JOIN Member m ON d.memberID = m.memberID
                                                    INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                                    LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                                    	AND (s.endDate > getdate()
                                                    	OR s.endDate is null
                                                    	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))
                                                    	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode)
                                                    	AND s.active = 1
                                                    LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                                    LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                                    WHERE ma.Active = 1
                                                    AND m.active = 1
											        --AND m.membershipNo NOT LIKE '%QA%'
                                                    AND d.Active = 1
                                                    AND d.DependantID in ({0})
                                                    AND ma.MedicalAidID = '{1}'
                                                    AND pr.code = '{2}'
                                                    ", concatenation1.TrimEnd(','), medicalaid, program, fromDate, toDate);

                    using (IDbConnection db2 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                    {
                        db2.Open();
                        var results = (List<RiskSearch>)db2.Query<RiskSearch>(sql2, null, commandTimeout: 5000);
                        db2.Close();

                        #region risk-rating-filter
                        if (!String.IsNullOrEmpty(riskrating))
                        {
                            string[] riskratings = riskrating.Split(',');
                            results = (from result in results
                                       where riskratings.Contains(result.RiskID)
                                       select result).ToList();

                            if (riskrating.ToLower().Contains("n"))
                            {
                                string sql3 = string.Format(@"SELECT pph.dependantId
                                                                FROM Dependant d
                                                                INNER JOIN Member m on d.memberID = m.memberID
                                                                INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                        						                INNER JOIN PatientProgramHistory pph on d.DependantID = pph.dependantId 

                                                                WHERE d.DependantID NOT IN (SELECT rr.dependantID FROM PatientRiskRatingHistory rr where rr.active = 1 and rr.programType = '{1}')
                                                                AND ma.Active = 1
                                                                AND ma.MedicalAidID = '{0}'
                        						                AND pph.programCode = '{1}'
                                                                ", medicalaid, program);

                                using (IDbConnection db3 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                {
                                    db3.Open();
                                    var riskResults = (List<RiskSearch>)db3.Query<RiskSearch>(sql3, null, commandTimeout: 5000);
                                    db3.Close();

                                    if (riskResults != null)
                                    {
                                        string concatenation2 = string.Empty;
                                        string programName = string.Empty;
                                        string icd10 = string.Empty;

                                        foreach (var item in riskResults)
                                        {
                                            concatenation2 += "'" + item.DependantID + "',";
                                        }
                                        string newsql = string.Format(@"SET DATEFORMAT YMD
                                                                        SELECT d.dependantId, ma.Name[MedicalScheme], pr.programID[ProgramID], pr.code, pr.ProgramName[Program], d.firstName + ' ' + d.lastName[PatientName], d.idNumber[IDNumber], m.membershipNo[MembershipNumber], 
                                                                        d.dependentCode[DependantCode], d.birthDate[BirthDate], s.managementStatusCode as statusCode, ms.statusName[ManagementStatus], 
                                                                        
													                    --#region gender
													                    (SELECT sexName from Sex where sex = (select sex from Dependant where dependantId = d.DependantID)) [Gender],
													                    --#endregion
													                    --#region program-icd10-code
													                    (SELECT(CASE 
													                    -->> CHECK 1 - does the history contain a date greater than today
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode)
													                    -->> CHECK 2 - does the history contain a date less than today
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode  = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode = ms.programCode)
													                    -->> CHECK 3 - does the history contain a NULL end date
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode )
													                    ELSE (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) END)) [ICD10],
													                    --#endregion

                                                                        --#region age
                                                                        (SELECT(CASE WHEN (exists (SELECT 'yes' where (SELECT DATEDIFF(MONTH,d.birthDate,GETDATE())/12) >= 65)) THEN ('yes') ELSE 'no' END)) [Age65],
                                                                        --#endregion
                                                                        --#region risk-rating
                                                                        NULL[EffectiveDate], 'N'[RiskID],'NONE'[RiskRating],'Not yet risk rated'[RiskReason], 
                                                                        --#endregion
                                                                        --#region pathology
                                                                        (SELECT top 1 eGfr FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFR,
                                                                        (SELECT top 1 eGfreffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFREffectiveDate,
                                                                        (SELECT top 1 hba1c FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1c,
                                                                        (SELECT top 1 hba1ceffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1ceffectiveDate,
                                                                        (SELECT top 1 viralLoad FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoad,
                                                                        (SELECT top 1 viralLoadeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoadeffectiveDate,
                                                                        (SELECT top 1 CD4Count FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4Count,
                                                                        (SELECT top 1 CD4CounteffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4CountEffectiveDate,
                                                                        (SELECT top 1 CD4Percentage FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4Percentage,
                                                                        (SELECT top 1 CD4PercentageeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4PercentageEffectiveDate,
                                                                        --#endregion
																		--#region comorbids
																		(SELECT(CASE 
																		    WHEN (exists (select glucose from pathology where dependentID = d.DependantID and active = 1 and glucose between 0.01 and 4.0 and pathologyID = (select top 1 pathologyID from Pathology where dependentID = d.DependantID and active = 1 ORDER BY effectiveDate DESC)))  
																				THEN ('yes')
																			ELSE 'no' END)) [HypoRisk],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'DTP028' AND ICD10Code in ('E10.3','E10.8','E11.3','E11.8','E12.3','E13.3','E14.3'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [DiabeticRetinopathy],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL005' AND ICD10Code in ('I25.5','I42.0','I42.1','I42.2','I42.3','I42.4','I42.5','I42.6','I42.7','I42.8','I42.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [Cardiomyopathy],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL027' AND ICD10Code in ('I20.0','I20.1','I20.8','I20.9','I25.0','I25.1','I25.2','I25.3','I25.4','I25.5','I25.6','I25.8','I25.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [CAD],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL004' AND ICD10Code in ('I11.0','I13.0','I13.2','I50.0','I50.1','I50.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [CCF],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL013' AND icd10Code in ('I47.2','I48.0','I48.1','I48.2','I48.3','I48.4','I48.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [Dysrhythmia],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL007' AND ICD10Code in ('I12.0','I13.1','I13.2','N03.0','N03.1','N03.2','N03.3','N03.4','N03.5','N03.6','N03.7','N03.8','N03.9','N11.0','N11.1','N11.8','N11.9','N18.1','N18.2','N18.3','N18.4','N18.5','N18.9','N25.0','O10.2','O10.3'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [ChronicRenalFailure],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%hyperlipidaemia%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Hyperlipidaemia],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription = 'hypertension')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Hypertension],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%tuberculosis%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Tuberculosis],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%kaposi sarcoma%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Cancer],
																		--#endregion
                                                                        --#region other
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from Podiatry where dependentID = d.DependantID and active = 1 and (programType like '%diab%' or programType = '' or programType is null) and amputationCheck = 1))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [Amputation],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from Hypoglycaemia where dependentID = d.dependantId and active = 1 and hypoglycaemiaSymptomsComment like '%none%' and glucoseReading not like '%none%'))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [HypoglycaemicUnaware],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from DiabeticDairy where dependentID = d.DependantID))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [DiabeticDiary],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from ClinicalHistoryQuestionaire where dependentID = d.DependantID and active = 1 and smoker = 1))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [Smoker]
                                                                        --#endregion
                                                                        
                                                                        FROM Dependant d 
                                                                        INNER JOIN Member m ON d.memberID = m.memberID
                                                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                                                        	AND (s.endDate > getdate()
                                                                        	OR s.endDate is null
                                                                        	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))
                                                                        	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode)
                                                                        	AND s.active = 1
                                                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                                                        WHERE ma.Active = 1
                                                                        AND m.active = 1
                                                                        AND d.Active = 1
											                            --AND m.membershipNo NOT LIKE '%QA%'
                                                                        AND d.DependantID in ({0})
                                                                        AND ma.MedicalAidID = '{1}'
                                                                        AND pr.code = '{2}'
                                                                        ", concatenation2.TrimEnd(','), medicalaid, program);

                                        using (IDbConnection db4 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                        {
                                            db4.Open();
                                            var newResults = (List<RiskSearch>)db4.Query<RiskSearch>(newsql, null, commandTimeout: 5000);
                                            db4.Close();

                                            foreach (var item in newResults)
                                            {
                                                results.Add(item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        return results.OrderByDescending(x => x.EffectiveDate).ToList();
                    }
                }
                else //hcare-1138: update
                {
                    var results = new List<RiskSearch>();

                    #region risk-rating-filter
                    if (!String.IsNullOrEmpty(riskrating))
                    {
                        if (riskrating.ToLower().Contains("n"))
                        {
                            string sql3 = string.Format(@"SELECT pph.dependantId
                                                                FROM Dependant d
                                                                INNER JOIN Member m on d.memberID = m.memberID
                                                                INNER JOIN MedicalAid ma on m.medicalAidID = ma.MedicalAidID
                        						                INNER JOIN PatientProgramHistory pph on d.DependantID = pph.dependantId 

                                                                WHERE d.DependantID NOT IN (SELECT rr.dependantID FROM PatientRiskRatingHistory rr where rr.active = 1 and rr.programType = '{1}')
                                                                AND ma.Active = 1
                                                                AND ma.MedicalAidID = '{0}'
                        						                AND pph.programCode = '{1}'
                                                                ", medicalaid, program);

                            using (IDbConnection db3 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                            {
                                db3.Open();
                                var riskResults = (List<RiskSearch>)db3.Query<RiskSearch>(sql3, null, commandTimeout: 5000);
                                db3.Close();

                                if (riskResults != null)
                                {
                                    string concatenation2 = string.Empty;
                                    string programName = string.Empty;
                                    string icd10 = string.Empty;

                                    foreach (var item in riskResults)
                                    {
                                        concatenation2 += "'" + item.DependantID + "',";
                                    }
                                    string newsql = string.Format(@"SET DATEFORMAT YMD
                                                                        SELECT d.dependantId, ma.Name[MedicalScheme], pr.programID[ProgramID], pr.code, pr.ProgramName[Program], d.firstName + ' ' + d.lastName[PatientName], d.idNumber[IDNumber], m.membershipNo[MembershipNumber], 
                                                                        d.dependentCode[DependantCode], d.birthDate[BirthDate], s.managementStatusCode as statusCode, ms.statusName[ManagementStatus], 
                                                                        
													                    --#region gender
													                    (SELECT sexName from Sex where sex = (select sex from Dependant where dependantId = d.DependantID)) [Gender],
													                    --#endregion
													                    --#region program-icd10-code
													                    (SELECT(CASE 
													                    -->> CHECK 1 - does the history contain a date greater than today
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode)
													                    -->> CHECK 2 - does the history contain a date less than today
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode  = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode = ms.programCode)
													                    -->> CHECK 3 - does the history contain a NULL end date
													                    WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode))  
													                    	THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode )
													                    ELSE (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) END)) [ICD10],
													                    --#endregion

                                                                        --#region age
                                                                        (SELECT(CASE WHEN (exists (SELECT 'yes' where (SELECT DATEDIFF(MONTH,d.birthDate,GETDATE())/12) >= 65)) THEN ('yes') ELSE 'no' END)) [Age65],
                                                                        --#endregion
                                                                        --#region risk-rating
                                                                        NULL[EffectiveDate], 'N'[RiskID],'NONE'[RiskRating],'Not yet risk rated'[RiskReason], 
                                                                        --#endregion
                                                                        --#region pathology
                                                                        (SELECT top 1 eGfr FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFR,
                                                                        (SELECT top 1 eGfreffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND eGfr <> 0 ORDER BY effectiveDate DESC) AS eGFREffectiveDate,
                                                                        (SELECT top 1 hba1c FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1c,
                                                                        (SELECT top 1 hba1ceffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND hba1c <> 0 ORDER BY effectiveDate DESC) AS hba1ceffectiveDate,
                                                                        (SELECT top 1 viralLoad FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoad,
                                                                        (SELECT top 1 viralLoadeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND viralLoad <> 0 ORDER BY effectiveDate DESC) AS viralLoadeffectiveDate,
                                                                        (SELECT top 1 CD4Count FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4Count,
                                                                        (SELECT top 1 CD4CounteffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Count <> 0 ORDER BY effectiveDate DESC) AS CD4CountEffectiveDate,
                                                                        (SELECT top 1 CD4Percentage FROM Pathology WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4Percentage,
                                                                        (SELECT top 1 CD4PercentageeffectiveDate FROM Pathology  WHERE active = 1 AND dependentID = d.DependantID AND CD4Percentage <> 0 ORDER BY effectiveDate DESC) AS CD4PercentageEffectiveDate,
                                                                        --#endregion
																		--#region comorbids
																		(SELECT(CASE 
																		    WHEN (exists (select glucose from pathology where dependentID = d.DependantID and active = 1 and glucose between 0.01 and 4.0 and pathologyID = (select top 1 pathologyID from Pathology where dependentID = d.DependantID and active = 1 ORDER BY effectiveDate DESC)))  
																				THEN ('yes')
																			ELSE 'no' END)) [HypoRisk],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'DTP028' AND ICD10Code in ('E10.3','E10.8','E11.3','E11.8','E12.3','E13.3','E14.3'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [DiabeticRetinopathy],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL005' AND ICD10Code in ('I25.5','I42.0','I42.1','I42.2','I42.3','I42.4','I42.5','I42.6','I42.7','I42.8','I42.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [Cardiomyopathy],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL027' AND ICD10Code in ('I20.0','I20.1','I20.8','I20.9','I25.0','I25.1','I25.2','I25.3','I25.4','I25.5','I25.6','I25.8','I25.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [CAD],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL004' AND ICD10Code in ('I11.0','I13.0','I13.2','I50.0','I50.1','I50.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [CCF],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL013' AND icd10Code in ('I47.2','I48.0','I48.1','I48.2','I48.3','I48.4','I48.9'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [Dysrhythmia],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingCode = 'CDL007' AND ICD10Code in ('I12.0','I13.1','I13.2','N03.0','N03.1','N03.2','N03.3','N03.4','N03.5','N03.6','N03.7','N03.8','N03.9','N11.0','N11.1','N11.8','N11.9','N18.1','N18.2','N18.3','N18.4','N18.5','N18.9','N25.0','O10.2','O10.3'))))  
																				THEN ('yes')
																			ELSE 'no' END)) [ChronicRenalFailure],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%hyperlipidaemia%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Hyperlipidaemia],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription = 'hypertension')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Hypertension],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%tuberculosis%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Tuberculosis],
																		(SELECT(CASE 
																			WHEN (exists (select * from CoMormidConditions where dependantID = d.DependantID and active = 1 and followUp = 0 and (treatementEndDate is null or treatementEndDate >= GETDATE()) and coMorbidId in (select id from ComorbidConditionExclusions where mappingDescription like '%kaposi sarcoma%')))  
																				THEN ('yes')
																			ELSE 'no' END)) [Cancer],
																		--#endregion
                                                                        --#region other
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from Podiatry where dependentID = d.DependantID and active = 1 and (programType like '%diab%' or programType = '' or programType is null) and amputationCheck = 1))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [Amputation],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from Hypoglycaemia where dependentID = d.dependantId and active = 1 and hypoglycaemiaSymptomsComment like '%none%' and glucoseReading not like '%none%'))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [HypoglycaemicUnaware],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from DiabeticDairy where dependentID = d.DependantID))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [DiabeticDiary],
													                    (SELECT(CASE 
                                                                        	WHEN (exists (select * from ClinicalHistoryQuestionaire where dependentID = d.DependantID and active = 1 and smoker = 1))  
                                                                        		THEN ('yes')
                                                                        	ELSE 'no' END)) [Smoker]
                                                                        --#endregion
                                                                        
                                                                        FROM Dependant d 
                                                                        INNER JOIN Member m ON d.memberID = m.memberID
                                                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                                                        	AND (s.endDate > getdate()
                                                                        	OR s.endDate is null
                                                                        	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))
                                                                        	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode)
                                                                        	AND s.active = 1
                                                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                                                        WHERE ma.Active = 1
                                                                        AND m.active = 1
                                                                        AND d.Active = 1
											                            --AND m.membershipNo NOT LIKE '%QA%'
                                                                        AND d.DependantID in ({0})
                                                                        AND ma.MedicalAidID = '{1}'
                                                                        AND pr.code = '{2}'
                                                                        ", concatenation2.TrimEnd(','), medicalaid, program);

                                    using (IDbConnection db4 = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
                                    {
                                        db4.Open();
                                        var newResults = (List<RiskSearch>)db4.Query<RiskSearch>(newsql, null, commandTimeout: 5000);
                                        db4.Close();

                                        foreach (var item in newResults)
                                        {
                                            results.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    return results.OrderByDescending(x => x.EffectiveDate).ToList();
                }
            }
        }
        public List<ManagementStatusSearch> GetManagementStatusSearchResults(string medicalaid = "", string program = "", string managementstatus_current = "", string fromdate_current = "", string todate_current = "", string managementstatus_previous = "") //HCare-1267
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedMedicalAidList = rights.accessList.Select(x => x.medicalAidId).ToList();


            DateTime dt_from_current = Convert.ToDateTime(fromdate_current);
            DateTime dt_to_current = Convert.ToDateTime(todate_current);

            string sql = string.Format(@"SET DATEFORMAT YMD
                                        SELECT d.DependantID, ma.Name[MedicalScheme],  
                                        --#region program-information
                                        pr.programID[ProgramID], pr.ProgramName[Program], pr.code[ProgramCode],
                                        (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId AND pr.code = programCode AND active = 1)[ProgramStartDate],
                                        
                                        (SELECT(CASE 
                                        	-->> CHECK 1 - does the history contain a date greater than today
                                        	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode))  
                                        		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate > GETDATE() OR endDate is null) AND programCode = ms.programCode)
                                        	-->> CHECK 2 - does the history contain a date less than today
                                        	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode  = ms.programCode))  
                                        		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND (endDate < GETDATE() OR endDate is null) AND programCode = ms.programCode)
                                        	-->> CHECK 3 - does the history contain a NULL end date
                                        	WHEN (exists (SELECT top 1 endDate FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode))  
                                        		THEN (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID AND endDate is NULL AND programCode = ms.programCode )
                                        	ELSE (SELECT top 1 icd10Code FROM PatientProgramHistory  WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate DESC) END)) [ICD10],
                                        --#endregion
                                        --#region dependant-information
                                        m.membershipNo[MembershipNumber], d.dependentCode[DependantCode], d.idNumber[IDNumber], d.firstName + ' ' + d.lastName [PatientName], 
                                        --#endregion
                                        --#region ms-information
                                        (SELECT top 1 ms.statusType FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode WHERE managementStatusCode = (SELECT TOP 1 managementStatusCode From (SELECT TOP 2 ssh.* From PatientManagementStatusHistory ssh INNER JOIN ManagementStatus mss on ssh.managementStatusCode = mss.statusCode WHERE ssh.active = 1 AND ssh.dependantId = d.DependantID AND mss.programCode = pr.code ORDER BY ssh.effectiveDate DESC) x WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate ASC) AND sh.active = 1 AND sh.dependantId = d.DependantID)[StatusTypePrevious],
                                        (SELECT top 1 ms.statusCode FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode WHERE managementStatusCode = (SELECT TOP 1 managementStatusCode From (SELECT TOP 2 ssh.* From PatientManagementStatusHistory ssh INNER JOIN ManagementStatus mss on ssh.managementStatusCode = mss.statusCode WHERE ssh.active = 1 AND ssh.dependantId = d.DependantID AND mss.programCode = pr.code ORDER BY ssh.effectiveDate DESC) x WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate ASC) AND sh.active = 1 AND sh.dependantId = d.DependantID)[StatusCodePrevious],
                                        (SELECT top 1 ms.statusName FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode WHERE managementStatusCode = (SELECT TOP 1 managementStatusCode From (SELECT TOP 2 ssh.* From PatientManagementStatusHistory ssh INNER JOIN ManagementStatus mss on ssh.managementStatusCode = mss.statusCode WHERE ssh.active = 1 AND ssh.dependantId = d.DependantID AND mss.programCode = pr.code ORDER BY ssh.effectiveDate DESC) x WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate ASC) AND sh.active = 1 AND sh.dependantId = d.DependantID)[ManagementStatusPrevious],
                                        (SELECT top 1 sh.effectiveDate FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode WHERE managementStatusCode = (SELECT TOP 1 managementStatusCode From (SELECT TOP 2 ssh.* From PatientManagementStatusHistory ssh INNER JOIN ManagementStatus mss on ssh.managementStatusCode = mss.statusCode WHERE ssh.active = 1 AND ssh.dependantId = d.DependantID AND mss.programCode = pr.code ORDER BY ssh.effectiveDate DESC) x WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate ASC) AND sh.active = 1 AND sh.dependantId = d.DependantID)[ManagementStatusPrevious_StartDate],
                                        (SELECT top 1 sh.endDate FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode WHERE managementStatusCode = (SELECT TOP 1 managementStatusCode From (SELECT TOP 2 ssh.* From PatientManagementStatusHistory ssh INNER JOIN ManagementStatus mss on ssh.managementStatusCode = mss.statusCode WHERE ssh.active = 1 AND ssh.dependantId = d.DependantID AND mss.programCode = pr.code ORDER BY ssh.effectiveDate DESC) x WHERE active = 1 AND dependantId = d.DependantID ORDER BY effectiveDate ASC) AND sh.active = 1 AND sh.dependantId = d.DependantID)[ManagementStatusPrevious_EndDate],
                                        
                                        (SELECT top 1 ms.statusType FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode where sh.active = 1 AND sh.dependantId = d.DependantID AND ms.programCode = pr.code ORDER BY sh.effectiveDate DESC) [StatusTypeCurrent], 
                                        (SELECT top 1 ms.statusCode FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode where sh.active = 1 AND sh.dependantId = d.DependantID AND ms.programCode = pr.code ORDER BY sh.effectiveDate DESC) [StatusCodeCurrent], 
                                        (SELECT top 1 ms.statusName FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode where sh.active = 1 AND sh.dependantId = d.DependantID AND ms.programCode = pr.code ORDER BY sh.effectiveDate DESC) [ManagementStatusCurrent], 
                                        (SELECT top 1 sh.effectiveDate FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode where sh.active = 1 AND sh.dependantId = d.DependantID AND ms.programCode = pr.code ORDER BY sh.effectiveDate DESC) [ManagementStatusCurrent_StartDate], 
                                        (SELECT top 1 sh.endDate FROM PatientManagementStatusHistory sh INNER JOIN ManagementStatus ms on sh.managementStatusCode = ms.statusCode where sh.active = 1 AND sh.dependantId = d.DependantID AND ms.programCode = pr.code ORDER BY sh.effectiveDate DESC) [ManagementStatusCurrent_EndDate] 
                                        --#endregion
                                        
                                        FROM Dependant d 
                                        INNER JOIN Member m ON d.memberID = m.memberID 
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        	AND (s.endDate > getdate()
                                        	OR s.endDate is null
                                        	OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus WHERE statusType IN ('C', 'P', 'O') AND active = 1) ))--hcare-1171
                                        	AND s.effectiveDate IN (SELECT MAX(ss.effectiveDate) FROM PatientManagementStatusHistory ss INNER JOIN ManagementStatus mss on ss.managementStatusCode = mss.statusCode WHERE ss.active = 1 AND ss.dependantId = d.DependantID GROUP BY mss.programCode) --hcare-1234
                                        	AND s.active = 1
                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                        WHERE ma.Active = 1
                                        AND m.active = 1
                                        AND d.Active = 1
                                        AND ma.MedicalAidID = '{0}'
                                        AND pr.code = '{1}'
                                        ", medicalaid, program);


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<ManagementStatusSearch>)db.Query<ManagementStatusSearch>(sql, null, commandTimeout: 5000);
                db.Close();

                var results_c = new List<ManagementStatusSearch>(); results_c = results;
                var results_p = new List<ManagementStatusSearch>(); results_p = results;

                var newResult = new List<ManagementStatusSearch>();

                if (!String.IsNullOrEmpty(managementstatus_current))
                {
                    string[] mstatus_c = managementstatus_current.Split(',');
                    results_c = (from r in results_c
                                 where mstatus_c.Contains(r.StatusTypeCurrent)
                                 select r).ToList();
                }

                foreach (var rc in results_c)
                {
                    if ((rc.ManagementStatusCurrent != null) &&
                        (rc.ManagementStatusCurrent_StartDate >= dt_from_current) &&
                        (rc.ManagementStatusCurrent_EndDate <= dt_to_current || rc.ManagementStatusCurrent_EndDate == null))
                    {
                        string[] mstatus_p = managementstatus_previous.Split(',');
                        if (mstatus_p.Contains(rc.StatusTypePrevious))
                        {
                            rc.ManagementStatusPrevious = rc.ManagementStatusPrevious;
                            rc.ManagementStatusPrevious_StartDate = rc.ManagementStatusPrevious_StartDate;
                            rc.ManagementStatusPrevious_EndDate = rc.ManagementStatusPrevious_EndDate;
                        }
                        else
                        {
                            rc.ManagementStatusPrevious = null;
                            rc.ManagementStatusPrevious_StartDate = null;
                            rc.ManagementStatusPrevious_EndDate = null;
                        }

                        //if (rc.StatusCodeCurrent.Substring(0, 2).ToUpper().Contains("ER")) //a member can have an enrolment after the fact - corrections made by development team - eg. SAPPI / BESTMED 
                        //{
                        //    rc.ManagementStatusPrevious = null;
                        //    rc.ManagementStatusPrevious_StartDate = null;
                        //    rc.ManagementStatusPrevious_EndDate = null;
                        //}

                        newResult.Add(rc);
                    }
                }

                #region hcare-1267: correction
                var freshResult = new List<ManagementStatusSearch>();

                if (!String.IsNullOrEmpty(managementstatus_previous))
                {
                    foreach (var nr in newResult)
                    {
                        if (!String.IsNullOrEmpty(nr.ManagementStatusPrevious) && !String.IsNullOrEmpty(nr.ManagementStatusPrevious_StartDate.ToString()))
                        {
                            freshResult.Add(nr);
                        }
                    }
                }
                else
                {
                    freshResult = newResult;
                }
                #endregion
                return freshResult;
            }
        }
        public List<EnquiryResultsVM> GetEnquirySearchResults(string medicalaid = "", string program = "", string managementstatus = "", string querypriorities = "", string querytypes = "", string fromdate = "", string todate = "") //hcare-874
        {
            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedMedicalAidList = rights.accessList.Select(x => x.medicalAidId).ToList();


            string[] scheme_list = medicalaid.Split(',');
            string[] program_list = program.Split(',');
            string[] ms_list = managementstatus.Split(',');
            string[] priority_list = querypriorities.Split(',');
            string[] querytype_list = querytypes.Split(',');

            DateTime dt_from = Convert.ToDateTime(fromdate);
            DateTime dt_to = Convert.ToDateTime(todate);

            var results1 = (from q in _context.Queries
                            join d in _context.Dependants on q.dependentID equals d.DependantID
                            join m in _context.Members on d.memberID equals m.memberID
                            join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                            join pr in _context.Priorities on q.priority equals pr.prioritytype into prInfo
                            from pr in prInfo.DefaultIfEmpty()
                            join qt in _context.QueryTypes on q.enquiryBy equals qt.code
                            join p in _context.Program on q.programId equals p.programID into pInfo
                            from p in pInfo.DefaultIfEmpty()

                            where ma.Active == true
                            where q.effectiveDate >= dt_from
                            where q.effectiveDate <= dt_to

                            where scheme_list.Contains(ma.MedicalAidID.ToString())

                            select new EnquiryResultsVM()
                            {
                                DependantID = d.DependantID,
                                SchemeID = ma.MedicalAidID,
                                MedicalScheme = ma.Name,
                                ProgramID = p.programID,
                                Program = p.ProgramName,
                                ProgramCode = p.code,
                                MemberID = d.idNumber,
                                MemberNumber = m.membershipNo,
                                MemberName = d.firstName + " " + d.lastName,
                                ManagementStatus = _context.ManagementStatus.Where(p => p.statusCode == _context.PatientManagementStatusHistory.Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault()).Select(p => p.statusName).FirstOrDefault(),
                                DependantCode = d.dependentCode,
                                EnquirySource = q.querySubject,
                                EnquiryBy = q.enquiryBy,
                                EnquiryType = qt.queryDescription,
                                EnquiryEffectiveDate = q.effectiveDate,
                                EnquiryPriority = pr.priorityName,
                                EnquiryDetails = q.query,
                                EnquiryStatus = q.queryStatus.ToString(),
                                EnquiryOwner = q.Owner,
                                CreatedBy = q.createdBy,
                                CreatedDate = q.createdDate,
                                ModifiedBy = q.modifiedBy,
                                ModifiedDate = q.modifiedDate,
                                ResolvedBy = q.resolvedBy,
                                ResolvedDate = q.resolutionDate,
                                EnquiryComment = q.query,
                                EnquiryID = q.queryID.ToString(),
                                EnquiryReference = q.ReferenceNumber,
                                DateFrom = dt_from,
                                DateTo = dt_to,
                                FollowUp = q.queryStatus

                            }).OrderByDescending(x => x.EnquiryEffectiveDate).ToList();

            #region query-priority-null-check
            foreach (var result in results1.Where(x => x.EnquiryPriority == "" || x.EnquiryPriority == null))
            {
                if (String.IsNullOrEmpty(result.EnquiryPriority))
                {
                    result.EnquiryPriority = "LOW";
                }
            }
            #endregion
            #region program-filter
            var results = new List<EnquiryResultsVM>();
            foreach (var p in program_list)
            {
                foreach (var result in results1.Where(x => x.ProgramID == new Guid(p) || x.ProgramID == null))
                {
                    results.Add(result);
                }
            }
            #endregion
            #region management-status-filter
            if (!String.IsNullOrEmpty(managementstatus))
            {
                results = (from r in results
                           where managementstatus.ToLower().Contains(r.ManagementStatus.ToLower())
                           select r).ToList();
            }
            #endregion
            #region query-priority-filter
            if (!String.IsNullOrEmpty(querypriorities))
            {
                results = (from r in results
                           where querypriorities.ToLower().Contains(r.EnquiryPriority.ToLower())
                           select r).ToList();
            }
            #endregion
            #region query-type-filter
            if (!String.IsNullOrEmpty(querytypes))
            {
                results = (from r in results
                           where querytypes.ToLower().Contains(r.EnquiryType.ToLower())
                           select r).ToList();
            }
            #endregion
            #region allowed-program-filter
            var programValidation = new List<EnquiryResultsVM>();
            foreach (var result in results)
            {
                if (String.IsNullOrEmpty(result.Program)) { result.Program = "N/A"; }
                var options = GetAllowedProgramsPerScheme(result.SchemeID);
                foreach (var o in options)
                {
                    if (result.ProgramID == o.programID || result.ProgramID == null)
                    {
                        programValidation.Add(result);
                    }
                }
            }

            results = programValidation;
            #endregion

            return results = results.GroupBy(p => p.EnquiryID).Select(g => g.First()).OrderBy(x => x.MemberID).ThenBy(x => x.Program).ThenByDescending(x => x.CreatedDate).ToList();

        }
        public List<ProductionResultsVM> GetProductionSearchResults(string medicalaid = "", string program = "", string users = "", string workitems = "", string fromdate = "", string todate = "") //hcare-1289
        {
            var results = new List<ProductionResultsVM>();

            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedMedicalAidList = rights.accessList.Select(x => x.medicalAidId).ToList();


            string[] scheme_list = medicalaid.Split(',');
            string[] program_list = program.Split(',');
            string[] user_list = users.Split(',');
            string[] workitem_list = workitems.Split(',');

            DateTime dt_from = Convert.ToDateTime(fromdate);
            DateTime dt_to = Convert.ToDateTime(todate);

            #region work-items
            if (workitems.ToUpper().Contains("AT01")) //attachments
            {
                var attachments = (from a in _context.Attachments
                                   join p in _context.Program on a.programId equals p.programID
                                   join d in _context.Dependants on a.dependentID equals d.DependantID
                                   join m in _context.Members on d.memberID equals m.memberID
                                   join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                                   join u in _context.Users on a.createdBy equals u.username
                                   where program_list.Contains(a.programId.ToString())
                                   where user_list.Contains(a.createdBy)
                                   where a.createdDate >= dt_from
                                   where a.createdDate <= dt_to
                                   select new ProductionResultsVM
                                   {
                                       Id = a.attachmentID,
                                       DependantID = a.dependentID,
                                       SchemeID = ma.MedicalAidID,
                                       ProgramID = a.programId,
                                       UserID = u.userID,
                                       MedicalScheme = ma.Name,
                                       Program = p.ProgramName,
                                       UserName = u.Firstname + " " + u.Lastname,
                                       WorkItem = "Attachment",
                                       DateFrom = dt_from,
                                       DateTo = dt_to,
                                       MemberID = d.idNumber,
                                       MemberNumber = m.membershipNo,
                                       DependantCode = d.dependentCode,
                                       MemberName = d.firstName + " " + d.lastName,
                                       //AttachmentCode = a.attachmentType,
                                       //AttachmentName = a.attachmentName,
                                       //AttachmentLink = a.Link,
                                       ProgramCode = p.code,
                                       CreatedBy = a.createdBy,
                                       CreatedDate = a.createdDate,
                                       ModifiedBy = a.modifiedBy,
                                       ModifiedDate = a.modifiedDate,
                                       Active = a.Active

                                   }).ToList();

                foreach (var result in attachments) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("CP01")) //care-plan
            {
                var careplan = (from it in _context.AssignmentItemTasks
                                join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id into aio
                                from ai in aio.DefaultIfEmpty()
                                join a in _context.Assignments on ai.assignmentId equals a.assignmentID into ao
                                from a in ao.DefaultIfEmpty()
                                join d in _context.Dependants on a.dependentID equals d.DependantID into dpo
                                from d in dpo.DefaultIfEmpty()
                                join m in _context.Members on d.memberID equals m.memberID into mo
                                from m in mo.DefaultIfEmpty()
                                join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                from ma in mao.DefaultIfEmpty()
                                join p in _context.Program on a.programId equals p.programID into po
                                from p in po.DefaultIfEmpty()
                                join u in _context.Users on it.closedBy equals u.username

                                where program_list.Contains(a.programId.ToString())
                                where user_list.Contains(it.closedBy)
                                where it.taskTypeId == "005"
                                where it.requirementId == 87
                                where it.closedDate >= dt_from
                                where it.closedDate <= dt_to

                                select new ProductionResultsVM
                                {
                                    Id = a.assignmentID,
                                    DependantID = a.dependentID,
                                    SchemeID = ma.MedicalAidID,
                                    ProgramID = a.programId,
                                    UserID = u.userID,
                                    MedicalScheme = ma.Name,
                                    Program = p.ProgramName,
                                    UserName = u.Firstname + " " + u.Lastname,
                                    WorkItem = "Care plan",
                                    DateFrom = dt_from,
                                    DateTo = dt_to,
                                    MemberID = d.idNumber,
                                    MemberNumber = m.membershipNo,
                                    DependantCode = d.dependentCode,
                                    MemberName = d.firstName + " " + d.lastName,
                                    ProgramCode = p.code,
                                    CreatedBy = it.createdBy,
                                    CreatedDate = it.createdDate,
                                    Active = a.Active

                                }).ToList();

                foreach (var result in careplan) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("DD01")) //diabetic-diary
            {
                var diabeticdiary = (from dd in _context.DiabeticDairy
                                     join d in _context.Dependants on dd.dependentID equals d.DependantID into dpo
                                     from d in dpo.DefaultIfEmpty()
                                     join m in _context.Members on d.memberID equals m.memberID into mo
                                     from m in mo.DefaultIfEmpty()
                                     join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                     from ma in mao.DefaultIfEmpty()
                                     join p in _context.Program on dd.programId equals p.programID into po
                                     from p in po.DefaultIfEmpty()
                                     join u in _context.Users on dd.createdBy equals u.username

                                     where program_list.Contains(dd.programId.ToString())
                                     where user_list.Contains(dd.createdBy)
                                     where dd.createdDate >= dt_from
                                     where dd.createdDate <= dt_to

                                     select new ProductionResultsVM
                                     {
                                         Id = dd.dairyId,
                                         DependantID = dd.dependentID,
                                         SchemeID = ma.MedicalAidID,
                                         ProgramID = dd.programId,
                                         UserID = u.userID,
                                         MedicalScheme = ma.Name,
                                         Program = p.ProgramName,
                                         UserName = u.Firstname + " " + u.Lastname,
                                         WorkItem = "Diabetic diary: sent",
                                         DateFrom = dt_from,
                                         DateTo = dt_to,
                                         MemberID = d.idNumber,
                                         MemberNumber = m.membershipNo,
                                         DependantCode = d.dependentCode,
                                         MemberName = d.firstName + " " + d.lastName,
                                         ProgramCode = p.code,
                                         CreatedBy = dd.createdBy,
                                         CreatedDate = dd.createdDate,
                                         Active = true

                                     }).ToList();

                foreach (var result in diabeticdiary) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("DD02")) //diabetic-diary-returns
            {
                var diaryreturn = (from dd in _context.DiabeticDairy
                                   join d in _context.Dependants on dd.dependentID equals d.DependantID into dpo
                                   from d in dpo.DefaultIfEmpty()
                                   join m in _context.Members on d.memberID equals m.memberID into mo
                                   from m in mo.DefaultIfEmpty()
                                   join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                   from ma in mao.DefaultIfEmpty()
                                   join p in _context.Program on dd.programId equals p.programID into po
                                   from p in po.DefaultIfEmpty()
                                   join u in _context.Users on dd.createdBy equals u.username

                                   where program_list.Contains(dd.programId.ToString())
                                   where user_list.Contains(dd.modifiedBy)
                                   where dd.returnedDate != null
                                   where dd.modifiedDate >= dt_from
                                   where dd.modifiedDate <= dt_to


                                   select new ProductionResultsVM
                                   {
                                       Id = dd.dairyId,
                                       DependantID = dd.dependentID,
                                       SchemeID = ma.MedicalAidID,
                                       ProgramID = dd.programId,
                                       UserID = u.userID,
                                       MedicalScheme = ma.Name,
                                       Program = p.ProgramName,
                                       UserName = u.Firstname + " " + u.Lastname,
                                       WorkItem = "Diabetic diary: return",
                                       DateFrom = dt_from,
                                       DateTo = dt_to,
                                       MemberID = d.idNumber,
                                       MemberNumber = m.membershipNo,
                                       DependantCode = d.dependentCode,
                                       MemberName = d.firstName + " " + d.lastName,
                                       ProgramCode = p.code,
                                       CreatedBy = dd.modifiedBy,
                                       CreatedDate = dd.modifiedDate,
                                       Active = true

                                   }).ToList();

                foreach (var result in diaryreturn) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("DI01")) //disclaimers
            {
                var disclaimers = (from it in _context.AssignmentItemTasks
                                   join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id into aio
                                   from ai in aio.DefaultIfEmpty()
                                   join a in _context.Assignments on ai.assignmentId equals a.assignmentID into ao
                                   from a in ao.DefaultIfEmpty()
                                   join d in _context.Dependants on a.dependentID equals d.DependantID into dpo
                                   from d in dpo.DefaultIfEmpty()
                                   join m in _context.Members on d.memberID equals m.memberID into mo
                                   from m in mo.DefaultIfEmpty()
                                   join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                   from ma in mao.DefaultIfEmpty()
                                   join p in _context.Program on a.programId equals p.programID into po
                                   from p in po.DefaultIfEmpty()
                                   join u in _context.Users on it.closedBy equals u.username

                                   where program_list.Contains(a.programId.ToString())
                                   where user_list.Contains(it.closedBy)
                                   where it.taskTypeId == "007"
                                   where it.requirementId == 41
                                   where it.closedDate >= dt_from
                                   where it.closedDate <= dt_to

                                   select new ProductionResultsVM
                                   {
                                       Id = a.assignmentID,
                                       DependantID = a.dependentID,
                                       SchemeID = ma.MedicalAidID,
                                       ProgramID = a.programId,
                                       UserID = u.userID,
                                       MedicalScheme = ma.Name,
                                       Program = p.ProgramName,
                                       UserName = u.Firstname + " " + u.Lastname,
                                       WorkItem = "Disclaimer",
                                       DateFrom = dt_from,
                                       DateTo = dt_to,
                                       MemberID = d.idNumber,
                                       MemberNumber = m.membershipNo,
                                       DependantCode = d.dependentCode,
                                       MemberName = d.firstName + " " + d.lastName,
                                       ProgramCode = p.code,
                                       CreatedBy = it.createdBy,
                                       CreatedDate = it.createdDate,
                                       Active = a.Active

                                   }).ToList();

                foreach (var result in disclaimers) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("EM01")) //emails
            {
                var emails = (from e in _context.Emails
                              join d in _context.Dependants on e.dependantID equals d.DependantID into dpo
                              from d in dpo.DefaultIfEmpty()
                              join m in _context.Members on d.memberID equals m.memberID into mo
                              from m in mo.DefaultIfEmpty()
                              join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                              from ma in mao.DefaultIfEmpty()
                              join p in _context.Program on e.programId equals p.programID into po
                              from p in po.DefaultIfEmpty()
                              join u in _context.Users on e.createdBy equals u.username

                              where program_list.Contains(e.programId.ToString())
                              where user_list.Contains(e.createdBy)
                              where e.createdDate >= dt_from
                              where e.createdDate <= dt_to

                              select new ProductionResultsVM
                              {
                                  Id = e.emailId,
                                  DependantID = e.dependantID,
                                  SchemeID = ma.MedicalAidID,
                                  ProgramID = e.programId,
                                  UserID = u.userID,
                                  MedicalScheme = ma.Name,
                                  Program = p.ProgramName,
                                  UserName = u.Firstname + " " + u.Lastname,
                                  WorkItem = "Email",
                                  DateFrom = dt_from,
                                  DateTo = dt_to,
                                  MemberID = d.idNumber,
                                  MemberNumber = m.membershipNo,
                                  DependantCode = d.dependentCode,
                                  MemberName = d.firstName + " " + d.lastName,
                                  ProgramCode = p.code,
                                  CreatedBy = e.createdBy,
                                  CreatedDate = e.createdDate,
                                  Active = true

                              }).ToList();

                foreach (var result in emails) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("EN01")) //enquiries
            {
                var enquiries = (from q in _context.Queries
                                 join d in _context.Dependants on q.dependentID equals d.DependantID into dpo
                                 from d in dpo.DefaultIfEmpty()
                                 join m in _context.Members on d.memberID equals m.memberID into mo
                                 from m in mo.DefaultIfEmpty()
                                 join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                 from ma in mao.DefaultIfEmpty()
                                 join p in _context.Program on q.programId equals p.programID into po
                                 from p in po.DefaultIfEmpty()
                                 join u in _context.Users on q.resolvedBy equals u.username

                                 where program_list.Contains(q.programId.ToString())
                                 where user_list.Contains(q.resolvedBy)
                                 where q.resolutionDate >= dt_from
                                 where q.resolutionDate <= dt_to

                                 select new ProductionResultsVM
                                 {
                                     Id = q.queryID,
                                     DependantID = q.dependentID,
                                     SchemeID = ma.MedicalAidID,
                                     ProgramID = q.programId,
                                     UserID = u.userID,
                                     MedicalScheme = ma.Name,
                                     Program = p.ProgramName,
                                     UserName = u.Firstname + " " + u.Lastname,
                                     WorkItem = "Queries",
                                     DateFrom = dt_from,
                                     DateTo = dt_to,
                                     MemberID = d.idNumber,
                                     MemberNumber = m.membershipNo,
                                     DependantCode = d.dependentCode,
                                     MemberName = d.firstName + " " + d.lastName,
                                     ProgramCode = p.code,
                                     CreatedBy = q.resolvedBy,
                                     CreatedDate = q.resolutionDate,
                                     Active = true

                                 }).ToList();

                foreach (var result in enquiries) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("NO01")) //notes
            {
                var notes = (from n in _context.Notes
                             join d in _context.Dependants on n.dependentID equals d.DependantID into dpo
                             from d in dpo.DefaultIfEmpty()
                             join m in _context.Members on d.memberID equals m.memberID into mo
                             from m in mo.DefaultIfEmpty()
                             join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                             from ma in mao.DefaultIfEmpty()
                             join p in _context.Program on n.programId equals p.programID into po
                             from p in po.DefaultIfEmpty()
                             join u in _context.Users on n.createdBy equals u.username

                             where program_list.Contains(n.programId.ToString())
                             where user_list.Contains(n.createdBy)
                             where n.createdDate >= dt_from
                             where n.createdDate <= dt_to

                             select new ProductionResultsVM
                             {
                                 Id = n.noteID,
                                 DependantID = n.dependentID,
                                 SchemeID = ma.MedicalAidID,
                                 ProgramID = n.programId,
                                 UserID = u.userID,
                                 MedicalScheme = ma.Name,
                                 Program = p.ProgramName,
                                 UserName = u.Firstname + " " + u.Lastname,
                                 WorkItem = "Note",
                                 DateFrom = dt_from,
                                 DateTo = dt_to,
                                 MemberID = d.idNumber,
                                 MemberNumber = m.membershipNo,
                                 DependantCode = d.dependentCode,
                                 MemberName = d.firstName + " " + d.lastName,
                                 ProgramCode = p.code,
                                 CreatedBy = n.createdBy,
                                 CreatedDate = n.createdDate,
                                 Active = true

                             }).ToList();

                foreach (var result in notes) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("P001")) //pathology
            {
                var programs = "";
                foreach (string pr in program_list)
                {
                    var pinformation = _admin.GetProgramById(new Guid(pr));
                    programs += pinformation.code.Substring(0, 3) + ",";
                }

                var pathology = (from p in _context.Pathology
                                 join d in _context.Dependants on p.dependentID equals d.DependantID into dpo
                                 from d in dpo.DefaultIfEmpty()
                                 join m in _context.Members on d.memberID equals m.memberID into mo
                                 from m in mo.DefaultIfEmpty()
                                 join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                 from ma in mao.DefaultIfEmpty()
                                 join pr in _context.Program on p.pathologyType equals pr.code into po
                                 from pr in po.DefaultIfEmpty()
                                 join u in _context.Users on p.createdBy equals u.username

                                 where programs.Contains(p.pathologyType) || p.pathologyType.ToLower() == "gen"
                                 where user_list.Contains(p.createdBy)
                                 where p.createdDate >= dt_from
                                 where p.createdDate <= dt_to

                                 select new ProductionResultsVM
                                 {
                                     Id = p.pathologyID,
                                     DependantID = p.dependentID,
                                     SchemeID = ma.MedicalAidID,
                                     ProgramID = null,
                                     UserID = u.userID,
                                     MedicalScheme = ma.Name,
                                     Program = _context.Program.Where(x => x.code.Contains(p.pathologyType)).Select(x => x.ProgramName).FirstOrDefault(),
                                     UserName = u.Firstname + " " + u.Lastname,
                                     WorkItem = "Pathology",
                                     DateFrom = dt_from,
                                     DateTo = dt_to,
                                     MemberID = d.idNumber,
                                     MemberNumber = m.membershipNo,
                                     DependantCode = d.dependentCode,
                                     MemberName = d.firstName + " " + d.lastName,
                                     ProgramCode = p.pathologyType,
                                     CreatedBy = p.createdBy,
                                     CreatedDate = p.createdDate,
                                     Active = true

                                 }).ToList();



                foreach (var result in pathology) { if (result.ProgramCode == null || result.ProgramCode.ToUpper() == "GEN") { result.Program = "General"; } results.Add(result); }
            }
            if (workitems.ToUpper().Contains("QD01")) //diabetes-questionnaire
            {
                var questionnaire_diabetes = (from it in _context.AssignmentItemTasks
                                              join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id into aio
                                              from ai in aio.DefaultIfEmpty()
                                              join a in _context.Assignments on ai.assignmentId equals a.assignmentID into ao
                                              from a in ao.DefaultIfEmpty()
                                              join d in _context.Dependants on a.dependentID equals d.DependantID into dpo
                                              from d in dpo.DefaultIfEmpty()
                                              join m in _context.Members on d.memberID equals m.memberID into mo
                                              from m in mo.DefaultIfEmpty()
                                              join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                              from ma in mao.DefaultIfEmpty()
                                              join p in _context.Program on a.programId equals p.programID into po
                                              from p in po.DefaultIfEmpty()
                                              join u in _context.Users on it.closedBy equals u.username

                                              where program_list.Contains(a.programId.ToString())
                                              where user_list.Contains(it.closedBy)
                                              where it.taskTypeId == "007"
                                              where it.requirementId == 80
                                              where it.closedDate >= dt_from
                                              where it.closedDate <= dt_to

                                              select new ProductionResultsVM
                                              {
                                                  Id = a.assignmentID,
                                                  DependantID = a.dependentID,
                                                  SchemeID = ma.MedicalAidID,
                                                  ProgramID = a.programId,
                                                  UserID = u.userID,
                                                  MedicalScheme = ma.Name,
                                                  Program = p.ProgramName,
                                                  UserName = u.Firstname + " " + u.Lastname,
                                                  WorkItem = "Questionnaire: Diabetes",
                                                  DateFrom = dt_from,
                                                  DateTo = dt_to,
                                                  MemberID = d.idNumber,
                                                  MemberNumber = m.membershipNo,
                                                  DependantCode = d.dependentCode,
                                                  MemberName = d.firstName + " " + d.lastName,
                                                  ProgramCode = p.code,
                                                  CreatedBy = it.createdBy,
                                                  CreatedDate = it.createdDate,
                                                  Active = a.Active

                                              }).ToList();

                foreach (var result in questionnaire_diabetes) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("QH01")) //hiv-questionnaire
            {
                var questionnaire_hiv = (from it in _context.AssignmentItemTasks
                                         join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id into aio
                                         from ai in aio.DefaultIfEmpty()
                                         join a in _context.Assignments on ai.assignmentId equals a.assignmentID into ao
                                         from a in ao.DefaultIfEmpty()
                                         join d in _context.Dependants on a.dependentID equals d.DependantID into dpo
                                         from d in dpo.DefaultIfEmpty()
                                         join m in _context.Members on d.memberID equals m.memberID into mo
                                         from m in mo.DefaultIfEmpty()
                                         join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                         from ma in mao.DefaultIfEmpty()
                                         join p in _context.Program on a.programId equals p.programID into po
                                         from p in po.DefaultIfEmpty()
                                         join u in _context.Users on it.closedBy equals u.username

                                         where program_list.Contains(a.programId.ToString())
                                         where user_list.Contains(it.closedBy)
                                         where it.taskTypeId == "007"
                                         where it.requirementId == 75
                                         where it.closedDate >= dt_from
                                         where it.closedDate <= dt_to

                                         select new ProductionResultsVM
                                         {
                                             Id = a.assignmentID,
                                             DependantID = a.dependentID,
                                             SchemeID = ma.MedicalAidID,
                                             ProgramID = a.programId,
                                             UserID = u.userID,
                                             MedicalScheme = ma.Name,
                                             Program = p.ProgramName,
                                             UserName = u.Firstname + " " + u.Lastname,
                                             WorkItem = "Questionnaire: HIV",
                                             DateFrom = dt_from,
                                             DateTo = dt_to,
                                             MemberID = d.idNumber,
                                             MemberNumber = m.membershipNo,
                                             DependantCode = d.dependentCode,
                                             MemberName = d.firstName + " " + d.lastName,
                                             ProgramCode = p.code,
                                             CreatedBy = it.createdBy,
                                             CreatedDate = it.createdDate,
                                             Active = a.Active

                                         }).ToList();

                foreach (var result in questionnaire_hiv) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("QM01")) //dsm5-questionnaire
            {
                var questionnaire_dsm5 = (from it in _context.AssignmentItemTasks
                                          join ai in _context.AssignmentItems on it.assignmentItemID equals ai.id into aio
                                          from ai in aio.DefaultIfEmpty()
                                          join a in _context.Assignments on ai.assignmentId equals a.assignmentID into ao
                                          from a in ao.DefaultIfEmpty()
                                          join d in _context.Dependants on a.dependentID equals d.DependantID into dpo
                                          from d in dpo.DefaultIfEmpty()
                                          join m in _context.Members on d.memberID equals m.memberID into mo
                                          from m in mo.DefaultIfEmpty()
                                          join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                          from ma in mao.DefaultIfEmpty()
                                          join p in _context.Program on a.programId equals p.programID into po
                                          from p in po.DefaultIfEmpty()
                                          join u in _context.Users on it.closedBy equals u.username

                                          where program_list.Contains(a.programId.ToString())
                                          where user_list.Contains(it.closedBy)
                                          where it.taskTypeId == "007"
                                          where it.requirementId == 82 || it.requirementId == 83  //uat-id is 82 / live-id is 83
                                          where it.closedDate >= dt_from
                                          where it.closedDate <= dt_to

                                          select new ProductionResultsVM
                                          {
                                              Id = a.assignmentID,
                                              DependantID = a.dependentID,
                                              SchemeID = ma.MedicalAidID,
                                              ProgramID = a.programId,
                                              UserID = u.userID,
                                              MedicalScheme = ma.Name,
                                              Program = p.ProgramName,
                                              UserName = u.Firstname + " " + u.Lastname,
                                              WorkItem = "Questionnaire: DSM5",
                                              DateFrom = dt_from,
                                              DateTo = dt_to,
                                              MemberID = d.idNumber,
                                              MemberNumber = m.membershipNo,
                                              DependantCode = d.dependentCode,
                                              MemberName = d.firstName + " " + d.lastName,
                                              ProgramCode = p.code,
                                              CreatedBy = it.createdBy,
                                              CreatedDate = it.createdDate,
                                              Active = a.Active

                                          }).ToList();

                foreach (var result in questionnaire_dsm5) { results.Add(result); }

            }
            if (workitems.ToUpper().Contains("SM01")) //sms
            {
                var sms = (from s in _context.SmsMessages
                           join d in _context.Dependants on s.dependantID equals d.DependantID into dpo
                           from d in dpo.DefaultIfEmpty()
                           join m in _context.Members on d.memberID equals m.memberID into mo
                           from m in mo.DefaultIfEmpty()
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                           from ma in mao.DefaultIfEmpty()
                           join p in _context.Program on s.programId equals p.programID into po
                           from p in po.DefaultIfEmpty()
                           join u in _context.Users on s.createdBy equals u.username

                           where program_list.Contains(s.programId.ToString()) || s.programId.ToString() == "00000000-0000-0000-0000-000000000000"
                           where user_list.Contains(s.createdBy)
                           where s.createdDate >= dt_from
                           where s.createdDate <= dt_to

                           select new ProductionResultsVM
                           {
                               Id = s.messageID,
                               DependantID = s.dependantID,
                               SchemeID = ma.MedicalAidID,
                               ProgramID = s.programId,
                               UserID = u.userID,
                               MedicalScheme = ma.Name,
                               Program = p.ProgramName,
                               UserName = u.Firstname + " " + u.Lastname,
                               WorkItem = "SMS",
                               DateFrom = dt_from,
                               DateTo = dt_to,
                               MemberID = d.idNumber,
                               MemberNumber = m.membershipNo,
                               DependantCode = d.dependentCode,
                               MemberName = d.firstName + " " + d.lastName,
                               ProgramCode = p.code,
                               CreatedBy = s.createdBy,
                               CreatedDate = s.createdDate,
                               Active = true

                           }).ToList();

                foreach (var result in sms) { results.Add(result); }
            }
            if (workitems.ToUpper().Contains("WF01")) //workflow
            {
                var workflow = (from wf in _context.wfUserLogs
                                join d in _context.Dependants on wf.DependantID equals d.DependantID into dpo
                                from d in dpo.DefaultIfEmpty()
                                join m in _context.Members on d.memberID equals m.memberID into mo
                                from m in mo.DefaultIfEmpty()
                                join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID into mao
                                from ma in mao.DefaultIfEmpty()
                                join p in _context.Program on wf.ProgramID equals p.programID into po
                                from p in po.DefaultIfEmpty()
                                join u in _context.Users on wf.ModifiedBy equals u.username

                                where program_list.Contains(wf.ProgramID.ToString())
                                where user_list.Contains(wf.ModifiedBy)
                                where wf.ModifiedDate >= dt_from
                                where wf.ModifiedDate <= dt_to
                                where wf.Status.ToLower() == "closed"

                                select new ProductionResultsVM
                                {
                                    Id = wf.Id,
                                    DependantID = wf.DependantID,
                                    SchemeID = ma.MedicalAidID,
                                    ProgramID = wf.ProgramID,
                                    UserID = u.userID,
                                    MedicalScheme = ma.Name,
                                    Program = p.ProgramName,
                                    UserName = u.Firstname + " " + u.Lastname,
                                    WorkItem = "Workflow",
                                    DateFrom = dt_from,
                                    DateTo = dt_to,
                                    MemberID = d.idNumber,
                                    MemberNumber = m.membershipNo,
                                    DependantCode = d.dependentCode,
                                    MemberName = d.firstName + " " + d.lastName,
                                    ProgramCode = p.code,
                                    CreatedBy = wf.ModifiedBy,
                                    CreatedDate = wf.ModifiedDate,
                                    Active = true

                                }).ToList();

                foreach (var result in workflow) { results.Add(result); }

            }
            #endregion

            results = (from r in results where scheme_list.Contains(r.SchemeID.ToString()) select r).ToList(); //allowed-medical-aid-filter

            //allowed-program-filter
            var programValidation = new List<ProductionResultsVM>();
            foreach (var result in results)
            {
                var options = GetAllowedProgramsPerScheme(result.SchemeID);
                foreach (var o in options)
                {
                    if (result.WorkItem.ToLower() == "pathology")
                    {
                        if ((result.ProgramCode.Substring(0, 3) == o.code.Substring(0, 3) || result.Program.ToLower() == "general"))
                        {
                            programValidation.Add(result);
                        }
                    }
                    if ((result.ProgramID == o.programID))
                    {
                        programValidation.Add(result);
                    }
                }
            }

            results = programValidation;

            return results.OrderBy(x => x.MemberID).ThenBy(x => x.Program).ThenBy(x => x.WorkItem).ThenByDescending(x => x.CreatedDate).ToList();

        }
        public List<WorkflowUser> GetUserWorkflowRecords(Guid UserID) //HCare-1134
        {
            var workflowSettings = _context.WorkflowSettings.Where(x => x.UserID == UserID).Where(x => x.Active == true).ToList();

            foreach (var setting in workflowSettings)
            {
                var medicalscheme = "";
                var program = "";
                var managementStatus = "";
                var fromDate = "";
                var toDate = "";
                var riskRating = "";

                medicalscheme = setting.MedicalAidID.ToString();
                program = setting.ProgramID.ToString();
                managementStatus = setting.ManagementStatus;
                fromDate = setting.FromDate.ToString();
                toDate = setting.ToDate.ToString();
                riskRating = setting.RiskRating;

            }

            string sql = string.Format(@"SET DATEFORMAT YMD
                                        SELECT ma.Name[MedicalScheme], pr.ProgramName[Program], m.membershipNo[MembershipNumber], d.dependentCode[DependantCode], d.firstName[FirstName], d.lastName[LastName], ms.statusName[ManagementStatus],
                                        (SELECT top 1 RiskId FROM PatientRiskRatingHistory  WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) AS RiskRating
                                        
                                        FROM Member m 
                                        INNER JOIN Dependant d ON m.memberID = d.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        AND (s.endDate > getdate()
                                        OR s.endDate is null
                                        OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus  WHERE statusType IN ('C', 'P', 'O') AND active = 1) 
                                        AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM  PatientManagementStatusHistory  WHERE active = 1 AND dependantId = d.DependantID)))
                                        AND s.active = 1
                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                        
                                        WHERE ma.Active = 1
                                        AND m.active = 1
                                        AND d.Active = 1");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<WorkflowUser>)db.Query<WorkflowUser>(sql, null, commandTimeout: 5000);
                db.Close();

                return results;
            }

        }


        public List<WorkflowUser> InsertUserWorkflowRecords(Guid userID, string medicalscheme, string program, string managementStatus, string fromDate, string toDate, string riskRating, string assignment, string instruction) //HCare-1134
        {

            #region sql-get-workflow-user
            string sql = string.Format(@"SET DATEFORMAT YMD
                                        SELECT (SELECT userID FROM Users  WHERE active = 1 AND userID = '{0}') AS UserID,
                                        ma.Name[MedicalScheme], pr.ProgramName[Program], m.membershipNo[MembershipNumber], d.DependantID[DependantID], d.dependentCode[DependantCode], d.title[Title], d.firstName[FirstName], d.lastName[LastName], ms.statusName[ManagementStatus],
                                        (SELECT top 1 RiskId FROM PatientRiskRatingHistory  WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) AS RiskRating,
                                        (SELECT top 1 Instruction FROM WorkflowSettings  WHERE Active = 1 AND UserID = '{0}' AND MedicalAidID = '{1}' AND ProgramID = '{2}' AND ManagementStatus = '{5}') AS Instruction

                                        FROM Member m 
                                        INNER JOIN Dependant d ON m.memberID = d.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        AND (s.endDate > getdate()
                                        OR s.endDate is null
                                        OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus  WHERE statusType IN ('C', 'P', 'O') AND active = 1) 
                                        AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM  PatientManagementStatusHistory  WHERE active = 1 AND dependantId = d.DependantID)))
                                        AND s.active = 1
                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code

                                        WHERE ma.Active = 1
                                        AND m.active = 1
                                        AND d.Active = 1
                                        AND ma.MedicalAidID = '{1}'
                                        AND pr.programID = '{2}'
                                        AND s.effectiveDate BETWEEN '{3}' AND '{4}'", userID, medicalscheme, program, fromDate, toDate, managementStatus);

            if (!String.IsNullOrEmpty(managementStatus))
            {
                sql = sql + string.Format(" AND s.managementStatusCode LIKE '%{0}%'", managementStatus);
            }

            #endregion

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<WorkflowUser>)db.Query<WorkflowUser>(sql, null, commandTimeout: 5000);
                db.Close();

                var wf1 = new List<WorkflowUser>();

                foreach (var result in results)
                {
                    result.Open = true;
                    wf1.Add(result);
                }
                #region risk-rating-filter
                var wf2 = new List<WorkflowUser>();
                var remove = new List<WorkflowUser>();

                foreach (var item in wf1.Where(x => x.UserID == userID))
                {
                    var exists = 0;
                    if (item.RiskRating != riskRating)
                    {
                        remove.Add(item);
                    }
                    else
                    {
                        var existing = _context.WorkflowUsers.Where(x => x.UserID == userID).ToList();
                        if (existing.Count == 0)
                        {
                            wf2.Add(item);
                        }
                        else
                        {
                            foreach (var record in existing)
                            {
                                if (record.UserID + record.MedicalScheme + record.Program + record.MembershipNumber + record.DependantCode + record.RiskRating == item.UserID + item.MedicalScheme + item.Program + item.MembershipNumber + item.DependantCode + item.RiskRating)
                                {
                                    exists++;
                                }
                            }
                            if (exists == 0)
                            {
                                wf2.Add(item);
                            }
                        }
                    }
                }
                #endregion
                #region assignment-filter
                wf1 = wf2;
                var assignments = new List<Assignments>();
                var wf3 = new List<WorkflowUser>();
                var removea = new List<WorkflowUser>();

                if (!String.IsNullOrEmpty(assignment))
                {
                    assignments = (from ai in _context.AssignmentItems
                                   join a in _context.Assignments on ai.assignmentId equals a.assignmentID
                                   join d in _context.Dependants on a.dependentID equals d.DependantID
                                   join m in _context.Members on d.memberID equals m.memberID
                                   where a.Active == true
                                   where ai.itemType == assignment

                                   select a).ToList();
                }
                else
                {
                    wf3 = wf2;
                    foreach (var wf in wf3.ToList())
                    {
                        wf3.Add(wf);
                        _context.WorkflowUsers.Add(wf);
                        _context.SaveChanges();
                    }
                }

                foreach (var a in assignments)
                {
                    foreach (var wf in wf2)
                    {
                        if (wf.DependantID == a.dependentID)
                        {
                            wf3.Add(wf);
                            _context.WorkflowUsers.Add(wf);
                            _context.SaveChanges();
                        }
                    }
                }
                #endregion
                return wf3;
            }

        }


        public List<wfQueue> InsertWFQueue(Guid queueID, string medicalscheme, string program, string managementStatus, string fromDate, string toDate, string riskRating, string assignment, string instruction) //HCare-1134
        {

            #region sql-get-workflow-user
            string sql = string.Format(@"SET DATEFORMAT YMD

                                        SELECT (SELECT DISTINCT QueueID FROM wfSettings WHERE Active = 1 AND QueueID = '{0}') AS QueueID,
                                        (SELECT UserID FROM wfQueue WHERE dependantId = d.DependantID) AS UserID,
                                        ma.MedicalAidID[SchemeID], ma.Name[MedicalScheme], pr.programID[ProgramID], pr.ProgramName[Program], m.membershipNo[MembershipNumber], d.DependantID[DependantID], d.dependentCode[DependantCode], d.title[Title], d.firstName[FirstName], d.lastName[LastName], ms.statusName[ManagementStatus],
                                        (SELECT top 1 RiskId FROM PatientRiskRatingHistory  WHERE active = 1 AND dependantId = d.DependantID AND programType = pr.code ORDER BY effectiveDate DESC) AS RiskRating,
                                        (SELECT top 1 Instruction FROM wfSettings  WHERE Active = 1 AND MedicalAidID = '{1}' AND ProgramID = '{2}' AND ManagementStatus = '{5}') AS Instruction

                                        FROM Member m 
                                        INNER JOIN Dependant d ON m.memberID = d.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        LEFT OUTER JOIN PatientManagementStatusHistory s ON d.DependantID = s.dependantId
                                        AND (s.endDate > getdate()
                                        OR s.endDate is null
                                        OR (s.managementStatusCode IN (SELECT statusCode FROM ManagementStatus  WHERE statusType IN ('C', 'P', 'O') AND active = 1) 
                                        AND s.effectiveDate = (SELECT MAX(effectiveDate) FROM  PatientManagementStatusHistory  WHERE active = 1 AND dependantId = d.DependantID)))
                                        AND s.active = 1
                                        LEFT OUTER JOIN ManagementStatus ms on s.managementStatusCode = ms.statusCode              
                                        LEFT OUTER JOIN Programs pr ON ms.programCode = pr.code
                                        
                                        WHERE ma.Active = 1
                                        AND m.active = 1
                                        AND d.Active = 1
                                        AND ma.MedicalAidID = '{1}'
                                        AND pr.programID = '{2}'
                                        AND s.effectiveDate BETWEEN '{3}' AND '{4}'", queueID, medicalscheme, program, fromDate, toDate, managementStatus);

            if (!String.IsNullOrEmpty(managementStatus))
            {
                sql = sql + string.Format(" AND s.managementStatusCode LIKE '%{0}%'", managementStatus);
            }

            #endregion

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfQueue>)db.Query<wfQueue>(sql, null, commandTimeout: 5000);
                db.Close();

                results = (from r in results
                           where r.UserID == null
                           select r).ToList();

                if (results.Count != 0)
                {
                    foreach (var result in results)
                    {
                        result.Status = "Open";
                        if (result.RiskRating != null) { result.RiskRating = _context.RiskRatingTypes.Where(x => x.RiskType == result.RiskRating).Select(x => x.RiskName).FirstOrDefault(); } else { result.RiskRating = "NONE"; }
                    }
                    #region risk-rating-filter
                    if (!String.IsNullOrEmpty(riskRating))
                    {
                        string[] riskratings = riskRating.Split(',');
                        results = (from r in results
                                   where riskratings.Contains(r.RiskRating)
                                   select r).ToList();
                    }
                    #endregion
                    #region pathology-filter
                    var newResults = new List<wfQueue>();
                    var pathologyRules = _context.wfSettings.Where(x => x.QueueID == queueID).ToList();
                    foreach (var result in results)
                    {
                        #region pathology-fields
                        var pathology = GetWFPathology(result.DependantID, result.ProgramID);
                        var viralload = pathology[0].ViralLoad;
                        var cd4count = pathology[0].CD4Count;
                        var cd4percentage = pathology[0].CD4Percentage;
                        var hba1c = pathology[0].HbA1c;
                        var haemoglobin = pathology[0].Haemoglobin;
                        var totalcholestrol = pathology[0].TotalCholestrol;
                        var hdl = pathology[0].HDL;
                        var ldl = pathology[0].LDL;
                        var triglycerides = pathology[0].Triglycerides;
                        var glucose = pathology[0].Glucose;
                        var alt = pathology[0].ALT;
                        var ast = pathology[0].AST;
                        var egfr = pathology[0].eGFR;
                        var bilirubin = pathology[0].Bilirubin;
                        var maucreatratio = pathology[0].MauCreatRatio;
                        var systolicBP = pathology[0].systolicBP;
                        var diastolicBP = pathology[0].diastolicBP;
                        var fev1 = pathology[0].FEV1;
                        var eosinophylia = pathology[0].Eosinophylia;
                        var ucreatinine = pathology[0].UCreatinine;
                        var salbumin = pathology[0].SAlbumin;
                        var ualbuminuria = pathology[0].UAlbuminuria;
                        var urea = pathology[0].Urea;
                        var creatinine = pathology[0].Creatinine;
                        #endregion
                        foreach (var rule in pathologyRules)
                        {
                            if ((rule.Less.ToString() != "0.00") && (rule.Greater.ToString() != "0.00"))
                            {
                                if (rule.PathologyField.ToLower() == "viralload" && (viralload > rule.Less && viralload < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4count" && (cd4count > rule.Less && cd4count < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4percentage" && (cd4percentage > rule.Less && cd4percentage < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hba1c" && (hba1c > rule.Less && hba1c < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "haemoglobin" && (haemoglobin > rule.Less && haemoglobin < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "totalcholestrol" && (totalcholestrol > rule.Less && totalcholestrol < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hdl" && (hdl > rule.Less && hdl < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ldl" && (ldl > rule.Less && ldl < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "triglycerides" && (triglycerides > rule.Less && triglycerides < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "glucose" && (glucose > rule.Less && glucose < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "alt" && (alt > rule.Less && alt < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ast" && (ast > rule.Less && ast < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "egfr" && (egfr > rule.Less && egfr < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "bilirubin" && (bilirubin > rule.Less && bilirubin < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "maucreatratio" && (maucreatratio > rule.Less && maucreatratio < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "systolicBP" && (systolicBP > rule.Less && systolicBP < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "diastolicBP" && (diastolicBP > rule.Less && diastolicBP < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "fev1" && (fev1 > rule.Less && fev1 < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "eosinophylia" && (eosinophylia > rule.Less && eosinophylia < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ucreatinine" && (ucreatinine > rule.Less && ucreatinine < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "salbumin" && (salbumin > rule.Less && salbumin < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ualbuminuria" && (ualbuminuria > rule.Less && ualbuminuria < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "urea" && (urea > rule.Less && urea < rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "creatinine" && (creatinine > rule.Less && creatinine < rule.Greater)) { newResults.Add(result); continue; }
                            }
                            else if (rule.Less.ToString() != "0.00")
                            {
                                if (rule.PathologyField.ToLower() == "viralload" && (viralload < rule.Less && viralload != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4count" && (cd4count < rule.Less && cd4count != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4percentage" && (cd4percentage < rule.Less && cd4percentage != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hba1c" && (hba1c < rule.Less && hba1c != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "haemoglobin" && (haemoglobin < rule.Less && haemoglobin != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "totalcholestrol" && (totalcholestrol < rule.Less && totalcholestrol != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hdl" && (hdl < rule.Less && hdl != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ldl" && (ldl < rule.Less && ldl != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "triglycerides" && (triglycerides < rule.Less && triglycerides != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "glucose" && (glucose < rule.Less && glucose != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "alt" && (alt < rule.Less && alt != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ast" && (ast < rule.Less && ast != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "egfr" && (egfr < rule.Less && egfr != 0)) { newResults.Add(result); }
                                if (rule.PathologyField.ToLower() == "bilirubin" && (bilirubin < rule.Less && bilirubin != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "maucreatratio" && (maucreatratio < rule.Less && maucreatratio != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "systolicBP" && (systolicBP < rule.Less && systolicBP != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "diastolicBP" && (diastolicBP < rule.Less && diastolicBP != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "fev1" && (fev1 < rule.Less && fev1 != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "eosinophylia" && (eosinophylia < rule.Less && eosinophylia != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ucreatinine" && (ucreatinine < rule.Less && ucreatinine != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "salbumin" && (salbumin < rule.Less && salbumin != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ualbuminuria" && (ualbuminuria < rule.Less && ualbuminuria != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "urea" && (urea < rule.Less && urea != 0)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "creatinine" && (creatinine < rule.Less && creatinine != 0)) { newResults.Add(result); continue; }
                            }
                            else if (rule.Greater.ToString() != "0.00")
                            {
                                if (rule.PathologyField.ToLower() == "viralload" && (viralload > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4count" && (cd4count > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "cd4percentage" && (cd4percentage > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hba1c" && (hba1c > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "haemoglobin" && (haemoglobin > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "totalcholestrol" && (totalcholestrol > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "hdl" && (hdl > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ldl" && (ldl > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "triglycerides" && (triglycerides > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "glucose" && (glucose > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "alt" && (alt > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ast" && (ast > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "egfr" && (egfr > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "bilirubin" && (bilirubin > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "maucreatratio" && (maucreatratio > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "systolicBP" && (systolicBP > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "diastolicBP" && (diastolicBP > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "fev1" && (fev1 > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "eosinophylia" && (eosinophylia > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ucreatinine" && (ucreatinine > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "salbumin" && (salbumin > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "ualbuminuria" && (ualbuminuria > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "urea" && (urea > rule.Greater)) { newResults.Add(result); continue; }
                                if (rule.PathologyField.ToLower() == "creatinine" && (creatinine > rule.Greater)) { newResults.Add(result); continue; }
                            }
                        }
                    }

                    var distinct = newResults.GroupBy(p => p.DependantID).Select(g => g.First()).ToList();

                    results = distinct;

                    #endregion
                    #region assignment-filter
                    if (results.Count > 0)
                    {
                        var assignments = new List<Assignments>();
                        var wfResults = new List<wfQueue>();

                        if (!String.IsNullOrEmpty(assignment))
                        {
                            assignments = (from ai in _context.AssignmentItems
                                           join a in _context.Assignments on ai.assignmentId equals a.assignmentID
                                           join d in _context.Dependants on a.dependentID equals d.DependantID
                                           join m in _context.Members on d.memberID equals m.memberID
                                           where a.Active == true
                                           where ai.itemType == assignment

                                           select a).ToList();
                        }
                        else
                        {
                            wfResults = results;
                            foreach (var wf in wfResults.ToList())
                            {
                                wfResults.Add(wf);
                                _context.wfQueues.Add(wf);
                                _context.SaveChanges();
                            }
                        }

                        foreach (var a in assignments)
                        {
                            foreach (var wf in results)
                            {
                                if (wf.DependantID == a.dependentID)
                                {
                                    wfResults.Add(wf);
                                    _context.wfQueues.Add(wf);
                                    _context.SaveChanges();
                                }
                            }
                        }

                        results = wfResults;
                    }
                    #endregion
                    #region round-robin
                    if (results.Where(x => x.UserID == null).Count() > 0)
                    {
                        var wfQueue = _context.wfQueues.Where(x => x.UserID == null);
                        var userQueue = _admin.GetWFUserQueueList().Where(x => x.QueueID == queueID).OrderBy(x => x.ShuffleDate);
                        foreach (var result in wfQueue)
                        {
                            foreach (var user in userQueue)
                            {
                                _admin.UpdateWFUserID(result.Id, user.UserID);
                                _admin.UpdateUserQueueShuffleDate(user.UserID, user.QueueID);
                                break;
                            }
                        }
                    }
                    #endregion
                    return results;
                }

                return results;
            }
        }

        public List<wfPathologyVM> GetWFPathology(Guid dependantID, Guid programID) //HCare-1134
        {
            var program = _context.Program.Where(x => x.programID == programID).FirstOrDefault();
            var programcode = program.code.Substring(0, 3);

            string sql = string.Format(@"Select
                                        (SELECT top 1 viralLoad from Pathology where viralLoadeffectiveDate = (select MAX(viralLoadeffectiveDate) from Pathology WHERE viralLoad is not NULL AND viralLoad not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS ViralLoad,
                                        (SELECT top 1 CD4Count from Pathology where CD4CounteffectiveDate = (select MAX(CD4CounteffectiveDate) from Pathology WHERE CD4Count is not NULL AND CD4Count not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1)AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS CD4Count,
                                        (SELECT top 1 CD4Percentage from Pathology where CD4PercentageeffectiveDate = (select MAX(CD4PercentageeffectiveDate) from Pathology WHERE CD4Percentage is not NULL AND CD4Percentage not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1)AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS CD4Percentage,
                                        (SELECT top 1 hba1c from Pathology where hba1ceffectiveDate = (select MAX(hba1ceffectiveDate) from Pathology WHERE hba1c is not NULL AND hba1c not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1)AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS HbA1c,
                                        (SELECT top 1 haemoglobin from Pathology where haemoglobineffectiveDate = (select MAX(haemoglobineffectiveDate) from Pathology WHERE haemoglobin is not NULL AND haemoglobin not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1)AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Haemoglobin,
                                        (SELECT top 1 totalCholestrol from Pathology where totalCholestroleffectiveDate = (select MAX(totalCholestroleffectiveDate) from Pathology WHERE totalCholestrol is not NULL AND totalCholestrol not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1)AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS TotalCholestrol,
                                        (SELECT top 1 hdl from Pathology where hdleffectiveDate = (select MAX(hdleffectiveDate) from Pathology WHERE hdl is not NULL AND hdl not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS HDL,
                                        (SELECT top 1 ldl from Pathology where ldleffectiveDate = (select MAX(ldleffectiveDate) from Pathology WHERE ldl is not NULL AND ldl not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS LDL,
                                        (SELECT top 1 triglycerides from Pathology where triglycerideseffectiveDate = (select MAX(triglycerideseffectiveDate) from Pathology WHERE triglycerides is not NULL AND triglycerides not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Triglycerides,
                                        (SELECT top 1 glucose from Pathology where glucoseeffectiveDate = (select MAX(glucoseeffectiveDate) from Pathology WHERE glucose is not NULL AND glucose not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Glucose,
                                        (SELECT top 1 alt from Pathology where alteffectiveDate = (select MAX(alteffectiveDate) from Pathology WHERE alt is not NULL AND alt not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS ALT,
                                        (SELECT top 1 ast from Pathology where asteffectiveDate = (select MAX(asteffectiveDate) from Pathology WHERE ast is not NULL AND ast not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS AST,
                                        (SELECT top 1 eGfr from Pathology where eGfreffectiveDate = (select MAX(eGfreffectiveDate) from Pathology WHERE eGfr is not NULL AND eGfr not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS eGFR,
                                        (SELECT top 1 bilirubin from Pathology where bilirubineffectiveDate = (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Bilirubin,
                                        (SELECT top 1 bilirubin from Pathology where bilirubineffectiveDate = (select MAX(bilirubineffectiveDate) from Pathology WHERE bilirubin is not NULL AND bilirubin not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS MauCreatRatio,
                                        (SELECT top 1 systolicBP from Pathology where BPeffectiveDate = (select MAX(BPeffectiveDate) from Pathology WHERE systolicBP is not NULL AND systolicBP not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS systolicBP,                                        
                                        (SELECT top 1 diastolicBP from Pathology where BPeffectiveDate = (select MAX(BPeffectiveDate) from Pathology WHERE diastolicBP is not NULL AND diastolicBP not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS diastolicBP,                                        
                                        (SELECT top 1 FEV1 from Pathology where FEV1effectiveDate = (select MAX(FEV1effectiveDate) from Pathology WHERE FEV1 is not NULL AND FEV1 not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%'AND Active = 1) AS FEV1,
                                        (SELECT top 1 Eosinophylia from Pathology where EosinophyliaeffectiveDate = (select MAX(EosinophyliaeffectiveDate) from Pathology WHERE Eosinophylia is not NULL AND Eosinophylia not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Eosinophylia,
                                        --(SELECT top 1 hivEliza from Pathology where hivElizaeffectiveDate = (select MAX(hivElizaeffectiveDate) from Pathology WHERE hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND hivEliza is not NULL AND hivEliza not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS HIVEliza,
                                        --(SELECT top 1 normGtt from Pathology where normGtteffectiveDate = (select MAX(normGtteffectiveDate) from Pathology WHERE normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND normGtt is not NULL AND normGtt not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS NormalGtt,
                                        --(SELECT top 1 abnGtt from Pathology where abnGtteffectiveDate = (select MAX(abnGtteffectiveDate) from Pathology WHERE abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND abnGtt is not NULL AND abnGtt not like '0' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS AbnormalGtt,
                                        (SELECT top 1 ucreatinine from Pathology where ucreatinineeffectiveDate = (select MAX(ucreatinineeffectiveDate) from Pathology WHERE ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND ucreatinine is not NULL AND ucreatinine not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS UCreatinine,
                                        (SELECT top 1 salbumin from Pathology where salbumineffectiveDate = (select MAX(salbumineffectiveDate) from Pathology WHERE salbumin is not NULL AND salbumin not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND salbumin is not NULL AND salbumin not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS SAlbumin,
                                        (SELECT top 1 ualbuminuria from Pathology where ualbuminuriaeffectiveDate= (select MAX(ualbuminuriaeffectiveDate) from Pathology WHERE ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND ualbuminuria is not NULL AND ualbuminuria not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS UAlbuminuria,
                                        (SELECT top 1 urea from Pathology where ureaeffectiveDate= (select MAX(ureaeffectiveDate) from Pathology WHERE urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND urea is not NULL AND urea not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Urea,
                                        (SELECT top 1 creatinine from Pathology where creatinineeffectiveDate= (select MAX(creatinineeffectiveDate) from Pathology WHERE creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AND creatinine is not NULL AND creatinine not like '0.00' AND dependentID = '{0}' AND pathologyType LIKE '{1}%' AND Active = 1) AS Creatinine", dependantID, programcode);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfPathologyVM>)db.Query<wfPathologyVM>(sql, null, commandTimeout: 5000);
                db.Close();

                return results;
            }
        }

        public List<WorkflowUser> DeleteWorkflow(Guid userID) //HCare-1134
        {
            string sql1 = string.Format(@"DELETE WorkflowUser where UserID = '{0}' AND InProgress = 0 AND Closed = 0", userID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<WorkflowUser>)db.Query<WorkflowUser>(sql1, null, commandTimeout: 5000);
                db.Close();

                return results;
            }
        }

        public List<wfUserVM> GetUserWorkflowIndex(Guid userID)
        {
            string sql = string.Format(@"SELECT wfq.Id, wfq.QueueID[QueueID], wfq.UserID[UserID], wfs.QueueName[QueueName], ma.Name[MedicalScheme], ma.MedicalAidID[MedicalAidID], p.ProgramName[Program], p.programID[ProgramID], m.membershipNo[MembershipNumber], d.DependantID[DependantID], d.dependentCode[DependantCode], d.title[Title], d.firstName[UserFirstName], 
                                        d.lastName[UserLastName], wfq.Instruction[Instruction], wfq.Status[Status], wfq.RiskRating[RiskRating],
                                        wfq.ManagementStatus[ManagementStatus],
										(SELECT top 1 ms.statusCode from wfQueue wf INNER JOIN ManagementStatus ms on wf.ManagementStatus = ms.statusName where wf.dependantId = d.DependantID)[ManagementStatusCode]
                                        --(SELECT top 1 ms.statusName from PatientManagementStatusHistory msh INNER JOIN ManagementStatus ms on msh.managementStatusCode = ms.statusCode where msh.dependantId = d.DependantID order by effectiveDate desc)[ManagementStatus],
										--(SELECT top 1 ms.statusCode from PatientManagementStatusHistory msh INNER JOIN ManagementStatus ms on msh.managementStatusCode = ms.statusCode where msh.dependantId = d.DependantID order by effectiveDate desc)[ManagementStatusCode]
                                        FROM wfQueue wfq
                                        INNER JOIN Dependant d ON d.DependantID = wfq.DependantID
                                        INNER JOIN Member m ON d.memberID = m.memberID
                                        INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfq.ProgramID = p.programID
                                        INNER JOIN wfSettings wfs on wfq.QueueID = wfs.QueueID
                                        
                                        WHERE wfq.UserID = '{0}'", userID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfUserVM>)db.Query<wfUserVM>(sql, null, commandTimeout: 10000);
                db.Close();

                return results.Where(x => x.Status != "Closed").OrderByDescending(x => x.ManagementStatusCode.Contains("ER")).ThenBy(x => x.Status).ThenBy(x => x.QueueID).ToList(); //hcare-1315

            }

        }

        public wfUserVM GetUserWorkflowTask(Guid dependantID)
        {
            var results = (from wfq in _context.wfQueues
                           join d in _context.Dependants on wfq.DependantID equals d.DependantID
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           join p in _context.Program on wfq.ProgramID equals p.programID
                           where wfq.DependantID == dependantID
                           where wfq.UserID != null //hcare-1315: update
                           where wfq.Status.ToLower() != "closed" //hcare-1315: update


                           select new wfUserVM
                           {
                               Id = wfq.Id,
                               UserID = wfq.UserID,
                               DependantID = wfq.DependantID,
                               QueueID = wfq.QueueID,
                               MedicalScheme = ma.Name,
                               MedicalAidID = ma.MedicalAidID,
                               Program = p.ProgramName,
                               ProgramID = p.programID,
                               ManagementStatus = wfq.ManagementStatus,
                               MembershipNumber = wfq.MembershipNumber,
                               DependantCode = wfq.DependantCode,
                               UserFirstName = wfq.FirstName,
                               UserLastName = wfq.LastName,
                               Title = wfq.Title,
                               Instruction = wfq.Instruction,
                               Status = wfq.Status,
                               RiskRating = wfq.RiskRating,

                           }).ToList();

            return results.FirstOrDefault();
        }

        public wfQueue GetUserWFById(int Id, Guid UserID, Guid medicalaidID, Guid programID, string managementstatus, string membershipnumber, string dependantcode, string riskrating) //hcare-1311
        {
            var risk = _context.RiskRatingTypes.Where(x => x.RiskName == riskrating).FirstOrDefault();
            var results = _context.wfQueues.Where(x => x.Id == Id).Where(x => x.UserID == UserID).Where(x => x.SchemeID == medicalaidID).Where(x => x.ProgramID == programID)
                .Where(x => x.MembershipNumber == membershipnumber).Where(x => x.DependantCode == dependantcode).Where(x => x.RiskRating == risk.RiskName).FirstOrDefault();

            return results;
        }

        public ServiceResult UpdateWorkflowUser(wfQueue model)
        {
            var old = _context.wfQueues.Where(x => x.Id == model.Id).FirstOrDefault();

            old.Status = model.Status;

            return _context.SaveChanges();
        }

        public ServiceResult InsertUserWorkflowLog(wfUserLog model)
        {
            _context.wfUserLogs.Add(model);
            return _context.SaveChanges();
        }


        public List<wfUserVM> GetUWFList(Guid UserID)
        {
            var results = (from wfu in _context.WorkflowUsers
                           join wfl in _context.WorkflowLogs on wfu.MembershipNumber + wfu.DependantCode equals wfl.MembershipNumber + wfl.DependantCode
                           where wfu.UserID == UserID

                           select new wfUserVM
                           {
                               Id = wfu.Id,
                               UserID = wfu.UserID,
                               DependantID = wfu.DependantID,
                               MedicalScheme = wfu.MedicalScheme,
                               Program = wfu.Program,
                               ManagementStatus = wfu.ManagementStatus,
                               MembershipNumber = wfu.MembershipNumber,
                               DependantCode = wfu.DependantCode,
                               UserFirstName = wfu.FirstName,
                               UserLastName = wfu.LastName,
                               Title = wfu.Title,
                               InProgress = wfl.InProgress,
                               Closed = wfl.Closed,

                           }).ToList();

            return results;
        }
        public List<wfQueue> GetWFQueueByUserAndQueue(Guid userID, Guid queueID)
        {
            return _context.wfQueues.Where(x => x.UserID == userID).Where(x => x.QueueID == queueID).ToList();

        }
        public wfUserQueue GetUserQueueInfo(Guid userID, Guid queueID)
        {
            return _context.wfUserQueues.Where(x => x.UserID == userID).Where(x => x.QueueID == queueID).FirstOrDefault();
        }
        public ServiceResult RemoveWFQueueFromUser(Guid userID, Guid queueID)
        {
            var list = _context.wfQueues.Where(x => x.UserID == userID).Where(x => x.QueueID == queueID).ToList();

            foreach (var item in list)
            {
                item.UserID = null;
            }

            return _context.SaveChanges();
        }
        public ServiceResult UpdateWFQueueForUser(Guid userID, Guid queueID, Guid newUserID)
        {
            var list = _context.wfQueues.Where(x => x.UserID == userID).Where(x => x.QueueID == queueID).ToList();

            foreach (var item in list)
            {
                item.UserID = newUserID;
            }

            return _context.SaveChanges();
        }

        #region HCare-1196 ----->
        public Pathology GetHIVPathologyForManualStaging(Guid DepID)
        {
            string sql = string.Format(@"SELECT p.DependantID, 
                                            (SELECT top 1 CD4Count FROM pathology WHERE dependentID = p.DependantID AND CD4Count <> 0 and Active = 1  AND CD4CountEffectiveDate IS NOT NULL ORDER BY pathologyID DESC) AS CD4Count, 
                                            (SELECT top 1 CD4CounteffectiveDate FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND CD4CounteffectiveDate IS NOT NULL AND CD4Count <> 0 ORDER BY pathologyID desc) CD4CounteffectiveDate,
                                            (SELECT top 1 pathologyID FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND CD4CounteffectiveDate IS NOT NULL AND CD4Count <> 0 ORDER BY pathologyID desc) CD4CountPathId, 
                                            (SELECT top 1 CD4Percentage FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND CD4PercentageeffectiveDate IS NOT NULL AND CD4Percentage <> 0 ORDER BY pathologyID desc) CD4Percentage, 
                                            (SELECT top 1 CD4PercentageeffectiveDate FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND CD4PercentageeffectiveDate IS NOT NULL AND CD4Percentage <> 0 ORDER BY pathologyID desc) CD4PercentageeffectiveDate, 
                                            (SELECT top 1 pathologyID FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND CD4PercentageeffectiveDate IS NOT NULL AND CD4Percentage <> 0 ORDER BY pathologyID desc) CD4PercentagePathId, 
                                            (SELECT top 1 viralLoad FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND viralLoadeffectiveDate IS NOT NULL AND viralLoad <> 0 ORDER BY pathologyID desc) viralLoad, 
                                            (SELECT top 1 viralLoadeffectiveDate FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND viralLoadeffectiveDate IS NOT NULL AND viralLoad <> 0 ORDER BY pathologyID desc) viralLoadeffectiveDate,
                                            (SELECT top 1 pathologyID FROM Pathology WHERE dependentID = p.DependantID and Active = 1 AND viralLoadeffectiveDate IS NOT NULL AND viralLoad <> 0 ORDER BY pathologyID desc) viralLoadPathId 
                                                                                        FROM Dependant p
											                                            INNER JOIN Member m ON p.memberID = m.memberID
											                                            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
																						INNER JOIN PatientProgramHistory ph ON p.DependantID = ph.dependantId
                                                                                        WHERE p.active = 1
											                                            AND m.active = 1
											                                            AND ma.Active = 1
                                                                                        AND ph.programCode = 'HIVPR'
                                                                                        AND p.DependantID = '{0}'", DepID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (Pathology)db.Query<Pathology>(sql, null, commandTimeout: 500).SingleOrDefault();
                db.Close();
                return ourCustomers;
            }
        }

        public ServiceResult InsertHIVPatientStagingHistoryManualStaging(PatientStagingHistory model)
        {
            var _riskRatingHistory = new PatientRiskRatingHistory();
            _riskRatingHistory.reason = model.comment;
            _riskRatingHistory.dependantID = model.dependantId;
            _riskRatingHistory.createdDate = model.createdDate;
            _riskRatingHistory.effectiveDate = model.effectiveDate;
            _riskRatingHistory.createdBy = model.createdBy;
            _riskRatingHistory.programType = "HIVPR";

            _context.PatientStagingHistory.Add(model);
            var res = SaveResult();
            _riskRatingHistory.patientStagingHistoryId = model.id;
            _context.PatientRiskRatingHistory.Add(_riskRatingHistory);
            var ress = SaveResult();





            return res;
        }
        public ServiceResult UpdateHIVPatientStagingHistoryForManualStaging(PatientStagingHistory patientStagingHistory)
        {
            var oldstatus = _context.PatientStagingHistory.Where(x => x.id == patientStagingHistory.id && x.createdBy != "Rating App").OrderByDescending(x => x.effectiveDate).FirstOrDefault();
            oldstatus.stageCode = patientStagingHistory.stageCode;
            oldstatus.comment = patientStagingHistory.comment;
            oldstatus.active = patientStagingHistory.active;
            oldstatus.effectiveDate = patientStagingHistory.effectiveDate;


            return SaveResult();
        }
        #endregion


        //public List<MHDiagnosisVM> GetMHDiagnosis(Guid dependantID)
        //{
        //    var results = (from pph in _context.PatientProgramHistory
        //                   join cme in _context.ComorbidConditionExclusions on pph.conditionCode equals cme.mappingCode
        //                   join p in _context.Program on pph.programCode equals p.code
        //                   where pph.dependantId == dependantID
        //                   where pph.icd10Code == cme.ICD10Code
        //                   where pph.conditionCode == cme.mappingCode
        //                   where pph.programCode == "MNTLH"

        //                   select new MHDiagnosisVM
        //                   {
        //                       Id = pph.id,
        //                       DependantID = pph.dependantId,
        //                       Program = p.ProgramName,
        //                       ICD10Code = pph.icd10Code,
        //                       MappingCode = cme.mappingCode,
        //                       Description = cme.mappingDescription,
        //                       ForumlaryCode = cme.formularyCode,
        //                       EffectiveDate = pph.effectiveDate,
        //                       EndDate = pph.endDate,
        //                       CreatedBy = pph.createdBy,
        //                       CreatedDate = pph.createdDate,
        //                       Comment = pph.Comment,
        //                       Active = pph.active,
        //                       ProgramHistory = true,

        //                   }).ToList();


        //    var subhistory = (from pps in _context.PatientProgramSubHistory
        //                      join cme in _context.ComorbidConditionExclusions on pps.conditionCode equals cme.mappingCode
        //                      join p in _context.Program on pps.programCode equals p.code
        //                      where pps.dependantId == dependantID
        //                      where pps.conditionCode == cme.mappingCode
        //                      where pps.icd10Code == cme.ICD10Code

        //                      select new MHDiagnosisVM
        //                      {
        //                          Id = pps.id,
        //                          DependantID = pps.dependantId,
        //                          Program = p.ProgramName,
        //                          ICD10Code = pps.icd10Code,
        //                          MappingCode = cme.mappingCode,
        //                          Description = cme.mappingDescription,
        //                          ForumlaryCode = cme.formularyCode,
        //                          EffectiveDate = pps.effectiveDate,
        //                          EndDate = pps.endDate,
        //                          CreatedBy = pps.createdBy,
        //                          CreatedDate = pps.createdDate,
        //                          Comment = pps.Comment,
        //                          Active = pps.active,
        //                          ProgramHistory = false,

        //                      }).ToList();

        //    foreach (var item in subhistory)
        //    {
        //        results.Add(item);
        //    }

        //    return results.OrderByDescending(x => x.CreatedDate).ToList();
        //}

        public List<MHDiagnosisVM> GetMHDiagnosis(Guid dependantID)
        {
            var results = (from mhd in _context.MentalHealthDiagnoses
                           join cme in _context.ComorbidConditionExclusions on mhd.ConditionCode equals cme.mappingCode
                           join p in _context.Program on mhd.ProgramCode equals p.code
                           where mhd.DependantID == dependantID
                           where mhd.ICD10Code == cme.ICD10Code
                           where mhd.ConditionCode == cme.mappingCode
                           where mhd.ProgramCode == "MNTLH"

                           select new MHDiagnosisVM
                           {
                               Id = mhd.Id,
                               DependantID = mhd.DependantID,
                               Program = p.ProgramName,
                               ICD10Code = mhd.ICD10Code,
                               MappingCode = cme.mappingCode,
                               Description = cme.mappingDescription,
                               ForumlaryCode = cme.formularyCode,
                               EffectiveDate = mhd.EffectiveDate,
                               EndDate = mhd.EndDate,
                               CreatedBy = mhd.CreatedBy,
                               CreatedDate = mhd.CreatedDate,
                               Comment = mhd.Comment,
                               Active = mhd.Active,

                           }).ToList();

            return results.OrderByDescending(x => x.CreatedDate).Where(x => x.Active == true).ToList();

        }

        public List<MentalHealthDiagnosis> GetAvailableMHDiagnosisForDependantID(Guid dependantID)
        {
            return _context.MentalHealthDiagnoses.Where(x => x.DependantID == dependantID).Where(x => x.ProgramCode == "MNTLH").Where(x => x.EndDate >= DateTime.Today || x.EndDate == null).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

        }

        public List<ProductionWorkItems> GetProductionWorkItems()
        {
            return _context.ProductionWorkItems.Where(x => x.Active == true).OrderBy(x => x.Description).ToList();
        }

        public NextOFKin GetNextOfKinByID(Guid id) //hcare-1361
        {
            return _context.NextOFKins.Where(x => x.Active == true).Where(x => x.Id == id).FirstOrDefault();
        }
        public List<NextOFKin> GetNextOfKinInformation(Guid dependantID) //hcare-1361
        {
            return _context.NextOFKins.Where(x => x.Active == true).Where(x => x.DependantID == dependantID).OrderByDescending(x => x.EffectiveDate).ToList();
        }
        public ServiceResult InsertNextOfKin(NextOFKin model)
        {
            _context.NextOFKins.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateNextOfKin(NextOFKin model)
        {
            var old = _context.NextOFKins.Where(x => x.Id == model.Id).Where(x => x.DependantID == model.DependantID).FirstOrDefault();
            old.FirstName = model.FirstName;
            old.LastName = model.LastName;
            old.Mobile = model.Mobile;
            old.Telephone = model.Telephone;
            old.Email = model.Email;
            old.Relation = model.Relation;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.EffectiveDate = model.EffectiveDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public NextOFKin GetNOKValidation01(string firstname, string lastname)
        {
            var result = _context.NextOFKins.Where(x => x.FirstName == firstname).Where(x => x.LastName == lastname).Where(x => x.Active == true).FirstOrDefault();
            if (result == null) { return null; } else { return result; }
        }
        public NextOFKin GetNOKValidation02(string firstname, string lastname, string telephone)
        {
            var result = _context.NextOFKins.Where(x => x.FirstName == firstname).Where(x => x.LastName == lastname).Where(x => x.Telephone == telephone).Where(x => x.Active == true).FirstOrDefault();
            if (result == null) { return null; } else { return result; }
        }
        public NextOFKin GetNOKValidation03(string firstname, string lastname, string telephone, string email)
        {
            var result = _context.NextOFKins.Where(x => x.FirstName == firstname).Where(x => x.LastName == lastname).Where(x => x.Telephone == telephone).Where(x => x.Email == email).Where(x => x.Active == true).FirstOrDefault();
            if (result == null) { return null; } else { return result; }
        }
        public NextOFKin GetNOKValidation04(string firstname, string lastname, string telephone, string email, string relation)
        {
            var result = _context.NextOFKins.Where(x => x.FirstName == firstname).Where(x => x.LastName == lastname).Where(x => x.Telephone == telephone).Where(x => x.Email == email).Where(x => x.Relation == relation).Where(x => x.Active == true).FirstOrDefault();
            if (result == null) { return null; } else { return result; }
        }
        public List<NextOFKin> GetNextOfKinValidation()
        {
            return _context.NextOFKins.Where(x => x.Active == true).ToList();
        }
        public NextOFKin GetNextOfKinByDependantID(Guid dependantID) //hcare-1363
        {
            return _context.NextOFKins.Where(x => x.Active == true).Where(x => x.DependantID == dependantID).OrderByDescending(x => x.EffectiveDate).FirstOrDefault();
        }

        public MedicalAidPatientVM GetMedicalAidByDependantID(Guid dependantID) //hcare-1363
        {
            var results = (from d in _context.Dependants
                           join m in _context.Members on d.memberID equals m.memberID
                           join ma in _context.MedicalAids on m.medicalAidID equals ma.MedicalAidID
                           where d.DependantID == dependantID
                           where ma.Active == true

                           select new MedicalAidPatientVM
                           {
                               DependantID = d.DependantID,
                               MedicalAidID = ma.MedicalAidID,
                               MedicalAidCode = ma.medicalAidCode,
                               MedicalAidName = ma.Name

                           }).OrderBy(x => x.MedicalAidCode).FirstOrDefault();

            return results;
        }
        public MemberRecord GetMemberRecordByDependantID(Guid dependantID, Guid programID, string username) //hcare-1374
        {
            return _context.MemberRecords.Where(x => x.DependantID == dependantID).Where(x => x.ProgramID == programID).Where(x => x.AccessedBy == username).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        }
        public ServiceResult InsertMemberRecord(MemberRecord model) //hcare-1374
        {
            _context.MemberRecords.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateMemberRecord(MemberRecord model) //hcare-1374
        {
            var old = _context.MemberRecords.Where(x => x.Id == model.Id).Where(x => x.DependantID == model.DependantID).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.ProgramPopUp = model.ProgramPopUp;
            old.ProfilePopUp = model.ProfilePopUp;
            old.Active = model.Active;

            return _context.SaveChanges();
        }


        public ServiceResult InsertEmail(Emails model) //hcare-1380
        {
            _context.Emails.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult InsertSMS(SmsMessages model) //hcare-1380
        {
            _context.SmsMessages.Add(model);
            return _context.SaveChanges();
        }

        public List<EmailLayout> GetEmailLayoutByID(Guid id)
        {
            return _context.EmailLayouts.Where(x => x.Active == true).Where(x => x.Id == id).ToList();
        }

        public PatientDoctorHistory GetPatientDoctorHistory(Guid dependantID) //hcare-1391
        {
            return _context.PatientDoctorHistory.Where(x => x.dependantId == dependantID).OrderByDescending(x => x.effectiveDate).FirstOrDefault();
        }

        //public void AttachEmailAttachmens(ComsViewModel model, string[] file)
        //{
        //    string root = Configuration.GetSection("EmailSettings")["root"];
        //    if (!Directory.Exists(root)) { Directory.CreateDirectory(root); }

        //    string directory = Directory.CreateDirectory(@"" + root + "_" + model.emailMessages.emailId).ToString();

        //    //foreach (var fileName in file)
        //    //{
        //    //    HttpPostedFileBase _file = fileName;
        //    //    var nameFile = Path.GetFileName(_file.FileName);
        //    //    string _path = Path.Combine(serverDir + directory, model.emailMessages.emailId + nameFile);

        //    //    _file.SaveAs(_path);
        //    //    AddAttachmentPath(model.emailMessages.emailId, _path);

        //    //}
        //}

        public Emails GetMemberEmailByID(int emailID) //hcare-1448
        {
            return _context.Emails.Where(x => x.emailId == emailID).FirstOrDefault();
        }

        public ServiceResult UpdateEmailStatus(Emails model) //hcare-1448
        {
            var old = _context.Emails.Where(x => x.emailId == model.emailId).FirstOrDefault();

            old.status = model.status;
            return _context.SaveChanges();
        }

        //================================================================================ Disposables =============================================================================//

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /*public List<EnrolmentViewModel> GetAdvancedSearchResults(AdvancedSearchView model)
        {
            //Get value based on not null fields
            //I.e if model.med is not null
            if (!string.IsNullOrEmpty(model.medicalAid[0].MedicalAidID.ToString()))
            {
                var result = _context.Members.Where(x => x.medicalAidID == model.medicalAid[0].MedicalAidID).ToList();
                result = result.Where(x => x.membershipNo == model.membershipNumber).ToList();
                result = result.Where(x => x. == model.membershipNumber).ToList();

            }
            /* var results = (from d in _context.Dependants
            join m in _context.Members
            on d.memberID equals m.memberID
            join mi in _context.MemberInformation
            on d.DependantID equals mi.dependantID
            join a in _context.Address
            on mi.addressID equals a.addressID
            join c in _context.Contacts
            on mi.contactID equals c.ContactID
            where m.membershipNo == membershipno
                           select new EnrolmentViewModel
                           {
                               member = m,
                               dependent = d,
                               address = a,
                               contact = c,
                               memberInformation = mi
                           }).FirstOrDefault();

        }*/

    }
}