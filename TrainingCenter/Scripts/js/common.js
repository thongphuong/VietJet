var TITLE_STATUS_SUCCESS = 1;
var TITLE_STATUS_WARNING = 2;
var TITLE_STATUS_DANGER = 3;
var TITLE_STATUS_INFO = 4;

var DEFAULT_LOCALE = 'vi';
var spinner = {
    id: $($('#modal-spinner').html()),
    open: actionOpen,
    close: actionClose
}
var notify = {
    alert: actionAlert,
    confirm: actionConfirm
}
function formatDatetimepicker(selector, xLocale) {
    //sontt dòng bên dưới gán cứng định dạng tiếng việt.
    xLocale = "vi";
    var $selector;
    var locale = xLocale == null || xLocale.length == 0 ? DEFAULT_LOCALE : xLocale;
    if (selector instanceof jQuery) {
        $selector = selector;
    } else {
        $selector = $(selector);
    }

    $selector.datetimepicker({
        format: 'DD/MM/YYYY',
        //locale: locale
    });
}
function actionOpen() {
    spinner.id.modal({ backdrop: 'static', keyboard: false, show: true });
}
function actionClose() {
    spinner.id.modal('hide');
}

function actionAlert(msg, title, status) {
    var $modal = $($('#modal-notify').html());
    $modal.find('#notify-body').html(msg);
    status = "panel-" + getStatusText(status);
    if (title == undefined || title.length > 0) {
        $modal.find('#notify-title').html(title);
        $modal.find('#notify-title').closest('.panel-heading').addClass(status);
    } else {
        $modal.find('#notify-title').closest('.panel-heading').remove();
    }
    $modal.modal({ keyboard: false, show: true });
    $modal.on('hidden.bs.modal', function (e) {
        $modal.remove();
    });
}
function actionConfirm(msg, title, status, fn, data) {
    var $modal = $($('#modal-confirm').html());
    $modal.find('#notify-body').html(msg);
    status = "panel panel-" + getStatusText(status);
    $modal.find('#notify-title').html(title);
    $modal.find('#notify-title').closest('.panel-heading').addClass(status);
    //$modal.find('#notify-title').closest('.panel-heading').attr(status);
    $modal.modal({ keyboard: false, show: true });
    $modal.on('hidden.bs.modal', function (e) {
        $modal.remove();
    });
    $modal.on('click', 'button#confirm', function () {
        fn(data);
        $modal.modal('hide');
    });
}
function getStatusText(status) {
    switch (status) {
        case TITLE_STATUS_SUCCESS:
            return 'success';
        case TITLE_STATUS_WARNING:
            return 'warning';
        case TITLE_STATUS_DANGER:
            return 'danger';
        case TITLE_STATUS_INFO:
            return 'info';
        default:
            return 'default';
    }
}

function initDataTable(selector, url, customRows) {
    if (customRows == null) customRows = [];
    
    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        //"scrollY": 300,
        //"scrollX": true,
        "responsive:": true,
        "searching": false,
        "aaSorting": [],
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
        "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });
    
}
function initDataTableNotLengthChange(selector, url, customRows) {
    if (customRows == null) customRows = [];

    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        //"scrollY": 300,
        //"scrollX": true,
        "bLengthChange": false,
        "pageLength": 5,
        "responsive:": true,
        "searching": false,
        "aaSorting": [],
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true
    });

}
function initDataTableMaxPageLength(selectorMax, url, customRows) {
    if (customRows == null) customRows = [];

    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }

    });
    return $(selectorMax).DataTable({
        "responsive:": true,
        "bLengthChange": false,
        "searching": false,
        "pageLength": 9000000,
        "aaSorting": [],
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
       "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });

}

function initDataTablechil(selector, url, customRows) {
    if (customRows == null) customRows = [];
    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        "responsive:": true,
        "searching": false,
        "columnDefs": customRows,
        "aaSorting": [],
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
        "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });
}


function getTblFilter(oData) {
    var $filter = $('#frmFilter').find('.frmFilter');
    $.each($filter, function (i, o) {
        oData.push({ "name": $(o).attr('name'), "value": $(o).val() });
    });
    //console.log(oData);
}
function initSelectTags(selector) {
    $(selector).select2({
        allowClear: true, tags: true
    });
}
function openModalEditor(selector, action) {
    var $this = $(selector);
    var url = $this.data('url');
    $.ajax({
        url: url,
        success: function (res) {
            var $modal = $(res).modal({ backdrop: 'static', show: true });
            $modal.on('hidden.bs.modal', function (e) {
                $modal.remove();
            });
            $modal.on('submit', 'form', function (e) {
                e.preventDefault();
                var data = $(this).serialize();
                action($modal, data);
            });

        },
        error: processAjaxError
    });
}
function processAjaxError(response) {
    notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
}

// Chinh Sua tu Ham initDataTable
function initDataTable2(selector, url, customRows) {
    if (customRows == null) customRows = [];

    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        //"scrollY": 300,
        //"scrollX": true,
        "responsive:": true,
        "pageLength": 100,
        "searching": false,
        "aaSorting": [],
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
        "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });

}
function initDataTable_huy(selector, url, customRows) {
    if (customRows == null) customRows = [];

    customRows.unshift({
        "targets": 1,
        "className": "text-center",
        "sortable": false,
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        //"scrollY": 300,
        "responsive:": true,
        "searching": false,
        "aaSorting": [],
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
        "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });

}
function initDataTable_huy2(selector, url, customRows) {
    if (customRows == null) customRows = [];

    customRows.unshift({
        "targets": 0,
        "className": "text-center",
        "data": null,
        render: function (data, type, row, meta) {
            return meta.row + meta.settings._iDisplayStart + 1;
        }
    });
    return $(selector).DataTable({
        //"scrollY": 300,
        //"scrollX": true,
        "responsive:": true,
        "searching": false,
        "columnDefs": customRows,
        "fnServerParams": getTblFilter,
        "bServerSide": true,
        "sAjaxSource": url,
        "bProcessing": true,
        "drawCallback": function (settings) {

            $('[data-toggle="tooltip"]').tooltip();

        },
    });

}