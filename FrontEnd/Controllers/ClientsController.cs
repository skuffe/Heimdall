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
    public class ClientsController : Controller
    {
        private HeimdallContext db = new HeimdallContext();

        //
        // GET: /Client/

        public ActionResult Index()
        {
            var tbl_clients = db.tbl_Clients.Include(t => t.tbl_ClientTypes).Include(t => t.tbl_Groups);
            return View(tbl_clients.ToList());
        }

        //
        // GET: /Client/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            if (tbl_clients == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clients);
        }

        //
        // GET: /Client/Create

        public ActionResult Create()
        {
            ViewBag.ClientTypeID = new SelectList(db.tbl_ClientTypes, "ClientTypeID", "TypeName");
            ViewBag.GroupID = new SelectList(db.tbl_Groups, "GroupID", "GroupName");
            return View();
        }

        //
        // POST: /Client/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tbl_Clients tbl_clients)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Clients.Add(tbl_clients);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClientTypeID = new SelectList(db.tbl_ClientTypes, "ClientTypeID", "TypeName", tbl_clients.ClientTypeID);
            ViewBag.GroupID = new SelectList(db.tbl_Groups, "GroupID", "GroupName", tbl_clients.GroupID);
            return View(tbl_clients);
        }

        //
        // GET: /Client/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            if (tbl_clients == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClientTypeID = new SelectList(db.tbl_ClientTypes, "ClientTypeID", "TypeName", tbl_clients.ClientTypeID);
            ViewBag.GroupID = new SelectList(db.tbl_Groups, "GroupID", "GroupName", tbl_clients.GroupID);
            return View(tbl_clients);
        }

        //
        // POST: /Client/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tbl_Clients tbl_clients)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_clients).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClientTypeID = new SelectList(db.tbl_ClientTypes, "ClientTypeID", "TypeName", tbl_clients.ClientTypeID);
            ViewBag.GroupID = new SelectList(db.tbl_Groups, "GroupID", "GroupName", tbl_clients.GroupID);
            return View(tbl_clients);
        }

        //
        // GET: /Client/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            if (tbl_clients == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clients);
        }

        //
        // POST: /Client/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            db.tbl_Clients.Remove(tbl_clients);
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