using Microsoft.Azure.WebJobs.Description;
using System;

namespace Azure.Functions.Extensions.OData
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ODataAttribute : Attribute
    {
    }
}
