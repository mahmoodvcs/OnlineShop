
function post(url, data, success, additionalParams) {
    var obj = {
        method: "POST",
        url: url,
        data: data,
        success: success,
        error: handleAjaxError
    };
    obj = $.extend(obj, additionalParams);
    $.ajax(obj);
}


function handleAjaxError(xhr) {
    try {
        var obj = JSON.parse(xhr.responseText);
        if (obj.message || obj.msg) {
            swal.fire(obj.message || obj.msg, "", "error");
        }
    }
    catch { }
    finally {
        $.unblockUI();
    }
}