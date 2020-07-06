using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using TinyOAuth1;

namespace SmugMug.NET
{
    public class SmugMugClient : ISmugMugClient
    {
        private OAuthCredentials _credentials;
        private ITinyOAuth _oauthClient;

        const string SMUGMUG_API_v2_BaseEndpoint = "https://api.smugmug.com";
        const string SMUGMUG_API_v2_ApiEndpoint = "https://api.smugmug.com/api/v2/";
        const string SMUGMUG_API_v2_UploadEndpoint = "https://upload.smugmug.com/";

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
            LoginType = loginType;
            _credentials = credentials;
            _oauthClient = new TinyOAuth(new TinyOAuthConfig()
            {
                ConsumerKey = credentials.ConsumerKey,
                ConsumerSecret = credentials.ConsumerSecret
            });
        }

        #region REST Requests
        private async Task<T> GetRequestAsync<T>(string endpoint)
        {
            return await GetRequestAsync<T>(SMUGMUG_API_v2_BaseEndpoint, endpoint).ConfigureAwait(false);
        }

        private IFlurlRequest CreateRequest(string baseAddress, string endpoint)
        {
            return CreateRequest(baseAddress, endpoint, HttpMethod.Get);
        }

        private IFlurlRequest CreateRequest(string baseAddress, string endpoint, HttpMethod method)
        {
            var result = new FlurlRequest(Url.Combine(baseAddress, endpoint))
                .WithHeader("Accept", "application/json");

            switch (LoginType)
            {
                case LoginType.Anonymous:
                    result.SetQueryParam("APIKey", _credentials.ConsumerKey);
                    break;
                case LoginType.OAuth:
                    var authHeader = _oauthClient.GetAuthorizationHeader(_credentials.AccessToken, _credentials.AccessTokenSecret, 
                        result.Url, method);
                    
                    result.WithHeader("Authorization", authHeader);
                    break;
                default:
                    throw new NotSupportedException(string.Format("LoginType {0} is unsupported", LoginType));
            }

            return result;
        }

