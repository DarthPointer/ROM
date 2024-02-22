using Newtonsoft.Json.Linq;
using ROM.IMGUIUtilities;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.UserInteraction.ModMountManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    internal class ModMountRoomObjectsWindow : ResizeableAndDraggableIMGUIWindow
    {
        #region Fields
        private Vector2 _scrollState = Vector2.zero;
        private Vector2 _newObjectTypeScrollState = Vector2.zero;

        private string _newObjectFilePath = "";

        private string _newObjectTypeFilterText = "";
        private ITypeOperator? _newObjectTypeOperator = null;
        #endregion

        #region Properties
        protected override string HeaderText => $"{ModMountController?.ContextRoom?.abstractRoom.name ?? "NO ROOM"} objects of {ModMountController?.ModMount.Mod?.id ?? "NO MOD SET"} mount.";

        private ModMountController? ModMountController { get; set; }

        private string NewObjectFilePath
        {
            get
            {
                return _newObjectFilePath;
            }
            set
            {
                if (value != _newObjectFilePath)
                {
                    _newObjectFilePath = value;
                    UpdateNewObjectPathStatus();
                }
            }
        }

        private RegistryMountListWindow OwnerWindow { get; }

        private string NewObjectFilePathWithExtension => NewObjectFilePath + ".json";
        private bool NewObjectFilePathIsValid { get; set; } = true;
        private string NewObjectFilePathErrorString { get; set; } = "";

        private string NewObjectTypeFilterText
        {
            get
            {
                return _newObjectTypeFilterText;
            }
            set
            {
                if (value != _newObjectTypeFilterText)
                {
                    _newObjectTypeFilterText = value;
                    NewObjectTypeOperatorOptionsController.SearchFilter = _newObjectTypeFilterText;

                    NewObjectTypeOperator = GetTypeOperatorByFilterText();
                }
            }
        }

        private ITypeOperator? NewObjectTypeOperator
        {
            get
            {
                return _newObjectTypeOperator;
            }
            set
            {
                if (_newObjectTypeOperator != value)
                {
                    _newObjectTypeOperator = value;

                    if (_newObjectTypeOperator != null)
                    {
                        _newObjectTypeFilterText = _newObjectTypeOperator.TypeId;
                        NewObjectTypeOperatorOptionsController.SearchFilter = "";
                    }
                }
            }
        }

        private OptionsFilter<ITypeOperator> NewObjectTypeOperatorOptionsController { get; }
        private string? NewObjectCreationErrorString { get; set; } = null;

        private ObjectData? ConfirmCloseWindowUnsaved { get; set; } = null;
        private ObjectData? ConfirmDeletingObject { get; set; } = null;
        /// <summary>
        /// Delete the <see cref="ConfirmDeletingObject"/>?f
        /// </summary>
        private bool DeleteObjectConfirmed { get; set; } = false;

        private OptionsFilter<ObjectData> ExistingObjectsFilter { get; } = new();
        private IReadOnlyList<ObjectData>? PreviousExistingObjectsList { get; set; }

        private string? ExistingObjectListError { get; set; } = null;
        #endregion

        #region Constructors
        public ModMountRoomObjectsWindow(ModMountController modMountController, RegistryMountListWindow registryMountListWindow)
        {
            _windowRect = new Rect(100, 100, 400, 600);
            _collapsedRect.width = 200;

            ModMountController = modMountController;
            OwnerWindow = registryMountListWindow;

            NewObjectTypeOperatorOptionsController = new(TypeOperator.TypeOperators.Select(kvp => new Option<ITypeOperator>(kvp.Value, kvp.Key)));
        }
        #endregion

        #region Methods
        protected override void WindowFunction(int id)
        {
            if (ModMountController != null)
            {
                if (ModMountController.CurrentRoomObjectsList != PreviousExistingObjectsList)
                {
                    ExistingObjectsFilter.SetOptions(ModMountController.CurrentRoomObjectsList?.Select(CreateObjectDataOption) ?? []);
                    PreviousExistingObjectsList = ModMountController.CurrentRoomObjectsList;
                }

                GUILayout.BeginVertical();

                DrawTopButtons();

                if (!IsCollapsed)
                {

                    _scrollState = GUILayout.BeginScrollView(_scrollState);

                    GUILayout.BeginVertical();

                    NewObjectCreation();

                    CommonIMGUIUtils.HorizontalLine();

                    ListExistingRoomObjects();

                    GUILayout.EndVertical();

                    GUILayout.EndScrollView();
                }

                GUILayout.EndVertical();
            }
        }

        private void DrawTopButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(IsCollapsed ? "+" : "-"))
                OnCollapseExpandButtonClicked();

            if (GUILayout.Button("x"))
                OnCloseButtonClicked();

            GUILayout.EndHorizontal();
        }

        private void OnCollapseExpandButtonClicked()
        {
            IsCollapsed = !IsCollapsed;
        }

        private void OnCloseButtonClicked()
        {
            if (ModMountController == null) return;

            OwnerWindow.CloseMountWindow(ModMountController);
        }

        private void ListExistingRoomObjects()
        {
            ExistingObjectsFilter.SearchFilter = GUILayout.TextField(ExistingObjectsFilter.SearchFilter);

            if (ModMountController?.ContextRoom == null)
            {
                GUILayout.Label("No room found.");
                return;
            }

            if (ModMountController.CurrentRoomObjectsList == null || ModMountController.CurrentRoomObjectsList.Count == 0)
            {
                GUILayout.Label($"{ModMountController.ContextRoom.abstractRoom.name} has no objects from {ModMountController.ModMount.Mod.id} yet");
                return;
            }

            foreach (Option<ObjectData> obj in ExistingObjectsFilter.FilteredOptions)
            {
                ListExistingRoomObject(obj);
            }

            if (DeleteObjectConfirmed)
            {
                DeleteObject();
            }

            if (ExistingObjectListError != null)
            {
                GUILayout.Label(ExistingObjectListError);
            }
        }

        private void ListExistingRoomObject(Option<ObjectData> objectData)
        {
            if (ConfirmDeletingObject != null && ModMountController?.ContextRoom?.abstractRoom.name != ConfirmDeletingObject.RoomId)
            {
                ConfirmDeletingObject = null;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label(objectData.Name);
            GUILayout.FlexibleSpace();
            DrawToggleObjectWindowButton(objectData.Value);
            DrawDeleteObjectButton(objectData.Value);

            GUILayout.EndHorizontal();

            if (ConfirmDeletingObject == objectData.Value)
            {
                GUILayout.Label($"Click the cross button again to delete {objectData.Value.FilePath}.");
            }

            if (ConfirmCloseWindowUnsaved == objectData.Value)
            {
                GUILayout.Label($"Click the minus button again to close the window with unsaved changes.");
            }
        }

        private void DrawToggleObjectWindowButton(ObjectData objectData)
        {
            if (ModMountController == null)
                return;

            EditRoomObjectWindow? editRoomObjectWindow = ModMountController.EditObjectWindows[objectData];
            if (editRoomObjectWindow == null)
            {
                if (GUILayout.Button("+"))
                {
                    ModMountController.OpenWindowForObject(objectData);
                }

                if (ConfirmCloseWindowUnsaved == objectData)
                {
                    ConfirmCloseWindowUnsaved = null;
                }

                return;
            }

            if (GUILayout.Button("-"))
            {
                if (ConfirmCloseWindowUnsaved == objectData)
                {
                    ModMountController.CloseWindowForObject(objectData);
                    ConfirmCloseWindowUnsaved = null;
                    return;
                }

                if (editRoomObjectWindow.HasChanges)
                {
                    ConfirmCloseWindowUnsaved = objectData;
                    return;
                }

                ModMountController.CloseWindowForObject(objectData);
            }

            EditRoomObjectWindow? editObjectWindow = ModMountController.EditObjectWindows[objectData];
            if (ConfirmCloseWindowUnsaved == objectData && editObjectWindow?.HasChanges != true)
            {
                ConfirmCloseWindowUnsaved = null;
            }
        }

        private void DrawDeleteObjectButton(ObjectData objectData)
        {
            if (ModMountController?.ContextRoom == null)
                return;

            if (GUILayout.Button("x"))
            {
                if (ConfirmDeletingObject != objectData)
                {
                    ConfirmDeletingObject = objectData;
                    DeleteObjectConfirmed = false;
                    return;
                }

                DeleteObjectConfirmed = true;
                if (ConfirmCloseWindowUnsaved == objectData)
                {
                    ConfirmCloseWindowUnsaved = null;
                }
            }
        }

        private void DeleteObject()
        {
            if (ConfirmDeletingObject == null || DeleteObjectConfirmed == false || ModMountController?.ContextRoom == null)
                return;

            ExistingObjectListError = null;

            try
            {
                ModMountController.DeleteObject(ConfirmDeletingObject);
                ExistingObjectsFilter.RemoveOption(ConfirmDeletingObject);
            }
            catch (Exception ex)
            {
                DeleteObjectConfirmed = false;
                ExistingObjectListError = ex.Message;
                return;
            }

            ConfirmDeletingObject = null;
            DeleteObjectConfirmed = false;
        }

        private void NewObjectCreation()
        {
            if (ModMountController?.ContextRoom == null)
                return;

            if (ModMountController == null)
            {
                GUILayout.Label("Error: no controller assigned");
                return;
            }

            if (ModMountController.ModMount.Mod == null)
            {
                GUILayout.Label("Error: no mod assigned");
                return;
            }

            GUILayout.Label("Create new object here");

            DrawCreateNewObjectButton();
            DrawNewObjectFilePath();
            DrawNewObjectTypeOperator();
        }

        private void DrawNewObjectFilePath()
        {
            GUILayout.Label("Object file path:");
            NewObjectFilePath = GUILayout.TextField(NewObjectFilePath);

            if (NewObjectTypeOperator != null && ModMountController?.ContextRoom != null)
            {
                if (GUILayout.Button("Generate object file path"))
                {
                    NewObjectFilePath = Path.Combine(ModMountController.ContextRoom.abstractRoom.name, NewObjectTypeOperator.TypeId);
                }
            }

            if (!NewObjectFilePathIsValid)
            {
                GUILayout.Label(NewObjectFilePathErrorString);
            }
        }

        private void DrawNewObjectTypeOperator()
        {
            GUILayout.Label("Object type:");
            NewObjectTypeFilterText = GUILayout.TextField(NewObjectTypeFilterText);

            _newObjectTypeScrollState = GUILayout.BeginScrollView(_newObjectTypeScrollState, GUILayout.Height(200));

            foreach (Option<ITypeOperator> operatorOption in NewObjectTypeOperatorOptionsController.FilteredOptions)
            {
                GUILayout.BeginVertical();

                if (GUILayout.Button(operatorOption.Value.TypeId))
                {
                    NewObjectTypeOperator = operatorOption.Value;
                }

                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }

        private void UpdateNewObjectPathStatus()
        {
            if (ModMountController?.ModMount.Mod == null)
            {
                ROMPlugin.Logger?.LogError("Can not build object file path with no mod assigned.");

                NewObjectFilePathErrorString = "Null reference error";
                NewObjectFilePathIsValid = false;
                return;
            }

            string completePath = ObjectData.GetPrimarySourceFilePath(ModMountController.ModMount.Mod, NewObjectFilePathWithExtension);

            bool fileExists;
            try
            {
                fileExists = File.Exists(completePath);
            }
            catch
            {
                NewObjectFilePathErrorString = "Bad file path";
                NewObjectFilePathIsValid = false;
                return;
            }

            if (fileExists)
            {
                NewObjectFilePathErrorString = "File with this name already exists";
                NewObjectFilePathIsValid = false;
                return;
            }

            NewObjectFilePathIsValid = true;
        }

        private void DrawCreateNewObjectButton()
        {
            if (GUILayout.Button("+"))
            {
                AddObjectButtonClick();
            }

            if (NewObjectCreationErrorString != null)
            {
                GUILayout.Label(NewObjectCreationErrorString);
            }
        }

        private void AddObjectButtonClick()
        {
            NewObjectCreationErrorString = null;

            if (NewObjectTypeOperator == null)
            {
                NewObjectCreationErrorString = $"No object type is selected.";
                return;
            }

            if (ModMountController == null)
            {
                NewObjectCreationErrorString = "No controller assigned to the window." +
                    " This indicates that the window should have been closed and destroyed, but is not.";
                return;
            }

            if (ModMountController.ContextRoom == null)
            {
                NewObjectCreationErrorString = "No room found to add the object to.";
                return;
            }

            try
            {
                ObjectData newObjectData = ModMountController.AddObject(NewObjectFilePathWithExtension, NewObjectTypeOperator);
                ExistingObjectsFilter.AddOption(CreateObjectDataOption(newObjectData));
            }
            catch (Exception ex)
            {
                NewObjectCreationErrorString = ex.Message;
            }
        }

        private ITypeOperator? GetTypeOperatorByFilterText()
        {
            return NewObjectTypeOperatorOptionsController.FilteredOptions.FirstOrDefault(opt => opt.Value.TypeId == NewObjectTypeFilterText)?.Value;
        }

        private static Option<ObjectData> CreateObjectDataOption(ObjectData objectData)
        {
            return new Option<ObjectData>(objectData, $"{objectData.FilePath}: {objectData.TypeId}");
        }

        public void Close()
        {
            this.RemoveFromContainer();
            ModMountController = null;
        }
        #endregion
    }
}
