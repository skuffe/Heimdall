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
    public class InterfacesController : Controller
    {
        private HeimdallContext db = new HeimdallContext();

        //
        // GET: /Interfaces/

        public ActionResult Index()
        {
            var tbl_interfaces = db.tbl_Interfaces.Include(t => t.tbl_Clients);
            return View(tbl_interfaces.ToList());
        }

        //
        // GET: /Interfaces/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_Interfaces tbl_interfaces = db.tbl_Interfaces.Find(id);
            if (tbl_interfaces == null)
            {
                return HttpNotFound();
            }
            return View(tbl_interfaces);
        }

        //
        // GET: /Interfaces/Create

        public ActionResult Create()
        {
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName");
            return View();
        }

        //
        // POST: /Interfaces/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tbl_Interfaces tbl_interfaces)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Interfaces.Add(tbl_interfaces);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_interfaces.ClientID);
            return View(tbl_interfaces);
        }

        //
        // GET: /Interfaces/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tbl_Interfaces tbl_interfaces = db.tbl_Interfaces.Find(id);
            if (tbl_interfaces == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_interfaces.ClientID);
            return View(tbl_interfaces);
        }

        //
        // POST: /Interfaces/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tbl_Interfaces tbl_interfaces)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_interfaces).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_interfaces.ClientID);
            return View(tbl_interfaces);
        }

        //
        // GET: /Interfaces/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tbl_Interfaces tbl_interfaces = db.tbl_Interfaces.Find(id);
            if (tbl_interfaces == null)
            {
                return HttpNotFound();
            }
            return View(tbl_interfaces);
        }

        //
        // POST: /Interfaces/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_Interfaces tbl_interfaces = db.tbl_Interfaces.Find(id);
            db.tbl_Interfaces.Remove(tbl_interfaces);
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