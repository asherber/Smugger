using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smugger
{
    public interface ISmugMugClient
    {
        Task<Album> GetAlbumAsync(string albumId);
        Task<List<Album>> GetAlbumsAsync(User user, int maxAlbumCount = int.MaxValue);
        Task<List<Album>> GetFeaturedAlbumsAsync(User user, int maxAlbumCount = int.MaxValue);

        Task<Album> CreateAlbumAsync(string albumTitle, string userNickName, string folderPath, Dictionary<string, string> arguments = null);
        Task<Album> CreateAlbumAsync(string albumTitle, User user, string folderPath, Dictionary<string, string> arguments = null);
        Task<Album> CreateAlbumAsync(string albumTitle, Folder folder, Dictionary<string, string> arguments = null);

        Task<Album> UpdateAlbumAsync(Album album, Dictionary<string, string> arguments);

        Task DeleteAlbumAsync(Album album);

        Task<User> GetUserAsync(string userNickName);
        Task<User> GetAuthenticatedUserAsync();
        Task<User> GetSiteUserAsync();
        
        Task<UserProfile> GetUserProfileAsync(string userNickName);
        Task<UserProfile> GetUserProfileAsync(User user);

        Task<UserProfile> UpdateUserProfileAsync(UserProfile userProfile, Dictionary<string, string> updates);
        Task<UserProfile> UpdateUserProfileAsync(User user, Dictionary<string, string> updates);

        Task<Image> GetImageAsync(string imageKey);
        Task<Image> GetImageAsync(ImageUpload imageUpload);
        String GetImageKey(ImageUpload imageUpload);
        Task<ImageUpload> UploadImageAsync(string albumUri, string filePath);
        Task<ImageUpload> UploadImageAsync(Node node, string filePath);
        Task<ImageUpload> UploadImageAsync(Album album, string filePath);
        Task<Image> UpdateImageDataAsync(Image image, Dictionary<string, string> arguments);
        Task DeleteImageAsync(Image image);

        Task<AlbumImage> GetAlbumImageAsync(Album album, string imageKey);
        Task<AlbumImagesWithSizes> GetAlbumImagesWithSizesAsync(Album album, int maxAlbumImageCount = int.MaxValue);
        Task<List<AlbumImage>> GetAlbumImagesAsync(Album album, int maxAlbumImageCount = int.MaxValue);

        Task<Node> GetNodeAsync(string nodeId);
        Task<Node> GetRootNodeAsync(User user);
        Task<List<Node>> GetChildNodesAsync(Node node, int maxNodeCount = int.MaxValue);

        String GetDefaultNodeIDAsync(User user);

        Task<Node> CreateNodeAsync(NodeType type, string nodeName, string folderNodeId, Dictionary<string, string> arguments = null);

        Task<Node> UpdateNodeAsync(Node node, Dictionary<string, string> updates);

        Task DeleteNodeAsync(Node node);

        Task<Folder> GetFolderAsync(string userNickName, string folderPath);
        Task<Folder> GetFolderAsync(User user, string folderPath);

        Task<Folder> CreateFolderAsync(string folderName, string userNickName, string folderPath, Dictionary<string, string> arguments = null);
        Task<Folder> CreateFolderAsync(string folderName, User user, string folderPath, Dictionary<string, string> arguments = null);
        Task<Folder> CreateFolderAsync(string folderName, Folder folder, Dictionary<string, string> arguments = null);

        Task<Folder> UpdateFolderAsync(Folder folder, Dictionary<string, string> arguments);

        Task DeleteFolderAsync(Folder folder);   
    }
}
