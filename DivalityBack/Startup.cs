using System.Diagnostics.CodeAnalysis;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace DivalityBack
{
    [ExcludeFromCodeCoverage]
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
            services.AddCors();
            services.AddRazorPages();
            services.Configure<DivalityDatabaseSettings>(
                Configuration.GetSection(nameof(DivalityDatabaseSettings)));

            services.AddSingleton<IDivalityDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<DivalityDatabaseSettings>>().Value);
            services.AddSingleton<UsersCRUDService>();
            services.AddSingleton<UsersService>();
            services.AddSingleton<CardsCRUDService>();
            services.AddSingleton<CardsService>(); 
            services.AddSingleton<AuctionHousesCRUDService>();
            services.AddSingleton<AuctionHouseService>(); 
            services.AddSingleton<UtilServices>();
            services.AddSingleton<WebsocketService>();
            services.AddSingleton<FriendRequestsCRUDService>();
            
            services.AddHostedService<LongRunningService>();
            services.AddSingleton<BackgroundWorkerQueue>();
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Divality", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Divality v1"));
            }

            app.UseRouting();
            app.UseCors(
                options => options.WithOrigins("http://example.com").AllowAnyMethod()
            );
            
            app.UseAuthorization();

            app.UseWebSockets();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
