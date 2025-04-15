using FisherYates.Models;
using FisherYates.Services;

namespace FisherYates
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            builder.Services.Configure<GlobalSettings>(builder.Configuration.GetSection("FisherYatesSetting"));
            builder.Services.AddMvc();
            builder.Services.AddScoped<IRandomNumberGenerator, DefaultRandomNumberGenerator>();
            builder.Services.AddScoped<IFisherYatesService, FisherYatesService>();

            // Build and configure the app
            var app = builder.Build();

            app.UseRouting();
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

            app.Run();
        }
    }
}


