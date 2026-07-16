using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudentRegistration.Client;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Client.Features.Scheduling;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AcademicApiClient>();
builder.Services.AddScoped<CatalogueApiClient>();
builder.Services.AddScoped<IdentityApiClient>();
builder.Services.AddScoped<OperationsApiClient>();
builder.Services.AddScoped<RegistrationApiClient>();
builder.Services.AddScoped<SchedulingApiClient>();

await builder.Build().RunAsync();
