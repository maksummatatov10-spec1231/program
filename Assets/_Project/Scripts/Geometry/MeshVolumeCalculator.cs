using UnityEngine;

namespace PhysSim.Geometry
{
    /// <summary>
    /// Вычисление объёма меша (ARCHITECTURE.md §9.1).
    /// Точный метод — теорема о расходимости: объём замкнутого меша равен
    /// сумме знаковых объёмов тетраэдров (origin, v0, v1, v2) по всем треугольникам.
    /// Для незамкнутых мешей результат неточен — используем Estimate (bounds × заполнение).
    /// </summary>
    public static class MeshVolumeCalculator
    {
        /// <summary>Коэффициент заполнения AABB по умолчанию для незамкнутых мешей.</summary>
        public const float DefaultFillFactor = 0.5f;

        /// <summary>
        /// Знаковый объём замкнутого меша, м³ (по модулю). Для открытого меша
        /// может вернуть бессмысленное значение — проверяйте IsPlausible или берите
        /// CalculateOrEstimate.
        /// </summary>
        public static float Calculate(Mesh mesh)
        {
            if (mesh == null)
            {
                return 0f;
            }

            var vertices = mesh.vertices;
            var signedVolume = 0f;

            for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                var triangles = mesh.GetTriangles(subMesh);
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var a = vertices[triangles[i]];
                    var b = vertices[triangles[i + 1]];
                    var c = vertices[triangles[i + 2]];

                    // V_тетраэдра = dot(a, cross(b, c)) / 6
                    signedVolume += Vector3.Dot(a, Vector3.Cross(b, c)) / 6f;
                }
            }

            return Mathf.Abs(signedVolume);
        }

        /// <summary>
        /// Грубая оценка объёма: AABB × коэффициент заполнения.
        /// Запасной путь для «дырявых» мешей из интернета.
        /// </summary>
        public static float Estimate(Mesh mesh, float fillFactor = DefaultFillFactor)
        {
            if (mesh == null)
            {
                return 0f;
            }

            var bounds = mesh.bounds;
            var size = bounds.size;
            return Mathf.Max(0f, size.x * size.y * size.z) * Mathf.Clamp01(fillFactor);
        }

        /// <summary>Точный расчёт с проверкой правдоподобия и откатом к оценке.</summary>
        public static float CalculateOrEstimate(Mesh mesh, float fillFactor = DefaultFillFactor)
        {
            var volume = Calculate(mesh);
            if (volume > 1e-9f && IsPlausible(volume, mesh))
            {
                return volume;
            }

            return Estimate(mesh, fillFactor);
        }

        /// <summary>Объём тетраэдра по трём рёбрам (для тестов): |dot(a, cross(b, c))| / 6.</summary>
        public static float TetrahedronVolume(Vector3 a, Vector3 b, Vector3 c)
        {
            return Mathf.Abs(Vector3.Dot(a, Vector3.Cross(b, c))) / 6f;
        }

        private static bool IsPlausible(float volume, Mesh mesh)
        {
            var bounds = mesh.bounds;
            var size = bounds.size;
            var boundsVolume = size.x * size.y * size.z;
            return boundsVolume > 0f && volume <= boundsVolume * 1.01f;
        }
    }
}
