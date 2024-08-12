using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagnosticScene : 
    MonoBehaviour, 
    PxPre.AndWrap.IReporter
{
    static public DiagnosticScene instance = null;

    AndroidJavaObject androidMidi;
    AndroidJavaObject devices;

    PxPre.AndWrap.MidiInputPort mip = null;

    string log = "";

    public void Log(string str)
    { 
        this.log += str + "\n";
    }

    public static void InstLog(string str)
    { 
        if(instance != null)
            instance.Log(str);
    }

    private void Awake()
    {
        instance = this; 
    }
    OutputConnection oc = null;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        try
        {
            AndroidJavaObject ajcMidi = new AndroidJavaObject("tech.pixelprecision.midibridge.OutputConnection");
            if(ajcMidi != null)
                this.Log("Found the dangold class!");

            this.oc = new OutputConnection(ajcMidi, this);
            Debug.Log($"Max msg size {oc.getMaxMessageSize()}");

            //AndroidJavaObject ajo = devices.Call<AndroidJavaObject>("getClass").Call<AndroidJavaObject>("getFields");
            //this.log += ajo.Call<AndroidJavaObject>("getClass").Call<string>("getName") + "\n";
            //
            //AndroidJavaClass arrayClass  = new AndroidJavaClass("java.lang.reflect.Array");
            //AndroidJavaObject ajGet = arrayClass.CallStatic<AndroidJavaObject>("get", devices, 0);
            //this.log += ajGet.Call<AndroidJavaObject>("getClass").Call<string>("getName") + "\n";

            //AndroidJavaObject ajcFields = new AndroidJavaClass("com.unity3d.player.UnityPlayer").Call<AndroidJavaObject>("getFields");
            //Log += ajcFields.Call<AndroidJavaObject>("getClass").CallStatic<string>("getName") + "\n";

            //this.log += arrayLen.ToString() + "   !\n";
            //
            //this.log += arrayClass.CallStatic<AndroidJavaObject>("length", devices, 0) + " !!";

            PxPre.AndWrap.MidiManager midiMgr = new PxPre.AndWrap.MidiManager(null);
            List<PxPre.AndWrap.MidiDeviceInfo> lstDevices = midiMgr.GetDevices();

            this.Log("Log list of " + lstDevices.Count.ToString());
            foreach(PxPre.AndWrap.MidiDeviceInfo mdi in lstDevices)
            { 
                this.Log("======================");
                this.Log($"Input port count {mdi.getInputPortCount()}");
                this.Log($"Output port count {mdi.getOutputPortCount()}");
                this.Log($"STRING: {mdi.toString()}");

                List<PxPre.AndWrap.PortInfo> lstPort = mdi.getPorts();
                this.Log($"Port count {lstPort.Count}");

                PxPre.AndWrap.Bundle bundle = mdi.getProperties();
                this.Log( "MANUFACTURE - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_MANUFACTURER));
                this.Log( "NAME - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_NAME));
                this.Log( "PRODUCT - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_PRODUCT));
                this.Log( "SERIAL - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_SERIAL_NUMBER));
                this.Log( "USB - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_USB_DEVICE));
                this.Log( "VERSION - " + bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_VERSION));

                foreach(PxPre.AndWrap.PortInfo pi in lstPort)
                { 
                    this.Log("----------");
                    this.Log("Port Name: " + pi.getName());
                    this.Log("Port Number: " + pi.getPortNumber());
                    this.Log("Port Type: " + pi.getType());
                    
                }
            }

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call(
                "runOnUiThread", 
                new AndroidJavaRunnable(
                    () =>
                    {
                        midiMgr.openDevice( 
                            lstDevices[0], 
                            new TestODOL(this, this.oc),
                            new PxPre.AndWrap.Handler(null, null));
                    }));

        }
        catch(System.Exception ex)
        { 
            this.Log(ex.Message);
        }
    }

    class TestODOL : PxPre.AndWrap.OnDeviceOpenedListener
    { 
        DiagnosticScene ds;
        OutputConnection oc;

        public TestODOL(DiagnosticScene ds, OutputConnection oc)
        { 
            this.ds = ds;
            this.oc = oc;
        }

        public override void _impl_OnDeviceOpened(PxPre.AndWrap.MidiDevice device)
        { 
            DiagnosticScene.InstLog("_impl_OnDeviceOpened");

            if(device._ajo == null)
            { 
            }
            else
            { 
                //this.ds.mip = device.openInputPort(0);

                PxPre.AndWrap.MidiOutputPort outputPort = device.openOutputPort(0);
                outputPort.connect(oc);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(this.oc != null)
        { 
            if(this.oc.hasMessages() == true)
            { 
                this.Log("Has messages!");
                int msgct = this.oc.getCount();
                this.Log(msgct.ToString());

                for(int i = 0; i < msgct; ++i)
                {
                    OCMidiMsg omm = this.oc.popMessage();

                    byte [] rb = omm.bytes;
                    this.Log(" size = " + rb.Length.ToString());

                    int msg = (rb[0] & 0xF0) >> 4;
                    int channel = (rb[0] & 0x0F);

                    string str = "";
                    foreach(sbyte b in rb)
                        str += b.ToString() + " | ";

                    this.Log($"{str}      MSG:{msg}     CHAN:{channel}");
                }
            }
        }
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Clear") == true)
            this.log = string.Empty;

        GUILayout.Space(100.0f);
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(100.0f);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label($"DPI : {Screen.dpi}");
                GUILayout.Label($"Width : {Screen.width}");
                GUILayout.Label($"Height : {Screen.height}");
                GUILayout.Label($"Resolution : {Screen.currentResolution.width}, {Screen.currentResolution.height}");
                GUILayout.Label($"Has Vibrator : {Vibrator.HasVibrator()}");
                GUILayout.Space(20.0f);

                GUILayout.Label(this.log, GUILayout.ExpandWidth(true));

                GUILayout.Space(20.0f);
                if(GUILayout.Button("Go Back") == true)
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Calendar", GUILayout.Height(50), GUILayout.ExpandWidth(true)))
        {
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call(
                "runOnUiThread", 
                new AndroidJavaRunnable(
                    () =>
                    {
                        new AndroidJavaObject(
                            "android.app.DatePickerDialog", 
                            activity, 
                            new DateCallback(), 
                            selectedDate.Year, 
                            selectedDate.Month - 1, selectedDate.Day).Call("show");
                    }));
        }
        if (GUILayout.Button("MIDI", GUILayout.Height(50), GUILayout.ExpandWidth(true)))
        {
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call(
                "runOnUiThread", 
                new AndroidJavaRunnable(
                    () =>
                    {
                        PxPre.AndWrap.MidiManager midiMgr = new PxPre.AndWrap.MidiManager(this);
                        List<PxPre.AndWrap.MidiDeviceInfo> lstDevices = midiMgr.GetDevices();

                        midiMgr.openDevice( 
                            lstDevices[0], 
                            new TestODOL(this, this.oc),
                            new PxPre.AndWrap.Handler(null, null));
                    }));
        }

        if(this.mip != null)
        { 
            GUILayout.Label("ITS ALIVE!");

            if(GUILayout.Button("On", GUILayout.Height(70.0f)) == true)
            { 
                sbyte[] buffer = new sbyte[32];
                int numBytes = 0;
                int channel = 3; // MIDI channels 1-16 are encoded as 0-15.
                buffer[numBytes++] = (sbyte)(0x90 + (channel - 1)); // note on
                buffer[numBytes++] = (sbyte)60; // pitch is middle C
                buffer[numBytes++] = (sbyte)127; // max velocity
                int offset = 0;
                // post is non-blocking
                this.mip.send(buffer, offset, numBytes);
            }
            if(GUILayout.Button("Off", GUILayout.Height(70.0f)) == true)
            { 
                sbyte[] buffer = new sbyte[32];
                int numBytes = 0;
                int channel = 3; // MIDI channels 1-16 are encoded as 0-15.
                buffer[numBytes++] = (sbyte)(0x80 + (channel - 1)); // note off
                buffer[numBytes++] = (sbyte)60; // pitch is middle C
                buffer[numBytes++] = (sbyte)127; // max velocity
                int offset = 0;
                // post is non-blocking
                this.mip.send(buffer, offset, numBytes);
            }
        }
    }

    void PxPre.AndWrap.IReporter.ReportLog(string log)
    { 
        this.Log(log);
    }

    void PxPre.AndWrap.IReporter.ReportWarning(string log)
    { 
        this.Log("WARN: " + log);
    }

    void PxPre.AndWrap.IReporter.ReportError(string log)
    { 
        this.Log("ERR: " + log);
    }

    void PxPre.AndWrap.IReporter.ReportException(string log)
    { 
        this.Log("EXC: " + log);
    }

    private static System.DateTime selectedDate = System.DateTime.Now;

    class DateCallback : AndroidJavaProxy
    {
        public DateCallback() : base("android.app.DatePickerDialog$OnDateSetListener") {}
        void onDateSet(AndroidJavaObject view, int year, int monthOfYear, int dayOfMonth)
        {
            selectedDate = new System.DateTime(year, monthOfYear + 1, dayOfMonth);
        }
    }
}
