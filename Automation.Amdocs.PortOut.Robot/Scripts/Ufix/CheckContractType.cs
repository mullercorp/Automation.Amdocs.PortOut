using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
	public class CheckContractType : AccountPage
	{
		private MainPage _mainPage = new MainPage();
		private AccessibleWindow _accessibleWindow;
		private int _vmId;
		private AccessBridge _accessBridge;
		private AccessibleNode _customerNumberIdLink;
		private ViewCasePage _viewCasePage = new ViewCasePage();
		public CheckContractType(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessibleNode customerIdLink, AccessBridge accessBridge)
		{
			_accessibleWindow = accessibleWindowUfix;
			_vmId = vmIdUfix;
			_customerNumberIdLink = customerIdLink;
			_accessBridge = accessBridge;
		}
		
		public bool Run()
		{
			DoWork();

			return true;
		}
		
		private void DoWork()
		{
			Logger.MethodEntry("CheckContractType.DoWork");
			
			_viewCasePage.ClickCustomerIdLink(_vmId, _customerNumberIdLink);

			if(PopupChecker(_accessBridge, "Internal Error", 5, out var ufixErrorWindow1, out var vmIdUfixError1))
			{
				if (!ClickButton(ufixErrorWindow1, vmIdUfixError1, "Ok", "push button", 5))
					Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
			}
			
			GlobalValues.ContractType = GetAccountType(_accessibleWindow, _vmId);
			if(string.IsNullOrEmpty(GlobalValues.ContractType))
				Logger.FinishFlowAsError("accountType Combo Box Not Found", "AccountTypeComboBoxNotFound");
			
			_mainPage.ClickCloseSubScreenViaX();
			
			Logger.AddProcessLog($"<ContractType>={GlobalValues.ContractType}");
		}
	}
}


