using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewValley;


namespace Leclair.Stardew.Common
{
    public static class RenderHelper {

		private static IModHelper Helper;

		public static void SetHelper(IModHelper helper) {
			Helper = helper;
		}

		public static Rectangle GetIntersection(this Rectangle self, Rectangle other) {
			return Rectangle.Intersect(self, other);
		}

		public static Rectangle Clone(this Rectangle self) {
			return self;
		}

		public static void DrawBox(
			SpriteBatch b,
			Texture2D texture,
			Rectangle sourceRect,
			int x, int y,
			int width, int height,
			Color color,
			int topSlice = -1,
			int leftSlice = -1,
			int rightSlice = -1,
			int bottomSlice = -1,
			float scale = 1f,
			bool drawShadow = true,
			float draw_layer = -1f
		) {
			if (topSlice == -1)
				topSlice = sourceRect.Height / 3;
			if (bottomSlice == -1)
				bottomSlice = sourceRect.Height / 3;
			if (leftSlice == -1)
				leftSlice = sourceRect.Width / 3;
			if (rightSlice == -1)
				rightSlice = sourceRect.Width / 3;

			float layerDepth = draw_layer - 0.03f;
			if (draw_layer < 0f) {
				draw_layer = 0.8f - y * 1E-06f;
				layerDepth = 0.77f;
			}

			int sTop = (int) (topSlice * scale);
			int sLeft = (int) (leftSlice * scale);
			int sRight = (int) (rightSlice * scale);
			int sBottom = (int) (bottomSlice * scale);

			// Base
			b.Draw(
				texture,
				new Rectangle(
					x + sLeft, y + sTop,
					width - sLeft - sRight,
					height - sTop - sBottom
				),
				new Rectangle(
					x: sourceRect.X + leftSlice,
					y: sourceRect.Y + topSlice,
					width: sourceRect.Width - leftSlice - rightSlice,
					height: sourceRect.Height - topSlice - bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Top Left
			b.Draw(
				texture,
				new Rectangle(x, y, sLeft, sTop),
				new Rectangle(
					x: sourceRect.X,
					y: sourceRect.Y,
					width: leftSlice,
					height: topSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Top Middle
			b.Draw(
				texture,
				new Rectangle(x + sLeft, y, width - sLeft - sRight, sTop),
				new Rectangle(
					x: sourceRect.X + leftSlice,
					y: sourceRect.Y,
					width: sourceRect.Width - leftSlice - rightSlice,
					height: topSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Top Right
			b.Draw(
				texture,
				new Rectangle(x + width - sRight, y, sRight, sTop),
				new Rectangle(
					x: sourceRect.X + sourceRect.Width - rightSlice,
					y: sourceRect.Y,
					width: rightSlice,
					height: topSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Left
			b.Draw(
				texture,
				new Rectangle(x, y + sTop, sLeft, height - sTop - sBottom),
				new Rectangle(
					x: sourceRect.X,
					y: sourceRect.Y + topSlice,
					width: leftSlice,
					height: sourceRect.Height - topSlice - bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Right
			b.Draw(
				texture,
				new Rectangle(
					x: x + width - sRight,
					y: y + sTop,
					width: sRight,
					height: height - sTop - sBottom),
				new Rectangle(
					x: sourceRect.X + sourceRect.Width - rightSlice,
					y: sourceRect.Y + topSlice,
					width: rightSlice,
					height: sourceRect.Height - topSlice - bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Bottom Left
			b.Draw(
				texture,
				new Rectangle(
					x: x,
					y: y + height - sBottom,
					width: sLeft,
					height: sBottom
				),
				new Rectangle(
					x: sourceRect.X,
					y: sourceRect.Y + sourceRect.Height - bottomSlice,
					width: leftSlice,
					height: bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Bottom Middle
			b.Draw(
				texture,
				new Rectangle(
					x: x + sLeft,
					y: y + height - sBottom,
					width: width - sLeft - sRight,
					height: sBottom
				),
				new Rectangle(
					x: sourceRect.X + leftSlice,
					y: sourceRect.Y + sourceRect.Height - bottomSlice,
					width: sourceRect.Width - leftSlice - rightSlice,
					height: bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

			// Bottom Right
			b.Draw(
				texture,
				new Rectangle(
					x: x + width - sRight,
					y: y + height - sBottom,
					width: sRight,
					height: sBottom
				),
				new Rectangle(
					x: sourceRect.X + sourceRect.Width - rightSlice,
					y: sourceRect.Y + sourceRect.Height - bottomSlice,
					width: rightSlice,
					height: bottomSlice
				),
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				draw_layer
			);

		}


		public static void WithScissor(SpriteBatch b, SpriteSortMode mode, Rectangle rectangle, Action action) {

			var smField = Helper.Reflection.GetField<SpriteSortMode>(b, "_sortMode", false);
			SpriteSortMode old_sort = smField?.GetValue() ?? mode;

			var bsField = Helper.Reflection.GetField<BlendState>(b, "_blendState", false);
			BlendState old_blend = bsField?.GetValue();

			var ssField = Helper.Reflection.GetField<SamplerState>(b, "_samplerState", false);
			SamplerState old_sampler = ssField?.GetValue();

			var dsField = Helper.Reflection.GetField<DepthStencilState>(b, "_depthStencilState", false);
			DepthStencilState old_depth = dsField?.GetValue();

			var rsField = Helper.Reflection.GetField<RasterizerState>(b, "_rasterizerState", false);
			RasterizerState old_rasterizer = rsField?.GetValue();

			var efField = Helper.Reflection.GetField<Effect>(b, "_effect", false);
			Effect old_effect = efField?.GetValue();

			var old_scissor = b.GraphicsDevice.ScissorRectangle;

			RasterizerState state = new() {
				ScissorTestEnable = true
			};

			if (old_rasterizer != null) {
				state.CullMode = old_rasterizer.CullMode;
				state.FillMode = old_rasterizer.FillMode;
				state.DepthBias = old_rasterizer.DepthBias;
				state.MultiSampleAntiAlias = old_rasterizer.MultiSampleAntiAlias;
				state.SlopeScaleDepthBias = old_rasterizer.SlopeScaleDepthBias;
				state.DepthClipEnable = old_rasterizer.DepthClipEnable;
			}

			b.End();

			b.Begin(
				sortMode: mode,
				blendState: old_blend,
				samplerState: old_sampler,
				depthStencilState: old_depth,
				rasterizerState: state,
				effect: old_effect,
				transformMatrix: null
			);

			b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(rectangle, old_scissor);

			try {
				action?.Invoke();
			} finally {
				b.End();
				b.Begin(
					sortMode: old_sort,
					blendState: old_blend,
					samplerState: old_sampler,
					depthStencilState: old_depth,
					rasterizerState: old_rasterizer,
					effect: old_effect,
					transformMatrix: null
				);

				b.GraphicsDevice.ScissorRectangle = old_scissor;
			}
		}


    }
}
