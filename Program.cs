using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HangmanGameAvalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new HangmanWindow();

        base.OnFrameworkInitializationCompleted();
    }
}

public class HangmanWindow : Window
{
    private readonly string[] words =
    {
        "COMPUTER", "PROGRAMMER"
    };

    private readonly Random random = new();
    private readonly HashSet<char> guessedLetters = new();
    private readonly Dictionary<char, Button> letterButtons = new();

    private string secretWord = "";
    private int mistakes = 0;
    private const int MaxMistakes = 6;

    private readonly HangmanCanvas canvas = new();
    private readonly TextBlock wordText = new();
    private readonly TextBlock mistakesText = new();
    private readonly TextBlock currentLetterText = new();
    private readonly TextBlock messageText = new();
    private readonly StackPanel keyboardPanel = new();
    private readonly Button restartButton = new();

    public HangmanWindow()
    {
        Title = "Gallows";
        Width = 760;
        Height = 760;
        MinWidth = 700;
        MinHeight = 700;
        Background = Brushes.White;

        BuildUI();
        NewGame();
    }

    private void BuildUI()
    {
        var root = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12
        };

        root.Children.Add(new TextBlock
        {
            Text = "Game «Hangman»",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        canvas.Width = 360;
        canvas.Height = 270;
        canvas.HorizontalAlignment = HorizontalAlignment.Center;

        wordText.FontSize = 34;
        wordText.FontWeight = FontWeight.Bold;
        wordText.HorizontalAlignment = HorizontalAlignment.Center;
        wordText.LetterSpacing = 5;

        mistakesText.FontSize = 18;
        mistakesText.HorizontalAlignment = HorizontalAlignment.Center;

        currentLetterText.FontSize = 22;
        currentLetterText.FontWeight = FontWeight.Bold;
        currentLetterText.Foreground = Brushes.DarkBlue;
        currentLetterText.HorizontalAlignment = HorizontalAlignment.Center;

        messageText.FontSize = 18;
        messageText.FontWeight = FontWeight.Bold;
        messageText.HorizontalAlignment = HorizontalAlignment.Center;
        messageText.TextWrapping = TextWrapping.Wrap;

        keyboardPanel.Spacing = 6;
        keyboardPanel.HorizontalAlignment = HorizontalAlignment.Center;

        AddKeyboardRow("ABCDEFG");
        AddKeyboardRow("HIJKLMN");
        AddKeyboardRow("OPQRSTU");
        AddKeyboardRow("VWXYZ");

        restartButton.Content = "New game";
        restartButton.FontSize = 18;
        restartButton.Padding = new Thickness(20, 8);
        restartButton.HorizontalAlignment = HorizontalAlignment.Center;
        restartButton.Click += (_, _) => NewGame();

        root.Children.Add(canvas);
        root.Children.Add(wordText);
        root.Children.Add(mistakesText);
        root.Children.Add(currentLetterText);
        root.Children.Add(messageText);
        root.Children.Add(keyboardPanel);
        root.Children.Add(restartButton);

        Content = root;
    }

    private void AddKeyboardRow(string letters)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        foreach (char letter in letters)
        {
            var button = new Button
            {
                Content = letter.ToString(),
                Width = 56,
                Height = 42,
                FontSize = 18
            };

            button.Click += (_, _) => GuessLetter(letter);

            letterButtons[letter] = button;
            row.Children.Add(button);
        }

        keyboardPanel.Children.Add(row);
    }

    private void NewGame()
    {
        secretWord = words[random.Next(words.Length)];
        guessedLetters.Clear();
        mistakes = 0;

        foreach (Button button in letterButtons.Values)
            button.IsEnabled = true;

        messageText.Text = "Press the letters on the screen.";
        messageText.Foreground = Brushes.DarkBlue;
        currentLetterText.Text = "Сurrent letter: _";

        UpdateUI();
    }

    private void GuessLetter(char letter)
    {
        currentLetterText.Text = "Сurrent letter: " + letter;

        if (guessedLetters.Contains(letter))
            return;

        guessedLetters.Add(letter);

        if (letterButtons.TryGetValue(letter, out Button? button))
            button.IsEnabled = false;

        if (secretWord.Contains(letter))
        {
            messageText.Text = "Right!";
            messageText.Foreground = Brushes.DarkGreen;
        }
        else
        {
            mistakes++;
            messageText.Text = "Wrong.";
            messageText.Foreground = Brushes.DarkRed;
        }

        UpdateUI();
        CheckGame();
    }

    private void UpdateUI()
    {
        wordText.Text = string.Join(" ", secretWord.Select(c => guessedLetters.Contains(c) ? c.ToString() : "_"));
        mistakesText.Text = $"Mistakes: {mistakes}/{MaxMistakes}";
        canvas.Mistakes = mistakes;
        canvas.InvalidateVisual();
    }

    private void CheckGame()
    {
        bool win = secretWord.All(c => guessedLetters.Contains(c));

        if (win)
        {
            messageText.Text = "Victory! Word: " + secretWord;
            messageText.Foreground = Brushes.DarkGreen;
            DisableLetters();
        }
        else if (mistakes >= MaxMistakes)
        {
            messageText.Text = "Loss. Word: " + secretWord;
            messageText.Foreground = Brushes.DarkRed;
            DisableLetters();
        }
    }

    private void DisableLetters()
    {
        foreach (Button button in letterButtons.Values)
            button.IsEnabled = false;
    }
}

public class HangmanCanvas : Control
{
    public int Mistakes { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var pen = new Pen(Brushes.Black, 5);
        var bodyPen = new Pen(Brushes.DarkRed, 5);

        context.FillRectangle(Brushes.WhiteSmoke, new Rect(Bounds.Size));

        context.DrawLine(pen, new Point(50, 240), new Point(250, 240));
        context.DrawLine(pen, new Point(90, 240), new Point(90, 30));
        context.DrawLine(pen, new Point(90, 30), new Point(210, 30));
        context.DrawLine(pen, new Point(210, 30), new Point(210, 65));

        if (Mistakes >= 1)
            context.DrawEllipse(null, bodyPen, new Point(210, 90), 25, 25);

        if (Mistakes >= 2)
            context.DrawLine(bodyPen, new Point(210, 115), new Point(210, 170));

        if (Mistakes >= 3)
            context.DrawLine(bodyPen, new Point(210, 130), new Point(175, 150));

        if (Mistakes >= 4)
            context.DrawLine(bodyPen, new Point(210, 130), new Point(245, 150));

        if (Mistakes >= 5)
            context.DrawLine(bodyPen, new Point(210, 170), new Point(180, 210));

        if (Mistakes >= 6)
            context.DrawLine(bodyPen, new Point(210, 170), new Point(240, 210));
    }
}