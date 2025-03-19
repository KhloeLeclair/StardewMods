using System;
using System.Collections.Generic;

using HarmonyLib;

using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewModdingAPI;

namespace Leclair.Stardew.BetterGameMenu.Models;

internal sealed class PageOverlayWrapper {

	#region Delegate Types

	internal delegate void ActivationDelegate(object target);
	internal delegate void PageSizeChangedDelegate(object target, Rectangle oldSize, Rectangle newSize);
	internal delegate bool ReadyToCloseDelegate(object target);
	internal delegate void UpdateDelegate(object target, GameTime time, out bool suppressEvent);
	internal delegate void BasicClickDelegate(object target, int x, int y);
	internal delegate void ClickDelegate(object target, int x, int y, bool playSound, out bool suppressEvent);
	internal delegate void ScrollWheelDelegate(object target, int direction, out bool suppressEvent);
	internal delegate void HoverActionDelegate(object target, int x, int y, out bool suppressEvent);
	internal delegate void KeyPressDelegate(object target, Keys key, out bool suppressEvent);
	internal delegate void GamePadDelegate(object target, Buttons button, out bool suppressEvent);
	internal delegate void DrawDelegate(object target, SpriteBatch batch);

	#endregion

	internal readonly ActivationDelegate? OnActivate;
	internal readonly ActivationDelegate? OnDeactivate;
	internal readonly PageSizeChangedDelegate? PageSizeChanged;
	internal readonly ReadyToCloseDelegate? ReadyToClose;
	internal readonly UpdateDelegate? Update;
	internal readonly ClickDelegate? ReceiveLeftClick;
	internal readonly BasicClickDelegate? LeftClickHeld;
	internal readonly BasicClickDelegate? ReleaseLeftClick;
	internal readonly ClickDelegate? ReceiveRightClick;
	internal readonly ScrollWheelDelegate? ReceiveScrollWheelAction;
	internal readonly HoverActionDelegate? PerformHoverAction;
	internal readonly KeyPressDelegate? ReceiveKeyPress;
	internal readonly GamePadDelegate? ReceiveGamePadButton;
	internal readonly GamePadDelegate? GamePadButtonHeld;
	internal readonly DrawDelegate? PreDraw;
	internal readonly DrawDelegate? Draw;

	internal PageOverlayWrapper(Type type) {
		var method = AccessTools.Method(type, nameof(IPageOverlay.OnActivate));
		try {
			OnActivate = method.CreateFlexibleDelegate<ActivationDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.OnActivate), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.OnDeactivate));
		try {
			OnDeactivate = method.CreateFlexibleDelegate<ActivationDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.OnDeactivate), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReadyToClose));
		try {
			ReadyToClose = method.CreateFlexibleDelegate<ReadyToCloseDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReadyToClose), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.Update));
		try {
			Update = method.CreateFlexibleDelegate<UpdateDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.Update), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.PageSizeChanged));
		try {
			PageSizeChanged = method.CreateFlexibleDelegate<PageSizeChangedDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.PageSizeChanged), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReceiveLeftClick));
		try {
			ReceiveLeftClick = method.CreateFlexibleDelegate<ClickDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReceiveLeftClick), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.LeftClickHeld));
		try {
			LeftClickHeld = method.CreateFlexibleDelegate<BasicClickDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.LeftClickHeld), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReleaseLeftClick));
		try {
			ReleaseLeftClick = method.CreateFlexibleDelegate<BasicClickDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReleaseLeftClick), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReceiveRightClick));
		try {
			ReceiveRightClick = method.CreateFlexibleDelegate<ClickDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReceiveRightClick), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReceiveScrollWheelAction));
		try {
			ReceiveScrollWheelAction = method.CreateFlexibleDelegate<ScrollWheelDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReceiveScrollWheelAction), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.PerformHoverAction));
		try {
			PerformHoverAction = method.CreateFlexibleDelegate<HoverActionDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.PerformHoverAction), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReceiveKeyPress));
		try {
			ReceiveKeyPress = method.CreateFlexibleDelegate<KeyPressDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReceiveKeyPress), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.ReceiveGamePadButton));
		try {
			ReceiveGamePadButton = method.CreateFlexibleDelegate<GamePadDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.ReceiveGamePadButton), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.GamePadButtonHeld));
		try {
			GamePadButtonHeld = method.CreateFlexibleDelegate<GamePadDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.GamePadButtonHeld), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.PreDraw));
		try {
			PreDraw = method.CreateFlexibleDelegate<DrawDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.PreDraw), ex);
		}

		method = AccessTools.Method(type, nameof(IPageOverlay.Draw));
		try {
			Draw = method.CreateFlexibleDelegate<DrawDelegate>();
		} catch (Exception ex) {
			throw new InvalidCastException(nameof(IPageOverlay.Draw), ex);
		}
	}

}

