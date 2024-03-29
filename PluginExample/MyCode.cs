﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace PluginExample
{
	public class MyCode
	{
		private static HearthstoneTextBlock _info;
		private static int? _player;

		private static Entity[] Entities
		{
			// Get the Game.Entities, you need to clone it to avoid errors
			get { return Helper.DeepClone<Dictionary<int, Entity>>(Core.Game.Entities).Values.ToArray<Entity>(); }
		}

		private static Entity PlayerEntity
		{
			// Get the Entity representing the player, there is also the equivalent for the Opponent
			get { return Entities == null ? null : Entities.First(x => x.IsPlayer); }
		}
        private static Player Opponent
        {
            get { return Core.Game.Opponent; }
        }
        
        private static ObservableCollection<Deck> Decks
        {
            get { return DeckList.Instance.Decks; }
        }
        public static void Load() 
		{
            _player = null;
            //String[] fileNames = Directory.GetFiles("Sample Decks", "*.txt");
            /* TODO: Add automatic deck importing of Sample Decks after newest HDT Update
            foreach (String fileName in fileNames)
            {
                try
                {
                    Deck deck = null;
                    if (fileName.EndsWith(".txt"))
                    {
                        using (var sr = new StreamReader(fileName))
                            deck = Core.MainWindow.ParseCardString(sr.ReadToEnd());
                        
                    }
                    else if (fileName.EndsWith(".xml"))
                    {
                        deck = XmlManager<Deck>.Load(fileName);
                        //not all required information is saved in xml
                        foreach (var card in deck.Cards)
                            card.Load();
                        TagControlEdit.SetSelectedTags(deck.Tags);
                    }
                    SetNewDeck(deck);
                    if (Config.Instance.AutoSaveOnImport)
                        SaveDeckWithOverwriteCheck();
                }
                catch (Exception ex)
                {
                    Logger.WriteLine("Error getting deck from file: \n" + ex, "Import");
                }
            }*/ 
            // A border to put around the text block
            Border blockBorder = new Border();
			blockBorder.BorderBrush = Brushes.Black;
			blockBorder.BorderThickness = new Thickness(1.0);
			blockBorder.Padding = new Thickness(8.0);

			// A text block using the HS font
			_info = new HearthstoneTextBlock();
			_info.Text = "";
			_info.FontSize = 18;

			// Add the text block as a child of the border element
			blockBorder.Child = _info;

			// Create an image at the corner of the text bloxk
			Image image = new Image();
			// Create the image source
			BitmapImage bi = new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/card.png"));
			// Set the image source
			image.Source = bi;

			// Get the HDT Overlay canvas object
			var canvas = Core.OverlayCanvas;
			// Get canvas centre
			var fromTop = canvas.Height / 2;
			var fromLeft = canvas.Width / 2;
			// Give the text block its position within the canvas, roughly in the center
			Canvas.SetTop(blockBorder, fromTop);
			Canvas.SetLeft(blockBorder, fromLeft);
			// Give the text block its position within the canvas
			Canvas.SetTop(image, fromTop - 12);
			Canvas.SetLeft(image, fromLeft - 12);
			// Add the text block and image to the canvas
			canvas.Children.Add(blockBorder);
			canvas.Children.Add(image);

			// Register methods to be called when GameEvents occur
			GameEvents.OnGameStart.Add(NewGame);
			GameEvents.OnPlayerDraw.Add(DeckInfo);
            GameEvents.OnGameEnd.Add(analyzeDeck);
		}



        public static void analyzeDeck()
        {
            DeckList decklist = DeckList.Instance;
            List<Deck> decks = decklist.Decks.Where( 
                d=>d.Name.Length>8 && d.Name.Substring(0, 9).Equals("STANDARD_", StringComparison.OrdinalIgnoreCase)).ToList();
            if (Opponent != null)
            {
                List<Card> revealedCards = Opponent.DisplayRevealedCards.Where(x => !x.IsCreated).ToList();
                double maxScore = -1;
                string bestDeck = null;
                string usedClass = Opponent.Class;
                foreach (Deck d in decks)
                {
                    List<Card> deckCards = d.Cards.ToList();
                    if (d.Class.Equals(usedClass))
                    {
                        double score = getScore(deckCards, revealedCards);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestDeck = d.Name.Substring(9);
                        }
                        Logger.WriteLine(d.Name + " " + score);
                    }
                    
                }
                _info.Text = "Your opponent was playing " + bestDeck;
            }
        }

        public static double getScore(List<Card> deck, List<Card> revealed)
        {
            HashSet<Card> cards = new HashSet<Card>();
            foreach(Card c in deck)
            {
                cards.Add(c);
            }
            foreach(Card c in revealed)
            {
                cards.Add(c);
            }
            Card[] cardList = cards.ToArray();
            double[] vecDeck = new double[cardList.Length];
            double[] vecRevealed = new double[cardList.Length];
            for (int x = 0; x < cardList.Length; x++)
            {
                Card c = cardList[x];
                vecDeck[x] = deck.FindAll(d => d.Name.Equals(c.Name)).Count;
                vecRevealed[x] = revealed.FindAll(d => d.Name.Equals(c.Name)).Count;
            }

            double score = dot(vecDeck,vecRevealed)/magnitude(vecDeck)/magnitude(vecRevealed);
            return score;
        }

        private static double dot(double[] v1, double[] v2)
        {
            double result = 0;
            for (int x = 0; x < v1.Length; x++)
            {
                result += v1[x] * v2[x];
            }
            return result;
        }
        private static double magnitude(double[] v)
        {
            return Math.Sqrt(dot(v, v));
        }

		// Set the player controller id, used to tell who controls a particular
		// entity (card, health etc.)
		private static void NewGame()
		{
            _player = null;
			if (PlayerEntity != null)
				_player = PlayerEntity.GetTag(GAME_TAG.CONTROLLER);		
		}

		// Find all cards in the players hand and write to the text block
		public static void DeckInfo(Card c)
		{
			_info.Text = "";
            List<CardEntity> revealedCards;
            if (_player == null)
				NewGame();
            revealedCards = Opponent.RevealedCards;
            foreach (CardEntity e in revealedCards)
			{
				//_info.Text += e.Entity.Card.Name + "\n";	
			}			
		}

	}
}
