using CliFx;

namespace DeadlockTools {
	public static class Program {
		public static async Task<int> Main(string[] args) {
			return await new CliApplicationBuilder()
				.SetExecutableName("DeadlockTools")
				.AddCommandsFromThisAssembly()
				.Build()
				.RunAsync();
		}
	}
}
