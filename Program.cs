using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Suporte ao CORS
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

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware para arquivos estáticos
app.UseDefaultFiles();
app.UseStaticFiles();

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
        context.Request.Path = "/index.html";
        await context.Response.SendFileAsync("wwwroot/index.html");
    }
});

// Endpoint para a conversão de texto
app.MapPost("/convert", (TextRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new { Message = "O texto não pode ser nulo ou vazio." });
    }

    var response = new TextResponse
    {
        Bold = ConvertToBold(request.Text),
        Italic = ConvertToItalic(request.Text),
        BoldItalic = ConvertToBoldItalic(request.Text),
        Underline = ConvertToUnderline(request.Text)
    };

    return Results.Ok(response);
})
.WithName("TextConverter")
.WithOpenApi();

app.Run();

// Funções de conversão de texto
string ConvertToBold(string input)
{
    return string.Concat(RemoveAccents(input).Select(c =>
        char.IsLetter(c) && c < 128
            ? (char.IsUpper(c)
                ? (char)(c - 'A' + 0x1D400)
                : (char)(c - 'a' + 0x1D41A))
            : c));
}

string ConvertToItalic(string input)
{
    return string.Concat(RemoveAccents(input).Select(c =>
        char.IsLetter(c) && c < 128
            ? (char.IsUpper(c)
                ? (char)(c - 'A' + 0x1D434)
                : (char)(c - 'a' + 0x1D44E))
            : c));
}

string ConvertToBoldItalic(string input)
{
    return string.Concat(RemoveAccents(input).Select(c =>
        char.IsLetter(c) && c < 128
            ? (char.IsUpper(c)
                ? (char)(c - 'A' + 0x1D468)
                : (char)(c - 'a' + 0x1D482))
            : c));
}

string ConvertToUnderline(string input)
{
    return string.Concat(input.Select(c =>
        char.IsWhiteSpace(c)
            ? " "
            : $"{c}̲"));
}

// Função para remover acentos
string RemoveAccents(string input)
{
    var normalized = input.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();

    foreach (var c in normalized)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }

    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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
    public string Underline { get; set; } = string.Empty;
}
