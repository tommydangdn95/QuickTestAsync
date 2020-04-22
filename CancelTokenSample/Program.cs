using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CancelTokenSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            var watch = Stopwatch.StartNew();
            //await ExecuteTaskAsync();
            //await ExecuteTaskWithTimeoutAsync(TimeSpan.FromSeconds(2));
            await CancelANonCancellableTaskAsync();
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        public static async Task ExecuteTaskAsync()
        {
            Console.WriteLine(nameof(ExecuteTaskAsync));
            Console.WriteLine($"Result {await LongRunningOperation(100)}");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static Task<decimal> LongRunningOperation(int loop)
        {
            return Task.Run(() =>
            {
                decimal result = 0;
                for (int i = 0; i < loop; i++)
                {
                    Thread.Sleep(100);
                    result += 1;
                }

                return result;
            });
        }
        private static Task<decimal> LongRunningCancellableOperation(int loop, CancellationToken cancellationToken)
        {
            Task<decimal> task = null;

            task = Task.Run(() =>
            {
                decimal result = 0;
                for (int i = 0; i < loop; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    Thread.Sleep(100);
                    result += 1;
                }

                return result;
            });

            return task;
        }

        public static async Task ExecuteTaskWithTimeoutAsync (TimeSpan timeSpan)
        {
            Console.WriteLine(nameof(ExecuteTaskWithTimeoutAsync));

            using (var cancellationTokenSource = new CancellationTokenSource(timeSpan))
            {
                try
                {
                    var result = await LongRunningCancellableOperation(500, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was canncelled");
                }
            }
        }

        public static async Task ExecuteManuallyCancellableTaskAsync()
        {
            Console.WriteLine(nameof(ExecuteManuallyCancellableTaskAsync));

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var keyboardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press Enter to cancel");
                    Console.ReadKey();

                    cancellationTokenSource.Cancel();
                });


                try
                {
                    var longRunningTask = LongRunningCancellableOperation(500, cancellationTokenSource.Token);
                    var result = await longRunningTask;
                    Console.WriteLine("Result {0}", result);
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Task was cancelled with messs: {ex.Message}");
                }
            }
        }

        public static async Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            cancellationToken.Register(() =>
            {

                taskCompletionSource.TrySetCanceled();
            });

            var task = LongRunningOperation(loop);

            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);

            //if (completedTask == task)
            //{
            //    // Extract the result, the task is finished and the await will return immediately
            //    var result = await task;

            //    // Set the taskCompletionSource result
            //    taskCompletionSource.TrySetResult(result);
            //}

            //// Return the result of the TaskCompletionSource.Task
            //return await taskCompletionSource.Task;

            return await completedTask;

        }

        public static async Task CancelANonCancellableTaskAsync()
        {
            Console.WriteLine(nameof(CancelANonCancellableTaskAsync));

            using(var cancellationTokenSource = new CancellationTokenSource())
            {
                // Listening to key press to cancel
                var keyBoardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press enter to cancel");
                    Console.ReadKey();

                    // Sending the cancellation message
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    var longRunningTask = LongRunningOperationWithCancellationTokenAsync(100, cancellationTokenSource.Token);
                    var result = await longRunningTask;

                    Console.WriteLine("Result {0}", result);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }
            }
        }
    }
}
