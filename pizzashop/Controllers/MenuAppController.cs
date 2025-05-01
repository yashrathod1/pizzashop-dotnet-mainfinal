using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

public class MenuAppController : Controller
{
    private readonly IMenuAppService _menuAppService;

    public MenuAppController(IMenuAppService menuAppService)
    {
        _menuAppService = menuAppService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int orderId)
    {
        ViewBag.orderId = orderId;
        ViewBag.ActiveNav = "Menu";
        var categories = await _menuAppService.GetCategoriesAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(string type, int? categoryId = null)
    {
        List<MenuAppItemViewModel> items;

        switch (type?.ToLower())
        {
            case "all":
                items = await _menuAppService.GetItemsAsync();
                break;

            case "favorite":
                items = await _menuAppService.GetItemsAsync(isFavourite: true);
                break;

            case "category":
                items = await _menuAppService.GetItemsAsync(categoryId: categoryId);
                break;

            default:
                items = new List<MenuAppItemViewModel>();
                break;
        }

        return PartialView("_ItemCardPartial", items);
    }


    [HttpPost]
    public async Task<IActionResult> ToggleIsFavourite(int id)
    {
        bool isFavourite = await _menuAppService.ToggleIsFavourite(id);
        return Json(isFavourite);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifierInItemCard(int id)
    {
        var modifier = await _menuAppService.GetModifierInItemCardAsync(id);
        return PartialView("_ModifierItemPartial", modifier);
    }

    [HttpGet]
    public async Task<IActionResult> GetTableDetailsByOrderId(int orderId)
    {
        try
        {
            var result = await _menuAppService.GetTableDetailsByOrderIdAsync(orderId);
            return Json(new
            {
                floorName = result.SectionName,
                assignedTables = string.Join(", ", result.TableName)
            });
        }
        catch (Exception ex)
        {
            return Json(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddOrderItemPartial(int ItemId, List<int> ModifierIds)
    {
        try
        {
            var model = await _menuAppService.AddItemInOrder(ItemId, ModifierIds);
            return PartialView("_OrderItemPartial", model);
        }
        catch (Exception ex)
        {
            return Json(ex.Message);
        }
    }


}
