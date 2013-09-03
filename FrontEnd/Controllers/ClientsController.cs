using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using FrontEnd.Tools;

namespace FrontEnd.Controllers
{
    [Authorize(Roles = "AUH\\Heimdall_view")]
    public class ClientsController : Controller
    {
        private heimdallEntities db = new heimdallEntities();

        //
        // GET: /Clients/

        public ActionResult Index()
        {
            var tbl_clients = db.tbl_Clients.Include(t => t.tbl_ClientTypes).Include(t => t.tbl_Groups);

            // Check FTP for config files.
            //FtpConfig ftp = new FtpConfig { Server = "10.0.10.11", Port = "21", Username = "config", Password = "Config1234" };
            //List<string> folderList = Tools.Tools.GetFileList(ftp);

            //foreach (var client in tbl_clients)
            //{
            //    string search = client.HostName.Replace(".auh.lan", "").ToLower();
            //    var match = folderList.FirstOrDefault(s => s.ToLower().Contains(search));

            //    if (match != null)
            //    {
            //        ftp.RemotePath = search;
            //        List<string> fileList = Tools.Tools.GetFileList(ftp);
            //        ftp.RemotePath = null;

            //        string[] data = new string[fileList.Count];
            //        int index = 0;
            //        foreach (var file in fileList)
            //        {
            //            data[index] = Tools.Tools.GetFile(ftp, file);
            //            index++;
            //        }
            //    }
            //}

            return View(tbl_clients);
        }

        //
        // GET: /Clients/Details/5

