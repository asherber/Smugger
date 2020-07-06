using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SmugMug.NET
{
    public class Album : SmugMugObject
    {
        public string AlbumKey { get; set; }
        public bool AllowDownloads { get; set; }
        public string Backprinting { get; set; }
        public string BoutiquePackaging { get; set; }
        public bool CanRank { get; set; }
        public bool CanShare { get; set; }
        public bool Clean { get; set; }
        public bool Comments { get; set; }
        public string CommunityUri { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string DownloadPassword { get; set; }
        public bool EXIF { get; set; }
        public bool FamilyEdit { get; set; }
        public bool Filenames { get; set; }
        public bool FriendEdit { get; set; }
        public bool Geography { get; set; }
        public bool HasDownloadPassword { get; set; }
        public Header Header { get; set; }
        public bool HideOwner { get; set; }
        public string HighlightImageUri { get; set; }
        public int ImageCount { get; set; }
        public DateTime ImagesLastUpdated { get; set; }
        public string InterceptShipping { get; set; }
        public string Keywords { get; set; }
        public Size LargestSize { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Name { get; set; }
        public string NodeID { get; set; }
        public long OriginalSizes { get; set; }
        public bool PackagingBrand { get; set; }
        public string Password { get; set; }
        public string PasswordHint { get; set; }
        public bool Printable { get; set; }
        public string PrintmarkUri { get; set; }
        public PrivacyType Privacy { get; set; }
        public int ProofDays { get; set; }
        public bool Protected { get; set; }
        public SecurityType SecurityType { get; set; }
        public bool Share { get; set; }
        public string SmugSearchable { get; set; }
        public SortDirection SortDirection { get; set; }
        public AlbumSortMethod SortMethod { get; set; }
        public string TemplateUri { get; set; }
        public string ThemeUri { get; set; }
        public long TotalSizes { get; set; }
        public string UploadKey { get; set; }
        public string UrlName { get; set; }
        public string UrlPath { get; set; }
        public bool Watermark { get; set; }
        public string WatermarkUri { get; set; }
        public Uri WebUri { get; set; }
        public bool WorldSearchable { get; set; }
        public AlbumUris Uris { get; set; }

        public override string ToString()
        {
            return string.Format("Album: {0}, {1}", Name, JsonConvert.SerializeObject(this));
        }
    }

    public class AlbumUris
    {
        public SmugMugUri AlbumShareUris { get; set; }
        public SmugMugUri Node { get; set; }
        public SmugMugUri User { get; set; }
        public SmugMugUri Folder { get; set; }
        public SmugMugUri ParentFolders { get; set; }
        public SmugMugUri HighlightImage { get; set; }
        public SmugMugUri AlbumHighlightImage { get; set; }
        public SmugMugUri AlbumImages { get; set; }
        public SmugMugUri AlbumPopularMedia { get; set; }
        public SmugMugUri AlbumGeoMedia { get; set; }
        public SmugMugUri AlbumComments { get; set; }
        public SmugMugUri AlbumDownload { get; set; }
        public SmugMugUri AlbumPrices { get; set; }
    }

    public class AlbumGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public Album Album { get; set; }
    }

    public class AlbumPostResponse : SmugMugUri
    {
        public Album Album { get; set; }

        public override string ToString()
        {
            return Album.ToString();
        }
    }

    public class AlbumPagesResponse : SmugMugPagesObject
    {
        public IEnumerable<Album> Album { get; set; }
    }
}
