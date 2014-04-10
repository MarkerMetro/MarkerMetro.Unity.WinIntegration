using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using WSAStore = Windows.ApplicationModel.Store;

namespace MarkerMetro.Unity.WinIntegration.Store
{
    internal class WindowsStorePlatform : IStorePlatform
    {
        public bool TrialMode
        {
            get
            {
                if (_useSimulator)
                    return CurrentAppSimulator.LicenseInformation.IsTrial;
                else
                    return CurrentApp.LicenseInformation.IsTrial;
            }
        }

        public bool ActiveLicense
        {
            get
            {
                if (_useSimulator)
                    return CurrentAppSimulator.LicenseInformation.IsActive;
                else
                    return CurrentApp.LicenseInformation.IsActive;
            }
        }

        private bool _useSimulator;
        private StoreManager _store;

        public void Load(bool simulator, StoreManager storeInst)
        {
            _useSimulator = simulator;
            _store = storeInst;

            if (_useSimulator)
                LoadSimulator();
        }

        private async void LoadSimulator()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///iap_simulator.xml"));
            await CurrentAppSimulator.ReloadSimulatorAsync(file);
        }

        public void RetrieveProducts(ProductListDelegate callback)
        {
            if (_store == null ) return;
            Dispatcher.InvokeOnUIThread(async () =>
            {
                    var products = await RetrieveProductsAsync();
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(products));
                    };
            });
        }

        public void PurchaseProduct(Product product, PurchaseDelegate callback)
        {
            if (_store == null) return;

            Dispatcher.InvokeOnUIThread(async () =>
            {
                Receipt result = null;
                try
                {
                    result = await PurchaseProductAsync(product);
                }
                catch (Exception)
                {
                    result = new Receipt(product, StatusCode.ExceptionThrown, null);
                }
                if (callback != null)
                {
                    Dispatcher.InvokeOnAppThread(() => callback(result));
                };
            });
        }

        public void PurchaseApplication(PurchaseDelegate callback)
        {
            throw new NotImplementedException();
        }

        private async Task<Receipt> PurchaseProductAsync(Product product)
        {
            // Products can only be bought if the app is owned
            if (TrialMode)
            {
                var appReceipt = await PurchaseApplicationAsync();
                if (!appReceipt.Success)
                    return new Receipt(product, appReceipt.Status, null);
            }

            // Ensure that if this is a consumable, any previous purchases have been fulfilled
            if (product.Type == ProductType.Consumable)
            {
                var waiting = await GetUnfulfilledConsumablesAsync();
                UnfulfilledConsumable transaction = null;
                foreach (var unfulfilledConsumable in waiting)
                {
                    if (unfulfilledConsumable.ProductId == product.ProductID)
                    {
                        transaction = unfulfilledConsumable;
                        break;
                    }
                }

                if (transaction != null)
                {
                    var fulfillmentResult = await ReportConsumableFulfillmentAsync(transaction.ProductId, transaction.TransactionId);
                    if (fulfillmentResult != FulfillmentResult.NothingToFulfill &&
                        fulfillmentResult != FulfillmentResult.Succeeded)
                    {
                        // An error occured or the purchase is still pending and could fail - we consider this purchase failed
                        return new Receipt(product, StatusCode.ConsumablePending, null);
                    }
                }
            }

            var purchaseResult = await RequestProductPurchaseAsync(product.ProductID);
            if (purchaseResult.Status == ProductPurchaseStatus.Succeeded)
            {
                return new Receipt(product, StatusCode.Success, purchaseResult.ReceiptXml);
            }

            // If we reach this point then an error has occured in the purchase
            var status = StatusCode.UnknownError;
            switch (purchaseResult.Status)
            {
                case ProductPurchaseStatus.AlreadyPurchased:
                    status = StatusCode.DurableOwned;
                    break;

                case ProductPurchaseStatus.NotFulfilled:
                    status = StatusCode.ConsumablePending;
                    break;

                case ProductPurchaseStatus.NotPurchased:
                    status = StatusCode.UserCancelled;
                    break;
            }
            return new Receipt(product, status, null);
        }

        private async Task<Receipt> PurchaseApplicationAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<List<Product>> RetrieveProductsAsync()
        {
            ListingInformation listing;
            LicenseInformation license;
            if (_useSimulator)
            {
                listing = await CurrentAppSimulator.LoadListingInformationAsync();
                license = CurrentAppSimulator.LicenseInformation;
            }
            else
            {
                listing = await CurrentApp.LoadListingInformationAsync();
                license = CurrentApp.LicenseInformation;
            }

            if (listing != null)
            {
                var products = listing.ProductListings;
                var results = new List<Product>();
                foreach (var p in products.Values)
                {
                    var product = new Product();
                    product.ProductID = p.ProductId;
                    product.Name = p.Name;
                    product.FormattedPrice = p.FormattedPrice;
                    product.PriceValue = Helpers.GetValueFromFormattedPrice(product.FormattedPrice);

                    // Determine license status
                    product.Purchased = license.ProductLicenses[product.ProductID].IsActive;
                    if (product.Purchased)
                        product.Expires = license.ProductLicenses[product.ProductID].ExpirationDate.DateTime;

                    switch (p.ProductType)
                    {
                        case WSAStore.ProductType.Consumable:
                            product.Type = ProductType.Consumable;
                            break;
                        case WSAStore.ProductType.Durable:
                            product.Type = ProductType.Durable;
                            break;
                    }
                    results.Add(product);
                }
                return results;
            }
            return null;
        }

        private IAsyncOperation<PurchaseResults> RequestProductPurchaseAsync(string productId)
        {
            if (_useSimulator)
                return CurrentAppSimulator.RequestProductPurchaseAsync(productId);
            else
                return CurrentApp.RequestProductPurchaseAsync(productId);
        }

        private IAsyncOperation<IReadOnlyList<UnfulfilledConsumable>> GetUnfulfilledConsumablesAsync()
        {
            if (_useSimulator)
                return CurrentAppSimulator.GetUnfulfilledConsumablesAsync();
            else
                return CurrentApp.GetUnfulfilledConsumablesAsync();
        }

        private IAsyncOperation<FulfillmentResult> ReportConsumableFulfillmentAsync(string productId, Guid transactionId)
        {
            if (_useSimulator)
                return CurrentAppSimulator.ReportConsumableFulfillmentAsync(productId, transactionId);
            else
                return CurrentApp.ReportConsumableFulfillmentAsync(productId, transactionId);
        }
    }
}
