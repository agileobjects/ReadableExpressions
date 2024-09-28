#if NET35
global using Microsoft.Scripting.Ast;
global using Fact = NUnit.Framework.TestAttribute;
global using NUnitTestFixture = NUnit.Framework.TestFixtureAttribute;
global using static Microsoft.Scripting.Ast.Expression;
#else
global using System.Linq.Expressions;
global using Xunit;
global using static System.Linq.Expressions.Expression;
#endif

global using AgileObjects.NetStandardPolyfills;
global using AgileObjects.ReadableExpressions.UnitTests.Common;