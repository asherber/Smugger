using Newtonsoft.Json;
using System;

namespace SmugMug.NET
{
    public class User : SmugMugObject
    {
        public AccountStatus AccountStatus { get; set; }
        public string Domain { get; set; }
        public string DomainOnly { get; set; }
        public string FirstName { get; set; }
        public bool FriendsView { get; set; }
        public int ImageCount { get; set; }
        public bool IsTrial { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Plan { get; set; }
        public bool QuickShare { get; set; }
        public string RefTag { get; set; }
        public UserSortMethod SortBy { get; set; }
        public string TotalAccountSize { get; set; }
        public string TotalUploadedSize { get; set; }
        public string ViewPassHint { get; set; }
        public string ViewPassword { get; set; }
        public Uri WebUri { get; set; }
        public UserUris Uris { get; set; }

        public override string ToString()
        {
            return string.Format("User: {0}, {1}", Name, JsonConvert.SerializeObject(this));
        }
    }

    public class UserUris
    {
        public SmugMugUri BioImage { get; set; }
        public SmugMugUri CoverImage { get; set; }
        public SmugMugUri DuplicateImageSearch { get; set; }
        public SmugMugUri Features { get; set; }
        public SmugMugUri Folder { get; set; }
        public SmugMugUri Node { get; set; }
        public SmugMugUri SortUserFeaturedAlbums { get; set; }
        public SmugMugUri UnlockUser { get; set; }
        public SmugMugUri UrlPathLookup { get; set; }
        public SmugMugUri UserAlbumTemplates { get; set; }
        public SmugMugUri UserAlbums { get; set; }
        public SmugMugUri UserContacts { get; set; }
        public SmugMugUri UserDeletedAlbums { get; set; }
        public SmugMugUri UserDeletedFolders { get; set; }
        public SmugMugUri UserDeletedPages { get; set; }
        public SmugMugUri UserFeaturedAlbums { get; set; }
        public SmugMugUri UserGeoMedia { get; set; }
        public SmugMugUri UserGrants { get; set; }
        public SmugMugUri UserGuideSates { get; set; }
        public SmugMugUri UserHideGuides { get; set; }
        public SmugMugUri UserImageSearch { get; set; }
        public SmugMugUri UserLatestQuickNews { get; set; }
        public SmugMugUri UserPopularMedia { get; set; }
        public SmugMugUri UserProfile { get; set; }
        public SmugMugUri UserRecentImages { get; set; }
        public SmugMugUri UserTasks { get; set; }
        public SmugMugUri UserTopKeywords { get; set; }
        public SmugMugUri UserUploadLimits { get; set; }
        public SmugMugUri UserWatermarks { get; set; }
    }

    public class UserGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public User User { get; set; }
    }

    public class UserPostResponse : SmugMugUri
    {
        public User User { get; set; }

        public override string ToString()
        {
            return User.ToString();
        }
    }
}