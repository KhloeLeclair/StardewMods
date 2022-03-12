using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI {

	public class FlowBuilder {
		private FlowBuilder Parent;
		private NestedNode? Node;
		private List<IFlowNode> Nodes;
		private IFlowNode[] Built;

		public FlowBuilder(FlowBuilder parent = null, NestedNode? node = null) {
			Parent = parent;
			Node = node;
		}

		public int Count {
			get {
				if (Built != null)
					return Built.Length;
				if (Nodes != null)
					return Nodes.Count;
				return 0;
			}
		}

		private void AssertState() {
			if (Built != null) throw new ArgumentException("cannot modify built flow");
			if (Nodes == null) Nodes = new();
		}


		public FlowBuilder Add(IFlowNode node) {
			AssertState();
			Nodes.Add(node);
			return this;
		}

		public FlowBuilder AddRange(FlowBuilder builder) {
			return AddRange(builder.Build());
		}

		public FlowBuilder AddRange(IEnumerable<IFlowNode> nodes) {
			AssertState();
			Nodes.AddRange(nodes);
			return this;
		}

		public FlowBuilder Sprite(
			SpriteInfo sprite,
			float scale,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false,
			float size = 16,
			int frame = -1
		) {
			AssertState();
			Nodes.Add(new SpriteNode(
				sprite,
				scale,
				alignment,
				onClick,
				onHover,
				onRightClick,
				noComponent,
				size,
				frame
			));
			return this;
		}

		public FlowBuilder FormatText(
			string text,
			TextStyle style,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			AssertState();
			Nodes.AddRange(FlowHelper.FormatText(
				text,
				style,
				alignment,
				onClick,
				onHover,
				onRightClick,
				noComponent
			));
			return this;
		}

		public FlowBuilder Text(
			string text,
			TextStyle style,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			AssertState();
			Nodes.Add(new TextNode(
				text,
				style,
				alignment,
				onClick,
				onHover,
				onRightClick,
				noComponent
			));
			return this;
		}

		public FlowBuilder Translate(Translation source, object values, TextStyle? style = null, Alignment? alignment = null) {
			AssertState();
			Nodes.AddRange(FlowHelper.Translate(source, values, style, alignment));
			return this;
		}

		public FlowBuilder FormatText(
			string text,
			Color? color = null,
			bool? prismatic = null,
			SpriteFont font = null,
			bool? fancy = null,
			bool? bold = null,
			bool? shadow = null,
			Color? shadowColor = null,
			bool? strikethrough = null,
			bool? underline = null,
			float? scale = null,
			Alignment? align = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			TextStyle style = new TextStyle(
				color: color,
				prismatic: prismatic,
				font: font,
				fancy: fancy,
				shadow: shadow,
				shadowColor: shadowColor,
				bold: bold,
				strikethrough: strikethrough,
				underline: underline,
				scale: scale
			);

			return FormatText(
				text,
				style,
				align,
				onClick,
				onHover,
				onRightClick,
				noComponent
			);
		}

		public FlowBuilder Text(
			string text,
			Color? color = null,
			bool? prismatic = null,
			SpriteFont font = null,
			bool? fancy = null,
			bool? bold = null,
			bool? shadow = null,
			Color? shadowColor = null,
			bool? strikethrough = null,
			bool? underline = null,
			float? scale = null,
			Alignment? align = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			TextStyle style = new TextStyle(
				color: color,
				prismatic: prismatic,
				font: font,
				fancy: fancy,
				shadow: shadow,
				shadowColor: shadowColor,
				bold: bold,
				strikethrough: strikethrough,
				underline: underline,
				scale: scale
			);

			return Text(
				text,
				style,
				align,
				onClick,
				onHover,
				onRightClick,
				noComponent
			);
		}

		public FlowBuilder Group(
			Alignment? align = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			AssertState();
			NestedNode nested = new NestedNode(
				null,
				align,
				onClick,
				onHover,
				onRightClick,
				noComponent
			);
			Nodes.Add(nested);
			return new FlowBuilder(this, nested);
		}

		protected FlowBuilder ReplaceNode(IFlowNode source, IFlowNode replacement) {
			AssertState();
			int idx = Nodes.IndexOf(source);
			if (idx != -1)
				Nodes[idx] = replacement;
			return this;
		}

		public FlowBuilder EndGroup() {
			if (Parent != null && Node.HasValue)
				Parent.ReplaceNode(Node.Value, new NestedNode(BuildThis(), alignment: Node.Value.Alignment));

			return Parent ?? this;
		}

		public IFlowNode[] Build() {
			return Parent?.Build() ?? BuildThis();
		}

		public IFlowNode[] BuildThis() {
			if (Built != null) return Built;
			Built = Nodes?.ToArray() ?? new IFlowNode[0];
			Nodes = null;
			return Built;
		}
	}

	public static class FlowHelper {

		public static Regex I18N_REPLACER = new("{{([ \\w\\.\\-]+)}}");

		public static IFlowNode GetNode(object obj, Alignment? alignment = null, TextStyle? style = null, bool format = false, IModHelper helper = null) {
			if (obj == null)
				return null;
			if (obj is IFlowNode node)
				return node;
			if (obj is IFlowNode[] nodes)
				return new NestedNode(nodes, alignment: alignment);
			if (obj is IEnumerable<IFlowNode> nlist)
				return new NestedNode(nlist.ToArray(), alignment: alignment);
			if (obj is Tuple<int>) {
				return new DividerNode(null);
			}
			if (obj is Texture2D tex)
				return new SpriteNode(
					new SpriteInfo(tex, tex.Bounds),
					2f,
					alignment: alignment
				);
			if (obj is Tuple<Texture2D, float> twople)
				return new SpriteNode(
					new SpriteInfo(twople.Item1, twople.Item1.Bounds),
					twople.Item2,
					alignment: alignment
				);
			if (obj is Tuple<Texture2D, Rectangle> tuple)
				return new SpriteNode(
					new SpriteInfo(tuple.Item1, tuple.Item2),
					2f,
					alignment: alignment
				);
			if (obj is Tuple<Texture2D, Rectangle, float> triple)
				return new SpriteNode(
					new SpriteInfo(triple.Item1, triple.Item2),
					triple.Item3,
					alignment: alignment
				);
			if (obj is SpriteInfo sprite)
				return new SpriteNode(sprite, 2f, alignment: alignment);
			if (obj is Tuple<SpriteInfo, float> spriple)
				return new SpriteNode(spriple.Item1, spriple.Item2, alignment: alignment);
			if (obj is Item item && helper != null)
				return new SpriteNode(
					SpriteHelper.GetSprite(item),
					2f,
					alignment: alignment
				);
			if (obj is Tuple<Item, float> ituple && helper != null)
				return new SpriteNode(
					SpriteHelper.GetSprite(ituple.Item1),
					ituple.Item2,
					alignment: alignment
				);

			if (format) {
				IFlowNode[] nods = FormatText(obj.ToString(), style: style, alignment: alignment)?.ToArray();
				if (nods == null || nods.Length == 0)
					return null;
				if (nods.Length == 1)
					return nods[0];
				return new NestedNode(nods, alignment: alignment);
			}

			return new TextNode(obj.ToString(), style: style, alignment: alignment);
		}

		public static List<IFlowNode> GetNodes(object[] objs, Alignment? alignment = null, TextStyle? style = null, bool format = false, IModHelper helper = null) {
			List<IFlowNode> result = new();

			foreach(object obj in objs) {
				IFlowNode node = GetNode(obj, alignment, style, format, helper);
				if (node is NestedNode nn)
					result.AddRange(nn.Nodes);
				else if (node != null)
					result.Add(node);
			}

			return result;
		}

		private static string ReadSubString(string text, int i, out int end) {
			char chr = text[i];
			if (chr != '{') {
				end = i;
				return null;
			}

			i++;
			int start = i;

			while (i < text.Length && text[i] != '}')
				i++;

			end = i + 1;
			return text[start..i];
		}

		private static Color? ParseColor(string input) {
			if (string.IsNullOrEmpty(input))
				return null;

			System.Drawing.Color color;
			try {
				color = System.Drawing.ColorTranslator.FromHtml(input);
			} catch(Exception) {
				return null;
			}

			return new Color(color.R, color.G, color.B, color.A);
		}

		public static IEnumerable<IFlowNode> FormatText(
			string text,
			TextStyle? style = null,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			return FormatText(
				text: text,
				out TextStyle _,
				out Alignment __,
				style: style,
				alignment: alignment,
				onClick: onClick,
				onHover: onHover,
				onRightClick: onRightClick,
				noComponent: noComponent
			);
		}

		public static IEnumerable<IFlowNode> FormatText(
			string text,
			out TextStyle endStyle,
			out Alignment endAlignment,
			TextStyle? style = null,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			if (string.IsNullOrEmpty(text) || !text.Contains('@')) {
				endStyle = style ?? TextStyle.EMPTY;
				endAlignment = alignment ?? Alignment.None;

				return new IFlowNode[]{
					new TextNode(text, style, alignment, onClick, onHover, onRightClick, noComponent)
				};
			}

			List<IFlowNode> nodes = new();

			TextStyle s = style ?? TextStyle.EMPTY;
			Alignment a = alignment ?? Alignment.None;

			StringBuilder builder = new();

			int i = 0;

			while (i < text.Length) {
				char chr = text[i++];
				if (chr != '@' || i == text.Length) {
					builder.Append(chr);
					continue;
				}

				char next = text[i++];
				TextStyle ns = s;
				Alignment na = a;

				Color? color;
				int ni;

				switch (next) {
					// Alignment
					case '<':
						if (na.HasFlag(Alignment.Left))
							continue;
						na = na.With(Alignment.Left);
						break;

					case '|':
						if (na.HasFlag(Alignment.Center))
							continue;
						na = na.With(Alignment.Center);
						break;

					case '>':
						if (na.HasFlag(Alignment.Right))
							continue;
						na = na.With(Alignment.Right);
						break;

					case '^':
						if (na.HasFlag(Alignment.Top))
							continue;
						na = na.With(Alignment.Top);
						break;

					case '-':
						if (na.HasFlag(Alignment.Middle))
							continue;
						na = na.With(Alignment.Middle);
						break;

					case 'v':
						if (na.HasFlag(Alignment.Bottom))
							continue;
						na = na.With(Alignment.Bottom);
						break;

					// Style
					case '_':
						string size = ReadSubString(text, i, out ni);
						i = ni;
						float? scale = null;
						if (float.TryParse(size, out float sp))
							scale = sp;
						if (ns.Scale == scale)
							continue;
						ns = new(ns, font: ns.Font, color: ns.Color, backgroundColor: ns.BackgroundColor, shadowColor: ns.ShadowColor, scale: scale);
						break;

					case 'b':
						if (!ns.IsBold())
							continue;
						ns = new(ns, bold: false);
						break;

					case 'B':
						if (ns.IsBold())
							continue;
						ns = new(ns, bold: true);
						break;

					case 'c':
						color = ParseColor(ReadSubString(text, i, out ni));
						i = ni;
						if (ns.ShadowColor == color)
							continue;
						ns = new(ns, font: ns.Font, color: ns.Color, backgroundColor: ns.BackgroundColor, shadowColor: color, scale: ns.Scale);
						break;

					case 'C':
						color = ParseColor(ReadSubString(text, i, out ni));
						i = ni;
						if (ns.Color == color)
							continue;
						ns = new(ns, font: ns.Font, color: color, backgroundColor: ns.BackgroundColor, shadowColor: ns.ShadowColor, scale: ns.Scale);
						break;

					case 'f':
						if (!ns.IsFancy())
							continue;
						ns = new(ns, fancy: false);
						break;

					case 'F':
						if (ns.IsFancy())
							continue;
						ns = new(ns, fancy: true);
						break;

					case 'h':
						if (!ns.HasShadow())
							continue;
						ns = new(ns, shadow: false);
						break;

					case 'H':
						if (ns.HasShadow())
							continue;
						ns = new(ns, shadow: true);
						break;

					case 'i':
						if (!ns.IsInverted())
							continue;
						ns = new(ns, invert: false);
						break;

					case 'I':
						if (ns.IsInverted())
							continue;
						ns = new(ns, invert: true);
						break;

					case 'j':
						if (!ns.IsJunimo())
							continue;
						ns = new(ns, junimo: false);
						break;

					case 'J':
						if (ns.IsJunimo())
							continue;
						ns = new(ns, junimo: true);
						break;

					case 'p':
						if (!ns.IsPrismatic())
							continue;
						ns = new(ns, prismatic: false);
						break;

					case 'P':
						if (ns.IsPrismatic())
							continue;
						ns = new(ns, prismatic: true);
						break;

					case 'r':
					case 'R':
						color = ParseColor(ReadSubString(text, i, out ni));
						i = ni;
						if (ns.BackgroundColor == color)
							continue;
						ns = new(ns, font: ns.Font, color: ns.Color, backgroundColor: color, shadowColor: ns.ShadowColor, scale: ns.Scale);
						break;

					case 's':
						if (!ns.IsStrikethrough())
							continue;
						ns = new(ns, strikethrough: false);
						break;

					case 'S':
						if (ns.IsStrikethrough())
							continue;
						ns = new(ns, strikethrough: true);
						break;

					case 't':
					case 'T':
						string name = ReadSubString(text, i, out ni);
						i = ni;
						SpriteFont font = null;
						if (!string.IsNullOrEmpty(name)) {
							if (name.StartsWith("dialog", StringComparison.InvariantCultureIgnoreCase))
								font = Game1.dialogueFont;
							else if (name.Equals("small", StringComparison.InvariantCultureIgnoreCase))
								font = Game1.smallFont;
							else if (name.Equals("tiny", StringComparison.InvariantCultureIgnoreCase))
								font = Game1.tinyFont;
						}
						if (ns.Font == font)
							continue;
						ns = new(ns, font: font, color: ns.Color, backgroundColor: ns.BackgroundColor, shadowColor: ns.ShadowColor, scale: ns.Scale);
						break;

					case 'u':
						if (!ns.IsUnderline())
							continue;
						ns = new(ns, underline: false);
						break;

					case 'U':
						if (ns.IsUnderline())
							continue;
						ns = new(ns, underline: true);
						break;

					default:
						builder.Append(chr);
						continue;
				}

				if (builder.Length > 0) {
					nodes.Add(
						new TextNode(
							builder.ToString(),
							style: s,
							alignment: a,
							onClick: onClick,
							onHover: onHover,
							onRightClick: onRightClick,
							noComponent: noComponent
						)
					);

					builder.Clear();
				}

				s = ns;
				a = na;
			}

			if (builder.Length > 0)
				nodes.Add(
					new TextNode(
						builder.ToString(),
						style: s,
						alignment: a,
						onClick: onClick,
						onHover: onHover,
						onRightClick: onRightClick,
						noComponent: noComponent
					)
				);

			endStyle = s;
			endAlignment = a;

			return nodes;
		}


		public static IEnumerable<IFlowNode> Translate(
			Translation source,
			object values,
			TextStyle? style = null,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			IDictionary<string, object> vals = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (values is IDictionary dictionary) {
				foreach (DictionaryEntry entry in dictionary) {
					string key = entry.Key?.ToString().Trim();
					if (key != null)
						vals[key] = entry.Value;
				}
			} else {
				Type type = values.GetType();
				foreach (PropertyInfo prop in type.GetProperties())
					vals[prop.Name] = prop.GetValue(values);
				foreach (FieldInfo field in type.GetFields())
					vals[field.Name] = field.GetValue(values);
			}

			return Translate(
				source,
				vals,
				style,
				alignment,
				onClick,
				onHover,
				onRightClick,
				noComponent
			);
		}

		public static IEnumerable<IFlowNode> Translate(
			Translation source,
			IDictionary<string, object> values,
			TextStyle? style = null,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false
		) {
			string val = source.ToString();
			if (!source.HasValue())
				return new IFlowNode[] {
					new TextNode(val, style, alignment, onClick, onHover, onRightClick, noComponent)
				};

			string[] bits = I18N_REPLACER.Split(val);
			List<IFlowNode> nodes = new();

			bool replacement = false;

			TextStyle s = style ?? TextStyle.EMPTY;
			Alignment a = alignment ?? Alignment.None;

			foreach (string bit in bits) {
				if (replacement && !string.IsNullOrWhiteSpace(bit)) {
					if (values.TryGetValue(bit, out object node)) {
						if (node is NestedNode nested)
							nodes.AddRange(nested.Nodes);
						else if (node is IFlowNode n)
							nodes.Add(n);
						else if (node is IEnumerable<IFlowNode> nlist)
							nodes.AddRange(nlist);
						else {
							var nd = GetNode(node, a, s);
							if (nd != null)
								nodes.Add(nd);
						}
					}

				} else if (!string.IsNullOrEmpty(bit))
					nodes.AddRange(FormatText(
						text: bit,
						out s,
						out a,
						style: s,
						alignment: a,
						onClick: onClick,
						onHover: onHover,
						onRightClick: onRightClick,
						noComponent: noComponent
					));

				replacement = !replacement;
			}

			return nodes;
		}


		public static FlowBuilder Builder() {
			return new FlowBuilder();
		}

		private static float GetYOffset(Alignment alignment, float height, float containerHeight) {
			if (alignment.HasFlag(Alignment.Bottom))
				return (float) Math.Floor(containerHeight - height);
			if (alignment.HasFlag(Alignment.Middle))
				return (float) Math.Floor((containerHeight - height) / 2f);

			return 0;
		}

		private static float GetXOffset(Alignment alignment, IFlowNodeSlice slice, CachedFlowLine line, float scale, float pos, float maxWidth) {
			bool found = false;
			float remaining = 0;

			foreach(var s in line.Slices) {
				if (slice == s)
					found = true;

				if (found)
					remaining += s.Width * scale;
			}

			float before = maxWidth - remaining - pos;

			if (alignment.HasFlag(Alignment.Right))
				return (float) Math.Floor(before);

			if (alignment.HasFlag(Alignment.Center))
				return (float) Math.Floor(before / 2f);

			return 0;
		}

		private static SpriteFont GetDefaultFont() {
			return Game1.smallFont;
		}

		public static CachedFlow CalculateFlow(IEnumerable<IFlowNode> nodes, float maxWidth = -1, SpriteFont defaultFont = null) {
			SpriteFont font = defaultFont ?? GetDefaultFont();
			List<CachedFlowLine> lines = new();

			// Space Dimensions
			// float spaceWidth = font.MeasureString("A B").X - font.MeasureString("AB").X;

			// Boundary
			float bound = maxWidth < 0 ? float.PositiveInfinity : maxWidth;

			// Overall Dimensions
			float width = 0;
			float height = 0;

			// Current Line
			float lineWidth = 0;
			float lineHeight = 0;
			List<IFlowNodeSlice> cached = new();

			bool forceNew = false;

			foreach (IFlowNode node in nodes) {
				// Make sure the segment has content, or skip it.
				if (node == null || node.IsEmpty())
					continue;

				IFlowNodeSlice last = null;

				while (true) {
					IFlowNodeSlice slice = node.Slice(last, font, bound, bound - lineWidth);
					if (slice == null)
						break;

					last = slice;
					WrapMode mode = slice.ForceWrap;

					// Do we need to do a line wrap?
					if (mode.HasFlag(WrapMode.ForceBefore) || forceNew || (cached.Count > 0 && lineWidth + slice.Width >= bound)) {
						lines.Add(new CachedFlowLine(cached.ToArray(), width: lineWidth, height: lineHeight));
						width = Math.Max(lineWidth, width);
						height += lineHeight;
						lineWidth = lineHeight = 0;
						cached.Clear();
					}

					forceNew = mode.HasFlag(WrapMode.ForceAfter);
					cached.Add(slice);
					lineWidth += (float) Math.Ceiling(slice.Width);
					lineHeight = Math.Max((float) Math.Ceiling(slice.Height), lineHeight);
				}
			}

			// Add the remaining line to the output.
			if (cached.Count > 0) {
				lines.Add(new CachedFlowLine(cached.ToArray(), width: lineWidth, height: lineHeight));
				width = Math.Max(lineWidth, width);
				height += lineHeight;
			}

			return new CachedFlow(
				nodes: nodes.ToArray(),
				lines: lines.ToArray(),
				height: height,
				width: width,
				font: font,
				maxWidth: maxWidth
			);
		}

		public static CachedFlow CalculateFlow(CachedFlow flow, float maxWidth = -1, SpriteFont defaultFont = null) {
			defaultFont ??= GetDefaultFont();
			if (flow.IsCached(defaultFont, maxWidth))
				return flow;

			return CalculateFlow(flow.Nodes, maxWidth, defaultFont);
		}

		public static IFlowNodeSlice GetSliceAtPoint(CachedFlow flow, int x, int y, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			return GetSliceAtPoint(
				flow: flow,
				x: x,
				y: y,
				relativeX: out _,
				relativeY: out _,
				scale: scale,
				lineOffset: lineOffset,
				maxHeight: maxHeight
			);
		}

		public static IFlowNodeSlice GetSliceAtPoint(CachedFlow flow, int x, int y, out int relativeX, out int relativeY, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			float startX = 0;
			float startY = 0;

			foreach (CachedFlowLine line in flow.Lines) {
				if (lineOffset > 0) {
					lineOffset--;
					continue;
				}

				float lHeight = line.Height * scale;
				if (maxHeight >= 0 && startY + lHeight > maxHeight)
					break;

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice?.IsEmpty() ?? true)
						continue;

					IFlowNode node = slice.Node;

					float offsetY = GetYOffset(node.Alignment, slice.Height * scale, line.Height * scale);
					float offsetX = GetXOffset(node.Alignment, slice, line, scale, startX, Math.Max(flow.Width * scale, flow.MaxWidth));

					float sX = startX + offsetX;
					float endX = sX + slice.Width * scale;
					float sY = startY + offsetY;
					float endY = sY + slice.Height * scale;

					if (x >= sX && x <= endX && y >= sY && y <= endY) {
						relativeX = x - (int) sX;
						relativeY = y - (int) sY;

						return slice;
					}

					startX = (float) Math.Ceiling(endX);
					if (x < startX)
						break;
				}

				startX = 0;
				startY += (float) Math.Ceiling(lHeight);

				if (y < startY)
					break;
			}

			relativeX = -1;
			relativeY = -1;

			return null;
		}

		public static IFlowNodeSlice GetSliceAtPoint(CachedFlow flow, int x, int y, float scale = 1, float scrollOffset = 0, float maxHeight = -1) {
			return GetSliceAtPoint(
				flow: flow,
				x: x,
				y: y,
				relativeX: out _,
				relativeY: out _,
				scale: scale,
				scrollOffset: scrollOffset,
				maxHeight: maxHeight
			);
		}

		public static IFlowNodeSlice GetSliceAtPoint(CachedFlow flow, int x, int y, out int relativeX, out int relativeY, float scale = 1, float scrollOffset = 0, float maxHeight = -1) {
			float startX = 0;
			float startY = 0;

			foreach (CachedFlowLine line in flow.Lines) {
				float lHeight = line.Height * scale;

				if (lHeight < scrollOffset) {
					scrollOffset -= lHeight;
					continue;
				} else if (scrollOffset != 0) {
					startY -= scrollOffset;
					scrollOffset = 0;
				}

				if (maxHeight >= 0 && startY >= maxHeight)
					break;

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice?.IsEmpty() ?? true)
						continue;

					IFlowNode node = slice.Node;

					float offsetY = GetYOffset(node.Alignment, slice.Height * scale, line.Height * scale);
					float offsetX = GetXOffset(node.Alignment, slice, line, scale, startX, Math.Max(flow.Width * scale, flow.MaxWidth));

					float sX = startX + offsetX;
					float endX = sX + slice.Width * scale;
					float sY = startY + offsetY;
					float endY = sY + slice.Height * scale;

					if (x >= sX && x <= endX && y >= sY && y <= endY) {
						relativeX = x - (int) sX;
						relativeY = y - (int) sY;

						return slice;
					}

					startX = (float) Math.Ceiling(endX);
					if (x < startX)
						break;
				}

				startX = 0;
				startY += (float) Math.Ceiling(lHeight);

				if (y < startY)
					break;
			}

			relativeX = -1;
			relativeY = -1;

			return null;
		}


		public static int GetMaximumScrollOffset(CachedFlow flow, int height, int step = 1) {
			float remaining = height;

			for (int i = flow.Lines.Length - 1; i >= 0; i--) {
				remaining -= flow.Lines[i].Height;
				if (remaining < 0) {
					i++;
					return i + (i % step);
				}
			}

			return 0;
		}

		// Smooth Scroll

		public static bool ClickFlow(CachedFlow flow, int x, int y, float scale = 1, float scrollOffset = 0f, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, scrollOffset, maxHeight);
			return slice?.Node.OnClick?.Invoke(slice, x, y) ?? false;
		}

		public static bool HoverFlow(CachedFlow flow, int x, int y, float scale = 1, float scrollOffset = 0f, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, scrollOffset, maxHeight);
			return slice?.Node.OnHover?.Invoke(slice, x, y) ?? false;
		}

		public static bool RightClickFlow(CachedFlow flow, int x, int y, float scale = 1, float scrollOffset = 0f, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, scrollOffset, maxHeight);
			return slice?.Node.OnRightClick?.Invoke(slice, x, y) ?? false;
		}

		public static void UpdateComponentsForFlow(CachedFlow flow, List<ClickableComponent> components, int offsetX, int offsetY, float scale = 1, float scrollOffset = 0f, float maxHeight = -1, Action<ClickableComponent> onCreate = null, Action<ClickableComponent> onDestroy = null, int startID = 99000) {
			float x = 0;
			float y = 0;

			int i = -1;

			bool done = false;

			foreach (CachedFlowLine line in flow.Lines) {
				float lHeight = line.Height * scale;

				if (lHeight < scrollOffset) { 
					foreach (IFlowNodeSlice slice in line.Slices) {
						if (slice == null || slice.IsEmpty())
							continue;

						IFlowNode node = slice.Node;
						ClickableComponent cmp = node.UseComponent;
						if (cmp != null)
							cmp.visible = false;
					}

					scrollOffset -= (float) Math.Ceiling(lHeight);
					continue;

				} else if (scrollOffset != 0) {
					y -= scrollOffset;
					scrollOffset = 0;
				}

				if (maxHeight >= 0 && y >= maxHeight) {
					done = true;
					break;
				}

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice == null || slice.IsEmpty())
						continue;

					IFlowNode node = slice.Node;
					if (done) {
						ClickableComponent cmp = node.UseComponent;
						if (cmp != null)
							cmp.visible = false;
						continue;
					}

					float sHeight = slice.Height * scale;
					float sWidth = slice.Width * scale;

					float offX = GetXOffset(node.Alignment, slice, line, scale, x, Math.Max(flow.Width * scale, flow.MaxWidth));

					if ((node.OnHover != null || node.OnClick != null || node.OnRightClick != null) && !node.NoComponent) {
						float offY = GetYOffset(node.Alignment, sHeight, lHeight);

						// Get a component.
						ClickableComponent cmp;
						if (node.UseComponent != null) {
							cmp = node.UseComponent;
							cmp.visible = true;
						} else {
							i++;
							if (i >= components.Count) {
								cmp = new(Rectangle.Empty, (string) null) {
									myID = startID + i,
									upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									rightNeighborID = ClickableComponent.SNAP_AUTOMATIC
								};

								components.Add(cmp);
								onCreate?.Invoke(cmp);
							} else
								cmp = components[i];
						}

						int cHeight = (int) sHeight;
						if (maxHeight >= 0 && y + sHeight > maxHeight)
							cHeight -= (int) ((y + sHeight) - maxHeight);

						cmp.bounds = new Rectangle(
							(int) (offsetX + x + offX),
							(int) (offsetY + y + offY),
							(int) sWidth,
							cHeight
						);
					}

					x += offX + (float) Math.Ceiling(sWidth);
				}

				x = 0;
				y += (float) Math.Ceiling(lHeight);
			}

			// Remove excess components.
			while (components.Count > i + 1) {
				ClickableComponent last = components[components.Count - 1];
				onDestroy?.Invoke(last);
				components.Remove(last);
			}
		}

		public static void RenderFlow(SpriteBatch batch, CachedFlow flow, Vector2 position, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, float scrollOffset = 0f, float maxHeight = -1) {
			float x = 0;
			float y = 0;

			foreach (CachedFlowLine line in flow.Lines) {
				float height = line.Height * scale;
				if (height < scrollOffset) {
					scrollOffset -= (float) Math.Ceiling(height);
					continue;
				} else if (scrollOffset != 0) {
					y -= scrollOffset;
					scrollOffset = 0;
				}

				if (maxHeight >= 0 && y >= maxHeight)
					break;

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice == null || slice.IsEmpty())
						continue;

					IFlowNode node = slice.Node;
					float offsetY = GetYOffset(node.Alignment, slice.Height * scale, line.Height * scale);
					float offsetX = GetXOffset(node.Alignment, slice, line, scale, x, Math.Max(flow.Width * scale, flow.MaxWidth));

					node.Draw(
						slice,
						batch,
						new Vector2(
							position.X + x + offsetX,
							position.Y + y + offsetY
						),
						scale,
						flow.Font,
						defaultColor,
						defaultShadowColor,
						line,
						flow
					);

					x += offsetX + (float) Math.Ceiling(slice.Width * scale);
				}

				x = 0;
				y += (float) Math.Ceiling(height);
			}
		}

		public static CachedFlow RenderFlow(SpriteBatch batch, CachedFlow flow, Vector2 position, float maxWidth = -1, SpriteFont defaultFont = null, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, float scrollOffset = 0, float maxHeight = -1) {
			CachedFlow result = CalculateFlow(flow, maxWidth, defaultFont);
			RenderFlow(batch, result, position, defaultColor, defaultShadowColor, scale, scrollOffset, maxHeight);
			return result;
		}

		public static CachedFlow RenderFlow(SpriteBatch batch, IEnumerable<IFlowNode> nodes, Vector2 position, float maxWidth = -1, SpriteFont defaultFont = null, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, float scrollOffset = 0f, float maxHeight = -1) {
			CachedFlow result = CalculateFlow(nodes, maxWidth, defaultFont);
			RenderFlow(batch, result, position, defaultColor, defaultShadowColor, scale, scrollOffset, maxHeight);
			return result;
		}

		// Line Scroll

		public static bool ClickFlow(CachedFlow flow, int x, int y, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, lineOffset, maxHeight);
			return slice?.Node.OnClick?.Invoke(slice, x, y) ?? false;
		}

		public static bool HoverFlow(CachedFlow flow, int x, int y, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, lineOffset, maxHeight);
			return slice?.Node.OnHover?.Invoke(slice, x, y) ?? false;
		}

		public static bool RightClickFlow(CachedFlow flow, int x, int y, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			IFlowNodeSlice slice = GetSliceAtPoint(flow, x, y, scale, lineOffset, maxHeight);
			return slice?.Node.OnRightClick?.Invoke(slice, x, y) ?? false;
		}

		public static void UpdateComponentsForFlow(CachedFlow flow, List<ClickableComponent> components, int offsetX, int offsetY, float scale = 1, int lineOffset = 0, float maxHeight = -1, Action<ClickableComponent> onCreate = null, Action<ClickableComponent> onDestroy = null, int startID = 99000) {
			float x = 0;
			float y = 0;

			int i = -1;

			bool done = false;

			foreach (CachedFlowLine line in flow.Lines) {
				if (lineOffset > 0) {
					foreach(IFlowNodeSlice slice in line.Slices) {
						if (slice == null || slice.IsEmpty())
							continue;

						IFlowNode node = slice.Node;
						ClickableComponent cmp = node.UseComponent;
						if (cmp != null)
							cmp.visible = false;
					}

					lineOffset--;
					continue;
				}

				float lHeight = line.Height * scale;
				if (maxHeight >= 0 && y + lHeight > maxHeight) {
					done = true;
					break;
				}

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice == null || slice.IsEmpty())
						continue;

					IFlowNode node = slice.Node;
					if (done) {
						ClickableComponent cmp = node.UseComponent;
						if (cmp != null)
							cmp.visible = false;
						continue;
					}

					float sHeight = slice.Height * scale;
					float sWidth = slice.Width * scale;

					float offX = GetXOffset(node.Alignment, slice, line, scale, x, Math.Max(flow.Width * scale, flow.MaxWidth));

					if ((node.OnHover != null || node.OnClick != null || node.OnRightClick != null) && !node.NoComponent) {
						float offY = GetYOffset(node.Alignment, sHeight, lHeight);

						// Get a component.
						ClickableComponent cmp;
						if (node.UseComponent != null) {
							cmp = node.UseComponent;
							cmp.visible = true;
						} else {
							i++;
							if (i >= components.Count) {
								cmp = new(Rectangle.Empty, (string) null) {
									myID = startID + i,
									upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
									rightNeighborID = ClickableComponent.SNAP_AUTOMATIC
								};

								components.Add(cmp);
								onCreate?.Invoke(cmp);
							} else
								cmp = components[i];
						}

						cmp.bounds = new Rectangle(
							(int) (offsetX + x + offX),
							(int) (offsetY + y + offY),
							(int) sWidth,
							(int) sHeight
						);
					}

					x += offX + (float) Math.Ceiling(sWidth);
				}

				x = 0;
				y += (float) Math.Ceiling(lHeight);
			}

			// Remove excess components.
			while (components.Count > i + 1) {
				ClickableComponent last = components[components.Count - 1];
				onDestroy?.Invoke(last);
				components.Remove(last);
			}
		}

		public static void RenderFlow(SpriteBatch batch, CachedFlow flow, Vector2 position, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			float x = 0;
			float y = 0;

			foreach (CachedFlowLine line in flow.Lines) {
				if (lineOffset > 0) {
					lineOffset--;
					continue;
				}

				float height = line.Height * scale;
				if (maxHeight >= 0 && y + height > maxHeight)
					break;

				foreach (IFlowNodeSlice slice in line.Slices) {
					if (slice == null || slice.IsEmpty())
						continue;

					IFlowNode node = slice.Node;
					float offsetY = GetYOffset(node.Alignment, slice.Height * scale, line.Height * scale);
					float offsetX = GetXOffset(node.Alignment, slice, line, scale, x, Math.Max(flow.Width * scale, flow.MaxWidth));

					node.Draw(
						slice,
						batch,
						new Vector2(
							position.X + x + offsetX,
							position.Y + y + offsetY
						),
						scale,
						flow.Font,
						defaultColor,
						defaultShadowColor,
						line,
						flow
					);

					x += offsetX + (float) Math.Ceiling(slice.Width * scale);
				}

				x = 0;
				y += (float) Math.Ceiling(height);
			}
		}

		public static CachedFlow RenderFlow(SpriteBatch batch, CachedFlow flow, Vector2 position, float maxWidth = -1, SpriteFont defaultFont = null, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			CachedFlow result = CalculateFlow(flow, maxWidth, defaultFont);
			RenderFlow(batch, result, position, defaultColor, defaultShadowColor, scale, lineOffset, maxHeight);
			return result;
		}

		public static CachedFlow RenderFlow(SpriteBatch batch, IEnumerable<IFlowNode> nodes, Vector2 position, float maxWidth = -1, SpriteFont defaultFont = null, Color? defaultColor = null, Color? defaultShadowColor = null, float scale = 1, int lineOffset = 0, float maxHeight = -1) {
			CachedFlow result = CalculateFlow(nodes, maxWidth, defaultFont);
			RenderFlow(batch, result, position, defaultColor, defaultShadowColor, scale, lineOffset, maxHeight);
			return result;
		}
	}
}
