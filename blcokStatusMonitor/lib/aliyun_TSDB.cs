using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace blcokStatusMonitor.lib
{
    public class Aliyun_TSDB
    {
        public string TSDB_URL = string.Empty;
        httpHelper http = new httpHelper();

        public Aliyun_TSDB(string URL) {
            TSDB_URL = URL;
        }

        public class TSDB_Data {
            public string Metric { get; set; }
            public long Timestamp { get; set; }
            public object Value { get; set; }
            public JObject Tags { get; set; }

            public TSDB_Data(string metric, DateTime time, object value, JObject tags)
            {
                Metric = metric;

                Timestamp = getTimeStamp(time);

                Value = value;
                Tags = tags;
            }

            public TSDB_Data(string metric, long timeStamp, object value, JObject tags)
            {
                Metric = metric;

                Timestamp = timeStamp;

                Value = value;
                Tags = tags;
            }
        }

        public JObject TSDB_put_single(TSDB_Data tSDB_Data)
        {
            var inputParam = "[" + JsonConvert.SerializeObject(tSDB_Data) + "]";
            return JObject.Parse(http.Post(TSDB_URL + "/api/put?summary", inputParam, Encoding.UTF8, 1));
        }

        public JArray TSDB_query(DateTime startTime,DateTime endTime,string aggregator, string metric, JObject tags,string downsample)
        {
            var inputParamJ = JObject.Parse(@"
            {
                'start': [0],
                'end': [1],
                'queries': [
                    {
                        'aggregator': '[2]',
                        'metric': '[3]',
                        'tags': [4],
                        'downsample':'[5]'
                    }
                ]
            }");
            inputParamJ["start"] = getTimeStamp(startTime);
            inputParamJ["end"] = getTimeStamp(endTime);
            inputParamJ["queries"][0]["aggregator"] = aggregator;
            inputParamJ["queries"][0]["metric"] = metric;
            inputParamJ["queries"][0]["tags"] = tags;
            inputParamJ["queries"][0]["downsample"] = downsample;

            //var inputParam = @"
            //{
            //    'start': [0],
            //    'end': [1],
            //    'queries': [
            //        {
            //            'aggregator': 'count',
            //            'metric': '[2]',
            //            'tags': [3],
            //            'downsample':'[4]'
            //        }
            //    ]
            //}".Replace("[0]",getTimeStamp(startTime).ToString()).Replace("[1]",getTimeStamp(endTime).ToString()).Replace("[2]",metric).Replace("[3]",JsonConvert.SerializeObject(tags)).Replace("[4]",downsample).Replace("'","\"");

            return JArray.Parse(http.Post(TSDB_URL + "/api/query", inputParamJ.ToString(), Encoding.UTF8, 1));
        }

        static private long getTimeStamp(DateTime dateTime)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 8, 0, 0), TimeZoneInfo.Local); // 当地时区
            return (long)(dateTime - startTime).TotalMilliseconds;
        }
    }
}
