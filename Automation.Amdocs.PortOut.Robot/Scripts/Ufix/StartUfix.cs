using System;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.PortOut.Robot.Wrappers;
using AutomationFramework;
using AutomationFramework.DAO;

namespace Automation.Amdocs.PortOut.Robot.Scripts.UFix
{
    public class StartUfix : CoreWrappers
    {
        private UfixBase _ufixBase = new UfixBase();
        private MainPage _mainPage = new MainPage();
        private MainPageJab _mainPageJab = new MainPageJab();
        private UfixBaseJab _ufixBaseJab = new UfixBaseJab();

        public bool Execute()
        {
            Logger.MethodEntry("StartUfix.Execute");

            IScreenActions.Wait();

            try
            {
                bool alreadyLoggedIn = false;
                if (_ufixBase.OpenUfix(AssetsDAO.GetAssetByName("UfixFileLocation"), AssetsDAO.GetAssetByName("UfixExeName"), out alreadyLoggedIn, defaultTries: 40, defaultWaitTime:20) == false)
                {
                    Logger.AddProcessLog("Unable to startup Ufix. Please analyse.");
                    return false;
                }

                var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
                if (alreadyLoggedIn == false)
                {
                    if (_ufixBase.LoginUfix(credentials.Username, credentials.Password, AssetsDAO.GetAssetByName("Ufix_J63_Role")) == false)
                    {
                        Logger.AddProcessLog("Unable to login Ufix. Please analyse.");
                        return false;
                    }
                    
                    IScreenActions.Wait(2);
                    if (!_mainPageJab.AccessUfix(out var accessBridge, out var ufixMainWindow, out var vmIdUfix, out var acUfix))
                        return false;

                    accessBridge.Dispose();    
                }
                else
                {
                    _mainPage.ClickCloseSubScreenViaX(5);

                    try
                    {
                        if (!_ufixBaseJab.AccessUfix(out var accessBridge, out var ufixMainWindow, out var vmIdUfix, out var acUfix))
                            return false;
                        //Logger.FinishFlowAsError("AccessUfix Error");

                        if (_mainPageJab.MainDesktopPaneCleared(ufixMainWindow, vmIdUfix) == false)
                            return false;
                        
                        accessBridge.Dispose();
                    }
                    catch (Exception e)
                    {
                        Logger.AddProcessLog(e.ToString());
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.AddProcessLog(e.ToString());
                return false;
            }

            Logger.MethodExit("StartUfix.Execute");
            return true;
        }
    }
}