﻿using System.Linq.Expressions;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DancingGoat.Helpers;

internal static class ExpressionExtensions
{
    private static readonly ModelExpressionProvider modelExpressionProvider = new(new EmptyModelMetadataProvider());

    /// <summary>
    /// Returns the expression text for the specified expression.
    /// </summary>
    public static string GetExpressionText<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression) => modelExpressionProvider.GetExpressionText(expression);
}
