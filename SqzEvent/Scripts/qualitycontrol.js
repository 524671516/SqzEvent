var myApp = new Framework7({
    modalTitle: 'App',
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
    jsApiList: ["uploadImage", "downloadImage", "chooseImage", "getLocation", "previewImage", "openLocation"]
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
//主页
myApp.onPageInit('Home', function (page) {
})
//签到页
myApp.onPageInit('qccheckin', function (page) {
    $(function () {
        var $factoryselect = $("#factory-select")
        var $qccheckinsubmit = $("#qccheckin-submit");
        var $qccheckinform = $('#qccheckin-form');
        $(".item-content").keyup(function () {
            $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
        })
        //数字转换
        $("#OfficalWorkers").keyup(function () {
            var a = Number($("#OfficalWorkers").val())
            if (a > 0 || a == 0) {
                $("#OfficalWorkers").val(a);
            }
        })
        $("#TemporaryWorkers").keyup(function () {
            var b = Number($("#TemporaryWorkers").val())
            if (b > 0 || b == 0) {
                $("#TemporaryWorkers").val(b);
            }
        })
        //效验规则
        $qccheckinform.validate({
            rules: {
                FactoryId: {
                    required: true,
                },
                Remark: {
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
                //console.log(element);
                //element.parent().addClass("error");
            },
            errorClass: "invalid-input",
            //提交成功后处理函数
            submitHandler: function (form) {
                var photoList = splitArray($("#Photos").val());
                if (photoList.length == 0) {
                    myApp.hideIndicator();
                    myApp.alert("至少上传一张照片");
                    $qccheckinsubmit.prop("disabled", false).removeClass("color-gray");
                }else {
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
                        else {
                            myApp.hideIndicator();
                            myApp.addNotification({
                                title: "通知",
                                message: "表单提交失败"
                            });
                            clicked = false;
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
                }, 2000);
            }
        }
        )

    })
    //图片上传数量计算
    uploadCheckinFile("qccheckin-form", "qccheckin-photos", "Photos", "qccheckin-imgcount", 7);
    //textarea字数计算
    currentTextAreaLength("qccheckin-form", "Remark", 200, "qccheckin-currentlen");
})
//产品检验页
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
//新增产品页
myApp.onPageInit('Newproductinspection', function (page) {
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

})
//故障报告页
myApp.onPageInit('addbreakdown', function (page) {
    //创建picker
    var pickerInline = myApp.picker({
        input: '#BreakDownTime',
        //选择时执行函数
        onChange: function (picker, values, displayValues) {
            /*var daysInMonth = new Date(picker.value[0], picker.value[1] * 1 + 1, 0).getDate();//计算本月总天数
            var currentYear = new Date().getFullYear();
            var currentMonth = new Date().getMonth();
            picker.cols[1].setValue(currentMonth);
            picker.cols[0].setValue(currentYear);
            //判断日数是否超出范围超出设置本月最大天数
            if (values[2] > daysInMonth) {
                picker.cols[2].setValue(daysInMonth);
            }*/
        },
        //文本框显示格式
        formatValue: function (p, values, displayValues) {
            return  values[0] + ':' + values[1];
        },
        //选择框顶部模板
        toolbarTemplate:
        '<div class="toolbar">' +
            '<div class="toolbar-inner">' +
                '<div class="left">' +
                    '<a href="#" class="link toolbar-randomize-link"></a>' +
                '</div>' +
                '<div class="right">' +
                    '<a href="#" class="link close-picker">完成</a>' +
                '</div>' +
            '</div>' +
        '</div>',
        cols: [
             /*// 年
            {
                values: (function () {
                    var arr = [];
                    for (var i = 1999; i <= 2080; i++) { arr.push(i); }
                    return arr;
                })(),
            },
            // 月
            {
                values: ('0 1 2 3 4 5 6 7 8 9 10 11').split(' '),
                displayValues: ('1 2 3 4 5 6 7 8 9 10 11 12').split(' '),
                textAlign: 'left'
            },
            // 日
            {
                values: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31],
            },
            // 分隔符
            {
                divider: true,
                content: ''
            },*/
            // 小时
            {
                values: (function () {
                    var arr = [];
                    for (var i = 0; i <= 23; i++) { arr.push(i); }
                    return arr;
                })(),
            },
            // 分隔符
            {
                divider: true,
                content: ':'
            },
            // 分钟
            {
                values: (function () {
                    var arr = [];
                    for (var i = 0; i <= 59; i = i + 5) { arr.push(i < 10 ? '0' + i : i); }
                    return arr;
                })(),
            }
        ]
    });
    $(function () {
        var $factoryselect = $("#factory-select")
        var $addbreakdownsubmit = $("#addbreakdown-submit");
        var $addbreakdownform = $('#addbreakdown-form');
        $(".item-content").keyup(function () {
            $addbreakdownsubmit.prop("disabled", false).removeClass("color-gray");
        })
        //效验规则
        $addbreakdownform.validate({
            BreakDownTime:{
                required: true,
                date: true
            },
            rules: {
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
                //console.log(element);
                //element.parent().addClass("error");
            },
            errorClass: "invalid-input",
            //提交成功后处理函数
            submitHandler: function (form) {
                var photoList = splitArray($("#Photos").val());
                if (photoList.length == 0) {
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
                            clicked = false;
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
                }, 2000);
            }
        }
        )

    })


})
//新增故障报个页
myApp.onPageInit('addbreakdown', function (page) {
    /*console.log('新增故障报告页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
                $$('textarea').val('');
            }, 2000);
        });
    });*/
    uploadCheckinFile("addbreakdown-form", "addbreakdown-photos", "Photos", "addbreakdown-imgcount", 7);
    currentTextAreaLength("addbreakdown-form", "ReportContent", 200, "addbreakdown-currentlen");

})
//确认修复页
myApp.onPageInit('Confirmrepair', function (page) {
    console.log('故障修复页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
                $$('textarea').val('');
            }, 2000);
        });
    });
});

//每日工作总结页
myApp.onPageInit('Dailyworksummary', function (page) {
    console.log('每日工作总结页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
            }, 2000);
        });
    });

})

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
                            url: "/Seller/SaveOrignalImage",
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
        $$("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + photolist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + photolist[i] + '); background-size:cover"></div></li>');
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
                        url: "/Seller/SaveOrignalImage",
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
                $$("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + pl[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + pl[i] + '); background-size:cover"></div></li>');
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
                $("#" + imglist).append('<li><div class="rep-imgitem" data-rel=\'' + arraylist[i] + "' style=\"background-image:url(/Seller/ThumbnailImage?filename=" + arraylist[i] + '); background-size:cover"></div></li>');
            }
            $$("#" + imglist).append('<li><a href="javascript:;" class="rep-imgitem-btn" id="' + imglist + '-upload-btn"><i class="fa fa-plus"></i></a></li>');
        });
    });
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






