namespace CommandLineTools.Tools
{
    public interface ICommandLineTool<in TOptions>
    {
        int ExecuteCommand(TOptions options);
    }
}