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
    public class GroupsController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

        //
        // GET: /Groups/

        public ActionResult Index()
        {
            return View(db.tbl_Groups.ToList());
        }

        //
        // GET: /Groups/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_Groups tbl_groups = db.tbl_Groups.Find(id);
            if (tbl_groups == null)
            {
                return HttpNotFound();
            }
            return View(tbl_groups);
        }

        //
        // GET: /Groups/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Groups/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tbl_Groups tbl_groups)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Groups.Add(tbl_groups);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_groups);
        }

        //
        // GET: /Groups/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tbl_Groups tbl_groups = db.tbl_Groups.Find(id);
            if (tbl_groups == null)
            {
                return HttpNotFound();
            }
            return View(tbl_groups);
        }

        //
        // POST: /Groups/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tbl_Groups tbl_groups)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_groups).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_groups);
        }

        //
        // GET: /Groups/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tbl_Groups tbl_groups = db.tbl_Groups.Find(id);
            if (tbl_groups == null)
            {
                return HttpNotFound();
            }
            return View(tbl_groups);
        }

        //
        // POST: /Groups/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_Groups tbl_groups = db.tbl_Groups.Find(id);
            db.tbl_Groups.Remove(tbl_groups);
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