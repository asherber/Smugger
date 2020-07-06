﻿using System;
using System.Threading.Tasks;

namespace SmugMug.NET.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting anonymous authentication tests");
            var apiAnonymous = AuthenticationSample.AuthenticateUsingAnonymous();
            FoldersSample.WorkingWithFoldersAndAlbums(apiAnonymous).Wait();
            NodesSample.WorkingWithNodes(apiAnonymous).Wait();
            ImagesSample.WorkingWithAlbumImages(apiAnonymous).Wait();
            UserSample.WorkingWithUsers(apiAnonymous).Wait();


            Console.WriteLine("Starting OAuth authentication tests");
            var apiOAuth = await AuthenticationSample.AuthenticateUsingOAuth();
            FoldersSample.WorkingWithFoldersAndAlbums(apiOAuth).Wait();
            FoldersSample.ManagingFoldersAndAlbums(apiOAuth).Wait();
            NodesSample.WorkingWithNodes(apiOAuth).Wait();
            NodesSample.ManagingNodes(apiOAuth).Wait();
            ImagesSample.WorkingWithAlbumImages(apiOAuth).Wait();
            UserSample.WorkingWithUsers(apiOAuth).Wait();
        }
    }
}
