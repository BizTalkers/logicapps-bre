namespace ProcessManager
{
    public class ContosoPurchase
    {
        /// <summary>
        /// Purchase amount
        /// </summary>
        public int PurchaseAmount { get; set; }

        /// <summary>
        /// Zip code
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// tax rate
        /// </summary>
        public float TaxRate { get; set; }

        /// <summary>
        /// Constructor for Purchase
        /// </summary>
        public ContosoPurchase(int purchaseAmount, string zipCode)
        {
            this.PurchaseAmount = purchaseAmount;
            this.ZipCode = zipCode;
        }

        /// <summary>
        /// Business logic to calculate sales tax
        /// </summary>
        public int GetSalesTax() => (int)((PurchaseAmount * TaxRate) / 100);
    }

}