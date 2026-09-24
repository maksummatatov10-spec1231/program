using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// База всех материалов программы (ассет MaterialDatabase.asset).
    /// Наполняется CSV-импортёром (M3), расширяется в рантайме через Register.
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

        /// <summary>Все используемые категории (для фильтра библиотеки).</summary>
        public IEnumerable<MaterialCategory> GetCategories()
        {
            var seen = new HashSet<MaterialCategory>();
            for (var i = 0; i < materials.Count; i++)
            {
                seen.Add(materials[i].Category);
            }

            return seen;
        }

        /// <summary>Перестроить индекс (OnEnable/после десериализации/массового импорта).</summary>
        public void RebuildIndex()
        {
            _index = new Dictionary<string, MaterialDefinition>(materials.Count);
            for (var i = 0; i < materials.Count; i++)
            {
                var definition = materials[i];
                if (definition == null || string.IsNullOrEmpty(definition.Id))
                {
                    continue; // битые ассеты не роняем — репортим при валидации (M3)
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
