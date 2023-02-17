using ElasticSearchApi.BLL.DataService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSwaggerGen();
builder.Services.AddScoped<NewsService, NewsService>();
builder.Services.AddScoped<CategoryService, CategoryService>();



var app = builder.Build();



app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Elastic Search Api");
    //Swagger ekranýnýn gelmesini istediðimiz route routeprefix'e yazýlýr örn. localhost:1245/swmobile
    //yazýlmazsa proje ilk baþladýðýnda direk swagger ekraný gelecektir.
    options.RoutePrefix = "swmobile";

});


app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
