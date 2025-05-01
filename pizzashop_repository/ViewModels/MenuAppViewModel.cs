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
    public int Id {get; set;}
    public string ItemName { get; set ;} = null!;

    public decimal ItemAmount { get; set;}

    public int ItemQuantity { get; set; }

    public decimal TotalModifierAmount { get; set; }

    public List<MenuAppAddModifierViewModel> SelectedModifiers = new List<MenuAppAddModifierViewModel>();
}

public class MenuAppAddModifierViewModel
{   
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public decimal Amount {get; set;}

    

}