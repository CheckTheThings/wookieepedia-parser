using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

[assembly: InternalsVisibleTo("CheckTheThings.StarWars.Wookieepedia.Tests")]

namespace CheckTheThings.StarWars.Wookieepedia
{
    public class WookieepediaParser
    {
        private readonly static string[] ValidTypes = new string[] { "film", "novel", "comic", "videogame", "promotional", "tv", "short", "junior", "young" }; 

        public static IEnumerable<Media> Parse(IHtmlDocument document)
        {
            var element = GetMainContent(document);
            var sortableTable = GetSortableTables(element);

            foreach (var table in sortableTable)
            {
                var rows = table.QuerySelectorAll("tr");
                foreach (var row in rows.Skip(1).Select((element, index) => new { element, index }))
                {
                    var media = ParseRow(row.element);
                    media.Order = row.index;

                    yield return media;
                }
            }
        }

        internal static Media ParseRow(IElement row)
        {
            try
            {
                var columns = row.QuerySelectorAll("td");
                var yearColumn = columns[0];
                //var typeColumn = columns[1];
                var nameColumn = columns[2];
                //var writersColumn = columns[3];
                var releaseDateColumn = columns[4];


                var media = new Media
                {
                    Name = ParseName(nameColumn),
                    Title = ParseTitle(nameColumn),
                    Type = ParseType(row),
                    Year = ParseYear(yearColumn),
                    ReleaseDate = ParseReleaseDate(releaseDateColumn),
                    IsPublished = ParseIsPublished(row),
                };
                return media;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                throw;
            }
        }

        internal static string ParseName(IElement nameColumn) => (nameColumn.QuerySelector("a") as HtmlElement).TextContent.Trim();

        internal static string ParseTitle(IElement nameColumn) => (nameColumn.QuerySelector("a") as HtmlElement).Title.Trim();

        internal static bool ParseIsPublished(IElement row) =>
            !row.ClassList.Contains("unpublished") && !row.ClassList.Contains("unreleased");

        internal static DateTime? ParseReleaseDate(IElement releaseDateColumn) =>
            DateTime.TryParse(releaseDateColumn.TextContent, out var date) ? date : null;

        internal static string ParseType(IElement row) =>
            row.ClassList.Intersect(ValidTypes).SingleOrDefault();

        internal static string ParseYear(IElement yearColumn) =>
            yearColumn.QuerySelector("a")?.TextContent.Trim();

        internal static IHtmlCollection<IElement> GetSortableTables(IElement element) =>
            element.QuerySelectorAll("table.sortable");

        internal static IElement GetMainContent(IHtmlDocument document) =>
            document.GetElementById("mw-content-text");
    }
}
