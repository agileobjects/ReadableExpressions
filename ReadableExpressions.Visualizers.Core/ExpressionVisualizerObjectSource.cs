namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Translations;
    using Translations.StaticTranslators;

    public class ExpressionVisualizerObjectSource
    {
        public static void GetData(
            object target,
            Stream outgoingData,
            Action<Stream, string> serializer)
        {
            string value;

            switch (target)
            {
                case Expression expression:
                    value = GetTranslationForVisualizer(expression) ?? "default(void)";
                    break;

                case Type type:
                    value = type.GetFriendlyName();
                    break;

                case MethodInfo method:
                    value = DefinitionsTranslator.Translate(method);
                    break;

                case ConstructorInfo ctor:
                    value = DefinitionsTranslator.Translate(ctor);
                    break;

                default:
                    if (target == null)
                    {
                        return;
                    }
                    
                    value = target.GetType().GetFriendlyName();
                    break;
            }

            serializer.Invoke(outgoingData, value);
        }

        private static string GetTranslationForVisualizer(Expression expression)
        {
            return expression.ToReadableString(settings => settings
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }
    }

    internal class TranslationHtmlFormatter : ITranslationFormatter
    {
        public static readonly ITranslationFormatter Instance = new TranslationHtmlFormatter();

        public string PreToken(TokenType type)
        {
            switch (type)
            {
                case TokenType.Keyword:
                    return "<span class=\"kw\">";

                case TokenType.Parameter:
                    return "<span class=\"pm\">";

                case TokenType.TypeName:
                    return "<span class=\"tn\">";

                case TokenType.InterfaceName:
                    return "<span class=\"in\">";

                case TokenType.ControlStatement:
                    return "<span class=\"cs\">";

                case TokenType.Text:
                    return "<span class=\"tx\">";

                case TokenType.Numeric:
                    return "<span class=\"nm\">";

                case TokenType.MethodName:
                    return "<span class=\"mn\">";

                default:
                    return null;
            }
        }

        public string PostToken(TokenType type)
        {
            switch (type)
            {
                case TokenType.Keyword:
                case TokenType.Parameter:
                case TokenType.TypeName:
                case TokenType.InterfaceName:
                case TokenType.ControlStatement:
                case TokenType.Text:
                case TokenType.Numeric:
                case TokenType.MethodName:
                    return "</span>";

                default:
                    return null;
            }
        }

        public bool Encode(char character, out string encoded)
        {
            switch (character)
            {
                case '<':
                    encoded = "&lt;";
                    return true;

                case '>':
                    encoded = "&gt;";
                    return true;

                default:
                    encoded = null;
                    return false;
            }
        }
    }
}