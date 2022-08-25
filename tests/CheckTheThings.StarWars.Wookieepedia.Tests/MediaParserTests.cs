using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Xunit;

namespace CheckTheThings.StarWars.Wookieepedia.Tests
{
    public class MediaParserTests
    {
        [Theory]
        [InlineData("./The_High_Republic_Into_the_Dark.html", "Disney–Lucasfilm Press")]
        public async Task SinglePublisher(string fileName, string publisher)
        {
            var element = await GetHtmlDocumentAsync(fileName);
            var results = MediaParser.ParseAside(element);

            var publisherRow = results.First(x => x.Key == "Publisher");
            publisherRow.Value.Should().Be(publisher);
        }

        [Theory]
        [InlineData("./Force_Collector.html", "Egmont UK Ltd", "Disney–Lucasfilm Press")]
        public async Task MultiplePublishers(string fileName, params string[] publishers)
        {
            var element = await GetHtmlDocumentAsync(fileName);
            var results = MediaParser.ParseAside(element);

            var publisherRows = results.Where(x => x.Key == "Publisher").ToArray();
            publisherRows.Should().HaveCount(2);
            for (int i = 0; i < 2; i++)
            {
                publisherRows[i].Value.Should().Be(publishers[i]);
            }
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
