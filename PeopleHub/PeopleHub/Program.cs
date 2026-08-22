
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

            // Our mock "database" services. Singleton = one shared instance/list
            // for the whole app's lifetime (in-memory data needs to persist
            // across requests, not be recreated per request like the default).
            //
            // Order matters for readability, not for correctness — the container
            // resolves the graph itself: RequestService needs IEmployeeService,
            // and AttendanceService needs both.
            builder.Services.AddSingleton<IEmployeeService, EmployeeService>();
            builder.Services.AddSingleton<IRequestService, RequestService>();
            builder.Services.AddSingleton<IAttendanceService, AttendanceService>();

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

                // Asking for the services here builds all three singletons NOW,
                // so the ~100k attendance rows are generated during startup
                // rather than during the first unlucky request — and the check
                // throws immediately if the generated data is inconsistent.
                SeedSelfCheck.Verify(
                    app.Services.GetRequiredService<IEmployeeService>(),
                    app.Services.GetRequiredService<IAttendanceService>(),
                    app.Services.GetRequiredService<IRequestService>());
            }

            app.UseHttpsRedirection();

            app.UseCors(reactAppPolicy);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
