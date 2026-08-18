using Azure.Core;

namespace CLWebStore.Catalog.API;

// <summary>
// A dummy token credential for connecting to the Azure Key Vault Emulator.
// </summary>
public class DummyTokenCredential : TokenCredential
{
    // A structurally valid, but fake JWT
    private const string DummyJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(DummyJwt, DateTimeOffset.Now.AddDays(1));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
    }
}
