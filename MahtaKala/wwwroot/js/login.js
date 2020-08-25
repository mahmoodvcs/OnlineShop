$(document).on("submit", "form#FirstRequest", function (e) {  
    e.preventDefault();
    $.blockUI({ message: '<img src="/img/loading.gif" />' });
    var action = $(this).attr('action');
    var formdata = new FormData($('#FirstRequest').get(0));
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
            if (res.success)
            {
                $("#uxDivContent").load("/Account/Confirm/" + res.id);
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
//$(document).on("submit", "form#Confirm", function (e) {
//    e.preventDefault();
//    $.blockUI({ message: '<img src="/img/loading.gif" />' });
//    var action = $(this).attr('action');
//    var formdata = new FormData($('#Confirm').get(0));
//    $.ajax({
//        type: "POST",
//        dataType: "json",
//        url: action,
//        cache: false,
//        data: formdata,
//        processData: false,
//        contentType: false,
//        beforeSend: function () {
//        },
//        success: function (res) {
//            $.unblockUI();
//            if (res.success) {
                
//                $("#uxDivContent").load("/Account/ResultRequest/");
//            }
//            else {
//                toastr.warning(res.msg, '', { positionClass: "toast-bottom-center" });
//            }

//        },
//        error: function (xhr) {
//            $.unblockUI();
//            swal("خطایی در ثبت اطلاعات اتفاق افتاده است", "", "warning");
//        },
//        complete: function () {
//            $.unblockUI();
//        }
//    });
//    return false;
//});