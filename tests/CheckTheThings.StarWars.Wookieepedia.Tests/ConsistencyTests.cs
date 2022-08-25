using System.IO;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Xunit;

namespace CheckTheThings.StarWars.Wookieepedia.Tests
{
    public class ConsistencyTests
    {
        [Theory]
        [InlineData("./timeline_of_canon_media.html")]
        [InlineData("./Timeline_of_Legends_media.html")]
        public async Task ContentElementExists(string fileName)
        {
            var element = await GetMainContent(fileName);

            element.Should().NotBeNull();
        }

        [Theory]
        [InlineData("./timeline_of_canon_media.html")]
        [InlineData("./Timeline_of_Legends_media.html")]
        public async Task SortableTableExists(string fileName)
        {
            var element = await GetMainContent(fileName);

            var tables = element.QuerySelectorAll("table.sortable");

            tables.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Theory]
        [InlineData("./timeline_of_canon_media.html")]
        [InlineData("./Timeline_of_Legends_media.html")]
        public async Task SortableTablesHaveFiveColumns(string fileName)
        {
            var element = await GetMainContent(fileName);

            var tables = element.QuerySelectorAll("table.sortable");

            foreach (var table in tables)
            {
                var row = table.QuerySelector("tr");
                row.QuerySelectorAll("th").Should().HaveCount(5);
            }
        }

        [Theory]
        [InlineData("./timeline_of_canon_media.html")]
        [InlineData("./Timeline_of_Legends_media.html")]
        public async Task ManyRowsReturned(string fileName)
        {
            var element = await GetMainContent(fileName);

            var rows = element.QuerySelectorAll("table.sortable tr");

             rows.Should().NotBeNull().And.HaveCountGreaterThan(100);
        }

        [Theory]
        [InlineData("./timeline_of_canon_media.html")]
        [InlineData("./Timeline_of_Legends_media.html")]
        public async Task CompleteParse(string fileName)
        {
            var element = await GetHtmlDocumentAsync(fileName);
            var results = TimelineParser.Parse(element);
            results.Should().NotBeNull().And.HaveCountGreaterThan(100);
        }

        private static async Task<IElement> GetMainContent(string fileName)
        {
            var document = await GetHtmlDocumentAsync(fileName);
            return TimelineParser.GetMainContent(document);
        }

        private static async Task<IHtmlDocument> GetHtmlDocumentAsync(string fileName)
        {
            using var stream = File.OpenRead(fileName);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(stream);
            return document;
        }
    }
}
