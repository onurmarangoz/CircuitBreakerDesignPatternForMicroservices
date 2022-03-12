using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceA.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceA.API", Version = "v1" });
            });

            services.AddHttpClient<ProductService>(x =>
            {
                x.BaseAddress = new Uri("https://localhost:5003/api/products/");
            }).AddPolicyHandler(GetAdvanceCircuitBreakerPolicy()); //Circuit Breaker Pattern ile iþlem yapýlmasý gerektiði belirtiyoruz.
        }


        private IAsyncPolicy<HttpResponseMessage> GetBasicCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                        .CircuitBreakerAsync(3, //kaç baþarýsýz istekte devreye girecek
                                             TimeSpan.FromSeconds(10), //ne kadar süre bekleyecek.
                                              onBreak: (arg1,arg2) =>
                                              {
                                                  Debug.WriteLine("Circuit Breaker => on break");
                                              },
                                              onReset: () =>
                                              {
                                                  Debug.WriteLine("Circuit Breaker => on reset");
                                              },
                                              onHalfOpen: () =>
                                              {
                                                  Debug.WriteLine("Circuit Breaker => on half open");
                                              }); 
        }

        private IAsyncPolicy<HttpResponseMessage> GetAdvanceCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                        .AdvancedCircuitBreakerAsync(0.5,  // Hata oranýný belirtir. Örneðin gelen istekler toplamýnda %50 lik hata oraný varsa devreye girer
                                                     TimeSpan.FromSeconds(10), // Bakýlacak zaman aralýðýný belirtir. 
                                                     4, // Min hata miktarýný belirtir. Örneðin belirtilen süre için de oran tutsa dahi 4 hata olmadýðý sürece devreye girmez.
                                                     TimeSpan.FromSeconds(20), //Devrenin kesilme süresini belirtir. Circuit Breaker baþarýlý þekilde servis ile iletiþimi keserse 20 saniye snra tekrar açýlacaktýr.
                                                     onBreak: (arg1, arg2) =>
                                                     {
                                                         Debug.WriteLine("Circuit Breaker => on break");
                                                     },
                                                     onReset: () =>
                                                     {
                                                         Debug.WriteLine("Circuit Breaker => on reset");
                                                     },
                                                     onHalfOpen: () =>
                                                     {
                                                         Debug.WriteLine("Circuit Breaker => on half open");
                                                     });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceA.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
