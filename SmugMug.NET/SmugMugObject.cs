using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmugMug.NET
{
    public class SmugMugObject
    {
        public string Uri { get; set; }
        public string UriDescription { get; set; }
        public ResponseLevel ResponseLevel { get; set; }
    }

    public class SmugMugUri : SmugMugObject
    {
        public string EndpointType { get; set; }
        public string Locator { get; set; } //This is the data type of the object or objects in the response
        public string LocatorType { get; set; } //This tells you how many objects are in the response (Object means you get one object; Objects means an array of objects)
    }

    public class Pages
    {
        public int Count { get; set; }
        public string FirstPage { get; set; }
        public string LastPage { get; set; }
        public string NextPage { get; set; }
        public int RequestedCount { get; set; }
        public int Start { get; set; }
        public int Total { get; set; }
    }

    public class SmugMugPagesObject : SmugMugUri
    {
        public Pages Pages { get; set; }
    }
}
