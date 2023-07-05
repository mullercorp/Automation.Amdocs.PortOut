using System;
using System.Collections.Generic;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.MainFlow;
using Automation.Amdocs.PortOut.Robot.SourceData;
using AutomationFramework;
using Shared.Utilities;

namespace Automation.Amdocs.PortOut.Robot
{
    public class PortOut : PortOutRequestPage
    {
        private readonly AccessibleWindow _accessibleWindow;
        private readonly int _vmId;
        private AccessBridge _accessBridge;
        private readonly ViewCasePage _viewCasePage = new ViewCasePage();

        public PortOut(AccessibleWindow accessibleWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _accessibleWindow = accessibleWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Run()
        {
            Logger.MethodEntry("PortOut.Run");

            DoWork();

            return true;
        }

        private void DoWork()
        {
            var viewCaseFrame = _viewCasePage.GetViewCaseFrame(_accessibleWindow, Json.Data.CaseNumber);
            if (viewCaseFrame == null)
                Logger.FinishFlowAsError($"GetTab View Case: {Json.Data.CaseNumber} Error", $"GetTabViewCase{Json.Data.CaseNumber}Error");

            if (!_viewCasePage.ClickPortOutRequestBtn(_accessibleWindow, _vmId, viewCaseFrame))
                Logger.FinishFlowAsError("Unable to click on Port Out Request btn. Please investigate.", "NoPortOutRequestBtn");

            var portOutFrame = GetPortOutRequestFrame(_accessibleWindow);
            if (portOutFrame == null)
                Logger.FinishFlowAsError("Unable to get Port Out Request frame.", "PortOutRequestFrameError");

            if (!SelectBlocking(_accessibleWindow, _vmId, portOutFrame, GlobalValues.PortOutBlocking))
                Logger.FinishFlowAsError("Select Blocking error.", "SelectBlockingError");

            if (GlobalValues.PortOutBlocking == Blocking.N)
            {
                DateTime proposedDate = DateTime.Parse(GlobalValues.ProposedDate);
                DateTime newProposedDate = FunctionsAssistant.ReturnFirstAvailableWorkday(proposedDate);
                if (proposedDate != newProposedDate)
                {
                    Logger.AddProcessLog($"Initial proposed Port out date: {proposedDate} changed to {newProposedDate} due to not being a workday");
                    GlobalValues.ProposedDate = newProposedDate.ToString("d-M-yyyy HH:mm");
                }
            }

            if (GlobalValues.PortOutBlocking == Blocking.J)
            {
                var blockingCodeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Blocking Code:", "push button", 2, containsValue: true, child: portOutFrame);
                var contextInfoBlockingHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, blockingCodeHeader);
                var centerCoordinatesBlocking = GetCenterCoordinatesOfNode(contextInfoBlockingHeader);
                IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesBlocking.Item1, centerCoordinatesBlocking.Item2 + 20);
                IScreenActions.ClearField();
                IScreenActions.Send(GlobalValues.PortOutBlockingCode);
                IScreenActions.TabWait();
            }
            else
            {
                if (GlobalValues.DaysBetweenProposedDateAndToday <= 120)
                {
                    if (GlobalValues.Flash == "NWCO")
                    {
                        var proposedDateTimeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Proposed Date and Time:", "push button", 2, containsValue: true, child: portOutFrame);
                        var contextInfoProposedDtHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, proposedDateTimeHeader);
                        var centerCoordinatesPropDt = GetCenterCoordinatesOfNode(contextInfoProposedDtHeader);
                        IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesPropDt.Item1, centerCoordinatesPropDt.Item2 + 20);
                        IScreenActions.ClearField();
                        IScreenActions.Send(GlobalValues.ProposedDate);
                        IScreenActions.TabWait();
                    }
                    else if (GlobalValues.Flash == "ETF")
                    {
                        var proposedDateTimeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Proposed Date and Time:", "push button", 2, containsValue: true, child: portOutFrame);
                        var contextInfoProposedDtHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, proposedDateTimeHeader);
                        var centerCoordinatesPropDt = GetCenterCoordinatesOfNode(contextInfoProposedDtHeader);
                        IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesPropDt.Item1, centerCoordinatesPropDt.Item2 + 20);
                        IScreenActions.ClearField();
                        IScreenActions.Send(GlobalValues.ProposedDate);
                        IScreenActions.TabWait();
                    }
                    else
                    {
                        if (GlobalValues.PendingCease)
                        {
                            var proposedDateTimeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Proposed Date and Time:", "push button", 2, containsValue: true, child: portOutFrame);
                            var contextInfoProposedDtHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, proposedDateTimeHeader);
                            var centerCoordinatesPropDt = GetCenterCoordinatesOfNode(contextInfoProposedDtHeader);
                            IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesPropDt.Item1, centerCoordinatesPropDt.Item2 + 20);
                            IScreenActions.ClearField();
                            IScreenActions.Send(GlobalValues.ProposedDate);
                            IScreenActions.TabWait();
                        }
                        else
                        {
                            if (GlobalValues.ProposedDateViaApiSuccessfully)
                            {
                                var proposedDateTimeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Proposed Date and Time:", "push button", 2, containsValue: true, child: portOutFrame);
                                var contextInfoProposedDtHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, proposedDateTimeHeader);
                                var centerCoordinatesPropDt = GetCenterCoordinatesOfNode(contextInfoProposedDtHeader);
                                IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesPropDt.Item1, centerCoordinatesPropDt.Item2 + 20);
                                IScreenActions.ClearField();
                                IScreenActions.Send(GlobalValues.ProposedDate);
                                IScreenActions.TabWait();
                            }
                            else
                                Logger.AddProcessLog("DaysBetweenProposedDateAndToday < 120, leave the filled in date as it is.");                        
                        }
                    }
                }
                else
                {
                    var proposedDateTimeHeader = JabBaseActions.GetAccessibleNodeWithParameters(_accessibleWindow, "Proposed Date and Time:", "push button", 2, containsValue: true, child: portOutFrame);
                    var contextInfoProposedDtHeader = JabBaseActions.GetAccessibleContextInfo(_vmId, proposedDateTimeHeader);
                    var centerCoordinatesPropDt = GetCenterCoordinatesOfNode(contextInfoProposedDtHeader);
                    IScreenActions.DoubleLeftMouseClickAt(centerCoordinatesPropDt.Item1, centerCoordinatesPropDt.Item2 + 20);
                    IScreenActions.ClearField();
                    IScreenActions.Send(GlobalValues.ProposedDate);
                    IScreenActions.TabWait();
                }
            }

            if (!ClickSubmitBtn(_accessibleWindow, _vmId, portOutFrame))
                Logger.FinishFlowAsError("Can't find the Submit btn.", "SubmitBtnError");

            IScreenActions.Wait(3);

            AccessibleWindow popupDialogWindow = GetPopupDialogWindow(_accessBridge, out int vmDialogId, out JavaObjectHandle javaObjectHandle);
            Logger.AddProcessLog($"popupDialogWindow.Hwnd: {popupDialogWindow.Hwnd}");

            List<AccessibleNode> labelsFromPopupDialogWindows = JabBaseActions.GetAccessibleNodesWithRole(popupDialogWindow, "label");
            Logger.AddProcessLog($"labelsFromPopupDialogWindows: {labelsFromPopupDialogWindows.Count}");
            foreach (AccessibleNode label in labelsFromPopupDialogWindows)
            {
                if (label == null) continue;
                Logger.AddProcessLog($"label: {label}");
                if (!JabBaseActions.GetAccessibleContextInfoAlternative(label, out var accessibleContextInfoAlternativeData))
                    Logger.FinishFlowAsError("Could not read the accessibleContextInfoAlternativeData. Please contact FLX.", "AccConInfoAltDataError");

                if (!accessibleContextInfoAlternativeData.Name.ToUpper().Contains("ALL PROPOSED DATES WILL BE DISCARDED")) continue;
                Logger.AddProcessLog("All proposed dates will be discarded, since the request is not blocked. Popup detected.");
                IScreenActions.EnterWait();
                //if(!ClickButton(popupDialogWindow, vmDialogId, "OK", "push button", 3))
                //Logger.FinishFlowAsError("Could not click on OK btn in Popup. Please investigate.","ClickOkBtnInPopupError");

                break;
            }

            AccessibleWindow messageWindow = JabBaseActions.GetAccessibleWindowWithRoleOptionalPartialName(_accessBridge, "dialog", 5, out var messageVmId, out var javaObjectHandle2, "Message");
            List<AccessibleNode> labelsFromPopupmessageWindow = JabBaseActions.GetAccessibleNodesWithRole(messageWindow, "label");
            Logger.AddProcessLog($"labelsFromPopupmessageWindow: {labelsFromPopupmessageWindow.Count}");
            bool reqSubmittedSuccessfullyPopup = false;
            foreach (AccessibleNode label in labelsFromPopupmessageWindow)
            {
                if (label == null) continue;
                Logger.AddProcessLog($"label: {label}");
                if (!JabBaseActions.GetAccessibleContextInfoAlternative(label, out var accessibleContextInfoAlternativeData))
                    Logger.FinishFlowAsError("Could not read the accessibleContextInfoAlternativeData. Please contact FLX.", "AccConInfoAltDataError");

                if (!accessibleContextInfoAlternativeData.Name.ToUpper().Contains("REQUEST SUBMITTED SUCCESSFULLY")) continue;
                Logger.AddProcessLog("REQUEST SUBMITTED SUCCESSFULLY. Popup detected.");
                IScreenActions.EnterWait();
                reqSubmittedSuccessfullyPopup = true;
                //if(!ClickButton(popupDialogWindow, vmDialogId, "OK", "push button", 3))
                //Logger.FinishFlowAsError("Could not click on OK btn in Popup. Please investigate.","ClickOkBtnInPopupError");

                break;
            }
            
            if(!reqSubmittedSuccessfullyPopup)
                Logger.HandleBusinessRule("Not detected 'Request Submitted Successfully' popup. Please Investigate.","UnableToPortOutWithSuccess");
        }
    }
}