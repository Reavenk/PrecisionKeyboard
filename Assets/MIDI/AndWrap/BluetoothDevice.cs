// <copyright file="BluetoothDevices.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's BluetoothDevices Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class BluetoothDevice
        { 
            public const string ACTION_ACL_CONNECTED = "android.bluetooth.device.action.ACL_CONNECTED";
            public const string ACTION_ACL_DISCONNECTED = "android.bluetooth.device.action.ACL_DISCONNECTED";
            public const string ACTION_ACL_DISCONNECT_REQUESTED = "android.bluetooth.device.action.ACL_DISCONNECT_REQUESTED";
            public const string ACTION_ALIAS_CHANGED = "android.bluetooth.device.action.ALIAS_CHANGED";
            public const string ACTION_BOND_STATE_CHANGED = "android.bluetooth.device.action.BOND_STATE_CHANGED";
            public const string ACTION_CLASS_CHANGED = "android.bluetooth.device.action.CLASS_CHANGED";
            public const string ACTION_FOUND = "android.bluetooth.device.action.FOUND";
            public const string ACTION_NAME_CHANGED = "android.bluetooth.device.action.NAME_CHANGED";
            public const string ACTION_PAIRING_REQUEST = "android.bluetooth.device.action.PAIRING_REQUEST";
            public const string ACTION_UUID = "android.bluetooth.device.action.UUID";
            public const uint BOND_BONDED = 0x0000000c;
            public const uint BOND_BONDING = 0x0000000b;
            public const uint BOND_NONE = 0x0000000a;
            public const uint DEVICE_TYPE_CLASSIC = 0x00000001;
            public const uint DEVICE_TYPE_DUAL = 0x00000003;
            public const uint DEVICE_TYPE_LE = 0x00000002;
            public const uint DEVICE_TYPE_UNKNOWN = 0x00000000;
            public const uint ERROR = 0x80000000;
            public const string EXTRA_BOND_STATE = "android.bluetooth.device.extra.BOND_STATE";
            public const string EXTRA_CLASS = "android.bluetooth.device.extra.CLASS";
            public const string EXTRA_DEVICE = "android.bluetooth.device.extra.DEVICE";
            public const string EXTRA_NAME =  "android.bluetooth.device.extra.NAME";
            public const string EXTRA_PAIRING_KEY = "android.bluetooth.device.extra.PAIRING_KEY";
            public const string EXTRA_PAIRING_VARIANT = "android.bluetooth.device.extra.PAIRING_VARIANT";
            public const string EXTRA_PREVIOUS_BOND_STATE ="android.bluetooth.device.extra.PREVIOUS_BOND_STATE";
            public const string EXTRA_RSSI = "android.bluetooth.device.extra.RSSI";
            public const string EXTRA_UUID = "android.bluetooth.device.extra.UUID";
            public const uint PAIRING_VARIANT_PASSKEY_CONFIRMATION = 0x00000002;
            public const uint PAIRING_VARIANT_PIN = 0x00000000;
            public const uint PHY_LE_1M = 0x00000001;
            public const uint PHY_LE_1M_MASK = 0x00000001;
            public const uint PHY_LE_2M = 0x00000002;
            public const uint PHY_LE_2M_MASK = 0x00000002;
            public const uint PHY_LE_CODED = 0x00000003;
            public const uint PHY_LE_CODED_MASK = 0x00000004;
            public const uint PHY_OPTION_NO_PREFERRED = 0x00000000;
            public const uint PHY_OPTION_S2 = 0x00000001;
            public const uint PHY_OPTION_S8 = 0x00000002;
            public const uint TRANSPORT_AUTO = 0x00000000;
            public const uint TRANSPORT_BREDR = 0x00000001;
            public const uint TRANSPORT_LE = 0x00000002;

            public readonly AndroidJavaObject _ajo;
            public readonly IReporter _r;

            BluetoothDevice(AndroidJavaObject ajo, IReporter reporter)
            {
                this._ajo = ajo;
                this._r = reporter;
            }


            //BluetoothGatt connectGatt(Context context, boolean autoConnect, BluetoothGattCallback callback)
            //BluetoothGatt connectGatt(Context context, boolean autoConnect, BluetoothGattCallback callback, int transport, int phy, Handler handler)
            //BluetoothGatt connectGatt(Context context, boolean autoConnect, BluetoothGattCallback callback, int transport, int phy)
            //BluetoothGatt connectGatt(Context context, boolean autoConnect, BluetoothGattCallback callback, int transport)

            public bool createBond()
            { 
                return this._ajo.Call<bool>("createBond");
            }

            // BluetoothSocket	createInsecureL2capChannel(int psm)
            // BluetoothSocket	createInsecureRfcommSocketToServiceRecord(UUID uuid)
            // BluetoothSocket	createL2capChannel(int psm)
            // BluetoothSocket	createRfcommSocketToServiceRecord(UUID uuid)

            public int	describeContents()
            { 
                return this._ajo.Call<int>("describeContents");
            }

            public bool equals(AndroidJavaObject o)
            { 
                return this._ajo.Call<bool>("equals", o);
            }

            public bool fetchUuidsWithSdp()
            { 
                return this._ajo.Call<bool>("fetchUuidsWithSdp");
            }

            public string getAddress()
            { 
                return this._ajo.Call<string>("getAddress");
            }

            public string getAlias()
            { 
                return this._ajo.Call<string>("getAlias");
            }

            //public BluetoothClass getBluetoothClass()
            //{ 
            //}

            public int getBondState()
            { 
                return this._ajo.Call<int>("getBondState");
            }

            public string getName()
            { 
                return this._ajo.Call<string>("getName");
            }

            public int getType()
            { 
                return this._ajo.Call<int>("getType");
            }

            //public ParcelUuid[] getUuids()
            //{ 
            //}

            public bool setPairingConfirmation(bool confirm)
            { 
                return this._ajo.Call<bool>("setPairingConfirmation", confirm);
            }

            //public bool setPin(byte[] pin)
            //{ 
            //}

            //public void writeToParcel(Parcel out, int flags)
            //{ 
            //}
        }
    }
}
#endif