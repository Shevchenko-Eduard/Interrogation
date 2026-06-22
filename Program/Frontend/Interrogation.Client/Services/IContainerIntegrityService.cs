using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public interface IContainerIntegrityService
{
    IntegrityInfo Create(IEnumerable<string> protectedValues, string password);
    bool Verify(IEnumerable<string> protectedValues, string password, IntegrityInfo integrity);
}
