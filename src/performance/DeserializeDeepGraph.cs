using BenchmarkDotNet.Attributes;
using BinaryFormat;
using FormatTests.Common.TestTypes;
using System.Runtime.Serialization.Formatters.Binary;

namespace performance;

[MemoryDiagnoser]
public class DeserializeDeepGraph
{
    private static readonly BinaryFormatter s_formatter = new();

    private MemoryStream? _stream;

    [Params(10, 100, 1000, 10000)]
    public int Depth;

    [GlobalSetup]
    public void GlobalSetup()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);

        SimpleNode root = new();
        SimpleNode current = root;
        for (int i = 1; i < Depth; i++)
        {
            current.Next = new();
            current = current.Next;
        }

        _stream = new();
        s_formatter.Serialize(_stream, current);
        _stream.Position = 0;
    }

    [Benchmark(Baseline = true)]
    public bool Deserialize_BinaryFormatter()
    {
        bool result = true;

        result |= s_formatter.Deserialize(_stream!) is null;
        _stream!.Position = 0;

        return result;
    }

    [Benchmark]
    public bool Deserialize_BinaryFormattedObject()
    {
        bool result = true;

        BinaryFormattedObject format = new(_stream!);
        result |= format.Deserialize() is null;
        _stream!.Position = 0;

        return result;

    }
}
