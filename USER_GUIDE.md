# Arduino Relay Controller: User Guide

This guide explains how to set up the hardware and use the desktop app to control the four relays. For build and version details, see [README.md](README.md).

## 1. What you need

- Arduino Uno with the firmware from [RelayController.ino](RelayController.ino) uploaded
- 4-channel relay board (5 V, low-level trigger)
- USB cable from the Uno to the PC
- The Hydra board with its FLASH_MODE, WATCHDOG, and JTAG_BOOT pads
- The desktop app `RelayControllerApp.exe` (see the README for building it)

## 2. Connect the hardware

1. Connect the relay board **VCC** to the Uno's **5V** and **GND** to **GND**.
2. Connect the signal pins:
   - **IN1** to **D7**: FLASH_MODE
   - **IN2** to **D6**: WATCHDOG
   - **IN3** to **D5**: JTAG_BOOT
   - **IN4** to **D4**: SPARE_4 (not used for the Hydra board)
3. For each channel, connect the relay's **COM** to the Hydra board's **GND**.
4. Connect the relay's **NO** to the matching Hydra pad.
5. Plug the Uno into the PC with USB.

When a relay is on, its pad is shorted to GND. When it is off, the pad is left alone.

## 3. Start the app

1. Run `RelayControllerApp.exe`.
2. The window title shows the version, for example `Arduino Relay Controller v1.0.0`.

## 4. Connect to the Uno

1. Open the **Port** drop-down and select the Uno's COM port, for example `COM3`. The app picks the first port it finds. Click **Refresh** if the Uno is not listed.
2. Click **Connect**.
3. The Uno resets when the port opens, which turns all relays off. Wait about 2.5 seconds.
4. The status bar shows `Connected - 4 relay(s)`. Four rows appear: FLASH_MODE, WATCHDOG, JTAG_BOOT, and SPARE_4.

Click **Disconnect** to release the port. The Arduino Serial Monitor and this app cannot use the port at the same time.

## 5. Control a relay

Each row has:

| Control | Action |
|---|---|
| **State box** | Gray `?` when unknown, green `ON`, or red `OFF`. Shows the relay's current state as reported by the Uno. |
| **ON** | Turns the relay on, shorting the pad to GND. |
| **OFF** | Turns the relay off, releasing the pad. |
| **Toggle** | Switches the relay to the opposite state. |

The status bar at the bottom shows the last reply, for example `Connected - WATCHDOG is ON`.

## 6. Put the Hydra board into a mode

The Hydra board reads these pads at reset. Typical sequence:

1. Click **ON** for the pad you need, for example **FLASH_MODE**.
2. Press reset on the Hydra board.
3. Once the board has read the pad, click **OFF** for that pad.

Check the Hydra board's documentation for the exact order and timing. Only short the pads you need, and turn each one off when you are done.

## 7. Troubleshooting

| What you see | What to do |
|---|---|
| "No serial port selected." | Plug in the Uno and click **Refresh**, then select the port. |
| "Could not open COMx" | Close the Arduino Serial Monitor or any other app using the port. Then click **Connect** again. |
| Status stays at "Connecting... (waiting for Uno reset)" | Click **Disconnect**, then **Connect**. Confirm the firmware is uploaded. |
| `Write failed: ...` in the status bar | The Uno was unplugged or the port is in use. Reconnect the Uno and click **Connect**. |
| `Arduino: ERR ...` in the status bar | The firmware did not recognize the command. Check that the firmware version matches the app. |
| Relay clicks but the pad does not change | Check the COM and NO wiring and the relay board's LED for that channel. |

## 8. Closing the app

Close the window. The app disconnects from the Uno before exiting. The relays keep their last state while the Uno stays powered. Connecting again resets the Uno, which turns all relays off.
