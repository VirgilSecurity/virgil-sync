namespace Virgil.FolderLink.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Operations;

    public class OperationsBatch 
    {
        private readonly Task<int>[] work;

        public string BatchId { get; }

        public IEnumerable<Operation> Operations { get; }

        public OperationsBatch(string batchId, IEnumerable<Operation> operations)
        {
            this.BatchId = batchId;
            this.Operations = operations;

            this.work = this.Operations.Select(it => it.CompletionSource.Task).ToArray();
        }

        public async Task WhenAll()
        {
            await Task.WhenAll(work);
        }
    }
}