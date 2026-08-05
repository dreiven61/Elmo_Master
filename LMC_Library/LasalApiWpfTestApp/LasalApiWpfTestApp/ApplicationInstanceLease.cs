using System;
using System.Threading;

namespace LasalMotionControlApiExample
{
    internal sealed class ApplicationInstanceLease : IDisposable
    {
        internal const string DefaultMutexName =
            @"Local\Elmo.LasalMotionControlApiExample.SingleInstance";

        private Mutex mutex;
        private bool ownsMutex;

        private ApplicationInstanceLease(Mutex ownedMutex)
        {
            mutex = ownedMutex
                ?? throw new ArgumentNullException("ownedMutex");
            ownsMutex = true;
        }

        internal static bool TryAcquireDefault(
            out ApplicationInstanceLease lease)
        {
            return TryAcquire(DefaultMutexName, out lease);
        }

        internal static bool TryAcquire(
            string mutexName,
            out ApplicationInstanceLease lease)
        {
            if (string.IsNullOrWhiteSpace(mutexName))
            {
                throw new ArgumentException(
                    "A named mutex is required.",
                    "mutexName");
            }

            lease = null;
            Mutex candidate = null;
            try
            {
                bool createdNew;
                candidate = new Mutex(
                    true,
                    mutexName,
                    out createdNew);
                if (!createdNew)
                {
                    return false;
                }

                lease = new ApplicationInstanceLease(candidate);
                candidate = null;
                return true;
            }
            finally
            {
                if (candidate != null)
                {
                    candidate.Dispose();
                }
            }
        }

        public void Dispose()
        {
            var ownedMutex = mutex;
            if (ownedMutex == null)
            {
                return;
            }

            mutex = null;
            try
            {
                if (ownsMutex)
                {
                    ownedMutex.ReleaseMutex();
                }
            }
            finally
            {
                ownsMutex = false;
                ownedMutex.Dispose();
            }
        }
    }
}
