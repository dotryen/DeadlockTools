using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using ValveKeyValue;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization.KeyValues;
using KVObject = ValveResourceFormat.Serialization.KeyValues.KVObject;
using KVValue = ValveResourceFormat.Serialization.KeyValues.KVValue;

namespace DeadlockTools {
    [Command("add ag2", Description = "Adds AG2 Graph and Skeleton references needed for newer heroes.")]
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
            await using FileStream fileStream = File.Open(TargetFile, FileMode.Open, FileAccess.ReadWrite);

            // COPY FILE DATA INTO BUFFER
            // VRF lazily loads many resources, we need a copy before overwriting
            byte[] memBuffer = new byte[fileStream.Length];
            await fileStream.ReadExactlyAsync(memBuffer);
            using MemoryStream memStream = new MemoryStream(memBuffer);
            
            await console.Output.WriteLineAsync("Reading resource data");
            Resource resource = new Resource();
            resource.Read(memStream, true, true);

            if (resource.ResourceType != ResourceType.Model) {
                await console.Output.WriteLineAsync($"File is not a model! Aborting...");
                return;
            }
            
            Model modelBlock = (Model)resource.DataBlock;
            KVObject kvData = modelBlock.Data;
            bool modified = false;
            
            // GRAPH ARRAY
            if (!kvData.ContainsKey("m_animGraph2Refs")) {
                await console.Output.WriteLineAsync("Adding graph refs");
            
                KVObject graphArray = new KVObject("graphRefs", true, 2);
                graphArray.AddProperty(null, CreateRef("", string.Format(DEFAULT_GRAPH_FORMAT, HeroId)));
                graphArray.AddProperty(null, CreateRef("ui", string.Format(UI_GRAPH_FORMAT, HeroId)));
            
                kvData.AddProperty("m_animGraph2Refs", graphArray);
                modified = true;
            } else {
                using (console.WithForegroundColor(ConsoleColor.Yellow)) {
                    await console.Output.WriteLineAsync("Graph refs already exist! skipping");
                }
            }
            
            // SKELETON REF
            if (!kvData.ContainsKey("m_vecNmSkeletonRefs")) {
                await console.Output.WriteLineAsync("Adding skeleton ref");
                
                KVObject skeletonArray = new KVObject("skeletonRefs", true, 1);
                skeletonArray.AddProperty(null, new KVValue(KVValueType.String, KVFlag.Resource, string.Format(SKELETON_FORMAT, HeroId)));
                
                kvData.AddProperty("m_vecNmSkeletonRefs", skeletonArray);
                modified = true;
            } else {
                using (console.WithForegroundColor(ConsoleColor.Yellow)) {
                    await console.Output.WriteLineAsync("Skeleton refs already exist! skipping");
                }
            }
            
            // WRITE
            if (modified) {
                await console.Output.WriteLineAsync("Overwriting target file");

                fileStream.Position = 0;
                fileStream.SetLength(0);
                resource.Serialize(fileStream);
                await console.Output.WriteLineAsync("Done!");
            } else {
                await console.Output.WriteLineAsync("No changes made");
            }
        }

        private KVObject CreateRef(string id, string path) {
            KVObject obj = new KVObject("ref", 2);
            obj.AddProperty("m_sIdentifier", id);
            obj.AddProperty("m_hGraph", new KVValue(KVValueType.String, KVFlag.Resource, path));
            return obj;
        }
    }
}
