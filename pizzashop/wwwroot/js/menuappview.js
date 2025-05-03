
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
            $('#orderedItemsList .no-data').remove();
            $('#orderedItemsList').append(html);
            $('#ModifiersList').modal('hide');
            selectedModifiers = {};
            loadOrderTaxPartial();

            $('#saveBtn').prop('disabled', false);
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
        loadOrderTaxPartial();
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
        loadOrderTaxPartial();
    }
});


$(document).on('click', '.delete-item', function () {
    $(this).closest('tr').remove();
    loadOrderTaxPartial();
    if ($('#orderedItemsList tr').length === 0) {
        $('#orderedItemsList').html(`
            <tr class="no-data">
                <td colspan="5" class="text-center">No data found</td>
            </tr>
        `);
        }
        $('#saveBtn, #completeBtn, #invoiceBtn').prop('disabled', true);
});

function calculateSubTotal() {
    let subTotal = 0;

    $('#orderedItemsList .order-row').each(function () {
        let itemAmount = parseFloat($(this).find('.item-amount').data('base-amount'));
        let modifierAmount = parseFloat($(this).find('.modifier-amount').data('base-amount'));
        let quantity = parseInt($(this).find('.value').text());

        if (!isNaN(itemAmount) && !isNaN(modifierAmount) && !isNaN(quantity)) {
            subTotal += (itemAmount + modifierAmount) * quantity;
        }
    });

    return subTotal;
}

function calculateAndRenderTaxSummary() {
    let subTotal = calculateSubTotal();
    let taxTotal = 0;

    $('#tax-summary .tax-amount').each(function () {
        let $this = $(this);
        let rate = parseFloat($this.data('rate'));
        let type = $this.data('type');
        let isDefault = $this.data('default') === true || $this.data('default') === "true";

        let amount = 0;

        if (type === "Flat Amount") {
            if (isDefault || $this.closest('.d-flex').find('.tax-checkbox').is(':checked')) {
                amount = rate;
            }
        } else if (type === "Percentage") {
            if (isDefault || $this.closest('.d-flex').find('.tax-checkbox').is(':checked')) {
                amount = (subTotal * rate) / 100;
            }
        }

        $this.text('₹' + amount.toFixed(2));
        taxTotal += amount;
    });

    let grandTotal = subTotal + taxTotal;
    $('#tax-total').text('₹' + taxTotal.toFixed(2));
    $('#grand-total').text('₹' + grandTotal.toFixed(2));
}



function loadOrderTaxPartial() {
    let subTotal = calculateSubTotal();

    let checkedTaxRates = [];
    $('.tax-checkbox:checked').each(function () {
        let rate = $(this).data('rate');
        checkedTaxRates.push(rate);
    });

    $.ajax({
        url: '/MenuApp/LoadOrderTaxSummary',
        type: 'GET',
        data: { subTotal: subTotal },
        success: function (html) {
            $('#orderTaxSummaryContainer').html(html);

            $('.tax-checkbox').each(function () {
                let rate = $(this).data('rate');
                if (checkedTaxRates.includes(rate)) {
                    $(this).prop('checked', true);
                }
            });

            $('.tax-checkbox').on('change', function () {
                calculateAndRenderTaxSummary();
            });

            calculateAndRenderTaxSummary();
        }
    });
}


// save order into db
$(document).on('click','#saveBtn' ,function () {
    var orderData = {
        items: [],
        subtotal: 0,
        total: 0,
        taxes: []
    };

    $('.order-row').each(function () {
        var $row = $(this);
        var itemId = $row.data('item-id');
        var quantity = parseInt($row.find('.quantity-input').val());
        var itemAmount = parseFloat($row.find('.item-amount').text().replace('₹', ''));
        var modifierAmount = parseFloat($row.find('.modifier-amount').text().replace('₹', ''));

        var modifiers = [];
        $row.find('.modifier-checkbox:checked').each(function () {
            modifiers.push({
                modifierId: $(this).data('modifier-id'),
                modifierPrice: parseFloat($(this).data('price'))
            });
        });

        orderData.items.push({
            itemId: itemId,
            quantity: quantity,
            itemAmount: itemAmount,
            modifierAmount: modifierAmount,
            modifiers: modifiers
        });

        orderData.subtotal += (itemAmount + modifierAmount);
    });

    $.ajax({
        url: '/Order/SaveOrder',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(orderData),
        success: function (response) {
            toastr.success('Order saved!'); 
        }
    });
});
