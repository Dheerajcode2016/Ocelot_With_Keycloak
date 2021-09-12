using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ConsoleTest
{
  class Program
  {
    static void Main(string[] args)
    {
      HttpClient client = new HttpClient();
      var formContent = new FormUrlEncodedContent(new[]
      {
          new KeyValuePair<string, string>("client_id", "ocelot"),
          new KeyValuePair<string, string>("grant_type", "password"),
          new KeyValuePair<string, string>("client_secret", "10533ff2-c44c-48c4-8aeb-7fa4a2dfdcdf"),
          new KeyValuePair<string, string>("scope", "openid"),
          new KeyValuePair<string, string>("username", "bob"),
          new KeyValuePair<string, string>("password", "bob"),
      });
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
     var response =  client.PostAsync("http://keycloak:8080/auth/realms/master/protocol/openid-connect/token", formContent).Result;
     string res = "";
      var tken = new { access_token = "", expires_in = 0, refresh_expires_in = 0, refresh_token = "", token_type = "", id_token = "", session_state = "", scope = "" };
      using (HttpContent content = response.Content)
      {
        // ... Read the string.
        Task<string> result = content.ReadAsStringAsync();
        res = result.Result;
      }
      var a = JsonConvert.DeserializeAnonymousType(res, tken);
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", a.access_token);
     var d = client.GetAsync("http://keycloak.poc.gateway/todos/1").Result;
    }
    public static object Deserialize(string content)
    {
      var serializer = new JsonSerializer();

      using (TextReader tr = new StringReader(content))
      using (var reader = new JsonTextReader(tr))
      {
        return serializer.Deserialize(reader);
      }
    }
  }
}
