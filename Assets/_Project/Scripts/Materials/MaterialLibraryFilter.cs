using System.Collections.Generic;
using System.Globalization;

namespace PhysSim.Materials
{
    /// <summary>
    /// Поиск/фильтр библиотеки материалов (чистый C#, покрыт EditMode-тестом).
    /// </summary>
    public static class MaterialLibraryFilter
    {
        /// <summary>Фильтр по подстроке имени/id без учёта регистра. Пустой запрос — все материалы.</summary>
        public static List<MaterialDefinition> Filter(
            IReadOnlyList<MaterialDefinition> source, string query)
        {
            var result = new List<MaterialDefinition>(source.Count);
            if (string.IsNullOrEmpty(query))
            {
                for (var i = 0; i < source.Count; i++)
                {
                    result.Add(source[i]);
                }

                return result;
            }

            var normalized = Normalize(query);
            for (var i = 0; i < source.Count; i++)
            {
                var material = source[i];
                if (Normalize(material.DisplayName).Contains(normalized) ||
                    Normalize(material.Id).Contains(normalized))
                {
                    result.Add(material);
                }
            }

            return result;
        }

        /// <summary>Регистрозависимость/локаль не должны ломать поиск (й/и не смешиваем — v1).</summary>
        private static string Normalize(string text)
        {
            return text == null
                ? string.Empty
                : text.ToLower(new CultureInfo("ru-RU")).Trim();
        }
    }
}
