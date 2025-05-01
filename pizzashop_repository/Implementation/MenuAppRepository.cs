using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;

namespace pizzashop_repository.Implementation;

public class MenuAppRepository : IMenuAppRepository
{
    private readonly PizzaShopDbContext _context;

    public MenuAppRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.Where(c => !c.Isdeleted).OrderBy(m => m.Id).ToListAsync();
    }

    public IQueryable<MenuItem> GetAllItemsQuery()
    {
        try
        {
            return _context.MenuItems.Where(m => !m.IsDeleted);
        }
        catch
        {
            throw;
        }
    }


    public async Task<MenuItem?> GetItemById(int Id)
    {
        try
        {
            return await _context.MenuItems.Where(m => m.Id == Id && !m.IsDeleted).FirstOrDefaultAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> UpdateItemAsync(MenuItem item)
    {
        try
        {
            _context.MenuItems.Update(item);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the item in the database", ex);
        }
    }

    public async Task<List<MappingMenuItemWithModifier>> GetModifierInItemCardAsync(int id)
    {
        try
        {
            return await _context.MappingMenuItemWithModifiers.Include(mmim => mmim.ModifierGroup)
                                                        .ThenInclude(m => m.Modifiergroupmodifiers)
                                                        .ThenInclude(mgm => mgm.Modifier)
                                                        .Include(mmim => mmim.MenuItem)
                                                        .Where(mmim => mmim.MenuItemId == id).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error Fetching the modifer in itemcard", ex);
        }
    }

    public async Task<List<OrdersTableMapping>> GetTableDetailsByOrderIdAsync(int orderId)
    {
        try
        {
            return await _context.OrdersTableMappings.Include(ot => ot.Table).ThenInclude(t => t.Section).Where(o => o.Orderid == orderId).ToListAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Modifier>> GetModifiersByIds(List<int> ids)
    {
        try
        {
            return await _context.Modifiers.Where(m => ids.Contains(m.Id)).ToListAsync();
        }
        catch
        {
            throw;
        }
    }

}

