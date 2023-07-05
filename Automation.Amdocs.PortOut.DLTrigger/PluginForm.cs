using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Automation.Amdocs.PortOut.Data;
using AutomationFramework.Classes;
using AutomationFramework.DAO;
using AutomationFramework.Model;
using AutomationLoader.Classes;
using Newtonsoft.Json;

namespace Automation.Amdocs.PortOut.DLTrigger
{
    public partial class PluginForm : LoaderConsolePlugInBaseForm
    {
        private PlugInData _plugInData = new PlugInData();

        public PluginForm()
        {
            InitializeComponent();
        }

        public override void InitializePlugIn(Loader loaderEntry)
        {
            base.InitializePlugIn(loaderEntry);
        }

        public override void ProcessInputLine(IProgress<PlugInData> progress)
        {
            AddLineToConsole(ref progress, "Start Loading PortOutTrigger DataLoader");
            AddLineToConsole(ref progress, $"HandlePortOutTriggerOrders(): {HandlePortOutTriggerOrders(ref progress)}");
            AddLineToConsole(ref progress, "Finished PortOutTrigger DataLoader");
        }

        private void AddLineToConsole(ref IProgress<PlugInData> progress, string line)
        {
            _plugInData.ConsoleText = line;
            progress?.Report(_plugInData);
            Thread.Sleep(50);
        }
        
        public bool HandlePortOutTriggerOrders(ref IProgress<PlugInData> progress)
        {
            JsonSourceData finalSourceData = new JsonSourceData {CaseNumber = "TriggerTask"};
            string json = JsonConvert.SerializeObject(finalSourceData);
            
            Input inputLine = new Input {Action = "PortOutDLRobot", Json = json, Identifier = "TriggerTask", Field1 = LoaderEntry.Par1, 
                Field2 = LoaderEntry.Par2, Field3 = LoaderEntry.Par3, Field9 = LoaderEntry.Par9, Field10 = LoaderEntry.Par10};
            
            var inpUnique = InputDAO.InsertInputLineReturnID(LoaderEntry.Priority, Convert.ToInt32(LoaderEntry.Routing), LoaderEntry.StartRunWindow,
                LoaderEntry.EndRunWindow, DateTime.Now, DateTime.Now.AddDays(360), inputLine);
            
            LoaderLogDAO.AddLoaderLog(LoaderEntry, $"Added input: {inpUnique}", inpUnique);
            
            return true;
        }
    }
}
