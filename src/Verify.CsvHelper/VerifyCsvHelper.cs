namespace VerifyTests;

public static partial class VerifyCsvHelper
{
    public static bool Initialized { get; private set; }

    static CsvConfiguration config = new(CultureInfo.InvariantCulture)
    {
        NewLine = "\n"
    };

    public static void Initialize()
    {
        if (Initialized)
        {
            throw new("Already Initialized");
        }

        Initialized = true;

        InnerVerifier.ThrowIfVerifyHasBeenRun();
        VerifierSettings.AddScrubber("csv", Handle);
    }

    static void Handle(StringBuilder builder, Counter counter, IReadOnlyDictionary<string, object> context)
    {
        using var reader = new StringReader(builder.ToString());
        using var csvReader = new CsvReader(reader, config);
        builder.Clear();
        using var writer = new StringWriter(builder);
        using var csvWriter = new CsvWriter(writer, config);
        csvReader.Read();
        var columns = GetColumns(csvReader, context).ToList();

        foreach (var header in columns)
        {
            csvWriter.WriteField(header.column);
        }

        csvWriter.NextRecord();
        while (csvReader.Read())
        {
            foreach (var (column, translate) in columns)
            {
                var sourceCell = csvReader.GetField(column);
                csvWriter.WriteField(translate(sourceCell));
            }

            csvWriter.NextRecord();
        }
    }

    static IEnumerable<(string column, Func<string?, string?> translate)> GetColumns(CsvReader reader, IReadOnlyDictionary<string, object> context)
    {
        reader.ReadHeader();
        var translateBuilder = GetTranslate(context);

        var ignoreColumns = context.GetIgnoreCsvColumns();
        var scrubColumns = context.GetScrubCsvColumns();

        var columns = reader.HeaderRecord!;

        foreach (var column in columns.Except(ignoreColumns))
        {
            var handle = GetHandle(scrubColumns, column, translateBuilder);

            yield return (column, handle);
        }
    }

    static Func<string?, string?> translateScrubbed = _ => "{Scrubbed}";

    static Func<string?, string?> defaultHandle = _ => _;

    static Func<string?, string?> GetHandle(string[] scrubColumns, string column, Func<string, Func<string?, string?>?>? translateBuilder)
    {
        if (scrubColumns.Contains(column))
        {
            return translateScrubbed;
        }

        var translate = translateBuilder?.Invoke(column);
        if (translate == null)
        {
            return defaultHandle;
        }

        return _ => translate(_) ?? "null";
    }
}