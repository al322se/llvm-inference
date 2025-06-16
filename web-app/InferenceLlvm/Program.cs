using InferenceLlvm;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Refit client for vLLM service
var vllmBaseUrl = builder.Configuration.GetValue<string>("VllmService:BaseUrl") ?? "http://vllm-service:8000";
builder.Services.AddRefitClient<IRerankerService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(vllmBaseUrl));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();