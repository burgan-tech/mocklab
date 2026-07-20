using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Mocklab.Host.Services;

namespace Mocklab.Host.Tests;

public class ScribanTemplateProcessorTests
{
    private static string Render(string template)
    {
        var processor = new ScribanTemplateProcessor(NullLogger<ScribanTemplateProcessor>.Instance);
        var request = new DefaultHttpContext().Request;
        return processor.ProcessTemplate(template, request);
    }

    [Fact]
    public void UserReportedBody_RendersAllHelpers()
    {
        const string template = """
            {
              "data": {
                "kkbScore": {{ random_int 18 65 }},
                "totalExistingDebt": {{ random_int 18 65 }},
                "inquiryDate": "{{ iso_timestamp }}"
              }
            }
            """;

        var output = Render(template);

        Assert.DoesNotContain("{{", output);
        using var doc = JsonDocument.Parse(output);
        var data = doc.RootElement.GetProperty("data");
        var score = data.GetProperty("kkbScore").GetInt32();
        var debt = data.GetProperty("totalExistingDebt").GetInt32();
        Assert.InRange(score, 18, 65);
        Assert.InRange(debt, 18, 65);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("inquiryDate").GetString()));
    }

    [Fact]
    public void RandomInt_WithMinMax_RendersIntegerInRange()
    {
        var output = Render("{{ random_int 18 65 }}");

        var value = int.Parse(output);
        Assert.InRange(value, 18, 65);
    }

    [Fact]
    public void RandomInt_WithoutArguments_RendersInteger()
    {
        var output = Render("{{ random_int }}");

        var value = int.Parse(output);
        Assert.InRange(value, 1, 999_999);
    }

    [Fact]
    public void RandomString_WithLengthOnly_RendersStringOfLength()
    {
        var output = Render("{{ random_string 12 }}");

        Assert.Equal(12, output.Length);
    }

    [Fact]
    public void RandomString_WithLengthAndChars_UsesOnlyGivenChars()
    {
        var output = Render("{{ random_string 12 \"abc\" }}");

        Assert.Equal(12, output.Length);
        Assert.All(output, c => Assert.Contains(c, "abc"));
    }

    [Fact]
    public void HelpersNamespace_RandomInt_WithMinMax_RendersIntegerInRange()
    {
        var output = Render("{{ helpers.random_int 18 65 }}");

        var value = int.Parse(output);
        Assert.InRange(value, 18, 65);
    }

    [Fact]
    public void HelpersNamespace_RandInt_RendersIntegerInRange()
    {
        var output = Render("{{ helpers.rand_int 18 65 }}");

        var value = int.Parse(output);
        Assert.InRange(value, 18, 65);
    }

    [Fact]
    public void LegacyRandomIntRange_IsNormalizedAndRendered()
    {
        var output = Render("{{$randomInt(1,100)}}");

        var value = int.Parse(output);
        Assert.InRange(value, 1, 100);
    }

    [Fact]
    public void Now_RendersNonEmptyValue()
    {
        var output = Render("{{ now }}");

        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.DoesNotContain("{{", output);
    }

    [Fact]
    public void IsoTimestamp_RendersRoundTripDateTime()
    {
        var output = Render("{{ iso_timestamp }}");

        Assert.True(DateTime.TryParse(output, null, System.Globalization.DateTimeStyles.RoundtripKind, out _));
    }

    [Theory]
    [InlineData("guid")]
    [InlineData("random_int")]
    [InlineData("random_float")]
    [InlineData("random_name")]
    [InlineData("random_first_name")]
    [InlineData("random_last_name")]
    [InlineData("random_email")]
    [InlineData("random_phone")]
    [InlineData("random_bool")]
    [InlineData("timestamp")]
    [InlineData("iso_timestamp")]
    [InlineData("now")]
    [InlineData("now_fmt 'yyyy-MM-dd'")]
    [InlineData("random_number_string 6")]
    [InlineData("random_alpha_numeric 12")]
    [InlineData("random_string 8")]
    [InlineData("random_double 1.5 2.5")]
    [InlineData("random_company_name")]
    [InlineData("random_city")]
    [InlineData("random_country")]
    [InlineData("random_currency_code")]
    [InlineData("random_product_name")]
    [InlineData("random_job_title")]
    [InlineData("random_address")]
    [InlineData("random_iban")]
    [InlineData("random_username")]
    [InlineData("random_password")]
    [InlineData("random_age")]
    [InlineData("random_birthdate")]
    [InlineData("random_zip_code")]
    [InlineData("random_latitude")]
    [InlineData("random_longitude")]
    [InlineData("random_account_number")]
    [InlineData("random_swift_code")]
    [InlineData("random_credit_card_number")]
    [InlineData("random_price")]
    [InlineData("random_stock_symbol")]
    [InlineData("random_transaction_type")]
    [InlineData("random_ip")]
    [InlineData("random_mac_address")]
    [InlineData("random_url")]
    [InlineData("random_status")]
    [InlineData("random_http_status_code")]
    [InlineData("random_color")]
    [InlineData("random_hex_color")]
    [InlineData("random_department")]
    [InlineData("random_category")]
    [InlineData("random_role")]
    [InlineData("random_priority")]
    [InlineData("random_ticket_status")]
    [InlineData("random_order_status")]
    [InlineData("random_language_code")]
    [InlineData("random_continent")]
    [InlineData("random_timezone")]
    [InlineData("random_file_extension")]
    [InlineData("random_mime_type")]
    [InlineData("add 2 3")]
    [InlineData("subtract 5 3")]
    [InlineData("multiply 2 3")]
    [InlineData("divide 6 3")]
    [InlineData("upper \"abc\"")]
    [InlineData("lower \"ABC\"")]
    [InlineData("date_format (now) 'yyyy-MM-dd'")]
    [InlineData("date_time_add (now) 1 'days'")]
    [InlineData("date_time_add_fmt (now) 1 'days' 'yyyy-MM-dd'")]
    [InlineData("faker 'person.firstName'")]
    [InlineData("faker 'number.int' 1 100")]
    public void DocumentedTopLevelHelpers_Render(string expression)
    {
        var output = Render("{{ " + expression + " }}");

        Assert.DoesNotContain("{{", output);
        Assert.False(string.IsNullOrWhiteSpace(output), $"'{expression}' rendered empty output");
    }
}
