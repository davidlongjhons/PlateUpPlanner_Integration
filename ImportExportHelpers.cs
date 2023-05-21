using Kitchen.Layouts;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using TwitchLib.PubSub.Events;
using KitchenMods;
using System.IO;
using UnityEngine;


namespace PlateUpPlannerIntegration
{
    internal class ImportExportHelpers : RestaurantSystem, IModSystem
    {
        /* **If you are looking to understand the way this layout system works, read below**
        The way the planner link works is an lzstring encryption system
        Once decompressed, after the # in the link, it goes "heightxwidth", space, appliances, space, walls
        For the height and width, it will be a number format; for a 16 height 12 width, it will look like 16x12
        For the appliances, it is a string of 3 characters, the first two being the appliance codes(see applianceMap) and the 3rd being rotation. u, r, d, l (up right down left)
        The wall codes are a mystery to me since I have not messed with it, but it is the third item in splitString.
        */

        //this method grabs the appliances, walls etc from the planner
        public static string[] DecodePlannerURL()
        {
            string fullUrl = ImportGUIManager.GetLayoutString();
            string[] splitURL = fullUrl.Split('#');
            string layoutString = splitURL[1];
            string decodedString = LZString.DecompressFromEncodedURIComponent(layoutString);
            string[] splitString = decodedString.Split(' ');
            return splitString;
        }

        //the following set of methods should be used with DecodePlannerURL() as the argument
        public static string GetPlannerAppliances(string[] splitString)
        {
            string plannerAppliances = splitString[2];
            return plannerAppliances;
        }
        //
        public static string GetPlannerHeightWidth(string[] splitString)
        {
            string plannerHxW = splitString[1];
            return plannerHxW;
        }

        public static string GetPlannerWalls(string[] splitString)
        {
            string plannerWalls = splitString[3];
            return plannerWalls;
        }

        //list of planner appliances instead of a long string
        public static List<string> GetPlannerApplianceList(string plannerApplianceString)
        {
            var plannerApplianceList = new List<string>();
            for (int i = 0; i < plannerApplianceString.Length / 3; i++)
            {
                plannerApplianceList.Add(plannerApplianceString.Substring(i * 3, 2));
            }
            return plannerApplianceList;
        }

        //list of planner appliance rotations, indexes match up with GetPlannerApplianceList
        public static List<string> GetPlannerRotationsList(string plannerAppliancesString)
        {
            var plannerRotationsList = new List<string>();
            for (int i = 0; i < plannerAppliancesString.Length / 3; i++)
            {
                plannerRotationsList.Add(plannerAppliancesString.Substring(i * 3 + 2, 1));
            }
            return plannerRotationsList;
        }

        //converts all of the planner codes to plateup codes
        public static List<int> GetPlannerApplianceCodes(List<string> plannerApplianceList)
        {
            var plannerApplianceCodes = new List<int>();
            foreach (string appliance in plannerApplianceList)
            {
                //checks if the appliance is not an empty square or a chair, in which it can convert to a game codei th. tables will auto-gen chairs
                if (appliance != "00" && appliance != "qB")
                {
                    string newAppliance = appliance;
                    //Mod.LogInfo(appliance);
                    if (appliance == "U7" || appliance == "mq")
                    {
                        newAppliance = "3V";
                    }
                    //Mod.LogInfo(newAppliance);
                    plannerApplianceCodes.Add(applianceMap[newAppliance]);
                }
                else
                {
                    plannerApplianceCodes.Add(00);
                }
            }
            return plannerApplianceCodes;
        }


        //be sure to add "https://plateupplanner.github.io/workspace#" before the layout string if you are opening it as a link

        protected override void OnUpdate()
        {
            
        }

        //import appliance map is used to go from planner codes to plateup codes
        public static Dictionary<string, int> applianceMap = new Dictionary<string, int>  {
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
            //robot buffer (1z) may need to be RobotBufferMobile
            { "1Z", -1723340146 },
            //robot mop (9v) may need to be RobotMopMobile
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
            { "uW", -484165118 },
            { "dG", -1573812073 },
            { "Dc", 759552160 },
            { "Ar", -452101383 },
            { "zQ", -117339838 },
            { "NV", 961148621 },
            // this is SourceFish2? { "Ls", -1735137431 },
            { "Ls", -609358791 },
            { "AG", 925796718 },
            { "vu", -1533430406 },
            { "SS", 1193867305 },
            //{ "uW", -484165118 }, SourceMeat in game
            { "5B", -1097889139 },
            { "Ja", 1834063794 },
            { "WU", -1963699221 },
            { "2o", -1434800013 },
            { "Qi", -1201769154 },
            { "ET", -1506824829 },
            { "0s", -1353971407 },
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
            // chair glitches { "qB", -1979922052 },
            { "qC", 1648733244 },
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
            { "IX", 303858729 },
            { "zd", -2133205155 },
            { "jC", 976574457 },
            { "hp", 739504637 },
            { "xm", -823922901 },
            { "j0", -2092567672 },
            { "P7", 385684499 },
            { "HL", 148543530 },
            { "6D", -1609758240 },
            { "gN", 735786885 },
            { "1D", -1132411297 },
            { "Sx", 1799769627 },
            { "We", -965827229 },
            { "NG", -117356585 },
            { "i9", -1210117767 },
            { "CH", -1507801323 },
            { "jt", 1800865634 },
            { "E5", 269523389 },
            { "fH", -2042103798 },
            { "co", 44541785 },
            { "I4", -1055654549 },
            { "XJ", 595306349 },
            { "um", -471813067 },
            { "1K", -712909563 },
            { "3V", -331651461 },
        };
    }
}
