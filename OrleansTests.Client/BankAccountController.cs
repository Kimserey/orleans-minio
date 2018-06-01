using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansTests.GrainInterfaces;
using System;
using System.Threading.Tasks;

namespace OrleansTests.Client
{
    [Route("bankAccount")]
    public class BankAccountController : Controller
    {
        private IGrainFactory _factory;

        public BankAccountController(IGrainFactory factory)
        {
            _factory = factory;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var balance = await _factory.GetGrain<IBankAccount>(0).GetBalance();
            return Json(balance);
        }

        [HttpPost("balance")]
        public async Task<IActionResult> SetBalance([FromQuery] double b)
        {
            var balance = await _factory.GetGrain<IBankAccount>(0).SetBalance(b);
            return Json(balance);
        }
    }
}
