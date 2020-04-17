using Company.Fred.Core.Interfaces;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Services.Interfaces
{
    public interface IRemoteFileService
    {
        Task DownloadFileAsync(IFtpConfiguration centralFtpConfiguration, string fileName);
    }
}
