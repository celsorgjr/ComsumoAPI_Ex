using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ComsumoAPI_Ex.Services
{
    public class Consumir
    {
        private string _urlBase;


        private async Task<List<string>> ConsultarValues(HttpClient client)
        {
            HttpResponseMessage response = client.GetAsync(
                _urlBase + "values").Result;

            Console.WriteLine();
            if (response.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<List<String>>(response.Content.ReadAsStringAsync().Result);
            else
                return new List<string> { "Token provavelmente expirado!" };
        }

        public async Task<List<string>> Chamada()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile($"appsettings.json");
            var config = builder.Build();

            _urlBase = config.GetSection("API_Access:UrlBase").Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage respToken = client.PostAsync(
                    _urlBase + "login", new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            UserID = config.GetSection("API_Access:UserID").Value,
                            AccessKey = config.GetSection("API_Access:AccessKey").Value
                        }), Encoding.UTF8, "application/json")).Result;

                string conteudo =
                    respToken.Content.ReadAsStringAsync().Result;
                Console.WriteLine(conteudo);

                if (respToken.StatusCode == HttpStatusCode.OK)
                {
                    Token token = JsonConvert.DeserializeObject<Token>(conteudo);
                    if (token.Authenticated)
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token.AccessToken);
                        return await ConsultarValues(client);
                    }else
                        return new List<string> { "Não authenticado" };
                }else
                    return new List<string> { "Erro : " + respToken.StatusCode } ;

            }
        }
    }
}
