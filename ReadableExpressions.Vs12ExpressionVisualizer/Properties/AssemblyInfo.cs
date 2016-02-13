using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using AgileObjects.ReadableExpressions.ExpressionVisualizer;

[assembly: AssemblyDescription("A Debugger Visualizer providing a readable string representation of an Expression for Visual Studio 12")]

[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(Expression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(BinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(BlockExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(ConditionalExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(ConstantExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(DebugInfoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(DefaultExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(DynamicExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(GotoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(IndexExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(InvocationExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(LabelExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(LambdaExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(ListInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(LoopExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(MemberExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(MemberInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(MethodCallExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(NewArrayExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(NewExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(ParameterExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(RuntimeVariablesExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(SwitchExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(TryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(TypeBinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs12ExpressionVisualizer), typeof(Vs12ExpressionVisualizerObjectSource), Target = typeof(UnaryExpression), Description = "ReadableExpressions Visualizer")]