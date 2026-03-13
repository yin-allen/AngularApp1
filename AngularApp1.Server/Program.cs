var builder = WebApplication.CreateBuilder(args);

// 1. 註冊服務
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

// 2. 設定中間件順序
app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ?? 註解掉這行 (如果你前端是用 HTTP 59451 跑的話)
// app.UseHttpsRedirection(); 

// 3. 註冊路由 (順序很重要)
app.MapControllers();               // 先讓 API 路由去匹配請求
app.MapFallbackToFile("index.html"); // 如果 API 沒匹配到，才丟給 Angular

app.Run();