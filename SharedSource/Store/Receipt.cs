namespace MarkerMetro.Unity.WinIntegration.Store
{
    public enum PurchaseType
    {
        Application,
        Product
    }

    public enum StatusCode
    {
        Success,
        UnknownError,
        NotReady,
        ConsumablePending,
        DurableOwned,
        UserCancelled,
        ExceptionThrown,
        InvalidProduct
    }

    public class Receipt
    {
        public PurchaseType PurchaseType { get; private set; }
        public Product Product { get; private set; }
        public StatusCode Status { get; private set; }
        public bool Success { get { return Status == StatusCode.Success; } }
        public string StoreReceipt { get; private set; }

        /// <summary>
        /// ctor for product purchases
        /// </summary>
        public Receipt(Product product, StatusCode status, string storeReceipt)
        {
            PurchaseType = PurchaseType.Product;
            Product = product;
            Status = status;
            StoreReceipt = storeReceipt;
        }

        /// <summary>
        /// ctor for application purchases
        /// </summary>
        public Receipt(StatusCode status, string storeReceipt)
        {
            PurchaseType = PurchaseType.Application;
            Status = status;
            StoreReceipt = storeReceipt;
        }
    }
}