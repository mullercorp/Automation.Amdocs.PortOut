using System;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Data;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationFramework.Model;
using Newtonsoft.Json;
using MainPage = Amdocs.Shared.Ufix.Pages.MainPage;

namespace Automation.Amdocs.PortOut.DLRobot.Scripts.UFix
{
    public class CollectCases : MyQueuesPage
    {
        private MainPage _mainPage = new MainPage();
        private UfixBaseJab _ufixBaseJab = new UfixBaseJab();

        public bool Execute()
        {
            Logger.MethodEntry("CollectCases.Execute");

            if (!_mainPage.OpenQueueBoDoxGeneric("N"))
                Logger.FinishFlowAsError("Couldn't detect BO_DOX_.. Please investigate.", "BO_DOX-Error");

            var casesToUploadIntoDb = CasesToUploadIntoDb("N", "PortOut", "My Queues: BO_DOX_N");
            foreach (var caseToUpload in casesToUploadIntoDb)
                UploadingCasesIntoDb(caseToUpload);

            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, $"Collected:{casesToUploadIntoDb.Count}");

            new MainPage().ClickCloseSubScreenViaX();

            Logger.MethodExit("CollectCases.Execute");
            return true;
        }

        private static void UploadingCasesIntoDb(CollectData caseToUpload)
        {
            Logger.AddProcessLog($"Trying to create a FLX inputLine for {caseToUpload.CaseId}");

            JsonSourceData finalSourceData = new JsonSourceData {CaseNumber = caseToUpload.CaseId};
            string json = JsonConvert.SerializeObject(finalSourceData);

            Input inputLine = new Input
            {
                Action = "PortOutRobot",
                Json = json,
                Identifier = caseToUpload.CaseId,
                Field2 = caseToUpload.CaseId,
                ImportUser = "PortOutDLRobot"
            };

            InputDAO.InsertInputLine(int.Parse(CurrentScriptRun.Input.Field9.ToLower().Replace("priority", "")), int.Parse(CurrentScriptRun.Input.Field10.ToLower().Replace("routing", "")), new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59), DateTime.Now, DateTime.Today.AddMonths(3), inputLine);

            Logger.AddProcessLog($"InputLine for {caseToUpload.CaseId} created");
        }
    }
}