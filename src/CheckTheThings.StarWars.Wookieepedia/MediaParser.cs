using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace CheckTheThings.StarWars.Wookieepedia
{
    public class MediaParser
    {
        private static readonly Uri _baseUrl = new("https://starwars.fandom.com/");

        public static async Task<IEnumerable<KeyValuePair<string, string>>> ParseAsync(string urlPath)
        {
            var uri = new Uri(_baseUrl, urlPath);
            var response = await new HttpClient().GetAsync(uri);

            return await ParseAsync(await response.Content.ReadAsStreamAsync());
        }

        public static async Task<IEnumerable<KeyValuePair<string, string>>> ParseAsync(Stream stream)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(stream);
            return ParseAside(document);
        }

        internal static IEnumerable<KeyValuePair<string, string>> ParseAside(IHtmlDocument document)
        {
            var asideRows = document.QuerySelectorAll("aside:first-of-type > section > div");

            foreach (var row in asideRows)
            {
                var key = row.FirstElementChild.TextContent;
                var valueCell = row.LastElementChild;   // <div>

                switch (valueCell.FirstElementChild?.TagName)
                {
                    case "A":
                        var value = valueCell.FirstElementChild?.TextContent;
                        yield return new KeyValuePair<string, string>(key, value);
                        break;
                    case "UL":
                        var links = valueCell.QuerySelectorAll("ul li a");
                        foreach (var link in links)
                        {
                            yield return new KeyValuePair<string, string>(key, link.TextContent);
                        }
                        break;
                    default:
                        yield return new KeyValuePair<string, string>(key, valueCell.TextContent);
                        break;
                }
            }
        }
    }
}
