using System;

namespace MarkerMetro.Unity.WinIntegration.Store
{
    public enum ProductType
    {
        Consumable,
        Durable
    }

    public class Product
    {
        public ProductType Type { get; set; }
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string FormattedPrice { get; set; }
        public bool Purchased { get; set; }
        public DateTime Expires { get; set; }
    }
}