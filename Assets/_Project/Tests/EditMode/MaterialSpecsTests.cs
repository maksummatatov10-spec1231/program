using NUnit.Framework;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.Tests
{
    /// <summary>
    /// Санити встроенной библиотеки материалов: ≥120 позиций, уникальные id,
    /// физически осмысленные значения.
    /// </summary>
    public sealed class MaterialSpecsTests
    {
        [Test]
        public void Library_HasAtLeast120Materials()
        {
            Assert.GreaterOrEqual(MaterialSpecs.All.Length, 120);
        }

        [Test]
        public void AllIds_AreUniqueAndValid()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var spec in MaterialSpecs.All)
            {
                Assert.IsFalse(string.IsNullOrEmpty(spec.Id), "Пустой id материала");
                Assert.IsTrue(seen.Add(spec.Id), $"Дубликат id: {spec.Id}");
                Assert.IsFalse(string.IsNullOrEmpty(spec.DisplayName), $"Пустое имя: {spec.Id}");
            }
        }

        [Test]
        public void PhysicalValues_AreSane()
        {
            foreach (var spec in MaterialSpecs.All)
            {
                Assert.Greater(spec.Density, 0f, spec.Id);
                Assert.LessOrEqual(spec.Density, 25000f, spec.Id);
                Assert.GreaterOrEqual(spec.Restitution, 0f, spec.Id);
                Assert.LessOrEqual(spec.Restitution, 1f, spec.Id);
                Assert.GreaterOrEqual(spec.DragCoefficient, 0.01f, spec.Id);
                Assert.LessOrEqual(spec.DragCoefficient, 2.5f, spec.Id);
            }
        }

        [Test]
        public void ReferencePoints_MatchHandbook()
        {
            Assert.AreEqual(7870f, Find("iron").Density, 1f);
            Assert.AreEqual(19300f, Find("gold").Density, 1f);
            Assert.AreEqual(917f, Find("ice").Density, 1f);
            Assert.Less(Find("paper_office").Density, Find("iron").Density);
        }

        private static MaterialSpec Find(string id)
        {
            foreach (var spec in MaterialSpecs.All)
            {
                if (spec.Id == id)
                {
                    return spec;
                }
            }
            Assert.Fail("Материал не найден: " + id);
            return default;
        }
    }
}
