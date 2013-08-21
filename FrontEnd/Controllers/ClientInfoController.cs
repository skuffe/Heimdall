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
    public class ClientInfoController : Controller
    {
        private HeimdallContext db = new HeimdallContext();

        //
        // GET: /ClientInfo/

        public ActionResult Index()
        {
            var tbl_clientinfo = db.tbl_ClientInfo.Include(t => t.tbl_Clients);
            return View(tbl_clientinfo.ToList());
        }

        //
        // GET: /ClientInfo/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_ClientInfo tbl_clientinfo = db.tbl_ClientInfo.Find(id);
            if (tbl_clientinfo == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clientinfo);
        }

        //
        // GET: /ClientInfo/Create

        public ActionResult Create()
        {
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName");
            return View();
        }

        //
        // POST: /ClientInfo/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tbl_ClientInfo tbl_clientinfo)
        {
            if (ModelState.IsValid)
            {
                db.tbl_ClientInfo.Add(tbl_clientinfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_clientinfo.ClientID);
            return View(tbl_clientinfo);
        }

        //
        // GET: /ClientInfo/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tbl_ClientInfo tbl_clientinfo = db.tbl_ClientInfo.Find(id);
            if (tbl_clientinfo == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_clientinfo.ClientID);
            return View(tbl_clientinfo);
        }

        //
        // POST: /ClientInfo/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tbl_ClientInfo tbl_clientinfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_clientinfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName", tbl_clientinfo.ClientID);
            return View(tbl_clientinfo);
        }

        //
        // GET: /ClientInfo/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tbl_ClientInfo tbl_clientinfo = db.tbl_ClientInfo.Find(id);
            if (tbl_clientinfo == null)
            {
                return HttpNotFound();
            }
            return View(tbl_clientinfo);
        }

        //
        // POST: /ClientInfo/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_ClientInfo tbl_clientinfo = db.tbl_ClientInfo.Find(id);
            db.tbl_ClientInfo.Remove(tbl_clientinfo);
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