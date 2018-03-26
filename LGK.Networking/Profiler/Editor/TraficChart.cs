// See LICENSE file in the root directory
//

using UnityEditor;
using UnityEngine;

namespace LGK.Networking.Profiler
{
    public class TraficChart
    {
        public const byte BAR_WIDTH = 10;

        private Material m_Material;
        private Rect m_ChartRect;

        private NetworkTraficBuffer m_IncomingBuffer;
        private Color m_IncomingColor;

        private NetworkTraficBuffer m_OutgoingBuffer;
        private Color m_OutgoingColor;

        private Color m_HelpColor;
        private Color m_SelectedColor;

        public TraficChart(NetworkTraficBuffer incomingBuffer, Color incomingColor, NetworkTraficBuffer outgoingBuffer, Color outgointColor, Color helpColor, Color selectedColor)
        {
            m_IncomingBuffer = incomingBuffer;
            m_IncomingColor = incomingColor;
            m_OutgoingBuffer = outgoingBuffer;
            m_OutgoingColor = outgointColor;

            m_HelpColor = helpColor;
            m_SelectedColor = selectedColor;
        }

        public int CheckIndexFromRight(Vector2 mousePosition)
        {
            if (mousePosition.y < m_ChartRect.y || mousePosition.y > m_ChartRect.y + m_ChartRect.height)
                return -1;

            var index = (int)((m_ChartRect.x + m_ChartRect.width - mousePosition.x) / BAR_WIDTH);

            if (index >= 0)
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        public void Draw(int selectedIndexFromRight)
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("Hidden/Internal-Colored"));

            GUILayout.Label("Total Sample :" + m_IncomingBuffer.Count);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            m_ChartRect = EditorGUILayout.GetControlRect(GUILayout.Height(150));

            m_ChartRect.y += 15;
            m_ChartRect.height -= 15;

            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                var highestIncome = GetHighestValue(m_IncomingBuffer);
                var highestOutgoing = GetHighestValue(m_OutgoingBuffer);

                var highest = (highestIncome + highestOutgoing) * 1f;

                m_Material.SetPass(0);

                GL.PushMatrix();
                GL.LoadPixelMatrix();

                for (int h = 0; h < m_IncomingBuffer.Count; h++)
                {
                    var frameIndex = m_IncomingBuffer.Count - h - 1;
                    var incomingValue = m_IncomingBuffer[frameIndex].TotalChannelTrafic;
                    var persentageIncoming = (incomingValue / highest);

                    var outgoingValue = m_OutgoingBuffer[frameIndex].TotalChannelTrafic + incomingValue;
                    var persentageOutgoing = (outgoingValue / highest);

                    if (!DrawQuad(h, BAR_WIDTH, persentageOutgoing, m_OutgoingColor))
                    {
                        break;
                    }

                    if (!DrawQuad(h, BAR_WIDTH, persentageIncoming, m_IncomingColor))
                    {
                        break;
                    }
                }

                DrawIndicatorLine(0f, m_HelpColor);
                DrawIndicatorLine(0.25f, m_HelpColor);
                DrawIndicatorLine(0.5f, m_HelpColor);
                DrawIndicatorLine(0.75f, m_HelpColor);
                DrawIndicatorLine(1f, m_HelpColor);

                if (selectedIndexFromRight >= 0)
                {
                    DrawSelection(selectedIndexFromRight, BAR_WIDTH, m_SelectedColor);
                }

                GL.PopMatrix();

                DrawIndicatorLabels(highest);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawIndicatorLabels(float highest)
        {
            int zeroY = (int)(m_ChartRect.y + (0 * m_ChartRect.height));
            int additional1 = (int)(m_ChartRect.y + (0.25 * m_ChartRect.height));
            int halfY = (int)(m_ChartRect.y + (0.5f * m_ChartRect.height));
            int additional2 = (int)(m_ChartRect.y + (0.75 * m_ChartRect.height));
            int fullY = (int)(m_ChartRect.y + (1f * m_ChartRect.height));

            byte labelHeight = 15;
            byte labelWidth = 100;
            GUI.Label(new Rect(m_ChartRect.x, zeroY - labelHeight, labelWidth, labelHeight), (highest).ToString() + " bytes");
            GUI.Label(new Rect(m_ChartRect.x, additional1 - labelHeight, labelWidth, labelHeight), ((int)(0.75f * highest)).ToString() + " bytes");
            GUI.Label(new Rect(m_ChartRect.x, halfY - labelHeight, labelWidth, labelHeight), ((int)(0.5f * highest)).ToString() + " bytes");
            GUI.Label(new Rect(m_ChartRect.x, additional2 - labelHeight, labelWidth, labelHeight), ((int)(0.25f * highest)).ToString() + " bytes");
            GUI.Label(new Rect(m_ChartRect.x, fullY - labelHeight, labelWidth, labelHeight), "0 bytes");

        }

        void DrawIndicatorLine(float heightPercentace, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            var bottomY = m_ChartRect.y + m_ChartRect.height;

            var x1 = m_ChartRect.x;
            var x2 = m_ChartRect.x + m_ChartRect.width;

            var y = bottomY - (heightPercentace * m_ChartRect.height);
            var z = 0;

            GL.Vertex3(x1, y, z);
            GL.Vertex3(x2, y, 0);

            GL.End();
        }

        bool DrawQuad(int index, int width, float heightPercentace, Color color)
        {

            var rightX = m_ChartRect.x + m_ChartRect.width;
            var bottomY = m_ChartRect.y + m_ChartRect.height;

            var x1 = rightX - (index * width);
            if (x1 < 0)
                return false;

            var x2 = x1 - width;
            if (x2 < 0)
                x2 = 0;

            GL.Begin(GL.QUADS);
            GL.Color(color);

            var y1 = bottomY;
            var y2 = y1 - (heightPercentace * m_ChartRect.height);
            var z = 0;

            GL.Vertex3(x1, y1, z);
            GL.Vertex3(x2, y1, z);
            GL.Vertex3(x2, y2, z);
            GL.Vertex3(x1, y2, z);

            GL.End();

            return true;
        }

        void DrawSelection(int index, int width, Color color)
        {
            DrawQuad(index, width, 1, color);
        }

        ushort GetHighestValue(NetworkTraficBuffer values)
        {
            ushort highest = 10;

            for (int i = 0; i < values.Count; i++)
            {
                if (highest < values[i].TotalChannelTrafic)
                {
                    highest = values[i].TotalChannelTrafic;
                }
            }

            return highest;
        }
    }
}

