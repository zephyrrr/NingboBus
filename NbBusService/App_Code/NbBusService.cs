using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace NbBusService
{
    public class NbBusService : INbBusService
    {
        private static List<BusLineInfo> s_busLines;
        private static Dictionary<int, string> s_dictBusLines = new Dictionary<int, string>();
        private static object m_getBusLineLock = new object();

        static NbBusService()
        {
        }
        private static void LoadData()
        {
            lock (m_getBusLineLock)
            {
                if (s_busLines == null)
                {
                    var dt = Feng.Data.DbHelper.Instance.ExecuteDataTable("SELECT Id, Name FROM BusLines");
                    s_busLines = new List<BusLineInfo>();
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        s_busLines.Add(new BusLineInfo { Id = (int)row["Id"], Name = (string)row["Name"] });
                        s_dictBusLines[(int)row["Id"]] = (string)row["Name"];
                    }
                }
            }
        }
        public List<BusLineInfo> GetBusLines()
        {
            return s_busLines;
        }

        public BusLineRunInfo GetBusLineCurrentInfo(string strBusLineId)
        {
            // "15路(锦江年华始发站=>汽车东站始发站)"
            int busLineId = Convert.ToInt32(strBusLineId);
            if (!s_dictBusLines.ContainsKey(busLineId))
                return null;
            var x = NingboBusHelper.Instance.GetBusLocations(busLineId);
            var s = NingboBusHelper.ConvertBusLocationsToString(x);
            return new BusLineRunInfo { Name = s_dictBusLines[busLineId], RunInfo = s };
        }
    }
}
