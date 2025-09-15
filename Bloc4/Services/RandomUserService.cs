using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Bloc4.Data;
using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Services
{
    public class RandomUserService : IRandomUserService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _http;

        public RandomUserService(AppDbContext db, HttpClient? httpClient = null)
        {
            _db = db;
            _http = httpClient ?? new HttpClient();
        }

        public async Task<int> ImportRandomUsersAsync(int count = 10, string nat = "fr")
        {
            var url = $"https://randomuser.me/api/?results={count}&nat={nat}";
            using var res = await _http.GetAsync(url);
            res.EnsureSuccessStatusCode();

            var stream = await res.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<RandomUserResponse>(stream);
            if (payload?.Results == null) return 0;

            var services = await _db.Services.ToListAsync();
            var sites = await _db.Sites.ToListAsync();
            if (!services.Any() || !sites.Any()) return 0;

            var rnd = new Random();
            foreach (var r in payload.Results!)
            {
                var email = r.Email.ToLowerInvariant();
                if (await _db.Salaries.AnyAsync(s => s.Email == email)) continue;

                var sal = new Salarie
                {
                    Nom = Capitalize(r.Name.Last),
                    Prenom = Capitalize(r.Name.First),
                    Email = email,
                    TelephoneFixe = r.Phone,
                    TelephonePortable = r.Cell,
                    ServiceId = services[rnd.Next(services.Count)].Id,
                    SiteId = sites[rnd.Next(sites.Count)].Id
                };
                _db.Salaries.Add(sal);
            }

            return await _db.SaveChangesAsync();
        }

        private static string Capitalize(string s)
            => string.IsNullOrWhiteSpace(s) ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();

        public class RandomUserResponse { [JsonPropertyName("results")] public List<RandomUserItem>? Results { get; set; } }
        public class RandomUserItem
        {
            [JsonPropertyName("name")] public RandomUserName Name { get; set; } = new();
            [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
            [JsonPropertyName("phone")] public string Phone { get; set; } = string.Empty;
            [JsonPropertyName("cell")] public string Cell { get; set; } = string.Empty;
        }
        public class RandomUserName { [JsonPropertyName("first")] public string First { get; set; } = string.Empty; [JsonPropertyName("last")] public string Last { get; set; } = string.Empty; }
    }
}