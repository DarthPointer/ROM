using ROM.IMGUIUtilities;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.LevelPosition;

internal class PolygonElement : IObjectEditorElement
{
    string displayName;
    bool isExpanded, drawPolygon;
    PointElement[] vertices;
    FSprite[] fSprites;

    public bool HasChanges => vertices.Any(x => x.HasChanges);

    public struct PointAccessor
    {
        public Func<Vector2> getter;
        public Action<Vector2> setter;
    }
    public PolygonElement(string displayName, params PointAccessor[] vertices)
    {
        List<PointElement> list = new();
        for(int i=0; i<vertices.Length; i++)
        {
            list.Add(new PointElement($"Vertex {i+1}", (i+1).ToString(), vertices[i].getter, vertices[i].setter, displayTogglePointButton: false));
        }
        this.vertices = [.. list];

        
        fSprites = new FSprite[vertices.Length];
        Array.ForEach(fSprites, sprite =>
        {
            sprite = new FSprite("pixel", true);
            
        });
        

        this.displayName = displayName;
    }

    public void Draw(RoomCamera? roomCamera)
    {
        CommonIMGUIUtils.HorizontalLine();
        GUILayout.Label(displayName);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"{vertices.Count()} vertices");
        GUILayout.FlexibleSpace();


        if(GUILayout.Button(drawPolygon ? "hide" : "show"))
        {
            OnShowHideButtonClicked();
        }
        if(GUILayout.Button(isExpanded ? "-" : "+"))
        {
            OnExpandButtonClicked();
        }
        
        GUILayout.EndHorizontal();

        if (isExpanded)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            Array.ForEach(this.vertices, vertice => vertice.Draw(roomCamera));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

    }

    public void OnSaved()
    {
        Array.ForEach(vertices, vertice => vertice.OnSaved());
    }

    public void ResetChanges()
    {
        Array.ForEach(vertices, vertice => vertice.ResetChanges());
    }

    public void DrawPostWindow(RoomCamera? roomCamera)
    {
        Array.ForEach(vertices, vertice => vertice.DrawPostWindow(roomCamera));
        this.
        Array.ForEach(vertices, vertice =>
        {
            
        });
    }

    private void OnShowHideButtonClicked()
    {
        drawPolygon = !drawPolygon;
        Array.ForEach(vertices, vertice => vertice.DrawPoint = drawPolygon);
    }
    private void OnExpandButtonClicked()
    {
        isExpanded = !isExpanded;
    }
}
