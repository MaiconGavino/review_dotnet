var builder = WebApplication.CreateBuilder(args);

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

app.MapPost("/convert", (TextRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest("O texto não pode ser nulo");
    }
    var response = new TextResponse
    {
        Bold = $"**{request.Text}**",
        Italic = $"*{request.Text}*",
        BoldItalic = $"***{request.Text}***",
        Undeline = $"__{request.Text}__"
    };
    return Results.Ok(response);
})
.WithName("TextConverter")
.WithOpenApi();

app.Run();

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