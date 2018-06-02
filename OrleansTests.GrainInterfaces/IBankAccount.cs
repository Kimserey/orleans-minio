using Orleans;
using System;
using System.Threading.Tasks;

namespace OrleansTests.GrainInterfaces
{
    public interface IBankAccount: IGrainWithIntegerKey
    {
        Task Clear();

        Task<double> SetBalance(double amount);

        Task<double> GetBalance();
    }
}
