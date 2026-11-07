namespace MonorepoDotnet.Core.Abstractions;

/// <summary>
/// Renders templates using <c>{{Token}}</c> placeholders and
/// <c>{{#if Flag}}...{{/if}}</c> / <c>{{#if !Flag}}...{{/if}}</c> blocks.
/// Blocks can be nested.
/// </summary>
public interface ITemplateRenderer
{
    string Render(
        string template,
        IReadOnlyDictionary<string, string> values,
        IReadOnlyDictionary<string, bool> flags);
}
