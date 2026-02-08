using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    PrintHelp();
    return;
}

var options = CliOptions.Parse(args);

if (options.WriteDefaultMapPath is not null)
{
    var json = JsonSerializer.Serialize(TransliterationMap.Default, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(options.WriteDefaultMapPath, json, Encoding.UTF8);
    Console.WriteLine($"Default map written to: {options.WriteDefaultMapPath}");

    if (options.InputPath is null)
    {
        return;
    }
}

if (options.InputPath is null || options.OutputPath is null)
{
    Console.Error.WriteLine("Error: input and output paths are required unless only writing default map.");
    PrintHelp();
    Environment.ExitCode = 1;
    return;
}

var map = TransliterationMap.Default;
if (options.MapPath is not null)
{
    map = await TransliterationMap.LoadAsync(options.MapPath);
}

var inputLines = await File.ReadAllLinesAsync(options.InputPath, Encoding.UTF8);
var outputLines = new string[inputLines.Length];

for (var i = 0; i < inputLines.Length; i++)
{
    outputLines[i] = ProcessLine(inputLines[i], map);
}

await File.WriteAllLinesAsync(options.OutputPath, outputLines, Encoding.UTF8);
Console.WriteLine($"Transliteration completed: {options.OutputPath}");

return;

static string ProcessLine(string line, IReadOnlyDictionary<char, string> map)
{
    // Skip metadata tag lines like [by:...], [id:...], [ti:...], etc.
    if (Regex.IsMatch(line, @"^\[[A-Za-z]+:.*\]$"))
    {
        return line;
    }

    var sb = new StringBuilder(line.Length * 2);
    foreach (var ch in line)
    {
        if (map.TryGetValue(ch, out var replacement))
        {
            sb.Append(replacement);
        }
        else
        {
            sb.Append(ch);
        }
    }

    return sb.ToString();
}

static void PrintHelp()
{
    Console.WriteLine("""
RusLatinizeHelper (.NET 10)

Usage:
  dotnet run -- <input.lrc> <output.lrc> [--map <map.json>] [--write-default-map <path>]

Options:
  --map <path>               Use a custom 33-letter transliteration map in JSON.
  --write-default-map <path> Export default map JSON so you can edit it.
  -h, --help                 Show help.

Map format example:
{
  "А": "A",
  "Б": "B",
  "В": "V",
  "Г": "G",
  "Д": "D",
  "Е": "E",
  "Ё": "Yo",
  "Ж": "Zh",
  "З": "Z",
  "И": "I",
  "Й": "Y",
  "К": "K",
  "Л": "L",
  "М": "M",
  "Н": "N",
  "О": "O",
  "П": "P",
  "Р": "R",
  "С": "S",
  "Т": "T",
  "У": "U",
  "Ф": "F",
  "Х": "Kh",
  "Ц": "Ts",
  "Ч": "Ch",
  "Ш": "Sh",
  "Щ": "Shch",
  "Ъ": "",
  "Ы": "Y",
  "Ь": "",
  "Э": "E",
  "Ю": "Yu",
  "Я": "Ya",
  "а": "a",
  "б": "b",
  "в": "v",
  "г": "g",
  "д": "d",
  "е": "e",
  "ё": "yo",
  "ж": "zh",
  "з": "z",
  "и": "i",
  "й": "y",
  "к": "k",
  "л": "l",
  "м": "m",
  "н": "n",
  "о": "o",
  "п": "p",
  "р": "r",
  "с": "s",
  "т": "t",
  "у": "u",
  "ф": "f",
  "х": "kh",
  "ц": "ts",
  "ч": "ch",
  "ш": "sh",
  "щ": "shch",
  "ъ": "",
  "ы": "y",
  "ь": "",
  "э": "e",
  "ю": "yu",
  "я": "ya"
}
""");
}

internal sealed record CliOptions(string? InputPath, string? OutputPath, string? MapPath, string? WriteDefaultMapPath)
{
    public static CliOptions Parse(string[] args)
    {
        string? input = null;
        string? output = null;
        string? map = null;
        string? writeDefaultMap = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--map":
                    map = RequireValue(args, ref i, "--map");
                    break;
                case "--write-default-map":
                    writeDefaultMap = RequireValue(args, ref i, "--write-default-map");
                    break;
                default:
                    if (args[i].StartsWith('-'))
                    {
                        throw new ArgumentException($"Unknown option: {args[i]}");
                    }

                    if (input is null)
                    {
                        input = args[i];
                    }
                    else if (output is null)
                    {
                        output = args[i];
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected argument: {args[i]}");
                    }
                    break;
            }
        }

        return new CliOptions(input, output, map, writeDefaultMap);
    }

    private static string RequireValue(string[] args, ref int i, string option)
    {
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"Option {option} requires a value.");
        }

        i++;
        return args[i];
    }
}

internal static class TransliterationMap
{
    public static readonly IReadOnlyDictionary<char, string> Default = new Dictionary<char, string>
    {
        ['А'] = "A", ['Б'] = "B", ['В'] = "V", ['Г'] = "G", ['Д'] = "D", ['Е'] = "E", ['Ё'] = "Yo", ['Ж'] = "Zh", ['З'] = "Z", ['И'] = "I", ['Й'] = "Y", ['К'] = "K", ['Л'] = "L", ['М'] = "M", ['Н'] = "N", ['О'] = "O", ['П'] = "P", ['Р'] = "R", ['С'] = "S", ['Т'] = "T", ['У'] = "U", ['Ф'] = "F", ['Х'] = "Kh", ['Ц'] = "Ts", ['Ч'] = "Ch", ['Ш'] = "Sh", ['Щ'] = "Shch", ['Ъ'] = "", ['Ы'] = "Y", ['Ь'] = "", ['Э'] = "E", ['Ю'] = "Yu", ['Я'] = "Ya",
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "yo", ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch", ['ъ'] = "", ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
    };

    public static async Task<IReadOnlyDictionary<char, string>> LoadAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                  ?? throw new InvalidOperationException("Map JSON is empty or invalid.");

        var map = new Dictionary<char, string>();
        foreach (var pair in raw)
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Key.Length != 1)
            {
                throw new InvalidOperationException($"Invalid key '{pair.Key}'. Each key must be exactly one character.");
            }

            map[pair.Key[0]] = pair.Value ?? string.Empty;
        }

        return map;
    }
}
