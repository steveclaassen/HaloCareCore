using Dapper;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace HaloCareCore.DAL
{
    public class ClinicalRepository : IClinicalRepository
    {
        private OH17Context _context;
        private IAdminRepository _admin;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public ClinicalRepository(OH17Context context, IConfiguration _configuration)
        {
            this._context = context;
            _admin = new AdminRepository(context,_configuration);
        }
        //HCare-920
        public List<OpenAssignments> GetOpenAssignments(int? count)
        {
            if (count == null)
                count = 0;

            string sql = string.Format(@"DECLARE @x INT
                                        select @x = DATEDIFF(s, a.createdDate, GETDATE())
                                        from Assignments a
                                        SELECT ma.MedicalAidId, a.assignmentID as assignmentID, a.active as Active, ast.assignmentDescription as assignmentType, ma.Name AS medicalScheme, m.membershipNo as membershipNumber, d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName, 
                                        a.effectiveDate as assignmentEffective, CAST(d.DependantID AS nvarchar(max)) AS dependantID, pr.ProgramName AS program, ph.planName as [option], ms.statusName as patientStatus, DATEDIFF(day, a.effectiveDate, getDate()) AS assignmentAge, 
                                        ait.itemDescription as assignmentitemType, (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id) AS taskClosedCount, pr.programID,

                                        CONVERT(VARCHAR, DATEDIFF(dd, a.createdDate, GETDATE())) + ' Days '
                                        + CONVERT(VARCHAR, DATEDIFF(hh, a.createdDate, GETDATE()) % 24) + ' Hours '
                                        + CONVERT(VARCHAR, DATEDIFF(mi, a.createdDate, GETDATE()) % 60) + ' Minutes '
                                        + CONVERT(VARCHAR, DATEPART(ss, DATEADD(s, @x, CONVERT(DATETIME2, '0001-01-01')))) + ' Seconds' [elapsedTime],
                                        ait.assignmentItemType as itemType

							            FROM Assignments a
							            INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
							            INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
							            INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
							            INNER JOIN Dependant d ON a.dependentID = d.DependantID
							            INNER JOIN Member m ON d.memberID = m.memberID
							            INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
							            LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
							            AND p.createdDate = (SELECT MAX(createdDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId)
							            LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
							            AND ph.createdDate = (SELECT MAX(createdDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
							            LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
							            LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
							            AND ps.createdDate = (SELECT MAX(createdDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
							            LEFT OUTER JOIN ManagementStatus ms ON ps.managementStatusCode = ms.statusCode
							            WHERE a.Active = 1
							            AND (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) = 0
							            AND ma.Active = 1
							            ORDER BY a.createdDate asc
							            OFFSET {0} ROWS
							            FETCH NEXT 50 ROWS ONLY", count);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<OpenAssignments>)db.Query<OpenAssignments>(sql, null, commandTimeout: 500);
                db.Close();
                var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();
                
                results = (from r in results
                           where allowedItems.Contains(r.itemType)
                           select r).ToList();

                var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                var filteredRes = (from r in results
                                   where medaidlist.Contains(r.MedicalAidID)
                                   select r).ToList();

                return filteredRes;
            }
        }
        //HCare-920
        public List<OpenAssignments> GetOpenAssignmentsFull(int? count)
        {
           
            if (count == null)
                count = 0;
            string sql = "";

            if (count == 0)
            {
                sql = string.Format(@"DECLARE @x INT
										SELECT @x = DATEDIFF(s, a.createdDate, GETDATE())
										FROM Assignments a
										SELECT a.assignmentID AS assignmentID, a.active AS Active, ast.assignmentDescription AS assignmentType, ma.Name AS medicalScheme, m.membershipNo AS membershipNumber, d.idNumber AS idNumber, d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName, 
										a.effectiveDate AS assignmentEffective, CAST(d.DependantID AS nvarchar(max)) AS dependantID, pr.ProgramName AS program, ph.planName AS [option], ms.statusName AS patientStatus, DATEDIFF(day, a.effectiveDate, getDate()) AS assignmentAge, a.status AS assignmentStatus, 
										ait.itemDescription as assignmentitemType, (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id) AS taskClosedCount, ma.MedicalAidID,
										
										CONVERT(VARCHAR, DATEDIFF(dd, a.createdDate, GETDATE())) + ' Days '
										+ CONVERT(VARCHAR, DATEDIFF(hh, a.createdDate, GETDATE()) % 24) + ' Hours '
										+ CONVERT(VARCHAR, DATEDIFF(mi, a.createdDate, GETDATE()) % 60) + ' Minutes '
										+ CONVERT(VARCHAR, DATEPART(ss, DATEADD(s, @x, CONVERT(DATETIME2, '0001-01-01')))) + ' Seconds' [elapsedTime],
										ait.assignmentItemType as itemType, pr.programID, a.programId AS assignmentProgramID
										
										FROM Assignments a
										    INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
										    INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
										    INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
										    INNER JOIN Dependant d ON a.dependentID = d.DependantID
										    INNER JOIN Member m ON d.memberID = m.memberID
										    INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
										    LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
										        AND p.createdDate = (SELECT MAX(createdDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId AND ait.program = programCode) --//hcare-1112									
										    LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
										        AND ph.createdDate = (SELECT MAX(createdDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
										    LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
										    LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
										        AND ps.createdDate = (SELECT MAX(createdDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
										    LEFT OUTER JOIN ManagementStatus ms ON ps.managementStatusCode = ms.statusCode
												AND ait.program = ms.programCode --//hcare-1112
										WHERE a.Active = 1
										    AND (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) = 0
										    AND ma.Active = 1
										ORDER BY a.createdDate ASC", count);
            }
            else
            {
                sql = string.Format(@"DECLARE @x INT
										SELECT @x = DATEDIFF(s, a.createdDate, GETDATE())
										FROM Assignments a
										SELECT top {0} a.assignmentID AS assignmentID, a.active AS Active, ast.assignmentDescription AS assignmentType, ma.Name AS medicalScheme, m.membershipNo AS membershipNumber, d.idNumber AS idNumber, d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName, 
										a.effectiveDate AS assignmentEffective, CAST(d.DependantID AS nvarchar(max)) AS dependantID, pr.ProgramName AS program, ph.planName AS [option], ms.statusName AS patientStatus, DATEDIFF(day, a.effectiveDate, getDate()) AS assignmentAge, a.status AS assignmentStatus, 
										ait.itemDescription as assignmentitemType, (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id) AS taskClosedCount, ma.MedicalAidID,
										
										CONVERT(VARCHAR, DATEDIFF(dd, a.createdDate, GETDATE())) + ' Days '
										+ CONVERT(VARCHAR, DATEDIFF(hh, a.createdDate, GETDATE()) % 24) + ' Hours '
										+ CONVERT(VARCHAR, DATEDIFF(mi, a.createdDate, GETDATE()) % 60) + ' Minutes '
										+ CONVERT(VARCHAR, DATEPART(ss, DATEADD(s, @x, CONVERT(DATETIME2, '0001-01-01')))) + ' Seconds' [elapsedTime],
										ait.assignmentItemType as itemType, pr.programID, a.programId AS assignmentProgramID
										
										FROM Assignments a
										    INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
										    INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
										    INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
										    INNER JOIN Dependant d ON a.dependentID = d.DependantID
										    INNER JOIN Member m ON d.memberID = m.memberID
										    INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
										    LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
										        AND p.createdDate = (SELECT MAX(createdDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId AND ait.program = programCode)	--//hcare-1112									
										    LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
										        AND ph.createdDate = (SELECT MAX(createdDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
										    LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
										    LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
										        AND ps.createdDate = (SELECT MAX(createdDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
										    LEFT OUTER JOIN ManagementStatus ms ON ps.managementStatusCode = ms.statusCode
												AND ait.program = ms.programCode --//hcare-1112
										WHERE a.Active = 1
										    AND (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) = 0
										    AND ma.Active = 1
										ORDER BY a.createdDate ASC", count);
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<OpenAssignments>)db.Query<OpenAssignments>(sql, null, commandTimeout: 500).Distinct().ToList();
                db.Close();

                var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();

                results = (from r in results
                           where allowedItems.Contains(r.itemType)
                           select r).ToList();

                var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                var filteredResponse = (from r in results
                                   where medaidlist.Contains(r.MedicalAidID)
                                   select r).ToList();

                var freshResponse = new List<OpenAssignments>();

                foreach (var med in medaidlist)
                {
                    var progs = rights.accessList.Where(x => x.medicalAidId == med).Select(x => x.programId).ToList();
                    var filteredRes = (from r in filteredResponse
                                       where r.MedicalAidID == med
                                       where progs.Contains(r.programID) && (r.assignmentProgramID == null || progs.Contains(new Guid(r.assignmentProgramID.ToString())))//hcare-1112
                                       select r).ToList();

                    freshResponse.AddRange(filteredRes);
                }

                results = freshResponse.Distinct().ToList();


                return results;
            }
        }

        public List<CompactOpenAssignments> GetCompactOpenAssignments()
        {
           
            string sql = @"SELECT Distinct ma.MedicalAidID, ma.Name AS medicalScheme, m.membershipNo as membershipNumber, d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName
								  , CAST(d.DependantID AS nvarchar(max)) AS dependantID, (SELECT COUNT(*) FROM Assignments WHERE dependentID = d.DependantID AND Active = 1) AS assignmentCount,
									pr.ProgramName AS program, ph.planName as [option], ms.statusName as patientStatus, pr.programID
							FROM Member m
							INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
							INNER JOIN Dependant d ON m.memberID = d.memberID
							LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
							AND p.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
							AND ph.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
							LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
							AND ps.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN ManagementStatus ms ON ps.managementStatusCode = ms.statusCode
							WHERE (SELECT COUNT(*) FROM Assignments WHERE dependentID = d.DependantID AND active = 1) > 0
							AND ma.Active = 1
							order by dependantId ASC";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<CompactOpenAssignments>)db.Query<CompactOpenAssignments>(sql, null, commandTimeout: 500);
                db.Close();
                var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                var filteredRes = (from r in results
                                   where medaidlist.Contains(r.MedicalAidID)
                                   select r).ToList();

                return filteredRes;
            }
        }

        public List<OpenAssignments> GetOpenAssignmentPatients()
        {
           
            var results = (from a in _context.Assignments
                           join at in _context.AssignmentTypes
                           on a.assignmentType equals at.assignmentType
                           join d in _context.Dependants
                           on a.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           where a.Active == true
                           select new OpenAssignments
                           {
                               assignmentID = a.assignmentID,
                               Active = a.Active,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               patientName = d.firstName + " " + d.lastName,
                               medicalScheme = ma.Name,
                               assignmentEffective = a.effectiveDate,
                               dependantID = d.DependantID.ToString(),
                               assignmentAge = (a.effectiveDate- DateTime.Now).Days + " Days",
                               program = _context.Program.Where(p => p.code == (_context.PatientProgramHistory.Where(x => x.active == true).Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault())).Select(x => x.ProgramName).FirstOrDefault(),
                               option = _context.PatientPlanHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.planName).FirstOrDefault(),
                               itemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.assignmentItemType).FirstOrDefault(),
                               patientStatus = _context.ManagementStatus.Where(p => p.statusCode == (_context.PatientManagementStatusHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.managementStatusCode).FirstOrDefault())).Select(p => p.statusName).FirstOrDefault(),
                               assignmentitemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.itemDescription).FirstOrDefault(),
                               taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count()
                           }).Where(x => x.taskClosedCount == 0).Where(x => x.patientStatus != "Deactivated").OrderBy(x => x.assignmentEffective).Take(100).ToList();

            foreach (var result in results)
            {
                if (String.IsNullOrEmpty(result.assignmentitemType))
                {
                    var assignment = GetAssignmentById(result.assignmentID);
                    assignment.Active = false;
                    assignment.comment = "No item avaliable";
                    assignment.modifiedBy = "System";
                    assignment.modifiedDate = DateTime.Now;
                    var response = UpdateAssignment(assignment);
                }
            }

            results = (from a in _context.Assignments
                       join at in _context.AssignmentTypes
                       on a.assignmentType equals at.assignmentType
                       join d in _context.Dependants
                       on a.dependentID equals d.DependantID
                       join m in _context.Members
                       on d.memberID equals m.memberID
                       join ma in _context.MedicalAids
                       on m.medicalAidID equals ma.MedicalAidID
                       where a.Active == true
                       select new OpenAssignments
                       {
                           assignmentID = a.assignmentID,
                           Active = a.Active,
                           membershipNumber = m.membershipNo,
                           dependantCode = d.dependentCode,
                           assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                           patientName = d.firstName + " " + d.lastName,
                           medicalScheme = ma.Name,
                           assignmentEffective = a.effectiveDate,
                           dependantID = d.DependantID.ToString(),
                           assignmentAge = (a.effectiveDate- DateTime.Now).Days + " Days",
                           program = _context.Program.Where(p => p.code == (_context.PatientProgramHistory.Where(x => x.active == true).Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault())).Select(x => x.ProgramName).FirstOrDefault(),
                           option = _context.PatientPlanHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.planName).FirstOrDefault(),
                           itemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.assignmentItemType).FirstOrDefault(),
                           patientStatus = _context.ManagementStatus.Where(p => p.statusCode == (_context.PatientManagementStatusHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.managementStatusCode).FirstOrDefault())).Select(p => p.statusName).FirstOrDefault(),
                           assignmentitemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.itemDescription).FirstOrDefault(),
                           taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count()
                       }).Where(x => x.taskClosedCount == 0).Where(x => x.patientStatus != "Deactivated").OrderBy(x => x.assignmentEffective).Take(100).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();
            
            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public List<OpenAssignments> GetOpenAssignments(Guid DepID)
        {
            var results = (from a in _context.Assignments
                           join at in _context.AssignmentTypes
                           on a.assignmentType equals at.assignmentType
                           join d in _context.Dependants
                           on a.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           where a.Active == true
                           select new OpenAssignments
                           {
                               MedicalAidID = ma.MedicalAidID,
                               assignmentID = a.assignmentID,
                               Active = a.Active,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                               patientName = d.firstName + " " + d.lastName,
                               medicalScheme = ma.Name,
                               assignmentEffective = a.effectiveDate,
                               dependantID = d.DependantID.ToString(),
                               assignmentAge = (a.effectiveDate - DateTime.Now).Days + " Days",
                               program = _context.Program.Where(p => p.code == (_context.PatientProgramHistory.Where(x => x.active == true).Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault())).Select(x => x.ProgramName).FirstOrDefault(),
                               option = _context.PatientPlanHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.planName).FirstOrDefault(),
                               patientStatus = _context.ManagementStatus.Where(p => p.statusCode == (_context.PatientManagementStatusHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.managementStatusCode).FirstOrDefault())).Select(p => p.statusName).FirstOrDefault(),
                               assignmentitemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.itemDescription).FirstOrDefault(),
                               taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count()
                           }).Where(x => x.taskClosedCount == 0).OrderBy(x => x.assignmentEffective).Take(100).ToList();

            foreach (var result in results)
            {
                if (String.IsNullOrEmpty(result.assignmentitemType))
                {
                    var assignment = GetAssignmentById(result.assignmentID);
                    assignment.Active = false;
                    assignment.comment = "No item avaliable";
                    assignment.modifiedBy = "System";
                    assignment.modifiedDate = DateTime.Now;
                    var response = UpdateAssignment(assignment);
                }
            }

            results = (from a in _context.Assignments
                       join at in _context.AssignmentTypes
                       on a.assignmentType equals at.assignmentType
                       join d in _context.Dependants
                       on a.dependentID equals d.DependantID
                       join m in _context.Members
                       on d.memberID equals m.memberID
                       join ma in _context.MedicalAids
                       on m.medicalAidID equals ma.MedicalAidID
                       where a.Active == true
                       select new OpenAssignments
                       {
                           MedicalAidID = ma.MedicalAidID,
                           assignmentID = a.assignmentID,
                           Active = a.Active,
                           membershipNumber = m.membershipNo,
                           dependantCode = d.dependentCode,
                           assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == a.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault(),
                           patientName = d.firstName + " " + d.lastName,
                           medicalScheme = ma.Name,
                           assignmentEffective = a.effectiveDate,
                           dependantID = d.DependantID.ToString(),
                           assignmentAge = (a.effectiveDate- DateTime.Now).Days + " Days",
                           program = _context.Program.Where(p => p.code == (_context.PatientProgramHistory.Where(x => x.active == true).Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault())).Select(x => x.ProgramName).FirstOrDefault(),
                           option = _context.PatientPlanHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.planName).FirstOrDefault(),
                           patientStatus = _context.ManagementStatus.Where(p => p.statusCode == (_context.PatientManagementStatusHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.managementStatusCode).FirstOrDefault())).Select(p => p.statusName).FirstOrDefault(),
                           assignmentitemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).OrderByDescending(p => p.createdDate).Select(p => p.itemType).FirstOrDefault())).Select(x => x.itemDescription).FirstOrDefault(),
                           taskClosedCount = _context.AssignmentItemTasks.Where(x => x.assignmentItemID == (_context.AssignmentItems.Where(p => p.assignmentId == a.assignmentID).Where(p => p.active == true).Select(p => p.id).FirstOrDefault())).Where(x => x.closed == true).ToList().Count()
                       }).Where(x => x.taskClosedCount == 0).OrderBy(x => x.assignmentEffective).Take(100).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();
            
            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
            results = (from r in results
                       where medaidlist.Contains(r.MedicalAidID)
                       select r).ToList();

            return results;
        }

        public List<CompactOpenAssignments> GetCompactInProgressAssignments()
        {
            var results = (from m in _context.Members
                           join d in _context.Dependants
                           on m.memberID equals d.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           select new CompactOpenAssignments
                           {
                               medicalScheme = ma.Name,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               patientName = d.firstName + " " + d.lastName,
                               dependantID = d.DependantID.ToString(),
                               assignmentCount = (from a in _context.Assignments
                                                  join at in _context.AssignmentTypes
                                                  on a.assignmentType equals at.assignmentType
                                                  join ai in _context.AssignmentItems
                                                  on a.assignmentID equals ai.assignmentId
                                                  join ail in _context.AssignmentItemLogs
                                                  on ai.id equals ail.assignmentItemID
                                                  where a.Active == true
                                                  where a.dependentID == d.DependantID
                                                  select a).Count(),
                               program = _context.Program.Where(p => p.code == (_context.PatientProgramHistory.Where(x => x.active == true).Where(x => x.dependantId == d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault())).Select(x => x.ProgramName).FirstOrDefault(),
                               option = _context.PatientPlanHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.planName).FirstOrDefault(),
                               patientStatus = _context.ManagementStatus.Where(p => p.statusCode == (_context.PatientManagementStatusHistory.OrderByDescending(x => x.effectiveDate).Where(x => x.dependantId == d.DependantID).Select(x => x.managementStatusCode).FirstOrDefault())).Select(p => p.statusName).FirstOrDefault(),
                           }).Where(x => x.assignmentCount != 0).OrderBy(x => x.membershipNumber).Take(100).ToList();

            return results;
        }

        public List<OpenAssignments> GetInProgressAssignments()
        {
            string sql = @"SELECT a.assignmentID as assignmentID, a.active as Active, ast.assignmentDescription as assignmentType, ma.Name AS medicalScheme, m.membershipNo as membershipNumber,
									d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName, a.effectiveDate as assignmentEffective, 
									CAST(d.DependantID AS nvarchar(max)) AS dependantID,
									pr.ProgramName AS program, ph.planName as [option], ps.managementStatusCode as patientStatus,
									DATEDIFF(day, a.effectiveDate, getDate()) AS assignmentAge,
									(SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) AS taskClosedCount,
                                        ait.assignmentItemType as itemType
							FROM Assignments a
							INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
							INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
							INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
							INNER JOIN Dependant d ON a.dependentID = d.DependantID
							INNER JOIN Member m ON d.memberID = m.memberID
							INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
							LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
							AND p.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
							AND ph.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
							LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
							AND ps.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
							WHERE a.Active = 1
							AND ma.medicalAidCode LIKE 'MH%'
							AND (SELECT COUNT(*) FROM AssignmentItemTasks WHERE assignmentItemID = ai.id AND closed = 1) > 0";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<OpenAssignments>)db.Query<OpenAssignments>(sql, null, commandTimeout: 500);
                db.Close();

                var rights = _admin.GetUserRights(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
                var medaidlist = rights.accessList.Select(x => x.medicalAidId).ToList();
                results = (from r in results
                           where medaidlist.Contains(r.MedicalAidID)
                           select r).ToList();

                return results;
            }
        }

        public List<CompactOpenAssignments> GetCompactClosedAssignments()
        {
            string sql = @"SELECT Distinct ma.Name AS medicalScheme, m.membershipNo as membershipNumber, d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName
								  , CAST(d.DependantID AS nvarchar(max)) AS dependantID, (SELECT COUNT(*) FROM Assignments WHERE dependentID = d.DependantID AND Active = 0) AS assignmentCount,
									pr.ProgramName AS program, ph.planName as [option], ps.managementStatusCode as patientStatus
							FROM Member m
							INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
							INNER JOIN Dependant d ON m.memberID = d.memberID
							LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
							AND p.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
							AND ph.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
							LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
							AND ps.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
							WHERE (SELECT COUNT(*) FROM Assignments WHERE dependentID = d.DependantID AND active = 0) > 0
							order by dependantId ASC";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var ourCustomers = (List<CompactOpenAssignments>)db.Query<CompactOpenAssignments>(sql, null, commandTimeout: 500);
                db.Close();
                return ourCustomers;
            }

        }

        public List<OpenAssignments> GetClosedAssignments()
        {
            string sql = @"SELECT a.assignmentID as assignmentID, a.active as Active, ast.assignmentDescription as assignmentType, ma.Name AS medicalScheme, m.membershipNo as membershipNumber,
									d.dependentCode AS dependantCode, d.firstName + ' ' + d.lastName AS patientName, a.effectiveDate as assignmentEffective, 
									CAST(d.DependantID AS nvarchar(max)) AS dependantID,
									pr.ProgramName AS program, ph.planName as [option], ps.managementStatusCode as patientStatus,
									DATEDIFF(day, a.effectiveDate, getDate()) AS assignmentAge,
                                        ait.assignmentItemType as itemType
							FROM Assignments a
							INNER JOIN AssignmentTypes ast ON a.assignmentType = ast.assignmentType
							INNER JOIN AssignmentItems ai on a.assignmentID = ai.assignmentId
							INNER JOIN AssignmentItemTypes ait ON ai.itemType = ait.assignmentItemType
							INNER JOIN Dependant d ON a.dependentID = d.DependantID
							INNER JOIN Member m ON d.memberID = m.memberID
							INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
							LEFT OUTER JOIN PatientProgramHistory p ON d.DependantID = p.dependantId
							AND p.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientProgramHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN PatientPlanHistory ph ON d.DependantID = ph.dependantId
							AND ph.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientPlanHistory WHERE d.DependantID = dependantId)
							LEFT OUTER JOIN Programs pr ON p.programCode = pr.code
							LEFT OUTER JOIN PatientManagementStatusHistory ps ON d.DependantID = ps.dependantId
							AND ps.effectiveDate = (SELECT MAX(effectiveDate) FROM PatientManagementStatusHistory WHERE d.DependantID = dependantId)
							WHERE a.Active = 0
							AND a.modifiedDate BETWEEN DATEADD(dd, -7, GETDATE()) AND getdate()
							AND ma.medicalAidCode LIKE 'MH%'";

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<OpenAssignments>)db.Query<OpenAssignments>(sql, null, commandTimeout: 500);
                db.Close();

                return results;
            }
        }

        public List<PostponedAssignments> GetPostponedAssignments()
        {
            var results = (from a in _context.Assignments
                           join at in _context.AssignmentTypes
                           on a.assignmentType equals at.assignmentType
                           join d in _context.Dependants
                           on a.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           where a.Active == true
                           select new PostponedAssignments
                           {
                               assignmentID = a.assignmentID,
                               Active = a.Active,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               patientName = d.firstName + " " + d.lastName,
                               medicalScheme = ma.medicalAidCode,
                               assignmentEffective = a.effectiveDate,
                           }).Take(100).ToList();

            return results;
        }

        public List<OpenAssignments> GetDocumentAssignments()
        {
            var results = (from a in _context.Assignments
                           join at in _context.AssignmentTypes
                           on a.assignmentType equals at.assignmentType
                           join d in _context.Dependants
                           on a.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join ma in _context.MedicalAids
                           on m.medicalAidID equals ma.MedicalAidID
                           where a.Active == true
                           where a.assignmentType.Contains("DOC")
                           select new OpenAssignments
                           {
                               assignmentID = a.assignmentID,
                               Active = a.Active,
                               membershipNumber = m.membershipNo,
                               dependantCode = d.dependentCode,
                               assignmentType = a.assignmentType,
                               patientName = d.firstName + " " + d.lastName,
                               medicalScheme = ma.medicalAidCode,
                               assignmentEffective = a.effectiveDate,
                               dependantID = d.DependantID.ToString(),
                           }).OrderBy(x => x.assignmentEffective).Take(100).ToList();

            var role = _admin.GetUserRoleById(_context.Users.Where(x => x.username == _session.GetString("userName")).Select(x => x.userID).FirstOrDefault());
            var allowedItems = _context.UserRoleWorkflowRights.Where(x => x.roleId == role.roleId).Where(x => x.active == true).Select(x => x.assignmentTypeId).ToList();
            
            results = (from r in results
                       where allowedItems.Contains(r.itemType)
                       select r).ToList();

            return results;
        }

        public int InsertAuthorizationRequest(Authorisations model)
        {
            _context.Authorisations.Add(model);
            _context.SaveChanges();

            return model.authID;
        }

        public ServiceResult UpdateAuthorizationRequest(Authorisations model)
        {
            var auth = _context.Authorisations.Where(x => x.authID == model.authID).FirstOrDefault();

            auth.response = model.response;
            auth.request = model.request;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateTask(AssignmentItemTasks task)
        {
            var old = GetTask(task.id);
            old.closed = task.closed;
            old.closedBy = task.closedBy;
            old.closedDate = task.closedDate;
            old.modifiedBy = task.modifiedBy;
            old.modifiedDate = task.modifiedDate;
            old.status = task.status;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateAssignmentItem(AssignmentItems model)
        {
            var item = GetAssignmentItemById(model.id);

            item.modifiedBy = model.modifiedBy;
            item.modifiedDate = model.modifiedDate;
            item.documentName = model.documentName;
            item.capturedId = model.capturedId;

            return _context.SaveChanges();
        }

        public string getProgram(Guid dependantID)
        {
            return _context.DependentPrograms.Where(x => x.dependentId == dependantID).Select(x => x.programCode).FirstOrDefault();
        }

        public string getBHFNumber(int scriptNo)
        {
            Guid? doctorID = _context.Scripts.Where(x => x.scriptID == scriptNo).Select(x => x.doctorID).FirstOrDefault();
            return _context.Doctors.Where(x => x.doctorID == doctorID).Select(x => x.practiceNo).FirstOrDefault();
        }

        public Assignments GetAssignmentById(int id)
        {
            var res = _context.Assignments.Where(x => x.assignmentID == id).FirstOrDefault();
            res.assignmentType = _context.AssignmentTypes.Where(x => x.assignmentType == res.assignmentType).Select(x => x.assignmentDescription).FirstOrDefault();
            return res;
        }


        public List<TaskViewModel> GetTasks(int id, string itemType)
        {
            var res = (from t in _context.AssignmentItemTasks
                       join r in _context.TaskRequirementItemLinking
                       on t.requirementId equals r.requirementId
                       where t.assignmentItemID == id
                       where r.itemId == itemType
                       select new TaskViewModel()
                       {
                           assignmentitemID = t.assignmentItemID,
                           id = t.id,
                           closedby = t.closedBy,
                           requirement = _context.TaskTypeRequirements.Where(x => x.id == r.requirementId).Select(x => x.requirementText).FirstOrDefault(),
                           templateID = r.templateID.ToString(),
                           closed = t.closed,
                           comment = t.comment,
                           taskType = _context.AssignmentItemTaskTypes.Where(x => x.taskId == t.taskTypeId).Select(x => x.taskDescription).FirstOrDefault(),
                           taskSequence = r.taskSequence,
                           RequirementID = r.requirementId,
                           itemID = r.itemId //HCare-1192



                       }).OrderBy(x => x.taskSequence).ToList();

            return res;
        }

        public TaskViewModel GetTaskView(int id)
        {
            var res = (from t in _context.AssignmentItemTasks
                       join r in _context.TaskTypeRequirements
                       on t.requirementId equals r.id
                       where t.id == id
                       select new TaskViewModel()
                       {
                           assignmentitemID = t.assignmentItemID,
                           id = t.id,
                           closedby = t.closedBy,
                           requirement = r.requirementText,
                           templateID = r.templateID.ToString(),
                           closed = t.closed,
                           taskType = _context.AssignmentItemTaskTypes.Where(x => x.taskId == t.taskTypeId).Select(x => x.taskDescription).FirstOrDefault()
                       }).FirstOrDefault();

            return res;
        }

        public AssignmentItemTasks GetTask(int id)
        {
            return _context.AssignmentItemTasks.Where(x => x.id == id).Where(x => x.active == true).FirstOrDefault();
        }

        public AssignmentItemTasks GetTaskByComment(string id)
        {
            return _context.AssignmentItemTasks.Where(x => x.comment == id).Where(x => x.active == true).FirstOrDefault();
        }

        public string GetEmailTemplate(int id)
        {
            return _context.EmailTemplates.Where(x => x.templateID == id).Select(x => x.template).FirstOrDefault();
        }

        public string GetSmsTemplate(int id)
        {
            return _context.SmsMessageTemplates.Where(x => x.templateID == id).Select(x => x.template).FirstOrDefault();
        }

        public List<SmsMessageTemplates> GetSmsTemplate()
        {
            return _context.SmsMessageTemplates.Where(x => x.Active == true).ToList();
        }

        public List<AssignmentItems> GetAssignmentItemsById(int id)
        {
            var res = _context.AssignmentItems.Where(x => x.assignmentId == id).Where(x => x.active == true).ToList();
            if (res != null)
            {
                foreach (var item in res)
                {
                    item.itemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType.Contains(item.itemType)).Select(x => x.itemDescription).FirstOrDefault();
                }
            }
            return res;
        }

        public AssignmentItems GetAssignmentItemById(int id)
        {
            var res = _context.AssignmentItems.Where(x => x.id == id).FirstOrDefault();
            return res;
        }

        public List<AssignmentItemView> GetAsItemViewById(int id)
        {
            var results = (from a in _context.AssignmentItems
                           join at in _context.AssignmentItemTypes
                           on a.itemType equals at.assignmentItemType
                           join al in _context.AssignmentItemLogs
                           on a.id equals al.assignmentItemID into alo
                           from al in alo.DefaultIfEmpty()
                           where a.active == true
                           where a.assignmentId == id
                           select new AssignmentItemView
                           {
                               assignmentID = a.assignmentId.ToString(),
                               assignmentItemId = a.id.ToString(),
                               requirement = at.requirements,
                               itemTypeName = at.itemDescription,
                               assignmentItemType = at.assignmentItemType,
                               attach = at.attach,
                               attached = al.attached,
                               capture = at.capture,
                               captured = al.captured,
                               followup = at.followUp,
                               folloedwup = al.followedUp,
                               itemCapturedId = a.capturedId,
                               itemDocName = a.documentName,
                               logid = al.id,
                           }).ToList();
            return results;
        }

        public ServiceResult UpdateAssignment(Assignments model)
        {
            var old = _context.Assignments.Where(x => x.assignmentID == model.assignmentID).FirstOrDefault();
            if (!String.IsNullOrEmpty(old.comment))
                old.comment = model.comment;

            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;
            old.postpone = model.postpone;
            old.postponementReason = model.postponementReason;
            old.postponeToDate = model.postponeToDate;

            return _context.SaveChanges();
        }

        public List<QuestionnaireTemplate> GetQuestionnaireList(string TemplateName)
        {
            var questionnaire = _context.QuestionnaireTemplates.Where(x => x.Active == true).Where(x => x.TemplateName == TemplateName).OrderBy(o => o.QuestionnaireTemplateID).ToList();

            return questionnaire;
        }

        public ServiceResult InsertPatientQuestionnaireResponse(PatientQuestionnaireResponse model)
        {
            _context.PatientQuestionnaireResponses.Add(model);

            return _context.SaveChanges();
        }


        //=========================================================================================================================================================================//
        //                                                                          Patient Disclaimer                                                                             // 
        //=========================================================================================================================================================================//

        public PatientDisclaimer GetPatientDisclaimerById(int id)
        {
            return _context.PatientDisclaimers.Where(x => x.PatientDisclaimerId == id).FirstOrDefault();
        }

        public List<PatientDisclaimer> GetPatientDisclaimerResults(Guid depId)
        {
            return _context.PatientDisclaimers.Where(x => x.DependentID == depId).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public ServiceResult InsertPatientDisclaimer(PatientDisclaimer model)
        {
            _context.PatientDisclaimers.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdatePatientDisclaimerResult(PatientDisclaimer model)
        {
            var old = _context.PatientDisclaimers.Where(x => x.PatientDisclaimerId == model.PatientDisclaimerId).FirstOrDefault();

            old.SignedAcknowledgement = model.SignedAcknowledgement;
            old.TelephonicAcknowledgement = model.TelephonicAcknowledgement;
            old.Comment = model.Comment;
            old.Active = model.Active;

            return _context.SaveChanges();
        }


        public void Save()
        {
            _context.SaveChanges();
        }

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
    }
}