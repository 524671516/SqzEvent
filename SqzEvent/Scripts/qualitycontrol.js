//实例化framework7
var myApp = new Framework7({
    modalTitle: '生产管理',
    pushState: true,
    modalButtonOk: "确定",
    modalButtonCancel: "取消",
    cache:false
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
updateHomeInfo();
//首页刷新
var ptrContent = $$("#home-refresh");
ptrContent.on("refresh", function (e) {
    setTimeout(updateHomeInfo, 500);
});
/*==========
主页
=========*/
myApp.onPageInit('Home', function (page) {
    updateHomeInfo();
    var ptrContent = $$("#home-refresh");
    // Add 'refresh' listener on it
    ptrContent.on("refresh", function (e) {
        setTimeout(updateHomeInfo, 500);
    });
})
/*==========
签到页
=========*/
myApp.onPageInit('qccheckin', function (page) {
    var $factoryselect = $("#factory-select")       //工厂选择按钮
    var $qccheckinsubmit = $("#qccheckin-submit");  //表单提交按钮
    var $qccheckinform = $('#qccheckin-form');      //form表单
    //键盘事件
    $qccheckinform.on("keyup", ".required", function () {
        $(this).attr("placeholder", "").removeClass("invalid-input");
    });
    //效验规则
    $qccheckinform.validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
        },
        //错误样式
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("qccheckin-submit", "qccheckin-form")
        }
    });
    //按钮点击事件
    $qccheckinsubmit.on("click", function () {
        if (!$qccheckinsubmit.prop("disabled")) {
            $qccheckinsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $qccheckinform.submit();
            }, 500);
        }
    });
    //改变工厂触发事件
    $factoryselect.on("change", function () {
        if ($("#FactoryId").val() != "") {
            $("#input-content").removeClass("hidden");
            $$.ajax({
                url: "/QualityControl/QCCheckinPartial",
                data: {
                    factoryId: $$("#FactoryId").val()
                },
                success: function (data) {
                    $$("#template-content").html(data);                   
                    $$.ajax({
                        url: "/QualityControl/CheckCheckinAjax",
                        type: "post",
                        data: {
                            fid: $$("#FactoryId").val()
                        },
                        success: function (checkdata) {
                            var checkdata = JSON.parse(checkdata);
                            if (checkdata.result) {
                                myApp.alert("该工厂当天已有签到，重新签到将覆盖之前的记录");
                                // 填入数据
                                $$.ajax({
                                    url: "/QualityControl/CheckinContent",
                                    data: {
                                        cid: checkdata.agendaId
                                    },
                                    type: "post",
                                    success: function (contentdata) {
                                        var contentdata = JSON.parse(contentdata);
                                        if (contentdata.result == "SUCCESS") {                                           
                                            $("#Photos").val(contentdata.photos);
                                            $("#CheckinRemark").val(contentdata.remark);
                                            for (var i = 0; i < contentdata.template.length; i++) {
                                                $("#" + contentdata.template[i].key).val(contentdata.template[i].value);
                                            }                                           
                                            uploadCheckinFile("qccheckin-form", "qccheckin-photos", "Photos", "qccheckin-imgcount", 9);
                                            //textarea字数计算
                                            currentTextAreaLength("qccheckin-form", "CheckinRemark", 200, "qccheckin-currentlen");
                                        }
                                    }
                                });
                            }
                            else {
                                //图片上传数量计算
                                $("#Photos").val("");
                                $("#CheckinRemark").val("");
                                uploadCheckinFile("qccheckin-form", "qccheckin-photos", "Photos", "qccheckin-imgcount", 9);
                                //textarea字数计算
                                currentTextAreaLength("qccheckin-form", "CheckinRemark", 200, "qccheckin-currentlen");
                            }
                        }
                    });
                }
            });
        }
        else {
            $("#input-content").addClass("hidden");
        }
    });    
});
/*==========
签退页 
=========*/
myApp.onPageInit("addcheckout", function (page) {
    var $checkoutnoinfo = $("#checkout-noinfo");                    //无信息
    var $checkout = $("#checkout");                                 //keyup对象
    var $addqccheckoutsubmit = $("#addqccheckout-submit");          //提交按钮
    //AgendaId改变时触发
    $$("#AgendaId").on("change", function () {
        $$.ajax({
            url: "/QualityControl/AddQCCheckoutPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#addcheckout-content').html(data);
                currentTextAreaLength("addqccheckout-form", "CheckoutRemark", 200, "qccheckout-currentlen");
            }
        });
    });
    //键盘事件
    $checkout.on("keyup", ".required", function () {
            $(this).attr("placeholder", "").removeClass("invalid-input");
    });
    //判断AgendaId是否为空
    if ($$("#AgendaId").val() == "") {
        $addqccheckoutsubmit.prop("disabled", true).addClass("color-gray");
        $checkoutnoinfo.append("<div  class=\"content-block-title\">无可签退项</div>");
    } else {
        $$.ajax({
            url: "/QualityControl/AddQCCheckoutPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#addcheckout-content').html(data);
                currentTextAreaLength("addqccheckout-form", "CheckoutRemark", 200, "qccheckout-currentlen");
            }
        });
    }
    //按钮点击事件
    $addqccheckoutsubmit.on("click", function () {
        if (!$addqccheckoutsubmit.prop("disabled")) {
            $addqccheckoutsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                CheckError("addqccheckout-submit", "addqccheckout-form");
            }, 500)
        }
    });
});
/*==========
厂检报告页
=========*/
myApp.onPageInit("factorytestlist", function (page) {
    if ($$("#SelectFactoryId").val() != "") {
        $$.ajax({
            url: "/QualityControl/QualityFactoryTestPartial",
            data: {
                fid: $$("#SelectFactoryId").val()
            },
            success: function (data) {
                $$("#factorytest-list").html(data);
            }
        })
    }
    $$("#factorytestlist").on("change", "#SelectFactoryId", function () {
        if ($$("#SelectFactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/QualityFactoryTestPartial",
                data: {
                    fid: $$("#SelectFactoryId").val()
                },
                success: function (data) {
                    $$("#factorytest-list").html(data);
                }
            })
        } else {
            $$("#factorytest-list").html("");
        }
    })
    $$("#factorytestlist").on("deleted", ".swipeout", function (e) {
        $$.ajax({
            url: "/QualityControl/DeleteFactoryTest",
            data: {
                FtId: $$(e.target).find(".swipeout-delete").attr("data-url")
            },
            method: "post",
            success: function (data) {
                data = JSON.parse(data);
                if (data.result != "SUCCESS") {
                    myApp.alert("删除失败");
                }
            }
        });
    });
    PhotoBrowser("factorytestlist");
})
/*==========
故障报告列表
=========*/
myApp.onPageInit("breakdownlist", function (page) {
    //添加日历
    var calendarMultiple = myApp.calendar({
        input: "#breakdownlist-date",
        dateFormat: "yyyy-mm-dd",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    $$.ajax({
        url: "/QualityControl/BreakdownListPartial",
        data: {
            date: $$("#breakdownlist-date").val()
        },
        success: function (data) {
            $$("#breakdownlist-content").html(data);
        }
    });
    $$("#breakdownlist-date").on("change", function () {
        $$.ajax({
            url: "/QualityControl/BreakdownListPartial",
            data: {
                date: $$("#breakdownlist-date").val()
            },
            success: function (data) {
                $$("#breakdownlist-content").html(data);
            }
        });
    });
});
/*==========
定期检测页 
=========*/
myApp.onPageInit("qualityregulartest", function (page) {
    //添加日历
    var calendarMultiple = myApp.calendar({
        input: "#ApplyDate",
        dateFormat: "yyyy-mm-dd",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    $$("#qualityregulartest-submit").on("click", function () {
        if (!$$("#qualityregulartest-submit").prop("disabled")) {
            $$("#qualityregulartest-submit").prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                CheckError("qualityregulartest-submit", "qualityregulartest-form");
            }, 500)
        }
    })
    $$("#qualityregulartest").on("change", "#FactoryId", function () {
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
    $$("#qualityregulartest").on("change", "#ProductClassId", function () {
        $("#ProductId").html("<option value=\"\">- 请选择 -</option>");
        $("#ProductId").parent().find(".smart-select-value").html("- 请选择 -")
        if ($$("#ProductClassId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshQualityTestProductListAjax",
                type: "post",
                data: {
                    factoryId: $$("#FactoryId").val(),
                    productClassId: $$("#ProductClassId").val()
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
    //图片上传数量计算
    uploadCheckinFile("qualityregulartest-form", "qualityregulartest-photos", "Photo", "qualityregulartest-imgcount", 3);
})
/*==========
新增故障报告 
=========*/
myApp.onPageInit("addbreakdown", function (page) {
    //获取当前时间
    var _date = new Date();
    var hor = (_date.getHours() < 10) ? "0" + _date.getHours().toString() : _date.getHours().toString();
    var min = (_date.getMinutes() < 10) ? "0" + _date.getMinutes().toString() : _date.getMinutes().toString();
    var realmin = min[0] + (min[1] < 5 ? 0 : 5);
    var $equipmentselect = $("#equipment-select")          //设备选择
    var $factoryselect = $("#factory-select")              //工厂选择
    var $addbreakdownsubmit = $("#addbreakdown-submit");   //提交按钮
    var $addbreakdownform = $('#addbreakdown-form');       //form表单
    //创建picker
    var pickerInline = myApp.picker({
        input: '#BreakDownTimeTiny',
        formatValue: function (p, values, displayValues) {
            return values[0] + ':' + values[1];           //文本框显示格式
        },
        toolbarCloseText: "关闭",
        value: [hor, realmin],
        cols: time_col
    });
    $factoryselect.on("change", function () {
        $$("#QCEquipmentId").html("");
        $$("#QCEquipmentId").append("<option>- 请选择 -</option>");
        $$("#BreakDownTypeId").html("");
        $$("#BreakDownTypeId").append("<option>- 请选择 -</option>");
        $$("#QCEquipmentId").val("");
        $$("#BreakDownTypeId").val("");
        $$("#equipment-select .item-after").text("- 请选择 -");
        $$("#breakdowntype-select .item-after").text("- 请选择 -");
        if ($$("#FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshEquipmentListAjax",
                data: {
                    factoryId: $("#FactoryId").val()
                },
                type: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        for (var i = 0; i < data.content.length; i++) {
                            $$("#QCEquipmentId").append("<option value=\"" + data.content[i].Id + "\">" + data.content[i].Name + "</option>");
                        }
                        $("#QCEquipmentLi").removeClass("hidden");
                    }
                }
            });
        } else {
            $("#BreakDownTypeLi").addClass("hidden");
            $("#QCEquipmentLi").addClass("hidden");
        }
    });
    $equipmentselect.on("change", function () {
        $$("#BreakDownTypeId").html("");
        $$("#BreakDownTypeId").append("<option>- 请选择 -</option>");
        $$("#BreakDownTypeId").val("");
        $$("#breakdowntype-select .item-after").text("- 请选择 -");
        if ($$("#QCEquipmentId").val() != "" && $$("#QCEquipmentId").val() != "- 请选择 -") {
            $$.ajax({
                url: "/QualityControl/RefreshBreakdownTypeListAjax",
                data: {
                    equipmentId: $("#QCEquipmentId").val()
                },
                type: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        for (var i = 0; i < data.content.length; i++) {
                            $$("#BreakDownTypeId").append("<option value=\"" + data.content[i].Id + "\">" + data.content[i].Name + "</option>");
                        }
                        $("#BreakDownTypeLi").removeClass("hidden");
                    }
                }
            });
        } else {
            $("#BreakDownTypeLi").addClass("hidden");
        }
    });
    $addbreakdownform.validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
        },
        errorClass: "invalid-input",
        submitHandler: function (form) {
            CheckError("addbreakdown-submit", "addbreakdown-form")
        }
    });
    //按钮点击事件
    $addbreakdownsubmit.on("click", function () {
        if (!$addbreakdownsubmit.prop("disabled")) {
            $addbreakdownsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $addbreakdownform.submit();
                setTimeout(function () {
                    $.ajax({
                        aysnc: false,
                        url: "/QualityControl/BreakdownListPartial",
                        data: {
                            date: $("#breakdownlist-date").val()
                        },
                        success: function (data) {
                            $("#breakdownlist-content").html(data);
                        }
                    })}, 1500);
            }, 500);
        }
    });
    //图片上传数量计算
    uploadCheckinFile("addbreakdown-form", "addbreakdown-photos", "Photos", "addbreakdown-imgcount", 7);
    //textarea字数计算
    currentTextAreaLength("addbreakdown-form", "ReportContent", 200, "addbreakdown-currentlen");
});
/*==========
每日工作总结页
=========*/
myApp.onPageInit('dailysummary', function (page) {
    var $checkoutnoinfo = $("#checkout-noinfo");                       //无信息
    var $dailysummary = $("#dailysummary");                            //keyup对象
    var $dailysummarysubmit = $("#dailysummary-submit");               //提交按钮
    if ($$("#AgendaId").val() == "") {
        $dailysummarysubmit.prop("disabled", true).addClass("color-gray");
        $checkoutnoinfo.append("<div  class=\"content-block-title\">无内容</div>");
    } else {
        $$.ajax({
            url: "/QualityControl/QCDailySummaryPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#dailysummary-content').html(data);
                uploadCheckinFile("qcdailysummarypartial-form", "qcsummary-photos", "SummaryPhotos", "qcsummary-imgcount", 7);
                currentTextAreaLength("qcdailysummarypartial-form", "Remark", 200, "qccheckin-currentlen");
            }
        });
    }
    //键盘事件
    $dailysummary.on("keyup", ".required", function () {
        $(this).attr("placeholder", "").removeClass("invalid-input");
    });
    //按钮点击事件
    $dailysummarysubmit.on("click", function () {
        if (!$dailysummarysubmit.prop("disabled")) {
            $dailysummarysubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                CheckError("dailysummary-submit", "qcdailysummarypartial-form")
            }, 500)
        }
    });
    $$("#AgendaId").on("change", function () {
        $$.ajax({
            url: "/QualityControl/QCDailySummaryPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#dailysummary-content').html(data);
                uploadCheckinFile("qcdailysummarypartial-form", "qcsummary-photos", "SummaryPhotos", "qcsummary-imgcount", 7);
                currentTextAreaLength("qcdailysummarypartial-form", "Remark", 200, "qccheckin-currentlen");
            }
        });
    });
});
/*==========
确认修复页
=========*/
myApp.onPageInit('recoverybreakdown', function (page) {
    var $recoverybreakdownsubmit = $("#recoverybreakdown-submit");   //提交按钮
    var $recoveybreakdownform = $('#recoverybreakdown-form');        //form表单
    //创建picker
    var pickerInline = myApp.picker({
        input: '#RecoveryTimeTiny',
        formatValue: function (p, values, displayValues) {
            return values[0] + ':' + values[1];                     //文本框显示格式
        },
        toolbarCloseText: "关闭",
        value: ["10", "00"],
        cols: time_col
    });
    //效验规则
    $recoveybreakdownform.validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $recoverybreakdownsubmit.prop("disabled", false).removeClass("color-gray");
        },
        errorClass: "invalid-input",
        //提交成功后处理函数
        submitHandler: function (form) {
            /// 可以用checkerror函数代替
            $recoveybreakdownform.ajaxSubmit(function (data) {
                CheckError("recoverybreakdown-submit", "recoverybreakdown-form")
            });
        }
    });
    //按钮点击事件
    $recoverybreakdownsubmit.on("click", function () {
        if (!$recoverybreakdownsubmit.prop("disabled")) {
            myApp.showIndicator();
            $recoverybreakdownsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $recoveybreakdownform.submit();
                setTimeout(function () {
                    $.ajax({
                        aysnc: false,
                        url: "/QualityControl/BreakdownListPartial",
                        data: {
                            date: $("#breakdownlist-date").val()
                        },
                        success: function (data) {
                            $("#breakdownlist-content").html(data);
                        }
                    })
                }, 1500);
            }, 500);
        }
    });
    //textarea上传字数计算
    currentTextAreaLength("recoverybreakdown-form", "RecoveryRemark", 200, "recoverybreakdown-currentlen");
    //查看图片
    PhotoBrowser("recoverybreakdown");
});
/*==========
工作计划
=========*/
myApp.onPageInit("productionplan", function (page) {
    //添加日历
    //var calendarMultiple = myApp.calendar({
    //    input: "#productionplan-date",
    //    dateFormat: "yyyy-mm-dd",
    //    monthNames: monthNames,
    //    monthNamesShort: monthNamesShort,
    //    dayNames: dayNames,
    //    dayNamesShort: dayNamesShort,
    //    closeOnSelect: true
    //});
    var today = new Date();
    var currentYear = today.getFullYear();
    var currentMonth = (today.getMonth() + 1) < 10 ? "0" + (today.getMonth() + 1) : (today.getMonth() + 1);
    var pickerInline = myApp.picker({
        input: '#productionplan-date',
        formatValue: function (p, values, displayValues) {
            return values[0] + '-' + values[1];           //文本框显示格式
        },
        toolbarCloseText: "关闭",
        value: [currentYear, currentMonth],
        cols: time_col2
    });
    $$.ajax({
        url: "/QualityControl/ProductPlanPartial",
        data: {
            date: $$("#productionplan-date").val()
        },
        success: function (data) {
            $$("#productionplan-content").html(data);
        }
    });
    $$("#productionplan-date").on("change", function () {
        $$.ajax({
            url: "/QualityControl/ProductPlanPartial",
            data: {
                date: $$("#productionplan-date").val()
            },
            success: function (data) {
                $$("#productionplan-content").html(data);
            }
        });
    });
});
$$(document).on('pageAfterAnimation', '.page[data-page="qualitytestlist"]', function (e) {
    
    $$.ajax({
        url: "/QualityControl/QualityTestListPartial",
        data: {
            date: $$("#qualitytestlist-date").val()
        },
        success: function (data) {
            $$("#qualitytestlist-content").html(data);
        }
    });
})
/*==========
产品检测列表
=========*/
myApp.onPageInit("qualitytestlist", function (page) {
    
    //添加日历
    var calendarMultiple = myApp.calendar({
        input: "#qualitytestlist-date",
        dateFormat: "yyyy-mm-dd",
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        dayNames: dayNames,
        dayNamesShort: dayNamesShort,
        closeOnSelect: true
    });
    $$("#qualitytestlist-date").on("change", function () {
        $$.ajax({
            url: "/QualityControl/QualityTestListPartial",
            data: {
                date: $$("#qualitytestlist-date").val()
            },
            success: function (data) {
                $$("#qualitytestlist-content").html(data);
            }
        });
    });
    $$("#qualitytestlist-content").on("deleted", ".swipeout", function (e) {
        $$.ajax({
            url: "/QualityControl/DeleteQualityTest",
            data: {
                qtId: $$(e.target).attr("data-url")
            },
            method: "post",
            success: function (data) {
                data = JSON.parse(data);
                if (data.result != "SUCCESS") {
                    myApp.alert("删除失败");
                }
            }
        });
    });
});
/*==========
新增产品检验
=========*/
myApp.onPageInit("addqualitytest", function (page) {
    var $addqualitytestform = $("#addqualitytest-form");
    var $addqualitytestsubmit = $("#addqualitytest-submit");
    $addqualitytestform.validate({
        //调试模式取消submit的默认提交功能
        debug: false,
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $addqualitytestsubmit.prop("disabled", false).removeClass("color-gray");
        },
        errorClass: "invalid-input",
        submitHandler: function (form) {
            var nophotoarry = [];
            var pass = false;
            $(".one_photo").each(function () {
                if ($(this).val() == "") {
                    nophotoarry.push($(this).parents(".accordion-item").find(".item-title").html())
                } else {
                    pass = true;
                }
            });
            if (pass) {
                CheckError("addqualitytest-submit", "addqualitytest-form");
            } else {
                if (nophotoarry.length > 0) {
                    $addqualitytestsubmit.prop("disabled", false).removeClass("color-gray");
                    myApp.alert(nophotoarry[0] + "需要添加图片")
                } else {
                    CheckError("addqualitytest-submit", "addqualitytest-form");
                }
            }
        }
    });
    $$("#FactoryId").on("change", function () {
        $addqualitytestsubmit.prop("disabled", true).addClass("color-gray");
        $$("#list-type-template").html("");
        $$("#list-type-template").addClass("hidden");
        $("#productli").addClass("hidden")
        $("#productclassli").addClass("hidden")
        $("#productboxli").addClass("hidden")
        $("#ProductId").html("<option value=\"\" select>- 请选择 -</option>");
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
                        $("#productclassli").removeClass("hidden");
                    }
                }
            })
        }
    })
    $$("#ProductClassId").on("change", function () {
        $addqualitytestsubmit.prop("disabled", true).addClass("color-gray");
        $$("#list-type-template").html("");
        $$("#list-type-template").addClass("hidden");
        $("#productli").addClass("hidden")
        $("#ProductId").html("<option value=\"\" select>- 请选择 -</option>");
        $("#ProductId").parent().find(".smart-select-value").html("- 请选择 -")
        if ($("#ProductClassId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshQualityTestProductListAjax",
                type: "post",
                data: {
                    factoryId: $$("#FactoryId").val(),
                    productClassId: $("#ProductClassId").val(),
                },
                success: function (data) {
                    $$("#ProductId").html("").append("<option value=\"\">- 请选择 -</option>")
                    var _data = JSON.parse(data);
                    var _len = _data.content.length;
                    if (_data.result == "SUCCESS") {
                        for (var i = 0; i < _len; i++) {
                            $$("#ProductId").append("<option value=\"" + _data.content[i].Id + "\">" + _data.content[i].Name + "</option>");
                        }
                        $("#productli").removeClass("hidden");
                    }
                }
            })
        }
    })
    $$("#ProductId").on("change", function () {
        $addqualitytestsubmit.prop("disabled", true).addClass("color-gray");
        $$("#list-type-template").html("");
        $$("#list-type-template").addClass("hidden");
        if ($$("#ProductId").val() != "") {
            $$.ajax({
                url: "/QualityControl/AddQualityTestPartial",
                data: {
                    pid: $$("#ProductId").val(),
                },
                success: function (data) {
                    $$("#list-type-template").html(data);
                    $$("#list-type-template").removeClass("hidden");
                    $addqualitytestsubmit.prop("disabled", false).removeClass("color-gray");
                    var max_num = $(".one_photo").parent().parent().parent().parent().text();
                    if (max_num.substring(max_num.indexOf("/") + 1, max_num.indexOf("/") + 2) == 5) {
                        uploadCheckinFile("addqualitytest-form", "addqualitytest-photos", "Photos", "addqualitytest-imgcount", 5);
                    } else {
                        uploadCheckinFile("addqualitytest-form", "addqualitytest-photos", "Photos", "addqualitytest-imgcount", 12);
                    }
                    currentTextAreaLength("addqualitytest", "Remark", 200, "qctest-currentlen");
                    $(".one_photo").each(function () {
                        var pid = $(this).attr("Id");
                        var uid = $(this).parent().parent().find("ul").attr("Id");
                        var countid = $(this).parent().parent().parent().parent().find("abbr").attr("Id");
                        if (max_num.substring(max_num.indexOf("/") + 1, max_num.indexOf("/") + 2) == 5) {
                            uploadCheckinFile("addqualitytest-form", uid, pid, countid, 5);
                        } else {
                            uploadCheckinFile("addqualitytest-form", uid, pid, countid, 12);
                        }
                    });
                    $("#list-type-template .item-after").on("click", function () {
                        var _self = $(this).parent().parent().parent().parent();
                        myApp.confirm("是否确认删除此检测项", "提示", function () {
                            _self.remove();
                        });
                        return false;
                    })
                },
                error: function (data) {
                    myApp.alert("请求失败。")
                }
            })
        }
    })
    $addqualitytestform.on("click", ".scan-input", function () {
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

    //提交按钮事件
    $addqualitytestsubmit.on("click", function () {
        if (!$addqualitytestsubmit.hasClass("color-gray")) {
            $addqualitytestsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $addqualitytestform.submit();
            }, 500);
        }
    });
});
/*故障详情页*/
myApp.onPageInit("breakdowndetails", function (page) {
    PhotoBrowser("breakdowndetails");
})
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
        uploadCheckinFile("addqualitytest-form", uid, pid, countid, 5);
    })
    PhotoBrowser("qualitytestdetails");
    $("#edit-qt").on("click", function () {
        if (!$(this).hasClass("color-gray")) {
           $(this).prop("disabled", true).addClass("color-gray");
           var nophotoarry = [];
           var pass = false;
           $(".one_photo").each(function () {
               if ($(this).val() == "") {
                   nophotoarry.push($(this).parents(".accordion-item").find(".item-title").html())
               } else {
                   pass = true;
               }
           });
           if (pass) {
               $("#edit-qt-form").ajaxSubmit(function (data) {
                   if (data.result == "SUCCESS") {
                       mainView.router.back();
                       myApp.addNotification({
                           title: "通知",
                           message: "表单修改成功"
                       });
                       setTimeout(function () {
                           myApp.closeNotification(".notifications");
                       }, 2e3);
                   } else {
                       mainView.router.back();
                       myApp.addNotification({
                           title: "通知",
                           message: "表单修改失败"
                       });
                       $("#edit-qt").prop("disabled", false).removeClass("color-gray");
                       setTimeout(function () {
                           myApp.closeNotification(".notifications");
                       }, 2e3);
                   }
               });
           } else {
               if (nophotoarry.length > 0) {
                   $("#edit-qt").prop("disabled", false).removeClass("color-gray");
                   myApp.alert(nophotoarry[0] + "需要添加图片")
               } else {
                   $("#edit-qt-form").ajaxSubmit(function (data) {
                       if (data.result == "SUCCESS") {
                           mainView.router.back();
                           myApp.addNotification({
                               title: "通知",
                               message: "表单修改成功"
                           });
                           setTimeout(function () {
                               myApp.closeNotification(".notifications");
                           }, 2e3);
                       } else {
                           mainView.router.back();
                           myApp.addNotification({
                               title: "通知",
                               message: "表单修改失败"
                           });
                           $("#edit-qt").prop("disabled", false).removeClass("color-gray");
                           setTimeout(function () {
                               myApp.closeNotification(".notifications");
                           }, 2e3);
                       }
                   });
               }
           }
        }
    })
})

