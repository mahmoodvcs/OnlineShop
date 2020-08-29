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
                if (data.success)
                {
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

