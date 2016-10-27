using System;

#if NETFX_CORE
using Windows.UI.Core;
#endif

namespace MarkerMetro.Unity.WinIntegration.Store
{
    public class StoreManager
    {
        private bool _useSimulator;
        private IStorePlatform _storeObject;


        public bool IsActiveTrial
        {
            get
            {
                if (_storeObject == null) return false;
                return _storeObject.TrialMode && _storeObject.ActiveLicense;
            }
        }

        private static StoreManager _inst;
        private static object _sync = new object();

        public static StoreManager Instance
        {
            get
            {
                lock (_sync)
                {
                    if (_inst == null)
                        _inst = new StoreManager();
                }
                return _inst;
            }
        }

        private StoreManager()
        {
            _storeObject = null;
        }

        public void Initialise(bool useSimulator)
        {
            if (_storeObject != null) return;
            _useSimulator = useSimulator;

#if UNITY_EDITOR
            System.Diagnostics.Debug.WriteLine("Loaded mock store for Unity Editor");
#elif NETFX_CORE
            _storeObject = new WindowsStorePlatform();
#endif
            _storeObject.Load(_useSimulator);
        }

        public void RetrieveProducts(ProductListDelegate callback)
        {
            if (_storeObject == null)
            {
                if (callback != null)
                    callback(null);
                return;
            }

            _storeObject.RetrieveProducts(callback);
        }

        public void PurchaseProduct(Product product, PurchaseDelegate callback)
        {
            if (_storeObject == null)
            {
                if (callback != null)
                    callback(new Receipt(product, StatusCode.NotReady, null));
                return;
            }

            _storeObject.PurchaseProduct(product, callback);
        }

        public void PurchaseApplication(PurchaseDelegate callback)
        {
            if (_storeObject == null)
            {
                if (callback != null)
                    callback(new Receipt(StatusCode.NotReady, null));
                return;
            }

            _storeObject.PurchaseApplication(callback);
        }
    }
}