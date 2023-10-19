

var sys = (function () {
    var CallAjax, CallAjaxReturnHtml,
        CallAjaxasync, CallAjaxasyncReturnHtml,
        CallAjaxPost, CallAjaxPostReturnHtml,
        CallAjaxExcel, CallAjaxExcelReturnHtml,
        CallAjaxPostasync, CallAjaxPostasyncReturnHtml,
        CallAjaxPostFormasync, CallAjaxPostFormasyncReturnHtml,

        disButon, enButon, OnTop, blockUI, unblockUI,

        Success, Notsuccessfully, Alert, Warning, Error, ConfirmDialog,
            
        EncryptS, DecryptS,

        Loading, HideLoading;
  ///////////////////////////////////////////////////////////////////////// call ajax  
    //call ajax to get data
    CallAjax = function (url, params) {
        return $.ajax({
            Type: "GET",
            url: url,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        });
    };
    //call ajax to get data return HTML
    CallAjaxReturnHtml = function (url, params) {
        return $.ajax({
            Type: "GET",
            url: url,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "html"
        });
    };
    //call ajax to get data
    CallAjaxPost = function (url, params) {
        return $.ajax({
            url: url,
            type: "POST",
            data: JSON.stringify(params),
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        });
    };
    //call ajax to get data return HTML
    CallAjaxPostReturnHtml = function (url, params) {
        return $.ajax({
            url: url,
            type: "POST",
            data: JSON.stringify(params),
            contentType: "application/json; charset=utf-8",
            dataType: "html"
        });
    };
    //call ajax to get data
    CallAjaxPostFormasync = function (url, params) {
        return $.ajax({
            url: url,
            type: "POST",
            data: JSON.stringify(params),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false
        });
    };
    //call ajax to get data return HTML
    CallAjaxPostFormasyncReturnHtml = function (url, params) {
        return $.ajax({
            url: url,
            type: "POST",
            data: JSON.stringify(params),
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            async: false
        });
    };
    //call ajax to get data
    CallAjaxPostasync = function (url, params) {
        return $.ajax({
            type: "POST",
            url: url,
            data: params,
            dataType: 'json',
            contentType: false,
            processData: false,
            async: false
        });
    };
    //call ajax to get data return HTML
    CallAjaxPostasyncReturnHtml = function (url, params) {
        return $.ajax({
            type: "POST",
            url: url,
            data: params,
            dataType: 'html',
            contentType: false,
            processData: false,
            async: false
        });
    };
    //call ajax async
    CallAjaxasync = function (url, params) {
        return $.ajax({
            Type: "GET",
            url: url,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: false
        });
    };
    //call ajax async return HTML
    CallAjaxasyncReturnHtml = function (url, params) {
        return $.ajax({
            Type: "GET",
            url: url,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            async: false
        });
    };
    //Call ajax upload excel file
    CallAjaxExcel = function (url, params) {
        return $.ajax({
            type: "POST",
            url: url,
            data: params,
            dataType: 'json',
            contentType: false,
            processData: false
        });
    };
    //Call ajax upload excel file return HTML
    CallAjaxExcelReturnHtml = function (url, params) {
        return $.ajax({
            type: "POST",
            url: url,
            data: params,
            dataType: 'html',
            contentType: false,
            processData: false
        });
    };
    ///////////////////////////////////////////////////////////////////////// blockUI 
    //vo hieu hoa button
    disButon = function (object) {
        $(object).attr('disabled', 'disabled');
    };
    //tat vo hieu hoa
    enButon = function (object) {
        $(object).removeAttr('disabled');
    };
    //len top
    OnTop = function () {
        $("html, body").animate({ scrollTop: 0 }, 'slow');
    };
    //blockUI
    blockUI = function () {
        $.blockUI(
            {
                message: ' <div class="uil-ripple-css" style="transform:scale(0.6);"><div></div><div></div></div>',
                css: { border:'none' ,    background: 'none'},
                overlayCSS: { backgroundColor: '#011a21', cursor: 'default' }
            }
            );
    };
    //unblockUI
    unblockUI = function () {
        $.unblockUI();
    };
    ///////////////////////////////////////////////////////////////////////// alert
    //successfully
    Success = function (content) {
        //var objectinsert = $("#dialogSuccess");
        //processAll(objectinsert);

        ////set content thong bao
        //$("#data-content-Success").text(Content);

        ////định dạng cho text xuống dòng
        //if (Content.length > 34) {
        //    $("#data-content-Success").css("line-height", "35px");
        //    $("#data-content-Success").css("height", 35 * (Content.length / 34));
        //    $("#dialogSuccess").css("min-height", "168px");
        //    $("#dialogSuccess").css("height", $("#dialogSuccess").height() + $("#data-content-Success").height() - 60);
        //}
        swal("Complete", content, "success");
    };
    //Notsuccessfully
    Notsuccessfully = function (content) {
        var objectinsert = $("#dialogNotSuccess");
        processAll(objectinsert);
        //set content thong bao
        $("#data-content-NotSuccess").text(content);

        //định dạng cho text xuống dòng
        if (content.length > 34) {
            $("#data-content-NotSuccess").css("line-height", "35px");
            $("#data-content-NotSuccess").css("height", 35 * (content.length / 34));
            $("#dialogNotSuccess").css("min-height", "168px");
            $("#dialogNotSuccess").css("height", $("#dialogNotSuccess").height() + $("#data-content-NotSuccess").height() - 60);
        }

        //bin nut ok
        $("#data-NotSuccess").click(function () {
            //tra popup ve vi tri ban dau
            var heightscreem = $(window).height();
            var top = $(window).scrollTop();
            var truheigt = -((heightscreem / 3) + parseInt(top) + 150);

            objectinsert.animate({
                top: truheigt
            }, 500, function () { });

            //an di
            objectinsert.fadeOut();
        });
    };
    //ham xu ly insert,edit,delete
    function processAll(object) {
        //goiinsert
        object.fadeIn("slow");
        var heightscreem = $(window).height();
        var top = $(window).scrollTop();
        var iheight = heightscreem / 3 + parseInt(top);
        var truheigt = -((heightscreem / 3) + parseInt(top) + (-60));
        object.animate({
            top: iheight
        }, 500, function () {

            //delay 
            $(this).delay(1000);
            object.fadeOut(); // an truoc khi load.

            //tra popup ve vi tri ban dau
            $(this).animate({
                top: truheigt
            }, 500, function () { });
        });
    };
    //alert dialog
    Alert = function (title, content, button) {
        swal(title, content, "success");
    };
    Warning = function (title, content, button) {
        swal(title, content, "warning");
    };
    Error = function (title, content, button) {
        swal(title, content, "error");
    };
    //confirm dialog
    ConfirmDialog = function (title, content, callback) {
        swal({
            title: title,
            text: content,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Thực hiện!",
            cancelButtonText: "Hủy bỏ!",
            closeOnConfirm: true
        },
        function () {
            if (callback && typeof callback === 'function') {
                callback();
            }
        });
    };
    ///////////////////////////////////////////////////////////////////////// ma hoa javascript
    //ma hoa javascript
    EncryptS = function (text) {
        var key = CryptoJS.enc.Utf8.parse('8080808080808080');
        var iv = CryptoJS.enc.Utf8.parse('8080808080808080');

        var x = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(text), key,
        {
            keySize: 128 / 8,
            iv: iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });

        $(".CryptoJS").val(x); // class .CryptoJS duoc khai bao o trang layout
        var Contents = $(".CryptoJS").val();

        //clear gia tri khi get xong
        $(".CryptoJS").val("");
        return Contents; //tra ve du lieu da ma hoa.
    };
    // giai ma javascipt
    DecryptS = function (text) {
        //return CryptoJS.AES.decrypt(text, "My Secret Passphrase");
        var key = CryptoJS.enc.Utf8.parse('8080808080808080');
        return CryptoJS.AES.decrypt(text, key).toString(iv);
    };
    ///////////////////////////////////////////////////////////////////////// loading
    //loading trang
    Loading = function () {
        var height = $(window).height();
        var scrool = $(document).scrollTop();
        $("#loading").css("top", height / 3 + scrool);
        $("#loading").css("display", "block");
    };
    //hide load trang
    HideLoading = function () {
        $("#loading").fadeOut('slow');
        $("#loading").css("display", "none");
    };



    //return ket qua
    return {
        CallAjax: CallAjax, CallAjaxReturnHtml: CallAjaxReturnHtml, CallAjaxPost: CallAjaxPost, CallAjaxPostReturnHtml: CallAjaxPostReturnHtml, CallAjaxPostasync: CallAjaxPostasync, CallAjaxPostasyncReturnHtml: CallAjaxPostasyncReturnHtml, CallAjaxPostFormasync: CallAjaxPostFormasync, CallAjaxPostFormasyncReturnHtml: CallAjaxPostFormasyncReturnHtml, CallAjaxasync: CallAjaxasync, CallAjaxasyncReturnHtml: CallAjaxasyncReturnHtml, CallAjaxExcel: CallAjaxExcel, CallAjaxExcelReturnHtml: CallAjaxExcelReturnHtml,
        disButon: disButon, enButon: enButon, OnTop: OnTop, blockUI: blockUI, unblockUI: unblockUI,
        Success: Success, Notsuccessfully: Notsuccessfully, Alert: Alert, Warning: Warning, Error: Error, ConfirmDialog: ConfirmDialog,
        EncryptS: EncryptS, DecryptS: DecryptS,
        Loading: Loading, HideLoading: HideLoading
    };
})();

//bin run
$(window).bind("load", function () {
    
});