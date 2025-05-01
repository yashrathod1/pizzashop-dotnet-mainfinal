using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class WaitingListService : IWaitingListService
{
    private readonly IWaitingListRepository _waitingListRepository;

    public WaitingListService(IWaitingListRepository waitingListRepository)
    {
        _waitingListRepository = waitingListRepository;
    }

    public async Task<WaitingListViewModel> GetSectionAsync()
    {
        List<Section> section = await _waitingListRepository.GetSectionAsync();

        var WaitingList = await _waitingListRepository.GetAllWaitingList();

        WaitingListViewModel? viewModel = new WaitingListViewModel
        {
            Sections = section.Select(s => new WaitingListSectionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                WaitingListCount = WaitingList.Count(w => w.SectionId == s.Id && w.IsAssign == false)
            }).ToList()
        };

        return viewModel;
    }

    public async Task<List<OrderAppSectionViewModel>> GetAllSectionsAsync()
    {
        var sections = await _waitingListRepository.GetSectionAsync();

        return sections.Select(s => new OrderAppSectionViewModel
        {
            Id = s.Id,
            Name = s.Name,
        }).ToList();
    }

    public async Task<WaitingListViewModel> GetWaitingListAsync(int? sectionId)
    {
        List<WaitingToken> waitingTokens = await _waitingListRepository.GetWaitingListAsync(sectionId);

        WaitingListViewModel? viewModel = new WaitingListViewModel
        {
            WaitingList = waitingTokens.Select(w => new WaitingListItemViewModel
            {
                Id = w.Id,
                CreatedAt = w.Createdat,
                WaitingTime = $"{(DateTime.Now - w.Createdat).Days} Days {(DateTime.Now - w.Createdat).Hours} hours {(DateTime.Now - w.Createdat).Minutes} min {(DateTime.Now - w.Createdat).Seconds} sec",
                Name = w.Customer.Name,
                Email = w.Customer.Email,
                NoOfPerson = w.NoOfPersons,
                PhoneNumber = w.Customer.PhoneNumber
            }).ToList(),

            SectionId = sectionId
        };

        return viewModel;
    }

    public async Task<bool> AddWaitingTokenInWaitingListAsync(WaitingTokenViewModel waitingTokenVm)
    {
        var existingCustomer = await _waitingListRepository.GetCustomerByEmail(waitingTokenVm.Email);

        Customer customer;

        if (existingCustomer == null)
        {
            customer = new Customer
            {
                Name = waitingTokenVm.Name,
                Email = waitingTokenVm.Email,
                PhoneNumber = waitingTokenVm.MobileNo
            };

            await _waitingListRepository.AddCustomer(customer);
        }
        else
        {
            customer = existingCustomer;

            var existingToken = await _waitingListRepository.GetWaitingTokenByCustomerId(customer.Id);
            if (existingToken != null)
            {
                return false;
            }
        }

        var waitingToken = new WaitingToken
        {
            CustomerId = customer.Id,
            NoOfPersons = waitingTokenVm.NoOfPerson,
            SectionId = waitingTokenVm.SectionId
        };

        var result = await _waitingListRepository.AddWaitingToken(waitingToken);

        return result;
    }

    public async Task<CustomerViewModel?> GetCustomerByEmail(string email)
    {
        try
        {
            Customer? customer = await _waitingListRepository.GetCustomerByEmail(email);
            if (customer == null) return null;

            var viewModel = new CustomerViewModel
            {
                Id = customer.Id,
                Email = customer.Email,
                Name = customer.Name,
                MobileNo = customer.PhoneNumber,
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching token details", ex);
        }
    }

    public async Task<WaitingListItemViewModel?> GetTokenByIdAsync(int id)
    {
        var token = await _waitingListRepository.GetWaitingTokenById(id);

        if (token == null) return null;

        return new WaitingListItemViewModel
        {
            Id = token.Id,
            Name = token.Customer.Name,
            Email = token.Customer.Email,
            PhoneNumber = token.Customer.PhoneNumber,
            NoOfPerson = token.NoOfPersons,
            SectionId = token.SectionId

        };
    }

    public async Task<(bool success, string message)> EditWaitingTokenAsync(WaitingTokenViewModel model)
    {
        try
        {
            var waitingToken = await _waitingListRepository.GetWaitingTokenById(model.Id);

            if (waitingToken == null)
            {
                return (false, "Waiting token not found.");
            }

            if (waitingToken.Customer.Name == model.Name &&
                waitingToken.Customer.PhoneNumber == model.MobileNo &&
                waitingToken.SectionId == model.SectionId &&
                waitingToken.NoOfPersons == model.NoOfPerson &&
                waitingToken.Customer.Email == model.Email)
            {
                return (false, "No changes detected.");
            }

            waitingToken.Customer.Name = model.Name;
            waitingToken.Customer.PhoneNumber = model.MobileNo;
            waitingToken.SectionId = model.SectionId;
            waitingToken.NoOfPersons = model.NoOfPerson;
            waitingToken.Customer.Email = model.Email;

            await _waitingListRepository.UpdateAsync(waitingToken);

            return (true, "Waiting token updated successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception("Error in updating waiting token", ex);
        }
    }

    public async Task<bool> SoftDeleteTokenAsync(int id)
    {
        try
        {
            var token = await _waitingListRepository.GetWaitingTokenById(id);
            if (token == null)
                return false;

            token.Isdeleted = true;

            await _waitingListRepository.UpdateAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while soft deleting the token.", ex);
        }
    }


}
