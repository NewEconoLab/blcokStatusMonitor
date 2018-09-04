using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;

using Aliyun.Acs.Dysmsapi.Model.V20170525;

using Microsoft.Extensions.Configuration;

using blcokStatusMonitor.lib;

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

            Aliyun_SMS aliSMS = new Aliyun_SMS(accessKeyId,accessKeySecret);
            string sendSMSResulet = aliSMS.SendSMS_blockNotIn(phoneNumbers, "测试网", "5分钟", "1773910", DateTime.Now.ToString("yyyyMMddHHmm"));

            Console.WriteLine(sendSMSResulet);

            Console.ReadKey();
        }
    }
}
