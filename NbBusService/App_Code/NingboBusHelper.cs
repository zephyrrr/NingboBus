using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NbBusService
{
    public class NingboBusHelper : Feng.Singleton<NingboBusHelper>
    {
        private const string serverAddr = "http://211.140.14.22:8480/wxgj";

        public NingboBusHelper()
        {
            webProxy.Encoding = System.Text.Encoding.UTF8;
        }
        Feng.Net.WebProxy webProxy = new Feng.Net.WebProxy();

        private static string DecodeText(string s)
        {
            return s.Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("&rarr;", "=>").Trim(); 
        }
        #region "Data Generator"
        private void InsertBusLine(string name, string link)
        {
            string sCmd = "INSERT BusLines (Name, Link) VALUES (@Name, @Link)";
            var cmd = Feng.Data.DbHelper.Instance.CreateCommand();
            cmd.CommandText = sCmd;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.Add(Feng.Data.DbHelper.Instance.CreateParameter("@Name", name));
            cmd.Parameters.Add(Feng.Data.DbHelper.Instance.CreateParameter("@Link", link));
            Feng.Data.DbHelper.Instance.ExecuteNonQuery(cmd);
        }
        private List<Tuple<int, string>> m_listBusLine = new List<Tuple<int, string>>();
        private void TryLoadBusLineData()
        {
            if (m_listBusLine.Count == 0)
            {
                var dt = Feng.Data.DbHelper.Instance.ExecuteDataTable("SELECT Id, Name, Link FROM BusLines");
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    m_listBusLine.Add(new Tuple<int, string>((int)row["Id"], row["Link"].ToString()));
                }
            }
        }
        private void InsertBusStations(string name, int busLineId, string link)
        {
            string sCmd = "INSERT BusStations (Name, Link, BusLine) VALUES (@Name, @Link, @BusLine)";
            var cmd = Feng.Data.DbHelper.Instance.CreateCommand();
            cmd.CommandText = sCmd;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.Add(Feng.Data.DbHelper.Instance.CreateParameter("@Name", name));
            cmd.Parameters.Add(Feng.Data.DbHelper.Instance.CreateParameter("@Link", link));
            cmd.Parameters.Add(Feng.Data.DbHelper.Instance.CreateParameter("@BusLine", busLineId));
            Feng.Data.DbHelper.Instance.ExecuteNonQuery(cmd);
        }

        public void ResetDb()
        {
            try
            {
                Feng.Data.DbHelper.Instance.ExecuteNonQuery("DROP TABLE BusStations");
            }
            catch (Exception)
            {
            }
            try
            {
                Feng.Data.DbHelper.Instance.ExecuteNonQuery("DROP TABLE BusLines");
            }
            catch (Exception)
            {
            }

            //dbHelper.ExecuteNonQuery("DELETE FROM BusLine");
            //dbHelper.ExecuteNonQuery("DBCC CHECKIDENT('BusLine', RESEED, 0)");
            Feng.Data.DbHelper.Instance.ExecuteNonQuery(@"CREATE TABLE BusLines
(
Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
Name nvarchar(255) NOT NULL,
Link nvarchar(255) NOT NULL
)");
            Feng.Data.DbHelper.Instance.ExecuteNonQuery(@"CREATE TABLE BusStations
(
Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
Name nvarchar(255) NOT NULL,
Link nvarchar(255) NOT NULL,
BusLine int
)");
            Feng.Data.DbHelper.Instance.ExecuteNonQuery(@"ALTER TABLE [BusStations] ADD CONSTRAINT [FK_Station_Line]
FOREIGN KEY ([BusLine]) REFERENCES [BusLines]([Id])");
        }

        public void GenerateBusStationData()
        {
            var dbHelper = Feng.Data.DbHelper.Instance;

            TryLoadBusLineData();
            try
            {
                int webCnt = 0;
                
                foreach (var busLine in m_listBusLine)
                {
                    var s = webProxy.GetToString(string.Format("{0}/{1}", serverAddr, busLine.Item2));
                    HtmlAgilityPack.HtmlDocument htmlBusStation = new HtmlAgilityPack.HtmlDocument();
                    htmlBusStation.LoadHtml(s);
                    List<Tuple<string, string>> listBusStations = new List<Tuple<string, string>>();
                    int stationId = 3;
                    while (true)
                    {
                        var node = htmlBusStation.DocumentNode.SelectSingleNode(string.Format("/html[1]/body[1]/div[{0}]/a[1]", stationId));
                        if (node == null || node.InnerText == "返回公交首页")
                            break;
                        var stationName = DecodeText(node.InnerText);
                        var stationLink = node.Attributes[0].Value;
                        listBusStations.Add(new Tuple<string, string>(stationName, stationLink));

                        var busLineId = busLine.Item1;
                        Console.WriteLine(string.Format("{1}", busLineId, stationName, stationLink));
                        InsertBusStations(stationName, busLineId, stationLink);
                        stationId++;
                    }

                    webCnt++;
                    if (webCnt % 50 == 0)
                        System.Threading.Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void GenerateBusLineData()
        {
            var dbHelper = Feng.Data.DbHelper.Instance;

            try
            {
                int webCnt = 0;
                List<Tuple<string, string>> listBus = new List<Tuple<string, string>>();
                int busId = 1;
                while (true)
                {
                    var s = webProxy.PostToString(string.Format("{0}/selectBusAllBusAction.action", serverAddr),
                        string.Format("busBean.lineName={0}%E8%B7%AF&btn_search2.x=33&btn_search2.y=10", busId));
                    HtmlAgilityPack.HtmlDocument htmlBusLine = new HtmlAgilityPack.HtmlDocument();
                    htmlBusLine.LoadHtml(s);

                    int busLineId = 2;
                    while (true)
                    {
                        var node = htmlBusLine.DocumentNode.SelectSingleNode(
                            string.Format("/html[1]/body[1]/div[{0}]/a[1]", busLineId));
                        if (node == null || node.InnerText == "返回公交首页")
                            break;

                        var busLineName = DecodeText(node.InnerText);
                        var busLineLink = node.Attributes[0].Value;
                        listBus.Add(new Tuple<string, string>(busLineName, busLineLink));

                        Console.WriteLine("{1}", busId, busLineName, busLineLink);
                        InsertBusLine(busLineName, busLineLink);

                        busLineId++;
                    }

                    busId++;
                    if (busId >= 1000)
                        break;

                    webCnt++;
                    if (webCnt % 50 == 0)
                        System.Threading.Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        public static string ConvertBusLocationsToString(int busLineId, Tuple<List<string>, List<string>> tuple)
        {
            StringBuilder sb = new StringBuilder();
            var stations = m_cacheBusLineStations[busLineId];

            StringBuilder sb2 = new StringBuilder();
            foreach (var kvp in stations)
            {
                if (tuple.Item1.Contains(kvp.Item1))
                {
                    sb2.Append("＞");
                }
                else if (tuple.Item2.Contains(kvp.Item1))
                {
                    sb2.Append("＝");
                }
                else
                {
                    sb2.Append("　");
                }
            }
            sb2.Append("\n");
            sb.Append(sb2.ToString());

            int idx = 0;
            while (true)
            {
                sb2.Clear();
                bool hasData = false;
                foreach (var kvp in stations)
                {
                    if (idx < kvp.Item1.Length)
                    {
                        sb2.Append(kvp.Item1[idx]);
                        hasData = true;
                    }
                    else
                    {
                        sb2.Append("　");
                    }
                }
                sb2.Append("\n");
                idx++;
                if (!hasData)
                    break;
                sb.Append(sb2.ToString());
            }
            //Dictionary<string, string> removeSame = new Dictionary<string, string>();
            //if (tuple.Item1.Count > 0)
            //{
            //    sb.Append("正开往");
            //    foreach (var i in tuple.Item1)
            //    {
            //        if (removeSame.ContainsKey(i))
            //            continue;
            //        removeSame[i] = i;
            //        sb.Append(i);
            //        sb.Append(",");
            //    }
            //}
            //if (tuple.Item2.Count > 0)
            //{
            //    removeSame.Clear();
            //    sb.Append("当前停靠");
            //    foreach (var i in tuple.Item2)
            //    {
            //        if (removeSame.ContainsKey(i))
            //            continue;
            //        removeSame[i] = i;
            //        sb.Append(i);
            //        sb.Append(",");
            //    }
            //}
            return sb.ToString();
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<int, List<Tuple<string, string>>> m_cacheBusLineStations = new System.Collections.Concurrent.ConcurrentDictionary<int, List<Tuple<string, string>>>();
        public Tuple<List<string>, List<string>> GetBusLocations(int busLineId)
        {
            List<Tuple<string, string>> stations;
            lock (m_cacheBusLineStations)
            {
                if (!m_cacheBusLineStations.ContainsKey(busLineId))
                {
                    //var dt = Feng.Data.DbHelper.Instance.ExecuteDataTable("SELECT A.[Name], A.Link FROM BusStations A " + 
                    //    "INNER JOIN BusLines B ON A.BusLine = B.Id AND B.[Name] = '" + busLineName + "'");
                    var dt = Feng.Data.DbHelper.Instance.ExecuteDataTable("SELECT A.[Name], A.Link FROM BusStations A " +
                        "INNER JOIN BusLines B ON A.BusLine = B.Id AND B.[ID] = '" + busLineId + "'");
                    stations = new List<Tuple<string, string>>();
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        stations.Add(new Tuple<string, string>(row["Name"].ToString(), row["Link"].ToString()));
                    }
                    m_cacheBusLineStations[busLineId] = stations;
                }
                else
                {
                    stations = m_cacheBusLineStations[busLineId];
                }
            }

            Tuple<List<string>, List<string>> ret = new Tuple<List<string>,List<string>>(new List<string>(), new List<string>());
            int i = 8;
            if (i > stations.Count)
                return ret; // if less than 8 stations, don't process. too complicate and no use
            while(true) 
            {
                var x = GetBusLocationPartial(stations[i].Item2);
                foreach(var j in x.Item1)
                {
                    int idx = i - 8 + j;
                    if (i == stations.Count - 1)
                        idx--;
                    ret.Item1.Add(string.Format("{0}", stations[idx].Item1, idx));    // 不以初始站开始的话，第一站为。。。 - (i == 8 ? 0 : 1)
                }
                foreach (var j in x.Item2)
                {
                    int idx = i - 8 + j;
                    if (i == stations.Count - 1)
                        idx--;
                    ret.Item2.Add(string.Format("{0}", stations[i - 8 + j].Item1, i - 8 + j)); 
                }
                if (i == stations.Count - 1)
                    break;
                i += 8;
                if (i > stations.Count)
                    i = stations.Count - 1;
            }
            return ret;
        }

        private bool IsColorInRange(System.Drawing.Color c, int r, int g, int b, int range = 15)
        {
            if (c.R >= r - range && c.R <= r + range
                && c.G >= g - range && c.G <= g + range
                && c.B >= b - range && c.B <= b + range)
                return true;
            else
                return false;
        }
        private Tuple<List<int>, List<int>> GetBusLocationPartial(string ImgAddr)
        {
            var s = webProxy.GetToString(string.Format("{0}/{1}", serverAddr, ImgAddr));
            HtmlAgilityPack.HtmlDocument _html = new HtmlAgilityPack.HtmlDocument();
            _html.LoadHtml(s);
            var node = _html.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[3]/img[1]/@src[1]");
            var imgData = webProxy.GetToBytes(string.Format("{0}/{1}", serverAddr, node.Attributes[0].Value));
            //Console.WriteLine(string.Format("站点图片{0}", node.Attributes[0].Value));

            Tuple<List<int>, List<int>> ret = new Tuple<List<int>, List<int>>(new List<int>(), new List<int>());
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imgData))
            {
                var image = System.Drawing.Image.FromStream(ms);
                if (image.Width != 240 || image.Height != 145)
                    return ret;

                //image.Save(string.Format("NBBUS_{0}.bmp", System.DateTime.Now.ToString("yyyy-MM-ddTHH.mm.ss")));
                ms.Close();
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
                {
                    
                    for (int x = 15; x < image.Width; x += 22)
                        for (int y = 40; y <= 40; ++y)
                        {
                            var p = bmp.GetPixel(x, y);
                            if (IsColorInRange(p, 0, 100, 255))
                                ret.Item1.Add((x - 15) / 22);
                            if (IsColorInRange(p, 100, 155, 0))
                                ret.Item2.Add((x - 15) / 22);
                        }
                }
            }

            return ret;
        }
    }
}
