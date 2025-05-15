using System;
using System.Threading;

namespace Synchronization
{
    class MinResultCollector
    {
        private int globalMin = int.MaxValue;
        private int globalIndex = -1;
        private int completedThreads = 0;
        private readonly object locker = new object();
        private readonly int totalThreads;

        public MinResultCollector(int totalThreads)
        {
            this.totalThreads = totalThreads;
        }

        public void Collect(int localMin, int localIndex)
        {
            lock (locker)
            {
                if (localMin < globalMin)
                {
                    globalMin = localMin;
                    globalIndex = localIndex;
                }
                completedThreads++;
                if (completedThreads == totalThreads)
                {
                    Monitor.Pulse(locker);
                }
            }
        }

        public (int, int) GetResult()
        {
            lock (locker)
            {
                while (completedThreads < totalThreads)
                {
                    Monitor.Wait(locker);
                }
            }
            return (globalMin, globalIndex);
        }
    }

    class MinFinderThread
    {
        private readonly int[] array;
        private readonly int start;
        private readonly int end;
        private readonly MinResultCollector collector;

        public MinFinderThread(int[] array, int start, int end, MinResultCollector collector)
        {
            this.array = array;
            this.start = start;
            this.end = end;
            this.collector = collector;
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
            collector.Collect(localMin, localIndex);
        }
    }

    class MinFinderExecutor
    {
        private readonly int[] array;
        private readonly int numThreads;
        private readonly MinResultCollector collector;

        public MinFinderExecutor(int size, int numThreads)
        {
            this.numThreads = numThreads;
            array = GenerateArray(size);
            collector = new MinResultCollector(numThreads);
        }

        public (int, int) Execute()
        {
            int chunkSize = array.Length / numThreads;
            Thread[] threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int start = i * chunkSize;
                int end = (i == numThreads - 1) ? array.Length : start + chunkSize;

                MinFinderThread worker = new MinFinderThread(array, start, end, collector);
                threads[i] = new Thread(worker.FindMin);
                threads[i].Start();
            }

            return collector.GetResult();
        }

        private int[] GenerateArray(int size)
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
    
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the size of the array: ");
            int arraySize = int.Parse(Console.ReadLine());

            Console.Write("Enter the number of threads: ");
            int numThreads = int.Parse(Console.ReadLine());

            MinFinderExecutor executor = new MinFinderExecutor(arraySize, numThreads);
            var (globalMin, globalIndex) = executor.Execute();

            Console.WriteLine($"Global Min: {globalMin}, Index: {globalIndex}");
        }
    }
}