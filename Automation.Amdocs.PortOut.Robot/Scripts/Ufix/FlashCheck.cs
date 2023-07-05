using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using AutomationFramework;
using AutomationFramework.DAO;
using JAB;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
	public class FlashCheck: FindCallerCimSearchPage
	{
		private AccessibleWindow _accessibleWindow;
		private int _vmId;
		private AccessBridge _accessBridge;
		private InteractionHomePage _interactionHomePage = new InteractionHomePage();
		private SearchContactAndCustomerPage _searchContactAndCustomerPage = new SearchContactAndCustomerPage();
		public FlashCheck(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
		{
			_accessibleWindow = accessibleWindowUfix;
			_vmId = vmIdUfix;
			_accessBridge = accessBridge;
		}

		public bool Run()
		{
			DoWork();

			return true;
		}
		
		private void DoWork()
		{
			Logger.MethodEntry("FlashCheck.DoWork");
			
			//Fill in <CTN> and click on the search icon next to it

			var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
			new MainPage().ClickSearchCustomer(credentials.Password);
			
			if(!CheckPageVisibleAndReturnNode(_accessibleWindow, out AccessibleNode findCallerFrame))
				Logger.FinishFlowAsError("Find Caller CIM Search Tab not open", "FindCallerCIM_SearchTabNotOpen");

			ClickCustomerRadioBtn(_accessibleWindow, _vmId, findCallerFrame);
			
			FillInCustomerId(_accessibleWindow, _vmId, GlobalValues.CustomerNumberUfix, findCallerFrame);
			IScreenActions.Wait();
			
			var searchContactAndCustomerInternalFrame = _searchContactAndCustomerPage.GetSearchContactAndCustomerInternalFrame(_accessibleWindow);
			if(searchContactAndCustomerInternalFrame!=null)
			{
				var customerNrSearch = CheckSearchResultsAndSelectRowWithParameter(_accessibleWindow, _vmId, 7, "PRIMAIR", out var noEntry, searchContactAndCustomerInternalFrame);
				if (!customerNrSearch && noEntry)
					Logger.FinishFlowAsError("No result on customerId. Please analyse.","NoResultsOnCustomerNr");
				else if (!customerNrSearch)
					Logger.FinishFlowAsError("Search with customerNumber Error", "SearchWithCustomerNrError");

				_searchContactAndCustomerPage.ClickSelectBtn(_accessibleWindow, _vmId, searchContactAndCustomerInternalFrame);
				IScreenActions.Wait(2);
			}
			
			Logger.AddProcessLog("Check if a Flash message pops up. Read the value of column Title in the flash message.\r\nIf value starts with 'NWCO' then save <Flash>='NWCO'\r\nIf value starts with 'ETF' then save <Flash>='ETF'\r\nIf there is no Flash popup then <Flash>=''");
			if(!_interactionHomePage.CheckPageVisibleAndReturnNode(_accessibleWindow, out var interactionHomeFrame))
				Logger.FinishFlowAsError("Unable to claim Interaction Home frame", "UnableToClaimInteractionHomeFrame");

			IScreenActions.Wait(6);

			AccessibleWindowWithVmIdJavaObjectHandleContextInfo flashPopupWindow = GetActiveFlashPopup(_accessBridge);
			if(flashPopupWindow!=null)
			{
				Logger.AddProcessLog("Flash message detected in Interaction Home page", true, true);

				var urgentMessagesList = _interactionHomePage.GetUrgentFlashMessagesList(flashPopupWindow);
				var flashMessageFound = false;
				foreach (var urgentFlashMessage in urgentMessagesList)
				{
					Logger.AddProcessLog($"urgentFlashMessage: {urgentFlashMessage}");
					if (urgentFlashMessage.ToUpper().StartsWith("NWCO") && !urgentFlashMessage.ToUpper().Contains("Converged".ToUpper()))
					{
						GlobalValues.Flash = "NWCO";
						flashMessageFound = true;
						break;
					}
					if (urgentFlashMessage.ToUpper().StartsWith("ETF"))
					{
						GlobalValues.Flash = "ETF";
						flashMessageFound = true;
						break;
					}
					if (urgentFlashMessage.ToUpper().StartsWith("NWCO") && urgentFlashMessage.ToUpper().Contains("Converged".ToUpper()))
					{
						GlobalValues.Flash = "NWCO_Converged";
						flashMessageFound = true;
						break;
					}
				}
				
				ClickButtonJab(flashPopupWindow.AccessibleWindow, flashPopupWindow.VmId, "Close Window", "push button", 2);
			}
			else
				Logger.AddProcessLog("No Flash message detected in Interaction Home page", true, true);
			
			new MainPage().ClickCloseSubScreenViaX();

			if (!HandleSaveFormDialog(_accessBridge, SaveFormButtons.Discard))
				Logger.FinishFlowAsError("Could not handle the Save Form Dialog. Please investigate.", "SaveFormDialogError");
			
			Logger.AddProcessLog($"<Flash>={GlobalValues.Flash}", true, true);
		}
	}
}


