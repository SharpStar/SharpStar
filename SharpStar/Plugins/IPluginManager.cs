namespace SharpStar.Plugins
{
    public interface IPluginManager
    {

        void LoadPlugins();

        void LoadPlugin(string file);

        void UnloadPlugin(string file);

        void UnloadPlugins();

    }
}
