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
    //[Authorize(Roles = "AUH\\Heimdall_view")]
    public class HomeController : Controller
    {
        private HeimdallContext db = new HeimdallContext();

        public ActionResult Index()
        {
            ViewBag.ClientCount = db.tbl_Clients.Count();
            return View(db.tbl_Clients.ToList());
        }
    }
}
