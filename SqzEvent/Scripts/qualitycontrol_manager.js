﻿//实例化framework7
var myApp = new Framework7({
    swipeBackPage:false,
    modalTitle: '生产管理',
    pushState: true,
    cache: false,
    domCache: false,
    cacheIgnoreGetParameters: false,
    smartSelectInPopup: false,
    swipeBackPage:false

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
//tab-list 底部工具栏转换
$$(".tab-link").on("click", function (data) {
    if (!$$(this).hasClass("active")) {
        var url = $$(this).attr("data-href");
        mainView.router.load({
            url: url,
            animatePages: false
        });
        $(this).addClass("active").siblings().removeClass("active");
    }
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
/*检测详情页*/
myApp.onPageInit("qualitytestdetails", function (page) {
    $("#qualitytestdetails").on("click", ".scan-input", function () {
        var $input = $$(this);
        wx.scanQRCode({
            needResult: 1, // 默认为0，扫描结果由微信处理，1则直接返回扫描结果，
            scanType: ["barCode"], // 可以指定扫二维码还是一维码，默认二者都有
            success: function (res) {
                var result = res.resultStr; // 当needResult 为 1 时，扫码返回的结果
                var _result = result.toString().substr(result.toString().lastIndexOf(",") + 1);
                $input.val(_result);
            }
        });
    });
    $(".one_photo").each(function () {
        var pid = $(this).attr("Id");
        var uid = $(this).parent().parent().find("ul").attr("Id");
        var countid = $(this).parent().parent().parent().parent().find("abbr").attr("Id");
        uploadCheckinFile("addqualitytest-form", uid, pid, countid, 1);
    })
    PhotoBrowser("qualitytestdetails");
    $("#edit-qt").on("click", function () {
        $("#edit-qt-form").ajaxSubmit(function (data) {
            if (data.result == "SUCCESS") {
                mainView.router.back();
                myApp.addNotification({
                    title: "通知",
                    message: "表单修改成功"
                });
            } else {
                mainView.router.back();
                myApp.addNotification({
                    title: "通知",
                    message: "表单修改失败"
                });
            }
            setTimeout(function () {
                myApp.closeNotification(".notifications");
            }, 2e3);
        });
    })
})
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
myApp.onPageAfterAnimation('manager-equipment', function () {
    if ($$("#equipment_fid").val() != "") {
        $$.ajax({
            url: "/QualityControl/Manager_QCEquipmentListPartial",
            data: {
                fid: $$("#equipment_fid").val()
            },
            success: function (data) {
                $$("#equipment-list").html(data);
                $$("#equipmentbar .right a").prop("href", "/QualityControl/Manager_CreateEquipment" + "?fid=" + $$("#equipment_fid").val());
            }
        })
    }
})
myApp.onPageInit('manager-equipment', function (page) {
    $$('#manager-equipment').on('deleted',".swipeout", function (e) {
        $$.ajax({
            url: $(this).attr("data-url"),
            type: "post",
            success: function (data) {
                var _data = JSON.parse(data);
                if (_data.result == "SUCCESS") {
                    myApp.alert("删除成功");
                    if ($$("#equipment_fid").val() != "") {
                        $$.ajax({
                            url: "/QualityControl/Manager_QCEquipmentListPartial",
                            data: {
                                fid: $$("#equipment_fid").val()
                            },
                            success: function (data) {
                                $$("#equipment-list").html(data);
                            }
                        })
                    } else {
                        $$("#equipment-list").html("");
                    }
                } else {
                    myApp.alert("删除失败");
                }
            }
        })
    });
    $$("#factory-select").on("change", function () {
        if ($$("#equipment_fid").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_QCEquipmentListPartial",
                data: {
                    fid: $$("#equipment_fid").val()
                },
                success: function (data) {
                    $$("#equipment-list").html(data);
                    $$("#equipmentbar .right a").prop("href", "/QualityControl/Manager_CreateEquipment" + "?fid=" + $$("#equipment_fid").val());
                }
            })
        } else {
            $$("#equipment-list").html("");
            $$("#equipmentbar .right a").prop("href", "/QualityControl/Manager_CreateEquipment" + "?fid=" + $$("#equipment_fid").val());
        }
    });
});
//新增设备
myApp.onPageInit('addequipment', function (page) {
    createCalendar("ManufactureDate", false, true);
    //效验规则
    $("#addequipment-form").validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#addequipment-submit").prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("addequipment-submit", "addequipment-form")
        }
    });
    //按钮点击事件
    $("#addequipment-submit").on("click", function () {
        if (!$("#addequipment-submit").prop("disabled")) {
            $("#addequipment-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $("#addequipment-form").submit();
            }, 500);
        }
    });
    //textarea字数计算
    currentTextAreaLength("addequipment-form", "Subscribe", 200, "addequipment-currentlen");
});
//修改设备信息
myApp.onPageInit('editequipment', function (page) {
    createCalendar("ManufactureDate", false, true)
    //效验规则
    $("#editequipment-form").validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#editequipment-submit").prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("editequipment-submit", "editequipment-form");
        }
    });
    //按钮点击事件
    $("#editequipment-submit").on("click", function () {
        if (!$("#editequipment-submit").prop("disabled")) {
            $("#editequipment-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $("#editequipment-form").submit();
            }, 500);
        }
    });
    //textarea字数计算
    currentTextAreaLength("editequipment-form", "Subscribe", 200, "editequipment-currentlen");
});
//设备故障页
myApp.onPageAfterAnimation('manager_breakdownlist', function () {
    if ($$("#manager_breakdownlist #FactoryId").val() != "" && $$("#manager_breakdownlist #EquipmentId").val() != "") {
        $$("#breakdownbar .right a").prop("href", "/QualityControl/Manager_CreateBreakdown" + "?Fid=" + $$("#manager_breakdownlist #FactoryId").val() + "&&Eid=" + $$("#manager_breakdownlist #EquipmentId").val());
        $$.ajax({
            url: "/QualityControl/Manager_BreakdownListPartial",
            data: {
                Eid: $$("#manager_breakdownlist #EquipmentId").val()
            },
            success: function (data) {
                $$("#breakdowntype-list").html(data);
            }
        })
    }
})
myApp.onPageInit('manager_breakdownlist', function (page) {
    $$('#manager_breakdownlist').on('deleted', ".swipeout", function (e) {
        $$.ajax({
            url: $(this).attr("data-url"),
            type: "post",
            success: function (data) {
                var _data = JSON.parse(data);
                if (_data.result == "SUCCESS") {
                    myApp.alert("删除成功");
                    if ($$("#EquipmentId").val() != "") {
                        $$.ajax({
                            url: "/QualityControl/Manager_BreakdownListPartial",
                            data: {
                                Eid: $$("#manager_breakdownlist #EquipmentId").val()
                            },
                            success: function (data) {
                                $$("#breakdowntype-list").html(data);
                            }
                        })
                    } else {
                        $$("#breakdowntype-list").html("");
                    }
                } else {
                    myApp.alert("删除失败");
                }
            }
        })
    });
    $$("#manager_breakdownlist #factory-select").on("change", function () {
        $$("#breakdowntype-list").html("");
        $$("#manager_breakdownlist #EquipmentId").html("");
        $$("#manager_breakdownlist #EquipmentId").append("<option value=\"\">- 请选择 -</option>");
        $$("#manager_breakdownlist #equipment-select .item-after").text("- 请选择 -");
        if ($$("#manager_breakdownlist #FactoryId").val() != "") {
            $$("#breakdownbar .right a").prop("href", "/QualityControl/Manager_CreateBreakdown");
            $$.ajax({
                url: "/QualityControl/RefreshEquipmentListAjax",
                data: {
                    factoryId: $$("#manager_breakdownlist #FactoryId").val()
                },
                type: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        for (var i = 0; i < data.content.length; i++) {
                            $$("#manager_breakdownlist #EquipmentId").append("<option value=\"" + data.content[i].Id + "\">" + data.content[i].Name + "</option>");
                        }
                    }
                }
            });
        }
    })
    $$("#manager_breakdownlist #equipment-select").on("change", function () {
        if ($$("#manager_breakdownlist #FactoryId").val() != "" && $$("#manager_breakdownlist #EquipmentId").val() != "") {
            $$("#breakdownbar .right a").prop("href", "/QualityControl/Manager_CreateBreakdown" + "?Fid=" + $$("#manager_breakdownlist #FactoryId").val() + "&&Eid=" + $$("#manager_breakdownlist #EquipmentId").val());
            $$.ajax({
                url: "/QualityControl/Manager_BreakdownListPartial",
                data:{
                    Eid: $$("#manager_breakdownlist #EquipmentId").val()
                },
                success: function (data) {
                    $$("#breakdowntype-list").html(data);
                }
            })
        } else {
            $$("#breakdowntype-list").html("");
            myApp.alert("请选择工厂或设备")
        }
    })
});
//新增故障类型页
myApp.onPageInit('addbreakdowntype', function (page) {
    $$("#addbreakdowntype #factory-select").on("change", function () {
        $$("#addbreakdowntype #EquipmentId").html("");
        $$("#addbreakdowntype #EquipmentId").append("<option value=\"\">- 请选择 -</option>");
        $$("#addbreakdowntype #equipment-select .item-after").text("- 请选择 -");
        if ($$("#addbreakdowntype #FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshEquipmentListAjax",
                data: {
                    factoryId: $$("#addbreakdowntype #FactoryId").val()
                },
                type: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        for (var i = 0; i < data.content.length; i++) {
                            $$("#addbreakdowntype #EquipmentId").append("<option value=\"" + data.content[i].Id + "\">" + data.content[i].Name + "</option>");
                        }
                    }
                }
            });
        }
    })
    //效验规则
    $("#addbreakdowntype-form").validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#addbreakdowntype-submit").prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("addbreakdowntype-submit", "addbreakdowntype-form");
        }
    });
    //按钮点击事件
    $("#addbreakdowntype-submit").on("click", function () {
        if (!$("#addbreakdowntype-submit").prop("disabled")) {
            $("#addbreakdowntype-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $("#addbreakdowntype-form").submit();
            }, 500);
        }
    });
    //textarea字数计算
    currentTextAreaLength("addbreakdowntype-form", "Describe", 200, "addDescribe-currentlen");
    currentTextAreaLength("addbreakdowntype-form", "Recommand", 200, "addRecommand-currentlen");
});
myApp.onPageInit('editbreakdown', function (page) {
    //效验规则
    $("#editbreakdown-form").validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $("#editbreakdown-submit").prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("editbreakdown-submit", "editbreakdown-form");
        }
    });
    //按钮点击事件
    $("#editbreakdown-submit").on("click", function () {
        if (!$("#editbreakdown-submit").prop("disabled")) {
            $("#editbreakdown-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $("#editbreakdown-form").submit();
            }, 500);
        }
    });
    //textarea字数计算
    currentTextAreaLength("editbreakdown-form", "Describe", 200, "editDescribe-currentlen");
    currentTextAreaLength("editbreakdown-form", "Recommand", 200, "editRecommand-currentlen");
});
myApp.onPageAfterBack("Add-schedule", function (page) {
    $$.ajax({
        url: "/QualityControl/Manager_ScheduleDetails",
        data: {
            date: $$("#calendar-inline-container").val() + "-01"
        },
        success: function (data) {
            $$(".list-month").html(data);
        }
    });
});
//添加产量计划页
myApp.onPageInit('Plan', function (page) {
    //创建picker
    var today = new Date();
    var currentYear = today.getFullYear();
    var currentMonth = (today.getMonth() + 1) < 10 ? "0" + (today.getMonth() + 1) : (today.getMonth() + 1);

    var pickerInline = myApp.picker({
        input: '#calendar-inline-container',
        formatValue: function (p, values, displayValues) {
            return values[0] + '-' + values[1];           //文本框显示格式
        },
        toolbarCloseText: "关闭",
        value: [currentYear, currentMonth],
        cols: time_col
    });
    $$.ajax({
        url: "/QualityControl/Manager_ScheduleDetails",
        data: {
            date: $$("#calendar-inline-container").val() + "-01"
        },
        success: function (data) {
            $$(".list-month").html(data);
        }
    });
    $$("#calendar-inline-container").on("change", function () {
        if ($$("#calendar-inline-container").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_ScheduleDetails",
                data: {
                    date: $$("#calendar-inline-container").val() + "-01"
                },
                success: function (data) {
                    $$(".list-month").html(data);
                }
            });
        }
    });
    $$(document).on("touchstart", ".production-details", function () {
        var date = $$("#calendar-inline-container").val() + "-01";
        var url = $$(this).attr("href") + "&date=" + date;
        $$(this).attr("href", url);
    });
});
myApp.onPageInit('Add-schedule', function (page) {
    //createCalendar("DateList",true,false)
    var today = new Date();
    var currentYear = today.getFullYear();
    var currentMonth = (today.getMonth() + 1) < 10 ? "0" + (today.getMonth() + 1) : (today.getMonth() + 1);
    var pickerInline = myApp.picker({
        input: '#DateList',
        formatValue: function (p, values, displayValues) {
            return values[0] + '-' + values[1];           //文本框显示格式
        },
        toolbarCloseText: "关闭",
        value: [currentYear, currentMonth],
        cols: time_col
    });
    $$("#DateList").on("change", function () {
        $$("#tempalate-content").html("");
        if ($("#ProductId").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_AddSchedulePartial",
                data: {
                    fid: $$("#FactoryId").val(),
                    pid: $$("#ProductId").val(),
                    date: $$("#DateList").val()
                },
                success: function (data) {
                    $$("#tempalate-content").html(data);
                },
                error: function (data) {
                    myApp.alert("请求失败。")
                }
            })
        }
    });
    $$("#FactoryId").on("change", function () {
        $$("#tempalate-content").html("");
        $("#ProductId").html("<option value=\"\">- 请选择 -</option>");
        $("#ProductId").parent().find(".smart-select-value").html("- 请选择 -")
        $("#ProductClassId").html("<option value=\"\">- 请选择 -</option>");
        $("#ProductClassId").parent().find(".smart-select-value").html("- 请选择 -")
        if ($$("#FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshQualityProductClassListAjax",
                type: "post",
                data: {
                    factoryId: $$("#FactoryId").val(),
                },
                success: function (data) {
                    $$("#ProductClassId").html("").append("<option value=\"\">- 请选择 -</option>")
                    var _data = JSON.parse(data);
                    var _len = _data.content.length;
                    if (_data.result == "SUCCESS") {
                        for (var i = 0; i < _len; i++) {
                            $$("#ProductClassId").append("<option value=\"" + _data.content[i].Id + "\">" + _data.content[i].Name + "</option>");
                        }
                    }
                }
            })
        }
    })
    $$("#ProductClassId").on("change", function () {
        $$("#tempalate-content").html("");
        $("#ProductId").html("<option value=\"\">- 请选择 -</option>");
        $("#ProductId").parent().find(".smart-select-value").html("- 请选择 -")
        if ($("#ProductClassId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshQualityTestProductListAjax",
                type: "post",
                data: {
                    factoryId: $$("#FactoryId").val(),
                    productClassId: $("#ProductClassId").val()
                },
                success: function (data) {
                    $$("#ProductId").html("").append("<option value=\"\">- 请选择 -</option>")
                    var _data = JSON.parse(data);
                    var _len = _data.content.length;
                    if (_data.result == "SUCCESS") {
                        for (var i = 0; i < _len; i++) {
                            $$("#ProductId").append("<option value=\"" + _data.content[i].Id + "\">" + _data.content[i].Name + "</option>");
                        }
                    }
                }
            })
        }
    })
    $$("#ProductId").on("change", function () {
        $$("#tempalate-content").html("");
        if ($$("#ProductId").val() != "") {
            $$.ajax({
                url: "/QualityControl/Manager_AddSchedulePartial",
                data: {
                    fid: $$("#FactoryId").val(),
                    pid: $$("#ProductId").val(),
                    date: $$("#DateList").val()
                },
                success: function (data) {
                    $$("#tempalate-content").html(data);
                },
                error: function (data) {
                    myApp.alert("请求失败。")
                }
            })
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
    $("#"+SubmitForm+" "+"input.required").each(function () {
        if ($(this).val() == "") {
            $(this).attr("placeholder", "请输入信息").addClass("invalid-input");
            pass = false;
        } else {
            $(this).removeClass("invalid-input");
        }
    });
    $("#"+SubmitForm+ " " + "input.isnumber").each(function () {
        if ($(this).val() != "") {
            if (!isPInt($(this).val())) {
                $(this).attr("placeholder", "请输入合法数字").addClass("invalid-input");
                pass = false;
            }
        } else {
            $(this).val("0");
        }
    });
    $("#" + SubmitForm + " " + "textarea.required").each(function () {
        if ($(this).val() == "") {
            $(this).attr("placeholder", "请输入备注信息").addClass("invalid-input");
            pass = false;
        }
    });
    $("#" + SubmitForm + " " + "textarea.maxlength_200").each(function () {
        if ($(this).length > 200) {
            $(this).attr("placeholder", "超过字数").addClass("invalid-input");
            pass = false;
        }
    });
    // 然后弹窗信息
    if (pass) {
        $("#" + SubmitForm + " " + "select.required").each(function () {
            if ($(this).val() == "- 请选择 -" || $(this).val() == null || $(this).val() == "") {
                myApp.alert($(this).next().find(".item-title").html() + " 未选择");
                pass = false;
                return false;
            }
        });
    }
    if (pass) {
        $("#" + SubmitForm + " " + ".photos").each(function () {
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
// 当前字数更新
function currentTextAreaLength(pagename, id_name, max_length, result_id) {
    var tl_c = $$("#" + id_name).val().length;
    $$("#" + result_id).text(tl_c);
    $$("#" + pagename).on("change", "#" + id_name, function () {
        var tl = $$("#" + id_name).val().length;
        if (tl < max_length) {
            $$("#" + result_id).text(tl);
        } else {
            myApp.alert("已超出最大值，请重新填写或删除部分信息");
            var str = $$("#" + id_name).val();
            $$("#" + id_name).val(str.slice(0, 50));
            $$("#" + result_id).text(max_length);
        }
    });
}
var time_col = [
    {
        values: (function () {
            var arr = [];
            for (var i = 2000 ; i <= 2030; i++) { arr.push(i); }
            return arr;
        })(),
    },
    {
        divider: true,
        content: '-'
    },
     {
         values: ('01 02 03 04 05 06 07 08 09 10 11 12').split(' '),
         textAlign: 'right'
     },
]
// 上传签到图片文件模块
function uploadCheckinFile(pagename, imglist, photolist_id, current_count, max_count) {
    $$("#" + imglist).html("");
    var photolist = splitArray($$("#" + photolist_id).val());
    $$("#" + current_count).text(photolist.length);
    for (var i = 0; i < photolist.length; i++) {
        $$("#" + imglist).append("<li><div class=\"rep-imgitem\" data-rel=\"" + photolist[i] + "\" style=\"background-image:url(/QualityControl/ThumbnailImage?filename=" + photolist[i] + "); background-size:cover\"></div></li>");
    }
    $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
    $$("#" + imglist).on("click", "#" + imglist + "-upload-btn", function (e) {
        var localIds;
        var photolist = splitArray($("#" + photolist_id).val());
        if (photolist.length < max_count) {
            wx.chooseImage({
                count: max_count - photolist.length,
                // 默认9
                sizeType: ["compressed"],
                // 可以指定是原图还是压缩图，默认二者都有
                sourceType: ["album", "camera"],
                // 可以指定来源是相册还是相机，默认二者都有
                success: function (res) {
                    localIds = res.localIds;
                    // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                    //$("#preview").attr("src", localIds);
                    upload_img(localIds, 0, photolist);
                }
            });
        } else {
            myApp.alert("上传图片不得大于" + max_count + "张，无法添加");
        }
    });
    function upload_img(localIds, arraycount, pl) {
        if (arraycount < localIds.length) {
            wx.uploadImage({
                localId: localIds[arraycount], // 需要上传的图片的本地ID，由chooseImage接口获得
                isShowProgressTips: 1, // 默认为1，显示进度提示
                success: function (res) {
                    var serverId = res.serverId; // 返回图片的服务器端ID
                    $$.ajax({
                        url: "/QualityControl/SaveOrignalImage",
                        type: "post",
                        data: {
                            serverId: serverId
                        },
                        success: function (data) {
                            data = JSON.parse(data);
                            if (data.result == "SUCCESS") {
                                pl.push(data.filename);
                                arraycount++;
                                upload_img(localIds, arraycount, pl);
                            }
                            else {
                                myApp.alert("上传失败");
                            }
                        }
                    });
                }
            });
        }
        else {
            $$("#" + imglist).html("");
            $$("#" + current_count).text(pl.length);
            $$("#" + photolist_id).val(pl.toString());
            for (var i = 0; i < pl.length; i++) {
                $$("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + pl[i] + "' style=\"background-image:url(/QualityControl/ThumbnailImage?filename=" + pl[i] + '); background-size:cover"></div></li>');
            }
            $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
            myApp.hideIndicator();
        }
    }
    // 删除图片
    //$$("#" + pagename).off("click", ".rep-imgitem");
    $$("#" + imglist).on("click", ".rep-imgitem", function (e) {
        var img_item = $$(this);
        $$(".rep-imgitem").each(function () {
            $$(this).html("");
        });
        img_item.html("<div class='rep-imgitem-selected'><i class='fa fa-minus'></i></div>");
    });
    $$("#" + imglist).on("click", ".rep-imgitem-selected", function () {
        myApp.confirm("是否确认删除已上传图片?", "提示", function () {
            var delete_item = $(".rep-imgitem-selected").closest(".rep-imgitem").attr("data-rel");
            var arraylist = splitArray($("#" + photolist_id).val());
            var pos = $.inArray(delete_item, arraylist);
            arraylist.splice(pos, 1);
            $$("#" + photolist_id).val(arraylist.toString());
            $$("#" + current_count).text(arraylist.length);
            $$("#" + imglist).html("");
            for (var i = 0; i < arraylist.length; i++) {
                $("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + arraylist[i] + "' style=\"background-image:url(/QualityControl/ThumbnailImage?filename=" + arraylist[i] + '); background-size:cover"></div></li>');
            }
            $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
        });
    });
}
