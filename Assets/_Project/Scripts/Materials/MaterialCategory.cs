namespace PhysSim.Materials
{
    /// <summary>
    /// Категории материалов библиотеки (используются фильтром UI и CSV-импортёром).
    /// </summary>
    public enum MaterialCategory
    {
        Metal = 0,      // металлы
        Alloy = 1,      // сплавы
        Wood = 2,       // дерево и древесные плиты
        Paper = 3,      // бумага и картон
        Plastic = 4,    // полимеры
        Rubber = 5,     // каучук и эластомеры
        Glass = 6,      // стекло
        Ceramic = 7,    // керамика и фарфор
        Stone = 8,      // камень и стройматериалы
        Fabric = 9,     // ткани, кожа
        Organic = 10,   // органика (лёд, воск, соль...)
        Composite = 11, // композиты (карбон, стеклопластик...)
        Other = 12
    }
}
