using NUnit.Framework;
using PhysSim.Materials;
using UnityEngine;

namespace PhysSim.Tests
{
    public sealed class MaterialLibraryFilterTests
    {
        [Test]
        public void Filter_ByRussianNameCaseInsensitive()
        {
            var iron = CreateDefinition("iron", "Железо");
            var paper = CreateDefinition("paper_office", "Офисная бумага");

            var result = MaterialLibraryFilter.Filter(new[] { iron, paper }, "желез");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("iron", result[0].Id);

            Object.DestroyImmediate(iron);
            Object.DestroyImmediate(paper);
        }

        [Test]
        public void Filter_ById()
        {
            var iron = CreateDefinition("iron", "Железо");
            var steel = CreateDefinition("steel_structural", "Сталь конструкционная");

            var result = MaterialLibraryFilter.Filter(new[] { iron, steel }, "steel");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("steel_structural", result[0].Id);

            Object.DestroyImmediate(iron);
            Object.DestroyImmediate(steel);
        }

        [Test]
        public void Filter_EmptyQuery_ReturnsAll()
        {
            var a = CreateDefinition("a", "А");
            var b = CreateDefinition("b", "Б");

            var result = MaterialLibraryFilter.Filter(new[] { a, b }, "");
            Assert.AreEqual(2, result.Count);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        private static MaterialDefinition CreateDefinition(string id, string displayName)
        {
            var definition = ScriptableObject.CreateInstance<MaterialDefinition>();
            definition.RuntimeSetup(new MaterialSpec(id, displayName, MaterialCategory.Metal,
                1000f, 0.5f, 0.4f, 0.3f, 1f, Color.white, 0f, 0.5f));
            return definition;
        }
    }
}
