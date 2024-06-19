using System.Diagnostics.CodeAnalysis;

namespace Diagnostics.Generator.Core.Test
{
    [TestClass]
    public class BatchBufferOperatorTest
    {
        [TestMethod]
        public void Given_Null_Handle_Throw_NullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new BatchBufferOperator<int>(null!));
        }
        [ExcludeFromCodeCoverage]
        class Handler<T> : IBatchOperatorHandler<T>
        {
            public int Count;

            public Task HandleAsync(BatchData<T> inputs, CancellationToken token)
            {
                Count += inputs.Count;
                return Task.CompletedTask;
            }
        }

        [TestMethod]
        public void IntervalInvoke()
        {
            var handler=new Handler<int>();
            using var @operator = new BatchBufferOperator<int>(handler, swapDelayTimeMs: 1000);
            for (int i = 0; i < 100; i++)
            {
                @operator.Add(i);
            }

            Thread.Sleep(1300);

            Assert.AreEqual(100,handler.Count);
        }
        [TestMethod]
        [Timeout(3000)]
        public void SizeInvoke()
        {
            var handler = new Handler<int>();
            using var @operator = new BatchBufferOperator<int>(handler,bufferSize:500, swapDelayTimeMs: 100000);
            for (int i = 0; i < @operator.Capacity+1; i++)
            {
                @operator.Add(i);
            }

            Thread.Sleep(500);
            
            Assert.AreEqual(@operator.Capacity, handler.Count);
        }
    }
}
