using System.Reflection;
using Foundation;
using UIKit;
using Xunit.Runner;
using Xunit.Sdk;

namespace ImageSharpVersusNativeiOS
{
	[Register("AppDelegate")]
    public class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            AddTestAssembly(Assembly.GetExecutingAssembly());

			return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}

