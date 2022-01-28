using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Repos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaloCareCore.Management
{
    public class ClinicalRulesEngine
    {
        private IMemberRepository _member;
        private IAdminRepository _admin;
        private OH17Context context;
        private readonly IConfiguration Configuration;
        public ClinicalRulesEngine()
        {
            _member = new MemberRepository(Configuration, context);
            _admin = new AdminRepository(context, Configuration);
        }

        public void CheckPathology(Guid dependentID, int pathID)
        {
            var member = _member.GetDependentDetails(dependentID, null);
            var results = _member.GetPathology(dependentID);
            var memberProgram = _member.GetProgramHistories(dependentID);
            var cd4Count = results[0].CD4Count;
            var prevCD4Count = new decimal();
            decimal prevCD4Perc = new decimal();
            if (results.Count > 1)
            {
                prevCD4Count = Convert.ToDecimal(results[1].CD4Count);
                prevCD4Perc = Convert.ToDecimal(results[1].CD4Percentage);
            }

            var cd4Perc = results[0].CD4Percentage;
            var viralLoad = results[0].viralLoad;
            var haemoglobin = results[0].haemoglobin;
            var bilirubin = results[0].bilirubin;
            var total_cholestrol = results[0].totalCholestrol;
            var hdl = results[0].hdl;
            var ldl = results[0].ldl;
            var triglycerides = results[0].triglycerides;
            var glucose = results[0].glucose;
            var hba1c = results[0].hba1c;
            var alt = results[0].alt;
            var ast = results[0].ast;
            var urea = results[0].urea;
            var creatinine = results[0].creatinine;
            var eGfr = results[0].eGfr;
            var mau_creat_ratio = results[0].mauCreatRatio;
            var sistalic = results[0].systolicBP;
            var diastalic = results[0].diastolicBP;
            var fev1 = results[0].FEV1;
            var eosinophylia = results[0].Eosinophylia;
            var hivEliza = results[0].hivEliza;
            var norgtt = results[0].normGtt;
            var abngtt = results[0].abnGtt;
            var ucreatinine = results[0].ucreatinine;
            var salbumin = results[0].salbumin;
            var ualbuminuria = results[0].ualbuminuria;
            var pathologyType = results[0].pathologyType;
            var pro = _member.GetPrograms().Where(x => x.code.Contains(results[0].pathologyType)).Select(x => x.programID).FirstOrDefault();
            int count = 0;

            if (memberProgram != null)
            {
                foreach (var prog in memberProgram.Where(x => x.programCode.Contains(pathologyType)))
                {
                    var AllowedRules = _admin.GetClinicalRulesByScheme(member.medicalAid.MedicalAidID).Where(x => x.ruleType == prog.programCode).ToList(); //hcare-809
                    var Rules = AllowedRules.Where(x => x.ruleType == prog.programCode).ToList();

                    if (member.dependent.sex.ToUpper().Contains("F"))
                    {
                        var female = true;
                        Rules = AllowedRules.Where(x => x.active == true).Where(x => x.female == female).ToList();
                    }
                    else
                    {
                        var male = true;
                        Rules = AllowedRules.Where(x => x.active == true).Where(x => x.male == male).ToList();
                    }

                    foreach (var rule in Rules)
                    {
                        var exists = false;
                        var rulesbroken = _member.GetClinicalRulesHistory(dependentID);
                        var hasAssignment = rule.hasAssignment;
                        var assignmentitem = rule.assignmentItem;

                        try
                        {
                            var prop = results[0].GetType().GetProperty(rule.pathologyField);
                            if (prop != null)
                            {
                                var value = prop.GetValue(results[0], null);
                                if (Convert.ToDecimal(value) != 0.00M)
                                {
                                    if (rule.greater != 0)
                                    {
                                        if (Convert.ToDecimal(value) > rule.greater)
                                        {
                                            foreach (var ruleBroken in rulesbroken.Where(x => x.ProgramCode == prog.programCode)) //HCare-1116

                                            {
                                                if (ruleBroken.PathologyID == pathID && ruleBroken.pathFieldName == rule.pathologyField && ruleBroken.pathFieldValue == value.ToString())
                                                    exists = true;
                                            }
                                            if (!exists)
                                            {
                                                InsertpathologyMessage(dependentID, rule.gtMessage, pathID, rule.pathologyField, value.ToString());
                                                if (hasAssignment == true)
                                                {
                                                    InsertAssignment(dependentID, pro, pathID, assignmentitem); //HCare-809
                                                }
                                            }
                                            count++;
                                        }
                                    }

                                    if (rule.less != 0)
                                    {
                                        //Hcare-1116
                                        if (Convert.ToDecimal(value) < rule.less)
                                        {
                                            foreach (var ruleBroken in rulesbroken.Where(x => x.ProgramCode == prog.programCode)) //HCare-1116
                                            {
                                                if (ruleBroken.PathologyID == pathID && ruleBroken.pathFieldName == rule.pathologyField && ruleBroken.pathFieldValue == value.ToString())
                                                    exists = true;
                                            }
                                            if (!exists)
                                            {
                                                InsertpathologyMessage(dependentID, rule.ltMessage, pathID, rule.pathologyField, value.ToString()); //hcare-1294: correction
                                                if (hasAssignment == true)
                                                {
                                                    InsertAssignment(dependentID, pro, pathID, assignmentitem); //HCare-809
                                                }
                                            }
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = ex;
                        }

                    }
                }
            }

            //if (count > 0) //removed as part of hcare-809
            //{
            //    InsertAssignment(dependentID, pro, pathID, "CLIIN"); //hcare-809
            //}
        }

        public List<ClinicalRules> GetPathologyRules(string type, string sex, Guid medicalAidId)
        {
            var male = false;
            var female = false;
            var rules = new List<ClinicalRules>();
            if (sex.ToUpper().Contains("F"))
            {
                female = true;
                rules = _member.GetClinicalRules().Where(x => x.active == true).Where(x => x.female == female).Where(x => x.ruleType == type).ToList();
            }
            else
            {
                male = true;
                rules = _member.GetClinicalRules().Where(x => x.active == true).Where(x => x.male == male).Where(x => x.ruleType == type).ToList();
            }

            return rules;
        }

        public bool InsertpathologyMessage(Guid depId, string rule, int pathid, string pathfield, string pathvalue)
        {
            var model = new PatientClinicalRulesHistory()
            {
                dependentId = depId,
                ruleBroken = rule,
                pathologyID = pathid,
                pathFieldName = pathfield, //HCare-1116
                pathFieldValue = pathvalue,
                //assignmentItem = assignmentItem,
                active = true,
            };
            ServiceResult result = _member.InsertClinicalRule(model);
            return false;
        }
        public bool InsertAssignment(Guid dependentID, Guid pro, int pathologyID, string assignmentItemType) //HCare-809
        {
            var model = new Assignments()
            {
                Active = true,
                assignmentType = "INTER",
                createdBy = "Clinical Rules",
                dependentID = dependentID,
                effectiveDate = DateTime.Now,
                pathologyID = pathologyID,
                programId = pro,
            };

            var assignmentview = new AssignmentsView()
            {
                newAssignment = model,
                assignmentItemType = assignmentItemType,
            };

            var assignExists = _member.GetAssignment(dependentID, assignmentview.assignmentItemType, pro);
            if (assignExists == null)
            {
                ServiceResult res = _member.InsertAssignment(assignmentview);

                var task = new AssignmentItemTasks();
                task.assignmentItemID = assignmentview.itemID;

                var tasks = _member.GetTaskRequirements(assignmentview.assignmentItemType);

                foreach (var tas in tasks)
                {
                    task.taskTypeId = tas.taskType;
                    task.requirementId = tas.requirementId;
                    _member.InsertTask(task);
                }
            }
            return false;
        }
    }
}