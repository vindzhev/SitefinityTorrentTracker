using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace SitefinityWebApp.Services
{
    public class DocumentsService
    {
        public Guid UploadTorrentFile(string torrentTitle, Stream torrentFileStream, string torrentFileName, string torrentExtention, string torrentLibrary)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            LibrariesManager librariesManager = LibrariesManager.GetManager(LibrariesManager.GetDefaultProviderName());
            DocumentLibrary documentLibrary = librariesManager.GetDocumentLibraries().Where(l => l.Title.Equals(torrentLibrary)).SingleOrDefault();

            Document document = librariesManager.CreateDocument();
            document.Parent = documentLibrary;            
            document.Title = torrentTitle;
            document.DateCreated = DateTime.UtcNow;
            document.PublicationDate = DateTime.UtcNow;
            document.LastModified = DateTime.UtcNow;
            document.UrlName = Regex.Replace(torrentTitle.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            document.MediaFileUrlName = Regex.Replace(torrentFileName.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

            librariesManager.Upload(document, torrentFileStream, torrentExtention);
            librariesManager.RecompileAndValidateUrls(document);

            document.ApprovalWorkflowState.Value = "Published";

            librariesManager.Lifecycle.Publish(document, culture);
            librariesManager.SaveChanges();

            return document.Id;
        }
    }
}