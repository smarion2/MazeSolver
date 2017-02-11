﻿using System;
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
                Console.Clear();
                Solve(nodes[0, 0]);
                while (true) ;
                Console.ReadKey();
            }   
        }
        public static void CreateNewPath(WallNode node, Path path)
        {

        }
        
        public static void Solve(WallNode node)
        {
            //Path path = new Path();

            //path.wallNodes.Add(node);
            if (node.NodeId == 90300)
            {
                Console.WriteLine("!!!!!!!!!!MAZED SOLVED!!!!!!!");
                return;
            }
            if (node.LeftWall != WallState.Open && node.RightWall != WallState.Open && node.TopWall != WallState.Open && node.BottomWall != WallState.Open)
            {
                Console.WriteLine("Path is dead Jim! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                //path.wallNodes.Clear();
            }
            if (node.LeftWall == WallState.Open && node.NumberOfOpenings <= 2)
            {
                nodes[node.PosX - 1, node.PosY].RightWall = WallState.Entrance;
                Console.WriteLine("Going Left! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                Solve(nodes[node.PosX - 1, node.PosY]);
            }
            else if (node.RightWall == WallState.Open && node.NumberOfOpenings <= 2)
            {
                nodes[node.PosX + 1, node.PosY].LeftWall = WallState.Entrance;
                Console.WriteLine("Going right! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                Solve(nodes[node.PosX + 1, node.PosY]);
            }
            else if (node.TopWall == WallState.Open && node.NumberOfOpenings <= 2)
            {
                nodes[node.PosX, node.PosY - 1].BottomWall = WallState.Entrance;
                Console.WriteLine("Going up! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                Solve(nodes[node.PosX, node.PosY - 1]);
            }
            else if (node.BottomWall == WallState.Open && node.NumberOfOpenings <= 2)
            {
                nodes[node.PosX, node.PosY + 1].TopWall = WallState.Entrance;
                Console.WriteLine("Going down! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                Solve(nodes[node.PosX, node.PosY + 1]);
            }
            if (node.NumberOfOpenings > 2)
            {
                if (node.TopWall == WallState.Open)
                {
                    node.TopWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    Thread t = new Thread(() =>
                    {
                        Path newPath = new Path();
                        //newPath = path;
                        nodes[node.PosX, node.PosY - 1].BottomWall = WallState.Entrance;
                        Console.WriteLine("Fork in the road... Going up! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                        Solve(nodes[node.PosX, node.PosY - 1]);
                    });
                    t.Name = "Thread up";
                    t.Start();
                    Solve(node);
                }
                else if (node.BottomWall == WallState.Open)
                {
                    node.BottomWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    Thread t = new Thread(() =>
                    {
                        Path newPath = new Path();
                        //newPath = path;
                        nodes[node.PosX, node.PosY + 1].TopWall = WallState.Entrance;
                        Console.WriteLine("Fork in the road... Going down! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                        Solve(nodes[node.PosX, node.PosY + 1]);
                    });
                    t.Name = "Thread bot";
                    t.Start();
                    Solve(node);
                }
                else if (node.LeftWall == WallState.Open)
                {
                    node.LeftWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    Thread t = new Thread(() =>
                    {
                        Path newPath = new Path();
                        //newPath = path;
                        nodes[node.PosX - 1, node.PosY].RightWall = WallState.Entrance;
                        Console.WriteLine("Fork in the road... Going Left! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                        Solve(nodes[node.PosX - 1, node.PosY]);
                    });
                    t.Name = "Left thread";
                    t.Start();
                    Solve(node);
                }
                else if (node.RightWall == WallState.Open)
                {
                    node.RightWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    Thread t = new Thread(() =>
                    {
                        Path newPath = new Path();
                        //newPath = path;
                        nodes[node.PosX + 1, node.PosY].LeftWall = WallState.Entrance;
                        Console.WriteLine("Fork in the road... Going right! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                        Solve(nodes[node.PosX + 1, node.PosY]);
                    });
                    t.Name = "Right thread";
                    t.Start();
                    Solve(node);
                }
            }
        }

        public static WallNode CreateNode(int x, int y, int posX, int posY, Color top, Color bottom, Color left, Color right, int id)
        {
            Console.WriteLine("Creating Maze Node {0} at {1},{2}", id, x, y);
            WallNode node = new WallNode();
            node.NodeId = id;
            node.PixleX = x;
            node.PixleY = y;
            node.PosX = posX;
            node.PosY = posY;

            if (top.Name == "ffffffff")
            {
                node.TopWall = WallState.Open;
                node.NumberOfOpenings++;
            }
            else
            {
                node.TopWall = WallState.Closed;
            }
            if (bottom.Name == "ffffffff")
            {
                node.BottomWall = WallState.Open;
                node.NumberOfOpenings++;
            }
            else
            {
                node.BottomWall = WallState.Closed;
            }
            if (left.Name == "ffffffff")
            {
                node.LeftWall = WallState.Open;
                node.NumberOfOpenings++;
            }
            else
            {
                node.LeftWall = WallState.Closed;
            }
            if (right.Name == "ffffffff")
            {
                node.RightWall = WallState.Open;
                node.NumberOfOpenings++;
            }
            else
            {
                node.RightWall = WallState.Closed;
            }
            return node;
        }
    }
}
