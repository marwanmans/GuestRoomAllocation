using GuestRoomAllocation.Application;
using GuestRoomAllocation.Infrastructure;
using GuestRoomAllocation.Persistence;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages(options =>
{
    // Configure Razor Pages options
    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddControllers(); // ← ADD THIS LINE

// Add Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);

// Add HTTP context accessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Add model binding and validation
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // ← ADD THIS LINE

// Optional: Add some helpful startup information
app.Logger.LogInformation("Guest Room Allocation application started");
app.Logger.LogInformation("Navigate to: {Url}", app.Urls.FirstOrDefault() ?? "https://localhost:7225");

app.Run();