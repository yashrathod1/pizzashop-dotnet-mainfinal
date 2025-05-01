using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface IMenuAppService
{
     Task<MenuAppViewModel> GetCategoriesAsync();

     Task<List<MenuAppItemViewModel>> GetItemsAsync(int? categoryId = null, bool? isFavourite = null);

     Task<bool> ToggleIsFavourite(int id);

     Task<MenuAppModifierDetailViewModel> GetModifierInItemCardAsync(int id);

     Task<MenuAppTableSectionViewModel> GetTableDetailsByOrderIdAsync(int orderId);

     Task<MenuAppAddItemViewModel> AddItemInOrder(int itemId, List<int> modifierIds);
}
