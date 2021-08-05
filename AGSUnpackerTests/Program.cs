using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AGSUnpackerTests.Extensions;

namespace AGSUnpackerTests
{
  public class TestGame
  {
    public string Name { get; set; }
    public string Version { get; set; }
    public string EngineVersion { get; set; }

    public TestGame(string name, string version, string engineVersion)
    {
      Name = name;
      Version = version;
      EngineVersion = engineVersion;
    }
  }

  public class Program
  {
    public static List<TestGame> BuildTestGamesList(string directory)
    {
      List<TestGame> games = new List<TestGame>();

      Console.WriteLine("Searching for tests in: '{0}'...", directory);
      string[] gameDirectories = Directory.GetDirectories(directory);
      for (int i = 0; i < gameDirectories.Length; ++i)
      {
        string gameDirectoryName = PathExtensions.GetLastDirectoryName(gameDirectories[i]);
        Console.WriteLine("\tParsing game '{0}'...", gameDirectoryName);

        string gameDescriptionFilepath = Path.Combine(gameDirectories[i], "game.txt");
        if (!File.Exists(gameDescriptionFilepath))
        {
          Console.WriteLine("\t\tWARNING: Could not locate 'game.txt' file, skipping game...");
          continue;
        }

        // Read 'game.txt' file
        Console.WriteLine("\tReading 'game.txt' file...");
        string[] gameDescriptions = File.ReadAllLines(gameDescriptionFilepath);

        string name = string.Empty;
        string version = string.Empty;
        string engineVersion = string.Empty;

        for (int j = 0; j < gameDescriptions.Length; ++j)
        {
          string[] pairs = gameDescriptions[j].Split(':');
          if (pairs.Length != 2)
          {
            Console.WriteLine("\t\tWARNING: could not parse line: '{0}', skipping...", gameDescriptions[j]);
            continue;
          }

          string key = pairs[0].Trim().ToLower();
          string value = pairs[1].Trim();

          /*
            game: The Blackwell Legacy
            version: 1.8
            engine: 3.5.0
          */
          switch (key)
          {
            case "game":
              {
                name = value;
              } break;
            case "version":
              {
                version = value;
              } break;
            case "engine":
              {
                engineVersion = value;
              } break;

            default:
              {
                Console.WriteLine("\t\tWARNING: unknown game description: '{0} = {1}', skipping...", key, value);
              } break;
          }
        }

        // Check for duplicate
        if (games.Exists(x => x.Name.Equals(name)))
        {
          Console.WriteLine("\tWARNING: game '{0}' already exists, skipping...", name);
          continue;
        }

        // Check 'game' folder
        Console.WriteLine("\tSearching for game files...");

        const string gameFolder = "game";
        string gameFolderPath = Path.Combine(gameDirectories[i], gameFolder);

        if (!Directory.Exists(gameFolderPath))
        {
          Console.WriteLine("\t\tWARNING: game folder doesn't exist, skipping...");
          continue;
        }

        string[] files = Directory.GetFiles(gameFolderPath);
        if (files.Length <= 0)
        {
          Console.WriteLine("\t\tWARNING: game folder is empty, skipping...");
          continue;
        }

        TestGame testGame = new TestGame(name, version, engineVersion);
        games.Add(testGame);

        Console.WriteLine("Found game {0} {1} (ags v{2})", name, version, engineVersion);
      }

      return games;
    }

    public static void Main(string[] args)
    {
      string testsDirectory = Path.GetFullPath(Environment.CurrentDirectory);

      IList games = BuildTestGamesList(testsDirectory);
    }
  }
}
