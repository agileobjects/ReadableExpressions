namespace AgileObjects.ReadableExpressions.Build.Logging
{
    using System;
    using Microsoft.Build.Framework;
    using MsBuildTask = Microsoft.Build.Utilities.Task;

    internal class MsBuildTaskLogger : ILogger
    {
        private MsBuildTask _task;

        public void SetTask(MsBuildTask task) => _task = task;

        public void Info(string message)
            => _task.Log.LogMessage(MessageImportance.Normal, message);

        public void Warning(string message)
            => _task.Log.LogWarning(message);

        public void Error(string message)
            => _task.Log.LogError(message);

        public void Error(Exception ex)
            => _task.Log.LogErrorFromException(ex);
    }
}