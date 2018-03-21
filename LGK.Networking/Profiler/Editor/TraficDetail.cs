// See LICENSE file in the root directory
//

using UnityEditor;
using UnityEngine;

namespace LGK.Networking.Profiler
{
    public class TraficDetail
    {
        readonly string m_Label;

        public TraficDetail(string label)
        {
            m_Label = label;
        }

        public void Draw(ref bool foldout, float width, NetworkTrafic trafic)
        {
            var cellWidth = (width - 20) / 6f;

            foldout = EditorGUILayout.Foldout(foldout, m_Label);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                GUILayout.Label("By Channel", EditorStyles.boldLabel); ;
                DrawChannel(cellWidth, trafic);
                GUILayout.Label("By Message", EditorStyles.boldLabel); ;
                DrawMessage(cellWidth, trafic);
            }
            EditorGUI.indentLevel--;
        }

        void DrawMessage(float cellWidth, NetworkTrafic trafic)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Id", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label("Total\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Avg\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Min\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Max\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            if (trafic != null && trafic.MessageTrafics.Count > 0)
            {
                var channelTrafics = trafic.MessageTrafics;

                foreach (var item in channelTrafics)
                {
                    var info = item.Value;

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label(info.Category, GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Count.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Total.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Avg.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Min.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Max.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                string empty = "-";
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        void DrawChannel(float cellWidth, NetworkTrafic trafic)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Id", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label("Total\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Avg\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Min\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.Label(" Max\n(byte)", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            if (trafic != null && trafic.ChannelTrafics.Count > 0)
            {
                var channelTrafics = trafic.ChannelTrafics;

                foreach (var item in channelTrafics)
                {
                    var info = item.Value;

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label(info.Category, GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Count.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Total.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Avg.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Min.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.Label(info.Max.ToString(), GUILayout.Width(cellWidth));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                string empty = "-";
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.Label(empty, GUILayout.Width(cellWidth));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}

