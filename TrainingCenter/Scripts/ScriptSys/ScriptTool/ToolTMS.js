var Shared = { Shared_layout: Shared_layout }
var Home = { Home_Index: Home_Index }
var Approve = { Approve_SubjectResult: Approve_SubjectResult}
////////////////////////////////////////////////////////////// Shared
function Shared_layout() {
   // $(document).ready(function () {
   ////     getAllMessages();
   // });

    //var notifications = $.connection.messagesHub;
    //$(document).ready(function (objchat) {
    //    notifications.client.updateMessages = function () {
    //        getAllMessages();

    //    };
    //    // Start the connection.
    //    $.connection.hub.start().done(function (objchat) {

    //        getAllMessages();
    //    }).fail(function (e) {
    //        alert(e);
    //    });
    //});
    //$(document).ready(function () {
    //    notifications.client.updateMessages = function () {
    //        getAllMessages();

    //    };
    //    // Start the connection.
    //    $.connection.hub.start().done(function () {

    //        getAllMessages();
    //    }).fail(function (e) {
    //        alert(e);
    //    });
    //});


    function getAllMessages() {
        var tbl = $('#messagesTable');
        var tbl2 = $('#messagesTable2');

        $.when(sys.CallAjaxReturnHtml("/Home/GetMessages")
    ).done(function (result) {
        tbl.empty().append(result);

    });
        $.when(sys.CallAjaxReturnHtml("/Home/GetMessages2")
    ).done(function (result) {
        tbl2.empty().append(result);
    });
    }
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
        
    });
}
////////////////////////////////////////////////////////////// Home
function Home_Index() {
    function OnChangeCourseList(val) {
        var data = { id_course: val };
        $.when(sys.CallAjaxPost("/Home/ChangeCourseReturnSubject", data)
    ).done(function (result) {
        $('#ddl_subject').empty();
        $('#ddl_subject').prop('selectedIndex', 0);
        if (result.value_null == "0") {
            $('#ddl_subject').append(result.value_option);
        }
        else {
            $('#ddl_subject').append("<option value='-1'>-- Subject --</option>");
        }
    });
    }
    $(document).ready(function () {
        $(".home").find("a").css("background-color", "#FFEA00 !important");
        $(".home").find("a").css("color", "#df3333");
        //sys.blockUI();

        var sTable;
        $("#frmFilter").submit(function (e) {
            sTable.draw();
            e.preventDefault();
        });
        var customRow = [{
            "targets": 0,
            "className": "text-center",
            "data": null,
            render: function (data, type, row, meta) {
                return meta.row + meta.settings._iDisplayStart + 1;
            }
        }, {
            "targets": 6, "orderable": false
        }, {
            "targets": 8, "orderable": false
        }, {
            "targets": 7,
            "className": "text-center",
        }];
        sTable = initDataTable("#Trainee", "/Home/AjaxHandlerSchedule", customRow);
        //sys.unblockUI();
        $('#btFilter').click(function (e) {
            sTable.draw();

        });
        $('#fSearchDate_from').datetimepicker({
            format: 'DD/MM/YYYY'
        });
    });
}
////////////////////////////////////////////////////////////// Approve
function Approve_SubjectResult(Course_Detail_Id, int_reject) {
    function Submit() {
        var form = $("#createform").serialize();
        var $forms = $('#createform');
        $.when(sys.CallAjaxPost("/Approve/SubjectResult", data)
        ).done(function (result) {
            $("#messageout").html('');
            $("#messageout").append(data);
            $("#submit").hide();
        });
    }
    $(document).ready(function () {
        //datatable serverside
        var sTable = $("#Trainee").DataTable({
            "responsive:": true,
            "searching": false,
            "pageLength": 9000000,
            "aaSorting": [[5, "desc"]],
            "columnDefs": [{
                "targets": 0,
                "className": "text-center",
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                },
                "sortable":false
            }],
            "bServerSide": true,
            "sAjaxSource": "/Report/AjaxHandlResultHasInsert",
            "bProcessing": true,
            "fnServerParams": function (aoData) {
                aoData.push(
                { "name": "ddl_subject", "value": Course_Detail_Id}
                );
            }
        });
        $('input:radio[name="radioroption"]').change(function () {
            if (this.value === int_reject) {
                $("#idRejectReason").removeClass("hidden");
            } else {
                $("#idRejectReason").addClass("hidden");
            }
        });
        $(".line_350").addClass("active");
    });
}