// 首页更新
function updateHomeInfo() {
    $$.ajax({
        url: "/QualityControl/HomeInfoAjax",
        type: "post",
        success: function (data) {
            var data = JSON.parse(data);
            if (data.result == "SUCCESS") {
                if (data.qt_dot)
                    $$("#qt_dot").removeClass("hidden");
                else
                    $$("#qt_dot").addClass("hidden");
                if (data.bd_dot)
                    $$("#bd_dot").removeClass("hidden");
                else
                    $$("#bd_dot").addClass("hidden");
                $$("#bd_count").text(data.bd_count);
                $$("#qt_count").text(data.qt_count);
                if (data.checkout_cnt != 0)
                    $$("#checkout_dot").removeClass("hidden");
                else
                    $$("#checkout_dot").addClass("hidden");
                if (data.summary_cnt != 0)
                    $$("#summary_dot").removeClass("hidden");
                else
                    $$("#summary_dot").addClass("hidden");
                $$("#checkout_cnt").text(data.checkout_cnt);
                $$("#summary_cnt").text(data.summary_cnt);
                $$("#datecode").text(data.datecode);
                myApp.pullToRefreshDone();
            }
        }
    });
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
//上传图片信息
function uploadImage(img_id, img_filename) {
    $$("#" + img_id).on("click", function () {
        myApp.showIndicator();
        setTimeout(function () {
            if (!img_success) {
                myApp.hideIndicator();
                myApp.alert("上传图片失败");
            }
        }, 2000);
        var localIds
        wx.chooseImage({
            count: 1, // 默认9
            sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
            sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
            success: function (res) {
                localIds = res.localIds; // 返回选定照片的本地ID列表，localId可以作为img标签的src属性显示图片
                wx.uploadImage({
                    localId: localIds[0], // 需要上传的图片的本地ID，由chooseImage接口获得
                    isShowProgressTips: 1, // 默认为1，显示进度提示
                    success: function (res) {
                        var serverId = res.serverId; // 返回图片的服务器端ID
                        var img_success = false;
                        $.ajax({
                            url: "/QualityControl/SaveOrignalImage",
                            type: "post",
                            data: {
                                serverId: serverId
                            },
                            success: function (data) {
                                if (data.result == "SUCCESS") {
                                    img_success = true;
                                    $$("#" + img_id).find(".item-title").children("i").remove();
                                    $$("#" + img_id).find(".item-title").prepend("<i class='fa fa-check-circle color-green' aria-hidden='true'></i>");
                                    $$("#" + img_id).find(".item-after").text("图片上传成功");
                                    $$("#" + img_filename).val(data.filename);
                                    myApp.hideIndicator();
                                }
                                else {
                                    myApp.alert("上传失败，请重试");
                                }
                            }
                        });
                    }
                });
            },
            cancel: function () {
                myApp.hideIndicator();
            }
        });
    });
}
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
var time_col2 = [
    {
        values: (function () {
            var arr = [];
            for (var i = 1999; i <= 2030; i++) { arr.push(i); }
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
var time_col = [
    {
        values: (function () {
            var arr = [];
            for (var i = 0; i <= 23; i++) { arr.push(i < 10 ? '0' + i : i); }
            return arr;
        })(),
    },
    {
        divider: true,
        content: ':'
    },
    {
        values: (function () {
            var arr = [];
            for (var i = 0; i <= 59; i = i + 5) {
                arr.push(i < 10 ? '0' + i : i);
            }
            return arr;
        })(),
    }
]
var monthNames = ["一月份", "二月份", "三月份", "四月份", "五月份", "六月份", "七月份", "八月份", "九月份", "十月份", "十一月份", "十二月份"];

var monthNamesShort = ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"];

var dayNames = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

var dayNames = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];

var dayNamesShort = ["日", "一", "二", "三", "四", "五", "六"];
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
    $("textarea.maxlength_200").each(function(){
        if ($(this).length > 200) {
            $(this).attr("placeholder", "超过字数").addClass("invalid-input");
            pass = false;
        }
    });
    // 然后弹窗信息
    if (pass) {
        $("select.required").each(function () {
            if ($(this).val() == "- 请选择 -" || $(this).val() == null || $(this).val() =="") {
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
                else if (data == "MODIFIED" || data == "FAIL") {
                    if(data == "MODIFIED"){
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单修改成功"
                        });
                    } else {
                        myApp.hideIndicator();
                        mainView.router.back();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                    }   
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

