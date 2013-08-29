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
    public class ProcessesController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

        //
        // GET: /Processes/

        public ActionResult Index()
        {
            var tbl_processes = db.tbl_Processes.Include(t => t.tbl_Clients);
            return View(tbl_processes.ToList());
        }

        //
        // GET: /Processes/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_Processes tbl_processes = db.tbl_Processes.Find(id);
            if (tbl_processes == null)
            {
                return HttpNotFound();
            }
            return View(tbl_processes);
        }

        //
        // GET: /Processes/Create

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create()
        {
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName");
            return View();
        }

        //
        // POST: /Processes/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create(tbl_Processes tbl_processes)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Processes.Add(tbl_processes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_processes.ClientID);
            return View(tbl_processes);
        }

        //
        // GET: /Processes/Edit/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Edit(int id = 0)
        {
            tbl_Processes tbl_processes = db.tbl_Processes.Find(id);
            if (tbl_processes == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_processes.ClientID);
            return View(tbl_processes);
        }

        //
        // POST: /Processes/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Edit(tbl_Processes tbl_processes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_processes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_processes.ClientID);
            return View(tbl_processes);
        }

        //
        // GET: /Processes/Delete/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Delete(int id = 0)
        {
            tbl_Processes tbl_processes = db.tbl_Processes.Find(id);
            if (tbl_processes == null)
            {
                return HttpNotFound();
            }
            return View(tbl_processes);
        }

        //
        // POST: /Processes/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_Processes tbl_processes = db.tbl_Processes.Find(id);
            db.tbl_Processes.Remove(tbl_processes);
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