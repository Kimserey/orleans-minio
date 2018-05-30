using Orleans;
using System;
using System.Threading.Tasks;

namespace OrleansTests.GrainInterfaces
{
    public interface IBankAccount: IGrainWithGuidKey
    {
        Task SetBalance(double amount);

        Task<double> GetBalance();
    }
}
