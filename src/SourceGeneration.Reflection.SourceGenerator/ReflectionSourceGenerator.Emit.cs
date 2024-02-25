﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SourceGeneration.Reflection;

public partial class ReflectionSourceGenerator
{
    private static void Emit(SourceProductionContext context, List<SourceTypeInfo> types, CancellationToken cancellationToken)
    {
        CSharpCodeBuilder builder = new();

        foreach (SourceTypeInfo type in types)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Emit(builder, type, cancellationToken);

            context.AddSource($"__SourceReflectorInitializer.{type.FullName}.g.cs", builder.ToString());
            builder.Clear();
        }


        builder.AppendAutoGeneratedComment();
        builder.AppendBlock("internal static partial class __SourceReflectorInitializer", () =>
        {
            builder.AppendLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
            builder.AppendBlock("public static void Initialize()", () =>
            {
                foreach (SourceTypeInfo type in types)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    builder.AppendLine($"global::SourceGeneration.Reflection.SourceReflector.Add({type.FullName.Replace('.', '_')});");

                    //Emit(builder, type, cancellationToken);
                }
            });
        });

        context.AddSource($"__SourceReflectorInitializer.g.cs", builder.ToString());
    }

    private static void Emit(CSharpCodeBuilder builder, SourceTypeInfo type, CancellationToken cancellationToken)
    {
        builder.AppendAutoGeneratedComment();
        builder.AppendBlock("internal static partial class __SourceReflectorInitializer", () =>
        {
            builder.AppendBlock($"private static readonly global::SourceGeneration.Reflection.SourceTypeInfo {type.FullName.Replace('.', '_')} =  new global::SourceGeneration.Reflection.SourceTypeInfo()", () =>
            {
                builder.AppendAssignment("Name", type.Name);
                builder.AppendAssignment("IsStatic", type.IsStatic);
                builder.AppendAssignment("IsRecord", type.IsRecord);
                builder.AppendAssignment("IsStruct", type.IsStruct);
                builder.AppendAssignment("IsEnum", type.IsEnum);
                builder.AppendAssignment("IsReadOnly", type.IsReadOnly);

                builder.AppendLine($"Type = typeof({type.FullGlobalName}),");
                if (type.BaseType != null)
                {
                    builder.AppendLine($"BaseType = typeof({type.BaseType}),");
                }
                if (type.EnumUnderlyingType != null)
                {
                    builder.AppendLine($"EnumUnderlyingType = typeof({type.EnumUnderlyingType}),");
                }
                builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{type.Accessibility},");

                if (type.Fields.Count > 0)
                {
                    builder.AppendBlock("DeclaredFields = new global::SourceGeneration.Reflection.SourceFieldInfo[]", () =>
                    {
                        foreach (var field in type.Fields)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            builder.AppendIndent();
                            builder.Append($"new global::SourceGeneration.Reflection.SourceFieldInfo(() => typeof({type.FullGlobalName}).GetField(\"{field.Name}\", ");
                            AppendBindingFlags(builder, field.Accessibility, field.IsStatic);
                            builder.Append("))");
                            builder.AppendLine();

                            builder.AppendBlock(() =>
                            {
                                builder.AppendLine($"FieldType = typeof({field.FieldType}),");
                                builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{field.Accessibility},");
                                builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{field.NullableAnnotation},");
                                builder.AppendAssignment("Name", field.Name);
                                builder.AppendAssignment("IsStatic", field.IsStatic);
                                builder.AppendAssignment("IsRequired", field.IsRequired);
                                builder.AppendAssignment("IsReadOnly", field.IsReadOnly);
                                builder.AppendAssignment("IsConst", field.IsConst);

                                if (!type.IsGenericTypeDefinition &&
                                    !type.IsRefLikeType &&
                                    field.Accessibility != Accessibility.Private && field.Accessibility != Accessibility.Protected)
                                {
                                    if (field.IsConst)
                                    {
                                        builder.AppendLine($@"GetValue = _ => {CSharpCodeBuilder.GetConstantLiteral(field.ConstantValue)},");
                                    }
                                    else if (field.IsStatic)
                                    {
                                        builder.AppendLine($@"GetValue = instance => {type.FullGlobalName}.{field.Name},");
                                    }
                                    else
                                    {
                                        builder.AppendLine($@"GetValue = instance => (({type.FullGlobalName})instance).{field.Name},");
                                    }

                                    if (!type.IsStruct && !field.IsReadOnly && !field.IsConst)
                                    {
                                        if (field.IsStatic)
                                        {
                                            builder.AppendLine($@"SetValue = (instance, value) => {type.FullGlobalName}.{field.Name} = ({field.FieldType})value");
                                        }
                                        else
                                        {
                                            builder.AppendLine($@"SetValue = (instance, value) => (({type.FullGlobalName})instance).{field.Name} = ({field.FieldType})value");
                                        }
                                    }
                                }

                            }, ",");
                        }
                    }, ",");
                }
                else
                {
                    builder.AppendLine("DeclaredFields = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceFieldInfo>(),");
                }

                if (type.Properties.Count > 0)
                {
                    builder.AppendBlock("DeclaredProperties = new global::SourceGeneration.Reflection.SourcePropertyInfo[]", () =>
                    {
                        foreach (var property in type.Properties)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            builder.AppendIndent();
                            builder.Append($"new global::SourceGeneration.Reflection.SourcePropertyInfo(() => typeof({type.FullGlobalName}).GetProperty(\"{property.Name}\", ");
                            AppendBindingFlags(builder, property.Accessibility, property.IsStatic);
                            builder.Append("))");
                            builder.AppendLine();

                            builder.AppendBlock(() =>
                            {
                                builder.AppendLine($"PropertyType = typeof({property.PropertyType}),");
                                builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{property.Accessibility},");
                                builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{property.NullableAnnotation},");
                                builder.AppendAssignment("Name", property.Name);
                                builder.AppendAssignment("IsVirtual", property.IsVirtual);
                                builder.AppendAssignment("IsStatic", property.IsStatic);
                                builder.AppendAssignment("IsRequired", property.IsRequired);
                                builder.AppendAssignment("IsAbstract", property.IsAbstract);
                                builder.AppendAssignment("IsInitOnly", property.IsInitOnly);

                                builder.AppendAssignment("CanWrite", property.CanWrite);
                                builder.AppendAssignment("CanRead", property.CanRead);

                                if (!type.IsGenericTypeDefinition &&
                                    !type.IsRefLikeType &&
                                    property.Accessibility != Accessibility.Private && property.Accessibility != Accessibility.Protected)
                                {
                                    if (property.CanRead && property.GetMethodAccessibility != Accessibility.Private && property.GetMethodAccessibility != Accessibility.Protected)
                                    {
                                        if (property.IsStatic)
                                        {
                                            builder.AppendLine($@"GetValue = instance => {type.FullGlobalName}.{property.Name},");
                                        }
                                        else
                                        {
                                            builder.AppendLine($@"GetValue = instance => (({type.FullGlobalName})instance).{property.Name},");
                                        }
                                    }

                                    if (!type.IsStruct && !property.IsInitOnly && property.CanWrite && property.SetMethodAccessibility != Accessibility.Private && property.SetMethodAccessibility != Accessibility.Protected)
                                    {
                                        if (property.IsStatic)
                                        {
                                            builder.AppendLine($@"SetValue = (instance, value) => {type.FullGlobalName}.{property.Name} = ({property.PropertyType})value");
                                        }
                                        else
                                        {
                                            builder.AppendLine($@"SetValue = (instance, value) => (({type.FullGlobalName})instance).{property.Name} = ({property.PropertyType})value");
                                        }
                                    }
                                }

                            }, ",");
                        }
                    }, ",");
                }
                else
                {
                    builder.AppendLine("DeclaredProperties = global::System.Array.Empty<global::SourceGeneration.Reflection.SourcePropertyInfo>(),");
                }

                if (type.Methods.Count > 0)
                {
                    builder.AppendBlock("DeclaredMethods = new global::SourceGeneration.Reflection.SourceMethodInfo[]", () =>
                    {
                        foreach (var method in type.Methods)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            builder.AppendIndent();

                            if (method.IsGenericMethod)
                            {
                                builder.Append($"new global::SourceGeneration.Reflection.SourceMethodInfo(() => global::SourceGeneration.Reflection.ReflectionExtensions.FindGenericMethod(typeof({type.FullGlobalName}), \"{method.Name}\", {method.TypeParameters.Length}, ");

                                builder.AppendArrayInitializer("global::System.String", method.Parameters.Select(x => $"\"{x.DisplayType}\"").ToArray());
                            }
                            else
                            {
                                builder.Append($"new global::SourceGeneration.Reflection.SourceMethodInfo(() => typeof({type.FullGlobalName}).GetMethod(\"{method.Name}\", ");
                                AppendBindingFlags(builder, method.Accessibility, method.IsStatic);
                                builder.Append(", ");
                                builder.AppendArrayInitializer("global::System.Type", method.Parameters.Select(x => $"typeof({x.ParameterType})").ToArray());
                            }
                            builder.Append("))");
                            builder.AppendLine();

                            builder.AppendBlock(() =>
                            {
                                builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{method.Accessibility},");
                                if (method.ReturnType != null)
                                {
                                    builder.AppendLine($"ReturnType = typeof({method.ReturnType}),");
                                }
                                builder.AppendLine($"ReturnNullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{method.ReturnNullableAnnotation},");

                                builder.AppendAssignment("Name", method.Name);
                                builder.AppendAssignment("IsStatic", method.IsStatic);

                                EmitParameters(builder, method.Parameters);

                                if (!type.IsGenericTypeDefinition && !type.IsRefLikeType && method.CanInvoke())
                                {
                                    builder.AppendIndent();
                                    builder.Append("Invoke = (instance, parameters) => ");

                                    if (method.ReturnType == "void")
                                    {
                                        builder.Append("{ ");
                                    }

                                    if (method.IsStatic)
                                    {
                                        builder.Append($"{type.FullGlobalName}.{method.Name}(");
                                    }
                                    else
                                    {
                                        builder.Append($"(({type.FullGlobalName})instance).{method.Name}(");
                                    }
                                    for (int i = 0; i < method.Parameters.Count; i++)
                                    {
                                        SourceParameterInfo parameter = method.Parameters[i];
                                        if (parameter.IsTypeParameter)
                                        {
                                            var typeParameter = method.TypeParameters.First(x => x.Name == parameter.ParameterType);
                                            if (typeParameter.ConstraintTypes.Length == 0)
                                            {
                                                builder.Append($"parameters[{i}]");
                                            }
                                            else
                                            {
                                                builder.Append($"({typeParameter.ConstraintTypes[0]})parameters[{i}]");
                                            }
                                        }
                                        else
                                        {
                                            builder.Append($"({parameter.ParameterType})parameters[{i}]");
                                        }

                                        if (i < method.Parameters.Count - 1)
                                        {
                                            builder.Append(", ");
                                        }
                                    }
                                    builder.Append(")");

                                    if (method.ReturnType == "void")
                                    {
                                        builder.Append("; return null; }");
                                    }

                                    builder.AppendLine();
                                }

                            }, ",");
                        }
                    }, ",");
                }
                else
                {
                    builder.AppendLine("DeclaredMethods = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceMethodInfo>(),");
                }

                if (type.Constructors.Count > 0)
                {
                    builder.AppendBlock("DeclaredConstructors = new global::SourceGeneration.Reflection.SourceConstructorInfo[]", () =>
                    {
                        foreach (var constructor in type.Constructors)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            builder.AppendIndent();
                            builder.Append($"new global::SourceGeneration.Reflection.SourceConstructorInfo(() => typeof({type.FullGlobalName}).GetConstructor(");
                            AppendBindingFlags(builder, constructor.Accessibility, constructor.IsStatic);
                            builder.Append(", ");
                            builder.AppendArrayInitializer("global::System.Type", constructor.Parameters.Select(x => $"typeof({x.ParameterType})").ToArray());
                            builder.Append("))");
                            builder.AppendLine();

                            builder.AppendBlock(() =>
                            {
                                builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{constructor.Accessibility},");
                                builder.AppendAssignment("Name", constructor.Name);
                                builder.AppendAssignment("IsStatic", constructor.IsStatic);

                                if (constructor.IsStatic)
                                {
                                    builder.AppendLine("Parameters = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceParameterInfo>(),");
                                }
                                else
                                {
                                    EmitParameters(builder, constructor.Parameters);

                                    if (!type.IsGenericTypeDefinition &&
                                        !type.IsRefLikeType &&
                                        constructor.Accessibility != Accessibility.Private && constructor.Accessibility != Accessibility.Protected &&
                                        !type.Properties.Any(x => x.IsRequired))
                                    {
                                        builder.AppendIndent();
                                        builder.Append("Invoke = (parameters) => ");
                                        builder.Append($"new {type.FullGlobalName}(");
                                        for (int i = 0; i < constructor.Parameters.Count; i++)
                                        {
                                            SourceParameterInfo parameter = constructor.Parameters[i];
                                            builder.Append($"({parameter.ParameterType})parameters[{i}]");
                                            if (i < constructor.Parameters.Count - 1)
                                            {
                                                builder.Append(", ");
                                            }
                                        }
                                        builder.Append(")");
                                        builder.AppendLine();
                                    }
                                }

                            }, ",");
                        }
                    });
                }
                else
                {
                    builder.AppendLine("DeclaredConstructors = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceConstructorInfo>(),");
                }
            }, ";");
        });
    }

    private static void AppendBindingFlags(CSharpCodeBuilder builder, Accessibility accessibility, bool isStatic)
    {
        if (isStatic)
        {
            builder.Append("global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Static");
        }
        else
        {
            builder.Append("global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Instance");
        }

        if (accessibility == Accessibility.Public)
        {
            builder.Append(" | global::System.Reflection.BindingFlags.Public");
        }
        else
        {
            builder.Append(" | global::System.Reflection.BindingFlags.NonPublic");
        }
    }

    private static void EmitParameters(CSharpCodeBuilder builder, List<SourceParameterInfo> parameters)
    {
        if (parameters.Count == 0)
        {
            builder.AppendLine("Parameters = Array.Empty<global::SourceGeneration.Reflection.SourceParameterInfo>(),");
        }
        else
        {
            builder.AppendBlock("Parameters = new global::SourceGeneration.Reflection.SourceParameterInfo[]", () =>
            {
                foreach (var parameter in parameters)
                {
                    builder.AppendBlock("new global::SourceGeneration.Reflection.SourceParameterInfo()", () =>
                    {
                        builder.AppendAssignment("Name", parameter.Name);
                        builder.AppendAssignment("HasDefaultValue", parameter.HasDefaultValue);
                        builder.AppendAssignment("DefaultValue", parameter.DefaultValue);
                        if (!parameter.HasNestedTypeParameter)
                        {
                            builder.AppendLine($"ParameterType = typeof({parameter.ParameterType}),");
                        }
                        builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{parameter.NullableAnnotation},");
                    }, ",");
                }
            }, ",");
        }
    }

}