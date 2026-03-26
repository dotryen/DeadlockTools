using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using ValveResourceFormat;
using ValveResourceFormat.Serialization.KeyValues;

namespace DeadlockTools {
    [Command("print")]
    public class PrintDataCommand : ICommand {
        [CommandParameter(0)]
        public string TargetFile { get; set; }
        
        public async ValueTask ExecuteAsync(IConsole console) {
            await using var fileStream = File.OpenRead(TargetFile);
            var resource = new Resource();
            resource.Read(fileStream);

            var modelBlock = (ValveResourceFormat.ResourceTypes.Model)resource.DataBlock;
            var writer = new IndentedTextWriter();
            modelBlock.Data.Serialize(writer);
            await console.Output.WriteLineAsync(writer.ToString());
        }
    }
}
