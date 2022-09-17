using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace WebApp.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class AssociatedAssemblyNameAttribute : Attribute
{
    private static readonly ConcurrentDictionary<Type, AssociatedAssemblyNameAttribute?> s_attributeCache = new ConcurrentDictionary<Type, AssociatedAssemblyNameAttribute?>();

    public static AssociatedAssemblyNameAttribute? GetCachedFor(Type type) => s_attributeCache.GetOrAdd(type, type => type.GetCustomAttribute<AssociatedAssemblyNameAttribute>());

    public AssociatedAssemblyNameAttribute(string assemblyName)
    {
        AssemblyName = new AssemblyName(assemblyName);
    }

    public AssemblyName AssemblyName { get; }
}
