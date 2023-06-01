using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;

using Serilog; // logging framework
using Microsoft.EntityFrameworkCore; // ORM library
using Microsoft.Extensions.DependencyInjection; // dependency injection library
using System.Threading.Channels;

namespace QuarantinedMailHandler
{
    class IdleClientDisposable : IDisposable
    {
        readonly string host, username, password;
        readonly SecureSocketOptions sslOptions;
        readonly int port;
        List<IMessageSummary> messages;
        CancellationTokenSource cancel;
        CancellationTokenSource? done;
        FetchRequest request;
        bool messagesArrived;
        ImapClient client;
        private List<UniqueId> messageIds;
        private BlockingCollection<string> messageBodies; // thread-safe collection for message bodies
        private QuarantinedMailDbContext MyDbContext; // database context for writing messages

        public IdleClientDisposable(string host, int port, SecureSocketOptions sslOptions, string username, string password, QuarantinedMailDbContext dbContext)
        {
            this.client = new ImapClient(new ProtocolLogger(Console.OpenStandardError()));
            this.request = new FetchRequest(MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
            this.messages = new List<IMessageSummary>();
            this.messageIds = new List<UniqueId>();
            this.cancel = new CancellationTokenSource();
            this.messageBodies = new BlockingCollection<string>();
            this.sslOptions = sslOptions;
            this.username = username;
            this.password = password;
            this.host = host;
            this.port = port;
            this.MyDbContext = dbContext; // inject the database context
        }

        public void StartIdleClient()
        {
            const SecureSocketOptions sslOptions = SecureSocketOptions.Auto;

            // configure logging using Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink("logs\\idleclient.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var idleClientConfig = new ConfigurationBuilder()
                .AddJsonFile("IdleClientConfig.json")
                .Build();

            var mailServer = idleClientConfig["mailserver"];
            int port = int.Parse(idleClientConfig["port"]);
            var username = idleClientConfig["email"];
            var password = idleClientConfig["password"];

            // configure dependency injection using Microsoft.Extensions.DependencyInjection
            var services = new ServiceCollection();
            services.AddDbContext<MyDbContext>(options => options.UseSqlServer(idleClientConfig["connectionString"])); // use Entity Framework Core with SQL Server
            var serviceProvider = services.BuildServiceProvider();

            using (var dbContext = serviceProvider.GetRequiredService<MyDbContext>()) // get the database context
            using (var client = new IdleClientDisposable(mailServer, port, sslOptions, username, password, dbContext))
            {
                Log.Information("Starting the idle client.");

                var idleTask = client.RunAsync(); // start the idle task

                var writeTask = client.WriteMessagesAsync(); // start the write task

                Task.WaitAll(idleTask, writeTask); // wait for both tasks to finish

                Log.Information("Exiting the idle client.");
            }
        }

        async Task ReconnectAsync()
        {
            Log.Debug("Reconnecting...");
            if (!client.IsConnected)
                await client.ConnectAsync(host, port, sslOptions, cancel.Token);

            if (!client.IsAuthenticated)
            {
                await client.AuthenticateAsync(username, password, cancel.Token);

                await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);
            }
        }

        async Task FetchMessageSummariesAsync(bool print)
        {
            Log.Debug("Fetching Message Summaries...");
            IList<IMessageSummary> fetched = null;
            do
            {
                try
                {
                    // fetch summary information for messages that we don't already have
                    int startIndex = messages.Count;

                    fetched = client.Inbox.Fetch(startIndex, -1, request, cancel.Token);
                    break;
                }
                catch (ImapProtocolException)
                {
                    // protocol exceptions often result in the client getting disconnected
                    await ReconnectAsync();
                }
                catch (IOException)
                {
                    // I/O exceptions always result in the client getting disconnected
                    await ReconnectAsync();
                }
            } while (true);

            foreach (var message in fetched)
            {
                if (print)
                {
                    messageIds.Add(message.UniqueId);
                    Console.WriteLine("{0}: new message: {1}", client.Inbox, message.Envelope.Subject);
                    Console.WriteLine(message.ToString());
                }
                messages.Add(message);
            }
        }


            // ... other fields and methods ...

        async Task GetMessageBodiesAsync(bool print)
        {
            do
            {
                try
                {
                    foreach (UniqueId id in messageIds)
                    {
                        // Get the message body from the IMAP server
                        var body = client.Inbox.GetMessage(id).HtmlBody;

                        // Add it to the BlockingCollection
                        messageBodies.Add(body, cancel.Token);
                    }
                    break;
                }
                catch (ImapProtocolException ex)
                {
                    // protocol exceptions often result in the client getting disconnected
                    await ReconnectAsync();
                    Log.Error(ex, "An IMAP protocol exception occurred while getting message bodies.");
                }
                catch (IOException ex)
                {
                    // I/O exceptions always result in the client getting disconnected
                    await ReconnectAsync();
                    Log.Error(ex, "An I/O exception occurred while getting message bodies.");
                }
                catch (OperationCanceledException ex)
                {
                    // The cancellation token was triggered
                    Log.Information(ex, "The operation was canceled while getting message bodies.");
                    break;
                }
            } while (true);

            if (print)
            {
                // Remove this part
                /*
                foreach (var messageBody in messageBodies)
                {
                    Console.WriteLine("{0}: new message: {1}", client.Inbox, messageBody);
                }
                */

                // Replace it with this part
                foreach (var messageBody in messageBodies)
                {
                    try
                    {
                        // Parse the message body using MimeMessage
                        var mimeMessage = MimeMessage.Load(messageBody);

                        // Create a QuarantinedMail object from the MimeMessage
                        var quarantinedMail = new QuarantinedMail(mimeMessage);

                        // Add the QuarantinedMail object to the ConcurrentDictionary using its ID as the key
                        quarantinedMails.TryAdd(quarantinedMail.ID, quarantinedMail);

                        // Log the event
                        Log.Information("Created a QuarantinedMail object from a new message: {0}", quarantinedMail.Subject);
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that may occur while parsing or creating the QuarantinedMail object
                        Log.Error(ex, "An exception occurred while creating a QuarantinedMail object from a new message.");
                    }
                }
            }
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // ... other fields and methods ...
    }
}