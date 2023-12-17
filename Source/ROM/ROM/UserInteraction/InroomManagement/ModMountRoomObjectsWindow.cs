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
    internal class ModMountRoomObjectsWindow : IIMGUIWindow
    {
        #region Fields
        private Rect _windowRect = new Rect(100, 100, 400, 600);
        private Vector2 _windowSize;
        private Vector2 _scrollState = Vector2.zero;
        private Vector2 _newObjectTypeScrollState = Vector2.zero;

        private string _newObjectFilePath = "";

        private string _newObjectTypeFilterText = "";
        private ITypeOperator? _newObjectTypeOperator = null;
        #endregion

        #region Consts
        private static readonly Texture2D WHITE_TEXTUE;
        private static readonly GUIStyle WHITE_BACKGROUND;
        #endregion

        static ModMountRoomObjectsWindow()
        {
            WHITE_TEXTUE = CommonIMGUIUtils.GetSingleColorTexture(16, 16, Color.white);

            WHITE_BACKGROUND = new();

            WHITE_BACKGROUND.normal.background = WHITE_TEXTUE;
        }

        #region Properties
        private ModMountController? ModMountController { get; set; }

        private List<ObjectData>? CurrentRoomObjects
        {
            get
            {
                if (ModMountController?.ContextRoom == null) { return null; }

                if (ModMountController.ModMount.ObjectsByRooms.TryGetValue(ModMountController.ContextRoom.abstractRoom.name, out List<ObjectData> result))
                {
                    return result;
                }

                return null;
            }
        }

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

        private OptionsController<ITypeOperator> NewObjectTypeOperatorOptionsController { get; }

        private string? NewObjectCreationErrorString { get; set; } = null;

        private ObjectData? ConfirmDeletingObject { get; set; } = null;
        /// <summary>
        /// Delete the <see cref="ConfirmDeletingObject"/>?
        /// </summary>
        private bool DeleteObjectConfirmed { get; set; } = false;

        private string? ExistingObjectListError { get; set; } = null;
        #endregion

        #region Constructors
        public ModMountRoomObjectsWindow(ModMountController modMountController)
        {
            _windowSize = _windowRect.size;

            ModMountController = modMountController;

            NewObjectTypeOperatorOptionsController = new(TypeOperator.TypeOperators.Select(kvp => new Option<ITypeOperator>(kvp.Value, kvp.Key)));
        }
        #endregion

        #region Methods
        void IIMGUIWindow.Display()
        {
            _windowRect.size = _windowSize;

            _windowRect = GUILayout.Window(GetHashCode(), _windowRect, ModMountWindow,
                $"{ModMountController?.ContextRoom?.abstractRoom.name ?? "NO ROOM"} objects of {ModMountController?.ModMount.Mod?.id ?? "NO MOD SET"} mount.");
        }

        private void ModMountWindow(int id)
        {
            GUILayout.BeginVertical();

            _scrollState = GUILayout.BeginScrollView(_scrollState);

            GUILayout.BeginVertical();

            ListExistingRoomObjects();

            NewObjectCreation();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            _windowSize = WindowResizer.GetNewSizeByDragButton(this, _windowSize);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void ListExistingRoomObjects()
        {
            if (ModMountController?.ContextRoom == null)
            {
                GUILayout.Label("No room found.");
                return;
            }

            if (CurrentRoomObjects == null || CurrentRoomObjects.Count == 0)
            {
                GUILayout.Label($"{ModMountController.ContextRoom.abstractRoom.name} has no objects from {ModMountController.ModMount.Mod.id} yet");
                return;
            }

            foreach (ObjectData obj in CurrentRoomObjects)
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

        private void ListExistingRoomObject(ObjectData objectData)
        {
            if (ConfirmDeletingObject != null && ModMountController?.ContextRoom?.abstractRoom.name != ConfirmDeletingObject.RoomId)
            {
                ConfirmDeletingObject = null;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{objectData.FilePath}: {objectData.TypeId}");
            GUILayout.FlexibleSpace();
            DrawDeleteObjectButton(objectData);

            GUILayout.EndHorizontal();

            if (ConfirmDeletingObject == objectData)
            {
                GUILayout.Label($"Click the cross button again to delete {objectData.FilePath}.");
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

            GUILayout.Label("", WHITE_BACKGROUND, GUILayout.Height(2));

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

            DrawNewObjectFilePath();

            DrawNewObjectTypeOperator();

            DrawCreateNewObjectButton();
        }

        private void DrawNewObjectFilePath()
        {
            GUILayout.Label("Object file path:");
            NewObjectFilePath = GUILayout.TextField(NewObjectFilePath);

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
                ModMountController.AddObject(NewObjectFilePathWithExtension, NewObjectTypeOperator);
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

        public void Close()
        {
            this.RemoveFromContainer();
            ModMountController = null;
        }
        #endregion
    }
}
