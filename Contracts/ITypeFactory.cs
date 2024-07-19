using Bytz.Common.Contracts;

namespace Bytz.Reflection.Emit.Contracts;

/// <summary>
/// factory to create dynamic classes from string/type dictionaries on the fly
/// </summary>
/// <remarks>
/// very rudimentary, all properties have public get/set accessors
/// </remarks>
public interface ITypeFactory
: IBytz
{
    /// <summary>
    /// create a class with specified public properties and types.
    /// </summary>
    /// <param name="fullName">the fully qualified name for the class to be created</param>
    /// <param name="properties">key-value pair implementiing idictionary with the names and public property types.</param>
    /// <returns>reflected class type</returns>
    Type CreateType(string fullName, IDictionary<string, Type> properties);

    /// <summary>
    /// create an anonymous type.
    /// </summary>
    /// <param name="types">key is the alias, type is the type</param>
    /// <returns>an "anonymous" type from the supplied key-value pair instance</returns>
    /// <remarks>
    /// *** initial implementation, in-progress. api likely not to change but could.
    /// </remarks>
    Type CreateAnyonymousType(IDictionary<string, Type> types);
}