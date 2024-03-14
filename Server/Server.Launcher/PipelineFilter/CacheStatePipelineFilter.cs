using System.Buffers;
using Server.DBServer.State;
using Server.NetWork.Messages;
using SuperSocket.ProtoBase;

namespace Server.Launcher.PipelineFilter;

public class CacheStatePipelineFilter : IPipelineFilter<ICacheState>
{
    public void Reset()
    {
    }

    public object? Context { get; set; }

    public ICacheState Filter(ref SequenceReader<byte> reader)
    {
        ReadOnlySequence<byte> buffer = reader.Sequence;
        reader.Advance(buffer.Length);
        return this.Decoder.Decode(ref buffer, this.Context);
    }

    public IPackageDecoder<ICacheState> Decoder { get; set; }
    public IPipelineFilter<ICacheState> NextFilter { get; }
}