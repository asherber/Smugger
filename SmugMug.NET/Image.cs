using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace SmugMug.NET
{
    public class Image : SmugMugObject
    {
        public int Altitude;
        public string ArchivedMD5;
        public int ArchivedSize;
        public string ArchivedUri;
        public bool CanEdit;
        public string Caption;
        public bool Collectable;
        public DateTime Date;
        public bool EZProject;
        public string FileName;
        public string Format;
        public bool Hidden;
        public string ImageKey;
        public bool IsArchive;
        public bool IsVideo;
        public string[] KeywordArray;
        public string Keywords;
        public DateTime LastUpdated;
        public double Latitude;
        public double Longitude;
        public int OriginalHeight;
        public int OriginalSize;
        public int OriginalWidth;
        public bool Processing;
        public bool Protected;
        public string ThumbnailUrl;
        public string Title;
        public string UploadKey;
        public bool Watermarked;
        public Uri WebUri;
        public Dictionary<string, SmugMugUri> Uris;

        public override string ToString()
        {
            return string.Format("Image: {0}, {1}", FileName, JsonConvert.SerializeObject(this));
        }
    }

    public class ImageUris
    {
        public SmugMugUri ImageAlbum { get; set; }
        public SmugMugUri ImageComments { get; set; }
        public SmugMugUri ImageDownload { get; set; }
        public SmugMugUri ImageMetadata { get; set; }
        public SmugMugUri ImageOwner { get; set; }
        public SmugMugUri ImagePrices { get; set; }
        public SmugMugUri ImageSizeDetails { get; set; }
        public SmugMugUri ImageSizes { get; set; }
        public SmugMugUri LargestImage { get; set; }
        public SmugMugUri CSMILVideo { get; set; }
        public SmugMugUri EmbedVideo { get; set; }
        public SmugMugUri Regions { get; set; }
        public SmugMugUri PointOfInterestCrops { get; set; }
        public SmugMugUri PointOfInterest { get; set; }
        public SmugMugUri LargestVideo { get; set; }
    }

    public class ImageGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public Image Image { get; set; }
    }

    public class ImageUpload
    {
        public string StatusImageReplaceUri { get; set; }
        public string ImageUri { get; set; }
        public string AlbumImageUri { get; set; }
        public string URL { get; set; }
    }

    public class ImagePostResponse 
    {
        public string Stat { get; set; }
        public string Method { get; set; }
        public ImageUpload Image { get; set; }

        public override string ToString()
        {
            return Image.ToString();
        }
    }

    public class ImagePatchResponse : SmugMugUri
    {
        public Image Image { get; set; }

        public override string ToString()
        {
            return Image.ToString();
        }
    }

    public class ImageSizesGetResponse : SmugMugUri
    {
        public ImageSizes ImageSizes { get; set; }
    }

    public class ImageSizes
    {
        [JsonProperty("110VideoUrl")]
        public string VideoUrl110 { get; set; }

        [JsonProperty("1280VideoUrl")]
        public string VideoUrl1280 { get; set; }

        [JsonProperty("1920VideoUrl")]
        public string VideoUrl1920 { get; set; }

        [JsonProperty("200VideoUrl")]
        public string VideoUrl200 { get; set; }

        [JsonProperty("320VideoUrl")]
        public string VideoUrl320 { get; set; }

        [JsonProperty("4KImageUrl")]
        public string ImageUrl4k { get; set; }

        [JsonProperty("5KImageUrl")]
        public string ImageUrl5k { get; set; }

        [JsonProperty("640VideoUrl")]
        public string VideoUrl640 { get; set; }

        [JsonProperty("960VideoUrl")]
        public string VideoUrl960 { get; set; }

        public string LargeImageUrl { get; set; }

        public string LargestImageUrl { get; set; }
        public string LargestVideoUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string OriginalImageUrl { get; set; }
        public string SMILVideoUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string ThumbImageUrl { get; set; }
        public string TinyImageUrl { get; set; }
        public string X2LargeImageUrl { get; set; }
        public string X3LargeImageUrl { get; set; }
        public string X4LargeImageUrl { get; set; }
        public string X5LargeImageUrl { get; set; }
        public string XLargeImageUrl { get; set; }
    }

}