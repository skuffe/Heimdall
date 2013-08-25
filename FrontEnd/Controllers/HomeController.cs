using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using FrontEnd.Tools;

namespace FrontEnd.Controllers
{
    //[Authorize(Roles = "AUH\\Heimdall_view")]
    public class HomeController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

        public ActionResult Index()
        {
            ViewBag.ClientCount = db.tbl_Clients.Count();
            ViewBag.ErrorCount = 0;
            ViewBag.Now = DateTime.Now;

            var tbl_clients = db.tbl_Clients
                            .Include(t => t.tbl_ClientTypes)
                            .Include(t => t.tbl_Groups);

            var alerts = tbl_clients.ToList();
            var alertsRemove = new List<int>();

            foreach (var item in alerts)
            {
                if (item.tbl_ClientInfo.Count > 0 && item.tbl_ClientInfo.LastOrDefault().IsResponding == false)
                {
                    if (item.tbl_Groups.GroupID == 1)
                    {
                        alertsRemove.Add(alerts.IndexOf(item));
                        continue;
                    }

                    var lastUp = item.tbl_ClientInfo
                               .Where(m => m.IsResponding == true)
                               .OrderByDescending(m => m.ClientInfoID)
                               .FirstOrDefault();

                    if (lastUp != null)
                        item.DownTime = Tools.Tools.ToReadableString(DateTime.Now.Subtract(lastUp.TimeStamp.Value));
                    else
                        item.DownTime = "N/A";
                }
                else
                {
                    alertsRemove.Add(alerts.IndexOf(item));
                    continue;
                }
                ViewBag.ErrorCount++;
            }

            foreach (var item in alertsRemove)
            {
                if (alerts.Count > item) // prevent out of bounds.
                    alerts.RemoveAt(item);
            }

            if (Request.IsAjaxRequest())
                return PartialView("_AlertPartial", alerts);
            return View(alerts);
        }

        // POST: /Clients/Disable/#

        public ActionResult Disable(int id = 0)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            if (tbl_clients == null)
            {
                return HttpNotFound();
            }
            else
            {
                tbl_clients.GroupID = 1;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}
