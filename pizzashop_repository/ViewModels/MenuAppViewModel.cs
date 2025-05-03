namespace pizzashop_repository.ViewModels;

public class MenuAppViewModel
{
    public List<CategoryViewModel> CategoryList { get; set; } = new();
}

public class MenuAppTableSectionViewModel
{
    public string? SectionName { get; set; }

    public List<string> TableName { get; set; } = null!;
}

public class MenuAppAddItemViewModel
{
    public int Id { get; set; }
    public string ItemName { get; set; } = null!;

    public decimal ItemAmount { get; set; }

    public int ItemQuantity { get; set; }

    public decimal TotalModifierAmount { get; set; }

    public List<MenuAppAddModifierViewModel> SelectedModifiers = new List<MenuAppAddModifierViewModel>();
}

public class MenuAppAddModifierViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

}


public class MenuAppOrderSummaryViewModel
{
    public decimal Subtotal { get; set; }
    public List<MenuAppOrderTaxSummaryViewModel> Taxes { get; set; } = new();
    public decimal Total => Subtotal + Taxes.Sum(t => Subtotal * t.Value / 100);
}

public class MenuAppOrderTaxSummaryViewModel
{

    public string Name { get; set; } = null!;

    public decimal Value { get; set; }

    public bool IsEnable { get; set; }
    public bool IsDefault { get; set; }

    public string Type { get; set; } = null!;
}