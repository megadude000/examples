using Company.Pebbles.Examination.Enumerations;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.Interfaces;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pebbles.Examination.Services
{
    [AddINotifyPropertyChangedInterface]
    public class AnswersService : IAnswersService
    {
        private readonly IHttpRequestService httpRequestService;

        public IReadOnlyCollection<ExaminationAnswerModel> Scripts { get; private set; }
        public IReadOnlyCollection<ExaminationAnswerModel> Objections { get; private set; }
        public IReadOnlyCollection<ExaminationAnswerModel> QuickAnswers { get; private set; }

        public AnswersService(IHttpRequestService httpRequestService)
        {
            this.httpRequestService = httpRequestService;
        }

        public async Task SetAnswers(long examinationId)
        {
            var answers = await httpRequestService.GetAnswersAsync(examinationId);

            Scripts = answers.Where(answer => answer.Type == AnswerType.Script).ToList().AsReadOnly();
            Objections = answers.Where(answer => answer.Type == AnswerType.Objection).ToList().AsReadOnly();
            QuickAnswers = answers.Where(answer => answer.Type == AnswerType.QuickAnswer).ToList().AsReadOnly();
        }

        public void ClearAnswers()
        {
            Scripts = new List<ExaminationAnswerModel>().AsReadOnly();
            Objections = new List<ExaminationAnswerModel>().AsReadOnly();
            QuickAnswers = new List<ExaminationAnswerModel>().AsReadOnly();
        }
    }
}
