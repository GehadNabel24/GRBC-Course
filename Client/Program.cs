using Client;
using Client.Proto;
using Client.Services;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

var _configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();

//builder.Services.AddSingleton<TokenCredential>(sp =>
//{
//    var apiKeyProvider = sp.GetRequiredService<IAuthService>();
//    var apiKey = apiKeyProvider.GetApiKey();
//    return new TokenCredential(apiKey);
//});

//builder.Services.AddGrpcClient<Inventory.InventoryClient>((services, options) =>
//{
//    var address = services.GetRequiredService<IConfiguration>().GetValue<string>(Declartions.GrpcServiceAddressSettingName);
//    options.Address = new Uri(address);

//    // Use the custom TokenCredential for authentication
//    var tokenCredential = services.GetRequiredService<TokenCredential>();
//    options.Interceptors.Add(tokenCredential);
//});

builder.Services.AddGrpcClient<Inventory.InventoryClient>(options =>
{
    var address = _configuration.GetValue<string>(Declartions.GrpcServiceAddressSettingName);
    options.Address = new Uri(address);
}).AddCallCredentials((context, metadata, serviceProvider) =>
{
    var apiKeyProvider = serviceProvider.GetRequiredService<IAuthService>();
    var apiKey = apiKeyProvider.GetApiKey();
    metadata.Add(Declartions.ApiKeyHeaderName, apiKey); ;
    return Task.CompletedTask;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
