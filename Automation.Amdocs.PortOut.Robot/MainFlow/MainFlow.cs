using System;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Data;
using Automation.Amdocs.PortOut.Robot.Scripts;
using Automation.Amdocs.PortOut.Robot.Scripts.Ufix;
using Automation.Amdocs.PortOut.Robot.Scripts.UFix;
using Automation.Amdocs.PortOut.Robot.SourceData;
using Automation.Amdocs.PortOut.Robot.Wrappers;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationLoader.Classes;
using Newtonsoft.Json;
using Shared.Utilities;

namespace Automation.Amdocs.PortOut.Robot.MainFlow
{
    public class MainFlow : AutomationBaseScript
    {
        public override void ExecuteScript()
        {
            Logger.AddProcessLog($"Started MainFlow with Key: {CurrentScriptRun.Input.Key}. Timestamp: {DateTime.Now.TimeOfDay}");
            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, "");

            //Test();
            ProcessAssistant.KillProcesses("rdpclip");
            ProcessAssistant.KillProcesses("jp2Launcher");
            CoreWrappers.IScreenActions.Wait();

            if (new StartUfix().Execute() == false)
            {
                ProcessAssistant.KillProcesses("jp2Launcher");
                CoreWrappers.IScreenActions.Wait();

                if (new StartUfix().Execute() == false)
                    Logger.FinishFlowAsError("StartUfix Failed.", "UfixStartupError");
            }

            CoreWrappers.IScreenActions.Wait(2);
            new Initialize().Run(20, out AccessBridge accessBridge, out AccessibleWindow ufixMainWindow, out int vmIdUfix, out JavaObjectHandle acUfix);

            if (new UfixBaseJab().PopupChecker(accessBridge, "Internal Error", 3, out var ufixErrorWindowPopup, out var vmIdUfixErrorPopup))
            {
                if (!new UfixBaseJab().ClickButton(ufixErrorWindowPopup, vmIdUfixErrorPopup, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }

            new OpenCase(ufixMainWindow, vmIdUfix).Run(Json.Data.CaseNumber);

            new CheckHistory(ufixMainWindow, vmIdUfix, accessBridge).Run(out AccessibleNode customerIdLink);
            UpdateInputDataForResultGen();

            if (!GlobalValues.CTN.StartsWith("316"))
                Logger.HandleBusinessRule($"Ctn doesn't starts with '316'. Please process manually.", "CtnNot316");

            if (string.IsNullOrEmpty(GlobalValues.CustomerNumberHistory))
            {
                new CheckContractType(ufixMainWindow, vmIdUfix, customerIdLink, accessBridge).Run();

                switch (GlobalValues.ContractType.ToUpper())
                {
                    case "INDIVIDUAL":
                        new FlashCheck(ufixMainWindow, vmIdUfix, accessBridge).Run();
                        break;
                    case "BUSINESS":
                        Logger.AddProcessLog($"<CustomerNumber> is empty and <ContractType> is buiness\r\nPortOutBlocking{GlobalValues.PortOutBlocking}=J\r\nPortOutBlockingCode{GlobalValues.PortOutBlockingCode}=27\r\nCloseCaseNotes{GlobalValues.CloseCaseNotes}=Rejected J27");
                        GlobalValues.PortOutBlocking = Blocking.J;
                        GlobalValues.PortOutBlockingCode = "27";
                        GlobalValues.CloseCaseNotes = "Rejected J27";
                        break;
                    default:
                        Logger.HandleBusinessRule($"ContractType: {GlobalValues.ContractType}", $"ContractType: {GlobalValues.ContractType}");
                        break;
                }
            }
            else if (GlobalValues.CustomerNumberUfix != GlobalValues.CustomerNumberHistory)
            {
                Logger.AddProcessLog($"<CustomerNumber> is a mismatch with CustomerNumber\r\nPortOutBlocking{GlobalValues.PortOutBlocking}=J\r\nPortOutBlockingCode{GlobalValues.PortOutBlockingCode}=63\r\nCloseCaseNotes{GlobalValues.CloseCaseNotes}=Rejected J63");
                GlobalValues.PortOutBlocking = Blocking.J;
                GlobalValues.PortOutBlockingCode = "63";
                GlobalValues.CloseCaseNotes = "Rejected J63";
            }
            else if (!string.IsNullOrEmpty(GlobalValues.CustomerNumberHistory))
                new FlashCheck(ufixMainWindow, vmIdUfix, accessBridge).Run();

            if(GlobalValues.Flash=="NWCO_Converged")
                Logger.HandleBusinessRule("Fix 05-07-2023: NWCO_Converged","NWCO_Converged");
            
            new PortOutLogic().SetMandetoryValuesForPortout();
            if (GlobalValues.PortOutBlocking == Blocking.N)
            {
                new PendingCheck(ufixMainWindow, vmIdUfix, accessBridge).Run();
                if (GlobalValues.Pending && !GlobalValues.PendingCease)
                {
                    UpdateInputDataForResultGen();
                    Logger.HandleBusinessRule("Pending order present.", "PendingOrder");
                }
            }

            if (GlobalValues.PortOutBlocking == Blocking.N && string.IsNullOrEmpty(GlobalValues.Flash) && GlobalValues.DaysBetweenProposedDateAndToday <= 30/*120*/ && !GlobalValues.ProposedDateChangedBecauseCease)
            {
                Logger.AddProcessLog("New proposedDate logic since 12-11-2021 calling the uhelp 101 api...");
                var creds = CredentialsDAO.GetAppCredentials(0, "Uhelp_RestApi");
                var oldest101Data = new Shared.Uhelp.UhelpBase().GePortingDataOldest101ViaRestApi(AssetsDAO.GetAssetByName("Uhelp_RestApi_PortingDataOldest101Url"), Json.Data.CTN, creds.Password, creds.Username);
                if (oldest101Data.Success)
                {
                    GlobalValues.ProposedDate = oldest101Data.Oldest101Date.AddMonths(1).ToString("d-M-yyyy HH:mm");
                    Logger.AddProcessLog($"New proposedData after uhelp call {GlobalValues.ProposedDate}");
                    GlobalValues.ProposedDateViaApiSuccessfully = true;
                }
            }
        
            UpdateInputDataForResultGen();
            
            //ALEX CODE
            if (GlobalValues.PendingCease)
            {
                new CancelCease(ufixMainWindow, vmIdUfix, accessBridge).Run();
            }

            new PortOut(ufixMainWindow, vmIdUfix, accessBridge).Run();

            new CloseCase(ufixMainWindow, vmIdUfix, accessBridge).Run(Json.Data.CaseNumber);

            //accessBridge.Dispose();
            if (GlobalValues.Flash.ToUpper() == "ETF")
                InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, "ETF");

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
        }

