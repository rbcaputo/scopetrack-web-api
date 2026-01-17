using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Services;
using ScopeTrack.Application.Validators;
using ScopeTrack.Infrastructure.Data;
using ScopeTrack.Infrastructure.Interceptors;

namespace ScopeTrack.API
{
  public class Program
  {
    public static void Main(string[] args)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      // Database configuration with interceptor
      builder.Services.AddDbContext<ScopeTrackDbContext>(options =>
      {
        options.UseSqlServer(
          builder.Configuration.GetConnectionString("DefaultConnection"),
          sqlOptions => sqlOptions.MigrationsAssembly("ScopeTrack.Infrastructure")
        );
        options.AddInterceptors(new ActivityLogInterceptor());
      });

      // Register application services
      builder.Services.AddScoped<IClientService, ClientService>();
      builder.Services.AddScoped<IContractService, ContractService>();
      builder.Services.AddScoped<IDeliverableService, DeliverableService>();
      builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

      // CORS configuration
      builder.Services.AddCors(options =>
      {
        options.AddPolicy("AllowAll", policy =>
        {
          policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
      });

      // Controllers with FluentValidation
      builder.Services.AddControllers();

      // FluentValidation - register validators
      builder.Services.AddValidatorsFromAssemblyContaining<ClientPostDTOValidator>();

      // Swagger/OpenAPI
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(options =>
      {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
          Title = "ScopeTack API",
          Version = "v1",
          Description = "REST API for tracking clients, contracts, and deliverables"
        });
      });

      WebApplication app = builder.Build();

      // Configure HTTP request pipeline
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "ScopeTrack API v1");
          options.RoutePrefix = string.Empty; // Swagger at root
        });
      }

      app.UseHttpsRedirection();

      // Enable CORS
      app.UseCors("AllowAll");

      app.UseAuthorization();
      app.MapControllers();

      app.Run();
    }
  }
}
