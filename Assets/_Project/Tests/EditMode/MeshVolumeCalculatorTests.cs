using NUnit.Framework;
using PhysSim.Geometry;
using UnityEngine;

namespace PhysSim.Tests
{
    /// <summary>
    /// Проверка объёма меша (теорема о расходимости) против аналитики.
    /// Отсюда считается масса (ρ·V) — точность критична.
    /// </summary>
    public sealed class MeshVolumeCalculatorTests
    {
        [Test]
        public void UnitCube_HasVolumeOne()
        {
            var cube = CreateCube(1f);
            var volume = MeshVolumeCalculator.Calculate(cube);
            Assert.AreEqual(1f, volume, 1e-4f);
        }

        [Test]
        public void ScaledCube_VolumeScalesWithSide()
        {
            var cube = CreateCube(0.5f);
            var volume = MeshVolumeCalculator.Calculate(cube);
            Assert.AreEqual(0.125f, volume, 1e-4f);
        }

        [Test]
        public void UvSphere_MatchesAnalyticWithinFivePercent()
        {
            var sphere = CreateUvSphere(0.5f, 24, 12);
            var volume = MeshVolumeCalculator.Calculate(sphere);
            var expected = 4f / 3f * Mathf.PI * Mathf.Pow(0.5f, 3f);
            Assert.AreEqual(expected, volume, expected * 0.05f);
        }

        [Test]
        public void CalculateOrEstimate_FallsBackForOpenMesh()
        {
            var openQuad = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0), new Vector3(0, 1, 0)
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            var volume = MeshVolumeCalculator.CalculateOrEstimate(openQuad, 0.5f);
            // Плоский квад — знаковый объём ~0, должен сработать Estimate: AABB(1×1×0)·0.5.
            Assert.AreEqual(0f, volume, 1e-6f);
        }

        private static Mesh CreateCube(float side)
        {
            var h = side * 0.5f;
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(-h, h, -h),
                    new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(-h, h, h)
                },
                triangles = new[]
                {
                    0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
                    0, 1, 5, 0, 5, 4, 3, 6, 2, 3, 7, 6,
                    0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5
                }
            };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateUvSphere(float radius, int segments, int rings)
        {
            var vertices = new Vector3[(rings + 1) * (segments + 1)];
            for (var r = 0; r <= rings; r++)
            {
                var phi = Mathf.PI * r / rings;
                for (var s = 0; s <= segments; s++)
                {
                    var theta = 2f * Mathf.PI * s / segments;
                    vertices[r * (segments + 1) + s] = new Vector3(
                        Mathf.Sin(phi) * Mathf.Cos(theta),
                        Mathf.Cos(phi),
                        Mathf.Sin(phi) * Mathf.Sin(theta)) * radius;
                }
            }

            var triangles = new System.Collections.Generic.List<int>(rings * segments * 6);
            for (var r = 0; r < rings; r++)
            {
                for (var s = 0; s < segments; s++)
                {
                    var a = r * (segments + 1) + s;
                    var b = a + segments + 1;
                    triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                    triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
                }
            }

            var mesh = new Mesh { vertices = vertices, triangles = triangles.ToArray() };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
