using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;

namespace FrontEnd.Controllers
{
    [Authorize(Roles = "AUH\\Heimdall_view")]
    public class ClientTypesController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

        //
        // GET: /ClientTypes/

        public ActionResult Index()
        {
            return View(db.tbl_ClientTypes.ToList());
        }

        //
        // GET: /ClientTypes/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_ClientTypes tbl_clienttypes = db.tbl_ClientTypes.Find(id);
            if (tbl_clienttypes == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clienttypes);
        }

        //
        // GET: /ClientTypes/Create

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ClientTypes/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create(tbl_ClientTypes tbl_clienttypes)
        {
            if (ModelState.IsValid)
            {
                db.tbl_ClientTypes.Add(tbl_clienttypes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_clienttypes);
        }

        //
        // GET: /ClientTypes/Edit/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Edit(int id = 0)
        {
            tbl_ClientTypes tbl_clienttypes = db.tbl_ClientTypes.Find(id);
            if (tbl_clienttypes == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clienttypes);
        }

        //
        // POST: /ClientTypes/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Edit(tbl_ClientTypes tbl_clienttypes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_clienttypes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_clienttypes);
        }

        //
        // GET: /ClientTypes/Delete/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Delete(int id = 0)
        {
            tbl_ClientTypes tbl_clienttypes = db.tbl_ClientTypes.Find(id);
            if (tbl_clienttypes == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clienttypes);
        }

        //
        // POST: /ClientTypes/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_ClientTypes tbl_clienttypes = db.tbl_ClientTypes.Find(id);
            db.tbl_ClientTypes.Remove(tbl_clienttypes);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}