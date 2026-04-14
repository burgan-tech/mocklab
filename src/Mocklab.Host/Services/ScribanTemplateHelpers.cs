using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Mocklab.Host.Services;

/// <summary>
/// Built-in helper functions exposed to Scriban templates (guid, random_*, timestamp, etc.).
/// All methods are stateless and safe (no file/network access).
/// </summary>
public static class ScribanTemplateHelpers
{
    private static readonly string[] FirstNames =
    [
        "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah",
        "Ivan", "Julia", "Kevin", "Laura", "Michael", "Nancy", "Oscar", "Patricia",
        "Quentin", "Rachel", "Samuel", "Tina"
    ];

    private static readonly string[] LastNames =
    [
        "Johnson", "Smith", "Brown", "Ross", "Norton", "Apple", "Miller", "Montana",
        "Petrov", "Roberts", "Hart", "Palmer", "Scott", "Drew", "Wilde", "Arquette",
        "Blake", "Green", "Jackson", "Turner"
    ];

    private static readonly string[] SampleDomains =
    [
        "example.com", "test.org", "mock.io", "demo.dev", "sample.net"
    ];

    private const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Guid() => System.Guid.NewGuid().ToString();

    public static int RandomInt() => Random.Shared.Next(1, 1000000);
    public static int RandomInt(int min, int max) => Random.Shared.Next(min, max + 1);
    public static double RandomFloat() => Random.Shared.NextDouble() * 1000;
    public static double RandomDouble(double min, double max)
    {
        var u = Random.Shared.NextDouble();
        return min + (u * (max - min));
    }

    public static string RandomName() =>
        $"{FirstNames[Random.Shared.Next(FirstNames.Length)]} {LastNames[Random.Shared.Next(LastNames.Length)]}";

    public static string RandomFirstName() => FirstNames[Random.Shared.Next(FirstNames.Length)];
    public static string RandomLastName() => LastNames[Random.Shared.Next(LastNames.Length)];

    public static string RandomEmail()
    {
        var name = FirstNames[Random.Shared.Next(FirstNames.Length)].ToLowerInvariant() + "." +
                   LastNames[Random.Shared.Next(LastNames.Length)].ToLowerInvariant();
        var domain = SampleDomains[Random.Shared.Next(SampleDomains.Length)];
        return $"{name}@{domain}";
    }

    /// <summary>
    /// Random phone in E.164-like format +90 5XX XXX XX XX (Turkey) or simple 10-digit.
    /// </summary>
    public static string RandomPhone() =>
        $"+90 5{Random.Shared.Next(3, 6)} {Random.Shared.Next(100, 1000)} {Random.Shared.Next(10, 100)} {Random.Shared.Next(10, 100)}";

    public static string RandomAlphaNumeric(int length) => RandomString(length, null);
    public static string RandomString(int length, string? chars = null)
    {
        var set = chars ?? AlphaNumericChars;
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append(set[Random.Shared.Next(set.Length)]);
        return sb.ToString();
    }
    /// <summary>Overload for Scriban: random_string(length) with no custom chars.</summary>
    public static string RandomStringLengthOnly(int length) => RandomString(length, null);
    /// <summary>Overload for Scriban: random_string(length, chars).</summary>
    public static string RandomStringWithChars(int length, string chars) => RandomString(length, chars);

    public static long Timestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public static string IsoTimestamp() => DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    public static DateTime Now() => DateTime.UtcNow;

    public static bool RandomBool() => Random.Shared.Next(2) == 1;

    public static string Upper(string? input) => input?.ToUpperInvariant() ?? string.Empty;
    public static string Lower(string? input) => input?.ToLowerInvariant() ?? string.Empty;

