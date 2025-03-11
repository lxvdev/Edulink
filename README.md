![Edulink Full Logo Dark Theme](Assets/Logo/EdulinkLogoFullDarkTheme@0.25x.png#gh-dark-mode-only)
![Edulink Full Logo Light Theme](Assets/Logo/EdulinkLogoFullLightTheme@0.25x.png#gh-light-mode-only)

Seamless classroom control at your fingertips

# Download
### Server - [Download v2.0.0.0](https://github.com/lxvdev/Edulink/releases/tag/v2.0.0.0-server)
### Client - [Download v2.0.0.0](https://github.com/lxvdev/Edulink/releases/tag/v2.0.0.0-client)

The versions of the client and the server aren't the same, so make sure to download the correct version for each one.

# Important
If a student tries to kill the Edulink process you can press <kbd> ![Windows Key Light](https://upload.wikimedia.org/wikipedia/commons/6/6a/Windows_logo_-_2021_%28White%29.svg#gh-dark-mode-only) ![Windows Key Dark](https://upload.wikimedia.org/wikipedia/commons/c/c4/Windows_logo_-_2021_%28Black%29.svg#gh-light-mode-only)</kbd> + <kbd>L</kbd> to lock the screen and then unlock it to restart the application automatically.

You can also search the application in the start menu and start it from there.

# Instructions

<h2 font-size>
    <img align="Top" src="Assets\Logo\Edulink_Server_32px.png" alt="Edulink Server">
    Server
</h2>

No setup is required for the server to start working immediately.

1. **Change Port (Optional):**  
   - Navigate to ```File > Settings``` in the application menu.
   - Adjust the port number as needed.

2. **Disable previews (Optional):**
   - Navigate to ```File > Settings``` in the application menu.
   - Uncheck the box for ```Enable desktop preview``` and save. (A restart of the application is required in order to apply the setting)

<h2>
    <img align="Top" src="Assets\Logo\Edulink_Client_32px.png" alt="Edulink Client">
    Client
</h2>

1. **Initial Configuration:**  
    When launching the client for the first time, you will be prompted to setup Edulink:
    - **Computer Name:** An identifier for the computer that will be shown on the server.
    - **IP Address:** It can be found on the server on ```File > Settings``` at the bottom.
    - **Port:** The port number used for communication (it should match the server's port).
    - **Password:** A password to prevent students from changing the settings.

2. **Accessing Edulink:**
   - After the setup, Edulink will run in the background and can be accessed from the tray icon.
   - Right-click the icon for quick access to features such as settings, the computer name with the connection status, exiting the app or updating.

# Commands
- **Send Link**: Directly open a specified link on student's computers.
- **Send Message**: Communicate with students and receive their feedback.
- **View Desktop**: Shows the student's desktop.
- **Restart Application**: Restart the Edulink application on student computer as needed.
- **Shut down**: Shuts down student's computer.
- **Restart**: Restarts the student's computer.
- **Lock Screen**: Sends the student to the lock screen.
- **Log Off**: Logs the student off.
- **Rename computer**: Changes the computer name remotely.
- **Update**: Updates the client to the latest version.
- **Reset password**: Resets the password for the client.
- **Block/unblock input**: Blocks or unblocks input on the student's computer.

# Server features
- **Excluding/including preview**: Excludes or includes the preview of the student's desktop.
- **Hiding/showing computers**: Hides or shows the computers on the list (passive mode).
- **Notification on disconnection**: Notifies when a student's computer disconnects.

# Client features
- **Password protection**: Prevents students from changing the settings.
- **Tray icon**: Quick access to the client's features.
- **Automatic startup**: Starts the client automatically when you log in.

# System requirements
- **Architecture:** x86 or x64.
- **Operating system:** Windows 7, 8, 10 and 11.

# Language support
- English
- Spanish
- Polish

# Credits
## Translating
- English - [Andrew](https://github.com/lxvdev)
- Spanish - [Andrew](https://github.com/lxvdev)
- Polish - [R3ikou](https://github.com/R3ikou)

## Thanks to
- [Hardcodet NotifyIcon for WPF](https://github.com/hardcodet/wpf-notifyicon) for the tray icon.
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) for JSON serialization.

## Special thanks to
- [WPFLocalizeExtension](https://github.com/XAMLMarkupExtensions/WPFLocalizeExtension) for making my life easier.
- [ResX Manager](https://github.com/dotnet/ResXResourceManager) for helping with translations.
- [Material Design In Xaml](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) for saving this project (literally).

## Screenshots
### Server
![Edulink Server MainWindow](Assets/Screenshots/Edulink_Server_MainScreenConnected.png)
![Edulink Server Send Message](Assets/Screenshots/Edulink_Server_SendMessage.png)
![Edulink Server Disconnected Notification](Assets/Screenshots/Edulink_Server_DisconnectedNotification.png)
![Edulink Server Updater](Assets/Screenshots/Edulink_Server_Updater.png)
![Edulink Server Settings](Assets/Screenshots/Edulink_Server_Settings.png)
![Edulink Server View Desktop](Assets/Screenshots/Edulink_Server_ViewDesktop.png)

### Client
![Edulink Client Setup](Assets/Screenshots/Edulink_Client_Setup.png)
![Edulink Client Message](Assets/Screenshots/Edulink_Client_TrayIcon.png)
![Edulink Client Message](Assets/Screenshots/Edulink_Client_Message.png)