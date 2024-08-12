package tech.pixelprecision.midibridge;
import java.util.Arrays;

public class OCMidiMsg
{
    public byte[] bytes;
    public long timestamp;

    public OCMidiMsg(byte [] origbytes, int offset, int count, long timestamp)
    {
        this.bytes = Arrays.copyOfRange(origbytes, offset, offset + count);
        this.timestamp = timestamp;
    }
}