    /// <summary>
    /// Generates a random numeric string of exactly <paramref name="length"/> digits.
    /// First digit is never 0 so the result always has the requested length when treated as a number.
    /// </summary>
    public static string RandomNumberString(int length)
    {
        if (length <= 0) return string.Empty;
        var sb = new StringBuilder(length);
        sb.Append((char)('1' + Random.Shared.Next(9)));
        for (var i = 1; i < length; i++)
            sb.Append((char)('0' + Random.Shared.Next(10)));
        return sb.ToString();
    }

    // --- Pre-generated domain data helpers ---

    private static readonly string[] CompanyNames =
    [
        "Acme Corp", "Globex Industries", "Initech Solutions", "Umbrella Technologies",
        "Soylent Systems", "Massive Dynamic", "Cyberdyne Services", "Nakatomi Trading",
        "Dunder Mifflin Inc", "Stark Enterprises", "Wayne Industries", "Oscorp Limited",
        "Aperture Science", "Black Mesa Research", "Weyland-Yutani Corp", "Tyrell Corporation",
        "Rekall Inc", "OCP International", "Versalife Group", "Omni Consumer Products"
    ];

    private static readonly string[] Cities =
    [
        "Istanbul", "Ankara", "Izmir", "Bursa", "Adana", "Gaziantep", "Konya", "Antalya",
        "London", "Paris", "Berlin", "Amsterdam", "Madrid", "Rome", "Vienna", "Prague",
        "New York", "Los Angeles", "Chicago", "Toronto", "Sydney", "Tokyo", "Singapore", "Dubai"
    ];

    private static readonly string[] Countries =
    [
        "Turkey", "Germany", "United Kingdom", "France", "United States", "Netherlands",
        "Spain", "Italy", "Sweden", "Norway", "Poland", "Austria", "Switzerland",
        "Canada", "Australia", "Japan", "South Korea", "Singapore", "United Arab Emirates", "Brazil"
    ];

    private static readonly string[] CurrencyCodes =
    [
        "TRY", "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "SEK", "NOK",
        "DKK", "PLN", "CZK", "HUF", "AED", "SAR", "SGD", "HKD", "KRW", "BRL"
    ];

    private static readonly string[] ProductNames =
    [
        "Premium Widget", "Ultra Gadget", "Smart Device", "Pro Toolkit", "Deluxe Package",
        "Standard Bundle", "Elite Controller", "Basic Sensor", "Advanced Module", "Classic Unit",
        "Turbo Connector", "Mega Adapter", "Mini Component", "Flex Platform", "Core Engine",
        "Edge Gateway", "Cloud Node", "Nano Chip", "Macro Board", "Hybrid Interface"
    ];

    private static readonly string[] JobTitles =
    [
        "Software Engineer", "Product Manager", "Data Scientist", "DevOps Engineer", "UX Designer",
        "Backend Developer", "Frontend Developer", "QA Engineer", "System Architect", "Tech Lead",
        "Engineering Manager", "CTO", "CEO", "CFO", "COO", "Marketing Manager",
        "Sales Executive", "HR Manager", "Business Analyst", "Project Manager"
    ];

    private static readonly string[] StreetNames =
    [
        "Main Street", "Oak Avenue", "Maple Road", "Cedar Lane", "Pine Boulevard",
        "Elm Street", "Park Drive", "Lake View", "Hill Crest", "Valley Way",
        "River Road", "Forest Path", "Sunset Boulevard", "Harbor View", "Mountain Pass"
    ];

    private static readonly string[] IbanCountryPrefixes =
    [
        "TR", "DE", "GB", "FR", "NL", "ES", "IT", "SE", "NO", "PL"
    ];

    public static string RandomCompanyName() => CompanyNames[Random.Shared.Next(CompanyNames.Length)];

    public static string RandomCity() => Cities[Random.Shared.Next(Cities.Length)];

    public static string RandomCountry() => Countries[Random.Shared.Next(Countries.Length)];

    public static string RandomCurrencyCode() => CurrencyCodes[Random.Shared.Next(CurrencyCodes.Length)];

    public static string RandomProductName() => ProductNames[Random.Shared.Next(ProductNames.Length)];

