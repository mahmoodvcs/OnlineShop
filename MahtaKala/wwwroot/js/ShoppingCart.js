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
