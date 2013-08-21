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
        private HeimdallContext db = new HeimdallContext();

        public ActionResult Index()
        {
            ViewBag.ClientCount = db.tbl_Clients.Count();
            ViewBag.ErrorCount = 0;
            ViewBag.Now = DateTime.Now;

            var tbl_clients = db.tbl_Clients.Include(t => t.tbl_ClientTypes).Include(t => t.tbl_Groups);
            var tbl_clientinfo = db.tbl_ClientInfo.Include(t => t.tbl_Clients);

            foreach (var item in tbl_clientinfo)
            {
                if (!(bool)item.IsResponding)
                {
                    ViewBag.ErrorCount++;
                }
                else if ((bool)item.IsResponding)
                {
                    item.tbl_Clients.DownTime = Tools.Tools.ToReadableString(DateTime.Now.Subtract(item.TimeStamp.Value));
                }
            }

            if (Request.IsAjaxRequest())
                return PartialView("_AlertPartial", tbl_clients);
            return View(tbl_clients);
        }
    }
}
