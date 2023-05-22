using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PlateUpPlannerIntegration
{
    public class LayoutImporter : RestaurantSystem, IModSystem
    {
        protected struct SImportRequest : IComponentData, IModComponent { }
        protected struct SImportCheckRequest : IComponentData, IModComponent { }
        protected struct SStaticImportRequest : IComponentData, IModComponent { }

        private static LayoutImporter _instance;

        private static ImportExportHelpers Iehelpers = new ImportExportHelpers();

        //static string wallPacking = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        //for a static import to work the user must do an import check, and this bool will store it.
        private static bool _importCheckStatus;

        //set import check status
        public static void SetImportCheckStatus(bool status)
        {
            _importCheckStatus = status;
        }
        //retrieve the import check status
        public static bool GetImportCheckStatus()
        {
            return _importCheckStatus;
        }

        EntityQuery Appliances;
        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
            Appliances = GetEntityQuery(new QueryHelper()
                .All(typeof(CAppliance), typeof(CPosition))
                .None(typeof(CImmovable)));
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
            if (TryGetSingletonEntity<SStaticImportRequest>(out Entity e3))
            {
                StaticImport();
                EntityManager.DestroyEntity(e3);
            }
        }

        //if you are in a kitchen, a simpleton will be created which is affected in OnUpdate()
        public static void RequestImportCheck()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SImportCheckRequest>();
            }
        }
        public static void RequestImport()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SImportRequest>();
            }
        }

        public static void RequestStaticImport()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SStaticImportRequest>();
            }
        }

        protected void StaticImport()
        {
            //find the HxW of the plateup map
            var bounds = base.Bounds;
            /*LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());*/
            int height = (int)(bounds.max.z - bounds.min.z + 1);
            int width = (int)(bounds.max.x - bounds.min.x + 1);

            //decompress then extract data from the planner url
            if (ImportGUIManager.GetLayoutString().IndexOf("#") != -1)
            {
                //string of appliances straight from the planner; if you want to know more info about the planner link, see the ImportExportHelpers class
                string plannerAppliancesString = ImportExportHelpers.GetPlannerAppliances(ImportExportHelpers.DecodePlannerURL());
                //list of planner appliances in the planner format - not game codes. for more info, see https://github.com/plateupplanner/plateupplanner.github.io/blob/main/src/utils/helpers.tsx
                var plannerApplianceList = ImportExportHelpers.GetPlannerApplianceList(plannerAppliancesString);
                //list of rotations from the planner url as a list, each rotation corresponding with it's appliance. "u" is up, "l" is left, "d" is down, "r" is right
                var plannerApplianceRotations = ImportExportHelpers.GetPlannerRotationsList(plannerAppliancesString);
                //list of appliances as game codes, for more info see https://github.com/KitchenMods/KitchenLib/blob/master/KitchenLib/src/References/GDOReferences.cs 
                var plannerApplianceCodes = ImportExportHelpers.GetPlannerApplianceCodes(plannerApplianceList);
                for (float i = bounds.max.z; i >= bounds.min.z - 2; i--)
                {
                    for (float j = bounds.min.x; j <= bounds.max.x; j++)
                    {
                        string rotation = plannerApplianceRotations[0];
                        Quaternion switchedRotation;
                        ////Mod.LogInfo(switchedRotation);
                        switch (rotation)
                        {
                            default:
                                switchedRotation = new Quaternion(0f, 0f, 0f, 1f);
                                break;
                            case "r":
                                switchedRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
                                break;
                            case "l":
                                switchedRotation = new Quaternion(0f, -0.7071068f, 0f, 0.7071068f);
                                break;
                            case "d":
                                switchedRotation = new Quaternion(0f, 0.9999999f, 0f, 0f);
                                break;
                        }

                        var layoutPosition = new CPosition
                        {
                            Position = new Vector3 { x = j, z = i },
                            Rotation = switchedRotation
                        };
                        CCreateAppliance appliance = plannerApplianceCodes[0];
                        var newAppliance = EntityManager.CreateEntity();
                        if (base.GetOccupant(layoutPosition) != default(Entity))
                        {
                            EntityManager.DestroyEntity(base.GetPrimaryOccupant(layoutPosition));
                            base.SetOccupant(layoutPosition, default(Entity));
                        }

                        if (appliance == 00)
                        {
                            EntityManager.AddComponent<CCreateAppliance>(default(Entity));
                            EntityManager.AddComponent<CPosition>(newAppliance);
                            EntityManager.SetComponentData<CPosition>(newAppliance, layoutPosition);
                        }
                        else
                        {
                            EntityManager.AddComponent<CCreateAppliance>(newAppliance);
                            EntityManager.SetComponentData<CCreateAppliance>(newAppliance, appliance);
                            EntityManager.AddComponent<CPosition>(newAppliance);
                            EntityManager.SetComponentData<CPosition>(newAppliance, layoutPosition);
                        }
                        plannerApplianceCodes.RemoveAt(0);
                        plannerApplianceRotations.RemoveAt(0);
                    }
                }
            }
        }

        protected void Import()
        {
            var bounds = base.Bounds;
            int width = (int)(bounds.max.x - bounds.min.x + 1);

            string importStr = ImportExportHelpers.GetPlannerAppliances(ImportExportHelpers.DecodePlannerURL());
            var importLst = ImportExportHelpers.GetPlannerApplianceList(importStr);
            var importRot = ImportExportHelpers.GetPlannerRotationsList(importStr);
            var importCodes = ImportExportHelpers.GetPlannerApplianceCodes(importLst);

            using NativeArray<Entity> ents = Appliances.ToEntityArray(Allocator.Temp);
            using NativeArray<CAppliance> appliances = Appliances.ToComponentDataArray<CAppliance>(Allocator.Temp);
            using NativeArray<CPosition> poss = Appliances.ToComponentDataArray<CPosition>(Allocator.Temp);

            for (int i = 0; i < ents.Length; i++)
            {
                Entity e = ents[i];
                CAppliance app = appliances[i];
                CPosition pos = poss[i];

                int index = importCodes.IndexOf(app.ID);
                Vector3 newPos = GetFallbackTile();
                Quaternion newRot = Orientation.Up.ToRotation();
                if (index != -1)
                {
                    newPos = IndexToPos(index);
                    newRot = RotStrToRotation(importRot[index]);
                    importCodes[index] = 0;
                }
                pos.Position = newPos;
                pos.Rotation = newRot;
                Set(e, pos);
            }

            Vector3 IndexToPos(int index)
            {
                Mod.LogInfo($"{bounds.max.z} - {bounds.max.z - (index / width)}");
                return new Vector3((index % width) + bounds.min.x, 0f, bounds.max.z - (index / width));
            }

            Quaternion RotStrToRotation(string rotation)
            {
                switch (rotation)
                {
                    case "d":
                        return Orientation.Down.ToRotation();
                    case "l":
                        return Orientation.Left.ToRotation();
                    case "r":
                        return Orientation.Right.ToRotation();
                    default:
                        return Orientation.Up.ToRotation();
                }
            }
        }
        //import check is needed because you want to make sure you are not cheating by spawning in appliances
        public void ImportCheck()
        {
            //find the HxW of the plateup map
            var bounds = base.Bounds;
            /*LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());*/
            int height = (int)(bounds.max.z - bounds.min.z + 1);
            int width = (int)(bounds.max.x - bounds.min.x + 1);

            //string of appliances straight from the planner; if you want to know more info about the planner link, see the ImportExportHelpers class
            string plannerAppliancesString = ImportExportHelpers.GetPlannerAppliances(ImportExportHelpers.DecodePlannerURL());
            //list of planner appliances in the planner format - not game codes. for more info, see https://github.com/plateupplanner/plateupplanner.github.io/blob/main/src/utils/helpers.tsx
            var plannerApplianceList = ImportExportHelpers.GetPlannerApplianceList(plannerAppliancesString);
            //list of rotations from the planner url as a list, each rotation corresponding with it's appliance. "u" is up, "l" is left, "d" is down, "r" is right
            var plannerApplianceRotations = ImportExportHelpers.GetPlannerRotationsList(plannerAppliancesString);
            //list of appliances as game codes, for more info see https://github.com/KitchenMods/KitchenLib/blob/master/KitchenLib/src/References/GDOReferences.cs 
            var plannerApplianceCodes = ImportExportHelpers.GetPlannerApplianceCodes(plannerApplianceList);

            //extract game codes from the game
            Mod.LogInfo("sorry");
            var gameApplianceCodes = new List<int>();
            for (float i = bounds.max.z; i >= bounds.min.z; i--)
            {
                for (float j = bounds.min.x; j <= bounds.max.x; j++)
                {
                    CAppliance appliance;
                    var layoutPosition = new Vector3 { x = j, z = i };
                    var applianceEntity = GetPrimaryOccupant(layoutPosition);
                    if (EntityManager.RequireComponent<CAppliance>(applianceEntity, out appliance) && LayoutExporter.exportApplianceMap.ContainsKey(appliance.ID) && applianceEntity != default(Entity))
                    {
                        gameApplianceCodes.Add(appliance.ID);
                    }
                }
            }
            //check for each item in the planner, if it is in the game, and if it is remove and proceed, if not add it to an error list
            var applianceMismatchList = new List<int>();
            var gameApplianceRemoves = new List<int>();
            var plannerApplianceRemoves = new List<int>();
            //foreach (int appliance in plannerApplianceCodes)
            for (int i = 0; i < plannerApplianceCodes.Count; i++)
            {
                int appliance = plannerApplianceCodes[i];
                if (gameApplianceCodes.IndexOf(appliance) != -1)
                {
                    gameApplianceRemoves.Add(appliance);
                    plannerApplianceRemoves.Add(appliance);
                }
                else
                {
                    if (appliance != 0)
                    {
                        applianceMismatchList.Add(appliance);
                    }
                }
            }
            Mod.LogInfo("4");
            foreach (int appliance in gameApplianceRemoves)
            {
                gameApplianceCodes.Remove(appliance);
            }
            foreach (int appliance in plannerApplianceRemoves)
            {
                plannerApplianceCodes.Remove(appliance);
            }

            //set the import check value
            string mismatches = "Missing appliances: ";

            Dictionary<string, int> mismatchDictionary = new Dictionary<string, int>();
            foreach (int mismatch in applianceMismatchList)
            {
                if (GameData.Main.TryGet<Appliance>(mismatch, out Appliance appliance))
                {
                    if (mismatchDictionary.ContainsKey(appliance.Name))
                    {
                        mismatchDictionary[appliance.Name]++;
                    }
                    else
                    {
                        mismatchDictionary[appliance.Name] = 1;
                    }
                }
            }
            Mod.LogInfo("5");
            mismatches += string.Join(", ", mismatchDictionary.Select(kv => $"{kv.Value} {kv.Key}"));
            Mod.LogInfo(mismatches);
            Mod.LogInfo(applianceMismatchList);
            Mod.LogInfo(applianceMismatchList.Count);
            if (applianceMismatchList.Count == 0)
            {
                ImportGUIManager.SetStatus("None");
                SetImportCheckStatus(true);
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