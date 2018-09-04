using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;

using Aliyun.Acs.Dysmsapi.Model.V20170525;

namespace blcokStatusMonitor.lib
{
    public class Aliyun_SMS
    {
        string accessKeyId;
        string accessKeySecret;

        public Aliyun_SMS(string IaccessKeyId, string IaccessKeySecret)
        {
            accessKeyId = IaccessKeyId;
            accessKeySecret = IaccessKeySecret;
        }

        public string SendSMS_blockNotIn(string PhoneNumbers, string netType, string timeOut, string blockIndex, string blockTime) {
            String product = "Dysmsapi";//短信API产品名称
            String domain = "dysmsapi.aliyuncs.com";//短信API产品域名
            //String accessKeyId = configuration["accessKeyId"];//你的accessKeyId
            //String accessKeySecret = configuration["accessKeySecret"];//你的accessKeySecret

            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            //IAcsClient client = new DefaultAcsClient(profile);
            // SingleSendSmsRequest request = new SingleSendSmsRequest();

            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendSmsRequest request = new SendSmsRequest();
            try
            {
                //必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为20个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumbers = PhoneNumbers;
                //必填:短信签名-可在短信控制台中找到
                request.SignName = "上海旷昊";
                //必填:短信模板-可在短信控制台中找到
                request.TemplateCode = "SMS_143714309";
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                string temp = "{{\"netType\":\"{0}\",\"timeOut\":\"{1}\",\"blockIndex\":\"{2}\",\"blockTime\":\"{3}\"}}";
                request.TemplateParam = string.Format(temp, netType, timeOut, blockIndex, blockTime);
                //可选:outId为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者
                request.OutId = "161616161616";
                //请求失败这里会抛ClientException异常
                SendSmsResponse sendSmsResponse = acsClient.GetAcsResponse(request);

                return sendSmsResponse.Message;


            }
            catch (ServerException e)
            {
                return "ServerException:" + e.Message;
            }
            catch (ClientException e)
            {
                return "ClientException:" + e.Message;
            }
        }
    }
}
