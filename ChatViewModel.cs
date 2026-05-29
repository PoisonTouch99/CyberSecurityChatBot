using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CyberSecurityChatBot.Models;

namespace CyberSecurityChatBot.ViewModels
{
    /// <summary>
    /// Main ViewModel — binds the chat UI to the ResponseService.
    /// Implements INotifyPropertyChanged for WPF data binding.
    /// </summary>
    public class ChatViewModel : INotifyPropertyChanged
    {
        // ── Services ─────────────────────────────────────────────────────────
        private readonly ResponseService _responseService;
        private readonly VoiceService _voiceService;
        private readonly UserMemory _userMemory;

        // ── Bindable collections ──────────────────────────────────────────────
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        // ── Bindable properties ───────────────────────────────────────────────
        private string _userInput = string.Empty;

        // UserInput is bound to the input TextBox. When it changes, we raise PropertyChanged to update the UI.
        public string UserInput
        {
            get => _userInput;
            set { _userInput = value; OnPropertyChanged(); }
        }

        // StatusText is bound to a status label in the UI to show current status (e.g., "Waiting for your name...", "Thinking...)
        private string _statusText = "Ready";

        // StatusText is updated throughout the conversation to reflect the chatbot's current state.
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // MemoryDisplay is bound to a panel in the UI that shows what the chatbot currently "remembers" about the user.
        private string _memoryDisplay = "No user details remembered yet.";

        // MemoryDisplay is updated after each interaction to reflect the current state of the UserMemory.
        public string MemoryDisplay
        {
            get => _memoryDisplay;
            set { _memoryDisplay = value; OnPropertyChanged(); }
        }

        // Internal flag to track if we're still waiting for the user's name (used in SendMessage logic)
        private bool _isWaitingForName = true;

        // These commands are bound to the Send button, Clear button, and quick topic buttons in the UI. They trigger the corresponding methods when executed.
        public ICommand SendCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand QuickTopicCommand { get; }

        // Constructor initializes services, commands, and starts the conversation by calling Initialise.
        public ChatViewModel()
        {
            _userMemory = new UserMemory();
            _responseService = new ResponseService(_userMemory);
            _voiceService = new VoiceService();

            // Subscribe to bot action events (delegate usage — Part 2 requirement)
            _responseService.OnBotAction += HandleBotAction;

            SendCommand = new RelayCommand(_ => SendMessage(), _ => !string.IsNullOrWhiteSpace(UserInput));
            ClearCommand = new RelayCommand(_ => ClearChat());
            QuickTopicCommand = new RelayCommand(topic => SendQuickTopic(topic?.ToString() ?? ""));

            Initialise();
        }

        // ─────────────────────────────────────────────────────────────────────
        // INITIALISE — show ASCII art, play voice, ask for name
        // ─────────────────────────────────────────────────────────────────────
        private void Initialise()
        {
            // Play voice greeting (Part 1 / Part 2 carry-over)
            _voiceService.PlayGreeting();

            // ASCII art logo displayed in first message
            string asciiArt =
@"
  ██████╗██╗   ██╗██████╗ ███████╗██████╗
 ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗   ██║   ██████╔╝███████╗██║  ██║
  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝
   Cybersecurity Awareness Assistant v2.0
";

            AddBotMessage(asciiArt);
            AddBotMessage("Welcome to the Cybersecurity Awareness Chatbot!\n\nI'm here to help South African citizens stay safe online.\n\nTo get started, please tell me your name:");
            StatusText = "Waiting for your name...";
        }

        // This method is called when the user clicks the Send button or presses Enter. It processes the user input, updates the chat messages, and interacts with the ResponseService to get a response.
        private void SendMessage()
        {
            string input = UserInput.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            // Add user message to chat
            string displayName = string.IsNullOrEmpty(_userMemory.UserName) ? "You" : _userMemory.UserName;
            Messages.Add(new ChatMessage(input, true, displayName));
            UserInput = string.Empty;

            // First message — capture name if not yet set
            if (_isWaitingForName && string.IsNullOrEmpty(_userMemory.UserName))
            {
                // Try to extract name from input
                string cleaned = input.ToLower()
                    .Replace("my name is", "").Replace("i am", "").Replace("i'm", "").Trim();

                // Simple validation: check if cleaned input is a single word (basic name check)
                if (!string.IsNullOrEmpty(cleaned))
                {
                    string name = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
                    _userMemory.UserName = name;
                    _isWaitingForName = false;
                    UpdateMemoryDisplay();
                    AddBotMessage($"Welcome, **{name}**! It's great to meet you.\n\nI'm your personal Cybersecurity Awareness Assistant. You can ask me about:\n Passwords |  Phishing | Scams | Privacy | Malware | Wi-Fi | Browsing | 2FA\n\nWhat would you like to learn about first?");
                    StatusText = $"Chatting with {name}";
                    return;
                }
            }

            _isWaitingForName = false;

            // Process user input and get response from ResponseService
            StatusText = "Thinking...";
            string response = _responseService.GetResponse(input);
            AddBotMessage(response);
            UpdateMemoryDisplay();
            StatusText = string.IsNullOrEmpty(_userMemory.UserName) ? "Ready" : $"Chatting with {_userMemory.UserName}";
        }

        // This method is used for the quick topic buttons. It sets the UserInput to the topic and then calls SendMessage to process it as if the user had typed it.
        private void SendQuickTopic(string topic)
        {
            UserInput = topic;
            SendMessage();
        }

        // For the Clear button — resets the conversation and clears memory. This is a simple way to start over without restarting the app.
        private void ClearChat()
        {
            Messages.Clear();
            Initialise();
            _isWaitingForName = true;
            _userMemory.UserName = string.Empty;
            _userMemory.FavouriteTopic = string.Empty;
            _userMemory.LastTopic = string.Empty;
            _userMemory.ConversationTurns = 0;
            UpdateMemoryDisplay();
        }

        // This method adds a bot message to the Messages collection. It takes the content as a parameter and creates a new ChatMessage with IsFromUser set to false and SenderName set to "CyberBot".
        private void AddBotMessage(string content)
        {
            Messages.Add(new ChatMessage(content, false, "CyberBot"));
        }

        // This method handles bot actions sent from the ResponseService. It currently just logs the action, but in a full implementation it could update the UI or trigger other behaviors based on the action type.
        private void HandleBotAction(string action)
        {
            System.Diagnostics.Debug.WriteLine($"[BotAction] {action}");
            // In a full implementation this feeds the activity log
        }

        // This method updates the MemoryDisplay string based on the current state of the UserMemory. It checks each property and builds a display string accordingly.
        private void UpdateMemoryDisplay()
        {
            var mem = _responseService.GetMemory();
            string display = "";
            if (!string.IsNullOrEmpty(mem.UserName))
                display += $" Name: {mem.UserName}\n";
            if (!string.IsNullOrEmpty(mem.FavouriteTopic))
                display += $" Favourite topic: {mem.FavouriteTopic}\n";
            if (!string.IsNullOrEmpty(mem.LastTopic))
                display += $" Last topic: {mem.LastTopic}\n";
            if (!string.IsNullOrEmpty(mem.LastSentiment))
                display += $" Last mood: {mem.LastSentiment}\n";
            display += $" Turns: {mem.ConversationTurns}";
            MemoryDisplay = display;
        }


        // INotifyPropertyChanged implementation for WPF data binding
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Simple implementation of ICommand for WPF commands (Send, Clear, QuickTopic)
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // CanExecute checks if the command can run (e.g., if input is not empty for SendCommand)
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
