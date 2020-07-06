using Newtonsoft.Json;

namespace SmugMug.NET
{
    public class UserProfile : SmugMugObject
    {
        public string BioText { get; set; }
        public string Blogger { get; set; }
        public string Custom { get; set; }
        public string DisplayName { get; set; }
        public string Facebook { get; set; }
        public string FirstName { get; set; }
        public string Flickr { get; set; }
        public string GooglePlus { get; set; }
        public string Instagram { get; set; }
        public string LastName { get; set; }
        public string LinkedIn { get; set; }
        public string Pinterest { get; set; }
        public string Tumblr { get; set; }
        public string Twitter { get; set; }
        public string Vimeo { get; set; }
        public string Wordpress { get; set; }
        public string YouTube { get; set; }
        public UserProfileUris Uris { get; set; }

        public override string ToString()
        {
            return string.Format("UserProfile: {0}, {1}", DisplayName, JsonConvert.SerializeObject(this));
        }
    }

    public class UserProfileUris
    {
        public SmugMugUri BioImage { get; set; }
        public SmugMugUri CoverImage { get; set; }
        public SmugMugUri User { get; set; }
    }

    public class UserProfileGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public UserProfile UserProfile { get; set; }
    }

    public class UserProfilePostResponse : SmugMugUri
    {
        public UserProfile UserProfile { get; set; }

        public override string ToString()
        {
            return UserProfile.ToString();
        }
    }
}