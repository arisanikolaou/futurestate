using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FutureState.IO
{
    public class ProcessRunner : IProcessRunner
    {
        #region Implementation of IProcessRunner

        public Task<ProcessExecutionResult> RunAsync(FileInfo executable,
                                                     string[] arguments,
                                                     IDictionary<string, string> environment = null)
        {
            var startInfo = new ProcessStartInfo
                            {
                                Arguments = string.Join(" ", arguments),
                                CreateNoWindow = true,
                                FileName = executable.FullName,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false
                            };

            if (environment != null)
            {
                foreach (var kvp in environment)
                {
                    startInfo.Environment.Add(kvp);
                }
            }

            var process = new Process
                          {
                              EnableRaisingEvents = true,
                              StartInfo = startInfo
                          };

            var completionSource = new TaskCompletionSource<ProcessExecutionResult>();

            var executionResult = new ProcessExecutionResult();

            var stopwatch = new Stopwatch();

            process.ErrorDataReceived += (sender, args) =>
                                         {
                                             if (args.Data != null)
                                             {
                                                 executionResult.StandardError.Add(args.Data);
                                             }
                                         };

            process.OutputDataReceived += (sender, args) =>
                                          {
                                              if (args.Data != null)
                                              {
                                                  executionResult.StandardOutput.Add(args.Data);
                                              }
                                          };

            process.Exited += (sender, args) =>
                              {
                                  stopwatch.Stop();

                                  executionResult.ExitCode = process.ExitCode;
                                  executionResult.ElapsedTime = stopwatch.Elapsed;

                                  process.Dispose();

                                  completionSource.SetResult(executionResult);
                              };

            if (!process.Start())
            {
                throw new InvalidOperationException($"Failed to start process ({executable} {arguments})");
            }

            stopwatch.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return completionSource.Task;
        }

        #endregion
    }
}
