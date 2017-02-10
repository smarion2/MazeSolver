using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
{
    public class WallNode
    {
        public int X { get; set; }
        public int Y { get; set; }

        public WallState LeftWall { get; set; }
        public WallState RightWall { get; set; }
        public WallState TopWall { get; set; }
        public WallState BottomWall { get; set; }
    }
}
