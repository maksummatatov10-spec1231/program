using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysSim.Core;
using UnityEngine;

namespace PhysSim.ImportExport
{
    /// <summary>
    /// Экспорт объектов и сцены в Wavefront OBJ: мировые координаты, LH→RH (флип X,
    /// реверс winding), один объект = одна группа 'o', UV/нормали включаются.
    /// Файлы открываются в Blender/Max/CAD. ExportMeshTo используется сериализатором сцены.
    /// </summary>
    public sealed class ObjExporter : MonoBehaviour
    {
        private WorldRegistry _world;

        public void Configure(WorldRegistry world)
        {
            _world = world;
        }

        /// <summary>Экспортировать перечисленные объекты в один OBJ-файл.</summary>
        public void ExportObjects(IReadOnlyList<SceneObject> objects, string filePath)
        {
            var builder = new StringBuilder(1 << 16);
            builder.AppendLine("# PhysSim Studio OBJ export");
            builder.AppendLine("# Y-up, метры");

            var vertexOffset = 1; // OBJ-индексы 1-based
            var uvOffset = 1;
            var normalOffset = 1;

            for (var i = 0; i < objects.Count; i++)
            {
                WriteObject(builder, objects[i], ref vertexOffset, ref uvOffset, ref normalOffset);
            }

            File.WriteAllText(filePath, builder.ToString());
        }

        /// <summary>Экспортировать всю сцену в один OBJ-файл.</summary>
        public void ExportWholeScene(string filePath)
        {
            var objects = new List<SceneObject>(_world.Objects.Count);
            for (var i = 0; i < _world.Objects.Count; i++)
            {
                objects.Add(_world.Objects[i]);
            }

            ExportObjects(objects, filePath);
        }

        /// <summary>Экспорт одного меша в локальных координатах (сейв сцены, meshes/*.obj).</summary>
        public static void ExportMeshTo(Mesh mesh, string filePath)
        {
            var builder = new StringBuilder(1 << 14);
            builder.AppendLine("# PhysSim Studio mesh export");
            WriteGeometry(builder, mesh, Matrix4x4.identity, "mesh", ref dummyInt, ref dummyInt2, ref dummyInt3);
            File.WriteAllText(filePath, builder.ToString());
        }

        private static int dummyInt = 1, dummyInt2 = 1, dummyInt3 = 1;

        private static void WriteObject(StringBuilder builder, SceneObject sceneObject,
            ref int vertexOffset, ref int uvOffset, ref int normalOffset)
        {
            var meshFilter = sceneObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return;
            }

            var localToWorld = sceneObject.transform.localToWorldMatrix;
            WriteGeometry(builder, meshFilter.sharedMesh, localToWorld, sceneObject.DisplayName,
                ref vertexOffset, ref uvOffset, ref normalOffset);
        }

        private static void WriteGeometry(StringBuilder builder, Mesh mesh, Matrix4x4 localToWorld,
            string objectName, ref int vertexOffset, ref int uvOffset, ref int normalOffset)
        {
            var vertices = mesh.vertices;
            var uvs = mesh.uv;
            var normals = mesh.normals;
            var hasUvs = uvs != null && uvs.Length == vertices.Length;
            var hasNormals = normals != null && normals.Length == vertices.Length;

            builder.AppendLine($"o {objectName}");

            var culture = CultureInfo.InvariantCulture;
            for (var i = 0; i < vertices.Length; i++)
            {
                // LH → RH: флип X уже после world-преобразования (экспорт для Blender и т.п.).
                var world = localToWorld.MultiplyPoint3x4(vertices[i]);
                world.x = -world.x;
                builder.AppendLine(FormattableString.Invariant(
                    $"v {world.x.ToString("R", culture)} {world.y.ToString("R", culture)} {world.z.ToString("R", culture)}"));
            }

            if (hasUvs)
            {
                for (var i = 0; i < uvs.Length; i++)
                {
                    builder.AppendLine(FormattableString.Invariant(
                        $"vt {uvs[i].x.ToString("R", culture)} {uvs[i].y.ToString("R", culture)}"));
                }
            }

            if (hasNormals)
            {
                for (var i = 0; i < normals.Length; i++)
                {
                    var world = localToWorld.MultiplyVector(normals[i]);
                    world.x = -world.x;
                    builder.AppendLine(FormattableString.Invariant(
                        $"vn {world.x.ToString("R", culture)} {world.y.ToString("R", culture)} {world.z.ToString("R", culture)}"));
                }
            }

            for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                var triangles = mesh.GetTriangles(subMesh);
                for (var t = 0; t < triangles.Length; t += 3)
                {
                    // Реверс winding под смену хиральности.
                    var a = triangles[t] + vertexOffset;
                    var b = triangles[t + 2] + vertexOffset;
                    var c = triangles[t + 1] + vertexOffset;

                    string va, vb, vc;
                    if (hasUvs && hasNormals)
                    {
                        var ta = triangles[t] + uvOffset;
                        var tb = triangles[t + 2] + uvOffset;
                        var tc = triangles[t + 1] + uvOffset;
                        var na = triangles[t] + normalOffset;
                        var nb = triangles[t + 2] + normalOffset;
                        var nc = triangles[t + 1] + normalOffset;
                        va = $"{a}/{ta}/{na}";
                        vb = $"{b}/{tb}/{nb}";
                        vc = $"{c}/{tc}/{nc}";
                    }
                    else if (hasNormals)
                    {
                        va = $"{a}//{triangles[t] + normalOffset}";
                        vb = $"{b}//{triangles[t + 2] + normalOffset}";
                        vc = $"{c}//{triangles[t + 1] + normalOffset}";
                    }
                    else
                    {
                        va = a.ToString(culture);
                        vb = b.ToString(culture);
                        vc = c.ToString(culture);
                    }

                    builder.AppendLine(FormattableString.Invariant($"f {va} {vb} {vc}"));
                }
            }

            vertexOffset += vertices.Length;
            if (hasUvs)
            {
                uvOffset += uvs.Length;
            }
            if (hasNormals)
            {
                normalOffset += normals.Length;
            }
        }
    }
}
