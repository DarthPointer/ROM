using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.RoomObjectService.SpawningCondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    internal class EditSpawningConditionWindow : ResizeableAndDraggableIMGUIWindow
    {
        #region Fields
        Vector2 _mainScrollState;
        #endregion

        #region Properties
        private string Header { get; }
        protected override string HeaderText => Header;

        private ObjectData ObjectData { get; }
        private string? SaveErrorString { get; set; }

        private IReadOnlyList<IObjectEditorElement> EditorElements { get; }
        private RoomCamera? RoomCamera { get; }
        private EditRoomObjectWindow OwnerWindow { get; }
        #endregion

        #region Constructors
        public EditSpawningConditionWindow(string header, ObjectData objectData, EditRoomObjectWindow ownerWindow,
            RoomCamera roomCamera, IReadOnlyList<IObjectEditorElement> editorElements)
        {
            Header = header;
            ObjectData = objectData;
            OwnerWindow = ownerWindow;
            RoomCamera = roomCamera;
            EditorElements = editorElements;
        }
        #endregion

        #region Methods
        protected override void WindowFunction(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(ObjectData.SpawningConditionTypeId ?? "");

            _mainScrollState = GUILayout.BeginScrollView(_mainScrollState);

            GUILayout.BeginVertical();
            foreach (IObjectEditorElement element in EditorElements)
            {
                element.Draw(RoomCamera);
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
        }

        private void CloseClick()
        {
            Close();
        }

        private void ResetChangesClick()
        {
            foreach (IObjectEditorElement element in EditorElements)
            {
                element.ResetChanges();
            }
        }

        protected override void PostCall()
        {
            foreach (IObjectEditorElement editorElement in EditorElements)
            {
                editorElement.DrawPostWindow(RoomCamera);
            }
        }

        public void Close()
        {
            foreach (IObjectEditorElement editorElement in EditorElements)
            {
                editorElement.Terminate();
            }

            this.RemoveFromContainer();
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

                ROMPlugin.Logger?.LogError($"Failed to save an spawning condition of type {ObjectData.SpawningConditionTypeId ?? ""}.\n{ex}");
            }
        }

        public void Save()
        {
            ISpawningConditionOperator spawningConditionOperator = ObjectData.GetSpawningConditionOperator();

            ISpawningCondition? spawningCondition = SpawningManager.SpawnedObjectsTracker[ObjectData].SpawningCondition;
            if (spawningCondition != null) ObjectData.SpawningConditionDataJson = spawningConditionOperator.Save(spawningCondition);

            OwnerWindow.SaveCurrentObjectDataState();

            foreach (IObjectEditorElement editorElement in EditorElements)
            {
                editorElement.OnSaved();
            }
        }
        #endregion
    }
}
