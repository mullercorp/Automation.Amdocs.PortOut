using System;
using Automation.Amdocs.PortOut.DLRobot.Scripts.UFix;
using Automation.Amdocs.PortOut.DLRobot.Wrappers;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationLoader.Classes;

namespace Automation.Amdocs.PortOut.DLRobot.MainFlow
{
    public class MainFlow : AutomationBaseScript
    {
        public override void ExecuteScript()
        {
            Logger.AddProcessLog($"Started MainFlow with Key: {CurrentScriptRun.Input.Key}. Timestamp: {DateTime.Now.TimeOfDay}");
            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, "");

            Shared.Utilities.ProcessAssistant.KillProcesses("jp2Launcher");
            CoreWrappers.IScreenActions.Wait();

            if (new StartUfix().Execute() == false)
            {
                Shared.Utilities.ProcessAssistant.KillProcesses("jp2Launcher");
                CoreWrappers.IScreenActions.Wait();

                if (new StartUfix().Execute() == false)
                    Logger.FinishFlowAsError("StartUfix Failed.", "UfixStartupError");
            }

            new CollectCases().Execute();

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
        }
    }
}