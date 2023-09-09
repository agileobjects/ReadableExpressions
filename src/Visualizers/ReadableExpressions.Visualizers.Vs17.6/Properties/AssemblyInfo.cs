﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using AgileObjects.ReadableExpressions.Visualizers;

[assembly: AssemblyDescription("A Debugger Visualizer providing readable string representations of Expressions, Types and MethodInfos for Visual Studio 17")]

[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Expression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(BinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(BlockExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(ConditionalExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(ConstantExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(DebugInfoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(DefaultExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(DynamicExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(GotoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(IndexExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(InvocationExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(LabelExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(LambdaExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(ListInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(LoopExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(MemberExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(MemberInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(MethodCallExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(NewArrayExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(NewExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(ParameterExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(RuntimeVariablesExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(SwitchExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(TryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(TypeBinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(UnaryExpression), Description = "ReadableExpressions Visualizer")]

[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Type), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(MethodBase), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(ConstructorInfo), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(MethodInfo), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(PropertyInfo), Description = "ReadableExpressions Visualizer")]

[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Func<,,,,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]

[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs17ExpressionVisualizer), "AgileObjects.ReadableExpressions.Visualizers.Vs17ExpressionVisualizerObjectSource, AgileObjects.ReadableExpressions.Visualizers.Vs17.6.ObjectSource", Target = typeof(Action<,,,,,,,,,,,,,,>), Description = "ReadableExpressions Visualizer")]