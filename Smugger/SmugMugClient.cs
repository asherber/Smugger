using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smugger.Flurl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Smugger
{
    public class SmugMugClient : ISmugMugClient
    {        
        private SmugMugAuthorizer _authorizer;

        const string BASE_ENDPOINT = "https://api.smugmug.com";
        const string UPLOAD_ENDPOINT = "https://upload.smugmug.com/";
        const string API_SEGMENT = "/api/v2";

        public LoginType LoginType { get; private set; }

        public SmugMugClient(string apiKey) : this(LoginType.Anonymous, new OAuthCredentials() { ConsumerKey = apiKey })
        {
        }

        public SmugMugClient(OAuthCredentials credentials) : this(LoginType.OAuth, credentials)
        {
        }

        public SmugMugClient(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret) 
            : this(LoginType.OAuth, new OAuthCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret))
        {
        }

        private SmugMugClient(LoginType loginType, OAuthCredentials credentials)
        {
            credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));

            void CheckForNull(string input, string name)
            {
                if (String.IsNullOrEmpty(input))
                    throw new ArgumentNullException(name);
            }

            if (loginType == LoginType.Anonymous)
                CheckForNull(credentials.ConsumerKey, "apiKey");
            else
            {
                CheckForNull(credentials.ConsumerKey, "consumerKey");
                CheckForNull(credentials.ConsumerSecret, "consumerSecret");
                CheckForNull(credentials.AccessToken, "accessToken");
                CheckForNull(credentials.AccessTokenSecret, "accessTokenSecret");
            }

            LoginType = loginType;
            _authorizer = new SmugMugAuthorizer(loginType, credentials);

            var factory = new SmugMugHttpClientFactory(_authorizer);
            factory.ConfigureFlurlClient(BASE_ENDPOINT);
            factory.ConfigureFlurlClient(UPLOAD_ENDPOINT);
        }        

        #region REST Requests
        private IFlurlRequest CreateRequest(string baseAddress, string endpoint)
        {
            if (baseAddress == BASE_ENDPOINT)
            {
                if (!endpoint.StartsWith(API_SEGMENT))
                    endpoint = API_SEGMENT + endpoint;
            }

            return new FlurlRequest(Url.Combine(baseAddress, endpoint));
        }        

        private async Task<T> GetAsync<T>(string endpoint)
        {
            return await GetAsync<T>(BASE_ENDPOINT, endpoint).ConfigureAwait(false);
        }

        private async Task<T> GetAsync<T>(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("GET {0}", request.Url));
            var result = await request.GetAsync().ReceiveJson<GetResponseStub<T>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));

            return result.Response;            
        }

        private async Task<Tuple<T, Dictionary<string,TE>>> GetWithExpansionsAsync<T, TE>(string endpoint)
        {
            return await GetWithExpansionsAsync<T, TE>(BASE_ENDPOINT, endpoint).ConfigureAwait(false);
        }

        private async Task<Tuple<T, Dictionary<string, TE>>> GetWithExpansionsAsync<T, TE>(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("GET {0}", request.Url));
            var result = await request.GetAsync().ReceiveJson< GetResponseWithExpansionStub<T, TE>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));
            
            return new Tuple<T, Dictionary<string, TE>>(result.Response, result.Expansions);            
        }

        private async Task<T> PostAsync<T>(string endpoint, string jsonContent)
        {
            return await PostAsync<T>(BASE_ENDPOINT, endpoint, jsonContent).ConfigureAwait(false);
        }

        private async Task<T> PostAsync<T>(string baseAddress, string endpoint, string jsonContent)
        {
            var request = CreateRequest(baseAddress, endpoint);

            try
            {
                Trace.WriteLine(string.Format("POST {0}: {1}", request.Url, jsonContent));
                var result = await request.PostAsync(new StringContent(jsonContent))
                    .ReceiveJson<PostResponseStub<T>>().ConfigureAwait(false);
                Trace.WriteLine(string.Format("---{0} {1}: {2}", result.Code, result.Message, result.Response));
                return result.Response;
            }
            catch (FlurlHttpException ex)
            {
                var failedResponse = await ex.GetResponseStringAsync().ConfigureAwait(false);
                JObject response = JObject.Parse(failedResponse);
                var invalidParameters =
                    (from p in response["Options"]["Parameters"]["POST"]
                    where p["Problems"] != null
                    select new POSTParameter
                    {
                        ParameterName = (string)p["Name"],
                        Problem = (string)p["Problems"].First()
                    })
                    .ToList();

                if (invalidParameters.Any())
                {
                    var argumentExceptions = invalidParameters.Select(p => new ArgumentException(p.Problem, p.ParameterName));
                    throw new AggregateException("HTTP POST Request failed. See inner exceptions for individual reasons.", 
                        argumentExceptions);
                }
                else
                    throw new HttpRequestException("HTTP POST Request failed for unknown reasons");
            }                  
        }

        private async Task<ImageUpload> UploadImageAsync(string albumUri, string fileName, byte[] image, CancellationToken cancellationToken)
        {
            if (LoginType != LoginType.OAuth)
                throw new NotSupportedException(string.Format("LoginType {0} is unsupported", LoginType));

            var request = CreateRequest(UPLOAD_ENDPOINT, null)
                .WithHeaders(new
                {
                    X_Smug_AlbumUri = albumUri,
                    X_Smug_FileName = fileName,
                    X_Smug_ResponseType = "JSON",
                    X_Smug_Version = "v2"
                });

            
            Trace.WriteLine(string.Format("POST {0}", request.Url));
            var content = new StreamContent(new MemoryStream(image));
            var result = await request.PostAsync(content, cancellationToken)
                .ReceiveJson<ImagePostResponse>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0} {1}: {2}", result.Stat, result.Method, result.Image));

            return result.Image;           
        }

        private async Task<T> PatchAsync<T>(string endpoint, string jsonContent)
        {
            return await PatchAsync<T>(BASE_ENDPOINT, endpoint, jsonContent).ConfigureAwait(false);
        }

        private async Task<T> PatchAsync<T>(string baseAddress, string endpoint, string jsonContent)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("PATCH {0}: {1}", request.Url, jsonContent));
            var result = await request.PatchAsync(new StringContent(jsonContent))
                .ReceiveJson<PostResponseStub<T>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0} {1}: {2}", result.Code, result.Message, result.Response));

            return result.Response;            
        }

        private async Task DeleteAsync(string endpoint)
        {
            await DeleteAsync(BASE_ENDPOINT, endpoint).ConfigureAwait(false);
        }

        private async Task DeleteAsync(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("DELETE {0}", request.Url));
            var result = await request.DeleteAsync().ReceiveJson<DeleteResponseStub>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));            
        }
        #endregion

        #region Helpers
        private string GenerateNodeJson(string name, Dictionary<string, string> arguments = null)
        {
            name = Regex.Replace(name, @"[^a-zA-Z0-9 ]", "");
            var urlName = name.Replace(' ', '-');

            var result = new JObject
            {
                { "Name", name },
                { "UrlName", urlName }
            };

            result.Add(JObject.FromObject(arguments).Children());
            
            return result.ToString(Formatting.None);
        }

        private string GenerateJson(Dictionary<string, string> arguments)
        {
            return JObject.FromObject(arguments).ToString();
        }
        #endregion

        #region User
        public async Task<User> GetUserAsync(string userNickName)
        {
            string endpoint = string.Format("/user/{0}", userNickName);
            UserGetResponse response = await GetAsync<UserGetResponse>(endpoint).ConfigureAwait(false);
            return response.User;
        }

        public async Task<User> GetAuthenticatedUserAsync()
        {
            string endpoint = "!authuser";
            UserGetResponse response = await GetAsync<UserGetResponse>(endpoint).ConfigureAwait(false);
            return response.User;
        }

        public async Task<User> GetSiteUserAsync()
        {
            string endpoint = "!siteuser";
            UserGetResponse response = await GetAsync<UserGetResponse>(endpoint).ConfigureAwait(false);
            return response.User;
        }
        #endregion

        #region UserProfile
        public async Task<UserProfile> GetUserProfileAsync(string userNickName)
        {
            string endpoint = string.Format("/user/{0}!profile", userNickName);
            UserProfileGetResponse response = await GetAsync<UserProfileGetResponse>(endpoint).ConfigureAwait(false);
            return response.UserProfile;
        }

        public async Task<UserProfile> GetUserProfileAsync(User user)
        {
            user = user ?? throw new ArgumentNullException("user");
            
            string endpoint = user.Uris.UserProfile.Uri;
            UserProfileGetResponse response = await GetAsync<UserProfileGetResponse>(endpoint).ConfigureAwait(false);
            return response.UserProfile;           
        }

        private async Task<UserProfile> UpdateUserProfileAsync(string uri, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a user profile");

            if (updates?.Any() != true)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            
            string content = GenerateJson(updates);
            string endpoint = uri;
            var response = await PatchAsync<UserProfilePostResponse>(endpoint, content).ConfigureAwait(false);
            return response.UserProfile;
        }

        private void CheckForOAuth(string action)
        {
            if (LoginType != LoginType.OAuth)
                throw new UnauthorizedAccessException($"You must be logged in using OAuth to {action}.");
        }

        public async Task<UserProfile> UpdateUserProfileAsync(UserProfile userProfile, Dictionary<string, string> updates)
        {
            if (userProfile == null)
                throw new ArgumentException(string.Format("UserProfile {0} not found", userProfile), "userProfile");
            
            return await UpdateUserProfileAsync(userProfile.Uri, updates).ConfigureAwait(false);
        }

        public async Task<UserProfile> UpdateUserProfileAsync(User user, Dictionary<string, string> updates)
        {
            if (user == null)
                throw new ArgumentException(string.Format("User {0} not found", user), "user");
            
            return await UpdateUserProfileAsync(user.Uris.UserProfile.Uri, updates).ConfigureAwait(false);
        }
        #endregion

        #region Node
        public async Task<Node> GetNodeAsync(string nodeId)
        {
            string endpoint = string.Format("/node/{0}", nodeId);
            NodeGetResponse response = await GetAsync<NodeGetResponse>(endpoint).ConfigureAwait(false);
            return response.Node;
        }

        public async Task<Node> GetRootNodeAsync(User user)
        {
            user = user ?? throw new ArgumentNullException("user");
            
            string endpoint = user.Uris.Node.Uri;
            NodeGetResponse response = await GetAsync<NodeGetResponse>(endpoint).ConfigureAwait(false);
            return response.Node;            
        }

        public String GetDefaultNodeIDAsync(User user)
        {
            user = user ?? throw new ArgumentNullException("user");
            
            string[] splitUri = user.Uris.Node.Uri.Split(new char[] { '/' });
            return splitUri[splitUri.Length - 1];
        }

        private async Task<List<Node>> GetPagedNodesAsync(string initialUri, int maxCount)
        {
            List<Node> results = new List<Node>();
            string nextPage = initialUri;
            NodePagesResponse nodePagesResponse;
            do
            {
                nodePagesResponse = await GetAsync<NodePagesResponse>(nextPage).ConfigureAwait(false);
                results.AddRange(nodePagesResponse.Node.Take(maxCount - results.Count));
                nextPage = nodePagesResponse.Pages?.NextPage;
            }
            while (!String.IsNullOrEmpty(nextPage) && (results.Count < maxCount));

            return results;
        }

        public async Task<List<Node>> GetChildNodesAsync(Node node, int maxNodeCount = int.MaxValue)
        {
            if (node.HasChildren)
                return await GetPagedNodesAsync(node.Uris.ChildNodes.Uri, maxNodeCount).ConfigureAwait(false);
            else
                return null;
        }

        public async Task<Node> CreateNodeAsync(NodeType nodeType, string nodeName, string folderNodeId, Dictionary<string, string> arguments = null)
        {
            CheckForOAuth("create a node");

            Node parentNode = await GetNodeAsync(folderNodeId).ConfigureAwait(false);
            if (parentNode == null)
                throw new ArgumentException(string.Format("Node {0} not found", folderNodeId), "folderNodeId");

            if (nodeName.Length > 32)
                throw new ArgumentException("Node names must be less than 32 characters long.", nodeName);

            arguments = arguments ?? new Dictionary<string, string>();            
            arguments.Add("Type", nodeType.ToString());

            string content = GenerateNodeJson(nodeName, arguments);
            string endpoint = parentNode.Uris.ChildNodes.Uri;
            var response = await PostAsync<NodePostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Node;
        }

        public async Task<Node> UpdateNodeAsync(Node node, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a node");
            if (node == null)
                throw new ArgumentException(string.Format("Node {0} not found", node), "node");
            if (updates?.Any() != true)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            
            string content = GenerateJson(updates);
            string endpoint = node.Uri;
            var response = await PatchAsync<NodePostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Node;                     
        }

        public async Task DeleteNodeAsync(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "You must provide a valid node to delete.");             

            await DeleteAsync(node.Uri).ConfigureAwait(false);
        }

        #endregion

        #region Folder
        public async Task<Folder> GetFolderAsync(string userNickName, string folderPath)
        {
            folderPath = folderPath ?? String.Empty;
            var endpoint = "/folder/user".AppendPathSegments(userNickName, folderPath);
            
            FolderGetResponse response = await GetAsync<FolderGetResponse>(endpoint).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task<Folder> GetFolderAsync(User user, string folderPath)
        {
            if (user == null)
                throw new ArgumentNullException(string.Format("User {0} not found", user), "user");

            folderPath = folderPath ?? String.Empty;
            var endpoint = user.Uris.Folder.Uri.AppendPathSegment(folderPath);

            FolderGetResponse response = await GetAsync<FolderGetResponse>(endpoint).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task<Folder> CreateFolderAsync(string folderName, string userNickName, string folderPath, Dictionary<string, string> arguments = null)
        {
            Folder parentFolder = await GetFolderAsync(userNickName, folderPath).ConfigureAwait(false);
            if (parentFolder == null)
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");

            return await CreateFolderAsync(folderName, parentFolder, arguments).ConfigureAwait(false);            
        }

        public async Task<Folder> CreateFolderAsync(string folderName, User user, string folderPath, Dictionary<string, string> arguments = null)
        {
            Folder parentFolder = await GetFolderAsync(user, folderPath).ConfigureAwait(false);
            if (parentFolder == null)
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");
            
            return await CreateFolderAsync(folderName, parentFolder, arguments).ConfigureAwait(false);
        }

        public async Task<Folder> CreateFolderAsync(string folderName, Folder folder, Dictionary<string, string> arguments = null)
        {
            CheckForOAuth("create a folder");

            if (folderName.Length > 32)
                throw new ArgumentException("Folder names must be less than 32 characters long.", folderName);

            string content = GenerateNodeJson(folderName, arguments);
            string endpoint = folder.Uris.Folders.Uri;
            var response = await PostAsync<FolderPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a folder");
            if (folder == null)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            if (updates?.Any() != true)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");

            
            string content = GenerateJson(updates);
            string endpoint = folder.Uri;
            var response = await PatchAsync<FolderPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder", "You must provide a valid folder to delete.");
            
            await DeleteAsync(folder.Uri).ConfigureAwait(false);
        }
        #endregion

        #region Album
        public async Task<Album> GetAlbumAsync(string albumKey)
        {
            string endpoint = string.Format("/album/{0}", albumKey);
            AlbumGetResponse response = await GetAsync<AlbumGetResponse>(endpoint).ConfigureAwait(false);
            return response.Album;
        }

        private async Task<List<Album>> GetPagedAlbumsAsync(string initialUri, int maxCount)
        {
            List<Album> results = new List<Album>();
            string nextPage = initialUri;
            AlbumPagesResponse albumPagesResponse;
            do
            {
                albumPagesResponse = await GetAsync<AlbumPagesResponse>(nextPage).ConfigureAwait(false);
                if (albumPagesResponse.Album != null)
                {
                    results.AddRange(albumPagesResponse.Album.Take(maxCount - results.Count));
                    nextPage = albumPagesResponse.Pages?.NextPage;
                }
                else
                    throw new NullReferenceException("The user has not specified any albums.");
            }
            while (!String.IsNullOrEmpty(nextPage) && (results.Count < maxCount));

            return results;
        }

        public async Task<List<Album>> GetAlbumsAsync(User user, int maxAlbumCount = int.MaxValue)
        {
            return await GetPagedAlbumsAsync(user.Uris.UserAlbums.Uri, maxAlbumCount).ConfigureAwait(false);
        }

        public async Task<List<Album>> GetFeaturedAlbumsAsync(User user, int maxAlbumCount = int.MaxValue)
        {
            return await GetPagedAlbumsAsync(user.Uris.UserFeaturedAlbums.Uri, maxAlbumCount).ConfigureAwait(false);
        }

        private async Task<AlbumImagesWithSizes> GetPagedAlbumImagesWithSizesAsync(string initialUri, int maxCount)
        {
            var result = new AlbumImagesWithSizes()
            {
                AlbumImages = new List<AlbumImage>(),
                ImageSizes = new Dictionary<string, ImageSizesGetResponse>()
            };

            string nextPage = initialUri;

            Tuple<AlbumImagePagesResponse, Dictionary<string, ImageSizesGetResponse>> response;
            do
            {
                nextPage = nextPage.SetQueryParam("_expand", "ImageSizes");

                response = await GetWithExpansionsAsync<AlbumImagePagesResponse, ImageSizesGetResponse>(nextPage).ConfigureAwait(false);
                result.AlbumImages.AddRange(response.Item1.AlbumImage.Take(maxCount - result.AlbumImages.Count));
                result.ImageSizes = result.ImageSizes.Concat(response.Item2)
                    .GroupBy(d => d.Key)
                    .ToDictionary(d => d.Key, d => d.First().Value);

                nextPage = response.Item1.Pages?.NextPage;
            }
            while (!String.IsNullOrEmpty(nextPage) && (result.AlbumImages.Count < maxCount));

            return result;
        }

        public async Task<AlbumImagesWithSizes> GetAlbumImagesWithSizesAsync(Album album, int maxAlbumImageCount = int.MaxValue)
        {
            return await GetPagedAlbumImagesWithSizesAsync(album.Uris.AlbumImages.Uri, maxAlbumImageCount).ConfigureAwait(false);
        }

        private async Task<List<AlbumImage>> GetPagedAlbumImagesAsync(string initialUri, int maxCount)
        {
            List<AlbumImage> results = new List<AlbumImage>();
            string nextPage = initialUri;
            AlbumImagePagesResponse albumImagePagesResponse;
            do
            {
                albumImagePagesResponse = await GetAsync<AlbumImagePagesResponse>(nextPage).ConfigureAwait(false);
                results.AddRange(albumImagePagesResponse.AlbumImage.Take(maxCount - results.Count));
                nextPage = albumImagePagesResponse.Pages?.NextPage;
            }
            while (!String.IsNullOrEmpty(nextPage) && (results.Count < maxCount));

            return results;
        }

        public async Task<List<AlbumImage>> GetAlbumImagesAsync(Album album, int maxAlbumImageCount = int.MaxValue)
        {
            return await GetPagedAlbumImagesAsync(album.Uris.AlbumImages.Uri, maxAlbumImageCount).ConfigureAwait(false);
        }

        public async Task<Album> CreateAlbumAsync(string albumTitle, string userNickName, string folderPath, Dictionary<string, string> arguments = null)
        {
            User user = await GetUserAsync(userNickName).ConfigureAwait(false);
            return await CreateAlbumAsync(albumTitle, user, folderPath, arguments).ConfigureAwait(false);
        }

        public async Task<Album> CreateAlbumAsync(string albumTitle, User user, string folderPath, Dictionary<string, string> arguments = null)
        {
            Folder parentFolder = await GetFolderAsync(user, folderPath).ConfigureAwait(false);
            if (parentFolder == null)
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");
            
            return await CreateAlbumAsync(albumTitle, parentFolder, arguments).ConfigureAwait(false);            
        }

        public async Task<Album> CreateAlbumAsync(string albumTitle, Folder folder, Dictionary<string, string> arguments = null)
        {
            CheckForOAuth("create an album");

            if (albumTitle.Length > 32)
                throw new ArgumentException("Album titles must be less than 32 characters long.", albumTitle);

            string content = GenerateNodeJson(albumTitle, arguments);
            string endpoint = folder.Uris.FolderAlbums.Uri;
            var response = await PostAsync<AlbumPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Album;
        }

        public async Task<Album> UpdateAlbumAsync(Album album, Dictionary<string, string> updates)
        {
            CheckForOAuth("update an album");
            if (album == null)
                throw new ArgumentException(string.Format("Album {0} not found", album), "album");
            if (updates?.Any() != true)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            
            string content = GenerateJson(updates);
            string endpoint = album.Uri;
            var response = await PatchAsync<AlbumPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Album;                
        }

        public async Task DeleteAlbumAsync(Album album)
        {
            if (album == null)            
                throw new ArgumentNullException("album", "You must provide a valid album to delete.");            

            await DeleteAsync(album.Uri).ConfigureAwait(false);
        }

        #endregion

        #region Image
        public async Task<Image> GetImageAsync(string imageKey)
        {
            string endpoint = string.Format("/image/{0}", imageKey);
            ImageGetResponse response = await GetAsync<ImageGetResponse>(endpoint).ConfigureAwait(false);
            return response.Image;
        }

        public async Task<Image> GetImageAsync(ImageUpload imageUpload)
        {
            var imageKey = GetImageKey(imageUpload);

            return await GetImageAsync(imageKey).ConfigureAwait(false);
        }

        public async Task<AlbumImage> GetAlbumImageAsync(Album album, string imageKey)
        {
            album = album ?? throw new ArgumentNullException("album");

            string endpoint = string.Format(album.Uri + "/image/{0}", imageKey);
            AlbumImageResponse response = await GetAsync<AlbumImageResponse>(endpoint).ConfigureAwait(false);
            return response.AlbumImage;
        }

        public String GetImageKey(ImageUpload imageUpload)
        {
            imageUpload = imageUpload ?? throw new ArgumentNullException("imageUpload");
            
            string[] splitUri = imageUpload.ImageUri.Split(new char[] { '/' });
            return splitUri.Last();          
        }

        public async Task<ImageUpload> UploadImageAsync(string albumUri, string filePath)
        {
            CheckForOAuth("upload an image");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Cannot find the file to upload", filePath);

            
            FileInfo fi = new FileInfo(filePath);
            byte[] fileContents = File.ReadAllBytes(fi.FullName);

            ImageUpload response = await UploadImageAsync(albumUri, fi.Name, fileContents, CancellationToken.None).ConfigureAwait(false);
            return response;
        }

        public async Task<ImageUpload> UploadImageAsync(Node node, string filePath)
        {
            if (node.Type != NodeType.Album)
                throw new ArgumentException("Images can only be uploaded to album nodes.");

            return await UploadImageAsync(node.Uris.Album.Uri, filePath).ConfigureAwait(false);
        }

        public async Task<ImageUpload> UploadImageAsync(Album album, string filePath)
        {
            return await UploadImageAsync(album.Uri, filePath).ConfigureAwait(false);
        }

        public async Task<Image> UpdateImageAsync(Image image, Dictionary<string, string> updates)
        {
            CheckForOAuth("update an image");
            if (image == null)
                throw new ArgumentException(string.Format("Image {0} not found", image), "image");
            if (updates?.Any() != true)
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            
            string content = GenerateJson(updates);
            string endpoint = image.Uri;
            var response = await PatchAsync<ImagePatchResponse>(endpoint, content).ConfigureAwait(false);
            return response.Image;
        }

        public async Task DeleteImageAsync(Image image)
        {
            if (image == null)
                throw new ArgumentNullException("image", "You must provide a valid image to delete.");
            
            await DeleteAsync(image.Uri).ConfigureAwait(false);
        }
        #endregion

        #region Raw
        public async Task<JObject> GetJsonAsync(string endpoint)
        {
            return await GetJsonAsync(BASE_ENDPOINT, endpoint).ConfigureAwait(false);
        }

        public async Task<JObject> GetJsonAsync(string baseAddress, string endpoint)
        {
            return await GetAsync<JObject>(baseAddress, endpoint);
        }
        #endregion
    }
}
 