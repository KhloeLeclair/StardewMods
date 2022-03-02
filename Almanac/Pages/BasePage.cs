using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Leclair.Stardew.Almanac.Menus;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;
using Leclair.Stardew.Common.UI.SimpleLayout;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Almanac.Pages {

	public class BaseState {
		public int FlowScroll;
		public int FlowStep;
	}

	public abstract class BasePage<T> : IAlmanacPage, ITab where T : BaseState, new() {

		// Id
		public string Id { get; }

		public readonly AlmanacMenu Menu;
		public readonly ModEntry Mod;

		// Flow State
		private IEnumerable<IFlowNode> Flow;
		private int FlowStep;
		private int FlowScroll;
		private bool FlowScrollRestore = false;

		// Update State
		protected WorldDate LastDate;

		public bool Active { get; private set; }

		public BasePage(AlmanacMenu menu, ModEntry mod) {
			Id = GetType().Name;
			Menu = menu;
			Mod = mod;
		}

		public BasePage(string id, AlmanacMenu menu, ModEntry mod) {
			Id = id;
			Menu = menu;
			Mod = mod;
		}

		#region Update Logic

		public virtual bool WantDateUpdates => false;

		public virtual void Update() {
			LastDate = new(Menu.Date);
		}

		public void SetFlow(FlowBuilder builder, int step = 1, int scroll = 0) {
			SetFlow(builder.Build(), step, scroll);
		}

		public void SetFlow(IEnumerable<IFlowNode> flow, int step = 1, int scroll = 0) {
			Flow = flow;
			FlowStep = step;

			if (FlowScrollRestore)
				FlowScrollRestore = false;
			else if (scroll >= 0)
				FlowScroll = scroll;

			if (Active)
				Menu.SetFlow(Flow, FlowStep, FlowScroll);
		}

		public object GetState() {
			return SaveState();
		}

		public virtual T SaveState() {
			if (Active)
				FlowScroll = Menu.GetFlowScroll();

			return new T() {
				FlowScroll = FlowScroll,
				FlowStep = FlowStep,
			};
		}

		public void LoadState(object state) {
			if (state is T tstate)
				LoadState(tstate);
		}

		public virtual void LoadState(T state) {
			FlowStep = state.FlowStep;
			FlowScroll = state.FlowScroll;
			FlowScrollRestore = true;

			if (Active)
				Menu.SetFlow(Flow, FlowStep, FlowScroll);
		}

		#endregion

		#region IAlmanacPage

		public virtual PageType Type => typeof(ICalendarPage).IsAssignableFrom(GetType()) ? PageType.Calendar : PageType.Blank;

		public virtual bool IsMagic => false;

		public virtual void Activate() {
			Active = true;
			if (LastDate != Menu.Date && (WantDateUpdates || Menu.Date.Season != LastDate?.Season))
				Update();

			if (Flow != null)
				Menu.SetFlow(Flow, FlowStep, FlowScroll);
		}

		public virtual void Deactivate() {
			Active = false;
			if (Flow != null)
				FlowScroll = Menu.GetFlowScroll();
		}

		public virtual void DateChanged(WorldDate oldDate, WorldDate newDate) {
			if (Active && (WantDateUpdates || newDate.Season != LastDate?.Season))
				Update();
		}

		public virtual void UpdateComponents() {

		}

		private bool IsComponent(Type type) {
			return type == typeof(ClickableComponent) || type.IsSubclassOf(typeof(ClickableComponent));
		}

		public virtual List<ClickableComponent> GetComponents() {
			List<ClickableComponent> result = new();

			Type type = GetType();

			foreach (FieldInfo field in type.GetFields()) {
				if (field.GetCustomAttributes(typeof(SkipForClickableAggregation), true).Length != 0)
					continue;

				Type ftype = field.FieldType;

				if (IsComponent(ftype)) {
					if (field.GetValue(this) is ClickableComponent cmp)
						result.Add(cmp);

				} else if (ftype.IsGenericType && typeof(IEnumerable).IsAssignableFrom(ftype) && IsComponent(ftype.GetGenericArguments()[0])) {
					if (field.GetValue(this) is IEnumerable enumerable)
						foreach (object obj in enumerable) {
							if (obj is ClickableComponent cmp)
								result.Add(cmp);
						}
				}
			}

			return result;
		}

		public virtual bool ReceiveGamePadButton(Buttons b) {
			return false;
		}

		public virtual bool ReceiveKeyPress(Keys key) {
			return false;
		}

		public virtual bool ReceiveScroll(int x, int y, int direction) {
			return false;
		}

		public virtual bool ReceiveLeftClick(int x, int y, bool playSound) {
			return false;
		}

		public virtual bool ReceiveRightClick(int x, int y, bool playSound) {
			return false;
		}

		public virtual void PerformHover(int x, int y) {

		}

		public virtual void Draw(SpriteBatch b) {

		}

		#endregion

		#region ITab

		public virtual bool TabMagic => IsMagic;

		public virtual int SortKey => 50;
		public virtual bool TabVisible => TabTexture != null;
		public virtual string TabSimpleTooltip => null;
		public virtual ISimpleNode TabAdvancedTooltip => null;
		public virtual Texture2D TabTexture => null;
		public virtual Rectangle? TabSource => null;
		public virtual float? TabScale => 3f;

		#endregion

	}
}
