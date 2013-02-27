using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Feng;

namespace NbBusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //NingboBusHelper.Instance.ResetDb();
            //NingboBusHelper.Instance.GenerateBusLineData();
            //NingboBusHelper.Instance.GenerateBusStationData();

            var x = NbBusService.NingboBusHelper.Instance.GetBusLocations(35); // 15路(锦江年华始发站=>汽车东站始发站)
            var s = NbBusService.NingboBusHelper.Instance.ConvertBusLocationsToString(x);
        }
    }
}
