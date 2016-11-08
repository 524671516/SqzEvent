var myApp = new Framework7({
    modalTitle: 'App',
    pushState: true,
});
var $$ = Dom7;
var mainView = myApp.addView('.view-main', {
    dynamicNavbar: true,
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
//主页
var sContent = $$('#signin_content');
sContent.on('refresh', function () {
    setTimeout(function () {
        console.log(1);
        var num = [1, 2, 3, 4, 56];
        var num1 = num[Math.floor(Math.random() * 5)];
        sContent.find('#chenck-num').html(num1);
        myApp.pullToRefreshDone();
    }, 2000);
});
$$.ajax({
    url: "/QualityControl/UserInfoPartial",
    type: "post",
    data: {},
    success: function (data) {
        $$("#userinfo").html(data);
    }
});
myApp.onPageInit('Home', function (page) {

})
//签到页
myApp.onPageInit('qccheckin', function (page) {
    /*var myPicker = myApp.picker({
        input: '#picker-device',
        cols: [
    {
        values: ['Apple', 'Orange', 'Bananna'],
        textAlign: 'center'
    }
        ]
    });
    console.log('签到页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
                $$('input').val('');
            }, 2000);
        });
    });*/
    $(function () {
        var $submitlink = $("#submit-link");
        var $qccheckinform = $('#qccheckin-form');
        $qccheckinform.validate({
            rules: {
                FactoryId: {
                    required: true
                },
                Remark:{
                    required: true,
                    maxlength: 200
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
            errorPlacement: function (error, element) {
                //console.log(element);
                //element.parent().addClass("error");
            },
            errorClass: "invalid-input",
            submitHandler: function (form) {
                form.submit();
                alert("Submitted!")
            }

        });
        $submitlink.on("click", function () {
            
                $qccheckinform.submit();
        })

    })

    uploadCheckinFile("qccheckin-form", "qccheckin-photos", "Photos", "qccheckin-imgcount", 7);
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
    /*console.log('故障报告');
    var tContent = $$('#troublereport_content');
    tContent.on('refresh', function () {
        setTimeout(function () {
            console.log('故障报告已刷新');
            myApp.pullToRefreshDone();
        }, 2000);
    })*/
    
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




