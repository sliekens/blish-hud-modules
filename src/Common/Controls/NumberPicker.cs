using System.Globalization;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace SL.Common.Controls;

public sealed class NumberPicker : TextInputBase
{
    private int _horizontalOffset;

    private Rectangle _textBoxRectangle = Rectangle.Empty;

    private Rectangle _textRectangle = Rectangle.Empty;

    private Rectangle _highlightRectangle = Rectangle.Empty;

    private Rectangle _cursorRectangle = Rectangle.Empty;

    private Rectangle _upArrowRectangle = Rectangle.Empty;

    private Rectangle _downArrowRectangle = Rectangle.Empty;

    private bool _highlightUpArrow;

    private bool _highlightDownArrow;

    private string _controlPressed = "none";

    private TimeSpan _incrementInterval = TimeSpan.FromMilliseconds(150);

    private TimeSpan _incrementTimer;

    private const int TextPaddingX = 10;

    private static readonly Texture2D ArrowButtonSprite = Resources.Texture("arrow-button.png");

    private static readonly Texture2D ArrowButtonFocusSprite = Resources.Texture("arrow-button-focus.png");

    private static readonly Texture2D TextBoxSprite = Resources.Texture("textbox.png");

    private static readonly SoundEffect ClickSoundEffect = Resources.Sound("click.wav");

    public int Value
    {
        get => int.TryParse(Text, out int value) ? value : 0;
        set => Text = value.ToString(NumberFormatInfo.InvariantInfo);
    }

    public NumberPicker()
    {
        TextChanged += OnTextChanged;
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        Invalidate();
    }

    protected override void OnMouseMoved(MouseEventArgs e)
    {
        var relativePosition = e.MousePosition - AbsoluteBounds.Location;
        _highlightUpArrow = _upArrowRectangle.Contains(relativePosition);
        _highlightDownArrow = _downArrowRectangle.Contains(relativePosition);
        base.OnMouseMoved(e);
    }
    protected override void OnMouseLeft(MouseEventArgs e)
    {
        _highlightDownArrow = false;
        _highlightUpArrow = false;
        base.OnMouseLeft(e);
    }

    protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
    {
        var relativePosition = e.MousePosition - AbsoluteBounds.Location;
        if (_upArrowRectangle.Contains(relativePosition))
        {
            ClickSoundEffect.Play(0.4f, 0, 0);
            _controlPressed = "up";
            Focused = false;
        }
        else if (_downArrowRectangle.Contains(relativePosition))
        {
            ClickSoundEffect.Play(0.4f, 0, 0);
            _controlPressed = "down";
            Focused = false;
        }
        else if (_textBoxRectangle.Contains(relativePosition))
        {
            _controlPressed = "none";
            base.OnLeftMouseButtonPressed(e);
        }
    }

    protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
    {
        var relativePosition = e.MousePosition - AbsoluteBounds.Location;
        if (_controlPressed == "up" && _upArrowRectangle.Contains(relativePosition))
        {
            Focused = false;
            Value++;
        }
        else if (_controlPressed == "down" && _downArrowRectangle.Contains(relativePosition))
        {
            Focused = false;
            Value--;
        }
        else if (_textBoxRectangle.Contains(relativePosition))
        {
            base.OnLeftMouseButtonReleased(e);
        }

        _controlPressed = "none";
    }

    protected override void OnClick(MouseEventArgs e)
    {
        SelectAll();
        base.OnClick(e);
    }

    public override void DoUpdate(GameTime gameTime)
    {

        var relativePosition = GameService.Input.Mouse.Position - AbsoluteBounds.Location;
        if (_controlPressed == "up" && _upArrowRectangle.Contains(relativePosition))
        {
            _incrementTimer += gameTime.ElapsedGameTime;
            if (_incrementTimer > _incrementInterval)
            {
                Value++;
                _incrementTimer = TimeSpan.Zero;
                _incrementInterval = TimeSpan.FromMilliseconds(100);
            }
        }
        else if (_controlPressed == "down" && _downArrowRectangle.Contains(relativePosition))
        {
            _incrementTimer += gameTime.ElapsedGameTime;
            if (_incrementTimer > _incrementInterval)
            {
                Value--;
                _incrementTimer = TimeSpan.Zero;
                _incrementInterval = TimeSpan.FromMilliseconds(100);
            }
        }
        else
        {
            _incrementTimer = TimeSpan.Zero;
            _incrementInterval = TimeSpan.FromMilliseconds(150);
        }

        base.DoUpdate(gameTime);
    }

