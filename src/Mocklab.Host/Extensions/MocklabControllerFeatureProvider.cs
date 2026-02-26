using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Mocklab.Host.Controllers;

namespace Mocklab.Host.Extensions;

/// <summary>
/// Registers Mocklab controllers explicitly by type, avoiding full assembly scanning
/// that can cause ReflectionTypeLoadException in consumer projects.
/// </summary>
internal class MocklabControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var controllerTypes = new[]
        {
            typeof(CatchAllController),
            typeof(CollectionAdminController),
            typeof(DataBucketAdminController),
            typeof(FolderAdminController),
            typeof(MockAdminController),
            typeof(RequestLogAdminController),
        };

        foreach (var type in controllerTypes)
        {
            var typeInfo = type.GetTypeInfo();
            if (!feature.Controllers.Contains(typeInfo))
            {
                feature.Controllers.Add(typeInfo);
            }
        }
    }
}
