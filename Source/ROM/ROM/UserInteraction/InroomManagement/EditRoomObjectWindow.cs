using Newtonsoft.Json.Linq;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.UserInteraction.ModMountManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    //public interface IEditRoomObjectWindow
    //{
        
    //}

    internal class EditRoomObjectWindow : ResizeableAndDraggableIMGUIWindow//, IEditRoomObjectWindow
    {
        public const string CONFIRM_CLOSE_CHANGES_UNSAVED = "Unsaved changes, save them or click again to close anyway.";

        #region Fields
        private Vector2 _propertiesScrollState;
        #endregion

        #region Properties
        protected override string HeaderText => WindowHeader;
        private string WindowHeader { get; }

        private ModMountController OwningController { get; }

        private IReadOnlyList<IObjectEditorElement> ObjectEditorElements { get; set; }
        private Func<object, IEnumerable<IObjectEditorElement>> ObjectEditorElementsFactory { get; }

        private ObjectData ObjectData { get; }

        private ObjectHost TargetHost { get; }
        private object? TargetObject { get; set; }

        public bool HasChanges => ObjectEditorElements.Any(x => x.HasChanges);

        private string? ConfirmCloseUnsavedString { get; set; } = null;
        private bool TriedToCloseUnsaved { get; set; } = false;
        private string? SaveErrorString { get; set; } = null;

        private RoomCamera? RoomCamera { get; }
        #endregion

        #region Constructors
        public EditRoomObjectWindow(ModMountController owningController,
            ObjectData objectData, ObjectHost objectHost, Func<object, IEnumerable<IObjectEditorElement>> objectElementsFactory, RoomCamera? roomCamera, FContainer? container)
        {
            WindowHeader = objectData.FilePath;

            _windowRect = new(100, 100, 300, 300);

            OwningController = owningController;
            ObjectEditorElementsFactory = objectElementsFactory;

            TargetHost = objectHost;
            ObjectData = objectData;

            objectHost.Object.TryGetTarget(out object? obj);
            TargetObject = obj;

            RoomCamera = roomCamera;

            RegenerateObjectEditorElements();

            foreach (var item in ObjectEditorElements)
            {
                item.ReceiveFContainer(container);
            }

        }
        #endregion

        #region Methods
        [MemberNotNull(nameof(ObjectEditorElements))]
        private void RegenerateObjectEditorElements()
        {
            if (TargetObject != null)
            {
                ObjectEditorElements = ObjectEditorElementsFactory(TargetObject).ToList();
            }
            else
            {
                ObjectEditorElements = [];
            }
        }

        protected override void WindowFunction(int id)
        {
            GUILayout.BeginVertical();

            _propertiesScrollState = GUILayout.BeginScrollView(_propertiesScrollState);
            GUILayout.BeginVertical();

            if (TargetObject != null)
            {
                foreach (IObjectEditorElement element in ObjectEditorElements)
                {
                    element.Draw(RoomCamera);
                }
            }
            else
            {
                GUILayout.Label("The object is not created. Spawning conditions were not met or an error occurred while spawning.");
            }

            GUILayout.EndVertical();

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

            if (TargetObject != null) ObjectData.DataJson = typeOperator.Save(TargetObject);

            ObjectData.Save();

            foreach (IObjectEditorElement editorElement in ObjectEditorElements)
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
            foreach (IObjectEditorElement editorElement in ObjectEditorElements)
            {
                editorElement.ResetChanges();
            }

            TriedToCloseUnsaved = false;
        }

        public void Close()
        {
            foreach(IObjectEditorElement editorElement in ObjectEditorElements)
            {
                editorElement.Terminate();
            }

            this.RemoveFromContainer();
            OwningController.RemoveWindowForObject(ObjectData);
        }

        protected override void PostCall()
        {
            foreach (IObjectEditorElement objectEditorElement in ObjectEditorElements)
            {
                objectEditorElement.DrawPostWindow(RoomCamera);
            }
        }
        #endregion
    }
}
