using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace blcokStatusMonitor.lib
{
    public class blockMonitor
    {
        string mongodbConnStr;
        string mongodbDatabase;

        public blockMonitor(string ImongodbConnStr, string ImongodbDatabase)
        {
            mongodbConnStr = ImongodbConnStr;
            mongodbDatabase = ImongodbDatabase;
        }

        public JObject isNewBlockInByDB()
        {
            mongodbHelper mh = new mongodbHelper();
            JArray JA = mh.GetData(mongodbConnStr, mongodbDatabase, "block", "{}", "{index:-1}",2);

            long timeDiff = 0;
            var lastBlockTime = (long)JA[0]["time"];
            var secondLastBlockTime = (long)JA[1]["time"];
            timeDiff = lastBlockTime - secondLastBlockTime;

            //5分钟没有新块
            bool isNewBlockIn = (timeDiff > 300) ? false : true;

            JObject Jresult = new JObject();
            Jresult.Add("isNewBlockIn", isNewBlockIn);
            Jresult.Add("lastBlockIndex", (long)JA[0]["index"]);
            Jresult.Add("lastBlockTime", timeStampToDateStr(lastBlockTime));

            return Jresult;
        }

        string timeStampToDateStr(long unixTimeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimeStamp);

            return dt.ToString("yyyyMMddHHmmss");
        }
    }
}
