using System.Collections.Generic;

namespace MarkerMetro.Unity.WinIntegration.Store
{
    public delegate void ProductListDelegate(List<Product> products);

    public delegate void PurchaseDelegate(Receipt receipt);

    public interface IStorePlatform
    {
        bool TrialMode { get; }

        bool ActiveLicense { get; }

        void Load(bool simulator);

        void RetrieveProducts(ProductListDelegate callback);

        void PurchaseProduct(Product product, PurchaseDelegate callback);

        void PurchaseApplication(PurchaseDelegate callback);
    }
}