using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Toto
{
    public class IdiotsKupon
    {
        public short[] Cells;
        public int Count;
    }

    public class IdiotKuponsProvider
    {
        private int _drawingId;

        public IdiotKuponsProvider(int drawingId)
        {
            _drawingId = drawingId;
        }

        public IdiotsKupon[] GetKupons()
        {
            var data = GetData();
//            var data = GetDataFromFile();
            var json = JsonConvert.DeserializeObject<dynamic>(data);
            var res = new IdiotsKupon[json.d.Items.Count];

            var i = 0;
            foreach (var item in json.d.Items)
            {
                res[i] = new IdiotsKupon
                {
                    Cells = GetCellFromString(item.Code.Value),
                    Count = (int)item.Count.Value
                };
                i++;
            }

            return res;
        }

        short[] GetCellFromString(string code)
        {
            var res = new short[3];
            for (int i = 0; i < 15; i++)
            {
                var index = -1;
                switch (code[i])
                {
                    case '1':
                        index = 0;
                        break;
                    case 'X':
                        index = 1;
                        break;
                    case '2':
                        index = 2;
                        break;
                }
                res[index] |= (short)(1 << i);
            }
            return res;
        }

        string GetDataFromFile()
        {
            return File.ReadAllText(@"D:\000\betres.txt");
        }

        string GetData()
        {
            var req = WebRequest.CreateHttp(@"http://toto-info.bkfonbet.com/DataService.svc/GetStakeDict");
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write("{\"options\":{\"StartFrom\":0,\"Count\":200,\"DrawingId\":" + _drawingId + "}}");
            }

            var resp = req.GetResponse();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var res = sr.ReadToEnd();
                return res;
            }
        }
    }
}
