using System.Runtime.CompilerServices;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

[assembly: InternalsVisibleTo("CheckTheThings.StarWars.Wookieepedia.Tests")]
namespace CheckTheThings.StarWars.Wookieepedia
{
    public class TimelineParser
    {
        private static readonly string[] ValidTypes = new string[] { "film", "novel", "comic", "videogame", "promotional", "tv", "short", "junior", "young" };

        public static async Task<IEnumerable<Media>> ParseCanonTimelineAsync()
        {
            using var stream = await GetContentStream("https://starwars.fandom.com/wiki/Timeline_of_canon_media");
            var media = (await Parse(stream)).ToList();

            foreach (var mediaItem in media)
            {
                mediaItem.Tags.Add("Canon");
            }

            return media;
        }

        public static async Task<IEnumerable<Media>> ParseLegendsTimelineAsync()
        {
            using var stream = await GetContentStream("https://starwars.fandom.com/wiki/Timeline_of_Legends_media");
            var media = (await Parse(stream)).ToList();

            foreach (var mediaItem in media)
                mediaItem.Tags.Add("Legends");

            return media;
        }

        private static async Task<Stream> GetContentStream(string url)
        {
            var response = await new HttpClient().GetAsync(url);

            return await response.Content.ReadAsStreamAsync();
        }

        public static async Task<IEnumerable<Media>> Parse(Stream stream)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(stream);
            return Parse(document);
        }

        internal static IEnumerable<Media> Parse(IHtmlDocument document)
        {
            var element = GetMainContent(document);
            var sortableTable = GetSortableTables(element);

            foreach (var table in sortableTable)
            {
                var rows = table.QuerySelectorAll("tr");
                foreach (var row in rows.Skip(1))
                {
                    if (ParseIsReleased(row))
                        yield return ParseRow(row);
                }
            }
        }

        internal static Media ParseRow(IElement row)
        {
            var columns = row.QuerySelectorAll("td");
            var yearColumn = columns[0];
            //var typeColumn = columns[1];
            var nameColumn = columns[2];
            var authorsColumn = columns[3];
            var releaseDateColumn = columns[4];


            try
            {
                var media = new Media
                {
                    Name = ParseName(nameColumn),
                    Title = ParseTitle(nameColumn),
                    Slug = ParseSlug(nameColumn),
                    Type = ParseType(row),
                    Year = ParseYear(yearColumn),
                    ReleaseDate = ParseReleaseDate(releaseDateColumn),
                    //IsReleased = true,
                    Authors = ParseAuthors(authorsColumn),
                };
                return media;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal static string ParseName(IElement nameColumn) => (nameColumn.QuerySelector("a") as HtmlElement).TextContent.Trim();

        internal static string ParseTitle(IElement nameColumn) => (nameColumn.QuerySelector("a") as HtmlElement).Title.Trim();

        internal static string ParseSlug(IElement nameColumn) => (nameColumn.QuerySelector("a") as IHtmlAnchorElement).PathName;

        internal static bool ParseIsReleased(IElement row) =>
            !row.ClassList.Contains("unpublished") && !row.ClassList.Contains("unreleased");

        internal static DateTime? ParseReleaseDate(IElement releaseDateColumn)
        {
            if (DateTime.TryParse(releaseDateColumn.TextContent, out var date))
                return date;
            if (releaseDateColumn.TextContent.Trim().Length == 4 && int.TryParse(releaseDateColumn.TextContent.Trim(), out var year))
                return new DateTime(year, 1, 1);
            return null;
        }

        internal static string ParseType(IElement row) =>
            row.ClassList.Intersect(ValidTypes).SingleOrDefault();

        internal static string ParseYear(IElement yearColumn)
        {
            var supElements = yearColumn.QuerySelectorAll("sup");
            foreach (var element in supElements)
            {
                element.Remove();
            }

            var yearContent = yearColumn.TextContent.Trim();
            if (yearContent.StartsWith("c."))
            {
                yearContent = yearContent[2..].Trim();
            }

            return yearContent == string.Empty ? null : yearContent;
        }

        internal static List<Author> ParseAuthors(IElement authorsColumn) =>
            authorsColumn.QuerySelectorAll("a")
                .Cast<IHtmlAnchorElement>()
                .Select(a => new Author(a.Text.Trim(), ParseAuthorSlug(a)))
                .ToList();

        private static string ParseAuthorSlug(IHtmlAnchorElement a) =>
            !string.IsNullOrWhiteSpace(a.PathName)
                ? a.PathName
                : a.Text.Replace(' ', '_');

        internal static IHtmlCollection<IElement> GetSortableTables(IElement element) =>
            element.QuerySelectorAll("table.sortable");

        internal static IElement GetMainContent(IHtmlDocument document) =>
            document.GetElementById("mw-content-text");
    }
}
