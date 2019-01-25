using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Libraries.Model;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Telerik.Sitefinity.Workflow;
using System.Web;

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "AddTorrentWidget", Title = "AddTorrentWidget", SectionName = "Torrents")]
    public class AddTorrentWidgetController : Controller
    {
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
            return View("Default", new AddTorrentWidgetModel());
        }

        [HttpPost]
        public ActionResult Index(AddTorrentWidgetModel model)
        {
            try
            {
                string torrentTitle = model.TorrentFile.FileName.Split('.').First();
                string torrentExtention = model.TorrentFile.FileName.Split('.').Last();

                string imageTitle = model.CoverImage.FileName.Split('.').First();
                string imageExtention = model.CoverImage.FileName.Split('.').Last();

                Document torrent = CreateDocumentNativeAPI(Guid.NewGuid(), Guid.NewGuid(), torrentTitle, model.TorrentFile.InputStream, torrentTitle, "." + torrentExtention, "Torrents");
                Document image = CreateDocumentNativeAPI(Guid.NewGuid(), Guid.NewGuid(), imageTitle, model.CoverImage.InputStream, imageTitle, "." + imageExtention, "TorrentCoverImageLibrary");

                //CreateImageWithNativeAPI(Guid.NewGuid(), Guid.NewGuid(), imageTitle, model.TorrentFile.InputStream, imageTitle, "." + imageExtention);

                CreateTorrent(model, image, torrent);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(500);
            }

            return RedirectToAction("Index", "AddTorrentWidget");
        }

        // Creates a new torrent item
        public void CreateTorrent(AddTorrentWidgetModel model, Document image, Document torrent)
        {
            // Set the provider name for the DynamicModuleManager here. All available providers are listed in
            // Administration -> Settings -> Advanced -> DynamicModules -> Providers
            var providerName = String.Empty;

            // Set a transaction name and get the version manager
            var transactionName = "someTransactionName";
            var versionManager = VersionManager.GetManager(null, transactionName);

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
            Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.TorrentObject.Torrent");
            DynamicContent torrentItem = dynamicModuleManager.CreateDataItem(torrentType);

            // This is how values for the properties are set

            LibrariesManager coverImageManager = LibrariesManager.GetManager();
            //var coverImageItem = coverImageManager.GetImages().FirstOrDefault(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
            if (image != null)
            {
                // This is how we relate an item
                torrentItem.CreateRelation(image, "CoverImage");
            }

            LibrariesManager torrentFileManager = LibrariesManager.GetManager();
            //var torrentFileItem = torrentFileManager.GetDocuments().FirstOrDefault(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
            if (torrent != null)
            {
                // This is how we relate an item
                torrentItem.CreateRelation(torrent, "TorrentFile");
            }
            torrentItem.SetValue("Title", model.Title);
            torrentItem.SetValue("Description", model.Description);


            torrentItem.SetString("UrlName", model.Title);
            torrentItem.SetValue("Owner", SecurityManager.GetCurrentUserId());
            torrentItem.SetValue("PublicationDate", DateTime.UtcNow);


            torrentItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");

            // Create a version and commit the transaction in order changes to be persisted to data store
            versionManager.CreateVersion(torrentItem, true);
            TransactionManager.CommitTransaction(transactionName);
        }

        public static Document CreateDocumentNativeAPI(Guid masterDocumentId, Guid parentDocumentLibraryId, string documentTitle, Stream documentStream, string documentFileName, string documentExtension, string libraryTitle)
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();
            //Document document = librariesManager.GetDocuments().First();//.Where(d => d.Id != masterDocumentId).FirstOrDefault();

            //string torrentCoverImageLibrary = "TorrentCoverImageLibrary";
            //string torrentLibrary = "Torrents";
            DocumentLibrary documentLibrary = librariesManager.GetDocumentLibraries().Where(x => x.Title.Equals(libraryTitle)).FirstOrDefault();
            if (documentLibrary == null)
            {
                documentLibrary = librariesManager.CreateDocumentLibrary(new Guid());// GetDocumentLibraries().First();
            }

            //if (document == null)
            {
                //    //The document is created as master. The masterDocumentId is assigned to the master version.
                Document document = librariesManager.CreateDocument(masterDocumentId);

                //Set the parent document library.
                document.Parent = documentLibrary;

                //Set the properties of the document.
                document.Title = documentTitle;
                document.DateCreated = DateTime.UtcNow;
                document.PublicationDate = DateTime.UtcNow;
                document.LastModified = DateTime.UtcNow;
                document.UrlName = Regex.Replace(documentTitle.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
                document.MediaFileUrlName = Regex.Replace(documentFileName.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

                //Upload the document file.
                librariesManager.Upload(document, documentStream, documentExtension);

                //Recompiles and validates the url of the document.
                librariesManager.RecompileAndValidateUrls(document);

                //Save the changes.
                librariesManager.SaveChanges();

                //Publish the DocumentLibraries item. The live version acquires new ID.
                var bag = new Dictionary<string, string>();
                bag.Add("ContentType", typeof(Document).FullName);
                WorkflowManager.MessageWorkflow(masterDocumentId, typeof(Document), null, "Publish", false, bag);
                //}

                return document;
            }
        }
    }
}