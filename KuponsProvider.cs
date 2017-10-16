using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Toto
{
    class KuponsProvider
    {
        public static double[][][] GetKefs(string url, bool isFon)
        {
            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            var page = wc.DownloadString(url);
            if (isFon)
            {
                return ParseFon(page);
            }
            return ParseBolt(page);
        }

        private static double[][][] ParseFon(string page)
        {
            var rx = new Regex("<div id=\"drawing_json_data\">(.+)</div>");
            var jstr = rx.Match(page).Groups[1].Value;

            var json = JObject.Parse(jstr);
            var coll = json.SelectToken("Details.Events").AsEnumerable();

            var i = 0;
            var res = new double[15][][];

            foreach (var item in coll)
            {
                res[i] = new double[3][];

                res[i][0] = new double[2];
                res[i][0][0] = item["UserWin1"]["Probability"].Value<double>() / 100.0;
                res[i][0][1] = item["UserWin1"]["Percentage"].Value<double>() / 100.0;

                res[i][1] = new double[2];
                res[i][1][0] = item["UserDraw"]["Probability"].Value<double>() / 100.0;
                res[i][1][1] = item["UserDraw"]["Percentage"].Value<double>() / 100.0;

                res[i][2] = new double[2];
                res[i][2][0] = item["UserWin2"]["Probability"].Value<double>() / 100.0;
                res[i][2][1] = item["UserWin2"]["Percentage"].Value<double>() / 100.0;

                i++;
            }

            return res;
        }

        private static double[][][] ParseBolt(string page)
        {
            var res = new double[15][][];
            var rx = new Regex(@"\>(?<narod>\d\d)%/(?<buk>\d\d)%\s\<");

            var nfi = CultureInfo.CurrentCulture.NumberFormat.Clone() as NumberFormatInfo;
            nfi.NumberDecimalSeparator = ".";

            var i = 0;
            foreach (Match match in rx.Matches(page))
            {
                if (i % 3 == 0)
                {
                    res[i / 3] = new double[3][];
                }
                res[i / 3][i % 3] = new double[2];
                res[i / 3][i % 3][0] = double.Parse(match.Groups["buk"].Value, nfi) / 100.0;
                res[i / 3][i % 3][1] = double.Parse(match.Groups["narod"].Value, nfi) / 100.0;

                i++;
            }
            return res;
        }
    }
}
