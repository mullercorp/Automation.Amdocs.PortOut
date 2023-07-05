using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
    public class Initialize : UfixBaseJab
    {
        public void Run(int retries, out AccessBridge accessBridge, out AccessibleWindow ufixMainWindow, out int vmIdUfix, out JavaObjectHandle acUfix)
        {
            Logger.MethodEntry("Ufix_Initialize.Execute");

            if(!AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out acUfix, retries))
                Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
            
            Logger.MethodExit("Ufix_Initialize.Execute");
        }        
    }
}