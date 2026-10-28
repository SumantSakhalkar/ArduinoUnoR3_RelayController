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

- **State box:** gray `?` = unknown, green `ON`, red `OFF`
- **ON:** turns the relay on (pad shorted to GND)
- **OFF:** turns the relay off (pad released)
- **Toggle:** switches to the opposite state

The status bar at the bottom shows the last reply, for example `Connected - WATCHDOG is ON`.

## 6. Put the Hydra board into a mode

The Hydra board reads these pads at reset. Typical sequence:

1. Click **ON** for the pad you need, for example **FLASH_MODE**.
2. Press reset on the Hydra board.
3. Once the board has read the pad, click **OFF** for that pad.

Check the Hydra board's documentation for the exact order and timing. Only short the pads you need, and turn each one off when you are done.

## 7. Troubleshooting

- **"No serial port selected.":** plug in the Uno, click **Refresh**, and select the port.
- **"Could not open COMx":** close the Arduino Serial Monitor or other app using the port, then click **Connect**.
- **Stuck on "Connecting... (waiting for Uno reset)":** click **Disconnect**, then **Connect**. Confirm the firmware is uploaded.
- **`Write failed: ...`:** the Uno was unplugged or the port is in use. Reconnect and click **Connect**.
- **`Arduino: ERR ...`:** the firmware did not recognize the command. Check that the firmware matches the app version.
- **Relay clicks but the pad does not change:** check the COM and NO wiring and the channel's LED on the relay board.

## 8. Closing the app

Close the window. The app disconnects from the Uno before exiting. The relays keep their last state while the Uno stays powered. Connecting again resets the Uno, which turns all relays off.
