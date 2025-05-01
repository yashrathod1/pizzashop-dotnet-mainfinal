using pizzashop_repository.Models;

namespace pizzashop_repository.Interface;

public interface IWaitingListRepository
{
    Task<List<Section>> GetSectionAsync();

    Task<List<WaitingToken>> GetWaitingListAsync(int? sectionId);

    Task<bool> AddWaitingToken(WaitingToken token);

    Task<bool> AddCustomer(Customer customer);

    Task<Customer?> GetCustomerByEmail(string email);

    Task<WaitingToken?> GetWaitingTokenByCustomerId(int customerId);

    Task<WaitingToken?> GetWaitingTokenById(int id);

    Task<List<WaitingToken>> GetAllWaitingList();

     Task UpdateAsync(WaitingToken waitingToken);
}
