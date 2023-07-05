using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using AutomationFramework;
using AutomationFramework.DAO;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
    public class OpenCase : UfixBaseJab
    {
        private AccessibleWindow _accessibleWindow;
        private int _vmId;
        private UfixBase _ufixBase = new UfixBase();
        public OpenCase(AccessibleWindow accessibleWindowUfix, int vmIdUfix)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
        }
        
        public void Run(string caseNumber)
        {
            Logger.MethodEntry("Ufix_Open_Case.Execute");
            
            if (!OpenTab("k", true, true))
                Logger.FinishFlowAsError("OpenTab Error", "OpenTab Error");
            
            if (IScreenActions.WaitUntilWinTitleIsActive("Login", 10))
            {
                var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
                if (!_ufixBase.LoginAfterTimout(credentials.Password))
                {
                    Logger.FinishFlowAsError("Could not Login to Ufix after timeout", "CannotLoginAfterTimeout");
                }
            }
                
            if(!CheckNodeVisible(_accessibleWindow, "Search: Case Results", "internal frame", 10))
            {
                InitializeUfixAtFirstStartup(_accessibleWindow);
                
                if (!OpenTab("k", true, true))
                    Logger.FinishFlowAsError("OpenTab Error", "OpenTabError");
                
                if (IScreenActions.WaitUntilWinTitleIsActive("Login", 10))
                {
                    var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
                    if (!_ufixBase.LoginAfterTimout(credentials.Password))
                    {
                        Logger.FinishFlowAsError("Could not Login to Ufix after timeout", "CannotLoginAfterTimeout");
                    }
                }
                
                if (!CheckNodeVisible(_accessibleWindow, "Search: Case Results", "internal frame", 10))
                    Logger.FinishFlowAsError("Search: Case Results Tab not open", "SearchCaseResultsTabNotOpen");
            }

            if(!TypeIntoTabField(_accessibleWindow, _vmId, "ID", "push button", 20,caseNumber))
                Logger.FinishFlowAsError("TypeIntoTabField Error", "TypeIntoTabField Error");
                
            if(!OpenEntryWithField(_accessibleWindow, _vmId, "Account ID", "push button", 20))
                Logger.FinishFlowAsError("OpenEntryWithField Failed", "OpenEntryWithField Failed");

            Logger.AddProcessLog($"Process started with Case Number: {caseNumber}", true, true);
            Logger.MethodExit("Ufix_Open_Case.Execute");
        }
    }
}