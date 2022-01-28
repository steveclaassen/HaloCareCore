using HaloCareCore.DAL;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HaloCareCore.Controllers
{
    public class RulesController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public RulesController(OH17Context context, IConfiguration configuration)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);

        }
        public ActionResult Index()
        {
            var viewModel = new RulesVM()
            {
                ClinicalRules = _admin.GetClinicalRiskRulesList(),
                HIVRiskRatingRules = _admin.GetHIVRiskRules(),
                MHRiskRatingRules = _admin.GetMHRiskRatingRules(),
                RiskRules = _admin.GetHypoRiskRules(),
                LifeExpectancyRules = _admin.GetLifeRules(),
                HIVStagingRiskRules = _admin.GetHIVStageRiskRules(),
                HivcomorbidRules = _admin.GetHivcomorbidRules(),
                RiskRatingMonitors = _admin.GetRiskRatingMonitorIndex(),
            };
            return View(viewModel);
        }
    }
}