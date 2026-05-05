namespace Ideogram.Client.ConsoleApp;

internal static class SimpleArgsParser
{
    public static ManualRunOptions? Parse(string[] args, out string? error)
    {
        error = null;

        if (args.Length == 0)
        {
            return null;
        }

        if (args.Length == 1 && IsHelpToken(args[0]))
        {
            return new ManualRunOptions { ShowHelp = true };
        }

        var offset = 0;
        var commandName = string.Empty;
        if (!args[0].StartsWith("--", StringComparison.Ordinal))
        {
            commandName = args[0].Trim();
            offset = 1;
        }

        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? apiKey = null;
        bool? download = null;

        while (offset < args.Length)
        {
            var token = args[offset];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                error = $"Unexpected argument '{token}'.";
                return null;
            }

            var key = token[2..];
            if (offset + 1 >= args.Length)
            {
                error = $"Missing value for '--{key}'.";
                return null;
            }

            var value = args[offset + 1];
            offset += 2;

            if (string.Equals(key, "help", StringComparison.OrdinalIgnoreCase))
            {
                return new ManualRunOptions { ShowHelp = true };
            }

            if (string.Equals(key, "api-key", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = value;
                continue;
            }

            if (string.Equals(key, "download", StringComparison.OrdinalIgnoreCase))
            {
                if (!bool.TryParse(value, out var parsedBool))
                {
                    error = "Value for '--download' must be true or false.";
                    return null;
                }

                download = parsedBool;
                continue;
            }

            arguments[key] = value;
        }

        return new ManualRunOptions
        {
            ApiKey = apiKey,
            CommandName = string.IsNullOrWhiteSpace(commandName) ? null : commandName,
            Arguments = arguments,
            Download = download
        };
    }

    public static string? GetValue(ManualRunOptions options, string key)
    {
        return options.Arguments.TryGetValue(key, out var value) ? value : null;
    }

    private static bool IsHelpToken(string value)
    {
        return string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "help", StringComparison.OrdinalIgnoreCase);
    }
}
