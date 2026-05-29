using System;

namespace CyberSecurityChatBot.Models
{
    /// <summary>
    /// Represents a single chat message in the conversation.
    /// Contains the message content, sender info, timestamp, and whether it's from the user or bot.
    /// </summary>

    // This class is used to store and display messages in the chat interface.
    public class ChatMessage
    {
        // Properties to store message content, sender info, timestamp, and sender name.
        public string Content { get; set; }
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string SenderName { get; set; }

        // Constructor to initialize a chat message with content, sender info, and optional sender name.
        public ChatMessage(string content, bool isFromUser, string senderName = "")
        {
            Content = content;
            IsFromUser = isFromUser;
            Timestamp = DateTime.Now;
            SenderName = senderName;
        }
    }
}
