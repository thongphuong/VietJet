var modifyTemplate =
    '<div">' + 
        '<div class="editor form-group clearfix {menu-action}">' +
        '   <input type="hidden" value="{menu-id}" id="menu-id" data-value="{menu-id}" />' +
        '   <div class="col-md-3">' +
        "       {menu-functions}" +
        "   </div>" +
        '   <div class="col-md-3">' +
        '      <input type="text" class="form-control" placeholder="menu title" id="menu-title" value="{menu-title}" data-value="{menu-title}" />' +
        "   </div>" +
        '   <div class="col-md-3">' +
        '      <div class="input-group">' +
        '          <input type="text" class="form-control input-icon" placeholder="menu icon" id="menu-icon" value="{menu-icon}" data-value="{menu-icon}">' +
        '          <span class="input-group-addon"><i id="icon-preview" class="{menu-icon}"></i></span>' +
        "      </div>" +
        "   </div>" +
        '   <div class="col-md-1">' +
        '      <input type="text" value="{menu-index}" id="menu-index" class="form-control" placeholder="Index" data-value="{menu-index}" />' +
        '   </div>' +
        // end data
        // actions
        '   <div class="col-md-2">' +
        '       <button class="btn btn-primary saveItem" title="Save" data-parentid ="{menu-parentId}"><i class="fa fa-save"></i></button>' +
        '       <button class="btn btn-default cancelModify" title="Cancel"><i class="fa fa-undo"></i></button>' +
        '   </div>' +
        // end actions
        '</div>' +
    '</div>';
var menuTemplate = '<div class="menu-item">' +
    '   <div class="col-md-4"> {menu-title} </div>' +
    '   <div class="col-md-8">' +
    '      <button class="editMenu btn"><i class="fa fa-pencil"></i></button>' +
    '      <button class="addMenu btn" data-parentid ="{menu-id}" ><i class="fa fa-plus" ></i></button>' +
    '      <button class="deleteMenu btn"  data-id="{menu-id}"><i class="fa fa-trash"></i></button>' +
    '   </div>' +
    '</div>' + modifyTemplate;
var menu = {
    $funcs: {},
    loadData: actionLoadData,
    modify: actionModify,
    cancelModify: actionCancelModify,
    saveModify: actionSaveModify,
    addMenu: actionAddMenu,
    iconChange: actionIconChange,
    deleteMenu: actionDeleteMenu,
    configMenu: actionConfigMenu,
    addFunction: actionAddFunction,
    addNode: actionaddNode,
    saveNode: actionSaveNode,

    showModify: menuactionShowModify,
    create: menuactionCreate,
    grant: menuactionGrant,
    confirmActive: menuactionConfirmActive,
    confirmDelete: menuactionConfirmDelete,
    selectChildren: actionSelectChildren
}
var groupuser = {
    newItem: actionNewGroupUser,
    showSubItem: actionShowSubItemGroupUser,
    updateGroupUser: actionUpdateGroupUser,
    grant: actionGrantGoupUsers,
    grantAll: actionGrantGoupUsersAll
}
var role = {
    showSubItem: actionShowSubItem,
    updateRole: actionUpdateRole,
}
var permission = {
    showModify: actionShowModify,
    create: actionCreate,
    grant: actionGrant,
    confirmActive: actionConfirmActive,
    confirmDelete: actionConfirmDelete,
    selectChildren: actionSelectChildren
}
var subject = {
    subjectDetail: [],
    showDetail: actionShowDetail,
    addDetail: actionAddDetail,
    addScore: actionAddScore,
    removeScore: actionRemoveScore,
    loadScores : actionLoadScores
}
var jobTitle = {
    newHeader: actionNewHeader,
    newPosition: actionNewPosition,
    loadPosition: actionLoadPosition,
    loadHeader: actionLoadHeader,
}

