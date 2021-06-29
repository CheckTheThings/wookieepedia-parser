using System;

namespace CheckTheThings.StarWars.Wookieepedia
{
    public class Media
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public bool IsPublished { get; set; }
        public string Year { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
