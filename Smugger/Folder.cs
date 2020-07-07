using Newtonsoft.Json;
using System;

namespace Smugger
{
    public class Folder : SmugMugObject
    {
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public string Description { get; set; }
        public string HighlightImageUri { get; set; }
        public bool IsEmpty { get; set; }
        public string Keywords { get; set; }
        public string Name { get; set; }
        public string NodeID { get; set; }
        public string Password { get; set; }
        public string PasswordHint { get; set; }
        public PrivacyType Privacy { get; set; }
        public SecurityType SecurityType { get; set; }
        public SmugSearchable SmugSearchable { get; set; }
        public SortDirection SortDirection { get; set; }
        public SortMethod SortMethod { get; set; }
        public string UrlName { get; set; }
        public string UrlPath { get; set; }
        public WorldSearchable WorldSearchable { get; set; }
        public Uri WebUri { get; set; }
        public FolderUris Uris { get; set; }

        public override string ToString()
        {
            return string.Format("Folder: {0}, {1}", Name, JsonConvert.SerializeObject(this));
        }
    }

    public class FolderUris
    {
        public SmugMugUri AlbumList { get; set; }
        public SmugMugUri FolderAlbums { get; set; }
        public SmugMugUri FolderById { get; set; }
        public SmugMugUri FolderHighlightImage { get; set; }
        public SmugMugUri FolderList { get; set; }
        public SmugMugUri FolderPages { get; set; }
        public SmugMugUri Folders { get; set; }
        public SmugMugUri HighlightImage { get; set; }
        public SmugMugUri Node { get; set; }
        public SmugMugUri ParentFolder { get; set; }
        public SmugMugUri ParentFolders { get; set; }
        public SmugMugUri Size { get; set; }
        public SmugMugUri User { get; set; }
    }

    public class FolderGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public Folder Folder { get; set; }
    }

    public class FolderPostResponse : SmugMugUri
    {
        public Folder Folder { get; set; }

        public override string ToString()
        {
            return Folder.ToString();
        }
    }

    public class POSTParameter
    {
        public string ParameterName  { get; set; }
        public string Problem  { get; set; }
    }
}