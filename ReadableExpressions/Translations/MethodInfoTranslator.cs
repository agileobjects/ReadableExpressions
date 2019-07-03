namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Reflection;
    using System.Text;
    using Extensions;
    using static Constants;

    /// <summary>
    /// Translates a MethodInfo object into a readable string. Used to provide MethodInfo visualization.
    /// </summary>
    public static class MethodInfoTranslator
    {
        /// <summary>
        /// Translates the given <paramref name="method"/> into a readable string.
        /// </summary>
        /// <param name="method">The MethodInfo to translate.</param>
        /// <returns>A readable string version of the given <paramref name="method"/>.</returns>
        public static string Translate(MethodInfo method)
        {
            var buffer = new TranslationBuffer(method.ToString().Length);

            buffer.WriteFriendlyName(method.ReturnType);
            buffer.WriteSpaceToTranslation();

            if (method.DeclaringType != null)
            {
                buffer.WriteFriendlyName(method.DeclaringType);
                buffer.WriteToTranslation('.');
            }

            buffer.WriteToTranslation(method.Name);

            var parameters = method.GetParameters();

            if (!parameters.Any())
            {
                buffer.WriteToTranslation("()");
                return buffer.GetContent();
            }

            buffer.WriteNewLineToTranslation();
            buffer.WriteToTranslation('(');
            buffer.Indent();

            for (var i = 0; i < parameters.Length;)
            {
                var parameter = parameters[i];
                
                buffer.WriteNewLineToTranslation();
                buffer.WriteFriendlyName(parameter.ParameterType);
                buffer.WriteSpaceToTranslation();
                buffer.WriteToTranslation(parameter.Name);

                if (++i == parameters.Length)
                {
                    break;
                }

                buffer.WriteToTranslation(',');
            }

            buffer.Unindent();
            buffer.WriteToTranslation(')');

            return buffer.ToString();
        }
    }
}
