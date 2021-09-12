using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Keycloak.POC.Gateway
{
  public class Startup
  {
    IWebHostEnvironment ApplicationEnvironment;
    public Startup(IConfiguration configuration,IWebHostEnvironment  env)
    {
      Configuration = configuration;
      ApplicationEnvironment = env;
      var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddJsonFile("ocelot.json")
            .AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      IdentityModelEventSource.ShowPII = true;
      var authenticationProviderKey = "TestKey";
      services.AddAuthentication()
        //.AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(60))
        .AddJwtBearer(authenticationProviderKey, options =>
        {
          options.Authority = "http://keycloak:8080/auth/realms/master/";
          options.Audience =" ocelot";
          options.RequireHttpsMetadata = false;
          options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
          {
            ValidAudiences = new string[] { "master-realm", "account", "ocelot" }
          };
          options.Events = new JwtBearerEvents()
          {
            OnAuthenticationFailed = c =>
            {
              c.NoResult();

              c.Response.StatusCode = 500;
              c.Response.ContentType = "text/plain";
              if (ApplicationEnvironment.EnvironmentName.Equals("Development"))
              {
                c.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(c.Exception.Message));
                c.Response.BodyWriter.FlushAsync();
                return Task.CompletedTask;
              }
              c.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("An error occured processing your authentication."));
              c.Response.BodyWriter.FlushAsync();
              return Task.CompletedTask;
            }
          };
        });
        
      services.AddOcelot(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseHttpsRedirection();

      app.UseOcelot().Wait();
    }
  }
}