internal sealed class WrappedPageOverlay : IPageOverlay {

	internal readonly static Dictionary<Type, PageOverlayWrapper> _Wrappers = [];

	internal PageOverlayWrapper GetOrCreateWrapper(Type type) {
		if (!_Wrappers.TryGetValue(type, out var wrapper)) {
			wrapper = new(type);
			_Wrappers[type] = wrapper;
		}

		return wrapper;
	}

	internal readonly IDisposable Actual;
	internal readonly IModInfo Source;
	internal readonly PageOverlayWrapper Wrapper;

	internal WrappedPageOverlay(IDisposable actual, IModInfo source) {
		Actual = actual;
		Source = source;
		Wrapper = GetOrCreateWrapper(actual.GetType());
	}

	public void Dispose() {
		try {
			Actual.Dispose();
		} catch (Exception ex) {
			ModEntry.Instance.Log($"Error in overlay Dispose from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
		}
	}

	#region Life Cycle and Updates

	public void OnActivate() {
		if (Wrapper.OnActivate is not null)
			try {
				Wrapper.OnActivate(Actual);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay OnActivate from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void OnDeactivate() {
		if (Wrapper.OnDeactivate is not null)
			try {
				Wrapper.OnDeactivate(Actual);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay OnDeactivate from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void PageSizeChanged(Rectangle oldSize, Rectangle newSize) {
		if (Wrapper.PageSizeChanged is not null)
			try {
				Wrapper.PageSizeChanged(Actual, oldSize, newSize);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay PageSizeChanged from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public bool ReadyToClose() {
		if (Wrapper.ReadyToClose is not null)
			try {
				return Wrapper.ReadyToClose(Actual);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReadyToClose from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
		return true;
	}

	public void Update(GameTime time, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.Update is not null)
			try {
				Wrapper.Update(Actual, time, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay Update from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	#endregion

	#region Input

	public void ReceiveLeftClick(int x, int y, bool playSound, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.ReceiveLeftClick is not null)
			try {
				Wrapper.ReceiveLeftClick(Actual, x, y, playSound, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReceiveLeftClick from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void LeftClickHeld(int x, int y) {
		if (Wrapper.LeftClickHeld is not null)
			try {
				Wrapper.LeftClickHeld(Actual, x, y);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay LeftClickHeld from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void ReleaseLeftClick(int x, int y) {
		if (Wrapper.ReleaseLeftClick is not null)
			try {
				Wrapper.ReleaseLeftClick(Actual, x, y);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReleaseLeftClick from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void ReceiveRightClick(int x, int y, bool playSound, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.ReceiveRightClick is not null)
			try {
				Wrapper.ReceiveRightClick(Actual, x, y, playSound, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReceiveRightClick from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void ReceiveScrollWheelAction(int direction, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.ReceiveScrollWheelAction is not null)
			try {
				Wrapper.ReceiveScrollWheelAction(Actual, direction, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReceiveScrollWheelAction from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void PerformHoverAction(int x, int y, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.PerformHoverAction is not null)
			try {
				Wrapper.PerformHoverAction(Actual, x, y, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay PerformHoverAction from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void ReceiveKeyPress(Keys key, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.ReceiveKeyPress is not null)
			try {
				Wrapper.ReceiveKeyPress(Actual, key, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReceiveKeyPress from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void ReceiveGamePadButton(Buttons button, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.ReceiveGamePadButton is not null)
			try {
				Wrapper.ReceiveGamePadButton(Actual, button, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay ReceiveGamePadButton from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void GamePadButtonHeld(Buttons button, out bool suppressEvent) {
		suppressEvent = false;
		if (Wrapper.GamePadButtonHeld is not null)
			try {
				Wrapper.GamePadButtonHeld(Actual, button, out suppressEvent);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay GamePadButtonHeld from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	#endregion

	#region Drawing

	public void PreDraw(SpriteBatch batch) {
		if (Wrapper.PreDraw is not null)
			try {
				Wrapper.PreDraw(Actual, batch);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay PreDraw from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	public void Draw(SpriteBatch batch) {
		if (Wrapper.Draw is not null)
			try {
				Wrapper.Draw(Actual, batch);
			} catch (Exception ex) {
				ModEntry.Instance.Log($"Error in overlay Draw from mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}: {ex}", LogLevel.Error);
			}
	}

	#endregion

}
