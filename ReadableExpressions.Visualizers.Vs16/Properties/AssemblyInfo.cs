﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using AgileObjects.ReadableExpressions.Visualizers;

[assembly: AssemblyDescription("A Debugger Visualizer providing readable string representations of Expressions, Types and MethodInfos for Visual Studio 16")]

[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(Expression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(BinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(BlockExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(ConditionalExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(ConstantExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(DebugInfoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(DefaultExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(DynamicExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(GotoExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(IndexExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(InvocationExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(LabelExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(LambdaExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(ListInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(LoopExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(MemberExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(MemberInitExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(MethodCallExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(NewArrayExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(NewExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(ParameterExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(RuntimeVariablesExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(SwitchExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(TryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(TypeBinaryExpression), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(UnaryExpression), Description = "ReadableExpressions Visualizer")]

[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(Type), Description = "ReadableExpressions Visualizer")]
[assembly: DebuggerVisualizer(typeof(Vs16ExpressionVisualizer), typeof(Vs16ExpressionVisualizerObjectSource), Target = typeof(MethodInfo), Description = "ReadableExpressions Visualizer")]