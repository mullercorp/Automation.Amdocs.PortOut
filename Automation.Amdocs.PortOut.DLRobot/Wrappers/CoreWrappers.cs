using JAB;
using Shared.AutoIt;

namespace Automation.Amdocs.PortOut.DLRobot.Wrappers
{
    public class CoreWrappers
    {
        public static IScreenActions IScreenActions
        {
            get
            {
                IScreenActions screen = new ScreenActions();
                return screen;
            }
        }
        
        public static JabBase JabBaseActions
        {
            get
            {
                JabBase jabBase = new JabBase();
                return jabBase;
            }
        }
    }
}