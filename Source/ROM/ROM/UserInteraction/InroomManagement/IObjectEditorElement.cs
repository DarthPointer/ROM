using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.InroomManagement
{
    public interface IObjectEditorElement
    {
        bool HasChanges { get; }

        void Draw();

        void OnSaved();

        void ResetChanges();

        void DrawPostWindow();
    }
}
