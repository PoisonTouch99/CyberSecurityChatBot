using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatBot
{
    /// <summary>
    /// Core chatbot response engine.
    /// Handles: keyword recognition, random responses, sentiment detection,
    /// conversation flow, memory recall, input validation, and personal recall queries.
    /// </summary>
    public class ResponseService(UserMemory memory) : ResponseServiceBase
    {
        private readonly Random _random = new();
        private readonly UserMemory _memory = memory;

        // ── Delegate for logging bot actions ──────────────────────────────────
        public delegate void BotActionDelegate(string action);
        public event BotActionDelegate? OnBotAction;

        // ─────────────────────────────────────────────────────────────────────
        // KEYWORD → MULTI-RESPONSE DICTIONARY
        // ─────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, List<string>> _keywordResponses = new()
        {
            ["password"] =
            [
                "- Use strong, unique passwords for every account — at least 12 characters mixing letters, numbers, and symbols.",
                "- Never reuse passwords across sites. A password manager like Bitwarden or 1Password can help you stay organised.",
                "- Avoid using personal info (birthdays, names) in passwords. Attackers can guess these easily through social engineering.",
                "- Enable multi-factor authentication (MFA) alongside a strong password — it adds a critical extra layer of protection."
            ],
            ["phishing"] =
            [
                "<|>Be suspicious of emails asking you to click links or enter credentials. Legitimate companies rarely ask for passwords via email.",
                "<|>Always hover over links before clicking to check the real URL. Phishing sites often mimic real sites with slight spelling changes.",
                "<|> Look for urgency tactics — 'Your account will be closed!' These pressure you into acting without thinking.",
                "<|> When in doubt, navigate directly to the website by typing the URL instead of clicking email links."
            ],
            ["scam"] =
            [
                "__If something sounds too good to be true — a prize, lottery, or job offer — it almost certainly is a scam.",
                "__Never send money or gift cards to someone you haven't verified. Scammers often impersonate banks, SARS, or family members.",
                "__Report scams to the South African Fraud Prevention Service (SAFPS) at 0860 101 248.",
                "__Verify unexpected communications by contacting the organisation directly through their official website or number."
            ],
            ["privacy"] =
            [
                ">> Review your social media privacy settings regularly — limit who can see your posts, location, and contact details.",
                ">> Use a VPN on public Wi-Fi to encrypt your traffic and prevent eavesdropping.",
                ">> Be mindful of what personal data apps request. Deny permissions that aren't necessary for the app's function.",
                ">> Regularly audit which third-party apps have access to your accounts (Google, Facebook) and revoke unnecessary access."
            ],
            ["malware"] =
            [
                "~ Keep your operating system and software updated — many malware attacks exploit unpatched vulnerabilities.",
                "~ Only download software from official sources. Pirated software often contains hidden malware.",
                "~ Use a reputable antivirus solution and run regular scans on your device.",
                "~ Be cautious with USB drives from unknown sources — they can silently install malware when plugged in."
            ],
            ["wifi"] =
            [
                "= Avoid conducting sensitive transactions (banking, shopping) on public Wi-Fi without a VPN.",
                "= Ensure your home router uses WPA3 or WPA2 encryption and has a strong, unique password.",
                "= Disable Wi-Fi auto-connect on your phone to prevent it joining unknown networks automatically.",
                "= Public hotspots in malls and airports are common targets for 'man-in-the-middle' attacks."
            ],
            ["browsing"] =
            [
                "+ Look for HTTPS (the padlock icon) before entering any personal information on a website.",
                "+ Use browser extensions like uBlock Origin to block malicious ads and trackers.",
                "+ Clear your browser cache and cookies regularly, especially after using shared computers.",
                "+ Be wary of browser pop-ups claiming your device is infected — these are almost always scareware."
            ],
            ["2fa"] =
            [
                "* Two-Factor Authentication (2FA) adds a second verification step, making it much harder for attackers to access your accounts.",
                "* Use an authenticator app (like Google Authenticator or Authy) rather than SMS for 2FA — it's more secure.",
                "* Enable 2FA on your email first — it's usually the key to resetting all your other accounts."
            ],
            ["social engineering"] =
            [
                "- Social engineering manipulates people rather than systems. Be sceptical of unsolicited contact asking for access or information.",
                "- Verify identities before sharing any sensitive information, even if the caller claims to be from IT support.",
                "- 'Pretexting' is when attackers invent a scenario to gain your trust. Always verify through a second channel."
            ]
        };

        // ─────────────────────────────────────────────────────────────────────
        // SENTIMENT DETECTION KEYWORDS
        // ─────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string[]> _sentimentKeywords = new()
        {
            ["worried"] = ["worried", "scared", "afraid", "fear", "nervous", "anxious", "concern", "concerned"],
            ["frustrated"] = ["frustrated", "angry", "annoyed", "fed up", "upset", "furious", "irritated"],
            ["curious"] = ["curious", "interested", "want to know", "wondering", "tell me more", "explain", "how does"],
            ["confused"] = ["confused", "don't understand", "not sure", "lost", "unclear", "what do you mean", "help"],
            ["happy"] = ["happy", "great", "awesome", "thanks", "thank you", "helpful", "perfect", "excellent"]
        };

        // ─────────────────────────────────────────────────────────────────────
        // SENTIMENT → EMPATHY RESPONSES
        // ─────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentResponses = new()
        {
            ["worried"] = " It's completely understandable to feel worried — cyber threats are real and growing. Let me help ease your concern.",
            ["frustrated"] = " I hear your frustration. Cybersecurity can feel overwhelming, but let's break it down step by step.",
            ["curious"] = " I love your curiosity! That's exactly the mindset that keeps you safe online.",
            ["confused"] = " No worries at all — cybersecurity has a lot of jargon. Let me explain it more clearly.",
            ["happy"] = " Glad you're feeling good! Let's keep that positive energy and keep your digital life secure too."
        };

        // ─────────────────────────────────────────────────────────────────────
        // GENERAL CONVERSATION RESPONSES
        // ─────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _generalResponses = new()
        {
            ["how are you"] = " I'm fully operational and ready to help you stay safe online! How can I assist you today?",
            ["what is your purpose"] = " My purpose is to educate South African citizens about cybersecurity threats and best practices. Ask me about phishing, passwords, privacy, scams, malware, and more!",
            ["what can i ask"] = " You can ask me about:\n•  Password safety\n•  Phishing emails\n•  Online scams\n•  Privacy settings\n•  Malware protection\n•  Safe Wi-Fi use\n•  Safe browsing\n•  Two-factor authentication\n•  Social engineering",
            ["hello"] = " Hello! Great to have you here. I'm your Cybersecurity Awareness Assistant. What would you like to learn about today?",
            ["hi"] = " Hi there! Ready to boost your cyber awareness? Ask me anything about staying safe online.",
            ["help"] = " I'm here to help! You can ask about password safety, phishing, scams, privacy, malware, Wi-Fi safety, safe browsing, 2FA, or social engineering.",
            ["bye"] = " Stay safe online! Remember to keep your software updated and never share your passwords. Goodbye!",
            ["goodbye"] = " Take care! Your cybersecurity journey continues — stay vigilant out there.",
            ["thanks"] = " You're welcome! Security is a journey, not a destination. Keep asking questions!",
            ["thank you"] = " It's my pleasure to help keep you safe online. Don't hesitate to ask more!"
        };

        // ─────────────────────────────────────────────────────────────────────
        // FOLLOW-UP / CONVERSATION FLOW TRIGGERS
        // ─────────────────────────────────────────────────────────────────────
        private readonly string[] _followUpTriggers =
            ["tell me more", "explain more", "give me another", "more info", "another tip", "continue", "go on", "elaborate"];

        // ─────────────────────────────────────────────────────────────────────
        // PERSONAL RECALL QUERIES
        // Questions the user can ask about what the bot remembers about them.
        // Checked BEFORE general/keyword matching so they always resolve correctly.
        // ─────────────────────────────────────────────────────────────────────
        private string? TryHandlePersonalQuery(string input)
        {
            // ── "What's my name?" / "Do you know my name?" ───────────────────
            if (input.Contains("what") && input.Contains("my name") ||
                input.Contains("do you know my name") ||
                input.Contains("remember my name") ||
                input.Contains("what's my name") ||
                input.Contains("whats my name"))
            {
                if (!string.IsNullOrEmpty(_memory.UserName))
                    return $"Of course! Your name is **{_memory.UserName}**. I remember everything you tell me during our chat.";
                return "I don't know your name yet! You can tell me by saying something like \"My name is Sarah\".";
            }

            // ── "Who am I?" ───────────────────────────────────────────────────
            if (input == "who am i" || input.Contains("who am i?"))
            {
                if (!string.IsNullOrEmpty(_memory.UserName))
                    return $"You are **{_memory.UserName}**! At least, that's the name you gave me. 😄";
                return "I don't know who you are yet — why not introduce yourself? Say \"My name is ...\"";
            }

            // ── "What's my favourite topic?" / "What do I like?" ────────────
            if (input.Contains("my favourite topic") ||
                input.Contains("my favorite topic") ||
                input.Contains("what do i like") ||
                input.Contains("what am i interested in"))
            {
                if (!string.IsNullOrEmpty(_memory.FavouriteTopic))
                    return $"⭐ You told me you're most interested in **{_memory.FavouriteTopic}**. Would you like another tip on that?";
                return "You haven't told me a favourite topic yet. Try saying \"I'm interested in privacy\" or any other topic!";
            }

            // ── "What was my last topic?" / "What did we talk about?" ────────
            if (input.Contains("last topic") ||
                input.Contains("what did we talk about") ||
                input.Contains("what were we discussing") ||
                input.Contains("previous topic"))
            {
                if (!string.IsNullOrEmpty(_memory.LastTopic))
                    return $"The last topic we discussed was **{_memory.LastTopic}**. Want to continue from there?";
                return "We haven't dived into a specific topic yet. Ask me about phishing, passwords, scams, or anything else!";
            }

            // ── "How many questions have I asked?" / "How long have we chatted?" ──
            if (input.Contains("how many") && (input.Contains("question") || input.Contains("message") || input.Contains("turn")) ||
                input.Contains("how long have we") ||
                input.Contains("conversation count") ||
                input.Contains("how many times"))
            {
                return $"We've exchanged **{_memory.ConversationTurns}** message{(_memory.ConversationTurns == 1 ? "" : "s")} so far in this session. Keep the questions coming!";
            }

            // ── "What do you know about me?" / "What do you remember?" ───────
            if (input.Contains("what do you know about me") ||
                input.Contains("what do you remember") ||
                input.Contains("what have you remembered") ||
                input.Contains("tell me about me") ||
                input.Contains("what have you stored"))
            {
                var lines = new List<string> { " Here's everything I remember about you:" };

                if (!string.IsNullOrEmpty(_memory.UserName))
                    lines.Add($"   Name: {_memory.UserName}");
                else
                    lines.Add("   Name: not told yet");

                if (!string.IsNullOrEmpty(_memory.FavouriteTopic))
                    lines.Add($"   Favourite topic: {_memory.FavouriteTopic}");

                if (!string.IsNullOrEmpty(_memory.LastTopic))
                    lines.Add($"   Last topic discussed: {_memory.LastTopic}");

                if (!string.IsNullOrEmpty(_memory.LastSentiment))
                    lines.Add($"   Last detected mood: {_memory.LastSentiment}");

                lines.Add($"   Messages exchanged: {_memory.ConversationTurns}");

                return string.Join("\n", lines);
            }

            // ── "What's my mood?" / "What mood did you detect?" ──────────────
            if (input.Contains("my mood") ||
                input.Contains("what mood") ||
                input.Contains("how am i feeling") ||
                input.Contains("what sentiment"))
            {
                if (!string.IsNullOrEmpty(_memory.LastSentiment))
                    return $"Based on your messages, the last mood I detected was **{_memory.LastSentiment}**. Is that accurate?";
                return "I haven't detected a particular mood from you yet — you seem pretty neutral! ";
            }

            // ── "What's my name and favourite topic?" (combined) ─────────────
            if (input.Contains("my name") && input.Contains("favourite"))
            {
                string name = string.IsNullOrEmpty(_memory.UserName) ? "unknown" : _memory.UserName;
                string topic = string.IsNullOrEmpty(_memory.FavouriteTopic) ? "not set yet" : _memory.FavouriteTopic;
                return $"Your name is **{name}** and your favourite topic is **{topic}**.";
            }

            return null; // not a personal query — continue normal processing
        }

        // ─────────────────────────────────────────────────────────────────────
        // MAIN ENTRY POINT
        // ─────────────────────────────────────────────────────────────────────
        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "❓ It looks like you didn't type anything. Feel free to ask me about cybersecurity topics!";

            string input = userInput.Trim().ToLower();
            _memory.ConversationTurns++;

            // ── 1. FOLLOW-UP / CONVERSATION FLOW ─────────────────────────────
            if (_followUpTriggers.Any(t => input.Contains(t)))
            {
                if (!string.IsNullOrEmpty(_memory.LastTopic))
                {
                    OnBotAction?.Invoke($"Continued topic: {_memory.LastTopic}");
                    return GetRandomResponseForTopic(_memory.LastTopic);
                }
                return "I'd love to tell you more — could you specify which topic you'd like to explore further?";
            }

            // ── 2. NAME CAPTURE ───────────────────────────────────────────────
            if (input.StartsWith("my name is ") || input.StartsWith("i am ") || input.StartsWith("i'm "))
            {
                string name = ExtractName(input);
                if (!string.IsNullOrEmpty(name))
                {
                    _memory.UserName = name;
                    OnBotAction?.Invoke($"Remembered user name: {name}");
                    return $"Nice to meet you, {_memory.UserName}! I'll remember your name throughout our conversation. How can I help you stay safe online today?";
                }
            }

            // ── 3. FAVOURITE TOPIC CAPTURE ────────────────────────────────────
            if (input.Contains("i'm interested in") || input.Contains("i am interested in") || input.Contains("i care about"))
            {
                foreach (var key in _keywordResponses.Keys)
                {
                    if (input.Contains(key))
                    {
                        _memory.FavouriteTopic = key;
                        OnBotAction?.Invoke($"Remembered favourite topic: {key}");
                        string namePrefix = _memory.UserName.Length > 0 ? $"{_memory.UserName}, " : "";
                        return $" {namePrefix}I'll remember that you're particularly interested in **{key}**. It's a crucial part of staying safe online! Here's something relevant:\n\n{GetRandomResponseForTopic(key)}";
                    }
                }
            }

            // ── 4. PERSONAL RECALL QUERIES ────────────────────────────────────
            string? personalResponse = TryHandlePersonalQuery(input);
            if (personalResponse != null)
            {
                OnBotAction?.Invoke("Personal recall query answered");
                return personalResponse;
            }

            // ── 5. SENTIMENT DETECTION ────────────────────────────────────────
            string detectedSentiment = DetectSentiment(input);
            string sentimentPrefix = string.Empty;

            if (!string.IsNullOrEmpty(detectedSentiment) && detectedSentiment != _memory.LastSentiment)
            {
                _memory.LastSentiment = detectedSentiment;
                sentimentPrefix = _sentimentResponses[detectedSentiment] + "\n\n";
                OnBotAction?.Invoke($"Detected sentiment: {detectedSentiment}");
            }

            // ── 6. GENERAL CONVERSATION ───────────────────────────────────────
            foreach (var kvp in _generalResponses)
            {
                if (input.Contains(kvp.Key))
                {
                    OnBotAction?.Invoke($"General query: {kvp.Key}");
                    return sentimentPrefix + kvp.Value;
                }
            }

            // ── 7. KEYWORD TOPIC RECOGNITION ─────────────────────────────────
            foreach (var kvp in _keywordResponses)
            {
                if (input.Contains(kvp.Key))
                {
                    _memory.LastTopic = kvp.Key;
                    OnBotAction?.Invoke($"Keyword recognised: {kvp.Key}");
                    string response = GetRandomResponseForTopic(kvp.Key);

                    if (!string.IsNullOrEmpty(_memory.FavouriteTopic) && _memory.FavouriteTopic != kvp.Key)
                    {
                        string nameNote = _memory.UserName.Length > 0 ? $"{_memory.UserName}, " : "";
                        response += $"\n\n {nameNote}as someone interested in {_memory.FavouriteTopic}, you might also want to review your {_memory.FavouriteTopic} settings while you're at it!";
                    }

                    return sentimentPrefix + response;
                }
            }

            // ── 8. MEMORY-AWARE PERIODIC RECALL ──────────────────────────────
            if (_memory.ConversationTurns % 5 == 0 && !string.IsNullOrEmpty(_memory.FavouriteTopic))
            {
                string namePrefix = _memory.UserName.Length > 0 ? _memory.UserName : "you";
                return $"By the way — since {namePrefix} mentioned being interested in **{_memory.FavouriteTopic}**, here's a timely tip:\n\n{GetRandomResponseForTopic(_memory.FavouriteTopic)}";
            }

            // ── 9. DEFAULT / FALLBACK ─────────────────────────────────────────
            OnBotAction?.Invoke("Unrecognised input — fallback response");
            return sentimentPrefix +
                   "I didn't quite understand that. Could you rephrase?\n\n" +
                   "Try asking about: **password**, **phishing**, **scam**, **privacy**, **malware**, **wifi**, **browsing**, **2fa**, or **social engineering**.\n" +
                   "You can also ask things like \"What's my name?\" or \"What do you remember about me?\"";
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER: extract name from "my name is X" / "I am X" / "I'm X"
        // ─────────────────────────────────────────────────────────────────────
        private string ExtractName(string input)
        {
            string[] prefixes = ["my name is ", "i am ", "i'm "];
            foreach (var prefix in prefixes)
            {
                if (input.StartsWith(prefix))
                {
                    string name = input.Substring(prefix.Length).Trim();
                    if (!string.IsNullOrEmpty(name))
                        return char.ToUpper(name[0]) + name.Substring(1);
                }
            }
            return string.Empty;
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER: random response for a topic key
        // ─────────────────────────────────────────────────────────────────────
        private string GetRandomResponseForTopic(string topicKey)
        {
            if (_keywordResponses.TryGetValue(topicKey, out var responses))
                return responses[_random.Next(responses.Count)];
            return "I have tips on that topic but need more context. Could you ask more specifically?";
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER: detect sentiment in input
        // ─────────────────────────────────────────────────────────────────────
        private string DetectSentiment(string input)
        {
            foreach (var kvp in _sentimentKeywords)
                if (kvp.Value.Any(word => input.Contains(word)))
                    return kvp.Key;
            return string.Empty;
        }

        /// <summary>Returns current user memory for display.</summary>
        public UserMemory GetMemory() => _memory;
    }
}
