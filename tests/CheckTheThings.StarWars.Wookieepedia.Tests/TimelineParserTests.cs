using System;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Xunit;

namespace CheckTheThings.StarWars.Wookieepedia.Tests
{
    public class TimelineParserTests
    {
        private static readonly HtmlParser Parser = new();

        [Fact]
        public void Template()
        {
            var html = "<table><tr><td></td></tr></table>";
            var element = GetTdElement(html);
            var result = TimelineParser.ParseYear(element);
            result.Should().BeNull();
        }

        [Fact]
        public void NoisyName()
        {
            var html = "<table><tr><td><i><a>The High Republic: Into the Dark</a></i></td></tr></table>";
            var element = GetTdElement(html);
            var result = TimelineParser.ParseName(element);
            result.Should().Be("The High Republic: Into the Dark");
        }


        [Fact]
        public void SimpleTitle()
        {
            var html = "<table><tr><td><i><a title=\"The High Republic: Into the Dark\">Name</a></i></td></tr></table>";
            var element = GetTdElement(html);
            var result = TimelineParser.ParseTitle(element);
            result.Should().Be("The High Republic: Into the Dark");
        }

        [Fact]
        public void Parses_item_slug_correctly()
        {
            var html = @"
                <table><tr><td>
                    <i><a href=""/wiki/The_High_Republic:_Into_the_Dark"">Name</a></i>
                    <ul><li><small>blah<i><a>Another name</a></i>.</small></li></ul>
                </td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseSlug(element);
            result.Should().Be("/wiki/The_High_Republic:_Into_the_Dark");
        }

        [Fact]
        public void ValidYear()
        {
            var html = "<table><tr><td>c. <a>232 BBY</a><sup><a>1</a></sup></td></tr></table>";
            var element = GetTdElement(html);
            var year = TimelineParser.ParseYear(element);
            year.Should().Be("232 BBY");
        }

        [Fact]
        public void MissingYear()
        {
            var html = "<table><tr><td></td></tr></table>";
            var element = GetTdElement(html);
            var year = TimelineParser.ParseYear(element);
            year.Should().BeNull();
        }

        [Fact]
        public void YearIsRange()
        {
            var html = "<table><tr><td><a>13</a>–<a>10 BBY</a><sup><a>[55]</a></sup></td></tr></table>";
            var element = GetTdElement(html);
            var year = TimelineParser.ParseYear(element);
            year.Should().Be("13–10 BBY");
        }

        [Fact]
        public void CircaYear()
        {
            var html = "<table><tr><td>c. <a>13 BBY</a><sup><a>[54]</a></sup></td></td></tr></table>";
            var element = GetTdElement(html);
            var year = TimelineParser.ParseYear(element);
            year.Should().Be("13 BBY");
        }

        [Fact]
        public void ValidReleaseDate()
        {
            var html = "<table><tr><td>2021-02-02</td></tr></table>";
            var element = GetTdElement(html);
            var result = TimelineParser.ParseReleaseDate(element);
            result.Should().Be(new DateTime(2021, 02, 02));
        }

        [Fact]
        public void MalformedReleaseDate()
        {
            var html = "<table><tr><td>2021-??-??</td></tr></table>";
            var element = GetTdElement(html);
            var result = TimelineParser.ParseReleaseDate(element);
            result.Should().BeNull();
        }

        [Fact]
        public void Release_date_only_has_month_and_year()
        {
            var releaseDateString = "2021-02";
            var releaseDate = new DateTime(2021, 02, 01);
            var element = GetElement($"<td>{releaseDateString}</td>");

            var result = TimelineParser.ParseReleaseDate(element);

            result.Should().Be(releaseDate);
        }

        [Theory]
        [InlineData("2021", 2021)]
        [InlineData(" 1990 ", 1990)]
        public void Release_date_only_has_year(string dateString, int expectedYear)
        {
            var element = GetElement($"<td>{dateString}</td>");

            var result = TimelineParser.ParseReleaseDate(element);
             
            result.Should().Be(new DateTime(expectedYear, 1, 1));
        }


