using Asp.Versioning;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using ModCaches.Orleans.Server.Cluster;
using ModularMonolith.Hosts.Api.ApiVersioning;
using ModularMonolith.Modules.FirstService.Extensions;
using ModularMonolith.Modules.SecondService.Extensions;
using ModularMonolith.Shared.Data;
using ModularMonolith.Shared.Data.SimpleOutbox;
using ModularMonolith.Shared.Masstransit;
using ModularMonolith.Shared.MinimalApis.ServerTimeout;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ModularMonolith.Hosts.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
  public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
  {
    //Open Telemetry Configuration
    // Define some important constants to initialize tracing with
    var serviceName = "ModularMonolith.Hosts.Api";
    var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "?";

    var otelAttributes = new Dictionary<string, object>
        {
            { "deployment.environment", builder.Environment.EnvironmentName }
        };

    builder.Logging.AddOpenTelemetry(logging =>
    {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;
      // Set a service name
      logging.SetResourceBuilder(
        ResourceBuilder.CreateDefault()
          .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
          .AddAttributes(otelAttributes)
          .AddTelemetrySdk());
    });

    builder.Services.AddOpenTelemetry()
      .WithMetrics(metrics =>
      {
        // Set a service name
        metrics.SetResourceBuilder(
          ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(otelAttributes)
            .AddTelemetrySdk());
        metrics.AddRuntimeInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Net.Http")
            .AddMeter("Microsoft.Orleans");
      })
      .WithTracing(tracing =>
      {
        if (builder.Environment.IsDevelopment())
        {
          tracing.SetSampler(new AlwaysOnSampler());
        }

        // Set a service name
        tracing.SetResourceBuilder(
          ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(otelAttributes)
            .AddTelemetrySdk());

        //tracerProviderBuilder
        //    .AddSource(serviceName)

        tracing
          //Microsoft Orleans Activity Source
          .AddSource("Microsoft.Orleans.Runtime")
          .AddSource("Microsoft.Orleans.Application")

          //MassTransit ActivitySource
          .AddSource(DiagnosticHeaders.DefaultListenerName)

          .AddSource(SimpleOutboxDefinitions.OutboxPublisherActivitySourceName)
          .AddSource(PgDbMigrationServiceDefinitions.ActivitySourceName)

          .AddHttpClientInstrumentation()
          .AddAspNetCoreInstrumentation()
          .AddNpgsql();
      });

    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    if (useOtlpExporter)
    {
      builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
      builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
      builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
    }
    return builder;
  }

  public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
  {
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen(options =>
    {
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
      {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
      });
      options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecuritySchemeReference("Bearer", document),
          new List<string>()
        }
      });

      options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
      options.CustomSchemaIds(type => type.ToString());
    });
    return builder;
  }

  public static WebApplicationBuilder AddMassTransit(this WebApplicationBuilder builder)
  {
    var options = builder.Configuration.GetSection("MtOptions").Get<MtOptions>();
    options ??= new MtOptions();
    builder.Services.AddMassTransit(x =>
    {
      x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(options.EndpointNamePrefix, true));

      x.AddFirstServiceModule();
      x.AddSecondServiceModule();

      x.UsingRabbitMq((context, cfg) =>
      {
        cfg.AutoStart = true;
        cfg.ConfigureEndpoints(context);
        cfg.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter(options.EntityNamePrefix, true));
        cfg.Host(
          options.RabbitMq.Host,
          options.RabbitMq.Port,
          options.RabbitMq.VirtualHost,
          c =>
          {
            c.Username(options.RabbitMq.Username);
            c.Password(options.RabbitMq.Password);
          });
      });
    });
    return builder;
  }
  public static WebApplicationBuilder AddWebServices(this WebApplicationBuilder builder)
  {
    builder.Services.AddProblemDetails(options =>
    {
      options.CustomizeProblemDetails = context =>
      {
        context.ProblemDetails.Instance =
          $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId",
          context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId",
          activity?.Id);
      };
    });

    builder.Services
      .AddApiVersioning(opt =>
      {
        opt.ApiVersionReader = new UrlSegmentApiVersionReader();
      })
      .AddApiExplorer(options =>
      {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
      });

    builder.AddServerTimeout();

    return builder;
  }

  public static WebApplicationBuilder AddOrleans(this WebApplicationBuilder builder)
  {
    builder.Services.AddOrleansClusterCache(options =>
    {
      options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
      options.SlidingExpiration = TimeSpan.FromMinutes(1);
    });

    builder.Host.UseOrleans(siloBuilder =>
    {
      siloBuilder.UseLocalhostClustering();
      //Open telemetry propagation
      siloBuilder.AddActivityPropagation();
    });
    return builder;
  }
}
