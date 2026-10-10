namespace TFR.Player
{
    public enum CharacterMotionState
    {
        /// <summary>Бег без доски.</summary>
        RunningWithoutBoard,
        /// <summary>Бег с доской в руке.</summary>
        RunningWithBoard,
        /// <summary>Игрок стоит на доске.</summary>
        Riding
    }
    /// <summary>
    /// Хранит состояние игрока и пускает только допустимые переходы.
    /// </summary>
    public sealed class Character
    {
        public CharacterMotionState State { get; private set; } = CharacterMotionState.RunningWithoutBoard;
        /// <summary>Подобрать доску: бег без доски → бег с доской.</summary>
        public bool TryTakeBoard()
        {
            if (State != CharacterMotionState.RunningWithoutBoard)
                return false;
            State = CharacterMotionState.RunningWithBoard;
            return true;
        }
        /// <summary>Встать на доску: бег с доской → езда.</summary>
        public bool TryMountBoard()
        {
            if (State != CharacterMotionState.RunningWithBoard)
                return false;
            State = CharacterMotionState.Riding;
            return true;
        }

        /// <summary>Выбросить доску: бег с доской → бег без доски.</summary>
        public bool TryDropBoard()
        {
            if (State != CharacterMotionState.RunningWithBoard)
                return false;
            State = CharacterMotionState.RunningWithoutBoard;
            return true;
        }

        /// <summary>Снять доску в руку: езда → бег с доской.</summary>
        public bool TryStowBoard()
        {
            if (State != CharacterMotionState.Riding)
                return false;
            State = CharacterMotionState.RunningWithBoard;
            return true;
        }
    }
}