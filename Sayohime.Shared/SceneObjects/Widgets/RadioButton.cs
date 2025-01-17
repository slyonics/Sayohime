using Microsoft.Xna.Framework;
using Sayohime.Main;
using System.Linq;

namespace Sayohime.SceneObjects.Widgets
{
	public class RadioButton : Button
	{
		protected bool selected = false;
		public bool Selected { get => selected; }

		public RadioButton(Widget iParent, float widgetDepth)
			: base(iParent, widgetDepth)
		{

		}

		public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
		{
			if (!Enabled || !parent.Enabled) return;

			clicking = false;

			if (otherWidget == this)
			{
				Audio.PlaySound(Sound);

				if (selected) Activate();
				RadioSelect();
			}
			else
			{
				if (!Selected) buttonFrame?.SetSprite(style);
			}
		}

		public override void EndMouseOver()
		{
			if (!Enabled || !parent.Enabled) return;

			base.EndMouseOver();

			if (buttonFrame == null) buttonFrame = new NinePatch(style, Depth);
			if (selected && !string.IsNullOrEmpty(pushedStyle)) buttonFrame.SetSprite(pushedStyle);
			else if (!string.IsNullOrEmpty(style)) buttonFrame.SetSprite(style);
		}

		protected override void UpdateFrame()
		{
			if (buttonFrame == null) buttonFrame = new NinePatch(style, Depth);

			if (!Enabled) buttonFrame.SetSprite(disabledStyle);
			else if ((clicking || selected) && !string.IsNullOrEmpty(pushedStyle)) buttonFrame.SetSprite(pushedStyle);
			else if (!string.IsNullOrEmpty(style)) buttonFrame.SetSprite(style);
		}

		public void RadioSelect()
		{
			foreach (Widget peer in parent.ChildList)
			{
				RadioButton buttonPeer = peer as RadioButton;
				buttonPeer?.UnSelect();
			}

			var radiobox = parent as RadioBox;
			if (radiobox != null)
			{
				radiobox.Selection = radiobox.ChildList.IndexOf(this);
				if (radiobox.Selection == 0 && !radiobox.IsChildVisible(this))
				{
					radiobox.ScrollOffset = new Vector2(0);
				}
			}

			selected = true;
			buttonFrame?.SetSprite(PushedStyle);
			if (buttonFrame != null)
			{
				buttonFrame.FrameDepth = ((Button)parent.ChildList.Last(x => x is Button)).Depth + Widget.WIDGET_PEER_DEPTH_OFFSET;
			}

			foreach (Widget child in ChildList) child.Depth = buttonFrame.FrameDepth + Widget.WIDGET_DEPTH_OFFSET;
		}

		public void UnSelect()
		{
			//if (!enabled) return;

			selected = false;
			buttonFrame?.SetSprite(Style);

			if (buttonFrame != null)
			{
				buttonFrame.FrameDepth = Depth;
			}

			foreach (Widget child in ChildList) child.Depth = this.Depth + Widget.WIDGET_DEPTH_OFFSET;
		}
	}
}
