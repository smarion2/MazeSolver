using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
{
    public class WallNode
    {
        public int NodeId { get; set; }
        public int PixleX { get; set; }
        public int PixleY { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }

        public int NumberOfOpenings { get; set; }

        public WallState LeftWall { get; set; }
        public WallState RightWall { get; set; }
        public WallState TopWall { get; set; }
        public WallState BottomWall { get; set; }
    }
}
