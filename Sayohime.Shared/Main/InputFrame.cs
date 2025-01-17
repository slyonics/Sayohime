using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sayohime.Main
{
	public enum Command
	{
		Up,
		Right,
		Down,
		Left,
		Confirm,
		Cancel,
		Interact,
		Menu
    }

	public class InputFrame
	{
		private class CommandState
		{
			public List<Keys> keyBindings;

			public bool down;
			public bool previouslyDown;
			public bool pressed;
			public bool released;
		}

		private const int INPUT_ACTIVITY_HISTORY_LENGTH = 40;

		private static Dictionary<Command, List<Keys>> MANDATORY_KEYBOARD_BINDINGS = new Dictionary<Command, List<Keys>>()
		{
			{ Command.Up, new List<Keys>() { Keys.Up, Keys.W } },
			{ Command.Right, new List<Keys>() { Keys.Right, Keys.D } },
			{ Command.Down, new List<Keys>() { Keys.Down, Keys.S } },
			{ Command.Left, new List<Keys>() { Keys.Left, Keys.A } },
			{ Command.Confirm, new List<Keys>() { Keys.Enter, Keys.Space, Keys.Z } },
			{ Command.Cancel, new List<Keys>() { Keys.Escape, Keys.Back, Keys.X } },
			{ Command.Interact, new List<Keys>() { Keys.Enter, Keys.Space, Keys.Z } },
			{ Command.Menu, new List<Keys>() { Keys.Escape, Keys.Back, Keys.X } }
		}; 

		private List<float> keyActivity = new List<float>();
		private KeyboardState newKeyState;
		private KeyboardState oldKeyState;

		private CommandState[] commandStates = new CommandState[Enum.GetNames(typeof(Command)).Length];

		public void ApplySettings()
		{
			foreach (Command command in Enum.GetValuesAsUnderlyingType(typeof(Command)))
			{
				CommandState commandState = new CommandState();
				commandState.keyBindings = new List<Keys>();

				Keys keyBinding;
				List<Keys> mandatoryKeyList;
				if (Settings.KeyboardBindings.TryGetValue(command, out keyBinding)) commandState.keyBindings.Add(keyBinding);
				if (MANDATORY_KEYBOARD_BINDINGS.TryGetValue(command, out mandatoryKeyList))
				{
					foreach (Keys key in mandatoryKeyList)
					{
						if (!commandState.keyBindings.Contains(key)) commandState.keyBindings.Add(key);
					}
				}

				commandStates[(int)command] = commandState;
			}
		}

		public void Update(GameTime gameTime)
		{
			foreach (CommandState commandState in commandStates)
			{
				commandState.previouslyDown = commandState.down;
				commandState.down = commandState.pressed = commandState.released = false;
			}

			oldKeyState = newKeyState;
			newKeyState = Keyboard.GetState();

			float keyPresses = 0;

			foreach (CommandState commandState in commandStates)
			{
				foreach (Keys key in commandState.keyBindings)
				{
					if (newKeyState.IsKeyDown(key))
					{
						Input.MOUSE_MODE = false;
						commandState.down = true;
						keyPresses++;
						break;
					}
				}

				if (commandState.down && !commandState.previouslyDown) commandState.pressed = true;
				else if (!commandState.down && commandState.previouslyDown) commandState.released = true;
			}

			keyActivity.Add(keyPresses);
			if (keyActivity.Count > INPUT_ACTIVITY_HISTORY_LENGTH) keyActivity.RemoveAt(0);
		}

		public bool CommandDown(Command command)
		{
			return commandStates[(int)command].down;
		}

		public bool CommandPressed(Command command)
		{
			return commandStates[(int)command].pressed;
		}

		public bool CommandReleased(Command command)
		{
			return commandStates[(int)command].released;
		}

		public bool AnythingPressed()
		{
			if (newKeyState.GetPressedKeys().Length > 0) return true;

			if (Input.LeftMouseClicked || Input.RightMouseClicked)
				return true;

			return false;
		}

		public Keys KeyBinding(Command command)
		{
			return commandStates[(int)command].keyBindings[0];
		}

		public Keys GetKey()
		{
			for (Keys key = Keys.D0; key <= Keys.D9; key++)
			{
				if (oldKeyState.IsKeyUp(key) && newKeyState.IsKeyDown(key)) return key;
			}

			for (Keys key = Keys.A; key <= Keys.Z; key++)
			{
				if (oldKeyState.IsKeyUp(key) && newKeyState.IsKeyDown(key)) return key;
			}

			return Keys.None;
		}

		public bool KeyDown(Keys key)
		{
			return newKeyState.IsKeyDown(key);
		}

		public bool KeyPressed(Keys key)
		{
			return newKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
		}

		public bool KeyReleased(Keys key)
		{
			return newKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
		}
	}
}
