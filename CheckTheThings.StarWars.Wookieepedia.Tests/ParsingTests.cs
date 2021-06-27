using System;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Xunit;

namespace CheckTheThings.StarWars.Wookieepedia.Tests
{
    public class ParsingTests
    {
        private static readonly HtmlParser parser = new();

        //        <tr class="novel">
        //<td data-sort-value="-257">c. <a href = "/wiki/232_BBY" title="232 BBY">232 BBY</a><sup id = "cite_ref-High_Republic_Date_2-0" class="reference"><a href = "#cite_note-High_Republic_Date-2" > &#91;2&#93;</a></sup></td>
        //< td style="background-color: #8DB3E2; text-align: center;">N</td>
        //<td><i><a href = "/wiki/The_High_Republic:_Into_the_Dark" title="The High Republic: Into the Dark">The High Republic: Into the Dark</a></i>
        //<ul><li><small>Occurs prior to and concurrently with<i><a href = "/wiki/The_High_Republic:_Light_of_the_Jedi" title= "The High Republic: Light of the Jedi" > The High Republic: Light of the Jedi</a></i>.</small></li></ul>
        //</td>
        //<td><a href = "/wiki/Claudia_Gray" title= "Claudia Gray" > Claudia Gray</a>
        //</td>
        //<td>2021-02-02
        //</td></tr>

        [Fact]
        public void Template()
        {
            var html = "<table><tr><td></td></tr></table>";
            var element = GetTdElement(html);
            var result = WookieepediaParser.ParseYear(element);
            result.Should().BeNull();
        }

        [Fact]
        public void NoisyName()
        {
            var html = "<table><tr><td><i><a>The High Republic: Into the Dark</a></i></td></tr></table>";
            var element = GetTdElement(html);
            var result = WookieepediaParser.ParseName(element);
            result.Should().Be("The High Republic: Into the Dark");
        }


        [Fact]
        public void SimpleTitle()
        {
            var html = "<table><tr><td><i><a title=\"The High Republic: Into the Dark\">Name</a></i></td></tr></table>";
            var element = GetTdElement(html);
            var result = WookieepediaParser.ParseTitle(element);
            result.Should().Be("The High Republic: Into the Dark");
        }

        [Fact]
        public void ValidYear()
        {
            var html = "<table><tr><td>c. <a>232 BBY</a><sup><a>1</a></sup></td></tr></table>";
            var element = GetTdElement(html);
            var year = WookieepediaParser.ParseYear(element);
            year.Should().Be("232 BBY");
        }

        [Fact]
        public void MissingYear()
        {
            var html = "<table><tr><td></td></tr></table>";
            var element = GetTdElement(html);
            var year = WookieepediaParser.ParseYear(element);
            year.Should().BeNull();
        }

        [Fact]
        public void ValidReleaseDate()
        {
            var html = "<table><tr><td>2021-02-02</td></tr></table>";
            var element = GetTdElement(html);
            var result = WookieepediaParser.ParseReleaseDate(element);
            result.Should().Be(new DateTime(2021, 02, 02));
        }

        [Fact]
        public void MalformedReleaseDate()
        {
            var html = "<table><tr><td>2021-??-??</td></tr></table>";
            var element = GetTdElement(html);
            var result = WookieepediaParser.ParseReleaseDate(element);
            result.Should().BeNull();
        }


        [Fact]
        public void SimpleType()
        {
            var html = "<table><tr class=\"novel\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void UnpublishedType()
        {
            var html = "<table><tr class=\"novel unpublished\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void UnreleasedType()
        {
            var html = "<table><tr class=\"novel unreleased\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseType(element);
            result.Should().Be("novel");
        }

        [Fact]
        public void MissingType()
        {
            var html = "<table><tr class=\"\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseType(element);
            result.Should().BeNull();
        }

        [Fact]
        public void Published()
        {
            var html = "<table><tr class=\"novel\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseIsPublished(element);
            result.Should().BeTrue();
        }

        [Fact]
        public void Unpublished()
        {
            var html = "<table><tr class=\"unpublished\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseIsPublished(element);
            result.Should().BeFalse();
        }

        [Fact]
        public void Unreleased()
        {
            var html = "<table><tr class=\"unreleased\"><td></td></tr></table>";
            var element = GetTrElement(html);
            var result = WookieepediaParser.ParseIsPublished(element);
            result.Should().BeFalse();
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
            var result = WookieepediaParser.ParseRow(element);
            result.Name.Should().Be("The High Republic: Into the Dark");
            result.Title.Should().Be("The High Republic: Into the Dark");
            result.Year.Should().Be("232 BBY");
            result.ReleaseDate.Should().Be(new DateTime(2021, 02, 02));
            result.IsPublished.Should().BeTrue();
            result.Type.Should().Be("novel");
        }

        private static IElement GetTrElement(string html) => parser.ParseFragment(html, null).GetElementsByTagName("tr").First();
        private static IElement GetTdElement(string html) => parser.ParseFragment(html, null).GetElementsByTagName("td").First();
    }
}
