using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace ProjectorNamer
{
    internal static class GraphClient
    {
        private static IPublicClientApplication application = default!;

        public static async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(PublicClientApplicationOptions configuration, string[] scopes)
        {
            GraphServiceClient graphClient = new(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var token = await SignInUserAndGetToken(configuration, scopes);
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                    }));
            return await Task.FromResult(graphClient);
        }

        private static async Task<string> SignInUserAndGetToken(PublicClientApplicationOptions configuration, string[] scopes)
        {
            string authority = $"{configuration.Instance}{configuration.TenantId}";

            application = PublicClientApplicationBuilder
                .Create(configuration.ClientId)
                .WithAuthority(authority)
                .WithDefaultRedirectUri()
                .Build();

            AuthenticationResult result;
            try
            {
                var accounts = await application.GetAccountsAsync();
                result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                result = await application
                    .AcquireTokenInteractive(scopes)
                    .WithClaims(ex.Claims)
                    .ExecuteAsync();
            }
            return result.AccessToken;
        }
    }
}
