using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace HDT_Reconnector
{
    internal class Wfp
    {

        public static readonly Guid FWPM_CONDITION_IP_PROTOCOL = new Guid("3971ef2b-623e-4f9a-8cb1-6e79b806b9a7");
        public static readonly Guid FWPM_CONDITION_IP_REMOTE_PORT = new Guid("c35a604d-d22b-4e1a-91b4-68f674ee674b");
        public static readonly Guid FWPM_CONDITION_IP_REMOTE_ADDRESS = new Guid("b235ae9a-1d64-49b8-a44c-5ff3d9095045");
        public static readonly Guid FWPM_LAYER_INBOUND_TRANSPORT_V4 = new Guid("5926dfc8-e3cf-4426-a283-dc393f5d0f9d");

        public enum IPPROTO : byte
        {
            HOPOPTS = 0,
            ICMP = 1,
            IGMP = 2,
            GGP = 3,
            IPV4 = 4,
            ST = 5,
            TCP = 6,
            CBT = 7,
            EGP = 8,
            IGP = 9,
            PUP = 12,
            UDP = 17,
            IDP = 22,
            RDP = 27,
            IPV6 = 41,
            ROUTING = 43,
            FRAGMENT = 44,
            ESP = 50,
            AH = 51,
            ICMPV6 = 58,
            NONE = 59,
            DSTOPTS = 60,
            ND = 77,
            ICLFXBM = 78,
            PIM = 103,
            PGM = 113,
            L2TP = 115,
            SCTP = 132,
            RAW = 255,
        }

        public enum FWP_E : uint
        {
            SUCCESS = 0x00000000,
            CALLOUT_NOT_FOUND = 0x80320001,
            CONDITION_NOT_FOUND = 0x80320002,
            FILTER_NOT_FOUND = 0x80320003,
            LAYER_NOT_FOUND = 0x80320004,
            PROVIDER_NOT_FOUND = 0x80320005,
            PROVIDER_CONTEXT_NOT_FOUND = 0x80320006,
            SUBLAYER_NOT_FOUND = 0x80320007,
            NOT_FOUND = 0x80320008,
            ALREADY_EXISTS = 0x80320009,
            IN_USE = 0x8032000A,
            DYNAMIC_SESSION_IN_PROGRESS = 0x8032000B,
            WRONG_SESSION = 0x8032000C,
            NO_TXN_IN_PROGRESS = 0x8032000D,
            TXN_IN_PROGRESS = 0x8032000E,
            TXN_ABORTED = 0x8032000F,
            SESSION_ABORTED = 0x80320010,
            INCOMPATIBLE_TXN = 0x80320011,
            TIMEOUT = 0x80320012,
            NET_EVENTS_DISABLED = 0x80320013,
            INCOMPATIBLE_LAYER = 0x80320014,
            KM_CLIENTS_ONLY = 0x80320015,
            LIFETIME_MISMATCH = 0x80320016,
            BUILTIN_OBJECT = 0x80320017,
            TOO_MANY_CALLOUTS = 0x80320018,
            NOTIFICATION_DROPPED = 0x80320019,
            TRAFFIC_MISMATCH = 0x8032001A,
            INCOMPATIBLE_SA_STATE = 0x8032001B,
            NULL_POINTER = 0x8032001C,
            INVALID_ENUMERATOR = 0x8032001D,
            INVALID_FLAGS = 0x8032001E,
            INVALID_NET_MASK = 0x8032001F,
            INVALID_RANGE = 0x80320020,
            INVALID_INTERVAL = 0x80320021,
            ZERO_LENGTH_ARRAY = 0x80320022,
            NULL_DISPLAY_NAME = 0x80320023,
            INVALID_ACTION_TYPE = 0x80320024,
            INVALID_WEIGHT = 0x80320025,
            MATCH_TYPE_MISMATCH = 0x80320026,
            TYPE_MISMATCH = 0x80320027,
            OUT_OF_BOUNDS = 0x80320028,
            RESERVED = 0x80320029,
            DUPLICATE_CONDITION = 0x8032002A,
            DUPLICATE_KEYMOD = 0x8032002B,
            ACTION_INCOMPATIBLE_WITH_LAYER = 0x8032002C,
            ACTION_INCOMPATIBLE_WITH_SUBLAYER = 0x8032002D,
            CONTEXT_INCOMPATIBLE_WITH_LAYER = 0x8032002E,
            CONTEXT_INCOMPATIBLE_WITH_CALLOUT = 0x8032002F,
            INCOMPATIBLE_AUTH_METHOD = 0x80320030,
            INCOMPATIBLE_DH_GROUP = 0x80320031,
            EM_NOT_SUPPORTED = 0x80320032,
            NEVER_MATCH = 0x80320033,
            PROVIDER_CONTEXT_MISMATCH = 0x80320034,
            INVALID_PARAMETER = 0x80320035,
            TOO_MANY_SUBLAYERS = 0x80320036,
            CALLOUT_NOTIFICATION_FAILED = 0x80320037,
            INVALID_AUTH_TRANSFORM = 0x80320038,
            INVALID_CIPHER_TRANSFORM = 0x80320039,
            INCOMPATIBLE_CIPHER_TRANSFORM = 0x8032003A,
            INVALID_TRANSFORM_COMBINATION = 0x8032003B,
            DUPLICATE_AUTH_METHOD = 0x8032003C,
            INVALID_TUNNEL_ENDPOINT = 0x8032003D,
            L2_DRIVER_NOT_READY = 0x8032003E,
            KEY_DICTATOR_ALREADY_REGISTERED = 0x8032003F,
            KEY_DICTATION_INVALID_KEYING_MATERIAL = 0x80320040,
            CONNECTIONS_DISABLED = 0x80320041,
            INVALID_DNS_NAME = 0x80320042,
            STILL_ON = 0x80320043,
            IKEEXT_NOT_RUNNING = 0x80320044,
            DROP_NOICMP = 0x80320104,
        }

        public enum RPC_C_AUTHN : uint
        {
            NONE = 0x00000000,
            WINNT = 0x0000000A,
            DEFAULT = 0xFFFFFFFF,
        }

        public enum FWPM_SESSION_FLAG : uint
        {
            NONE = 0x00000000,
            DYNAMIC = 0x00000001,
            RESERVED = 0x10000000,
        }

        public enum FWPM_FILTER_FLAG : uint
        {
            NONE = 0x00000000,
            PERSISTENT = 0x00000001,
            BOOTTIME = 0x00000002,
            HAS_PROVIDER_CONTEXT = 0x00000004,
            CLEAR_ACTION_RIGHT = 0x00000008,
            PERMIT_IF_CALLOUT_UNREGISTERED = 0x00000010,
            DISABLED = 0x00000020,
            INDEXED = 0x00000040,
        }

        public enum FWP_DATA_TYPE : uint
        {
            EMPTY = 0x00000000,
            UINT8 = 0x00000001,
            UINT16 = 0x00000002,
            UINT32 = 0x00000003,
            UINT64 = 0x00000004,
            INT8 = 0x00000005,
            INT16 = 0x00000006,
            INT32 = 0x00000007,
            INT64 = 0x00000008,
            FLOAT = 0x00000009,
            DOUBLE = 0x0000000A,
            BYTE_ARRAY16_TYPE = 0x0000000B,
            BYTE_BLOB_TYPE = 0x0000000C,
            SID = 0x0000000D,
            SECURITY_DESCRIPTOR_TYPE = 0x0000000E,
            TOKEN_INFORMATION_TYPE = 0x0000000F,
            TOKEN_ACCESS_INFORMATION_TYPE = 0x00000010,
            UNICODE_STRING_TYPE = 0x00000011,
            BYTE_ARRAY6_TYPE = 0x00000012,
            SINGLE_DATA_TYPE_MAY = 0x000000FF,
            V4_ADDR_MASK = 0x00000100,
            V6_ADDR_MASK = 0x00000101,
            RANGE_TYPE = 0x00000102,
            MAX = 0x00000103,
        }

        public enum FWP_ACTION_TYPE : uint
        {
            BLOCK = 0x00001001,
            PERMIT = 0x00001002,
            CALLOUT_UNKNOWN = 0x00004005,
            CALLOUT_TERMINATING = 0x00005003,
            CALLOUT_INSPECTION = 0x00006004,
        }

        public enum FWP_MATCH
        {
            EQUAL,
            GREATER,
            LESS,
            GREATER_OR_EQUAL,
            LESS_OR_EQUAL,
            RANGE,
            FLAGS_ALL_SET,
            FLAGS_ANY_SET,
            FLAGS_NONE_SET,
            EQUAL_CASE_INSENSITIVE,
            NOT_EQUAL,
            MAX,
        }

        public enum FWPM_PROVIDER_FLAG : uint
        {
            PERSISTENT = 0x00000001,
            DISABLED = 0x00000010
        }

        public enum FWP_FILTER_ENUM_TYPE : uint
        {
            FULLY_CONTAINED,
            OVERLAPPING,
            MAX,
        }

        public enum FWP_FILTER_ENUM_FLAG : uint
        {
            BEST_TERMINATING_MATCH = 0x00000001,
            SORTED = 0x00000002,
            BOOTTIME_ONLY = 0x00000004,
            INCLUDE_BOOTTIME = 0x00000008,
            INCLUDE_DISABLED = 0x00000010,
        }

        public enum FWPM_SUBLAYER_FLAG : uint
        {
            NONE = 0x00000000,
            PERSISTENT = 0x00000001,
        }

        public enum FWPM_TRANSACTION_BEGIN_FLAG : uint
        {
            ZERO = 0x00000000,
            FWPM_TXN_READ_ONLY = 0x00000001
        }

        public struct FWPM_DISPLAY_DATA0
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string name;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string description;
        }

        public struct FWPM_SESSION0
        {
            public Guid sessionKey;
            public FWPM_DISPLAY_DATA0 displayData;
            public FWPM_SESSION_FLAG flags;
            public uint txnWaitTimeoutInMSec;
            public uint processId;
            public IntPtr sid;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string username;
            [MarshalAs(UnmanagedType.Bool)]
            public bool kernelMode;
        }

        public struct FWP_VALUE0
        {
            public FWP_DATA_TYPE type;
            public Union value;

            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public byte uint8;
                [FieldOffset(0)]
                public ushort uint16;
                [FieldOffset(0)]
                public uint uint32;
                [FieldOffset(0)]
                public IntPtr uint64;
                [FieldOffset(0)]
                public sbyte int8;
                [FieldOffset(0)]
                public short int16;
                [FieldOffset(0)]
                public int int32;
                [FieldOffset(0)]
                public IntPtr int64;
                [FieldOffset(0)]
                public float float32;
                [FieldOffset(0)]
                public IntPtr double64;
                [FieldOffset(0)]
                public IntPtr byteArray16;
                [FieldOffset(0)]
                public IntPtr byteBlob;
                [FieldOffset(0)]
                public IntPtr sid;
                [FieldOffset(0)]
                public IntPtr sd;
                [FieldOffset(0)]
                public IntPtr tokenInformation;
                [FieldOffset(0)]
                public IntPtr tokenAccessInformation;
                [FieldOffset(0)]
                public IntPtr unicodeString;
                [FieldOffset(0)]
                public IntPtr byteArray6;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct FWPM_ACTION0
        {
            [FieldOffset(0)]
            public FWP_ACTION_TYPE type;
            [FieldOffset(4)]
            public Guid filterType;
            [FieldOffset(4)]
            public Guid calloutKey;
        }

        public struct FWPM_FILTER0
        {
            public Guid filterKey;
            public FWPM_DISPLAY_DATA0 displayData;
            public FWPM_FILTER_FLAG flags;
            public IntPtr providerKey;
            public FWP_BYTE_BLOB providerData;
            public Guid layerKey;
            public Guid subLayerKey;
            public FWP_VALUE0 weight;
            public uint numFilterConditions;
            public IntPtr filterConditions;
            public FWPM_ACTION0 action;
            public Union context;
            public IntPtr reserved;
            public ulong filterId;
            public FWP_VALUE0 effectiveWeight;

            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public ulong rawContext;
                [FieldOffset(0)]
                public Guid providerContextKey;
            }
        }

        public struct FWP_BYTE_BLOB
        {
            public uint size;
            public IntPtr data;
        }

        public struct FWP_CONDITION_VALUE0
        {
            public FWP_DATA_TYPE type;
            public Union value;

            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public byte uint8;
                [FieldOffset(0)]
                public ushort uint16;
                [FieldOffset(0)]
                public uint uint32;
                [FieldOffset(0)]
                public IntPtr uint64;
                [FieldOffset(0)]
                public sbyte int8;
                [FieldOffset(0)]
                public short int16;
                [FieldOffset(0)]
                public int int32;
                [FieldOffset(0)]
                public IntPtr int64;
                [FieldOffset(0)]
                public float float32;
                [FieldOffset(0)]
                public IntPtr double64;
                [FieldOffset(0)]
                public IntPtr byteArray16;
                [FieldOffset(0)]
                public IntPtr byteBlob;
                [FieldOffset(0)]
                public IntPtr sid;
                [FieldOffset(0)]
                public IntPtr sd;
                [FieldOffset(0)]
                public IntPtr tokenInformation;
                [FieldOffset(0)]
                public IntPtr tokenAccessInformation;
                [FieldOffset(0)]
                public IntPtr unicodeString;
                [FieldOffset(0)]
                public IntPtr byteArray6;
                [FieldOffset(0)]
                public IntPtr v4AddrMask;
                [FieldOffset(0)]
                public IntPtr v6AddrMask;
                [FieldOffset(0)]
                public IntPtr rangeValue;
            }
        }

        public struct FWPM_FILTER_CONDITION0
        {
            public Guid fieldKey;
            public FWP_MATCH matchType;
            public FWP_CONDITION_VALUE0 conditionValue;
        }

        public struct FWPM_PROVIDER0
        {
            public Guid providerKey;
            public FWPM_DISPLAY_DATA0 displayData;
            public FWPM_PROVIDER_FLAG flags;
            public FWP_BYTE_BLOB providerData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string serviceName;
        }

        // public struct FWPM_FILTER_ENUM_TEMPLATE0
        // {
        //     public IntPtr providerKey;
        //     public Guid layerKey;
        //     public FWP_FILTER_ENUM_TYPE enumType;
        //     public FWP_FILTER_ENUM_FLAG flags;
        //     public IntPtr providerContextTemplate;
        //     public uint numFilterConditions;
        //     public IntPtr filterCondition;
        //     public uint actionMask;
        //     public IntPtr calloutKey;
        // }

        public struct FWPM_SUBLAYER0
        {
            public Guid subLayerKey;
            public FWPM_DISPLAY_DATA0 displayData;
            public FWPM_SUBLAYER_FLAG flags;
            public IntPtr providerKey;
            public FWP_BYTE_BLOB providerData;
            public ushort weight;
        }

        public struct FWP_V4_ADDR_AND_MASK
        {
            public uint addr;
            public uint mask;
        }

        [DllImport("FWPUCLNT.DLL")]
        public static extern void FwpmFreeMemory0(
            ref IntPtr p);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmEngineOpen0(
            [MarshalAs(UnmanagedType.LPWStr)] string serverName,
            uint authnService,
            IntPtr authIdentity,
            ref FWPM_SESSION0 session,
            out IntPtr engineHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmEngineClose0(
            IntPtr engineHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmProviderAdd0(
            IntPtr engineHandle,
            ref FWPM_PROVIDER0 provider,
            IntPtr sd);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmProviderGetByKey0(
            IntPtr engineHandle,
            ref Guid key,
            out IntPtr provider);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmProviderDeleteByKey0(
            IntPtr engineHandle,
            ref Guid key);

        [DllImport("FWPUCLNT.DLL")]
        internal static extern uint FwpmFilterAdd0(
            IntPtr engineHandle,
            ref FWPM_FILTER0 filter,
            IntPtr sd,
            out ulong id);

        [DllImport("FWPUCLNT.DLL")]
        internal static extern uint FwpmFilterDeleteByKey0(
            IntPtr engineHandle,
            ref Guid key);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmFilterCreateEnumHandle0(
            IntPtr engineHandle,
            IntPtr enumTemplate,
            out IntPtr enumHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmFilterEnum0(
            IntPtr engineHandle,
            IntPtr enumHandle,
            uint numEntriesRequested,
            out IntPtr entries,
            out uint numEntriesReturned);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmFilterDestroyEnumHandle0(
            IntPtr engineHandle,
            IntPtr enumHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmSubLayerAdd0(
            IntPtr engineHandle,
            ref FWPM_SUBLAYER0 subLayer,
            IntPtr sd);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmSubLayerDeleteByKey0(
            IntPtr engineHandle,
            ref Guid key);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmSubLayerCreateEnumHandle0(
            IntPtr engineHandle,
            IntPtr enumTemplate,
            out IntPtr enumHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmSubLayerEnum0(
            IntPtr engineHandle,
            IntPtr enumHandle,
            uint numEntriesRequested,
            out IntPtr entries,
            out uint numEntriesReturned);


        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmSubLayerDestroyEnumHandle0(
            IntPtr engineHandle,
            IntPtr enumHandle);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmTransactionBegin0(
            IntPtr engineHandle,
            uint flags);

        [DllImport("FWPUCLNT.DLL")]
        public static extern uint FwpmTransactionCommit0(
            IntPtr engineHandle);
    }
}