        public ActionResult Details(int id = 0)
        {
            tbl_Clients tbl_clients = db.tbl_Clients.Find(id);
            if (tbl_clients == null)
                return HttpNotFound();

            var clientInfo = tbl_clients.tbl_ClientInfo;
                //.Where(p => p.RAM != null);

            object[,] ramObject = new object[clientInfo.Count(), 2];
            object[,] cpuObject = new object[clientInfo.Count(), 2];
            object[,] uptimeObject = new object[clientInfo.Count(), 2];
            object[,] pingObject = new object[clientInfo.Count(), 2];
            ViewBag.NoData = false;

            if (clientInfo.Count > 0 && tbl_clients.tbl_ClientTypes.IsSNMPDevice == false)
            {
                int index = 0;
                foreach (var item in clientInfo)
                {
                    ramObject[index, 0] = item.TimeStamp;
                    cpuObject[index, 0] = item.TimeStamp;
                    uptimeObject[index, 0] = item.TimeStamp;
                    pingObject[index, 0] = item.TimeStamp;
                    if (item.IsResponding == true && item.tbl_Clients.tbl_ClientTypes.IsSNMPDevice == false)
                    {
                        ramObject[index, 1] = Convert.ToInt32(new String(item.RAM.Where(Char.IsDigit).ToArray()));
                        pingObject[index, 1] = Convert.ToInt32(new String(item.Ping.Where(Char.IsDigit).ToArray()));
                        cpuObject[index, 1] = Convert.ToDouble(item.CPU.TrimEnd('%').Replace('.', ','));

                        string uptime = item.UpTime;

                        string re1 = "(\\d+)";	// Get uptime integer 1 (Days)
                        string re2 = ".*?";	// Non-greedy match on filler
                        string re3 = "(\\d+)";	// Get uptime integer 2 (Hours)
                        string re4 = ".*?";	// Non-greedy match on filler
                        string re5 = "(\\d+)";	// Get uptime integer 3 (Minutes)
                        string re6 = ".*?";	// Non-greedy match on filler
                        string re7 = "(\\d+)";	// Get uptime integer 4 (Seconds)

                        Regex r = new Regex(re1 + re2 + re3 + re4 + re5 + re6 + re7, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match m = r.Match(uptime);
                        if (m.Success)
                        {
                            TimeSpan t1 = new TimeSpan(Convert.ToInt32(m.Groups[1].ToString()), 0, 0, 0);
                            TimeSpan t2 = new TimeSpan(0, Convert.ToInt32(m.Groups[2].ToString()), 0, 0);
                            TimeSpan t3 = new TimeSpan(0, 0, Convert.ToInt32(m.Groups[3].ToString()), 0);
                            TimeSpan t4 = new TimeSpan(0, 0, 0, Convert.ToInt32(m.Groups[4].ToString()));
                            TimeSpan uptimeSpan = t1 + t2 + t3 + t4;
                            uptimeObject[index, 1] = uptimeSpan.TotalDays;
                        }

                    }
                    else
                    {
                        ramObject[index, 1] = null;
                        cpuObject[index, 1] = null;
                        uptimeObject[index, 1] = null;
                        pingObject[index, 1] = null;
                    }
                    index++;
                }
            }
            else
            {
                ViewBag.NoData = true;
                return View(new ClientDetailsViewModel() { tbl_clients = tbl_clients, RAMChart = new Highcharts("ramchart"), CPUChart = new Highcharts("cpuchart"), UptimeChart = new Highcharts("uptimechart") });
            }

            #region RAM Chart
            Highcharts ramChart = new Highcharts("ramchart")
                .InitChart(new Chart { ZoomType = ZoomTypes.X, DefaultSeriesType = ChartTypes.Area, SpacingRight = 30, SpacingLeft = 30, BackgroundColor = new BackColorOrGradient(Color.Transparent) })
                .SetTitle(new Title { Text = "RAM" })
                .SetSubtitle(new Subtitle { Text = "Shows amount of RAM used in MB" })
                .SetXAxis(new XAxis
                {
                    Type = AxisTypes.Datetime,
                    Labels = new XAxisLabels
                    {
                        Align = HorizontalAligns.Center,
                    }
                })
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "RAM Utilization (MB)" },
                    Labels = new YAxisLabels
                    {
                        Formatter = "function() { return (this.value) + 'MB'; }"
                    },
                    Min = 0
                })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.series.name +': '+ this.y +'MB' +'</b><br/>'+ Highcharts.dateFormat('%e. %b - %H:%M:%S', this.x); }" })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                {
                    Name = "RAM Usage",
                    Data = new Data(ramObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        //Color = System.Drawing.ColorTranslator.FromHtml("#FF0000"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5 }
                });
            #endregion

            #region CPU Chart
            Highcharts cpuChart = new Highcharts("cpuchart")
                .InitChart(new Chart { ZoomType = ZoomTypes.X, DefaultSeriesType = ChartTypes.Area, SpacingRight = 30, SpacingLeft = 30, BackgroundColor = new BackColorOrGradient(Color.Transparent) })
                .SetTitle(new Title { Text = "CPU" })
                .SetSubtitle(new Subtitle { Text = "Shows amount of CPU utilization in %" })
                .SetXAxis(new XAxis
                {
                    Type = AxisTypes.Datetime,
                    Labels = new XAxisLabels
                    {
                        Align = HorizontalAligns.Center,
                    }
                })
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "CPU Utilization (%)" },
                    Labels = new YAxisLabels
                    {
                        Formatter = "function() { return (this.value) + '%'; }"
                    },
                    Min = 0,
                    Max = 100
                })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.series.name +': '+ this.y +'%' +'</b><br/>'+ Highcharts.dateFormat('%e. %b - %H:%M:%S', this.x); }" })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                {
                    Name = "CPU Usage",
                    Data = new Data(cpuObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        Color = System.Drawing.ColorTranslator.FromHtml("#FF0000"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5 }
                });
            #endregion

            #region Uptime Chart
            Highcharts uptimeChart = new Highcharts("uptimechart")
                .InitChart(new Chart { ZoomType = ZoomTypes.X, DefaultSeriesType = ChartTypes.Area, SpacingRight = 30, SpacingLeft = 30, BackgroundColor = new BackColorOrGradient(Color.Transparent) })
                .SetOptions(new GlobalOptions { Global = new Global { UseUTC = false } })
                .SetTitle(new Title { Text = "Uptime" })
                .SetSubtitle(new Subtitle { Text = "Shows uptime" })
                .SetXAxis(new XAxis
                {
                    Type = AxisTypes.Datetime,
                    Labels = new XAxisLabels
                    {
                        Align = HorizontalAligns.Center,
                    }
                })
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "Uptime (Days)" },
                    Labels = new YAxisLabels
                    {
                        Formatter = "function() { return (this.value) + ' Days'; }"
                    },
                    Min = 0,
                })
                //.SetTooltip(new Tooltip { Shared = false })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.series.name +': '+ parseFloat(Math.round(this.y * 100) / 100).toFixed(2) +' Days'+'</b><br/>'+ Highcharts.dateFormat('%e. %b - %H:%M:%S', this.x); }" })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                {
                    Name = "Uptime",
                    Data = new Data(uptimeObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        Color = System.Drawing.ColorTranslator.FromHtml("#00FF00"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5 }
                });
            #endregion

            #region Ping Chart
            Highcharts pingChart = new Highcharts("pingchart")
                .InitChart(new Chart { ZoomType = ZoomTypes.X, DefaultSeriesType = ChartTypes.Area, SpacingRight = 30, SpacingLeft = 30, BackgroundColor = new BackColorOrGradient(Color.Transparent) })
                .SetOptions(new GlobalOptions { Global = new Global { UseUTC = false } })
                .SetTitle(new Title { Text = "Ping" })
                .SetSubtitle(new Subtitle { Text = "Shows ping times" })
                .SetXAxis(new XAxis
                {
                    Type = AxisTypes.Datetime,
                    Labels = new XAxisLabels
                    {
                        Align = HorizontalAligns.Center,
                    }
                })
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "Ping (ms)" },
                    Labels = new YAxisLabels
                    {
                        Formatter = "function() { return (this.value) + ' ms'; }"
                    },
                    Min = 0,
                })
                //.SetTooltip(new Tooltip { Shared = false })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.series.name +': '+ this.y +' ms'+'</b><br/>'+ Highcharts.dateFormat('%e. %b - %H:%M:%S', this.x); }" })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                {
                    Name = "Ping",
                    Data = new Data(pingObject),
                    PlotOptionsSeries = new PlotOptionsSeries
                    {
                        Color = System.Drawing.ColorTranslator.FromHtml("#0000FF"),
                        Marker = new PlotOptionsSeriesMarker
                        {
                            States = new PlotOptionsSeriesMarkerStates { Hover = new PlotOptionsSeriesMarkerStatesHover { Radius = 4 } },
                            Enabled = false
                        }
                    },
                    PlotOptionsArea = new PlotOptionsArea { FillOpacity = 0.5 }
                });
            #endregion

            return View(new ClientDetailsViewModel() { tbl_clients = tbl_clients, RAMChart = ramChart, CPUChart = cpuChart, UptimeChart = uptimeChart, PingChart = pingChart });
        }

        //
        // GET: /Clients/Create

        //[Authorize(Roles = "AUH\\Heimdall_admin")]
        public ActionResult Create()
        {
            ViewBag.ClientTypeID = new SelectList(db.tbl_ClientTypes, "ClientTypeID", "TypeName");
            ViewBag.GroupID = new SelectList(db.tbl_Groups, "GroupID", "GroupName");
            return View();
        }

        //
        // POST: /Clients/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "AUH\\Heimdall_admin")]
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
        // GET: /Clients/Edit/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
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
        // POST: /Clients/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
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
        // GET: /Clients/Delete/5

        [Authorize(Roles = "AUH\\Heimdall_admin")]
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
        // POST: /Clients/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AUH\\Heimdall_admin")]
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