function actionModify() {
    var $this = $(this);
    var $menuItem = $this.closest('.menu-item');
    $menuItem.hide();
    var $editor = $menuItem.next();
    var icon = $editor.find('#menu-icon').val();
    $editor.find('#icon-preview').attr('class', icon);
    $editor.addClass('active');
}
function actionCancelModify(e, o) {
    var $this = o ? $(o) : $(this);
    var $editor = $this.closest('.editor');
    $editor.removeClass('active');
    var $menuItem = $editor.prev();
    $editor.find('input').each(function (i, o) {
        var $o = $(o);
        $o.val($o.data('value'));
    });
    $menuItem.show();
}
function actionIconChange() {
    var $this = $(this);
    var $preview = $($this.parent().find('#icon-preview'));
    $preview.attr('class', $(this).val());
}
function actionSaveModify() {
    spinner.open();
    var $this = $(this);
    var $editor = $this.closest('.editor');
    var data = {
        Id: $editor.find('#menu-id').val(),
        MenuIndex: $editor.find('#menu-index').val(),
        MenuTitle: $editor.find('#menu-title').val(),
        Function: $editor.find('#menu-func').val(),
        Icon: $editor.find('#menu-icon').val(),
        ParentId: $this.data('parentid'),
    }
    $.ajax({
        url: '/Menu/Schedule',
        type: 'POST',
        data: data,
        success: function (response) {
            if (response.result) {
                if (data.Id == -1) {
                    processAddMenuSuccess(response, $editor);
                } else {
                    processUpdateMenuSuccess(response, $editor);
                }
            } else {
                notify.alert(response.message, 'SUCCESS', TITLE_STATUS_SUCCESS);
            }
            actionCancelModify(null, $this);
        }, error: function (response) {
            notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            actionCancelModify(null, $this);
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionAddMenu() {
    var $this = $(this);
    var parentId = $this.data('parentid');
    var $container = $this.parent().parent();

    var $content = $container.next().find('.panel-body');

    var newMenu = $(modifyTemplate).clone().html();
    var $item = $('<div></div>');
    var funcs = $('#menu-funcs').html();
    $item.addClass('menu-group');
    newMenu =
        newMenu.replace(/{menu-id}/g, -1)
            .replace(/{menu-icon}/g, 'fa fa-caret-right')
            .replace(/{menu-index}/g, 0)
            .replace(/{menu-title}/g, 'submenu')
            .replace(/{menu-functions}/g, funcs)
            .replace(/{menu-parentId}/g, parentId)
            .replace(/{menu-action}/g, 'active');
    $item.append(newMenu);
    if ($container.hasClass('menu-item')) {
        $content = $container;
        $item.addClass('margin-left-20');
    }
    $content.append($item);
}
function actionDeleteMenu() {
    spinner.open();
    var $this = $(this);
    var id = $this.data('id');
    $.ajax({
        url: '/Menu/DeleteItem',
        type: 'POST',
        data: { id: id },
        success: function (response) {
            if (response.result) {
                $this.parent().parent().remove();
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            }
        }, error: function (response) {
            notify.alert(response, 'ERROR', TITLE_STATUS_DANGER);
        },
        complete: function () {
            spinner.close();
        }
    });
}
function processUpdateMenuSuccess(response, $selector) {
    var data = response.data;
    $selector.find('#menu-id').data('value', data.Id);
    $selector.find('#menu-id').val(data.Id);
    $selector.find('#menu-index').data('value', data.MenuIndex);
    $selector.find('#menu-url').data('value', data.Url);
    $selector.find('#menu-title').data('value', data.MenuTitle);
    $selector.find('#menu-icon').data('value', data.Icon);
}
function processAddMenuSuccess(response, $selector) {
    var data = response.data;
    var $content = $selector.closest('.panel-body');
    var funcs = $('#menu-funcs').html();
    var newMenu = menuTemplate;
    newMenu =
        newMenu.replace(/{menu-id}/g, data.Id)
            .replace(/{menu-icon}/g, data.Icon)
            .replace(/{menu-index}/g, data.MenuIndex)
            .replace(/{menu-title}/g, data.MenuTitle)
            .replace(/{menu-functions}/g, funcs)
            .replace(/{menu-parentId}/g, data.ParentId)
            .replace(/{menu-action}/g, '');
    $selector.parent().remove();
    $content.append(newMenu);
}
function actionConfigMenu() {
    spinner.open();
    var $modal = $($('#menu-config').html());
    var $this = $(this);
    var menuId = $this.data('id');
    $.ajax({
        url: '/Menu/AjaxhandlerListMenuFunctions',
        type: 'POST',
        data: { id: menuId },
        success: function (response) {
            if (response.result) {
                $modal.data('id', menuId);
                var funcs = response.data;
                var modalBody = $modal.find('#modal-body');
                if (funcs.length > 0) {
                    funcs.each(function (i, o) {
                        var func = $('#menu-funcs').clone().html();
                        func = func.replace(/{menu-function-id}/g, o.id).replace(/{menu-function-alias}/g, o.Alias).replace(/{menu-function-index}/g, o.index);
                        var $func = $(func);
                        var $menuFunc = $func.find('#app-functions');
                        menu.$funcs.forEach(function (o, i) {
                            var selected = o.selected != null && o.selected == menuId;
                            if (selected || o.selectedMenu == null)
                                $menuFunc.append('<option value = "' + o.id + '" ' + (selected ? 'selected' : '') + '>' +
                                    o.name +
                                    '</option>');
                        });
                        modalBody.append(func);
                    });
                }
                $modal.modal("show");
            } else {
                $modal.find('.modal-title').text("error");
                $modal.find('#modal-body').html(response.message);
            }
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionLoadData() {
    spinner.open();
    $.ajax({
        url: '/Menu/AjaxhandlerListFunctions',
        type: 'POST',
        success: function (response) {
            if (response.result) {
                menu.$funcs = response.data;
            }
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionAddFunction() {
    var $modal = $('#menu-group-functions');
    var modalBody = $modal.find('#modal-body');
    var ids = $modal.find('.funcs');
    var func = $('#menu-funcs').clone().html();
    func = func.replace(/{menu-function-id}/g, (ids.length + 1)).replace(/{menu-function-alias}/g, '').replace(/{menu-function-index}/g, 99);
    var $func = $(func);
    var $menuFunc = $func.find('#app-functions');
    menu.$funcs.forEach(function (o, i) {
        var selected = o.selected != null && o.selected == menuId;
        if (selected || o.selectedMenu == null)
            $menuFunc.append('<option value = "' + o.id + '" ' + (selected ? 'selected' : '') + '>' +
                o.name +
                '</option>');
    });
    modalBody.append($func);
}
function actionaddNode() {
    $('#container').append($('#menu-node').html());
}
function actionSaveNode(obj) {
    var $parent = $(obj).closest('.form-group');
    var value = $parent.find('#menu-title').val();
    $.ajax({
        url: '/Menu/AddNode',
        data: { title: value },
        type: 'POST',
        success: function (response) {
            if (response.result) {
                $parent.html(value);
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_SUCCESS);
            }
        }, error: function (response) {
            notify.alert(response, 'ERROR', TITLE_STATUS_SUCCESS);
        }
    });
}


function actionShowSubItem(obj) {
    var $this = $(obj);
    var id = $this.data('id');
    var submenu = $('#submenuTemplate').html();
    submenu = submenu.replace(/{parent_menu_id}/g, id);
    var $tr = $this.closest('tr');
    $tr.attr("id", "menu_" + id);
    var $body = $tr.closest('tbody');
    var $submenus = $body.find("#submenu_" + id);
    if ($submenus.length > 0) {
        $body.find("#submenu_" + id).remove();
    } else {
        var $submenu = $(submenu);
        renderSubmenuDataTable($submenu, id, $('#roleId').val(), $tr);
    }
}
function renderSubmenuDataTable(selector, id, roleId, $afterRow) {
    var dtb = $(selector).dataTable({
        "responsive:": true,
        "searching": false,
        "pageLength": 9000000,
        "columnDefs": [{
            "targets": 0,
            "className": "text-center",
            "data": null,
            render: function (data, type, row, meta) {
                return meta.row + meta.settings._iDisplayStart + 1;
            }
        }],
        "aaSorting": [],
        "bServerSide": true,
        "sAjaxSource": "/Role/AjaxHandlerSubmenu/",
        "bProcessing": true,
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "id", "value": roleId });
            aoData.push({ "name": "menuId", "value": id });
        }
    });
    var $row = $('<tr></tr>');
    $row.attr('id', 'submenu_' + id);
    $row.append($('<td colspan="7"></td').append(dtb));
    $afterRow.after($row);
}

function actionNewGroupUser($modal, data) {
    $.ajax({
        url: "/GroupUser/CreateGroupUser",
        type: "POST",
        data: data,
        success: function (res) {
            if (res.result) {
                $modal.modal('hide');
                window.location.href = "/GroupUser/Modify/" + res.data;
            } else {
                notify.alert(res.message, "ERROR", TITLE_STATUS_DANGER);
            }
        },
        error: function (res) {
            notify.alert(res.message, "ERROR", TITLE_STATUS_DANGER);
        }
    });
}
function actionShowSubItemGroupUser(obj) {
    var $this = $(obj);
    var id = $this.data('id');
    var submenu = $('#submenuTemplate').html();
    submenu = submenu.replace(/{parent_menu_id}/g, id);
    var $tr = $this.closest('tr');
    $tr.attr("id", "menu_" + id);
    var $body = $tr.closest('tbody');
    var $submenus = $body.find("#submenu_" + id);
    if ($submenus.length > 0) {
        $body.find("#submenu_" + id).remove();
    } else {
        var $submenu = $(submenu);
        renderSubmenuDataTableGroupUser($submenu, id, $('#Id').val(), $tr);
    }
}
function renderSubmenuDataTableGroupUser(selector, id, roleId, $afterRow) {
    var dtb = $(selector).dataTable({
        "responsive:": true,
        "searching": false,
        "pageLength": 9000000,
        "columnDefs": [{
            "targets": 0,
            "className": "text-center",
            "data": null,
            render: function (data, type, row, meta) {
                return meta.row + meta.settings._iDisplayStart + 1;
            }
        }],
        "aaSorting": [],
        "bServerSide": true,
        "sAjaxSource": "/GroupUser/AjaxHandlerSubMenu/",
        "bProcessing": true,
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "id", "value": roleId });
            aoData.push({ "name": "menuId", "value": id });
        }
    });
    var $row = $('<tr></tr>');
    $row.attr('id', 'submenu_' + id);
    $row.append($('<td colspan="7"></td').append(dtb));
    $afterRow.after($row);
}


function actionUpdateRole(idrole, idpage, optiontype, isCheck, obj) {
    spinner.open();
    $.ajax({
        type: 'POST',
        url: "/Role/click_option/",
        datatype: 'json',
        data: { idrole: idrole, idpage: idpage, optiontype: optiontype, onoff: isCheck },
        success: function (data) {
            if (data.result) {
                
              //  notify.alert(data.message, "SUCCESS", TITLE_STATUS_SUCCESS);
            } else {
                notify.alert(data.message, "ERROR", TITLE_STATUS_DANGER);
            }
        }, complete: function () {
            loadRoleData(idrole);
            spinner.close();
        }
       
    });
}

function loadRoleData(id) {
    spinner.open();
    $.ajax({
        url: '/ROLE/MenuListView',
        data: { id: id },
        success: function (response) {
            $('#tree').empty();
            $('#tree').html(response);
        }, complete: function () {
            spinner.close();
        }
    });
}

function actionUpdateGroupUser(idGroupUser, idMenu, type, isCheck, obj) {
    spinner.open();
    var $tbl = $(obj).closest('table');
    $.ajax({
        type: 'POST',
        url: "/GroupUser/ClickOption/",
        datatype: 'json',
        data: { idGroupUser: idGroupUser, idMenu: idMenu, type: type, onoff: isCheck },
        success: function (data) {
            //$("#DepTable").DataTable().draw();
            $tbl.DataTable().draw();
            spinner.close();
        }
    });
}

function actionShowModify(id) {
    spinner.open();
    $.ajax({
        url: '/Department/Modify/' + id,
        success: function (response) {
            var $modal = $(response).modal('show');
            $modal.on('submit', 'form', function (e) {
                updateDepartment($modal);
                //e.preventDefault();
                return false;
            });
        },
        complete: function () {
            spinner.close();
        }
    });
}
function menuactionShowModify(id) {
    spinner.open();
    $.ajax({
        url: '/Menu/ModifySchedule/' + id,
        success: function (response) {
            var $modal = $(response).modal('show');
            $modal.on('submit', 'form', function (e) {
                updateMenu($modal);
                //e.preventDefault();
                return false;
            });
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionCreate(id) {
    spinner.open();
    $.ajax({
        url: '/Department/Create/' + id,
        success: function (response) {
            var $modal = $(response).modal('show');
            $modal.on('submit', 'form', function (e) {
                updateDepartment($modal);
                e.preventDefault();
            });
        },
        complete: function () {
            spinner.close();
        }
    });
}
function menuactionCreate(id) {
    spinner.open();
    $.ajax({
        url: '/Menu/CreateSchedule/' + id,
        success: function (response) {
            var $modal = $(response).modal('show');
            $modal.on('submit', 'form', function (e) {
                updateMenu($modal);
                e.preventDefault();
            });
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionConfirmActive() {
    var $this = $(this);
    var id = $this.data('id');
    var data = { id: id }
    notify.confirm("Are your sure to active/inactive this item?", "Warning", 2, activeItem, data);
}
function menuactionConfirmActive() {
    var $this = $(this);
    var id = $this.data('id');
    var data = { id: id }
    notify.confirm("Are your sure to active/inactive this item?", "Warning", 2, activeItem, data);
}
function actionConfirmDelete() {
    var $this = $(this);
    var id = $this.data('id');
    var data = { id: id }
    notify.confirm("Are your sure to delete this item?", "Warning", 2, deleteItem, data);
}
function menuactionConfirmDelete() {
    var $this = $(this);
    var id = $this.data('id');
    var data = { id: id }
    notify.confirm("Are your sure to delete this item?", "Warning", 2, deleteItemMenu, data);
}
function activeItem(data) {
    spinner.open();
    $.ajax({
        url: '/Department/UpdateStatus',
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, "SUCCESS", TITLE_STATUS_SUCCESS);
                loadPermissionData();
            } else {
                notify.alert(response.message, "ERROR", TITLE_STATUS_DANGER);
            }
        }, complete: function () {
            spinner.close();
        }
    });
}
function deleteItem(data) {
    spinner.open();
    $.ajax({
        url: '/Department/Delete',
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, "SUCCESS", TITLE_STATUS_SUCCESS);
                loadPermissionData();
            } else {
                notify.alert(response.message, "ERROR", TITLE_STATUS_DANGER);
            }
        }, complete: function () {
            spinner.close();
        }
    });
}
function deleteItemMenu(data) {
    spinner.open();
    $.ajax({
        url: '/Menu/DeleteItem',
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, "SUCCESS", TITLE_STATUS_SUCCESS);
                loadMenuData();
            } else {
                notify.alert(response.message, "ERROR", TITLE_STATUS_DANGER);
            }
        }, complete: function () {
            spinner.close();
        }
    });
}
function loadPermissionData() {
    spinner.open();
    $.ajax({
        url: '/Department/DepartmentListView',
        success: function (response) {
            $('#tree').html(response);
        }, complete: function () {
            spinner.close();
        }
    });
}
function loadMenuData() {
    spinner.open();
    $.ajax({
        url: '/Menu/MenuListView',
        success: function (response) {
            $('#tree').html(response);
        }, complete: function () {
            spinner.close();
        }
    });
}
function loadUserPermissionData() {
    spinner.open();
    var userId = $('#Id').val();
    var groupUserId = $('#GroupUser').val();
    $.ajax({
        url: '/User/DepartmentListView',
        type: 'POST',
        data: { id: userId, groupUsers: groupUserId },
        success: function (response) {
            $('#tree').html(response);
        }, complete: function () {
            spinner.close();
        }
    });
}
function actionGrant(permissionId) {
    spinner.open();
    var userId = $('#Id').val();
    var data = { id: permissionId, userId: userId };
    $.ajax({
        url: '/User/GrantUserPermission',
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, 'SUCCESS', TITLE_STATUS_SUCCESS);
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            }
            loadUserPermissionData();
        }, complete: function () {
            spinner.close();
        }
    });
}
function menuactionGrant(permissionId) {
    spinner.open();
    var userId = $('#Id').val();
    var data = { id: permissionId, userId: userId };
    $.ajax({
        url: '/User/GrantUserPermission',
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, 'SUCCESS', TITLE_STATUS_SUCCESS);
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            }
            loadUserPermissionData();
        }, complete: function () {
            spinner.close();
        }
    });
}
function loadGroupUserPermissionData() {
    spinner.open();
    var userId = $('#Id').val();
    $.ajax({
        url: '/GroupUser/UserModifyPermission/' + userId,
        type: 'POST',
        success: function (response) {
            $('#tree').html(response);
        }, complete: function () {
            spinner.close();
        }
    });
}
function actionGrantGoupUsers(permissionId) {
    spinner.open();
    var id = $('#Id').val();
    var data = { id: permissionId, groupUserId: id };
    $.ajax({
        url: "/GroupUser/GrantGroupUserPermission",
        data: data,
        type: "POST",
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, 'SUCCESS', TITLE_STATUS_SUCCESS);
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            }
            loadGroupUserPermissionData();
        }, complete: function () {
            spinner.close();
        }
    });
}
function actionGrantGoupUsersAll(type) {
    spinner.open();
    var id = $('#Id').val();
    var data = { groupUserId: id, typecheck: type };
    $.ajax({
        type: "POST",
        url: "/GroupUser/GrantGroupUserPermissionAll",
        datatype: 'json',
        data: data,
        success: function (response) {
            loadGroupUserPermissionData();
            if (response.result) {
                notify.alert(response.message, 'SUCCESS', TITLE_STATUS_SUCCESS);
            } else {
                notify.alert(response.message, 'ERROR', TITLE_STATUS_DANGER);
            }
           
        }, complete: function () {
            spinner.close();
        }
    });
}
function updateDepartment($modal) {
    var $this = $modal.find('form');
    var formData = $this.serializeArray();
    var data = mergArrayObject(formData);
    $.ajax({
        url: $this.attr('action'),
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, 'Notification', TITLE_STATUS_SUCCESS);
                loadPermissionData();
                $modal.on('hidden.bs.modal', function () { $(this).remove(); });
                $modal.modal('hide');
            } else {
                notify.alert(response.message, 'Warning', TITLE_STATUS_DANGER);
            }
        },
        complete: function () {
            spinner.close();
        }
    });
}
function updateMenu($modal) {
    var $this = $modal.find('form');
    var formData = $this.serializeArray();
    var data = mergArrayObject(formData);
    $.ajax({
        url: $this.attr('action'),
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                notify.alert(response.message, 'Notification', TITLE_STATUS_SUCCESS);
                loadMenuData();
                $modal.on('hidden.bs.modal', function () { $(this).remove(); });
                $modal.modal('hide');
            } else {
                notify.alert(response.message, 'Warning', TITLE_STATUS_DANGER);
            }
        },
        complete: function () {
            spinner.close();
        }
    });
}
function mergArrayObject(arrObj) {
    var obj = {};
    arrObj.reduce(function (acc, cur, i) {
        obj[cur.name] = cur.value;
        return obj;
    }, {});
    return obj;
}
function actionSelectChildren(selector) {
    var $this = $(selector);
    var status = $this.prop('checked');
    var parent = $this.closest('li');
    parent.find('input[type="checkbox"]:not(:disabled)').prop('checked', status);
}
function actionAddDetail() {
  
    var $frmDetail = $('#editor-subject-detail');
    var $score = $frmDetail.find('.score');
    var $instructor = $frmDetail.find('#editor-Instructor');
    var text = [];
    $.each($instructor.find(':selected'), function (i, o) {
        text.push($(o).text());
    });
    var id = $frmDetail.find('#editor-Id').val();
    var $objData = {
        Id: id.length > 0 ? id : null,
        Name: $frmDetail.find('#editor-Name').val(),
        Code: $frmDetail.find('#editor-Code').val(),
        instructor: {
            key: $instructor.val(),
            value: text
        },
        Duration: $frmDetail.find('#editor-Duration').val(),
        Recurrent: $frmDetail.find('#editor-Recurrent').val(),
        IsAverageCaculate: $frmDetail.find('#editor-IsAverageCaculate').val(),
        SubjectScoreModels: []
    };
    $.each($score, function (i, o) {
        $objData.SubjectScoreModels.push({
            Grade: $(o).find("#editor-score-grade").val(),
            PointFrom: $(o).find("#editor-score-from").val(),
            PointTo: $(o).find("#editor-score-to").val(),
        });
    });
    var index = $('#editor-index').val();
    if (index == '' || index == -1) {
        $subjectdetail.row.add($objData).draw();
    } else {
        $subjectdetail.row(index).data($objData).draw();
    }
}
function modifyNewSubjectDetail(index, $frmSelector) {
    if (index != null) {
        var objDetail = $subjectdetail.row(index).data();
        var $frmDetail = $frmSelector;
        $frmDetail.find('#editor-index').val(index);
        $frmDetail.find('#editor-Instructor').val(objDetail.instructor.key);
        $frmDetail.find('#editor-Name').val(objDetail.Name);
        $frmDetail.find('#editor-Id').val(objDetail.Id);
        $frmDetail.find('#editor-Duration').val(objDetail.Duration);
        $frmDetail.find('#editor-Code').val(objDetail.Code);
        $frmDetail.find('#editor-Recurrent').val(objDetail.Recurrent);
        $frmDetail.find('#editor-IsAverageCaculate').val(objDetail.IsAverageCaculate);
        var $scores = $frmDetail.find('.score');

        $.each($scores, function (i, o) {
            var $score = $(o);
            var $scoreData = objDetail.SubjectScoreModels[i];
            $score.find('#editor-score-grade').val($scoreData.Grade);
            $score.find('#editor-score-from').val($scoreData.PointFrom);
            $score.find('#editor-score-to').val($scoreData.PointTo);
        });
    }
}
function actionShowDetail(id, index) {
    spinner.open();
    $.ajax({
        url: '/Subject/SubjectDetail/' + id,
        type: 'POST',
        success: function (response) {
            var $modal = $(response).modal('show');
            if (id == null) {
                modifyNewSubjectDetail(index, $modal);
            } else {
                $modal.find('#editor-index').val(index);
            }
            $modal.on('submit', 'form', function (e) {
                actionAddDetail();
                $modal.modal('hide');
                e.preventDefault();
            });
            $modal.on('hidden.bs.modal', function (e) {
                $(this).remove();
            });
            $modal.find('select').select2({ allowClear: true });
        }, error: function (response) {
            notify.alert(response, 'Error', TITLE_STATUS_DANGER);
        }, complete: function () {
            spinner.close();
        }
    });
}

