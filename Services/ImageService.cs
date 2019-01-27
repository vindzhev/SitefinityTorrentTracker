using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace SitefinityWebApp.Services
{
    public class ImageService
    {
        public Guid Upload(string imageTitle, Stream imageStream, string imageFileName, string imageExtension, string albumTitle)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            LibrariesManager librariesManager = LibrariesManager.GetManager(LibrariesManager.GetDefaultProviderName());
            Album album = librariesManager.GetAlbums().Where(i => i.Title.Equals(albumTitle)).SingleOrDefault();
            
            Image image = librariesManager.CreateImage();
            image.Parent = album;
            image.Title[culture] = imageTitle;
            image.DateCreated = DateTime.UtcNow;
            image.LastModified = DateTime.UtcNow;
            image.PublicationDate = DateTime.UtcNow;
            image.UrlName[culture] = Regex.Replace(imageTitle.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            image.MediaFileUrlName[culture] = Regex.Replace(imageFileName.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            image.ApprovalWorkflowState.Value = "Published";
            
            librariesManager.Upload(image, imageStream, imageExtension);
            librariesManager.RecompileItemUrls(image);
            
            librariesManager.Lifecycle.Publish(image, culture);
            librariesManager.SaveChanges();

            return image.Id;
        }

        public Image GetById(Guid masterImageId)
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();
            Image image = librariesManager.GetImages().Where(i => i.Id == masterImageId).FirstOrDefault();

            if (image != null)
                image = librariesManager.Lifecycle.GetLive(image) as Image;

            return image;
        }
    }
}