
$(document).ready(function () {

    $('#ModifierList').on('hidden.bs.modal', function () {
        $('.modifier-box').removeClass('selected');
        selectedModifiers = {}
    });

    var orderId = $('#orderId').val();

    LoadItems("all")
    fetchTableInfo(orderId);

    $('.category-item').on('click', function () {
        let type = $(this).data('type');
        let categoryId = $(this).data('category-id');

        $('.category-item').removeClass('selected text-white').addClass('text-muted');
        $(this).addClass('selected text-white').removeClass('text-muted');
        LoadItems(type, categoryId)

    });
});

function LoadItems(type, categoryId) {
    $.ajax({
        url: '/MenuApp/GetItems',
        type: 'GET',
        data: { type: type, categoryId: categoryId },
        success: function (result) {
            $('#ItemsCardContainer').html(result);
        },
        error: function () {
            $('#ItemsCardContainer').html('<div class="text-danger fs-5">Failed to load items.</div>');
        }
    });
}



$(document).on('click', '.favourite', function () {
    const icon = $(this);
    const itemId = icon.data('itemid');

    $.ajax({
        url: '/MenuApp/ToggleIsFavourite',
        type: 'POST',
        data: { id: itemId },
        success: function (isFavourite) {
            if (isFavourite) {
                icon.removeClass('fa-regular text-secondary')
                    .addClass('fa-solid heart-color');

            } else {
                icon.removeClass('fa-solid heart-color')
                    .addClass('fa-regular text-secondary');
            }
        },
        error: function () {
            toastr.error("Failed to toggle favourite.");
        }
    });
});

// modifier in item card


$(document).on('click', '.item-card', function () {

    const itemId = $(this).data("id");
    console.log("hdfsh", itemId);

    selectedModifiers = {};

    $.ajax({
        url: '/MenuApp/GetModifierInItemCard',
        type: 'GET',
        data: { id: itemId },
        success: function (result) {
            $('#modifierDetailContainer').html(result);
            $('#ModifiersList').modal('show');
            checkMinSelection();
        },
        error: function () {
            toastr.error("Error loading Modifier");
        }
    });
});

//fetch order and table i--nfo
function fetchTableInfo(orderId) {
    $.ajax({
        url: '/MenuApp/GetTableDetailsByOrderId',
        type: 'GET',
        data: { orderId: orderId },
        success: function (response) {
            $('#sectionName').text(response.floorName);
            $('#assignedTables').text(response.assignedTables);
        },
        error: function () {
            alert('Error fetching table information.');
        }
    });
}


//modifiermodal
var selectedModifiers = {};

$(document).on('click', '.modifier-box', function () {
    var groupDiv = $(this).closest('.mb-3');

    var modifierGroupId = groupDiv.data('modifier-group-id');
    var modifierId = $(this).data('modifier-id');
    var maxCount = parseInt(groupDiv.data('max-quantity'));

    if (!selectedModifiers[modifierGroupId]) {
        selectedModifiers[modifierGroupId] = [];
    }

    var isSelected = $(this).hasClass('selected');

    if (!isSelected) {
        if (selectedModifiers[modifierGroupId].length < maxCount) {
            $(this).addClass('selected');
            selectedModifiers[modifierGroupId].push({ modifierId: modifierId });
        } else {
            toastr.warning(`Maximum limit is ${maxCount}`);
        }
    } else {
        $(this).removeClass('selected');
        selectedModifiers[modifierGroupId] = selectedModifiers[modifierGroupId].filter(function (mod) {
            return mod.modifierId !== modifierId;
        });
    }

    checkMinSelection();
    console.log("selectedModifiers", selectedModifiers);
});



function checkMinSelection() {
    var allGroupsSelected = true;

    $('.mb-3').each(function () {
        var minQuantity = parseInt($(this).data('min-quantity')) || 0;
        var maxQuantity = parseInt($(this).data('max-quantity')) || 0;
        var selectedCount = $(this).find('.modifier-box.selected').length;

        if (selectedCount < minQuantity || (maxQuantity > 0 && selectedCount > maxQuantity)) {
            allGroupsSelected = false;
        }
    });

    $('#AddToOrder').prop('disabled', !allGroupsSelected);
}

// add order of item and modifier
$(document).on('click', '#AddToOrder', function () {
    var itemId = $('#itemTitle').data('item-id');
    var availableQty = parseInt($('#itemTitle').data('available-qty'));
    var selectedModifierIds = [];

    var totalUsedQty = 0;

    $('.order-row').each(function () {
        if ($(this).data('item-id') == itemId) {
            var qty = parseInt($(this).find('.value').text());
            totalUsedQty += qty;
        }
    });

    if (totalUsedQty >= availableQty) {
        toastr.warning(`You cannot add more. Available quantity(${availableQty}) is already used.`);
        return;
    }

    for (var groupId in selectedModifiers) {
        selectedModifiers[groupId].forEach(function (mod) {
            selectedModifierIds.push(mod.modifierId);
        });
    }

    $.ajax({
        type: 'POST',
        url: '/MenuApp/AddOrderItemPartial',
        data: {
            ItemId: itemId,
            ModifierIds: selectedModifierIds
        },
        success: function (html) {
            $('#orderedItemsList').append(html);
            $('#ModifiersList').modal('hide');
            selectedModifiers = {};
        }
    });
});


// update item price as quantity increase and decrease

function updateAmounts($row, quantity) {
    var baseItemAmount = parseFloat($row.find('.item-amount').data('base-amount'));
    var baseModifierAmount = parseFloat($row.find('.modifier-amount').data('base-amount'));

    var totalItemAmount = baseItemAmount * quantity;
    var totalModifierAmount = baseModifierAmount * quantity;

    $row.find('.item-amount').text('₹' + totalItemAmount.toFixed(2));
    $row.find('.modifier-amount').text('₹' + totalModifierAmount.toFixed(2));
}

$(document).on('click', '.positive', function () {
    var $valueSpan = $(this).siblings('.value');
    var currentQty = parseInt($valueSpan.text());
    var $row = $(this).closest('tr');
    var itemId = $row.data('item-id');
    var availableQty = parseInt($row.data('available-qty'));

    var totalUsedQty = 0;

    $('.order-row').each(function () {
        if ($(this).data('item-id') == itemId) {
            totalUsedQty += parseInt($(this).find('.value').text());
        }
    });

    if (totalUsedQty < availableQty) {
        currentQty++;
        $valueSpan.text(currentQty);
        updateAmounts($row, currentQty);
    } else {
        toastr.warning(`You cannot select more quantity than available quantity ${availableQty}!`);
    }
});



$(document).on('click', '.negative', function () {
    var $valueSpan = $(this).siblings('.value');
    var quantity = parseInt($valueSpan.text());
    if (quantity > 1) {
        quantity--;
        $valueSpan.text(quantity);
        var $row = $(this).closest('tr');
        updateAmounts($row, quantity);
    }
});


$(document).on('click', '.delete-item', function () {
    $(this).closest('tr').remove();
});

