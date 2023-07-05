using System;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using AutomationFramework;
using JAB;
using AccessibleContextInfo = WindowsAccessBridgeInterop.AccessibleContextInfo;

namespace Automation.Amdocs.PortOut.Robot.Scripts.Ufix
{
    public class PendingCheck : SearchOrderPage
    {
        private readonly AccessibleWindow _accessibleWindow;
        private readonly int _vmId;
        private AccessBridge _accessBridge;
        private readonly ViewCasePage _viewCasePage = new ViewCasePage();
        
        public PendingCheck(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Run()
        {
            Logger.MethodEntry("PendingCheck.DoWork");

            return DoWork();
        }

        private bool DoWork()
        {
            if(!GoToSearchOrderingOrders(_accessibleWindow, _vmId))
                Logger.FinishFlowAsError("Unable to grab the layeredPane to open menu items. Please investigate", "UnableToNavigateViaMenu");
            
            if(!FillInCtnInServiceId(_accessibleWindow, _vmId, GlobalValues.CTN))
                Logger.FinishFlowAsError("TypeIntoTabField Error Service ID", "TypeIntoTabFieldServiceIdError");
            
            IScreenActions.EnterWait();
            
            var table = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "", "table", 5);
            if (table == null)
                Logger.FinishFlowAsError("Search Order Table Not Found", "SearchOrderTableNotFound");
            
            GlobalValues.Pending = CheckResultsAfterSearch(table, out var rowCount, out var columnCount);
            Logger.AddProcessLog($"Pending: {GlobalValues.Pending}");

            if (GlobalValues.Pending)
            {
                for (int row = 0; row < rowCount; ++row)
                {
                    JavaObjectHandle javaObjectHandle = Helpers.GetJavaObjectHandle(table);
                    AccessibleTableCellInfo tableCellInfo;
                    table.AccessBridge.Functions.GetAccessibleTableCellInfo(_vmId, javaObjectHandle, row, 1, out tableCellInfo);
                    JavaObjectHandle accessibleContext = tableCellInfo.accessibleContext;
                    AccessibleContextInfo info;
                    table.AccessBridge.Functions.GetAccessibleContextInfo(_vmId, accessibleContext, out info);
                    string tableCell = info.description;
                    Logger.AddProcessLog($"Table Value: {tableCell}");

                    if (tableCell.ToUpper().Contains("CEASE"))
                    {
                        Logger.AddProcessLog($"Pending Cease found");
                        GlobalValues.PendingCease = true;

                        //if (IScreenActions.WinWaitActiveImage("img_ExpandOrder", "region_ExpandOrder", true, 5))
                        //    IScreenActions.ClickAtCenterOfImage("img_ExpandOrder", "region_ExpandOrder");
                        
                        IScreenActions.LeftMouseClickAt("Position_ExpandOrder");
                        
                        table = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "", "table", 5);
                        if (table == null)
                            Logger.FinishFlowAsError("Search Order Table Not Found(2)", "SearchOrderTableNotFound(2)");
                        
                        if (!GetServiceRequireDate(table, out DateTime serviceRequireDate))
                            Logger.FinishFlowAsError("Fetching Service Require Date Error", "FetchServiceRequireDateError");

                        //MessageBox.Show($"proposed port out date: {GlobalValues.ProposedDate}");
                        //MessageBox.Show($"cease date: {serviceRequireDate}");
                        DateTime proposedPortOutDate = DateTime.Parse(GlobalValues.ProposedDate);
                        if (serviceRequireDate < proposedPortOutDate)
                        {
                            GlobalValues.ProposedDate = serviceRequireDate.ToString("d-M-yyyy HH:mm");
                            Logger.AddProcessLog($"New Proposed Port Out Date: {GlobalValues.ProposedDate}");
                            GlobalValues.ProposedDateChangedBecauseCease = true;
                            //MessageBox.Show($"new proposed port out date: {GlobalValues.ProposedDate}");
                        }
                        
                        new MainPage().ClickCloseSubScreenViaX();
                        return GlobalValues.Pending;
                    }
                }
            }

            new MainPage().ClickCloseSubScreenViaX();           
            return GlobalValues.Pending;
        }
        
        public bool GetServiceRequireDate(AccessibleNode table, out DateTime srd)
        {
            string serviceRequiredDate = JabBaseActions.GetTableCellValue(_vmId, table, 1, 6);
            srd = DateTime.ParseExact(serviceRequiredDate, "d-M-yyyy", (IFormatProvider) null);
            
            return true;
        }
    }
}

























