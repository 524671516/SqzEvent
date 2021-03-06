﻿//实例化framework7
var myApp = new Framework7({
    swipeBackPage:false,
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
//图标点击颜色变化
$$(".icon-link").each(function () {
    $$(this).on("click", function () {
        $$(".icon-link").removeClass("active");
        $$(this).addClass("active");
    });
});
// 实时状态页
myApp.onPageInit("Home", function (page) {
});
//实时状态信息详细页
myApp.onPageInit("Manager_agendadetails", function (page) {
    PhotoBrowser("manager_agendadetails");
    createCalendar("SelectDate", false, true)
    $$.ajax({
        url: "/QualityControl/Manager_AgendaDetailsList",
        data: {
            fid: $$("#FactoryId").val(),
            date: $$("#SelectDate").val()
        },
        success: function (data) {
            $$(".tempalate-select").html(data);
            if ($$("#StaffId").val() != "") {
                $$.ajax({
                    url: "/QualityControl/Manager_AgendaDetailsPartial",
                    data: {
                        agendaId: $$("#StaffId").val()
                    },
                    success: function (data) {
                        $$(".tempalate-info").html(data);
                        $$("#StaffId").on("change", function () {
                            if ($$("#StaffId").val() != "") {
                                $$.ajax({
                                    url: "/QualityControl/Manager_AgendaDetailsPartial",
                                    data: {
                                        agendaId: $$("#StaffId").val()
                                    },
                                    success: function (data) {
                                        $$(".tempalate-info").html(data);
                                    }
                                })
                            } else {
                                $$(".tempalate-info").html("");
                            }
                        });                                        
                    }
                })
            } else {
                $$(".tempalate-info").html("");
            }
        }
    })
    $$("#SelectDate").on("change", function () {
        var date = $$(this).val();
        $$.ajax({
            url: "/QualityControl/Manager_AgendaDetailsList",
            data: {
                fid: $$("#FactoryId").val(),
                date: date
            },
            success: function (data) {
                $$(".tempalate-select").html(data);
                if ($$("#StaffId").val() != "") {
                    $$.ajax({
                        url: "/QualityControl/Manager_AgendaDetailsPartial",
                        data: {
                            agendaId: $$("#StaffId").val()
                        },
                        success: function (data) {  
                            $$(".tempalate-info").html(data);
                        }
                    })
                } else {
                    $$(".tempalate-info").html("");
                }
                $$("#StaffId").on("change", function () {
                    if ($$("#StaffId").val() != "") {
                        $$.ajax({
                            url: "/QualityControl/Manager_AgendaDetailsPartial",
                            data: {
                                agendaId: $$("#StaffId").val()
                            },
                            success: function (data) {
                                $$(".tempalate-info").html(data);                               
                            }
                        })
                    } else {
                        $$(".tempalate-info").html("");
                    }
                });
            }
        });
    });
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
            $$('.calendar-custom-toolbar .center').text(p.currentYear + "年" + monthNames[p.currentMonth]);
            $$('.calendar-custom-toolbar .left .link').on('click', function () {
                calendarInline.prevMonth();
            });
            $$('.calendar-custom-toolbar .right .link').on('click', function () {
                calendarInline.nextMonth();
            });
            $$.ajax({
                url: "/QualityControl/Manager_MonthChange",
                data: {
                    year: p.currentYear,
                    month: p.currentMonth + 1
                }, type: "post",
                success: function (e) {
                    var data = JSON.parse(e);
                    for (var i = 0; i < data.result.length; i++) {                     
                        var _date = ChangeDateFormat(data.result[i].Key);
                        _date1 = data.result[i].Key.substring(data.result[i].Key.indexOf("(")+1, data.result[i].Key.indexOf(")"))
                        if (data.result[i].result) {
                            $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-green");
                        }
                        else {
                            var _today = new Date().getTime();                         
                            if (_today >= _date1) {
                                $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-red");
                            }
                            else {
                                $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-gray");
                            }
                        }
                    }
                    month = p.currentMonth + 1
                    day = new Date();
                    _day = day.getDate();
                    $$.ajax({
                        url: "/QualityControl/Manager_ScheduleDetails",
                        data: {
                            date: p.currentYear + "-" + month + "-" + _day
                        },
                        success: function (data) {
                            $$(".list-describe").html(data);
                        }
                    });
                }
            });
        },
        onMonthYearChangeEnd: function (p) {
            $$('.calendar-custom-toolbar .center').text(p.currentYear + "年" + monthNames[p.currentMonth]);
            $$.ajax({
                url: "/QualityControl/Manager_MonthChange",
                data: {
                    year: p.currentYear,
                    month: p.currentMonth + 1
                }, type: "post",
                success: function (e) {
                    var data = JSON.parse(e);
                    for (var i = 0; i < data.result.length; i++) {
                        var _date = ChangeDateFormat(data.result[i].Key);
                        _date1 = data.result[i].Key.substring(data.result[i].Key.indexOf("(") + 1, data.result[i].Key.indexOf(")"))
                        if (data.result[i].result) {
                            $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-green");
                        }
                        else {
                            var _today = new Date().getTime();
                            if (_today >= _date1) {
                                $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-red");
                            }
                            else {
                                $(".picker-calendar-day[data-date='" + _date + "']").find("span").addClass("picker-calendar-day-gray");
                            }
                        }
                    }
                }
            });
        },
        onDayClick: function (p, daycontainer, year, month, day) {
            month = parseInt(month) + 1;
            $$.ajax({
                url: "/QualityControl/Manager_ScheduleDetails",
                data: {
                    date: year + "-" + month + "-" + day
                },
                success: function (data) {
                    $$(".list-describe").html(data);
                }
            });
        },
    });

});
//添加产量计划页
myApp.onPageInit('Add-schedule', function (page) {
    createCalendar("DateList",true,false)
    $$("#FactoryId").on("change", function () {
        if ($$("#FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_AddSchedulePartial",
                data: {
                    fid: $$("#FactoryId").val()
                },
                success: function (data) {
                    $$(".tempalate-content").html(data);
                }
            })
        } else {
            $$(".tempalate-content").html("");
        }
    })
    $("#manager_addschedule-form").validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#addschedule-submit").prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("addschedule-submit", "manager_addschedule-form")
        }
    });
    $$("#addschedule-submit").on("click", function () {
        if (!$$("#addschedule-submit").prop("disabled")) {
            $$("#addschedule-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $("#manager_addschedule-form").submit();
            }, 500)
        }
    })
})
// 历史检验页面查询
myApp.onPageInit('manager-qualitytest', function (page) {
    changeBtn()
    createCalendar('Qt_SelectDate', false, true);
    if ($$("#Qt_fid").val() != "") {
        $$.ajax({
            url: "/QualityControl/Manager_QualityTestPartial",
            data: {
                fid: $$("#Qt_fid").val(),
                date: $$("#Qt_SelectDate").val()
            },
            success: function (data) {
                $$("#qualitytest-list").html(data);
            }
        })
    }
    $$("#Qt_fid").on("change", function () {
        var fid = $$(this).val();
        if ($$(this).val()!="") {
            $$.ajax({
                url: "/QualityControl/Manager_QualityTestPartial",
                data: {
                    fid: fid,
                    date: $$("#Qt_SelectDate").val()
                },
                success: function (data) {
                    $$("#qualitytest-list").html(data);
                }
            });
        } else {
            $$("#qualitytest-list").html("");
        }
    })
    $$("#Qt_SelectDate").on("change", function () {
        var fid = $$("#Qt_fid").val();
        if ($$("#Qt_fid").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_QualityTestPartial",
                data: {
                    fid: fid,
                    date:$$("#Qt_SelectDate").val()
                },
                success: function (data) {
                    $$("#qualitytest-list").html(data);
                }
            })
        }
    })
});
//历史故障页面查询
myApp.onPageInit("manager-breakdown", function () {
    createCalendar('Bd_SelectDate', false, true);
    if ($$("#Bd_fid").val() != "") {
        $$.ajax({
            url: "/QualityControl/Manager_BreakdownPartial",
            data: {
                fid: $$("#Bd_fid").val(),
                date: $$("#Bd_SelectDate").val()
            },
            success: function (data) {
                $$("#breakdown-list").html(data);
            }
        })
    }
    $$("#Bd_fid").on("change", function () {
        if ($$(this).val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_BreakdownPartial",
                data: {
                    fid:$$("#Bd_fid").val(),
                    date:$$("#Bd_SelectDate").val()
                },
                success: function (data) {                  
                    $$("#breakdown-list").html(data);
                }
            })
        } else {
            $$("#breakdown-list").html("");
        }
    })
    $$("#Bd_SelectDate").on("change", function () {
        if ($$("#Bd_fid").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_BreakdownPartial",
                data: {
                    fid: $$("#Bd_fid").val(),
                    date: $$("#Bd_SelectDate").val()
                },
                success: function (data) {
                    $$("#breakdown-list").html(data);
                }
            })
        }
    })
    
})
//历史定期检测记录页
myApp.onPageInit("manager-regulartest", function (page) {
    $$("#FactoryId").on("change", function () {
        if ($$("#FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_QualityRegularTestPartial",
                data: {
                    fid: $$("#FactoryId").val()
                },
                success:function(data){
                    $$("#regulartest-list").html(data);
                }
            })
        } else {
            $$("#regulartest-list").html("");
        }
    })
    PhotoBrowser("manager-regulartest");
})
//历史质检信息详情页
myApp.onPageInit("Manager_QualityTestDetail", function (page) {
    PhotoBrowser("Manager_QualityTestDetail");
})
//历史故障信息详情页
myApp.onPageInit("Manager_BreakdownDetail", function (page) {
    PhotoBrowser("Manager_BreakdownDetail");
})
//日期转化
function ChangeDateFormat(val) {
    if (val != null) {
        var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
        //月份为0-11，所以+1，月份小于10时补个0
        var month = date.getMonth();
        var currentDate = date.getDate();
        return date.getFullYear() + "-" + (month) + "-" + currentDate;
    }
    return "";
}
//文本验证
function CheckError(SubmitBtn, SubmitForm) {
    var pass = true;
    // 先字段信息
    $("input.required").each(function () {
        if ($(this).val() == "") {
            $(this).attr("placeholder", "请输入信息").addClass("invalid-input");
            pass = false;
        } else {
            $(this).removeClass("invalid-input");
        }
    });
    $("input.isnumber").each(function () {
        if ($(this).val() != "") {
            if (!isPInt($(this).val())) {
                $(this).attr("placeholder", "请输入合法数字").addClass("invalid-input");
                pass = false;
            }
        } else {
            $(this).val("0");
        }
    });
    $("textarea.required").each(function () {
        if ($(this).val() == "") {
            $(this).attr("placeholder", "请输入备注信息").addClass("invalid-input");
            pass = false;
        }
    });
    $("textarea.maxlength_200").each(function () {
        if ($(this).length > 200) {
            $(this).attr("placeholder", "超过字数").addClass("invalid-input");
            pass = false;
        }
    });
    // 然后弹窗信息
    if (pass) {
        $("select.required").each(function () {
            if ($(this).val() == "- 请选择 -" || $(this).val() == null || $(this).val() == "") {
                myApp.alert($(this).next().find(".item-title").html() + " 未选择");
                pass = false;
            }
        });
    }
    if (pass) {
        $(".photos").each(function () {
            var photoList = splitArray($(this).val());
            if (photoList.length == 0) {
                myApp.alert("至少上传一张照片");
                pass = false;
            }
        });
    }
    // 提交字段
    if (pass) {
        $("#" + SubmitBtn).prop("disabled", true).addClass("color-gray");
        myApp.showIndicator();
        setTimeout(function () {
            $("#" + SubmitForm).ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
                else if (data == "MODIFIED") {
                    myApp.hideIndicator();
                    mainView.router.back();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单修改成功"
                    });
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
                else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $("#" + SubmitBtn).prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        }, 500)
    } else {
        $("#" + SubmitBtn).prop("disabled", false).removeClass("color-gray");
    }
}
//判断是否是正整数
function isPInt(str) {
    if (!isNaN(str)) {
        if (str >= 0 && str != "")
            return true;
    } else { return false; }
}
// 图片数组转化
function splitArray(value) {
    var list = new Array();
    if (value != null) {
        if (value.trim() != "") {
            list = value.trim().split(",");
            return list;
        }
    }
    return list;
}
// 图片浏览模块
function PhotoBrowser(pagename) {
    $$("#" + pagename).on("click", ".qc-photos", function () {
        var photos = $(this).attr("data-photos");
        if (photos.trim() != "") {
            var images = photos.split(",");
            for (var i = 0; i < images.length; i++) {
                images[i] = "https://cdn2.shouquanzhai.cn/qc-img/" + images[i];
            }
            var myPhotoBrowser = myApp.photoBrowser({
                zoom: 500,
                photos: images,
                theme: 'dark',
                backLinkText: '关闭',
                toolbar: false,
                ofText: '/'
            });
            myPhotoBrowser.open();
        } else {
            myApp.alert("没有找到图片");
        }
    });
}
//按钮切换
function changeBtn() {
    $$(".sort-btn").each(function () {
        $$(this).on("click", function () {
            if ($$("#Qt_fid").val() != "") {
                if ($$(this).children("span").hasClass("sort-time")) {
                    $$(this).children("span").removeClass("sort-time");
                    $$(this).children("span").addClass("sort-product");
                    $$.ajax({
                        url: "/QualityControl/Manager_QualityTestPartial",
                        data: {
                            fid: $$("#Qt_fid").val(),
                            date: $$("#Qt_SelectDate").val(),
                            sorttype: true
                        },
                        success: function (data) {
                            $$("#qualitytest-list").html(data);
                        }
                    })
                }
                else {
                    $$(this).children("span").addClass("sort-time");
                    $$(this).children("span").removeClass("sort-product");
                    $$.ajax({
                        url: "/QualityControl/Manager_QualityTestPartial",
                        data: {
                            fid: $$("#Qt_fid").val(),
                            date: $$("#Qt_SelectDate").val(),
                            sorttype: false
                        },
                        success: function (data) {
                            $$("#qualitytest-list").html(data);
                        }
                    })
                }

            }
        })
    })
}
//日历生成
function createCalendar(calinput, multiplebool, closeOnSelectbool) {
    var calendarMultiple = myApp.calendar({
        input: '#'+calinput,
        dateFormat: 'yyyy-mm-dd',
        multiple: multiplebool,
        closeOnSelect: closeOnSelectbool,
        monthNames: ['1月', '2月', '3月', '4月 ', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'],
        dayNamesShort: ['日', '一', '二', '三', '四', '五', '六'],
        toolbarTemplate: '<div class="toolbar">' + '<div class="toolbar-inner">' +
            '<div class="picker-calendar-year-picker">' +
            '<a href="#" class="link icon-only picker-calendar-prev-year">' +
            '<i class="icon icon-prev">' + '</i>' + '</a>' + '<span class="current-year-value">' + '</span>' +
            '<a href="#" class="link icon-only picker-calendar-next-year">' +
            '<i class="icon icon-next">' + '</i>' + '</a>' + '</div>' +
            '<div class="picker-calendar-month-picker">' +
            '<a href="#" class="link icon-only picker-calendar-prev-month">' +
            '<i class="icon icon-prev">' + '</i>' + '</a>' + '<span class="current-month-value">' +
            '</span>' + '<a href="#" class="link icon-only picker-calendar-next-month">' +
            '<i class="icon icon-next">' + '</i>' + '</a>' + '</div>' + '</div>' + '</div>'
    });
    return calendarMultiple;
}
