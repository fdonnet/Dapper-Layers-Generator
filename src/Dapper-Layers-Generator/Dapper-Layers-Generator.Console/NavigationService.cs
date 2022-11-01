using Spectre.Console;

internal class NavigationService
{
    private readonly SettingsConfig _settingsConfig = null!;
    private readonly MainMenu _mainMenu = null!;

    public NavigationService(SettingsConfig settingsConfig, MainMenu mainMenu)
    {
        _settingsConfig = settingsConfig;
        _mainMenu = mainMenu;

        _mainMenu.OnGoToSettingsConfig += GoToSettingsConfAsync;
        _settingsConfig.OnBackToMainMenu += GoToMainMenuAsync;

    }

    internal async Task InitDBAndMainMenuAsync()
    {
        await _mainMenu.InitAsync();
    }

    internal async Task ShowMainMenuAsync()
    {
        await _mainMenu.ShowAsync();
    }

    internal async Task ShowSettingsConfigAsync()
    {
        await _settingsConfig.ShowAsync();
    }

    internal async Task BackToMainMenuAsync()
    {
        await _mainMenu.ReturnToMainMenuAsync();
    }

    internal async void GoToMainMenuAsync()
    {
        await ShowMainMenuAsync();
    }

    internal async void GoToSettingsConfAsync()
    {
        await ShowSettingsConfigAsync();
    }

}

