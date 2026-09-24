using System;
using System.Collections.Generic;

namespace PhysSim.Core
{
    /// <summary>
    /// Стек undo/redo. Execute() сразу выполняет команду; UI-кнопки
    /// активируются по CanUndo/CanRedo и событию HistoryChanged.
    /// </summary>
    public sealed class CommandStack
    {
        private readonly Stack<ICommand> _undo = new Stack<ICommand>(64);
        private readonly Stack<ICommand> _redo = new Stack<ICommand>(64);

        public bool CanUndo
        {
            get { return _undo.Count > 0; }
        }

        public bool CanRedo
        {
            get { return _redo.Count > 0; }
        }

        public event Action HistoryChanged;

        /// <summary>Выполнить команду и запомнить её для undo. Очищает ветку redo.</summary>
        public void Execute(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.Execute();
            _undo.Push(command);
            _redo.Clear();
            HistoryChanged?.Invoke();
        }

        public bool Undo()
        {
            if (_undo.Count == 0)
            {
                return false;
            }

            var command = _undo.Pop();
            command.Undo();
            _redo.Push(command);
            HistoryChanged?.Invoke();
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0)
            {
                return false;
            }

            var command = _redo.Pop();
            command.Execute();
            _undo.Push(command);
            HistoryChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            HistoryChanged?.Invoke();
        }
    }
}
