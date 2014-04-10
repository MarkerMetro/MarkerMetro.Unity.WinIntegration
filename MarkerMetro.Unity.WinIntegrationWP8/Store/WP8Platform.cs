namespace MarkerMetro.Unity.WinIntegration.Store
{
    internal class WP8Platform : IStorePlatform
    {
        public bool TrialMode { get; private set; }

        public bool ActiveLicense { get; private set; }

        public void Load(bool simulator, StoreManager storeInst)
        {
            throw new System.NotImplementedException();
        }

        public void RetrieveProducts(ProductListDelegate callback)
        {
            throw new System.NotImplementedException();
        }

        public void PurchaseProduct(Product product, PurchaseDelegate callback)
        {
            throw new System.NotImplementedException();
        }

        public void PurchaseApplication(PurchaseDelegate callback)
        {
            throw new System.NotImplementedException();
        }
    }
}
