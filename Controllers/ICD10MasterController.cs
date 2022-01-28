using HaloCareCore.DAL;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HaloCareCore.Controllers
{
    public class ICD10MasterController : Controller
    {
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ICD10MasterController(OH17Context context, IConfiguration configuration )
        {
            _admin = new AdminRepository(context, configuration);
        }
        // GET: ICD10Master
        public ActionResult Index(string icd10Code)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index()
        {
            var icd10Code = "";
            var icd10CodeDescription = "";
            if (Request.Query["icd10Code"].ToString() != null)
                icd10Code = Request.Query["icd10Code"].ToString();
            if (Request.Query["icd10CodeDescription"].ToString() != null)
                icd10CodeDescription = Request.Query["icd10CodeDescription"].ToString();
            var iCD10Master = _admin.GetICD10MasteSearch(icd10Code, icd10CodeDescription);
            return View(iCD10Master);
        }
        public ActionResult Details(string icd10Code)
        {
            var model = _admin.GetICD10MasterById(icd10Code);
            return View(model);
        }
    }
}