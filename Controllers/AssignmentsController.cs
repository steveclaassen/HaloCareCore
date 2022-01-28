using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaloCareCore.Controllers
{
    public class AssignmentsController : Controller
    {
        // GET: Assignments
        public ActionResult Index()
        {
            return View();
        }

        // GET: Assignments/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Assignments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Assignments/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Assignments/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Assignments/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("patientAssignments");
            }
            catch
            {
                return View();
            }
        }

        // GET: Assignments/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Assignments/Delete/5
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



    }
}