    public override void RecalculateLayout()
    {
        int buttonsWidth = ArrowButtonSprite.Width;
        string currentText = _text;
        int currentCursorIndex = _cursorIndex;

        _textBoxRectangle = TextBoxRectangle();

        _textRectangle = TextRectangle();

        _highlightRectangle = HighlightRectangle();

        _cursorRectangle = CursorRectangle();

        var verticalCenter = Height / 2;

        _upArrowRectangle = new Rectangle(
            new Point(Width - buttonsWidth, verticalCenter - 8),
            new Point(16, 8)
        );

        _downArrowRectangle = new Rectangle(
            new Point(Width - buttonsWidth, verticalCenter),
            new Point(16, 8)
                );

        Rectangle TextBoxRectangle()
        {
            float textWidth = _font.MeasureString(currentText).Width;
            float textStart = Width - textWidth - TextPaddingX * 2 - buttonsWidth;
            return new Rectangle(
                (int)textStart,
                0,
                (int)textWidth + TextPaddingX * 2,
                _size.Y
            );
        }

        Rectangle TextRectangle()
        {
            int verticalPadding = (_size.Y / 2) - (_font.LineHeight / 2);
            return new Rectangle(
                TextPaddingX - _horizontalOffset,
                verticalPadding,
                _size.X - TextPaddingX * 2 - buttonsWidth,
                _size.Y - (verticalPadding * 2)
            );
        }

        Rectangle HighlightRectangle()
        {
            int currentSelectionStart = _selectionStart;
            int currentSelectionEnd = _selectionEnd;
            int selectionOffset = Math.Min(currentSelectionStart, currentSelectionEnd);
            if (selectionOffset > currentText.Length)
            {
                return Rectangle.Empty;
            }

            int selectionLength = Math.Abs(currentSelectionStart - currentSelectionEnd);
            if (selectionLength <= 0 || selectionOffset + selectionLength > currentText.Length)
            {
                return Rectangle.Empty;
            }

            float textStart = Width - _font.MeasureString(currentText).Width - TextPaddingX - buttonsWidth;
            float highlightStart = textStart + _font.MeasureString(currentText[..selectionOffset]).Width;
            float highlightWidth = _font.MeasureString(currentText.Substring(selectionOffset, selectionLength)).Width;

            return new Rectangle(
                (int)highlightStart - 1,
                _textRectangle.Y,
                (int)highlightWidth,
                _font.LineHeight - 1);
        }

        Rectangle CursorRectangle()
        {
            if (currentCursorIndex > currentText.Length)
            {
                currentCursorIndex = currentText.Length;
            }

            float textStart = Width - _font.MeasureString(Text).Width - TextPaddingX - buttonsWidth;
            float cursorStart = textStart + _font.MeasureString(currentText[..currentCursorIndex]).Width;
            return new Rectangle(
                (int)cursorStart,
                _textRectangle.Y + 2,
                2,
                _font.LineHeight - 4
            );
        }
    }

    protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        if (Focused)
        {
            spriteBatch.DrawOnCtrl(
                this,
                TextBoxSprite,
                _textBoxRectangle
            );

            if (_highlightRectangle.IsEmpty)
            {
                PaintCursor(spriteBatch, _cursorRectangle);
            }
            else
            {
                PaintHighlight(spriteBatch, _highlightRectangle);
            }
        }

        PaintText(spriteBatch, _textRectangle, HorizontalAlignment.Right);

        spriteBatch.DrawOnCtrl(
            this,
            _highlightUpArrow ? ArrowButtonFocusSprite : ArrowButtonSprite,
            _upArrowRectangle,
            new Rectangle(
                new Point(0, 4),
                new Point(16, 9)),
            Color.White * AbsoluteOpacity(),
            0f,
            Vector2.Zero,
            SpriteEffects.FlipVertically
            );

        spriteBatch.DrawOnCtrl(
            this,
            _highlightDownArrow ? ArrowButtonFocusSprite : ArrowButtonSprite,
            _downArrowRectangle,
            new Rectangle(
                new Point(0, 4),
                new Point(16, 9)),
            Color.White * AbsoluteOpacity(),
            0f,
            Vector2.Zero
            );
    }

    protected override void MoveLine(int delta)
    {
    }

    protected override void UpdateScrolling()
    {
        float lineWidth = _font.MeasureString(_text[.._cursorIndex]).Width;

        if (_cursorIndex > _prevCursorIndex)
        {
            _horizontalOffset = (int)Math.Max(_horizontalOffset, lineWidth - _size.X);
        }
        else
        {
            _horizontalOffset = (int)Math.Min(_horizontalOffset, lineWidth);
        }

        _prevCursorIndex = _cursorIndex;
        Invalidate();
    }

    public override int GetCursorIndexFromPosition(int x, int y)
    {
        var textStart = Width - _font.MeasureString(Text).Width - TextPaddingX - ArrowButtonSprite.Width;

        int charIndex = 0;

        var glyphs = _font.GetGlyphs(_text);
        foreach (var glyph in glyphs)
        {
            if (textStart + glyph.Position.X + glyph.FontRegion.Width / 2f > _horizontalOffset + x)
            {
                break;
            }

            charIndex++;
        }

        return charIndex;
    }

    protected override void DisposeControl()
    {
        TextChanged -= OnTextChanged;
        base.DisposeControl();
    }
}