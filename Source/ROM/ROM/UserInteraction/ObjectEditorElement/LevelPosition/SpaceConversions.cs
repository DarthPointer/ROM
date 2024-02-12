using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.LevelPosition;

internal static class SpaceConversions
{
    public static Vector2 RoomSpaceToScreenSpace(Vector2 roomSpace, RoomCamera roomCamera)
    {
        Vector2 incamOffset = roomSpace - roomCamera.pos;

        float xScale = Screen.width / roomCamera.sSize.x;
        float yScale = Screen.height / roomCamera.sSize.y;

        Vector2 screenSpace = new Vector2(incamOffset.x * xScale, Screen.height - incamOffset.y * yScale);

        return screenSpace;
    }

    public static Vector2 ScreenSpaceToRoomSpace(Vector2 screenSpace, RoomCamera roomCamera)
    {
        float xScale = Screen.width / roomCamera.sSize.x;
        float yScale = Screen.height / roomCamera.sSize.y;

        screenSpace.y = Screen.height - screenSpace.y;

        Vector2 incamOffset = new Vector2(screenSpace.x / xScale, screenSpace.y / yScale);

        return roomCamera.pos + incamOffset;
    }
}
