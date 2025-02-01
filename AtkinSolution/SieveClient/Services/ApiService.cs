using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SieveClient.Models;

namespace SieveClient.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private string? _token;

    public ApiService(string baseUrl)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public void SetToken(string token)
    {
        _token = token;
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        if (_token != null && !_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            SetToken(_token);
        }

        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _client.PostAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PatchAsync<T>(string endpoint, object data)
    {
        if (_token != null && !_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            SetToken(_token);
        }

        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _client.PatchAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> DeleteAsync<T>(string endpoint)
    {
        var response = await _client.DeleteAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<byte[]?> GetBinaryAsync(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Error: {response.StatusCode}");
        }
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]?> PostBinaryAsync(string endpoint, object data)
    {
        if (_token != null && !_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            SetToken(_token);
        }

        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _client.PostAsync(endpoint, content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Error: {response.StatusCode}");
        }
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = string.IsNullOrEmpty(content) 
                ? $"Ошибка: {response.StatusCode}" 
                : $"Ошибка: {(content.StartsWith("{") ? JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Message : content)}";
            throw new Exception(errorMessage);
        }

        try 
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<T>(content, options);
        }
        catch (JsonException)
        {
            throw new Exception($"Ошибка при обработке ответа сервера: {content}");
        }
    }
} 