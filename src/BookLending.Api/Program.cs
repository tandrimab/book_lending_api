using BookLending.Domain.Loans;
using BookLending.Domain.Reminders;
using BookLending.Domain.Greetings;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<LoanService>();
builder.Services.AddSingleton<IReminderSender, EmailReminder>();
builder.Services.AddScoped<GreetingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    Console.WriteLine($"-->{context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"<-- {context.Request.Method} {context.Request.Path} completed in {sw.ElapsedMilliseconds} ms");
});

app.MapGet("/loans/stats", (LoanService loanService) =>
{
    var (total, overdue) = loanService.GetLoanStats();
    return Results.Ok(new { total, overdue });
});

app.MapGet("/greet/{name}", (string name, GreetingService greetServ) =>
{
    var greet = greetServ.Greet(name);
    return Results.Ok(new { greet });
});
app.Run();
