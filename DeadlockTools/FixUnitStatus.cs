using System.Text;
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
    [Command("fix unitstatus", Description = "Fixes the unit status when compiled from CSDK12.")]
    public class FixUnitStatus : ICommand {
        private const string UNIT_SETTINGS_STRUCT = "CitadelUnitStatusSettings_t";
        
        [CommandParameter(0)]
        public required string TargetFile { get; set; }
        
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
            KVObject modelInfo = kvData.GetProperty<KVObject>("m_modelInfo");
            
            string kvText = modelInfo.GetProperty<string>("m_keyValueText");
            byte[] kvTextBytes = Encoding.UTF8.GetBytes(kvText);
            using MemoryStream kvTextStream = new MemoryStream(kvTextBytes);

            KV3File modelGameData = KeyValues3.ParseKVFile(kvTextStream);
            
            // VERIFY UNIT STATUS
            if (!modelGameData.Root.ContainsKey(UNIT_SETTINGS_STRUCT)) {
                await console.Error.WriteLineAsync("No unit settings found! Aborting...");
                return;
            }
            
            // NEW SETTINGS
            KVObject array = modelGameData.Root.GetProperty<KVObject>(UNIT_SETTINGS_STRUCT);
            if (!array.IsArray) {
                await console.Error.WriteLineAsync("Data is not an array! Aborting...");
                return;
            }
            
            KVObject settings = (KVObject)array[0].Value;
            
            settings.Properties.Remove("m_vOffset");
            settings.Properties.Remove("m_sName");
            
            // REPLACE SETTINGS
            modelGameData.Root.AddProperty(UNIT_SETTINGS_STRUCT, settings);
            
            // CONVERT BACK TO TEXT
            modelInfo.AddProperty("m_keyValueText", new KVValue(KVValueType.String, modelGameData.ToString()));
            
            // OVERWRITE
            await console.Output.WriteLineAsync("Overwriting target file");

            fileStream.Position = 0;
            fileStream.SetLength(0);
            resource.Serialize(fileStream);
            await console.Output.WriteLineAsync("Done!");
        }
    }
}
