using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;

namespace HaloCareCore.Repos
{
    public interface IClinicalRepository:IDisposable
    {
        List<OpenAssignments> GetOpenAssignments(int? count);
        List<OpenAssignments> GetOpenAssignmentsFull(int? count);
        List<CompactOpenAssignments> GetCompactOpenAssignments();
        List<OpenAssignments> GetOpenAssignments(Guid depID);
        List<OpenAssignments> GetOpenAssignmentPatients();
        List<OpenAssignments> GetClosedAssignments();
        List<CompactOpenAssignments> GetCompactClosedAssignments();
        Assignments GetAssignmentById(int id);
        List<TaskViewModel> GetTasks(int id, string itemType);
        TaskViewModel GetTaskView(int id);
        AssignmentItemTasks GetTask(int id);
        AssignmentItemTasks GetTaskByComment(string id);
        ServiceResult UpdateTask(AssignmentItemTasks task);
        List<AssignmentItems> GetAssignmentItemsById(int id);
        List<AssignmentItemView> GetAsItemViewById(int id);
        ServiceResult UpdateAssignment(Assignments model);
        List<PostponedAssignments> GetPostponedAssignments();
        List<OpenAssignments> GetInProgressAssignments();
        List<CompactOpenAssignments> GetCompactInProgressAssignments();
        List<OpenAssignments> GetDocumentAssignments();
        int InsertAuthorizationRequest(Authorisations model);
        ServiceResult UpdateAuthorizationRequest(Authorisations model);
        string GetEmailTemplate(int id);
        string GetSmsTemplate(int id);
        List<SmsMessageTemplates> GetSmsTemplate();
        string getProgram(Guid dependantID);
        string getBHFNumber(int scriptNo);
        AssignmentItems GetAssignmentItemById(int id);
        ServiceResult UpdateAssignmentItem(AssignmentItems model);
        List<QuestionnaireTemplate> GetQuestionnaireList(string TemplateName);
        ServiceResult InsertPatientQuestionnaireResponse(PatientQuestionnaireResponse model);

        ServiceResult InsertPatientDisclaimer(PatientDisclaimer model);
        PatientDisclaimer GetPatientDisclaimerById(int id);
        List<PatientDisclaimer> GetPatientDisclaimerResults(Guid depId);
        ServiceResult UpdatePatientDisclaimerResult(PatientDisclaimer model);


    }
}