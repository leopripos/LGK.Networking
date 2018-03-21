// See LICENSE file in the root directory
//

using UnityEditor;

namespace LGK.Networking.Profiler
{
    public static class NetworkingMenuItem
    {
        [MenuItem("Tools/Networking/Profiler Window")]
        public static void ShowProfilerWindow()
        {
            var window = EditorWindow.GetWindow<NetworkProfilerWindow>("Network Profiler (LGK)");
            window.Show();
        }

#if NETWORK_PROFILER_ENABLED
        [MenuItem("Tools/Networking/Disable Profiler")]
        public static void DisableProfiler()
        {
            var symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            symbol = symbol.Replace(";NETWORK_PROFILER_ENABLED", "");
            symbol = symbol.Replace("NETWORK_PROFILER_ENABLED", "");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
        }
#else
        [MenuItem("Tools/Networking/Enable Profiler")]
        public static void EnableProfiler()
        {
            var symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            symbol += ";NETWORK_PROFILER_ENABLED";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
        }
#endif

#if NETWORK_DEBUGGER_ENABLED
        [MenuItem("Tools/Networking/Disable Debug")]
        public static void DisableDebugger()
        {
            var symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            symbol = symbol.Replace(";NETWORK_DEBUGGER_ENABLED", "");
            symbol = symbol.Replace("NETWORK_DEBUGGER_ENABLED", "");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
        }
#else
        [MenuItem("Tools/Networking/Enable Debug")]
        public static void EnableDebugger()
        {
            var symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            symbol += ";NETWORK_DEBUGGER_ENABLED";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
        }
#endif
    }
}

