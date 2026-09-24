using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysSim.Core;
using PhysSim.Geometry;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Импортёр Wavefront OBJ: собственный парсер без нативных зависимостей.
    /// Поддержка: v/vt/vn, полигоны любой арности (fan-триангуляция), отрицательные
    /// индексы, группы o/g → отдельные SceneObject. Координаты RH→LH (флип X +
    /// реверс обхода треугольников). Парсинг синхронный (v1); большие модели — бэклог.
    /// </summary>
    public sealed class ObjImporter : MonoBehaviour, IModelImporter
    {
        private ObjectFactory _factory;
        private MaterialDatabase _materials;

        public void Configure(ObjectFactory factory, MaterialDatabase materials)
        {
            _factory = factory;
            _materials = materials;
        }

        public string[] SupportedExtensions
        {
            get { return new[] { "obj" }; }
        }

        public bool CanImport(string filePath)
        {
            return filePath != null && filePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
        }

        public SceneObject[] Import(string filePath, ImportOptions options)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("OBJ-файл не найден", filePath);
            }

            var groups = ParseFile(filePath);
            var created = new List<SceneObject>(groups.Count);

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var offset = new Vector3(i * 0.75f, 0f, 0f); // группы рядом, не друг в друге
                var material = options.Material ?? GuessMaterial(group.Name);
                var sceneObject = _factory.CreateFromMesh(
                    group.Mesh,
                    group.Name,
                    options.Position + offset,
                    Vector3.Scale(Vector3.one * options.UniformScale, options.Scale),
                    true,
                    material);
                created.Add(sceneObject);
            }

            return created.ToArray();
        }

        /// <summary>Загрузить OBJ как один объединённый меш (используется загрузкой сцены).</summary>
        public static Mesh LoadSingleMesh(string filePath)
        {
            var groups = ParseFile(filePath);
            if (groups.Count == 0)
            {
                return null;
            }

            if (groups.Count == 1)
            {
                return groups[0].Mesh;
            }

            // Слияние групп в один меш.
            var totalVertices = 0;
            var totalTriangles = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                totalVertices += groups[i].Mesh.vertexCount;
                totalTriangles += groups[i].Mesh.triangles.Length;
            }

            var vertices = new Vector3[totalVertices];
            var triangles = new int[totalTriangles];
            var vertexOffset = 0;
            var triangleOffset = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                var mesh = groups[i].Mesh;
                Array.Copy(mesh.vertices, 0, vertices, vertexOffset, mesh.vertexCount);
                var tris = mesh.triangles;
                for (var t = 0; t < tris.Length; t++)
                {
                    triangles[triangleOffset + t] = tris[t] + vertexOffset;
                }
                triangleOffset += tris.Length;
                vertexOffset += mesh.vertexCount;
            }

            var combined = new Mesh { name = Path.GetFileNameWithoutExtension(filePath) };
            combined.vertices = vertices;
            combined.triangles = triangles;
            combined.RecalculateNormals();
            combined.RecalculateBounds();
            return combined;
        }

        /// <summary>Подобрать материал по имени группы (совпадение id базы — редкий бонус, не требование).</summary>
        private MaterialDefinition GuessMaterial(string groupName)
        {
            var candidate = _materials.GetById(groupName?.ToLowerInvariant());
            return candidate ?? _materials.Default;
        }

        // ───────────────────────────── Парсер ─────────────────────────────

        private sealed class ObjGroup
        {
            public string Name;
            public Mesh Mesh;
            public readonly List<Vector3> Vertices = new List<Vector3>(1024);
            public readonly List<Vector2> Uvs = new List<Vector2>(1024);
            public readonly List<Vector3> Normals = new List<Vector3>(1024);
            public readonly List<int> Triangles = new List<int>(2048);
            public readonly Dictionary<(int, int, int), int> VertexLookup =
                new Dictionary<(int, int, int), int>(1024);
        }

        public static List<ObjGroup> ParseFile(string filePath)
        {
            var groups = new List<ObjGroup>(8);
            ObjGroup current = null;
            var positions = new List<Vector3>(4096);
            var uvs = new List<Vector2>(4096);
            var normals = new List<Vector3>(4096);

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var spaceIndex = line.IndexOf(' ');
                    if (spaceIndex <= 0)
                    {
                        continue;
                    }

                    var keyword = line.Substring(0, spaceIndex);
                    var rest = line.Substring(spaceIndex + 1).Trim();

                    switch (keyword)
                    {
                        case "o":
                        case "g":
                            current = GetOrCreateGroup(groups, rest, ref current);
                            break;

                        case "v":
                            positions.Add(ParseVector3(rest));
                            current = EnsureGroup(groups, ref current);
                            break;

                        case "vt":
                            uvs.Add(ParseVector2(rest));
                            current = EnsureGroup(groups, ref current);
                            break;

                        case "vn":
                            normals.Add(ParseVector3(rest));
                            current = EnsureGroup(groups, ref current);
                            break;

                        case "f":
                            current = EnsureGroup(groups, ref current);
                            ParseFace(rest, positions, uvs, normals, current);
                            break;

                        // usemtl/mtllib/s и прочее — v1 игнорирует (материал назначается в UI).
                    }
                }
            }

            foreach (var group in groups)
            {
                group.Mesh = BuildMesh(group);
            }

            return groups;
        }

        private static ObjGroup EnsureGroup(List<ObjGroup> groups, ref ObjGroup current)
        {
            if (current != null)
            {
                return current;
            }

            var group = new ObjGroup { Name = "model" };
            groups.Add(group);
            return group;
        }

        private static ObjGroup GetOrCreateGroup(List<ObjGroup> groups, string name, ref ObjGroup current)
        {
            if (current == null || current.Vertices.Count > 0 || current.Triangles.Count > 0)
            {
                var group = new ObjGroup { Name = SanitizeName(name, groups.Count) };
                groups.Add(group);
                return group;
            }

            current.Name = SanitizeName(name, groups.Count);
            return current;
        }

        private static string SanitizeName(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return $"group_{index}";
            }

            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                builder.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ' ' ? c : '_');
            }
            return builder.ToString();
        }

        private static void ParseFace(string rest, List<Vector3> positions, List<Vector2> uvs,
            List<Vector3> normals, ObjGroup group)
        {
            var parts = rest.Split(' ');
            // fan-триангуляция: (0, i, i+1) — OBJ-полигоны предполагаются выпуклыми.
            var cornerIndices = new int[parts.Length];
            var count = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                {
                    continue;
                }
                cornerIndices[count++] = AddCorner(parts[i], positions, uvs, normals, group);
            }

            for (var i = 1; i + 1 < count; i++)
            {
                group.Triangles.Add(cornerIndices[0]);
                group.Triangles.Add(cornerIndices[i]);
                group.Triangles.Add(cornerIndices[i + 1]);
            }
        }

        private static int AddCorner(string token, List<Vector3> positions, List<Vector2> uvs,
            List<Vector3> normals, ObjGroup group)
        {
            var slashFirst = token.IndexOf('/');
            var positionIndex = ParseIndex(slashFirst < 0 ? token : token.Substring(0, slashFirst), positions.Count);
            var uvIndex = -1;
            var normalIndex = -1;

            if (slashFirst >= 0)
            {
                var secondSlash = token.IndexOf('/', slashFirst + 1);
                var uvPart = secondSlash > slashFirst + 1
                    ? token.Substring(slashFirst + 1, secondSlash - slashFirst - 1)
                    : string.Empty;
                var normalPart = secondSlash > 0 ? token.Substring(secondSlash + 1) : string.Empty;

                if (uvPart.Length > 0)
                {
                    uvIndex = ParseIndex(uvPart, uvs.Count);
                }
                if (normalPart.Length > 0)
                {
                    normalIndex = ParseIndex(normalPart, normals.Count);
                }
            }

            var key = (positionIndex, uvIndex, normalIndex);
            if (group.VertexLookup.TryGetValue(key, out var existing))
            {
                return existing;
            }

            group.Vertices.Add(positions[positionIndex]);
            group.Uvs.Add(uvIndex >= 0 && uvs.Count > 0 ? uvs[uvIndex] : Vector2.zero);
            group.Normals.Add(normalIndex >= 0 && normals.Count > 0 ? normals[normalIndex] : Vector3.zero);
            var newIndex = group.Vertices.Count - 1;
            group.VertexLookup.Add(key, newIndex);
            return newIndex;
        }

        /// <summary>OBJ-индексы 1-based; отрицательные — относительно конца списка.</summary>
        private static int ParseIndex(string text, int count)
        {
            var index = int.Parse(text, CultureInfo.InvariantCulture);
            return index > 0 ? index - 1 : count + index;
        }

        private static Vector3 ParseVector3(string text)
        {
            var parts = text.Split(' ');
            float x = 0f, y = 0f, z = 0f;
            var c = 0;
            for (var i = 0; i < parts.Length && c < 3; i++)
            {
                if (parts[i].Length == 0)
                {
                    continue;
                }
                var value = float.Parse(parts[i], CultureInfo.InvariantCulture);
                if (c == 0) x = value; else if (c == 1) y = value; else z = value;
                c++;
            }
            return new Vector3(x, y, z);
        }

        private static Vector2 ParseVector2(string text)
        {
            var parts = text.Split(' ');
            float x = 0f, y = 0f;
            var c = 0;
            for (var i = 0; i < parts.Length && c < 2; i++)
            {
                if (parts[i].Length == 0)
                {
                    continue;
                }
                var value = float.Parse(parts[i], CultureInfo.InvariantCulture);
                if (c == 0) x = value; else y = value;
                c++;
            }
            return new Vector2(x, y);
        }

        /// <summary>Сборка Unity-меша: RH→LH (x = −x, реверс winding).</summary>
        private static Mesh BuildMesh(ObjGroup group)
        {
            var mesh = new Mesh { name = group.Name };

            var vertices = group.Vertices.ToArray();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].x = -vertices[i].x;
            }
            mesh.vertices = vertices;

            if (group.Uvs.Count == vertices.Length)
            {
                mesh.uv = group.Uvs.ToArray();
            }

            var normals = group.Normals.ToArray();
            var hasNormals = group.Normals.Count == vertices.Length;
            if (hasNormals)
            {
                for (var i = 0; i < normals.Length; i++)
                {
                    normals[i].x = -normals[i].x;
                }
                mesh.normals = normals;
            }

            var triangles = group.Triangles.ToArray();
            for (var i = 0; i < triangles.Length; i += 3)
            {
                (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
            }
            mesh.triangles = triangles;

            if (!hasNormals)
            {
                mesh.RecalculateNormals();
            }
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
