using Kitchen;
using Shapes;
using UnityEngine;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Unity.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using KitchenMods;

namespace PlateUpPlannerIntegration
{
    public class ImportGUIManager : MonoBehaviour
    {

        private bool _show;
        private static readonly int WindowId = nameof(PlateUpPlannerIntegration).GetHashCode();
        public static float Scale { get; private set; } = 1f;

        public void Show()
        {
            Mod.LogInfo("show has started");
            _show = true;
            Mod.LogInfo(_show);
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _show = false;
            }
        }

        private static string _importStatus = "";
         public static void SetStatus(string newStatus)
        {
            _importStatus = newStatus;
        }
        private static string GetStatus()
        {
            if (_importStatus == "")
            {
                return "";
            }
            else if (_importStatus == "None")
            {
                return "All items in planner are also in plateup game. You can safely import";
            }
            else
            {
                return _importStatus;
            }
        }
        private Rect CalculateWindowRect()
        {
            Scale = Mathf.Max(1f, 0.65f * Screen.height / 500);

            var width = Mathf.Min(Screen.width, 650);
            var height = Mathf.Min(Screen.height, 475);
            var offsetX = Mathf.RoundToInt((Screen.width - width) / 2f);
            var offsetY = Mathf.RoundToInt((Screen.height - height) / 2f);

            return new Rect(offsetX, offsetY, width, height);
        }
        private void OnGUI()
        {
            Mod.LogInfo("ONGUI has started");
            if (_show == true)
            {
                Mod.LogInfo("SHOW is true");
                var windowRect = CalculateWindowRect();

                GUIUtility.ScaleAroundPivot(new Vector2(Scale, Scale), new Vector2(Screen.width / 2f, Screen.height / 2f));

                var backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1));
                backgroundTexture.Apply();

                var guiStyle = GUI.skin.window;
                guiStyle.normal.background = backgroundTexture;

                GUI.Box(windowRect, GUIContent.none, new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } });
                GUILayout.Window(WindowId, windowRect, Window, "Planner Integration - Import Options (ESC to close)");
                GUI.FocusWindow(WindowId);
            }
        }
        private static void Window(int WindowId)
        {
            Draw();
        }
        private static GUIStyle _headerStyle;
        private static Vector2 _scrollPos;
        private static string _newLayoutString = "";

        public static void Draw()
        {
            GUILayout.BeginVertical();

            _headerStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                stretchWidth = true,
                fontSize = 20
            };
            GUILayout.Label("PlateUp Planner Integration Import Menu", _headerStyle, GUILayout.ExpandWidth(true));

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.Label("Copy your entire planner link in the text area below (https://plateupplanner.github.io/workspace#..._:");

            var newLayoutString = GUILayout.TextArea(_newLayoutString, GUILayout.Height(100));
            var hasChanges = newLayoutString != _newLayoutString;
            _newLayoutString = newLayoutString;

            var style = new GUIStyle(GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Check if current layout has enough appliances for planner link", GUILayout.Width(350));
            if(GUILayout.Button("Import Check", GUILayout.ExpandWidth(true)))
            {
                LayoutImporter.RequestImportCheck();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Static imports will import without any smart grabber/teleporter configuration, and IMPORT CHECKS ARE REQUIRED. **If no button appears, first do an import check.", GUILayout.Width(350));
            GUILayout.Button("Static Import", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Import Status: ");
            GUILayout.Label(GetStatus());
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}