﻿using System.Globalization;
using System.Text;
using System.Windows.Input;

using Blish_HUD;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace SL.Common.Controls;

public class NumberInput : TextInputBase
{
    public event EventHandler<EventArgs>? EnterPressed;

    private const int TextPaddingX = 10;

    private const int SpinnerWidth = 32;

    private const int SpinnerButtonHeight = 16;

    // Points up
    private static readonly Texture2D SpinnerSprite = EmbeddedResources.Texture("spinner.png");

    // Points up
    private static readonly Texture2D SpinnerGlowSprite = EmbeddedResources.Texture("spinner-glow.png");

    private static readonly Texture2D TextBoxSprite = EmbeddedResources.Texture("textbox.png");

    private NumberInputSpinnerGlow _glow = NumberInputSpinnerGlow.None;

    private NumberInputAction _action = NumberInputAction.None;

    private TimeSpan _incrementInterval = TimeSpan.FromMilliseconds(150);

    private TimeSpan _incrementTimer;

    private Rectangle _textBoxRectangle = Rectangle.Empty;

    private Rectangle _textRectangle = Rectangle.Empty;

    private Rectangle _cursorRectangle = Rectangle.Empty;

    private Rectangle _highlightRectangle = Rectangle.Empty;

    private int _horizontalOffset;

    private int _minValue = int.MinValue;

    private int _maxValue = int.MaxValue;

    public NumberInput()
    {
        Width = 150;
        Height = SpinnerButtonHeight * 2;
        TextChanged += OnTextChanged;
        InputFocusChanged += OnInputFocusChanged;
        Input.Mouse.MouseMoved += OnGlobalMouseMoved;
        Input.Mouse.LeftMouseButtonReleased += OnGlobalLeftMouseButtonReleased;
        Input.Mouse.MouseWheelScrolled += OnGlobalMouseWheelScrolled;
    }

    public event EventHandler<EventArgs>? ValueChanged;

    public int Value
    {
        get => int.TryParse(Text, out int value) ? value : 0;
        set
        {
            if (value < MinValue)
            {
                value = MinValue;
            }
            else if (value > MaxValue)
            {
                value = MaxValue;
            }

            string text = value.ToString(NumberFormatInfo.InvariantInfo);
            if (Text != text)
            {
                Text = text;
                OnValueChanged();
            }
        }
    }

