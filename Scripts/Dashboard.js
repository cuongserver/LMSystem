$(".dropbtn2").click(function (e) {
    var ele = $(this).parent();
    var dropdown = ele.find(".dropdown-content2");
    dropdown.toggleClass("dropdown-open");
});

$(".list-group > a").click(function (e) {
    $(".list-group > a").removeClass("active-page");
    $(this).addClass("active-page");
    var viewURL = $(this).attr("view-url");
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        $(".xdsoft_datetimepicker").remove();
        filterString = defaultString;
        loadPartialView(viewURL);
        var _parentID = $(this).closest('div').attr('id');
        var _index = $("#" + _parentID + " > .active-page").index();
        deleteCookie("_selector");
        deleteCookie("_index");
        setCookie("_selector", "#" + _parentID + " > a");
        setCookie("_index", _index);
    }
});

$(".home-logout-button").on("click", function () {
    deleteCookie("_selector");
    deleteCookie("_index");
    window.location.href = $(this).attr("view-url")
});

$(".alert-ok").on("click", function () {
    hideAlert();
});

$("#general-detail").on("click",".close" ,function () {
    $("#general-detail").removeAttr("class");
    $("#general-detail").empty();
});

$("#general-detail").on('submit', '#user-edit', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('EditUserInfoResult');       
        if (result == '2') {
            var pageIndex = $(".pager-bar > ul > li.active").attr("page");
            loadPartialView("/User/Overview?pageIndex=" + pageIndex);
            $('#general-detail .close').click();
        }
        else {
            $('#general-detail').html(data);
            $('#general-detail .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#general-detail .form-input').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

$("#mainContent").on('submit', '#changepassword', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('ChangePasswordResult');
        if (result == '1') {
            $('#change-password-successful-message').html(data);
            $('#change-password-successful-message').addClass("mask-active");
        }
        else {
            $('#mainContent').html(data);
            $('#mainContent .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#mainContent .form-control').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

$("#mainContent").on('submit', '#user-addnew', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('Result');
        if (result == '1') {
            loadGeneralMessage(data);
            var viewUrl = "/User/AddNew";
            loadPartialView(viewUrl);
        }
        else {
            $('#mainContent').html(data);
            $('#mainContent .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#mainContent .form-control').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

function loadPartialView(viewURL) {
    activateLoadingMask();
    $("#mainContent").empty();
    $.ajax({
        url: viewURL,
        type: 'GET',
        cache: false
    }).done(function (data, textStatus, jqXHR) {
        deactivateLoadingMask();
        var _url = jqXHR.getResponseHeader('Url');
        var messg = $("#unauthorized-alert").attr("message");
        if (_url.indexOf("/Account/Login") != -1) {
            showAlert(messg);
        }
        else {
            $('#mainContent').html(data);
        }
    }).fail(function (jqXHR, exception) {
        deactivateLoadingMask();
        showError(jqXHR);
    });
};

$("#mainContent").on("click", "a.edit-link, a.udpw-link" ,function () {
    var viewURL = $(this).attr("view-url");
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        loadPartialViewForEdit(viewURL);
    }
});

function loadPartialViewForEdit(viewURL) {
    activateLoadingMask();
    $.ajax({
        url: viewURL,
        type: 'GET',
        cache: false
    }).done(function (data, textStatus, jqXHR) {
        deactivateLoadingMask();
        var _url = jqXHR.getResponseHeader('Url');
        var messg = $("#unauthorized-alert").attr("message");
        if (_url.indexOf("/Account/Login")!= -1) {
            showAlert(messg);
        }
        else {
            $('#general-detail').html(data);
            $('#general-detail').addClass("mask-active")
        }
    }).fail(function (jqXHR, exception) {
        deactivateLoadingMask();
        showError(jqXHR);
    });
};

function loadGeneralMessage(data) {
    var ele = $('#general-message');
    ele.html(data);
    ele.addClass("mask-active").removeClass("mask-inactive");
    ele.on('click', '.alert-ok', function () {
        hideAlert();
    });
};
function showAlert(messg) {
    $('.mask-active').removeClass('mask-inactive');
    $('.mask-active p').text(messg);
};

function hideAlert() {
    $('.mask-active').addClass('mask-inactive');
    $('.mask-active p').text('');
};

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
};

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
};

function deleteCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}

$(document).ready(function () {
    // đánh dấu link đã bấm vào khi load lại Dashboard
    // dãn accordion tương ứng với link
    var _selector = getCookie("_selector");
    var _index = getCookie("_index");
    if (_selector != "" && _index != "") {
        $(_selector).eq(_index).click();
        var _div = $(_selector).eq(_index).parent();
        if (!_div.is($('#account-maintenance')[0])) {
            $('.in').removeClass('in');
            _div.addClass('in');
        }
    };
});

//load data khi chọn page
$("#mainContent").on("click", "ul.pagination a", function () {
    var isDisabled = $(this).parent().hasClass("disabled");
    if (isDisabled == true) return;
    $("ul.pagination > .active").removeClass("active");
    $("ul.pagination >li > a.absolute-page").parent().addClass("active");
    var page = $(this).parent().attr("page");
    var _action = $(".pager-bar").first().attr("with-action");
    var viewURL = _action + "?pageIndex=" + page;
    var filterParam = $(".pager-bar").first().attr("filter-param");
    if (typeof filterParam !== typeof undefined) viewURL = viewURL + "&" + filterString + "x=0";
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        loadPartialView(viewURL);
    }
});

$("#mainContent").on("click", "div.go-to-page > button", function () {
    var pageIndex = $(this).prev().val();
    var _action = $("div.pager-bar").first().attr("with-action");
    $("ul.pagination > .active").removeClass("active");
    $("ul.pagination >li > a.absolute-page").parent().addClass("active");
    var page = pageIndex;
    var viewURL = _action + "?pageIndex=" + page;
    var filterParam = $(".pager-bar").first().attr("filter-param");
    if (typeof filterParam !== typeof undefined) viewURL = viewURL + "&" + filterString + "x=0";
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        loadPartialView(viewURL);
    }
});
//load data khi chọn page(xong)

