namespace A2G.Ideogram.Client.ConsoleApp;

internal static class ConsolePrompts
{
    public static string RequiredString(string label)
    {
        while (true)
        {
            System.Console.Write($"{label}: ");
            var value = System.Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            System.Console.WriteLine("Value is required.");
        }
    }

    public static string? OptionalString(string label, string? defaultValue = null)
    {
        var prompt = defaultValue is null ? $"{label}: " : $"{label} [{defaultValue}]: ";
        System.Console.Write(prompt);
        var value = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Trim();
    }

    public static int? OptionalInt(string label, int? defaultValue = null, int? min = null, int? max = null)
    {
        while (true)
        {
            var prompt = defaultValue.HasValue ? $"{label} [{defaultValue.Value}]: " : $"{label}: ";
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (!int.TryParse(input.Trim(), out var value))
            {
                System.Console.WriteLine("Enter a valid integer or leave blank.");
                continue;
            }

            if (min.HasValue && value < min.Value)
            {
                System.Console.WriteLine($"Value must be >= {min.Value}.");
                continue;
            }

            if (max.HasValue && value > max.Value)
            {
                System.Console.WriteLine($"Value must be <= {max.Value}.");
                continue;
            }

            return value;
        }
    }

    public static bool Confirm(string label, bool defaultValue = true)
    {
        var prompt = defaultValue ? $"{label} [Y/n]: " : $"{label} [y/N]: ";

        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            switch (input.Trim().ToLowerInvariant())
            {
                case "y":
                case "yes":
                    return true;
                case "n":
                case "no":
                    return false;
                default:
                    System.Console.WriteLine("Enter y or n.");
                    break;
            }
        }
    }

    public static string? OptionalPath(string label)
    {
        var value = OptionalString(label);
        return string.IsNullOrWhiteSpace(value) ? null : TrimOuterQuotes(value);
    }

    public static IdeogramFile RequiredImageFile(string label)
    {
        while (true)
        {
            var path = RequiredString(label);

            try
            {
                return IdeogramFile.FromPath(TrimOuterQuotes(path));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }

    public static IReadOnlyList<IdeogramFile>? OptionalImageFiles(string label)
    {
        while (true)
        {
            System.Console.Write($"{label}, separated by semicolon, blank to skip: ");
            var input = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            try
            {
                return input
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(TrimOuterQuotes)
                    .Select(static path => IdeogramFile.FromPath(path))
                    .ToArray();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }

    public static IReadOnlyList<string>? OptionalStringList(string label)
    {
        System.Console.Write($"{label}, separated by semicolon, blank to skip: ");
        var input = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var values = input
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return values.Length == 0 ? null : values;
    }

    public static string ReadSecret(string prompt)
    {
        System.Console.Write(prompt);
        var chars = new List<char>();

        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (chars.Count > 0)
                {
                    chars.RemoveAt(chars.Count - 1);
                    System.Console.Write("\b \b");
                }

                continue;
            }

            chars.Add(key.KeyChar);
            System.Console.Write("*");
        }

        return new string(chars.ToArray());
    }

    private static string TrimOuterQuotes(string value)
    {
        return value.Trim().Trim('"');
    }
}
