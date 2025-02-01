using System;
using System.Threading.Tasks;
using SieveClient.Models;
using SieveClient.Services;

namespace SieveClient;

partial class Program
{
    private static readonly ApiService _api = new("http://localhost:5000/api/");
    private static string? _token;

    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            if (string.IsNullOrEmpty(_token))
            {
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Вход");
                Console.WriteLine("0. Выход");

                var choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            await Register();
                            break;
                        case "2":
                            await Login();
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Клиент решета Аткина");
                Console.WriteLine("1. Получить N первых простых чисел");
                Console.WriteLine("2. Получить решето Аткина (строка)");
                Console.WriteLine("3. Получить решето как псевдокартинку");
                Console.WriteLine("4. Получить решето как строку base64");
                Console.WriteLine("5. Получить решето как бинарный файл");
                Console.WriteLine("6. Получить решето как URL");
                Console.WriteLine("7. Просмотр истории запросов");
                Console.WriteLine("8. Очистить историю запросов");
                Console.WriteLine("9. Сменить пароль");
                Console.WriteLine("10. Выйти из аккаунта");
                Console.WriteLine("0. Выход");

                var choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            await GetNPrimes();
                            break;
                        case "2":
                            await GetPrimesUpToN();
                            break;
                        case "3":
                            await GetVisualization("text");
                            break;
                        case "4":
                            await GetVisualization("base64");
                            break;
                        case "5":
                            await GetVisualization("binary");
                            break;
                        case "6":
                            await GetVisualization("url");
                            break;
                        case "7":
                            await ViewHistory();
                            break;
                        case "8":
                            await ClearHistory();
                            break;
                        case "9":
                            await ChangePassword();
                            break;
                        case "10":
                            _token = null;
                            _api.SetToken(string.Empty);
                            Console.WriteLine("Выход выполнен успешно");
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    private static async Task Register()
    {
        Console.Write("Введите имя пользователя: ");
        var username = Console.ReadLine() ?? "";
        Console.Write("Введите email: ");
        var email = Console.ReadLine() ?? "";
        Console.Write("Введите пароль: ");
        var password = Console.ReadLine() ?? "";

        var response = await _api.PostAsync<AuthResponse>("Auth/register", new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password
        });

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            _token = response.Token;
            _api.SetToken(_token);
            Console.WriteLine("Регистрация успешно завершена!");
        }
        else
        {
            var errorMessage = response?.Message;
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = "Ошибка регистрации. Нет сообщения об ошибке.";
            }
            throw new Exception(errorMessage);
        }
    }

    private static async Task Login()
    {
        Console.Write("Введите имя пользователя: ");
        var username = Console.ReadLine() ?? "";
        Console.Write("Введите пароль: ");
        var password = Console.ReadLine() ?? "";

        var response = await _api.PostAsync<AuthResponse>("Auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        });

        if (response?.Success == true)
        {
            _token = response.Token;
            _api.SetToken(_token);
            Console.WriteLine("Вход выполнен успешно!");
        }
        else
        {
            throw new Exception(response?.Message ?? "Ошибка входа");
        }
    }

    private static async Task GetNPrimes()
    {
        Console.Write("Введите N: ");
        if (int.TryParse(Console.ReadLine(), out int n))
        {
            var primes = await _api.PostAsync<List<int>>("Sieve/primes/count", n);
            Console.WriteLine($"Первые {n} простых чисел:");
            Console.WriteLine(string.Join(", ", primes ?? new List<int>()));
        }
    }

    private static async Task GetPrimesUpToN()
    {
        Console.Write("Введите N: ");
        if (int.TryParse(Console.ReadLine(), out int n))
        {
            var primes = await _api.PostAsync<List<int>>("Sieve/primes/range", n);
            Console.WriteLine($"Простые числа до {n}:");
            Console.WriteLine(string.Join(", ", primes ?? new List<int>()));
        }
    }

    private static async Task GetVisualization(string format)
    {
        Console.Write("Введите N: ");
        if (int.TryParse(Console.ReadLine(), out int n))
        {
            switch (format)
            {
                case "text":
                    var textResponse = await _api.PostAsync<VisualizationResponse>($"Sieve/visualization?format={format}", n);
                    Console.WriteLine("Визуализация:");
                    Console.WriteLine(textResponse?.Result);
                    break;
                case "base64":
                    var base64Response = await _api.PostAsync<VisualizationResponse>($"Sieve/visualization?format={format}", n);
                    Console.WriteLine("Строка Base64:");
                    Console.WriteLine(base64Response?.Result);
                    break;
                case "url":
                    var urlResponse = await _api.PostAsync<VisualizationResponse>($"Sieve/visualization?format={format}", n);
                    Console.WriteLine("URL изображения:");
                    Console.WriteLine(urlResponse?.Url);
                    break;
                case "binary":
                    var binaryData = await _api.PostBinaryAsync($"Sieve/visualization?format={format}", n);
                    if (binaryData != null)
                    {
                        var fileName = $"sieve_{n}.png";
                        await File.WriteAllBytesAsync(fileName, binaryData);
                        Console.WriteLine($"Бинарный файл сохранен как {fileName}");
                    }
                    break;
            }
        }
    }

    private static async Task ViewHistory()
    {
        var history = await _api.GetAsync<List<HistoryItem>>("History");
        if (history != null)
        {
            Console.WriteLine("История запросов пользователя:");
            foreach (var item in history)
            {
                Console.WriteLine($"[{item.Timestamp:yyyy-MM-dd HH:mm:ss}] {item.Method} {item.Endpoint}");
            }
        }
    }

    private static async Task ClearHistory()
    {
        var response = await _api.DeleteAsync<ApiResponse>("History");
        if (response?.Success == true)
        {
            Console.WriteLine("История успешно очищена");
        }
        else
        {
            throw new Exception(response?.Message ?? "Ошибка при очистке истории");
        }
    }

    private static async Task ChangePassword()
    {
        Console.Write("Введите текущий пароль: ");
        var currentPassword = Console.ReadLine() ?? "";
        Console.Write("Введите новый пароль: ");
        var newPassword = Console.ReadLine() ?? "";

        var response = await _api.PatchAsync<AuthResponse>("Auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        });

        if (response?.Success == true)
        {
            Console.WriteLine("Пароль успешно изменен");
        }
        else
        {
            throw new Exception(response?.Message ?? "Ошибка при смене пароля");
        }
    }
} 