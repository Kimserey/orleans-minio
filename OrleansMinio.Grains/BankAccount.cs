using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using OrleansMinio.GrainInterfaces;
using System.Threading.Tasks;

namespace OrleansMinio.Grains
{
    [StatelessWorker]
    [StorageProvider(ProviderName = "File")]
    public class BankAccount : Grain<BankAccountState>, IBankAccount
    {
        public Task Clear()
        {
            return ClearStateAsync();
        }

        public Task<double> GetBalance()
        {
            return Task.FromResult(State.Amount);
        }

        public async Task<double> SetBalance(double amount)
        {
            State.Amount = amount;
            await WriteStateAsync();
            return State.Amount;
        }
    }

    public class BankAccountState
    {
        public double Amount { get; set; }
    }
}
