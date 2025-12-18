using System;
using System.Threading;
using Spectre.Console;

namespace StromPi3ConsoleApp;

internal class Program
{
    static Style mainMenuStyle = new Style(foreground: Color.Yellow, background: Color.Black);
    static Style subMenuStyle = new Style(foreground: Color.Green3, background: Color.Black);

    static string mainMenuTitle = "Strompi3 - Management Console (.NET 8) V 0.6";
    static string subMenuTitle = "Strompi3 - Configuration Management";

    static void Main(string[] args)
    {
        AnsiConsole.Clear();

        bool bContinue = true;

        // endless loop for menu
        while (bContinue)
        {
            ShowMainMenu(ref bContinue);
            Thread.Sleep(500);
            AnsiConsole.Clear();
        }

    }

    private static void ShowMainMenu(ref bool bContinue)
    {

        AnsiConsole.Write(new Markup(mainMenuTitle, mainMenuStyle));
        AnsiConsole.WriteLine();

        int menuNumber = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("Please select an option:")
                .PageSize(10)
                .AddChoices(new[] { 1, 2, 3, 4, 5, 6, 7, 9, 0 })
                .UseConverter(x => x switch
                {
                    1 => "Check Connection of StromPi3",
                    2 => "Get StromPi3 Configuration",
                    3 => "Set StromPi3 Configuration",
                    4 => "Sync RTC of StromPi3 with Raspberry",
                    5 => "Monitor PowerChangedEvent (SERIAL)",
                    6 => "Do SelfCheck with expected Status",
                    7 => "Get Status and Monitor Power Events (SERIAL)",
                    9 => "Shutdown Raspberry PI",
                    0 => "Exit",
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                })
        );

        switch (menuNumber)
        {
            case 1:
                AnsiConsole.MarkupLine("[green]Checking Connection of StromPi3...[/]");
                break;
            case 2:
                AnsiConsole.MarkupLine("[green]Getting StromPi3 Configuration...[/]");
                break;
            case 3:
                AnsiConsole.MarkupLine("[green]Setting StromPi3 Configuration...[/]");
                ShowConfigurationSubMenu();
                break;
            case 4:
                AnsiConsole.MarkupLine("[green]Syncing RTC of StromPi3 with Raspberry...[/]");
                break;
            case 5:
                AnsiConsole.MarkupLine("[green]Monitoring PowerChangedEvent (SERIAL)...[/]");
                break;
            case 6:
                AnsiConsole.MarkupLine("[green]Doing SelfCheck with expected Status...[/]");
                break;
            case 7:
                AnsiConsole.MarkupLine("[green]Getting Status and Monitoring Power Events (SERIAL)...[/]");
                break;
            case 9:
                AnsiConsole.MarkupLine("[green]Shutting down Raspberry PI...[/]");
                // Here you would typically call a method to shutdown the Raspberry Pi
                break;
            case 0:
                AnsiConsole.MarkupLine("[red]Exiting...[/]");
                bContinue = false; // Set the flag to false to exit the loop
                return; // Exit the method to stop further processing
                break;
            default:
                AnsiConsole.MarkupLine("[red]Invalid option selected. Please try again.[/]");
                break;
        }
    }

    public static void ShowConfigurationSubMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Markup(subMenuTitle, subMenuStyle));
        AnsiConsole.WriteLine();

        int menuNumber = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("Please select an option:")
                .AddChoices(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 0 })
                .UseConverter(x => x switch
                {
                    1 => "Edit and Send complete Strompi3 Configuration",
                    2 => "Send Configuration to Strompi3",
                    3 => "TODO: Edit Power Priority",
                    4 => "TODO: Edit Shutdown-Enable, -Timer and Shutdown-battery-level",
                    5 => "TODO: Edit Power Save Mode",
                    6 => "TODO: Edit Alarm-Enable",
                    7 => "TODO: Set Power-ON-Button-Enable and -Timer",
                    8 => "TODO: Set Serialless-Mode ON/OFF",
                    0 => "Exit",
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                })
        );

        switch (menuNumber)
        {
            case 1:
                AnsiConsole.MarkupLine("[green]Editing and Sending complete Strompi3 Configuration...[/]");
                break;
            case 2:
                AnsiConsole.MarkupLine("[green]Sending Configuration to Strompi3...[/]");
                break;
            case 3:
                AnsiConsole.MarkupLine("[green]Editing Power Priority...[/]");
                break;
            case 4:
                AnsiConsole.MarkupLine("[green]Editing Shutdown-Enable, -Timer and Shutdown-battery-level...[/]");
                break;
            case 5:
                AnsiConsole.MarkupLine("[red]Editing Power Save Mode...[/]");
                break;
            case 6:
                AnsiConsole.MarkupLine("[green]Editing Alarm-Enable...[/]");
                break;
            case 7:
                AnsiConsole.MarkupLine("[green]Setting Power-ON-Button-Enable and -Timer...[/]");
                break;
            case 8:
                AnsiConsole.MarkupLine("[red]Setting Serialless-Mode ON/OFF...[/]");
                break;
            case 0:
                AnsiConsole.MarkupLine("[green]Exiting Configuration Submenu...[/]");
                return;
                break;
            default:
                AnsiConsole.MarkupLine("[red]Invalid option selected. Please try again.[/]");
                break;
        }
    }
}
