# CyberSecurity Awareness Chatbot — Part 2

**Student:** Thandolwethu Masilela  
**Student Number:** ST10471194  
**Module:** PROG6221 — Programming 2A  
**Submission:** Part 2 — GUI Interface, Dynamic Responses, Sentiment Detection & Memory

---

## What happens in this section?

Part 2 upgrades the console chatbot from Part 1 into a **GUI** application, which makes it much easier for the user to understand.  
The chatbot helps South African citizens learn about cybersecurity through an interactive, intelligent conversation interface.

---

## Part 2 Features Implemented

| Feature | Description |
|---|---|
| **WPF GUI** | Full Windows Presentation Foundation interface with dark cybersecurity theme |
| **Voice Greeting** | Plays `Resources/greeting.wav` on startup (carried forward from Part 1) |
| **ASCII Art Logo** | Displayed as the first chat message on launch |
| **Keyword Recognition** | Recognises 9 cybersecurity topics: password, phishing, scam, privacy, malware, wifi, browsing, 2fa, social engineering |
| **Random Responses** | Each keyword has 3–4 varied responses, randomly selected |
| **Conversation Flow** | "tell me more", "give me another tip", "explain more" continue the last topic |
| **Memory & Recall** | Remembers user's name, favourite topic, last topic, mood |
| **Sentiment Detection** | Detects worried, frustrated, curious, confused, happy — adjusts response tone |
| **Input Validation** | Handles empty input and unrecognised queries gracefully |
| **Quick Topic Buttons** | Left sidebar shortcuts to instantly explore any topic |
| **Memory Panel** | Right sidebar displays remembered user details in real-time |
| **MVVM Architecture** | ViewModel + RelayCommand + INotifyPropertyChanged |
| **Delegate / Event** | `BotActionDelegate` event fires on every significant bot action |

---

## 🚀 How to Run

### Requirements
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Visual Studio 2022 (or `dotnet` CLI)

### Steps

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/CyberSecurityAwarenessChatBot.git
cd CyberSecurityAwarenessChatBot

# Build and run
dotnet run --project CyberSecurityChatBot/CyberSecurityChatBot.csproj
```

Or open `CyberSecurityChatBot.csproj` in Visual Studio 2022 and press **F5**.

### Voice Greeting
Place your WAV file at:
```
CyberSecurityChatBot/Resources/greeting.wav
```
The app will play it on startup if present, and skip silently if not found.

---

## How to Use the Chatbot

1. **Enter your name** when prompted (just type it and press Enter)
2. **Ask about any cybersecurity topic** — type naturally or use the Quick Topic buttons
3. **Follow-up** — say "tell me more" or "give me another tip" to get more on the same topic
4. **Share your interest** — say "I'm interested in privacy" to personalise your experience
5. **Express your feelings** — say "I'm worried about scams" and the bot will respond empathetically

### Example Interactions

```
You:     Thandolwethu
Bot:     Welcome, Thandolwethu! How can I help you stay safe online?

You:     Tell me about phishing
Bot:     - Be suspicious of emails asking you to click links...

You:     Tell me more
Bot:     - Always hover over links before clicking to check the real URL...

You:     I'm worried about online scams
Bot:     - It's completely understandable to feel worried...
         - If something sounds too good to be true — it almost certainly is a scam.

You:     I'm interested in privacy
Bot:     I'll remember that you're particularly interested in privacy!
```

---

## This is the Project Structure

```
CyberSecurityChatBot/
--- Models/
|    -- ChatMessage.cs          # Single chat message model
|    -- UserMemory.cs           # User memory / recall store
--- ViewModels/
|    --ChatViewModel.cs        # MVVM ViewModel + RelayCommand
--- Views/
|    --MainWindow.xaml         # WPF GUI layout
|    -- MainWindow.xaml.cs      # Code-behind + value converters
--- Services/
|    -- ResponseService.cs      # All chatbot logic (keywords, sentiment, memory)
|    -- VoiceService.cs         # WAV playback
--- Resources/
|    -- greeting.wav            # (Add your own WAV here)
--- App.xaml                    # Global styles and colour palette
--- App.xaml.cs
```

---

## GitHub Actions CI

The CI workflow (`.github/workflows/dotnet.yml`) runs on every push:
- Restores NuGet packages
- Builds in Release configuration on `windows-latest`
- Uploads build artefacts

>  Screenshot of passing CI workflow: *(add screenshot here)*

---

##  Commit History

| # | Message |
|---|---------|
| 1 | Initial commit: Set up WPF project structure and solution |
| 2 | Added Models: ChatMessage and UserMemory |
| 3 | Implemented ResponseService with keyword recognition and random responses |
| 4 | Added sentiment detection and conversation flow |
| 5 | Built WPF MainWindow XAML with dark cybersecurity theme |
| 6 | Connected ViewModel, added memory panel and quick topic sidebar |

---

##  References

Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review. *The African Journal of Information and Communication*, 28(28). doi: https://doi.org/10.23962/10539/32213. [Online]. Available at: https://www.scielo.org.za/scielo.php?pid=S2077-72132021000200003&script=sci_arttext [Accessed 16 February 2026].
