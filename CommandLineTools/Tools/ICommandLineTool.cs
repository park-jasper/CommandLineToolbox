namespace CommandLineTools.Tools
{
    public interface ICommandLineTool<TOptions>
    {
        int ExecuteCommand(TOptions options);
    }
}