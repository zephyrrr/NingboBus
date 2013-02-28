using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace NbBusService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService”。
    [ServiceContract]
    public interface INbBusService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
                BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetBusLines")]
        List<BusLineInfo> GetBusLines();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
                BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetBusLineCurrentInfo/{strBusLineId}")]
        BusLineRunInfo GetBusLineCurrentInfo(string strBusLineId);
    }

    [DataContract]
    public class BusLineInfo
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public int Id
        {
            get;
            set;
        }
    }

    [DataContract]
    public class BusLineRunInfo
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public string RunInfo
        {
            get;
            set;
        }
    }
}
