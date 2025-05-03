using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class MenuAppService : IMenuAppService
{
    private readonly IMenuAppRepository _menuAppRepository;

    public MenuAppService(IMenuAppRepository menuAppRepository)
    {
        _menuAppRepository = menuAppRepository;
    }

    public async Task<MenuAppViewModel> GetCategoriesAsync()
    {
        var categories = await _menuAppRepository.GetCategoriesAsync();

        var model = new MenuAppViewModel
        {
            CategoryList = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList()
        };

        return model;
    }

    public async Task<List<MenuAppItemViewModel>> GetItemsAsync(int? categoryId = null, bool? isFavourite = null)
    {
        IQueryable<MenuItem> query = _menuAppRepository.GetAllItemsQuery();

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.Categoryid == categoryId.Value);
        }

        if (isFavourite.HasValue)
        {
            query = query.Where(m => m.Isfavourite == isFavourite.Value);
        }

        var items = await query.ToListAsync();

        return items.Select(item => new MenuAppItemViewModel
        {
            Id = item.Id,
            Name = item.Name,
            ItemType = item.Type,
            IsFavourite = item.Isfavourite,
            Rate = item.Rate,
            ItemImagePath = item.ItemImage
        }).ToList();
    }


    public async Task<bool> ToggleIsFavourite(int id)
    {
        try
        {
            var items = await _menuAppRepository.GetItemById(id);
            if (items == null) return false;

            items.Isfavourite = !items.Isfavourite;

            await _menuAppRepository.UpdateItemAsync(items);

            return items.Isfavourite;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Fetching item by category", ex);
        }
    }

    public async Task<MenuAppModifierDetailViewModel> GetModifierInItemCardAsync(int id)
    {
        try
        {
            var item = await _menuAppRepository.GetItemById(id);
            if (item == null)
                throw new Exception("Item Not Found");

            var mapping = await _menuAppRepository.GetModifierInItemCardAsync(id);

            var modifierGroups = mapping.Select(mapping => new MenuAppItemModifierGroupViewModel
            {

                ModifierGroupName = mapping.ModifierGroup.Name,
                ModifierGroupId = mapping.ModifierGroup.Id,
                MinQuantity = mapping.MinModifierCount,
                MaxQuantity = mapping.MaxModifierCount,
                Modifiers = mapping.ModifierGroup.Modifiergroupmodifiers.Where(mgm => !mgm.Isdeleted && !mgm.Modifier.Isdeleted)
                                                                    .Select(mgm => new MenuAppItemModifiersViewModel
                                                                    {
                                                                        Id = mgm.Modifier.Id,
                                                                        Name = mgm.Modifier.Name,
                                                                        Amount = mgm.Modifier.Price
                                                                    }).ToList()


            }).ToList();

            return new MenuAppModifierDetailViewModel
            {
                ItemQuantity = item.Quantity,
                ItemId = item.Id,
                ItemName = item.Name,
                ModifierGroups = modifierGroups
            };

        }
        catch (Exception ex)
        {
            throw new Exception("Error getting modifier in item card", ex);
        }
    }

    public async Task<MenuAppTableSectionViewModel> GetTableDetailsByOrderIdAsync(int orderId)
    {
        var mappings = await _menuAppRepository.GetTableDetailsByOrderIdAsync(orderId);

        if (mappings == null || mappings.Count == 0)
            throw new Exception("No table mappings found for this order.");

        var sectionName = mappings.First().Table.Section.Name;

        var tableNames = mappings
            .Select(m => m.Table.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        return new MenuAppTableSectionViewModel
        {
            SectionName = sectionName,
            TableName = tableNames
        };
    }



    public async Task<MenuAppAddItemViewModel> AddItemInOrder(int itemId, List<int> modifierIds)
    {
        var item = await _menuAppRepository.GetItemById(itemId);
        if (item == null)
            throw new ArgumentException("Invalid item ID");

        var selectedModifiers = await _menuAppRepository.GetModifiersByIds(modifierIds);

        var totalModifierPrice = selectedModifiers.Sum(m => m.Price);

        return new MenuAppAddItemViewModel
        {
            Id = item.Id,
            ItemName = item.Name,
            ItemAmount = item.Rate,
            ItemQuantity = item.Quantity,
            TotalModifierAmount = totalModifierPrice,
            SelectedModifiers = selectedModifiers.Select(m => new MenuAppAddModifierViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Amount = m.Price,
            }).ToList(),
        };
    }

    public async Task<MenuAppOrderSummaryViewModel> GetOrderSummaryAsync(decimal subTotal)
    {
        var taxes = await _menuAppRepository.GetAllTaxesAsync();

        var taxSummaries = taxes.Select(t => new MenuAppOrderTaxSummaryViewModel
        {
            Name = t.Name,
            Value = t.Value,
            IsEnable = t.Isenabled,
            IsDefault = t.Isdefault,
            Type = t.Type
        }).ToList();

        return new MenuAppOrderSummaryViewModel
        {
            Subtotal = subTotal,
            Taxes = taxSummaries
        };
    }


}
