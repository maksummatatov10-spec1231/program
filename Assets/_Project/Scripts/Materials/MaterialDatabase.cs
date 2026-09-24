using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// База всех материалов программы. Работает в двух режимах:
    ///  1) ассет MaterialDatabase.asset (designer-workflow, наполняется CSV-импортёром);
    ///  2) рантайм-экземпляр из встроенной таблицы MaterialSpecs (CreateRuntime) —
    ///     используется AppBootstrap, если ассет не назначен. Игра работает без ассетов.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MaterialDatabase",
        menuName = "PhysSim/Material Database",
        order = 0)]
    public sealed class MaterialDatabase : ScriptableObject
    {
        [SerializeField] private List<MaterialDefinition> materials = new List<MaterialDefinition>(128);
        [SerializeField] private string defaultMaterialId = "iron";

        private Dictionary<string, MaterialDefinition> _index;

        public IReadOnlyList<MaterialDefinition> All
        {
            get { return materials; }
        }

        /// <summary>Материал по умолчанию для новых объектов.</summary>
        public MaterialDefinition Default
        {
            get { return GetById(defaultMaterialId) ?? (materials.Count > 0 ? materials[0] : null); }
        }

        public MaterialDefinition GetById(string id)
        {
            EnsureIndex();
            return id != null && _index.TryGetValue(id, out var definition) ? definition : null;
        }

        /// <summary>Добавить материал (в т.ч. в рантайме). Возвращает false при дубликате id.</summary>
        public bool Register(MaterialDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id))
            {
                return false;
            }

            EnsureIndex();
            if (_index.ContainsKey(definition.Id))
            {
                return false;
            }

            materials.Add(definition);
            _index.Add(definition.Id, definition);
            return true;
        }

        public bool Remove(string id)
        {
            EnsureIndex();
            if (id == null || !_index.TryGetValue(id, out var definition))
            {
                return false;
            }

            _index.Remove(id);
            materials.Remove(definition);
            return true;
        }

        public IEnumerable<MaterialCategory> GetCategories()
        {
            var seen = new HashSet<MaterialCategory>();
            for (var i = 0; i < materials.Count; i++)
            {
                seen.Add(materials[i].Category);
            }

            return seen;
        }

        /// <summary>
        /// Рантайм-база из встроенной таблицы: работает в билде без единого ассета.
        /// </summary>
        public static MaterialDatabase CreateRuntime(IReadOnlyList<MaterialSpec> specs, string defaultId)
        {
            var database = CreateInstance<MaterialDatabase>();
            database.defaultMaterialId = defaultId;
            for (var i = 0; i < specs.Count; i++)
            {
                var definition = CreateInstance<MaterialDefinition>();
                definition.name = specs[i].Id;
                definition.RuntimeSetup(specs[i]);
                definition.hideFlags = HideFlags.HideAndDontSave; // не сохранять в сцену
                database.Register(definition);
            }

            database.RebuildIndex();
            return database;
        }

        public void RebuildIndex()
        {
            _index = new Dictionary<string, MaterialDefinition>(materials.Count);
            for (var i = 0; i < materials.Count; i++)
            {
                var definition = materials[i];
                if (definition == null || string.IsNullOrEmpty(definition.Id))
                {
                    continue;
                }

                if (!_index.TryAdd(definition.Id, definition))
                {
                    Debug.LogWarning($"[MaterialDatabase] Дубликат id '{definition.Id}' в материале '{definition.name}'.", this);
                }
            }
        }

        private void OnEnable()
        {
            RebuildIndex();
        }

        private void EnsureIndex()
        {
            if (_index == null)
            {
                RebuildIndex();
            }
        }
    }
}
