using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Libraries.Model;

using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

using SitefinityWebApp.Mvc.Models;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Versioning;
using System.Linq;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Modules.Libraries;
using System.Collections.Generic;
using Telerik.Sitefinity.Workflow;
using Telerik.Sitefinity.Lifecycle;

namespace SitefinityWebApp.Services
{
    public class TorrentService
    {
        private static string TORRENT_FILE_LIBRARY = "Torrents";
        private static string TORRENT_COVER_IMAGE_ALBUM = "Torrent Images";

        private ImageService _imageService;
        private DocumentsService _documentService;

        public TorrentService()
        {
            this._imageService = new ImageService();
            this._documentService = new DocumentsService();
        }

        public TorrentService(ImageService imageSerice, DocumentsService documentService)
        {
            this._imageService = imageSerice;
            this._documentService = documentService;
        }

        public void Upload(TorrentUploaderWidgetModel torrent)
        {
            string torrentExtention = Path.GetExtension(torrent.TorrentFile.FileName);
            string torrentTitle = Path.GetFileNameWithoutExtension(torrent.TorrentFile.FileName);
            Guid torrentFileId = this._documentService.UploadTorrentFile(torrentTitle, torrent.TorrentFile.InputStream, torrent.TorrentFile.FileName, torrentExtention, TORRENT_FILE_LIBRARY);

            string imageExtention = Path.GetExtension(torrent.CoverImage.FileName);
            string imageTitle = Path.GetFileNameWithoutExtension(torrent.CoverImage.FileName);
            Guid torrentImageId = this._imageService.Upload(imageTitle, torrent.CoverImage.InputStream, torrent.CoverImage.FileName, imageExtention, TORRENT_COVER_IMAGE_ALBUM);

            // Set the provider name for the DynamicModuleManager here. All available providers are listed in
            // Administration -> Settings -> Advanced -> DynamicModules -> Providers
            var providerName = String.Empty;

            // Set a transaction name and get the version manager
            var transactionName = "someTransactionName";
            var versionManager = VersionManager.GetManager(null, transactionName);

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
            Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Torrents.Torrent");
            DynamicContent torrentItem = dynamicModuleManager.CreateDataItem(torrentType);

            // This is how values for the properties are set
            torrentItem.SetValue("Title", torrent.Title);
            torrentItem.SetValue("Description", torrent.Description);

            LibrariesManager torrentFileManager = LibrariesManager.GetManager();
            var torrentFileItem = torrentFileManager.GetDocuments().FirstOrDefault(i => i.Id == torrentFileId && i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
            if (torrentFileItem != null)
                torrentItem.CreateRelation(torrentFileItem, "TorrentFile");

            LibrariesManager imageManager = LibrariesManager.GetManager();
            var imageItem = imageManager.GetImages().FirstOrDefault(i => i.Id == torrentImageId && i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);
            if (imageItem != null)
                torrentItem.CreateRelation(imageItem, "Image");
            
            torrentItem.SetString("UrlName", Guid.NewGuid().ToString());
            torrentItem.SetValue("Owner", SecurityManager.GetCurrentUserId());
            torrentItem.SetValue("PublicationDate", DateTime.UtcNow);

            // Create a version and commit the transaction in order changes to be persisted to data store
            torrentItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Draft");
            
            // Create a version and commit the transaction in order changes to be persisted to data store
            versionManager.CreateVersion(torrentItem, false);

            // We can now call the following to publish the item
            ILifecycleDataItem publishedTorrentItem = dynamicModuleManager.Lifecycle.Publish(torrentItem);

            // You need to set appropriate workflow status
            torrentItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");

            // Create a version and commit the transaction in order changes to be persisted to data store
            versionManager.CreateVersion(torrentItem, true);



            TransactionManager.CommitTransaction(transactionName);
        }
    }
}