        [Fact]
        public void SimpleType()
        {
            var html = "<table><tr class=\"novel\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void UnpublishedType()
        {
            var html = "<table><tr class=\"novel unpublished\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void UnreleasedType()
        {
            var html = "<table><tr class=\"novel unreleased\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void MissingType()
        {
            var html = "<table><tr class=\"\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseType(element);
            result.Should().BeNull();
        }

        [Fact]
        public void Released()
        {
            var html = "<table><tr class=\"novel\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseIsReleased(element);
            result.Should().BeTrue();
        }

        [Fact]
        public void Unpublished()
        {
            var html = "<table><tr class=\"unpublished\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseIsReleased(element);
            result.Should().BeFalse();
        }

        [Fact]
        public void Unreleased()
        {
            var html = "<table><tr class=\"unreleased\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = TimelineParser.ParseIsReleased(element);
            result.Should().BeFalse();
        }

        [Fact]
        public void A_single_author()
        {
            var html = "<table><tr><td><a href = \"/wiki/Claudia_Gray\" title= \"Claudia Gray\" > Claudia Gray</a></td></tr></table>";
            var element = GetTrElement(html);
            var results = TimelineParser.ParseAuthors(element);
            results.Should().HaveCount(1);

            var result = results.First();
            result.Name.Should().Be("Claudia Gray");
            result.Slug.Should().Be("/wiki/Claudia_Gray");
        }

        [Fact]
        public void A_author_without_a_page()
        {
            var html = "<td><a class=\"new\" title=\"Caitlin Sullivan Kelly (page does not exist)\">Caitlin Sullivan Kelly</a></td>";
            var element = GetElement(html);
            var results = TimelineParser.ParseAuthors(element);
            results.Should().HaveCount(1);

            var result = results.First();
            result.Name.Should().Be("Caitlin Sullivan Kelly");
            result.Slug.Should().Be("");
        }

        [Fact]
        public void Multiple_authors()
        {
            var html = "<table><td><a href = \"/wiki/Kevin_Burke\" title=\"Kevin Burke\">Kevin Burke</a><br /><a href = \"/wiki/Chris_Wyatt\" title= \" Chris Wyatt \" > Chris Wyatt</a></td></table>";
            var element = GetTdElement(html);
            var results = TimelineParser.ParseAuthors(element);
            results.Should().HaveCount(2);
        }

        [Fact]
        public void ParseRow()
        {
            var html = @"
<table>
    <tr class=""novel"">
        <td data-sort-value=""-257"">c. <a href = ""/wiki/232_BBY"" title=""232 BBY"">232 BBY</a><sup id = ""cite_ref-High_Republic_Date_2-0"" class=""reference""><a href = ""#cite_note-High_Republic_Date-2"" > &#91;2&#93;</a></sup></td>
        <td style=""background-color: #8DB3E2; text-align: center;"">N</td>
        <td><i><a href = ""/wiki/The_High_Republic:_Into_the_Dark"" title=""The High Republic: Into the Dark"">The High Republic: Into the Dark</a></i>
        <ul><li><small>Occurs prior to and concurrently with<i><a href = ""/wiki/The_High_Republic:_Light_of_the_Jedi"" title= ""The High Republic: Light of the Jedi"" > The High Republic: Light of the Jedi</a></i>.</small></li></ul>
        </td>
        <td><a href = ""/wiki/Claudia_Gray"" title= ""Claudia Gray"" > Claudia Gray</a>
        </td>
        <td>2021-02-02
        </td>
    </tr>
</table>";

            var element = GetTrElement(html);
            var result = TimelineParser.ParseRow(element);
            result.Name.Should().Be("The High Republic: Into the Dark");
            result.Title.Should().Be("The High Republic: Into the Dark");
            result.Year.Should().Be("232 BBY");
            result.ReleaseDate.Should().Be(new DateTime(2021, 02, 02));
            result.Type.Should().Be("novel");
            result.Authors.Should().HaveCount(1);
            result.Authors.First().Name.Should().Be("Claudia Gray");
        }

        private static IElement GetTrElement(string html) => Parser.ParseFragment(html, null).GetElementsByTagName("tr").First();
        private static IElement GetTdElement(string html) => Parser.ParseFragment(html, null).GetElementsByTagName("td").First();
        private static IElement GetElement(string html) => Parser.ParseFragment(html, null).First() as Element;
    }
}
