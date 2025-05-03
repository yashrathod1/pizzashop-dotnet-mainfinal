namespace pizzashop_repository.ViewModels;

public class MenuAppSaveOrderViewModel
{
    public List<MenuAppAddItemViewModel> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public List<MenuAppOrderTaxSummaryViewModel> Taxes { get; set; } = new();
}
