using System;

namespace CheckTheThings.StarWars.Wookieepedia
{
    public class Media
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        
        public string Type { get; set; }
        //public bool IsReleased { get; set; }
        public string Year { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<Author> Authors { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public record Author(string Name, string Slug);
}
