using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace UNIConsole.DataSet
{
    internal class CJsonContent : StringContent
    {
        public CJsonContent(object content) :
            base(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
        { }
    }
}
