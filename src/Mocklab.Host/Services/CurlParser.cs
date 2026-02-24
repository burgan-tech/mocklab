namespace Mocklab.Host.Services;

/// <summary>
/// Parses a cURL command string into its components (method, URL, headers, body).
/// </summary>
public class CurlParseResult
{
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
}

public static class CurlParser
{
    /// <summary>
    /// Parse a cURL command string into method, URL, headers, and body.
    /// </summary>
    public static CurlParseResult Parse(string curlCommand)
    {
        if (string.IsNullOrWhiteSpace(curlCommand))
            throw new ArgumentException("cURL command cannot be empty.");

        var result = new CurlParseResult();

        // Normalize: remove line continuations (backslash + newline) and trim
        var normalized = curlCommand
            .Replace("\\\n", " ")
            .Replace("\\\r\n", " ")
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Trim();

        // Remove leading "curl" if present
        if (normalized.StartsWith("curl ", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[5..].TrimStart();
        }

        var tokens = Tokenize(normalized);

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            switch (token)
            {
                case "-X" or "--request":
                    if (i + 1 < tokens.Count)
                        result.Method = tokens[++i].ToUpperInvariant();
                    break;

                case "-H" or "--header":
                    if (i + 1 < tokens.Count)
                    {
                        var headerValue = tokens[++i];
                        var colonIdx = headerValue.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            var key = headerValue[..colonIdx].Trim();
                            var val = headerValue[(colonIdx + 1)..].Trim();
                            result.Headers[key] = val;
                        }
                    }
                    break;

                case "-d" or "--data" or "--data-raw" or "--data-binary" or "--data-ascii":
                    if (i + 1 < tokens.Count)
                        result.Body = tokens[++i];
                    break;

                case "-b" or "--cookie":
                    if (i + 1 < tokens.Count)
                        result.Headers["Cookie"] = tokens[++i];
                    break;

                case "-A" or "--user-agent":
                    if (i + 1 < tokens.Count)
                        result.Headers["User-Agent"] = tokens[++i];
                    break;

                case "-e" or "--referer":
                    if (i + 1 < tokens.Count)
                        result.Headers["Referer"] = tokens[++i];
                    break;

                case "--compressed" or "-s" or "--silent" or "-S" or "--show-error"
                    or "-k" or "--insecure" or "-L" or "--location" or "-v" or "--verbose"
                    or "-i" or "--include":
                    // Skip boolean flags (no argument)
                    break;

                case "-o" or "--output" or "-u" or "--user" or "--connect-timeout"
                    or "-m" or "--max-time" or "--retry":
                    // Skip flags with one argument
                    if (i + 1 < tokens.Count) i++;
                    break;

                default:
                    // If token looks like a URL, capture it
                    if (token.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        token.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Url = token;
                    }
                    else if (string.IsNullOrEmpty(result.Url) && !token.StartsWith("-"))
                    {
                        // Could be a URL without scheme (unlikely but handle gracefully)
                        result.Url = token;
                    }
                    break;
            }
        }

        // If body exists but method is still GET, switch to POST
        if (result.Body != null && result.Method == "GET")
        {
            result.Method = "POST";
        }

        if (string.IsNullOrEmpty(result.Url))
        {
            throw new ArgumentException("No URL found in cURL command.");
        }

        return result;
    }

    /// <summary>
    /// Tokenize a command string, respecting single and double quotes.
    /// </summary>
    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        char? quoteChar = null;
        bool escaped = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (quoteChar.HasValue)
            {
                if (c == quoteChar.Value)
                {
                    quoteChar = null; // End quote
                }
                else
                {
                    current.Append(c);
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                quoteChar = c;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
