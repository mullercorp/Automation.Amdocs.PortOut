using Automation.Amdocs.PortOut.Data;
using AutomationFramework;
using Newtonsoft.Json;

namespace Automation.Amdocs.PortOut.DLRobot.SourceData
{
    public class Json
    {
        public static JsonSourceData Data
        {
            get { return JsonConvert.DeserializeObject<JsonSourceData>(CurrentScriptRun.Input.Json); }
        }
    }
}