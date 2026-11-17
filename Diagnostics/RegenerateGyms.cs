/// <summary>
/// Thin runner for the gym builders. It calls the methods Unity already compiled into
/// Assembly-CSharp-Editor instead of recompiling the builder sources in-memory. Lives outside
/// Assets/, so it is never part of the project build.
/// </summary>
public static class RegenerateGyms
{
    public static string BuildCmGym()
    {
        Sys3Tools.BuildGym();
        return "Sys3Tools.BuildGym done; active scene: " +
               UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
    }

    public static string BuildEmGym()
    {
        BastionZero.EditorTools.Sys4Tools.BuildGymScene();
        return "Sys4Tools.BuildGymScene done; active scene: " +
               UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
    }
}
