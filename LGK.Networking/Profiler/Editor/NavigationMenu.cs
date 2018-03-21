// See LICENSE file in the root directory
//

using UnityEditor;
using UnityEngine;

namespace LGK.Networking.Profiler
{
    public class NavigationMenu
    {
        public void Draw(ref bool isActive)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            isActive = GUILayout.Toggle(isActive, "Enable", EditorStyles.toolbarButton, GUILayout.MaxWidth(100));
            if (isActive)
            {
                ProfilerAction.Enable();
            }
            else
            {
                ProfilerAction.Disable();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                ProfilerAction.Clear();

            EditorGUILayout.EndHorizontal();
        }
    }
}

