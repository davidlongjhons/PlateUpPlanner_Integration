using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using System;
using System.Collections.Generic;
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

        static string wallPacking = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        public static Dictionary<int, string> applianceMap = new (string, int)[]{
            ("60", 505496455),
            ("eY", -1357906425),
            ("AY", -1440053805),
            ("Z9", 1329097317),
            ("oH", -1013770159),
            ("2V", 2127051779),
            ("Qs", -1632826946),
            ("70", -1855909480),
            ("n2", 481495292),
            ("0R", 1551609169),
            ("Ad", 1351951642),
            ("3D", 1765889988),
            ("6O", -1495393751),
            ("VX", 1776760557),
            ("HD", -1993346570),
            ("1Z", -751458770),
            ("1Z", -1723340146),
            ("9V", -2091039911),
            ("9V", -2147057861),
            ("fU", 1973114260),
            ("w5", -1906799936),
            ("sC", -1238047163),
            ("BM", -1029710921),
            ("Dg", -1462602185),
            ("zg", 459840623),
            ("ze", -1248669347),
            ("E2", -1573577293),
            ("qV", 756364626),
            ("H5", 532998682),
            ("e6", 1921027834),
            ("5V", -770041014),
            ("ud", -1448690107),
            ("96", 1266458729),
            ("O9", 1154757341),
            ("UQ", 862493270),
            ("5d", -1813414500),
            ("F5", -571205127),
            ("5T", -729493805),
            ("4K", -272437832),
            ("4K", 1586911545),
            ("CR", 1446975727),
            ("8B", 1139247360),
            ("pj", 238041352),
            ("UJ", -1817838704),
            ("Gt", -246383526),
            ("hM", -1610332021),
            ("2Q", -1311702572),
            ("JD", -1068749602),
            ("yi", -905438738),
            ("FG", 1807525572),
            ("uW", 269523389), //meat
            ("dG", -1573812073),
            ("Dc", 759552160),
            ("Ar", -452101383),
            ("zQ", -117339838),
            ("NV", 961148621),
            ("Ls", -1735137431),
            ("Ls", -609358791),
            ("AG", 925796718),
            ("vu", -1533430406),
            ("SS", 1193867305),
            ("uW", -484165118),
            ("5B", -1097889139),
            ("Ja", 1834063794),
            ("WU", -1963699221),
            ("2o", -1434800013),
            ("Qi", -1201769154),
            ("ET", -1506824829),
            ("0s", -1353971407),
            ("ze", -996680732),
            ("2M", 1653145961),
            ("2M", 434150763),
            ("ao", 380220741),
            ("m2", 1313469794),
            ("NH", -957949759),
            ("2A", 235423916),
            ("94", 314862254),
            ("wn", -1857890774),
            ("ot", -759808000),
            ("31", 1656358740),
            ("r6", 639111696),
            ("Yn", 1358522063),
            ("J1", 221442949),
            ("mD", 1528688658),
            ("zZ", 2080633647),
            ("CZ", 446555792),
            ("qB", 938247786),
            ("qC", 1648733244),
            ("qB", -1979922052),
            ("tV", -3721951),
            ("T2", -34659638),
            ("cJ", -203679687),
            ("GM", -2019409936),
            ("lq", 209074140),
            ("WS", 1738351766),
            ("1P", 624465484),
            ("kv", 2023704259),
            ("ZE", 723626409),
            ("kF", 1796077718),
            ("py", 230848637),
            ("3G", 1129858275),
            ("1g", -214126192),
            ("W1", 1083874952),
            ("v2", 1467371088),
            ("Nt", 1860904347),
            ("bZ", -266993023),
            ("0R", 1159228054),
            ("IX", 303858729),
            ("zd", -2133205155),
            ("fU", -667884240),
            ("96", -349733673),
            ("96", 1836107598),
            ("96", 369884364),
            ("jC", 976574457),
        }.ToDictionary(a => a.Item2, a => a.Item1);
        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
        }
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


        protected string Import()
        {

        }

        public string ImportCheck()
        {

        }
    }
}
