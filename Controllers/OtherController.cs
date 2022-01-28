using HaloCareCore.DAL;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HaloCareCore.Controllers
{
    public class OtherController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public OtherController(OH17Context context, IConfiguration _configuration)
        {
            _admin = new AdminRepository(context, _configuration);
            _member = new MemberRepository(_configuration, context);

        }
        public ActionResult Index()
        {
            var viewModel = new OtherVM()
            {
                AttachmentTypes = _admin.GetAttachmentTypes(),
                Laboratories = _admin.GetListOfLaboratories(),
                NonCLDFlags = _admin.GetNonCDLFlagsList(),
                PathologyTypes = _member.GetPathologyTypeList(),
                PreferredMethodOfContacts = _admin.GetListofPMOC(),
                Icd10Codes = _admin.GetListofICD10Codes(), //hcare-1280
            };
            //HCare-1339
            foreach (var item in viewModel.AttachmentTypes)
            {
                if (!string.IsNullOrEmpty(item.assignmentItemType))
                    item.assignmentItemType = _admin.GetAssignmentItemTypeByCode(item.assignmentItemType).itemDescription;
            }

            return View(viewModel);
        }
    }
}