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
        private static object lockObj = new object();
        public static byte[] ThreadPoolChunkFunction(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int chunkSize = 32;
            int chunkCount = fileBytes.Length / chunkSize;
            int remainingChunk = fileBytes.Length % chunkSize;
            byte[] hashedFile = new byte[(chunkCount + 1 )* chunkSize];
            int i = 0;
            for (; i < chunkCount; i++)
                ThreadPool.QueueUserWorkItem(Func, i);
            bool gotova_obrada = false;
            while (!gotova_obrada)
            {
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 0, 100));
                gotova_obrada = ThreadPool.PendingWorkItemCount == 0;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] chunk = new byte[chunkSize];
                byte[] hash;
                Array.Copy(fileBytes, i * chunkSize, chunk, 0, remainingChunk);
                hash = sha256.ComputeHash(chunk);
                hash.CopyTo(hashedFile, chunkSize * i);
            }

            void Func(object? obj)
            {
                //lock (lockObj)
                //{
                int i = (int)obj;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] chunk = new byte[chunkSize];
                    byte[] hash;
                    Array.Copy(fileBytes, i * chunkSize, chunk, 0, chunkSize);
                    hash = sha256.ComputeHash(chunk);
                    hash.CopyTo(hashedFile, chunkSize * i);
                }
                //}
            }

            return hashedFile;
        }
    }
}
