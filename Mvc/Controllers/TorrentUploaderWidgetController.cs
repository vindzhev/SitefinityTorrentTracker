using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;
using Telerik.Sitefinity.Libraries.Model;
using System.Web;
using System.IO;
using Telerik.Sitefinity.Modules.Libraries;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Telerik.Sitefinity.Workflow;
using System.Text;
using System.Globalization;
using SitefinityWebApp.Services;
using System.Net;

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "TorrentUploaderWidget", Title = "TorrentUploaderWidget", SectionName = "Torrents", ModuleName = "Torrents")]
    public class TorrentUploaderWidgetController : Controller
    {
        private TorrentService _torrentService;

        public TorrentUploaderWidgetController()
        {
            this._torrentService = new TorrentService();
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Category("String Properties")]
        public string Title { get; set; }

        [Category("String Properties")]
        public string Description { get; set; }

        [Category("File Properties")]
        public HttpPostedFileBase CoverImage { get; set; }

        [Category("File Properties")]
        public HttpPostedFileBase TorrentFile { get; set; }

        /// <summary>
        /// This is the default Action.
        /// </summary>
        public ActionResult Index()
        {
            return View("Default", new TorrentUploaderWidgetModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(TorrentUploaderWidgetModel model)
        {
            try
            {
                this._torrentService.Upload(model);
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText(@"C:\temp\inset.txt", $"{Environment.NewLine}{e.Message}");
                return View("Default", new TorrentUploaderWidgetModel());
            }

            return RedirectToAction(string.Empty);
        }
    }

}