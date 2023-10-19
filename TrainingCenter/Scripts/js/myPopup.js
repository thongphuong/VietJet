function setPopUpInform(title, body, button) {
    $('.modal-title').html(title);
    $('.modal-body').html(body);
    $('.modal-footer').html('');
    if (button.indexOf('Cancel') >= 0) {
        $('.modal-footer').append('<button type="button" class="btn btn-default" data-dismiss="modal" id="cancelPop">Cancel</button>');
    }
    if (button.indexOf('Delete') >= 0) {
        $('.modal-footer').append('<button type="button" class="btn btn-danger danger" data-dismiss="modal" id="deletePop">Delete</button>');
    }
    if (button.indexOf('Close') >= 0) {
        $('.modal-footer').append('<button type="button" class="btn btn-default default" data-dismiss="modal" id="closePop">Close</button>');
    }
}

function setInformPopup(title, body, button) {
    $('#modelPopupInform .modal-title').html(title);
    $('#modelPopupInform .modal-body').html(body);
    $('#modelPopupInform .modal-footer').html('');
    if (button.indexOf('Cancel') >= 0) {
        $('#modelPopupInform .modal-footer').append('<button type="button" class="btn btn-default" data-dismiss="modal" id="cancelPop">Cancel</button>');
    }
    if (button.indexOf('Delete') >= 0) {
        $('#modelPopupInform .modal-footer').append('<button type="button" class="btn btn-danger danger" data-dismiss="modal" id="deletePop">Delete</button>');
    }
    if (button.indexOf('Close') >= 0) {
        $('#modelPopupInform .modal-footer').append('<button type="button" class="btn btn-default default" data-dismiss="modal" id="closePop">Close</button>');
    }
}