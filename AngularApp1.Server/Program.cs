using Core.Entity;
using Core.Interface;
using Core.Repository;
using Core.Service;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using Scrutor; 
var builder = WebApplication.CreateBuilder(args);
var connectString = builder.Configuration.GetConnectionString("Yogurt");
builder.Services.AddDbContext<YogurtContext>(options =>
    options.UseMySql(connectString, ServerVersion.AutoDetect(connectString)));
//register Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


// builder.Services.AddOpenApi();// OpenApi 預設只會產生一個 JSON 檔案，並不包含 UI
builder.Services.AddControllers();

// 1. 註冊服務
builder.Services.Scan(scan => scan
    // 1. 指定掃描包含 ITestService 的那個 Assembly (Core 專案)
    // 2. 也掃描包含 Program 的這個 Assembly (Server 專案)
    .FromAssembliesOf(typeof(Program), typeof(Core.Interface.ITestService))

    // 3. 找出所有類別
    .AddClasses(classes => classes
        // 確保你的實作類別結尾是 Service
        .Where(t => t.Name.EndsWith("Service")||
        t.Name.EndsWith("Repository")))

    // 4. 註冊為它所實作的介面
    .AsImplementedInterfaces()

    // 5. 設定生命週期
    .WithScopedLifetime());

// 2. 註冊 NSwag (取代原本的 AddOpenApi)
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "AngularApp1 API";
    document.Version = "v1";
    document.Description = "AngularApp1 測試環境";

    // 🔑 加入 JWT 鎖頭設定
    document.AddSecurity("Bearer", new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "請輸入: Bearer {你的Token}"
    });

    document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});
var app = builder.Build();

// 2. 設定中間件順序
app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi(); 
    app.UseOpenApi();
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
    });
}

// ?? 註解掉這行 (如果你前端是用 HTTP 59451 跑的話)
// app.UseHttpsRedirection(); 

// 3. 註冊路由 (順序很重要)
app.MapControllers();               // 先讓 API 路由去匹配請求
app.MapFallbackToFile("index.html"); // 如果 API 沒匹配到，才丟給 Angular

app.Run();