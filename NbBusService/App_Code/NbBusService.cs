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
        public List<BusLineInfo> GetBusLines()
        {
            var dt = Feng.Data.DbHelper.Instance.ExecuteDataTable("SELECT Id, Name FROM BusLines");
            List<BusLineInfo> ret = new List<BusLineInfo>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                ret.Add(new BusLineInfo { Id = (int)row["Id"], Name = (string)row["Name"] });
            }
            return ret;
        }

        public string GetBusLineCurrentInfo(string busLineId)
        {
            // "15路(锦江年华始发站=>汽车东站始发站)"
            var x = NingboBusHelper.Instance.GetBusLocations(Convert.ToInt32(busLineId));
            var s = NingboBusHelper.Instance.ConvertBusLocationsToString(x);
            return s;
        }
    }
}
