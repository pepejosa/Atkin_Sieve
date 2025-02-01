using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Services;
using SkiaSharp;
using Microsoft.AspNetCore.Hosting;

namespace WebApi.Services
{
    public class SieveService
    {
        private readonly IWebHostEnvironment _environment;

        public SieveService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public List<int> GetPrimesUpToN(int limit)
        {
            bool[] sieve = new bool[limit + 1];
            for (int i = 0; i <= limit; i++)
                sieve[i] = false;

            // Предварительное просеивание
            for (int x = 1; x * x <= limit; x++)
            {
                for (int y = 1; y * y <= limit; y++)
                {
                    int n = (4 * x * x) + (y * y);
                    if (n <= limit && (n % 12 == 1 || n % 12 == 5))
                        sieve[n] ^= true;

                    n = (3 * x * x) + (y * y);
                    if (n <= limit && n % 12 == 7)
                        sieve[n] ^= true;

                    n = (3 * x * x) - (y * y);
                    if (x > y && n <= limit && n % 12 == 11)
                        sieve[n] ^= true;
                }
            }

            // Отметить все квадраты простых чисел как непростые
            for (int r = 5; r * r <= limit; r++)
            {
                if (sieve[r])
                {
                    for (int i = r * r; i <= limit; i += r * r)
                        sieve[i] = false;
                }
            }

            List<int> primes = new List<int>();
            primes.Add(2);
            primes.Add(3);
            for (int a = 5; a <= limit; a++)
                if (sieve[a])
                    primes.Add(a);

            return primes;
        }

        public List<int> GetNPrimes(int n)
        {
            if (n <= 0) return new List<int>();
            
            // Используем более точную оценку верхней границы для n-го простого числа
            // p(n) ≤ n(ln(n) + ln(ln(n))) для n ≥ 6
            int limit;
            if (n < 6)
            {
                limit = 13; // Достаточно для первых 5 простых чисел
            }
            else
            {
                limit = (int)(n * (Math.Log(n) + Math.Log(Math.Log(n))) * 1.2);
            }
            
            var allPrimes = GetPrimesUpToN(limit);
            
            // Если мы получили меньше простых чисел, чем нужно, увеличиваем предел
            while (allPrimes.Count < n)
            {
                limit = (int)(limit * 1.5);
                allPrimes = GetPrimesUpToN(limit);
            }
            
            return allPrimes.Take(n).ToList();
        }

        public string GenerateVisualization(int n, string format)
        {
            var sieve = new bool[n + 1];
            var primes = GetPrimesUpToN(n);
            foreach (var prime in primes)
            {
                sieve[prime] = true;
            }

            switch (format.ToLower())
            {
                case "text":
                    return GenerateTextVisualization(sieve);
                case "base64":
                    return GenerateBase64Visualization(sieve);
                default:
                    throw new ArgumentException("Неподдерживаемый формат визуализации");
            }
        }

        private string GenerateTextVisualization(bool[] sieve)
        {
            var sb = new StringBuilder();
            int width = (int)Math.Sqrt(sieve.Length);

            for (int i = 0; i < sieve.Length; i++)
            {
                if (i > 0 && i % width == 0)
                    sb.AppendLine();
                sb.Append(sieve[i] ? "█" : "░");
            }

            return sb.ToString();
        }

        private string GenerateBase64Visualization(bool[] sieve)
        {
            int size = (int)Math.Sqrt(sieve.Length);
            using (var surface = SKSurface.Create(new SKImageInfo(size, size)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                using (var paint = new SKPaint { Color = SKColors.Black })
                {
                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            int index = y * size + x;
                            if (index < sieve.Length && sieve[index])
                            {
                                canvas.DrawPoint(x, y, paint);
                            }
                        }
                    }
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var ms = new MemoryStream())
                {
                    data.SaveTo(ms);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public byte[] GenerateImageAsBinary(int n)
        {
            bool[] sieve = GenerateSieve(n);
            using var surface = GenerateImageSurface(sieve);
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public async Task<string> SaveImageAndGetUrl(int n)
        {
            var fileName = $"sieve_{n}_{DateTime.UtcNow.Ticks}.png";
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesPath = Path.Combine(webRootPath, "images");
            
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            var filePath = Path.Combine(imagesPath, fileName);
            var imageData = GenerateImageAsBinary(n);
            await File.WriteAllBytesAsync(filePath, imageData);

            return fileName;
        }

        private bool[] GenerateSieve(int n)
        {
            var primes = GetPrimesUpToN(n);
            var sieve = new bool[n + 1];
            foreach (var prime in primes)
            {
                sieve[prime] = true;
            }
            return sieve;
        }

        private SKSurface GenerateImageSurface(bool[] sieve)
        {
            int size = (int)Math.Sqrt(sieve.Length);
            var surface = SKSurface.Create(new SKImageInfo(size, size));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using (var paint = new SKPaint { Color = SKColors.Black })
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        int index = y * size + x;
                        if (index < sieve.Length && sieve[index])
                        {
                            canvas.DrawPoint(x, y, paint);
                        }
                    }
                }
            }

            return surface;
        }
    }
} 