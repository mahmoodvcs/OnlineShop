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
        $.post("/Cart/AddToCart", { id: recordid },
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
