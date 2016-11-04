var myApp = new Framework7({
    modalTitle: 'App',
    pushState: true,
});
var $$ = Dom7;
var mainView = myApp.addView('.view-main', {
    dynamicNavbar: true,
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
    data: {},
    success: function (data) {
        $$("#userinfo").html(data);
    }
});
myApp.onPageInit('Home', function (page) {

})
//签到页
myApp.onPageInit('Signin', function (page) {
    var myPicker = myApp.picker({
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
    });
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
myApp.onPageInit('Troublereport', function (page) {
    console.log('故障报告');
    var tContent = $$('#troublereport_content');
    tContent.on('refresh', function () {
        setTimeout(function () {
            console.log('故障报告已刷新');
            myApp.pullToRefreshDone();
        }, 2000);
    })
})
//新增故障报个页
myApp.onPageInit('Newtroublereport', function (page) {
    console.log('新增故障报告页');
    $$('.btn').on('click', function () {
        myApp.confirm('确认提交?', function (value) {
            myApp.showPreloader('正在提交')
            setTimeout(function () {
                myApp.hidePreloader();
                $$('textarea').val('');
            }, 2000);
        });
    });

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






