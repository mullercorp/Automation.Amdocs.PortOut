using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
    public class CloseCase : ViewCasePage
    {
        private CloseCasePage _closeCasePage = new CloseCasePage();
        private SearchCaseResultsPage _searchCaseResultsPage = new SearchCaseResultsPage();

        private readonly AccessibleWindow _accessibleWindow;
        private readonly int _vmId;
        private AccessBridge _accessBridge;

        public CloseCase(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }
        
        public void Run(string caseNumber)
        {
            Logger.MethodEntry("CloseCase.Execute");
            
            var viewCaseFrame = GetViewCaseFrame(_accessibleWindow, caseNumber);
            if (viewCaseFrame == null)
                SearchAndOpenCase(caseNumber);

            var closeCaseBtn = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Close Case", "push button", 5, "enabled", child:viewCaseFrame);
            if(closeCaseBtn==null)
                SearchAndOpenCase(caseNumber);
            
/*            if(!CheckNodeVisible(ufixMainWindow, "View Case: "+caseNumber, "internal frame", 20))
                Logger.FinishFlowAsError("CheckOpenEntryOpen not Visible", "CheckOpenEntryOpen not Visible");*/
            
/*            if(!ClickButton_Retrieve(ufixMainWindow, vmIdUfix, "Close Case", "push button", 20))
                Logger.FinishFlowAsError("CloseCase Button Error", "CloseCase Button Error");*/        

            if (!ClickButton_CloseCase(_accessibleWindow, _vmId))
                Logger.FinishFlowAsError("CloseCase Button Error", "CloseCaseButtonError");    
            
            if (!GetTab(_accessibleWindow, _vmId, "Close Case", 20, out var tab))
                Logger.FinishFlowAsError("GetTab Close Case Error", "GetTab Close Case Error");          
            
            if(!ClickButton(_accessibleWindow, _vmId, "Resolution:", "combo box", 20, "showing", false, tab))
                Logger.FinishFlowAsError("ClickButton Resolution listbox Error", "ClickButton Resolution listbox Error");
            
            IScreenActions.Wait();
            IScreenActions.Send("{TAB}");
            IScreenActions.Wait();
            IScreenActions.Send(GlobalValues.CloseCaseNotes);
            IScreenActions.Wait();

/*            if(!ClickButton_Retrieve(ufixMainWindow, vmIdUfix, "Save", "push button", 20))
                Logger.FinishFlowAsError("Second CloseCase Button Error", "Second CloseCase Button Error");*/
            if(!_closeCasePage.ClickButton_CloseCase_Save(_accessibleWindow, _vmId))
                Logger.FinishFlowAsError("Second CloseCase Button Error", "SecondCloseCaseButtonError");
            
            IScreenActions.Wait(6);
            
            if(_closeCasePage.GetCloseCaseInternalFrame(_accessibleWindow)!=null)
                Logger.FinishFlowAsError("Close Page Frame still present after clicking on Close Case. Please investigate.", "CloseCaseError");
            
/*            if(!ClickButtonWithTab(ufixMainWindow, vmIdUfix, "Search: Case Results", "Cancel", "push button", 20))            
                Logger.FinishFlowAsError("Cancel Button Error", "Cancel Button Error");*/
/*            if(!_searchCaseResultsPage.ClickButton_Cancel(_accessibleWindow, _vmId))
                Logger.FinishFlowAsError("Cancel Button Error", "CancelButtonError");*/

            Logger.MethodExit("CloseCase.Execute");
        }

        private void SearchAndOpenCase(string caseNumber)
        {
            if (!OpenTab("k", true, true))
                Logger.FinishFlowAsError("OpenTab Error", "OpenTab Error");
                
            if(!CheckNodeVisible(_accessibleWindow, "Search: Case Results", "internal frame", 20))
                Logger.FinishFlowAsError("Main Window Tab not open", "Main Window Tab not open");

            if(!TypeIntoTabField(_accessibleWindow, _vmId, "ID", "push button", 20,caseNumber))
                Logger.FinishFlowAsError("TypeIntoTabField Error", "TypeIntoTabField Error");
                
            if(!OpenEntryWithField(_accessibleWindow, _vmId, "Account ID", "push button", 20))
                Logger.FinishFlowAsError("OpenEntryWithField Failed", "OpenEntryWithField Failed");
        }
    }
}