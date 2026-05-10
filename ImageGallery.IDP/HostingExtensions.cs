using Serilog;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace ImageGallery.IDP;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;

                 // Override the issuer URI to match host or container networking
                var publicOrigin = builder.Configuration["IDP_PUBLIC_ORIGIN"];
                if (!string.IsNullOrWhiteSpace(publicOrigin))
                {
                    options.IssuerUri = publicOrigin;
                }
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.GetClients(builder.Configuration))
            .AddTestUsers(TestUsers.Users);

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        };

        var trustForwardHeaders = app.Configuration.GetValue<bool>("TRUST_FORWARD_HEADERS", false);

        if (trustForwardHeaders)
        {
            // Docker / Kubernetes → trust ingress / nginx
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
        }
        else
        {
            // Production → lock this down (placeholder for now)
            forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
        }

        app.UseForwardedHeaders(forwardedHeadersOptions);

        // ============================================================
        // IDP PATH BASE NORMALIZATION
        // ============================================================
        //
        // The Identity Provider is exposed at "/idp" in ALL environments
        // (local, Docker, Kubernetes). Reverse proxies DO NOT rewrite
        // the path, so requests arrive as:
        //
        //   /idp/...
        //
        // ASP.NET Core + IdentityServer expect:
        //
        //   PathBase = /idp
        //   Path     = /connect/... (etc)
        //
        // This middleware normalizes the request by splitting "/idp"
        // into PathBase and Path so OIDC flows work correctly.
        //
        // DESIGN DECISION:
        // - No path rewriting in nginx or ingress
        // - No X-Forwarded-Prefix usage
        // - Same behavior in all environments
        //
        // Do NOT replace this with app.UsePathBase("/idp")
        // or ingress rewrite rules, as that can break OIDC flows.
        //
        // FAILURE SYMPTOMS IF MISCONFIGURED:
        // - Login redirects loop or fail
        // - "An error occurred" after login
        // - 404 on /connect/authorize or /.well-known endpoints
        // ============================================================
        app.Use((context, next) =>
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/idp", out var remainder))
            {
                context.Request.PathBase = "/idp";
                context.Request.Path = remainder;
            }

            return next();
        });

        //Logging must be AFTER the above so you'll see the correct values
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("RequestDebug");

            logger.LogWarning("---- INCOMING REQUEST ----");
            logger.LogWarning("Scheme: {Scheme}", context.Request.Scheme);
            logger.LogWarning("Host: {Host}", context.Request.Host);
            logger.LogWarning("PathBase: {PathBase}", context.Request.PathBase);
            logger.LogWarning("Path: {Path}", context.Request.Path);

            foreach (var header in context.Request.Headers)
            {
                logger.LogWarning("Header: {Key} = {Value}", header.Key, header.Value);
            }

            await next();
        });

        // DO NOT enable:
        // PathBase is already set dynamically by the middleware above.
        // Using UsePathBase("/idp") here would apply it twice and break OIDC flows.
        //app.UsePathBase("/idp");

        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
