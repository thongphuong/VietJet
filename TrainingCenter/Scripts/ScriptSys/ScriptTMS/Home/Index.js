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

    //datatable serverside
    var sTable = $("#Trainee").dataTable({
        "responsive:": true,
        "searching": false,
        "columnDefs": [{
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
        }],
        "aaSorting": [],
        "bServerSide": true,
        "sAjaxSource": "/Home/AjaxHandlerSchedule",
        "bProcessing": true,
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "CourseList", "value": $('#CourseList').val() },
                        { "name": "RoomList", "value": $('#RoomList').val() },
                        { "name": "str_code", "value": $('#str_code').val() },
                        { "name": "str_Type", "value": $('#str_Type').val() },
                        { "name": "str_Status", "value": $('#str_Status').val() },
                        { "name": "InstructorList", "value": $('#InstructorList').val() },
                        { "name": "fSearchDate_from", "value": $('#fSearchDate_from').val() },
                        { "name": "ddl_subject", "value": $('#ddl_subject').val() });
        }
    });
   
    //sys.unblockUI();
    $('#btFilter').click(function (e) {
        sTable.fnDraw();

    });
    $('#fSearchDate_from').datetimepicker({
        format: 'L',locale:'vi'
    });
});


