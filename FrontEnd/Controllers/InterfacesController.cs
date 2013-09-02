using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using System.Drawing;
using System.Diagnostics;

namespace FrontEnd.Controllers
{
    [Authorize(Roles = "AUH\\Heimdall_view")]
    public class InterfacesController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

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

            var interfaceInfo = tbl_interfaces.tbl_InterfaceInfo;

            object[,] ifInObject = new object[interfaceInfo.Count(), 2];
            object[,] ifOutObject = new object[interfaceInfo.Count(), 2];
            ViewBag.NoData = false;

            if (interfaceInfo.Count > 0)
            {
                int index = -1;
                long lastIfInOctets = 0;
                long lastIfOutOctets = 0;
                DateTime lastTimeStamp = interfaceInfo.FirstOrDefault().TimeStamp;

                foreach (var item in interfaceInfo)
                {
                    if (index != -1 && item.IsUp != false)
                    {
                        //ifInObject[index, 0] = lastTimeStamp.AddSeconds((item.TimeStamp.Value.Second) / 2); // get the average TimeStamp between current and last timestamps.
                        //ifOutObject[index, 0] = lastTimeStamp.AddSeconds((item.TimeStamp.Value.Second) / 2); // same as above.
                        ifInObject[index, 0] = item.TimeStamp;
                        ifOutObject[index, 0] = item.TimeStamp;

                        TimeSpan timeDiff = item.TimeStamp - lastTimeStamp;
                        long inDiff = item.IfInOctets.Value - lastIfInOctets;
                        long outDiff = item.IfOutOctets.Value - lastIfOutOctets;
                        int ifSpeed = item.IfSpeed.Value;

                        if (item.IsUp == true)
                        {
                            ifInObject[index, 1] = Math.Round(((inDiff * 8 * 100) / timeDiff.TotalSeconds) / ifSpeed, 2);
                            ifOutObject[index, 1] = Math.Round(((outDiff * 8 * 100) / timeDiff.TotalSeconds) / ifSpeed, 2);
                        }
                        else
                        {
                            ifInObject[index, 1] = null;
                            ifOutObject[index, 1] = null;
                        }
                    }
                    if (item.IsUp != false)
                    {
                        lastTimeStamp = item.TimeStamp;
                        lastIfInOctets = (long)item.IfInOctets;
                        lastIfOutOctets = (long)item.IfOutOctets;
                    }
                    index++;
                }
            }
            else
            {
                ViewBag.NoData = true;
                return View(new InterfaceDetailsViewModel() { tbl_interfaces = tbl_interfaces, InterfaceChart = new Highcharts("ifchart") });
            }

            #region Interface Chart
            Highcharts interfaceChart = new Highcharts("ifchart")
                .InitChart(new Chart { ZoomType = ZoomTypes.X, DefaultSeriesType = ChartTypes.Area, SpacingRight = 30, SpacingLeft = 30, BackgroundColor = new BackColorOrGradient(Color.Transparent) })
                .SetTitle(new Title { Text = "Interface Utilization" })
                .SetSubtitle(new Subtitle { Text = "Shows interface utilization in %" })
                .SetXAxis(new XAxis
                {
                    Type = AxisTypes.Datetime,
                    Labels = new XAxisLabels
                    {
                        Align = HorizontalAligns.Center,
                    },
                })
                .SetYAxis(new YAxis
                {
                    Max = 100,
                    Title = new YAxisTitle { Text = "Interface Utilization (%)" },
                    Labels = new YAxisLabels
                    {
                        Formatter = "function() { return (this.value) + '%'; }"
                    },
                    Min = 0
                })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.series.name +': '+ this.y +'%' +'</b><br/>'+ Highcharts.dateFormat('%e. %b - %H:%M:%S', this.x); }" })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new[] { new Series
                {
                    Name = "Incoming",
                    Data = new Data(ifInObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        //Color = System.Drawing.ColorTranslator.FromHtml("#FF0000"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5, PointStart = new PointStart(DateTime.Parse(ifInObject[0,0].ToString())) }
                }, new Series
                {
                    Name = "Outgoing",
                    Data = new Data(ifOutObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        //Color = System.Drawing.ColorTranslator.FromHtml("#FF0000"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5, PointStart = new PointStart(DateTime.Parse(ifOutObject[0,0].ToString())) }
                }  });
            #endregion

            return View(new InterfaceDetailsViewModel() { tbl_interfaces = tbl_interfaces, InterfaceChart = interfaceChart });
        }

        //
        // GET: /Interfaces/Create

        [Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create()
        {
            ViewBag.ClientID = new SelectList(db.tbl_Clients, "ClientID", "HostName");
            return View();
        }

        //
        // POST: /Interfaces/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
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

        [Authorize(Roles = "AUH\\Heimdall_admin")]
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
        [Authorize(Roles = "AUH\\Heimdall_admin")]
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

        [Authorize(Roles = "AUH\\Heimdall_admin")]
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
        [Authorize(Roles = "AUH\\Heimdall_admin")]
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