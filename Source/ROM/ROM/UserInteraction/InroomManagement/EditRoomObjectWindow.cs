using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.UserInteraction.ModMountManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    public interface IEditRoomObjectWindow
    {
        
    }

    internal class EditRoomObjectWindow : ResizeableAndDraggableIMGUIWindow, IEditRoomObjectWindow
    {
        public const string CONFIRM_CLOSE_CHANGES_UNSAVED = "Unsaved changes, save them or click again to close anyway.";

        #region Fields
        private Vector2 _propertiesScrollState;
        #endregion

        #region Properties
        private ModMountController OwningController { get; }

        private IReadOnlyList<IObjectEditorElement> EditorElements { get; }

        private ObjectData ObjectData { get; }

        private object TargetObject { get; }

        public bool HasChanges => EditorElements.Any(x => x.HasChanges);

        private string? ConfirmCloseUnsavedString { get; set; } = null;
        private bool TriedToCloseUnsaved { get; set; } = false;
        private string? SaveErrorString { get; set; } = null;
        #endregion

        #region Constructors
        public EditRoomObjectWindow(ModMountController owningController,
            ObjectData objectData, object targetObject, IEnumerable<IObjectEditorElement> editorElements)
        {
            _windowRect = new(100, 100, 300, 300);

            OwningController = owningController;

            ObjectData = objectData;
            TargetObject = targetObject;
            EditorElements = editorElements.ToList();
        }
        #endregion

        #region Methods
        protected override void WindowFunction(int id)
        {
            GUILayout.BeginVertical();

            _propertiesScrollState = GUILayout.BeginScrollView(_propertiesScrollState);
            GUILayout.BeginVertical();

            GUILayout.EndVertical();

            foreach(IObjectEditorElement element in EditorElements)
            {
                element.Draw();
            }

            GUILayout.EndScrollView();

            DrawSaveAndClose();

            GUILayout.EndVertical();
        }

        private void DrawSaveAndClose()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("save"))
            {
                SaveClick();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("reset changes"))
            {
                ResetChangesClick();
            }

            if (GUILayout.Button("close"))
            {
                CloseClick();
            }

            GUILayout.EndHorizontal();

            if (SaveErrorString != null)
            {
                GUILayout.Label(SaveErrorString);
            }

            if (TriedToCloseUnsaved && ConfirmCloseUnsavedString != null)
            {
                GUILayout.Label(ConfirmCloseUnsavedString);
            }
        }

        private void SaveClick()
        {
            SaveErrorString = null;

            try
            {
                Save();
            }
            catch (Exception ex)
            {
                SaveErrorString = $"Failed to save: {ex.GetType()}, see logs.";

                ROMPlugin.Logger?.LogError($"Failed to save an object of type {ObjectData.TypeId}.\n{ex}");
            }
        }

        public void Save()
        {
            ITypeOperator typeOperator = ObjectData.GetTypeOperator();
            typeOperator.Save(TargetObject);

            foreach (IObjectEditorElement editorElement in EditorElements)
            {
                editorElement.OnSaved();
            }

            TriedToCloseUnsaved = false;
        }

        private void CloseClick()
        {
            if (TriedToCloseUnsaved == true)
            {
                Close();
                return;
            }

            if (HasChanges)
            {
                TriedToCloseUnsaved = true;
                ConfirmCloseUnsavedString = "This window has unsaved changes. Save them or click again to close it anyway.";

                return;
            }

            Close();
        }

        private void ResetChangesClick()
        {
            foreach (IObjectEditorElement editorElement in EditorElements)
            {
                editorElement.ResetChanges();
            }

            TriedToCloseUnsaved = false;
        }

        public void Close()
        {
            this.RemoveFromContainer();
            OwningController.RemoveWindowForObject(ObjectData);
        }

        protected override void PostCall()
        {
            foreach (IObjectEditorElement objectEditorElement in EditorElements)
            {
                objectEditorElement.DrawPostWindow();
            }
        }
        #endregion
    }
}
