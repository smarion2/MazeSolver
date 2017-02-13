using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;

namespace MazeSolver
{
    class Program
    {
        public static WallNode[,] nodes = new WallNode[301, 301];
        public static ConcurrentDictionary<int, ConcurrentQueue<WallNode>> PathList = new ConcurrentDictionary<int, ConcurrentQueue<WallNode>>();
        public static Object lockerObject = new Object();
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
                PathList[Thread.CurrentThread.ManagedThreadId] = new ConcurrentQueue<WallNode>();
                Solve(nodes[0, 0]);
                while (true) ;
                Console.ReadKey();
            }   
        }
        public static void DrawAnswer(ConcurrentQueue<WallNode> path)
        {
            //IList<WallNode> readOnlyPath = path.AsReadOnly();
            Console.Clear();
            int nodesLeft = 0;
            Console.WriteLine("Drawing solution");
            Bitmap maze = new Bitmap("MAZE.png");
            Bitmap solvedMaze = new Bitmap(maze.Width, maze.Height);
            using (Graphics g = Graphics.FromImage(solvedMaze))
            {
                g.DrawImage(maze, 0, 0, solvedMaze.Width, solvedMaze.Height);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    nodesLeft++;
                    Console.WriteLine("Drawing path... Remaining nodes: " + (path.Count - nodesLeft));
                    WallNode node = new WallNode();
                    path.TryDequeue(out node);
                    for (int x = 0; x < 6; x++)
                    {
                        for (int y = 0; y < 6; y++)
                        {
                            if (node != null)
                            {
                                if (solvedMaze.GetPixel(node.PixleX + x, node.PixleY + y).Name == "ffffffff")
                                {
                                    solvedMaze.SetPixel(node.PixleX + x, node.PixleY + y, Color.Aqua);
                                }
                            }
                        }
                    }
                    solvedMaze.Save("Solved.png");
                }
            }
        }
        public static int Counter = 0;
        public static void Solve(WallNode node)
        {
            Counter++;     
            lock (lockerObject)
            {
                //PathList[Thread.CurrentThread.ManagedThreadId] = path.wallNodes;
                PathList[Thread.CurrentThread.ManagedThreadId].Enqueue(node);
                //ConcurrentQueue<WallNode> nodeQueue = PathList[Thread.CurrentThread.ManagedThreadId];
                //nodeQueue.Enqueue(node);
                //PathList.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, nodeQueue, (k, v) => nodeQueue);
            }

           // path.wallNodes.Add(node);
            if (node.NodeId == 90300)
            {
                Console.WriteLine("Move Count: " + Counter);
                Console.WriteLine("!!!!!!!!!!MAZED SOLVED!!!!!!!");
                DrawAnswer(PathList[Thread.CurrentThread.ManagedThreadId]);
                return;
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
                    int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    node.TopWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    ConcurrentQueue<WallNode> newQueue = new ConcurrentQueue<WallNode>(PathList[currentThreadId]);
                    //foreach (var value in PathList[currentThreadId])
                    //{
                    //    newQueue.Enqueue(value);
                    //}
                    Thread t = new Thread(() =>
                    {
                        lock (lockerObject)
                        {

                            //PathList[Thread.CurrentThread.ManagedThreadId] = new ConcurrentQueue<WallNode>();
                            PathList.TryAdd(Thread.CurrentThread.ManagedThreadId, newQueue);
                        }
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
                    int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    node.BottomWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    ConcurrentQueue<WallNode> newQueue = new ConcurrentQueue<WallNode>(PathList[currentThreadId]);
                    //foreach (var value in PathList[currentThreadId])
                    //{
                    //    newQueue.Enqueue(value);
                    //}
                    Thread t = new Thread(() =>
                    {
                        PathList.TryAdd(Thread.CurrentThread.ManagedThreadId, newQueue);
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
                    int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    node.LeftWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    ConcurrentQueue<WallNode> newQueue = new ConcurrentQueue<WallNode>(PathList[currentThreadId]);
                    //foreach (var value in PathList[currentThreadId])
                    //{
                    //    newQueue.Enqueue(value);
                    //}
                    Thread t = new Thread(() =>
                    {
                        PathList.TryAdd(Thread.CurrentThread.ManagedThreadId, newQueue);
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
                    int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    node.RightWall = WallState.Closed;
                    node.NumberOfOpenings--;
                    ConcurrentQueue<WallNode> newQueue = new ConcurrentQueue<WallNode>(PathList[currentThreadId]);
                    //foreach (var value in PathList[currentThreadId])
                    //{
                    //    newQueue.Enqueue(value);
                    //}
                    Thread t = new Thread(() =>
                    {
                        PathList.TryAdd(Thread.CurrentThread.ManagedThreadId, newQueue);
                        nodes[node.PosX + 1, node.PosY].LeftWall = WallState.Entrance;
                        Console.WriteLine("Fork in the road... Going right! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                        Solve(nodes[node.PosX + 1, node.PosY]);
                    });
                    t.Name = "Right thread";
                    t.Start();
                    Solve(node);
                }
            }
            if (node.LeftWall != WallState.Open && node.RightWall != WallState.Open && node.TopWall != WallState.Open && node.BottomWall != WallState.Open)
            {
                lock (lockerObject)
                {
                    //Thread.Sleep(200);
                    //PathList[Thread.CurrentThread.ManagedThreadId].Clear();
                    ConcurrentQueue<WallNode> removeQueue = new ConcurrentQueue<WallNode>();
                    PathList.TryRemove(Thread.CurrentThread.ManagedThreadId, out removeQueue);
                }
                //path.wallNodes.Clear();
                Console.WriteLine("Path is dead Jim! On thread: {0} Node ID: {1}", Thread.CurrentThread.ManagedThreadId, node.NodeId);
                //path.wallNodes.Clear();
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
