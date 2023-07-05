using System;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using Automation.Amdocs.PortOut.Robot.Wrappers;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts
{
    public class PortOutLogic : CoreWrappers
    {
        public void SetMandetoryValuesForPortout()
        {
            Logger.MethodEntry("PortOutLogic.SetMandetoryValuesForPortout");

            if (GlobalValues.PortOutBlocking==Blocking.J)
                Logger.AddProcessLog("PortOutData is already set in the MainFlow");
            else
            {
                if (GlobalValues.Flash.ToUpper() == "NWCO")
                {
                    GlobalValues.PortOutBlocking = Blocking.N;
                    GlobalValues.ProposedDate = DateTime.Now.AddHours(1).ToString("d-M-yyyy HH:mm");
                    GlobalValues.CloseCaseNotes = "Approved by Flash message";
                }
                else if (GlobalValues.Flash.ToUpper() == "ETF")
                {
                    GlobalValues.PortOutBlocking = Blocking.N;
                    GlobalValues.ProposedDate = DateTime.Now.AddMonths(1).AddDays(1).AddHours(1).ToString("d-M-yyyy HH:mm");
                    GlobalValues.CloseCaseNotes = "Approved by Flash message";
                }
                else if (string.IsNullOrEmpty(GlobalValues.Flash))
                {
                    var proposedDateTime = DateTime.ParseExact(GlobalValues.ProposedDate, "yyyyMMdd", null);
                    GlobalValues.DaysBetweenProposedDateAndToday = (proposedDateTime - DateTime.Today).TotalDays;
                    Logger.AddProcessLog($"daysBetween: {GlobalValues.DaysBetweenProposedDateAndToday} ({GlobalValues.ProposedDate} / {DateTime.Today})");

                    if (GlobalValues.DaysBetweenProposedDateAndToday <= 120)
                    {
                        GlobalValues.PortOutBlocking = Blocking.N;
                        GlobalValues.ProposedDate = proposedDateTime.AddHours(1).ToString("d-M-yyyy HH:mm");
                        GlobalValues.CloseCaseNotes = "Approved";
                    }
                    else
                    {
                        GlobalValues.PortOutBlocking = Blocking.J;
                        GlobalValues.PortOutBlockingCode = "63";
                        GlobalValues.CloseCaseNotes = "Rejected J63";
                    }
                }
                else
                    Logger.FinishFlowAsError("Not defined situation. Please investigate.");
            }
            
            Logger.AddProcessLog($"PortOutLogic outcome:\r\nPortOutBlocking={GlobalValues.PortOutBlocking}\r\nPortOutBlockingCode={GlobalValues.PortOutBlockingCode}\r\nCloseCaseNotes={GlobalValues.CloseCaseNotes}\r\nProposedDate={GlobalValues.ProposedDate}");
            
            Logger.MethodExit("PortOutLogic.SetMandetoryValuesForPortout");
        }
    }
}

