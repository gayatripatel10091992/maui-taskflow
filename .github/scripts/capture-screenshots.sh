#!/usr/bin/env bash
# Installs the APK on the running emulator and captures light/dark screenshots of TaskFlow.
# Fails if TaskFlow is not the foreground app, so a broken screenshot is never committed.
set -euo pipefail

APP_ID="$1"
APK="$2"
OUT="docs/screenshots"
mkdir -p "$OUT"

adb wait-for-device
until [ "$(adb shell getprop sys.boot_completed | tr -d '\r')" = "1" ]; do sleep 2; done
# Give the launcher time to settle; a cold-booted emulator is slow for the first minute.
sleep 45

dismiss_dialogs() {
  adb shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS >/dev/null 2>&1 || true
}

launch_and_capture() {
  local file="$1"
  adb shell am force-stop "$APP_ID"
  dismiss_dialogs
  adb shell monkey -p "$APP_ID" -c android.intent.category.LAUNCHER 1 >/dev/null
  # Wait until TaskFlow is the resumed activity (up to 90 s).
  for _ in $(seq 1 45); do
    if adb shell dumpsys activity activities | grep -E "topResumedActivity|mResumedActivity" | grep -q "$APP_ID"; then
      break
    fi
    sleep 2
  done
  sleep 8   # let the first frame and data render
  dismiss_dialogs
  sleep 2
  if ! adb shell dumpsys activity activities | grep -E "topResumedActivity|mResumedActivity" | grep -q "$APP_ID"; then
    echo "::error::TaskFlow is not in the foreground; refusing to save $file"
    adb shell dumpsys activity activities | grep -E "topResumedActivity|mResumedActivity" || true
    adb logcat -d -s AndroidRuntime:E monodroid:E DOTNET:E | tail -60 || true
    exit 1
  fi
  adb exec-out screencap -p > "$OUT/$file"
  echo "Saved $OUT/$file"
}

adb install -r "$APK"

# Clean status bar: fixed clock, no notifications.
adb shell settings put global sysui_demo_allowed 1
adb shell am broadcast -a com.android.systemui.demo -e command enter >/dev/null
adb shell am broadcast -a com.android.systemui.demo -e command clock -e hhmm 0930 >/dev/null
adb shell am broadcast -a com.android.systemui.demo -e command notifications -e visible false >/dev/null

adb shell cmd uimode night no
launch_and_capture 01_android_light.png

adb shell cmd uimode night yes
launch_and_capture 02_android_dark.png
adb shell cmd uimode night no

ls -la "$OUT"
