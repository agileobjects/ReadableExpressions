namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class GotoTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private const string _returnKeyword = "return ";
        private readonly string _fixedTranslation;
        private readonly CodeBlockTranslation _returnValueTranslation;

        public GotoTranslation(GotoExpression @goto, ITranslationContext context)
        {
            switch (@goto.Kind)
            {
                case GotoExpressionKind.Break:
                    _fixedTranslation = "break;";
                    break;

                case GotoExpressionKind.Continue:
                    _fixedTranslation = "continue;";
                    break;

                case GotoExpressionKind.Return:
                    if (@goto.Value == null)
                    {
                        _fixedTranslation = "return;";
                        break;
                    }

                    _returnValueTranslation = context.GetCodeBlockTranslationFor(@goto.Value);
                    EstimatedSize = _returnValueTranslation.EstimatedSize + _returnKeyword.Length;
                    return;

                case GotoExpressionKind.Goto when context.GoesToReturnLabel(@goto):
                    goto case GotoExpressionKind.Return;

                default:
                    _fixedTranslation = "goto " + @goto.Target.Name + ";";
                    break;
            }

            EstimatedSize = _fixedTranslation.Length;
        }

        public bool IsTerminated => _fixedTranslation != null;

        public ExpressionType NodeType => ExpressionType.Goto;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_fixedTranslation != null)
            {
                context.WriteToTranslation(_fixedTranslation);
                return;
            }

            context.WriteToTranslation(_returnKeyword);
            _returnValueTranslation.WriteTo(context);
        }
    }
}