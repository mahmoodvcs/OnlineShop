
$(document).on("click", ".addtocart", function (e) {
    e.preventDefault();
    $.blockUI({
        message: '<img src="/img/loading.gif"/>',
        css: {
            border: 'none',
            backgroundColor: 'transparent'
        }
    });
    var recordid = $(this).data("id");
    if (recordid != '') {
        $.ajax({
            url: "/Cart/AddToCart",
            data: { id: recordid },
            method: "POST",
            success: function (data) {
                $.unblockUI();
                if (data.success) {
                    $('#cart-status').text(data.count);
                    toastr.success("محصول مورد نظر به سبد خرید افزوده شد", '', { positionClass: "toast-bottom-left" });
                }
                else {
                    toastr.warning(data.message, '', { positionClass: "toast-bottom-left" });
                }
            },
            error: function (data) {
                $.unblockUI();
                swal.fire(data.responseJSON.message, "", "error");
            }
        });
    }
    else {
        $.unblockUI();
    }
});

$(document).on("click", "#addtocart", function (e) {
    e.preventDefault();
    $.blockUI({
        message: '<img src="/img/loading.gif"/>',
        css: {
            border: 'none',
            backgroundColor: 'transparent'
        }
    });
    var recordid = $(this).data("id");
    var quantity = $("#uxQuantity").val();
    if (recordid != '') {
        $.post("/Cart/AddToCart", { id: recordid, count: quantity },
            function (data) {
                $.unblockUI();
                if (data.success) {
                    $('#cart-status').text(data.count);
                    toastr.success("محصول مورد نظر به سبد خرید افزوده شد", '', { positionClass: "toast-bottom-left" });
                }
                else {
                    toastr.warning(data.msg, '', { positionClass: "toast-bottom-left" });
                }
            }
        );
    }
    else {
        $.unblockUI();
    }
});


$("#cartshop").hover(function () {
    $.ajax({
        url: '/Cart/ShoppingBag/',
        cache: false,
        success: function (html) { $("#cart-block-content").html(html); }
    });
});
$(document).on("click", ".removeCart", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    $.post("/Cart/RemoveFromCart", { id: id },
        function (data) {
            $.unblockUI();
            if (data.success) {
                $('#cart-status').text(data.count);
                $.ajax({
                    url: '/Cart/ShoppingBag/',
                    cache: false,
                    success: function (html) { $("#cart-block-content").html(html); }
                });
                toastr.success("حذف از سبد خرید با موفقیت انجام شد", '', { positionClass: "toast-bottom-left" });
            }
            else {
                toastr.warning(data.msg, '', { positionClass: "toast-bottom-left" });
            }
        }
    );
});

$(document).on("keyup mouseup", ".cartValue", function () {
    var id = $(this).data("id");
    var count = $(this).val();
    $.post("/Cart/UpdateCart", { id: id, count: count },
        function (data) {
            if (data.success) {
                $('#cart-status').text(data.count);
                $("#finalcost" + data.id).html(data.finalcostRow);
                $('#sumPrice').text(data.sumPrice);
                $('#sumFinalPrice').text(data.sumFinalPrice);
            }
        }
    );
});
$(document).on("click", ".productremove", function (e) {
    e.preventDefault();
    var id = $(this).data("id");
    $.post("/Cart/DeleteItemCart", { id: id },
        function (data) {
            $.unblockUI();
            if (data.success) {
                $('#cart-status').text(data.count);
                $('#sumPrice').text(data.sumPrice);
                $('#sumFinalPrice').text(data.sumFinalPrice);
                $("#itemRow" + data.id).remove();
                toastr.success("حذف از سبد خرید با موفقیت انجام شد", '', { positionClass: "toast-bottom-left" });
            }
            else {
                toastr.warning(data.msg, '', { positionClass: "toast-bottom-left" });
            }
        }
    );
});

$(document).on("submit", "form#cartRequest", function (e) {
    e.preventDefault();
    $.blockUI({
        message: '<img src="/img/loading.gif"/>',
        css: {
            border: 'none',
            backgroundColor: 'transparent'
        }
    });
    var action = $(this).attr('action');
    var formdata = new FormData($('#cartRequest').get(0));
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
      
            if (res.success) {
                window.location.href = res.msg;
                $.unblockUI();
            }
            else {
                $.unblockUI();
                toastr.warning(res.msg, '', { positionClass: "toast-bottom-center" });
            }

        },
        error: function (xhr) {
            $.unblockUI();
            swal.fire("خطایی در ثبت اطلاعات اتفاق افتاده است", "", "error" );
        },
        complete: function () {
            $.unblockUI();
        }
    });
    return false;
});


