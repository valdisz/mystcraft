namespace advisor.Model {
    using System;

    public enum Direction {
        North = 1,
        Northeast = 2,
        Southeast = 3,
        South = 4,
        Southwest = 5,
        Northwest = 6
    }

    public static class DirectionExtensions {
        public static Direction Opposite(this Direction direction) {
            switch (direction) {
                case Direction.North: return Direction.South;
                case Direction.Northeast: return Direction.Southwest;
                case Direction.Northwest: return Direction.Southeast;
                case Direction.South: return Direction.North;
                case Direction.Southeast: return Direction.Northwest;
                case Direction.Southwest: return Direction.Northeast;
            }

            throw new ArgumentOutOfRangeException(nameof(direction));
        }
    }
}
