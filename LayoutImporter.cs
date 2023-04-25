using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace PlateUpPlannerIntegration
{
    public class LayoutImporter : RestaurantSystem, IModSystem
    {
        protected struct SImportRequest : IComponentData, IModComponent { }
        protected struct SImportCheckRequest : IComponentData, IModComponent { }

        private static LayoutImporter _instance;

        //static string wallPacking = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        //import appliance map is used to go from planner codes to plateup codes
        public static Dictionary<string, int> importApplianceMap = new Dictionary<string, int>  {
            { "60", 505496455 },
            { "eY", -1357906425 },
            { "AY", -1440053805 },
            { "Z9", 1329097317 },
            { "oH", -1013770159 },
            { "2V", 2127051779 },
            { "Qs", -1632826946 },
            { "70", -1855909480 },
            { "n2", 481495292 },
            { "0R", 1551609169 },
            { "Ad", 1351951642 },
            { "3D", 1765889988 },
            { "6O", -1495393751 },
            { "VX", 1776760557 },
            { "HD", -1993346570 },
            // { "1Z", -751458770 },
            { "1Z", -1723340146 },
            { "9V", -2091039911 },
            { "9V", -2147057861 },
            { "fU", 1973114260 },
            { "w5", -1906799936 },
            { "sC", -1238047163 },
            { "BM", -1029710921 },
            { "Dg", -1462602185 },
            { "zg", 459840623 },
            { "ze", -1248669347 },
            { "E2", -1573577293 },
            { "qV", 756364626 },
            { "H5", 532998682 },
            { "e6", 1921027834 },
            { "5V", -770041014 },
            { "ud", -1448690107 },
            { "96", 1266458729 },
            { "O9", 1154757341 },
            { "UQ", 862493270 },
            { "5d", -1813414500 },
            { "F5", -571205127 },
            { "5T", -729493805 },
            { "4K", -272437832 },
            { "4K", 1586911545 },
            { "CR", 1446975727 },
            { "8B", 1139247360 },
            { "pj", 238041352 },
            { "UJ", -1817838704 },
            { "Gt", -246383526 },
            { "hM", -1610332021 },
            { "2Q", -1311702572 },
            { "JD", -1068749602 },
            { "yi", -905438738 },
            { "FG", 1807525572 },
            { "uW", 269523389 }, //meat
            { "dG", -1573812073 },
            { "Dc", 759552160 },
            { "Ar", -452101383 },
            { "zQ", -117339838 },
            { "NV", 961148621 },
            { "Ls", -1735137431 },
            { "Ls", -609358791 },
            { "AG", 925796718 },
            { "vu", -1533430406 },
            { "SS", 1193867305 },
            { "uW", -484165118 },
            { "5B", -1097889139 },
            { "Ja", 1834063794 },
            { "WU", -1963699221 },
            { "2o", -1434800013 },
            { "Qi", -1201769154 },
            { "ET", -1506824829 },
            { "0s", -1353971407 },
            { "ze", -996680732 },
            { "2M", 1653145961 },
            { "2M", 434150763 },
            { "ao", 380220741 },
            { "m2", 1313469794 },
            { "NH", -957949759 },
            { "2A", 235423916 },
            { "94", 314862254 },
            { "wn", -1857890774 },
            { "ot", -759808000 },
            { "31", 1656358740 },
            { "r6", 639111696 },
            { "Yn", 1358522063 },
            { "J1", 221442949 },
            { "mD", 1528688658 },
            { "zZ", 2080633647 },
            { "CZ", 446555792 },
            { "qB", 938247786 },
            { "qC", 1648733244 },
            { "qB", -1979922052 },
            { "tV", -3721951 },
            { "T2", -34659638 },
            { "cJ", -203679687 },
            { "GM", -2019409936 },
            { "lq", 209074140 },
            { "WS", 1738351766 },
            { "1P", 624465484 },
            { "kv", 2023704259 },
            { "ZE", 723626409 },
            { "kF", 1796077718 },
            { "py", 230848637 },
            { "3G", 1129858275 },
            { "1g", -214126192 },
            { "W1", 1083874952 },
            { "v2", 1467371088 },
            { "Nt", 1860904347 },
            { "bZ", -266993023 },
            { "0R", 1159228054 },
            { "IX", 303858729 },
            { "zd", -2133205155 },
            { "fU", -667884240 },
            { "96", -349733673 },
            /*{ "96", 1836107598 },
            { "96", 369884364 }, */
            { "jC", 976574457 },
        };
        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
        }

        //baloney to do stuff with singletones so the methods actually run
        protected override void OnUpdate()
        {
            if (TryGetSingletonEntity<SImportRequest>(out Entity e))
            {
                Import();
                EntityManager.DestroyEntity(e);
            }
            if (TryGetSingletonEntity<SImportCheckRequest>(out Entity e2))
            {
                ImportCheck();
                EntityManager.DestroyEntity(e2);
            }
        }

        //if you are in a kitchen, a simpleton will be created which is affected in OnUpdate()
        public static void RequestImportCheck()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SImportRequest>();
            }
        }
        public static void RequestImport()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SImportRequest>();
            }
        }


        //
        protected void Import()
        {
            //firstly, we want to find the HxW of the plateup map
            var bounds = base.Bounds;
            LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());
            int height = (int)(bounds.max.z - bounds.min.z + 1);
            int width = (int)(bounds.max.x - bounds.min.x + 1);

            //next, we want to decompress then extract data from the planner url
            string fullUrl = "";
            string[] splitURL = fullUrl.Split('#');
            string layoutString = splitURL[1];
            string decodedString = LZString.DecompressFromEncodedURIComponent(layoutString);
            string[] splitString = decodedString.Split(' ');
            string plannerVersion = splitString[0];
            string plannerHxW = splitString[1];
            string plannerAppliances = splitString[2];
            string plannerWalls = splitString[3];
            string plannerHeight = plannerHxW.Split('x')[0];
            string plannerWidth = plannerHxW.Split('x')[1];
        }

        //import check is needed because you want to make sure you are not cheating by spawning in appliances
        public void ImportCheck()
        {
            //find the HxW of the plateup map
            var bounds = base.Bounds;
            LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());
            int height = (int)(bounds.max.z - bounds.min.z + 1);
            int width = (int)(bounds.max.x - bounds.min.x + 1);

            //decompress then extract data from the planner url
            string fullUrl = "";
            string[] splitURL = fullUrl.Split('#');
            string layoutString = splitURL[1];
            string decodedString = LZString.DecompressFromEncodedURIComponent(layoutString);
            string[] splitString = decodedString.Split(' ');
            string plannerVersion = splitString[0];
            string plannerHxW = splitString[1];
            string plannerAppliances = splitString[2];
            string plannerWalls = splitString[3];
            string plannerHeight = plannerHxW.Split('x')[0];
            string plannerWidth = plannerHxW.Split('x')[1];
            var plannerApplianceList = new List<string>();
            for(int i = 0; i  < plannerAppliances.Length/3; i++)
            {
                plannerApplianceList.Add(plannerAppliances.Substring(i*3, 2));
            }
            var plannerApplianceCodes = new List<int>();
            foreach (string appliance in plannerApplianceList)
            {
                plannerApplianceCodes.Add(importApplianceMap[appliance]);
            }

            var gameApplianceCodes = new List<int>();
            //extract appliances from plateup game
            for (float i = bounds.max.z; i >= bounds.min.z; i--)
            {
                for (float j = bounds.min.x; j <= bounds.max.x; j++)
                {
                    CAppliance appliance;
                    var layoutPosition = new Vector3 { x = j, z = i };
                    var applianceEntity = base.GetPrimaryOccupant(layoutPosition);
                    if (EntityManager.RequireComponent<CAppliance>(applianceEntity, out appliance) && LayoutExporter.exportApplianceMap.ContainsKey(appliance.ID))
                    {
                        gameApplianceCodes.Add(appliance.ID);
                    }
                }
            }

            //check for each item in the planner, if it is in the game, and if it is remove and proceed, if not add it to an error list
            var applianceMismatchList = new List<int>();
            foreach (int appliance in plannerApplianceCodes)
            {
                if (gameApplianceCodes.IndexOf(appliance) != -1)
                {
                    gameApplianceCodes.Remove(appliance);
                    plannerApplianceCodes.Remove(appliance);
                }
                else
                {
                    applianceMismatchList.Add(appliance);
                }
            }

            //set the import check value
            string mismatches = "";
            foreach (int mismatch in applianceMismatchList)
            {
                mismatches += mismatch + ' ';
            }

            foreach (int mismatch in applianceMismatchList)
            {

            }
            if (applianceMismatchList.Count == 0)
            {
                ImportGUIManager.SetStatus("None");
            }
            else
            {
                ImportGUIManager.SetStatus(mismatches);
            }
        }

        //this is really just for debugging
        static void LogVector(Vector3 vector)
        {
            // x is x, y floor to ceiling, z is "vertical" in the restaurant
            Debug.Log($"({vector.x},{vector.y},{vector.z})");
        }
    }
}
