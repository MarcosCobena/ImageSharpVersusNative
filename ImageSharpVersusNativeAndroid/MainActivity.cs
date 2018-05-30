using System.Reflection;
using Android.App;
using Android.OS;
using Xunit.Runners.UI;
using Xunit.Sdk;

namespace ImageSharpVersusNativeAndroid
{
	[Activity(Label = "ImageSharpVersusNativeAndroid", MainLauncher = true)]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            AddTestAssembly(Assembly.GetExecutingAssembly());
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            
			base.OnCreate(bundle);
        }
    }
}

