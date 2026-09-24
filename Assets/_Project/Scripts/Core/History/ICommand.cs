namespace PhysSim.Core
{
    /// <summary>
    /// Атомарное действие пользователя (P6). Команды оперируют ObjectId
    /// и маленькими снапшотами состояния, не держат тяжёлых ссылок.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Человекочитаемое описание для истории («Создать: Куб 1»).</summary>
        string Description { get; }

        void Execute();

        void Undo();
    }
}
