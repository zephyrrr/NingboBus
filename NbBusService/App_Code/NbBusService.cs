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
        private static object m_getBusLineLock = new object();
        public List<BusLineInfo> GetBusLines()
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
                    }
                }
            }
            return s_busLines;
        }

        public string GetBusLineCurrentInfo(string busLineId)
        {
            // "15路(锦江年华始发站=>汽车东站始发站)"
            var x = NingboBusHelper.Instance.GetBusLocations(Convert.ToInt32(busLineId));
            var s = NingboBusHelper.ConvertBusLocationsToString(x);
            return s;
        }
    }
}