        private async Task<T> GetRequestAsync<T>(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("GET {0}", request.Url));
            var result = await request.GetAsync().ReceiveJson<GetResponseStub<T>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));

            return result.Response;            
        }

        private async Task<Tuple<T, Dictionary<string,TE>>> GetRequestWithExpansionsAsync<T, TE>(string endpoint)
        {
            return await GetRequestWithExpansionsAsync<T, TE>(SMUGMUG_API_v2_BaseEndpoint, endpoint).ConfigureAwait(false);
        }

        private async Task<Tuple<T, Dictionary<string, TE>>> GetRequestWithExpansionsAsync<T, TE>(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint);

            Trace.WriteLine(string.Format("GET {0}", request.Url));
            var result = await request.GetAsync().ReceiveJson< GetResponseWithExpansionStub<T, TE>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));
            
            return new Tuple<T, Dictionary<string, TE>>(result.Response, result.Expansions);            
        }

        private async Task<T> PostRequestAsync<T>(string endpoint, string jsonContent)
        {
            return await PostRequestAsync<T>(SMUGMUG_API_v2_BaseEndpoint, endpoint, jsonContent).ConfigureAwait(false);
        }

        private async Task<T> PostRequestAsync<T>(string baseAddress, string endpoint, string jsonContent)
        {
            var request = CreateRequest(baseAddress, endpoint, HttpMethod.Post);

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

            var request = CreateRequest(SMUGMUG_API_v2_UploadEndpoint, null, HttpMethod.Post)
                .WithHeaders(new
                {
                    X_Smug_AlbumUri = albumUri,
                    X_Smug_FileName = fileName,
                    X_Smug_Response_Type = "JSON",
                    X_Smug_Version = "v2"
                });

            
            Trace.WriteLine(string.Format("POST {0}", request.Url));
            var content = new StreamContent(new MemoryStream(image));
            var result = await request.PostAsync(content, cancellationToken)
                .ReceiveJson<ImagePostResponse>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0} {1}: {2}", result.Stat, result.Method, result.Image));

            return result.Image;           
        }

        private async Task<T> PatchRequestAsync<T>(string endpoint, string jsonContent)
        {
            return await PatchRequestAsync<T>(SMUGMUG_API_v2_BaseEndpoint, endpoint, jsonContent).ConfigureAwait(false);
        }

        private async Task<T> PatchRequestAsync<T>(string baseAddress, string endpoint, string jsonContent)
        {
            var request = CreateRequest(baseAddress, endpoint, new HttpMethod("PATCH"));

            Trace.WriteLine(string.Format("PATCH {0}: {1}", request.Url, jsonContent));
            var result = await request.PatchAsync(new StringContent(jsonContent))
                .ReceiveJson<PostResponseStub<T>>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0} {1}: {2}", result.Code, result.Message, result.Response));

            return result.Response;            
        }

        private async Task DeleteRequestAsync(string endpoint)
        {
            await DeleteRequestAsync(SMUGMUG_API_v2_BaseEndpoint, endpoint).ConfigureAwait(false);
        }

        private async Task DeleteRequestAsync(string baseAddress, string endpoint)
        {
            var request = CreateRequest(baseAddress, endpoint, HttpMethod.Delete);

            Trace.WriteLine(string.Format("DELETE {0}", request.Url));
            var result = await request.DeleteAsync().ReceiveJson<DeleteResponseStub>().ConfigureAwait(false);
            Trace.WriteLine(string.Format("---{0}:{1}", result.Code, result.Message));            
        }
        #endregion

        #region Helpers
        private string GenerateNodeJson(string name, Dictionary<string, string> arguments = null)
        {
            var jsonContent = new StringBuilder();
            jsonContent.Append("{");

            //Remove all non alpha-numeric characters from the urlName
            char[] arr = name.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c))).ToArray();
            arr[0] = new string(arr).ToUpper()[0];
            string urlName = new string(arr);
            urlName = urlName.Replace(" ", "-");

            jsonContent.Append(string.Format("\"Name\":\"{0}\", \"UrlName\":\"{0}\"", name));

            //For each argument, append to Folder
            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    jsonContent.Append(string.Format(", \"{0}\":\"{1}\"", argument.Key, argument.Value));
                }
            }

            jsonContent.Append("}");

            return jsonContent.ToString();
        }

        private string GenerateJson(Dictionary<string, string> arguments)
        {
            JObject obj = JObject.FromObject(arguments);
            return obj.ToString();
        }
        #endregion

        #region User
        public async Task<User> GetUserAsync(string userNickName)
        {
            string endpoint = string.Format("user/{0}", userNickName);
            UserGetResponse response = await GetRequestAsync<UserGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.User;
        }

        public async Task<User> GetAuthenticatedUserAsync()
        {
            string endpoint = "/api/v2!authuser";
            UserGetResponse response = await GetRequestAsync<UserGetResponse>(endpoint).ConfigureAwait(false);
            return response.User;
        }

        public async Task<User> GetSiteUserAsync()
        {
            string endpoint = "/api/v2!siteuser";
            UserGetResponse response = await GetRequestAsync<UserGetResponse>(endpoint).ConfigureAwait(false);
            return response.User;
        }
        #endregion

        #region UserProfile
        public async Task<UserProfile> GetUserProfileAsync(string userNickName)
        {
            string endpoint = string.Format("user/{0}!profile", userNickName);
            UserProfileGetResponse response = await GetRequestAsync<UserProfileGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.UserProfile;
        }

        public async Task<UserProfile> GetUserProfileAsync(User user)
        {
            if (user != null)
            {
                string endpoint = user.Uris.UserProfile.Uri;
                UserProfileGetResponse response = await GetRequestAsync<UserProfileGetResponse>(endpoint).ConfigureAwait(false);
                return response.UserProfile;
            }
            else
            {
                throw new ArgumentNullException("user");
            }
        }

        private async Task<UserProfile> UpdateUserProfileAsync(string uri, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a user profile");

            if (updates != null && updates.Count > 0)
            {
                string content = GenerateJson(updates);
                string endpoint = uri;
                var response = await PatchRequestAsync<UserProfilePostResponse>(endpoint, content).ConfigureAwait(false);
                return response.UserProfile;
            }
            else
            {
                throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
            }
        }

        private void CheckForOAuth(string action)
        {
            if (LoginType != LoginType.OAuth)
                throw new UnauthorizedAccessException($"You must be logged in using OAuth to {action}.");
        }

        public async Task<UserProfile> UpdateUserProfileAsync(UserProfile userProfile, Dictionary<string, string> updates)
        {
            if (userProfile != null)
            {
                return await UpdateUserProfileAsync(userProfile.Uri, updates).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(string.Format("UserProfile {0} not found", userProfile), "userProfile");
            }
        }

        public async Task<UserProfile> UpdateUserProfileAsync(User user, Dictionary<string, string> updates)
        {
            if (user != null)
            {
                return await UpdateUserProfileAsync(user.Uris.UserProfile.Uri, updates).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(string.Format("User {0} not found", user), "user");
            }
        }
        #endregion

        #region Node
        public async Task<Node> GetNodeAsync(string nodeId)
        {
            string endpoint = string.Format("node/{0}", nodeId);
            NodeGetResponse response = await GetRequestAsync<NodeGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.Node;
        }

        public async Task<Node> GetRootNodeAsync(User user)
        {
            if (user != null)
            {
                string endpoint = user.Uris.Node.Uri;
                NodeGetResponse response = await GetRequestAsync<NodeGetResponse>(endpoint).ConfigureAwait(false);
                return response.Node;
            }
            else
            {
                throw new ArgumentNullException("user");
            }
        }

        public String GetDefaultNodeIDAsync(User user)
        {
            if (user != null)
            {
                string[] splitUri = user.Uris.Node.Uri.Split(new char[] { '/' });
                return splitUri[splitUri.Length - 1];
            }
            else
            {
                throw new ArgumentNullException("user");
            }
        }

        private async Task<List<Node>> GetPagedNodesAsync(string initialUri, int maxCount)
        {
            List<Node> results = new List<Node>();
            string nextPage = initialUri;
            NodePagesResponse nodePagesResponse;
            do
            {
                nodePagesResponse = await GetRequestAsync<NodePagesResponse>(nextPage).ConfigureAwait(false);
                results.AddRange(nodePagesResponse.Node);

                if (nodePagesResponse.Pages != null)
                {
                    nextPage = nodePagesResponse.Pages.NextPage;
                }
                else
                {
                    break;
                }
                //TODO: Update nextPage to Ensure we don't return more than maxCount
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
            if (parentNode != null)
            {
                if (nodeName.Length > 32)
                    throw new ArgumentException("Node names must be less than 32 characters long.", nodeName);

                if (arguments == null)
                {
                    arguments = new Dictionary<string, string>();
                }
                arguments.Add("Type", nodeType.ToString());

                string content = GenerateNodeJson(nodeName, arguments);
                string endpoint = parentNode.Uris.ChildNodes.Uri;
                var response = await PostRequestAsync<NodePostResponse>(endpoint, content).ConfigureAwait(false);
                return response.Node;
            }
            else
            {
                throw new ArgumentException(string.Format("Node {0} not found", folderNodeId), "folderNodeId");
            }
        }

        public async Task<Node> UpdateNodeAsync(Node node, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a node");

            if (node != null)
            {
                if (updates != null && updates.Count > 0)
                {
                    string content = GenerateJson(updates);
                    string endpoint = node.Uri;
                    var response = await PatchRequestAsync<NodePostResponse>(endpoint, content).ConfigureAwait(false);
                    return response.Node;
                }
                else
                {
                    throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Node {0} not found", node), "node");
            }
        }

        public async Task DeleteNodeAsync(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "You must provide a valid node to delete.");
            }

            await DeleteRequestAsync(node.Uri).ConfigureAwait(false);
        }

        #endregion

        #region Folder
        public async Task<Folder> GetFolderAsync(string userNickName, string folderPath)
        {
            StringBuilder sb = new StringBuilder("folder/user/");
            sb.Append(userNickName);
            if (!String.IsNullOrEmpty(folderPath))
            {
                sb.Append("/").Append(folderPath);
            }
            string endpoint = sb.ToString();
            FolderGetResponse response = await GetRequestAsync<FolderGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task<Folder> GetFolderAsync(User user, string folderPath)
        {
            if (user != null)
            {
                StringBuilder sb = new StringBuilder(user.Uris.Folder.Uri);
                if (!String.IsNullOrEmpty(folderPath))
                {
                    sb.Append("/").Append(folderPath);
                }
                string endpoint = sb.ToString();
                FolderGetResponse response = await GetRequestAsync<FolderGetResponse>(endpoint).ConfigureAwait(false);
                return response.Folder;
            }
            else
            {
                throw new ArgumentNullException(string.Format("User {0} not found", user), "user");
            }
        }

        public async Task<Folder> CreateFolderAsync(string folderName, string userNickName, string folderPath, Dictionary<string, string> arguments = null)
        {
            Folder parentFolder = await GetFolderAsync(userNickName, folderPath).ConfigureAwait(false);
            if (parentFolder != null)
            {
                return await CreateFolderAsync(folderName, parentFolder, arguments).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");
            }
        }

        public async Task<Folder> CreateFolderAsync(string folderName, User user, string folderPath, Dictionary<string, string> arguments = null)
        {
            Folder parentFolder = await GetFolderAsync(user, folderPath).ConfigureAwait(false);
            if (parentFolder != null)
            {
                return await CreateFolderAsync(folderName, parentFolder, arguments).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");
            }
        }

        public async Task<Folder> CreateFolderAsync(string folderName, Folder folder, Dictionary<string, string> arguments = null)
        {
            CheckForOAuth("create a folder");

            if (folderName.Length > 32)
                throw new ArgumentException("Folder names must be less than 32 characters long.", folderName);

            string content = GenerateNodeJson(folderName, arguments);
            string endpoint = folder.Uris.Folders.Uri;
            var response = await PostRequestAsync<FolderPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Folder;
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder, Dictionary<string, string> updates)
        {
            CheckForOAuth("update a folder");

            if (folder != null)
            {
                if (updates != null && updates.Count > 0)
                {
                    string content = GenerateJson(updates);
                    string endpoint = folder.Uri;
                    var response = await PatchRequestAsync<FolderPostResponse>(endpoint, content).ConfigureAwait(false);
                    return response.Folder;
                }
                else
                {
                    throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Folder {0} not found", folder), "folder");
            }
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder", "You must provide a valid folder to delete.");
            }

            await DeleteRequestAsync(folder.Uri).ConfigureAwait(false);
        }
        #endregion

        #region Album
        public async Task<Album> GetAlbumAsync(string albumId)
        {
            string endpoint = string.Format("album/{0}", albumId);
            AlbumGetResponse response = await GetRequestAsync<AlbumGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.Album;
        }

        private async Task<List<Album>> GetPagedAlbumsAsync(string initialUri, int maxCount)
        {
            List<Album> results = new List<Album>();
            string nextPage = initialUri;
            AlbumPagesResponse albumPagesResponse;
            do
            {
                albumPagesResponse = await GetRequestAsync<AlbumPagesResponse>(nextPage).ConfigureAwait(false);
                if (albumPagesResponse.Album != null)
                {
                    results.AddRange(albumPagesResponse.Album);

                    if (albumPagesResponse.Pages != null)
                    {
                        nextPage = albumPagesResponse.Pages.NextPage;
                    }
                    else
                    {
                        break;
                    }
                    //TODO: Update nextPage to Ensure we don't return more than maxCount
                }
                else
                    throw new NullReferenceException("The user has not specified anys albums.");

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
            var result = new AlbumImagesWithSizes();
            result.AlbumImages = new List<AlbumImage>();
            result.ImageSizes = new Dictionary<string, ImageSizesGetResponse>();
            string nextPage = initialUri;
            nextPage = string.Format("{0}{2}{1}", nextPage, "_expand=ImageSizes", nextPage.Contains('?') ? "&" : "?");
            //AlbumImagePagesResponse albumImagePagesResponse;
            //ImageSizesGetResponse albumImageSizesResponse;
            Tuple<AlbumImagePagesResponse, Dictionary<string, ImageSizesGetResponse>> response;
            do
            {
                response = await GetRequestWithExpansionsAsync<AlbumImagePagesResponse, ImageSizesGetResponse>(nextPage).ConfigureAwait(false);
                result.AlbumImages.AddRange(response.Item1.AlbumImage);
                result.ImageSizes = result.ImageSizes.Concat(response.Item2).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
                if (response.Item1.Pages != null)
                {
                    nextPage = response.Item1.Pages.NextPage;
                    if (!String.IsNullOrEmpty(nextPage))
                    {
                        nextPage = string.Format("{0}{2}{1}", nextPage, "_expand=ImageSizes", nextPage.Contains('?') ? "&" : "?"); 
                    }
                }
                else
                {
                    break;
                }
                //TODO: Update nextPage to Ensure we don't return more than maxCount
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
                albumImagePagesResponse = await GetRequestAsync<AlbumImagePagesResponse>(nextPage).ConfigureAwait(false);
                results.AddRange(albumImagePagesResponse.AlbumImage);

                if (albumImagePagesResponse.Pages != null)
                {
                    nextPage = albumImagePagesResponse.Pages.NextPage;
                }
                else
                {
                    break;
                }
                //TODO: Update nextPage to Ensure we don't return more than maxCount
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
            if (parentFolder != null)
            {
                return await CreateAlbumAsync(albumTitle, parentFolder, arguments).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(string.Format("Folder {0} not found", folderPath), "folderPath");
            }
        }

        public async Task<Album> CreateAlbumAsync(string albumTitle, Folder folder, Dictionary<string, string> arguments = null)
        {
            CheckForOAuth("create an album");

            if (albumTitle.Length > 32)
                throw new ArgumentException("Album titles must be less than 32 characters long.", albumTitle);

            string content = GenerateNodeJson(albumTitle, arguments);
            string endpoint = folder.Uris.FolderAlbums.Uri;
            var response = await PostRequestAsync<AlbumPostResponse>(endpoint, content).ConfigureAwait(false);
            return response.Album;
        }

        public async Task<Album> UpdateAlbumAsync(Album album, Dictionary<string, string> updates)
        {
            CheckForOAuth("update an album");

            if (album != null)
            {
                if (updates != null && updates.Count > 0)
                {
                    string content = GenerateJson(updates);
                    string endpoint = album.Uri;
                    var response = await PatchRequestAsync<AlbumPostResponse>(endpoint, content).ConfigureAwait(false);
                    return response.Album;
                }
                else
                {
                    throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Album {0} not found", album), "album");
            }
        }

        public async Task DeleteAlbumAsync(Album album)
        {
            if (album == null)
            {
                throw new ArgumentNullException("album", "You must provide a valid album to delete.");
            }

            await DeleteRequestAsync(album.Uri).ConfigureAwait(false);
        }

        #endregion

        #region Image
        public async Task<Image> GetImageAsync(string imageKey)
        {
            string endpoint = string.Format("image/{0}", imageKey);
            ImageGetResponse response = await GetRequestAsync<ImageGetResponse>(SMUGMUG_API_v2_ApiEndpoint, endpoint).ConfigureAwait(false);
            return response.Image;
        }

        public async Task<Image> GetImageAsync(ImageUpload imageUpload)
        {
            var imageKey = GetImageKey(imageUpload);

            return await GetImageAsync(imageKey).ConfigureAwait(false);
        }

        public async Task<AlbumImage> GetAlbumImageAsync(Album album, string imageKey)
        {
            if (album != null)
            {
                string endpoint = string.Format(album.Uri + "/image/{0}", imageKey);
                AlbumImageResponse response = await GetRequestAsync<AlbumImageResponse>(endpoint).ConfigureAwait(false);
                return response.AlbumImage;
            }
            else
            {
                throw new ArgumentNullException("album");
            }
        }

        public String GetImageKey(ImageUpload imageUpload)
        {
            if (imageUpload != null)
            {
                string[] splitUri = imageUpload.ImageUri.Split(new char[] { '/' });
                return splitUri[splitUri.Length - 1];
            }
            else
            {
                throw new ArgumentNullException("imageUpload");
            }
        }

        public async Task<ImageUpload> UploadImageAsync(string albumUri, string filePath)
        {
            CheckForOAuth("upload an image");

            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                byte[] fileContents = File.ReadAllBytes(fi.FullName);

                ImageUpload response = await UploadImageAsync(albumUri, fi.Name, fileContents, CancellationToken.None).ConfigureAwait(false);
                return response;
            }
            else
            {
                throw new FileNotFoundException("Cannot find the file to upload", filePath);
            }
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

            if (image != null)
            {
                if (updates != null && updates.Count > 0)
                {
                    string content = GenerateJson(updates);
                    string endpoint = image.Uri;
                    var response = await PatchRequestAsync<ImagePatchResponse>(endpoint, content).ConfigureAwait(false);
                    return response.Image;
                }
                else
                {
                    throw new ArgumentException(string.Format("Updates can not be null or empty", updates), "updates");
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Image {0} not found", image), "image");
            }
        }

        public async Task DeleteImageAsync(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image", "You must provide a valid image to delete.");
            }

            await DeleteRequestAsync(image.Uri).ConfigureAwait(false);
        }
        #endregion        
    }
}
 