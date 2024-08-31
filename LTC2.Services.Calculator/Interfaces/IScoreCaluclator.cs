using LTC2.Shared.Models.Domain;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.Interfaces
{
    public interface IScoreCalculator
    {
        public void Init();

        public Task Calculate(CalculationJob job);
    }
}
