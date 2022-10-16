using System;

namespace SimsTools.WinMaze
{
    [Flags]
    public enum MazeBlockWall
    {
        None = 0,
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8,
        All = 15
    }
}