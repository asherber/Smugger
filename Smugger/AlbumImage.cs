using System;
using System.Collections.Generic;

namespace Smugger
{
    public class AlbumImage : Image
    {
    }
    

    public class AlbumImageResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public AlbumImage AlbumImage { get; set; }
    }

    public class AlbumImagePagesResponse : SmugMugPagesObject
    {
        public IEnumerable<AlbumImage> AlbumImage { get; set; }
    }

    public class AlbumImagesWithSizes
    {
        public List<AlbumImage> AlbumImages { get; set; }
        public Dictionary<string, ImageSizesGetResponse> ImageSizes { get; set; }
    }

}