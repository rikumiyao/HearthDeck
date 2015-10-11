﻿using System;
using System.Windows.Controls;

using Hearthstone_Deck_Tracker.Plugins;

namespace PluginExample
{
    public class MyPlugin : IPlugin
    {
		public string Author
		{
			get { return "Riku Miyao"; }
		}

		public string ButtonText
		{
			get { return "Setttings"; }
		}

		public string Description
		{
			get { return "Matches decks"; }
		}

		public MenuItem MenuItem
		{
			get { return null; }
		}

		public string Name
		{
			get { return "DeckMatcher"; }
		}

		public void OnButtonPress()
		{
		}

		public void OnLoad()
		{
			MyCode.Load();
		}

		public void OnUnload()
		{
		}

		public void OnUpdate()
		{
		}

		public Version Version
		{
			get { return new Version(0, 0, 1); }
		}
    }
}
