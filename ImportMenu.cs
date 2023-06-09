﻿using Kitchen;
using Shapes;
using UnityEngine;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Unity.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using KitchenMods;
using System.Linq;

namespace PlateUpPlannerIntegration
{
    public class ImportGUIManager : MonoBehaviour
    {

        private bool _show;
        private static readonly int WindowId = nameof(PlateUpPlannerIntegration).GetHashCode();
        public static float Scale { get; private set; } = 1f;

        public void Show()
        {
            _show = true;
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _show = false;
            }
        }

        private static string _importStatus = "Import status will appear here";
         public static void SetStatus(string newStatus)
        {
            _importStatus = newStatus;
        }
        public static string GetStatus()
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
            if (_show == true)
            {
                var windowRect = CalculateWindowRect();

                GUIUtility.ScaleAroundPivot(new Vector2(Scale, Scale), new Vector2(Screen.width / 2f, Screen.height / 2f));

                var backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1));
                backgroundTexture.Apply();

                var guiStyle = new GUIStyle(GUI.skin.window);
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

        public static string GetLayoutString()
        {
            return _newLayoutString;
        }

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
            GUILayout.Label("PlateUp Planner Integration Menu", _headerStyle, GUILayout.ExpandWidth(true));

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Current", GUILayout.ExpandWidth(true)))
            {
                LayoutExporter.RequestExport();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Copy your entire planner link in the text area below (https://plateupplanner.github.io/workspace#...");

            var newLayoutString = GUILayout.TextArea(_newLayoutString, GUILayout.Height(100));
            _newLayoutString = newLayoutString;

            var style = new GUIStyle(GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Check if current layout has enough appliances for planner link", GUILayout.Width(350));
            if (GetLayoutString() != "")
            {
                if (GUILayout.Button("Import Check", GUILayout.ExpandWidth(true)))
                {
                    LayoutImporter.RequestImportCheck();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Import checks are required to use the import feature. Make sure if you have changed the planner URL or in-game appliances, you do another import check. This will make sure you do not spawn in extra appliances; if this is your goal(a creative mode of sorts), try a static import", GUILayout.Width(350));
            if (LayoutImporter.GetImportCheckStatus() == true)
            {
                if (GUILayout.Button("Import", GUILayout.ExpandWidth(true)))
                {
                    LayoutImporter.RequestImport();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Import Status: ");
            GUILayout.Label(GetStatus());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Static Imports do not require an import check, which means they spawn items in without regard to your current resturaunt. WARNING: this is similar to using creative mode and is NOT REVERSIBLE");
            if (GetLayoutString() != "")
            {
                if (GUILayout.Button("Static Import", GUILayout.ExpandWidth(true)))
                {
                    LayoutImporter.RequestStaticImport();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}