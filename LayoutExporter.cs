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

namespace KitchenMyMod
{
    public class LayoutExporter : RestaurantSystem, IModSystem
    {
        protected struct SExportRequest : IComponentData, IModComponent { }

        private static LayoutExporter _instance;

        static string wallPacking = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        //map Planner's appliance code to GDO ID
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

        public static void AppendToFile(string line)
        {
            StreamWriter file = new StreamWriter("plannerdata", append: true);
            file.WriteLine(line);
            file.Dispose();
        }

        protected override void OnUpdate()
        {
            if (TryGetSingletonEntity<SExportRequest>(out Entity e))
            {
                Mod.LogInfo("Exporting...");
                Export();
                EntityManager.DestroyEntity(e);
            }
        }

        public static void RequestExport()
        {
            if (GameInfo.CurrentScene == SceneType.Kitchen)
            {
                _instance?.GetOrCreate<SExportRequest>();
                Mod.LogInfo("RequestExport");
            }
        }

        protected string Export()
        {
            var bounds = base.Bounds;
            LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());
            int height = (int)(bounds.max.z - bounds.min.z + 1);
            int width = (int)(bounds.max.x - bounds.min.x + 1);
            string layoutString = $"v2 {height}x{width} ";
            string applianceString = "";
            IEnumerable<int> wallCodes = new List<int>();
            for (float i = bounds.max.z; i >= bounds.min.z; i--)
            {
                List<int> verticalWallString = new List<int>();
                List<int> horizontalWallString = new List<int>();
                for (float j = bounds.min.x; j <= bounds.max.x; j++)
                {
                    var layoutPosition = new Vector3 { x = j, z = i };
                    // add appliance or empty square to appliance string
                    var applianceEntity = base.GetPrimaryOccupant(layoutPosition);
                    CAppliance appliance;
                    CPosition position;
                    string applianceCode;
                    if (EntityManager.RequireComponent<CAppliance>(applianceEntity, out appliance) && applianceMap.ContainsKey(appliance.ID))
                    {
                        // TODO get appliance rotation
                        if (EntityManager.RequireComponent<CPosition>(applianceEntity, out position))
                        {
                            string rotation;
                            switch (position.Rotation.ToOrientation().ToString())
                            {
                                default:
                                    rotation = "u";
                                    break;
                                case "Right":
                                    rotation = "r";
                                    break;
                                case "Left":
                                    rotation = "l";
                                    break;
                                case "Down":
                                    rotation = "d";
                                    break;
                            }
                            applianceCode = applianceMap[appliance.ID] + rotation;
                        } else
                        {
                            applianceCode = applianceMap[appliance.ID] + "u";
                        }
                        Mod.LogInfo(position);
                        


                    }
                    else
                    {
                        applianceCode = "00u";
                    }
                    applianceString += applianceCode;

                    // check horizontal adjacencies for wall presence
                    if (j < bounds.max.x)
                    {
                        var right = layoutPosition + (Vector3)LayoutHelpers.Directions[3];
                        if (GetRoom(layoutPosition) == GetRoom(right))
                        {
                            // same room, must be no walls!
                            verticalWallString.Add(0b11);

                        }
                        else
                        if (CanReach(layoutPosition, right))
                        {
                            // can target into the next room, must be a half wall
                            // or maybe a door............? Don't know how to tell
                            verticalWallString.Add(0b10);
                        }
                        else
                        {
                            // different rooms and can't target, must be an actual wall
                            verticalWallString.Add(0b01);
                        }
                    }
                    // check vertical adjacencies for wall presence
                    if (i > bounds.min.z)
                    {
                        var down = layoutPosition + (Vector3)LayoutHelpers.Directions[1];
                        if (GetRoom(layoutPosition) == GetRoom(down))
                        {
                            // same room, must be no walls!
                            horizontalWallString.Add(0b11);

                        }
                        else if (CanReach(layoutPosition, down))
                        {
                            // can target into the next room, must be a half wall
                            // or maybe a door............? Don't know how to tell
                            horizontalWallString.Add(0b10);
                        }
                        else
                        {
                            // different rooms and can't target, must be an actual wall
                            horizontalWallString.Add(0b01);
                        }
                    }
                }

                // append wall strings in correct order
                wallCodes = wallCodes.Concat(verticalWallString);
                wallCodes = wallCodes.Concat(horizontalWallString);
            }
            Mod.LogError(applianceString);
            string wallString = "";
            int piece = 0;
            int accumulator = 0;
            foreach (var wallCode in wallCodes)
            {
                accumulator = accumulator | (wallCode << piece * 2);
                piece++;
                if (piece == 3)
                {
                    wallString += wallPacking[accumulator];
                    piece = 0;
                    accumulator = 0;
                }
            }
            if (piece != 0)
            {
                wallString += wallPacking[accumulator];
                piece = 0;
                accumulator = 0;
            }
            layoutString += applianceString;
            layoutString += " ";
            layoutString += wallString;
            string compressed = LZString.CompressToEncodedURIComponent(layoutString);
            //System.Diagnostics.Process.Start($"https://plateupplanner.github.io/workspace#{layoutString}");
            System.Diagnostics.Process.Start($"https://plateupplanner.github.io/workspace#{compressed}");
            this.Enabled = false;
            Debug.Log(layoutString);
            return layoutString;
        }

