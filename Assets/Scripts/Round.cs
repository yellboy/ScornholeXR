using System;

namespace Assets.Scripts
{
    public class Round
    {
        public TaskObject Task { get; private set; }
        public int Points { get; private set; } = 0;
        public int ThrowCount { get; private set; } = 0;

        public Round(TaskObject task)
        {
            Task = task;
        }

        public void ThrowMade(int thrownOption, ThrowResult throwResult)
        {
            switch (throwResult)
            {
                case ThrowResult.HoleHit:
                    Points += 2;
                    AddOptionPoints(thrownOption);
                    break;
                case ThrowResult.BoardHit:
                    AddOptionPoints(thrownOption);
                    break;
                case ThrowResult.Missed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(throwResult), throwResult, null);
            }

            ThrowCount++;
        }

        private void AddOptionPoints(int thrownOption)
        {
            var rank = Array.IndexOf(Task.Rank, thrownOption);
            var pointsFromOption = 5 - rank;
            Points += pointsFromOption;
        }
    }
}
