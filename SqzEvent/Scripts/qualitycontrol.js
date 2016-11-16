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
    data: {},
    success: function (data) {
        $$("#userinfo").html(data);
    }
});
/*==========
主页
=========*/
myApp.onPageInit('Home', function (page) {
})
/*==========
签到页
=========*/
myApp.onPageInit('qccheckin', function (page) {
    var $factoryselect = $("#factory-select")
    var $qccheckinsubmit = $("#qccheckin-submit");
    var $qccheckinform = $('#qccheckin-form');
    $("input[type='number']").val("");
    //效验规则
    $qccheckinform.validate({
        debug: false,
        //调试模式取消submit的默认提交功能
        focusInvalid: false,
        //当为false时，验证无效时，没有焦点响应
        onkeyup: false,
        rules: {
            CheckinRemark: {
                maxlength: 200,
                required: true
            },
            OfficalWorkers: {
                min: 0,
                digits: true,
                max: 100,
                required: true
            },
            TemporaryWorkers: {
                min: 0,
                digits: true,
                max: 100,
                required: true
            }
        },
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
        },
        errorClass: "invalid-input",
        //提交成功后处理函数
        submitHandler: function (form) {
            var photoList = splitArray($("#Photos").val());
            if ($("#FactoryId").val() == "") {
                myApp.hideIndicator();
                myApp.alert("请选择签到工厂");
                $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
            }
            else if (photoList.length == 0) {
                myApp.hideIndicator();
                myApp.alert("至少上传一张照片");
                $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
            }
            else {
                $qccheckinform.ajaxSubmit(function (data) {
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
                        $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        }
    });
    //按钮点击事件
    $qccheckinsubmit.on("click", function () {
        if (!$qccheckinsubmit.prop("disabled")) {
            myApp.showIndicator();
            $qccheckinsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $qccheckinform.submit();
            }, 500);
        }
    });
    //图片上传数量计算
    $("#Photos").val("1.jpg,2.jpg,3.jpg");
    uploadCheckinFile("qccheckin-form", "qccheckin-photos", "Photos", "qccheckin-imgcount", 7);
    //textarea字数计算
    currentTextAreaLength("qccheckin-form", "CheckinRemark", 200, "qccheckin-currentlen");
});
/*==========
签退页
=========*/
myApp.onPageInit("addcheckout", function (page) {
    $$("#AgendaId").on("change", function () {
        $$.ajax({
            url: "/QualityControl/AddQCCheckoutPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#addcheckout-content').html(data);
                currentTextAreaLength("addqccheckout-form", "CheckoutRemark", 200, "qccheckout-currentlen");
                $$("#CheckoutRemark").on("keyup", function () {
                    if ($$(this).val().length != 0) {
                        $$(this).removeClass("invalid-input");
                    }
                });
            }
        });
    });
    //判断数据是否为空
    if ($$("#AgendaId").val()=="") {
        $$("#addqccheckout-submit").prop("disabled", true).addClass("color-gray");
        $$("#checkout-noinfo").append("<div  class=\"content-block-title\">无可签退项</div>");
    } else {
        $$.ajax({
            url: "/QualityControl/AddQCCheckoutPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#addcheckout-content').html(data);
                currentTextAreaLength("addqccheckout-form", "CheckoutRemark", 200, "qccheckout-currentlen");
                $$("#CheckoutRemark").on("keyup", function () {
                    if ($$(this).val().length != 0) {
                        $$(this).removeClass("invalid-input");
                    }
                });
            }
        });
    }
    //按钮点击事件
    $$("#addqccheckout-submit").on("click", function () {
            if (!$$("#addqccheckout-submit").prop("disabled")) {
                myApp.showIndicator();
                $$("#addqccheckout-submit").prop("disabled", true).addClass("color-gray");
                setTimeout(function () {
                    var remark = $("#CheckoutRemark").val();
                    if (remark == "") {
                        $$("#CheckoutRemark").attr("placeholder", "请输入备注信息").addClass("invalid-input");
                        myApp.hideIndicator();
                        $$("#addqccheckout-submit").prop("disabled", false).removeClass("color-gray");
                    }
                    else if (remark.length > 200) {
                        $$("#CheckoutRemark").attr("placeholder", "超过字数").addClass("invalid-input");
                        myApp.hideIndicator();
                        $$("#addqccheckout-submit").prop("disabled", false).removeClass("color-gray");
                    }
                    else {
                        $("#addqccheckout-form").ajaxSubmit(function (data) {
                            if (data == "SUCCESS") {
                                myApp.hideIndicator();
                                mainView.router.back();
                                myApp.addNotification({
                                    title: "通知",
                                    message: "签退成功"
                                });
                                setTimeout(function () {
                                    myApp.closeNotification(".notifications");
                                }, 2e3);
                            }
                            else {
                                myApp.hideIndicator();
                                myApp.addNotification({
                                    title: "通知",
                                    message: "签退失败"
                                });
                                $$("#addqccheckout-submit").prop("disabled", false).removeClass("color-gray");
                                setTimeout(function () {
                                    myApp.closeNotification(".notifications");
                                }, 2e3);
                            }
                        });
                    }
                }, 500);
            }
        });
});
/*==========
产品检验页
=========*/
myApp.onPageInit('Productinspection', function (page) {
    console.log('产品检验');
    var pContent = $$('#productinspection_content');
    pContent.on('refresh', function () {
        setTimeout(function () {
            console.log('产品检验已刷新');
            myApp.pullToRefreshDone();
        }, 2000);
    })

})
/*==========
新增产品页
=========*/
myApp.onPageInit("Newproductinspection", function (page) {
    console.log('新增产品页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
                $$('input').attr('checked', 'checked');
            }, 2000);
        });
    });
});
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
新增故障报告
=========*/
myApp.onPageInit("addbreakdown", function (page) {
    //创建picker
    var pickerInline = myApp.picker({
        input: '#BreakDownTimeTiny',
        //文本框显示格式
        formatValue: function (p, values, displayValues) {
            return values[0] + ':' + values[1];
        },
        toolbarCloseText: "关闭",
        value: ["10", "00"],
        cols: time_col
    });
    var $factoryselect = $("#factory-select")
    var $addbreakdownsubmit = $("#addbreakdown-submit");
    var $addbreakdownform = $('#addbreakdown-form');
    $$("#factory-select").on("change", function () {
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
                    }
                }
            });
        }
    });
    $$("#equipment-select").on("change", function () {
        $$("#BreakDownTypeId").html("");
        $$("#BreakDownTypeId").append("<option>- 请选择 -</option>");
        $$("#BreakDownTypeId").val("");
        $$("#breakdowntype-select .item-after").text("- 请选择 -");
        if ($$("#QCEquipmentId").val() != "") {
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
                    }
                }
            });
        }
    });
    //效验规则
    $addbreakdownform.validate({
        rules: {
            BreakDownTime: {
                required: true,
                date: true
            },
            FactoryId: {
                required: true,
            },
            ReportContent: {
                maxlength: 200,
                required: true
            }
        },
        //错误处理
        errorPlacement: function (error, element) {
            myApp.hideIndicator();
            $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
        },
        errorClass: "invalid-input",
        //提交成功后处理函数
        submitHandler: function (form) {
            var photoList = splitArray($("#Photos").val());
            if ($$("#FactoryId").val() == "" || $$("#EquipmentId").val() == "" || $$("#BreakDownTypeId").val() == "") {
                myApp.hideIndicator();
                myApp.alert("请选择故障类型");
                $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
            } else if (photoList.length == 0) {
                myApp.hideIndicator();
                myApp.alert("至少上传一张照片");
                $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
            } else {
                $addbreakdownform.ajaxSubmit(function (data) {
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
                    else {
                        myApp.hideIndicator();
                        myApp.addNotification({
                            title: "通知",
                            message: "表单提交失败"
                        });
                        $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
                        setTimeout(function () {
                            myApp.closeNotification(".notifications");
                        }, 2e3);
                    }
                });
            }
        }
    });
    //按钮点击事件
    $addbreakdownsubmit.on("click", function () {
        if (!$addbreakdownsubmit.prop("disabled")) {
            myApp.showIndicator();
            $addbreakdownsubmit.prop("disabled", true).addClass("color-gray");
            setTimeout(function () {
                $addbreakdownform.submit();
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
    if ($("#AgendaId").val()==null) {
        $("#dailysummary-submit").prop("disabled", true).addClass("color-gray");
        $("#checkout-noinfo").append("<div  class=\"content-block-title\">无可选项</div>");
    } else {
        $$.ajax({
            url: "/QualityControl/QCDailySummaryPartial",
            data: {
                agendaId: $$("#AgendaId").val()
            },
            success: function (data) {
                $$('#dailysummary-content').html(data);
            }
        });
    }
    $("#dailysummary").on("keyup", ".keyup-input", function () {
        if (isPInt($(this).val()) == true) {
            $(this).attr("placeholder", "").removeClass("invalid-input");
        } else {
            $(this).attr("placeholder", "请输入合法数字").addClass("invalid-input");
        }
    });
    $("#dailysummary").on("keyup", "#Remark", function () {
        if (($(this).val()) == "") {
            $(this).attr("placeholder", "请输入备注信息").addClass("invalid-input");
        } else {
            $(this).attr("placeholder", "").removeClass("invalid-input");
        }
    });
    var $dailysummarysubmit = $("#dailysummary-submit");
    var $qcdailysummarypartialform = $('#qcdailysummarypartial-form');
    //按钮点击事件
    $dailysummarysubmit.on("click", function () {
        $(".keyup-input").each(function () {
            if (isPInt($(this).val()) == false) {
                $(this).attr("placeholder", "请输入合法数字").addClass("invalid-input");
            } else {
                if (!$dailysummarysubmit.prop("disabled")) {
                    $dailysummarysubmit.prop("disabled", true).addClass("color-gray");
                        var remark = $("#Remark").val();
                        if (remark == "") {
                            $$("#Remark").attr("placeholder", "请输入备注信息").addClass("invalid-input");
                            $dailysummarysubmit.prop("disabled", false).removeClass("color-gray");
                        }
                        else if (remark.length > 200) {
                            $$("#Remark").attr("placeholder", "超过字数").addClass("invalid-input");
                            $dailysummarysubmit.prop("disabled", false).removeClass("color-gray");
                        }
                        else if ($$(".invalid-input").length > 0) {
                            $dailysummarysubmit.prop("disabled", false).removeClass("color-gray");
                        }
                        else {
                            myApp.showIndicator();
                            setTimeout(function () {
                                $qcdailysummarypartialform.ajaxSubmit(function (data) {
                                    if (data == "SUCCESS") {
                                        myApp.hideIndicator();
                                        mainView.router.back();
                                        myApp.addNotification({
                                            title: "通知",
                                            message: "签退成功"
                                        });
                                        setTimeout(function () {
                                            myApp.closeNotification(".notifications");
                                        }, 2e3);
                                    }
                                    else {
                                        myApp.hideIndicator();
                                        myApp.addNotification({
                                            title: "通知",
                                            message: "签退失败"
                                        });
                                        $dailysummarysubmit.prop("disabled", false).removeClass("color-gray");
                                        setTimeout(function () {
                                            myApp.closeNotification(".notifications");
                                        }, 2e3);
                                    }
                                });
                            }, 500)
                        }
                }
            }
        })
    });
});
/*==========
确认修复页
=========*/
myApp.onPageInit('recoverybreakdown', function (page) {
    //创建picker
    var pickerInline = myApp.picker({
        input: '#RecoveryTimeTiny',
        //文本框显示格式
        formatValue: function (p, values, displayValues) {
            return values[0] + ':' + values[1];
        },
        toolbarCloseText: "关闭",
        value: ["10", "00"],
        cols: time_col
    });
        var $recoverybreakdownsubmit = $("#recoverybreakdown-submit");
        var $recoveybreakdownform = $('#recoverybreakdown-form');
        //效验规则
        $recoveybreakdownform.validate({
            rules: {
                RecoveryRemark: {
                    maxlength: 200,
                    required: true
                }
            },
            //错误处理
            errorPlacement: function (error, element) {
                myApp.hideIndicator();
                $recoverybreakdownsubmit.prop("disabled", false).removeClass("color-gray");
            },
            errorClass: "invalid-input",
            //提交成功后处理函数
            submitHandler: function (form) {
                var photoList = splitArray($("#Photos").val());
                if (photoList.length == 0) {
                    myApp.hideIndicator();
                    myApp.alert("至少上传一张照片");
                    $recoverybreakdownsubmit.prop("disabled", false).removeClass("color-gray");

                } else {
                    $recoveybreakdownform.ajaxSubmit(function (data) {
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
                        else {
                            myApp.hideIndicator();
                            myApp.addNotification({
                                title: "通知",
                                message: "表单提交失败"
                            });
                            $recoverybreakdownsubmit.prop("disabled", false).removeClass("color-gray");
                            setTimeout(function () {
                                myApp.closeNotification(".notifications");
                            }, 2e3);
                        }
                    });
                }
            }
        });
        //按钮点击事件
        $recoverybreakdownsubmit.on("click", function () {
            if (!$recoverybreakdownsubmit.prop("disabled")) {
                myApp.showIndicator();
                $recoverybreakdownsubmit.prop("disabled", true).addClass("color-gray");
                setTimeout(function () {
                    $recoveybreakdownform.submit();
                }, 500);
            }
        });
        uploadCheckinFile("recoverybreakdown-form", "recoverybreakdown-photos", "Photos", "recoverybreakdown-imgcount", 7);
        currentTextAreaLength("recoverybreakdown-form", "RecoveryRemark", 200, "recoverybreakdown-currentlen");
});
/*==========
显示故障明细
=========*/
myApp.onPageInit("breakdowndetails", function (page) {
    PhotoBrowser("breakdowndetails")
});

//每日工作总结页
myApp.onPageInit("dailysummary", function (page) {
    $$.ajax({
        url: "/QualityControl/QCDailySummaryPartial",
        data: {
            agendaId: $$("#AgendaId").val()
        },
        success: function (data) {
            $$('#dailysummary-content').html(data);
        }
    });
});


// 质检列表
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
    $$.ajax({
        url: "/QualityControl/QualityTestListPartial",
        data: {
            date: $$("#qualitytestlist-date").val()
        },
        success: function (data) {
            $$("#qualitytestlist-content").html(data);
        }
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
            url: "Manager_CancelRequestJson",
            data: {
                id: $$(e.target).attr("data-url")
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

// 新增产品检验
myApp.onPageInit("addqualitytest", function (page) {
    uploadCheckinFile("addqualitytest-form", "addqualitytest-photos", "Photos", "addqualitytest-imgcount", 7);
    currentTextAreaLength("addqualitytest-form", "Remark", 200, "addqualitytest-currentlen");
    $$("#factory-select").on("change", function () {
        $$("#ProductId").html("");
        $$("#ProductId").append("<option>- 请选择 -</option>");
        $$("#product-select .item-after").text("- 请选择 -");
        if ($$("#FactoryId").val() != "") {
            $$.ajax({
                url: "/QualityControl/RefreshQualityTestProductListAjax",
                data: {
                    factoryId: $("#FactoryId").val()
                },
                type: "post",
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result == "SUCCESS") {
                        for (var i = 0; i < data.content.length; i++) {
                            $$("#ProductId").append("<option value=\"" + data.content[i].Id + "\">" + data.content[i].Name + "</option>");
                        }
                    }
                }
            });
        }
    });
    $$("#product-select").on("change", function () {
        if ($$("#ProductId").val() != "") {
            $$.ajax({
                url: "/QualityControl/AddQualityTestPartial",
                data: {
                    pid: $$("#ProductId").val()
                },
                success: function (data) {
                    $$("#template-content").html(data);
                }
            });
        }
    });
    $$("#addqualitytest-form").on("click", ".scan-input", function () {
        var $input = $$(this);
        wx.scanQRCode({
            needResult: 1, // 默认为0，扫描结果由微信处理，1则直接返回扫描结果，
            scanType: ["barCode"], // 可以指定扫二维码还是一维码，默认二者都有
            success: function (res) {
                var result = res.resultStr; // 当needResult 为 1 时，扫码返回的结果
                $input.val(result);
            }
        });
    });
    $$("#addqualitytest-submit").on("click", function () {
        $$(this).prop("disabled", true).addClass("color-gray");
        setTimeout(function () {
            $("#addqualitytest-form").ajaxSubmit(function (data) {
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
                else {
                    myApp.hideIndicator();
                    myApp.addNotification({
                        title: "通知",
                        message: "表单提交失败"
                    });
                    $recoverybreakdownsubmit.prop("disabled", false).removeClass("color-gray");
                    setTimeout(function () {
                        myApp.closeNotification(".notifications");
                    }, 2e3);
                }
            });
        }, 500);
    });
});

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
                zoom: 400,
                photos: images,
                theme: 'dark',
                type:'popup',
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
            //myApp.alert('You clicked Ok button');
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
    var g = /^(?:0|[0-9][0-9]?|100)$/;
    return g.test(str);
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
            $$("#" + result_id).text("500");
        }
    });
}
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
