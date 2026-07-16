using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudentRegistration.Client;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Operations;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AcademicApiClient>();
builder.Services.AddScoped<IdentityApiClient>();
builder.Services.AddScoped<OperationsApiClient>();

await builder.Build().RunAsync();
