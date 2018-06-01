using Orleans;
using System;
using System.Threading.Tasks;

namespace OrleansTests.GrainInterfaces
{
    public interface IBankAccount: IGrainWithIntegerKey
    {
        Task<double> SetBalance(double amount);

        Task<double> GetBalance();
    }
}
