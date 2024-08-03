using ReenbitNetCamp.Components;
using Microsoft.Azure.SignalR;
using ReenbitNetCamp.Services;
using ReenbitNetCamp.Models;
using Microsoft.EntityFrameworkCore;
namespace ReenbitNetCamp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

             builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            //Add SignalR
            builder.Services.AddSignalR()
                .AddAzureSignalR(Environment.GetEnvironmentVariable("SIGNALR_CONNECTION_STRING"));
            //Add Azure AI Service
            builder.Services.AddTransient<IAiService>                (
                x=>new AiService(Environment.GetEnvironmentVariable("AI_ENDPOINT")!, Environment.GetEnvironmentVariable("AI_API_KEY")!));
            //Add database context
            builder.Services.AddDbContext<ChatContext>(
                options => options.UseSqlServer(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")));
            builder.Services.AddBlazorBootstrap();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            //Map hub
            app.MapHub<ChatHub>(ChatHub.HubUrl);


            app.Run();
        }
    }
}
