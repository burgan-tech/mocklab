using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Mocklab.Host.Controllers;

namespace Mocklab.Host.Extensions;

/// <summary>
/// Applies a configurable route prefix to the CatchAllController.
/// When RoutePrefix is set (e.g. "mock"), the catch-all route becomes "mock/{**catchAll}"
/// instead of "{**catchAll}", so only requests under that prefix are intercepted.
/// </summary>
public class MocklabRoutePrefixConvention(string routePrefix) : IApplicationModelConvention
{
    private readonly string _routePrefix = routePrefix.Trim('/');

    public void Apply(ApplicationModel application)
    {
        if (string.IsNullOrEmpty(_routePrefix))
            return;

        var catchAllController = application.Controllers
            .FirstOrDefault(c => c.ControllerType == typeof(CatchAllController));

        if (catchAllController == null) return;

        foreach (var action in catchAllController.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel.Template =
                        $"{_routePrefix}/{selector.AttributeRouteModel.Template}";
                }
            }
        }
    }
}
