## 📚 Telegram Quiz Bot (C# + MongoDB + UglyToad PDF Parser)

This is a Telegram bot built in **C#/.NET** that allows users to upload and take multiple-choice quizzes directly in chat. It parses questions from PDF files using **UglyToad.PdfPig**, stores them in a **MongoDB** database, and provides a clean, interactive UI/UX for quiz-taking via Telegram.

> ⚠️ **Note:**  
> This bot is currently in **active development** and not intended as a production-ready solution. Several features are incomplete or experimental.

---

## 🚀 Features

- Upload and parse multiple-choice quizzes from `.pdf` files  
- Store parsed questions in MongoDB  
- Take quizzes directly in Telegram using inline buttons  
- Add new tests manually or via command  
- Clean and responsive UI/UX for Telegram users

---

## ⚙️ Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/quiz-bot.git
   cd quiz-bot
   ```

2. **Configure your settings**
   - Open `appsettings.json`  
   - Replace the marked fields with your own:
     - Telegram Bot Token (from BotFather)  
     - MongoDB connection string  
     - Bot owner ID or target chat ID

3. **Build and run the bot**
   ```bash
   dotnet build
   dotnet run
   ```

4. **Start using your bot**
   - Open Telegram and start chatting with your bot  
   - Use commands to add tests or begin a quiz

---

## 🧪 Current Limitations

- Test names are currently auto-generated from the full content of the quiz, not user-defined titles  
- Answer options are not randomized — only option **A** is supported for correct answers  
- No admin panel or web interface (yet)  
- No authentication or user roles

---

## 🛠 Tech Stack

- **Language:** C# (.NET 8.0.0)  
- **Database:** MongoDB
- **PDF Parsing:** UglyToad.PdfPig  
- **Telegram API:** Telegram.Bot  
- **Architecture:** Console app with async handlers and command routing

---

## 📌 Status

This bot was originally built as a learning and automation tool. It is functional but requires further refinement before production use. Contributions, suggestions, and forks are welcome.
