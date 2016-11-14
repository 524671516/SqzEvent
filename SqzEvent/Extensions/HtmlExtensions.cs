﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqzEvent.Models;

namespace System.Web.Mvc
{
    public static class HtmlExtensions
    {
        public static string ExpensesPaymentType(this HtmlHelper helper, int type)
        {
            switch (type)
            {
                case 0:
                    return "进场费";
                case 1:
                    return "活动费";
                default:
                    return "未知费用";
            }
        }
        public static string ExpensesStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "未审核";
                case 1:
                    return "已审核";
                case 2:
                    return "已结算";
                case 3:
                    return "已核销";
                default:
                    return "位置状态";
            }
        }
        public static string ExpensesStatusSpan(this HtmlHelper helper, int status)
        {
            //string value = "";
            switch (status)
            {
                case -1:
                    return "<span class='text-danger'>已作废</span>";
                case 0:
                    return "<span>未审核</span>";
                case 1:
                    return "<span>已审核</span>";
                case 2:
                    return "<span>已结算</span>";
                case 3:
                    return "<span class='text-success'>已核销</span>";
                default:
                    return "<span class='text-danger'>未知状态</span>";
            }
        }
        public static string CheckinStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "无数据";
                case 1:
                    return "已签到";
                case 2:
                    return "已签退";
                case 3:
                    return "已提报";
                case 4:
                    return "已确认";
                case 5:
                    return "已结算";
                default:
                    return "位置状态";
            }
        }
        public static string BonusStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "待审核";
                case 1:
                    return "已发送";
                case 2:
                    return "已收款";
                case 3:
                    return "发送失败";
                case 4:
                    return "已退款";
                default:
                    return "未知";
            }
        }
        public static string ManagerRequestStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "已提交";
                case 1:
                    return "已审核";
                case 2:
                    return "已完成";
                case 3:
                    return "已驳回";
                default:
                    return "未知";
            }
        }
        public static string AttendanceStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case 0:
                    return "全勤";
                case 1:
                    return "迟到";
                case 2:
                    return "早退";
                case 3:
                    return "旷工";
                default:
                    return "未确认";
            }
        }
        public static string ManagerTaskStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "已提交";
                case 1:
                    return "已确认";
                default:
                    return "未知";
            }
        }
        public static string CompetitionInfoStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "已提交";
                case 1:
                    return "提报通过";
                case 2:
                    return "已收款";
                case 3:
                    return "发送失败";
                case 4:
                    return "已退款";
                default:
                    return "未知";
            }
        }
        public static string BreakdownStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "未恢复";
                case 1:
                    return "已恢复";
                default:
                    return "未知";
            }
        }

        public static string ManagerNickName(this HtmlHelper helper, string username, int systemid)
        {
            if (username == null)
                return "";
            OfflineSales offlineDB = new OfflineSales();
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == username && m.Off_System_Id == systemid);
            if (item != null)
                return item.NickName;
            else
                return username;
        }
        
        public static string ManagerNickNameCollection(this HtmlHelper helper, string usernames, int systemid)
        {
            string[] names = usernames.Split(',');
            List<string> nicknames = new List<string>();
            OfflineSales offlineDB = new OfflineSales();
            foreach(var item in names)
            {
                if (item == null)
                    nicknames.Add("");
                var singlename = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == item && m.Off_System_Id == systemid);
                if (item != null)
                    nicknames.Add(singlename.NickName);
                else
                    nicknames.Add("");
            }
            return string.Join(",", nicknames);
        }
    }
}