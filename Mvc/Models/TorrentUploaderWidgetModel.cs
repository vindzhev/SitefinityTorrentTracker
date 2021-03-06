using System;
using System.Linq;
using System.Web;

namespace SitefinityWebApp.Mvc.Models
{
    public class TorrentUploaderWidgetModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public HttpPostedFileBase CoverImage { get; set; }

        public HttpPostedFileBase TorrentFile { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}