/*using MailKit.Security;

class Program
{
	// Connection-related properties
	const SecureSocketOptions SslOptions = SecureSocketOptions.Auto;
	const string Host = "outlook.live.com";
	const int Port = 993;

	// Authentication-related properties
	const string Username = "testuser22223333@outlook.com";
	const string Password = "P@ssw0rd@1234";

	public static void Main(string[] args)
	{
		using (var client = new ImapIdler(Host, Port, SslOptions, Username, Password))
		{
			Console.WriteLine("Hit any key to end the demo.");

			var idleTask = client.RunAsync();

			Task.Run(() => {
				Console.ReadKey(true);
			}).Wait();

			client.Exit();

			idleTask.GetAwaiter().GetResult();
		}
	}
}*/