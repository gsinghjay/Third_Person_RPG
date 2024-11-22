using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

public static class GameLogger
{
    // Enable or disable console logging
    public static bool EnableConsoleLogging { get; set; } = true;
    // Enable or disable file logging
    public static bool EnableFileLogging { get; set; } = false; 

    // Log prefixes
    private const string MOVEMENT_PREFIX = "[Movement]";
    private const string ANIMATION_PREFIX = "[Animation]";
    private const string STATE_PREFIX = "[State]";
    private const string COMBAT_PREFIX = "[Combat]";
    private const string CAMERA_PREFIX = "[Camera]";

    private static readonly string LogFilePath;
    private static readonly ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
    private static readonly object LogLock = new object();
    private static bool isProcessingQueue;
    private static readonly StringBuilder LogBuffer = new StringBuilder();
    private static DateTime LastFileWrite = DateTime.Now;
    private const int BUFFER_FLUSH_INTERVAL_MS = 1000; // Flush every second

    static GameLogger()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        
        // Create logs directory if it doesn't exist
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Clean up old log files (keep last 5)
        CleanupOldLogFiles(logDirectory, 5);

        LogFilePath = Path.Combine(logDirectory, $"game_log_{timestamp}.txt");
        Debug.Log($"Logging to file: {LogFilePath}");

        // Write header to log file
        string header = $"=== Game Log Started at {timestamp} ===\n" +
                       $"Game Version: {Application.version}\n" +
                       $"Unity Version: {Application.unityVersion}\n" +
                       $"Platform: {Application.platform}\n" +
                       "=====================================\n\n";
                       
        File.WriteAllText(LogFilePath, header);

        // Start the background processing of the log queue
        ProcessLogQueue();
    }

    private static void CleanupOldLogFiles(string directory, int keepCount)
    {
        try
        {
            var files = new DirectoryInfo(directory).GetFiles("game_log_*.txt")
                                                  .OrderByDescending(f => f.CreationTime)
                                                  .Skip(keepCount);
                                                  
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to delete old log file {file.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to cleanup old log files: {ex.Message}");
        }
    }

    public static void LogMovement(string message, LogType type = LogType.Log)
    {
        Log(MOVEMENT_PREFIX, message, type);
    }

    public static void LogAnimation(string message, LogType type = LogType.Log)
    {
        Log(ANIMATION_PREFIX, message, type);
    }

    public static void LogState(string message, LogType type = LogType.Log)
    {
        Log(STATE_PREFIX, message, type);
    }

    public static void LogCombat(string message, LogType type = LogType.Log)
    {
        Log(COMBAT_PREFIX, message, type);
    }

    public static void LogCamera(string message, LogType type = LogType.Log)
    {
        Log(CAMERA_PREFIX, message, type);
    }

    private static void Log(string prefix, string message, LogType type)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string formattedMessage = $"{timestamp} {prefix} {message}";

        // Only log to console if enabled
        if (EnableConsoleLogging)
        {
            switch (type)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        // Only add to file logging queue if enabled
        if (EnableFileLogging)
        {
            LogQueue.Enqueue($"[{type}] {formattedMessage}");
        }
    }

    private static async void ProcessLogQueue()
    {
        if (isProcessingQueue) return;
        isProcessingQueue = true;

        try
        {
            while (Application.isPlaying)
            {
                while (LogQueue.TryDequeue(out string message))
                {
                    LogBuffer.AppendLine(message);
                }

                if (LogBuffer.Length > 0 && 
                    (DateTime.Now - LastFileWrite).TotalMilliseconds >= BUFFER_FLUSH_INTERVAL_MS)
                {
                    await WriteBufferToFile();
                }

                await Task.Delay(100); // Small delay to prevent excessive CPU usage
            }

            // Final flush when application is quitting
            if (LogBuffer.Length > 0)
            {
                await WriteBufferToFile();
            }
        }
        finally
        {
            isProcessingQueue = false;
        }
    }

    private static async Task WriteBufferToFile()
    {
        try
        {
            string textToWrite = LogBuffer.ToString();
            LogBuffer.Clear();
            
            await File.AppendAllTextAsync(LogFilePath, textToWrite);
            LastFileWrite = DateTime.Now;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
            LogBuffer.Clear(); // Prevent buffer from growing indefinitely if file writing fails
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnApplicationQuit()
    {
        // Ensure all remaining logs are written
        if (LogBuffer.Length > 0)
        {
            File.AppendAllText(LogFilePath, LogBuffer.ToString());
        }
        
        string footer = $"\n=== Game Log Ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n";
        File.AppendAllText(LogFilePath, footer);
    }
}

public enum LogType
{
    Log,
    Warning,
    Error
}