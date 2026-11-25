namespace Jero.Core;

/// <summary>
/// 控制台logo
/// </summary>
public static class ConsoleLogoSetup
{
    public static void AddConsoleLogo(this IServiceCollection services)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(@"
              _           _         _   _ ______ _______
     /\      | |         (_)       | \ | |  ____|__   __|
    /  \   __| |_ __ ___  _ _ __   |  \| | |__     | |
   / /\ \ / _` | '_ ` _ \| | '_ \  | . ` |  __|    | |
  / ____ \ (_| | | | | | | | | | |_| |\  | |____   | |
 /_/    \_\__,_|_| |_| |_|_|_| |_(_)_| \_|______|  |_| ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"让.NET更简单、更通用、更流行！");
    }
}