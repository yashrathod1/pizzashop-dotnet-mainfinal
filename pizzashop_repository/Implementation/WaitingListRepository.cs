using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;

namespace pizzashop_repository.Implementation;

public class WaitingListRepository : IWaitingListRepository
{
    private readonly PizzaShopDbContext _context;

    public WaitingListRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Section>> GetSectionAsync()
    {
        return await _context.Sections.Where(w => !w.Isdeleted).ToListAsync();
    }

    public async Task<List<WaitingToken>> GetWaitingListAsync(int? sectionId)
    {
        var query = _context.WaitingTokens.Include(w => w.Customer).Where(w => !w.IsAssign && !w.Isdeleted);

        if (sectionId.HasValue)
        {
            query = query.Where(w => w.SectionId == sectionId);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> AddWaitingToken(WaitingToken token)
    {
        await _context.WaitingTokens.AddAsync(token);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AddCustomer(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Customer?> GetCustomerByEmail(string email)
    {
        return await _context.Customers.FirstOrDefaultAsync(w => w.Email == email);
    }

    public async Task<WaitingToken?> GetWaitingTokenByCustomerId(int customerId)
    {
        return await _context.WaitingTokens
            .FirstOrDefaultAsync(wt => wt.CustomerId == customerId && !wt.IsAssign);
    }

    public async Task<WaitingToken?> GetWaitingTokenById(int id)
    {
        return await _context.WaitingTokens.Include(wt => wt.Customer).FirstOrDefaultAsync(wt => wt.Id == id);
    }

    public async Task<List<WaitingToken>> GetAllWaitingList()
    {
        return await _context.WaitingTokens.Where(wt => !wt.Isdeleted && !wt.IsAssign).ToListAsync();
    }

    public async Task UpdateAsync(WaitingToken waitingToken)
    {
        _context.WaitingTokens.Update(waitingToken);
        await _context.SaveChangesAsync();
    }
}
