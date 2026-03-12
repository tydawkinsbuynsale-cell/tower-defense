using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Analytics;

namespace RobotTD.Editor
{
    /// <summary>
    /// Editor window for viewing and testing analytics events in realtime.
    /// </summary>
    public class AnalyticsDashboard : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool trackEnabled = true;
        private bool autoScroll = true;
        private int maxDisplayEvents = 100;

        private List<AnalyticsEventData> eventHistory = new List<AnalyticsEventData>();
        private Dictionary<string, int> eventCounts = new Dictionary<string, int>();
        private Dictionary<string, double> sessionStats = new Dictionary<string, double>();

        private GUIStyle headerStyle;
        private GUIStyle eventStyle;
        private GUIStyle paramStyle;
        private GUIStyle statsStyle;

        private enum DisplayMode
        {
            RealtimeEvents,
            EventCounts,
            SessionInfo
        }
        private DisplayMode currentMode = DisplayMode.RealtimeEvents;

        private class AnalyticsEventData
        {
            public string timestamp;
            public string eventName;
            public Dictionary<string, object> parameters;

            public AnalyticsEventData(string name, Dictionary<string, object> param)
            {
                timestamp = System.DateTime.Now.ToString("HH:mm:ss");
                eventName = name;
                parameters = param ?? new Dictionary<string, object>();
            }
        }

        [MenuItem("Tools/Robot TD/Analytics Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnalyticsDashboard>("Analytics Dashboard");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // Hook into analytics events (you'd need to add this event to AnalyticsManager)
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            // Poll for new events (in real implementation, AnalyticsManager would fire events)
            if (Application.isPlaying && AnalyticsManager.Instance != null)
            {
                // Update session stats
                UpdateSessionStats();
            }
        }

        private void UpdateSessionStats()
        {
            if (AnalyticsManager.Instance == null) return;

            sessionStats["Session Number"] = AnalyticsManager.Instance.GetSessionNumber();
            sessionStats["Total Sessions"] = AnalyticsManager.Instance.GetTotalSessions();
            sessionStats["Total Play Time"] = AnalyticsManager.Instance.GetTotalPlayTimeSeconds();
            sessionStats["Session Duration"] = AnalyticsManager.Instance.GetSessionDurationSeconds();
            sessionStats["Is New User"] = AnalyticsManager.Instance.IsNewUser() ? 1 : 0;
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (eventStyle == null)
            {
                eventStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(5, 5, 2, 2)
                };
            }

            if (paramStyle == null)
            {
                paramStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 10,
                    wordWrap = true
                };
            }

