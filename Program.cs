var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Suporte ao CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin() // Permite qualquer origem (domínio)
              .AllowAnyMethod() // Permite qualquer método (GET, POST, etc.)
              .AllowAnyHeader(); // Permite qualquer cabeçalho
    });
});

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware para arquivos estáticos
app.UseDefaultFiles(); // Permite carregar automaticamente o index.html
app.UseStaticFiles();  // Serve arquivos estáticos da pasta wwwroot

// Adiciona o middleware de CORS
app.UseCors();

// Middleware de fallback para o index.html
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 &&
        !Path.HasExtension(context.Request.Path.Value) &&
        context.Request.Path.Value != null)
    {
        Console.WriteLine("Fallback para index.html");
        context.Request.Path = "/index.html";
        await context.Response.SendFileAsync("wwwroot/index.html");
    }
});

// Endpoint para a conversão de texto
app.MapPost("/convert", (TextRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest("O texto não pode ser nulo");
    }

    var response = new TextResponse
    {
        Bold = ConvertToBold(request.Text),
        Italic = ConvertToItalic(request.Text),
        BoldItalic = ConvertToBoldItalic(request.Text),
        Undeline = ConvertUndeline(request.Text)
    };

    return Results.Ok(response);
})
.WithName("TextConverter")
.WithOpenApi();

app.Run();

//Função para conversão para unicode 
string ConvertToBold(string input)
{
    return string.Concat(input.Select(char => char.IsLetter(c) ? (char)(char + 0x1D400 - 'A') : c));
}

string ConvertToBoldItalic(string input)
{
    return string.Concat(input.Select(char => char.IsLetter(c) ? (char)(char + 0x1D468 - 'A') : c));
}

string ConvertToItalic(string input)
{
    return string.Concat(input.Select(char => char.IsLetter(c) ? (char)(char + 0x1D434 - 'A') : c));
}

string ConvertUndeline(string input)
{
    return string.Concat(input.Select(char => char.IsLetter(c) ? $"{c}\u0332" : c));

}

// Classes de requisição e resposta
public class TextRequest
{
    public string Text { get; set; } = string.Empty;
}

public class TextResponse
{
    public string Bold { get; set; } = string.Empty;
    public string Italic { get; set; } = string.Empty;
    public string BoldItalic { get; set; } = string.Empty;
    public string Undeline { get; set; } = string.Empty;
}
