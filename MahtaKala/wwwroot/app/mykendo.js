
function getAdditionalData() {
    var data = {};
    $(".additionalData").each(function (i, el) {
        data[$(el).attr("name")] = $(el).val();
    });
    return data;
}

$(function () {
    $(".additionalData").change(function () {
        var g = $("div[data-role='grid']").data("kendoGrid");
        if (g) {
            g.dataSource.read();
            g.refresh();
        }
    });
});