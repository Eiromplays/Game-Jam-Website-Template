using System.Collections.Generic;

namespace GameJam.Api.Models
{
    public class PeopleApiPhotos
    {
        public string resourceName { get; set; }
        public string etag { get; set; }
        public List<Photo> photos { get; set; }

        public class Source
        {
            public string type { get; set; }
            public string id { get; set; }
        }

        public class Metadata
        {
            public bool primary { get; set; }
            public Source source { get; set; }
        }

        public class Photo
        {
            public Metadata metadata { get; set; }
            public string url { get; set; }
        }
    }
}
