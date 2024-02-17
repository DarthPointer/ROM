using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.List
{
    public class SelectedSubsetListItemElement<T> : IObjectEditorElement
    {
        #region Proeprties
        public bool HasChanges => false;

        public Option<T> Option { get; }
        private Action DeleteCall { get; }
        #endregion

        #region Constructors
        public SelectedSubsetListItemElement(Option<T> option, Action deleteCall)
        {
            Option = option;
            DeleteCall = deleteCall;
        }
        #endregion

        #region Methods
        public void Draw(RoomCamera? roomCamera)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Option.Name);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("x")) DeleteCall();
            GUILayout.EndHorizontal();
        }

        public void DrawPostWindow(RoomCamera? roomCamera)
        { }

        public void OnSaved()
        { }

        public void ReceiveFContainer(FContainer? container)
        { }

        public void ResetChanges()
        { }

        public void Terminate()
        { }
        #endregion
    }
}
