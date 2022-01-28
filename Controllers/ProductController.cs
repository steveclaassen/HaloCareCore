using HaloCareCore.DAL;
using HaloCareCore.Management;
using HaloCareCore.Models.Script;
using HaloCareCore.Repos;
using HaloCareCore.XmlModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace HaloCareCore.Controllers
{
    public class ProductController : Controller
    {
        private IAdminRepository _admin;

        private readonly MemoryCache _memoryCache;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductController(OH17Context context, IConfiguration configuration)
        {
            this._admin = new AdminRepository(context, configuration);
        }

        // GET: Product
        public ActionResult Index(string nappi)
        {
            if (nappi != null)
            {
                if (_memoryCache.Get("Nappies") == null)
                {
                    _memoryCache.Set("Nappies",_admin.GetProductsSearch(nappi, ""));
                    var model = _memoryCache.Get("Nappies");
                    return View(model);
                }
                else
                {
                    var model = _memoryCache.Get("Nappies");
                    return View(model);
                }
                // var model = _admin.GetProductsSearch(nappi, "");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Index()
        {
            var nappi = "";
            var prodName = "";

            if (Request.Query["nappi"].ToString() != null)
                nappi = Request.Query["nappi"].ToString();

            if (Request.Query["productname"].ToString() != null)
                prodName = Request.Query["productname"].ToString();

            ViewBag.productname = prodName;
            ViewBag.nappi = nappi;

            var products = _admin.GetProductsSearch(nappi, prodName);
            return View(products);

        }

        // GET: Product/Details/5
        public ActionResult Details(string nappi)
        {
            var model = _admin.GetProduct(nappi);
            return View(model);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        public ActionResult Create(Products product)
        {
            try
            {
                var res = _admin.GetProduct(product.nappiCode);
                if (res == null)
                {
                    product.createdBy = User.Identity.Name;
                    _admin.InsertPoduct(product);
                }
                else
                {
                    ModelState.AddModelError("", "Product Already in database");
                    return View(product);
                }

                return RedirectToAction("Index", new { nappi = product.nappiCode });
            }
            catch
            {
                return View();
            }
        }

        //G/*ET: Product/Edit/5*/
        //public ActionResult Edit(string id)
        //{
        //    var model = _admin.GetProduct(id);
        //    return View(model);
        //}

        // POST: Product/Edit/5

        public JsonResult Edit(string nappiCode)
        {

            var all = Request.Query.Keys;
            try
            {
                var jsonObj = new productRequest()
                {
                    productDetails = new productDetails()
                    {
                        productNo = nappiCode
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(jsonObj);

                string url = Configuration.GetSection("EmailSettings")["IseriesProductUpdateUrl"].ToString();

                var res = ProductValidationService.POSTREQ(url, jsonRequest);


                var result = res.detailsResponse;

                var status = false;

                if (result.productStatus.ToUpper() == "A")
                    status = true;

                var product = new Products()
                {
                    oldNappicode = nappiCode,
                    nappiCode = result.productNo,
                    productName = result.productName,
                    packSize = (int)result.totalPackQuantity,
                    modifiedBy = "HCareWebservice",
                    modifiedDate = DateTime.Now,
                    Active = status,
                    atcCode = result.atcCode,
                    therapeuticClass6Desc = result.therapeuticClass6Desc,
                    therapeuticClass6 = result.therapeuticClass6,
                    mmapIndicator = result.mmapIndicator,
                    mrpIndicator = result.mrpIndicator,
                    packUOM = result.packUOM,
                    genericIndicator = result.genericIndicator,
                    productSchedule = result.productSchedule,
                    strength = result.strengthUOM,
                    productStatus = result.productStatus,
                    createdDate = DateTime.Now,
                   
                };

                _admin.UpdateProduct(product);

                //  var products = _admin.GetProductsSearch(nappi, productname);
                return Json(product);
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
                return Json("");
            }
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Product/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ProductCheck(string NappiCode)
        {
            var options = _admin.GetProductByNappi(NappiCode);
            return Json(options);
        }
    }
}
