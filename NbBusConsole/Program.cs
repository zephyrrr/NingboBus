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

            int busLineId = 35;
            var x = NbBusService.NingboBusHelper.Instance.GetBusLocations(busLineId); // 15路(锦江年华始发站=>汽车东站始发站)
            var s = NbBusService.NingboBusHelper.ConvertBusLocationsToString(busLineId, x);
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
