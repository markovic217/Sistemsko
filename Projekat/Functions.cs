using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Projekat
{
    internal class Functions
    {
        private static int brojNiti = Environment.ProcessorCount;
        private static int chunkLength = brojNiti;
        public static byte[] ThreadChunkFunction(string filePath)
        {
            int br = 0;
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int chunkSize = fileBytes.Length / chunkLength;
            int remainingChunk = fileBytes.Length % chunkLength;
            byte[] hashedFile = new byte[chunkLength * (chunkSize + 1) + remainingChunk + 32];
            List<Thread> threads = new List<Thread>();
            int i = 0;
            for (; i < chunkLength; i++)
            {
                Thread t = new Thread(() =>
                {
                    Func(br++);
                });
                t.Start();
                threads.Add(t);
            }
            foreach (Thread n in threads)
            {
                n.Join();
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] chunk = new byte[remainingChunk];
                Array.Copy(fileBytes, i * chunkSize, chunk, 0, remainingChunk);
                byte[] hash = new byte[remainingChunk];
                hash = sha256.ComputeHash(chunk);
                hash.CopyTo(hashedFile, remainingChunk * i);
            }
            
            void Func(object? obj)
            {

                int j = (int)obj;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] chunk = new byte[chunkSize];
                    Array.Copy(fileBytes, j * chunkSize, chunk, 0, chunkSize);
                    byte[] hash = new byte[chunkSize];
                    hash = sha256.ComputeHash(chunk);
                    hash.CopyTo(hashedFile, chunkSize * j);
                }
            }

            return hashedFile;
        }

        public static byte[] ThreadPoolChunkFunction(string filePath)
        {
            //ThreadPool.SetMaxThreads(brojNiti, 0);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int chunkSize = 32;
            int chunkLength = fileBytes.Length / chunkSize;
            int remainingChunk = fileBytes.Length % chunkSize;
            byte[] hashedFile = new byte[chunkLength * (chunkSize) + 32];
            int i = 0;
                for (; i < chunkLength; i++)
                    ThreadPool.QueueUserWorkItem(Func, i);
            bool gotova_obrada = false;
            while (!gotova_obrada)
            {
              Thread.Sleep(new TimeSpan(0,0,0,0,0,10));
              gotova_obrada = ThreadPool.PendingWorkItemCount == 0; //cekamo da obrada bude gotova
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] chunk = new byte[remainingChunk];
                byte[] hash = new byte[remainingChunk];
                Array.Copy(fileBytes, i * chunkSize, chunk, 0, remainingChunk);
                hash = sha256.ComputeHash(chunk);
                hash.CopyTo(hashedFile, chunkSize * i);
            }
                       
            void Func(object? obj)
            {
                int j = (int)obj;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] chunk = new byte[chunkSize];
                    byte[] hash = new byte[chunkSize];
                    Array.Copy(fileBytes, j * chunkSize, chunk, 0, chunkSize);
                    hash = sha256.ComputeHash(chunk);
                    hash.CopyTo(hashedFile, chunkSize * j);
                }
            }

            return hashedFile;
        }

        public static byte[] ThreadPoolHashFunction(string filePath)
        {
            //ThreadPool.SetMaxThreads(brojNiti, 0);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int chunkSize = 32;
            int chunkLength = fileBytes.Length / chunkSize;
            int remainingChunk = fileBytes.Length % chunkSize;
            byte[] hashedFile = new byte[chunkLength * (chunkSize) + 32];
            int i = 0;
            for (; i < chunkLength; i++)
                Func(i);


            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] chunk = new byte[remainingChunk];
                byte[] hash = new byte[remainingChunk];
                Array.Copy(fileBytes, i * chunkSize, chunk, 0, remainingChunk);
                hash = sha256.ComputeHash(chunk);
                hash.CopyTo(hashedFile, chunkSize * i);
            }

            void Func(object? obj)
            {
                int j = (int)obj;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] chunk = new byte[chunkSize];
                    byte[] hash = new byte[chunkSize];
                    Array.Copy(fileBytes, j * chunkSize, chunk, 0, chunkSize);
                    hash = sha256.ComputeHash(chunk);
                    hash.CopyTo(hashedFile, chunkSize * j);
                }
            }

            return hashedFile;
        }

        public static byte[] ThreadHashFunction(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] hashedFile = new byte[fileBytes.Length];
            Thread t = new Thread(() =>
            {
                Func(null);
            });
            t.Start();
            t.Join();

            void Func(object? obj)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    
                    hashedFile = sha256.ComputeHash(fileBytes);
                }
            }
            return hashedFile;
        }
    }
}
