using EasyNetQ;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prometheus;
using Shared.Models;
using SmileTV.Data;
using SmileTV.Hubs;
using SmileTV.Models;
using SmileTV.UIModels;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
    .AllowAnyOrigin()
    //.WithOrigins("http://localhost:44448") // the Angular app url
    .AllowAnyMethod()
    .AllowAnyHeader());
    //.AllowCredentials());
});
builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

WebSocketOptions opt = new WebSocketOptions()
{
    KeepAliveInterval = new TimeSpan(50000)
};
app.UseWebSockets(opt);

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/client")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            var server = Control.Servers["Server1"];
            Client client = new Client(socketFinishedTcs, webSocket);

            server.Clients.Add(client);

            await socketFinishedTcs.Task;

            Control.Servers["Server1"].Clients.Remove(client);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseHttpMetrics();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.UseCors("CorsPolicy");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<QueueHub>("/hub/chat");
    endpoints.MapMetrics();
});

//app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallbackToFile("index.html");

KinesisController kinesis = new KinesisController();
KinesisController.Instance = kinesis;

QueueController queue = new QueueController(app.Services.GetRequiredService<IWebHostEnvironment>(), app.Services.GetRequiredService<IHubContext<QueueHub>>());

queue.StartUp();

Control.Servers.Add("Server1", new Server());

CountController.Counters.Add("topics_total_calls", Metrics.CreateCounter("topics_total_calls", "Number Of Topic Calls"));
CountController.Counters.Add("recieve_total_calls", Metrics.CreateCounter("recieve_total_calls", "Number Of Send/Recieve Calls"));
CountController.Counters.Add("random_total_calls", Metrics.CreateCounter("random_total_calls", "Number Of Random Calls"));
CountController.Counters.Add("button_total_calls", Metrics.CreateCounter("button_total_calls", "Number Of Button Update Calls"));

app.Run();