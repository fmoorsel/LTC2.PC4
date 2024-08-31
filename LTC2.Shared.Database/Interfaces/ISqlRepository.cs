using System;
using System.Threading.Tasks;

namespace LTC2.Shared.Database.Interfaces
{
    public interface ISqlRepository
    {
        public interface ISqlRepository
        {
            string DbConnectionString { get; set; }

            Task DeadlockRetryHelper<TArgument>(Func<TArgument, Task> method, TArgument argument, int maxRetries = 3);
        }
    }
}
