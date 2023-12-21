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
    public abstract class AbstractScrollbarElement<T> : IObjectEditorElement
        where T : notnull
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

        public bool HasChanges => !SavedValue.Equals(Getter());
        #endregion

        #region Constructors
        public AbstractScrollbarElement(string displayName, Func<T> getter, Action<T> setter)
        {
            DisplayName = displayName;

            Getter = getter;
            Setter = setter;

            SavedValue = Target;
        }
        #endregion

        #region Methods
        protected abstract T PosToValue(float pos);
        protected abstract float ValueToPos(T value);

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

    public class ScrollbarElement<T> : AbstractScrollbarElement<T>
        where T: notnull
    {
        #region Properties
        protected override float Left { get; }
        protected override float Right { get; }

        private Func<float, T> PosToValueCall { get; }
        private Func<T, float> ValueToPosCall { get; }

        private Func<T, string> FormatterCall{ get; }
        #endregion

        #region Constructors
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

    public class OptionsScrollbarElement<T> : AbstractScrollbarElement<T>
        where T : notnull
    {
        protected override float Left => 0;
        protected override float Right => Options.Count - 1;

        private IReadOnlyList<T> Options { get; }
        private Dictionary<T, string> OptionNames { get; }

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
                if (value.Equals(Options[i]))
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
