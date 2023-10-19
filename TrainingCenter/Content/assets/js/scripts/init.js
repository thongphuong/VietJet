var _0xd246 = ["metisMenu", "#side-menu", "250px", "slimScroll", ".side-nav-white", "Welcome to Deluxe Material Admin Template!", "black-shadow", "bottom", "right", "animated fadeInUp", "animated fadeOutDown", "notify", "click", ".animation", "animation", "data", "webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend", "animated ", "removeClass", "one", "addClass", ".panel", "parents", "preventDefault", "on", "body", "counterUp", ".counter", "Are you sure?", "All your saved local storage values will be removed!", "warning", "#DD6B55", "Yes, clear it!", "Deleted!", "Your local storage has been cleared.", "success", ".clear-storage", "hide-header", "toggleClass", ".navbar-header", "hide-sidebar", "full-wrapper", "#page-wrapper", "zmdi-arrow-right", ".toggle-sidebar i", ".toggle-sidebar", "ripple", ".effect, .btn, .navbar-left > li > a, #side-menu > li > a, .profile > li > a, .nav-level > li > a, .nav-second-level > li > a, li > .dropdown-toggle, .dropdown-messages > li > a, .dropdown-tasks > li > a, .dropdown-alerts > li > a", "active-box", "#color-switcher", ".setting-colors", "class", "attr", "TL", "split", "background", "#", "css", ".navbar-brand", ".hvr-underline-from-center:before", "background: #", "!important", "addRule", "styleSheets", ".colors li a", "$body", "$fullscreenBtn", ".fullscreen", "launchFullscreen", "prototype", "requestFullscreen", "mozRequestFullScreen", "webkitRequestFullscreen", "msRequestFullscreen", "exitFullscreen", "mozCancelFullScreen", "webkitExitFullscreen", "toggle_fullscreen", "fullscreenEnabled", "mozFullScreenEnabled", "webkitFullscreenEnabled", "fullscreenElement", "mozFullScreenElement", "webkitFullscreenElement", "msFullscreenElement", "documentElement", "init", "FullScreen", "Constructor", "pageScrollElement", "html, body", "App", "load resize", "innerWidth", "window", "width", "screen", "collapse", "div.navbar-collapse", "innerHeight", "height", "min-height", "px", "bind", "location", "parent", "active", "href", "filter", "ul.nav a", "li", "is", "in", "ready"];
$(document)[_0xd246[113]](function () {
    $(_0xd246[1])[_0xd246[0]](), $(_0xd246[4])[_0xd246[3]]({
        height: _0xd246[2]
    }), $(_0xd246[25])[_0xd246[24]](_0xd246[12], _0xd246[13], function (a) {
        var b = $(this)[_0xd246[15]](_0xd246[14]);
        $(this)[_0xd246[22]](_0xd246[21])[_0xd246[20]](_0xd246[17] + b)[_0xd246[19]](_0xd246[16], function () {
            $(this)[_0xd246[18]](_0xd246[17] + b)
        }), a[_0xd246[23]]()
    }), $(_0xd246[27])[_0xd246[26]]({
        delay: 10,
        time: 1e3
    }), $(_0xd246[36])[_0xd246[24]](_0xd246[12], function (a) {
        a[_0xd246[23]](), swal({
            title: _0xd246[28],
            text: _0xd246[29],
            type: _0xd246[30],
            showCancelButton: !0,
            confirmButtonColor: _0xd246[31],
            confirmButtonText: _0xd246[32],
            closeOnConfirm: !1
        }, function () {
            swal(_0xd246[33], _0xd246[34], _0xd246[35])
        })
    }), $(_0xd246[45])[_0xd246[24]](_0xd246[12], function (a) {
        a[_0xd246[23]](), $(_0xd246[39])[_0xd246[38]](_0xd246[37]), $(_0xd246[4])[_0xd246[38]](_0xd246[40]), $(_0xd246[42])[_0xd246[38]](_0xd246[41]), $(_0xd246[44])[_0xd246[38]](_0xd246[43])
    }), $(_0xd246[47])[_0xd246[46]]({
        dragging: !1,
        adaptPos: !1,
        scaleMode: !1
    }), $(_0xd246[50])[_0xd246[24]](_0xd246[12], function (a) {
        a[_0xd246[23]](), $(_0xd246[49])[_0xd246[38]](_0xd246[48])
    }), $(_0xd246[64])[_0xd246[24]](_0xd246[12], function (a) {
        a[_0xd246[23]]();
        var b = $(this)[_0xd246[52]](_0xd246[51]),
            c = b[_0xd246[54]](_0xd246[53]);
        $(_0xd246[58])[_0xd246[57]](_0xd246[55], _0xd246[56] + c[1]), document[_0xd246[63]][0][_0xd246[62]](_0xd246[59], _0xd246[60] + c[1] + _0xd246[61])
    });
    var a = function () {
        this[_0xd246[65]] = $(_0xd246[25]), this[_0xd246[66]] = $(_0xd246[67])
    };
    a[_0xd246[69]][_0xd246[68]] = function (a) {
        a[_0xd246[70]] ? a[_0xd246[70]]() : a[_0xd246[71]] ? a[_0xd246[71]]() : a[_0xd246[72]] ? a[_0xd246[72]]() : a[_0xd246[73]] && a[_0xd246[73]]()
    }, a[_0xd246[69]][_0xd246[74]] = function () {
        document[_0xd246[74]] ? document[_0xd246[74]]() : document[_0xd246[75]] ? document[_0xd246[75]]() : document[_0xd246[76]] && document[_0xd246[76]]()
    }, a[_0xd246[69]][_0xd246[77]] = function () {
        var a = this,
            b = document[_0xd246[78]] || document[_0xd246[79]] || document[_0xd246[80]];
        b && (document[_0xd246[81]] || document[_0xd246[82]] || document[_0xd246[83]] || document[_0xd246[84]] ? a[_0xd246[74]]() : a[_0xd246[68]](document[_0xd246[85]]))
    }, a[_0xd246[69]][_0xd246[86]] = function () {
        var a = this;
        a[_0xd246[66]][_0xd246[24]](_0xd246[12], function () {
            a[_0xd246[77]]()
        })
    }, $[_0xd246[87]] = new a, $[_0xd246[87]][_0xd246[88]] = a;
    var b = function () {
        this[_0xd246[89]] = _0xd246[90], this[_0xd246[65]] = $(_0xd246[25])
    };
    b[_0xd246[69]][_0xd246[86]] = function () {
        $[_0xd246[87]][_0xd246[86]]()
    }, $[_0xd246[91]] = new b, $[_0xd246[91]][_0xd246[88]] = b, $[_0xd246[91]][_0xd246[86]](), $(window)[_0xd246[103]](_0xd246[92], function () {
        var a = 50,
            b = this[_0xd246[94]][_0xd246[93]] > 0 ? this[_0xd246[94]][_0xd246[93]] : this[_0xd246[96]][_0xd246[95]];
        b < 768 ? ($(_0xd246[98])[_0xd246[20]](_0xd246[97]), a = 100) : $(_0xd246[98])[_0xd246[18]](_0xd246[97]);
        var c = (this[_0xd246[94]][_0xd246[99]] > 0 ? this[_0xd246[94]][_0xd246[99]] : this[_0xd246[96]][_0xd246[100]]) - 1;
        c -= a, c < 1 && (c = 1), c > a && $(_0xd246[42])[_0xd246[57]](_0xd246[101], c + _0xd246[102])
    });
    for (var c = window[_0xd246[104]], d = $(_0xd246[109])[_0xd246[108]](function () {
            return this[_0xd246[107]] == c
    })[_0xd246[20]](_0xd246[106])[_0xd246[105]]() ; ;) {
        if (!d[_0xd246[111]](_0xd246[110])) break;
        d = d[_0xd246[105]]()[_0xd246[20]](_0xd246[112])[_0xd246[105]]()
    }
});