using UnityEngine;

namespace PhysSim.Materials
{
    /// <summary>
    /// Встроенная библиотека из 121 материала и сплава.
    /// Плотности — справочные (кг/м³); трение/упругость/Cd — игровые оценки
    /// по категориям. Таблица — данные, не логика: пополняется одной строкой.
    /// </summary>
    public static class MaterialSpecs
    {
        public const string DefaultId = "iron";

        public static readonly MaterialSpec[] All =
        {
            // ─── Металлы (20) ───────────────────────────────────────────────
            S("iron", "Железо", MaterialCategory.Metal, 7870, .55f, .45f, .45f, .9f, 0x8a, 0x8d, 0x91, 1f, .55f),
            S("copper", "Медь", MaterialCategory.Metal, 8960, .5f, .42f, .45f, .9f, 0xc6, 0x7d, 0x4e, 1f, .8f),
            S("aluminium", "Алюминий", MaterialCategory.Metal, 2700, .45f, .38f, .45f, .9f, 0xc8, 0xcb, 0xd0, 1f, .72f),
            S("gold", "Золото", MaterialCategory.Metal, 19300, .45f, .38f, .45f, .9f, 0xff, 0xd7, 0x5e, 1f, .95f),
            S("silver", "Серебро", MaterialCategory.Metal, 10490, .5f, .42f, .45f, .9f, 0xdf, 0xe2, 0xe6, 1f, .95f),
            S("lead", "Свинец", MaterialCategory.Metal, 11340, .6f, .52f, .3f, .9f, 0x5d, 0x60, 0x68, 1f, .45f),
            S("titanium", "Титан", MaterialCategory.Metal, 4500, .5f, .42f, .45f, .9f, 0x9a, 0xa1, 0xab, 1f, .7f),
            S("tungsten", "Вольфрам", MaterialCategory.Metal, 19250, .6f, .5f, .4f, .9f, 0x6f, 0x73, 0x79, 1f, .6f),
            S("magnesium", "Магний", MaterialCategory.Metal, 1740, .4f, .35f, .4f, .9f, 0xb9, 0xbd, 0xc2, 1f, .6f),
            S("nickel", "Никель", MaterialCategory.Metal, 8900, .5f, .42f, .45f, .9f, 0xb3, 0xb8, 0xbf, 1f, .75f),
            S("zinc", "Цинк", MaterialCategory.Metal, 7140, .5f, .4f, .4f, .9f, 0xaa, 0xb0, 0xb8, 1f, .6f),
            S("tin", "Олово", MaterialCategory.Metal, 7310, .45f, .38f, .4f, .9f, 0xb6, 0xbc, 0xc4, 1f, .65f),
            S("platinum", "Платина", MaterialCategory.Metal, 21450, .45f, .38f, .45f, .9f, 0xd6, 0xda, 0xde, 1f, .92f),
            S("chromium", "Хром", MaterialCategory.Metal, 7190, .4f, .35f, .5f, .9f, 0xd9, 0xdd, 0xe2, 1f, .98f),
            S("cobalt", "Кобальт", MaterialCategory.Metal, 8900, .5f, .42f, .45f, .9f, 0x8d, 0x97, 0xa5, 1f, .7f),
            S("molybdenum", "Молибден", MaterialCategory.Metal, 10280, .55f, .45f, .4f, .9f, 0x9a, 0xa0, 0xa8, 1f, .65f),
            S("uranium", "Уран", MaterialCategory.Metal, 19050, .6f, .52f, .3f, .9f, 0x7a, 0x82, 0x65, 1f, .5f),
            S("mercury", "Ртуть", MaterialCategory.Metal, 13546, .2f, .1f, .1f, .9f, 0xc3, 0xc8, 0xcf, 1f, 1f),
            S("beryllium", "Бериллий", MaterialCategory.Metal, 1850, .45f, .38f, .4f, .9f, 0xc9, 0xce, 0xd4, 1f, .65f),
            S("lithium", "Литий", MaterialCategory.Metal, 534, .4f, .35f, .3f, .9f, 0xd4, 0xd8, 0xdc, 1f, .5f),

            // ─── Сплавы (20) ────────────────────────────────────────────────
            S("steel_structural", "Сталь конструкционная", MaterialCategory.Alloy, 7850, .5f, .42f, .5f, .95f, 0x6f, 0x77, 0x80, 1f, .6f),
            S("stainless_304", "Нержавеющая сталь AISI 304", MaterialCategory.Alloy, 7900, .45f, .38f, .5f, .95f, 0xb9, 0xc0, 0xc8, 1f, .85f),
            S("cast_iron", "Чугун серый", MaterialCategory.Alloy, 7200, .6f, .5f, .35f, .95f, 0x59, 0x5d, 0x63, 1f, .3f),
            S("bronze", "Бронза", MaterialCategory.Alloy, 8800, .5f, .42f, .45f, .95f, 0xb0, 0x81, 0x50, 1f, .75f),
            S("brass", "Латунь", MaterialCategory.Alloy, 8500, .45f, .38f, .45f, .95f, 0xcf, 0xae, 0x54, 1f, .8f),
            S("duralumin_d16", "Дюралюминий Д16", MaterialCategory.Alloy, 2780, .45f, .38f, .45f, .92f, 0xb6, 0xbc, 0xc4, 1f, .7f),
            S("nichrome", "Нихром", MaterialCategory.Alloy, 8400, .5f, .42f, .45f, .95f, 0x8f, 0x8a, 0x86, 1f, .7f),
            S("invar", "Инвар", MaterialCategory.Alloy, 8100, .5f, .42f, .45f, .95f, 0x9a, 0xa0, 0xa6, 1f, .6f),
            S("solder_pos61", "Припой ПОС-61", MaterialCategory.Alloy, 8500, .4f, .32f, .35f, .95f, 0xc2, 0xc8, 0xce, 1f, .8f),
            S("woods_metal", "Сплав Вуда", MaterialCategory.Alloy, 9700, .45f, .38f, .3f, .95f, 0xb9, 0xbe, 0xc4, 1f, .7f),
            S("babbitt", "Баббит", MaterialCategory.Alloy, 7400, .45f, .38f, .35f, .95f, 0xb5, 0xba, 0xc0, 1f, .65f),
            S("constantan", "Константан", MaterialCategory.Alloy, 8900, .5f, .42f, .4f, .95f, 0xa8, 0x8f, 0x72, 1f, .6f),
            S("manganin", "Манганин", MaterialCategory.Alloy, 8500, .5f, .42f, .4f, .95f, 0xa2, 0x9a, 0x8c, 1f, .6f),
            S("titanium_vt6", "Титановый сплав ВТ6", MaterialCategory.Alloy, 4430, .5f, .42f, .5f, .92f, 0x98, 0xa0, 0xaa, 1f, .72f),
            S("magnesium_az91", "Магниевый сплав AZ91", MaterialCategory.Alloy, 1810, .42f, .36f, .42f, .92f, 0xb4, 0xb8, 0xbe, 1f, .6f),
            S("zamak", "Цинковый сплав Zamak", MaterialCategory.Alloy, 6700, .45f, .38f, .42f, .95f, 0xae, 0xb4, 0xbc, 1f, .7f),
            S("hss_r6m5", "Быстрорез Р6М5", MaterialCategory.Alloy, 8140, .55f, .46f, .5f, .95f, 0x8b, 0x92, 0x9a, 1f, .8f),
            S("inconel", "Суперсплав Inconel", MaterialCategory.Alloy, 8440, .55f, .46f, .45f, .95f, 0x8e, 0x94, 0x88, 1f, .7f),
            S("permalloy", "Пермаллой", MaterialCategory.Alloy, 8600, .5f, .42f, .45f, .95f, 0x9a, 0xa2, 0xac, 1f, .75f),
            S("electrum", "Электрум", MaterialCategory.Alloy, 15000, .45f, .38f, .45f, .9f, 0xe0, 0xcf, 0x8a, 1f, .9f),

            // ─── Дерево (12) ────────────────────────────────────────────────
            S("wood_oak", "Дуб", MaterialCategory.Wood, 750, .5f, .42f, .4f, 1.1f, 0x9c, 0x7a, 0x4a, 0f, .3f),
            S("wood_pine", "Сосна", MaterialCategory.Wood, 520, .45f, .38f, .4f, 1.1f, 0xc8, 0xa0, 0x6a, 0f, .3f),
            S("wood_birch", "Берёза", MaterialCategory.Wood, 670, .45f, .38f, .4f, 1.1f, 0xd9, 0xc3, 0x9a, 0f, .3f),
            S("wood_beech", "Бук", MaterialCategory.Wood, 700, .48f, .4f, .4f, 1.1f, 0xb9, 0x8d, 0x5f, 0f, .32f),
            S("wood_ash", "Ясень", MaterialCategory.Wood, 680, .48f, .4f, .4f, 1.1f, 0xc9, 0xad, 0x7e, 0f, .3f),
            S("wood_mahogany", "Красное дерево", MaterialCategory.Wood, 780, .5f, .42f, .4f, 1.1f, 0x7d, 0x4a, 0x2e, 0f, .35f),
            S("plywood", "Фанера", MaterialCategory.Wood, 550, .45f, .38f, .35f, 1.15f, 0xc1, 0x9a, 0x62, 0f, .25f),
            S("mdf", "МДФ", MaterialCategory.Wood, 750, .45f, .38f, .3f, 1.15f, 0xa9, 0x82, 0x5a, 0f, .25f),
            S("chipboard", "ДСП", MaterialCategory.Wood, 650, .5f, .42f, .3f, 1.15f, 0xb0, 0x8b, 0x5e, 0f, .2f),
            S("bamboo", "Бамбук", MaterialCategory.Wood, 700, .48f, .4f, .42f, 1.1f, 0xcb, 0xb5, 0x77, 0f, .35f),
            S("cork", "Пробка", MaterialCategory.Wood, 240, .55f, .48f, .3f, 1.4f, 0xc8, 0xa8, 0x78, 0f, .1f),
            S("wood_aspen", "Осина", MaterialCategory.Wood, 480, .45f, .38f, .4f, 1.1f, 0xd6, 0xbd, 0x8f, 0f, .28f),

            // ─── Бумага и картон (8) ────────────────────────────────────────
            S("paper_office", "Офисная бумага", MaterialCategory.Paper, 800, .38f, .3f, .15f, 1.8f, 0xf2, 0xef, 0xe6, 0f, .05f),
            S("paper_news", "Газетная бумага", MaterialCategory.Paper, 450, .35f, .28f, .12f, 1.8f, 0xd8, 0xd3, 0xc4, 0f, .05f),
            S("paper_kraft", "Крафт-бумага", MaterialCategory.Paper, 700, .4f, .32f, .15f, 1.75f, 0xb5, 0x8a, 0x5a, 0f, .05f),
            S("cardboard_corrugated", "Гофрокартон", MaterialCategory.Paper, 150, .45f, .38f, .2f, 1.75f, 0xc8, 0xa8, 0x70, 0f, .05f),
            S("paper_whatman", "Ватман", MaterialCategory.Paper, 250, .38f, .3f, .15f, 1.8f, 0xf4, 0xf1, 0xe8, 0f, .08f),
            S("paper_parchment", "Пергамент", MaterialCategory.Paper, 650, .4f, .32f, .15f, 1.75f, 0xe5, 0xd9, 0xb8, 0f, .12f),
            S("cardboard_dense", "Картон плотный", MaterialCategory.Paper, 680, .45f, .38f, .2f, 1.7f, 0xbf, 0xa0, 0x6a, 0f, .05f),
            S("paper_napkin", "Салфеточная бумага", MaterialCategory.Paper, 100, .32f, .26f, .1f, 1.85f, 0xf6, 0xf3, 0xea, 0f, 0f),

            // ─── Пластик (12) ───────────────────────────────────────────────
            S("plastic_abs", "Пластик ABS", MaterialCategory.Plastic, 1050, .38f, .3f, .4f, 1.2f, 0xd8, 0xd8, 0xd8, 0f, .5f),
            S("plastic_pla", "Пластик PLA", MaterialCategory.Plastic, 1240, .38f, .3f, .4f, 1.2f, 0xcf, 0xe3, 0xd2, 0f, .6f),
            S("hdpe", "Полиэтилен ПНД", MaterialCategory.Plastic, 940, .3f, .24f, .35f, 1.2f, 0xe0, 0xe0, 0xda, 0f, .4f),
            S("polypropylene", "Полипропилен", MaterialCategory.Plastic, 910, .3f, .24f, .35f, 1.2f, 0xdc, 0xdc, 0xd4, 0f, .45f),
            S("polycarbonate", "Поликарбонат", MaterialCategory.Plastic, 1200, .35f, .28f, .5f, 1.15f, 0xc8, 0xd8, 0xe0, 0f, .9f),
            S("pvc", "ПВХ", MaterialCategory.Plastic, 1380, .38f, .3f, .35f, 1.2f, 0xb8, 0xbc, 0xb8, 0f, .55f),
            S("ptfe", "Тефлон (ПТФЭ)", MaterialCategory.Plastic, 2200, .08f, .06f, .2f, 1.2f, 0xe8, 0xe8, 0xe4, 0f, .25f),
            S("nylon_pa6", "Нейлон PA6", MaterialCategory.Plastic, 1140, .32f, .26f, .4f, 1.2f, 0xe2, 0xdd, 0xcf, 0f, .5f),
            S("acrylic_pmma", "Акрил (ПММА)", MaterialCategory.Plastic, 1190, .35f, .28f, .5f, 1.15f, 0xdc, 0xe8, 0xec, 0f, .95f),
            S("foam_eps", "Пенопласт ППС", MaterialCategory.Plastic, 25, .4f, .34f, .3f, 1.5f, 0xf4, 0xf4, 0xf2, 0f, .15f),
            S("foam_pu", "Пенополиуретан", MaterialCategory.Plastic, 60, .45f, .38f, .45f, 1.5f, 0xd8, 0xcf, 0xc4, 0f, .2f),
            S("epoxy", "Эпоксидная смола", MaterialCategory.Plastic, 1150, .4f, .32f, .35f, 1.15f, 0xc9, 0xb9, 0x8a, 0f, .8f),

            // ─── Резина (6) ─────────────────────────────────────────────────
            S("rubber_natural", "Натуральный каучук", MaterialCategory.Rubber, 930, 1f, .85f, .85f, 1.3f, 0x3c, 0x3c, 0x40, 0f, .2f),
            S("rubber_technical", "Техническая резина", MaterialCategory.Rubber, 1300, 1.05f, .9f, .85f, 1.3f, 0x2e, 0x2e, 0x32, 0f, .18f),
            S("silicone", "Силикон", MaterialCategory.Rubber, 1150, .8f, .7f, .6f, 1.3f, 0xd8, 0xd2, 0xcc, 0f, .25f),
            S("neoprene", "Неопрен", MaterialCategory.Rubber, 1240, .95f, .8f, .7f, 1.3f, 0x38, 0x38, 0x3c, 0f, .2f),
            S("rubber_pu", "Полиуретановая резина", MaterialCategory.Rubber, 1100, .9f, .78f, .7f, 1.3f, 0x7a, 0x6a, 0x58, 0f, .3f),
            S("ebonite", "Эбонит", MaterialCategory.Rubber, 1150, .8f, .68f, .3f, 1.25f, 0x26, 0x26, 0x2a, 0f, .3f),

            // ─── Стекло (5) ─────────────────────────────────────────────────
            S("glass_sheet", "Листовое стекло", MaterialCategory.Glass, 2500, .3f, .25f, .6f, 1f, 0xcf, 0xe4, 0xea, 0f, .95f),
            S("glass_tempered", "Закалённое стекло", MaterialCategory.Glass, 2500, .3f, .25f, .6f, 1f, 0xc4, 0xde, 0xe8, 0f, .95f),
            S("glass_quartz", "Кварцевое стекло", MaterialCategory.Glass, 2200, .3f, .25f, .55f, 1f, 0xd8, 0xec, 0xf0, 0f, .95f),
            S("crystal", "Хрусталь", MaterialCategory.Glass, 2900, .3f, .25f, .6f, 1f, 0xd2, 0xe6, 0xee, 0f, .98f),
            S("glass_frosted", "Матовое стекло", MaterialCategory.Glass, 2500, .32f, .26f, .5f, 1f, 0xdc, 0xeb, 0xee, 0f, .6f),

            // ─── Керамика (5) ───────────────────────────────────────────────
            S("porcelain", "Фарфор", MaterialCategory.Ceramic, 2400, .5f, .42f, .4f, 1f, 0xf0, 0xed, 0xe6, 0f, .6f),
            S("ceramic_tile", "Керамическая плитка", MaterialCategory.Ceramic, 2000, .55f, .45f, .35f, 1f, 0xcf, 0xd6, 0xd2, 0f, .55f),
            S("fireclay", "Шамот", MaterialCategory.Ceramic, 1900, .6f, .5f, .25f, 1f, 0xb5, 0x9a, 0x80, 0f, .3f),
            S("sanitary_ware", "Санфаянс", MaterialCategory.Ceramic, 2300, .5f, .42f, .35f, 1f, 0xf2, 0xf0, 0xea, 0f, .65f),
            S("ceramic_alumina", "Техническая керамика Al2O3", MaterialCategory.Ceramic, 3900, .55f, .46f, .3f, 1f, 0xe8, 0xe4, 0xdc, 0f, .5f),

            // ─── Камень и стройматериалы (10) ───────────────────────────────
            S("granite", "Гранит", MaterialCategory.Stone, 2700, .7f, .6f, .3f, 1f, 0x8d, 0x8d, 0x8f, 0f, .3f),
            S("marble", "Мрамор", MaterialCategory.Stone, 2700, .65f, .55f, .3f, 1f, 0xe4, 0xe2, 0xdc, 0f, .45f),
            S("concrete", "Бетон", MaterialCategory.Stone, 2400, .75f, .65f, .2f, 1f, 0x9a, 0x9a, 0x96, 0f, .15f),
            S("reinforced_concrete", "Железобетон", MaterialCategory.Stone, 2500, .75f, .65f, .2f, 1f, 0x8f, 0x90, 0x94, 0f, .15f),
            S("brick", "Кирпич", MaterialCategory.Stone, 1800, .75f, .65f, .25f, 1f, 0xa9, 0x57, 0x3c, 0f, .12f),
            S("gypsum", "Гипс", MaterialCategory.Stone, 1200, .55f, .46f, .15f, 1.05f, 0xe8, 0xe4, 0xda, 0f, .15f),
            S("sandstone", "Песчаник", MaterialCategory.Stone, 2300, .7f, .6f, .2f, 1f, 0xc9, 0xab, 0x7a, 0f, .1f),
            S("limestone", "Известняк", MaterialCategory.Stone, 2500, .7f, .6f, .2f, 1f, 0xd2, 0xcc, 0xba, 0f, .12f),
            S("asphalt", "Асфальт", MaterialCategory.Stone, 2300, .8f, .7f, .1f, 1f, 0x4a, 0x4a, 0x4c, 0f, .1f),
            S("claydite_concrete", "Керамзитобетон", MaterialCategory.Stone, 1200, .7f, .6f, .2f, 1.05f, 0xa8, 0xa4, 0x9c, 0f, .12f),

            // ─── Ткань и кожа (6) ───────────────────────────────────────────
            S("fabric_cotton", "Хлопок", MaterialCategory.Fabric, 350, .6f, .5f, .3f, 1.6f, 0xe0, 0xd8, 0xc8, 0f, 0f),
            S("fabric_wool", "Шерсть", MaterialCategory.Fabric, 250, .65f, .55f, .3f, 1.65f, 0xb0, 0x98, 0x88, 0f, 0f),
            S("leather", "Кожа", MaterialCategory.Fabric, 900, .6f, .5f, .3f, 1.5f, 0x7a, 0x5a, 0x3c, 0f, .25f),
            S("felt", "Войлок", MaterialCategory.Fabric, 200, .65f, .55f, .25f, 1.65f, 0xc8, 0xb8, 0xa8, 0f, 0f),
            S("canvas", "Парусина", MaterialCategory.Fabric, 500, .6f, .5f, .25f, 1.7f, 0xc0, 0xb2, 0x94, 0f, 0f),
            S("silk", "Шёлк", MaterialCategory.Fabric, 150, .5f, .4f, .3f, 1.7f, 0xe8, 0xe2, 0xd2, 0f, .3f),

            // ─── Прочее и композиты (17) ────────────────────────────────────
            S("ice", "Лёд", MaterialCategory.Other, 917, .08f, .05f, .3f, 1f, 0xcf, 0xea, 0xf4, 0f, .85f),
            S("snow", "Снег", MaterialCategory.Other, 200, .3f, .25f, .1f, 1.5f, 0xf2, 0xf6, 0xf8, 0f, .1f),
            S("paraffin_wax", "Парафин", MaterialCategory.Other, 900, .3f, .25f, .2f, 1.1f, 0xf0, 0xe6, 0xd0, 0f, .3f),
            S("salt", "Соль поваренная", MaterialCategory.Other, 2160, .5f, .42f, .2f, 1.2f, 0xf2, 0xf0, 0xea, 0f, .15f),
            S("sugar", "Сахар", MaterialCategory.Other, 1600, .5f, .42f, .2f, 1.2f, 0xf4, 0xf0, 0xe4, 0f, .1f),
            S("soil", "Грунт", MaterialCategory.Other, 1500, .7f, .6f, .1f, 1.2f, 0x6b, 0x55, 0x40, 0f, 0f),
            S("sand_dry", "Песок сухой", MaterialCategory.Other, 1600, .6f, .5f, .1f, 1.2f, 0xd4, 0xbc, 0x84, 0f, 0f),
            S("clay", "Глина", MaterialCategory.Other, 1700, .5f, .42f, .05f, 1.1f, 0x96, 0x70, 0x5a, 0f, .1f),
            S("graphite", "Графит", MaterialCategory.Other, 2250, .15f, .12f, .2f, 1f, 0x3a, 0x3a, 0x3c, 0f, .5f),
            S("diamond", "Алмаз", MaterialCategory.Other, 3500, .2f, .16f, .5f, 1f, 0xe2, 0xf2, 0xf4, 0f, .95f),
            S("carbon_fiber", "Углепластик", MaterialCategory.Composite, 1600, .4f, .34f, .4f, 1.1f, 0x2c, 0x2c, 0x30, .1f, .6f),
            S("fiberglass", "Стеклопластик", MaterialCategory.Composite, 1900, .45f, .38f, .4f, 1.1f, 0xc8, 0xd2, 0xb8, 0f, .5f),
            S("foam_concrete", "Пенобетон", MaterialCategory.Composite, 600, .7f, .6f, .2f, 1.4f, 0xc2, 0xc2, 0xbe, 0f, .1f),
            S("mineral_wool", "Минеральная вата", MaterialCategory.Composite, 100, .6f, .5f, .2f, 1.7f, 0xd8, 0xc8, 0x90, 0f, 0f),
            S("mica", "Слюда", MaterialCategory.Other, 2800, .35f, .3f, .25f, 1f, 0xb8, 0xb4, 0xac, 0f, .45f),
            S("asbestos", "Асбест", MaterialCategory.Other, 2500, .55f, .46f, .15f, 1.5f, 0xc2, 0xbe, 0xb2, 0f, .1f),
            S("sponge", "Губка", MaterialCategory.Other, 30, .5f, .42f, .5f, 1.7f, 0xe8, 0xc8, 0x80, 0f, 0f),
        };

        /// <summary>Фабрика строк: компактная запись таблицы выше.</summary>
        private static MaterialSpec S(string id, string displayName, MaterialCategory category, float density,
            float staticFriction, float dynamicFriction, float restitution, float drag,
            byte r, byte g, byte b, float metallic, float smoothness)
        {
            return new MaterialSpec(
                id, displayName, category, density,
                staticFriction, dynamicFriction, restitution, drag,
                new Color32(r, g, b, 255), metallic, smoothness);
        }
    }
}
