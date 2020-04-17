using Company.Pebbles.Examination.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IAnswersService
    {
        IReadOnlyCollection<ExaminationAnswerModel> Scripts { get; }
        IReadOnlyCollection<ExaminationAnswerModel> Objections { get; }
        IReadOnlyCollection<ExaminationAnswerModel> QuickAnswers { get; }

        Task SetAnswers(long examinationId);
        void ClearAnswers();
    }
}
