using System;
using System.Threading;

namespace Synchronization
{
    class MinFinderThread
    {
        private int[] array;
        private int start, end;
        private static int globalMin = int.MaxValue;
        private static int globalIndex = -1;
        private static int completedThreads = 0;
        private static int totalThreads = 0;
        private static readonly object locker = new object();

        public MinFinderThread(int[] array, int start, int end)
        {
            this.array = array;
            this.start = start;
            this.end = end;
        }

        public void FindMin()
        {
            int localMin = int.MaxValue;
            int localIndex = -1;

            for (int i = start; i < end; i++)
            {
                if (array[i] < localMin)
                {
                    localMin = array[i];
                    localIndex = i;
                }
            }

            lock (locker)
            {
                if (localMin < globalMin)
                {
                    globalMin = localMin;
                    globalIndex = localIndex;
                }

                completedThreads++;
            }
        }

        public static int GlobalMin => globalMin;
        public static int GlobalIndex => globalIndex;
        public static int CompletedThreads => completedThreads;
        public static int TotalThreads { set { totalThreads = value; } }

        public static bool AllThreadsCompleted()
        {
            lock (locker)
            {
                return completedThreads == totalThreads;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int arraySize = 1000000;
            int[] array = GenerateArray(arraySize);

            Console.Write("Enter the number of threads: ");
            int numThreads = int.Parse(Console.ReadLine());

            MinFinderThread.TotalThreads = numThreads;

            int chunkSize = arraySize / numThreads;
            Thread[] threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int start = i * chunkSize;
                int end = (i == numThreads - 1) ? arraySize : start + chunkSize;

                MinFinderThread worker = new MinFinderThread(array, start, end);
                threads[i] = new Thread(worker.FindMin);
                threads[i].Start();
            }

            while (!MinFinderThread.AllThreadsCompleted())
            {
                Thread.Sleep(10); 
            }

            Console.WriteLine($"Global Min: {MinFinderThread.GlobalMin}, Index: {MinFinderThread.GlobalIndex}");
        }

        static int[] GenerateArray(int size)
        {
            Random rand = new Random();
            int[] array = new int[size];

            for (int i = 0; i < size; i++)
            {
                array[i] = rand.Next(1000);
            }

            int negativeIndex = rand.Next(size);
            array[negativeIndex] = -rand.Next(1, 100);

            return array;
        }
    }
}
