using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddRazorPages(o => o.Conventions.AddPageRoute("/index", "{*url}"));

var app = builder.Build();

if (app.Configuration.GetValue("UsePathBaseNet7Workaround", false))
{
    const string key = "__GlobalEndpointRouteBuilder";
    var keyExisted = false;

    if (app is IApplicationBuilder a)
    {
        if (a.Properties.TryGetValue(key, out var routeBuilder))
        {
            a.Properties.Remove(key);
            keyExisted = true;
        }

        app.UsePathBase("/dilbert");

        // set it back to how it originally was
        if (keyExisted)
        {
            a.Properties.Add(key, routeBuilder);
        }
    }
}
else
{
    // removing the UsePathBase() on net7 removes the error
    app.UsePathBase("/dilbert");
}

//app.Use((httpContext, next) =>
//{
//    var ep = httpContext.GetEndpoint();
//    if (ep != null)
//    {
//        // since this is running before UseRouting(), how does it know it's endpoint?
//        throw new InvalidOperationException("Enpdoint should be null");
//    }
//    return next();
//});

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "Content"))
});
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

public partial class Program
{
}