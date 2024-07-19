using Bytz.Reflection.Emit.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace Bytz.Reflection.Emit;

/// <summary>
/// emit types from a dictionary of name/types via reflection
/// </summary>
/// <remarks>
/// found on some stack overflow page years ago and adapted/customized.
/// </remarks>
public class TypeFactory
: ITypeFactory
{
    public Type CreateType
    (
        string fullName,
        IDictionary<string, Type> properties
    )
    {
        AssemblyBuilder assemblyBuilder = CreateAssemblyBuilder("emitted");
        ModuleBuilder moduleBuilder = CreateModuleBuilder(assemblyBuilder);
        TypeBuilder typeBuilder = CreateTypeBuilder(moduleBuilder, fullName);

        BuildProperties(typeBuilder, properties);

        return typeBuilder.CreateTypeInfo();
    }

    /// <summary>
    /// create an assembly builder class instance of the specified name.
    /// </summary>
    /// <param name="assemblyName">name for the assembly builder</param>
    /// <returns>an instance of an assembly builder for the specified assembly name</returns>
    private static AssemblyBuilder CreateAssemblyBuilder
    (
        string assemblyName
    )
    {
        return AssemblyBuilder
        .DefineDynamicAssembly
        (
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.Run
        );
    }

    /// <summary>
    /// Create a ModuleBuilder instance.
    /// </summary>
    /// <param name="builder">Instance of an AssemblyBuilder</param>
    /// <returns>An instance of a ModuleBuilder</returns>
    private static ModuleBuilder CreateModuleBuilder
    (
        AssemblyBuilder builder
    )
    {
        return builder.DefineDynamicModule("Classes");
    }

    private static TypeBuilder CreateTypeBuilder
    (
        ModuleBuilder moduleBuilder,
        string fullName
    )
    {
        return moduleBuilder
        .DefineType
        (
            fullName,
            TypeAttributes.Public
            | TypeAttributes.Class
            | TypeAttributes.AutoClass
            | TypeAttributes.AnsiClass
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AutoLayout,
            null
        );
    }

    /// <summary>
    /// Build properties.
    /// </summary>
    /// <param name="typeBuilder">instance of a typebuilder</param>
    /// <param name="properties">key is the name of the property, value is the type of the property</param>
    private static void BuildProperties
    (
        TypeBuilder typeBuilder,
        IDictionary<string, Type> properties
    )
    {
        properties
            .ToList()
            .ForEach
            (pr =>
            {
                FieldBuilder fieldBuilder = CreateFieldBuilder(typeBuilder, pr.Key, pr.Value);
                PropertyBuilder propertyBuilder = CreatePropertyBuilder(typeBuilder, pr.Key, pr.Value);

                CreateSetAccessor(typeBuilder, fieldBuilder, propertyBuilder, pr.Key, pr.Value);
                CreateGetAccessor(typeBuilder, fieldBuilder, propertyBuilder, pr.Key, pr.Value);
            }
            );
    }

    /// <summary>
    /// Create a FieldBuilder for the PropertyItem.
    /// </summary>
    /// <param name="typeBuilder">Instance of a TypeBuilder</param>
    /// <param name="property">Instance of PropertyItem</param>
    /// <returns>Returns an instance of a FieldBuilder</returns>
    private static FieldBuilder CreateFieldBuilder
    (
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType
    )
    {
        return typeBuilder.DefineField
        (
            $"_{propertyName}",
            propertyType,
            FieldAttributes.Private
        );
    }

    /// <summary>
    /// Create an instance of a PropertyBuilder.
    /// </summary>
    /// <param name="typeBuilder">Instance of a TypeBuilder</param>
    /// <param name="propertyName">name for the property</param>
    /// <param name="propertyType">type of the property</param>
    /// <returns></returns>
    private static PropertyBuilder CreatePropertyBuilder
    (
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType
    )
    {
        return typeBuilder.DefineProperty
        (
            propertyName,
            PropertyAttributes.HasDefault,
            propertyType,
            null
        );
    }

    /// <summary>
    /// Create a Set accessor for the FieldBuilder.
    /// </summary>
    /// <param name="typeBuilder">Instance of a TypeBuilder</param>
    /// <param name="fieldBuilder">Instance of a FieldBuilder</param>
    /// <param name="propertyBuilder">Instance of a PropertyBuilder</param>
    /// <param name="propertyName">Instance of a PropertyItem</param>
    /// <param name="propertyType">type of the property</param>
    private static void CreateSetAccessor
    (
        TypeBuilder typeBuilder,
        FieldBuilder fieldBuilder,
        PropertyBuilder propertyBuilder,
        string propertyName,
        Type propertyType
    )
    {
        MethodBuilder setMethodBuilder =
            typeBuilder.DefineMethod
            (
                $"set_{propertyName}",
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig,
                null,
                [propertyType]
            );

        ILGenerator setIl = setMethodBuilder.GetILGenerator();

        Label modifyProperty = setIl.DefineLabel();
        Label exitSet = setIl.DefineLabel();

        setIl.MarkLabel(modifyProperty);

        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);

        setIl.Emit(OpCodes.Nop);
        setIl.MarkLabel(exitSet);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetSetMethod(setMethodBuilder);
    }


    /// <summary>
    /// create a get accessopr for a property.
    /// </summary>
    /// <param name="typeBuilder">instance of a typebuilder</param>
    /// <param name="fieldBuilder">instance of a fieldbuilder</param>
    /// <param name="propertyBuilder">instance of a propertybuilder</param>
    /// <param name="propertyName">name for the property</param>
    /// <param name="propertyType">type of the property</param>
    private static void CreateGetAccessor
    (
        TypeBuilder typeBuilder,
        FieldBuilder fieldBuilder,
        PropertyBuilder propertyBuilder,
        string propertyName,
        Type propertyType
    )
    {
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod
        (
            $"get_{propertyName}",
            MethodAttributes.Public
            | MethodAttributes.SpecialName
            | MethodAttributes.HideBySig,
            propertyType,
            Type.EmptyTypes
        );

        ILGenerator getIl = getMethodBuilder.GetILGenerator();
        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);
    }


    public Type CreateAnyonymousType(IDictionary<string, Type> types)
    {
        string typeName = $"<>f__AnonymousType`{types.Count}";

        return CreateType(typeName, types);
    }
}