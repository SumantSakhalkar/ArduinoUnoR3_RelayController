// Arduino Relay Controller firmware, version 1.0.0
// Arduino Uno -> low-level-trigger relay module (IN pin LOW = relay ON)
// Wiring: Relay board VCC -> 5V, GND -> GND
//   IN1 -> D7 (relay 1 FLASH_MODE)   IN2 -> D6 (relay 2 WATCHDOG)
//   IN3 -> D5 (relay 3 JTAG_BOOT)    IN4 -> D4 (relay 4 spare)
// Each relay's COM -> Hydra GND, NO -> Hydra pad. Relay ON = pad shorted to GND.
// Serial 9600 baud, newline-terminated commands:
//   ON  [n]   OFF [n]   TOGGLE [n]   STATUS
// n = relay number starting at 1 (defaults to 1). Reply: "STATE n 1|0"

const uint8_t RELAY_PINS[] = {7, 6, 5, 4};
const uint8_t NUM_RELAYS = sizeof(RELAY_PINS) / sizeof(RELAY_PINS[0]);
const uint8_t RELAY_ON = LOW;              // low-level trigger
const uint8_t RELAY_OFF = HIGH;

bool state[NUM_RELAYS];
String line;

void setRelay(uint8_t i, bool on) {
  state[i] = on;
  digitalWrite(RELAY_PINS[i], on ? RELAY_ON : RELAY_OFF);
}

void report(uint8_t i) {
  Serial.print(F("STATE "));
  Serial.print(i + 1);
  Serial.print(' ');
  Serial.println(state[i] ? 1 : 0);
}

void handle(String cmd) {
  cmd.trim();
  cmd.toUpperCase();
  if (cmd.length() == 0) return;

  int sp = cmd.indexOf(' ');
  String word = sp < 0 ? cmd : cmd.substring(0, sp);
  int n = sp < 0 ? 1 : cmd.substring(sp + 1).toInt();

  if (word == "STATUS") {
    for (uint8_t i = 0; i < NUM_RELAYS; i++) report(i);
    return;
  }
  if (n < 1 || n > NUM_RELAYS) {
    Serial.println(F("ERR bad relay number"));
    return;
  }
  uint8_t i = n - 1;
  if (word == "ON") setRelay(i, true);
  else if (word == "OFF") setRelay(i, false);
  else if (word == "TOGGLE") setRelay(i, !state[i]);
  else { Serial.println(F("ERR unknown command")); return; }
  report(i);
}

void setup() {
  for (uint8_t i = 0; i < NUM_RELAYS; i++) {
    digitalWrite(RELAY_PINS[i], RELAY_OFF);  // set HIGH before OUTPUT so the relay doesn't click on at boot
    pinMode(RELAY_PINS[i], OUTPUT);
    state[i] = false;
  }
  Serial.begin(9600);
  Serial.print(F("READY "));
  Serial.println(NUM_RELAYS);
}

void loop() {
  while (Serial.available()) {
    char c = Serial.read();
    if (c == '\n' || c == '\r') {
      handle(line);
      line = "";
    } else if (line.length() < 32) {
      line += c;
    }
  }
}