    public int MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (Value < value)
            {
                Value = value;
            }
        }
    }

    public int MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (Value > value)
            {
                Value = value;
            }
        }
    }

    public override void DoUpdate(GameTime gameTime)
    {
        ThrowHelper.ThrowIfNull(gameTime);
        switch (_action)
        {
            case NumberInputAction.Increment:
                _incrementTimer += gameTime.ElapsedGameTime;
                if (_incrementTimer >= _incrementInterval)
                {
                    Value++;
                    _incrementTimer += gameTime.ElapsedGameTime;
                    _incrementInterval = TimeSpan.FromMilliseconds(100);
                }
                break;
            case NumberInputAction.Decrement:
                _incrementTimer += gameTime.ElapsedGameTime;
                if (_incrementTimer >= _incrementInterval)
                {
                    Value--;
                    _incrementTimer += gameTime.ElapsedGameTime;
                    _incrementInterval = TimeSpan.FromMilliseconds(100);
                }
                break;
            case NumberInputAction.Drag:
            case NumberInputAction.None:
            default:
                _incrementTimer = TimeSpan.Zero;
                _incrementInterval = TimeSpan.FromMilliseconds(150);
                break;
        }

        base.DoUpdate(gameTime);
    }

    public override void RecalculateLayout()
    {
        _textBoxRectangle = TextBoxRectangle();

        _textRectangle = TextRectangle();

        _highlightRectangle = HighlightRectangle();

        _cursorRectangle = CursorRectangle();

        base.RecalculateLayout();

        Rectangle TextBoxRectangle()
        {
            return new Rectangle(0, 0, Width - SpinnerWidth, Height);
        }

        Rectangle TextRectangle()
        {
            int verticalPadding = (Height / 2) - (_font.LineHeight / 2);
            return new Rectangle(
                _horizontalOffset - TextPaddingX,
                verticalPadding,
                Width - SpinnerWidth,
                Height - (verticalPadding * 2)
            );
        }

        Rectangle CursorRectangle()
        {
            int currentCursorIndex = _cursorIndex;
            if (currentCursorIndex > _text.Length)
            {
                currentCursorIndex = _text.Length;
            }

            float textStart = Width - _font.MeasureString(Text).Width - SpinnerWidth + _horizontalOffset - TextPaddingX;
            float cursorStart = textStart + _font.MeasureString(_text[..currentCursorIndex]).Width;
            return new Rectangle(
                (int)cursorStart,
                _textRectangle.Y + 2,
                2,
                _font.LineHeight - 4
            );
        }

        Rectangle HighlightRectangle()
        {
            int currentSelectionStart = _selectionStart;
            int currentSelectionEnd = _selectionEnd;
            int selectionOffset = Math.Min(currentSelectionStart, currentSelectionEnd);
            if (selectionOffset > _text.Length)
            {
                return Rectangle.Empty;
            }

            int selectionLength = Math.Abs(currentSelectionStart - currentSelectionEnd);
            if (selectionLength <= 0 || selectionOffset + selectionLength > _text.Length)
            {
                return Rectangle.Empty;
            }

            float textStart = Width - _font.MeasureString(_text).Width - SpinnerWidth + _horizontalOffset - TextPaddingX;
            float highlightStart = textStart + _font.MeasureString(_text[..selectionOffset]).Width;
            float highlightWidth = _font.MeasureString(_text.Substring(selectionOffset, selectionLength)).Width;

            return new Rectangle(
                (int)highlightStart - 1,
                _textRectangle.Y,
                (int)highlightWidth,
                _font.LineHeight - 1).Clip(_textBoxRectangle);
        }
    }

    public override int GetCursorIndexFromPosition(int x, int y)
    {
        float textStart = Width - _font.MeasureString(Text).Width - TextPaddingX - SpinnerWidth;

        int charIndex = 0;

        BitmapFont.StringGlyphEnumerable glyphs = _font.GetGlyphs(_text);
        foreach (BitmapFontGlyph glyph in glyphs)
        {
            if (textStart + glyph.Position.X + (glyph.FontRegion.Width / 2f) > _horizontalOffset + x)
            {
                break;
            }

            charIndex++;
        }

        return charIndex;
    }

    protected override void OnMouseMoved(MouseEventArgs e)
    {
        ThrowHelper.ThrowIfNull(e);
        bool mouseOverSpinner = e.MousePosition.X > AbsoluteBounds.Right - SpinnerWidth;
        bool mouseOverUpButton = mouseOverSpinner && e.MousePosition.Y < AbsoluteBounds.Top + SpinnerButtonHeight;
        _glow = (mouseOverSpinner, mouseOverUpButton) switch
        {
            (true, true) => NumberInputSpinnerGlow.Up,
            (true, false) => NumberInputSpinnerGlow.Down,
            _ => NumberInputSpinnerGlow.None
        };

        base.OnMouseMoved(e);
    }

    protected override void OnMouseLeft(MouseEventArgs e)
    {
        _glow = NumberInputSpinnerGlow.None;
        base.OnMouseLeft(e);
    }

    protected override void MoveLine(int delta)
    {
        // Not sure what to do here
    }

    protected override void UpdateScrolling()
    {
        float lineWidth = MeasureStringWidth(_text[_cursorIndex..]);

        _horizontalOffset = _cursorIndex < _prevCursorIndex
            ? (int)Math.Max(_horizontalOffset, lineWidth + (TextPaddingX * 2) - _textBoxRectangle.Width)
            : (int)Math.Min(_horizontalOffset, lineWidth);

        _prevCursorIndex = _cursorIndex;
        Invalidate();
    }

    protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
    {
        ThrowHelper.ThrowIfNull(e);
        if (e.MousePosition.X > AbsoluteBounds.Right - SpinnerWidth)
        {
            UnsetFocus();
            Soundboard.Click();
            _action = e.MousePosition.Y < AbsoluteBounds.Top + SpinnerButtonHeight
                ? NumberInputAction.Increment
                : NumberInputAction.Decrement;
        }
        else
        {
            _action = NumberInputAction.Drag;
            base.OnLeftMouseButtonPressed(e);
        }
    }

    protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
    {
        switch (_action)
        {
            case NumberInputAction.Increment:
                Value++;
                UnsetFocus();
                break;
            case NumberInputAction.Decrement:
                Value--;
                UnsetFocus();
                break;
            case NumberInputAction.Drag:
                base.OnLeftMouseButtonReleased(e);
                break;
            case NumberInputAction.None:
                break;
            default:
                break;
        }

        _action = NumberInputAction.None;
    }

    protected override void OnClick(MouseEventArgs e)
    {
        SelectAll();
        base.OnClick(e);
    }

    protected override void HandleEnter()
    {
        OnEnterPressed();
    }

    protected virtual void OnEnterPressed()
    {
        UnsetFocus();
        EnterPressed?.Invoke(this, EventArgs.Empty);
    }

    /// <remarks>
    /// Direct copy of <see cref="TextInputBase.PaintText(SpriteBatch, Rectangle, HorizontalAlignment)"/>
    /// that also exposes the clippingRectangle parameter of
    /// <see cref="BitmapFontExtensions.DrawString(SpriteBatch, BitmapFont, string, Vector2, Color, Rectangle?)"/>.
    /// </remarks>
    protected virtual void PaintText(SpriteBatch spriteBatch, Rectangle textRegion, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, Rectangle? clippingRectangle = null)
    {
        if (!_focused && _text.Length == 0)
        {
            spriteBatch.DrawStringOnCtrl(this, _placeholderText, _font, textRegion, Color.LightGray, false, false, 0, horizontalAlignment, VerticalAlignment.Top, clippingRectangle);
        }

        spriteBatch.DrawStringOnCtrl(this, _text, _font, textRegion, _foreColor, false, false, 0, horizontalAlignment, VerticalAlignment.Top, clippingRectangle);
    }

    protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        #region Text

        if (Focused)
        {
            spriteBatch.DrawOnCtrl(
                this,
                TextBoxSprite,
                _textBoxRectangle
            );

            if (_action != NumberInputAction.Drag)
            {
                if (_highlightRectangle.IsEmpty)
                {
                    PaintCursor(spriteBatch, _cursorRectangle);
                }
                else
                {
                    PaintHighlight(spriteBatch, _highlightRectangle);
                }
            }
        }

        PaintText(spriteBatch, _textRectangle, HorizontalAlignment.Right, _textBoxRectangle);

        #endregion Text

        #region Spinner

        Rectangle buttonsRectangle = new(bounds.Right - SpinnerWidth, 0, SpinnerWidth, SpinnerButtonHeight * 2);
        switch ((hoverButton: _glow, pressedButton: _action))
        {
            case (NumberInputSpinnerGlow.Up, NumberInputAction.None):
                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerGlowSprite,
                    buttonsRectangle
                );

                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerSprite,
                    buttonsRectangle,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically
                );
                break;
            case (NumberInputSpinnerGlow.Down, NumberInputAction.None):
                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerSprite,
                    buttonsRectangle
                );
                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerGlowSprite,
                    buttonsRectangle,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically
                );
                break;
            default:
                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerSprite,
                    buttonsRectangle
                );

                spriteBatch.DrawOnCtrl(
                    this,
                    SpinnerSprite,
                    buttonsRectangle,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically
                );
                break;
        }

        #endregion Spinner
    }

    protected override void DisposeControl()
    {
        ValueChanged = null;
        Input.Mouse.MouseMoved -= OnGlobalMouseMoved;
        Input.Mouse.LeftMouseButtonReleased -= OnGlobalLeftMouseButtonReleased;
        Input.Mouse.MouseWheelScrolled -= OnGlobalMouseWheelScrolled;
        base.DisposeControl();
    }

    private void OnInputFocusChanged(object sender, ValueEventArgs<bool> e)
    {
        if (!e.Value)
        {
            Text = Value.ToString(NumberFormatInfo.InvariantInfo);
            _horizontalOffset = 0;
            Invalidate();
        }
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_text))
        {
            return;
        }

        StringBuilder numericBuilder = new(_text.Length);
        ReadOnlySpan<char> input = _text.AsSpan();
        foreach (char c in input)
        {
            if (numericBuilder.Length == 0)
            {
                if (c is '+' or '-')
                {
                    _ = numericBuilder.Append(c);
                }
            }

            if (c is >= '0' and <= '9')
            {
                _ = numericBuilder.Append(c);
            }
        }

        if (int.TryParse(numericBuilder.ToString(), out int value))
        {
            Value = value;
            OnValueChanged();
        }
    }

    private void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnGlobalMouseMoved(object sender, MouseEventArgs e)
    {
        if (_action == NumberInputAction.Drag)
        {
            int center = AbsoluteBounds.Y + _textBoxRectangle.Center.Y;
            switch (e.MousePosition.Y - center)
            {
                case < -1:
                    Value++;
                    HideMousePosition();
                    break;
                case > 1:
                    Value--;
                    HideMousePosition();
                    break;
                default:
                    break;
            }

            CursorIndex = Text.Length;
        }
    }

    private void OnGlobalLeftMouseButtonReleased(object sender, MouseEventArgs e)
    {
        if (!MouseOver)
        {
            _action = NumberInputAction.None;
        }
    }


    private void OnGlobalMouseWheelScrolled(object sender, MouseEventArgs e)
    {
        if (MouseOver)
        {
            if (Input.Mouse.State.ScrollWheelValue > 0)
            {
                Value++;
            }
            else
            {
                Value--;
            }
        }
    }


    private void HideMousePosition()
    {
        int x = AbsoluteBounds.X + Width - (SpinnerWidth / 2);
        int y = AbsoluteBounds.Y + (Height / 2);
        System.Windows.Input.Mouse.OverrideCursor = Cursors.None;
        Mouse.SetPosition(
            (int)(x * GameService.Graphics.UIScaleMultiplier),
            (int)(y * GameService.Graphics.UIScaleMultiplier));
    }
}
