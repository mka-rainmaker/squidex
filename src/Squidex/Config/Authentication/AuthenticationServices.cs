﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddMyAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>();
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddMyAzureActiveDirectoryAuthentication(services, config);

            services.AddAuthentication()
                .AddMyIdentityServerAuthentication(identityOptions, config)
                .AddCookie();
        }
    }
}
