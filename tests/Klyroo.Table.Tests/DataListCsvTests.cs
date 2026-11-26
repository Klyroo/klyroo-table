using System.Globalization;
using Xunit;

namespace Klyroo.Table.Tests;

public sealed class DataListCsvTests
{
    private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    private static string Write(string[] header, string?[][] rows, DataListCsvOptions? options = null) =>
        DataListCsv.Write(header, rows.Select(r => (IReadOnlyList<DataListCsvCell>)[.. r.Select(c => new DataListCsvCell(c))]), options ?? new DataListCsvOptions { Culture = En });

    [Fact]
    public void The_delimiter_follows_the_decimal_separator_of_the_culture_unless_it_is_given()
    {
        Assert.Equal(',', DataListCsv.DelimiterFor(En));
        Assert.Equal(';', DataListCsv.DelimiterFor(Fr));
        Assert.Equal(';', DataListCsv.DelimiterFor(CultureInfo.GetCultureInfo("de-DE")));
        Assert.Equal("a;b\r\n1;2\r\n", Write(["a", "b"], [["1", "2"]], new DataListCsvOptions { Culture = Fr }));
        Assert.Equal("a\tb\r\n1\t2\r\n", Write(["a", "b"], [["1", "2"]], new DataListCsvOptions { Culture = Fr, Delimiter = '\t' }));
    }

    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData("two\nlines", "\"two\nlines\"")]
    [InlineData("cr\rhere", "\"cr\rhere\"")]
    [InlineData(" leading", "\" leading\"")]
    [InlineData("trailing ", "\"trailing \"")]
    [InlineData("semi;colon", "semi;colon")]
    [InlineData("", "")]
    public void A_field_is_quoted_only_when_it_needs_to_be(string text, string expected) =>
        Assert.Equal(expected, DataListCsv.Field(text, ','));

    [Fact]
    public void A_field_with_the_delimiter_of_the_file_is_quoted()
    {
        Assert.Equal("\"semi;colon\"", DataListCsv.Field("semi;colon", ';'));
        Assert.Equal("a,b", DataListCsv.Field("a,b", ';'));
    }

    [Theory]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+1", "'+1")]
    [InlineData("-1", "'-1")]
    [InlineData("@SUM(A1)", "'@SUM(A1)")]
    [InlineData("\tcmd", "'\tcmd")]
    [InlineData("=HYPERLINK(\"x\",\"y\")", "\"'=HYPERLINK(\"\"x\"\",\"\"y\"\")\"")]
    [InlineData("a=b", "a=b")]
    public void A_text_that_starts_like_a_formula_gets_an_apostrophe(string text, string expected) =>
        Assert.Equal(expected, DataListCsv.Field(text, ','));

    [Fact]
    public void A_carriage_return_at_the_start_is_protected_and_quoted()
    {
        var field = DataListCsv.Field("\r=1", ',');
        Assert.Equal("\"'\r=1\"", field);
    }

    [Fact]
    public void A_typed_value_and_a_switched_off_protection_are_left_alone()
    {
        Assert.Equal("-5.00", DataListCsv.Field(new DataListCsvCell("-5.00", Trusted: true), ','));
        Assert.Equal("=1+1", DataListCsv.Field(new DataListCsvCell("=1+1"), ',', protectFormulas: false));
        Assert.Equal("=1+1", Write(["h"], [["=1+1"]], new DataListCsvOptions { Culture = En, ProtectFormulas = false }).Split("\r\n")[1]);
    }

    [Fact]
    public void The_header_is_protected_like_any_text_and_can_be_left_out()
    {
        Assert.Equal("'=evil,ok\r\n", Write(["=evil", "ok"], []));
        Assert.Equal("x\r\n", Write(["h"], [["x"]], new DataListCsvOptions { Culture = En, IncludeHeader = false }));
    }

    [Fact]
    public void The_byte_order_mark_and_the_new_line_are_options()
    {
        var withBom = Write(["h"], [["é"]], new DataListCsvOptions { Culture = En, Utf8Bom = true, NewLine = "\n" });
        Assert.Equal("﻿h\né\n", withBom);
        var result = new DataListExportResult("x.csv", withBom, 1, DataListExportScope.Page);
        Assert.Equal([0xEF, 0xBB, 0xBF], result.ToBytes()[..3]);
        Assert.Equal("text/csv;charset=utf-8", result.MimeType);
        // No mark in the text, none in the bytes.
        Assert.NotEqual(0xEF, new DataListExportResult("x.csv", "h", 0, DataListExportScope.Page).ToBytes()[0]);
    }

    [Theory]
    [InlineData("Orders", "Orders.csv")]
    [InlineData("orders.CSV", "orders.csv")]
    [InlineData("a/b\\c:d", "abcd.csv")]
    [InlineData("  ", "export.csv")]
    [InlineData(null, "export.csv")]
    [InlineData("..", "export.csv")]
    public void The_file_name_is_safe_and_ends_with_csv(string? name, string expected) =>
        Assert.Equal(expected, DataListCsv.FileName(name));
}