function actionNewHeader() {
    spinner.open();
    $.ajax({
        url: '/Jobtitle/CreateHeader',
        success: function (response) {
            var $modal = $(response).modal({ backdrop: 'static', show: true });
            $modal.on('submit', 'form', function (e) {
                proccessFormInModal($modal, jobTitle.loadHeader);
                e.preventDefault();
            });
            $modal.on('hidden.bs.modal', function () { $(this).remove(); });
        },
        error: function (response) {
            response.alert(response);
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionNewPosition() {
    spinner.open();
    $.ajax({
        url: '/Jobtitle/CreatePosition',
        success: function (response) {
            var $modal = $(response).modal({ backdrop: 'static', show: true });
            $modal.on('submit', 'form', function (e) {
                proccessFormInModal($modal, jobTitle.loadPosition);
                e.preventDefault();
            });
            $modal.on('hidden.bs.modal', function () { $(this).remove(); });
        },
        error: function (response) {
            response.alert(response);
        },
        complete: function () {
            spinner.close();
        }
    });
}


function actionAddScore(table,selector) {
    spinner.open();
    var $table = table;
    var $row = selector == null ? null : $(selector).parents('tr');
    var data = {};
    if ($row != null) {
        data = $table.row($row).data();
    }
    $.ajax({
        url: '/Subjects/AddScore/' + data.Id,
        success: function (response) {
            response = response.replace(/undefined/g, '');
            response = response.replace(/null/g, '');
            var $modal = $(response).modal({ backdrop: 'static', show: true });
            if ($row != null) { // mapping data
                data = $table.row($row).data();
                $modal.find('#Grade').val(data.Grade);
                $modal.find('#Point').val(data.Point);
            }
            $modal.on('submit', 'form', function (e) {
                proccessFormInModal($modal, proccessAddNewScore, { dataTable:$table ,dataRow : $row});
                e.preventDefault();
            });
            $modal.on('hidden.bs.modal', function () { $(this).remove(); });
        },
        error: function (response) {
            response.alert(response);
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionRemoveScore(table,selector) {
    var $row = $(selector).parents('tr');
    var tblRow = table.row($row);
    notify.confirm("Are you sure?", "Warning", TITLE_STATUS_WARNING, proccessRemoveScore, { dataTable: table , tableRow : tblRow});
}
function proccessRemoveScore(o) {
    o.tableRow.remove();
    o.dataTable.cells().invalidate('data').draw();
}
function proccessAddNewScore(data, table) {
    if (table.dataRow == null) {
        table.dataTable.row.add(data);
    } else {
        table.dataTable.row(table.dataRow).data(data);
    }
    table.dataTable.cells().invalidate('data').draw();
}
function actionLoadScores(id,dataTable) {
    spinner.open();
    $.ajax({
        url: '/Subjects/GetSubjectScores/' + id,
        type:'POST',
        success: function (response) {
            if (response.result) {
                dataTable.rows.add(response.data).draw();
            } else {
                notify.alert(response.message, 'Warning', TITLE_STATUS_DANGER);
            }
        },
        error: function (response) {
            notify.alert(response.message, 'Warning', TITLE_STATUS_DANGER);
        },
        complete: function () {
            spinner.close();
        }
    });
}
function actionLoadHeader(selectedVal) {
    var headerId = $('#headerId').val();
    $.ajax({
        url: '/Jobtitle/ListHeader',
        type: 'POST',
        success: function (response) {
            var $selectList = $('#jobheaders');
            if (response.result) {
                var selected = selectedVal == undefined ? headerId : selectedVal;
                var options = '';
                $.each(response.data, function (i, o) {
                    options += '<option ' + (o.Key == selected ? 'selected = "selected"' : '') + ' value="' + o.Key + '" title="' + (o.Title == null ? '' : o.Title) + '"> ' + o.Value + '</option>';
                });

                $selectList.select2('destroy');
                $selectList.html(options);
                initSelectTags('#jobheaders');
               
                $selectList.change(function () {
                    proccessJobtitleSelectOptions(this, '#JobPositions');
                });
            } else {
                notify.alert(response.message);
            }
        },
        complete: function () {
            proccessJobtitleSelectOptionsStart();
        }
    });
}
function actionLoadPosition(selectedVal) {
    var positionId = $('#positionId').val();
    $.ajax({
        url: '/Jobtitle/ListPosition',
        type: 'POST',
        success: function (response) {
            var $selectList = $('#JobPositions');
            if (response.result) {
                var selected = selectedVal == undefined ? positionId : selectedVal;
                var options = '';
                $.each(response.data, function (i, o) {
                    options += '<option ' + (o.Key == selected ? 'selected = "selected"' : '') + ' value="' + o.Key + '" title="' + (o.Title == null ? '' : o.Title) + '"> ' + o.Value + '</option>';
                });
                $selectList.select2('destroy');
                $selectList.html(options);
                initSelectTags('#JobPositions');
                $selectList.change(function () {
                    proccessJobtitleSelectOptions('#jobheaders', this);
                });
            } else {
                notify.alert(response.message);
            }
        },
        complete: function () {
            proccessJobtitleSelectOptionsStart();
        }
    });
}
function proccessFormInModal($modal,submitSuccess,params) {
    var $this = $modal.find('form');
    var formData = $this.serializeArray();
    var data = mergArrayObject(formData);
    $.ajax({
        url: $this.attr('action'),
        data: data,
        type: 'POST',
        success: function (response) {
            if (response.result) {
                if (response.message != null)
                    notify.alert(response.message, 'Notification', TITLE_STATUS_SUCCESS);
                if (submitSuccess != undefined) {
                    submitSuccess(response.data,params);
                }
                $modal.on('hidden.bs.modal', function () { $(this).remove(); });
                $modal.modal('hide');
            } else {
                notify.alert(response.message, 'Warning', TITLE_STATUS_DANGER);
            }
        },
        complete: function () {
            spinner.close();
        }
    });
}
function proccessJobtitleSelectOptions(jobHeader, jobPosition) {
    var $jobHeader = $(jobHeader).find(':selected').text();
    var $jobPosition = $(jobPosition).find(':selected').text();
    $('#Name').val($jobHeader + ' ' + $jobPosition);
}
function proccessJobtitleSelectOptionsStart() {
    var $jobHeader = $("#jobheaders").find(':selected').text();
    var $jobPosition = $("#JobPositions").find(':selected').text();
    $('#Name').val($jobHeader + ' ' + $jobPosition);
}