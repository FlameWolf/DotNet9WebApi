﻿using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotNet9WebApi.SwaggerFilters;

public class SwaggerArrayParameterFilter : IOperationAsyncFilter
{
	private const string multipartFormData = "multipart/form-data";
	private static readonly ConcurrentDictionary<Type, bool> typeCache = new();

	public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
	{
		foreach (var parameter in context.ApiDescription.ParameterDescriptions)
		{
			var parameterType = parameter.ParameterDescriptor?.ParameterType;
			if (parameterType != null && ShouldProcessAsComplexType(parameterType))
			{
				ProcessProperties(parameterType, operation, []);
			}
		}
		return Task.CompletedTask;
	}

	private static void ProcessProperties(Type type, OpenApiOperation operation, HashSet<Type> visitedTypes)
	{
		if (type == null || operation.RequestBody?.Content == null || visitedTypes.Contains(type))
		{
			return;
		}
		visitedTypes.Add(type);
		if (!operation.RequestBody.Content.TryGetValue(multipartFormData, out OpenApiMediaType? value))
		{
			value = new OpenApiMediaType
			{
				Schema = new OpenApiSchema
				{
					Type = "object",
					Properties = new Dictionary<string, OpenApiSchema>()
				},
				Encoding = new Dictionary<string, OpenApiEncoding>()
			};
			operation.RequestBody.Content[multipartFormData] = value;
		}
		var schemaProperties = value.Schema.Properties;
		var encodings = value.Encoding;
		foreach (var property in type.GetProperties())
		{
			if (HasSwaggerIgnoreAttribute(property))
			{
				continue;
			}
			if (IsCollectionType(property.PropertyType))
			{
				ProcessCollectionProperty(property, schemaProperties, encodings);
			}
			else if (ShouldProcessAsComplexType(property.PropertyType))
			{
				ProcessProperties(property.PropertyType, operation, visitedTypes);
			}
		}
	}

	private static bool HasSwaggerIgnoreAttribute(PropertyInfo property) => property.GetCustomAttributes(typeof(SwaggerIgnoreAttribute), true).Length != 0;

	private static bool IsCollectionType(Type type) => typeCache.GetOrAdd
	(
		type,
		t => t.IsArray ||
			(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) ||
			(typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string))
	);

	private static bool ShouldProcessAsComplexType(Type type) => type.IsClass &&
		type != typeof(string) &&
		!type.IsPrimitive &&
		!type.IsValueType;

	private static void ProcessCollectionProperty
	(
		PropertyInfo property,
		IDictionary<string, OpenApiSchema> schemaProperties,
		IDictionary<string, OpenApiEncoding> encodings
	)
	{
		var propertyName = ToCamelCase(property.Name);
		schemaProperties[propertyName] = new();
		encodings[propertyName] = new()
		{
			Explode = true
		};
	}

	private static string ToCamelCase(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (value.Length == 1)
		{
			return value.ToLowerInvariant();
		}
		return $"{char.ToLowerInvariant(value[0])}{value[1..]}";
	}
}