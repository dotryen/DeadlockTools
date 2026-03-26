using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using ValveKeyValue;
using ValveResourceFormat;
using ValveResourceFormat.Serialization.KeyValues;
using KVObject = ValveResourceFormat.Serialization.KeyValues.KVObject;
using KVValue = ValveResourceFormat.Serialization.KeyValues.KVValue;

namespace DeadlockTools {
    [Command("add ag2")]
    public class AddAnimGraphRefs : ICommand {
        private const string DEFAULT_GRAPH_FORMAT = "animgraphs/animgraph2/hero/hero.vnmgraph+{0}.vnmgraph";
        private const string UI_GRAPH_FORMAT = "animgraphs/animgraph2/hero/hero_ui.vnmgraph+{0}.vnmgraph";
        private const string SKELETON_FORMAT = "models/heroes_wip/{0}/{0}.vnmskel";
        
        [CommandParameter(0)]
        public required string TargetFile { get; set; }
        
        [CommandOption("hero", 'h', IsRequired = true)]
        public required string HeroId { get; set; }
        
        public async ValueTask ExecuteAsync(IConsole console) {
            await console.Output.WriteLineAsync("Opening file: " + TargetFile);
            await using var fileStream = File.OpenRead(TargetFile);

            await console.Output.WriteLineAsync("Reading resource data");
            var resource = new Resource();
            resource.Read(fileStream);

            var modelBlock = (ValveResourceFormat.ResourceTypes.Model)resource.DataBlock;

            // GRAPH ARRAY
            await console.Output.WriteLineAsync("Adding graph refs");
            
            KVObject graphArray = new KVObject("graphRefs", true, 2);
            graphArray.AddProperty(null, CreateRef("", string.Format(DEFAULT_GRAPH_FORMAT, HeroId)));
            graphArray.AddProperty(null, CreateRef("ui", string.Format(UI_GRAPH_FORMAT, HeroId)));
            
            modelBlock.Data.AddProperty("m_animGraph2Refs", graphArray);
            
            // SKELETON REF
            await console.Output.WriteLineAsync("Adding skeleton ref");
            
            KVObject skeletonArray = new KVObject("skeletonRefs", true, 1);
            skeletonArray.AddProperty(null, new KVValue(KVValueType.String, KVFlag.Resource, string.Format(SKELETON_FORMAT, HeroId)));
            
            modelBlock.Data.AddProperty("m_vecNmSkeletonRefs", skeletonArray);
            
            // WRITE
            await console.Output.WriteLineAsync("Overwriting target file");
            var directory = Path.GetDirectoryName(TargetFile);
            var fileName = Path.GetFileNameWithoutExtension(TargetFile);
            // fileName += "_test";
            fileName += ".vmdl_c";

            using var outStream = File.OpenWrite(Path.Combine(directory, fileName));
            resource.Serialize(outStream);
        }

        private KVObject CreateRef(string id, string path) {
            KVObject obj = new KVObject("ref", 2);
            obj.AddProperty("m_sIdentifier", id);
            obj.AddProperty("m_hGraph", new KVValue(KVValueType.String, KVFlag.Resource, path));
            return obj;
        }
    }
}
