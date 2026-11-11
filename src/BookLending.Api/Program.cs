using BookLending.Domain.Loans;
using BookLending.Application.Loans;
using BookLending.Domain.Reminders;
using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using BookLending.Infrastructure.Persistence;
using BookLending.Api.Extensions;
using BookLending.Api.OpenApi;
using BookLending.Application.Auth;
using BookLending.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Declares JWT Bearer auth so Swagger UI shows the Authorize button.
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});
builder.Services.AddProblemDetails();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IMemberRepository, EfMemberRepository>();
builder.Services.AddScoped<ILoanRepository, EfLoanRepository>();
builder.Services.AddSingleton<IReminderSender, EmailReminder>();
builder.Services.AddControllers();
builder.Services.AddDbContext<BookLendingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BookLending")));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddServiceBusMessaging(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    Console.WriteLine($"-->{context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"<-- {context.Request.Method} {context.Request.Path} completed in {sw.ElapsedMilliseconds} ms");
});


if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Book Lending API V1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
