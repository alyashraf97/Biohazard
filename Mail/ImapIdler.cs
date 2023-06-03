using Biohazard.Shared;
using Biohazard.Worker;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace Biohazard.Mail
{
	class ImapIdler : IDisposable
	{
		private IMapConfig conf;
		Dictionary<UniqueId, MimeMessage> messages;
		CancellationTokenSource cancel;
		CancellationTokenSource done;
		FetchRequest request;
		bool messagesArrived;
		ImapClient client;
		private Serilog.ILogger _log = QLogger.GetLogger<ImapIdler>();
		QMailQueue<MimeMessage> queue;

		public ImapIdler()
		{
			client = new ImapClient(new ProtocolLogger(File.Open("imap_protocol_logs.log", FileMode.OpenOrCreate)));
			request = new FetchRequest(MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
			messages = new Dictionary<UniqueId, MimeMessage>();
			cancel = new CancellationTokenSource();
			conf = new IMapConfig();
			queue = QMailQueue<MimeMessage>.Instance;
		}

		public void StartIdler()
		{
			using (var client = new ImapIdler())
			{
				var idleTask = client.RunAsync();

				Task.Run(() =>
				{
					Task.WaitAll();
				}).Wait();

				client.Exit();
				idleTask.GetAwaiter().GetResult();
				_log.Information($"Idle Client Exited at: {DateTime.Now}");
			}
		}

		async Task ReconnectAsync()
		{
			if (!client.IsConnected)
				await client.ConnectAsync(conf.Host, conf.Port, conf.Encryption, cancel.Token);

			if (!client.IsAuthenticated)
			{
				await client.AuthenticateAsync(conf.Username, conf.Password, cancel.Token);

				await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);
			}
		}

		async Task GetMessages()
		{
			Dictionary<UniqueId, MimeMessage> fetched = new();

			do
			{
				try
				{
					// fetch summary information for messages that we don't already have
					//int startIndex = messages.Count;
					var uids = client.Inbox.Search(SearchQuery.NotSeen);
					foreach (var uid in uids)
					{
						if (!(messages.ContainsKey(uid)))
						{
							var message = await client.Inbox.GetMessageAsync(uid, cancel.Token);
							if (message != null)
							{
								fetched.Add(uid, message);
							}
						}
					}

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
				_log.Information($"{client.Inbox}: new message: UID {message.Key}");
				messages.Add(message.Key, message.Value);
				queue.EnqueueQMail(message.Value);
			}
			fetched.Clear();
		}

		async Task WaitForNewMessagesAsync()
		{
			do
			{
				try
				{
					if (client.Capabilities.HasFlag(ImapCapabilities.Idle))
					{
						// Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
						// we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
						// about 10 minutes, so we'll only idle for 9 minutes.
						done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
						try
						{
							await client.IdleAsync(done.Token, cancel.Token);
						}
						finally
						{
							done.Dispose();
							done = null;
						}
					}
					else
					{
						// Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
						// between each NOOP command.
						await Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
						await client.NoOpAsync(cancel.Token);
					}
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
		}

		async Task IdleAsync()
		{
			do
			{
				try
				{
					await WaitForNewMessagesAsync();

					if (messagesArrived)
					{
						await GetMessages();
						messagesArrived = false;
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
			} while (!cancel.IsCancellationRequested);
		}

		public async Task RunAsync()
		{
			// connect to the IMAP server and get our initial list of messages
			try
			{
				await ReconnectAsync();
				await GetMessages();
			}
			catch (OperationCanceledException)
			{
				await client.DisconnectAsync(true);
				return;
			}

			// Note: We capture client.Inbox here because cancelling IdleAsync() *may* require
			// disconnecting the IMAP client connection, and, if it does, the `client.Inbox`
			// property will no longer be accessible which means we won't be able to disconnect
			// our event handlers.
			var inbox = client.Inbox;

			// keep track of changes to the number of messages in the folder (this is how we'll tell if new messages have arrived).
			inbox.CountChanged += OnCountChanged;

			// keep track of messages being expunged so that when the CountChanged event fires, we can tell if it's
			// because new messages have arrived vs messages being removed (or some combination of the two).

			// keep track of flag changes
			inbox.MessageFlagsChanged += OnMessageFlagsChanged;

			await IdleAsync();

			inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
			inbox.CountChanged -= OnCountChanged;

			await client.DisconnectAsync(true);
		}

		// Note: the CountChanged event will fire when new messages arrive in the folder and/or when messages are expunged.
		void OnCountChanged(object sender, EventArgs e)
		{
			var folder = (ImapFolder)sender;

			// Note: because we are keeping track of the MessageExpunged event and updating our
			// 'messages' list, we know that if we get a CountChanged event and folder.Count is
			// larger than messages.Count, then it means that new messages have arrived.
			if (folder.Count > messages.Count)
			{
				int arrived = folder.Count - messages.Count;

				if (arrived > 1)
					_log.Information("\t{0} new messages have arrived.", arrived);
				else
					_log.Information("\t1 new message has arrived.");

				// Note: your first instinct may be to fetch these new messages now, but you cannot do
				// that in this event handler (the ImapFolder is not re-entrant).
				// 
				// Instead, cancel the `done` token and update our state so that we know new messages
				// have arrived. We'll fetch the summaries for these new messages later...
				messagesArrived = true;
				done?.Cancel();
			}
		}

		void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
		{
			var folder = (ImapFolder)sender;

			_log.Information("{0}: flags have changed for message #{1} ({2}).", folder, e.Index, e.Flags);
		}

		public void Exit()
		{
			cancel.Cancel();
		}

		public void Dispose()
		{
			client.Dispose();
			cancel.Dispose();
		}
	}
}