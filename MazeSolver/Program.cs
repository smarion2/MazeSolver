using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MazeSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            WallNode[,] nodes = new WallNode[301, 301]; 
                    
            using (Image image = Image.FromFile("MAZE.png"))
            {
                int nodeX = 0;
                int nodeY = 0;
                for (int x = 1; x < image.Width; x += 6)
                {                         
                    for (int y = 1; y < image.Height; y += 6)
                    {                        
                        Rectangle crop = new Rectangle(x, y, 6, 6);
                        Bitmap target = new Bitmap(crop.Width, crop.Height);
                        using (Graphics g = Graphics.FromImage(target))
                        {
                            g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), crop, GraphicsUnit.Pixel);
                            Color topTest = target.GetPixel(3, 0);
                            Color bottomTest = target.GetPixel(3, 5);
                            Color leftTest = target.GetPixel(0, 3);
                            Color rightTest = target.GetPixel(5, 3);                            

                            nodes[nodeX, nodeY] = CreateNode(x, y, topTest, bottomTest, leftTest, rightTest);
                        }
                        nodeY++;
                    }
                    nodeY = 0;
                    nodeX++;
                }
                System.Diagnostics.Debugger.Break();
            }   
        }

        public static WallNode CreateNode(int x, int y, Color top, Color bottom, Color left, Color right)
        {
            WallNode node = new WallNode();
            node.X = x;
            node.Y = y;

            if (top.Name == "ffffffff")
                node.TopWall = WallState.Open;
            else
                node.TopWall = WallState.Closed;

            if (bottom.Name == "ffffffff")
                node.BottomWall = WallState.Open;
            else
                node.BottomWall = WallState.Closed;

            if (left.Name == "ffffffff")
                node.LeftWall = WallState.Open;
            else
                node.LeftWall = WallState.Closed;

            if (right.Name == "ffffffff")
                node.RightWall = WallState.Open;
            else
                node.RightWall = WallState.Closed;

            return node;
        }
    }
}
