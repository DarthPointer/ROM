using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.Scrollbar
{
    /// <summary>
    /// An abstract base for scrollbars.
    /// </summary>
    /// <typeparam name="T">The type of edited value.</typeparam>
    public abstract class AbstractScrollbarElement<T> : IObjectEditorElement
    {
        #region Properties
        private string DisplayName { get; }

        private Func<T> Getter { get; }
        private Action<T> Setter { get; }

        protected abstract float Left { get; }
        protected abstract float Right { get; }

        private float PipWidth => (Right - Left) / 20;
        private float ExtraLength => PipWidth / 10;

        private float Min => Math.Min(Left, Right);
        private float Max => Math.Max(Left, Right);

        private T Target
        {
            get => Getter();
            set => Setter(value);
        }

        private T SavedValue { get; set; }

        public bool HasChanges => !Equals(SavedValue, Getter());
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        public AbstractScrollbarElement(string displayName, Func<T> getter, Action<T> setter)
        {
            DisplayName = displayName;

            Getter = getter;
            Setter = setter;

            SavedValue = Target;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts scroll position to desired value.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected abstract T PosToValue(float pos);
        /// <summary>
        /// Converts value to scroll coordinate.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract float ValueToPos(T value);

        /// <summary>
        /// Formats a string to display value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract string Formatter(T value);

        public void Draw()
        {
            float left, right;

            if (Left < Right)
            {
                left = Left - ExtraLength;
                right = Right + PipWidth + ExtraLength;
            }
            else
            {
                left = Left - PipWidth - ExtraLength;
                right = Right + ExtraLength;
            }

            GUILayout.Label(DisplayName + ' ' + Formatter(Target));
            float currentPos = ValueToPos(Target);
            float newPos = GUILayout.HorizontalScrollbar(currentPos, Mathf.Abs(PipWidth), left, right);
            newPos = Mathf.Min(newPos, Max);
            newPos = Mathf.Max(newPos, Min);
            if (newPos != currentPos)
            {
                Target = PosToValue(newPos);
            }
        }

        public void OnSaved()
        {
            SavedValue = Target;
        }

        public void ResetChanges()
        {
            Target = SavedValue;
        }

        public void DrawPostWindow()
        { }
        #endregion
    }

    /// <summary>
    /// A basic implementation of scrollbar that gets its calls to use as delegates in ctor args.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScrollbarElement<T> : AbstractScrollbarElement<T>
    {
        #region Properties
        protected override float Left { get; }
        protected override float Right { get; }

        private Func<float, T> PosToValueCall { get; }
        private Func<T, float> ValueToPosCall { get; }

        private Func<T, string> FormatterCall{ get; }
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="left">The leftmost coordinate of the scrollbar.</param>
        /// <param name="right">The rightmost coordinate of the scrollbar.</param>
        /// <param name="posToValue">Converts scroll position to desired value.</param>
        /// <param name="valueToPos">Converts value to scroll coordinate.</param>
        /// <param name="formatter">Formats a string to display value.</param>
        public ScrollbarElement(string displayName, Func<T> getter, Action<T> setter,
            float left, float right, Func<float, T> posToValue, Func<T, float> valueToPos, Func<T, string> formatter) :
            base(displayName, getter, setter)
        {
            Left = left;
            Right = right;

            PosToValueCall = posToValue;
            ValueToPosCall = valueToPos;

            FormatterCall = formatter;
        }
        #endregion

        #region Methods
        protected override T PosToValue(float pos)
        {
            return PosToValueCall(pos);
        }

        protected override float ValueToPos(T value)
        {
            return ValueToPosCall(value);
        }

        protected override string Formatter(T value)
        {
            return FormatterCall(value);
        }
        #endregion
    }

    /// <summary>
    /// A scrollbar implementation that selects an option from a list.
    /// </summary>
    /// <typeparam name="T">The options type.</typeparam>
    public class OptionsScrollbarElement<T> : AbstractScrollbarElement<T>
    {
        protected override float Left => 0;
        protected override float Right => Options.Count - 1;

        private IReadOnlyList<T> Options { get; }
        private Dictionary<T, string> OptionNames { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="options">The options to select from.</param>
        public OptionsScrollbarElement(string displayName, Func<T> getter, Action<T> setter,
            IEnumerable<Option<T>> options) :
            base(displayName, getter, setter)
        {
            Options = options.Select(opt => opt.Value).ToList();

            OptionNames = options.ToDictionary(opt => opt.Value, opt => opt.Name);
        }

        protected override T PosToValue(float pos)
        {
            return Options[Mathf.RoundToInt(pos)];
        }

        protected override float ValueToPos(T value)
        {
            for (int i = 0; i < Options.Count; i++)
            {
                if (Equals(value, Options[i]))
                    return i;
            }

            return -1;
        }

        protected override string Formatter(T value)
        {
            if (OptionNames.TryGetValue(value, out var name))
                return name;

            return "INVALID VALUE";
        }
    }
}
