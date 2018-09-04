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

            bool isDailySend = false;
            while (true)
            {
                blockMonitor bm_testnet = new blockMonitor(mongodbConnStr_testnet, mongodbDatabase_testnet);
                var isNewBlockInByDB = bm_testnet.isNewBlockInByDB();

                Aliyun_SMS aliSMS = new Aliyun_SMS(accessKeyId, accessKeySecret);
                string sendSMSResulet = string.Empty;

                string lastBlockIndex = (string)isNewBlockInByDB["lastBlockIndex"];
                string lastBlockTime = (string)isNewBlockInByDB["lastBlockTime"];

                if ((bool)isNewBlockInByDB["isNewBlockIn"])
                {
                    if (DateTime.Now.ToString("HHmm") == dailyTime)
                    {
                        if (!isDailySend) {
                            sendSMSResulet = aliSMS.SendSMS_blockNotIn(phoneNumbers, "测试网", "0分钟", lastBlockIndex, lastBlockTime);
                            Console.WriteLine("发送定时正常信息状态：" + sendSMSResulet + "|" + DateTime.Now.ToString("yyyy.MM.dd HH:mm"));

                            isDailySend = true;
                        }            
                    }
                    else { isDailySend = false; }
                }
                else
                {
                    sendSMSResulet = aliSMS.SendSMS_blockNotIn(phoneNumbers, "测试网", "5分钟", lastBlockIndex, lastBlockTime);
                    Console.WriteLine("》发送入库异常信息状态：" + sendSMSResulet + "|" + DateTime.Now.ToString("yyyy.MM.dd HH:mm"));
                }                 

                //Console.ReadKey();

                Thread.Sleep(15*1000);
            }
        }
    }
}
