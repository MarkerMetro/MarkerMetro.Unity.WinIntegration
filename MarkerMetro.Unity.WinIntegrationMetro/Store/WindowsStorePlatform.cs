using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;

namespace MarkerMetro.Unity.WinIntegration.Store
{
    internal class WindowsStorePlatform : IStorePlatform
    {
        private bool _useSimulator;

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

        public void Load(bool simulator)
        {
            _useSimulator = simulator;
            
            if (_useSimulator)
                LoadSimulator();
        }

        private async void LoadSimulator()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///iap_simulator.xml"));
            await CurrentAppSimulator.ReloadSimulatorAsync(file);
        }

        public void PurchaseApplication(PurchaseDelegate callback)
        {
            Dispatcher.InvokeOnUIThread(async () =>
            {
                try
                {
                    var result = await PurchaseApplicationAsync();
                    if (callback != null)
                    {
                        var receipt = new Receipt(StatusCode.Success, result);
                        Dispatcher.InvokeOnAppThread(() => callback(receipt));
                    };            
                }
                catch
                {
                    if (callback != null)
                    {
                        var receipt = new Receipt(StatusCode.ExceptionThrown, null);
                        Dispatcher.InvokeOnAppThread(() => callback(receipt));
                    };
                }
            });
        }

        private async Task<string> PurchaseApplicationAsync()
        {
            if (_useSimulator)
            {
                return await CurrentAppSimulator.RequestAppPurchaseAsync(false);
            }
            else
            {
                return await CurrentApp.RequestAppPurchaseAsync(false);
            }
        }

        public void RetrieveProducts(ProductListDelegate callback)
        {
            Dispatcher.InvokeOnUIThread(async () =>
            {
                try
                {
                    var networkAvailable = NetworkInterface.GetIsNetworkAvailable();
                    if (!networkAvailable)
                    {
                        throw new Exception();
                    }

                    var products = await RetrieveProductsAsync();
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(products));
                    };

                }
                catch
                {
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(null));
                    };
                }
            });
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

                    // Determine license status
                    product.Purchased = license.ProductLicenses[product.ProductID].IsActive;
                    if (product.Purchased)
                        product.Expires = license.ProductLicenses[product.ProductID].ExpirationDate.DateTime;

                    switch (p.ProductType)
                    {
                        case Windows.ApplicationModel.Store.ProductType.Consumable:
                            product.Type = MarkerMetro.Unity.WinIntegration.Store.ProductType.Consumable;
                            break;
                        case Windows.ApplicationModel.Store.ProductType.Durable:
                            product.Type = MarkerMetro.Unity.WinIntegration.Store.ProductType.Durable;
                            break;
                    }
                    results.Add(product);
                }
                return results;
            }
            return null;
        }

        public void PurchaseProduct(Product product, PurchaseDelegate callback)
        {
            Dispatcher.InvokeOnUIThread(async () =>
            {
                try
                {
                    var networkAvailable = NetworkInterface.GetIsNetworkAvailable();
                    if (!networkAvailable)
                    {
                        throw new Exception();
                    }
                    if (TrialMode)
                    {
                        await PurchaseApplicationAsync();
                    }
                    var result = await PurchaseProductAsync(product);
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(result));
                    };
                }
                catch
                {
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(new Receipt(product, StatusCode.ExceptionThrown, null)));
                    };
                }
            });
        }

        private async Task<Receipt> PurchaseProductAsync(Product product)
        {
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
