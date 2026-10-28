# Arduino Relay Controller

Controls a 4-channel, low-level-trigger relay board from an Arduino Uno. The relays short the boot/jumper pads FLASH_MODE, WATCHDOG, and JTAG_BOOT on a Hydra board. A Windows desktop app (C#, WinForms) sends ON / OFF / Toggle commands over USB serial.

- Firmware: [RelayController.ino](RelayController.ino)
- Desktop app: [RelayControllerApp](RelayControllerApp/)
- Operating instructions: [USER_GUIDE.md](USER_GUIDE.md)

## Version

Current version: **1.0.0**

The app version is set in `<Version>` in [RelayControllerApp.csproj](RelayControllerApp/RelayControllerApp.csproj) and shown in the window title. The firmware version is in the header comment of [RelayController.ino](RelayController.ino). Both are released together and tagged in git as `v1.0.0`.

### Changelog

- **1.0.0**: Initial release. Four relay channels (FLASH_MODE, WATCHDOG, JTAG_BOOT, SPARE_4), ON / OFF / Toggle per channel, live state display, and COM port selection in a C# WinForms app.

## Requirements

- **Arduino Uno (or compatible):** runs the firmware
- **4-channel relay board:** 5 V, low-level trigger (IN pin LOW = relay ON), switches the pads
- **USB cable:** serial link between the PC and the Uno
- **Windows 10/11:** runs the app
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0):** builds the app from source
- **Arduino IDE 2.x:** uploads the firmware

## Wiring

Power:

- VCC: connect to Uno 5V
- GND: connect to Uno GND

Channels:

- IN1: Uno D7, button FLASH_MODE, Hydra pad FLASH_MODE
- IN2: Uno D6, button WATCHDOG, Hydra pad WATCHDOG
- IN3: Uno D5, button JTAG_BOOT, Hydra pad JTAG_BOOT
- IN4: Uno D4, button SPARE_4, no Hydra pad (not used)

For each channel, connect the relay's **COM** to the Hydra board's **GND** and its **NO** (normally open) to the matching pad. When the relay is on, that pad is shorted to GND.

Only work on mains-voltage terminals with power disconnected. If the Hydra board is powered from a supply other than the Uno, make sure the grounds are common.

To use a different pin, change `RELAY_PINS` in the firmware. To add relays on a board with more channels, add pins there and add a matching name to `RelayNames` in [MainForm.cs](RelayControllerApp/MainForm.cs).

## Serial protocol

9600 baud, 8N1, newline-terminated commands.

- `ON n`: reply `STATE n 1`. Turns relay n on.
- `OFF n`: reply `STATE n 0`. Turns relay n off.
- `TOGGLE n`: reply `STATE n 0` or `STATE n 1`. Flips relay n.
- `STATUS`: reply `STATE 1 x` ... `STATE 4 x`. Reports every relay.
- Power-up or reset: reply `READY 4`. Firmware started with 4 relays.
- Bad input: reply `ERR ...`. Unknown command or relay number.

Relays are all off at power-up and after reset.

## Building the firmware

1. Open `RelayController.ino` in the Arduino IDE.
2. Select **Tools > Board > Arduino Uno** and the Uno's COM port.
3. Click **Upload**.
4. Close the Serial Monitor before running the desktop app. Only one program can use the port at a time.

## Building the desktop app

From the `RelayControllerApp` folder:

```powershell
dotnet restore
dotnet build -c Release
```

The executable is written to `RelayControllerApp\bin\Release\net8.0-windows\RelayControllerApp.exe`.

To run from source instead:

```powershell
dotnet run
```

## Source control

The repository is tracked with git. Build output (`bin/`, `obj/`) is excluded by [.gitignore](.gitignore). Releases are tagged `vX.Y.Z`, for example:

```powershell
git tag -a v1.0.0 -m "Version 1.0.0"
```

## Troubleshooting

- **No COM port listed:** check the USB cable and that the Uno driver is installed. Click **Refresh**.
- **"Could not open COMx":** close the Arduino Serial Monitor or any other program using the port.
- **Status stays at "waiting for Uno reset":** click **Disconnect**, then **Connect** again. Confirm the firmware uploaded.
- **Relay clicks but the pad is not shorted:** check COM and NO wiring. Check the relay board's LED for the channel.
- **Buttons do nothing:** confirm the status bar says "Connected". Reconnect if needed.