        private void Test()
        {
            //GlobalValues.CTN = "31631786561";
            new Initialize().Run(20, out AccessBridge accessBridge, out AccessibleWindow ufixMainWindow, out int vmIdUfix, out JavaObjectHandle acUfix);
            //new FlashCheck(ufixMainWindow, vmIdUfix, accessBridge).Run();
            //new PendingCheck(ufixMainWindow, vmIdUfix, accessBridge).Run();
            new PortOut(ufixMainWindow, vmIdUfix, accessBridge).Run();
            Logger.HandleBusinessRule("Test King Muller", "TestKingMuller");
        }

        public void UpdateInputDataForResultGen()
        {
            var input = CurrentScriptRun.Input;
            JsonSourceData jsonSourceData = JsonConvert.DeserializeObject<JsonSourceData>(CurrentScriptRun.Input.Json);
            jsonSourceData.Flash = GlobalValues.Flash;
            jsonSourceData.ProposedDate = GlobalValues.ProposedDate;
            jsonSourceData.PortOutBlocking = GlobalValues.PortOutBlocking.ToString();
            jsonSourceData.CustomerNumberHistory = GlobalValues.CustomerNumberHistory;
            jsonSourceData.ContractType = GlobalValues.ContractType;
            jsonSourceData.CTN = GlobalValues.CTN;
            jsonSourceData.CloseCaseNotes = GlobalValues.CloseCaseNotes;
            jsonSourceData.CustomerNumberUfix = GlobalValues.CustomerNumberUfix;
            jsonSourceData.PortOutBlockingCode = GlobalValues.PortOutBlockingCode;
            jsonSourceData.DaysBetweenProposedDateAndToday = GlobalValues.DaysBetweenProposedDateAndToday.ToString();
            jsonSourceData.Pending = GlobalValues.Pending.ToString();
            jsonSourceData.PendingCease = GlobalValues.PendingCease.ToString();
            input.Json = JsonConvert.SerializeObject(jsonSourceData);
            InputDAO.UpdateInputLineById(input);
        }
    }

    public static class GlobalValues
    {
        public static string CustomerNumberUfix { get; set; }
        public static string CustomerNumberHistory { get; set; }
        public static string CTN { get; set; }
        public static string ContractType { get; set; }
        public static Blocking PortOutBlocking { get; set; }
        public static string PortOutBlockingCode { get; set; }
        public static string CloseCaseNotes { get; set; }
        public static string Flash { get; set; }
        public static string ProposedDate { get; set; }
        public static bool Pending { get; set; }
        public static bool PendingCease { get; set; }
        public static bool ProposedDateChangedBecauseCease { get; set; }

        public static double DaysBetweenProposedDateAndToday { get; set; }

        public static bool ProposedDateViaApiSuccessfully { get; set; }

        static GlobalValues()
        {
            CustomerNumberUfix = string.Empty;
            CustomerNumberHistory = string.Empty;
            CTN = string.Empty;
            ContractType = string.Empty;
            PortOutBlocking = Blocking.N;
            PortOutBlockingCode = string.Empty;
            CloseCaseNotes = string.Empty;
            Flash = string.Empty;
            ProposedDate = string.Empty;
            Pending = false;
            PendingCease = false;
            ProposedDateChangedBecauseCease = false;
            DaysBetweenProposedDateAndToday = 0;
            ProposedDateViaApiSuccessfully = false;
        }
    }
}