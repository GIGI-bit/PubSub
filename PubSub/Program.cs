using StackExchange.Redis;

namespace PubSub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            //var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
            var muxer = ConnectionMultiplexer.Connect(
           new ConfigurationOptions
           {
               EndPoints = { { "redis-14728.c326.us-east-1-3.ec2.redns.redis-cloud.com", 14728 } },
               User = "default",
               Password = "2cncW8bIijfJh68IgKu6Pm29jGvlnGck"
           });
            //redis - cli - u redis://default:2cncW8bIijfJh68IgKu6Pm29jGvlnGck@redis-14728.c326.us-east-1-3.ec2.redns.redis-cloud.com:14728
            builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Message}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
