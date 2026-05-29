namespace CyberSecurityChatBot
{
    /// <summary>
    /// Stores information the chatbot "remembers" about the user across the conversation.
    /// Implements memory and recall feature (Part 2 requirement).
    /// </summary>
    public class UserMemory
    {
        // Properties to store user information and conversation context.
        public string UserName { get; set; } = string.Empty;
        public string FavouriteTopic { get; set; } = string.Empty;
        public string LastTopic { get; set; } = string.Empty;
        public string LastSentiment { get; set; } = string.Empty;
        public bool HasGreeted { get; set; } = false;
        public int ConversationTurns { get; set; } = 0;
    }
}
