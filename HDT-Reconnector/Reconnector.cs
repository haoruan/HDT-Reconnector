using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;

using HDT_Reconnector.Native;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector
{
    public class Reconnector
    {
        private const string hsName = "Hearthstone";

        public const string ReconnectString = "Reconnect";
        public const string DisconnectedString = "Disconnected";

        private const string wfpSessionName = "Hearthstone Reconnector";

        private const string providerName = "Hearthstone Reconnector Wall";
        private Guid providerKey = new Guid("f73528d8-f758-43a5-bcd2-845265a8b1be");

        private const string subLayerName = "V4 Layer";
        private Guid subLayerKey = new Guid("1fb3cf7b-fc3e-40fb-afae-6f50e1b71acf");

        private const string filterName = "Port filter";
        private Guid filterKey = new Guid("25a10c5b-883c-498c-ab21-946ea39d169c");
        private IntPtr engine = IntPtr.Zero;

        public CONNECTION_STATUS Status { get; set; } = CONNECTION_STATUS.CONNECTED;

        public enum CONNECTION_STATUS
        {
            DISCONNECTED,
            CONNECTED
        }

        public Reconnector()
        {
            if (InstallWfp() != 0)
            {
                throw new WfpException("Wfp Init failed");
            }
        }

        ~Reconnector()
        {
            if (engine != IntPtr.Zero)
            {
                UninstallWfp();
            }
        }

        public void ResumeConnect()
        {
            Log.Info("Reconnecting...");
            DeleteFilter();
        }

        public uint Disconnect(uint addr, ushort port)
        {
            Log.Info("Disconnecting...");
            Status = CONNECTION_STATUS.DISCONNECTED;
            return AddWfpFilter(addr, port);
        }

        private uint InstallWfp()
        {
            var session = new Wfp.FWPM_SESSION0()
            {
                txnWaitTimeoutInMSec = int.MaxValue
            };
            session.displayData.name = wfpSessionName;

            var result = Wfp.FwpmEngineOpen0(null, (uint)Wfp.RPC_C_AUTHN.DEFAULT, IntPtr.Zero, ref session, out engine);
            if (result != (uint)Wfp.FWP_E.SUCCESS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmEngineOpen0), result));
                goto Cleanup;
            }

            result = Wfp.FwpmTransactionBegin0(engine, (uint)Wfp.FWPM_TRANSACTION_BEGIN_FLAG.ZERO);
            if (result != (uint)Wfp.FWP_E.SUCCESS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmTransactionBegin0), result));
                goto Cleanup;
            }

            var provider = new Wfp.FWPM_PROVIDER0
            {
                providerKey = providerKey,
                flags = Wfp.FWPM_PROVIDER_FLAG.PERSISTENT
            };
            provider.displayData.name = providerName;

            result = Wfp.FwpmProviderAdd0(engine, ref provider, IntPtr.Zero);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.ALREADY_EXISTS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmProviderAdd0), result));
                goto Cleanup;
            }

            var ptrs = new NativePtrs();
            var subLayer = new Wfp.FWPM_SUBLAYER0
            {
                subLayerKey = subLayerKey,
                flags = Wfp.FWPM_SUBLAYER_FLAG.PERSISTENT,
                providerKey = ptrs.Add(providerKey),
                weight = 0x8000
            };
            subLayer.displayData.name = subLayerName;

            result = Wfp.FwpmSubLayerAdd0(engine, ref subLayer, IntPtr.Zero);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.ALREADY_EXISTS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmSubLayerAdd0), result));
                goto Cleanup;
            }

            result = Wfp.FwpmTransactionCommit0(engine);
            if (result != (uint)Wfp.FWP_E.SUCCESS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmTransactionCommit0), result));
                goto Cleanup;
            }

            return 0;

        Cleanup:
            Wfp.FwpmEngineClose0(engine);
            return result == (uint)Wfp.FWP_E.SUCCESS || result == (uint)Wfp.FWP_E.ALREADY_EXISTS ? 0 : result;
        }

        private void UninstallWfp()
        {
            var result = Wfp.FwpmTransactionBegin0(engine, (uint)Wfp.FWPM_TRANSACTION_BEGIN_FLAG.ZERO);
            if (result != 0)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmTransactionBegin0), result));
                goto Cleanup;
            }

            DeleteFilter();

            result = Wfp.FwpmSubLayerDeleteByKey0(engine, ref subLayerKey);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.SUBLAYER_NOT_FOUND)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmSubLayerDeleteByKey0), result));
                goto Cleanup;
            }

            result = Wfp.FwpmProviderDeleteByKey0(engine, ref providerKey);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.PROVIDER_NOT_FOUND)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmProviderDeleteByKey0), result));
                goto Cleanup;
            }

            result = Wfp.FwpmTransactionCommit0(engine);
            if (result != (uint)Wfp.FWP_E.SUCCESS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmTransactionCommit0), result));
                goto Cleanup;
            }

        Cleanup:
            Wfp.FwpmEngineClose0(engine);
        }

        private uint AddWfpFilter(uint addr, ushort port)
        {
            var ptrs = new NativePtrs();
            var conds = new Wfp.FWPM_FILTER_CONDITION0[3];

            conds[0].matchType = Wfp.FWP_MATCH.EQUAL;
            conds[0].fieldKey = Wfp.FWPM_CONDITION_IP_PROTOCOL;
            conds[0].conditionValue.type = Wfp.FWP_DATA_TYPE.UINT8;
            conds[0].conditionValue.value.uint8 = (byte)Wfp.IPPROTO.TCP;

            var addr4 = new Wfp.FWP_V4_ADDR_AND_MASK()
            {
                addr = addr,
                // 255.255.255.255
                mask = uint.MaxValue
            };

            conds[1].matchType = Wfp.FWP_MATCH.EQUAL;
            conds[1].fieldKey = Wfp.FWPM_CONDITION_IP_REMOTE_ADDRESS;
            conds[1].conditionValue.type = Wfp.FWP_DATA_TYPE.V4_ADDR_MASK;
            conds[1].conditionValue.value.v4AddrMask = ptrs.Add(addr4);

            conds[2].matchType = Wfp.FWP_MATCH.EQUAL;
            conds[2].fieldKey = Wfp.FWPM_CONDITION_IP_REMOTE_PORT;
            conds[2].conditionValue.type = Wfp.FWP_DATA_TYPE.UINT16;
            conds[2].conditionValue.value.uint16 = port;

            var filter = new Wfp.FWPM_FILTER0()
            {
                filterKey = filterKey,
                providerKey = ptrs.Add(providerKey),
                layerKey = Wfp.FWPM_LAYER_INBOUND_TRANSPORT_V4,
                subLayerKey = subLayerKey,
                numFilterConditions = (uint)conds.Length,
                filterConditions = ptrs.Add(conds)
            };

            filter.displayData.name = filterName;
            filter.action.type = Wfp.FWP_ACTION_TYPE.BLOCK;
            filter.weight.type = Wfp.FWP_DATA_TYPE.EMPTY;

            var result = Wfp.FwpmFilterAdd0(engine, ref filter, IntPtr.Zero, out _);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.ALREADY_EXISTS)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmFilterAdd0), result));
            }

            return result == (uint)Wfp.FWP_E.SUCCESS || result == (uint)Wfp.FWP_E.ALREADY_EXISTS ? 0 : result;
        }

        private void DeleteFilter()
        {
            var result = Wfp.FwpmFilterDeleteByKey0(engine, ref filterKey);
            if (result != (uint)Wfp.FWP_E.SUCCESS && result != (uint)Wfp.FWP_E.FILTER_NOT_FOUND)
            {
                Log.Error(String.Format("{0} failed with {1}", nameof(Wfp.FwpmFilterDeleteByKey0), result));
            }
        }
    }
}
