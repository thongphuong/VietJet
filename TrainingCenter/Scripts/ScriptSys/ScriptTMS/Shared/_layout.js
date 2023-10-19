


//$(window).load(function () {
//    getAllMessages();
//});
//function getAllMessages() {
//    var tbl = $('#messagesTable');


//        $.when(sys.CallAjaxReturnHtml("/Home/GetMessages")
//        ).done(function (result) {
//        tbl.empty().append(result);

//        });

//        var tbl2 = $('#messagesTable2');
//        $.when(sys.CallAjaxReturnHtml("/Home/GetMessages2")
//        ).done(function (result) {
//            tbl2.empty().append(result);
//        });
       
//}

function changelang(type) {
    var data = { lang: type };
    $.when(sys.CallAjaxPost("/Home/Change", data)
).done(function (result) {
    location.reload();
});
}


function viewnotification(MessageID) {
    var data = { MessageID: MessageID };
    $.when(sys.CallAjaxPost("/Home/viewnotification", data)
).done(function (result) {
    getAllMessages();
});
}

function MarkAllasRead() {
    $.when(sys.CallAjaxPost("/Home/MarkAllasRead")
).done(function (result) {
    getAllMessages();
});
}



$.extend(true, $.fn.dataTable.defaults, {
    "oLanguage": {
        "sProcessing": "<div class='loader-md'></div>"
    }
});

$(document).ready(function () {
    var newURL = window.location.pathname;
    $("ul a[href='" + newURL + "']").addClass('li_a_active');

    $(':input[type=number]').on('mousewheel', function (e) { $(this).blur(); });
});