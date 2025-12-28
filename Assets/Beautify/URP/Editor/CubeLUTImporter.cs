using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;
using System.Text;

namespace Beautify.Universal {

    public class CubeLUTImporter : EditorWindow {

        [MenuItem("Window/Beautify/Import CUBE LUT")]
        public static void ShowBrowser() {
            string path = EditorUtility.OpenFilePanel("Select .CUBE file", "", "cube");
            if (string.IsNullOrEmpty(path)) return;
            Texture tex = Import(path);
            if (tex != null) {
                Beautify b = BeautifySettings.sharedSettings;
                b.lutIntensity.Override(1);
                b.lutTexture.Override(tex);
                b.lut.Override(true);
            }
        }

        public static Texture3D Import(string path) {

            string assetPath = path;
            int k = path.IndexOf("Assets/");
            if (k >= 0) {
                assetPath = assetPath.Substring(k);
            } else {
                assetPath = "Assets/Imported CUBE LUTs/" + Path.GetFileName(assetPath);
            }

            assetPath = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));
            if (!assetPath.ToUpper().EndsWith("_LUT")) {
                assetPath += "_LUT";
            }
            assetPath += ".asset";
            var tex = AssetDatabase.LoadAssetAtPath<Texture3D>(assetPath);

            if (tex != null) return tex;

            string[] lines = File.ReadAllLines(path);

            int i = 0;
            int size = -1;
            int sizeCube = -1;
            var table = new List<Color>();
            var domainMin = Color.black;
            var domainMax = Color.white;

            while (true) {
                if (i >= lines.Length) {
                    if (table.Count != sizeCube)
                        
                    break;
                }

                string line = FilterLine(lines[i]);

                if (string.IsNullOrEmpty(line))
                    goto next;

                if (line.StartsWith("TITLE"))
                    goto next;

                if (line.StartsWith("LUT_3D_SIZE")) {
                    string sizeStr = line.Substring(11).TrimStart();

                    if (!int.TryParse(sizeStr, out size)) {
                        
                        break;
                    }

                    if (size < 2 || size > 256) {
                        
                        break;
                    }

                    sizeCube = size * size * size;
                    goto next;
                }

                if (line.StartsWith("DOMAIN_MIN")) {
                    if (!ParseDomain(i, line, ref domainMin)) break;
                    goto next;
                }

                if (line.StartsWith("DOMAIN_MAX")) {
                    if (!ParseDomain(i, line, ref domainMax)) break;
                    goto next;
                }

                string[] row = line.Split();

                if (row.Length != 3) {
                    
                    break;
                }

                var color = Color.black;
                for (int j = 0; j < 3; j++) {
                    float d;
                    if (!float.TryParse(row[j], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out d)) {
                        
                        break;
                    }

                    color[j] = d;
                }

                table.Add(color);

            next:
                i++;
            }

            if (sizeCube != table.Count) {
                
                return null;
            }

            tex = new Texture3D(size, size, size, TextureFormat.RGBAHalf, false) {
                anisoLevel = 0,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            tex.SetPixels(table.ToArray(), 0);
            tex.Apply();

            AssetDatabase.CreateAsset(tex, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            tex = AssetDatabase.LoadAssetAtPath<Texture3D>(assetPath);
            return tex;
        }

        static string FilterLine(string line) {
            var filtered = new StringBuilder();
            line = line.TrimStart().TrimEnd();
            int len = line.Length;
            int i = 0;

            while (i < len) {
                char c = line[i];

                if (c == '#')
                    break;

                filtered.Append(c);
                i++;
            }

            return filtered.ToString();
        }

        static bool ParseDomain(int i, string line, ref Color domain) {
            string[] domainStrs = line.Substring(10).TrimStart().Split();

            if (domainStrs.Length != 3) {
                
                return false;
            }

            for (int j = 0; j < 3; j++) {
                float d;
                if (!float.TryParse(domainStrs[j], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out d)) {
                    
                    return false;
                }

                domain[j] = d;
            }

            return true;
        }
    }

}