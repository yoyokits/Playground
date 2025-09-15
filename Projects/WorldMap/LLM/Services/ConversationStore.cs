// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using LLM.Models;

    public static class ConversationStore
    {
        private static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WorldMapApp", "Conversations");
        private static readonly string FilePath = Path.Combine(Folder, "conversations.json");

        public static ObservableCollection<Conversation> Conversations { get; } = new();
        public static Conversation? ActiveConversation { get; private set; }

        public static event EventHandler? ActiveConversationChanged;

        public static void EnsureActive()
        {
            if (ActiveConversation == null)
            {
                var conv = new Conversation();
                Conversations.Add(conv);
                ActiveConversation = conv;
                ActiveConversationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static Conversation AddNew()
        {
            var conv = new Conversation();
            Conversations.Add(conv);
            ActiveConversation = conv;
            ActiveConversationChanged?.Invoke(null, EventArgs.Empty);
            return conv;
        }

        public static void Activate(Conversation conversation)
        {
            if (conversation == ActiveConversation) return;
            ActiveConversation = conversation;
            ActiveConversationChanged?.Invoke(null, EventArgs.Empty);
        }

        public static bool RemoveConversation(Conversation conversation)
        {
            if (conversation == null) return false;
            
            var removed = Conversations.Remove(conversation);
            if (removed && conversation == ActiveConversation)
            {
                // If we removed the active conversation, set a new active one
                ActiveConversation = Conversations.FirstOrDefault();
                if (ActiveConversation == null)
                {
                    // If no conversations left, create a new one
                    EnsureActive();
                }
                else
                {
                    ActiveConversationChanged?.Invoke(null, EventArgs.Empty);
                }
            }
            return removed;
        }

        public static void UpdateFromMessages(IEnumerable<ChatMessage> messages)
        {
            if (ActiveConversation == null) return;
            ActiveConversation.Messages = messages.ToList();
            ActiveConversation.LastUpdated = DateTime.UtcNow;
        }

        public static async Task LoadAsync()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = await File.ReadAllTextAsync(FilePath);
                    var items = JsonSerializer.Deserialize<Conversation[]>(json) ?? Array.Empty<Conversation>();
                    Conversations.Clear();
                    foreach (var c in items.OrderByDescending(c => c.LastUpdated))
                        Conversations.Add(c);
                    ActiveConversation = Conversations.FirstOrDefault();
                }
            }
            catch
            {
                // ignore load errors
            }
            finally
            {
                EnsureActive();
            }
        }

        public static async Task SaveAsync()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                var ordered = Conversations.OrderByDescending(c => c.LastUpdated).ToArray();
                var json = JsonSerializer.Serialize(ordered, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(FilePath, json);
            }
            catch
            {
                // ignore save errors
            }
        }
    }
}
