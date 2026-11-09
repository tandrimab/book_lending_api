using BookLending.Domain.Loans;
using BookLending.Domain.Reminders;
using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using BookLending.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IMemberRepository, EfMemberRepository>();
builder.Services.AddScoped<ILoanRepository, EfLoanRepository>();
builder.Services.AddSingleton<IReminderSender, EmailReminder>();
builder.Services.AddControllers();
builder.Services.AddDbContext<BookLendingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BookLending")));

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

app.MapControllers();
app.Run();
