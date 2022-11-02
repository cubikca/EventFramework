using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace EventFramework.Service.Filters;

public class TokenConsumeFilter<T> : IFilter<ConsumeContext<T>>
where T : class
{
    private readonly IConfiguration _configuration;

    public TokenConsumeFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var token = context.Headers.Get<string>("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            await next.Send(context);
            return;
        }
        var domain = _configuration["Oidc:Domain"];
        var configuration = await OpenIdConnectConfigurationRetriever.GetAsync(
            $"https://{domain}/.well-known/openid-configuration", CancellationToken.None);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{domain}/",
            ValidateAudience = true,
            ValidAudience = _configuration["Oidc:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys
        };
        var handler = new JwtSecurityTokenHandler();
        Thread.CurrentPrincipal = handler.ValidateToken(token, validationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken accessToken) throw new Exception("Invalid token");
        if (Thread.CurrentPrincipal is ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity is not ClaimsIdentity identity) throw new Exception("null identity??");
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.RawData);
            var claims = await httpClient.GetFromJsonAsync<Dictionary<string, object>>($"https://{domain}/userinfo");
            if (claims != null)
                foreach (var (k, v) in claims)
                    identity.AddClaim(new Claim(k, v.ToString() ?? throw new Exception("null claim value")));
        }
        else
            throw new Exception("no claims principal??");
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}