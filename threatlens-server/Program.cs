using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using threatlens_server.Common;
using threatlens_server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options => options.AddDefaultPolicy(
        policy => policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "VSA Todo API", Version = "v1" }));

builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton(new MlModelService(@"C:\Users\Sead\Desktop\Github\Threatlens\AI Model\dynamic_trained_model(2).zip"));

builder.Services.AddSingleton<KafkaConsumerService>();
builder.Services.AddTransient<PacketCaptureService>();

builder.Services.AddSingleton(new KafkaConsumerConfig
{
    BootstrapServers = "localhost:29092",
    GroupId = "network-packets-consumer-group",
    Topic = "network-packets"
});



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

var consumerService = app.Services.GetRequiredService<KafkaConsumerService>();
var cts = new CancellationTokenSource();
Task.Run(() => consumerService.ConsumeMessages(cts.Token));

app.Run();