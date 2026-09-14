using ScriptableFramework;
using System.Text.RegularExpressions;

namespace BatchCmdDsl
{
    public interface IBatchCmdPlugin
    {
        int Init(string cmdLine, string basePath);
        int Tick();
        int Shutdown();
    }
}
