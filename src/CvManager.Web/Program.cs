using CvManager.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppOptions(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddBusinessServices();
builder.Services.ConfigureIdentity();
builder.Services.AddExternalLogins(builder.Configuration);
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureForwardedHeaders();
builder.Services.AddWebUi();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAppRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.MapAppEndpoints();

app.Run();
