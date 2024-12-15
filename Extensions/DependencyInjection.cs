using Auth;

namespace Api.Extensions;

public static class DependencyInjection
{
    public static void AddGrpcClients(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpcClient<Authenticator.AuthenticatorClient>(options =>
        {
            var grpcUri = builder.Configuration["GrpcSettings:AuthServiceUri"];
            options.Address = new Uri(grpcUri!);
        });
    }
}