using InferenciaApi.Configuration;
using InferenciaApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração tipada
builder.Services.Configure<InferenciaOptions>(
    builder.Configuration.GetSection(InferenciaOptions.SectionName)
);

// 2. Service de inferência (Singleton porque o ChatClient é thread-safe e caro de criar)
builder.Services.AddSingleton<IInferenciaService, InferenciaService>();

// 3. Controllers
builder.Services.AddControllers();

// 4. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. CORS (caso vá consumir de um front-end)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();