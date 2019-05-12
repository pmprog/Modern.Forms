﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Modern.Forms
{
    // TODO:
    public class TextBox : LiteControl
    {
        public new static ControlStyle DefaultStyle = new ControlStyle (Control.DefaultStyle,
            (style) => {
                style.Border.Width = 1;
                style.BackgroundColor = ModernTheme.LightNeutralGray;
            });

        public override ControlStyle Style { get; } = new ControlStyle (DefaultStyle);

        private int cursor_index = 0;
        private bool read_only = false;
        private string placeholder;

        protected override Size DefaultSize => new Size (100, 28);

        public TextBox ()
        {
            SetControlBehavior (ControlBehaviors.InvalidateOnTextChanged);
        }

        public string Placeholder {
            get => placeholder;
            set {
                if (placeholder != value) {
                    placeholder = value;
                    Invalidate ();
                }
            }
        }

        public int GetCharIndexFromPosition (Point location)
        {
            if (CurrentText.Length == 0)
                return 0;

            var widths = TextMeasurer.MeasureCharacters (CurrentText + " ", CurrentStyle.GetFont (), CurrentStyle.GetFontSize (), ClientBounds.Left, ClientBounds.Top);

            for (var i = 0; i < widths.Length; i++)
                if (widths[i].X > location.X)
                    return i - 1;

            return widths.Length - 1;
        }

        protected override void OnKeyDown (KeyEventArgs e)
        {
            base.OnKeyDown (e);

            switch (e.KeyData & Keys.KeyCode) {
                case Keys.Delete:
                    if (read_only || cursor_index >= CurrentText.Length)
                        return;

                    Text = Text.Remove (cursor_index, 1);
                    e.Handled = true;
                    return;
                case Keys.Left:
                    if (cursor_index == 0)
                        return;

                    cursor_index--;
                    Invalidate ();
                    e.Handled = true;

                    return;
                case Keys.Right:
                    if (cursor_index == CurrentText.Length)
                        return;

                    cursor_index++;
                    Invalidate ();
                    e.Handled = true;

                    return;
                case Keys.Home:
                case Keys.Up:
                    cursor_index = 0;
                    Invalidate ();
                    e.Handled = true;

                    return;
                case Keys.End:
                case Keys.Down:
                    cursor_index = CurrentText.Length;
                    Invalidate ();
                    e.Handled = true;

                    return;
            }
        }

        protected override void OnKeyPress (KeyPressEventArgs e)
        {
            base.OnKeyPress (e);

            // Backspace = 8
            if (e.KeyChar == 8) {
                if (read_only || cursor_index == 0)
                    return;

                Text = CurrentText.Remove (--cursor_index, 1);
                e.Handled = true;
                return;
            }

            // Ctrl-Backspace = 127
            if (e.KeyChar == 127) {
                if (read_only || cursor_index == 0)
                    return;

                var new_index = TextMeasurer.FindNextSeparator (CurrentText, cursor_index, false);

                Text = CurrentText.Remove (new_index, cursor_index - new_index);
                cursor_index = new_index;
                e.Handled = true;
                return;
            }

            if (e.KeyChar >= 32)
                Text = CurrentText.Insert (cursor_index++, (e.KeyChar).ToString ());
        }

        protected override void OnMouseDown (MouseEventArgs e)
        {
            base.OnMouseDown (e);

            cursor_index = GetCharIndexFromPosition (e.Location);
            Invalidate ();
        }

        protected override void OnPaint (SKPaintEventArgs e)
        {
            base.OnPaint (e);

            var y = (int)(ClientBounds.Top + ((ClientBounds.Height - (14)) / 2) + 14) - 1;
            if (!string.IsNullOrEmpty (Text))
                e.Canvas.DrawText (Text, ClientBounds, CurrentStyle, ContentAlignment.MiddleLeft);
            else if (!string.IsNullOrEmpty (placeholder))
                e.Canvas.DrawText (placeholder, CurrentStyle.GetFont (), CurrentStyle.GetFontSize (), ClientBounds, ModernTheme.DisabledTextColor, ContentAlignment.MiddleLeft);

            var cursor_loc =
                    cursor_index == 0 ? 0f
                                      : TextMeasurer.MeasureText (Text.Substring (0, cursor_index), CurrentStyle.GetFont (), CurrentStyle.GetFontSize ());

            if (Selected)
                e.Canvas.DrawLine (ClientBounds.Left + cursor_loc, ClientBounds.Top + 4, ClientBounds.Left + cursor_loc, ClientBounds.Bottom - 4, ModernTheme.DarkTextColor);
        }

        private string CurrentText => Text ?? string.Empty;
    }
}