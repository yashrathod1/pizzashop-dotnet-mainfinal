
$(document).ready(function () {

    $('#WaitingToken').on('hidden.bs.modal', function () {
        $(this).find('input[type="text"], input[type="email"], input[type="hidden"], textarea').val('');
        $(this).find('span.text-danger').text('');
    });

    loadWaitingList(null);

    $(".nav-link").on("click", function () {
        const sectionId = $(this).data("id");
        loadWaitingList(sectionId);
    });

});

function loadWaitingList(sectionId) {
    const containerId = sectionId ? `#waitingListContainer-${sectionId}` : "#waitingListContainer";

    $.ajax({
        url: '/WaitingList/GetWaitingListBySection',
        type: 'GET',
        data: { sectionId: sectionId },
        success: function (html) {
            $(containerId).html(html);
        },
        error: function () {
            $(containerId).html('<p class="text-danger">Failed to load waiting list.</p>');
        }
    });
}

// get customer by email
$(document).on('blur', '#customerEmail', function () {
    var email = $(this).val();
    if (email) {
        $.ajax({
            url: '/WaitingList/GetCustomerByEmail',
            type: 'GET',
            data: { email: email },
            success: function (data) {
                if (data) {
                    $('#customerName').val(data.name);
                    $('#customerMobile').val(data.mobileNo);
                    $('#customerTokenId').val(data.id);
                    toastr.success("Customer Details Loaded Successfull");
                } else {
                    $('#customerName, #customerMobile, #totalPerson').val('');

                }
            }
        });
    }
});

$(document).on("submit", "#AddWaitingTokenTable", function (e) {
    e.preventDefault();

    if (!$(this).valid()) {
        return false;
    }

    var formData = new FormData(this);

    $.ajax({
        url: '/WaitingList/CreateWaitingToken',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                $('#WaitingToken').modal('hide');
                $('#AddWaitingTokenTable')[0].reset();
                toastr.success(response.message);
            } else {
                toastr.error(response.message);
            }

        },
        error: function (error) {
            toastr.error(error.message);
        }
    });
});

// dropdown in add waiting token the modal

$(document).on("click", ".wait-token-btn", function () {

    var sectionId = $(".nav-tabs .nav-link.active").data("id");

    $.ajax({
        url: '/WaitingList/GetSections',
        type: 'GET',
        success: function (sections) {
            var $dropdown = $("#WaititngTokensectionId");
            $dropdown.empty();
            $.each(sections, function (i, section) {
                var isSelected = section.id === sectionId;
                $dropdown.append($('<option>', {
                    value: section.id,
                    text: section.name,
                    selected: isSelected
                }));
            });

            $("#selectedSectionId").val(sectionId)
        },
        error: function () {
            alert('Failed to load sections.');
        }
    });
});

// get detail on click pencil

$(document).on("click", ".edit-waiting-token", function () {
    var tokenId = $(this).data("id");

    $.ajax({
        url: '/WaitingList/GetSections',
        type: 'GET',
        success: function (sections) {
            var $dropdown = $("#eWaititngTokensectionId");
            $dropdown.empty();
            $.each(sections, function (i, section) {
                $dropdown.append($('<option>', {
                    value: section.id,
                    text: section.name,
                }));
            });
        },
        error: function () {
            alert('Failed to load sections.');
        }
    });

    $.ajax({
        url: '/WaitingList/GetTokenById',
        type: 'GET',
        data: { id: tokenId },
        success: function (data) {
            if (data) {
                $('#tokenId').val(data.id);
                $('#ecustomerName').val(data.name);
                $('#ecustomerEmail').val(data.email);
                $('#ecustomerMobile').val(data.phoneNumber);
                $('#enop').val(data.noOfPerson);
                $('#eWaititngTokensectionId').val(data.sectionId);
            } else {
                toastr.error("Token not found.");
            }
            $('#editWaitingToken').modal('show');
        },
        error: function () {
            toastr.error("Error fetching token.");
        }
    });
});


// edit waiting token

$(document).on("submit", "#EditWaitingTokenTable", function (e) {
    e.preventDefault();

    if (!$(this).valid()) {
        return;
    }

    var sectionId = $(".nav-tabs .nav-link.active").data("id");

    formData = {
        Id: $("#tokenId").val(),
        Name: $("#ecustomerName").val(),
        Email: $("#ecustomerEmail").val(),
        MobileNo: $("#ecustomerMobile").val(),
        SectionId: $("#eWaititngTokensectionId").val(),
        NoOfPerson: $("#enop").val()
    };

    $.ajax({
        url: "/WaitingList/EditWaitingToken",
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                toastr.success("WaitingToken edited successfully");
                $("#editWaitingToken").modal("hide");
                $("#EditWaitingTokenTable")[0].reset();
                loadWaitingList(sectionId);

            } else {
                toastr.error(response.message);
            }
        },
        error: function () {
            toastr.error("Error updating modifier group.");
        }
    });
});


// delete the waitingtoken

$(document).on('click', '.token-delete-btn', function () {
    var id = $(this).data('id');
    $('#deleteWaitingTokenModal').modal('show');
    $('#confirmDeleteBtnToken').data('id', id);
});

$('#confirmDeleteBtnToken').on('click', function () {
    var id = $(this).data('id');
    var sectionId = $(".nav-tabs .nav-link.active").data("id");

    $.ajax({
        url: '/WaitingList/SoftDeleteToken',
        type: 'POST',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                $('#deleteWaitingTokenModal').modal('hide');
                loadWaitingList(sectionId);
            } else {
                toastr.error(response.message);
            }
        },
        error: function () {
            alert('An error occurred while attempting to delete the waiting token.');
        }
    });

});

