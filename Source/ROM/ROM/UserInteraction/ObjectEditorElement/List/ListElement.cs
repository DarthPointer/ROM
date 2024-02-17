using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.List
{
    public class ListElement<T> : IObjectEditorElement
    {
        #region Fields
        Vector2 _itemsScrollState;
        #endregion

        #region Properties
        private string DisplayName { get; }

        public bool IsCollapsed { get; set; } = true;
        private bool DrawCollaspeButton { get; }

        public bool HasChanges => throw new NotImplementedException();

        public delegate IObjectEditorElement ItemElementFactoryDelegate(T item, Action deleteCall);
        private ItemElementFactoryDelegate ItemElementFactory { get; }

        private List<IObjectEditorElement> ItemElements { get; set; }
        private Dictionary<T, IObjectEditorElement> ElementsByItems { get; set; }

        private List<IObjectEditorElement> ElementsToDelete { get; } = [];

        private Func<List<T>> Getter { get; }
        private Action<List<T>> Setter { get; }

        private List<T> Target
        {
            get => Getter();
            set => Setter(value);
        }

        private List<T> SavedList { get; set; }
        #endregion

        #region Constructors
        public ListElement(string displayName, Func<List<T>> getter, Action<List<T>> setter, ItemElementFactoryDelegate itemElementFactory, bool drawCollapseButton = true)
        {
            DisplayName = displayName;
            ItemElementFactory = itemElementFactory;
            Getter = getter;
            Setter = setter;

            SavedList = [.. Target];
            RegenerateElements();

            DrawCollaspeButton = drawCollapseButton;
        }
        #endregion

        #region Methods
        public void AddItem(T item)
        {
            Target.Add(item);
            IObjectEditorElement newElement = ItemElementFactory(item, () => DeleteItem(item));

            ElementsByItems[item] = newElement;
            ItemElements.Add(newElement);
        }

        private void DeleteItem(T item)
        {
            Target.Remove(item);
            ElementsByItems[item].Terminate();
            ElementsToDelete.Add(ElementsByItems[item]);
            ElementsByItems.Remove(item);
        }

        private void CleanupElements()
        {
            ItemElements.RemoveAll(ElementsToDelete.Contains);
            ElementsToDelete.Clear();
        }

        public void Draw(RoomCamera? roomCamera)
        {
            GUILayout.Label(DisplayName);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{ItemElements.Count} items");
            GUILayout.FlexibleSpace();

            if (DrawCollaspeButton)
            {
                if (GUILayout.Button(IsCollapsed ? "+" : "-")) IsCollapsed = !IsCollapsed;
            }
            GUILayout.EndHorizontal();

            if (!IsCollapsed)
            {
                _itemsScrollState = GUILayout.BeginScrollView(_itemsScrollState, GUILayout.Height(200));
                GUILayout.BeginVertical();

                foreach (var element in ItemElements)
                {
                    element.Draw(roomCamera);
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            CleanupElements();
        }

        public void DrawPostWindow(RoomCamera? roomCamera)
        {
            foreach (var element in ItemElements)
            {
                element.DrawPostWindow(roomCamera);
                ElementsToDelete.Add(element);
            }
        }

        public void OnSaved()
        {
            SavedList = [.. Target];

            foreach (var element in ItemElements)
            {
                element.OnSaved();
            }
        }

        public void ReceiveFContainer(FContainer? container)
        {
            foreach (var element in ItemElements)
            {
                element.ReceiveFContainer(container);
            }
        }

        public void ResetChanges()
        {
            foreach (var element in ItemElements)
            {
                element.ResetChanges();
            }

            Target = [.. SavedList];
            RegenerateElements();
        }

        public void Terminate()
        {
            foreach (var element in ItemElements)
            {
                element.Terminate();
            }
        }

        [MemberNotNull(nameof(ItemElements), nameof(ElementsByItems))]
        private void RegenerateElements()
        {
            ItemElements = [];
            ElementsByItems = [];

            foreach (var item in Target)
            {
                IObjectEditorElement newElement = ItemElementFactory(item, () => DeleteItem(item));

                ItemElements.Add(newElement);
                ElementsByItems.Add(item, newElement);
            }
        }
        #endregion
    }
}
