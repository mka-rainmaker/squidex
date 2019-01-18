// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class AzureAdAuthentificationSerivce
    {
        public static AuthenticationBuilder AddMyAzureActiveDirectoryAuthentication(
            this AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration config)
        {
            var azureOptions = config.GetSection("azureAd").Get<AzureADOptions>();
            if (azureOptions?.ClientId != null)
            {
                authBuilder.AddAzureAD(options =>
                {
                    options.Instance = azureOptions.Instance;
                    options.ClientId = azureOptions.ClientId;
                    options.TenantId = azureOptions.TenantId;
                    options.Domain = azureOptions.Domain;
                    options.CallbackPath = azureOptions.CallbackPath;
                    options.CookieSchemeName = "AzureADCookie";
                });
                //authBuilder.AddAzureAD(options => config.Bind("azureAd", options));

                services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
                {
                    options.Authority = options.Authority + "/v2.0/";

                    // Per the code below, this application signs in users in any Work and School
                    // accounts and any Microsoft Personal Accounts.
                    // If you want to direct Azure AD to restrict the users that can sign-in, change
                    // the tenant value of the appsettings.json file in the following way:
                    // - only Work and School accounts => 'organizations'
                    // - only Microsoft Personal accounts => 'consumers'
                    // - Work and School and Personal accounts => 'common'

                    // If you want to restrict the users that can sign-in to only one tenant
                    // set the tenant value in the appsettings.json file to the tenant ID of this
                    // organization, and set ValidateIssuer below to true.

                    // If you want to restrict the users that can sign-in to several organizations
                    // Set the tenant value in the appsettings.json file to 'organizations', set
                    // ValidateIssuer, above to 'true', and add the issuers you want to accept to the
                    // options.TokenValidationParameters.ValidIssuers collection

                    options.TokenValidationParameters.ValidateIssuer = false;
                });
            }

            return authBuilder;
        }
    }
}
