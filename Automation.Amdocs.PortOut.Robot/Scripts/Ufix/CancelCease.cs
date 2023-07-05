using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using AutomationFramework;

namespace Automation.Amdocs.PortOut.Robot.Scripts.Ufix
{
    public class CancelCease : UfixBaseJab
    {
        private readonly AccessibleWindow _accessibleWindow;
        private readonly int _vmId;
        private AccessBridge _accessBridge;
        private readonly ViewCasePage _viewCasePage = new ViewCasePage();

        public CancelCease(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Run()
        {
            Logger.MethodEntry("CancelCease.DoWork");

            return DoWork();
        }

        private bool DoWork()
        {

            var mobilePhoneSearch =SearchWithPhoneNumberAndSelect(_accessBridge, _accessibleWindow, _vmId, GlobalValues.CTN, out bool noEntry2);
            if (!mobilePhoneSearch && noEntry2)
                Logger.HandleBusinessRule("Entry Not Found", "EntryNotFound");
            else if (!mobilePhoneSearch)
            {
                Logger.FinishFlowAsError("Search with Mobile Phone Error", "SearchWithMobilePhoneError");
            }

            if (!ClickButtonWithTab(_accessibleWindow, _vmId, "Search: Contact and Subscription","View Assigned Products", "push button", 20, "enabled"))
                Logger.FinishFlowAsError("View Assigned Products Button Error", "ViewAssignedProductsButtonError");         

            if (GetTab(_accessibleWindow, _vmId, "Assigned Product's Details", 30, out var apdTab))
            {
                if(PopupChecker(_accessBridge, "Internal Error", 5, out var ufixErrorWindow1, out var vmIdUfixError1) || PopupChecker(_accessBridge, "Problem", 5, out ufixErrorWindow1, out vmIdUfixError1))
                {
                    if (!ClickButton(ufixErrorWindow1, vmIdUfixError1, "Ok", "push button", 5))
                        Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
                }
            }
            
            var pageTabChangeHistory = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow,"Change History", "page tab", 30, child: apdTab);
            if (pageTabChangeHistory==null)
                Logger.FinishFlowAsError("Page Tab Change History Not Claimed", "PageTabChangeHistoryNotClaimed");  

            if (!ClickButton(_vmId, pageTabChangeHistory))
                Logger.FinishFlowAsError("Click Page Tab Change History error", "ClickPageTabChangeHistoryError");  
                
            if (!ClickEntryWithCellValue(_accessibleWindow, _vmId, 20, 1, "CEASE", 6, "ORDERED", "State", pageTabChangeHistory))
                Logger.FinishFlowAsError("Clicking Ordered Entry error", "ClickingOrderedEntryError");  
            
            if(!GetOrderIdAndOpen(_accessibleWindow, _vmId, 20, out var pendingOrderId))
                Logger.FinishFlowAsError("Pending Order ID Claim or/end Access Error", "PendingOrderIdClaimOrAccessError");
                
            if (!GetTab(_accessibleWindow, _vmId, "Order Details:", 30, out var odTab))
                Logger.FinishFlowAsError("Tab Order Details Not Claimed", "TabOrderDetailsNotClaimed"); 
                
            if (!ClickButton(_accessibleWindow, _vmId, "Cancel Order", "push button", 15, "enabled", child: odTab))
                Logger.FinishFlowAsError("Cancel Order Button Not Found/Enabled", "CancelOrderButtonNotFound/Enabled"); 
           
            if (!ConfirmCancellation("01", 20))
                Logger.FinishFlowAsError("Confirm Cancellation Failed", "ConfirmCancellationFailed");    
            
            if (!GetTab(_accessibleWindow, _vmId, "Order Summary", 30, out var osTab))
                Logger.FinishFlowAsError("Tab Order Summary Not Claimed", "TabOrderSummaryNotClaimed"); 
            
            if (!ClickButton(_accessibleWindow, _vmId, "Submit Cancel", "push button", 15, "enabled", child: osTab, waitBeforeClicking: 3))
                Logger.FinishFlowAsError("Submit Cancel Button Not Found/Enabled", "SubmitCancelButtonNotFound/Enabled");
            
            if(PopupChecker(_accessBridge, "Internal Error", 5, out var ufixErrorWindow2, out var vmIdUfixError2) || PopupChecker(_accessBridge, "Problem", 5, out ufixErrorWindow2, out vmIdUfixError2))
            {
                if (!ClickButton(ufixErrorWindow2, vmIdUfixError2, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }
            
            new MainPage().ClickCloseSubScreenViaX(3);           
            
            return true;
        }

        public bool GetOrderIdAndOpen(AccessibleWindow _accessibleWindow, int _vmId, int retries, out string orderId)
        {
            if (!GetTab(_accessibleWindow, _vmId, "View Order Action", 30, out var tab))
                Logger.FinishFlowAsError("Account Tab not found", "AccountTabNotFound");
            
            var orderLabel = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Order ID:", "label", retries, "showing", true, tab);
            if (orderLabel == null)
            {
                orderId = null;
                return false;
            }

            var orderLabelAci = JabBaseActions.GetAccessibleContextInfo(_vmId, orderLabel);
            
            var parent = orderLabel.GetParent();

            var orderField = JabBaseActions.GetChildFromParentNode(parent, orderLabelAci.indexInParent - 1);
            
            var orderFieldAci = JabBaseActions.GetAccessibleContextInfo(_vmId, orderField);

            orderId = orderFieldAci.name.Trim();
            
            int fetchRetries = 0;
            while (string.IsNullOrEmpty(orderId) && fetchRetries<10)
            {
                IScreenActions.Wait();
                orderField = JabBaseActions.GetChildFromParentNode(parent, orderLabelAci.indexInParent - 1);
                orderFieldAci = JabBaseActions.GetAccessibleContextInfo(_vmId, orderField);
                orderId = orderFieldAci.name.Trim();
            }
            if (string.IsNullOrEmpty(orderId))
                return false;
            
            IScreenActions.LeftMouseClickAt(orderFieldAci.x + orderFieldAci.width/5, orderFieldAci.y + orderFieldAci.height/2);

            return true;
        }
        
        public bool ClickEntryWithCellValue(AccessibleWindow accessibleWindow, int vmIdUfix, int retries, int columnIndex1, string stringToSearchFor1, int columnIndex2, string stringToSearchFor2, string rowHeaderHelper, AccessibleNode parent=null)
        {
            AccessibleNode nodeWithParameters = JabBaseActions.GetAccessibleNodeWithParameters(accessibleWindow, "", "table", retries, "showing", false, parent);
            if (nodeWithParameters == null)
                return false;
            
            int rowCount;
            int columnCount;
            CheckResultsAfterSearch(nodeWithParameters, out rowCount, out columnCount);
            if (rowCount == 0)
                return false;
            
            int num = 0;
            bool flag = false;
            for (int row = 0; row < rowCount; ++row)
            {
                if (JabBaseActions.GetTableCellValue(vmIdUfix, nodeWithParameters, row, columnIndex1).ToUpper().Contains(stringToSearchFor1))
                {
                    if (JabBaseActions.GetTableCellValue(vmIdUfix, nodeWithParameters, row, columnIndex2).ToUpper() == stringToSearchFor2)
                    {
                        flag = true;
                        num = row + 1;
                        break;
                    }
                }
            }
            
            if (!flag)
                return false;
            
            AccessibleNode rowHeader = JabBaseActions.GetAccessibleNodeWithParameters(accessibleWindow, rowHeaderHelper, "push button", retries, "showing", true, parent);
            if (rowHeader == null)
                return false;

            var rowHeaderAci = JabBaseActions.GetAccessibleContextInfo(vmIdUfix, rowHeader);
            
            IScreenActions.Wait(1);
            IScreenActions.LeftMouseClickAt(rowHeaderAci.x + rowHeaderAci.width/5, rowHeaderAci.y + rowHeaderAci.height/2 + 36 + num * 18);
            IScreenActions.Wait(1);

            return true;
        }
        
        public bool ConfirmCancellation(string cancellationReasonCode, int retries)
        {
            var confirmCancellationPopup = JabBaseActions.GetAccessibleWindowWithPartialName(_accessBridge, "Confirm Cancellation", 10, out int vmIdConfirmCancellationPopup);
            if (confirmCancellationPopup == null)
                return false;
            
            var reasonLabel = JabBaseActions.GetAccessibleNodeWithParameters(confirmCancellationPopup, "Reason:", "label", retries, "showing", true);
            if (reasonLabel == null)
                return false;

            var reasonLabelAci = JabBaseActions.GetAccessibleContextInfo(_vmId, reasonLabel);
            
            var parent = reasonLabel.GetParent();

            var reasonField = JabBaseActions.GetChildFromParentNode(parent, reasonLabelAci.indexInParent + 1);
            
            if (!ClickButton(vmIdConfirmCancellationPopup, reasonField))
                return false;
            
            IScreenActions.Send(cancellationReasonCode);
            IScreenActions.Wait();
            IScreenActions.EnterWait();

            if (!ClickButton(confirmCancellationPopup, vmIdConfirmCancellationPopup, "OK", "push button", retries,"enabled"))
                return false;

            return true;
        }
    }
}