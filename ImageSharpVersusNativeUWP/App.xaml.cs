using System.Reflection;
using Xunit.Runners.UI;

namespace UwpTestRunner
{
    sealed partial class App : RunnerApplication
    {
        protected override void OnInitializeRunner()
        {
            AddTestAssembly(GetType().GetTypeInfo().Assembly);
        }
    }
}