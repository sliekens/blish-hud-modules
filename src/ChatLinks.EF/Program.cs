using SL.ChatLinks.EF;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
Console.Write("dotnet ef> ");
#pragma warning restore CA1303 // Do not pass literals as localized parameters


string input = Console.ReadLine();
DotnetEF.Run(input);
