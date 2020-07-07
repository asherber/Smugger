using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smugger.Samples
{
    class ImagesSample
    {
        public static async Task WorkingWithAlbumImages(SmugMugClient api)
        {
            //Get a specific album node, "TrBCmb"
            Album album = await api.GetAlbumAsync("TrBCmb");
            Console.WriteLine("Album '{0}' has {1} images", album.Name, album.ImageCount);

            //Get all the images in the given album
            var albumImages = await api.GetAlbumImagesAsync(album);
            
            //Get a specific image in the given album
            var albumImage = await api.GetAlbumImageAsync(album, "ktwWSFX-0");
            Console.WriteLine("'{0}' ({1}) with keywords \"{2}\"", albumImage.Title, albumImage.FileName, albumImage.Keywords);
        }
    }
}