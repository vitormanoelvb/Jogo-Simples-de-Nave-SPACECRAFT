using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    private static int playerPosition = Console.WindowWidth / 2; // Posição inicial da nave do jogador
    private static List<int> bullets = new List<int>(); // Lista de tiros
    private static List<int> enemies = new List<int>(); // Lista de inimigos
    private static bool gameRunning = true;
    private static bool playerHit = false;
    private static Random random = new Random();

    private static int difficultyLevel = 1; // Nível inicial de dificuldade
    private static int spawnRate = 10; // Frequência de spawn de inimigos (quanto menor, mais frequente)
    private static int gameSpeed = 100; // Intervalo de atualização do jogo (em ms)

    private static bool isFullScreen = false; // Controle de modo Tela Cheia ou Janela

    // Definição da API do Windows para alternar para tela cheia
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_MAXIMIZE = 3;
    private const int SW_RESTORE = 9;

    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        ShowIntro(); // Exibe a introdução antes do menu principal
        ShowMenu();
    }

    private static void ShowIntro()
    {
        DisplayMessage("DESENVOLVIDO POR VITOR MANOEL", 1500);
        DisplayMessage("VM STUDIO APRESENTA", 1500);
        DisplayMessage("SPACE CRAFT", 2000);
    }

    private static void DisplayMessage(string message, int delay)
    {
        Console.Clear();
        Console.WriteLine(message);
        Thread.Sleep(delay);
        Console.Clear();
    }

    private static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Space Craft ===");
        Console.WriteLine("1. Iniciar Jogo");
        Console.WriteLine("2. Tutorial");
        Console.WriteLine("3. Configurações");
        Console.WriteLine("4. Sair do Jogo");
        Console.WriteLine("Escolha uma opção:");

        var choice = Console.ReadKey(true).Key;

        if (choice == ConsoleKey.D1 || choice == ConsoleKey.NumPad1)
        {
            StartGame();
        }
        else if (choice == ConsoleKey.D2 || choice == ConsoleKey.NumPad2)
        {
            ShowTutorial();
        }
        else if (choice == ConsoleKey.D3 || choice == ConsoleKey.NumPad3)
        {
            ShowSettings();
        }
        else if (choice == ConsoleKey.D4 || choice == ConsoleKey.NumPad4)
        {
            ExitGame();
        }
        else
        {
            Console.WriteLine("Opção inválida. Tente novamente.");
            Thread.Sleep(1000);
            ShowMenu();
        }
    }

    private static void ShowSettings()
    {
        Console.Clear();
        Console.WriteLine("=== Configurações ===");
        Console.WriteLine("1. Resolução");
        Console.WriteLine("2. Voltar ao Menu Principal");
        Console.WriteLine("Escolha uma opção:");

        var choice = Console.ReadKey(true).Key;

        if (choice == ConsoleKey.D1 || choice == ConsoleKey.NumPad1)
        {
            ShowResolutionSettings();
        }
        else if (choice == ConsoleKey.D2 || choice == ConsoleKey.NumPad2)
        {
            ShowMenu();
        }
        else
        {
            Console.WriteLine("Opção inválida. Tente novamente.");
            Thread.Sleep(1000);
            ShowSettings();
        }
    }

    private static void ShowResolutionSettings()
    {
        Console.Clear();
        Console.WriteLine("=== Resolução ===");
        Console.WriteLine("1. Modo Tela Cheia");
        Console.WriteLine("2. Modo Janela");
        Console.WriteLine("Escolha uma opção:");

        var choice = Console.ReadKey(true).Key;

        if (choice == ConsoleKey.D1 || choice == ConsoleKey.NumPad1)
        {
            SetFullScreenMode(true);
        }
        else if (choice == ConsoleKey.D2 || choice == ConsoleKey.NumPad2)
        {
            SetFullScreenMode(false);
        }
        else
        {
            Console.WriteLine("Opção inválida. Tente novamente.");
            Thread.Sleep(1000);
            ShowResolutionSettings();
        }

        Console.WriteLine("Configuração alterada com sucesso. Pressione qualquer tecla para voltar.");
        Console.ReadKey(true);
        ShowSettings();
    }

    private static void SetFullScreenMode(bool fullScreen)
    {
        IntPtr handle = GetConsoleWindow();

        if (fullScreen)
        {
            ShowWindow(handle, SW_MAXIMIZE);
            isFullScreen = true;
        }
        else
        {
            ShowWindow(handle, SW_RESTORE);
            isFullScreen = false;
            // Redefine para tamanho padrão após restaurar
            Console.SetWindowSize(80, 25);
            Console.SetBufferSize(80, 25);
        }
    }

    private static void ShowTutorial()
    {
        Console.Clear();
        Console.WriteLine("=== Tutorial ===");
        Console.WriteLine("Use as setas para controlar a nave:");
        Console.WriteLine("← (Esquerda), → (Direita), ↑ (Cima), ↓ (Baixo)");
        Console.WriteLine("Pressione a Barra de Espaço para atirar.");
        Console.WriteLine("A dificuldade aumenta gradualmente ao longo do tempo.");
        Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
        Console.ReadKey(true);
        ShowMenu();
    }

    private static void StartGame()
    {
        Console.Clear();
        Console.WriteLine("Iniciando o jogo...");
        gameRunning = true;
        playerHit = false;
        difficultyLevel = 1; // Define o nível de dificuldade inicial

        Thread inputThread = new Thread(Input);
        inputThread.Start();

        while (gameRunning)
        {
            IncreaseDifficulty();
            SpawnEnemy();
            Update();
            Draw();

            // Verifica se o jogador foi atingido
            if (playerHit)
            {
                EndGame();
                break;
            }

            Thread.Sleep(gameSpeed); // Controle da velocidade do jogo
        }
    }

    private static void Input()
    {
        while (gameRunning)
        {
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.LeftArrow && playerPosition > 0)
                playerPosition--;
            else if (key == ConsoleKey.RightArrow && playerPosition < Console.WindowWidth - 1)
                playerPosition++;
            else if (key == ConsoleKey.Spacebar)
                bullets.Add(playerPosition); // Adiciona um tiro na posição da nave
            else if (key == ConsoleKey.Escape)
                gameRunning = false;
        }
    }

    private static void SpawnEnemy()
    {
        if (random.Next(spawnRate) < 2) // Chance de spawn de inimigo ajustada pela taxa de spawn
        {
            int enemyPosition = random.Next(Console.WindowWidth);
            enemies.Add(enemyPosition); // Adiciona inimigo no topo da tela
        }
    }

    private static void Update()
    {
        // Atualiza a posição dos tiros
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i] -= Console.WindowWidth; // Move o tiro para cima
            if (bullets[i] < 0) bullets.RemoveAt(i--); // Remove o tiro se sair da tela
        }

        // Verifica se algum tiro acerta um inimigo
        for (int i = 0; i < bullets.Count; i++)
        {
            for (int j = 0; j < enemies.Count; j++)
            {
                if (bullets[i] == enemies[j])
                {
                    bullets.RemoveAt(i--); // Remove o tiro
                    enemies.RemoveAt(j); // Remove o inimigo atingido
                    break;
                }
            }
        }

        // Atualiza a posição dos inimigos
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i] += Console.WindowWidth; // Move o inimigo para baixo

            // Verifica se o inimigo colidiu com o jogador
            if (enemies[i] / Console.WindowWidth == Console.WindowHeight - 1 &&
                enemies[i] % Console.WindowWidth == playerPosition)
            {
                playerHit = true; // Marca o jogador como atingido
                gameRunning = false;
                break;
            }

            if (enemies[i] / Console.WindowWidth >= Console.WindowHeight)
            {
                enemies.RemoveAt(i--); // Remove o inimigo se sair da tela
            }
        }
    }

    private static void Draw()
    {
        Console.Clear();

        // Desenha a nave do jogador
        Console.SetCursorPosition(playerPosition, Console.WindowHeight - 1);
        Console.Write("^");

        // Desenha os tiros
        foreach (var bullet in bullets)
        {
            int bulletX = bullet % Console.WindowWidth;
            int bulletY = bullet / Console.WindowWidth;
            if (bulletY >= 0 && bulletY < Console.WindowHeight)
            {
                Console.SetCursorPosition(bulletX, bulletY);
                Console.Write("|");
            }
        }

        // Desenha os inimigos
        foreach (var enemy in enemies)
        {
            int enemyX = enemy % Console.WindowWidth;
            int enemyY = enemy / Console.WindowWidth;
            if (enemyY >= 0 && enemyY < Console.WindowHeight)
            {
                Console.SetCursorPosition(enemyX, enemyY);
                Console.Write("X");
            }
        }

        // Exibe o nível de dificuldade
        Console.SetCursorPosition(0, 0);
        Console.Write($"Nível de Dificuldade: {difficultyLevel}");
    }

    private static void IncreaseDifficulty()
    {
        if (difficultyLevel < 10 && gameRunning) // Define um limite para a dificuldade
        {
            difficultyLevel++;
            spawnRate = Math.Max(2, spawnRate - 1); // Aumenta a frequência dos inimigos
            gameSpeed = Math.Max(30, gameSpeed - 10); // Acelera o tempo de atualização, até um limite mínimo
        }
    }

    private static void EndGame()
    {
        Console.Clear();
        Console.WriteLine("Você foi atingido! Fim de Jogo.");

        // Exibe a mensagem de "Fim de Jogo" em um bloco de notas
        CreateEndGameNote();

        // Reinicia o sistema após a mensagem de fim de jogo ser exibida
        RestartComputer();
    }

    private static void CreateEndGameNote()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FimDeJogo.txt");
        File.WriteAllText(filePath, "Fim de jogo. Obrigado por jogar!");

        // Abre o bloco de notas com a mensagem de "Fim de Jogo"
        Process.Start("notepad.exe", filePath);
    }

    private static void RestartComputer()
    {
        // Aguardar alguns segundos para permitir que o jogador veja a mensagem
        Thread.Sleep(5000); // Aguarda 5 segundos antes de reiniciar

        // Comando para reiniciar o computador
        Process.Start("shutdown", "/r /t 0");
    }

    private static void ExitGame()
    {
        Console.Clear();
        Console.WriteLine("Saindo do jogo. Obrigado por jogar!");
        Thread.Sleep(1000);
        Environment.Exit(0); // Sai do programa
    }
}