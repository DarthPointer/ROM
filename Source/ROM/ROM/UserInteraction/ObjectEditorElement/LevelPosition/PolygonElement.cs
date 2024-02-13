using ROM.IMGUIUtilities;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ROM.UserInteraction.ObjectEditorElement.LevelPosition.SpaceConversions;
using static RWCustom.Custom;

namespace ROM.UserInteraction.ObjectEditorElement.LevelPosition;

public class PolygonElement : IObjectEditorElement
{
    FContainer? container;
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
        List<PointElement> list = [];
        for(int i=0; i<vertices.Length; i++)
        {
            list.Add(new PointElement($"Vertex {i+1}", (i+1).ToString(), vertices[i].getter, vertices[i].setter, displayTogglePointButton: false));
        }
        this.vertices = [.. list];

        
        fSprites = new FSprite[vertices.Length];
        for(int i=0; i<fSprites.Length; i++)
        {
            fSprites[i] = new FSprite("pixel", true);
        }


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
        if (roomCamera == null) return;
        if (!drawPolygon) return;
        //point draw
        Array.ForEach(vertices, vertice => vertice.DrawPostWindow(roomCamera));

        //edge draw
        for(int i = 0; i < fSprites.Length; i++)
        {
            fSprites[i].SetPosition(FutileRoomSpaceToScreenSpace(vertices[i].Target, roomCamera));
            Vector2 coordinateDifference = FutileRoomSpaceToScreenSpace(vertices[i + 1 >= vertices.Length ? 0 : i + 1].Target, roomCamera) - FutileRoomSpaceToScreenSpace(vertices[i].Target, roomCamera);
            fSprites[i].scaleY = coordinateDifference.magnitude;
            fSprites[i].rotation = VecToDeg( coordinateDifference );
            fSprites[i].anchorY = 0;
        }
    }

    private void OnShowHideButtonClicked()
    {
        drawPolygon = !drawPolygon;
        Array.ForEach(vertices, vertice => vertice.DrawPoint = drawPolygon);
        Array.ForEach(fSprites, sprite => sprite.isVisible = drawPolygon);
    }
    private void OnExpandButtonClicked()
    {
        isExpanded = !isExpanded;
    }

    public void ReceiveFContainer(FContainer? container)
    {
        if(container == null)
        {
            ROMPlugin.Logger?.LogError("Polygon Element received null fContainer and won't draw its edges");
            return;
        }
        this.container = container;
        Array.ForEach(fSprites, sprite => container.AddChild(sprite));
    }

    public void Terminate()
    {
        foreach(var sprite in fSprites)
        {
            sprite.RemoveFromContainer();
        }
    }
}
