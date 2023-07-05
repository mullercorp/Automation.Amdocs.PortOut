using System.Linq;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using Automation.Amdocs.PortOut.Robot.SourceData;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
    public class CheckHistory : ViewCasePage
    {
        private AccessibleWindow _accessibleWindow;
        private int _vmId;
        private AccessBridge _accessBridge;

        public CheckHistory(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Run(out AccessibleNode customerIdLink)
        {
            DoWork(out customerIdLink);
            return true;
        }

        private void DoWork(out AccessibleNode customerIdLink)
        {
            Logger.MethodEntry("CheckHistory.DoWork");

            customerIdLink = null;

            IScreenActions.Wait(2);
            
            var viewCaseFrame = GetViewCaseFrame(_accessibleWindow, Json.Data.CaseNumber);
            if (viewCaseFrame == null)
                Logger.FinishFlowAsError($"GetTab View Case: {Json.Data.CaseNumber} Error", $"GetTabViewCase{Json.Data.CaseNumber}Error");

            if (!ClickHistoryTab(_accessibleWindow, _vmId, viewCaseFrame))
                Logger.FinishFlowAsError("Can't click History tab in View Case page. Please analyse", "HistoryTabClickError");

            IScreenActions.Wait(3);
            
            if(PopupChecker(_accessBridge, "Internal Error", 5, out var ufixErrorWindow1, out var vmIdUfixError1))
            {
                if (!ClickButton(ufixErrorWindow1, vmIdUfixError1, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }

            IScreenActions.Wait(2);
            
/*            var historyTab = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "History", "page tab", 5, child: viewCaseFrame);
            Logger.AddProcessLog("Historytab geclaimd.");*/
            
            var trees = GetTreesInHistoryTab(_accessibleWindow, Json.Data.CaseNumber);
            if (trees == null)
                Logger.FinishFlowAsError("No 'trees' found in History. Please analyse. (1)", "NoTreesInHistory(1)");
            if (!trees.Any())
                Logger.FinishFlowAsError("No 'trees' found in History. Please analyse. (2)", "NoTreesInHistory(2)");

            Logger.AddProcessLog($"trees: {trees.Count}");

            var caseAangevuldNode = GetCaseAangevuldNodeInHistory(_accessibleWindow, _vmId, trees);
            if (caseAangevuldNode == null)
                Logger.FinishFlowAsError("Could not locate 'Notes' in 'Case aangevuld' in History tab. Please analyse.", "NoNotesInCaseAangevuld");

            var caseAangevuldNotes = GetCaseAangevuldNotesAsString(_vmId, caseAangevuldNode);
            if (string.IsNullOrEmpty(caseAangevuldNotes))
                Logger.FinishFlowAsError("Cant get contextInfo for labelCaseAangevuld", "NoContextInfo");

            CaseAangevuldNotesData caseAangevuldNotesData = GetCaseAangevuldNotesData(caseAangevuldNotes);
            if(caseAangevuldNotesData==null)
                Logger.FinishFlowAsError("Notes in History tab is not filled properly. Please investigate.","NoNotesInHistoryTab");

            GlobalValues.CustomerNumberHistory = caseAangevuldNotesData.CustomerNumber;
            GlobalValues.CTN = caseAangevuldNotesData.CTN;
            GlobalValues.ProposedDate = caseAangevuldNotesData.ProposedDate;

            if (!string.IsNullOrEmpty(caseAangevuldNotesData.CtnCount))
            {
                if (int.Parse(caseAangevuldNotesData.CtnCount) > 1)
                    Logger.HandleBusinessRule("There is a CTN range. Please process manually.", "CtnRangeDetected");
            }

            Logger.AddProcessLog("If CustomerNumberHistory is filled in, then also save Customer ID in the above section below label 'Customer ID:'. Save as <CustomerNumber>");
            customerIdLink = GetCustomerNumberNode(_accessibleWindow, viewCaseFrame);
            GlobalValues.CustomerNumberUfix = GetCustomerNumberInContactSection(_accessibleWindow, _vmId, customerIdLink);
            Logger.AddProcessLog($"CustomerNumber: {GlobalValues.CustomerNumberUfix}", true, true);

            Logger.MethodExit("CheckHistory.DoWork");
        }
    }
}