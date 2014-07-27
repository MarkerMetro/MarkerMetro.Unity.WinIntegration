using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;

namespace MarkerMetro.Unity.WinIntegration.Store
{

    internal class WP8Platform : IStorePlatform
    {

        private bool _useSimulator;
        private StoreManager _store;

        public bool TrialMode
        {
            get
            {
                if (_useSimulator)
                    return MockIAPLib.CurrentApp.LicenseInformation.IsTrial;
                else
                    return CurrentApp.LicenseInformation.IsTrial;
            }
        }

        public bool ActiveLicense
        {
            get
            {
                if (_useSimulator)
                    return MockIAPLib.CurrentApp.LicenseInformation.IsActive;
                else
                    return CurrentApp.LicenseInformation.IsActive;
            }
        }

        public void Load(bool simulator, StoreManager storeInst)
        {
            _useSimulator = simulator;
            _store = storeInst;
            if (_useSimulator)
                LoadSimulator();  
        }

        private void LoadSimulator()
        {
            MockIAPLib.MockIAP.Init();
            MockIAPLib.MockIAP.RunInMockMode(true);
            MockIAPLib.MockIAP.SetListingInformation(1, "en-us", "Mock App Description", "1.98", "Mock App Title");
            StreamResourceInfo xml = Application.GetResourceStream(new Uri("iap_simulator.xml", UriKind.Relative));
            XElement appDataXml = XElement.Load(xml.Stream);
            MockIAPLib.MockIAP.PopulateIAPItemsFromXml(appDataXml.ToString());
            MockIAPLib.MockIAP.ClearCache();
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
                return await MockIAPLib.CurrentApp.RequestAppPurchaseAsync(false);
            }
            else
            {
                return await CurrentApp.RequestAppPurchaseAsync(false);
            }
        }

        public void RetrieveProducts(ProductListDelegate callback)
        {
            if (_store == null) return;
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
            if (_useSimulator)
            {
                var mockListing = await MockIAPLib.CurrentApp.LoadListingInformationAsync();
                var mockLicense = MockIAPLib.CurrentApp.LicenseInformation;
                if (mockListing != null && mockLicense != null)
                {
                    var products = mockListing.ProductListings;
                    var results = new List<Product>();
                    foreach (var p in products.Values)
                    {
                        var product = new Product();
                        product.ProductID = p.ProductId;
                        product.Name = p.Name;
                        product.FormattedPrice = p.FormattedPrice;

                        // Determine license status
                        product.Purchased = mockLicense.ProductLicenses[product.ProductID].IsActive;
                        if (product.Purchased)
                            product.Expires = mockLicense.ProductLicenses[product.ProductID].ExpirationDate.DateTime;

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
            }
            else
            {
                var listing = await CurrentApp.LoadListingInformationAsync();
                var license = CurrentApp.LicenseInformation;
                if (listing != null && license != null)
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
            }
            return null;
        }

        public void PurchaseProduct(Product product, PurchaseDelegate callback)
        {
            if (_store == null) return;

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
                catch (Exception)
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
            if (_useSimulator)
            {
                var purchaseResult = await MockIAPLib.CurrentApp.RequestProductPurchaseAsync(product.ProductID, true);
                var productLicenses = MockIAPLib.CurrentApp.LicenseInformation.ProductLicenses;
                var license = productLicenses[product.ProductID];
                if (!license.IsActive)
                {
                    return new Receipt(product, StatusCode.NotReady, purchaseResult);
                }
                if (license.IsConsumable)
                {
                    MockIAPLib.CurrentApp.ReportProductFulfillment(product.ProductID);
                }
                return new Receipt(product, StatusCode.Success, purchaseResult);
            }
            else
            {
                var purchaseResult = await CurrentApp.RequestProductPurchaseAsync(product.ProductID, true);
                var productLicenses = CurrentApp.LicenseInformation.ProductLicenses;
                var license = productLicenses[product.ProductID];
                if (!license.IsActive)
                {
                    return new Receipt(product, StatusCode.NotReady, purchaseResult);
                }
                if (license.IsConsumable)
                {
                    CurrentApp.ReportProductFulfillment(product.ProductID);
                }
                return new Receipt(product, StatusCode.Success, purchaseResult);
            }

        }

    }
}
