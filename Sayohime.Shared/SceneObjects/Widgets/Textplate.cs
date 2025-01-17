using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sayohime.Main;
using Sayohime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sayohime.SceneObjects.Widgets
{
	public class Textplate : Widget
	{
		private string text = "";
		private string Text { get => text; set { text = value; ResizeTextplate(); } }

		private NinePatch textplateFrame;
		private string style;
		private string Style { get => style; set { style = value; UpdateFrame(); } }

		public Textplate(Widget iParent, float widgetDepth)
			: base(iParent, widgetDepth)
		{

		}

		public override void LoadAttributes(XmlNode xmlNode)
		{
			foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
			{
				switch (xmlAttribute.Name)
				{
					default: ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;
				}
			}

			UpdateFrame();
		}

		public void UpdateFrame()
		{
			if (style != null)
			{
				if (textplateFrame == null) textplateFrame = new NinePatch(style, Depth);
				textplateFrame.SetSprite(style);
			}
		}

		public override void ApplyAlignment()
		{
			base.ApplyAlignment();

			string[] textLines = Text.Split('\n');
			string longestLine = textLines.MaxBy(x => Main.Text.GetStringLength(Font, x));

			int width = Main.Text.GetStringLength(Font, longestLine) + InnerMargin.X + InnerMargin.Width + 7; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
			int height = Main.Text.GetStringHeight(Font) * textLines.Count() + InnerMargin.Y + InnerMargin.Height; //Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);

			if (Alignment == Alignment.Center)
			{
				currentWindow.Width = width;
				currentWindow.Height = height;

				base.ApplyAlignment();

				currentWindow.X -= width / 2;
				currentWindow.Y -= (Main.Text.GetStringHeight(Font) + InnerMargin.Y + InnerMargin.Height) / 2 - 1;
				currentWindow.Width = width;
				currentWindow.Height = height;
			}
			else
			{
				currentWindow.Width = width;
				currentWindow.Height = height;
			}

			textplateFrame.Bounds = currentWindow;
		}

		private void ResizeTextplate()
		{
			string[] textLines = Text.Split('\n');
			string longestLine = textLines.MaxBy(x => Main.Text.GetStringLength(Font, x));

			int width = Main.Text.GetStringLength(Font, longestLine) + InnerMargin.X + InnerMargin.Width + 7; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
			int height = Main.Text.GetStringHeight(Font) * textLines.Count() + InnerMargin.Y + InnerMargin.Height; // Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);

			if (Alignment == Alignment.Center)
			{
				currentWindow.Width = width;
				currentWindow.Height = height;

				base.ApplyAlignment();

				currentWindow.X -= width / 2;
				currentWindow.Y -= (Main.Text.GetStringHeight(Font) + InnerMargin.Y + InnerMargin.Height) / 2 - 1;
				currentWindow.Width = width;
				currentWindow.Height = height;
			}
			else
			{
				currentWindow.Width = width;
				currentWindow.Height = height;
			}

			textplateFrame.Bounds = currentWindow;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (String.IsNullOrEmpty(Text)) return;

			base.Draw(spriteBatch);

			textplateFrame.Draw(spriteBatch, Position);
			Main.Text.DrawCenteredText(spriteBatch, new Vector2(InnerBounds.Center.X, InnerBounds.Center.Y - 2) + Position, Font, base.ParseString(Text), Color);

			foreach (Widget widget in ChildList)
			{
				if (widget.Visible)

					widget.Draw(spriteBatch);
			}
		}
	}
}
