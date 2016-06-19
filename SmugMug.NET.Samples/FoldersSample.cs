﻿using SmugMug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SmugMug.NET.Samples
{
    class FoldersSample
    {
        public static async Task WorkingWithFoldersAndAlbums(SmugMugAPI api)
        {
            //Get access to the user you want to enumerate albums for
            User user = await api.GetUser("cmac");
            Console.WriteLine(user.Name);

            //Get a specific folder, "SmugMug"
            Folder folder = await api.GetFolder(user, "SmugMug");
            Console.WriteLine(folder);

            //Get a specific subfolder, "Heroes" under folder "SmugMug"
            Folder subFolder = await api.GetFolder("cmac", "SmugMug/Heroes");
            Console.WriteLine(subFolder);

            //Get the first 100 albums for the user
            List<Album> albums = await api.GetAlbums(user, 100);
            Console.WriteLine("The first album is '{0}' with {1} images", albums[0].Name, albums[0].ImageCount);

            //Get the featured albums for the user
            List<Album> featuredAlbums = await api.GetFeaturedAlbums(user);
            Console.WriteLine("{0} has {1} featured albums", user.Name, featuredAlbums.Count);

            //Get a specific album, "SJT3DX"
            Album album = await api.GetAlbum("SJT3DX");
            Console.WriteLine("Album '{0}' has {1} images", album.Name, album.ImageCount);
        }
    }
}
