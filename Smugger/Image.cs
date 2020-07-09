using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace Smugger
{
    public class Image : SmugMugObject
    {
        public int Altitude { get; set; }
        public string ArchivedMD5 { get; set; }
        public int ArchivedSize { get; set; }
        public string ArchivedUri { get; set; }
        public bool CanEdit { get; set; }
        public string Caption { get; set; }
        public bool Collectable { get; set; }
        public DateTime Date { get; set; }
        public bool EZProject { get; set; }
        public string FileName { get; set; }
        public string Format { get; set; }
        public bool Hidden { get; set; }
        public string ImageKey { get; set; }
        public bool IsArchive { get; set; }
        public bool IsVideo { get; set; }
        public string[] KeywordArray { get; set; }
        public string Keywords { get; set; }
        public DateTime LastUpdated { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalSize { get; set; }
        public int OriginalWidth { get; set; }
        public bool Processing { get; set; }
        public bool Protected { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Title { get; set; }
        public string UploadKey { get; set; }
        public bool Watermarked { get; set; }
        public Uri WebUri { get; set; }
        public Dictionary<string, SmugMugUri> Uris { get; set; }

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