    public static string RandomJobTitle() => JobTitles[Random.Shared.Next(JobTitles.Length)];

    public static string RandomAddress()
    {
        var number = Random.Shared.Next(1, 999);
        var street = StreetNames[Random.Shared.Next(StreetNames.Length)];
        var city = Cities[Random.Shared.Next(Cities.Length)];
        return $"{number} {street}, {city}";
    }

    /// <summary>
    /// Generates a random IBAN-like string (not cryptographically valid, suitable for mock data only).
    /// Format: CC + 2-digit check + 4-digit bank code + 16-digit account number.
    /// </summary>
    public static string RandomIban()
    {
        var country = IbanCountryPrefixes[Random.Shared.Next(IbanCountryPrefixes.Length)];
        var check = Random.Shared.Next(10, 100);
        var bank = Random.Shared.Next(1000, 9999);
        var account = (long)(Random.Shared.NextDouble() * 9_999_999_999_999_999L + 1_000_000_000_000_000L);
        return $"{country}{check:D2}{bank:D4}{account:D16}";
    }

    // --- Kişisel & İletişim ---

    private static readonly string[] Adjectives =
    [
        "fast", "quiet", "bright", "silent", "brave", "dark", "swift", "cold",
        "iron", "lunar", "solar", "wild", "lazy", "happy", "keen", "noble"
    ];

    private static readonly string[] Animals =
    [
        "tiger", "eagle", "wolf", "fox", "bear", "hawk", "lion", "shark",
        "owl", "raven", "cobra", "panda", "lynx", "falcon", "bison", "otter"
    ];

    private static readonly string[] ZipCodes =
    [
        "34100", "06100", "35100", "16100", "01100", "27100", "42100", "07100",
        "10115", "75001", "EC1A", "1010", "28001", "00100", "1000", "110001"
    ];

    public static string RandomUsername()
    {
        var adj = Adjectives[Random.Shared.Next(Adjectives.Length)];
        var animal = Animals[Random.Shared.Next(Animals.Length)];
        var num = Random.Shared.Next(10, 100);
        return $"{adj}_{animal}{num}";
    }

