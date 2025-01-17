using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sayohime.Main;
using System.Reflection;
using System.Xml;

namespace Sayohime.SceneObjects.Widgets
{
	public class Button : Widget
	{
		public MethodInfo Action { get; set; }
		protected object ActionParameter { get; set; }

		protected NinePatch buttonFrame;
		protected string style;
		protected string pushedStyle;
		protected string disabledStyle;
		protected bool clicking;
		protected GameSound Sound { get; set; } = GameSound.Cursor;



		public Button(Widget iParent, float widgetDepth)
			: base(iParent, widgetDepth)
		{

		}

		public override void LoadAttributes(XmlNode xmlNode)
		{
			base.LoadAttributes(xmlNode);

			UpdateFrame();
		}

		protected virtual void UpdateFrame()
		{
			if (buttonFrame == null) buttonFrame = new NinePatch(style, Depth);

			if (!Enabled) buttonFrame.SetSprite(disabledStyle);
			else if (clicking && !string.IsNullOrEmpty(pushedStyle)) buttonFrame.SetSprite(pushedStyle);
			else if (!string.IsNullOrEmpty(style)) buttonFrame.SetSprite(style);
		}

		public override void ApplyAlignment()
		{
			base.ApplyAlignment();

			if (buttonFrame != null) buttonFrame.Bounds = currentWindow;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			Color drawColor = (Enabled) ? Color : new Color(120, 120, 120, 255);
			if (buttonFrame != null) buttonFrame.FrameColor = drawColor;



			buttonFrame?.Draw(spriteBatch, Position);
		}

		public override Widget GetWidgetAt(Vector2 mousePosition)
		{
			if (!currentWindow.Contains(mousePosition - Position)) return null;

			return this;
		}

		public override void StartLeftClick(Vector2 mousePosition)
		{
			if (!Enabled || !parent.Enabled) return;

			//Audio.PlaySound(GameSound.a_mainmenuselection);

			clicking = true;
			UpdateFrame();
		}

		public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
		{
			if (!Enabled || !parent.Enabled) return;

			clicking = false;

			if (otherWidget == this)
			{
				Activate();
				EndMouseOver();
			}
			else UpdateFrame();
		}

		public void Activate()
		{
			if (!Enabled)
			{
				Audio.PlaySound(GameSound.Back);
				return;
			}

			Audio.PlaySound(Sound);

			object[] parameters = (ActionParameter == null) ? null : new object[] { ActionParameter };

			Action?.Invoke(GetParent<ViewModel>(), parameters);
		}

		public override void StartRightClick(Vector2 mousePosition)
		{
			base.StartRightClick(mousePosition);

			UpdateFrame();
		}

		public override void StartMouseOver()
		{
			if (!Enabled || !parent.Enabled) return;

			base.StartMouseOver();

			UpdateFrame();
		}

		public override void EndMouseOver()
		{
			if (!Enabled || !parent.Enabled) return;

			base.EndMouseOver();

			if (buttonFrame == null) buttonFrame = new NinePatch(style, Depth);
			if (!string.IsNullOrEmpty(style)) buttonFrame.SetSprite(style);
		}

		public override bool Enabled
		{
			set
			{
				base.Enabled = value;
				UpdateFrame();
			}
		}

		protected string Style
		{
			get => style;
			set
			{
				style = value;
				UpdateFrame();
			}
		}
		protected string PushedStyle { get => pushedStyle; set { pushedStyle = value; UpdateFrame(); } }
		protected string DisabledStyle { get => disabledStyle; set { disabledStyle = value; UpdateFrame(); } }
	}
}
