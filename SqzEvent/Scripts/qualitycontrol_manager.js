//实例化framework7
var myApp = new Framework7({
    modalTitle: '生产管理',
    pushState: true,
});
var $$ = Dom7;
var mainView = myApp.addView('.view-main', {
    dynamicNavbar: true,
});
$$(document).on("ajaxStart", function (e) {
    if (e.detail.xhr.requestUrl.indexOf("autocomplete-languages.json") >= 0) {
        return;
    }
    myApp.showIndicator();
});
$$(document).on("ajaxComplete", function (e) {
    if (e.detail.xhr.requestUrl.indexOf("autocomplete-languages.json") >= 0) {
        return;
    }
    myApp.hideIndicator();
});
wx.config({
    debug: false,
    // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
    appId: $("#appId").text(),
    // 必填，公众号的唯一标识
    timestamp: $("#timeStamp").text(),
    // 必填，生成签名的时间戳
    nonceStr: $("#nonce").text(),
    // 必填，生成签名的随机串
    signature: $("#signature").text(),
    // 必填，签名，见附录1
    jsApiList: ["uploadImage", "downloadImage", "chooseImage", "getLocation", "previewImage", "openLocation", "scanQRCode"]
});
//获取个人信息
$$.ajax({
    url: "/QualityControl/UserInfoPartial",
    type: "post",
    success: function (data) {
        $$("#userinfo").html(data);
    }
});
//设置页
myApp.onPageInit('Setting', function (page) {
    var monthNames = ['1月', '2月', '3月', '4月 ', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'];
    var calendarInline = myApp.calendar({
        container: '#calendar-inline-container',
        value: [new Date()],
        weekHeader: false,
        toolbarTemplate:
            '<div class="toolbar calendar-custom-toolbar">' +
                '<div class="toolbar-inner">' +
                    '<div class="left">' +
                        '<a href="#" class="link icon-only"><i class="icon icon-back"></i></a>' +
                    '</div>' +
                    '<div class="center"></div>' +
                    '<div class="right">' +
                        '<a href="#" class="link icon-only"><i class="icon icon-forward"></i></a>' +
                    '</div>' +
                '</div>' +
            '</div>',

        onOpen: function (p) {
            $$('.calendar-custom-toolbar .center').text(p.currentYear+"年"+monthNames[p.currentMonth]);
            $$('.calendar-custom-toolbar .left .link').on('click', function () {
                calendarInline.prevMonth();
            });
            $$('.calendar-custom-toolbar .right .link').on('click', function () {
                calendarInline.nextMonth();
            });
            $$(".picker-modal-inner .picker-calendar-day span").append("<i class=\"hidden\">199<i>");
            $(".picker-modal-inner .picker-calendar-day span .hidden").each(function () {
                var planyield = 2000;
                var actualyield = parseInt($(this).text());
                if (actualyield < planyield) {
                    $(this).parent().addClass("picker-calendar-day-green");
                }else{
                    $(this).parent().addClass("picker-calendar-day-red");
                }
            })
        },
        onMonthYearChangeStart: function (p) {
            $$('.calendar-custom-toolbar .center').text(p.currentYear + "年" + monthNames[p.currentMonth]);
            $$(".picker-modal-inner .picker-calendar-day span").append("<i class=\"hidden\">199<i>");
            $(".picker-modal-inner .picker-calendar-day span .hidden").each(function () {
                var planyield = 2000;
                var actualyield = parseInt($(this).text());
                if (actualyield < planyield) {
                    $(this).parent();
                } else {
                    $(this).parent();
                }
            })
        },
        onDayClick: function (p, daycontainer, year, month, day) {
        },
        onMonthAdd: function (p,monthContainer) {
        }
    });

})