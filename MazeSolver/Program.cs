using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace MazeSolver
{
    class Program
    {
        public static WallNode[,] nodes = new WallNode[301, 301];
        public static bool isSolved = false;
        static void Main(string[] args)
        {                    
            using (Image image = Image.FromFile("MAZE.png"))
            {
                int nodeX = 0;
                int nodeY = 0;
                int id = 1;
                for (int x = 1; x < image.Width; x += 6)
                {                         
                    for (int y = 1; y < image.Height; y += 6)
                    {
                        id++;                        
                        Rectangle crop = new Rectangle(x, y, 6, 6);
                        Bitmap target = new Bitmap(crop.Width, crop.Height);
                        using (Graphics g = Graphics.FromImage(target))
                        {
                            g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), crop, GraphicsUnit.Pixel);
                            Color topTest = target.GetPixel(3, 0);
                            Color bottomTest = target.GetPixel(3, 5);
                            Color leftTest = target.GetPixel(0, 3);
                            Color rightTest = target.GetPixel(5, 3);                            

                            nodes[nodeX, nodeY] = CreateNode(x, y, nodeX, nodeY, topTest, bottomTest, leftTest, rightTest, id);
                        }
                        nodeY++;
                    }
                    nodeY = 0;
                    nodeX++;
                }
                nodes[0, 0].TopWall = WallState.Entrance;
                Solve(nodes[0, 0]);
                Console.ReadKey();
            }   
        }
        public static void CreateNewPath(WallNode node, Path path)
        {

        }
        
        public static void Solve(WallNode node)
        {
            
            //Thread t = new Thread(() =>
            //{
               // Path path = new Path();
                while (!node.isDeadEnd)
                {
                    //path.wallNodes.Add(node);
                    if (node.LeftWall != WallState.Open && node.RightWall != WallState.Open && node.TopWall != WallState.Open && node.BottomWall != WallState.Open)
                    {
                        node.isDeadEnd = true;
                        //path.wallNodes.Clear();
                        Console.WriteLine("Path is dead Jim! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                    }
                    if (node.LeftWall == WallState.Open && !node.isFork)
                    {
                        nodes[node.PosX - 1, node.PosY].RightWall = WallState.Entrance;
                        Solve(nodes[node.PosX - 1, node.PosY]);
                        Console.WriteLine("Going Left! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                    }
                    else if (node.RightWall == WallState.Open && !node.isFork)
                    {
                        nodes[node.PosX + 1, node.PosY].LeftWall = WallState.Entrance;
                        Solve(nodes[node.PosX + 1, node.PosY]);
                        Console.WriteLine("Going right! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                    }
                    else if (node.TopWall == WallState.Open && !node.isFork)
                    {
                        nodes[node.PosX, node.PosY - 1].BottomWall = WallState.Entrance;
                        Solve(nodes[node.PosX, node.PosY - 1]);
                        Console.WriteLine("Going up! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                    }
                    else if (node.BottomWall == WallState.Open && !node.isFork)
                    {
                        nodes[node.PosX, node.PosY + 1].TopWall = WallState.Entrance;
                        Solve(nodes[node.PosX, node.PosY + 1]);
                        Console.WriteLine("Going down! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                    }
                }
            //});
            //t.Start();
        }

        public static WallNode CreateNode(int x, int y, int posX, int posY, Color top, Color bottom, Color left, Color right, int id)
        {
            WallNode node = new WallNode();
            int numberOfOpenings = 0;
            node.NodeId = id;
            node.PixleX = x;
            node.PixleY = y;
            node.PosX = posX;
            node.PosY = posY;

            if (top.Name == "ffffffff")
            {
                node.TopWall = WallState.Open;
                numberOfOpenings++;
            }
            else
            {
                node.TopWall = WallState.Closed;
            }
            if (bottom.Name == "ffffffff")
            {
                node.BottomWall = WallState.Open;
                numberOfOpenings++;
            }
            else
            {
                node.BottomWall = WallState.Closed;
            }
            if (left.Name == "ffffffff")
            {
                node.LeftWall = WallState.Open;
                numberOfOpenings++;
            }
            else
            {
                node.LeftWall = WallState.Closed;
            }
            if (right.Name == "ffffffff")
            {
                node.RightWall = WallState.Open;
                numberOfOpenings++;
            }
            else
            {
                node.RightWall = WallState.Closed;
            }
            if (numberOfOpenings > 2)
            {
                node.isFork = true;
            }
            return node;
        }
    }
}