//admin đổi mk user
$("#general-detail").on('submit', '#user-update-password', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        if (result == '2') {
            var userID = jqXHR.getResponseHeader('userID');
            loadGeneralMessage(data);
            $('#general-detail .close').click();
        }
        else {
            $('#general-detail').html(data);
            $('#general-detail .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#general-detail .form-input').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

//admin đổi mk user(xong)

//admin cập nhật hạn mức nghỉ
$("#general-detail").on('submit', '#quota-edit', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('EditQuotaResult');
        if (result == '1') {
            var pageIndex = $(".pager-bar > ul > li.active").attr("page");
            loadPartialView("/User/QuotaSummary?pageIndex=" + pageIndex);
            $('#general-detail .close').click();
        }
        else {
            $('#general-detail').html(data);
            $('#general-detail .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#general-detail .form-input').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});
//admin cập nhật hạn mức nghỉ(xong)
// gửi yêu cầu nghỉ phép
$("#mainContent").on('submit', '#newApp', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    //alert(form.serialize());
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        if (result == '1') {         
            $("#see-balance").click();
            loadGeneralMessage(data);
        }
        else {
            $('#mainContent').html(data);
            $('#mainContent .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#mainContent .form-control').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});
// gửi yêu cầu nghỉ phép (xong)

// duyet don nghi phep
$("#general-detail").on('submit', '#authorizeApp', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        var authorizeAction = jqXHR.getResponseHeader('authorizeAction');
        var appID = jqXHR.getResponseHeader('appID');

        var ele = $("#" + appID)
        if (authorizeAction == '1') {
            ele.text(ele.attr("result-approve"));
            ele.addClass('stamp is-approved');
        }
        if (authorizeAction == '0') {
            ele.text(ele.attr("result-reject"));
            ele.addClass('stamp is-nope');
        }
        if (result == '1') {
            $('#general-detail .close').click();
        }


    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

// duyet don nghi phep (xong)

//filter don nghi phep admin

var filter0 = "check0=false&op0=equal&criteria0=&";
var filter1 = "check1=false&op1=equal&criteria1=&";
var filter2 = "check2=false&op2=equal&criteria2=&";
var filter3 = "check3=false&op3=equal&criteria3=&";
var filter4 = "check4=false&op4=equal&criteria4=&";
var filter5 = "check5=false&op5=equal&criteria5=&";
var defaultString = filter0 + filter1 + filter2 + filter3 + filter4 + filter5;
var filterString = filter0 + filter1 + filter2 + filter3 + filter4 + filter5;
var commonName = "filter";
var navbarToggler = false;

$("#mainContent").on('change', '#navbarToggleExternalContent input[type=checkbox]', function (e) {
    var thisClass = $(this).attr("filterClass");
    var boolCheck = $(this).prop("checked");
    $(this).val(boolCheck);
    if (boolCheck == true) {
        $("#navbarToggleExternalContent ." + thisClass).removeClass("not-used").removeAttr("disabled");
    };
    if (boolCheck == false) {
        $("#navbarToggleExternalContent ." + thisClass).addClass("not-used").attr("disabled", "disabled")
        $("#navbarToggleExternalContent ." + thisClass + ":eq(1)").val(null);
    };
});

$("#mainContent").on('click', '#refreshList', function (e) {
    var allFilters = $("#navbarToggleExternalContent [name=" + commonName + "]");
    var x = allFilters.length - 1;
    var arr = [];
    const regex = /(=)([^=&]*)(&)/ug;
    for (var i = 0; i < x + 1; i += 1) {
        arr.push(allFilters.eq(i).val());
        var t = -1;
        filterString = filterString.replace(regex, function (match) {
            t++;
            return (t === i) ? "=" + encodeURIComponent(allFilters.eq(i).val()) + "&" : match;
        });
    };

    $(".pager-bar").first().attr("filter-param", filterString);
    var _action = $(".pager-bar").first().attr("with-action");
    var viewURL = _action + "?pageIndex=1";
    var filterParam = $(".pager-bar").first().attr("filter-param");
    if (typeof filterParam !== typeof undefined) viewURL = viewURL + "&" + filterString + "x=0";
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        loadPartialView(viewURL);
    }
});
//filter don nghi phep admin (xong)

//admin vo hieu hoa don nghi phep

$("#mainContent").on('click', '#pendingApp .terminate-link', function (e) {
    var form = $(this).parent();
    var method = form.attr("method");
    var url = form.attr('action');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        var appID = jqXHR.getResponseHeader('appID');
        var ele = $("#" + appID)
        if (result == '1') {
            ele.text(ele.attr("result-terminate"));
            ele.addClass('stamp is-nope');
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

// them holiday
$("#mainContent").on('submit', '#system-setting-add-holiday', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        if (result == '1') {
            loadGeneralMessage(data);
            var viewUrl = "/SystemSetting/NewPublicHoliday";
            loadPartialView(viewUrl);
        }
        else {
            $('#mainContent').html(data);
            $('#mainContent .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#mainContent .form-control').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

// them holiday (het)

// xoa holiday
$("#mainContent").on('click', '#list-holiday .terminate-link', function (e) {
    var form = $(this).parent();
    var method = form.attr("method");
    var url = form.attr('action');
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        var id = jqXHR.getResponseHeader('id');
        var ele = $("#" + id)
        loadGeneralMessage(data);
        if (result == '1') {
            ele.remove();
        }
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
    });
});

// xoa holiday (het)

// appByAdmin
$("#mainContent").on('click', '#newAppByAdmin .dropbtn', function () {
    $("#dropdown-list").toggleClass("show");
});

$("#mainContent").on('keyup', '#newAppByAdmin #search-input', function () {
    var filter, a, i, txtValue;
    filter = $(this).val().toUpperCase();
    a = $("#dropdown-list a");
    for (i = 0; i < a.length; i++) {
        txtValue = a[i].textContent || a[i].innerText;
        if (txtValue.toUpperCase().indexOf(filter) > -1) {
            a[i].style.display = "";
        }
        else {
            a[i].style.display = "none";
        }
    }
});

$("#mainContent").on('click', "#dropdown-list a", function () {
    var userid = $(this).text();
    var username = $(this).attr("prop2");
    var deptname = $(this).attr("prop4");
    var rank = $(this).attr("prop6");
    var input = $("#target-input");
    input.val(userid);
    $("#dropdown-list").toggleClass("show");
    $("#user-detail").text(username + " - " + deptname + " ( " + rank + " )").removeClass("disp-none");
});

$("#mainContent").on('keyup', '#newAppByAdmin #target-input', function () {
    var filter, a, i, txtValue, div;
    div = $("#user-detail");
    filter = $(this).val();
    a = $("#dropdown-list a");
    console.log(filter);
    for (i = 0; i < a.length; i++) {
        txtValue = a[i].textContent || a[i].innerText;
        if (txtValue == filter) {
            var username = a.eq(i).attr("prop2");
            var deptname = a.eq(i).attr("prop4");
            var rank = a.eq(i).attr("prop6");
            div.text(username + " - " + deptname + " ( " + rank + " )").removeClass("disp-none");
            return;
        }
    }
    div.text("").addClass("disp-none");
});


$("#mainContent").on('submit', '#newAppByAdmin', function (e) {
    e.preventDefault();
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    activateLoadingMask();
    $.ajax({
        type: method,
        url: url,
        data: form.serialize()
    }).done(function (data, textStatus, jqXHR) {
        var result = jqXHR.getResponseHeader('executionResult');
        if (result == '1') {
            $("#mainContent").empty();
            loadGeneralMessage(data);
        }
        else {
            $("#mainContent").empty();
            $('#mainContent').html(data);
            $('#mainContent .validate-input').each(function () {
                var errorContent = $(this).attr('data-validate');
                if (errorContent.length != 0) {
                    $(this).addClass('alert-validate');
                }
            });
            $('#mainContent .form-control').each(function () {
                $(this).on('focus', function () {
                    $(this).parent().removeClass("alert-validate");
                })
            });
        }
        deactivateLoadingMask();
    }).fail(function (jqXHR, exception) {
        showError(jqXHR);
        deactivateLoadingMask();
    });
    
});
// appByAdmin (het)
//show leave balance
$("#mainContent").on('click', '#filter-by-year button', function (e) {
    var ele = $("#filter-by-year select")
    var but = $("#filter-by-year button")
    var reportYear = ele.val();
    var viewURL = but.attr("view-url") + "?reportYear=" + reportYear;
    if (typeof viewURL !== typeof undefined && viewURL !== false) {
        loadPartialView(viewURL);
    }
});
//download list app addmin
$("#mainContent").on('click', '#download-list', function (e) {
    var allFilters = $("#navbarToggleExternalContent [name=" + commonName + "]");
    var x = allFilters.length - 1;
    var arr = [];
    const regex = /(=)([^=&]*)(&)/ug;
    for (var i = 0; i < x + 1; i += 1) {
        arr.push(allFilters.eq(i).val());
        var t = -1;
        filterString = filterString.replace(regex, function (match) {
            t++;
            return (t === i) ? "=" + encodeURIComponent(allFilters.eq(i).val()) + "&" : match;
        });
    };

    var _action = $(this).attr("with-action");
    var viewURL = _action + "?pageIndex=-1";
    viewURL = viewURL + "&" + filterString + "x=0";
    window.location.href = viewURL;
});

//download list app user
$("#mainContent").on('click', '#download-list-my-apps', function (e) {
    var _action = $(this).attr("with-action");
    var viewURL = _action
    window.location.href = viewURL;
});