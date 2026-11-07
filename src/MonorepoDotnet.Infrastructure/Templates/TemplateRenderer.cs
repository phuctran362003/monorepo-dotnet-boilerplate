using System.Text;
using MonorepoDotnet.Core.Abstractions;

namespace MonorepoDotnet.Infrastructure.Templates;

/// <summary>
/// Small, deterministic template renderer. Supports:
/// <list type="bullet">
/// <item><c>{{Token}}</c> value replacement.</item>
/// <item><c>{{#if Flag}}...{{/if}}</c> conditional blocks.</item>
/// <item><c>{{#if !Flag}}...{{/if}}</c> negated conditional blocks.</item>
/// <item>Nested blocks.</item>
/// </list>
/// Unknown tokens and flags fail fast so template mistakes never produce
/// silently broken code.
/// </summary>
public sealed class TemplateRenderer : ITemplateRenderer
{
    public string Render(
        string template,
        IReadOnlyDictionary<string, string> values,
        IReadOnlyDictionary<string, bool> flags)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(flags);

        var output = new StringBuilder(template.Length);
        var conditionStack = new Stack<bool>();
        var isActive = true;
        var index = 0;

        while (index < template.Length)
        {
            var openIndex = template.IndexOf("{{", index, StringComparison.Ordinal);
            if (openIndex < 0)
            {
                if (isActive)
                {
                    output.Append(template, index, template.Length - index);
                }

                break;
            }

            if (isActive)
            {
                output.Append(template, index, openIndex - index);
            }

            var closeIndex = template.IndexOf("}}", openIndex + 2, StringComparison.Ordinal);
            if (closeIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Unclosed template expression starting at position {openIndex}.");
            }

            var expression = template.Substring(openIndex + 2, closeIndex - openIndex - 2).Trim();
            index = closeIndex + 2;

            if (expression.StartsWith("#if ", StringComparison.Ordinal))
            {
                var flagExpression = expression[4..].Trim();
                var negate = flagExpression.StartsWith('!');
                var flagName = negate ? flagExpression[1..].Trim() : flagExpression;

                if (!flags.TryGetValue(flagName, out var flagValue))
                {
                    throw new InvalidOperationException($"Unknown template flag '{flagName}'.");
                }

                conditionStack.Push(isActive);
                isActive = isActive && (negate ? !flagValue : flagValue);
            }
            else if (expression == "/if")
            {
                if (conditionStack.Count == 0)
                {
                    throw new InvalidOperationException("Template contains a '{{/if}}' without a matching '{{#if}}'.");
                }

                isActive = conditionStack.Pop();
            }
            else if (expression.Length == 0)
            {
                throw new InvalidOperationException("Template contains an empty expression.");
            }
            else if (isActive)
            {
                if (!values.TryGetValue(expression, out var value))
                {
                    throw new InvalidOperationException($"Unknown template token '{expression}'.");
                }

                output.Append(value);
            }
        }

        if (conditionStack.Count > 0)
        {
            throw new InvalidOperationException("Template contains an unclosed '{{#if}}' block.");
        }

        return output.ToString();
    }
}
