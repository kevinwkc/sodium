﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sodium.Tests.Internal
{
    [TestFixture]
    public class TransactionTests
    {
        [Test]
        public async Task PostSeeOutside()
        {
            OperationCanceledException actual = null;
            AutoResetEvent re = new AutoResetEvent(false);
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task task = Task.Run(
                    () =>
                    {
                        Transaction.Post(
                            () =>
                            {
                                re.Set();

                                Thread.Sleep(500);

                                cts.Token.ThrowIfCancellationRequested();
                            });
                    });

                re.WaitOne();

                cts.Cancel();

                try
                {
                    await task;
                }
                catch (OperationCanceledException e)
                {
                    actual = e;
                }
            }

            Assert.IsNotNull(actual);
        }

        [Test]
        public async Task PostSeeInside()
        {
            OperationCanceledException actual = null;
            AutoResetEvent re = new AutoResetEvent(false);
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task task = Task.Run(
                    () =>
                    {
                        Transaction.Post(
                            () =>
                            {
                                re.Set();

                                Thread.Sleep(500);

                                cts.Token.ThrowIfCancellationRequested();
                            });
                    });

                re.WaitOne();

                StreamSink<Unit> sink2 = new StreamSink<Unit>();
                sink2.Listen(_ => cts.Cancel());
                sink2.Send(Unit.Value);

                try
                {
                    await task;
                }
                catch (OperationCanceledException e)
                {
                    actual = e;
                }
            }

            Assert.IsNotNull(actual);
        }
    }
}