    public static string RandomPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%&*";
        var parts = new[]
        {
            upper[Random.Shared.Next(upper.Length)].ToString(),
            lower[Random.Shared.Next(lower.Length)].ToString(),
            lower[Random.Shared.Next(lower.Length)].ToString(),
            digits[Random.Shared.Next(digits.Length)].ToString(),
            special[Random.Shared.Next(special.Length)].ToString(),
            upper[Random.Shared.Next(upper.Length)].ToString(),
            digits[Random.Shared.Next(digits.Length)].ToString(),
            lower[Random.Shared.Next(lower.Length)].ToString(),
        };
        return string.Concat(parts.OrderBy(_ => Random.Shared.Next()));
    }

    public static int RandomAge() => Random.Shared.Next(18, 81);

    public static string RandomBirthdate()
    {
        var year = DateTime.UtcNow.Year - Random.Shared.Next(18, 70);
        var month = Random.Shared.Next(1, 13);
        var day = Random.Shared.Next(1, 29);
        return new DateTime(year, month, day).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string RandomZipCode() => ZipCodes[Random.Shared.Next(ZipCodes.Length)];

    public static double RandomLatitude() =>
        Math.Round(-90.0 + Random.Shared.NextDouble() * 180.0, 4);

    public static double RandomLongitude() =>
        Math.Round(-180.0 + Random.Shared.NextDouble() * 360.0, 4);

    // --- Finans & Ticaret ---

    private static readonly string[] SwiftCodes =
    [
        "ISBKTRISXXX", "TGBATRISXXX", "AKBKTRISXXX", "YAPITRISXXX", "ZIRAATRIXXX",
        "DEUTDEDBXXX", "CHASGB2LXXX", "BNPAFRPPXXX", "BARCGB22XXX", "CITIUS33XXX",
        "HSBCHKHHXXX", "UBSWCHZHXXX", "INGBNL2AXXX", "BNPANL2AXXX", "BBVAESMM"
    ];

    private static readonly string[] StockSymbols =
    [
        "AAPL", "MSFT", "GOOGL", "AMZN", "META", "TSLA", "NVDA", "BRK.B",
        "JPM", "V", "JNJ", "WMT", "PG", "UNH", "HD", "BAC", "MA", "XOM",
        "THYAO", "GARAN", "AKBNK", "EREGL", "BIMAS", "SASA", "KCHOL"
    ];

    private static readonly string[] TransactionTypes =
    [
        "credit", "debit", "transfer", "payment", "refund", "withdrawal", "deposit"
    ];

    public static string RandomAccountNumber()
    {
        var country = IbanCountryPrefixes[Random.Shared.Next(IbanCountryPrefixes.Length)];
        var digits = string.Concat(Enumerable.Range(0, 22).Select(_ => Random.Shared.Next(10).ToString()));
        return $"{country}{digits}";
    }

    public static string RandomSwiftCode() => SwiftCodes[Random.Shared.Next(SwiftCodes.Length)];

    /// <summary>
    /// Generates a random Luhn-valid 16-digit Visa-style card number (mock data only).
    /// </summary>
    public static string RandomCreditCardNumber()
    {
        var digits = new int[16];
        digits[0] = 4;
        for (var i = 1; i < 15; i++)
            digits[i] = Random.Shared.Next(10);

        // Luhn check digit
        var sum = 0;
        for (var i = 14; i >= 0; i--)
        {
            var d = digits[i];
            if ((14 - i) % 2 == 0) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
        }
        digits[15] = (10 - (sum % 10)) % 10;
        return string.Concat(digits);
    }

    public static string RandomPrice() =>
        (Random.Shared.Next(1, 10000) + Random.Shared.NextDouble()).ToString("F2", CultureInfo.InvariantCulture);

    public static string RandomStockSymbol() => StockSymbols[Random.Shared.Next(StockSymbols.Length)];

    public static string RandomTransactionType() => TransactionTypes[Random.Shared.Next(TransactionTypes.Length)];

    // --- Sistem & Teknik ---

    private static readonly string[] Statuses = ["active", "inactive", "pending", "suspended", "archived"];

    private static readonly int[] HttpStatusCodes = [200, 201, 204, 301, 302, 400, 401, 403, 404, 409, 422, 429, 500, 502, 503];

    private static readonly string[] ColorNames =
    [
        "crimson", "teal", "indigo", "amber", "coral", "slate", "violet", "emerald",
        "rose", "cyan", "lime", "fuchsia", "orange", "sky", "purple", "pink"
    ];

    private static readonly string[] BaseUrls =
    [
        "https://demo.dev", "https://mock.io", "https://api.example.com",
        "https://test.sample.net", "https://service.mock.io"
    ];

    private static readonly string[] UrlPaths =
    [
        "/api/v1/users", "/api/v2/orders", "/api/v1/products", "/api/v1/accounts",
        "/api/v1/transactions", "/api/v2/reports", "/api/v1/notifications", "/health"
    ];

    public static string RandomIp() =>
        $"{Random.Shared.Next(1, 255)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 255)}";

    public static string RandomMacAddress()
    {
        var bytes = new byte[6];
        Random.Shared.NextBytes(bytes);
        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }

    public static string RandomUrl()
    {
        var base_ = BaseUrls[Random.Shared.Next(BaseUrls.Length)];
        var path = UrlPaths[Random.Shared.Next(UrlPaths.Length)];
        return $"{base_}{path}";
    }

    public static string RandomStatus() => Statuses[Random.Shared.Next(Statuses.Length)];

    public static int RandomHttpStatusCode() => HttpStatusCodes[Random.Shared.Next(HttpStatusCodes.Length)];

    public static string RandomColor() => ColorNames[Random.Shared.Next(ColorNames.Length)];

    public static string RandomHexColor() =>
        $"#{Random.Shared.Next(0x1000000):X6}";

    // --- Domain & İş Süreçleri ---

    private static readonly string[] Departments =
    [
        "Engineering", "Product", "Design", "Marketing", "Sales", "Finance",
        "Human Resources", "Legal", "Operations", "Customer Support", "Data Science",
        "DevOps", "Security", "Research", "Strategy"
    ];

    private static readonly string[] Categories =
    [
        "Electronics", "Clothing", "Food & Beverage", "Health & Beauty", "Sports",
        "Automotive", "Home & Garden", "Books", "Toys", "Software", "Financial Services",
        "Travel", "Education", "Entertainment", "Real Estate"
    ];

    private static readonly string[] Roles =
    [
        "admin", "user", "moderator", "editor", "viewer", "manager",
        "developer", "analyst", "support", "guest"
    ];

    private static readonly string[] Priorities = ["low", "medium", "high", "critical"];

    private static readonly string[] TicketStatuses = ["open", "in progress", "resolved", "closed", "on hold"];

    private static readonly string[] OrderStatuses =
    [
        "pending", "confirmed", "processing", "shipped", "delivered", "cancelled", "refunded"
    ];

    private static readonly string[] LanguageCodes =
    [
        "tr", "en", "de", "fr", "es", "it", "pt", "nl", "pl", "ar",
        "zh", "ja", "ko", "ru", "sv", "no", "da", "fi", "cs", "hu"
    ];

    private static readonly string[] Continents =
    [
        "Africa", "Antarctica", "Asia", "Europe", "North America", "Oceania", "South America"
    ];

    private static readonly string[] Timezones =
    [
        "Europe/Istanbul", "Europe/London", "Europe/Berlin", "Europe/Paris", "Europe/Madrid",
        "America/New_York", "America/Chicago", "America/Los_Angeles", "America/Toronto",
        "Asia/Tokyo", "Asia/Singapore", "Asia/Dubai", "Asia/Kolkata", "Asia/Seoul",
        "Australia/Sydney", "Pacific/Auckland", "Africa/Cairo", "America/Sao_Paulo"
    ];

    private static readonly string[] FileExtensions =
    [
        "pdf", "docx", "xlsx", "png", "jpg", "csv", "json", "xml", "zip", "mp4",
        "txt", "svg", "pptx", "mp3", "html", "yaml", "log", "sql"
    ];

    private static readonly string[] MimeTypes =
    [
        "application/json", "application/xml", "text/plain", "text/html", "text/csv",
        "application/pdf", "image/png", "image/jpeg", "image/svg+xml",
        "application/zip", "audio/mpeg", "video/mp4", "application/octet-stream",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];

    public static string RandomDepartment() => Departments[Random.Shared.Next(Departments.Length)];

    public static string RandomCategory() => Categories[Random.Shared.Next(Categories.Length)];

    public static string RandomRole() => Roles[Random.Shared.Next(Roles.Length)];

    public static string RandomPriority() => Priorities[Random.Shared.Next(Priorities.Length)];

    public static string RandomTicketStatus() => TicketStatuses[Random.Shared.Next(TicketStatuses.Length)];

    public static string RandomOrderStatus() => OrderStatuses[Random.Shared.Next(OrderStatuses.Length)];

    public static string RandomLanguageCode() => LanguageCodes[Random.Shared.Next(LanguageCodes.Length)];

    public static string RandomContinent() => Continents[Random.Shared.Next(Continents.Length)];

    public static string RandomTimezone() => Timezones[Random.Shared.Next(Timezones.Length)];

    public static string RandomFileExtension() => FileExtensions[Random.Shared.Next(FileExtensions.Length)];

    public static string RandomMimeType() => MimeTypes[Random.Shared.Next(MimeTypes.Length)];

    // ── Arithmetic helpers ──────────────────────────────────────────────────

    public static double Add(double a, double b) => a + b;
    public static double Subtract(double a, double b) => a - b;
    public static double Multiply(double a, double b) => a * b;
    public static double Divide(double a, double b) => b == 0 ? 0 : a / b;

    // ── Date/time helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Returns current UTC date-time as a formatted string.
    /// <paramref name="format"/> defaults to ISO 8601 when omitted.
    /// Example: {{ now "yyyy-MM-dd" }}
    /// </summary>
    public static string NowFormatted(string? format = null) =>
        DateTime.UtcNow.ToString(format ?? "O", CultureInfo.InvariantCulture);

    private static DateTime ShiftDateTime(DateTime dt, double amount, string unit) =>
        unit.ToLowerInvariant() switch
        {
            "years"   or "year"   => dt.AddYears((int)amount),
            "months"  or "month"  => dt.AddMonths((int)amount),
            "weeks"   or "week"   => dt.AddDays(amount * 7),
            "days"    or "day"    => dt.AddDays(amount),
            "hours"   or "hour"   => dt.AddHours(amount),
            "minutes" or "minute" => dt.AddMinutes(amount),
            "seconds" or "second" => dt.AddSeconds(amount),
            _ => dt
        };

    /// <summary>
    /// Adds <paramref name="amount"/> of <paramref name="unit"/> to a DateTime and returns an ISO 8601 string.
    /// Supported units: years, months, weeks, days, hours, minutes, seconds.
    /// Example: {{ date_time_add (now) 1 'days' }}
    /// Use date_time_add_fmt to control the output format.
    /// </summary>
    public static string DateTimeAdd(DateTime dt, double amount, string unit) =>
        ShiftDateTime(dt, amount, unit).ToString("O", CultureInfo.InvariantCulture);

    /// <summary>
    /// Adds <paramref name="amount"/> of <paramref name="unit"/> to a DateTime and returns a formatted string.
    /// This overload avoids the need to nest date_format inside date_time_add.
    /// Example: {{ date_time_add_fmt (now) 1 "days" "yyyy-MM-dd" }}
    /// </summary>
    public static string DateTimeAddFmt(DateTime dt, double amount, string unit, string? format = null) =>
        ShiftDateTime(dt, amount, unit).ToString(format ?? "O", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a DateTime to a string.
    /// Example: {{ date_format someDateTime "yyyy-MM-dd" }}
    /// </summary>
    public static string DateFormat(DateTime dt, string? format = null) =>
        dt.ToString(format ?? "O", CultureInfo.InvariantCulture);

    // ── Faker-style helper ──────────────────────────────────────────────────

    /// <summary>
    /// faker helper. Supports a subset of common categories.
    /// Usage: {{ faker "number.int" 1 100 }}  {{ faker "number.float" 0.5 5.0 2 }}
    ///        {{ faker "person.firstName" }}    {{ faker "internet.email" }}
    ///        {{ faker "date.future" }}         {{ faker "lorem.word" }}
    /// </summary>
    public static object Faker(string category, params object[] args)
    {
        return category.ToLowerInvariant() switch
        {
            "number.int"    or "number.integer"   => FakerNumberInt(args),
            "number.float"  or "number.decimal"   => FakerNumberFloat(args),
            "number.binary"                        => Random.Shared.Next(2),
            "number.hex"                           => Random.Shared.Next(256).ToString("X2"),
            "person.firstname" or "name.firstname" => RandomFirstName(),
            "person.lastname"  or "name.lastname"  => RandomLastName(),
            "person.fullname"  or "name.fullname"
                or "person.name" or "name.name"    => RandomName(),
            "internet.email" or "person.email"
                or "name.email"                    => RandomEmail(),
            "internet.username"                    => RandomUsername(),
            "internet.url"                         => RandomUrl(),
            "internet.ip"    or "internet.ipv4"    => RandomIp(),
            "internet.mac"                         => RandomMacAddress(),
            "internet.color"                       => RandomHexColor(),
            "date.past"      or "date.recent"      => DateTimeAdd(DateTime.UtcNow, -Random.Shared.Next(1, 365), "days"),
            "date.future"                          => DateTimeAdd(DateTime.UtcNow, Random.Shared.Next(1, 365), "days"),
            "date.birthdate" or "person.birthdate" => RandomBirthdate(),
            "location.city"  or "address.city"     => RandomCity(),
            "location.country" or "address.country" => RandomCountry(),
            "location.latitude"  or "address.latitude"  => RandomLatitude(),
            "location.longitude" or "address.longitude" => RandomLongitude(),
            "location.zipcode"   or "address.zipcode"
                or "address.zipCode"               => RandomZipCode(),
            "location.streetaddress" or "address.streetaddress"
                or "address.streetAddress"         => RandomAddress(),
            "finance.iban"                         => RandomIban(),
            "finance.bic"    or "finance.swift"    => RandomSwiftCode(),
            "finance.currencycode" or "finance.currency" => RandomCurrencyCode(),
            "finance.creditcardnumber"             => RandomCreditCardNumber(),
            "finance.accountnumber"                => RandomAccountNumber(),
            "finance.amount" or "commerce.price"   => RandomPrice(),
            "company.name"   or "company.companyname" => RandomCompanyName(),
            "commerce.productname" or "commerce.product" => RandomProductName(),
            "string.uuid"    or "datatype.uuid"    => Guid(),
            "string.alpha"   or "string.alphanumeric" =>
                RandomAlphaNumeric(args.Length > 0 && int.TryParse(args[0]?.ToString(), out var al) ? al : 8),
            "lorem.word"     or "lorem.words"      => RandomString(Random.Shared.Next(4, 10)),
            "phone.number"   or "phone.phonenumber" => RandomPhone(),
            "system.mimetype" or "system.mimeType" => RandomMimeType(),
            "system.fileext" or "system.fileExt"
                or "system.fileextension"          => RandomFileExtension(),
            _ => $"[faker:{category}]"
        };
    }

    private static object FakerNumberInt(object[] args)
    {
        var min = args.Length > 0 && double.TryParse(args[0]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v0) ? (int)v0 : 0;
        var max = args.Length > 1 && double.TryParse(args[1]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v1) ? (int)v1 : 1000;
        return Random.Shared.Next(min, max + 1);
    }

    private static object FakerNumberFloat(object[] args)
    {
        var min = args.Length > 0 && double.TryParse(args[0]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v0) ? v0 : 0.0;
        var max = args.Length > 1 && double.TryParse(args[1]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v1) ? v1 : 1.0;
        var decimals = args.Length > 2 && int.TryParse(args[2]?.ToString(), out var d) ? d : 2;
        var value = min + (Random.Shared.NextDouble() * (max - min));
        return Math.Round(value, decimals);
    }

    // ── Body field shorthand ────────────────────────────────────────────────

    /// <summary>
    /// Extracts a top-level field from a JSON body string by key name.
    /// This is a convenience helper for templates that receive body as a raw string.
    /// Equivalent to request.body.fieldName when body is already parsed.
    /// </summary>
    public static object? BodyField(string? jsonBody, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(jsonBody) || string.IsNullOrWhiteSpace(fieldName))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(jsonBody);
            if (doc.RootElement.TryGetProperty(fieldName, out var el))
            {
                return el.ValueKind switch
                {
                    JsonValueKind.Number when el.TryGetInt64(out var i) => i,
                    JsonValueKind.Number => el.GetDouble(),
                    JsonValueKind.True   => true,
                    JsonValueKind.False  => false,
                    JsonValueKind.Null   => null,
                    _                    => el.GetRawText().Trim('"')
                };
            }
        }
        catch { /* ignore parse errors */ }
        return null;
    }
}
