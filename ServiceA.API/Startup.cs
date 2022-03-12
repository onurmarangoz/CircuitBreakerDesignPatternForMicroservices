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
            }).AddPolicyHandler(GetAdvanceCircuitBreakerPolicy()); //Circuit Breaker Pattern ile i�lem yap�lmas� gerekti�i belirtiyoruz.
        }


        private IAsyncPolicy<HttpResponseMessage> GetBasicCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                        .CircuitBreakerAsync(3, //ka� ba�ar�s�z istekte devreye girecek
                                             TimeSpan.FromSeconds(10), //ne kadar s�re bekleyecek.
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
                        .AdvancedCircuitBreakerAsync(0.5,  // Hata oran�n� belirtir. �rne�in gelen istekler toplam�nda %50 lik hata oran� varsa devreye girer
                                                     TimeSpan.FromSeconds(10), // Bak�lacak zaman aral���n� belirtir. 
                                                     4, // Min hata miktar�n� belirtir. �rne�in belirtilen s�re i�in de oran tutsa dahi 4 hata olmad��� s�rece devreye girmez.
                                                     TimeSpan.FromSeconds(20), //Devrenin kesilme s�resini belirtir. Circuit Breaker ba�ar�l� �ekilde servis ile ileti�imi keserse 20 saniye snra tekrar a��lacakt�r.
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
