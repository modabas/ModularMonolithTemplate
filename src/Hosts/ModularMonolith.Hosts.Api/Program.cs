using ModEndpoints;
using ModularMonolith.Hosts.Api.Extensions;
using ModularMonolith.Modules.FirstService.Extensions;
using ModularMonolith.Modules.SecondService.Extensions;
using ModularMonolith.Shared.MinimalApis.ServerTimeout;

var builder = WebApplication.CreateBuilder(args);

//Configure endpoints, problem detail responses and request validation pipeline
builder.AddWebServices();

// Add services to the container.
builder.AddFirstServiceModule();
builder.AddSecondServiceModule();

builder.AddMassTransit();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddSwagger();

builder.AddOrleans();

builder.AddOpenTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//app.UseAuthentication()
//  .UseAuthorization();

app.MapModEndpoints(
  (serviceProvider, builder, group, endpoint) =>
  {
    builder.WithSummary(endpoint.GetType().Name);
    var endpointFullName = endpoint.GetType().FullName;
    if (!string.IsNullOrWhiteSpace(endpointFullName))
    {
      builder.WithName(endpointFullName);
    }
    builder.WithServerTimeout(serviceProvider, endpoint)
      .AddEndpointFilter<ServerTimeoutFilter>();
  });

//do this after endpoints are configured
//so swagger ui will correctly show all versions of apis
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    var descriptions = app.DescribeApiVersions();

    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
      var url = $"/swagger/{description.GroupName}/swagger.json";
      var name = description.GroupName.ToUpperInvariant();
      options.SwaggerEndpoint(url, name);
    }
  });
}

app.Run();

