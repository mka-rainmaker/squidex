// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Squidex.Config.Authentication
{
    public static class AzureAdAuthentificationSerivce
    {
        public static AuthenticationBuilder AddMyAzureActiveDirectoryAuthentication(
            this AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration config)
        {
            var azureOptions = config.GetSection("AzureAd").Get<AzureADOptions>();
            if (azureOptions?.ClientId != null)
            {
                authBuilder.AddAzureAD(options =>
                {
                    options.Instance = azureOptions.Instance;
                    options.ClientId = azureOptions.ClientId;
                    options.TenantId = azureOptions.TenantId;
                    options.ClientSecret = azureOptions.ClientSecret;
                    options.Domain = azureOptions.Domain;
                    options.CallbackPath = azureOptions.CallbackPath;
                    options.CookieSchemeName = "Identity.External";
                });

                services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
                {
                    options.Authority = options.Authority + "/v2.0/";
                    options.TokenValidationParameters.ValidateIssuer = false;
                });
            }

            return authBuilder;
        }
    }
}
