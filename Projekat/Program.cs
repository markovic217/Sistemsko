using System.Net;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Projekat
{
    internal class Program
    {
        static readonly string rootFolder = @"C:\Users\gornj\Desktop\Sistemsko\Projekat\Fajlovi"; // promeniti putanju u kojoj su fajlovi koji će biti kriptovani
        static readonly string server = "http://localhost:5050/";
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/"); // promeniti po potrebi

            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(ProcessRequest, context);
            }
        }

        static void ProcessRequest(object state)
        {
            HttpListenerContext context = (HttpListenerContext)state;
            string filename = context.Request.Url.ToString().Substring(server.Length);
            Console.WriteLine(filename);
            if (string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("Nije naveden fajl!");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
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

            byte[] hash;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(fs);
                Console.WriteLine("Uspesno hesiranje fajla!");
            }

            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = hash.Length;
            context.Response.OutputStream.Write(hash, 0, hash.Length);
            context.Response.OutputStream.Close();
        }
    }
}