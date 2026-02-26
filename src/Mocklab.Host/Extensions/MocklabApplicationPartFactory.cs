using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Mocklab.Host.Extensions;

/// <summary>
/// Returns no application parts, preventing MVC from auto-scanning
/// the Mocklab.Host assembly for controller types.
/// Controllers are registered explicitly via MocklabControllerFeatureProvider.
/// </summary>
public class MocklabApplicationPartFactory : ApplicationPartFactory
{
    public override IEnumerable<ApplicationPart> GetApplicationParts(Assembly assembly)
    {
        return Enumerable.Empty<ApplicationPart>();
    }
}