        static void LogVector(Vector3 vector)
        {
            // x is x, y floor to ceiling, z is "vertical" in the restaurant
            Debug.Log($"({vector.x},{vector.y},{vector.z})");
        }
    }



    /*
    In case I need to get appliance ids from names again:

                         var appliances = GameData.Main.Get<Appliance>();
                        foreach (var a in appliances)
                        {
                            if (applianceMap.ContainsKey(a.Name))
                            {
                                var value = applianceMap[a.Name];
                                int id = a.ID;
                                AppendToFile($"(\"{value}\", \"{id}\"),");
                            }
                        }
                    }
    */

    /// <summary>
    /// Converted from lz-string 1.4.4
    /// https://github.com/pieroxy/lz-string/blob/c58a22021000ac2d99377cc0bf9ac193a12563c5/libs/lz-string.js
    /// </summary>
    public class LZString
    {
        private const string KeyStrBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        private const string KeyStrUriSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$";
        private static readonly IDictionary<char, char> KeyStrBase64Dict = CreateBaseDict(KeyStrBase64);
        private static readonly IDictionary<char, char> KeyStrUriSafeDict = CreateBaseDict(KeyStrUriSafe);

        private static IDictionary<char, char> CreateBaseDict(string alphabet)
        {
            var dict = new Dictionary<char, char>();
            for (var i = 0; i < alphabet.Length; i++)
            {
                dict[alphabet[i]] = (char)i;
            }
            return dict;
        }

        public static string CompressToEncodedURIComponent(string input)
        {
            if (input == null) return "";

            return Compress(input, 6, code => KeyStrUriSafe[code]);
        }

        public static string DecompressFromEncodedURIComponent(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input = input.Replace(" ", "+");
            return Decompress(input.Length, 32, index => KeyStrUriSafeDict[input[index]]);
        }

        public static string Compress(string uncompressed)
        {
            return Compress(uncompressed, 16, code => (char)code);
        }

        private static string Compress(string uncompressed, int bitsPerChar, Func<int, char> getCharFromInt)
        {
            if (uncompressed == null) throw new ArgumentNullException(nameof(uncompressed));

            int i, value;
            var context_dictionary = new Dictionary<string, int>();
            var context_dictionaryToCreate = new Dictionary<string, bool>();
            var context_wc = "";
            var context_w = "";
            var context_enlargeIn = 2; // Compensate for the first entry which should not count
            var context_dictSize = 3;
            var context_numBits = 2;
            var context_data = new StringBuilder();
            var context_data_val = 0;
            var context_data_position = 0;

            foreach (var context_c in uncompressed)
            {
                if (!context_dictionary.ContainsKey(context_c.ToString()))
                {
                    context_dictionary[context_c.ToString()] = context_dictSize++;
                    context_dictionaryToCreate[context_c.ToString()] = true;
                }

                context_wc = context_w + context_c;
                if (context_dictionary.ContainsKey(context_wc))
                {
                    context_w = context_wc;
                }
                else
                {
                    if (context_dictionaryToCreate.ContainsKey(context_w))
                    {
                        if (FirstOrDefault(context_w) < 256)
                        {
                            for (i = 0; i < context_numBits; i++)
                            {
                                context_data_val = (context_data_val << 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                            }
                            value = FirstOrDefault(context_w);
                            for (i = 0; i < 8; i++)
                            {
                                context_data_val = (context_data_val << 1) | (value & 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = value >> 1;
                            }
                        }
                        else
                        {
                            value = 1;
                            for (i = 0; i < context_numBits; i++)
                            {
                                context_data_val = (context_data_val << 1) | value;
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = 0;
                            }
                            value = FirstOrDefault(context_w);
                            for (i = 0; i < 16; i++)
                            {
                                context_data_val = (context_data_val << 1) | (value & 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = value >> 1;
                            }
                        }
                        context_enlargeIn--;
                        if (context_enlargeIn == 0)
                        {
                            context_enlargeIn = (int)Math.Pow(2, context_numBits);
                            context_numBits++;
                        }
                        context_dictionaryToCreate.Remove(context_w);
                    }
                    else
                    {
                        value = context_dictionary[context_w];
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }


                    }
                    context_enlargeIn--;
                    if (context_enlargeIn == 0)
                    {
                        context_enlargeIn = (int)Math.Pow(2, context_numBits);
                        context_numBits++;
                    }
                    // Add wc to the dictionary.
                    context_dictionary[context_wc] = context_dictSize++;
                    context_w = context_c.ToString();
                }
            }

            // Output the code for w.
            if (context_w != "")
            {
                if (context_dictionaryToCreate.ContainsKey(context_w))
                {
                    if (FirstOrDefault(context_w) < 256)
                    {
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                        }
                        value = FirstOrDefault(context_w);
                        for (i = 0; i < 8; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }
                    }
                    else
                    {
                        value = 1;
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1) | value;
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = 0;
                        }
                        value = FirstOrDefault(context_w);
                        for (i = 0; i < 16; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }
                    }
                    context_enlargeIn--;
                    if (context_enlargeIn == 0)
                    {
                        context_enlargeIn = (int)Math.Pow(2, context_numBits);
                        context_numBits++;
                    }
                    context_dictionaryToCreate.Remove(context_w);
                }
                else
                {
                    value = context_dictionary[context_w];
                    for (i = 0; i < context_numBits; i++)
                    {
                        context_data_val = (context_data_val << 1) | (value & 1);
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value = value >> 1;
                    }


                }
                context_enlargeIn--;
                if (context_enlargeIn == 0)
                {
                    // context_enlargeIn = (int)Math.Pow(2, context_numBits); // Unused. Kept for tracking changes with https://github.com/pieroxy/lz-string/blob/master/libs/lz-string.js#L295
                    context_numBits++;
                }
            }

            // Mark the end of the stream
            value = 2;
            for (i = 0; i < context_numBits; i++)
            {
                context_data_val = (context_data_val << 1) | (value & 1);
                if (context_data_position == bitsPerChar - 1)
                {
                    context_data_position = 0;
                    context_data.Append(getCharFromInt(context_data_val));
                    context_data_val = 0;
                }
                else
                {
                    context_data_position++;
                }
                value = value >> 1;
            }

            // Flush the last char
            while (true)
            {
                context_data_val = (context_data_val << 1);
                if (context_data_position == bitsPerChar - 1)
                {
                    context_data.Append(getCharFromInt(context_data_val));
                    break;
                }
                else context_data_position++;
            }
            return context_data.ToString();
        }

        public static string Decompress(string compressed)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));

            //TODO: Use an enumerator
            return Decompress(compressed.Length, 32768, index => compressed[index]);
        }

        private static string Decompress(int length, int resetValue, Func<int, char> getNextValue)
        {
            var dictionary = new List<string>();
            var enlargeIn = 4;
            var numBits = 3;
            string entry;
            var result = new StringBuilder();
            int i;
            string w;
            int bits = 0, resb, maxpower, power;
            var c = '\0';

            var data_val = getNextValue(0);
            var data_position = resetValue;
            var data_index = 1;

            for (i = 0; i < 3; i += 1)
            {
                dictionary.Add(((char)i).ToString());
            }

            maxpower = (int)Math.Pow(2, 2);
            power = 1;
            while (power != maxpower)
            {
                resb = data_val & data_position;
                data_position >>= 1;
                if (data_position == 0)
                {
                    data_position = resetValue;
                    data_val = getNextValue(data_index++);
                }
                bits |= (resb > 0 ? 1 : 0) * power;
                power <<= 1;
            }

            switch (bits)
            {
                case 0:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 8);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = (char)bits;
                    break;
                case 1:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 16);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = (char)bits;
                    break;
                case 2:
                    return "";
            }
            w = c.ToString();
            dictionary.Add(w);
            result.Append(c);
            while (true)
            {
                if (data_index > length)
                {
                    return "";
                }

                bits = 0;
                maxpower = (int)Math.Pow(2, numBits);
                power = 1;
                while (power != maxpower)
                {
                    resb = data_val & data_position;
                    data_position >>= 1;
                    if (data_position == 0)
                    {
                        data_position = resetValue;
                        data_val = getNextValue(data_index++);
                    }
                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }

                int c2;
                switch (c2 = bits)
                {
                    case (char)0:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 8);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data_val & data_position;
                            data_position >>= 1;
                            if (data_position == 0)
                            {
                                data_position = resetValue;
                                data_val = getNextValue(data_index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }

                        c2 = dictionary.Count;
                        dictionary.Add(((char)bits).ToString());
                        enlargeIn--;
                        break;
                    case (char)1:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 16);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data_val & data_position;
                            data_position >>= 1;
                            if (data_position == 0)
                            {
                                data_position = resetValue;
                                data_val = getNextValue(data_index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }
                        c2 = dictionary.Count;
                        dictionary.Add(((char)bits).ToString());
                        enlargeIn--;
                        break;
                    case (char)2:
                        return result.ToString();
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }

                if (dictionary.Count - 1 >= c2)
                {
                    entry = dictionary[c2];
                }
                else
                {
                    if (c2 == dictionary.Count)
                    {
                        entry = w + w[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                result.Append(entry);

                // Add w+entry[0] to the dictionary.
                dictionary.Add(w + entry[0]);
                enlargeIn--;

                w = entry;

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }
            }
        }

        private static char FirstOrDefault(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(char);
            }

            return value[0];
        }
    }
}