            if (statsStyle == null)
            {
                statsStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    padding = new RectOffset(10, 10, 5, 5)
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Analytics Dashboard", headerStyle);
            EditorGUILayout.Space(10);

            // Toolbar
            DrawToolbar();

            EditorGUILayout.Space(5);

            // Main content area
            switch (currentMode)
            {
                case DisplayMode.RealtimeEvents:
                    DrawRealtimeEvents();
                    break;
                case DisplayMode.EventCounts:
                    DrawEventCounts();
                    break;
                case DisplayMode.SessionInfo:
                    DrawSessionInfo();
                    break;
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Mode selection
            if (GUILayout.Toggle(currentMode == DisplayMode.RealtimeEvents, "Realtime", EditorStyles.toolbarButton))
                currentMode = DisplayMode.RealtimeEvents;
            if (GUILayout.Toggle(currentMode == DisplayMode.EventCounts, "Event Counts", EditorStyles.toolbarButton))
                currentMode = DisplayMode.EventCounts;
            if (GUILayout.Toggle(currentMode == DisplayMode.SessionInfo, "Session Info", EditorStyles.toolbarButton))
                currentMode = DisplayMode.SessionInfo;

            GUILayout.FlexibleSpace();

            // Controls
            trackEnabled = GUILayout.Toggle(trackEnabled, "Track", EditorStyles.toolbarButton);
            autoScroll = GUILayout.Toggle(autoScroll, "Auto-Scroll", EditorStyles.toolbarButton);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                ClearHistory();
            }

            if (GUILayout.Button("Test Event", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SendTestEvent();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRealtimeEvents()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField($"Events: {eventHistory.Count} (max: {maxDisplayEvents})", EditorStyles.miniLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (eventHistory.Count == 0)
            {
                EditorGUILayout.HelpBox("No events tracked yet. Events will appear here during play mode.", MessageType.Info);
            }
            else
            {
                for (int i = eventHistory.Count - 1; i >= 0; i--)
                {
                    DrawEvent(eventHistory[i]);
                }
            }

            EditorGUILayout.EndScrollView();

            if (autoScroll && Event.current.type == EventType.Repaint)
            {
                scrollPosition.y = float.MaxValue;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEvent(AnalyticsEventData eventData)
        {
            EditorGUILayout.BeginVertical(eventStyle);

            // Event header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{eventData.timestamp}]", GUILayout.Width(70));
            EditorGUILayout.LabelField(eventData.eventName, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // Parameters
            if (eventData.parameters.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var kvp in eventData.parameters)
                {
                    EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value}", paramStyle);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEventCounts()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField($"Unique Events: {eventCounts.Count}", EditorStyles.miniLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (eventCounts.Count == 0)
            {
                EditorGUILayout.HelpBox("No event counts yet. Track some events first!", MessageType.Info);
            }
            else
            {
                // Sort by count descending
                var sorted = eventCounts.OrderByDescending(kvp => kvp.Value);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var kvp in sorted)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(250));
                    EditorGUILayout.LabelField(kvp.Value.ToString(), EditorStyles.boldLabel, GUILayout.Width(50));
                    
                    // Draw simple bar graph
                    float maxCount = eventCounts.Values.Max();
                    float normalized = (float)kvp.Value / maxCount;
                    Rect barRect = GUILayoutUtility.GetRect(normalized * 200, 18);
                    EditorGUI.DrawRect(barRect, new Color(0.3f, 0.7f, 1f, 0.5f));
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export CSV", GUILayout.Height(25)))
            {
                ExportEventCountsToCSV();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSessionInfo()
        {
            EditorGUILayout.BeginVertical();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see session information.", MessageType.Info);
            }
            else if (AnalyticsManager.Instance == null)
            {
                EditorGUILayout.HelpBox("AnalyticsManager not found in scene.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Session Information", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                DrawStatRow("Session ID", AnalyticsManager.Instance.GetSessionId());
                DrawStatRow("Session Number", AnalyticsManager.Instance.GetSessionNumber().ToString());
                DrawStatRow("Total Sessions", AnalyticsManager.Instance.GetTotalSessions().ToString());
                
                int playTime = AnalyticsManager.Instance.GetTotalPlayTimeSeconds();
                DrawStatRow("Total Play Time", FormatTime(playTime));
                
                int sessionTime = AnalyticsManager.Instance.GetSessionDurationSeconds();
                DrawStatRow("Session Duration", FormatTime(sessionTime));
                
                DrawStatRow("New User", AnalyticsManager.Instance.IsNewUser() ? "Yes" : "No");

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);

                // Session actions
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Print Full Info", GUILayout.Height(25)))
                {
                    var info = AnalyticsManager.Instance.GetSessionInfo();
                    foreach (var kvp in info)
                    {
                        Debug.Log($"{kvp.Key}: {kvp.Value}");
                    }
                }

                if (GUILayout.Button("Force New Session", GUILayout.Height(25)))
                {
                    Debug.LogWarning("Force new session not implemented (would require restart)");
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private string FormatTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            if (hours > 0)
                return $"{hours}h {minutes}m {secs}s";
            else if (minutes > 0)
                return $"{minutes}m {secs}s";
            else
                return $"{secs}s";
        }

        private void ClearHistory()
        {
            eventHistory.Clear();
            eventCounts.Clear();
            Repaint();
        }

        private void SendTestEvent()
        {
            if (Application.isPlaying && AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("test_event", new Dictionary<string, object>
                {
                    { "test_param", "test_value" },
                    { "random_number", UnityEngine.Random.Range(1, 100) },
                    { "timestamp", System.DateTime.Now.ToString() }
                });

                Debug.Log("Test analytics event sent");
            }
            else
            {
                Debug.LogWarning("Enter play mode first to send test events");
            }
        }

        private void ExportEventCountsToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Event Counts", "", "event_counts.csv", "csv");
            
            if (!string.IsNullOrEmpty(path))
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Event Name,Count");
                
                foreach (var kvp in eventCounts.OrderByDescending(k => k.Value))
                {
                    csv.AppendLine($"{kvp.Key},{kvp.Value}");
                }
                
                System.IO.File.WriteAllText(path, csv.ToString());
                Debug.Log($"Event counts exported to: {path}");
            }
        }

        // ── Public API for AnalyticsManager to call ───────────────────────────

        /// <summary>
        /// Call this from AnalyticsManager when an event is tracked.
        /// Add to AnalyticsManager.TrackEvent():
        /// #if UNITY_EDITOR
        /// AnalyticsDashboard.OnEventTracked(eventName, parameters);
        /// #endif
        /// </summary>
        public static void OnEventTracked(string eventName, Dictionary<string, object> parameters)
        {
            var window = GetWindow<AnalyticsDashboard>(false, "Analytics Dashboard", false);
            if (window != null && window.trackEnabled)
            {
                window.AddEvent(eventName, parameters);
            }
        }

        private void AddEvent(string eventName, Dictionary<string, object> parameters)
        {
            // Add to history
            eventHistory.Add(new AnalyticsEventData(eventName, parameters));
            
            // Trim if exceeds max
            if (eventHistory.Count > maxDisplayEvents)
            {
                eventHistory.RemoveAt(0);
            }

            // Update counts
            if (!eventCounts.ContainsKey(eventName))
                eventCounts[eventName] = 0;
            eventCounts[eventName]++;

            Repaint();
        }
    }
}
