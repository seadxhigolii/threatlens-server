using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using threatlens_server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options => options.AddDefaultPolicy(
        policy => policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "SAI API", Version = "v1" }));

builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var modelPath = builder.Configuration["Model:Path"];
builder.Services.AddSingleton(new MlModelService(modelPath));

builder.Services.AddScoped<LabelEncoder>();

builder.Services.AddTransient<PacketCaptureService>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

//app.UseAuthorization();
app.MapControllers();

app.Run();