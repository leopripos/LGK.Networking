// See LICENSE file in the root directory
//

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace LGK.Networking.Profiler
{
    public class NetworkProfilerWindow : EditorWindow
    {
        public const byte BAR_WIDTH = 20;

        [SerializeField]
        private int m_SelectedFromRightIndex;
        [SerializeField]
        private bool m_IsActive;
        [SerializeField]
        private bool m_CollapseIcomningDetail;
        [SerializeField]
        private bool m_CollapseOutgoingDetail;

        [SerializeField]
        public NetworkTraficBuffer m_FoldoutIncomingTrafic;
        [SerializeField]
        private NetworkTraficBuffer m_FoldoutOutgoingTrafic;

        static NavigationMenu m_NavigationMenu;
        static TraficChart m_TraficChart;
        static TraficDetail m_IncomingTraficDetail;
        static TraficDetail m_OutgoingTraficDetail;

        private void Awake()
        {
            if (m_IsActive)
            {
                ProfilerAction.Enable();
            }
            else
            {
                ProfilerAction.Disable();
            }
        }

        private void OnEnable()
        {
            ProfilerAction.SetupDatabase(m_FoldoutIncomingTrafic, m_FoldoutOutgoingTrafic);

            EditorApplication.playModeStateChanged += HandlePlayMoveChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayMoveChanged;
        }

        [NonSerialized]
        NetworkProfilerRunner m_Runner;
        private void HandlePlayMoveChanged(PlayModeStateChange obj)
        {
            if (EditorApplication.isPlaying && m_Runner == null)
            {
                m_Runner = new GameObject("Network Profiler Runner").AddComponent<NetworkProfilerRunner>();

                m_Runner.StartCoroutine(CaptureFrame());
            }
            else if (!EditorApplication.isPlaying && m_Runner != null)
            {
                UnityEngine.Object.Destroy(m_Runner.gameObject);
            }
        }

        [NonSerialized]
        WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();
        IEnumerator CaptureFrame()
        {
            while (EditorApplication.isPlaying)
            {
                yield return m_WaitForEndOfFrame;

                if (!EditorApplication.isPaused)
                    ProfilerAction.Capture();
            }
        }

        void OnGUI()
        {
            CheckPreparation();

            m_NavigationMenu.Draw(ref m_IsActive);
            m_TraficChart.Draw(m_SelectedFromRightIndex);


            var lastIndex = m_FoldoutIncomingTrafic.Count - 1;

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                m_IncomingTraficDetail.Draw(ref m_CollapseIcomningDetail, position.width, lastIndex >= 0 ? m_FoldoutIncomingTrafic[lastIndex] : null);
                m_OutgoingTraficDetail.Draw(ref m_CollapseOutgoingDetail, position.width, lastIndex >= 0 ? m_FoldoutOutgoingTrafic[lastIndex] : null);
            }
            else
            {
                if (m_SelectedFromRightIndex >= 0 && m_SelectedFromRightIndex <= lastIndex)
                {
                    var realSelectedIndex = lastIndex - m_SelectedFromRightIndex;
                    m_IncomingTraficDetail.Draw(ref m_CollapseIcomningDetail, position.width, m_FoldoutIncomingTrafic[realSelectedIndex]);
                    m_OutgoingTraficDetail.Draw(ref m_CollapseOutgoingDetail, position.width, m_FoldoutOutgoingTrafic[realSelectedIndex]);
                }
                else
                {
                    m_IncomingTraficDetail.Draw(ref m_CollapseIcomningDetail, position.width, null);
                    m_OutgoingTraficDetail.Draw(ref m_CollapseOutgoingDetail, position.width, null);
                }
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp)
            {
                if (EditorApplication.isPlaying) { EditorApplication.isPaused = true; }

                m_SelectedFromRightIndex = m_TraficChart.CheckIndexFromRight(Event.current.mousePosition);
            }
        }

        private void Update()
        {
            if (m_IsActive && EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                ProfilerAction.Capture();
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void CheckPreparation()
        {
            if (m_FoldoutIncomingTrafic == null)
                m_FoldoutIncomingTrafic = new NetworkTraficBuffer();

            if (m_FoldoutOutgoingTrafic == null)
                m_FoldoutOutgoingTrafic = new NetworkTraficBuffer();

            if (m_NavigationMenu == null)
                m_NavigationMenu = new NavigationMenu();

            if (m_TraficChart == null)
                m_TraficChart = new TraficChart(m_FoldoutIncomingTrafic, Color.red, m_FoldoutOutgoingTrafic, Color.blue, Color.grey, new Color(1, 1, 1, 0.5f));

            if (m_IncomingTraficDetail == null)
                m_IncomingTraficDetail = new TraficDetail("Incoming By Channel");

            if (m_OutgoingTraficDetail == null)
                m_OutgoingTraficDetail = new TraficDetail("Outgoing By Channel");
        }

        int CheckSelectedFrame(Rect layoutRectangle, int frameCount, Vector2 mousePosition)
        {
            if (mousePosition.y < layoutRectangle.y || mousePosition.y > layoutRectangle.y + layoutRectangle.height)
                return m_SelectedFromRightIndex;

            var index = (int)((layoutRectangle.x + layoutRectangle.width - mousePosition.x) / BAR_WIDTH);

            if (index < frameCount && index >= 0)
            {
                return index;
            }
            else
            {
                return -1;
            }
        }
    }
}
