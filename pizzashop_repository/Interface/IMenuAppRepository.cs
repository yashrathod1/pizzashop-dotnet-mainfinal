using pizzashop_repository.Models;

namespace pizzashop_repository.Interface;

public interface IMenuAppRepository
{
     Task<List<Category>> GetCategoriesAsync();

     IQueryable<MenuItem> GetAllItemsQuery();

     Task<MenuItem?> GetItemById(int Id);

     Task<bool> UpdateItemAsync(MenuItem item);

     Task<List<MappingMenuItemWithModifier>> GetModifierInItemCardAsync(int id);

     Task<List<OrdersTableMapping>> GetTableDetailsByOrderIdAsync(int orderId);

     Task<List<Modifier>> GetModifiersByIds(List<int> ids);

     Task<List<Taxesandfee>> GetAllTaxesAsync();
}
