
$(document).on("submit", "form#EditPhoneNumber", function (e) {
    e.preventDefault();
    $.blockUI({
        message: '<img src="/img/loading.gif"/>',
        css: {
            border: 'none',
            backgroundColor: 'transparent'
        }
    });
    var action = $(this).attr('action');
    var formdata = new FormData($('#EditPhoneNumber').get(0));
    $.ajax({
        type: "POST",
        dataType: "json",
        url: action,
        cache: false,
        data: formdata,
        processData: false,
        contentType: false,
        beforeSend: function () {
        },
        success: function (res) {
            $.unblockUI();
            if (res.success) {
                $("#uxDivContent").load("/Profile/ConfirmPhoneNumber/" + res.id);
            }
            else {
                toastr.warning(res.msg, '', { positionClass: "toast-bottom-center" });
            }

        },
        error: function (xhr) {
            $.unblockUI();
            swal("خطایی در ثبت اطلاعات اتفاق افتاده است", "", "warning");
        },
        complete: function () {
            $.unblockUI();
        }
    });
    return false;
});



$(document).on("submit", "form#ConfirmPhoneNumber", function (e) {
    e.preventDefault();
    $.blockUI({
        message: '<img src="/img/loading.gif"/>',
        css: {
            border: 'none',
            backgroundColor: 'transparent'
        }
    });
    var action = $(this).attr('action');
    var formdata = new FormData($('#ConfirmPhoneNumber').get(0));
    $.ajax({
        type: "POST",
        dataType: "json",
        url: action,
        cache: false,
        data: formdata,
        processData: false,
        contentType: false,
        beforeSend: function () {
        },
        success: function (res) {
            $.unblockUI();
            if (res.success) {
                window.location.href = "/profile/profileEdit";
            }
            else {
                toastr.warning(res.msg, '', { positionClass: "toast-bottom-center" });
            }

        },
        error: function (xhr) {
            $.unblockUI();
            swal("خطایی در ثبت اطلاعات اتفاق افتاده است", "", "warning");
        },
        complete: function () {
            $.unblockUI();
        }
    });
    return false;
});