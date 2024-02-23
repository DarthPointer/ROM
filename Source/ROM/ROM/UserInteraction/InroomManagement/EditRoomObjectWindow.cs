using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using ROM.IMGUIUtilities;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.RoomObjectService.SpawningCondition;
using ROM.UserInteraction.ModMountManagement;
using ROM.UserInteraction.ObjectEditorElement;
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
        private IReadOnlyList<IObjectEditorElement> SpawningConditionEditorElements { get; set; }
        private EditSpawningConditionWindow? EditSpawningConditionWindow { get; set; }
        private FContainer? FContainer { get; }

        private IObjectEditorElement SpawningConditionTypeSelectionElement { get; }

        private ObjectData ObjectData { get; }

        private ObjectHost TargetHost { get; }
        private object? TargetObject { get; set; }
        private bool SpawningConditionTypeSelectionIsExpanded { get; set; }

        private bool SpawningConditionTypeChangeIsNotSaved { get; set; } = false;

        public bool HasChanges =>
            SpawningConditionTypeChangeIsNotSaved ||
            ObjectEditorElements.Any(x => x.HasChanges) ||
            SpawningConditionEditorElements.Any(x => x.HasChanges);

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

            FContainer = container;
            RoomCamera = roomCamera;

            RegenerateObjectEditorElements();
            RegenerateSpawningConditionEditorElements();

            foreach (var item in ObjectEditorElements)
            {
                item.ReceiveFContainer(container);
            }

            SpawningConditionTypeSelectionElement =
                Elements.CollapsableOptionSelect("Select spawning condition type",
                    getter: () => null,
                    setter: SetSpawningConditionType,

                    SpawningConditionOperator.ConditionTypeOperators.Select(kvp => new Option<ISpawningConditionOperator?>(kvp.Value, kvp.Key)).
                        Append(new Option<ISpawningConditionOperator?>(null, "")),
                    
                    displayNullOption: false);
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

        [MemberNotNull(nameof(SpawningConditionEditorElements))]
        private void RegenerateSpawningConditionEditorElements()
        {
            if (ObjectData.SpawningConditionTypeId != null &&
                TargetHost.SpawningCondition != null &&
                SpawningConditionOperator.ConditionTypeOperators.TryGetValue(ObjectData.SpawningConditionTypeId, out ISpawningConditionOperator spawningConditionOperator))
            {
                SpawningConditionEditorElements = spawningConditionOperator.GetEditorElements(TargetHost.SpawningCondition).ToList();

                foreach (IObjectEditorElement element in SpawningConditionEditorElements)
                {
                    element.ReceiveFContainer(FContainer);
                }
            }
            else
            {
                SpawningConditionEditorElements = [];
            }
        }

        protected override void WindowFunction(int id)
        {
            GUILayout.BeginVertical();

            DrawSpawningConditionHeader(RoomCamera);

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

        private void SetSpawningConditionType(ISpawningConditionOperator? spawningConditionOperator)
        {

        }

        private void DrawSpawningConditionHeader(RoomCamera? roomCamera)
        {
            if (TargetHost.SpawningCondition == null)
            {
                SpawningConditionTypeSelectionElement.Draw(roomCamera);
            }
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

            if (ObjectData.SpawningConditionTypeId != null &&
                TargetHost.SpawningCondition != null &&
                SpawningConditionOperator.ConditionTypeOperators.TryGetValue(ObjectData.SpawningConditionTypeId, out var spawningConditionOperator))
            {
                ObjectData.SpawningConditionDataJson = spawningConditionOperator.Save(TargetHost.SpawningCondition);
            }
            else
            {
                ObjectData.SpawningConditionDataJson = null;
            }

            SaveCurrentObjectDataState();

            foreach (IObjectEditorElement editorElement in ObjectEditorElements)
            {
                editorElement.OnSaved();
            }

            foreach (IObjectEditorElement conditionEditorElement in SpawningConditionEditorElements)
            {
                conditionEditorElement.OnSaved();
            }

            TriedToCloseUnsaved = false;
        }

        public void SaveCurrentObjectDataState()
        {
            ObjectData.Save();
            SpawningConditionTypeChangeIsNotSaved = false;
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

            // If the spawning condition was removed/created since last save of its type.
            if (SpawningConditionTypeChangeIsNotSaved)
            {
                // If the condition's id is set
                if (ObjectData.SpawningConditionTypeId != null)
                {
                    // then we check if we have the rest of the data needed to restore its instance.
                    if (SpawningConditionOperator.ConditionTypeOperators.TryGetValue(ObjectData.SpawningConditionTypeId, out var spawningConditionOperator) &&
                    ObjectData.SpawningConditionDataJson != null)
                    {
                        TargetHost.SpawningCondition = spawningConditionOperator.Load(ObjectData.SpawningConditionDataJson);
                        if (EditSpawningConditionWindow != null)
                        {
                            EditSpawningConditionWindow.Close();
                        }
                    }
                    // If we don't then we just drop the condition and logspam.
                    else
                    {
                        string failureReason = ObjectData.SpawningConditionDataJson != null ? "its data is null." : "this condition type is not registered.";

                        ROMPlugin.Logger?.LogError($"Unable to restore a spawning condition of type {ObjectData.SpawningConditionTypeId} for {ObjectData.FullLogString} because " +
                            failureReason);

                        TargetHost.SpawningCondition = null;
                        if (EditSpawningConditionWindow != null)
                        {
                            EditSpawningConditionWindow.Close();
                        }
                    }
                }
                // If the condition's type id is null then we just drop the current one.
                {
                    TargetHost.SpawningCondition = null;
                    if (EditSpawningConditionWindow != null)
                    {
                        EditSpawningConditionWindow.Close();
                    }
                }
            }
            // If the last condition set/removal is has been saved, then the changes are only reset inside it.
            else
            {
                foreach (IObjectEditorElement conditionEditorElement in SpawningConditionEditorElements)
                {
                    conditionEditorElement.ResetChanges();
                }
            }

            TriedToCloseUnsaved = false;
        }

        public void Close()
        {
            foreach (IObjectEditorElement editorElement in ObjectEditorElements)
            {
                editorElement.ResetChanges();
            }

            foreach (IObjectEditorElement editorElement in ObjectEditorElements)
            {
                editorElement.Terminate();
            }

            foreach (IObjectEditorElement conditionEditorElement in SpawningConditionEditorElements)
            {
                conditionEditorElement.ResetChanges();
            }

            foreach (IObjectEditorElement conditionEditorElement in SpawningConditionEditorElements)
            {
                conditionEditorElement.Terminate();
            }

            EditSpawningConditionWindow?.Close();

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
