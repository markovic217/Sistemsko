using System.Net;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Caching;
using System.Text;
using System.Collections;

namespace Projekat
{
    internal class Program
    {
        static TimeSpan vremeProsecno = new TimeSpan();
        static TimeSpan ukupnoVreme = new TimeSpan();
        static int putaProlazak = 1;
        static readonly string rootFolder = @"C:\Users\gornj\Desktop\Sistemsko\ProjekatGit\Fajlovi";
        static readonly string server = "http://localhost:5050/";
        static readonly MemoryCache cache = new MemoryCache("Memory cache");
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/"); 

            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
        }

        static void ProcessRequest(object state)
        {
            string result;
            Stopwatch stopwatch = new Stopwatch();
            byte[] hashedFile;

            HttpListenerContext context = (HttpListenerContext)state;

            stopwatch.Start();
            string filename = context.Request.Url.ToString().Substring(server.Length);

            if (string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("Nije naveden fajl!");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
                return;
            }

            CacheItem cachedItem = cache.GetCacheItem(filename);
            if (cachedItem != null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = ((byte[])cachedItem.Value).Length;
                context.Response.OutputStream.Write((byte[])cachedItem.Value, 0, ((byte[])cachedItem.Value).Length);
                context.Response.OutputStream.Close();
               // result = System.Text.Encoding.UTF8.GetString((byte[])cachedItem.Value);
               // Console.WriteLine(result);
                Console.WriteLine("Pisanje iz kesa uspesno!");
                return;
            }

            string filepath = Path.Combine(rootFolder, filename);

            if (!File.Exists(filepath))
            {
                Console.WriteLine("Ne postoji tekstualni fajl u folderu!");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            hashedFile = Functions.ThreadPoolChunkFunction(filepath);

            stopwatch.Stop();
  
            ukupnoVreme += stopwatch.Elapsed;
            vremeProsecno = ukupnoVreme / putaProlazak++;
            Console.WriteLine($"Proteklo vreme: {stopwatch.Elapsed}");
            Console.WriteLine($"Prosecno vreme izvrsenja: {vremeProsecno}");
            Console.WriteLine($"Broj prolazaka {putaProlazak - 1}");
            
            //Console.WriteLine($"Proteklo vreme: {stopwatch.Elapsed}");
            stopwatch.Reset();

            CacheItem cacheItem = new CacheItem(filename, hashedFile);
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            cacheItemPolicy.SlidingExpiration = new TimeSpan(0, 15, 0);
            cache.Add(cacheItem, cacheItemPolicy);



            //result = System.Text.Encoding.UTF8.GetString(hashedFile);
            //Console.WriteLine(result);
            //context.Response.ContentEncoding = Encoding.ASCII;
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = hashedFile.Length;
            context.Response.OutputStream.Write(hashedFile, 0, hashedFile.Length);           
            context.Response.OutputStream.Close();
                      
        }
    }
}