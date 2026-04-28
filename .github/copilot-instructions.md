# Copilot Instructions

## Projektrichtlinien
- For AudioBag sizing, HeightOffset should represent the configurable bottom free space ('Kinn') below the last ListBox entry and should be preserved across manual resizing while entry changes grow the window by item height.
- In the AudioView, playback scrolling should be smooth: initially scroll to the caret position, keep the caret position centered, and then scroll out at the end. Additionally, the Play button should function as a Play/Stop toggle, and waveform selection should support mouse interaction with context editing. 
- In AudioView, when looping back, the caret should jump immediately (no smooth return). 
- In AudioView, seek-to-time should occur on textbox leave and while text changes (not via Apply button). Apply should only commit edited AudioView content back to the origin AudioBag track, and if no valid origin bag exists, create a new AudioBag containing that track.

## Hotkeys
- Implement the following hotkeys for enhanced workflow in AudioView:
  - Backspace / Ctrl+Backspace: Seek shortcuts
  - Space: Play/Pause
  - Ctrl+Space: Stop
  - L: Loop Toggle
  - Ctrl+C / Ctrl+V: Selection workflow
  - Ctrl+N: New empty AudioView