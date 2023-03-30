#if !BESTHTTP_DISABLE_SIGNALR_CORE
using System;

namespace BestHTTP.SignalRCore
{
    public static class HubConnectionExtensions
    {
        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 1, true, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 1, true, args);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 2, true, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 2, true, args);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 3, true, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 3, true, args);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3, T4>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 4, true, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3, T4>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 4, true, args);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3, T4, T5>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 5, true, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpAndDownStreamController<TResult, T1, T2, T3, T4, T5>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 5, true, args);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 1, false, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 1, false, args);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 2, false, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 2, false, args);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 3, false, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 3, false, args);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3, T4>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 4, false, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3, T4>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 4, false, args);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3, T4, T5>(this HubConnection hub, string target)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 5, false, HubConnection.EmptyArgs);
        }

        public static UpStreamItemController<TResult> GetUpStreamController<TResult, T1, T2, T3, T4, T5>(this HubConnection hub, string target, params object[] args)
        {
            if (hub.State != ConnectionStates.Connected)
                return null;

            return hub.GetUpStreamController<TResult>(target, 5, false, args);
        }
    }

    public static class UploadItemControllerExtensions
    {
        public static void UploadParam<TResult, P1>(this UpStreamItemController<TResult> controller, P1 item)
        {
            controller.UploadParam<P1>(controller.streamingIds[0], item);
        }

        public static void UploadParam<TResult, P1, P2>(this UpStreamItemController<TResult> controller, P1 param1, P2 param2)
        {
            controller.UploadParam<P1>(controller.streamingIds[0], param1);
            controller.UploadParam<P2>(controller.streamingIds[1], param2);
        }

        public static void UploadParam<TResult, P1, P2, P3>(this UpStreamItemController<TResult> controller, P1 param1, P2 param2, P3 param3)
        {
            controller.UploadParam<P1>(controller.streamingIds[0], param1);
            controller.UploadParam<P2>(controller.streamingIds[1], param2);
            controller.UploadParam<P3>(controller.streamingIds[2], param3);
        }

        public static void UploadParam<TResult, P1, P2, P3, P4>(this UpStreamItemController<TResult> controller, P1 param1, P2 param2, P3 param3, P4 param4)
        {
            controller.UploadParam<P1>(controller.streamingIds[0], param1);
            controller.UploadParam<P2>(controller.streamingIds[1], param2);
            controller.UploadParam<P3>(controller.streamingIds[2], param3);
            controller.UploadParam<P4>(controller.streamingIds[3], param4);
        }

        public static void UploadParam<TResult, P1, P2, P3, P4, P5>(this UpStreamItemController<TResult> controller, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
        {
            controller.UploadParam<P1>(controller.streamingIds[0], param1);
            controller.UploadParam<P2>(controller.streamingIds[1], param2);
            controller.UploadParam<P3>(controller.streamingIds[2], param3);
            controller.UploadParam<P4>(controller.streamingIds[3], param4);
            controller.UploadParam<P5>(controller.streamingIds[4], param5);
        }
    }
}

#endif
