using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;

using Aliyun.Acs.Dysmsapi.Model.V20170525;

using Microsoft.Extensions.Configuration;

using blcokStatusMonitor.lib;
using System.Threading;

namespace blcokStatusMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("监控开始");

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("keySetting.json");

            var configuration = builder.Build();

            String accessKeyId = configuration["accessKeyId"];//你的accessKeyId
            String accessKeySecret = configuration["accessKeySecret"];//你的accessKeySecret
            string phoneNumbers = configuration["phoneNumbers"];
            string dailyTime = configuration["dailyTime"];
            string mongodbConnStr_testnet = configuration["mongodbConnStr_testnet"];
            string mongodbDatabase_testnet = configuration["mongodbDatabase_testnet"];
            string mongodbConnStr_mainnet = configuration["mongodbConnStr_mainnet"];
            string mongodbDatabase_mainnet = configuration["mongodbDatabase_mainnet"];

            bool isDailySend_testnet = false;
            string blockNotInHour_testnet = string.Empty;
            bool isDailySend_mainnet = false;
            string blockNotInHour_mainnet = string.Empty;
            Aliyun_SMS aliSMS = new Aliyun_SMS(accessKeyId, accessKeySecret);
            while (true)
            {
                //监控测试网
                exeNotify("测试网",mongodbConnStr_testnet, mongodbDatabase_testnet, dailyTime,ref isDailySend_testnet,ref blockNotInHour_testnet, aliSMS, phoneNumbers);

                //监控主网
                exeNotify("主网", mongodbConnStr_mainnet, mongodbDatabase_mainnet, dailyTime, ref isDailySend_mainnet, ref blockNotInHour_mainnet, aliSMS, phoneNumbers);

                //Console.ReadKey();

                Thread.Sleep(15*1000);
            }


        }

        private static void exeNotify(string netType,string mongodbConnStr,string mongodbDatabase,string dailyTime,ref bool isDailySend,ref string blockNotInHour, Aliyun_SMS aliSMS,string phoneNumbers)
        {
            blockMonitor bm = new blockMonitor(mongodbConnStr, mongodbDatabase);
            var isNewBlockInByDB = bm.isNewBlockInByDB();

            string lastBlockIndex = (string)isNewBlockInByDB["lastBlockIndex"];
            string lastBlockTime = (string)isNewBlockInByDB["lastBlockTime"];
            if ((bool)isNewBlockInByDB["isNewBlockIn"])
            {
                if (DateTime.Now.ToString("HHmm") == dailyTime)
                {
                    if (!isDailySend)
                    {
                        var sendSMSResulet = aliSMS.SendSMS_blockNotIn(phoneNumbers, netType, "0分钟", lastBlockIndex, lastBlockTime);
                        Console.WriteLine("发送定时正常信息状态：" + sendSMSResulet + "|" + DateTime.Now.ToString("yyyy.MM.dd HH:mm"));

                        isDailySend = true;
                    }
                }
                else { isDailySend = false; }
            }
            else
            {
                //每次启动，每小时只发一次
                string notifyHour = DateTime.Now.ToString("HH");
                if (notifyHour != blockNotInHour)
                {
                    var sendSMSResulet = aliSMS.SendSMS_blockNotIn(phoneNumbers, netType, "5分钟", lastBlockIndex, lastBlockTime);
                    Console.WriteLine("》发送入库异常信息状态：" + sendSMSResulet + "|" + DateTime.Now.ToString("yyyy.MM.dd HH:mm"));

                    blockNotInHour = notifyHour;
                }

            }

        }
    }
}
