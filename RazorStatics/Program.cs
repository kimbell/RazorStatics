
// you can just start the application on different runtimes
// net7 and net8 should crash on the InvalidOperationException below

var builder = WebApplication.CreateBuilder(args);

// our original application has a convention like this to handle SPA url
builder.Services.AddRazorPages(o => o.Conventions.AddPageRoute("/index", "{*url}"));

var app = builder.Build();

app.UsePathBase("/dilbert"); // if we remove this, things work on net7 and net8
app.Use((httpContext, next) =>
{
    // this code runs way before UseRouting(), so I would not expect it to know endpoint information yet.
    var ep = httpContext.GetEndpoint();
    if (ep != null)
    {
        // this exception is not triggered on net6, but getst triggered on net7 and net8
        throw new InvalidOperationException("Enpdoint should be null");
    }
    return next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
