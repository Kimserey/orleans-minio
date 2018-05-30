using Orleans;
using OrleansTests.GrainInterfaces;
using System.Threading.Tasks;

namespace OrleansTests.Grains
{
    public class BankAccount : Grain<BankAccountState>, IBankAccount
    {
        public Task<double> GetBalance()
        {
            return Task.FromResult(State.Amount);
        }

        public async Task SetBalance(double amount)
        {
            State.Amount = amount;
            await WriteStateAsync();
        }
    }

    public class BankAccountState
    {
        public double Amount { get; set; }
    }
}
