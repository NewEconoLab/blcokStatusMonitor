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

using Newtonsoft.Json.Linq;
using System.Linq;

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
            string TSDB_URL = configuration["TSDB_URL"];

            bool isDailySend_testnet = false;
            string blockNotInHour_testnet = string.Empty;
            bool isDailySend_mainnet = false;
            string blockNotInHour_mainnet = string.Empty;
            Aliyun_SMS aliSMS = new Aliyun_SMS(accessKeyId, accessKeySecret);
            Aliyun_TSDB aliyun_TSDB = new Aliyun_TSDB(TSDB_URL);


            while (true)
            {
                //block时间信息入库
                insertBlockTime2TSDB(mongodbConnStr_testnet, mongodbDatabase_testnet, aliyun_TSDB,"testnet");

                //监控测试网
                exeNotify("测试网",mongodbConnStr_testnet, mongodbDatabase_testnet, dailyTime,ref isDailySend_testnet,ref blockNotInHour_testnet, aliSMS, phoneNumbers);

                //监控主网
                exeNotify("主网", mongodbConnStr_mainnet, mongodbDatabase_mainnet, dailyTime, ref isDailySend_mainnet, ref blockNotInHour_mainnet, aliSMS, phoneNumbers);

                //Console.ReadKey();

                Thread.Sleep(500);
            }


        }

        private static void insertBlockTime2TSDB(string mongodbConnStr, string mongodbDatabase, Aliyun_TSDB aliyun_TSDB,string netType) {
            long maxBloclHeight = -1;
            try
            {
                var maxBlock = aliyun_TSDB.TSDB_query(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "max", "neo.block.height.done", JObject.Parse("{'net': '" + netType + "'}"), "2d-max");
                maxBloclHeight = (long)decimal.Parse(maxBlock[0]["dps"].First.ToString().Split(":")[1]);
            }
            catch { }

            mongodbHelper mh = new mongodbHelper();
            JArray JA = mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{}");
            long maxBlockHeight_DB = long.Parse(JA.Children().Where(n => n["counter"].ToString() == "block").Select(n => (string)n["lastBlockindex"]).ToArray()[0]);

            long doBlockHeight = maxBloclHeight + 1;
            if (doBlockHeight <= maxBlockHeight_DB)
            {
                JArray blockJA = mh.GetData(mongodbConnStr, mongodbDatabase, "block", "{index:" + doBlockHeight + "}");
                var lastBlockTime = (long)blockJA[0]["time"];
                try
                {
                    var a = aliyun_TSDB.TSDB_put_single(new Aliyun_TSDB.TSDB_Data("neo.block.height", lastBlockTime, doBlockHeight, JObject.Parse("{'net': '" + netType + "'}")));
                    var b = aliyun_TSDB.TSDB_put_single(new Aliyun_TSDB.TSDB_Data("neo.block.height.done", DateTime.Now, doBlockHeight, JObject.Parse("{'net': '" + netType + "'}")));
                    Console.WriteLine("写入高度：" + doBlockHeight + "块时间到TSDB");
                } 
                catch {
                    Console.WriteLine("写入TSDB失败，重试！");
                }
                    
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

            Console.WriteLine(netType + "检查无异常");
        }
    }
}
