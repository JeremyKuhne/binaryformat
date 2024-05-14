using BenchmarkDotNet.Attributes;
using BinaryFormat;
using FormatTests.Legacy;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace performance;

[MemoryDiagnoser]
public class DeserializeCorpus
{
    private IEnumerable<MemoryStream>? _formattedData;

    private static readonly BinaryFormatter s_formatter = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);

        _formattedData =
        (
            from value in BinaryFormatterTests.RawSerializableObjects()
            from FormatterAssemblyStyle assemblyFormat in new[] { FormatterAssemblyStyle.Full, FormatterAssemblyStyle.Simple }
            from FormatterTypeStyle typeFormat in new[] { FormatterTypeStyle.TypesAlways, FormatterTypeStyle.TypesWhenNeeded, FormatterTypeStyle.XsdString }
            where value.Object is not Array array || array.GetLowerBound(0) == 0
            select Serialize(value.Object, assemblyFormat, typeFormat)
        ).ToArray();

        static MemoryStream Serialize(object value, FormatterAssemblyStyle assemblyFormat, FormatterTypeStyle typeFormat)
        {
            s_formatter.AssemblyFormat = assemblyFormat;
            s_formatter.TypeFormat = typeFormat;
            MemoryStream stream = new();
            s_formatter.Serialize(stream, value);
            stream.Position = 0;
            return stream;
        }
    }

    [Benchmark(Baseline = true)]
    public bool Deserialize_BinaryFormatter()
    {
        bool result = true;

        foreach (MemoryStream stream in _formattedData!)
        {
            result |= s_formatter.Deserialize(stream) is null;
            stream.Position = 0;
        }

        return result;
    }

    [Benchmark]
    public bool Deserialize_BinaryFormattedObject()
    {
        bool result = true;

        foreach (MemoryStream stream in _formattedData!)
        {
            BinaryFormattedObject format = new(stream);
            result |= format.Deserialize() is null;
            stream.Position = 0;
        }

        return result;

    }
}
