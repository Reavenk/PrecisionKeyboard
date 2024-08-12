package tech.pixelprecision.midibridge;
import android.media.midi.MidiReceiver;

import java.io.IOException;
import java.util.*;

public class OutputConnection extends MidiReceiver
{
    public Queue<OCMidiMsg> entries = new LinkedList<OCMidiMsg>();
    private boolean open = true;

    public void seal()
    {
        synchronized (this.entries)
        {
            this.entries.clear();
            this.open = false;
        }
    }

    public int getCount()
    {
        synchronized (this.entries)
        {
            if(this.open == false)
                return 0;

            return this.entries.size();
        }
    }

    public boolean hasMessages()
    {
        synchronized (this.entries)
        {
            if(this.open == false)
                return false;

            return this.entries.size() != 0;
        }
    }

    public OCMidiMsg popMessage()
    {
        synchronized (this.entries)
        {
            if(this.open == false)
                return null;

            return this.entries.poll();
        }
    }

    @Override
    public void onSend(byte[] bytes, int offset, int count, long timestamp) throws IOException
    {
        synchronized (this.entries)
        {
            if(this.open == false)
                return;

            this.entries.add(new OCMidiMsg(bytes, offset, count, timestamp));
        }
    }
}
