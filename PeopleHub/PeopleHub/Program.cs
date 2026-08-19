
using PeopleHub.Services;

namespace PeopleHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Our mock "database" service. Singleton = one shared instance/list
            // for the whole app's lifetime (in-memory data needs to persist
            // across requests, not be recreated per request like the default).
            builder.Services.AddSingleton<IEmployeeService, EmployeeService>();

            // Let the React dev server (Vite, localhost:5173) call this API
            // from the browser.
            const string reactAppPolicy = "ReactApp";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(reactAppPolicy, policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(reactAppPolicy);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
