using System.Text;

namespace {{InfrastructureNamespace}}.Utils;

public static class StringExtensions
{
    public static string StripJsonWhitespace(this string json)
    {
        var builder = new StringBuilder(json.Length);
        var inString = false;
        var escaped = false;

        foreach (var character in json)
        {
            if (inString)
            {
                builder.Append(character);
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (character == '"')
            {
                inString = true;
                builder.Append(character);
                continue;
            }

            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
