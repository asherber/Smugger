using SmugMug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SmugMug.NET.Samples
{
    class FoldersSample
    {
        public static async Task WorkingWithFoldersAndAlbums(SmugMugClient api)
        {
            //Get access to the user you want to enumerate albums for
            User user = await api.GetUserAsync("cmac");
            Console.WriteLine(user.Name);

            //Get a specific folder, "SmugMug"
            Folder folder = await api.GetFolderAsync(user, "SmugMug");
            Console.WriteLine(folder);

            //Get a specific subfolder, "Heroes" under folder "SmugMug"
            Folder subFolder = await api.GetFolderAsync("cmac", "SmugMug/Heroes");
            Console.WriteLine(subFolder);

            //Get the first 100 albums for the user
            List<Album> albums = await api.GetAlbumsAsync(user, 100);
            Console.WriteLine("The first album is '{0}' with {1} images", albums[0].Name, albums[0].ImageCount);

            //Get the featured albums for the user
            List<Album> featuredAlbums = await api.GetFeaturedAlbumsAsync(user);
            Console.WriteLine("{0} has {1} featured albums", user.Name, featuredAlbums.Count);

            //Get a specific album, "SJT3DX"
            Album album = await api.GetAlbumAsync("SJT3DX");
            Console.WriteLine("Album '{0}' has {1} images", album.Name, album.ImageCount);
        }

        public static async Task ManagingFoldersAndAlbums(SmugMugClient api)
        {
            //Get access to the logged in user
            User user = await api.GetAuthenticatedUserAsync();
            Console.WriteLine("{0} is currently authenticated", user.Name);

            //Create a new folder at the root
            Folder folder = await api.CreateFolderAsync("TestFolder", user, "");
            Console.WriteLine("Created folder {0}", folder.Name);

            //Create a new album in that folder
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "test description" } };
            Album album = await api.CreateAlbumAsync("TestAlbum", folder, arguments);
            Console.WriteLine("Created album {0}: {1}", album.Name, album.Description);

            Dictionary<string, string> albumUpdates = new Dictionary<string, string>() { { "Name", "Updated Album Name" }, { "Description", "Updated description" }, { "SortDirection", "Ascending" } };
            Album updatedAlbum = await api.UpdateAlbumAsync(album, albumUpdates);
            Console.WriteLine("Updated album {0}: {1}", updatedAlbum.Name, updatedAlbum.Description);

            //Delete the newly created album and folder
            await api.DeleteAlbumAsync(album);
            await api.DeleteFolderAsync(folder);
        }
    }
}
